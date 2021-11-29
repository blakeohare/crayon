using Interpreter.Structs;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interpreter.Vm
{
    internal static class ProcessHelper
    {
        internal static int GetCurrentId()
        {
            return System.Diagnostics.Process.GetCurrentProcess().Id;
        }

        internal static string ListProcesses()
        {
            System.Diagnostics.Process[] processes = System.Diagnostics.Process.GetProcesses();
            List<string> rows = new List<string>();
            foreach (System.Diagnostics.Process proc in processes)
            {
                int id = proc.Id;
                string name = proc.ProcessName;
                rows.Add(id + ":" + name);
            }
            return string.Join('\n', rows);
        }

        internal static bool KillProcess(int pid)
        {
            System.Diagnostics.Process p = System.Diagnostics.Process.GetProcessById(pid);
            if (p == null) return false;
            try
            {
                p.Kill();
            }
            catch (System.Exception)
            {
                return false;
            }
            return true;
        }

        internal static object LaunchProcess(VmContext vm, string execName, string[] args, Value callbackFp, int[] intOut, string cwd, int flags)
        {
            bool isWindows = System.Environment.OSVersion.Platform == System.PlatformID.Win32NT;
            bool useShell = (flags & 1) != 0;

            System.Diagnostics.Process p = new System.Diagnostics.Process();

            List<string> actualArgs = new List<string>();
            if (isWindows)
            {
                actualArgs.AddRange(new string[] { "/C", execName });
            }
            else if (!useShell && execName.EndsWith(".app") && System.IO.Directory.Exists(execName))
            {
                intOut[0] = -2;
                return null;
            }
            actualArgs.AddRange(args);

            string argsFlatString = string.Join(" ", actualArgs.Select(s => EscapeStringForCommandLine(s)));

            System.Diagnostics.ProcessStartInfo startInfo;
            if (isWindows)
            {
                startInfo = new System.Diagnostics.ProcessStartInfo("C:\\Windows\\System32\\cmd.exe", argsFlatString);
            }
            else
            {
                startInfo = new System.Diagnostics.ProcessStartInfo(execName, argsFlatString);
            }

            List<string> unifiedStream = new List<string>();

            startInfo.UseShellExecute = useShell;
            if (!useShell)
            {
                startInfo.RedirectStandardOutput = true;
                startInfo.RedirectStandardError = true;
                p.OutputDataReceived += (sender, e) => {
                    EmitToBuffer(vm, callbackFp, unifiedStream, "S", e.Data);
                };
                p.ErrorDataReceived += (sender, e) => {
                    EmitToBuffer(vm, callbackFp, unifiedStream, "E", e.Data);
                };
            }
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            if (cwd != null) startInfo.WorkingDirectory = cwd;
            p.EnableRaisingEvents = true;
            p.StartInfo = startInfo;

            p.Exited += (sender, e) =>
            {
                if (!useShell)
                {
                    string stdRemaining = p.StandardOutput.ReadToEnd();
                    if (stdRemaining.Length > 0)
                    {
                        EmitToBuffer(vm, callbackFp, unifiedStream, "S", stdRemaining);
                    }
                }
                EmitToBuffer(vm, callbackFp, unifiedStream, "X", "0");
            };
            bool started = p.Start();
            if (started)
            {
                int pid = p.Id;
                intOut[0] = pid;
            }
            else
            {
                intOut[0] = -1;
            }
            return p;
        }

        private static void EmitToBuffer(VmContext vm, Value cb, List<string> unifiedStream, string streamName, string value)
        {
            string[] lines = value.Split('\n').Select(line => line + "\n").ToArray();
            bool newFlushTriggerRequired = false;
            lock (unifiedStream)
            {
                newFlushTriggerRequired = unifiedStream.Count == 0;
                foreach (string line in lines)
                {
                    unifiedStream.Add(streamName);
                    unifiedStream.Add(line);
                }
            }

            if (newFlushTriggerRequired)
            {
                Task.Delay(2).ContinueWith(t =>
                {
                    FlushStream(vm, cb, unifiedStream);
                });
            }
        }

        private static void FlushStream(VmContext vm, Value cb, List<string> stream)
        {
            lock (stream)
            {
                if (stream.Count > 0)
                {
                    object[] values = stream.Cast<object>().ToArray();
                    stream.Clear();
                    TranslationHelper.GetEventLoop(vm).ExecuteFunctionPointerNativeArgs(cb, new object[] { values });
                }
            }
        }

        private static string EscapeStringForCommandLine(string s)
        {
            StringBuilder sb = new StringBuilder();
            char c;
            for (int i = 0; i < s.Length; ++i)
            {
                c = s[i];
                switch (c)
                {
                    case ' ': sb.Append("\\ "); break;
                    case '\\': sb.Append("\\\\"); break;
                    case '"': sb.Append("\\\""); break;
                    default: sb.Append(c); break;
                }
            }
            return sb.ToString();
        }
    }
}
