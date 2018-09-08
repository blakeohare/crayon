using Build;
using Common;
using Exporter.ByteCode;
using Exporter.Workers;
using System.Collections.Generic;

namespace Exporter.Pipeline
{
    public static class ExportStandaloneCbxPipeline
    {
        public static string Run(ExportCommand command, BuildContext buildContext)
        {
            string outputDirectory = new GetOutputDirectoryWorker().DoWorkImpl(buildContext);
            ExportBundle compilationResult = ExportBundle.Compile(buildContext);
            ResourceDatabase resDb = ResourceDatabaseBuilder.PrepareResources(buildContext, null);
            string byteCode = ByteCodeEncoder.Encode(compilationResult.ByteCode);
            byte[] cbxFileBytes = new GenerateCbxFileContentWorker().GenerateCbxBinaryData(buildContext, resDb, compilationResult, byteCode);
            Dictionary<string, FileOutput> fileOutputContext = new Dictionary<string, FileOutput>();
            new PopulateFileOutputContextForCbxWorker().GenerateFileOutput(fileOutputContext, buildContext, resDb, cbxFileBytes);
            new EmitFilesToDiskWorker().DoWorkImpl(fileOutputContext, outputDirectory);
            string absoluteCbxFilePath = new GetCbxFileLocation().DoWorkImpl(outputDirectory, buildContext);
            return absoluteCbxFilePath;
        }
    }
}
