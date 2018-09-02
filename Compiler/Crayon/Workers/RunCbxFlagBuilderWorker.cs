using Common;
using Exporter;
using System.Linq;

namespace Crayon
{
    class RunCbxFlagBuilderWorker
    {
        public string DoWorkImpl(ExportCommand command, string finalCbxPath)
        {
            string cbxFile = FileUtil.GetPlatformPath(finalCbxPath);
            int processId = System.Diagnostics.Process.GetCurrentProcess().Id;
            string runtimeArgs = string.Join(",", command.DirectRunArgs.Select(s => Utf8Base64.ToBase64(s)));
            string flags = "\"" + cbxFile + "\" vmpid:" + processId + " " + (command.DirectRunShowLibStack ? "showLibStack" : "ignore");

            if (runtimeArgs.Length > 0)
            {
                flags += " runtimeargs:" + runtimeArgs;
            }
            return flags;
        }
    }
}
