using Build;
using Common;
using CommonUtil.Disk;
using System;

namespace Crayon
{
    class GetBuildContextWorker
    {
        public BuildContext DoWorkImpl(Command command, CommonUtil.Wax.WaxHub hub)
        {
            string buildFile = command.BuildFilePath;
            string target = command.BuildTarget;

            if (buildFile == null || target == null)
            {
                throw new InvalidOperationException("Build file and target must be specified together.");
            }

            buildFile = BuildContext.GetValidatedCanonicalBuildFilePath(buildFile, hub);

            string projectDirectory = FileUtil.GetParentDirectory(buildFile);

            BuildContext buildContext = null;

            buildContext = BuildContext.Parse(projectDirectory, FileUtil.ReadFileText(buildFile), target, command.ResourceErrorsShowRelativeDir);

            buildContext = buildContext ?? new BuildContext();

            // command line arguments override build file values if present.

            if (buildContext.Platform == null)
                throw new InvalidOperationException("No platform specified in build file.");

            if (buildContext.SourceFolders.Length == 0)
                throw new InvalidOperationException("No source folder specified in build file.");

            if (buildContext.OutputFolder == null)
                throw new InvalidOperationException("No output folder specified in build file.");

            buildContext.OutputFolder = FileUtil.JoinAndCanonicalizePath(projectDirectory, buildContext.OutputFolder);

            if (buildContext.LaunchScreenPath != null)
            {
                buildContext.LaunchScreenPath = FileUtil.JoinAndCanonicalizePath(projectDirectory, buildContext.LaunchScreenPath);
            }

            foreach (FilePath sourceFolder in buildContext.SourceFolders)
            {
                if (!FileUtil.DirectoryExists(sourceFolder.AbsolutePath))
                {
                    throw new InvalidOperationException("Source folder does not exist.");
                }
            }

            buildContext.ProjectID = buildContext.ProjectID ?? "Untitled";

            buildContext.TranspileFrontendLanguage(hub);

            return buildContext;
        }
    }
}
