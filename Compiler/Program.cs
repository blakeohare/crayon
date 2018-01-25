using Build;
using Common;
using Parser;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Crayon
{
    internal class Program
    {
#if DEBUG
#else
        private static readonly string USAGE = Util.JoinLines(
            "Usage:",
            "  crayon BUILD-FILE -target BUILD-TARGET-NAME [OPTIONS...]",
            "",
            "Flags:",
            "",
            "  -target            When a build file is specified, selects the",
            "                     target within that build file to build.",
            "",
            "  -vm                Output a standalone VM for a platform.",
            "",
            "  -vmdir             Directory to output the VM to (when -vm is",
            "                     specified).",
            "",
            "  -genDefaultProj    Generate a default boilerplate project to",
            "                     the current directory.",
            "",
            "  -genDefaultProjES  Generates a default project with ES locale.",
            "",
            "  -genDefaultProjJP  Generates a default project with JP locale.",
            "");
#endif

        static void Main(string[] args)
        {
            using (new PerformanceSection("Crayon"))
            {
#if DEBUG
                args = GetEffectiveArgs(args);

                // First chance exceptions should crash in debug builds.
                ExecuteProgramUnchecked(args);

                // Crash if there were any graphics contexts that weren't cleaned up.
                // This is okay on Windows, but on OSX this is a problem, so ensure that a
                // regressions are quickly noticed.
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
                        ExecuteProgramUnchecked(args);
                    }
                    catch (InvalidOperationException e)
                    {
                        System.Console.WriteLine(e.Message);
                    }
                    catch (MultiParserException e)
                    {
                        System.Console.WriteLine(e.Message);
                    }
                    catch (ParserException e)
                    {
                        System.Console.WriteLine(e.Message);
                    }
                }
#endif
            }
        }

        private static void ExecuteProgramUnchecked(string[] args)
        {
            ExportCommand command = FlagParser.Parse(args);
            if (command.IsGenerateDefaultProject)
            {
                DefaultProjectGenerator generator = new DefaultProjectGenerator(command.DefaultProjectId, command.DefaultProjectLocale);
                Dictionary<string, FileOutput> project = generator.Validate().Export();

                string directory = FileUtil.JoinPath(
                    FileUtil.GetCurrentDirectory(),
                    generator.ProjectID);
                new FileOutputExporter(directory).ExportFiles(project);

                Console.WriteLine("Empty project exported to directory '" + generator.ProjectID + "/'");
            }
            else
            {
                Program.Compile(command);
            }
#if DEBUG
            if (command.ShowPerformanceMarkers)
            {
                string summary = PerformanceTimer.GetSummary();
                Console.WriteLine(summary);
            }
#endif
        }

        private static string[] GetEffectiveArgs(string[] actualArgs)
        {
#if DEBUG
            if (actualArgs.Length == 0)
            {
                string crayonHome = System.Environment.GetEnvironmentVariable("CRAYON_HOME");
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

        private enum ExecutionType
        {
            EXPORT_VM_BUNDLE,
            EXPORT_VM_STANDALONE,
            EXPORT_CBX,
            RUN_CBX,
            SHOW_USAGE,
        }

        private static ExecutionType IdentifyUseCase(ExportCommand command)
        {
            if (command.IsEmpty) return ExecutionType.SHOW_USAGE;
            if (command.IsVmExportCommand) return ExecutionType.EXPORT_VM_STANDALONE;
            if (command.HasTarget) return ExecutionType.EXPORT_VM_BUNDLE;
            if (command.IsCbxExport) return ExecutionType.EXPORT_CBX;
            return ExecutionType.RUN_CBX;
        }

        private static void Compile(ExportCommand command)
        {
            switch (IdentifyUseCase(command))
            {
                case ExecutionType.EXPORT_CBX:
                    new CbxExporter(command).Export();
                    return;

                case ExecutionType.EXPORT_VM_BUNDLE:
                    Program.ExportVmBundle(command);
                    return;

                case ExecutionType.EXPORT_VM_STANDALONE:
                    Program.ExportStandaloneVm(command);
                    return;

                case ExecutionType.RUN_CBX:
                    string cbxFile = new CbxExporter(command).Export().GetCbxPath();

                    string crayonRuntimePath = FileUtil.JoinPath(Environment.GetEnvironmentVariable("CRAYON_HOME"), "vm", "CrayonRuntime.exe");
                    cbxFile = FileUtil.GetPlatformPath(cbxFile);
                    System.Diagnostics.Process appProcess = new System.Diagnostics.Process();

                    int processId = System.Diagnostics.Process.GetCurrentProcess().Id;
                    string runtimeArgs = string.Join(",", command.DirectRunArgs.Select(s => Utf8Base64.ToBase64(s)));
                    string flags = cbxFile + " vmpid:" + processId;
                    if (runtimeArgs.Length > 0)
                    {
                        flags += " runtimeargs:" + runtimeArgs;
                    }

                    appProcess.StartInfo = new System.Diagnostics.ProcessStartInfo(crayonRuntimePath, flags)
                    {
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                    };
                    appProcess.OutputDataReceived += (sender, e) => { if (e.Data != null) Console.WriteLine(e.Data); };
                    appProcess.ErrorDataReceived += (sender, e) => { if (e.Data != null) Console.Error.WriteLine(e.Data); };
                    appProcess.Start();
                    appProcess.BeginOutputReadLine();
                    appProcess.BeginErrorReadLine();
                    appProcess.WaitForExit();
                    return;

                case ExecutionType.SHOW_USAGE:
#if RELEASE
                    Console.WriteLine(USAGE);
#endif
                    return;

                default:
                    throw new Exception(); // unknown use case.
            }
        }

        private static void ExportStandaloneVm(ExportCommand command)
        {
            using (new PerformanceSection("ExportStandaloneVm"))
            {
                string vmPlatform = command.VmPlatform;
                string vmTargetDirectory = command.VmExportDirectory;
                if (vmPlatform == null || vmTargetDirectory == null)
                {
                    throw new InvalidOperationException("-vm and -vmdir flags must both have correct values.");
                }
                Platform.AbstractPlatform standaloneVmPlatform = platformProvider.GetPlatform(vmPlatform);
                vmTargetDirectory = FileUtil.FinalizeTilde(vmTargetDirectory);
                VmGenerator vmGenerator = new VmGenerator();
                LibraryMetadata[] allLibraries = new LibraryFinder().LibraryFlatList;
                Dictionary<string, FileOutput> result = vmGenerator.GenerateVmSourceCodeForPlatform(
                    standaloneVmPlatform,
                    null,
                    null,
                    allLibraries,
                    vmTargetDirectory,
                    new InlineImportCodeLoader(),
                    VmGenerationMode.EXPORT_VM_AND_LIBRARIES);
                FileOutputExporter exporter = new FileOutputExporter(vmTargetDirectory);
                exporter.ExportFiles(result);
            }
        }

        private static void ExportVmBundle(ExportCommand command)
        {
            using (new PerformanceSection("ExportVmBundle"))
            {
                BuildContext buildContext = GetBuildContext(command);
                Platform.AbstractPlatform platform = GetPlatformInstance(buildContext);
                if (platform == null) throw new InvalidOperationException("Unrecognized platform. See usage.");

                CompilationBundle compilationResult = CompilationBundle.Compile(buildContext);
                LibraryMetadata[] libraries = compilationResult.LibraryScopesUsed.Select(scope => scope.Library).ToArray();

                ResourceDatabase resourceDatabase = ResourceDatabaseBuilder.PrepareResources(buildContext, compilationResult.ByteCode);

                string outputDirectory = buildContext.OutputFolder;
                if (!FileUtil.IsAbsolutePath(outputDirectory))
                {
                    outputDirectory = FileUtil.JoinPath(buildContext.ProjectDirectory, outputDirectory);
                }
                outputDirectory = FileUtil.GetCanonicalizeUniversalPath(outputDirectory);
                FileOutputExporter exporter = new FileOutputExporter(outputDirectory);

                VmGenerator vmGenerator = new VmGenerator();
                Dictionary<string, FileOutput> result = vmGenerator.GenerateVmSourceCodeForPlatform(
                    platform,
                    compilationResult,
                    resourceDatabase,
                    libraries,
                    outputDirectory,
                    new InlineImportCodeLoader(),
                    VmGenerationMode.EXPORT_SELF_CONTAINED_PROJECT_SOURCE);

                exporter.ExportFiles(result);

                if (command.ShowLibraryDepTree)
                {
                    string libs = LibraryDependencyResolver.GetDependencyTreeLog(compilationResult.LibraryScopesUsed.Select(scope => scope.Library).ToArray());
                    Console.WriteLine("<LibraryDependencies>");
                    Console.WriteLine(libs.Trim());
                    Console.WriteLine("</LibraryDependencies>");
                }
            }
        }

        private static Platform.AbstractPlatform GetPlatformInstance(BuildContext buildContext)
        {
            string platformId = buildContext.Platform.ToLowerInvariant();
            return platformProvider.GetPlatform(platformId);
        }

        private static PlatformProvider platformProvider = new PlatformProvider();

        private static BuildContext GetBuildContext(ExportCommand command)
        {
            using (new PerformanceSection("GetBuildContext"))
            {
                string buildFile = command.BuildFilePath;
                string target = command.BuildTarget;

                if (buildFile == null || target == null)
                {
                    throw new InvalidOperationException("Build file and target must be specified together.");
                }

                buildFile = BuildContext.GetValidatedCanonicalBuildFilePath(buildFile);

                string projectDirectory = FileUtil.GetParentDirectory(buildFile);

                BuildContext buildContext = null;

                buildContext = BuildContext.Parse(projectDirectory, FileUtil.ReadFileText(buildFile), target);

                buildContext = buildContext ?? new BuildContext();

                // command line arguments override build file values if present.

                if (buildContext.Platform == null)
                    throw new InvalidOperationException("No platform specified in build file.");

                if (buildContext.SourceFolders.Length == 0)
                    throw new InvalidOperationException("No source folder specified in build file.");

                if (buildContext.OutputFolder == null)
                    throw new InvalidOperationException("No output folder specified in build file.");

                buildContext.OutputFolder = FileUtil.JoinAndCanonicalizePath(projectDirectory, buildContext.OutputFolder);

                if (buildContext.LaunchScreenPath != null)
                {
                    buildContext.LaunchScreenPath = FileUtil.JoinAndCanonicalizePath(projectDirectory, buildContext.LaunchScreenPath);
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
        }
    }
}
