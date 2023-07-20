using System;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;

namespace GHPCommerce.CrossCuttingConcerns.ExtensionMethods
{
   public static class ByteExtension
    {
        public static T Deserialize<T>( this byte[] stream)
        {
            if (stream == null)
                return (default);

            BinaryFormatter binaryFormatter = new BinaryFormatter();

            using (MemoryStream memoryStream = new MemoryStream(stream))
            {
                T result = (T)binaryFormatter.Deserialize(memoryStream);
                return result;
            }
        }
        public static async Task<byte[]> Decompress(this byte[] input)
        {
            using (var source = new MemoryStream(input))
            {
                byte[] lengthBytes = new byte[4];
                source.Read(lengthBytes, 0, 4);

                var length = BitConverter.ToInt32(lengthBytes, 0);
                using (var decompressionStream = new GZipStream(source,
                    CompressionMode.Decompress))
                {
                    var result = new byte[length];
                     await decompressionStream.ReadAsync(result, 0, length);
                    return result;
                }
            }
        }

        public static async Task<byte[]> Compress(this byte[] input)
        {
            using (var result = new MemoryStream())
            {
                var lengthBytes = BitConverter.GetBytes(input.Length);
                result.Write(lengthBytes, 0, 4);

                using (var compressionStream = new GZipStream(result,
                    CompressionMode.Compress))
                {
                    await compressionStream.WriteAsync(input, 0, input.Length);
                    await compressionStream.FlushAsync();

                }
                return result.ToArray();
            }
        }
    }
}
