using Builder.Localization;
using System.Collections.Generic;
using System.Linq;
using Wax.Util.Disk;

namespace AssemblyResolver
{
    internal class AssemblyFinder
    {
        public InternalAssemblyMetadata[] AssemblyFlatList { get; private set; }
        private Dictionary<string, InternalAssemblyMetadata> libraryLookup;

        public AssemblyFinder(
            string[] nullableBuildFileLocalDepsList,
            string[] libraryDirectories,
            string nullableProjectDirectory,
            string nullableCrayonSourceRoot)
        {
            this.AssemblyFlatList = GetAvailableLibraryPathsByLibraryName(nullableBuildFileLocalDepsList, libraryDirectories, nullableProjectDirectory).ToArray();

            libraryLookup = this.AssemblyFlatList.ToDictionary(metadata => metadata.ID);
            foreach (InternalAssemblyMetadata assemblyMetadata in this.AssemblyFlatList)
            {
                foreach (Locale supportedLocale in assemblyMetadata.SupportedLocales)
                {
                    libraryLookup[supportedLocale.ID + ":" + assemblyMetadata.GetName(supportedLocale)] = assemblyMetadata;
                }
            }
        }

        public InternalAssemblyMetadata GetAssemblyMetadataFromAnyPossibleKey(string name)
        {
            InternalAssemblyMetadata assembly;
            return libraryLookup.TryGetValue(name, out assembly)
                ? assembly
                : null;
        }

        private static InternalAssemblyMetadata[] GetAvailableLibraryPathsByLibraryName(
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
                unverifiedLibraryDirectories.AddRange(FileUtil.DirectoryListDirectoryPaths(registeredLibraryPath));
            }

            List<string> verifiedLibraryPaths = new List<string>();

            foreach (string dir in unverifiedLibraryDirectories)
            {
                string manifestPath = Wax.Util.Disk.DiskUtil.JoinPathNative(dir, "manifest.json");
                if (FileUtil.FileExists(manifestPath))
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
                InternalAssemblyMetadata metadata = AssemblyMetadataFactory.CreateLibrary(path, defaultName);

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
