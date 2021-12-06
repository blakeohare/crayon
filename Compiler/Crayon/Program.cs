using System.Collections.Generic;
using System.Linq;
using Wax.Util.Disk;

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
            waxHub.ErrorsAsExceptions = !IS_RELEASE;

            waxHub.RegisterService(new Router.RouterService());
            waxHub.RegisterService(new AssemblyResolver.AssemblyService());
            waxHub.RegisterService(new Runtime.RuntimeService());
            waxHub.RegisterService(new Compiler.CompilerService());

            Wax.ToolchainCommand command = FlagParser.Parse(commandLineArgs);

            if (command.UseOutputPrefixes)
            {
                Wax.ConsoleWriter.EnablePrefixes();
            }

            foreach (string directory in GetExtensionDirectories())
            {
                waxHub.RegisterExtensionDirectory(directory);
            }

            Dictionary<string, object> result = waxHub.AwaitSendRequest("router", command);
            Wax.Error[] errors = Wax.Error.GetErrorsFromResult(result);

            if (command.UseJsonOutput)
            {
                string jsonErrors = "{\"errors\":[" +
                    string.Join(',', errors.Select(err => err.ToJson())) +
                    "]}";
                Wax.ConsoleWriter.Print(Wax.ConsoleMessageType.COMPILER_INFORMATION, jsonErrors);
            }
            else if (errors.Length > 0)
            {
                ErrorPrinter.ShowErrors(errors, waxHub.ErrorsAsExceptions);
            }
        }

        private static IList<string> GetExtensionDirectories()
        {
            List<string> directories = new List<string>();

            string crayonSource = SourceDirectoryFinder.CrayonSourceDirectory;
            if (crayonSource != null)
            {
                directories.Add(System.IO.Path.Combine(crayonSource, "Extensions"));
            }

            string crayonHome = Wax.Util.EnvironmentVariables.Get("CRAYON_HOME");
            if (crayonHome != null)
            {
                directories.Add(System.IO.Path.Combine(crayonHome, "extensions"));
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
