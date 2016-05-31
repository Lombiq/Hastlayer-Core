using Hast.VhdlBuilder.Representation.Declaration;
using Hast.VhdlBuilder.Extensions;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Hast.VhdlBuilder.Representation.Expression
{
    [DebuggerDisplay("{ToVhdl(VhdlGenerationOptions.Debug)}")]
    public class Value : IVhdlElement
    {
        public static readonly Value True = "true".ToVhdlIdValue();
        public static readonly Value False = "false".ToVhdlIdValue();

        // These below need to be Lazy, because otherwise there would be a circular dependency between the static
        // ctors with KnownDataTypes (since it uses ToVhdlValue() and thus this class a lot for default values).
        private static readonly Lazy<Value> _zeroCharacterLazy = new Lazy<Value>(() => new Character('0'));
        public static Value ZeroCharacter { get { return _zeroCharacterLazy.Value; } }
        private static readonly Lazy<Value> _oneCharacterLazy = new Lazy<Value>(() => new Character('1'));
        public static Value OneCharacter { get { return _oneCharacterLazy.Value; } }

        public DataType DataType { get; set; }
        public string Content { get; set; }


        public string ToVhdl(IVhdlGenerationOptions vhdlGenerationOptions)
        {
            if (DataType == null) return Content;

            // Handling signed and unsigned types specially.
            if (KnownDataTypes.Integers.Contains(DataType))
            {
                var conversionFunctionName = DataType.Name == KnownDataTypes.UInt32.Name ? "to_unsigned" : "to_signed";
                var size = ((SizedDataType)DataType).Size;

                return new Invocation
                {
                    Target = conversionFunctionName.ToVhdlIdValue(),
                    Parameters = new List<IVhdlElement>
                    {
                        { new Raw(Content) },
                        { size.ToVhdlValue(KnownDataTypes.UnrangedInt) }
                    }
                }.ToVhdl(vhdlGenerationOptions);
            }

            if (DataType.TypeCategory == DataTypeCategory.Numeric || 
                DataType.TypeCategory == DataTypeCategory.Unit) return Content;

            if (DataType.TypeCategory == DataTypeCategory.Identifier) return vhdlGenerationOptions.ShortenName(Content);

            if (DataType.TypeCategory == DataTypeCategory.Array)
            {
                if (DataType.IsLiteralArrayType()) return "\"" + Content + "\"";
                else return "(" + Content + ")";
            }

            if (DataType.TypeCategory == DataTypeCategory.Character) return "'" + Content + "'";

            return Content;
        }
    }
}
