using Build;
using Common;
using Parser;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Crayon
{
    class TemporaryWorkers
    {
        public class ExportStandaloneCbxWorker : AbstractCrayonWorker
        {
            public override string Name { get { return "Crayon::ExportStandaloneCbx"; } }

            public override CrayonWorkerResult DoWorkImpl(CrayonWorkerResult[] args)
            {
                ExportCommand command = (ExportCommand)args[0].Value;
                string buildFilePath = command.BuildFilePath;
                CbxExporter exporter = new CbxExporter(buildFilePath).Export();
                return new CrayonWorkerResult()
                {
                    Value = exporter,
                };
            }
        }

        public class ExportCbxVmBundleWorker : AbstractCrayonWorker
        {
            public override string Name { get { return "Crayon::ExportCbxVmBundle"; } }

            public override CrayonWorkerResult DoWorkImpl(CrayonWorkerResult[] args)
            {
                ExportCommand command = (ExportCommand)args[0].Value;
                this.ExportVmBundle(command);
                return new CrayonWorkerResult();
            }

            private void ExportVmBundle(ExportCommand command)
            {
                using (new PerformanceSection("ExportVmBundle"))
                {
                    BuildContext buildContext = this.GetBuildContext(command);
                    Platform.AbstractPlatform platform = Program.GetPlatformInstance(buildContext);
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

            private BuildContext GetBuildContext(ExportCommand command)
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

        public class ExportStandaloneVmWorker : AbstractCrayonWorker
        {
            public override string Name { get { return "Crayon::ExportStandaloneVm"; } }

            public override CrayonWorkerResult DoWorkImpl(CrayonWorkerResult[] args)
            {
                ExportCommand command = (ExportCommand)args[0].Value;
                this.ExportStandaloneVm(command);
                return new CrayonWorkerResult();
            }

            private void ExportStandaloneVm(ExportCommand command)
            {
                using (new PerformanceSection("ExportStandaloneVm"))
                {
                    string vmPlatform = command.VmPlatform;
                    string vmTargetDirectory = command.VmExportDirectory;
                    if (vmPlatform == null || vmTargetDirectory == null)
                    {
                        throw new InvalidOperationException("-vm and -vmdir flags must both have correct values.");
                    }
                    Platform.AbstractPlatform standaloneVmPlatform = Program.platformProvider.GetPlatform(vmPlatform);
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
        }

        public class RunCbxWorker : AbstractCrayonWorker
        {
            public override string Name { get { return "Crayon::RunCbx"; } }

            public override CrayonWorkerResult DoWorkImpl(CrayonWorkerResult[] args)
            {
                ExportCommand command = (ExportCommand)args[0].Value;
                CbxExporter exporter = (CbxExporter)args[1].Value;
                string cbxFile = exporter.GetCbxPath();

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
                return new CrayonWorkerResult();
            }
        }
    }
}
