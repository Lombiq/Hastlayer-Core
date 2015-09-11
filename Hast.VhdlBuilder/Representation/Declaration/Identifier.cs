
namespace Hast.VhdlBuilder.Representation.Declaration
{
    public class Identifier : DataType
    {
        public Identifier()
        {
            TypeCategory = DataTypeCategory.Identifier;
        }


        public override string ToVhdl(IVhdlGenerationContext vhdlGenerationContext)
        {
            return string.Empty;
        }
    }
}
