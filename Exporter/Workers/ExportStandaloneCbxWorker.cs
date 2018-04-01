using Build;
using Common;

namespace Exporter
{
    public class ExportStandaloneCbxWorker : AbstractCrayonWorker
    {
        public override string Name { get { return "Exporter::ExportStandaloneCbxImpl"; } }

        public override CrayonWorkerResult DoWorkImpl(CrayonWorkerResult[] args)
        {
            BuildContext buildContext = (BuildContext)args[1].Value;
            CbxExporter exporter = new CbxExporter();
            exporter.Export(buildContext);
            return new CrayonWorkerResult()
            {
                Value = exporter,
            };
        }
    }
}
