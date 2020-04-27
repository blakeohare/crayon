using AssemblyResolver;
using Common;
using CommonUtil.Disk;
using Platform;
using System;
using System.Collections.Generic;

namespace Exporter
{
    public static class StandaloneVmExporter
    {
        public static void Run(
            string platformId,
            IPlatformProvider platformProvider,
            string vmTargetDirectoryRaw)
        {
            string vmTargetDirectory = vmTargetDirectoryRaw;
            if (platformId == null || vmTargetDirectory == null)
            {
                // TODO: this should maybe go earlier during the command line parsing.
                throw new InvalidOperationException("-vm and -vmdir flags must both have correct values.");
            }
            vmTargetDirectory = FileUtil.FinalizeTilde(vmTargetDirectory);

            AbstractPlatform platform = platformProvider.GetPlatform(platformId);
            AssemblyMetadata[] assemblyMetadataList = new AssemblyFinder().AssemblyFlatList;
            Dictionary<string, FileOutput> fileOutputContext = new Dictionary<string, FileOutput>();
            ExportStandaloneVmSourceCodeForPlatform(
                fileOutputContext,
                platform,
                assemblyMetadataList,
                vmTargetDirectory);
            ExportUtil.EmitFilesToDisk(fileOutputContext, vmTargetDirectory);
        }

        private static void ExportStandaloneVmSourceCodeForPlatform(
            Dictionary<string, FileOutput> fileOutput,
            AbstractPlatform platform,
            AssemblyMetadata[] allLibraries,
            string vmTargetDir)
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
