using AssemblyResolver;
using Common;
using CommonUtil.Disk;
using Platform;
using System;
using System.Collections.Generic;
using System.Linq;

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
            bool usesU3 = assemblyMetadataList.Any(a => a.ID == "U3Direct");
            Dictionary<string, FileOutput> fileOutputContext = new Dictionary<string, FileOutput>();
            ExportStandaloneVmSourceCodeForPlatform(
                fileOutputContext,
                platform,
                usesU3,
                vmTargetDirectory);
            ExportUtil.EmitFilesToDisk(fileOutputContext, vmTargetDirectory);
        }

        private static void ExportStandaloneVmSourceCodeForPlatform(
            Dictionary<string, FileOutput> fileOutput,
            AbstractPlatform platform,
            bool usesU3,
            string vmTargetDir)
        {
            new VmGenerator().GenerateVmSourceCodeForPlatform(
                fileOutput,
                "",
                platform,
                null,
                null,
                usesU3,
                vmTargetDir,
                VmGenerationMode.EXPORT_VM_AND_LIBRARIES);
        }
    }
}
