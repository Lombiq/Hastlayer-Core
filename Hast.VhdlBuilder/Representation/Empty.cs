namespace Hast.VhdlBuilder.Representation
{
    /// <summary>
    /// Represents a no-op VHDL element.
    /// </summary>
    public class Empty : IVhdlElement
    {
        private static readonly Empty _instance = new Empty();
        public static Empty Instance { get { return _instance; } }


        private Empty()
        {
        }


        public string ToVhdl(IVhdlGenerationOptions vhdlGenerationOptions)
        {
            return string.Empty;
        }
    }
}
