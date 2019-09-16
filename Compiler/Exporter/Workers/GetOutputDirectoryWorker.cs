using Build;
using CommonUtil.Disk;

namespace Exporter.Workers
{
    public class GetOutputDirectoryWorker
    {
        public string DoWorkImpl(BuildContext buildContext)
        {
            string outputFolder = buildContext.OutputFolder.Replace("%TARGET_NAME%", "cbx");
            string fullyQualifiedOutputFolder = FileUtil.JoinPath(buildContext.ProjectDirectory, outputFolder);
            return fullyQualifiedOutputFolder;
        }
    }
}
