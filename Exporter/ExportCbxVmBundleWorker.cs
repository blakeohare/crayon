using Build;
using Common;
using Parser;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Exporter
{
    public class ExportCbxVmBundleWorker : AbstractCrayonWorker
    {
        public override string Name { get { return "Exporter::ExportCbxVmBundleImpl"; } }

        public override CrayonWorkerResult DoWorkImpl(CrayonWorkerResult[] args)
        {
            ExportCommand command = (ExportCommand)args[0].Value;
            BuildContext buildContext = (BuildContext)args[1].Value;
            this.ExportVmBundle(command, buildContext);
            return new CrayonWorkerResult();
        }

        private void ExportVmBundle(ExportCommand command, BuildContext buildContext)
        {
            string platformId = buildContext.Platform.ToLowerInvariant();
            Platform.AbstractPlatform platform = command.PlatformProvider.GetPlatform(platformId);
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
            Dictionary<string, FileOutput> result = new Dictionary<string, FileOutput>();
            vmGenerator.GenerateVmSourceCodeForPlatform(
                result,
                platform,
                compilationResult,
                resourceDatabase,
                libraries,
                outputDirectory,
                command.InlineImportCodeLoader,
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
}
