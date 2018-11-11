namespace Exporter.Pipeline
{
    public static class ExportCbxVmBundlePipeline
    {
        public static ExportBundle Run(ExportCommand command, Build.BuildContext buildContext, Pastel.IInlineImportCodeLoader codeLoader)
        {
            return new Workers.ExportCbxVmBundleImplWorker().ExportVmBundle(command, buildContext, codeLoader);
        }
    }
}
