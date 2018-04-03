using Build;
using Common;

namespace Exporter.Workers
{
    public class GetResourceDatabaseFromBuildWorker : AbstractCrayonWorker
    {
        public override CrayonWorkerResult DoWorkImpl(CrayonWorkerResult[] args)
        {
            // resDb = GetResourceDatabaseFromBuild(buildContext)
            BuildContext buildContext = (BuildContext)args[0].Value;
            ResourceDatabase resDb = ResourceDatabaseBuilder.PrepareResources(buildContext, null);
            return new CrayonWorkerResult() { Value = resDb };
        }
    }
}
