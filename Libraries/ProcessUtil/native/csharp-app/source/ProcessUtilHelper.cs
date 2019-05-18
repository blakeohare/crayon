using Interpreter.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Interpreter.Libraries.ProcessUtil
{
    internal static class ProcessUtilHelper
    {
        internal static void LaunchProcessImpl(object[] bridge, string execName, List<string> args, bool isAsync, Value callbackFp, VmContext vm)
        {
            System.Diagnostics.Process p = new System.Diagnostics.Process();
            List<string> actualArgs = new List<string>() { "/C", execName };
            actualArgs.AddRange(args);
            string argsFlatString = string.Join(" ", actualArgs.Select(s => EscapeStringForCommandLine(s)));

            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo("C:\\Windows\\System32\\cmd.exe", argsFlatString);

            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            startInfo.UseShellExecute = false;
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            p.OutputDataReceived += (sender, e) => { AddStringToBuffer(bridge[4], bridge, 2, e.Data); };
            p.ErrorDataReceived += (sender, e) => { AddStringToBuffer(bridge[4], bridge, 3, e.Data); };
            p.EnableRaisingEvents = true;
            p.StartInfo = startInfo;

            if (isAsync)
            {
                p.Exited += (sender, e) =>
                {
                    AddStringToBuffer(bridge[4], bridge, 2, p.StandardOutput.ReadToEnd());
                    Vm.TranslationHelper.RunInterpreter(callbackFp, new Value[0]);
                };
                p.Start();
            }
            else
            {
                p.Start();

                p.WaitForExit();
                AddStringToBuffer(bridge[4], bridge, 2, p.StandardOutput.ReadToEnd());

                int exitCode = p.ExitCode;
                lock (bridge[4])
                {
                    bridge[0] = false;
                    bridge[1] = exitCode;
                }
            }
        }

        private static void AddStringToBuffer(object mtx, object[] bridge, int index, string value)
        {
            string[] lines = value.Split('\n');
            lock (mtx)
            {
                ((List<string>)bridge[index]).AddRange(lines);
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

        internal static int ReadBridgeInt(object mtx, object[] bridge, int ndIndex)
        {
            lock (mtx)
            {
                return (int)bridge[ndIndex];
            }
        }

        internal static void ReadBridgeStrings(object mtx, object[] bridge, int ndIndex, List<string> output)
        {
            lock (mtx)
            {
                List<string> buffer = (List<string>)bridge[ndIndex];
                output.AddRange(buffer);
                buffer.Clear();
            }
        }
    }
}
