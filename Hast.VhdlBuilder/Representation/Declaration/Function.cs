﻿using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Hast.VhdlBuilder.Extensions;

namespace Hast.VhdlBuilder.Representation.Declaration
{
    [DebuggerDisplay("{ToVhdl()}")]
    public class Function : ISubProgram
    {
        public string Name { get; set; }
        public List<FunctionArgument> Arguments { get; set; }
        public DataType ReturnType { get; set; }
        public List<IVhdlElement> Declarations { get; set; }
        public List<IVhdlElement> Body { get; set; }


        public Function()
        {
            Arguments = new List<FunctionArgument>();
            Declarations = new List<IVhdlElement>();
            Body = new List<IVhdlElement>();
        }


        public string ToVhdl()
        {
            return
                "function " +
                Name +
                " (" +
                string.Join("; ", Arguments.Select(parameter => parameter.ToVhdl())) +
                ") return " +
                ReturnType.Name +
                " is " +
                Declarations.ToVhdl() + (Declarations != null && Declarations.Any() ? " " : string.Empty) +
                "begin " +
                Body.ToVhdl() +
                " end " +
                Name +
                ";";
        }
    }


    public class FunctionArgument : TypedDataObjectBase
    {
        public override string ToVhdl()
        {
            return
                (DataObjectKind.ToString() ?? string.Empty) +
                Name +
                ": " +
                DataType.ToVhdl();
        }
    }
}
