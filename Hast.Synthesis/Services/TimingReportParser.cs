using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using CsvHelper;
using Hast.Synthesis.Models;
using ICSharpCode.NRefactory.CSharp;

namespace Hast.Synthesis.Services
{
    public class TimingReportParser : ITimingReportParser
    {
        public ITimingReport Parse(TextReader reportReader)
        {
            using (var csvReader = new CsvReader(reportReader))
            {
                csvReader.Configuration.Delimiter = "	";
                csvReader.Configuration.CultureInfo = CultureInfo.InvariantCulture;

                csvReader.Read();
                csvReader.ReadHeader();

                var timingReport = new TimingReport();

                while (csvReader.Read())
                {
                    var operatorString = csvReader.GetField<string>("Op");

                    // These are not directly applicable in .NET.
                    var skippedOperators = new[] { "nand", "nor", "xnor" };
                    if (skippedOperators.Contains(operatorString))
                    {
                        continue;
                    }

                    // Instead of the shift_left/right* versions we use dotnet_shift_left/right, which also takes a
                    // surrounding SmartResize() call into account.
                    if (operatorString.StartsWith("shift_left") || operatorString.StartsWith("shift_right"))
                    {
                        continue;
                    }

                    var dpdString = csvReader.GetField<string>("DPD");

                    // If the DPD is not specified then nothing to do.
                    if (string.IsNullOrEmpty(dpdString))
                    {
                        continue;
                    }

                    // Operators can be simple ones (like and and add) or ones that can also take a constant operand
                    // (like div). This is so that if one operand is a const that's a power of two we have a different
                    // timing value, addressing specific VHDL compiler optimizations (like with div_by_4).

                    var constantOperand = string.Empty;
                    var byStartIndex = operatorString.IndexOf("_by_");
                    if (byStartIndex != -1)
                    {
                        constantOperand = operatorString.Substring(byStartIndex + 4);
                    }

                    var operandType = csvReader.GetField<string>("InType");
                    var isSigned = operandType.StartsWith("signed");
                    var operandSizeMatch = Regex.Match(operandType, "([0-9]+)", RegexOptions.Compiled);
                    if (!operandSizeMatch.Success)
                    {
                        throw new InvalidOperationException("The \"" + operandType + "\" operand type doesn't have a size.");
                    }
                    var operandSizeBits = ushort.Parse(operandSizeMatch.Groups[1].Value);

                    var isSignAgnosticBinaryOperatorType = false;
                    var isSignAgnosticUnaryOperatorType = false;

                    BinaryOperatorType? binaryOperator = null;
                    UnaryOperatorType? unaryOperator = null;
                    switch (operatorString)
                    {
                        case "and":
                            isSignAgnosticBinaryOperatorType = true;
                            if (operandSizeBits == 1) binaryOperator = BinaryOperatorType.ConditionalAnd;
                            else binaryOperator = BinaryOperatorType.BitwiseAnd;
                            break;
                        case "add":
                            binaryOperator = BinaryOperatorType.Add;
                            break;
                        case var op when (op.StartsWith("div")):
                            binaryOperator = BinaryOperatorType.Divide;
                            break;
                        case "eq":
                            binaryOperator = BinaryOperatorType.Equality;
                            break;
                        case "ge":
                            binaryOperator = BinaryOperatorType.GreaterThanOrEqual;
                            break;
                        case "gt":
                            binaryOperator = BinaryOperatorType.GreaterThan;
                            break;
                        case "le":
                            binaryOperator = BinaryOperatorType.LessThanOrEqual;
                            break;
                        case "lt":
                            binaryOperator = BinaryOperatorType.LessThan;
                            break;
                        case "mod":
                            // BinaryOperatorType.Modulus is actually the remainder operator and corresponds to the
                            // VHDL operator rem, see below.
                            break;
                        case var op when (op.StartsWith("mul")):
                            binaryOperator = BinaryOperatorType.Multiply;
                            break;
                        case "neq":
                            binaryOperator = BinaryOperatorType.InEquality;
                            break;
                        case "not":
                            isSignAgnosticUnaryOperatorType = true;
                            if (operandSizeBits == 1) unaryOperator = UnaryOperatorType.Not;
                            else unaryOperator = UnaryOperatorType.BitNot;
                            break;
                        case "or":
                            isSignAgnosticBinaryOperatorType = true;
                            if (operandSizeBits == 1) binaryOperator = BinaryOperatorType.ConditionalOr;
                            else binaryOperator = BinaryOperatorType.BitwiseOr;
                            break;
                        case var op when(op.StartsWith("dotnet_shift_left")):
                            binaryOperator = BinaryOperatorType.ShiftLeft;
                            break;
                        case var op when (op.StartsWith("dotnet_shift_right")):
                            binaryOperator = BinaryOperatorType.ShiftRight;
                            break;
                        case "rem":
                            binaryOperator = BinaryOperatorType.Modulus;
                            break;
                        case "sub":
                            binaryOperator = BinaryOperatorType.Subtract;
                            break;
                        case "unary_minus":
                            unaryOperator = UnaryOperatorType.Minus;
                            break;
                        case "xor":
                            // There is no separate bitwise and conditional version for XOR.
                            isSignAgnosticBinaryOperatorType = true;
                            binaryOperator = BinaryOperatorType.ExclusiveOr;
                            break;
                        default:
                            throw new NotSupportedException("Unrecognized operator in timing report: " + operatorString + ".");
                    }

                    // For more info on DPD and TWDFR see the docs of Hastlayer Timing Tester.
                    // Data Path Delay, i.e. the propagation of signals through the operation and the nets around it.
                    var dpd = decimal.Parse(
                        dpdString.Replace(',', '.'), // Taking care of decimal commas.
                        NumberStyles.Any, 
                        CultureInfo.InvariantCulture);

                    // Timing window difference from requirement, i.e.:
                    // For Vivado:
                    // TWDFR = Requirement plus delays - Source clock delay - Requirement for arrival (clock period)
                    // For Quartus Prime:
                    // TWDFR = Data required time -(Data Arrival Time -Data Delay) -Setup relationship(clock period)
                    var twdfr = decimal.Parse(
                        csvReader.GetField<string>("TWDFR").Replace(',', '.'), // Taking care of decimal commas.
                        NumberStyles.Any,
                        CultureInfo.InvariantCulture);

                    if (binaryOperator.HasValue)
                    {
                        timingReport.SetLatencyNs(binaryOperator.Value, operandSizeBits, isSigned, constantOperand, dpd, twdfr);

                        if (isSignAgnosticBinaryOperatorType)
                        {
                            timingReport.SetLatencyNs(binaryOperator.Value, operandSizeBits, !isSigned, constantOperand, dpd, twdfr);
                        }
                    }
                    else if (unaryOperator.HasValue)
                    {
                        timingReport.SetLatencyNs(unaryOperator.Value, operandSizeBits, isSigned, constantOperand, dpd, twdfr);

                        if (isSignAgnosticUnaryOperatorType)
                        {
                            timingReport.SetLatencyNs(unaryOperator.Value, operandSizeBits, !isSigned, constantOperand, dpd, twdfr);
                        }
                    }
                }

                return timingReport;
            }
        }


        private class TimingReport : ITimingReport
        {
            private readonly Dictionary<string, decimal> _timings = new Dictionary<string, decimal>();


            public void SetLatencyNs(
                dynamic operatorType, 
                int operandSizeBits, 
                bool isSigned, 
                string constantOperand, 
                decimal dpd, 
                decimal twdfr)
            {
                _timings[GetKey(operatorType, operandSizeBits, isSigned, constantOperand)] = dpd;

                // If the operand size is 1 that means that the operation also works with single-bit non-composite types
                // where the latter may not have an explicit size. E.g. and std_logic_vector1 would be the same as 
                // std_logic but the latter is not a sized data type (and thus it's apparent size will be 0, despite it
                // being stored on at least one bit).
                // Therefore, saving a 0 bit version here too.
                if (operandSizeBits == 1)
                {
                    SetLatencyNs(operatorType, 0, isSigned, constantOperand, dpd, twdfr);
                }
            }

            public decimal GetLatencyNs(BinaryOperatorType binaryOperator, int operandSizeBits, bool isSigned, string constantOperand) =>
                GetLatencyNsInternal(binaryOperator, operandSizeBits, isSigned, constantOperand);

            public decimal GetLatencyNs(UnaryOperatorType unaryOperator, int operandSizeBits, bool isSigned) =>
                GetLatencyNsInternal(unaryOperator, operandSizeBits, isSigned, string.Empty);


            private decimal GetLatencyNsInternal(dynamic operatorType, int operandSizeBits, bool isSigned, string constantOperand)
            {
                if (_timings.TryGetValue(GetKey(operatorType, operandSizeBits, isSigned, constantOperand), out decimal latency))
                {
                    return latency;
                }

                return -1;
            }

            private static string GetKey(dynamic operatorType, int operandSizeBits, bool isSigned, string constantOperand) =>
                    operatorType.ToString() + 
                    operandSizeBits.ToString() + 
                    isSigned.ToString() + 
                    (string.IsNullOrEmpty(constantOperand) ? "-" : constantOperand);
        }
    }
}
