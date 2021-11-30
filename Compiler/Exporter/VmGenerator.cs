using Common;
using System.Collections.Generic;
using Wax;

namespace Exporter
{
    public enum VmGenerationMode
    {
        EXPORT_SELF_CONTAINED_PROJECT_SOURCE,
        EXPORT_VM_AND_LIBRARIES,
    }

    public class VmGenerator
    {
        public void GenerateVmSourceCodeForPlatform(
            Dictionary<string, FileOutput> output,
            BuildData buildData,
            Platform.AbstractPlatform platform,
            string verifiedAbsoluteOutputPath,
            VmGenerationMode mode)
        {
            if (mode == VmGenerationMode.EXPORT_SELF_CONTAINED_PROJECT_SOURCE)
            {
                ExportProperties exportProperties = buildData.ExportProperties;
                platform.GleanInformationFromPreviouslyExportedProject(exportProperties, verifiedAbsoluteOutputPath);
                platform.ExportProject(output, buildData, exportProperties);
            }
            else
            {
                platform.ExportStandaloneVm(output);
            }
        }
    }
}
