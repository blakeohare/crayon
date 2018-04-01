using System;
using System.Diagnostics;
using Common;

namespace Crayon
{
    public class RunCbxWorker : AbstractCrayonWorker
    {
        public override string Name { get { return "Crayon::RunCbx"; } }

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

        public override CrayonWorkerResult DoWorkImpl(CrayonWorkerResult[] args)
        {
            string flags = (string)args[0].Value;

            Process appProcess = new Process();
            appProcess.StartInfo = new ProcessStartInfo(this.CrayonRuntimePath, flags)
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };
            appProcess.OutputDataReceived += (sender, e) => { if (e.Data != null) Console.WriteLine(e.Data); };
            appProcess.ErrorDataReceived += (sender, e) => { if (e.Data != null) Console.Error.WriteLine(e.Data); };
            appProcess.Start();
            appProcess.BeginOutputReadLine();
            appProcess.BeginErrorReadLine();
            appProcess.WaitForExit();

            return new CrayonWorkerResult();
        }
    }
}
