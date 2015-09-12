
namespace Hast.VhdlBuilder.Representation
{
    /// <summary>
    /// Represents an entity in a VHDL source.
    /// </summary>
    /// <remarks>
    /// It's basically anything in VHDL.
    /// </remarks>
    public interface IVhdlElement
    {
        string ToVhdl(IVhdlGenerationOptions vhdlGenerationOptions);
    }


    public static class VhdlElementExtensions
    {
        public static string ToVhdl(this IVhdlElement vhdlElement)
        {
            return vhdlElement.ToVhdl(new VhdlGenerationOptions());
        }
    }
}
