using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hast.VhdlBuilder.Representation.Declaration
{
    [DebuggerDisplay("{ToVhdl(VhdlGenerationOptions.Debug)}")]
    public class BitVector : SizedDataType
    {
        public BitVector(DataType baseType) : base(baseType)
        {
        }

        public BitVector(SizedDataType previous)
            : base(previous)
        {
            Size = previous.Size;
            SizeExpression = previous.SizeExpression;
        }

        public BitVector()
        {
            Name = "bit_vector";
            TypeCategory = DataTypeCategory.Array;
        }
    }
}
