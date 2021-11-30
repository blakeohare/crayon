using CommonUtil.Disk;
using System;
using System.Collections.Generic;
using Wax;

namespace Exporter
{
    public static class CbxVmBundleExporter
    {
        public static ExportResponse Run(
            string platformId,
            string projectDirectory,
            string outputDirectory,
            BuildData buildData,
            Platform.IPlatformProvider platformProvider)
        {
            RunImpl(platformId, projectDirectory, outputDirectory, buildData, platformProvider);
            return new ExportResponse();
        }

        public static void RunImpl(
            string platformId,
            string projectDirectory,
            string outputDirectory,
            BuildData buildData,
            Platform.IPlatformProvider platformProvider)
        {
            Platform.AbstractPlatform platform = platformProvider.GetPlatform(platformId);
            if (platform == null) throw new InvalidOperationException("Unrecognized platform. See usage.");

            if (!Path.IsAbsolute(outputDirectory))
            {
                outputDirectory = FileUtil.JoinPath(projectDirectory, outputDirectory);
            }
            outputDirectory = FileUtil.GetCanonicalizeUniversalPath(outputDirectory);
            FileOutputExporter exporter = new FileOutputExporter(outputDirectory);

            Dictionary<string, FileOutput> result = new Dictionary<string, FileOutput>();

            ExportProperties exportProperties = buildData.ExportProperties;
            platform.GleanInformationFromPreviouslyExportedProject(exportProperties, outputDirectory);
            platform.ExportProject(result, buildData, exportProperties);

            exporter.ExportFiles(result);
        }
    }
}
