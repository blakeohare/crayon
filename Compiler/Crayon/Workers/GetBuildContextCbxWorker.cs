using Build;
using CommonUtil.Disk;
using Exporter;
using System;

namespace Crayon
{
    public class GetBuildContextCbxWorker
    {
        internal BuildContext DoWorkImpl(Command command, Wax.WaxHub hub)
        {
            string buildFilePath = command.BuildFilePath;

            if (buildFilePath == null)
            {
                throw new InvalidOperationException("No build path was provided.");
            }

            string buildFile = BuildContext.GetValidatedCanonicalBuildFilePath(buildFilePath, hub);
            string projectDirectory = FileUtil.GetParentDirectory(buildFile);
            string buildFileContent = FileUtil.ReadFileText(buildFile);
            BuildContext buildContext = BuildContext.Parse(projectDirectory, buildFileContent, null, command.ResourceErrorsShowRelativeDir);
            buildContext.TranspileFrontendLanguage(hub);
            return buildContext;
        }
    }
}
