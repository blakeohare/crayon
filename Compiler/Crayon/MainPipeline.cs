﻿using Build;
using Common;
using CommonUtil.Disk;
using Exporter;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Crayon.Pipeline
{
    internal static class MainPipeline
    {
        public static void Run(Command command, bool isRelease, CommonUtil.Wax.WaxHub waxHub)
        {
            Result result = RunImpl(command, isRelease, waxHub);

            if (command.IsJsonOutput)
            {
                RenderErrorInfoAsJson(result.Errors ?? new Error[0]);
            }
            else if (result.HasErrors)
            {
                ErrorPrinter.ShowErrors(result.Errors);
            }
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

        private static Parser.ExternalCompilationBundle Compile(
            Dictionary<string, object> request,
            CommonUtil.Wax.WaxHub waxHub)
        {
            Dictionary<string, object> resultRaw = waxHub.AwaitSendRequest("compiler", request);
            List<Error> errors = new List<Error>();
            string[] errorsRaw = (string[])request["errors"];
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
            Parser.ExternalCompilationBundle result = new Parser.ExternalCompilationBundle()
            {
                ByteCode = (string)resultRaw["byteCode"],
                DependencyTreeJson = (string)resultRaw["depTree"],
                Errors = errors.ToArray(),
                UsesU3 = (bool)resultRaw["usesU3"],
            };
            return result;
        }

        public static Result RunImpl(Command command, bool isRelease, CommonUtil.Wax.WaxHub waxHub)
        {
            if (command.UseOutputPrefixes)
            {
                ConsoleWriter.EnablePrefixes();
            }

            BuildContext buildContext;

            switch (TopLevelCheckWorker.IdentifyUseCase(command))
            {
                case ExecutionType.SHOW_USAGE:
                    new UsageDisplayWorker().DoWorkImpl();
                    return new Result();

                case ExecutionType.SHOW_VERSION:
                    new VersionDisplayWorker().DoWorkImpl();
                    return new Result();

                case ExecutionType.GENERATE_DEFAULT_PROJECT:
                    new DefaultProjectGenerator().DoWorkImpl(command.DefaultProjectId, command.DefaultProjectLocale, command.DefaultProjectType);
                    return new Result();

                case ExecutionType.EXPORT_VM_BUNDLE:
                    Parser.ExternalCompilationBundle compilation;

                    if (isRelease)
                    {
                        try
                        {
                            buildContext = new GetBuildContextWorker().DoWorkImpl(command);
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
                        buildContext = new GetBuildContextWorker().DoWorkImpl(command);
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

                    ExportRequest exportBundle = BuildExportRequest(compilation.ByteCode, buildContext);
                    ExportResponse response = CbxVmBundleExporter.Run(
                            buildContext.Platform.ToLowerInvariant(),
                            buildContext.ProjectDirectory,
                            outputDirectory,
                            compilation.ByteCode,
                            resourceDatabase,
                            compilation.UsesU3,
                            exportBundle,
                            new PlatformProvider(),
                            isRelease);
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

                    string cmdLineFlags = new RunCbxFlagBuilderWorker().DoWorkImpl(command, exportResult.CbxOutputPath);

                    NotifyStatusChange("RUN-START");
                    new RunCbxWorker().DoWorkImpl(cmdLineFlags);
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
            CommonUtil.Wax.WaxHub waxHub)
        {
            BuildContext buildContext;
            if (isRelease)
            {
                try
                {
                    buildContext = new GetBuildContextCbxWorker().DoWorkImpl(command);
                }
                catch (InvalidOperationException ioe)
                {
                    return new ExportResponse() { Errors = new Error[] { new Error() { Message = ioe.Message } } };
                }
            }
            else
            {
                buildContext = new GetBuildContextCbxWorker().DoWorkImpl(command);
            }

            Parser.ExternalCompilationBundle compilation = Compile(CreateCompileRequest(buildContext, isRelease), waxHub);
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

        private static void RenderErrorInfoAsJson(Error[] errors)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append("{ \"errors\": [");
            for (int i = 0; i < errors.Length; ++i)
            {
                Error error = errors[i];
                if (i > 0) sb.Append(',');

                sb.Append("\n  {");
                if (error.FileName != null)
                {
                    sb.Append("\n    \"file\": \"");
                    sb.Append(error.FileName.Replace("\\", "\\\\"));
                    sb.Append("\",");
                }

                if (error.HasLineInfo)
                {
                    sb.Append("\n    \"col\": ");
                    sb.Append(error.Column + 1);
                    sb.Append(",");
                    sb.Append("\n    \"line\": ");
                    sb.Append(error.Line + 1);
                    sb.Append(",");
                }
                sb.Append("\n    \"message\": \"");
                sb.Append(error.Message.Replace("\\", "\\\\").Replace("\"", "\\\""));
                sb.Append("\"\n  }");
            }
            sb.Append(" ] }");
            string output = sb.ToString();
            WriteCompileInformation(output);
        }

        private static void WriteCompileInformation(string value)
        {
            ConsoleWriter.Print(ConsoleMessageType.COMPILER_INFORMATION, value);
        }
    }
}
