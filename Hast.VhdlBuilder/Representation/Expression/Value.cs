using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Hast.VhdlBuilder.Representation.Declaration;

namespace Hast.VhdlBuilder.Representation.Expression
{
    [DebuggerDisplay("{ToVhdl(VhdlGenerationOptions.Debug)}")]
    public class Value : IVhdlElement
    {
        // These below need to be Lazy, because otherwise there would be a circular dependency between the static
        // ctors with KnownDataTypes (since it uses ToVhdlValue() and thus this class a lot for default values).
        private static readonly Lazy<Value> _trueLazy = new Lazy<Value>(() => new Value { DataType = KnownDataTypes.Boolean, Content = "true" });
        public static Value True { get { return _trueLazy.Value; } }
        private static readonly Lazy<Value> _falseLazy = new Lazy<Value>(() => new Value { DataType = KnownDataTypes.Boolean, Content = "false" });
        public static Value False { get { return _falseLazy.Value; } }
        private static readonly Lazy<Value> _zeroCharacterLazy = new Lazy<Value>(() => new Character('0'));
        public static Value ZeroCharacter { get { return _zeroCharacterLazy.Value; } }
        private static readonly Lazy<Value> _oneCharacterLazy = new Lazy<Value>(() => new Character('1'));
        public static Value OneCharacter { get { return _oneCharacterLazy.Value; } }

        public DataType DataType { get; set; }
        public string Content { get; set; }
        public IVhdlElement EvaluatedContent { get; set; }


        public string ToVhdl(IVhdlGenerationOptions vhdlGenerationOptions)
        {
            var content = EvaluatedContent != null ? EvaluatedContent.ToVhdl(vhdlGenerationOptions) : Content;

            if (DataType == null) return content;

            // Handling signed and unsigned types specially.
            if (KnownDataTypes.Integers.Contains(DataType))
            {
                // Using ToVhdlValue() on content would cause a stack overflow, since this would be called again. So
                // this needs to be the root of the Value callchain.
                var value = new Raw(content);
                var size = ((SizedDataType)DataType).Size;

                return (DataType.Name == KnownDataTypes.UInt32.Name ?
                    Invocation.ToUnsigned(value, size) :
                    Invocation.ToSigned(value, size)).ToVhdl(vhdlGenerationOptions);
            }

            if (DataType == KnownDataTypes.Real) return Invocation.ToReal(new Raw(content)).ToVhdl(vhdlGenerationOptions);

            if (DataType.TypeCategory == DataTypeCategory.Scalar ||
                DataType.TypeCategory == DataTypeCategory.Unit) return content;

            if (DataType.TypeCategory == DataTypeCategory.Identifier) return vhdlGenerationOptions.ShortenName(content);

            if (DataType.TypeCategory == DataTypeCategory.Array)
            {
                if (content.Contains("others =>"))
                {
                    return "(" + content + ")";
                }
                else if (DataType.IsLiteralArrayType())
                {
                    return "\"" + content + "\"";
                }
                else if (EvaluatedContent is IBlockElement)
                {
                    content = ((IBlockElement)EvaluatedContent).Body.ToVhdl(vhdlGenerationOptions, ", ", string.Empty);
                }

                return "(" + content + ")";
            }

            if (DataType.TypeCategory == DataTypeCategory.Character) return "'" + content + "'";

            return content;
        }
    }
}
