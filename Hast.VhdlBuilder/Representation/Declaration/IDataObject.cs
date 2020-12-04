
namespace Hast.VhdlBuilder.Representation.Declaration
{
    public enum DataObjectKind
    {
        Constant,
        Variable,
        Signal,
        File
    }

    public interface IDataObject : INamedElement, IReferenceableDeclaration<IDataObject>
    {
        DataObjectKind DataObjectKind { get; set; }
    }
}
