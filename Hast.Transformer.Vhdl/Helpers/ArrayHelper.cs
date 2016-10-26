using Hast.VhdlBuilder.Representation.Declaration;

namespace Hast.Transformer.Vhdl.Helpers
{
    internal static class ArrayHelper
    {
        public static string CreateArrayTypeName(string elementTypeName)
        {
            return elementTypeName + "_Array";
        }

        public static UnconstrainedArrayInstantiation CreateArrayInstantiation(DataType elementType, int length)
        {
            return new UnconstrainedArrayInstantiation
            {
                Name = CreateArrayTypeName(elementType.Name),
                RangeFrom = 0,
                RangeTo = length - 1
            };
        }
    }
}
