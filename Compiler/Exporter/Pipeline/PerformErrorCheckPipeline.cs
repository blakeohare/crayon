using Build;
using Exporter.ByteCode;

namespace Exporter.Pipeline
{
    public static class PerformErrorCheckPipeline
    {
        public static void Run(ExportCommand command, BuildContext buildContext)
        {
            ExportBundle compilationResult = ExportBundle.Compile(buildContext);
            ByteCodeEncoder.Encode(compilationResult.ByteCode);
        }
    }
}
