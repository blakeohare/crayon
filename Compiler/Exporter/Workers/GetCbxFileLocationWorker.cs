using Build;
using CommonUtil.Disk;

namespace Exporter.Workers
{
    public class GetCbxFileLocation
    {
        public string DoWorkImpl(
            string fullyQualifiedOutputFolder,
            BuildContext buildContext)
        {
            string cbxPath = FileUtil.JoinPath(fullyQualifiedOutputFolder, buildContext.ProjectID + ".cbx");
            cbxPath = FileUtil.GetCanonicalizeUniversalPath(cbxPath);
            return cbxPath;
        }
    }
}
