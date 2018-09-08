using Common;
using Localization;
using System.Collections.Generic;
using System.Linq;

namespace Parser
{
    public class AssemblyFinder
    {
        public AssemblyMetadata[] LibraryFlatList { get; private set; }
        private Dictionary<string, AssemblyMetadata> libraryLookup;

        public AssemblyFinder() : this(null, null) { }

        public AssemblyFinder(
            string nullableBuildFileCrayonPath,
            string nullableProjectDirectory)
        {
            this.LibraryFlatList = GetAvailableLibraryPathsByLibraryName(nullableBuildFileCrayonPath, nullableProjectDirectory);

            libraryLookup = this.LibraryFlatList.ToDictionary(metadata => metadata.ID);
            foreach (AssemblyMetadata libraryMetadata in this.LibraryFlatList)
            {
                foreach (Locale supportedLocale in libraryMetadata.SupportedLocales)
                {
                    libraryLookup[supportedLocale.ID + ":" + libraryMetadata.GetName(supportedLocale)] = libraryMetadata;
                }
            }
        }

        internal AssemblyMetadata GetLibraryMetadataFromAnyPossibleKey(string name)
        {
            AssemblyMetadata library;
            return libraryLookup.TryGetValue(name, out library)
                ? library
                : null;
        }

        private static AssemblyMetadata[] GetAvailableLibraryPathsByLibraryName(
            string nullableBuildFileCrayonPath,
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
            if (nullableBuildFileCrayonPath != null)
            {
                placesWhereLibraryDirectoriesCanExist += ";" + nullableBuildFileCrayonPath;
            }
            placesWhereLibraryDirectoriesCanExist += ";" + (System.Environment.GetEnvironmentVariable("CRAYON_PATH") ?? "");

#if OSX
            placesWhereLibraryDirectoriesCanExist = placesWhereLibraryDirectoriesCanExist.Replace(':', ';');
#endif
            string[] paths = placesWhereLibraryDirectoriesCanExist.Split(new char[] { ';' }, System.StringSplitOptions.RemoveEmptyEntries);
            List<string> unverifiedLibraryDirectories = new List<string>();
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
            // Presumably running from source. Walk up to the root directory and find the Libraries directory.
            // From there use the list of folders.
            string currentDirectory = FileUtil.GetCurrentDirectory();
            while (!string.IsNullOrEmpty(currentDirectory))
            {
                string path = FileUtil.JoinPath(currentDirectory, "Libraries");
                if (FileUtil.DirectoryExists(path) &&
                    FileUtil.FileExists(FileUtil.JoinPath(currentDirectory, "Compiler", "CrayonWindows.sln"))) // quick sanity check
                {
                    unverifiedLibraryDirectories.AddRange(FileUtil.DirectoryListDirectoryPaths(path));
                    break;
                }
                currentDirectory = FileUtil.GetParentDirectory(currentDirectory);
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
            Dictionary<string, AssemblyMetadata> uniqueLibraries = new Dictionary<string, AssemblyMetadata>();
            foreach (string path in verifiedLibraryPaths)
            {
                string defaultName = FileUtil.GetFileNameFromPath(path);
                AssemblyMetadata metadata = new AssemblyMetadata(path, defaultName);

                // TODO: don't hardcode EN
                string uniqueKey = "en:" + metadata.ID;
                uniqueLibraries[uniqueKey] = metadata;
            }

            return uniqueLibraries.Values
                .OrderBy(metadata => metadata.ID.ToLower())
                .ToArray();
        }
    }
}
