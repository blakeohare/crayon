using Build;
using Common;

namespace Exporter.Workers
{
    public class GetResourceDatabaseFromBuildWorker
    {
        public ResourceDatabase DoWorkImpl(BuildContext buildContext)
        {
            return ResourceDatabaseBuilder.PrepareResources(buildContext, null);
        }
    }
}
