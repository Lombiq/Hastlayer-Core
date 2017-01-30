using Hast.VhdlBuilder.Representation.Declaration;
using Hast.VhdlBuilder.Extensions;

namespace Hast.Transformer.Vhdl.Helpers
{
    internal static class ArrayHelper
    {
        public static string CreateArrayTypeName(string elementTypeName)
        {
            return (elementTypeName.TrimExtendedVhdlIdDelimiters() + "_Array").ToExtendedVhdlId();
        }

        public static UnconstrainedArrayInstantiation CreateArrayInstantiation(DataType elementType, int length)
        {
            return new UnconstrainedArrayInstantiation
            {
                Name = CreateArrayTypeName(elementType.Name),
                ElementType = elementType,
                RangeFrom = 0,
                RangeTo = length - 1
            };
        }
    }
}
