
namespace Hast.VhdlBuilder.Representation.Declaration
{
    public class Identifier : DataType
    {
        public Identifier()
        {
            TypeCategory = DataTypeCategory.Identifier;
        }


        public override string ToVhdl()
        {
            return string.Empty;
        }
    }
}
