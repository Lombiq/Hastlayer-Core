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

                    BinaryOperatorType? binaryOperator = null;
                    UnaryOperatorType? unaryOperator = null;
                    switch (operatorString)
                    {
                        case "and":
                            binaryOperator = BinaryOperatorType.BitwiseAnd;
                            break;
                        case "add":
                            binaryOperator = BinaryOperatorType.Add;
                            break;
                        case "div":
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
                            binaryOperator = BinaryOperatorType.Modulus;
                            break;
                        case "mul":
                            binaryOperator = BinaryOperatorType.Multiply;
                            break;
                        case "neq":
                            binaryOperator = BinaryOperatorType.InEquality;
                            break;
                        case "not":
                            unaryOperator = UnaryOperatorType.Not;
                            break;
                        case "or":
                            binaryOperator = BinaryOperatorType.BitwiseOr;
                            break;
                        case "shift_left":
                            binaryOperator = BinaryOperatorType.ShiftLeft;
                            break;
                        case "shift_right":
                            binaryOperator = BinaryOperatorType.ShiftRight;
                            break;
                        case "sub":
                            binaryOperator = BinaryOperatorType.Subtract;
                            break;
                        case "xor":
                            binaryOperator = BinaryOperatorType.ExclusiveOr;
                            break;
                        default:
                            throw new NotSupportedException("Unrecognized operator in timing report: " + operatorString + ".");
                    }

                    var operandType = csvReader.GetField<string>("InType");
                    var isSigned = operandType.StartsWith("signed");
                    var operandSizeMatch = Regex.Match(operandType, "([0-9]+)", RegexOptions.Compiled);
                    if (!operandSizeMatch.Success)
                    {
                        throw new InvalidOperationException("The \"" + operandType + "\" operand type doesn't have a size.");
                    }
                    var operandSizeBits = ushort.Parse(operandSizeMatch.Groups[1].Value);

                    // For more info on DPD and TWD see the docs of Hastlayer Timing Tester.
                    // Data Path Delay, i.e. the propagation of signals through the operation and the nets around it.
                    var dpd = decimal.Parse(
                        csvReader.GetField<string>("DPD").Replace(',', '.'), // Taking care of decimal commas.
                        NumberStyles.Any, 
                        CultureInfo.InvariantCulture);

                    // Timing window, i.e.:
                    // For Vivado:
                    // TWD = Requirement plus delays - Source clock delay - Requirement for arrival (clock period)
                    // For Quartus Prime:
                    // TWD = Data required time -(Data Arrival Time -Data Delay) -Setup relationship(clock period)
                    var twd = decimal.Parse(
                        csvReader.GetField<string>("TWD").Replace(',', '.'), // Taking care of decimal commas.
                        NumberStyles.Any,
                        CultureInfo.InvariantCulture);

                    if (binaryOperator.HasValue)
                    {
                        timingReport.SetLatencyNs(binaryOperator.Value, operandSizeBits, isSigned, dpd, twd); 
                    }
                    else if (unaryOperator.HasValue)
                    {
                        timingReport.SetLatencyNs(unaryOperator.Value, operandSizeBits, isSigned, dpd, twd);
                    }
                }

                return timingReport;
            }
        }


        private class TimingReport : ITimingReport
        {
            private readonly Dictionary<string, decimal> _timings = new Dictionary<string, decimal>();


            public void SetLatencyNs(dynamic operatorType, int operandSizeBits, bool isSigned, decimal dpd, decimal twd)
            {
                // Debug code to test TWD.
                //var timing = dpd + twd;
                //var timing = dpd + twd < 0 ? twd : 0;
                var timing = dpd;

                _timings[GetKey(operatorType, operandSizeBits, isSigned)] = timing;

                // If the operand size is 1 that means that the operation also works with single-bit non-composite types
                // where the latter may not have an explicit size. E.g. and std_logic_vector1 would be the same as 
                // std_logic but the latter is not a sized data type (and thus it's apparent size will be 0, despite it
                // being stored on at least one bit).
                // Therefore, saving a 0 bit version here too.
                if (operandSizeBits == 1)
                {
                    SetLatencyNs(operatorType, 0, isSigned, dpd, twd);
                }
            }

            public decimal GetLatencyNs(BinaryOperatorType binaryOperator, int operandSizeBits, bool isSigned)
            {
                return GetLatencyNsInternal(binaryOperator, operandSizeBits, isSigned);
            }

            public decimal GetLatencyNs(UnaryOperatorType unaryOperator, int operandSizeBits, bool isSigned)
            {
                return GetLatencyNsInternal(unaryOperator, operandSizeBits, isSigned);
            }


            private decimal GetLatencyNsInternal(dynamic operatorType, int operandSizeBits, bool isSigned)
            {
                if (_timings.TryGetValue(GetKey(operatorType, operandSizeBits, isSigned), out decimal latency))
                {
                    return latency;
                }

                return -1;
            }

            private static string GetKey(dynamic operatorType, int operandSizeBits, bool isSigned)
            {
                return operatorType.ToString() + operandSizeBits.ToString() + isSigned.ToString();
            }
        }
    }
}
