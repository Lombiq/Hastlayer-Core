﻿using System.Diagnostics;
using Hast.VhdlBuilder.Extensions;

namespace Hast.VhdlBuilder.Representation.Expression
{
    [DebuggerDisplay("{ToVhdl(VhdlGenerationOptions.Debug)}")]
    public class If<T> : IVhdlElement where T : IVhdlElement
    {
        public IVhdlElement Condition { get; set; }
        public T True { get; set; }


        public virtual string ToVhdl(IVhdlGenerationOptions vhdlGenerationOptions)
        {
            return Terminated.Terminate(
                "if (" + Condition.ToVhdl(vhdlGenerationOptions) + ") then " + vhdlGenerationOptions.NewLineIfShouldFormat() +
                    True.ToVhdl(vhdlGenerationOptions).IndentLinesIfShouldFormat(vhdlGenerationOptions) +
                "end if", vhdlGenerationOptions);
        }
    }


    [DebuggerDisplay("{ToVhdl(VhdlGenerationOptions.Debug)}")]
    public class If : If<IVhdlElement>
    {
    }
}
