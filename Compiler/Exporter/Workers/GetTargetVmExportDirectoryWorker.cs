using Common;
using System;

namespace Exporter.Workers
{
    public class GetTargetVmExportDirectoryWorker
    {
        public string DoWorkImpl(ExportCommand command)
        {
            string vmTargetDirectory = command.VmExportDirectory;
            if (command.VmPlatform == null || vmTargetDirectory == null)
            {
                // TODO: this should maybe go earlier during the command line parsing.
                throw new InvalidOperationException("-vm and -vmdir flags must both have correct values.");
            }
            vmTargetDirectory = FileUtil.FinalizeTilde(vmTargetDirectory);
            return vmTargetDirectory;
        }
    }
}
