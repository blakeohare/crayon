using Common;
using Exporter;
using System.Linq;

namespace Crayon
{
    // cmdLineFlags = Crayon::RunCbxFlagBuilder(command, buildContext)
    class RunCbxFlagBuilderWorker : AbstractCrayonWorker
    {
        public override CrayonWorkerResult DoWorkImpl(CrayonWorkerResult[] args)
        {
            ExportCommand command = (ExportCommand)args[0].Value;
            string finalCbxPath = (string)args[1].Value;

            string cbxFile = FileUtil.GetPlatformPath(finalCbxPath);
            int processId = System.Diagnostics.Process.GetCurrentProcess().Id;
            string runtimeArgs = string.Join(",", command.DirectRunArgs.Select(s => Utf8Base64.ToBase64(s)));
            string flags = "\"" + cbxFile + "\" vmpid:" + processId + " " + (command.DirectRunShowLibStack ? "showLibStack" : "ignore");

            if (runtimeArgs.Length > 0)
            {
                flags += " runtimeargs:" + runtimeArgs;
            }

            return new CrayonWorkerResult() { Value = flags };
        }
    }
}
