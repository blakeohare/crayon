using Build;
using Common;
using Parser;
using System.Collections.Generic;
using System.Linq;

namespace Exporter.Workers
{
    public class GenerateCbxFileContentWorker
    {
        public byte[] GenerateCbxBinaryData(BuildContext buildContext, ResourceDatabase resDb, CompilationBundle compilationResult, string byteCode)
        {
            List<byte> cbxOutput = new List<byte>() { 0 };
            cbxOutput.AddRange("CBX".ToCharArray().Select(c => (byte)c));
            cbxOutput.AddRange(GetBigEndian4Byte(0));
            cbxOutput.AddRange(GetBigEndian4Byte(2));
            cbxOutput.AddRange(GetBigEndian4Byte(0));

            byte[] code = StringToBytes(byteCode);
            cbxOutput.AddRange("CODE".ToCharArray().Select(c => (byte)c));
            cbxOutput.AddRange(GetBigEndian4Byte(code.Length));
            cbxOutput.AddRange(code);

            List<string> libraries = new List<string>();
            foreach (LibraryCompilationScope scopeForLibrary in compilationResult.LibraryScopesUsed.Where(scope => scope.Library.IsMoreThanJustEmbedCode))
            {
                libraries.Add(scopeForLibrary.Library.ID);
                libraries.Add(scopeForLibrary.Library.Version);
            }
            string libsData = string.Join(",", libraries);
            byte[] libsDataBytes = StringToBytes(libsData);
            cbxOutput.AddRange("LIBS".ToCharArray().Select(c => (byte)c));
            cbxOutput.AddRange(GetBigEndian4Byte(libsDataBytes.Length));
            cbxOutput.AddRange(libsDataBytes);

            byte[] resourceManifest = StringToBytes(resDb.ResourceManifestFile.TextContent);
            cbxOutput.AddRange("RSRC".ToCharArray().Select(c => (byte)c));
            cbxOutput.AddRange(GetBigEndian4Byte(resourceManifest.Length));
            cbxOutput.AddRange(resourceManifest);

            if (resDb.ImageSheetManifestFile != null)
            {
                byte[] imageSheetManifest = StringToBytes(resDb.ImageSheetManifestFile.TextContent);
                cbxOutput.AddRange("IMSH".ToCharArray().Select(c => (byte)c));
                cbxOutput.AddRange(GetBigEndian4Byte(imageSheetManifest.Length));
                cbxOutput.AddRange(imageSheetManifest);
            }

            return cbxOutput.ToArray();
        }

        private static byte[] GetBigEndian4Byte(int value)
        {
            return new byte[]
            {
                (byte) ((value >> 24) & 255),
                (byte) ((value >> 16) & 255),
                (byte) ((value >> 8) & 255),
                (byte) (value & 255)
            };
        }

        private static byte[] StringToBytes(string value)
        {
            return System.Text.Encoding.UTF8.GetBytes(value);
        }
    }
}
