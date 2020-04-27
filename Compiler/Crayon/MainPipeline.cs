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
        private static void NotifyStatusChange(string status)
        {
            ConsoleWriter.Print(ConsoleMessageType.STATUS_CHANGE, status);
        }

        private static void WriteCompileInformation(string value)
        {
            ConsoleWriter.Print(ConsoleMessageType.COMPILER_INFORMATION, value);
        }

        public static void Run()
        {
            Command command = new TopLevelCheckWorker().DoWorkImpl();

            if (command.UseOutputPrefixes)
            {
                ConsoleWriter.EnablePrefixes();
            }

            BuildContext buildContext;

            switch (TopLevelCheckWorker.IdentifyUseCase(command))
            {
                case ExecutionType.SHOW_USAGE:
                    new UsageDisplayWorker().DoWorkImpl();
                    break;

                case ExecutionType.SHOW_VERSION:
                    new VersionDisplayWorker().DoWorkImpl();
                    break;

                case ExecutionType.GENERATE_DEFAULT_PROJECT:
                    new DefaultProjectGenerator().DoWorkImpl(command.DefaultProjectId, command.DefaultProjectLocale);
                    break;

                case ExecutionType.EXPORT_VM_BUNDLE:
                    buildContext = new GetBuildContextWorker().DoWorkImpl(command);
                    Parser.CompilationBundle compilation = Parser.CompilationBundle.Compile(buildContext);
                    if (command.ShowDependencyTree)
                    {
                        new ShowAssemblyDepsWorker().DoWorkImpl(compilation.RootScope);
                    }
                    string outputDirectory = command.HasOutputDirectoryOverride
                        ? command.OutputDirectoryOverride
                        : buildContext.OutputFolder;
                    IList<AssemblyResolver.AssemblyMetadata> assemblies = compilation.AllScopes.Select(s => s.Metadata).ToArray();
                    ResourceDatabase resourceDatabase = ResourceDatabaseBuilder.PrepareResources(buildContext);
                    ExportRequest exportBundle = BuildExportRequest(compilation.ByteCode, assemblies, buildContext);
                    Exporter.CbxVmBundleExporter.Run(
                        buildContext.Platform.ToLowerInvariant(),
                        buildContext.ProjectDirectory,
                        outputDirectory,
                        compilation.ByteCode,
                        resourceDatabase,
                        assemblies,
                        exportBundle,
                        command.PlatformProvider);
                    break;

                case ExecutionType.EXPORT_VM_STANDALONE:
                    Exporter.StandaloneVmExporter.Run(
                        command.VmPlatform,
                        command.PlatformProvider,
                        command.VmExportDirectory);
                    break;

                case ExecutionType.ERROR_CHECK_ONLY:
                    NotifyStatusChange("COMPILE-START");
                    if (command.IsJsonOutput)
                    {
                        try
                        {
                            DoExportStandaloneCbxFileAndGetPath(command, true);
                        }
                        catch (Exception e)
                        {
                            RenderErrorInfoAsJson(command, e);
                        }
                    }
                    else
                    {
                        DoExportStandaloneCbxFileAndGetPath(command, true);
                        RenderErrorInfoAsJson(command, null); // renders the JSON object with the right schema, but empty.
                    }
                    NotifyStatusChange("COMPILE-END");
                    break;

                case ExecutionType.EXPORT_CBX:
                    NotifyStatusChange("COMPILE-START");
                    if (command.IsJsonOutput)
                    {
                        try
                        {
                            DoExportStandaloneCbxFileAndGetPath(command, false);
                        }
                        catch (Exception e)
                        {
                            RenderErrorInfoAsJson(command, e);
                        }
                    }
                    else
                    {
                        DoExportStandaloneCbxFileAndGetPath(command, false);
                    }
                    NotifyStatusChange("COMPILE-END");
                    break;

                case ExecutionType.RUN_CBX:
                    NotifyStatusChange("COMPILE-START");
                    string cbxFileLocation = null;
                    if (command.IsJsonOutput)
                    {
                        try
                        {
                            cbxFileLocation = DoExportStandaloneCbxFileAndGetPath(command, false);
                            NotifyStatusChange("COMPILE-END");
                        }
                        catch (Exception e)
                        {
                            RenderErrorInfoAsJson(command, e);
                            NotifyStatusChange("COMPILE-END");
                            NotifyStatusChange("RUN-ABORTED");
                            return;
                        }
                    }
                    else
                    {
                        cbxFileLocation = DoExportStandaloneCbxFileAndGetPath(command, false);
                        NotifyStatusChange("COMPILE-END");
                    }

                    string cmdLineFlags = new RunCbxFlagBuilderWorker().DoWorkImpl(command, cbxFileLocation);

                    if (command.ShowPerformanceMarkers)
                    {
                        ShowPerformanceMetrics(command);
                    }

                    NotifyStatusChange("RUN-START");

                    new RunCbxWorker().DoWorkImpl(cmdLineFlags);
                    NotifyStatusChange("RUN-END");
                    return;
            }

            if (command.ShowPerformanceMarkers)
            {
                ShowPerformanceMetrics(command);
            }
        }

        private static string SanitizeJsFilePrefix(string jsFilePrefix)
        {
            return (jsFilePrefix == null || jsFilePrefix == "" || jsFilePrefix == "/")
                ? ""
                : ("/" + jsFilePrefix.Trim('/') + "/");
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

        private static string DoExportStandaloneCbxFileAndGetPath(Command command, bool isDryRunErrorCheck)
        {
            BuildContext buildContext = new GetBuildContextCbxWorker().DoWorkImpl(command);

            Parser.CompilationBundle compilation = Parser.CompilationBundle.Compile(buildContext);
            if (isDryRunErrorCheck)
            {
                return null;
            }

            Dictionary<string, FileOutput> outputFiles = new Dictionary<string, FileOutput>();
            ResourceDatabase resDb = ResourceDatabaseBuilder.PrepareResources(buildContext);
            resDb.PopulateFileOutputContextForCbx(outputFiles);

            string outputFolder = buildContext.OutputFolder.Replace("%TARGET_NAME%", "cbx");
            outputFolder = FileUtil.JoinPath(buildContext.ProjectDirectory, outputFolder);

            return StandaloneCbxExporter.Run(
                buildContext.ProjectID,
                outputFiles,
                outputFolder,
                compilation.ByteCode,
                compilation.AllScopes.Select(s => s.Metadata).ToArray(),
                resDb.ResourceManifestFile.TextContent,
                resDb.ImageSheetManifestFile == null ? null : resDb.ImageSheetManifestFile.TextContent);
        }

        private static void RenderErrorInfoAsJson(Command command, Exception exception)
        {
            List<Exception> exceptions = new List<Exception>();
            if (exception != null)
            {
                if (exception is Parser.MultiParserException)
                {
                    exceptions.AddRange(((Parser.MultiParserException)exception).ParseExceptions);
                }
                else
                {
                    exceptions.Add(exception);
                }
            }

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append("{ \"errors\": [");
            for (int i = 0; i < exceptions.Count; ++i)
            {
                if (i > 0) sb.Append(',');
                Parser.FileScope fileInfo = null;
                Parser.Token tokenInfo = null;
                string message = exceptions[i].Message;
                Parser.ParserException parserException = exceptions[i] as Parser.ParserException;
                if (parserException != null)
                {
                    fileInfo = parserException.File;
                    tokenInfo = parserException.TokenInfo;
                    message = parserException.OriginalMessage;
                }
                sb.Append("\n  {");
                if (fileInfo != null)
                {
                    sb.Append("\n    \"file\": \"");
                    sb.Append(fileInfo.Name.Replace("\\", "\\\\"));
                    sb.Append("\",");
                }
                if (tokenInfo != null)
                {
                    sb.Append("\n    \"col\": ");
                    sb.Append(tokenInfo.Col + 1);
                    sb.Append(",");
                    sb.Append("\n    \"line\": ");
                    sb.Append(tokenInfo.Line + 1);
                    sb.Append(",");
                }
                sb.Append("\n    \"message\": \"");
                sb.Append(message.Replace("\\", "\\\\").Replace("\"", "\\\""));
                sb.Append("\"\n  }");
            }
            sb.Append(" ] }");
            string output = sb.ToString();
            WriteCompileInformation(output);
        }

        private static void ShowPerformanceMetrics(Command command)
        {
#if DEBUG
            ConsoleWriter.Print(Common.ConsoleMessageType.PERFORMANCE_METRIC, Common.PerformanceTimer.GetSummary());
#endif
        }
    }
}
