using System.Diagnostics;
using System.Globalization;

namespace Hast.Transformer.Models
{
    [DebuggerDisplay("{ToString()}")]
    // Used in ArraySizeHolder.
    internal class ArraySize : IArraySize
    {
        public int Length { get; set; }

        public override string ToString() => "Length: " + Length.ToString(CultureInfo.InvariantCulture);
    }
}
