using Common;
using Parser;
using System.Collections.Generic;

namespace Exporter.Workers
{
    /*
        Exporter::ExportStandaloneVmSourceCodeForPlatform(
            fileOutput,
            platform,
            libraryMetadataList,
            vmTargetDir,
            command)
    */
    public class ExportStandaloneVmSourceCodeForPlatformWorker : AbstractCrayonWorker
    {
        public override CrayonWorkerResult DoWorkImpl(CrayonWorkerResult[] args)
        {
            Dictionary<string, FileOutput> fileOutput = (Dictionary<string, FileOutput>)args[0].Value;
            Platform.AbstractPlatform platform = (Platform.AbstractPlatform)args[1].Value;
            LibraryMetadata[] allLibraries = (LibraryMetadata[])args[2].Value;
            string vmTargetDir = (string)args[3].Value;
            ExportCommand command = (ExportCommand)args[4].Value;

            new VmGenerator().GenerateVmSourceCodeForPlatform(
                fileOutput,
                platform,
                null,
                null,
                allLibraries,
                vmTargetDir,
                command.InlineImportCodeLoader,
                VmGenerationMode.EXPORT_VM_AND_LIBRARIES);

            return new CrayonWorkerResult();
        }
    }
}
