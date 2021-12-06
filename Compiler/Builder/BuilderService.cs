using System;
using System.Collections.Generic;
using System.Linq;
using Wax;

namespace Builder
{
    public class BuilderService : WaxService
    {
        public BuilderService() : base("builder") { }

        public override void HandleRequest(Dictionary<string, object> request, Func<Dictionary<string, object>, bool> cb)
        {
            BuildData result = this.HandleRequestImpl(new BuildRequest(request));
            cb(result.GetRawData());
        }

        private BuildData HandleRequestImpl(BuildRequest request)
        {
            BuildContext buildContext = GetBuildContext(
                request.BuildFile,
                request.BuildTarget,
                this.Hub,
                request.BuildArgOverrides,
                request.ExtensionArgOverrides);
            ResourceDatabase resourceDatabase = ResourceDatabaseBuilder.PrepareResources(buildContext);

            // TODO: this should no longer be a wax request and can be called directly.
            BuildData buildData = Compile(buildContext, resourceDatabase, this.Hub);

            buildData.ExportProperties = BuildExportRequest(buildContext);
            buildData.ExportProperties.ExportPlatform = buildContext.Platform;
            buildData.ExportProperties.ProjectDirectory = buildContext.ProjectDirectory;

            buildData.ExportProperties.OutputDirectory = request.OutputDirectoryOverride ?? buildContext.OutputFolder;

            if (!buildData.HasErrors)
            {
                buildData.CbxBundle.ResourceDB.ConvertToFlattenedFileData();
            }

            return buildData;
        }

        private static BuildContext GetBuildContext(
            string buildFilePath,
            string buildTarget,
            WaxHub hub,
            IList<BuildArg> buildArgOverrides,
            IList<ExtensionArg> extensionArgOverrides)
        {
            string buildFile = buildFilePath;

            if (buildFile == null)
            {
                throw new InvalidOperationException("No build path was provided.");
            }

            string target = buildTarget;

            buildFile = BuildContext.GetValidatedCanonicalBuildFilePath(buildFile, hub);

            string projectDirectory = Wax.Util.Disk.FileUtil.GetParentDirectory(buildFile);
            string buildFileContent = Wax.Util.Disk.FileUtil.ReadFileText(buildFile);

            BuildContext buildContext = BuildContext.Parse(projectDirectory, buildFileContent, target, buildArgOverrides, extensionArgOverrides);

            if (buildContext.SourceFolders.Length == 0)
            {
                throw new InvalidOperationException("No source folder specified in build file.");
            }

            foreach (ProjectFilePath sourceFolder in buildContext.SourceFolders)
            {
                if (!Wax.Util.Disk.FileUtil.DirectoryExists(sourceFolder.AbsolutePath))
                {
                    throw new InvalidOperationException("Source folder does not exist: '" + sourceFolder.AbsolutePath + "'.");
                }
            }

            return buildContext;
        }

        private static Dictionary<string, object> ActualCompilation(Builder.InternalCompilationBundle icb)
        {
            Dictionary<string, object> output = new Dictionary<string, object>();
            List<string> errors = new List<string>();
            if (icb.HasErrors)
            {
                foreach (Error err in icb.Errors)
                {
                    errors.AddRange(new string[] { err.FileName, err.Line + "", err.Column + "", err.Message });
                }
            }
            else
            {
                output["byteCode"] = icb.ByteCode;
                output["depTree"] = Builder.AssemblyDependencyUtil.GetDependencyTreeJson(icb.RootScopeDependencyMetadata).Trim();
                output["usesU3"] = icb.AllScopesMetadata.Any(a => a.ID == "U3Direct");
                if (icb.HasErrors)
                {
                    foreach (Error err in icb.Errors)
                    {
                        errors.Add(err.FileName);
                        errors.Add(err.Line + "");
                        errors.Add(err.Column + "");
                        errors.Add(err.Message + "");
                    }
                }
            }
            output["errors"] = errors.ToArray();

            return output;
        }

        private static BuildData Compile(
            BuildContext buildContext,
            ResourceDatabase resDb,
            WaxHub waxHub)
        {
            Builder.CompileRequest cr = new Builder.CompileRequest(buildContext, waxHub.SourceRoot);

            Builder.InternalCompilationBundle icb = Builder.Compiler.Compile(cr, waxHub);

            // TODO: the CBX Bundle should be constructed directly from a Dictionary in this result.
            Dictionary<string, object> resultRaw = ActualCompilation(icb);

            List<Error> errors = new List<Error>();
            string[] errorsRaw = (string[])resultRaw["errors"];
            for (int i = 0; i < errorsRaw.Length; i += 4)
            {
                Error err = new Error()
                {
                    FileName = errorsRaw[i],
                    Line = int.Parse(errorsRaw[i + 1]),
                    Column = int.Parse(errorsRaw[i + 2]),
                    Message = errorsRaw[i + 3],
                };
                errors.Add(err);
            }

            if (errors.Count > 0) return new BuildData() { Errors = errors.ToArray() };

            BuildData buildData = new BuildData()
            {
                UsesU3 = (bool)resultRaw["usesU3"],
                DependencyTreeJson = resultRaw.ContainsKey("depTree") ? (string)resultRaw["depTree"] : null,
                CbxBundle = new CbxBundle()
                {
                    ByteCode = (string)resultRaw["byteCode"],
                    ResourceDB = resDb,
                },
                ProjectID = buildContext.ProjectID,
            };

            return buildData;
        }

        private static ExportProperties BuildExportRequest(BuildContext buildContext)
        {
            return new ExportProperties()
            {
                Version = buildContext.Version,
                Description = buildContext.Description,
                GuidSeed = buildContext.GuidSeed,
                ProjectTitle = buildContext.ProjectTitle,
                JsFilePrefix = SanitizeJsFilePrefix(buildContext.JsFilePrefix),
                JsFullPage = buildContext.JsFullPage,
                IosBundlePrefix = buildContext.IosBundlePrefix,
                JavaPackage = buildContext.JavaPackage,
                IconPaths = buildContext.IconFilePaths,
                LaunchScreenPath = buildContext.LaunchScreenPath,
                Orientations = buildContext.Orientation,
                ExtensionArgs = buildContext.ExtensionArgs,
                SkipRun = buildContext.SkipRun,
            };
        }

        private static string SanitizeJsFilePrefix(string jsFilePrefix)
        {
            return (jsFilePrefix == null || jsFilePrefix == "" || jsFilePrefix == "/")
                ? ""
                : ("/" + jsFilePrefix.Trim('/') + "/");
        }
    }
}
