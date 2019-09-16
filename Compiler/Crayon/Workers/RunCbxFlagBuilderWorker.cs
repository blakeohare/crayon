using CommonUtil.Disk;
using Exporter;
using System.Collections.Generic;
using System.Linq;

namespace Crayon
{
    class RunCbxFlagBuilderWorker
    {
        public string DoWorkImpl(ExportCommand command, string finalCbxPath)
        {
            string cbxFile = FileUtil.GetPlatformPath(finalCbxPath);
            int processId = CommonUtil.Process.ProcessUtil.GetCurrentProcessId();

            List<string> flagGroups = new List<string>();
            flagGroups.Add("\"" + cbxFile + "\"");
            flagGroups.Add("parentProcessId:" + processId);
            flagGroups.Add("showLibStack:" + (command.DirectRunShowLibStack ? "yes" : "no"));
            flagGroups.Add("showOutputPrefixes:" + (command.UseOutputPrefixes ? "yes" : "no"));

            int userCmdLineArgCount = command.DirectRunArgs.Length;
            if (userCmdLineArgCount > 0)
            {
                flagGroups.Add("runtimeargs:" + userCmdLineArgCount + ":" + string.Join(",", command.DirectRunArgs.Select(s => CommonUtil.Core.Base64.ToBase64(s))));
            }
            else
            {
                flagGroups.Add("runtimeargs:0");
            }

            string flags = string.Join(" ", flagGroups);
            return flags;
        }
    }
}
