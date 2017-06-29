using Hast.Layer;

namespace Hast.Samples.Compression.Services.Lzma.Exceptions
{
    /// <summary>
    /// The exception that is thrown when the value of an argument is outside the allowable range.
    /// </summary>
    internal class LzmaInvalidParamException : HastlayerException
    {
        public LzmaInvalidParamException() : base("LZMA - invalid parameter.") { }
    }
}
