﻿using System.Diagnostics;

namespace Hast.VhdlBuilder.Representation.Declaration
{
    [DebuggerDisplay("{ToVhdl(VhdlGenerationOptions.Debug)}")]
    public class StdLogicVector : SizedDataType
    {
        public StdLogicVector(DataType baseType) : base(baseType)
        {
        }

        public StdLogicVector(SizedDataType previous)
            : base(previous)
        {
            Size = previous.Size;
            SizeExpression = previous.SizeExpression;
        }

        public StdLogicVector()
        {
            Name = "std_logic_vector";
            TypeCategory = DataTypeCategory.Array;
        }
    }
}