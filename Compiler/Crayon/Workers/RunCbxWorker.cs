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
                return FileUtil.JoinPath(
                    Environment.GetEnvironmentVariable("CRAYON_HOME"),
                    "vm",
                    "CrayonRuntime.exe");
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
                Console.WriteLine(data);
            }

            public override void OnStdErrReceived(string data)
            {
                Console.Error.WriteLine(data);
            }
        }

        public void DoWorkImpl(string flags)
        {
            new CrayonRuntimeProcess(this.CrayonRuntimePath, flags).RunBlocking();
        }
    }
}
