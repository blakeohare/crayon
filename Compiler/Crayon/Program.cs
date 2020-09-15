using Common;
using CommonUtil.Disk;

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

            Command command = FlagParser.Parse(commandLineArgs, IS_RELEASE);
            if (command.HasErrors)
            {
                ErrorPrinter.ShowErrors(command.Errors);
            }
            else
            {
                using (new PerformanceSection("Crayon"))
                {
                    Pipeline.MainPipeline.Run(command, IS_RELEASE);
                }
            }

#if DEBUG
            if (command.ShowPerformanceMarkers)
            {
                ConsoleWriter.Print(Common.ConsoleMessageType.PERFORMANCE_METRIC, Common.PerformanceTimer.GetSummary());
            }
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
