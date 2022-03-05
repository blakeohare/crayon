using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        [STAThread]
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

            MainTask(commandLineArgs).Wait();
        }

        private static async Task MainTask(string[] args)
        {
            Wax.WaxHub waxHub = new Wax.WaxHub();

            waxHub.SourceRoot = SourceDirectoryFinder.CrayonSourceDirectory;
            waxHub.ErrorsAsExceptions = !IS_RELEASE;

            waxHub.RegisterService(new Router.RouterService());
            waxHub.RegisterService(new AssemblyResolver.AssemblyService());
            waxHub.RegisterService(new Runtime.RuntimeService());
            waxHub.RegisterService(new Builder.BuilderService());
            waxHub.RegisterService(new DiskService());
            waxHub.RegisterService(new U3.U3Service());

            Wax.ToolchainCommand command = FlagParser.Parse(args);

            if (command.UseOutputPrefixes)
            {
                Wax.ConsoleWriter.EnablePrefixes();
            }

            foreach (string directory in GetExtensionDirectories())
            {
                waxHub.RegisterExtensionDirectory(directory);
            }

            waxHub.RegisterLibraryDirectory(GetLibraryDirectory());

            Dictionary<string, object> result = await waxHub.SendRequest("router", command);

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

        private static string[] GetExtensionDirectories()
        {
            List<string> directories = new List<string>();

            string crayonSource = SourceDirectoryFinder.CrayonSourceDirectory;
            if (crayonSource != null)
            {
                directories.Add(System.IO.Path.Combine(crayonSource, "Extensions"));
            }

            directories.Add(System.IO.Path.Combine(GetExecutableDirectory(), "extensions"));

            return directories
                .Where(path => System.IO.Directory.Exists(path))
                .ToArray();
        }

        private static string GetExecutableDirectory()
        {
            string codeBase = System.Reflection.Assembly.GetExecutingAssembly().CodeBase;
            Uri uri = new Uri(codeBase + "/..");
            return uri.AbsolutePath;
        }

        private static string GetLibraryDirectory()
        {
            string libraryDir = System.IO.Path.Combine(GetExecutableDirectory(), "libs");
            if (System.IO.Directory.Exists(libraryDir)) return libraryDir;

            string srcDir = SourceDirectoryFinder.CrayonSourceDirectory;
            if (srcDir != null)
            {
                return System.IO.Path.Combine(srcDir, "Libraries");
            }

            return null;
        }

        private static string[] GetEffectiveArgs(string[] actualArgs)
        {
#if DEBUG
            if (actualArgs.Length == 0)
            {
                string crayonHome = SourceDirectoryFinder.CrayonSourceDirectory;
                if (crayonHome != null)
                {
                    string debugArgsFile = System.IO.Path.Combine(crayonHome, "DEBUG_ARGS.txt");
                    if (System.IO.File.Exists(debugArgsFile))
                    {
                        string[] debugArgs = System.IO.File.ReadAllText(debugArgsFile).Trim().Split('\n');
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
