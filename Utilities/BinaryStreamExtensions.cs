#region

using System;
using System.IO;
using System.Text;
using ICSharpCode.SharpZipLib.GZip;

#endregion

namespace Tabster.Data.Utilities
{
    internal static class BinaryStreamExtensions
    {
        #region String Encoding

        public static string ReadString(this BinaryReader reader, Encoding encoding)
        {
            var length = Read7BitEncodedInt(reader);

            if (length == 0)
                return String.Empty;

            var bytes = reader.ReadBytes(length);
            return encoding.GetString(bytes);
        }

        public static void Write(this BinaryWriter writer, string str, Encoding encoding)
        {
            var bytes = encoding.GetBytes(str);
            var length = encoding.GetByteCount(str);
            Write7BitEncodedInt(writer, length);
            writer.Write(bytes);
        }

        private static int Read7BitEncodedInt(BinaryReader reader)
        {
            var count = 0;
            var shift = 0;
            byte b;
            do
            {
                b = reader.ReadByte();
                count |= (b & 0x7F) << shift;
                shift += 7;
            } while ((b & 0x80) != 0);
            return count;
        }

        private static void Write7BitEncodedInt(BinaryWriter writer, int value)
        {
            var v = (uint) value;
            while (v >= 0x80)
            {
                writer.Write((byte) (v | 0x80));
                v >>= 7;
            }
            writer.Write((byte) v);
        }

        #endregion

        #region String Compression

        /// <summary>
        ///     Writes a length-prefixed Gzipped string.
        /// </summary>
        public static void WriteCompressedString(this BinaryWriter writer, string str, Encoding encoding)
        {
            var zipped = ZipText(str, encoding);
            Write7BitEncodedInt(writer, zipped.Length);
            writer.Write(zipped);
        }

        /// <summary>
        ///     Reads a length-prefixed Gzipped string.
        /// </summary>
        public static string ReadCompressedString(this BinaryReader reader, Encoding encoding)
        {
            var length = Read7BitEncodedInt(reader);
            var zipped = reader.ReadBytes(length);
            return Unzip(zipped, encoding);
        }

        private static byte[] ZipText(string text, Encoding encoding)
        {
            if (text == null)
                return null;

            var textBytes = encoding.GetBytes(text);

            using (var ms = new MemoryStream())
            {
                using (var zipOut = new GZipOutputStream(ms))
                {
                    using (var writer = new StreamWriter(zipOut))
                    {
                        writer.Write(textBytes);

                        writer.Flush();
                        zipOut.Finish();

                        var bytes = new byte[ms.Length];
                        ms.Seek(0, SeekOrigin.Begin);
                        ms.Read(bytes, 0, bytes.Length);

                        return ms.ToArray();
                    }
                }
            }
        }

        private static string Unzip(byte[] bytes, Encoding encoding)
        {
            if (bytes == null)
                return null;

            using (var ms = new MemoryStream(bytes))
            {
                using (var zipInput = new GZipInputStream(ms))
                {
                    var buffer = new byte[zipInput.Length];

                    int read;
                    while ((read = zipInput.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        ms.Write(buffer, 0, read);
                    }

                    return encoding.GetString(ms.ToArray());
                }
            }

            #endregion
        }
    }
}