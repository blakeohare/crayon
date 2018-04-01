using Build;
using Common;
using Exporter;
using System;

namespace Crayon
{
    public class GetBuildContextCbxWorker : AbstractCrayonWorker
    {
        public override string Name { get { return "Crayon::GetBuildContextCbx"; } }

        public override CrayonWorkerResult DoWorkImpl(CrayonWorkerResult[] args)
        {
            ExportCommand command = (ExportCommand)args[0].Value;
            string buildFilePath = command.BuildFilePath;

            if (buildFilePath == null)
            {
                throw new InvalidOperationException("No build path was provided.");
            }

            string buildFile = BuildContext.GetValidatedCanonicalBuildFilePath(buildFilePath);
            string projectDirectory = FileUtil.GetParentDirectory(buildFile);
            string buildFileContent = FileUtil.ReadFileText(buildFile);
            BuildContext buildContext = BuildContext.Parse(projectDirectory, buildFileContent, null);
            return new CrayonWorkerResult() { Value = buildContext };
        }
    }
}
