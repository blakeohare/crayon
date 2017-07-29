using System.Collections.Generic;
using Crayon.ParseTree;
using Common;

namespace Crayon
{
    internal class LibraryManager
    {
        private Dictionary<string, Library> importedLibraries = new Dictionary<string, Library>();
        private Dictionary<string, Library> librariesByKey = new Dictionary<string, Library>();

        private Dictionary<string, string> functionNameToLibraryName = new Dictionary<string, string>();

        private Dictionary<string, int> libFunctionIds = new Dictionary<string, int>();
        private List<string> orderedListOfFunctionNames = new List<string>();

        public Platform.IPlatformProvider PlatformProvider { get; private set; }

        public LibraryManager(Platform.IPlatformProvider platformProvider)
        {
            this.PlatformProvider = platformProvider;
        }

        public static bool IsValidLibrary(string name)
        {
            return systemLibraryPathsByName.ContainsKey(name);
        }

        public Library GetLibraryFromName(string name)
        {
            Library library = this.GetLibraryFromKey(name.ToLower());
            if (library == null) return null;
            return name == library.Name ? library : null;
        }

        public Library GetLibraryFromKey(string key)
        {
            Library output;
            return this.librariesByKey.TryGetValue(key, out output) ? output : null;
        }

        public int GetIdForFunction(string name, string library)
        {
            if (this.libFunctionIds.ContainsKey(name))
            {
                return this.libFunctionIds[name];
            }

            this.functionNameToLibraryName[name] = library;
            this.orderedListOfFunctionNames.Add(name);
            int id = this.orderedListOfFunctionNames.Count;
            this.libFunctionIds[name] = id;
            return id;
        }

        public Dictionary<string, string> GetEmbeddedCode(string libraryName)
        {
            return this.importedLibraries[libraryName].GetEmbeddedCode();
        }

        public Dictionary<string, string> GetSupplementalTranslationFiles(bool isPastel)
        {
            Dictionary<string, string> output = new Dictionary<string, string>();
            foreach (Library library in this.importedLibraries.Values)
            {
                Dictionary<string, string> files = library.GetSupplementalTranslatedCode(isPastel);
                foreach (string key in files.Keys)
                {
                    output[key] = files[key];
                }
            }
            return output;
        }

        private static Dictionary<string, string> systemLibraryPathsByName = null;

        public List<Library> GetAllAvailableLibraries(Platform.AbstractPlatform platform)
        {
            Dictionary<string, string> allLibraries = GetAvailableLibraryPathsByLibraryName(null, null);
            List<Library> output = new List<Library>();
            foreach (string name in allLibraries.Keys)
            {
                string manifestPath = allLibraries[name];
                Library library = new Library(name, manifestPath, platform);
                output.Add(library);
            }
            return output;
        }

        private static Dictionary<string, string> GetAvailableLibraryPathsByLibraryName(
            string nullableBuildFileCrayonPath,
            string nullableProjectDirectory)
        {
            systemLibraryPathsByName = new Dictionary<string, string>();

            string crayonHome = System.Environment.GetEnvironmentVariable("CRAYON_HOME");

#if RELEASE
                if (crayonHome == null)
                {
                    throw new System.InvalidOperationException("Please set the CRAYON_HOME environment variable to the location of the directory containing both 'crayon.exe' and the 'lib' directory.");
                }
#endif

            List<string> directoriesToCheck = new List<string>();

            if (crayonHome != null)
            {
                string crayonHomeLibraries = System.IO.Path.Combine(crayonHome, "libs");
                if (System.IO.Directory.Exists(crayonHomeLibraries))
                {
                    directoriesToCheck.AddRange(System.IO.Directory.GetDirectories(crayonHomeLibraries));
                }
            }
            string crayonPaths =
                (nullableBuildFileCrayonPath ?? "") + ";" +
                (System.Environment.GetEnvironmentVariable("CRAYON_PATH") ?? "");

#if OSX
                crayonPaths = crayonPaths.Replace(':', ';');
#endif
            string[] paths = crayonPaths.Split(';');
            foreach (string path in paths)
            {
                if (path.Length > 0)
                {
                    string absolutePath = FileUtil.IsAbsolutePath(path)
                        ? path
                        : System.IO.Path.Combine(nullableProjectDirectory, path);
                    absolutePath = System.IO.Path.GetFullPath(absolutePath);
                    if (System.IO.Directory.Exists(absolutePath))
                    {
                        directoriesToCheck.AddRange(System.IO.Directory.GetDirectories(absolutePath));
                    }
                }
            }

#if DEBUG
            // Presumably running from source. Walk up to the root directory and find the Libraries directory.
            // From there use the list of folders.
            string currentDirectory = System.IO.Path.GetFullPath(".");
            while (!string.IsNullOrEmpty(currentDirectory))
            {
                string path = System.IO.Path.Combine(currentDirectory, "Libraries");
                if (System.IO.Directory.Exists(path))
                {
                    directoriesToCheck.AddRange(System.IO.Directory.GetDirectories(path));
                    break;
                }
                currentDirectory = System.IO.Path.GetDirectoryName(currentDirectory);
            }
#endif
            foreach (string dir in directoriesToCheck)
            {
                string libraryName = System.IO.Path.GetFileName(dir);
                string manifestPath = System.IO.Path.Combine(dir, "manifest.txt");
                if (System.IO.File.Exists(manifestPath))
                {
                    systemLibraryPathsByName[libraryName] = manifestPath;
                }
            }

            return systemLibraryPathsByName;
        }

        private string GetSystemLibraryPath(string name, string buildFileCrayonPath, string projectDirectory)
        {
            if (systemLibraryPathsByName == null)
            {
                systemLibraryPathsByName = GetAvailableLibraryPathsByLibraryName(buildFileCrayonPath, projectDirectory);
            }

            string fullpath;
            return systemLibraryPathsByName.TryGetValue(name, out fullpath)
                ? fullpath
                : null;
        }

        private readonly List<Library> librariesAlreadyImported = new List<Library>();
        // The index + 1 is the reference ID
        private readonly Dictionary<string, int> librariesAlreadyImportedIndexByName = new Dictionary<string, int>();
        private static readonly Executable[] EMPTY_EXECUTABLE = new Executable[0];

        public int GetLibraryReferenceId(string name)
        {
            return this.librariesAlreadyImportedIndexByName[name] + 1;
        }

        public Library[] LibrariesUsed { get { return this.librariesAlreadyImported.ToArray(); } }

        public Library ImportLibrary(Parser parser, Token throwToken, string name, List<Executable> executablesOut)
        {
            name = name.Split('.')[0];
            Library library = librariesAlreadyImportedIndexByName.ContainsKey(name)
                ? librariesAlreadyImported[librariesAlreadyImportedIndexByName[name]]
                : null;
            
            if (library == null)
            {
                string libraryManifestPath = this.GetSystemLibraryPath(name, parser.BuildContext.CrayonPath, parser.BuildContext.ProjectDirectory);

                if (libraryManifestPath == null)
                {
                    // No library found. Could just be a local namespace import.
                    // If this is a bogus import, it'll throw in the Resolver.
                    return null;
                }

                string platformName = parser.BuildContext.Platform;
                Platform.AbstractPlatform platform = platformName == null || this.PlatformProvider == null ? null : this.PlatformProvider.GetPlatform(platformName);
                library = new Library(name, libraryManifestPath, platform);

                this.librariesAlreadyImportedIndexByName[name] = this.librariesAlreadyImported.Count;
                this.librariesAlreadyImported.Add(library);

                this.importedLibraries[name] = library;
                this.librariesByKey[name.ToLowerInvariant()] = library;

                string oldSystemLibrary = parser.CurrentSystemLibrary;
                parser.CurrentSystemLibrary = name;
                
                Dictionary<string, string> embeddedCode = library.GetEmbeddedCode();
                foreach (string embeddedFile in embeddedCode.Keys)
                {
                    string fakeName = "[" + embeddedFile + "]";
                    string code = embeddedCode[embeddedFile];
                    executablesOut.AddRange(parser.ParseInterpretedCode(fakeName, code, name));
                }

                parser.CurrentSystemLibrary = oldSystemLibrary;
            }

            // Even if already imported, still must check to see if this import is allowed here.
            if (!library.IsAllowedImport(parser.CurrentSystemLibrary))
            {
                throw new ParserException(throwToken, "This library cannot be imported from here.");
            }

            return library;
        }
    }
}
