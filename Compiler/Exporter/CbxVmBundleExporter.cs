using System;
using System.Collections.Generic;
using Wax;
using Wax.Util.Disk;

namespace Exporter
{
    public static class CbxVmBundleExporter
    {
        public static ExportResponse Run(
            Platform.AbstractPlatform platform,
            string projectDirectory,
            string outputDirectory,
            BuildData buildData)
        {
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

            return new ExportResponse();
        }
    }
}
