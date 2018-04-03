using Build;
using Common;

namespace Exporter.Workers
{
    public class GetOutputDirectoryWorker : AbstractCrayonWorker
    {
        public override CrayonWorkerResult DoWorkImpl(CrayonWorkerResult[] args)
        {
            // outputDirectory = GetOutputDirectory(buildContext)
            BuildContext buildContext = (BuildContext)args[0].Value;
            string outputFolder = buildContext.OutputFolder.Replace("%TARGET_NAME%", "cbx");
            string fullyQualifiedOutputFolder = FileUtil.JoinPath(buildContext.ProjectDirectory, outputFolder);
            return new CrayonWorkerResult() { Value = fullyQualifiedOutputFolder };
        }
    }
}
