using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.Transformer.Abstractions.SimpleMemory;

namespace Hast.TestInputs.Invalid
{
    public class InvalidSimpleMemoryUsingCases
    {
        public void BatchedReadCountIsNotConstant(SimpleMemory memory)
        {
            var count = memory.ReadInt32(0);
            var numbers = memory.ReadInt32(1, count);
        }
    }
}
