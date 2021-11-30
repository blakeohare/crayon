using System.Collections.Generic;
using Wax;

namespace Exporter
{
    public class VmGenerator
    {
        public void GenerateVmSourceCodeForPlatform(
            Dictionary<string, FileOutput> output,
            BuildData buildData,
            Platform.AbstractPlatform platform,
            string verifiedAbsoluteOutputPath)
        {
            ExportProperties exportProperties = buildData.ExportProperties;
            platform.GleanInformationFromPreviouslyExportedProject(exportProperties, verifiedAbsoluteOutputPath);
            platform.ExportProject(output, buildData, exportProperties);
        }
    }
}
