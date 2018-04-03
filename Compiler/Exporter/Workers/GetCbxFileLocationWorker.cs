using Build;
using Common;

namespace Exporter.Workers
{
    public class GetCbxFileLocation : AbstractCrayonWorker
    {
        public override CrayonWorkerResult DoWorkImpl(CrayonWorkerResult[] args)
        {
            // absoluteCbxFilePath = GetCbxFileLocation(outputDirectory, buildContext)
            string fullyQualifiedOutputFolder = (string)args[0].Value;
            BuildContext buildContext = (BuildContext)args[1].Value;
            string cbxPath = FileUtil.JoinPath(fullyQualifiedOutputFolder, buildContext.ProjectID + ".cbx");
            cbxPath = FileUtil.GetCanonicalizeUniversalPath(cbxPath);
            return new CrayonWorkerResult() { Value = cbxPath };
        }
    }
}
