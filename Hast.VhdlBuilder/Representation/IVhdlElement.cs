
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
        string ToVhdl(IVhdlGenerationContext vhdlGenerationContext);
    }


    public static class VhdlElementExtensions
    {
        //public static string ToVhdl(this IVhdlElement vhdlElement)
        //{
        //    return vhdlElement.ToVhdl(new VhdlGenerationContext());
        //}

        public static string ToVhdl(this IVhdlElement vhdlElement, IVhdlGenerationOptions vhdlGenerationOptions)
        {
            return vhdlElement.ToVhdl(new VhdlGenerationContext(vhdlGenerationOptions));
        }
    }
}
