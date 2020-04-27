using Build;
using CommonUtil.Disk;

namespace Exporter.Workers
{
    public class GetCbxFileLocation
    {
        public string DoWorkImpl(
            string fullyQualifiedOutputFolder,
            string projectId)
        {
            string cbxPath = FileUtil.JoinPath(fullyQualifiedOutputFolder, projectId + ".cbx");
            cbxPath = FileUtil.GetCanonicalizeUniversalPath(cbxPath);
            return cbxPath;
        }
    }
}
