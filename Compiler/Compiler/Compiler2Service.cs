﻿using Build;
using System;
using System.Collections.Generic;
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
            Dictionary<string, object> compileRequest = CreateCompileRequest(buildContext, command.ErrorsAsExceptions);
            BuildData buildData = Compile(compileRequest, resourceDatabase, this.Hub);

            buildData.ExportProperties = BuildExportRequest(buildContext);
            buildData.ExportProperties.ExportPlatform = buildContext.Platform;
            buildData.ExportProperties.ProjectDirectory = buildContext.ProjectDirectory;

            buildData.ExportProperties.OutputDirectory = command.HasOutputDirectoryOverride
                ? command.OutputDirectoryOverride
                : buildContext.OutputFolder;

            buildData.CbxBundle.ResourceDB.ConvertToFlattenedFileData();

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

            string projectDirectory = CommonUtil.Disk.FileUtil.GetParentDirectory(buildFile);
            string buildFileContent = CommonUtil.Disk.FileUtil.ReadFileText(buildFile);

            BuildContext buildContext = BuildContext.Parse(projectDirectory, buildFileContent, target, command.ResourceErrorsShowRelativeDir);

            // command line arguments override build file values if present.

            if (target != null && buildContext.Platform == null)
                throw new InvalidOperationException("No platform specified in build file.");

            if (buildContext.SourceFolders.Length == 0)
                throw new InvalidOperationException("No source folder specified in build file.");

            if (buildContext.OutputFolder == null)
                throw new InvalidOperationException("No output folder specified in build file.");

            buildContext.OutputFolder = CommonUtil.Disk.FileUtil.JoinAndCanonicalizePath(projectDirectory, buildContext.OutputFolder);

            if (buildContext.LaunchScreenPath != null)
            {
                buildContext.LaunchScreenPath = CommonUtil.Disk.FileUtil.JoinAndCanonicalizePath(projectDirectory, buildContext.LaunchScreenPath);
            }

            foreach (Common.FilePath sourceFolder in buildContext.SourceFolders)
            {
                if (!CommonUtil.Disk.FileUtil.DirectoryExists(sourceFolder.AbsolutePath))
                {
                    throw new InvalidOperationException("Source folder does not exist.");
                }
            }

            buildContext.ProjectID = buildContext.ProjectID ?? "Untitled";

            return buildContext;
        }

        private static Dictionary<string, object> CreateCompileRequest(BuildContext buildContext, bool errorsAsExceptions)
        {
            Dictionary<string, object> request = new Dictionary<string, object>();
            request["projectId"] = buildContext.ProjectID;
            request["errorsAsExceptions"] = errorsAsExceptions;
            request["delegateMainTo"] = buildContext.DelegateMainTo;
            request["locale"] = buildContext.CompilerLocale.ID;
            request["localDeps"] = buildContext.LocalDeps;
            request["projectDirectory"] = buildContext.ProjectDirectory; // TODO: create a disk service that uses scoped instances where you can register an ID for a folder or remote virtual disk of some sort and then pass that ID instead of the hardcoded folder name here.
            request["codeFiles"] = CommonUtil.Collections.DictionaryUtil.DictionaryToFlattenedDictionary(buildContext.GetCodeFiles());
            request["lang"] = buildContext.RootProgrammingLanguage + "";
            request["removeSymbols"] = buildContext.RemoveSymbols;
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

        private static BuildData Compile(
            Dictionary<string, object> request,
            ResourceDatabase resDb,
            WaxHub waxHub)
        {
            // TODO: the CBX Bundle should be constructed directly from a Dictionary in this result.
            Dictionary<string, object> resultRaw = waxHub.AwaitSendRequest("compiler", request);
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
