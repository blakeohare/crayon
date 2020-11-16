using Build;
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
        public static void Run(Command command, bool isRelease)
        {
            Result result = RunImpl(command, isRelease);

            if (command.IsJsonOutput)
            {
                RenderErrorInfoAsJson(result.Errors ?? new Error[0]);
            }
            else if (result.HasErrors)
            {
                ErrorPrinter.ShowErrors(result.Errors);
            }
        }

        private static Parser.CompileRequest CreateCompileRequest(BuildContext buildContext)
        {
            Parser.CompileRequest compileRequest = new Parser.CompileRequest() { BuildContext = buildContext };

            foreach (string key in buildContext.TopLevelAssembly.BuildVariableLookup.Keys)
            {
                BuildVarCanonicalized buildVar = buildContext.TopLevelAssembly.BuildVariableLookup[key];
                switch (buildVar.Type)
                {
                    case VarType.BOOLEAN: compileRequest.AddCompileTimeBoolean(key, buildVar.BoolValue); break;
                    case VarType.FLOAT: compileRequest.AddCompileTimeFloat(key, buildVar.FloatValue); break;
                    case VarType.INT: compileRequest.AddCompileTimeInteger(key, buildVar.IntValue); break;
                    case VarType.STRING: compileRequest.AddCompileTimeString(key, buildVar.StringValue); break;
                    default: throw new Exception(); // this should not happen.
                }
            }
            return compileRequest;
        }

        public static Result RunImpl(Command command, bool isRelease)
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
                    new DefaultProjectGenerator().DoWorkImpl(command.DefaultProjectId, command.DefaultProjectLocale);
                    return new Result();

                case ExecutionType.EXPORT_VM_BUNDLE:
                    Parser.CompilationBundle compilation;

                    if (isRelease)
                    {
                        try
                        {
                            buildContext = new GetBuildContextWorker().DoWorkImpl(command);
                            compilation = Parser.Compiler.Compile(CreateCompileRequest(buildContext), isRelease);
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
                        compilation = Parser.Compiler.Compile(CreateCompileRequest(buildContext), isRelease);
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
                        string depTree = AssemblyResolver.AssemblyDependencyResolver.GetDependencyTreeJson(compilation.RootScopeDependencyMetadata).Trim();
                        ConsoleWriter.Print(ConsoleMessageType.LIBRARY_TREE, depTree);
                    }

                    string outputDirectory = command.HasOutputDirectoryOverride
                        ? command.OutputDirectoryOverride
                        : buildContext.OutputFolder;
                    IList<AssemblyResolver.AssemblyMetadata> assemblies = compilation.AllScopesMetadata;
                    ResourceDatabase resourceDatabase = ResourceDatabaseBuilder.PrepareResources(buildContext);
                    ExportRequest exportBundle = BuildExportRequest(compilation.ByteCode, assemblies, buildContext);
                    ExportResponse response = CbxVmBundleExporter.Run(
                            buildContext.Platform.ToLowerInvariant(),
                            buildContext.ProjectDirectory,
                            outputDirectory,
                            compilation.ByteCode,
                            resourceDatabase,
                            assemblies,
                            exportBundle,
                            new PlatformProvider(),
                            isRelease);
                    return new Result() { Errors = response.Errors };

                case ExecutionType.EXPORT_VM_STANDALONE:
                    ExportResponse standaloneVmExportResponse = StandaloneVmExporter.Run(
                        command.VmPlatform,
                        new PlatformProvider(),
                        command.VmExportDirectory,
                        isRelease);
                    return new Result() { Errors = standaloneVmExportResponse.Errors };


                case ExecutionType.ERROR_CHECK_ONLY:
                    NotifyStatusChange("COMPILE-START");
                    ExportResponse errorCheckOnlyResponse = DoExportStandaloneCbxFileAndGetPath(command, true, isRelease);
                    NotifyStatusChange("COMPILE-END");
                    return new Result() { Errors = errorCheckOnlyResponse.Errors };

                case ExecutionType.EXPORT_CBX:
                    NotifyStatusChange("COMPILE-START");
                    ExportResponse cbxOnlyResponse = DoExportStandaloneCbxFileAndGetPath(command, false, isRelease);
                    NotifyStatusChange("COMPILE-END");
                    return new Result() { Errors = cbxOnlyResponse.Errors };

                case ExecutionType.RUN_CBX:
                    NotifyStatusChange("COMPILE-START");
                    ExportResponse exportResult = DoExportStandaloneCbxFileAndGetPath(command, false, isRelease);
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
            IList<AssemblyResolver.AssemblyMetadata> libraryAssemblies,
            BuildContext buildContext)
        {
            return new ExportRequest()
            {
                ByteCode = byteCode,
                LibraryAssemblies = libraryAssemblies.ToArray(),
                ProjectID = buildContext.ProjectID,
                Version = buildContext.TopLevelAssembly.Version,
                Description = buildContext.TopLevelAssembly.Description,
                GuidSeed = buildContext.GuidSeed,
                ProjectTitle = buildContext.ProjectTitle,
                JsFilePrefix = SanitizeJsFilePrefix(buildContext.JsFilePrefix),
                JsFullPage = buildContext.JsFullPage,
                IosBundlePrefix = buildContext.IosBundlePrefix,
                JavaPackage = buildContext.JavaPackage,
                IconPaths = buildContext.IconFilePaths,
                LaunchScreenPath = buildContext.LaunchScreenPath,
                WindowWidth = buildContext.WindowWidth,
                WindowHeight = buildContext.WindowHeight,
                Orientations = buildContext.Orientation,
            };
        }

        private static ExportResponse DoExportStandaloneCbxFileAndGetPath(
            Command command,
            bool isDryRunErrorCheck,
            bool isRelease)
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

            Parser.CompilationBundle compilation = Parser.Compiler.Compile(CreateCompileRequest(buildContext), isRelease);
            if (isDryRunErrorCheck || compilation.HasErrors)
            {
                return new ExportResponse() { Errors = compilation.Errors };
            }

            Dictionary<string, FileOutput> outputFiles = new Dictionary<string, FileOutput>();
            ResourceDatabase resDb = ResourceDatabaseBuilder.PrepareResources(buildContext);
            resDb.PopulateFileOutputContextForCbx(outputFiles);

            string outputFolder = buildContext.OutputFolder.Replace("%TARGET_NAME%", "cbx");
            outputFolder = FileUtil.JoinPath(buildContext.ProjectDirectory, outputFolder);

            string cbxLocation = StandaloneCbxExporter.Run(
                buildContext.ProjectID,
                outputFiles,
                outputFolder,
                compilation.ByteCode,
                compilation.AllScopesMetadata,
                resDb.ResourceManifestFile.TextContent,
                resDb.ImageSheetManifestFile == null ? null : resDb.ImageSheetManifestFile.TextContent,
                resDb.Image2ResourceManifestFile == null ? null : resDb.Image2ResourceManifestFile.TextContent);
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
