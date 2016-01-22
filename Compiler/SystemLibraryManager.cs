using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Crayon
{
	internal class SystemLibraryManager
	{

		private Dictionary<string, ILibraryConfig> importedLibraries = new Dictionary<string, ILibraryConfig>();

		public SystemLibraryManager()
		{

		}

		public string GetEmbeddedCode(string libraryName)
		{
			return this.importedLibraries[libraryName].GetEmbeddedCode();
		}

		private static string libraryDirectory = null;

		public string LibraryDirectory
		{
			get
			{
				if (libraryDirectory != null)
				{
					return libraryDirectory;
				}

#if DEBUG
				// Walk up the current directory which is presumably running under source control and look for the
				// "Libraries" directory.
				string currentDirectory = System.IO.Path.GetFullPath(".");

				while (currentDirectory != null && currentDirectory.Length > 0)
				{
					string path = System.IO.Path.Combine(currentDirectory, "Libraries");
					if (System.IO.Directory.Exists(path))
					{
						libraryDirectory = path;
						return path;
					}
					currentDirectory = System.IO.Path.GetDirectoryName(currentDirectory);
				}
				throw new Exception("Library directory is not configured.");
#else
				// Require the CRAYON_HOME environment variable to be defined.
				string crayonHome = System.Environment.GetEnvironmentVariable("CRAYON_HOME");
				if (crayonHome != null && !System.IO.File.Exists(System.IO.Path.Combine(crayonHome, "crayon.exe")))
				{
					crayonHome = null;
				}

				if (crayonHome == null)
				{
					throw new Exception("CRAYON_HOME environment variable must be set to the directory where crayon.exe is located.");
				}

				libraryDirectory = System.IO.Path.Combine(crayonHome, "lib");
				return libraryDirectory;
#endif
			}
		}

		public bool ImportLibrary(string name)
		{
			if (importedLibraries.ContainsKey(name))
			{
				return true;
			}

			string libDir = this.LibraryDirectory;

			string dllPath = System.IO.Path.Combine(libDir, name, "bin", "Debug", name + ".dll");
			if (System.IO.File.Exists(dllPath))
			{
				System.Reflection.Assembly assembly = null;
				try
				{
					assembly = System.Reflection.Assembly.LoadFrom(dllPath);
				}
				catch (Exception e)
				{
					return false;
				}

				ILibraryConfig libraryConfig = assembly.CreateInstance(name + ".LibraryConfig") as ILibraryConfig;
				if (libraryConfig == null)
				{
					return false;
				}

				this.importedLibraries[name] = libraryConfig;
				return true;
			}
			return false;
		}
	}
}
