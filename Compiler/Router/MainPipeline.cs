﻿using System.Collections.Generic;
using System.Linq;
using Wax;
using Wax.Util.Disk;

namespace Router
{
    internal static class MainPipeline
    {
        public static Error[] Run(ToolchainCommand command, WaxHub waxHub)
        {
            NotifyStatusChange("TOOLCHAIN-START");
            Error[] errors = RunImpl(command, waxHub);
            NotifyStatusChange("TOOLCHAIN-END");
            return errors;
        }

        public static Error[] RunImpl(ToolchainCommand command, WaxHub waxHub)
        {
            // BUILD PHASE
            NotifyStatusChange("BUILD-START");
            BuildData buildResult = null;
            if (command.BuildFile != null)
            {
                buildResult = new BuildData(waxHub.AwaitSendRequest("compiler", new BuildRequest()
                {
                    BuildFile = command.BuildFile,
                    BuildTarget = command.BuildTarget,
                    OutputDirectoryOverride = command.OutputDirectoryOverride,
                }));

                if (buildResult.HasErrors || command.IsErrorCheckOnly)
                {
                    return buildResult.Errors;
                }
            }
            else
            {
                NotifyStatusChange("BUILD-SKIP");
            }
            NotifyStatusChange("BUILD-END");

            // GET BUNDLE PHASE (Create a CBX bundle OR use the provided one)
            string cbxFilePath = null;

            NotifyStatusChange("CBX-EXPORT-START");
            // Bundle phase version 1: Create one
            if (command.CbxExportPath != null)
            {
                if (buildResult == null) return new Error[] { new Error() { Message = "Cannot export CBX file without a build file." } };

                string outputFolder = (command.CbxExportPath ?? "").Length > 0
                    ? command.CbxExportPath
                    : buildResult.ExportProperties.OutputDirectory.Replace("%TARGET_NAME%", "cbx");
                if (!Path.IsAbsolute(outputFolder))
                {
                    outputFolder = FileUtil.JoinPath(
                        buildResult.ExportProperties.ProjectDirectory,
                        outputFolder);
                }

                byte[] cbxFileBytes = CbxFileEncoder.Encode(buildResult.CbxBundle);

                FileUtil.EnsureFolderExists(outputFolder);
                cbxFilePath = FileUtil.JoinPath(outputFolder, buildResult.ProjectID + ".cbx");
                System.IO.File.WriteAllBytes(cbxFilePath, cbxFileBytes);
            }
            else
            {
                NotifyStatusChange("CBX-EXPORT-SKIP");
            }
            NotifyStatusChange("CBX-EXPORT-END");

            // Bundle phase version 2: Use the provided one
            NotifyStatusChange("CBX-FETCH-START");
            if (command.CbxFile != null)
            {
                if (!System.IO.File.Exists(cbxFilePath))
                {
                    return new Error[] { new Error() { Message = "The provided CBX file does not exist: " + command.CbxFile } };
                }
                cbxFilePath = command.CbxFile;
            }
            else
            {
                NotifyStatusChange("CBX-FETCH-SKIP");
            }
            NotifyStatusChange("CBX-FETCH-END");

            // EXTENSION PHASE
            NotifyStatusChange("EXTENSIONS-START");
            if (command.Extensions.Length == 0)
            {
                NotifyStatusChange("EXTENSIONS-SKIP");
            }
            foreach (string extensionName in command.Extensions)
            {
                NotifyStatusChange("EXTENSION-RUN-START:" + extensionName);
                Dictionary<string, object> extensionRequest = new Dictionary<string, object>();
                if (cbxFilePath != null) extensionRequest["cbxFile"] = cbxFilePath;
                if (buildResult != null) extensionRequest["buildData"] = buildResult;
                foreach (ExtensionArg extensionArg in command.ExtensionArgs.Where(extArg => extArg.Extension == extensionName && extArg.Name != null && extArg.Name.Length > 0))
                {
                    extensionRequest[extensionArg.Name] = extensionArg.Value;
                }
                Dictionary<string, object> extensionResult = waxHub.AwaitSendRequest(extensionName, extensionRequest);
                Error[] extensionErrors = Error.GetErrorList(extensionResult);
                if (extensionErrors.Length > 0) return extensionErrors;
                NotifyStatusChange("EXTENSION-RUN-END:" + extensionName);
            }
            NotifyStatusChange("EXTENSIONS-END");

            // RUN PHASE
            NotifyStatusChange("RUN-START");
            if (cbxFilePath != null || buildResult != null)
            {
                Dictionary<string, object> runtimeRequest = new Dictionary<string, object>() {
                    { "realTimePrint", true },
                    { "args", command.RuntimeArgs },
                    { "showLibStack", command.ShowLibraryStackTraces },
                    { "useOutputPrefixes", command.UseOutputPrefixes },
                };
                if (buildResult != null)
                {
                    runtimeRequest["cbxBundle"] = buildResult.CbxBundle;
                }
                else
                {
                    runtimeRequest["cbxPath"] = cbxFilePath;
                }
                Dictionary<string, object> runtimeResult = waxHub.AwaitSendRequest("runtime", runtimeRequest);
                // TODO: return errors
            }
            else
            {
                NotifyStatusChange("RUN-SKIP");
            }
            NotifyStatusChange("RUN-END");

            return new Error[0];
        }

        private static void NotifyStatusChange(string status)
        {
            ConsoleWriter.Print(ConsoleMessageType.STATUS_CHANGE, status);
        }
    }
}
