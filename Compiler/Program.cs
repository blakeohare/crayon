using System;
using System.Collections.Generic;
using Common;

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
            SystemBitmap.Graphics.EnsureCleanedUp();
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
            if (platform != null)
            {
                platform.Compile(buildContext, buildContext.OutputFolder);
                return;
            }

            CompilationBundle compilationResult = CompilationBundle.Compile(buildContext);

            Platform.AbstractPlatform platform2 = GetPlatform2Instance(buildContext);
            if (platform2 != null)
            {
                VmGenerator vmGenerator = new VmGenerator();
                vmGenerator.GenerateVmSourceCodeForPlatform(
                    platform2, 
                    null,
                    compilationResult.LibrariesUsed, 
                    VmGenerationMode.EXPORT_SELF_CONTAINED_PROJECT_SOURCE);

                throw new NotImplementedException();
            }

            throw new InvalidOperationException("Unrecognized platform. See usage.");
        }

        private static Platform.AbstractPlatform GetPlatform2Instance(BuildContext buildContext)
        {
            string platformId = buildContext.Platform.ToLowerInvariant();
            return platformProvider.GetPlatform(platformId);
        }

        private static PlatformProvider platformProvider = new PlatformProvider();

        private static AbstractPlatform GetPlatformInstance(BuildContext buildContext)
        {
            switch (buildContext.Platform.ToLowerInvariant())
            {
                case "game-c-opengl": return new Crayon.Translator.C.COpenGlPlatform();
                case "game-csharp-android": return new Crayon.Translator.CSharp.CSharpXamarinAndroidPlatform();
                case "game-csharp-ios": return new Crayon.Translator.CSharp.CSharpXamarinIosPlatform();
                case "game-csharp-opentk": return new Crayon.Translator.CSharp.CSharpOpenTkPlatform();
                case "game-java-android": return new Crayon.Translator.Java.JavaAndroidPlatform();
                case "game-java-awt": return new Crayon.Translator.Java.JavaAwtPlatform();
                case "game-javascript": return new Crayon.Translator.JavaScript.JavaScriptPlatform();
                case "game-python-pygame": return new Crayon.Translator.Python.PythonPlatform();
                case "game-ruby-gosu": return new Crayon.Translator.Ruby.RubyPlatform();
                case "server-php": return new Crayon.Translator.Php.PhpPlatform();
                case "ui-csharp-winforms": return new Crayon.Translator.CSharp.CSharpWinFormsPlatform();
                case "ui-javascript": throw new NotImplementedException();

                // temporary hack to help rewrite the VM into Pastel
                case "vm-pastel-hack": return new Crayon.Translator.Pastel.PastelPlatform();

                default: return null;
            }
        }

        private static BuildContext GetBuildContext(string[] args)
        {
#if DEBUG
            if (args.Length == 0)
            {
                string crayonHome = System.Environment.GetEnvironmentVariable("CRAYON_HOME");
                if (crayonHome != null)
                {
                    string debugArgsFile = System.IO.Path.Combine(crayonHome, "DEBUG_ARGS.txt");
                    if (System.IO.File.Exists(debugArgsFile))
                    {
                        string[] debugArgs = System.IO.File.ReadAllText(debugArgsFile).Trim().Split('\n');
                        string lastArgSet = debugArgs[debugArgs.Length - 1].Trim();
                        if (lastArgSet.Length > 0)
                        {
                            args = lastArgSet.Split(' ');
                        }
                    }
                }
            }
#endif

            Dictionary<string, string> argLookup = Program.ParseArgs(args);

            string buildFile = argLookup.ContainsKey("buildfile") ? argLookup["buildfile"] : null;
            string target = argLookup.ContainsKey("target") ? argLookup["target"] : null;
            if (!buildFile.StartsWith("/") &&
                !(buildFile.Length > 1 && buildFile[1] == ':'))
            {
                // Build file will always be absolute. So make it absolute if it isn't already.
                buildFile = System.IO.Path.GetFullPath(
                    System.IO.Path.Combine(
                        System.IO.Directory.GetCurrentDirectory(), buildFile));

            }

            string projectDirectory = System.IO.Path.GetDirectoryName(buildFile);

            BuildContext buildContext = null;
            if (buildFile != null || target != null)
            {
                if (buildFile == null || target == null)
                {
                    throw new InvalidOperationException("Build file and target must be specified together.");
                }

                argLookup.Remove("buildfile");
                argLookup.Remove("target");
                projectDirectory = System.IO.Path.GetDirectoryName(buildFile);

                if (!System.IO.File.Exists(buildFile))
                {
                    throw new InvalidOperationException("Build file does not exist: " + buildFile);
                }

                buildContext = BuildContext.Parse(projectDirectory, System.IO.File.ReadAllText(buildFile), target);
            }

            buildContext = buildContext ?? new BuildContext();

            // command line arguments override build file values if present.

            if (buildContext.Platform == null)
                throw new InvalidOperationException("No platform specified in build file.");

            if (buildContext.SourceFolders.Length == 0)
                throw new InvalidOperationException("No source folder specified in build file.");

            if (buildContext.OutputFolder == null)
                throw new InvalidOperationException("No output folder specified in build file.");

            buildContext.OutputFolder = System.IO.Path.Combine(projectDirectory, buildContext.OutputFolder).Replace('/', '\\');
            if (buildContext.IconFilePath != null)
            {
                buildContext.IconFilePath = System.IO.Path.Combine(projectDirectory, buildContext.IconFilePath).Replace('/', '\\');
            }

            foreach (FilePath sourceFolder in buildContext.SourceFolders)
            {
                if (!FileUtil.DirectoryExists(sourceFolder.AbsolutePath))
                {
                    throw new InvalidOperationException("Source folder does not exist.");
                }
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
