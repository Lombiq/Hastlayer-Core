using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hast.VhdlBuilder.Representation.Expression
{
    public class Operator : IVhdlElement
    {
        private readonly string _source;

        public static readonly Operator Add = new Operator("+");
        public static readonly Operator Divide = new Operator("/");
        public static readonly Operator Equality = new Operator("=");
        public static readonly Operator ExclusiveOr = new Operator("XOR");
        public static readonly Operator GreaterThan = new Operator(">");
        public static readonly Operator GreaterThanOrEqual = new Operator(">=");
        public static readonly Operator InEquality = new Operator("/=");
        public static readonly Operator LessThan = new Operator("<");
        public static readonly Operator LessThanOrEqual = new Operator("<=");
        public static readonly Operator Modulus = new Operator("mod");
        public static readonly Operator Multiply = new Operator("*");
        public static readonly Operator ShiftLeft = new Operator("sll");
        public static readonly Operator ShiftRight = new Operator("srl");
        public static readonly Operator Subtract = new Operator("-");


        private Operator(string source)
        {
            _source = source;
        }


        public string ToVhdl(IVhdlGenerationOptions vhdlGenerationOptions)
        {
            return _source;
        }
    }
}
