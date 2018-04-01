using Common;
using Parser;
using System;
using System.Collections.Generic;

// TODO: split this file into separate files.

namespace Exporter.Workers
{
    // vmTargetDir = Exporter::GetTargetVmExportDirectory(command)
    public class GetTargetVmExportDirectoryWorker : AbstractCrayonWorker
    {
        public override CrayonWorkerResult DoWorkImpl(CrayonWorkerResult[] args)
        {
            ExportCommand command = (ExportCommand)args[0].Value;
            string vmTargetDirectory = command.VmExportDirectory;
            if (command.VmPlatform == null || vmTargetDirectory == null)
            {
                // TODO: this should maybe go earlier during the command line parsing.
                throw new InvalidOperationException("-vm and -vmdir flags must both have correct values.");
            }
            vmTargetDirectory = FileUtil.FinalizeTilde(vmTargetDirectory);
            return new CrayonWorkerResult() { Value = vmTargetDirectory };
        }
    }

    // platform = Exporter::GetPlatformFromVmCommand(command)
    public class GetPlatformFromVmCommandWorker : AbstractCrayonWorker
    {
        public override CrayonWorkerResult DoWorkImpl(CrayonWorkerResult[] args)
        {
            ExportCommand command = (ExportCommand)args[0].Value;
            Platform.AbstractPlatform standaloneVmPlatform = command.PlatformProvider.GetPlatform(command.VmPlatform);
            return new CrayonWorkerResult() { Value = standaloneVmPlatform };
        }
    }

    // libraryMetadataList = Exporter::GetAllLibrariesForPlatformUniverse()
    public class GetAllLibrariesForPlatformUniverseWorker : AbstractCrayonWorker
    {
        public override CrayonWorkerResult DoWorkImpl(CrayonWorkerResult[] args)
        {
            LibraryMetadata[] allLibraries = new LibraryFinder().LibraryFlatList;
            return new CrayonWorkerResult() { Value = allLibraries };
        }
    }

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
