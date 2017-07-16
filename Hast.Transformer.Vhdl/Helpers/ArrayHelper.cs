using Hast.VhdlBuilder.Extensions;
using Hast.VhdlBuilder.Representation.Declaration;

namespace Hast.Transformer.Vhdl.Helpers
{
    internal static class ArrayHelper
    {
        public static string CreateArrayTypeName(DataType elementType)
        {
            var elementSize = elementType.GetSize();

            return 
                (elementType.Name.TrimExtendedVhdlIdDelimiters() + 
                (elementSize != 0 ? elementSize.ToString() : string.Empty) + 
                "_Array")
                .ToExtendedVhdlId();
        }

        public static UnconstrainedArrayInstantiation CreateArrayInstantiation(DataType elementType, int length) =>
            new UnconstrainedArrayInstantiation
            {
                Name = CreateArrayTypeName(elementType),
                ElementType = elementType,
                RangeFrom = 0,
                RangeTo = length - 1
            };
    }
}
