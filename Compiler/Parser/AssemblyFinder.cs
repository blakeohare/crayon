using Common;
using Localization;
using System.Collections.Generic;
using System.Linq;

namespace Parser
{
    public class AssemblyFinder
    {
        public AssemblyMetadata[] AssemblyFlatList { get; private set; }
        private Dictionary<string, AssemblyMetadata> libraryLookup;

        public AssemblyFinder() : this(null, null) { }

        public AssemblyFinder(
            string[] nullableBuildFileLocalDepsList,
            string nullableProjectDirectory)
        {
            this.AssemblyFlatList = GetAvailableLibraryPathsByLibraryName(nullableBuildFileLocalDepsList, nullableProjectDirectory);

            libraryLookup = this.AssemblyFlatList.ToDictionary(metadata => metadata.ID);
            foreach (AssemblyMetadata assemblyMetadata in this.AssemblyFlatList)
            {
                foreach (Locale supportedLocale in assemblyMetadata.SupportedLocales)
                {
                    libraryLookup[supportedLocale.ID + ":" + assemblyMetadata.GetName(supportedLocale)] = assemblyMetadata;
                }
            }
        }

        internal AssemblyMetadata GetAssemblyMetadataFromAnyPossibleKey(string name)
        {
            AssemblyMetadata assembly;
            return libraryLookup.TryGetValue(name, out assembly)
                ? assembly
                : null;
        }

        private static AssemblyMetadata[] GetAvailableLibraryPathsByLibraryName(
            string[] nullableBuildFileLocalDepsList,
            string nullableProjectDirectory)
        {
            string crayonHome = System.Environment.GetEnvironmentVariable("CRAYON_HOME");

#if RELEASE
            if (crayonHome == null)
            {
                throw new System.InvalidOperationException("Please set the CRAYON_HOME environment variable to the location of the directory containing both 'crayon.exe' and the 'lib' directory.");
            }
#endif

            string placesWhereLibraryDirectoriesCanExist = "";

            if (crayonHome != null)
            {
                placesWhereLibraryDirectoriesCanExist += ";" + FileUtil.JoinPath(crayonHome, "libs");
            }
            placesWhereLibraryDirectoriesCanExist += ";" + (System.Environment.GetEnvironmentVariable("CRAYON_PATH") ?? "");

#if OSX
            placesWhereLibraryDirectoriesCanExist = placesWhereLibraryDirectoriesCanExist.Replace(':', ';');
#endif
            List<string> unverifiedLibraryDirectories = new List<string>();

            if (nullableBuildFileLocalDepsList != null)
            {
                foreach (string localDep in nullableBuildFileLocalDepsList)
                {
                    string localDepAbsolute = FileUtil.GetAbsolutePathFromRelativeOrAbsolutePath(nullableProjectDirectory, localDep);
                    unverifiedLibraryDirectories.Add(localDepAbsolute);
                }
            }

            string[] paths = placesWhereLibraryDirectoriesCanExist.Split(new char[] { ';' }, System.StringSplitOptions.RemoveEmptyEntries);
            foreach (string path in paths)
            {
                // TODO: figure out why this says nullable yet is being used directly.
                string absolutePath = FileUtil.GetAbsolutePathFromRelativeOrAbsolutePath(nullableProjectDirectory, path);
                if (FileUtil.DirectoryExists(absolutePath))
                {
                    unverifiedLibraryDirectories.AddRange(FileUtil.DirectoryListDirectoryPaths(absolutePath));
                }
            }

#if DEBUG
            string runningFromSourceDirectory = SourceDirectoryFinder.CrayonSourceDirectory;
            if (runningFromSourceDirectory != null)
            {
                string libraryPath = FileUtil.JoinPath(runningFromSourceDirectory, "Libraries");
                unverifiedLibraryDirectories.AddRange(FileUtil.DirectoryListDirectoryPaths(libraryPath));
            }
#endif

            List<string> verifiedLibraryPaths = new List<string>();

            foreach (string dir in unverifiedLibraryDirectories)
            {
                string manifestPath = FileUtil.JoinPath(dir, "manifest.json");
                if (FileUtil.FileExists(manifestPath))
                {
                    verifiedLibraryPaths.Add(dir);
                }
            }

            // Library name collisions will override any previous definition.
            // For example, a custom library referenced by a build file will override a built-in library.
            // An example use case of this would be to define a custom library called "Gamepad" for mobile that puts
            // buttons in the corners of the screen, but without having to change any code to be platform-aware.
            Dictionary<string, AssemblyMetadata> uniqueAssemblies = new Dictionary<string, AssemblyMetadata>();
            foreach (string path in verifiedLibraryPaths)
            {
                string defaultName = FileUtil.GetFileNameFromPath(path);
                AssemblyMetadata metadata = new AssemblyMetadata(path, defaultName);

                // TODO: don't hardcode EN
                string uniqueKey = "en:" + metadata.ID;
                uniqueAssemblies[uniqueKey] = metadata;
            }

            return uniqueAssemblies.Values
                .OrderBy(metadata => metadata.ID.ToLower())
                .ToArray();
        }
    }
}
