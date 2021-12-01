using Common;
using CommonUtil.Disk;
using System;
using System.Collections.Generic;
using System.Linq;
using Wax;

namespace Router
{
    internal static class MainPipeline
    {
        public static void Run(Command command, WaxHub waxHub)
        {
            Error[] errors = RunImpl(command, waxHub);

            if (command.IsJsonOutput)
            {
                string jsonErrors = "{\"errors\":[" +
                    string.Join(',', errors.Select(err => err.ToJson())) +
                    "]}";
                ConsoleWriter.Print(ConsoleMessageType.COMPILER_INFORMATION, jsonErrors);
            }
            else if (errors != null && errors.Length > 0)
            {
                ErrorPrinter.ShowErrors(errors, command.ErrorsAsExceptions);
            }
        }

        private enum ExecutionType
        {
            GENERATE_DEFAULT_PROJECT,
            EXPORT_VM_BUNDLE,
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
            if (command.HasTarget) return ExecutionType.EXPORT_VM_BUNDLE;
            if (command.IsCbxExport) return ExecutionType.EXPORT_CBX;
            return ExecutionType.RUN_CBX;
        }

        private static BuildData WrappedCompile(Command command, WaxHub waxHub)
        {
            Dictionary<string, object> result = waxHub.AwaitSendRequest("compiler2", command.GetRawData());
            BuildData buildData = new BuildData(result);
            if (!buildData.HasErrors)
            {
                buildData.ExportProperties.ActiveCrayonSourceRoot = command.ActiveCrayonSourceRoot;

                foreach (string buildWarning in buildData.CbxBundle.ResourceDB.IgnoredFileWarnings ?? new string[0])
                {
                    ConsoleWriter.Print(ConsoleMessageType.BUILD_WARNING, buildWarning);
                }
            }
            return buildData;
        }

        private static Error[] ExportVmBundle(Command command, WaxHub waxHub)
        {
            BuildData buildData = WrappedCompile(command, waxHub);

            if (buildData.HasErrors)
            {
                return buildData.Errors;
            }

            if (command.ShowDependencyTree)
            {
                ConsoleWriter.Print(ConsoleMessageType.LIBRARY_TREE, buildData.CbxBundle.DependencyTreeJson);
            }

            Dictionary<string, object> exportResponseRaw = waxHub.AwaitSendRequest(
                "export-" + buildData.ExportProperties.ExportPlatform.ToLowerInvariant(),
                buildData.GetRawData());

            ExportResponse exportResponse = new ExportResponse(exportResponseRaw);

            if (!exportResponse.HasErrors && command.ApkExportPath != null)
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

            return exportResponse.Errors;
        }

        private static Error[] RunImpl(Command command, WaxHub waxHub)
        {
            if (command.UseOutputPrefixes)
            {
                ConsoleWriter.EnablePrefixes();
            }

            switch (IdentifyUseCase(command))
            {
                case ExecutionType.SHOW_USAGE:
                    ConsoleWriter.Print(ConsoleMessageType.USAGE_NOTES, UsageDisplay.USAGE);
                    return null;

                case ExecutionType.SHOW_VERSION:
                    ConsoleWriter.Print(ConsoleMessageType.USAGE_NOTES, VersionInfo.VersionString);
                    return null;

                case ExecutionType.GENERATE_DEFAULT_PROJECT:
                    new DefaultProjectGenerator().DoWorkImpl(command.DefaultProjectId, command.DefaultProjectLocale, command.DefaultProjectType);
                    return null;

                case ExecutionType.EXPORT_VM_BUNDLE:
                    return ExportVmBundle(command, waxHub);

                case ExecutionType.ERROR_CHECK_ONLY:
                    NotifyStatusChange("COMPILE-START");
                    BuildData errorCheckOnlyResponse = ExportInMemoryCbxData(command, true, waxHub);
                    NotifyStatusChange("COMPILE-END");
                    return errorCheckOnlyResponse.Errors;

                case ExecutionType.EXPORT_CBX:
                    NotifyStatusChange("COMPILE-START");
                    List<Error> errors = new List<Error>();
                    DoExportStandaloneCbxFileAndGetPath(command, false, waxHub, errors);
                    NotifyStatusChange("COMPILE-END");
                    return errors.ToArray();

                case ExecutionType.RUN_CBX:
                    NotifyStatusChange("COMPILE-START");
                    BuildData buildData = ExportInMemoryCbxData(command, false, waxHub);
                    NotifyStatusChange("COMPILE-END");
                    if (buildData.HasErrors)
                    {
                        NotifyStatusChange("RUN-ABORTED");
                        return buildData.Errors;
                    }

                    NotifyStatusChange("RUN-START");
                    waxHub.AwaitSendRequest("runtime", new Dictionary<string, object>() {
                        { "realTimePrint", true },
                        { "cbxBundle", buildData.CbxBundle.GetRawData() },
                        { "args", command.DirectRunArgs },
                        { "showLibStack", command.DirectRunShowLibStack },
                        { "useOutputPrefixes", command.UseOutputPrefixes },
                    });
                    NotifyStatusChange("RUN-END");

                    return null;

                default: throw new Exception(); // this shouldn't happen.
            }
        }

        private static void NotifyStatusChange(string status)
        {
            ConsoleWriter.Print(ConsoleMessageType.STATUS_CHANGE, status);
        }

        private static BuildData ExportInMemoryCbxData(
            Command command,
            bool isDryRunErrorCheck,
            WaxHub waxHub)
        {
            BuildData buildData = WrappedCompile(command, waxHub);

            if (isDryRunErrorCheck || buildData.HasErrors)
            {
                return new BuildData() { Errors = buildData.Errors };
            }

            return buildData;
        }

        // TODO: ew, out params. Figure out a way to clean this up or avoid it.
        // This used to be ExportResponse but that's for a different purpose.
        private static string DoExportStandaloneCbxFileAndGetPath(
            Command command,
            bool isDryRunErrorCheck,
            WaxHub waxHub,
            List<Error> errorsOut)
        {
            BuildData buildData = WrappedCompile(command, waxHub);

            if (isDryRunErrorCheck || buildData.HasErrors)
            {
                errorsOut.AddRange(buildData.Errors);
                return null;
            }

            ResourceDatabase resDb = buildData.CbxBundle.ResourceDB;
            Dictionary<string, FileOutput> outputFiles = new Dictionary<string, FileOutput>();
            string[] fileNames = resDb.FlatFileNames;
            FileOutput[] files = resDb.FlatFiles;
            for (int i = 0; i < files.Length; i++)
            {
                outputFiles[fileNames[i]] = files[i];
            }

            string outputFolder = buildData.ExportProperties.OutputDirectory.Replace("%TARGET_NAME%", "cbx");
            if (!Path.IsAbsolute(outputFolder))
            {
                outputFolder = FileUtil.JoinPath(
                    buildData.ExportProperties.ProjectDirectory,
                    outputFolder);
            }

            byte[] cbxFileBytes = CbxFileEncoder.Encode(buildData.CbxBundle);

            FileUtil.EnsureFolderExists(outputFolder);
            string cbxFilePath = FileUtil.JoinPath(outputFolder, buildData.ExportProperties.ProjectID + ".cbx");
            System.IO.File.WriteAllBytes(cbxFilePath, cbxFileBytes);

            return cbxFilePath;
        }
    }
}
