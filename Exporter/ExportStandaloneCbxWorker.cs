using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exporter
{
    public class ExportStandaloneCbxWorker : AbstractCrayonWorker
    {
        public override string Name { get { return "Exporter::ExportStandaloneCbxImpl"; } }

        public override CrayonWorkerResult DoWorkImpl(CrayonWorkerResult[] args)
        {
            ExportCommand command = (ExportCommand)args[0].Value;
            string buildFilePath = command.BuildFilePath;
            CbxExporter exporter = new CbxExporter(buildFilePath).Export();
            return new CrayonWorkerResult()
            {
                Value = exporter,
            };
        }
    }
}
