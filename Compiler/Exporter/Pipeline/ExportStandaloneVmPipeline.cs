using Common;
using Exporter.Workers;
using Parser;
using Platform;
using System.Collections.Generic;

namespace Exporter.Pipeline
{
    public static class ExportStandaloneVmPipeline
    {
        public static void Run(ExportCommand command)
        {
            string vmTargetDir = new GetTargetVmExportDirectoryWorker().DoWorkImpl(command);
            AbstractPlatform platform = command.PlatformProvider.GetPlatform(command.VmPlatform);
            AssemblyMetadata[] libraryMetadataList = new AssemblyFinder().LibraryFlatList;
            Dictionary<string, FileOutput> fileOutputContext = new Dictionary<string, FileOutput>();
            new ExportStandaloneVmSourceCodeForPlatformWorker().DoWorkImpl(fileOutputContext, platform, libraryMetadataList, vmTargetDir, command);
            new EmitFilesToDiskWorker().DoWorkImpl(fileOutputContext, vmTargetDir);
        }
    }
}
