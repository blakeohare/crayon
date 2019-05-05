using Common;
using Localization;
using System.Collections.Generic;
using System.Linq;

namespace AssemblyResolver
{
    public class AssemblyFinder
    {
        public AssemblyMetadata[] AssemblyFlatList { get; private set; }
        private Dictionary<string, AssemblyMetadata> libraryLookup;

        public AssemblyFinder() : this(null, null, null) { }

        public AssemblyFinder(
            string[] nullableBuildFileLocalDepsList,
            string[] nullableBuildFileRemoteDepsList,
            string nullableProjectDirectory)
        {
            IList<AssemblyMetadata> localAssemblies = GetAvailableLibraryPathsByLibraryName(nullableBuildFileLocalDepsList, nullableProjectDirectory);
            IList<AssemblyMetadata> remoteAssemblies = GetRemoteAssemblies(nullableBuildFileRemoteDepsList);
            this.AssemblyFlatList = localAssemblies.Concat(remoteAssemblies).ToArray();

            libraryLookup = this.AssemblyFlatList.ToDictionary(metadata => metadata.ID);
            foreach (AssemblyMetadata assemblyMetadata in this.AssemblyFlatList)
            {
                foreach (Locale supportedLocale in assemblyMetadata.SupportedLocales)
                {
                    libraryLookup[supportedLocale.ID + ":" + assemblyMetadata.GetName(supportedLocale)] = assemblyMetadata;
                }
            }
        }

        public AssemblyMetadata GetAssemblyMetadataFromAnyPossibleKey(string name)
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

        public IList<AssemblyMetadata> GetRemoteAssemblies(string[] urlsAndVersionInfo)
        {
            // TODO: finish this function.
            return new AssemblyMetadata[0];
            /*
            Dictionary<string, AssemblyMetadata> assemblies = new Dictionary<string, AssemblyMetadata>();

            RemoteAssemblyManifest manifest = new RemoteAssemblyManifest();
            List<string[]> syncNeeded = new List<string[]>();
            List<string[]> syncWanted = new List<string[]>();
            foreach (string info in urlsAndVersionInfo)
            {
                string[] parts = info.Split(new char[] { ',' }, 2);
                string url = parts[0];
                string version = "LATEST";
                if (parts.Length == 2)
                {
                    version = parts[1];
                }

                RemoteAssemblyState currentState = manifest.GetAssemblyState(url, version);
                if (currentState == null)
                {
                    syncNeeded.Add(new string[] { url, version });
                }
                else if (version == "LATEST")
                {

                }
            }

            return assemblies.Values.ToArray();
            */
        }
    }
}
