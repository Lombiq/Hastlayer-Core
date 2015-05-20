
using Hast.VhdlBuilder.Representation.Declaration;
namespace Hast.Transformer.Vhdl.Constants
{
    internal static class SimpleMemoryNames
    {
        public const string DataInPort = @"\DataIn\";
        public const string DataOutPort = @"\DataOut\";
        public const string ReadAddressPort = @"\ReadAddress\";
        public const string WriteAddressPort = @"\WriteAddress\";

        // Aliases for the port signals to be used in local scopes (e.g. inside procedures).
        public const string DataInLocal = @"\DataInLocal\";
        public const string DataOutLocal = @"\DataOutLocal\";
        public const string ReadAddressLocal = @"\ReadAddressLocal\";
        public const string WriteAddressLocal = @"\WriteAddressLocal\";
    }
}
