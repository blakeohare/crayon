using Build;
using Exporter;

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
                    try
                    {
                        buildContext = new GetBuildContextCbxWorker().DoWorkImpl(command);
                        Exporter.Pipeline.PerformErrorCheckPipeline.Run(command, buildContext);

                        RenderErrorInfo();
                    }
                    catch (Parser.ParserException pe)
                    {
                        RenderErrorInfo(pe);
                    }
                    catch (Parser.MultiParserException mpe)
                    {
                        RenderErrorInfo(mpe.ParseExceptions);
                    }
                    catch (System.Exception e)
                    {
                        RenderErrorInfo(e);
                    }
                    break;

                case ExecutionType.EXPORT_CBX:
                    buildContext = new GetBuildContextCbxWorker().DoWorkImpl(command);
                    Exporter.Pipeline.ExportStandaloneCbxPipeline.Run(command, buildContext);
                    break;

                case ExecutionType.RUN_CBX:

                    buildContext = new GetBuildContextCbxWorker().DoWorkImpl(command);
                    string cbxFileLocation = Exporter.Pipeline.ExportStandaloneCbxPipeline.Run(command, buildContext);

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

        private static void RenderErrorInfo(params System.Exception[] exceptions)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append("{ \"errors\": [");
            for (int i = 0; i < exceptions.Length; ++i)
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
