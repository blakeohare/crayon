using AssemblyResolver;
using Common;
using CommonUtil.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Exporter
{
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
            Platform.AbstractPlatform platform,
            ICollection<AssemblyMetadata> relevantLibraries)
        {
            using (new PerformanceSection("VmGenerator.GenerateVmSourceCodeForPlatform"))
            {
                Dictionary<string, object> constantFlags = platform.GetFlattenedConstantFlags() ?? new Dictionary<string, object>();

                Dictionary<string, AssemblyMetadata> librariesByID = relevantLibraries.ToDictionary(lib => lib.ID);
                List<Platform.LibraryForExport> libraries = this.GetLibrariesForExportPastelFree(platform, librariesByID);

                platform.ExportStandaloneVm(
                    output,
                    libraries);
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
