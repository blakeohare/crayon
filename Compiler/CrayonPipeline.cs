using Build;
using Common;
using Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using Common;

namespace Crayon
{
    /*
        This code is kind of peculiar, but this is an effort to start unravelling highly
        coupled code. Eventually every major component of the parser/compiler/exporter/everything
        will exist as standalone worker objects.

        While the pipeline runner is hardcoded right now, eventually there'll be a more graph-like
        interface to clearly show the flow of the process.

        Because each worker is standalone and puts all its output into one data structure, this
        will be much more testable than things are now (and maybe I'll even be able to write tests).
        Additionally, certain types of work can be parallelized and a thread pool can interpret the
        graph.

        Also, eventually I'd like to move off of C# and into C, C++, or even Crayon itself. This
        structure will lend itself better to partial migrations where part of the codebase can
        be migrated and then communicate via the worker graph across processes. (contingent on
        all inputs and outputs being serializable)

        I apologize for how weird this looks right now.
    */
    internal class CrayonPipeline
    {
        private Dictionary<string, AbstractCrayonWorker> workers = new Dictionary<string, AbstractCrayonWorker>();

        private CrayonPipeline RegisterWorker(AbstractCrayonWorker worker)
        {
            this.workers[worker.Name] = worker;
            return this;
        }

        private object DoWork(string id, object arg)
        {
            // TODO: parallelization if arg is of certain type. Something like
            // ParallelizableWorkUnit which will have an object[] Items property
            // that can be arbitrarily split up.
            return this.workers[id].DoWork(arg);
        }

        public void Run(string[] args)
        {
            this.RegisterWorker(new TopLevelCheckWorker());
            this.RegisterWorker(new UsageDisplayWorker());
            this.RegisterWorker(new GenerateDefaultProjectWorker());
            this.RegisterWorker(new ExportStandaloneCbxWorker());
            this.RegisterWorker(new ExportCbxVmBundleWorker());
            this.RegisterWorker(new ExportStandaloneVmWorker());
            this.RegisterWorker(new RunCbxWorker());

            // TODO: maybe just remove this from the graph, since each of these results represent
            // a very specific and established pipeline.
            ExportCommand command = (ExportCommand)this.DoWork("Crayon.TopLevelCheck", args);
            ExecutionType action = command.IdentifyUseCase();
            switch (action)
            {
                case ExecutionType.SHOW_USAGE:
                    this.DoWork("Crayon.DisplayUsage", null);
                    break;

                case ExecutionType.GENERATE_DEFAULT_PROJECT:
                    this.DoWork("Crayon.GenerateDefaultProject", command);
                    break;

                case ExecutionType.EXPORT_VM_BUNDLE:
                    this.DoWork("Crayon.ExportCbxVmBundle", command);
                    break;

                case ExecutionType.EXPORT_VM_STANDALONE:
                    this.DoWork("Crayon.ExportStandaloneVm", command);
                    break;

                case ExecutionType.EXPORT_CBX:
                case ExecutionType.RUN_CBX:
                    CbxExporter cbxExporter = (CbxExporter)this.DoWork("Crayon.ExportStandaloneCbx", command.BuildFilePath);
                    if (action == ExecutionType.RUN_CBX)
                    {
                        string cbxPath = cbxExporter.GetCbxPath();
                        this.DoWork("Crayon.RunCbx", new object[] { command, cbxPath });
                    }
                    break;

                default: throw new Exception();
            }
        }

        public class GenerateDefaultProjectWorker : AbstractCrayonWorker
        {
            public override string Name { get { return "Crayon.GenerateDefaultProject"; } }

            public override object DoWork(object arg)
            {
                ExportCommand command = (ExportCommand)arg;
                DefaultProjectGenerator generator = new DefaultProjectGenerator(command.DefaultProjectId, command.DefaultProjectLocale);
                Dictionary<string, FileOutput> project = generator.Validate().Export();

                string directory = FileUtil.JoinPath(
                    FileUtil.GetCurrentDirectory(),
                    generator.ProjectID);
                new FileOutputExporter(directory).ExportFiles(project);

                Console.WriteLine("Empty project exported to directory '" + generator.ProjectID + "/'");
                return null;
            }
        }

        public class ExportStandaloneCbxWorker : AbstractCrayonWorker
        {
            public override string Name { get { return "Crayon.ExportStandaloneCbx"; } }

            public override object DoWork(object arg)
            {
                string buildFilePath = (string)arg;
                return new CbxExporter(buildFilePath).Export();
            }
        }

        public class ExportCbxVmBundleWorker : AbstractCrayonWorker
        {
            public override string Name { get { return "Crayon.ExportCbxVmBundle"; } }

            public override object DoWork(object arg)
            {
                ExportCommand command = (ExportCommand)arg;
                this.ExportVmBundle(command);
                return null;
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
            public override string Name { get { return "Crayon.ExportStandaloneVm"; } }

            public override object DoWork(object arg)
            {
                ExportCommand command = (ExportCommand)arg;
                this.ExportStandaloneVm(command);
                return null;
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
            public override string Name { get { return "Crayon.RunCbx"; } }

            public override object DoWork(object arg)
            {
                object[] compositeArgs = (object[])arg;
                this.DoWorkImpl((ExportCommand)compositeArgs[0], (string)compositeArgs[1]);
                return null;
            }

            public void DoWorkImpl(ExportCommand command, string cbxFile)
            {
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
            }
        }
    }
}
