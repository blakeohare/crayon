using Build;
using Common;
using Exporter.Workers;
using Parser;
using System.Collections.Generic;

namespace Exporter.Pipeline
{
    public static class ExportStandaloneCbxPipeline
    {
        public static string Run(CompilationBundle compilation, ExportCommand command, BuildContext buildContext)
        {
            string outputDirectory = new GetOutputDirectoryWorker().DoWorkImpl(buildContext);
            ExportBundle exportBundle = ExportBundle.Compile(compilation, buildContext);
            ResourceDatabase resDb = ResourceDatabaseBuilder.PrepareResources(buildContext, null);
            string byteCode = exportBundle.ByteCode;
            byte[] cbxFileBytes = new GenerateCbxFileContentWorker().GenerateCbxBinaryData(buildContext, resDb, exportBundle, byteCode);
            Dictionary<string, FileOutput> fileOutputContext = new Dictionary<string, FileOutput>();
            new PopulateFileOutputContextForCbxWorker().GenerateFileOutput(fileOutputContext, buildContext, resDb, cbxFileBytes);
            new EmitFilesToDiskWorker().DoWorkImpl(fileOutputContext, outputDirectory);
            string absoluteCbxFilePath = new GetCbxFileLocation().DoWorkImpl(outputDirectory, buildContext);
            return absoluteCbxFilePath;
        }
    }
}
