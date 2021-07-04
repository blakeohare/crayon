using Common;
using CommonUtil.Disk;

namespace Crayon
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string[] commandLineArgs = Program.GetEffectiveArgs(args);

            CommonUtil.Wax.WaxHub waxHub = new CommonUtil.Wax.WaxHub();
            waxHub.RegisterService(new RouterService());
            waxHub.AwaitSendRequest(
                "router",
                new System.Collections.Generic.Dictionary<string, object>() {
                    { "args", commandLineArgs }
                });
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
