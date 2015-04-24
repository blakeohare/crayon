using System;
using System.Collections.Generic;
using System.Linq;

namespace Crayon
{
	internal class Program
	{
		// TODO:
		// - flag to append platform to output folder
		// - nameformatted flag
		private static readonly string USAGE = string.Join("\n", new string[] {
			"Usage: [path_to_build_file -target targetname] [flags]",
			"",
			"Flags:",
			"",
			"  -jsfileprefix      Prefix to add to file references in JavaScript projects.",
			"                     Can be absolute.",
			"",
			"  -min               Minify the code (JavaScript platform only).",
			"",
			"  -output            Output directory. Compiled output will go here.",
			"",
			"  -name              Project name.",
			"",
			"  -platform          Platform to compile to.",
			"     Platform choices:",
			"        csopengl         C# Project for Desktop (uses OpenTK)",
			"        cswindowsphone   C# Project for Windows Phone",
			"        cswinforms       C# Project for Desktop (uses WinForms)",
			"        java             Java Project for Desktop (uses AWT)",
			"        js               JavaScript project",
			"        python           Python project (uses PyGame)",
			"",
			"  -readablebytecode  Output a file of the final byte code in a semi-readable",
			"                     fashion for debugging purposes.",
			"",
			"  -source            Source directory. Must contain a start.cry file.",
			"",
			"  -target            When a build file is specified, selects the target within",
			"                     that build file to build."
		});

		static void Main(string[] args)
		{
#if DEBUG
			// First chance exceptions should crash in debug builds.
			Program.Compile(args);
#else

			if (args.Length== 0)
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
			string readableByteCodeFile = buildContext.ReadableByteCode
				? "byte_code_dump.txt"
				: null;
			platform.Compile(buildContext, buildContext.SourceFolder, buildContext.OutputFolder, readableByteCodeFile);
		}

		private static AbstractPlatform GetPlatformInstance(BuildContext buildContext)
		{
			switch (buildContext.Platform.ToLowerInvariant())
			{
				case "copengl": return new Crayon.Translator.COpenGL.COpenGLPlatform();
				case "csopengl": return new Crayon.Translator.CSharp.CSharpOpenTkPlatform();
				case "cswindowsphone": return new Crayon.Translator.CSharp.CSharpWindowsPhonePlatform();
				case "cswinforms": return new Crayon.Translator.CSharp.CSharpWinFormsPlatform();
				case "java": return new Crayon.Translator.Java.JavaPlatform();
				case "js": return new Crayon.Translator.JavaScript.JavaScriptPlatform(buildContext.Minified, buildContext.JsFilePrefix);
				case "python": return new Crayon.Translator.Python.PythonPlatform();
				default:
					throw new InvalidOperationException("Unrecognized platform. See usage.");
			}
		}

		private static BuildContext GetBuildContext(string[] args)
		{
#if DEBUG
			if (args.Length == 0)
			{
				args = @"C:\Things\Crayon\Demos\Volcano\Volcano.build -target python".Split(' ');
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

			buildContext.SourceFolder = System.IO.Path.Combine(workingDirectory, buildContext.SourceFolder).Replace('/', '\\');
			buildContext.OutputFolder = System.IO.Path.Combine(workingDirectory, buildContext.OutputFolder).Replace('/', '\\');

			if (buildContext.Platform == null)
				throw new InvalidOperationException("No platform specified. See usage.");

			if (buildContext.SourceFolder == null)
				throw new InvalidOperationException("No source folder specified. See usage.");

			if (buildContext.OutputFolder == null)
				throw new InvalidOperationException("No output folder specified. See usage.");

			if (!System.IO.Directory.Exists(buildContext.SourceFolder))
				throw new InvalidOperationException("Source folder does not exist.");

			string startCry = System.IO.Path.Combine(buildContext.SourceFolder, "start.cry");
			if (!System.IO.File.Exists(startCry))
				throw new InvalidOperationException("Program entry point could not be found. (start.cry)");

			buildContext.ProjectID = buildContext.ProjectID ?? "Untitled Crayon Project";

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
