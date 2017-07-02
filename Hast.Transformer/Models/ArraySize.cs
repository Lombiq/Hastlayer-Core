using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hast.Transformer.Models
{
    [DebuggerDisplay("{ToString()}")]
    // Used in ArraySizeHolder.
    internal class ArraySize : IArraySize
    {
        public int Length { get; set; }


        public override string ToString() => "Length: " + Length.ToString();
    }
}
