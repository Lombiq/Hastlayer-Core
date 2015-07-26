using System.IO;

namespace Hast.Communication.Helpers
{
    public static class CommunicationHelpers
    {
        public static byte[] ConvertIntToByteArray(int from)
        {
            MemoryStream stream = new MemoryStream();
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                writer.Write(from);
            }
            return stream.ToArray();
        }
    }
}
