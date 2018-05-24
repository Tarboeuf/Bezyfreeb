using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using CommonStandardLib;
using ICSharpCode.SharpZipLib.Zip;

namespace BezyFB.Helpers
{
    public static class DownloadSousTitreHelper
    {
        public static async Task<OperationResult<byte[]>> DownloadAndUnzip(string encoding, string sousTitre)
        {
            var wc = new WebClient();
            var st = await wc.DownloadDataTaskAsync(sousTitre);
            try
            {
                var st2 = UnzipFromStream(st, encoding);
                if (st2 != null)
                {
                    st = Encoding.Convert(GetEncoding(st2), Encoding.UTF8, st2);
                }
            }
            catch (Exception ex)
            {
                return OperationResult.CreateKo<byte[]>(ex);
            }
            return OperationResult.CreateOk(st);
        }

        private static byte[] UnzipFromStream(byte[] st, string encoding)
        {
            MemoryStream zipStream = new MemoryStream(st);
            var zipInputStream = new ZipInputStream(zipStream);

            if (!zipInputStream.CanRead)
                return null;
            ZipEntry zipEntry;
            try
            {
                zipEntry = zipInputStream.GetNextEntry();
            }
            catch (Exception)
            {
                return null;
            }
            if (zipEntry == null)
                return new byte[0];
            while (zipEntry != null)
            {
                String entryFileName = zipEntry.Name;

                if (entryFileName.Contains(".srt") && entryFileName.Contains(encoding))
                {
                    int fileSize = (int)zipEntry.Size;
                    byte[] blob = new byte[(int)zipEntry.Size];

                    while ((zipInputStream.Read(blob, 0, fileSize)) != 0)
                    {
                    }

                    //closing every thing
                    zipInputStream.Close();
                    return blob;
                }

                zipEntry = zipInputStream.GetNextEntry();
            }
            if (zipInputStream.CanSeek)
            {
                zipInputStream.Seek(0, SeekOrigin.Begin);
            }
            else
            {
                zipStream = new MemoryStream(st);
                zipInputStream = new ZipInputStream(zipStream);
            }
            zipEntry = zipInputStream.GetNextEntry();
            if (zipEntry == null)
                return new byte[0];
            while (true)
            {
                String entryFileName = zipEntry.Name;

                if (entryFileName.Contains(".srt"))
                {
                    int fileSize = (int)zipEntry.Size;
                    byte[] blob = new byte[(int)zipEntry.Size];

                    while ((zipInputStream.Read(blob, 0, fileSize)) != 0)
                    {
                    }

                    //closing every thing
                    zipInputStream.Close();
                    return blob;
                }

                zipEntry = zipInputStream.GetNextEntry();
            }
        }

        /// <summary>
        /// Determines a text file's encoding by analyzing its byte order mark (BOM).
        /// Defaults to ASCII when detection of the text file's endianness fails.
        /// </summary>
        /// <param name="bom">The text file to analyze.</param>
        /// <returns>The detected encoding.</returns>
        public static Encoding GetEncoding(byte[] bom)
        {
            // Analyze the BOM
            if (bom[0] == 0x2b && bom[1] == 0x2f && bom[2] == 0x76) return Encoding.UTF7;
            if (bom[0] == 0xef && bom[1] == 0xbb && bom[2] == 0xbf) return Encoding.UTF8;
            if (bom[0] == 0xff && bom[1] == 0xfe) return Encoding.Unicode; //UTF-16LE
            if (bom[0] == 0xfe && bom[1] == 0xff) return Encoding.BigEndianUnicode; //UTF-16BE
            if (bom[0] == 0 && bom[1] == 0 && bom[2] == 0xfe && bom[3] == 0xff) return Encoding.UTF32;
            return Encoding.ASCII;
        }
    }
}