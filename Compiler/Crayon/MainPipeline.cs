using Build;
using Exporter;
using System;
using System.Collections.Generic;

namespace Crayon.Pipeline
{
    internal static class MainPipeline
    {
        public static void Run()
        {
            ExportCommand command = new TopLevelCheckWorker().DoWorkImpl();
            BuildContext buildContext;

            switch (TopLevelCheckWorker.IdentifyUseCase(command))
            {
                case ExecutionType.SHOW_USAGE:
                    new UsageDisplayWorker().DoWorkImpl();
                    break;

                case ExecutionType.GENERATE_DEFAULT_PROJECT:
                    new GenerateDefaultProjectWorker().DoWorkImpl(command);
                    break;

                case ExecutionType.EXPORT_VM_BUNDLE:
                    buildContext = new GetBuildContextWorker().DoWorkImpl(command);
                    ExportBundle result = Exporter.Pipeline.ExportCbxVmBundlePipeline.Run(command, buildContext);
                    if (command.ShowLibraryDepTree)
                    {
                        new ShowAssemblyDepsWorker().DoWorkImpl(result.UserCodeScope);
                    }
                    break;

                case ExecutionType.EXPORT_VM_STANDALONE:
                    Exporter.Pipeline.ExportStandaloneVmPipeline.Run(command);
                    break;

                case ExecutionType.ERROR_CHECK_ONLY:
                    if (command.IsJsonOutput)
                    {
                        try
                        {
                            DoExportStandaloneCbxFileAndGetPath(command, true);
                        }
                        catch (Exception e)
                        {
                            RenderErrorInfoAsJson(e);
                        }
                    }
                    else
                    {
                        DoExportStandaloneCbxFileAndGetPath(command, true);
                        RenderErrorInfoAsJson(null); // renders the JSON object with the right schema, but empty.
                    }
                    break;

                case ExecutionType.EXPORT_CBX:
                    if (command.IsJsonOutput)
                    {
                        try
                        {
                            DoExportStandaloneCbxFileAndGetPath(command, false);
                        }
                        catch (Exception e)
                        {
                            RenderErrorInfoAsJson(e);
                        }
                    }
                    else
                    {
                        DoExportStandaloneCbxFileAndGetPath(command, false);
                    }
                    break;

                case ExecutionType.RUN_CBX:
                    string cbxFileLocation = null;
                    if (command.IsJsonOutput)
                    {
                        try
                        {
                            cbxFileLocation = DoExportStandaloneCbxFileAndGetPath(command, false);
                        }
                        catch (Exception e)
                        {
                            RenderErrorInfoAsJson(e);
                            return;
                        }
                    }
                    else
                    {
                        cbxFileLocation = DoExportStandaloneCbxFileAndGetPath(command, false);
                    }

                    string cmdLineFlags = new RunCbxFlagBuilderWorker().DoWorkImpl(command, cbxFileLocation);

                    if (command.ShowPerformanceMarkers)
                    {
                        ShowPerformanceMetrics();
                    }

                    new RunCbxWorker().DoWorkImpl(cmdLineFlags);

                    return;
            }

            if (command.ShowPerformanceMarkers)
            {
                ShowPerformanceMetrics();
            }
        }

        private static string DoExportStandaloneCbxFileAndGetPath(ExportCommand command, bool isDryRunErrorCheck)
        {
            BuildContext buildContext = new GetBuildContextCbxWorker().DoWorkImpl(command);

            if (isDryRunErrorCheck)
            {
                Exporter.Pipeline.PerformErrorCheckPipeline.Run(command, buildContext);
                return null;
            }
            else
            {
                return Exporter.Pipeline.ExportStandaloneCbxPipeline.Run(command, buildContext);
            }
        }

        private static void RenderErrorInfoAsJson(System.Exception exception)
        {
            List<System.Exception> exceptions = new List<System.Exception>();
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
            System.Console.WriteLine(output);
        }

        private static void ShowPerformanceMetrics()
        {
#if DEBUG
            System.Console.WriteLine(Common.PerformanceTimer.GetSummary());
#endif
        }
    }
}
