using System;
using System.Collections.Generic;
using System.Linq;

namespace Crayon
{
    internal class Program
    {
#if RELEASE
		private static readonly string USAGE = string.Join("\n", new string[] {
			"Usage:",
            "  crayon BUILD-FILE -target BUILD-TARGET-NAME [OPTIONS...]",
			"",
			"Flags:",
			"",
			"  -target            (REQUIRED) When a build file is specified, selects the",
			"                     target within that build file to build.",
			"",
			"  -readablebytecode  (OPTIONAL) Output a file of the final byte code in a",
			"                     semi-readable fashion for debugging purposes.",
		});
#endif

        static void Main(string[] args)
        {
#if DEBUG
            // First chance exceptions should crash in debug builds.
            Program.Compile(args);
#else

			if (args.Length == 0)
			{
				System.Console.WriteLine(USAGE);
			}
			else
			{
				try
				{
					Program.Compile(args);
				}
				catch (InvalidOperationException e)
				{
					System.Console.Error.WriteLine(e.Message);
				}
				catch (ParserException e)
				{
					System.Console.Error.WriteLine(e.Message);
				}
			}
#endif
        }

        private static void Compile(string[] args)
        {
            BuildContext buildContext = Program.GetBuildContext(args);
            AbstractPlatform platform = GetPlatformInstance(buildContext);
            platform.Compile(buildContext, buildContext.SourceFolder, buildContext.OutputFolder);
        }

        private static AbstractPlatform GetPlatformInstance(BuildContext buildContext)
        {
            switch (buildContext.Platform.ToLowerInvariant())
            {
                case "game-csharp-android": return new Crayon.Translator.CSharp.CSharpXamarinAndroidPlatform();
                case "game-csharp-ios": return new Crayon.Translator.CSharp.CSharpXamarinIosPlatform();
                case "game-csharp-opentk": return new Crayon.Translator.CSharp.CSharpOpenTkPlatform();
                case "game-java-android": return new Crayon.Translator.Java.JavaAndroidPlatform();
                case "game-java-awt": return new Crayon.Translator.Java.JavaAwtPlatform();
                case "game-javascript": return new Crayon.Translator.JavaScript.JavaScriptPlatform(buildContext.Minified, buildContext.JsFilePrefix);
                case "game-python-pygame": return new Crayon.Translator.Python.PythonPlatform();
                case "server-php": return new Crayon.Translator.Php.PhpPlatform();
                case "ui-csharp-winforms": return new Crayon.Translator.CSharp.CSharpWinFormsPlatform();
                case "ui-javascript": throw new NotImplementedException();
                default:
                    throw new InvalidOperationException("Unrecognized platform. See usage.");
            }
        }

        private static BuildContext GetBuildContext(string[] args)
        {
#if DEBUG
            if (args.Length == 0)
            {
                string command;

                command = @"C:\Crayon\UnitTests\UnitTests.build -target windows";

                args = command.Split(' ');
            }
#endif

            Dictionary<string, string> argLookup = Program.ParseArgs(args);

            string buildFile = argLookup.ContainsKey("buildfile") ? argLookup["buildfile"] : null;
            string target = argLookup.ContainsKey("target") ? argLookup["target"] : null;
            string workingDirectory = ".";

            BuildContext buildContext = null;
            if (buildFile != null || target != null)
            {
                if (buildFile == null || target == null)
                {
                    throw new InvalidOperationException("Build file and target must be specified together.");
                }

                argLookup.Remove("buildfile");
                argLookup.Remove("target");
                workingDirectory = System.IO.Path.GetDirectoryName(buildFile);

                if (!System.IO.File.Exists(buildFile))
                {
                    throw new InvalidOperationException("Build file does not exist: " + buildFile);
                }

                buildContext = BuildContext.Parse(System.IO.File.ReadAllText(buildFile), target);
            }

            buildContext = buildContext ?? new BuildContext();

            // command line arguments override build file values if present.

            if (argLookup.ContainsKey("min"))
            {
                buildContext.Minified = true;
                argLookup.Remove("min");
            }

            if (argLookup.ContainsKey("readablebytecode"))
            {
                buildContext.ReadableByteCode = true;
                argLookup.Remove("readablebytecode");
            }

            if (argLookup.ContainsKey("source"))
            {
                buildContext.SourceFolder = argLookup["source"];
                argLookup.Remove("source");
            }

            if (argLookup.ContainsKey("output"))
            {
                buildContext.OutputFolder = argLookup["output"];
                argLookup.Remove("output");
            }

            if (argLookup.ContainsKey("jsfileprefix"))
            {
                buildContext.JsFilePrefix = argLookup["jsfileprefix"];
                argLookup.Remove("jsfileprefix");
            }

            if (argLookup.ContainsKey("platform"))
            {
                buildContext.Platform = argLookup["platform"];
                argLookup.Remove("platform");
            }

            if (argLookup.ContainsKey("name"))
            {
                buildContext.ProjectID = argLookup["name"];
                argLookup.Remove("name");
            }

            if (argLookup.Count > 0)
            {
                throw new InvalidOperationException("Unrecognized command line flags: " +
                    string.Join(", ", argLookup.Keys.OrderBy<string, string>(s => s.ToLowerInvariant())) +
                    ". See usage.");
            }

            if (buildContext.Platform == null)
                throw new InvalidOperationException("No platform specified. See usage.");

            if (buildContext.SourceFolder == null)
                throw new InvalidOperationException("No source folder specified. See usage.");

            if (buildContext.OutputFolder == null)
                throw new InvalidOperationException("No output folder specified. See usage.");

            buildContext.SourceFolder = System.IO.Path.Combine(workingDirectory, buildContext.SourceFolder).Replace('/', '\\');
            buildContext.OutputFolder = System.IO.Path.Combine(workingDirectory, buildContext.OutputFolder).Replace('/', '\\');

            if (!FileUtil.DirectoryExists(buildContext.SourceFolder))
            {
                throw new InvalidOperationException("Source folder does not exist.");
            }

            buildContext.ProjectID = buildContext.ProjectID ?? "Untitled";

            return buildContext;
        }

        private static readonly HashSet<string> ATOMIC_FLAGS = new HashSet<string>("min readablebytecode".Split(' '));
        private static Dictionary<string, string> ParseArgs(string[] args)
        {
            Dictionary<string, string> output = new Dictionary<string, string>();

            for (int i = 0; i < args.Length; ++i)
            {
                if (!args[i].StartsWith("-"))
                {
                    output["buildfile"] = args[i];
                }
                else
                {
                    string flagName = args[i].Substring(1);
                    if (flagName.Length == 0)
                    {
                        continue;
                    }

                    if (ATOMIC_FLAGS.Contains(flagName.ToLowerInvariant()))
                    {
                        output[flagName] = "true";
                    }
                    else if (i + 1 < args.Length)
                    {
                        output[flagName] = args[++i];
                    }
                    else
                    {
                        output[flagName] = "true";
                    }
                }
            }

            return output;
        }
    }
}
