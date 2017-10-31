using Common;
using Localization;
using System.Collections.Generic;
using System.Linq;

namespace Parser
{
    public class LibraryFinder
    {
        public LibraryMetadata[] LibraryFlatList { get; private set; }
        private Dictionary<string, LibraryMetadata> libraryLookup;

        public LibraryFinder() : this(null, null) { }

        public LibraryFinder(string nullableBuildFileCrayonPath,
            string nullableProjectDirectory)
        {
            this.LibraryFlatList = GetAvailableLibraryPathsByLibraryName(nullableBuildFileCrayonPath, nullableProjectDirectory);

            libraryLookup = this.LibraryFlatList.ToDictionary(metadata => metadata.ID);
            foreach (LibraryMetadata libraryMetadata in this.LibraryFlatList)
            {
                foreach (Locale supportedLocale in libraryMetadata.SupportedLocales)
                {
                    libraryLookup[supportedLocale.ID + ":" + libraryMetadata.GetName(supportedLocale)] = libraryMetadata;
                }
            }
        }

        internal LibraryMetadata GetLibraryMetadataFromAnyPossibleKey(string name)
        {
            LibraryMetadata library;
            return libraryLookup.TryGetValue(name, out library)
                ? library
                : null;
        }

        private static LibraryMetadata[] GetAvailableLibraryPathsByLibraryName(
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
                placesWhereLibraryDirectoriesCanExist += ";" + System.IO.Path.Combine(crayonHome, "libs");
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
                string absolutePath = FileUtil.IsAbsolutePath(path)
                    ? path
                    : System.IO.Path.Combine(nullableProjectDirectory, path);
                absolutePath = System.IO.Path.GetFullPath(absolutePath);
                if (System.IO.Directory.Exists(absolutePath))
                {
                    unverifiedLibraryDirectories.AddRange(System.IO.Directory.GetDirectories(absolutePath));
                }
            }

#if DEBUG
            // Presumably running from source. Walk up to the root directory and find the Libraries directory.
            // From there use the list of folders.
            string currentDirectory = System.IO.Path.GetFullPath(".");
            while (!string.IsNullOrEmpty(currentDirectory))
            {
                string path = System.IO.Path.Combine(currentDirectory, "Libraries");
                if (System.IO.Directory.Exists(path) &&
                    System.IO.File.Exists(System.IO.Path.Combine(currentDirectory, "Compiler", "CrayonWindows.sln"))) // quick sanity check
                {
                    unverifiedLibraryDirectories.AddRange(System.IO.Directory.GetDirectories(path));
                    break;
                }
                currentDirectory = System.IO.Path.GetDirectoryName(currentDirectory);
            }
#endif
            List<string> verifiedLibraryPaths = new List<string>();

            foreach (string dir in unverifiedLibraryDirectories)
            {
                string manifestPath = System.IO.Path.Combine(dir, "manifest.json");
                if (System.IO.File.Exists(manifestPath))
                {
                    verifiedLibraryPaths.Add(dir);
                }
            }

            // Library name collisions will override any previous definition.
            // For example, a custom library referenced by a build file will override a built-in library.
            // An example use case of this would be to define a custom library called "Gamepad" for mobile that puts
            // buttons in the corners of the screen, but without having to change any code to be platform-aware.
            Dictionary<string, LibraryMetadata> uniqueLibraries = new Dictionary<string, LibraryMetadata>();
            foreach (string path in verifiedLibraryPaths)
            {
                string defaultName = System.IO.Path.GetFileName(path);
                LibraryMetadata metadata = new LibraryMetadata(path, defaultName);

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
