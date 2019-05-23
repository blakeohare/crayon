using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace Interpreter.Libraries.Zip
{
    internal static class ZipHelper
    {
        public static object CreateZipReader(int[] bytesAsInts)
        {
            int length = bytesAsInts.Length;
            byte[] bytes = new byte[length];
            for (int i = 0; i < length; ++i)
            {
                bytes[i] = (byte)bytesAsInts[i];
            }
            ZipArchive zipArchive = new ZipArchive(new MemoryStream(bytes));
            return zipArchive;
        }

        private static byte[] BUFFER = new byte[400];

        public static void ReadNextZipEntry(object zipArchiveObj, int index, bool[] boolsOut, string[] nameOut, List<int> bytesOut)
        {
            boolsOut[0] = true;
            ZipArchive zipArchive = (ZipArchive)zipArchiveObj;
            IList<ZipArchiveEntry> entries = zipArchive.Entries;
            int length = entries.Count;
            bool isDone = index >= length;
            boolsOut[1] = !isDone;
            if (isDone) return;

            ZipArchiveEntry entry = entries[index];
            boolsOut[2] = false; // TODO: how to check this?
            nameOut[0] = entry.FullName;

            Stream byteStream = entry.Open();
            int bytesRead = 0;
            do
            {
                bytesRead = byteStream.Read(BUFFER, 0, BUFFER.Length);
                for (int i = 0; i < bytesRead; ++i)
                {
                    bytesOut.Add(BUFFER[i]);
                }
            } while (bytesRead > 0);
        }
    }
}
