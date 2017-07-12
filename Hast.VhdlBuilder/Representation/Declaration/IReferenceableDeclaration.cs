namespace Hast.VhdlBuilder.Representation.Declaration
{
    /// <summary>
    /// Represents a VHDL element that is a declaration which can be referenced from other places. E.g. a variable
    /// declaration can be referenced in a variable assignment.
    /// </summary>
    public interface IReferenceableDeclaration : IVhdlElement
    {
    }


    public interface IReferenceableDeclaration<T> : IReferenceableDeclaration where T : IVhdlElement
    {
        T ToReference();
    }
}
