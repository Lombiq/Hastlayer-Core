﻿using Hast.VhdlBuilder.Representation.Declaration;
using Hast.VhdlBuilder.Representation.Expression;

namespace Hast.VhdlBuilder.Extensions
{
    public static class IntExtensions
    {
        public static Value ToVhdlValue(this int valueInt, DataType dataType) =>
            valueInt.ToString().ToVhdlValue(dataType);
    }
}
