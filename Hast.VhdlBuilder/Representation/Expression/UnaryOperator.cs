﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hast.VhdlBuilder.Representation.Expression
{
    [DebuggerDisplay("{ToVhdl(VhdlGenerationOptions.Debug)}")]
    public class UnaryOperator : IVhdlElement
    {
        private readonly string _source;

        public static readonly UnaryOperator Identity = new UnaryOperator("+");
        public static readonly UnaryOperator Negation = new UnaryOperator("-");


        private UnaryOperator(string source)
        {
            _source = source;
        }


        public string ToVhdl(IVhdlGenerationOptions vhdlGenerationOptions)
        {
            return _source;
        }
    }
}