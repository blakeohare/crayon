using AssemblyResolver;
using Common;
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
    public class ExportStandaloneVmSourceCodeForPlatformWorker
    {
        public void DoWorkImpl(
            Dictionary<string, FileOutput> fileOutput,
            Platform.AbstractPlatform platform,
            AssemblyMetadata[] allLibraries,
            string vmTargetDir,
            ExportCommand command)
        {
            new VmGenerator().GenerateVmSourceCodeForPlatform(
                fileOutput,
                "",
                platform,
                null,
                null,
                allLibraries,
                vmTargetDir,
                VmGenerationMode.EXPORT_VM_AND_LIBRARIES);
        }
    }
}
