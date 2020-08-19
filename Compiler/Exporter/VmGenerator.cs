using AssemblyResolver;
using Common;
using CommonUtil.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Exporter
{
    public enum VmGenerationMode
    {
        EXPORT_SELF_CONTAINED_PROJECT_SOURCE,
        EXPORT_VM_AND_LIBRARIES,
    }

    public class VmGenerator
    {
        private List<Platform.LibraryForExport> GetLibrariesForExportPastelFree(
            Platform.AbstractPlatform platform,
            Dictionary<string, AssemblyMetadata> librariesById)
        {
            List<Platform.LibraryForExport> output = new List<Platform.LibraryForExport>();
            foreach (string libraryId in librariesById.Keys.OrderBy(k => k))
            {
                AssemblyMetadata libraryMetadata = librariesById[libraryId];
                LibraryExporter library = LibraryExporter.Get(libraryMetadata, platform);
                Platform.LibraryForExport libraryForExport = this.CreateLibraryForExport(
                    libraryMetadata.ID,
                    libraryMetadata.Version,
                    libraryMetadata.Directory,
                    library.Resources);
                output.Add(libraryForExport);
            }
            return output;
        }

        public void GenerateVmSourceCodeForPlatform(
            Dictionary<string, FileOutput> output,
            string byteCode,
            Platform.AbstractPlatform platform,
            ExportRequest nullableExportBundle,
            Build.ResourceDatabase resourceDatabase,
            ICollection<AssemblyMetadata> relevantLibraries,
            string verifiedAbsoluteOutputPath,
            VmGenerationMode mode)
        {
            using (new PerformanceSection("VmGenerator.GenerateVmSourceCodeForPlatform"))
            {
                bool isStandaloneVm = mode == VmGenerationMode.EXPORT_VM_AND_LIBRARIES;
                Dictionary<string, object> constantFlags = platform.GetFlattenedConstantFlags(isStandaloneVm) ?? new Dictionary<string, object>();

                Dictionary<string, AssemblyMetadata> librariesByID = relevantLibraries.ToDictionary(lib => lib.ID);
                List<Platform.LibraryForExport> libraries = this.GetLibrariesForExportPastelFree(platform, librariesByID);

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
                        .SetOption(ExportOptionKey.LIBRARIES_USED, libraries.Cast<object>().ToArray())
                        .SetOption(ExportOptionKey.HAS_ICON, nullableExportBundle.IconPaths.Length > 0)
                        .SetOption(ExportOptionKey.HAS_LAUNCHSCREEN, nullableExportBundle.LaunchScreenPath != null)
                        .SetOption(ExportOptionKey.IOS_BUNDLE_PREFIX, nullableExportBundle.IosBundlePrefix)
                        .SetOption(ExportOptionKey.JAVA_PACKAGE, nullableExportBundle.JavaPackage)
                        .SetOption(ExportOptionKey.ORGANIZATION_NAME, nullableExportBundle.OrganizationName)
                        .SetOption(ExportOptionKey.JS_FILE_PREFIX, nullableExportBundle.JsFilePrefix)
                        .SetOption(ExportOptionKey.JS_FULL_PAGE, nullableExportBundle.JsFullPage)
                        .SetOption(ExportOptionKey.SUPPORTED_ORIENTATION, nullableExportBundle.Orientations);

                    if (options.GetBool(ExportOptionKey.HAS_ICON)) options.SetOption(ExportOptionKey.ICON_PATH, nullableExportBundle.IconPaths);
                    if (options.GetBool(ExportOptionKey.HAS_LAUNCHSCREEN)) options.SetOption(ExportOptionKey.LAUNCHSCREEN_PATH, nullableExportBundle.LaunchScreenPath);

                    platform.GleanInformationFromPreviouslyExportedProject(options, verifiedAbsoluteOutputPath);

                    platform.ExportProject(
                        output,
                        byteCode,
                        libraries,
                        resourceDatabase,
                        options);
                }
                else
                {
                    platform.ExportStandaloneVm(
                        output,
                        libraries);
                }
            }
        }

        private Platform.LibraryForExport CreateLibraryForExport(
            string libraryName,
            string libraryVersion,
            string directory,
            LibraryResourceDatabase libResDb)
        {
            using (new PerformanceSection("VmGenerator.CreateLibraryForExport"))
            {
                Multimap<string, Platform.ExportEntity> exportEntities = libResDb.ExportEntities;

                return new Platform.LibraryForExport(directory)
                {
                    Name = libraryName,
                    Version = libraryVersion,
                    HasNativeCode = libResDb.HasNativeCode,
                    ExportEntities = exportEntities,
                };
            }
        }
    }
}
