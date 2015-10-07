﻿using System.Diagnostics;

namespace Hast.VhdlBuilder.Representation.Declaration
{
    [DebuggerDisplay("{ToVhdl(VhdlGenerationOptions.Debug)}")]
    public class StdLogicVector : SizedDataType
    {
        public StdLogicVector()
        {
            Name = "std_logic_vector";
            TypeCategory = DataTypeCategory.Array;
        }
    }
}
