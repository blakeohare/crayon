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
        public static ExportResponse Run(
            string platformId,
            IPlatformProvider platformProvider,
            string vmTargetDirectoryRaw,
            bool isRelease)
        {
            if (isRelease)
            {
                try
                {
                    RunImpl(platformId, platformProvider, vmTargetDirectoryRaw);
                }
                catch (InvalidOperationException ioe)
                {
                    return new ExportResponse()
                    {
                        Errors = new Error[] { new Error() { Message = ioe.Message } },
                    };
                }
            }
            else
            {
                RunImpl(platformId, platformProvider, vmTargetDirectoryRaw);
            }
            return new ExportResponse();
        }

        public static void RunImpl(
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
                assemblyMetadataList);
            ExportUtil.EmitFilesToDisk(fileOutputContext, vmTargetDirectory);
        }

        private static void ExportStandaloneVmSourceCodeForPlatform(
            Dictionary<string, FileOutput> fileOutput,
            AbstractPlatform platform,
            AssemblyMetadata[] allLibraries)
        {
            new VmGenerator().GenerateVmSourceCodeForPlatform(
                fileOutput,
                platform,
                allLibraries);
        }
    }
}
