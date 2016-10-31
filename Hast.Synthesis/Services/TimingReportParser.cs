using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
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
                    BinaryOperatorType binaryOperator;
                    switch (operatorString)
                    {
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

                    var operandTypes = csvReader.GetField<string>("InType");
                    var isSigned = operandTypes.StartsWith("signed");
                    var operandSizeBits = ushort.Parse(isSigned ? operandTypes.Substring(6) : operandTypes.Substring(8));

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
                return _timings[GetKey(binaryOperator, operandSizeBits, isSigned)];
            }


            private static string GetKey(BinaryOperatorType binaryOperator, ushort operandSizeBits, bool isSigned)
            {
                return binaryOperator.ToString() + operandSizeBits.ToString() + isSigned.ToString();
            }
        }
    }
}
