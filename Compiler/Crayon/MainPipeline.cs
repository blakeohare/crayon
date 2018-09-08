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

        private static void ShowPerformanceMetrics()
        {
#if DEBUG
            System.Console.WriteLine(Common.PerformanceTimer.GetSummary());
#endif
        }
    }
}
