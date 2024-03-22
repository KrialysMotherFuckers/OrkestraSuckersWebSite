using System.IO.Compression;
using System.Text;

namespace Krialys.Common.Extensions;

/// <summary>
/// Helper to compress/decompress from a byte array
/// </summary>
public static class GZipExtensions
{
    public static byte[] Compress(byte[] uncompressedBytes)
    {
        using (MemoryStream compressedStream = new MemoryStream())
        {
            using (GZipStream gzipStream = new GZipStream(compressedStream, CompressionMode.Compress, true))
                gzipStream.Write(uncompressedBytes, 0, uncompressedBytes.Length);

            return compressedStream.ToArray();
        }
    }

    public static byte[] Decompress(byte[] compressedBytes)
    {
        using (MemoryStream compressedStream = new MemoryStream(compressedBytes))
        {
            using (GZipStream gzipStream = new GZipStream(compressedStream, CompressionMode.Decompress))
            {
                using (MemoryStream decompressedStream = new MemoryStream())
                {
                    gzipStream.CopyTo(decompressedStream);

                    return decompressedStream.ToArray();
                }
            }
        }
    }
}