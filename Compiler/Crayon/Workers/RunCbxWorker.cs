using CommonUtil;
using CommonUtil.Disk;
using System;

namespace Crayon
{
    public class RunCbxWorker
    {
        private string CrayonRuntimePath
        {
            get
            {
                string binary = System.Environment.OSVersion.Platform == System.PlatformID.Unix
                    ? "CrayonRuntime"
                    : "CrayonRuntime.exe";

                return FileUtil.JoinPath(Environment.GetEnvironmentVariable("CRAYON_HOME"), "vm", binary);
            }
        }

        private class CrayonRuntimeProcess : CommonUtil.Processes.Process
        {
            public CrayonRuntimeProcess(string crayonRuntimePath, string flags)
                : base(crayonRuntimePath, flags) { }

            public override void OnStdOutReceived(string data)
            {
                // TODO: Use ConsoleWriter here. Currently the prefixes are being
                // added by the VM itself, which is not a very good place to put that
                // logic. Also update StdErr below as well.
                Print.Line(data);
            }

            public override void OnStdErrReceived(string data)
            {
                Print.ErrorLine(data);
            }
        }

        public void DoWorkImpl(string flags)
        {
            new CrayonRuntimeProcess(this.CrayonRuntimePath, flags).RunBlocking();
        }
    }
}
