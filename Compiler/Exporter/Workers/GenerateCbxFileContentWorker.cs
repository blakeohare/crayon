using Build;
using Common;
using CommonUtil;
using System.Collections.Generic;
using System.Linq;

namespace Exporter.Workers
{
    public class GenerateCbxFileContentWorker
    {
        public byte[] GenerateCbxBinaryData(BuildContext buildContext, ResourceDatabase resDb, ExportBundle compilationResult, string byteCode)
        {
            List<byte> cbxOutput = new List<byte>() { 0 };
            cbxOutput.AddRange("CBX".ToCharArray().Select(c => (byte)c));
            cbxOutput.AddRange(GetBigEndian4Byte(VersionInfo.VersionMajor));
            cbxOutput.AddRange(GetBigEndian4Byte(VersionInfo.VersionMinor));
            cbxOutput.AddRange(GetBigEndian4Byte(VersionInfo.VersionBuild));

            byte[] code = StringUtil.ToUtf8Bytes(byteCode);
            cbxOutput.AddRange("CODE".ToCharArray().Select(c => (byte)c));
            cbxOutput.AddRange(GetBigEndian4Byte(code.Length));
            cbxOutput.AddRange(code);

            List<string> libraries = new List<string>();
            foreach (AssemblyResolver.AssemblyMetadata libMetadata in compilationResult.LibraryAssemblies.Where(asm => asm.HasNativeCode))
            {
                libraries.Add(libMetadata.ID);
                libraries.Add(libMetadata.Version);
            }
            string libsData = string.Join(",", libraries);
            byte[] libsDataBytes = StringUtil.ToUtf8Bytes(libsData);
            cbxOutput.AddRange("LIBS".ToCharArray().Select(c => (byte)c));
            cbxOutput.AddRange(GetBigEndian4Byte(libsDataBytes.Length));
            cbxOutput.AddRange(libsDataBytes);

            byte[] resourceManifest = StringUtil.ToUtf8Bytes(resDb.ResourceManifestFile.TextContent);
            cbxOutput.AddRange("RSRC".ToCharArray().Select(c => (byte)c));
            cbxOutput.AddRange(GetBigEndian4Byte(resourceManifest.Length));
            cbxOutput.AddRange(resourceManifest);

            if (resDb.ImageSheetManifestFile != null)
            {
                byte[] imageSheetManifest = StringUtil.ToUtf8Bytes(resDb.ImageSheetManifestFile.TextContent);
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
    }
}
