using Common;

namespace Exporter.Workers
{
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
}
