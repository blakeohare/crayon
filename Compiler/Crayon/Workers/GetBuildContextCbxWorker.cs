using Build;
using Common;
using Exporter;
using System;

namespace Crayon
{
    public class GetBuildContextCbxWorker
    {
        public BuildContext DoWorkImpl(ExportCommand command)
        {
            string buildFilePath = command.BuildFilePath;

            if (buildFilePath == null)
            {
                throw new InvalidOperationException("No build path was provided.");
            }

            string buildFile = BuildContext.GetValidatedCanonicalBuildFilePath(buildFilePath);
            string projectDirectory = FileUtil.GetParentDirectory(buildFile);
            string buildFileContent = FileUtil.ReadFileText(buildFile);
            BuildContext buildContext = BuildContext.Parse(projectDirectory, buildFileContent, null, command.ResourceErrorsShowRelativeDir);
            return buildContext;
        }
    }
}
