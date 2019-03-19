using System.Diagnostics;
using System.Linq;
using Hast.VhdlBuilder.Extensions;
using Hast.VhdlBuilder.Representation.Expression;

namespace Hast.VhdlBuilder.Representation.Declaration
{
    [DebuggerDisplay("{ToVhdl(VhdlGenerationOptions.Debug)}")]
    public class StdLogicVector : SizedDataType
    {
        private Value _defaultValue;
        public override Value DefaultValue
        {
            get => _defaultValue == null ? "others => '0'".ToVhdlValue(this) : _defaultValue;
            set => _defaultValue = value;
        }


        public StdLogicVector(DataType baseType) : base(baseType)
        {
        }

        public StdLogicVector(SizedDataType previous)
            : base(previous)
        {
            Size = previous.Size;
            SizeExpression = previous.SizeExpression;
        }

        public StdLogicVector()
        {
            Name = "std_logic_vector";
            TypeCategory = DataTypeCategory.Array;
        }
    }
}
