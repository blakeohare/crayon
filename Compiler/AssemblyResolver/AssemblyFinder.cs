using Common;
using CommonUtil.Collections;
using CommonUtil.Disk;
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
            Dictionary<string, AssemblyMetadata> assemblies = new Dictionary<string, AssemblyMetadata>();
            if (urlsAndVersionInfo == null || urlsAndVersionInfo.Length == 0) return new AssemblyMetadata[0];

            RemoteAssemblyManifest manifest = new RemoteAssemblyManifest();
            List<string[]> librariesMissing = new List<string[]>();
            List<RemoteAssemblyState> librariesThatMightNeedRefresh = new List<RemoteAssemblyState>();
            bool anyLibrariesWereMissing = false;
            List<RemoteAssemblyState> usedAssemblies = new List<RemoteAssemblyState>();
            foreach (string info in urlsAndVersionInfo)
            {
                string[] parts = info.Split(new char[] { ',' }, 2);
                string url = parts[0];
                string version = null;
                if (parts.Length == 2)
                {
                    version = parts[1] == "LATEST" ? null : parts[1];
                }
                bool isLatestStableRequested = version == null;

                RemoteAssemblyState currentState = manifest.GetAssemblyState(url, version);
                if (currentState == null)
                {
                    currentState = PerformSyncForMissingAssembly(manifest, url, version);
                    if (currentState == null) throw new System.Exception(); // all code paths that return null from the above function should have thrown a proper error message exception.
                    anyLibrariesWereMissing = true;
                    usedAssemblies.Add(currentState);
                }
                else if (isLatestStableRequested)
                {
                    librariesThatMightNeedRefresh.Add(currentState);
                }
                else
                {
                    usedAssemblies.Add(currentState);
                }
            }

            int now = Util.UnixTime;
            int ageToDefinitelySync = 7 * 24 * 3600;
            int expiration = now - ageToDefinitelySync;
            bool refreshNeeded = anyLibrariesWereMissing || librariesThatMightNeedRefresh.Where(lib => lib.LastLatestCheck < expiration).FirstOrDefault() != null;
            if (refreshNeeded)
            {
                foreach (RemoteAssemblyState lib in librariesThatMightNeedRefresh)
                {
                    RemoteAssemblyState nullableNewLib = PerformSyncForOptionalRefresh(manifest, lib.Url);
                    usedAssemblies.Add(nullableNewLib ?? lib);
                }
            }
            else
            {
                usedAssemblies.AddRange(librariesThatMightNeedRefresh);
            }

            foreach (RemoteAssemblyState ras in usedAssemblies)
            {
                ras.LastUsed = now;
            }

            manifest.ReserializeFile();

            foreach (RemoteAssemblyState ras in usedAssemblies)
            {
                assemblies.Add(ras.Id, new AssemblyMetadata(ras.AbsolutePathToLibrary, ras.Id));
            }

            return assemblies.Values.ToArray();
        }

        private RemoteAssemblyState PerformSyncForMissingAssembly(RemoteAssemblyManifest manifest, string libraryUrl, string libraryVersionOrNullForLatest)
        {
            return PerformSyncImpl(manifest, libraryUrl, libraryVersionOrNullForLatest, true);
        }

        private RemoteAssemblyState PerformSyncForOptionalRefresh(RemoteAssemblyManifest manifest, string libraryUrl)
        {
            return PerformSyncImpl(manifest, libraryUrl, null, false);
        }

        private RemoteAssemblyState PerformSyncImpl(RemoteAssemblyManifest manifest, string libraryUrl, string libraryVersionOrNullForLatest, bool isRequired)
        {
            string fullStructuredUrl = libraryVersionOrNullForLatest == null
                ? libraryUrl
                : (libraryUrl + ":" + libraryVersionOrNullForLatest);
            Pair<FetchAssemblyStatus, RemoteAssemblyState> t = new RemoteAssemblyFetcher().FetchNewAssembly(manifest, fullStructuredUrl);

            switch (t.First)
            {
                case FetchAssemblyStatus.NO_CONNECTION:
                case FetchAssemblyStatus.SERVER_NOT_RESPONDING:
                case FetchAssemblyStatus.UNKNOWN_RESPONSE:
                    if (isRequired)
                    {
                        throw new System.InvalidOperationException("Could not sync required dependency: " + libraryUrl);
                    }
                    return null;

                case FetchAssemblyStatus.INVALID_URL:
                    throw new System.InvalidOperationException("Invalid remote library URL: " + libraryUrl);

                case FetchAssemblyStatus.LIBRARY_NOT_FOUND:
                    throw new System.InvalidOperationException("Remote Library not found: " + libraryUrl);

                case FetchAssemblyStatus.VERSION_NOT_FOUND:
                    if (isRequired)
                    {
                        throw new System.InvalidOperationException("Remote Library version not found: " + libraryUrl);
                    }
                    else
                    {
                        // This could happen if the library was taken down by the provider
                    }
                    return null;

                case FetchAssemblyStatus.SUCCESS:
                    manifest.AddOrReplaceAssemblyState(t.Second);
                    return t.Second;

                default:
                    throw new System.NotImplementedException(); // this should not happen.
            }
        }
    }
}
