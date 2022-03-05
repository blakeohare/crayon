using Builder.Localization;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wax.Util.Disk;

namespace AssemblyResolver
{
    internal class AssemblyFinder
    {
        private InternalAssemblyMetadata[] assemblyFlatList = null;
        private Dictionary<string, InternalAssemblyMetadata> libraryLookup = null;

        private DiskUtil diskUtil;
        private string[] nullableBuildFileLocalDepsList;
        private string[] libraryDirectories;
        private string nullableProjectDirectory;

        public AssemblyFinder(
            DiskUtil diskUtil,
            string[] nullableBuildFileLocalDepsList,
            string[] libraryDirectories,
            string nullableProjectDirectory)
        {
            this.diskUtil = diskUtil;
            this.nullableBuildFileLocalDepsList = nullableBuildFileLocalDepsList;
            this.libraryDirectories = libraryDirectories;
            this.nullableProjectDirectory = nullableProjectDirectory;
        }

        private async Task<Dictionary<string, InternalAssemblyMetadata>> GetLibraryLookup()
        {
            if (this.libraryLookup == null)
            {
                InternalAssemblyMetadata[] assemblyFlatList = await this.GetAssemblyFlatList();
                this.libraryLookup = assemblyFlatList.ToDictionary(metadata => metadata.ID);
                foreach (InternalAssemblyMetadata assemblyMetadata in this.assemblyFlatList)
                {
                    foreach (Locale supportedLocale in assemblyMetadata.SupportedLocales)
                    {
                        libraryLookup[supportedLocale.ID + ":" + assemblyMetadata.GetName(supportedLocale)] = assemblyMetadata;
                    }
                }
            }
            return this.libraryLookup;
        }

        public async Task<InternalAssemblyMetadata[]> GetAssemblyFlatList()
        {
            if (this.assemblyFlatList == null)
            {
                this.assemblyFlatList = (await GetAvailableLibraryPathsByLibraryName(diskUtil, nullableBuildFileLocalDepsList, libraryDirectories, nullableProjectDirectory));
            }
            return this.assemblyFlatList;
        }

        public async Task<InternalAssemblyMetadata> GetAssemblyMetadataFromAnyPossibleKey(string name)
        {
            InternalAssemblyMetadata assembly;
            return (await this.GetLibraryLookup()).TryGetValue(name, out assembly)
                ? assembly
                : null;
        }

        private static async Task<InternalAssemblyMetadata[]> GetAvailableLibraryPathsByLibraryName(
            DiskUtil diskUtil,
            string[] nullableBuildFileLocalDepsList,
            string[] libraryDirectories,
            string nullableProjectDirectory)
        {
            List<string> unverifiedLibraryDirectories = new List<string>();

            if (nullableBuildFileLocalDepsList != null)
            {
                foreach (string localDep in nullableBuildFileLocalDepsList)
                {
                    string localDepAbsolute = FileUtil.GetAbsolutePathFromRelativeOrAbsolutePath(nullableProjectDirectory, localDep);
                    unverifiedLibraryDirectories.Add(localDepAbsolute);
                }
            }

            foreach (string registeredLibraryPath in libraryDirectories)
            {
                unverifiedLibraryDirectories.AddRange(await FileUtil.DirectoryListDirectoryPathsAsync(registeredLibraryPath));
            }

            List<string> verifiedLibraryPaths = new List<string>();

            foreach (string dir in unverifiedLibraryDirectories)
            {
                string manifestPath = Wax.Util.Disk.DiskUtil.JoinPathNative(dir, "manifest.json");
                if (FileUtil.FileExists_DEPRECATED(manifestPath))
                {
                    verifiedLibraryPaths.Add(dir);
                }
            }

            // Library name collisions will override any previous definition.
            // For example, a custom library referenced by a build file will override a built-in library.
            // An example use case of this would be to define a custom library called "Gamepad" for mobile that puts
            // buttons in the corners of the screen, but without having to change any code to be platform-aware.
            Dictionary<string, InternalAssemblyMetadata> uniqueAssemblies = new Dictionary<string, InternalAssemblyMetadata>();
            foreach (string path in verifiedLibraryPaths.Reverse<string>())
            {
                string defaultName = Wax.Util.Disk.DiskUtil.GetFileName(path);
                InternalAssemblyMetadata metadata = await AssemblyMetadataFactory.CreateLibrary(diskUtil, path, defaultName);

                // TODO: don't hardcode EN
                string uniqueKey = "en:" + metadata.ID;
                uniqueAssemblies[uniqueKey] = metadata;
            }

            return uniqueAssemblies.Values
                .OrderBy(metadata => metadata.ID.ToLowerInvariant())
                .ToArray();
        }
    }
}
