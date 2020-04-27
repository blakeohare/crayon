using Build;
using Common;
using CommonUtil.Disk;
using Exporter.Workers;
using System.Collections.Generic;

namespace Exporter.Pipeline
{
    public static class ExportStandaloneCbxPipeline
    {
        public static string Run(
            string projectId,
            Dictionary<string, FileOutput> fileOutputContext,
            string outputDirectory,
            string byteCode,
            IList<AssemblyResolver.AssemblyMetadata> assemblies,
            string resourceManifest,
            string imageSheetManifest)
        {
            byte[] cbxFileBytes = GenerateCbxFileContentWorker.GenerateCbxBinaryData(
                resourceManifest,
                imageSheetManifest,
                assemblies,
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

    }
}
