using AssemblyResolver;
using Build;
using Common;
using CommonUtil.Disk;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Exporter.Workers
{
    public static class ExportCbxVmBundleImplWorker
    {
        public static ExportBundle ExportVmBundle(
            string byteCode,
            IList<AssemblyMetadata> assemblies,
            ExportCommand command,
            BuildContext buildContext)
        {
            // TODO: Worker: platform = GetPlatform(buildContext, command)
            string platformId = buildContext.Platform.ToLowerInvariant();
            Platform.AbstractPlatform platform = command.PlatformProvider.GetPlatform(platformId);
            if (platform == null) throw new InvalidOperationException("Unrecognized platform. See usage.");

            ExportBundle exportBundle = ExportBundle.Compile(byteCode, assemblies, buildContext);
            AssemblyMetadata[] libraries = exportBundle.LibraryAssemblies.ToArray();

            ResourceDatabase resourceDatabase = ResourceDatabaseBuilder.PrepareResources(buildContext, exportBundle.ByteCode);

            string outputDirectory = command.HasOutputDirectoryOverride
                ? command.OutputDirectoryOverride
                : buildContext.OutputFolder;
            if (!Path.IsAbsolute(outputDirectory))
            {
                outputDirectory = FileUtil.JoinPath(buildContext.ProjectDirectory, outputDirectory);
            }
            outputDirectory = FileUtil.GetCanonicalizeUniversalPath(outputDirectory);
            FileOutputExporter exporter = new FileOutputExporter(outputDirectory);

            VmGenerator vmGenerator = new VmGenerator();
            Dictionary<string, FileOutput> result = new Dictionary<string, FileOutput>();
            vmGenerator.GenerateVmSourceCodeForPlatform(
                result,
                byteCode,
                platform,
                exportBundle,
                resourceDatabase,
                libraries,
                outputDirectory,
                VmGenerationMode.EXPORT_SELF_CONTAINED_PROJECT_SOURCE);

            exporter.ExportFiles(result);

            // TODO: this needs to be the result of an earlier step after this is split into workers.
            return exportBundle;
        }
    }
}
