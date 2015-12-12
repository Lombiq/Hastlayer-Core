
namespace Hast.VhdlBuilder.Representation
{
    /// <summary>
    /// Represents an entity in a VHDL source
    /// </summary>
    /// <remarks>
    /// It's basically anything in VHDL.
    /// </remarks>
    public interface IVhdlElement
    {
        string ToVhdl();
    }
}
