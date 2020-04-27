using Common;
using CommonUtil.Disk;
using CommonUtil.Images;

namespace Crayon
{
    internal class Program
    {

#if DEBUG
        private const bool IS_RELEASE = false;
#else
        private const bool IS_RELEASE = true;
#endif

        static void Main(string[] args)
        {
            string[] commandLineArgs = Program.GetEffectiveArgs(args);

            Command command = FlagParser.Parse(commandLineArgs);

            using (new PerformanceSection("Crayon"))
            {
                Pipeline.MainPipeline.Run(command, IS_RELEASE);
            }

#if DEBUG
            if (command.ShowPerformanceMarkers)
            {
                ConsoleWriter.Print(Common.ConsoleMessageType.PERFORMANCE_METRIC, Common.PerformanceTimer.GetSummary());
            }

            // Crash if there were any graphics contexts that weren't cleaned up.
            // This is okay on Windows, but on OSX this is a problem, so ensure that a
            // regressions are quickly noticed.
            Bitmap.Graphics.EnsureCleanedUp();
#endif
        }

        private static string[] GetEffectiveArgs(string[] actualArgs)
        {
#if DEBUG
            if (actualArgs.Length == 0)
            {
                string crayonHome = CommonUtil.Environment.EnvironmentVariables.Get("CRAYON_HOME");
                if (crayonHome != null)
                {
                    string debugArgsFile = FileUtil.JoinPath(crayonHome, "DEBUG_ARGS.txt");
                    if (FileUtil.FileExists(debugArgsFile))
                    {
                        string[] debugArgs = FileUtil.ReadFileText(debugArgsFile).Trim().Split('\n');
                        string lastArgSet = debugArgs[debugArgs.Length - 1].Trim();
                        if (lastArgSet.Length > 0)
                        {
                            return lastArgSet.Split(' ');
                        }
                    }
                }
            }
#endif
            return actualArgs;
        }
    }
}
