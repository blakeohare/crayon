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
                    CompilationBundle result = Exporter.Pipeline.ExportCbxVmBundlePipeline.Run(command, buildContext);
                    if (command.ShowLibraryDepTree)
                    {
                        new ShowLibraryDepsWorker().DoWorkImpl(result);
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
                        new ShowPerformanceMetricsWorker().DoWork();
                    }

                    new RunCbxWorker().DoWorkImpl(cmdLineFlags);

                    return;
            }

            if (command.ShowPerformanceMarkers)
            {
                new ShowPerformanceMetricsWorker().DoWork();
            }
        }
    }
}
