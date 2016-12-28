using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
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

                    // "not" is a unary operator, the others are not directly applicable in .NET.
                    var skippedOperators = new[] { "not", "nand", "nor", "xnor" };
                    if (skippedOperators.Contains(operatorString))
                    {
                        continue;
                    }

                    BinaryOperatorType binaryOperator;
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
                        default:
                            throw new NotSupportedException("Unrecognized binary operator in timing report: " + operatorString);
                    }

                    var operandType = csvReader.GetField<string>("InType");
                    var isSigned = operandType.StartsWith("signed");
                    var operandSizeMatch = Regex.Match(operandType, "([0-9]+)", RegexOptions.Compiled);
                    if (!operandSizeMatch.Success)
                    {
                        throw new InvalidOperationException("The \"" + operandType + "\" operand type doesn't have a size.");
                    }
                    var operandSizeBits = ushort.Parse(operandSizeMatch.Groups[1].Value);

                    var dpd = decimal.Parse(csvReader.GetField<string>("DPD"), NumberStyles.AllowDecimalPoint);

                    timingReport.SetLatencyNs(binaryOperator, operandSizeBits, isSigned, dpd);
                }

                return timingReport;
            }
        }


        private class TimingReport : ITimingReport
        {
            private readonly Dictionary<string, decimal> _timings = new Dictionary<string, decimal>();


            public void SetLatencyNs(BinaryOperatorType binaryOperator, ushort operandSizeBits, bool isSigned, decimal timing)
            {
                _timings[GetKey(binaryOperator, operandSizeBits, isSigned)] = timing;
            }

            public decimal GetLatencyNs(BinaryOperatorType binaryOperator, ushort operandSizeBits, bool isSigned)
            {
                decimal latency;
                if (_timings.TryGetValue(GetKey(binaryOperator, operandSizeBits, isSigned), out latency))
                {
                    return latency;
                }

                return -1;
            }


            private static string GetKey(BinaryOperatorType binaryOperator, ushort operandSizeBits, bool isSigned)
            {
                return binaryOperator.ToString() + operandSizeBits.ToString() + isSigned.ToString();
            }
        }
    }
}
