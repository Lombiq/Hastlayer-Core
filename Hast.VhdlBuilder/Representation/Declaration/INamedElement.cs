
namespace Hast.VhdlBuilder.Representation.Declaration
{
    public interface INamedElement : IVhdlElement
    {
        string Name { get; set; }
    }
}
