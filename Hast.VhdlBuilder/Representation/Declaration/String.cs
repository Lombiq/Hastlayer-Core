﻿using System.Diagnostics;

namespace Hast.VhdlBuilder.Representation.Declaration
{
    [DebuggerDisplay("{ToVhdl(VhdlGenerationOptions.Debug)}")]
    public class String : DataType
    {
        public int Length { get; set; }


        public String()
        {
            TypeCategory = DataTypeCategory.Array;
        }


        public override string ToVhdl(IVhdlGenerationOptions vhdlGenerationOptions) => "string(1 to " + Length + ")";
    }
}
