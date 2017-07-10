﻿using Hast.VhdlBuilder.Extensions;
using Hast.VhdlBuilder.Representation;
using Hast.VhdlBuilder.Representation.Declaration;

namespace Hast.Transformer.Vhdl.Helpers
{
    internal static class ArrayHelper
    {
        public static string CreateArrayTypeName(string elementTypeName) =>
            (elementTypeName.TrimExtendedVhdlIdDelimiters() + "_Array").ToExtendedVhdlId();

        public static UnconstrainedArrayInstantiation CreateArrayInstantiation(DataType elementType, int length) =>
            new UnconstrainedArrayInstantiation
            {
                Name = CreateArrayTypeName(elementType.Name),
                ElementType = elementType,
                RangeFrom = 0,
                RangeTo = length - 1
            };
    }
}