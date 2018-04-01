using Common;
using Parser;
using System;
using System.Collections.Generic;

namespace Exporter
{
    public class ExportStandaloneVmImplWorker : AbstractCrayonWorker
    {
        public override CrayonWorkerResult DoWorkImpl(CrayonWorkerResult[] args)
        {
            ExportCommand command = (ExportCommand)args[0].Value;
            this.ExportStandaloneVm(command);
            return new CrayonWorkerResult();
        }

        private void ExportStandaloneVm(ExportCommand command)
        {
            string vmPlatform = command.VmPlatform;
            string vmTargetDirectory = command.VmExportDirectory;
            if (vmPlatform == null || vmTargetDirectory == null)
            {
                throw new InvalidOperationException("-vm and -vmdir flags must both have correct values.");
            }
            Platform.AbstractPlatform standaloneVmPlatform = command.PlatformProvider.GetPlatform(vmPlatform);

            vmTargetDirectory = FileUtil.FinalizeTilde(vmTargetDirectory);

            VmGenerator vmGenerator = new VmGenerator();

            LibraryMetadata[] allLibraries = new LibraryFinder().LibraryFlatList;
            Dictionary<string, FileOutput> fileOutputDescriptor = new Dictionary<string, FileOutput>();

            vmGenerator.GenerateVmSourceCodeForPlatform(
                fileOutputDescriptor,
                standaloneVmPlatform,
                null,
                null,
                allLibraries,
                vmTargetDirectory,
                command.InlineImportCodeLoader,
                VmGenerationMode.EXPORT_VM_AND_LIBRARIES);
            FileOutputExporter exporter = new FileOutputExporter(vmTargetDirectory);
            exporter.ExportFiles(fileOutputDescriptor);
        }
    }
}
