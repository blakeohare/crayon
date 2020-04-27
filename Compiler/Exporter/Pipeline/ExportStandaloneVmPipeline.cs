using AssemblyResolver;
using Common;
using CommonUtil.Disk;
using Exporter.Workers;
using Platform;
using System;
using System.Collections.Generic;

namespace Exporter.Pipeline
{
    public static class ExportStandaloneVmPipeline
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
            new ExportStandaloneVmSourceCodeForPlatformWorker().DoWorkImpl(
                fileOutputContext,
                platform,
                assemblyMetadataList,
                vmTargetDirectory);
            new EmitFilesToDiskWorker().DoWorkImpl(fileOutputContext, vmTargetDirectory);
        }
    }
}
