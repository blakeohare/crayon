using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crayon.ParseTree;
using LibraryConfig;

namespace Crayon
{
	internal class SystemLibraryManager
	{
        // object is either an ILibraryConfig or a LibraryLoader
        private Dictionary<string, ILibraryConfig> importedLibraries = new Dictionary<string, ILibraryConfig>();
		private Dictionary<string, ILibraryConfig> librariesByKey = new Dictionary<string, ILibraryConfig>();

		private Dictionary<string, string> functionNameToLibraryName = new Dictionary<string, string>();

		private Dictionary<string, int> libFunctionIds = new Dictionary<string, int>();
		private List<string> orderedListOfFunctionNames = new List<string>();

		public SystemLibraryManager() { }

		public string GetLibrarySwitchStatement(AbstractPlatform platform)
		{
			List<string> output = new List<string>();
			foreach (string name in this.orderedListOfFunctionNames)
			{
				output.Add("case " + this.libFunctionIds[name] + ":\n");
				output.Add("$_comment('" + name + "');");
				output.Add(this.importedLibraries[this.functionNameToLibraryName[name]].GetTranslationCode(platform, name));
				output.Add("\nbreak;\n");
			}
			return string.Join("\n", output);
		}

		public ILibraryConfig GetLibraryFromKey(string key)
		{
			ILibraryConfig output;
			return this.librariesByKey.TryGetValue(key, out output) ? output : null;
		}

		public int GetIdForFunction(string name, string library)
		{
			if (this.libFunctionIds.ContainsKey(name))
			{
				return this.libFunctionIds[name];
			}

			if (library != "Core" && name.StartsWith("lib_core_"))
			{
				library = "Core";
			}

			this.functionNameToLibraryName[name] = library;
			this.orderedListOfFunctionNames.Add(name);
			int id = this.orderedListOfFunctionNames.Count;
			this.libFunctionIds[name] = id;
			return id;
		}

		public string GetEmbeddedCode(string libraryName)
		{
			return this.importedLibraries[libraryName].GetEmbeddedCode();
		}

		public Dictionary<string, string> GetSupplementalTranslationFiles()
		{
			Dictionary<string, string> output = new Dictionary<string, string>();
			foreach (ILibraryConfig library in this.importedLibraries.Values)
			{
				Dictionary<string, string> files = library.GetSupplementalTranslatedCode();
				foreach (string key in files.Keys)
				{
					output[key] = files[key];
				}
			}
			return output;
		}

		private Dictionary<string, string> systemLibraryPathsByName = null;

		private string GetSystemLibraryPath(string name)
		{
			if (this.systemLibraryPathsByName == null)
			{
				this.systemLibraryPathsByName = new Dictionary<string, string>();

				List<string> crayonHomeDlls = new List<string>();
                List<string> crayonHomeNewLibs = new List<string>();

				string crayonHome = System.Environment.GetEnvironmentVariable("CRAYON_HOME");

				if (crayonHome == null)
				{
					throw new InvalidOperationException("Please set the CRAYON_HOME environment variable to the location of the directory containing both 'crayon.exe' and the 'lib' directory.");
				}

				if (crayonHome != null)
				{
					string crayonHomeLibraries = System.IO.Path.Combine(crayonHome, "lib");
					foreach (string dll in System.IO.Directory.GetFiles(crayonHomeLibraries))
					{
						if (dll.EndsWith(".dll"))
						{
							crayonHomeDlls.Add(dll);
						}
					}
				}

                if (crayonHome != null)
                {
                    string crayonHomeLibraries = System.IO.Path.Combine(crayonHome, "libs");
                    foreach (string dir in System.IO.Directory.GetDirectories(crayonHomeLibraries))
                    {
                        string libName = System.IO.Path.GetFileName(dir);
                        string manifestPath = System.IO.Path.Combine(dir, libName + ".manifest.txt");
                        if (System.IO.File.Exists(manifestPath))
                        {
                            crayonHomeNewLibs.Add(manifestPath);
                        }
                    }
                }

				List<string> debugDlls = new List<string>();
#if DEBUG

				// Presumably running from source. Walk up to the root directory and find the Libraries directory.
				// From there use the list of folders.
				string currentDirectory = System.IO.Path.GetFullPath(".");
				string librariesDirectory = null;
				while (currentDirectory != null && currentDirectory.Length > 0)
				{
					string path = System.IO.Path.Combine(currentDirectory, "Libraries");
					if (System.IO.Directory.Exists(path))
					{
						librariesDirectory = path;
						break;
					}
					currentDirectory = System.IO.Path.GetDirectoryName(currentDirectory);
				}

				if (librariesDirectory != null)
				{
					foreach (string libraryDirectory in System.IO.Directory.GetDirectories(librariesDirectory))
					{
						string libraryName = System.IO.Path.GetFileName(libraryDirectory);
						string dllPath = System.IO.Path.Combine(libraryDirectory, "bin", "Debug", libraryName + ".dll");
						if (System.IO.File.Exists(dllPath))
						{
							debugDlls.Add(dllPath);
						}
					}
				}
#endif
				List<string> allLibrariesInPrecedenceOrder = new List<string>();
				allLibrariesInPrecedenceOrder.AddRange(crayonHomeDlls);
				allLibrariesInPrecedenceOrder.AddRange(debugDlls);
                allLibrariesInPrecedenceOrder.AddRange(crayonHomeNewLibs);


				foreach (string file in allLibrariesInPrecedenceOrder)
				{
                    string libraryName;
                    if (file.EndsWith(".dll"))
                    {
                        libraryName = System.IO.Path.GetFileNameWithoutExtension(file);
                    }
                    else
                    {
                        libraryName = System.IO.Path.GetDirectoryName(file);
                        libraryName = System.IO.Path.GetFileName(libraryName);
                    }
					this.systemLibraryPathsByName[libraryName] = file;
				}
			}

			string fullpath;
			return this.systemLibraryPathsByName.TryGetValue(name, out fullpath)
				? fullpath
				: null;
		}

		private HashSet<string> alreadyImported = new HashSet<string>();
		private static readonly Executable[] EMPTY_EXECUTABLE = new Executable[0];

		public Executable[] ImportLibrary(Parser parser, Token throwToken, string name)
		{
			name = name.Split('.')[0];
			if (alreadyImported.Contains(name))
			{
				return EMPTY_EXECUTABLE;
			}

			alreadyImported.Add(name);

            // this is now either a DLL or a manifest path.
            // once this has been converted entirely to manifest files, remove all references to DLL loading.
			string dllPath = this.GetSystemLibraryPath(name);

			if (dllPath == null)
			{
                // TODO: figure out why this was here. Shouldn't this be an error?
				return EMPTY_EXECUTABLE;
			}

            if (dllPath.EndsWith(".manifest.txt"))
            {
                LibraryLoader library = new LibraryLoader(name, dllPath, parser.BuildContext.Platform);

                this.importedLibraries[name] = library;
                this.librariesByKey[name.ToLowerInvariant()] = library;

                string oldSystemLibrary = parser.CurrentSystemLibrary;
                parser.CurrentSystemLibrary = name;

                string libraryCode = library.GetEmbeddedCode();
                Executable[] libraryParseTree = parser.ParseInterpretedCode("[" + name + "]", libraryCode, name);

                parser.CurrentSystemLibrary = oldSystemLibrary;
                return libraryParseTree;
            }
            else if (dllPath.EndsWith(".dll"))
            {
                System.Reflection.Assembly assembly = null;
                try
                {
                    assembly = System.Reflection.Assembly.LoadFrom(dllPath);
                }
                catch (Exception)
                {
                    throw new ParserException(throwToken, "Could not import library: " + name);
                }

                ILibraryConfig libraryConfig = assembly.CreateInstance(name + ".Config") as ILibraryConfig;
                if (libraryConfig == null)
                {
                    throw new ParserException(throwToken, "Error creating LibraryConfig instance in Library '" + name + "'");
                }

                this.importedLibraries[name] = libraryConfig;
                this.librariesByKey[name.ToLowerInvariant()] = libraryConfig;

                string oldSystemLibrary = parser.CurrentSystemLibrary;
                parser.CurrentSystemLibrary = name;

                string libraryCode = libraryConfig.GetEmbeddedCode();
                Executable[] libraryParseTree = parser.ParseInterpretedCode("[" + name + "]", libraryCode, name);

                parser.CurrentSystemLibrary = oldSystemLibrary;
                return libraryParseTree;
            }
            else
            {
                throw new ParserException(throwToken, "Could not import library: " + name);
            }
		}
	}
}
