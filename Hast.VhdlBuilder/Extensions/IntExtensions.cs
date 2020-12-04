using Hast.VhdlBuilder.Representation.Declaration;
using Hast.VhdlBuilder.Representation.Expression;
using System.Globalization;

namespace Hast.VhdlBuilder.Extensions
{
    public static class IntExtensions
    {
        public static Value ToVhdlValue(this int valueInt, DataType dataType) =>
            valueInt.ToString(CultureInfo.InvariantCulture).ToVhdlValue(dataType);
    }
}
