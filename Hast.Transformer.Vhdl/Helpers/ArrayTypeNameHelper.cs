namespace Hast.Transformer.Vhdl.Helpers
{
    internal static class ArrayTypeNameHelper
    {
        public static string CreateArrayTypeName(string elementTypeName)
        {
            return elementTypeName + "_Array";
        }
    }
}
