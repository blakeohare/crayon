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
            public override string Name { get { return "TODO_Export::ExportStandaloneCbx"; } }

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
            public override string Name { get { return "TODO_Export::ExportCbxVmBundle"; } }

            public override CrayonWorkerResult DoWorkImpl(CrayonWorkerResult[] args)
            {
                ExportCommand command = (ExportCommand)args[0].Value;
                BuildContext buildContext = (BuildContext)args[1].Value;
                this.ExportVmBundle(command, buildContext);
                return new CrayonWorkerResult();
            }

            private void ExportVmBundle(ExportCommand command, BuildContext buildContext)
            {
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

        public class ExportStandaloneVmWorker : AbstractCrayonWorker
        {
            public override string Name { get { return "TODO_Export::ExportStandaloneVm"; } }

            public override CrayonWorkerResult DoWorkImpl(CrayonWorkerResult[] args)
            {
                ExportCommand command = (ExportCommand)args[0].Value;
                this.ExportStandaloneVm(command);
                return new CrayonWorkerResult();
            }

            private void ExportStandaloneVm(ExportCommand command)
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
}
