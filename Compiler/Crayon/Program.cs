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

            Wax.WaxHub waxHub = new Wax.WaxHub();
            waxHub.RegisterService(new Router.RouterService());
            waxHub.RegisterService(new AssemblyResolver.AssemblyService());
            waxHub.RegisterService(new Disk.DiskService());
            waxHub.RegisterService(new Runtime.RuntimeService());

            // TODO: these two need to be merged.
            waxHub.RegisterService(new Parser.CompilerService());
            waxHub.RegisterService(new Compiler.Compiler2Service());

            // TODO: these need to be turned into extensions
            waxHub.RegisterService(new Exporter.ExportService("javascript-app"));
            waxHub.RegisterService(new Exporter.ExportService("javascript-app-android"));
            waxHub.RegisterService(new Exporter.ExportService("javascript-app-ios"));

            Dictionary<string, object> request = new Dictionary<string, object>();
            request["args"] = commandLineArgs;

            foreach (string directory in GetExtensionDirectories())
            {
                waxHub.RegisterExtensionDirectory(directory);
            }

            if (!IS_RELEASE)
            {
                request["errorsAsExceptions"] = true;
                string crayonSourcePath = SourceDirectoryFinder.CrayonSourceDirectory;
                if (crayonSourcePath != null)
                {
                    request["crayonSourceRoot"] = crayonSourcePath;
                }
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
