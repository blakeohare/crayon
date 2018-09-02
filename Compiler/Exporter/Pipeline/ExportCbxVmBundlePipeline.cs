namespace Exporter.Pipeline
{
    public static class ExportCbxVmBundlePipeline
    {
        public static CompilationBundle Run(ExportCommand command, Build.BuildContext buildContext)
        {
            return new Workers.ExportCbxVmBundleImplWorker().ExportVmBundle(command, buildContext);
        }
    }
}
