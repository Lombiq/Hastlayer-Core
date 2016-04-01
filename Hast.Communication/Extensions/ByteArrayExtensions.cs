namespace System
{
    internal static class ByteArrayExtensions
    {
        public static byte[] Append(this byte[] originalBytes, byte[] additionalBytes)
        {
            var newByteArray = new byte[originalBytes.Length + additionalBytes.Length];

            Array.Copy(originalBytes, 0, newByteArray, 0, originalBytes.Length);
            Array.Copy(additionalBytes, 0, newByteArray, originalBytes.Length, additionalBytes.Length);

            return newByteArray;
        }
    }
}
