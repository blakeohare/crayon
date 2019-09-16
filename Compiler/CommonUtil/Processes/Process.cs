namespace CommonUtil.Processes
{
    public abstract class Process
    {
        private System.Diagnostics.Process internalProcess;

        public Process(string path, string flags)
        {
            this.internalProcess = new System.Diagnostics.Process();

            this.internalProcess.StartInfo = new System.Diagnostics.ProcessStartInfo(path, flags)
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };

            this.internalProcess.OutputDataReceived += (sender, e) =>
            {
                if (e.Data != null)
                {
                    // Do not use ConsoleWriter. The prefixes are handled by the VM directly.
                    // TODO: ^ that is silly. Undo that and then use ConsoleWriter. StdErr below, too.
                    this.OnStdOutReceived(e.Data);
                }
            };

            this.internalProcess.ErrorDataReceived += (sender, e) =>
            {
                if (e.Data != null)
                {
                    this.OnStdErrReceived(e.Data);
                }
            };
        }

        public void RunBlocking()
        {
            this.internalProcess.Start();
            this.internalProcess.BeginOutputReadLine();
            this.internalProcess.BeginErrorReadLine();
            this.internalProcess.WaitForExit();
        }

        public abstract void OnStdOutReceived(string data);
        public abstract void OnStdErrReceived(string data);
    }
}
