﻿using System.Diagnostics;
using Hast.VhdlBuilder.Representation.Expression;

namespace Hast.VhdlBuilder.Representation.Declaration
{
    [DebuggerDisplay("{ToVhdl(VhdlGenerationOptions.Debug)}")]
    public class TypedDataObject : TypedDataObjectBase
    {
        /// <summary>
        /// If specified, this default value will be used to reset the object's value.
        /// </summary>
        public Value DefaultValue { get; set; }


        public override string ToVhdl(IVhdlGenerationOptions vhdlGenerationOptions)
        {
            return Terminated.Terminate(
                DataObjectKind.ToString() +
                " " +
                vhdlGenerationOptions.ShortenName(Name) +
                (DataType != null ? ": " + DataType.ToReferenceVhdl() : string.Empty) +
                (DefaultValue != null ? ((DataObjectKind == DataObjectKind.Variable ? " := " : " <= ") + DefaultValue.ToVhdl(vhdlGenerationOptions)) : string.Empty),
                vhdlGenerationOptions);
        }
    }
}
