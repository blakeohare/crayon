using Build;
using System;
using System.Collections.Generic;
using System.Linq;
using Wax;

namespace Compiler
{
    public class Compiler2Service : WaxService
    {
        public Compiler2Service() : base("compiler2") { }

        public override void HandleRequest(Dictionary<string, object> request, Func<Dictionary<string, object>, bool> cb)
        {
            BuildData result = this.HandleRequestImpl(new Command(request));
            cb(result.GetRawData());
        }

        private BuildData HandleRequestImpl(Command command)
        {
            BuildContext buildContext = GetBuildContext(command, this.Hub);
            ResourceDatabase resourceDatabase = ResourceDatabaseBuilder.PrepareResources(buildContext);

            // TODO: this should no longer be a wax request and can be called directly.
            Dictionary<string, object> compileRequest = CreateCompileRequest(buildContext, command.ErrorsAsExceptions, command.ActiveCrayonSourceRoot);
            BuildData buildData = Compile(compileRequest, resourceDatabase, this.Hub);

            buildData.ExportProperties = BuildExportRequest(buildContext);
            buildData.ExportProperties.ExportPlatform = buildContext.Platform;
            buildData.ExportProperties.ProjectDirectory = buildContext.ProjectDirectory;

            buildData.ExportProperties.OutputDirectory = command.HasOutputDirectoryOverride
                ? command.OutputDirectoryOverride
                : buildContext.OutputFolder;

            if (!buildData.HasErrors)
            {
                buildData.CbxBundle.ResourceDB.ConvertToFlattenedFileData();
            }

            return buildData;
        }

        private static BuildContext GetBuildContext(Command command, WaxHub hub)
        {
            string buildFile = command.BuildFilePath;

            if (buildFile == null)
            {
                throw new InvalidOperationException("No build path was provided.");
            }

            string target = command.BuildTarget;

            buildFile = BuildContext.GetValidatedCanonicalBuildFilePath(buildFile, hub);

            string projectDirectory = Wax.Util.Disk.FileUtil.GetParentDirectory(buildFile);
            string buildFileContent = Wax.Util.Disk.FileUtil.ReadFileText(buildFile);

            BuildContext buildContext = BuildContext.Parse(projectDirectory, buildFileContent, target, command.ResourceErrorsShowRelativeDir);

            // command line arguments override build file values if present.

            if (target != null && buildContext.Platform == null)
                throw new InvalidOperationException("No platform specified in build file.");

            if (buildContext.SourceFolders.Length == 0)
                throw new InvalidOperationException("No source folder specified in build file.");

            if (buildContext.OutputFolder == null)
                throw new InvalidOperationException("No output folder specified in build file.");

            buildContext.OutputFolder = Wax.Util.Disk.FileUtil.JoinAndCanonicalizePath(projectDirectory, buildContext.OutputFolder);

            if (buildContext.LaunchScreenPath != null)
            {
                buildContext.LaunchScreenPath = Wax.Util.Disk.FileUtil.JoinAndCanonicalizePath(projectDirectory, buildContext.LaunchScreenPath);
            }

            foreach (ProjectFilePath sourceFolder in buildContext.SourceFolders)
            {
                if (!Wax.Util.Disk.FileUtil.DirectoryExists(sourceFolder.AbsolutePath))
                {
                    throw new InvalidOperationException("Source folder does not exist.");
                }
            }

            buildContext.ProjectID = buildContext.ProjectID ?? "Untitled";

            return buildContext;
        }

        private static Dictionary<string, object> CreateCompileRequest(BuildContext buildContext, bool errorsAsExceptions, string crayonSourceRoot)
        {
            Dictionary<string, object> request = new Dictionary<string, object>();

            request["projectId"] = buildContext.ProjectID;
            request["delegateMainTo"] = buildContext.DelegateMainTo;
            request["locale"] = buildContext.CompilerLocale.ID;
            request["localDeps"] = buildContext.LocalDeps;
            request["projectDirectory"] = buildContext.ProjectDirectory; // TODO: create a disk service that uses scoped instances where you can register an ID for a folder or remote virtual disk of some sort and then pass that ID instead of the hardcoded folder name here.
            request["codeFiles"] = DictionaryUtil.DictionaryToFlattenedDictionary(buildContext.GetCodeFiles());
            request["lang"] = buildContext.RootProgrammingLanguage + "";
            request["removeSymbols"] = buildContext.RemoveSymbols;

            request["errorsAsExceptions"] = errorsAsExceptions;
            request["crayonSourceRoot"] = crayonSourceRoot;

            List<string> vars = new List<string>();

            foreach (string key in buildContext.BuildVariableLookup.Keys)
            {
                BuildVarCanonicalized buildVar = buildContext.BuildVariableLookup[key];
                vars.Add(key);

                switch (buildVar.Type)
                {
                    case VarType.BOOLEAN: vars.Add("B"); vars.Add(buildVar.BoolValue ? "1" : "0"); break;
                    case VarType.FLOAT: vars.Add("F"); vars.Add(buildVar.FloatValue + ""); break;
                    case VarType.INT: vars.Add("I"); vars.Add(buildVar.IntValue + ""); break;
                    case VarType.STRING: vars.Add("S"); vars.Add(buildVar.StringValue); break;
                    case VarType.NULL: throw new InvalidOperationException("The build variable '" + key + "' does not have a value assigned to it.");
                    default: throw new Exception(); // this should not happen.
                }
            }
            request["buildVars"] = vars.ToArray();

            return request;
        }

        private static Dictionary<string, object> ActualCompilation(Parser.InternalCompilationBundle icb)
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
                output["depTree"] = Parser.AssemblyDependencyUtil.GetDependencyTreeJson(icb.RootScopeDependencyMetadata).Trim();
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
            Dictionary<string, object> request,
            ResourceDatabase resDb,
            WaxHub waxHub)
        {

            Parser.CompileRequest cr = new Parser.CompileRequest(request);

            Parser.InternalCompilationBundle icb = Parser.Compiler.Compile(cr, waxHub);

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
                CbxBundle = new CbxBundle()
                {
                    ByteCode = (string)resultRaw["byteCode"],
                    ResourceDB = resDb,
                    DependencyTreeJson = resultRaw.ContainsKey("depTree") ? (string)resultRaw["depTree"] : null,
                },
            };

            return buildData;
        }

        private static ExportProperties BuildExportRequest(BuildContext buildContext)
        {
            return new ExportProperties()
            {
                ProjectID = buildContext.ProjectID,
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
