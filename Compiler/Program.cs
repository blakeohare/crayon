using System;
using System.Collections.Generic;
using System.Linq;
using Common;

namespace Crayon
{
    internal class Program
    {
        // TODO: resource file
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
            "");

        static void Main(string[] args)
        {
#if DEBUG
            args = GetEffectiveArgs(args);

            // First chance exceptions should crash in debug builds.
            Program.Compile(args);

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

        private static string GetCommandLineFlagValue(string previousFlag, string[] args)
        {
            for (int i = 0; i < args.Length; ++i)
            {
                if (args[i] == previousFlag && i + 1 < args.Length)
                {
                    return args[i + 1];
                }
            }
            return null;
        }

        private static string[] GetEffectiveArgs(string[] actualArgs)
        {
#if DEBUG
            if (actualArgs.Length == 0)
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

        private static ExecutionType IdentifyUseCase(string[] args)
        {
            if (args.Length == 0) return ExecutionType.SHOW_USAGE;

            foreach (string arg in args)
            {
                if (arg == "-vm" || arg == "-vmdir") return ExecutionType.EXPORT_VM_STANDALONE;
                if (arg == "-target") return ExecutionType.EXPORT_VM_BUNDLE;
                if (arg == "-cbx") return ExecutionType.EXPORT_CBX;
            }
            return ExecutionType.RUN_CBX;
        }

        private static void Compile(string[] args)
        {
            switch (IdentifyUseCase(args))
            {
                case ExecutionType.EXPORT_CBX:
                    Program.ExportCbx(args);
                    return;

                case ExecutionType.EXPORT_VM_BUNDLE:
                    Program.ExportVmBundle(args);
                    return;

                case ExecutionType.EXPORT_VM_STANDALONE:
                    Program.ExportStandaloneVm(args);
                    return;

                case ExecutionType.RUN_CBX:
                    string cbxFile = Program.ExportCbx(args);
                    string crayonRuntimePath = System.IO.Path.Combine(System.Environment.GetEnvironmentVariable("CRAYON_HOME"), "Vm", "CrayonRuntime.exe");
                    cbxFile = FileUtil.GetPlatformPath(cbxFile);
                    System.Diagnostics.Process appProcess = new System.Diagnostics.Process();

                    appProcess.StartInfo = new System.Diagnostics.ProcessStartInfo(crayonRuntimePath, cbxFile)
                    {
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardOutput = true,
                    };
                    appProcess.OutputDataReceived += (sender, e) =>
                    {
                        System.Console.WriteLine(e.Data);
                    };
                    appProcess.Start();
                    appProcess.BeginOutputReadLine();
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

        // TODO: HELPER CLASS
        private static string ExportCbx(string[] args)
        {
            string buildFilePath = args[0]; // TODO: better use case identification that ensures this is actually here.
            BuildContext buildContext = GetBuildContextCbx(buildFilePath);
            CompilationBundle compilationResult = CompilationBundle.Compile(buildContext);
            ResourceDatabase resDb = PrepareResources(buildContext, null);
            string byteCode = ByteCodeEncoder.Encode(compilationResult.ByteCode);
            List<byte> cbxOutput = new List<byte>() { 0 };
            cbxOutput.AddRange("CBX".ToCharArray().Select(c => (byte)c));
            cbxOutput.AddRange(GetBigEndian4Byte(0));
            cbxOutput.AddRange(GetBigEndian4Byte(2));
            cbxOutput.AddRange(GetBigEndian4Byte(0));

            byte[] code = StringToBytes(byteCode);
            cbxOutput.AddRange("CODE".ToCharArray().Select(c => (byte)c));
            cbxOutput.AddRange(GetBigEndian4Byte(code.Length));
            cbxOutput.AddRange(code);

            List<string> libraries = new List<string>();
            foreach (Library library in compilationResult.LibrariesUsed.Where(lib => lib.IsMoreThanJustEmbedCode))
            {
                libraries.Add(library.Name);
                libraries.Add(library.Version);
            }
            string libsData = string.Join(",", libraries);
            byte[] libsDataBytes = StringToBytes(libsData);
            cbxOutput.AddRange("LIBS".ToCharArray().Select(c => (byte)c));
            cbxOutput.AddRange(GetBigEndian4Byte(libsDataBytes.Length));
            cbxOutput.AddRange(libsDataBytes);

            byte[] resourceManifest = StringToBytes(resDb.ResourceManifestFile.TextContent);
            cbxOutput.AddRange("RSRC".ToCharArray().Select(c => (byte)c));
            cbxOutput.AddRange(GetBigEndian4Byte(resourceManifest.Length));
            cbxOutput.AddRange(resourceManifest);

            string outputFolder = buildContext.OutputFolder.Replace("%TARGET_NAME%", "cbx");
            string fullyQualifiedOutputFolder = FileUtil.JoinPath(buildContext.ProjectDirectory, outputFolder);
            string cbxPath = FileUtil.JoinPath(fullyQualifiedOutputFolder, buildContext.ProjectID + ".cbx");
            cbxPath = FileUtil.GetCanonicalizeUniversalPath(cbxPath);
            FileUtil.EnsureParentFolderExists(fullyQualifiedOutputFolder);
            Dictionary<string, FileOutput> output = new Dictionary<string, FileOutput>();
            output[buildContext.ProjectID + ".cbx"] = new FileOutput()
            {
                Type = FileOutputType.Binary,
                BinaryContent = cbxOutput.ToArray(),
            };

            // Resource manifest is embedded into the CBX file

            output["res/image_sheet_manifest.txt"] = resDb.ImageSheetManifestFile;

            foreach (FileOutput txtResource in resDb.TextResources)
            {
                output["res/txt/" + txtResource.CanonicalFileName] = txtResource;
            }
            foreach (FileOutput sndResource in resDb.AudioResources)
            {
                output["res/snd/" + sndResource.CanonicalFileName] = sndResource;
            }
            foreach (FileOutput binResource in resDb.BinaryResources)
            {
                output["res/bin/" + binResource.CanonicalFileName] = binResource;
            }
            foreach (FileOutput imgResource in resDb.ImageResources)
            {
                output["res/img/" + imgResource.CanonicalFileName] = imgResource;
            }
            foreach (string key in resDb.ImageSheetFiles.Keys)
            {
                output["res/img/" + key] = resDb.ImageSheetFiles[key];
            }
            new FileOutputExporter(fullyQualifiedOutputFolder).ExportFiles(output);

            return cbxPath;
        }

        private static byte[] StringToBytes(string value)
        {
            return System.Text.Encoding.UTF8.GetBytes(value);
        }

        private static byte[] GetBigEndian4Byte(int value)
        {
            return new byte[]
            {
                (byte) ((value >> 24) & 255),
                (byte) ((value >> 16) & 255),
                (byte) ((value >> 8) & 255),
                (byte) (value & 255)
            };
        }

        private static void ExportStandaloneVm(string[] args)
        {
            Platform.AbstractPlatform standaloneVmPlatform = platformProvider.GetPlatform(GetCommandLineFlagValue("-vm", args));
            string targetDirectory = GetCommandLineFlagValue("-vmdir", args);
            if (targetDirectory == null || standaloneVmPlatform == null) throw new InvalidOperationException("-vm and -vmdir flags must both have correct values.");
            VmGenerator vmGenerator = new VmGenerator();
            List<Library> allLibraries = new LibraryManager(platformProvider).GetAllAvailableLibraries(standaloneVmPlatform);
            Dictionary<string, FileOutput> result = vmGenerator.GenerateVmSourceCodeForPlatform(
                standaloneVmPlatform,
                null,
                null,
                allLibraries,
                VmGenerationMode.EXPORT_VM_AND_LIBRARIES);
            FileOutputExporter exporter = new FileOutputExporter(targetDirectory);
            exporter.ExportFiles(result);
        }

        private static ResourceDatabase PrepareResources(
            BuildContext buildContext,
            ByteBuffer nullableByteCode) // CBX files will not have this in the resources
        {
            Dictionary<string, FileOutput> output = new Dictionary<string, FileOutput>();
            // This really needs to go in a separate helper file.
            ResourceDatabase resourceDatabase = ResourceDatabaseBuilder.CreateResourceDatabase(buildContext);
            if (nullableByteCode != null)
            {
                resourceDatabase.ByteCodeFile = new FileOutput()
                {
                    Type = FileOutputType.Text,
                    TextContent = ByteCodeEncoder.Encode(nullableByteCode),
                };
            }

            Common.ImageSheets.ImageSheetBuilder imageSheetBuilder = new Common.ImageSheets.ImageSheetBuilder();
            if (buildContext.ImageSheetIds != null)
            {
                foreach (string imageSheetId in buildContext.ImageSheetIds)
                {
                    imageSheetBuilder.PrefixMatcher.RegisterId(imageSheetId);

                    foreach (string fileMatcher in buildContext.ImageSheetPrefixesById[imageSheetId])
                    {
                        imageSheetBuilder.PrefixMatcher.RegisterPrefix(imageSheetId, fileMatcher);
                    }
                }
            }
            Common.ImageSheets.Sheet[] imageSheets = imageSheetBuilder.Generate(resourceDatabase);

            resourceDatabase.AddImageSheets(imageSheets);

            resourceDatabase.GenerateResourceMapping();

            return resourceDatabase;
        }

        private static void ExportVmBundle(string[] args)
        {
            BuildContext buildContext = GetBuildContext(args);
            Platform.AbstractPlatform platform = GetPlatformInstance(buildContext);
            if (platform == null) throw new InvalidOperationException("Unrecognized platform. See usage.");

            CompilationBundle compilationResult = CompilationBundle.Compile(buildContext);

            // Need to re-instantiate the libraries. The libraries are instantiated in a platform-context-free
            // for the purpose of compiling the byte code. For the VM bundle, they need to know about the platform.

            Library[] libraries = compilationResult.LibrariesUsed
                .Select(lib => lib.CloneWithNewPlatform(platform))
                .ToArray();

            ResourceDatabase resourceDatabase = PrepareResources(buildContext, compilationResult.ByteCode);

            VmGenerator vmGenerator = new VmGenerator();
            Dictionary<string, FileOutput> result = vmGenerator.GenerateVmSourceCodeForPlatform(
                platform,
                compilationResult,
                resourceDatabase,
                libraries,
                VmGenerationMode.EXPORT_SELF_CONTAINED_PROJECT_SOURCE);

            string outputDirectory = buildContext.OutputFolder;
            if (!FileUtil.IsAbsolutePath(outputDirectory))
            {
                outputDirectory = FileUtil.JoinPath(buildContext.ProjectDirectory, outputDirectory);
            }
            outputDirectory = FileUtil.GetCanonicalizeUniversalPath(outputDirectory);
            FileOutputExporter exporter = new FileOutputExporter(outputDirectory);

            exporter.ExportFiles(result);
        }

        private static Platform.AbstractPlatform GetPlatformInstance(BuildContext buildContext)
        {
            string platformId = buildContext.Platform.ToLowerInvariant();
            return platformProvider.GetPlatform(platformId);
        }

        private static PlatformProvider platformProvider = new PlatformProvider();

        private static string GetValidatedCanonicalBuildFilePath(string originalBuildFilePath)
        {
            string buildFilePath = originalBuildFilePath;
            buildFilePath = FileUtil.FinalizeTilde(buildFilePath);
            if (!buildFilePath.StartsWith("/") &&
                !(buildFilePath.Length > 1 && buildFilePath[1] == ':'))
            {
                // Build file will always be absolute. So make it absolute if it isn't already.
                buildFilePath = System.IO.Path.GetFullPath(
                    System.IO.Path.Combine(
                        System.IO.Directory.GetCurrentDirectory(), buildFilePath));

            }

            if (!System.IO.File.Exists(buildFilePath))
            {
                throw new InvalidOperationException("Build file does not exist: " + originalBuildFilePath);
            }

            return buildFilePath;
        }

        private static BuildContext GetBuildContextCbx(string rawBuildFilePath)
        {
            string buildFile = GetValidatedCanonicalBuildFilePath(rawBuildFilePath);
            string projectDirectory = System.IO.Path.GetDirectoryName(buildFile);
            string buildFileContent = System.IO.File.ReadAllText(buildFile);
            return BuildContext.Parse(projectDirectory, buildFileContent, null);
        }

        private static BuildContext GetBuildContext(string[] args)
        {
            Dictionary<string, string> argLookup = Program.ParseArgs(args);

            string buildFile = argLookup.ContainsKey("buildfile") ? argLookup["buildfile"] : null;
            string target = argLookup.ContainsKey("target") ? argLookup["target"] : null;

            if (buildFile == null || target == null)
            {
                throw new InvalidOperationException("Build file and target must be specified together.");
            }

            buildFile = GetValidatedCanonicalBuildFilePath(buildFile);

            string projectDirectory = System.IO.Path.GetDirectoryName(buildFile);

            BuildContext buildContext = null;

            argLookup.Remove("buildfile");
            argLookup.Remove("target");
            projectDirectory = System.IO.Path.GetDirectoryName(buildFile);

            buildContext = BuildContext.Parse(projectDirectory, System.IO.File.ReadAllText(buildFile), target);


            buildContext = buildContext ?? new BuildContext();

            // command line arguments override build file values if present.

            if (buildContext.Platform == null)
                throw new InvalidOperationException("No platform specified in build file.");

            if (buildContext.SourceFolders.Length == 0)
                throw new InvalidOperationException("No source folder specified in build file.");

            if (buildContext.OutputFolder == null)
                throw new InvalidOperationException("No output folder specified in build file.");

            buildContext.OutputFolder = FileUtil.JoinAndCanonicalizePath(projectDirectory, buildContext.OutputFolder);
            if (buildContext.IconFilePath != null)
            {
                buildContext.IconFilePath = FileUtil.JoinAndCanonicalizePath(projectDirectory, buildContext.IconFilePath);
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

        // TODO: remove this and just use the simple extractor O(n^2) is worth getting rid of extra code when there's only a handful of args.
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
