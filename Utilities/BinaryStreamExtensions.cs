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

        /// <summary>
        ///     Writes a length-prefixed Gzipped string.
        /// </summary>
        public static void WriteCompressedString(this BinaryWriter writer, string str, Encoding encoding)
        {
            byte[] zippedBytes;

            using (var ms = new MemoryStream())
            {
                using (var zipOut = new GZipOutputStream(ms))
                {
                    using (var sw = new StreamWriter(zipOut, encoding))
                    {
                        sw.Write(str);

                        sw.Flush();
                        zipOut.Finish();

                        var bytes = new byte[ms.Length];
                        ms.Seek(0, SeekOrigin.Begin);
                        ms.Read(bytes, 0, bytes.Length);

                        zippedBytes = bytes;
                    }
                }
            }

            Write7BitEncodedInt(writer, zippedBytes.Length);
            writer.Write(zippedBytes);
        }

        /// <summary>
        ///     Reads a length-prefixed Gzipped string.
        /// </summary>
        public static string ReadCompressedString(this BinaryReader reader, Encoding encoding)
        {
            var length = Read7BitEncodedInt(reader);

            if (length == 0)
                return string.Empty;

            using (Stream ms = new MemoryStream(reader.ReadBytes(length)))
            {
                using (var zipInput = new GZipInputStream(ms))
                {
                    using (var sr = new StreamReader(zipInput, encoding))
                    {
                        return sr.ReadToEnd();
                    }
                }
            }
        }
    }
}