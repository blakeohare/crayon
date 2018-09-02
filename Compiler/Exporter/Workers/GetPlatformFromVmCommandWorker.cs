namespace Exporter.Workers
{
    public class GetPlatformFromVmCommandWorker
    {
        public Platform.AbstractPlatform DoWorkImpl(ExportCommand command)
        {
            return command.PlatformProvider.GetPlatform(command.VmPlatform);
        }
    }
}
