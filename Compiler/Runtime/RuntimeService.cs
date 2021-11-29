using CommonUtil.Disk;
using System.Collections.Generic;
using System.Linq;
using Wax;

namespace Runtime
{
    public class RuntimeService : WaxService
    {
        public RuntimeService() : base("runtime") { }

        private string CrayonRuntimePath
        {
            get
            {
                string binary = System.Environment.OSVersion.Platform == System.PlatformID.Unix
                    ? "CrayonRuntime"
                    : "CrayonRuntime.exe";

                return FileUtil.JoinPath(System.Environment.GetEnvironmentVariable("CRAYON_HOME"), "vm", binary);
            }
        }

        private class CrayonRuntimeProcess : CommonUtil.Processes.Process
        {
            private bool realTimePrint;
            private List<string> buffer = new List<string>();

            public CrayonRuntimeProcess(string crayonRuntimePath, string flags, bool realTimePrint)
                : base(crayonRuntimePath, flags)
            {
                this.realTimePrint = realTimePrint;
            }

            public override void OnStdOutReceived(string data)
            {
                // TODO: Use ConsoleWriter here. Currently the prefixes are being
                // added by the VM itself, which is not a very good place to put that
                // logic. Also update StdErr below as well.
                if (this.realTimePrint)
                {
                    CommonUtil.Print.Line(data);
                }
                else
                {
                    this.buffer.Add(data);
                }
            }

            public override void OnStdErrReceived(string data)
            {
                if (this.realTimePrint)
                {
                    CommonUtil.Print.ErrorLine(data);
                }
                else
                {
                    this.buffer.Add(data);
                }
            }

            public string[] FlushBuffer()
            {
                string[] output = this.buffer.ToArray();
                this.buffer.Clear();
                return output;
            }
        }

        public override void HandleRequest(Dictionary<string, object> request, System.Func<Dictionary<string, object>, bool> cb)
        {
            bool realTimePrint = (bool)request["realTimePrint"];
            string cbxPath = (string)request["cbxPath"];
            bool showLibStack = (bool)request["showLibStack"];
            bool useOutputPrefixes = (bool)request["useOutputPrefixes"];
            string[] args = (string[])request["args"];

            if (cbxPath.Contains("{DISK:"))
            {
                cbxPath = (string)this.Hub.AwaitSendRequest("disk", new Dictionary<string, object>() {
                    { "command", "resolvePath" },
                    { "path", cbxPath },
                })["path"];
            }

            string cbxFile = FileUtil.GetPlatformPath(cbxPath);
            int processId = CommonUtil.Process.ProcessUtil.GetCurrentProcessId();

            List<string> flagGroups = new List<string>();
            flagGroups.Add("\"" + cbxFile + "\"");
            flagGroups.Add("parentProcessId:" + processId);
            flagGroups.Add("showLibStack:" + (showLibStack ? "yes" : "no"));
            flagGroups.Add("showOutputPrefixes:" + (useOutputPrefixes ? "yes" : "no"));

            int userCmdLineArgCount = args.Length;
            if (userCmdLineArgCount > 0)
            {
                flagGroups.Add("runtimeargs:" + userCmdLineArgCount + ":" + string.Join(",", args.Select(s => CommonUtil.Base64.ToBase64(s))));
            }
            else
            {
                flagGroups.Add("runtimeargs:0");
            }

            string flags = string.Join(" ", flagGroups);
            CrayonRuntimeProcess proc = new CrayonRuntimeProcess(this.CrayonRuntimePath, flags, realTimePrint);
            proc.RunBlocking();
            string[] output = proc.FlushBuffer();
            cb(new Dictionary<string, object>() {
                { "output", string.Join("\n", output) },
            });
        }
    }
}
