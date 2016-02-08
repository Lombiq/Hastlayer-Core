using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hast.VhdlBuilder.Representation.Expression
{
    [DebuggerDisplay("{ToVhdl(VhdlGenerationOptions.Debug)}")]
    public class BinaryOperator : IVhdlElement
    {
        private readonly string _source;

        public static readonly BinaryOperator Add = new BinaryOperator("+");
        public static readonly BinaryOperator ConditionalAnd = new BinaryOperator("and");
        public static readonly BinaryOperator ConditionalOr = new BinaryOperator("or");
        public static readonly BinaryOperator Divide = new BinaryOperator("/");
        public static readonly BinaryOperator Equality = new BinaryOperator("=");
        public static readonly BinaryOperator ExclusiveOr = new BinaryOperator("xor");
        public static readonly BinaryOperator GreaterThan = new BinaryOperator(">");
        public static readonly BinaryOperator GreaterThanOrEqual = new BinaryOperator(">=");
        public static readonly BinaryOperator InEquality = new BinaryOperator("/=");
        public static readonly BinaryOperator LessThan = new BinaryOperator("<");
        public static readonly BinaryOperator LessThanOrEqual = new BinaryOperator("<=");
        public static readonly BinaryOperator Modulus = new BinaryOperator("mod");
        public static readonly BinaryOperator Multiply = new BinaryOperator("*");
        public static readonly BinaryOperator ShiftLeft = new BinaryOperator("sll");
        public static readonly BinaryOperator ShiftRight = new BinaryOperator("srl");
        public static readonly BinaryOperator Subtract = new BinaryOperator("-");


        private BinaryOperator(string source)
        {
            _source = source;
        }


        public string ToVhdl(IVhdlGenerationOptions vhdlGenerationOptions)
        {
            return _source;
        }
    }
}
