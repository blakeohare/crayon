using CommonUtil.Disk;
using System;
using System.Diagnostics;

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

        public void DoWorkImpl(string flags)
        {
            Process appProcess = new Process();
            appProcess.StartInfo = new ProcessStartInfo(this.CrayonRuntimePath, flags)
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };
            appProcess.OutputDataReceived += (sender, e) =>
            {
                if (e.Data != null)
                {
                    // Do not use ConsoleWriter. The prefixes are handled by the VM directly.
                    // TODO: ^ that is silly. Undo that and then use ConsoleWriter. StdErr below, too.
                    Console.WriteLine(e.Data);
                }
            };
            appProcess.ErrorDataReceived += (sender, e) =>
            {
                if (e.Data != null)
                {
                    Console.Error.WriteLine(e.Data);
                }
            };
            appProcess.Start();
            appProcess.BeginOutputReadLine();
            appProcess.BeginErrorReadLine();
            appProcess.WaitForExit();
        }
    }
}
