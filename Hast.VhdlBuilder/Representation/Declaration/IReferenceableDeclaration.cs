namespace Hast.VhdlBuilder.Representation.Declaration
{
    /// <summary>
    /// Represents a VHDL element that is a declaration which can be referenced from other places. E.g. a variable
    /// declaration can be referenced in a variable assignment.
    /// </summary>
    public interface IReferenceableDeclaration : IVhdlElement
    {
    }

    /// <summary>
    /// Represents a VHDL element that is <see cref="IReferenceableDeclaration"/> and also can be referenced as an
    /// <see cref="IVhdlElement"/>.
    /// </summary>
    /// <typeparam name="T">The type of the reference.</typeparam>
    public interface IReferenceableDeclaration<out T> : IReferenceableDeclaration
        where T : IVhdlElement
    {
        T ToReference();
    }
}
