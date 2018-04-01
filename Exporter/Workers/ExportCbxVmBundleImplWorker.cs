using Build;
using Common;
using Parser;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Exporter.Workers
{
    public class ExportCbxVmBundleImplWorker : AbstractCrayonWorker
    {
        public override CrayonWorkerResult DoWorkImpl(CrayonWorkerResult[] args)
        {
            ExportCommand command = (ExportCommand)args[0].Value;
            BuildContext buildContext = (BuildContext)args[1].Value;
            CompilationBundle compilationResult = this.ExportVmBundle(command, buildContext);
            return new CrayonWorkerResult() { Value = compilationResult };
        }

        private CompilationBundle ExportVmBundle(ExportCommand command, BuildContext buildContext)
        {
            // TODO: Worker: platform = GetPlatform(buildContext, command)
            string platformId = buildContext.Platform.ToLowerInvariant();
            Platform.AbstractPlatform platform = command.PlatformProvider.GetPlatform(platformId);
            if (platform == null) throw new InvalidOperationException("Unrecognized platform. See usage.");

            // TODO: Worker: Compile
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

            // TODO: this needs to be the result of an earlier step after this is split into workers.
            return compilationResult;
        }
    }
}
