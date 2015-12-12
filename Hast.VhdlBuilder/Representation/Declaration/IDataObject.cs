
namespace Hast.VhdlBuilder.Representation.Declaration
{
    public enum DataObjectKind
    {
        Constant,
        Variable,
        Signal,
        File
    }


    public interface IDataObject : INamedElement
    {
        DataObjectKind DataObjectKind { get; set; }
    }
}
