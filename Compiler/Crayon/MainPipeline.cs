﻿using Common;
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

        private static BuildData WrappedCompile(Command command, WaxHub waxHub)
        {
            Dictionary<string, object> result = waxHub.AwaitSendRequest("compiler2", command.GetRawData());
            BuildData buildData = new BuildData(result);
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
                    BuildData buildData = ExportInMemoryCbxData(command, false, waxHub);
                    NotifyStatusChange("COMPILE-END");
                    if (buildData.HasErrors)
                    {
                        NotifyStatusChange("RUN-ABORTED");
                        return new Result() { Errors = buildData.Errors };
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

                    return new Result();

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

        private static ExportResponse DoExportStandaloneCbxFileAndGetPath(
            Command command,
            bool isDryRunErrorCheck,
            WaxHub waxHub)
        {
            BuildData buildData = WrappedCompile(command, waxHub);

            if (isDryRunErrorCheck || buildData.HasErrors)
            {
                return new ExportResponse() { Errors = buildData.Errors };
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
