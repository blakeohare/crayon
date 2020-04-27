using Build;
using Common;
using Exporter.Workers;
using System.Collections.Generic;

namespace Exporter.Pipeline
{
    public static class ExportStandaloneCbxPipeline
    {
        public static string Run(
            string byteCode,
            IList<AssemblyResolver.AssemblyMetadata> assemblies,
            BuildContext buildContext)
        {
            string projectId = buildContext.ProjectID;
            string outputDirectory = new GetOutputDirectoryWorker().DoWorkImpl(buildContext);
            ResourceDatabase resDb = ResourceDatabaseBuilder.PrepareResources(buildContext);
            byte[] cbxFileBytes = new GenerateCbxFileContentWorker().GenerateCbxBinaryData(resDb, assemblies, byteCode);
            Dictionary<string, FileOutput> fileOutputContext = new Dictionary<string, FileOutput>();
            new PopulateFileOutputContextForCbxWorker().GenerateFileOutput(fileOutputContext, projectId, resDb, cbxFileBytes);
            new EmitFilesToDiskWorker().DoWorkImpl(fileOutputContext, outputDirectory);
            string absoluteCbxFilePath = new GetCbxFileLocation().DoWorkImpl(outputDirectory, projectId);
            return absoluteCbxFilePath;
        }
    }
}
