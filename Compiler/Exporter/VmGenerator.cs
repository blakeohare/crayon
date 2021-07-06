using Common;
using System.Collections.Generic;

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
            string byteCode,
            Platform.AbstractPlatform platform,
            ExportRequest nullableExportBundle,
            Build.ResourceDatabase resourceDatabase,
            bool usesU3,
            string verifiedAbsoluteOutputPath,
            VmGenerationMode mode)
        {
            using (new PerformanceSection("VmGenerator.GenerateVmSourceCodeForPlatform"))
            {
                bool isStandaloneVm = mode == VmGenerationMode.EXPORT_VM_AND_LIBRARIES;
                Dictionary<string, object> constantFlags = platform.GetFlattenedConstantFlags(isStandaloneVm) ?? new Dictionary<string, object>();

                if (mode == VmGenerationMode.EXPORT_SELF_CONTAINED_PROJECT_SOURCE)
                {
                    Options options = new Options();
                    options
                        .SetOption(ExportOptionKey.PROJECT_ID, nullableExportBundle.ProjectID)
                        .SetOption(ExportOptionKey.DESCRIPTION, nullableExportBundle.Description)
                        .SetOption(ExportOptionKey.VERSION, nullableExportBundle.Version)
                        .SetOption(ExportOptionKey.EMBED_BYTE_CODE, nullableExportBundle.GuidSeed)
                        .SetOption(ExportOptionKey.EMBED_BYTE_CODE, true)
                        .SetOption(ExportOptionKey.PROJECT_TITLE, nullableExportBundle.ProjectTitle)
                        .SetOption(ExportOptionKey.HAS_ICON, nullableExportBundle.IconPaths.Length > 0)
                        .SetOption(ExportOptionKey.HAS_LAUNCHSCREEN, nullableExportBundle.LaunchScreenPath != null)
                        .SetOption(ExportOptionKey.IOS_BUNDLE_PREFIX, nullableExportBundle.IosBundlePrefix)
                        .SetOption(ExportOptionKey.JAVA_PACKAGE, nullableExportBundle.JavaPackage)
                        .SetOption(ExportOptionKey.JS_FILE_PREFIX, nullableExportBundle.JsFilePrefix)
                        .SetOption(ExportOptionKey.JS_FULL_PAGE, nullableExportBundle.JsFullPage)
                        .SetOption(ExportOptionKey.SUPPORTED_ORIENTATION, nullableExportBundle.Orientations)
                        .SetOption(ExportOptionKey.USES_U3, usesU3);

                    if (options.GetBool(ExportOptionKey.HAS_ICON)) options.SetOption(ExportOptionKey.ICON_PATH, nullableExportBundle.IconPaths);
                    if (options.GetBool(ExportOptionKey.HAS_LAUNCHSCREEN)) options.SetOption(ExportOptionKey.LAUNCHSCREEN_PATH, nullableExportBundle.LaunchScreenPath);

                    platform.GleanInformationFromPreviouslyExportedProject(options, verifiedAbsoluteOutputPath);

                    platform.ExportProject(
                        output,
                        byteCode,
                        resourceDatabase,
                        options);
                }
                else
                {
                    platform.ExportStandaloneVm(output);
                }
            }
        }

        private Platform.LibraryForExport CreateLibraryForExport(
            string libraryName,
            string libraryVersion,
            string directory)
        {
            using (new PerformanceSection("VmGenerator.CreateLibraryForExport"))
            {
                return new Platform.LibraryForExport(directory)
                {
                    Name = libraryName,
                    Version = libraryVersion,
                };
            }
        }
    }
}
