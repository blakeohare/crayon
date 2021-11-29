using CommonUtil.Disk;
using Platform;
using System;
using System.Collections.Generic;
using Wax;

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
            RunImpl(platformId, platformProvider, vmTargetDirectoryRaw);
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
            Dictionary<string, FileOutput> fileOutputContext = new Dictionary<string, FileOutput>();
            ExportStandaloneVmSourceCodeForPlatform(
                fileOutputContext,
                platform,
                vmTargetDirectory);
            ExportUtil.EmitFilesToDisk(fileOutputContext, vmTargetDirectory);
        }

        private static void ExportStandaloneVmSourceCodeForPlatform(
            Dictionary<string, FileOutput> fileOutput,
            AbstractPlatform platform,
            string vmTargetDir)
        {
            new VmGenerator().GenerateVmSourceCodeForPlatform(
                fileOutput,
                null,
                platform,
                vmTargetDir,
                VmGenerationMode.EXPORT_VM_AND_LIBRARIES);
        }
    }
}
