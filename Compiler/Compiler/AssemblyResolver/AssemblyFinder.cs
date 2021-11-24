using Common;
using CommonUtil;
using CommonUtil.Disk;
using Common.Localization;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace AssemblyResolver
{
    internal class AssemblyFinder
    {
        public InternalAssemblyMetadata[] AssemblyFlatList { get; private set; }
        private Dictionary<string, InternalAssemblyMetadata> libraryLookup;

        public AssemblyFinder() : this(null, null) { }

        public AssemblyFinder(
            string[] nullableBuildFileLocalDepsList,
            string nullableProjectDirectory)
        {
            this.AssemblyFlatList = GetAvailableLibraryPathsByLibraryName(nullableBuildFileLocalDepsList, nullableProjectDirectory).ToArray();

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
            string nullableProjectDirectory)
        {
            string crayonHome = CommonUtil.Environment.EnvironmentVariables.Get("CRAYON_HOME");

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
            placesWhereLibraryDirectoriesCanExist += ";" + (CommonUtil.Environment.EnvironmentVariables.Get("CRAYON_PATH") ?? "");

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) {
                placesWhereLibraryDirectoriesCanExist = placesWhereLibraryDirectoriesCanExist.Replace(':', ';');
            }

            List<string> unverifiedLibraryDirectories = new List<string>();

            if (nullableBuildFileLocalDepsList != null)
            {
                foreach (string localDep in nullableBuildFileLocalDepsList)
                {
                    string localDepAbsolute = FileUtil.GetAbsolutePathFromRelativeOrAbsolutePath(nullableProjectDirectory, localDep);
                    unverifiedLibraryDirectories.Add(localDepAbsolute);
                }
            }

            string[] paths = StringUtil.SplitRemoveEmpty(placesWhereLibraryDirectoriesCanExist, ";");
            foreach (string path in paths)
            {
                // TODO: figure out why this says nullable yet is being used directly.
                string absolutePath = FileUtil.GetAbsolutePathFromRelativeOrAbsolutePath(nullableProjectDirectory, path);
                if (FileUtil.DirectoryExists(absolutePath))
                {
                    unverifiedLibraryDirectories.AddRange(FileUtil.DirectoryListDirectoryPaths(absolutePath));
                }
            }

            string runningFromSourceDirectory = SourceDirectoryFinder.CrayonSourceDirectory;
            if (runningFromSourceDirectory != null) // returns null on release builds.
            {
                string libraryPath = FileUtil.JoinPath(runningFromSourceDirectory, "Libraries");
                unverifiedLibraryDirectories.AddRange(FileUtil.DirectoryListDirectoryPaths(libraryPath));
            }

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
            Dictionary<string, InternalAssemblyMetadata> uniqueAssemblies = new Dictionary<string, InternalAssemblyMetadata>();
            foreach (string path in verifiedLibraryPaths)
            {
                string defaultName = Path.GetFileName(path);
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
