using Hast.Layer;

namespace Hast.Samples.Compression.Services.Lzma.Exceptions
{
    /// <summary>
    /// The exception that is thrown when an error in input stream occurs during decoding.
    /// </summary>
    internal class LzmaDataErrorException : HastlayerException
    {
        public LzmaDataErrorException() : base("LZMA data error.") { }
    }
}
