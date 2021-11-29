using Build;
using Common;
using CommonUtil.Disk;
using Exporter;
using System;
using System.Collections.Generic;
using System.Linq;
using Wax;

namespace Crayon.Pipeline
{
    internal static class MainPipeline
    {
        public static void Run(Command command, WaxHub waxHub)
        {
            Result result = RunImpl(command, waxHub);

            if (command.IsJsonOutput)
            {
                string jsonErrors = "{\"errors\":[" +
                    string.Join(',', result.Errors.Select(err => err.ToJson())) +
                    "]}";
                ConsoleWriter.Print(ConsoleMessageType.COMPILER_INFORMATION, jsonErrors);
            }
            else if (result.HasErrors)
            {
                ErrorPrinter.ShowErrors(result.Errors, command.ErrorsAsExceptions);
            }
        }

        private enum ExecutionType
        {
            GENERATE_DEFAULT_PROJECT,
            EXPORT_VM_BUNDLE,
            EXPORT_VM_STANDALONE,
            EXPORT_CBX,
            RUN_CBX,
            SHOW_USAGE,
            SHOW_VERSION,
            ERROR_CHECK_ONLY,
            TRANSPILE_CSHARP_TO_ACRYLIC,
        }

        private static ExecutionType IdentifyUseCase(Command command)
        {
            if (command.ShowVersion) return ExecutionType.SHOW_VERSION;
            if (command.IsCSharpToAcrylicTranspiler) return ExecutionType.TRANSPILE_CSHARP_TO_ACRYLIC;
            if (command.IsGenerateDefaultProject) return ExecutionType.GENERATE_DEFAULT_PROJECT;
            if (command.IsEmpty) return ExecutionType.SHOW_USAGE;
            if (command.IsErrorCheckOnly) return ExecutionType.ERROR_CHECK_ONLY;
            if (command.IsVmExportCommand) return ExecutionType.EXPORT_VM_STANDALONE;
            if (command.HasTarget) return ExecutionType.EXPORT_VM_BUNDLE;
            if (command.IsCbxExport) return ExecutionType.EXPORT_CBX;
            return ExecutionType.RUN_CBX;
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

        private static BuildData WrappedCompile(Command command, WaxHub waxHub)
        {
            BuildContext buildContext = new GetBuildContextWorker().DoWorkImpl(command, waxHub);
            ResourceDatabase resourceDatabase = ResourceDatabaseBuilder.PrepareResources(buildContext);
            Dictionary<string, object> compileRequest = CreateCompileRequest(buildContext, command.ErrorsAsExceptions);
            BuildData buildData = Compile(compileRequest, resourceDatabase, waxHub);
            buildData.ExportProperties = BuildExportRequest(buildContext);
            buildData.ExportProperties.ExportPlatform = buildContext.Platform;
            buildData.ExportProperties.ProjectDirectory = buildContext.ProjectDirectory;

            buildData.ExportProperties.OutputDirectory = command.HasOutputDirectoryOverride
                ? command.OutputDirectoryOverride
                : buildContext.OutputFolder;

            return buildData;
        }

        private static Result ExportVmBundle(Command command, WaxHub waxHub)
        {
            BuildData buildData = WrappedCompile(command, waxHub);

            if (buildData.HasErrors)
            {
                return new Result()
                {
                    Errors = buildData.Errors,
                };
            }

            if (command.ShowDependencyTree)
            {
                ConsoleWriter.Print(ConsoleMessageType.LIBRARY_TREE, buildData.CbxBundle.DependencyTreeJson);
            }

            ExportResponse response = CbxVmBundleExporter.Run(
                buildData.ExportProperties.ExportPlatform.ToLowerInvariant(),
                buildData.ExportProperties.ProjectDirectory,
                buildData.ExportProperties.OutputDirectory,
                buildData,
                new PlatformProvider());
            if (!response.HasErrors && command.ApkExportPath != null)
            {
                if (!buildData.ExportProperties.IsAndroid)
                {
                    throw new InvalidOperationException("Cannot have an APK Export Path for non-Android projects");
                }

                if (!CommonUtil.Android.AndroidUtil.HasAndroidSdk)
                {
                    throw new InvalidOperationException("Cannot export APK because the Android SDK is not present.");
                }

                if (!command.ApkExportPath.ToLowerInvariant().EndsWith(".apk"))
                {
                    throw new InvalidOperationException("Cannot export APK to a file path that doesn't end with .apk");
                }

                CommonUtil.Android.AndroidApkBuildResult apkResult = CommonUtil.Android.AndroidUtil.BuildApk(buildData.ExportProperties.OutputDirectory);

                if (apkResult.HasError)
                {
                    throw new InvalidOperationException("An error occurred while generating the APK: " + apkResult.Error);
                }

                FileUtil.EnsureParentFolderExists(command.ApkExportPath);
                System.IO.File.Copy(apkResult.ApkPath, command.ApkExportPath);
            }

            return new Result() { Errors = response.Errors };
        }

        private static Result RunImpl(Command command, WaxHub waxHub)
        {
            if (command.UseOutputPrefixes)
            {
                ConsoleWriter.EnablePrefixes();
            }

            switch (IdentifyUseCase(command))
            {
                case ExecutionType.SHOW_USAGE:
                    ConsoleWriter.Print(ConsoleMessageType.USAGE_NOTES, UsageDisplay.USAGE);
                    return new Result();

                case ExecutionType.SHOW_VERSION:
                    ConsoleWriter.Print(ConsoleMessageType.USAGE_NOTES, Common.VersionInfo.VersionString);
                    return new Result();

                case ExecutionType.GENERATE_DEFAULT_PROJECT:
                    new DefaultProjectGenerator().DoWorkImpl(command.DefaultProjectId, command.DefaultProjectLocale, command.DefaultProjectType);
                    return new Result();

                case ExecutionType.EXPORT_VM_BUNDLE:
                    return ExportVmBundle(command, waxHub);

                case ExecutionType.EXPORT_VM_STANDALONE:
                    ExportResponse standaloneVmExportResponse = StandaloneVmExporter.Run(
                        command.VmPlatform,
                        new PlatformProvider(),
                        command.VmExportDirectory);
                    return new Result() { Errors = standaloneVmExportResponse.Errors };

                case ExecutionType.ERROR_CHECK_ONLY:
                    NotifyStatusChange("COMPILE-START");
                    ExportResponse errorCheckOnlyResponse = DoExportStandaloneCbxFileAndGetPath(command, true, waxHub);
                    NotifyStatusChange("COMPILE-END");
                    return new Result() { Errors = errorCheckOnlyResponse.Errors };

                case ExecutionType.EXPORT_CBX:
                    NotifyStatusChange("COMPILE-START");
                    ExportResponse cbxOnlyResponse = DoExportStandaloneCbxFileAndGetPath(command, false, waxHub);
                    NotifyStatusChange("COMPILE-END");
                    return new Result() { Errors = cbxOnlyResponse.Errors };

                case ExecutionType.RUN_CBX:
                    NotifyStatusChange("COMPILE-START");
                    ExportResponse exportResult = DoExportStandaloneCbxFileAndGetPath(command, false, waxHub);
                    NotifyStatusChange("COMPILE-END");
                    if (exportResult.HasErrors)
                    {
                        NotifyStatusChange("RUN-ABORTED");
                        return new Result() { Errors = exportResult.Errors };
                    }

                    NotifyStatusChange("RUN-START");
                    waxHub.AwaitSendRequest("cbxrunner", new Dictionary<string, object>() {
                        { "realTimePrint", true },
                        { "cbxPath", exportResult.CbxOutputPath },
                        { "args", command.DirectRunArgs },
                        { "showLibStack", command.DirectRunShowLibStack },
                        { "useOutputPrefixes", command.UseOutputPrefixes },
                    });
                    NotifyStatusChange("RUN-END");

                    return new Result();

                default: throw new Exception(); // this shouldn't happen.
            }
        }

        private static string SanitizeJsFilePrefix(string jsFilePrefix)
        {
            return (jsFilePrefix == null || jsFilePrefix == "" || jsFilePrefix == "/")
                ? ""
                : ("/" + jsFilePrefix.Trim('/') + "/");
        }

        private static void NotifyStatusChange(string status)
        {
            ConsoleWriter.Print(ConsoleMessageType.STATUS_CHANGE, status);
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

        private static ExportResponse DoExportStandaloneCbxFileAndGetPath(
            Command command,
            bool isDryRunErrorCheck,
            WaxHub waxHub)
        {
            Dictionary<string, FileOutput> outputFiles = new Dictionary<string, FileOutput>();

            BuildData buildData = WrappedCompile(command, waxHub);

            buildData.CbxBundle.ResourceDB.PopulateFileOutputContextForCbx(outputFiles);

            if (isDryRunErrorCheck || buildData.HasErrors)
            {
                return new ExportResponse() { Errors = buildData.Errors };
            }

            string outputFolder = FileUtil.JoinPath(
                buildData.ExportProperties.ProjectDirectory,
                buildData.ExportProperties.OutputDirectory.Replace("%TARGET_NAME%", "cbx"));

            string cbxLocation = StandaloneCbxExporter.Run(
                buildData.ExportProperties.ProjectID,
                outputFiles,
                outputFolder,
                buildData.CbxBundle.ByteCode,
                buildData.CbxBundle.ResourceDB.ResourceManifestFile.TextContent,
                buildData.CbxBundle.ResourceDB.ImageResourceManifestFile == null ? null : buildData.CbxBundle.ResourceDB.ImageResourceManifestFile.TextContent);

            return new ExportResponse()
            {
                CbxOutputPath = cbxLocation,
            };
        }
    }
}
