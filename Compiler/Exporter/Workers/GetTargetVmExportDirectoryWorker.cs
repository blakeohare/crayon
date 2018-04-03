using Common;
using System;

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
}
