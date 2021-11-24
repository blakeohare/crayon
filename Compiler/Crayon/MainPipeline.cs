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
        public static void Run(Command command, bool isRelease, Wax.WaxHub waxHub)
        {
            Result result = RunImpl(command, isRelease, waxHub);

            if (command.IsJsonOutput)
            {
                string jsonErrors = "{\"errors\":[" +
                    string.Join(',', result.Errors.Select(err => err.ToJson())) +
                    "]}";
                ConsoleWriter.Print(ConsoleMessageType.COMPILER_INFORMATION, jsonErrors);
            }
            else if (result.HasErrors)
            {
                ErrorPrinter.ShowErrors(result.Errors, !isRelease);
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

        private static Dictionary<string, object> CreateCompileRequest(BuildContext buildContext, bool isRelease)
        {
            Dictionary<string, object> request = new Dictionary<string, object>();
            request["projectId"] = buildContext.ProjectID;
            request["isRelease"] = isRelease;
            request["delegateMainTo"] = buildContext.DelegateMainTo;
            request["locale"] = buildContext.CompilerLocale.ID;
            request["localDeps"] = buildContext.LocalDeps;
            request["projectDirectory"] = buildContext.ProjectDirectory; // TODO: create a disk service that uses scoped isntances where you can like register an ID for a folder or remote virtual disk of some sort and then pass that ID instead of the hardecoded folder name here.
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

        private static ExternalCompilationBundle Compile(
            Dictionary<string, object> request,
            WaxHub waxHub)
        {
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
            return errors.Count == 0
                ? new ExternalCompilationBundle()
                {
                    ByteCode = (string)resultRaw["byteCode"],
                    DependencyTreeJson = (string)resultRaw["depTree"],
                    Errors = errors.ToArray(),
                    UsesU3 = (bool)resultRaw["usesU3"],
                }
                : new ExternalCompilationBundle()
                {
                    Errors = errors.ToArray()
                };
        }

        public static Result RunImpl(Command command, bool isRelease, Wax.WaxHub waxHub)
        {
            if (command.UseOutputPrefixes)
            {
                ConsoleWriter.EnablePrefixes();
            }

            BuildContext buildContext;

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
                    ExternalCompilationBundle compilation;

                    if (isRelease)
                    {
                        try
                        {
                            buildContext = new GetBuildContextWorker().DoWorkImpl(command, waxHub);
                            compilation = Compile(CreateCompileRequest(buildContext, isRelease), waxHub);
                        }
                        catch (InvalidOperationException ioe)
                        {
                            return new Result()
                            {
                                Errors = new Error[] { new Error() { Message = ioe.Message } },
                            };
                        }
                    }
                    else
                    {
                        buildContext = new GetBuildContextWorker().DoWorkImpl(command, waxHub);
                        compilation = Compile(CreateCompileRequest(buildContext, isRelease), waxHub);
                    }

                    if (compilation.HasErrors)
                    {
                        return new Result()
                        {
                            Errors = compilation.Errors,
                        };
                    }

                    if (command.ShowDependencyTree)
                    {
                        ConsoleWriter.Print(ConsoleMessageType.LIBRARY_TREE, compilation.DependencyTreeJson);
                    }

                    string outputDirectory = command.HasOutputDirectoryOverride
                        ? command.OutputDirectoryOverride
                        : buildContext.OutputFolder;

                    ResourceDatabase resourceDatabase;
                    if (isRelease)
                    {
                        try
                        {
                            resourceDatabase = ResourceDatabaseBuilder.PrepareResources(buildContext);
                        }
                        catch (InvalidOperationException ioe)
                        {
                            return new Result()
                            {
                                Errors = new Error[] { new Error() { Message = ioe.Message } },
                            };
                        }
                    }
                    else
                    {
                        resourceDatabase = ResourceDatabaseBuilder.PrepareResources(buildContext);
                    }

                    CbxBundleView cbxBundle = new CbxBundleView(compilation.ByteCode, resourceDatabase);

                    ExportRequest exportBundle = BuildExportRequest(compilation.ByteCode, buildContext);
                    ExportResponse response = CbxVmBundleExporter.Run(
                            buildContext.Platform.ToLowerInvariant(),
                            buildContext.ProjectDirectory,
                            outputDirectory,
                            cbxBundle,
                            compilation.UsesU3,
                            exportBundle,
                            new PlatformProvider(),
                            isRelease);
                    if (!response.HasErrors && command.ApkExportPath != null)
                    {
                        if (buildContext.Platform.ToLowerInvariant() != "javascript-app-android")
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

                        CommonUtil.Android.AndroidApkBuildResult apkResult = CommonUtil.Android.AndroidUtil.BuildApk(outputDirectory);

                        if (apkResult.HasError)
                        {
                            throw new InvalidOperationException("An error occurred while generating the APK: " + apkResult.Error);
                        }

                        FileUtil.EnsureParentFolderExists(command.ApkExportPath);
                        System.IO.File.Copy(apkResult.ApkPath, command.ApkExportPath);
                    }

                    return new Result() { Errors = response.Errors };

                case ExecutionType.EXPORT_VM_STANDALONE:
                    ExportResponse standaloneVmExportResponse = StandaloneVmExporter.Run(
                        command.VmPlatform,
                        new PlatformProvider(),
                        command.VmExportDirectory,
                        false,
                        isRelease);
                    return new Result() { Errors = standaloneVmExportResponse.Errors };

                case ExecutionType.ERROR_CHECK_ONLY:
                    NotifyStatusChange("COMPILE-START");
                    ExportResponse errorCheckOnlyResponse = DoExportStandaloneCbxFileAndGetPath(command, true, isRelease, waxHub);
                    NotifyStatusChange("COMPILE-END");
                    return new Result() { Errors = errorCheckOnlyResponse.Errors };

                case ExecutionType.EXPORT_CBX:
                    NotifyStatusChange("COMPILE-START");
                    ExportResponse cbxOnlyResponse = DoExportStandaloneCbxFileAndGetPath(command, false, isRelease, waxHub);
                    NotifyStatusChange("COMPILE-END");
                    return new Result() { Errors = cbxOnlyResponse.Errors };

                case ExecutionType.RUN_CBX:
                    NotifyStatusChange("COMPILE-START");
                    ExportResponse exportResult = DoExportStandaloneCbxFileAndGetPath(command, false, isRelease, waxHub);
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

        private static ExportRequest BuildExportRequest(
            string byteCode,
            BuildContext buildContext)
        {
            return new ExportRequest()
            {
                ByteCode = byteCode,
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
            bool isRelease,
            WaxHub waxHub)
        {
            BuildContext buildContext;
            if (isRelease)
            {
                try
                {
                    buildContext = new GetBuildContextCbxWorker().DoWorkImpl(command, waxHub);
                }
                catch (InvalidOperationException ioe)
                {
                    return new ExportResponse() { Errors = new Error[] { new Error() { Message = ioe.Message } } };
                }
            }
            else
            {
                buildContext = new GetBuildContextCbxWorker().DoWorkImpl(command, waxHub);
            }

            ExternalCompilationBundle compilation = Compile(CreateCompileRequest(buildContext, isRelease), waxHub);
            if (isDryRunErrorCheck || compilation.HasErrors)
            {
                return new ExportResponse() { Errors = compilation.Errors };
            }

            Dictionary<string, FileOutput> outputFiles = new Dictionary<string, FileOutput>();

            ResourceDatabase resourceDatabase;
            if (isRelease)
            {
                try
                {
                    resourceDatabase = ResourceDatabaseBuilder.PrepareResources(buildContext);
                }
                catch (InvalidOperationException ioe)
                {
                    return new ExportResponse()
                    {
                        Errors = new Error[] { new Error() { Message = ioe.Message } },
                    };
                }
            }
            else
            {
                resourceDatabase = ResourceDatabaseBuilder.PrepareResources(buildContext);
            }

            resourceDatabase.PopulateFileOutputContextForCbx(outputFiles);

            string outputFolder = buildContext.OutputFolder.Replace("%TARGET_NAME%", "cbx");
            outputFolder = FileUtil.JoinPath(buildContext.ProjectDirectory, outputFolder);

            string cbxLocation = StandaloneCbxExporter.Run(
                buildContext.ProjectID,
                outputFiles,
                outputFolder,
                compilation.ByteCode,
                resourceDatabase.ResourceManifestFile.TextContent,
                resourceDatabase.ImageResourceManifestFile == null ? null : resourceDatabase.ImageResourceManifestFile.TextContent);
            return new ExportResponse()
            {
                CbxOutputPath = cbxLocation,
            };
        }
    }
}
