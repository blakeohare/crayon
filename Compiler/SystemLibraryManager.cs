using System.Collections.Generic;
using Crayon.ParseTree;

namespace Crayon
{
    internal class SystemLibraryManager
    {
        private Dictionary<string, Library> importedLibraries = new Dictionary<string, Library>();
        private Dictionary<string, Library> librariesByKey = new Dictionary<string, Library>();

        private Dictionary<string, string> functionNameToLibraryName = new Dictionary<string, string>();

        private Dictionary<string, int> libFunctionIds = new Dictionary<string, int>();
        private List<string> orderedListOfFunctionNames = new List<string>();

        public static bool IsValidLibrary(string name)
        {
            return systemLibraryPathsByName.ContainsKey(name);
        }

        public string GetLibrarySwitchStatement(AbstractPlatform platform)
        {
            bool isPastel = platform.PlatformId == PlatformId.PASTEL_VM;
            List<string> output = new List<string>();
            foreach (string name in this.orderedListOfFunctionNames)
            {
                output.Add("case " + this.libFunctionIds[name] + ":\n");
                if (isPastel)
                {
                    output.Add("Core.EmitComment(\"" + name + "\");");
                }
                else
                {
                    output.Add("$_comment('" + name + "');");
                }
                output.Add(this.importedLibraries[this.functionNameToLibraryName[name]].GetTranslationCode(name, isPastel));
                output.Add("\nbreak;\n");
            }
            if (this.orderedListOfFunctionNames.Count == 0)
            {
                output.Add("case 0: break;");
            }
            return string.Join("\n", output);
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

        private string GetSystemLibraryPath(string name, string buildFileCrayonPath, string projectDirectory)
        {
            if (systemLibraryPathsByName == null)
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
                    directoriesToCheck.AddRange(System.IO.Directory.GetDirectories(crayonHomeLibraries));
                }

                string crayonPaths =
                    (buildFileCrayonPath ?? "") + ";" +
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
                            : System.IO.Path.Combine(projectDirectory, path);
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
            }

            string fullpath;
            return systemLibraryPathsByName.TryGetValue(name, out fullpath)
                ? fullpath
                : null;
        }

        private readonly Dictionary<string, Library> alreadyImported = new Dictionary<string, Library>();
        private static readonly Executable[] EMPTY_EXECUTABLE = new Executable[0];

        public Dictionary<string, Library> LibrariesImportedByName { get { return this.alreadyImported; } }

        public Executable[] ImportLibrary(Parser parser, Token throwToken, string name)
        {
            name = name.Split('.')[0];
            Library library = alreadyImported.ContainsKey(name) ? alreadyImported[name] : null;
            Executable[] embedCode = EMPTY_EXECUTABLE;
            if (library == null)
            {
                string libraryManifestPath = this.GetSystemLibraryPath(name, parser.BuildContext.CrayonPath, parser.BuildContext.ProjectDirectory);

                if (libraryManifestPath == null)
                {
                    // No library found. Could just be a local namespace import.
                    // If this is a bogus import, it'll throw in the Resolver.
                    return null;
                }

                string platform = parser.BuildContext.Platform;
                string language = platform.Split('-')[1];

                library = new Library(name, libraryManifestPath, platform, language);

                alreadyImported.Add(name, library);

                library.ExtractResources(platform, this.filesToCopy, this.contentToEmbed);

                this.importedLibraries[name] = library;
                this.librariesByKey[name.ToLowerInvariant()] = library;

                string oldSystemLibrary = parser.CurrentSystemLibrary;
                parser.CurrentSystemLibrary = name;

                List<Executable> output = new List<Executable>();
                Dictionary<string, string> embeddedCode = library.GetEmbeddedCode();
                foreach (string embeddedFile in embeddedCode.Keys)
                {
                    string fakeName = "[" + embeddedFile + "]";
                    string code = embeddedCode[embeddedFile];
                    output.AddRange(parser.ParseInterpretedCode(fakeName, code, name));
                }

                parser.CurrentSystemLibrary = oldSystemLibrary;
                embedCode = output.ToArray();
            }

            // Even if already imported, still must check to see if this import is allowed here.
            if (!library.IsAllowedImport(parser.CurrentSystemLibrary))
            {
                throw new ParserException(throwToken, "This library cannot be imported from here.");
            }

            return embedCode;
        }

        private Dictionary<string, string> filesToCopy = new Dictionary<string, string>();
        private List<string> contentToEmbed = new List<string>();

        public Dictionary<string, string> CopiedFiles
        {
            get
            {
                return new Dictionary<string, string>(this.filesToCopy);
            }
        }

        public string EmbeddedContent
        {
            get
            {
                // TODO: check the content itself to see if there's a \r\n or \n and then use the correct one.
                return string.Join("\n", contentToEmbed);
            }
        }
    }
}
