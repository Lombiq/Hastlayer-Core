
namespace Hast.VhdlBuilder.Representation.Declaration
{
    public abstract class DataObjectBase : IDataObject
    {
        public DataObjectKind DataObjectKind { get; set; }
        public string Name { get; set; }


        public abstract string ToVhdl(IVhdlGenerationOptions vhdlGenerationOptions);
    }
}
