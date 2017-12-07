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
                        case "or":
                            binaryOperator = BinaryOperatorType.BitwiseOr;
                            break;
                        case "xor":
                            binaryOperator = BinaryOperatorType.ExclusiveOr;
                            break;
                        case "+":
                            binaryOperator = BinaryOperatorType.Add;
                            break;
                        case "-":
                            binaryOperator = BinaryOperatorType.Subtract;
                            break;
                        case "/":
                            binaryOperator = BinaryOperatorType.Divide;
                            break;
                        case "*":
                            binaryOperator = BinaryOperatorType.Multiply;
                            break;
                        case "mod":
                            binaryOperator = BinaryOperatorType.Modulus;
                            break;
                        case ">":
                            binaryOperator = BinaryOperatorType.GreaterThan;
                            break;
                        case "<":
                            binaryOperator = BinaryOperatorType.LessThan;
                            break;
                        case ">=":
                            binaryOperator = BinaryOperatorType.GreaterThanOrEqual;
                            break;
                        case "<=":
                            binaryOperator = BinaryOperatorType.LessThanOrEqual;
                            break;
                        case "=":
                            binaryOperator = BinaryOperatorType.Equality;
                            break;
                        case "/=":
                            binaryOperator = BinaryOperatorType.InEquality;
                            break;
                        case "not":
                            unaryOperator = UnaryOperatorType.Not;
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

                    var dpd = decimal.Parse(
                        csvReader.GetField<string>("DPD").Replace(',', '.'), // Taking care of decimal commas.
                        NumberStyles.Any, 
                        CultureInfo.InvariantCulture);

                    if (binaryOperator.HasValue)
                    {
                        timingReport.SetLatencyNs(binaryOperator.Value, operandSizeBits, isSigned, dpd); 
                    }
                    else if (unaryOperator.HasValue)
                    {
                        timingReport.SetLatencyNs(unaryOperator.Value, operandSizeBits, isSigned, dpd);
                    }
                }

                return timingReport;
            }
        }


        private class TimingReport : ITimingReport
        {
            private readonly Dictionary<string, decimal> _timings = new Dictionary<string, decimal>();


            public void SetLatencyNs(dynamic operatorTyper, int operandSizeBits, bool isSigned, decimal timing)
            {
                _timings[GetKey(operatorTyper, operandSizeBits, isSigned)] = timing;

                // If the operand type is 1 that means that the operation also works with single-bit non-composite types
                // where the latter may not have an explicit size. E.g. and std_logic_vector1 would be the same as 
                // std_logic but the latter is not a sized data type (and thus it's apparent size will be 0, despite it
                // being stored in at least one bit).
                // Therefore, saving a 0 bit version here too.
                if (operandSizeBits == 1)
                {
                    SetLatencyNs(operatorTyper, 0, isSigned, timing);
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
