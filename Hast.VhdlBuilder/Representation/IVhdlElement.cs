namespace Hast.VhdlBuilder.Representation
{
    /// <summary>
    /// Represents an entity in a VHDL source.
    /// </summary>
    /// <remarks>
    /// <para>It's basically anything in VHDL.</para>
    /// </remarks>
    public interface IVhdlElement
    {
        string ToVhdl(IVhdlGenerationOptions vhdlGenerationOptions);
    }

    public static class VhdlElementExtensions
    {
        public static string ToVhdl(this IVhdlElement vhdlElement)
        {
            if (vhdlElement == null) return null;
            return vhdlElement.ToVhdl(new VhdlGenerationOptions());
        }
    }
}
