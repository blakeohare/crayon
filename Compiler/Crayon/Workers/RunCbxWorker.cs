using CommonUtil;
using CommonUtil.Disk;
using System.Collections.Generic;

namespace Crayon
{
    public class CbxRunnerService : CommonUtil.Wax.WaxService
    {
        public CbxRunnerService() : base("cbxrunner") { }

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
                    Print.Line(data);
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
                    Print.ErrorLine(data);
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
            string flags = (string)request["flags"];
            bool realTimePrint = (bool)request["realTimePrint"];
            CrayonRuntimeProcess proc = new CrayonRuntimeProcess(this.CrayonRuntimePath, flags, realTimePrint);
            proc.RunBlocking();
            string[] output = proc.FlushBuffer();
            cb(new Dictionary<string, object>() {
                { "output", string.Join("\n", output) },
            });
        }
    }
}
