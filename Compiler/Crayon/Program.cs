using Wax.Util.Disk;
using System.Collections.Generic;

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

            if (commandLineArgs.Length == 0)
            {
                System.Console.WriteLine(UsageDisplay.USAGE);
                return;
            }

            if (commandLineArgs.Length == 1 && commandLineArgs[0] == "-version")
            {
                System.Console.WriteLine(VersionInfo.VersionString);
                return;
            }

            Wax.WaxHub waxHub = new Wax.WaxHub();
            waxHub.SourceRoot = SourceDirectoryFinder.CrayonSourceDirectory;
            waxHub.RegisterService(new Router.RouterService());
            waxHub.RegisterService(new AssemblyResolver.AssemblyService());
            waxHub.RegisterService(new Runtime.RuntimeService());
            waxHub.RegisterService(new Compiler.CompilerService());

            Dictionary<string, object> request = new Dictionary<string, object>();
            request["args"] = commandLineArgs;

            foreach (string directory in GetExtensionDirectories())
            {
                waxHub.RegisterExtensionDirectory(directory);
            }

            if (!IS_RELEASE)
            {
                request["errorsAsExceptions"] = true;
            }

            waxHub.AwaitSendRequest("router", request);
        }

        private static IList<string> GetExtensionDirectories()
        {
            string crayonHome = Wax.Util.EnvironmentVariables.Get("CRAYON_HOME");
            List<string> directories = new List<string>();
            if (crayonHome != null)
            {
                directories.Add(System.IO.Path.Combine(crayonHome, "extensions"));
            }

            string crayonSource = SourceDirectoryFinder.CrayonSourceDirectory;
            if (crayonSource != null)
            {
                directories.Add(System.IO.Path.Combine(crayonSource, "Extensions"));
            }
            return directories;
        }

        private static string[] GetEffectiveArgs(string[] actualArgs)
        {
#if DEBUG
            if (actualArgs.Length == 0)
            {
                string crayonHome = Wax.Util.EnvironmentVariables.Get("CRAYON_HOME");
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
