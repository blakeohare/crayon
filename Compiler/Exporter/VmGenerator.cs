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
            ExportProperties exportProperties = buildData.ExportProperties;

            if (mode == VmGenerationMode.EXPORT_SELF_CONTAINED_PROJECT_SOURCE)
            {
                Options options = new Options();
                options
                    .SetOption(ExportOptionKey.PROJECT_ID, exportProperties.ProjectID)
                    .SetOption(ExportOptionKey.DESCRIPTION, exportProperties.Description)
                    .SetOption(ExportOptionKey.VERSION, exportProperties.Version)
                    .SetOption(ExportOptionKey.EMBED_BYTE_CODE, exportProperties.GuidSeed)
                    .SetOption(ExportOptionKey.EMBED_BYTE_CODE, true)
                    .SetOption(ExportOptionKey.PROJECT_TITLE, exportProperties.ProjectTitle)
                    .SetOption(ExportOptionKey.HAS_ICON, exportProperties.IconPaths.Length > 0)
                    .SetOption(ExportOptionKey.HAS_LAUNCHSCREEN, exportProperties.LaunchScreenPath != null)
                    .SetOption(ExportOptionKey.IOS_BUNDLE_PREFIX, exportProperties.IosBundlePrefix)
                    .SetOption(ExportOptionKey.JAVA_PACKAGE, exportProperties.JavaPackage)
                    .SetOption(ExportOptionKey.JS_FILE_PREFIX, exportProperties.JsFilePrefix)
                    .SetOption(ExportOptionKey.JS_FULL_PAGE, exportProperties.JsFullPage)
                    .SetOption(ExportOptionKey.SUPPORTED_ORIENTATION, exportProperties.Orientations)
                    .SetOption(ExportOptionKey.USES_U3, buildData.UsesU3);

                if (options.GetBool(ExportOptionKey.HAS_ICON)) options.SetOption(ExportOptionKey.ICON_PATH, exportProperties.IconPaths);
                if (options.GetBool(ExportOptionKey.HAS_LAUNCHSCREEN)) options.SetOption(ExportOptionKey.LAUNCHSCREEN_PATH, exportProperties.LaunchScreenPath);

                platform.GleanInformationFromPreviouslyExportedProject(options, verifiedAbsoluteOutputPath);

                platform.ExportProject(output, buildData, options);
            }
            else
            {
                platform.ExportStandaloneVm(output);
            }
        }
    }
}
