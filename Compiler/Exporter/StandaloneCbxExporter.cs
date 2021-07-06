using Common;
using CommonUtil;
using CommonUtil.Disk;
using System.Collections.Generic;
using System.Linq;

namespace Exporter
{
    public static class StandaloneCbxExporter
    {
        public static string Run(
            string projectId,
            Dictionary<string, FileOutput> fileOutputContext,
            string outputDirectory,
            string byteCode,
            string resourceManifest,
            string imageManifest2Text)
        {
            byte[] cbxFileBytes = GenerateCbxBinaryData(
                resourceManifest,
                imageManifest2Text,
                byteCode);

            fileOutputContext[projectId + ".cbx"] = new FileOutput()
            {
                Type = FileOutputType.Binary,
                BinaryContent = cbxFileBytes,
            };

            ExportUtil.EmitFilesToDisk(fileOutputContext, outputDirectory);
            string absoluteCbxFilePath = GetCbxFileLocation(outputDirectory, projectId);
            return absoluteCbxFilePath;
        }

        private static string GetCbxFileLocation(
            string fullyQualifiedOutputFolder,
            string projectId)
        {
            string cbxPath = FileUtil.JoinPath(fullyQualifiedOutputFolder, projectId + ".cbx");
            cbxPath = FileUtil.GetCanonicalizeUniversalPath(cbxPath);
            return cbxPath;
        }

        private static byte[] GenerateCbxBinaryData(
            string resourceManifestText,
            string imageManifest2Text,
            string byteCode)
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

            byte[] resourceManifest = StringUtil.ToUtf8Bytes(resourceManifestText);
            cbxOutput.AddRange("RSRC".ToCharArray().Select(c => (byte)c));
            cbxOutput.AddRange(GetBigEndian4Byte(resourceManifest.Length));
            cbxOutput.AddRange(resourceManifest);

            if (imageManifest2Text != null)
            {
                byte[] imageManifest = StringUtil.ToUtf8Bytes(imageManifest2Text);
                cbxOutput.AddRange("IMGS".ToCharArray().Select(c => (byte)c));
                cbxOutput.AddRange(GetBigEndian4Byte(imageManifest.Length));
                cbxOutput.AddRange(imageManifest);
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
