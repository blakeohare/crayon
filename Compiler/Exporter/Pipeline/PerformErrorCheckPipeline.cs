using Build;

namespace Exporter.Pipeline
{
    public static class PerformErrorCheckPipeline
    {
        public static void Run(ExportCommand command, BuildContext buildContext)
        {
            ExportBundle.Compile(buildContext);
        }
    }
}
