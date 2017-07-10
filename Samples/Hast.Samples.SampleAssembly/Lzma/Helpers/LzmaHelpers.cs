namespace Hast.Samples.SampleAssembly.Lzma.Helpers
{
    public static class LzmaHelpers
    {
        public static uint GetMinValue(uint first, uint second) =>
            first <= second ? first : second;
    }
}
