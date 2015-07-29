using System.IO;

namespace Hast.Communication.Helpers
{
    public static class CommunicationHelpers
    {
        public static byte[] ConvertIntToByteArray(int from)
        {
            var stream = new MemoryStream();
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write(from);
            }
            return stream.ToArray();
        }
    }
}
