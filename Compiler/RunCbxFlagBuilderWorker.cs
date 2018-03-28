using Common;
using Exporter;
using System.Linq;

namespace Crayon
{
    class RunCbxFlagBuilderWorker : AbstractCrayonWorker
    {
        public override string Name { get { return "Crayon::RunCbxFlagBuilder"; } }

        public override CrayonWorkerResult DoWorkImpl(CrayonWorkerResult[] args)
        {
            ExportCommand command = (ExportCommand)args[0].Value;
            CbxExporter exporter = (CbxExporter)args[1].Value;

            string cbxFile = exporter.GetCbxPath();
            cbxFile = FileUtil.GetPlatformPath(cbxFile);
            int processId = System.Diagnostics.Process.GetCurrentProcess().Id;
            string runtimeArgs = string.Join(",", command.DirectRunArgs.Select(s => Utf8Base64.ToBase64(s)));
            string flags = cbxFile + " vmpid:" + processId;
            if (runtimeArgs.Length > 0)
            {
                flags += " runtimeargs:" + runtimeArgs;
            }

            return new CrayonWorkerResult() { Value = flags };
        }
    }
}
