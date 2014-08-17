using System;
using System.Collections.Generic;

namespace Crayon
{
	internal class Packager
	{
		private PlatformTarget platform;
		private string folder;
		private string targetFolder;

		public Packager(PlatformTarget platform, string rootFolder, string targetFolder)
		{
			this.platform = platform;
			this.folder = rootFolder;
			this.targetFolder = targetFolder;
		}

		public void Do()
		{
			Compiler compiler = new Compiler(this.platform, false, this.folder);
			string finalcode = compiler.Compile();

			if (!System.IO.Directory.Exists(this.targetFolder))
			{
				throw new Exception("Target directory does not exist.");
			}

			string outputFolder = System.IO.Path.Combine(targetFolder, this.GetPlatformFolderName(this.platform));

			if (!System.IO.Directory.Exists(outputFolder))
			{
				System.IO.Directory.CreateDirectory(outputFolder);
			}
			else
			{
				// TODO: delete everything in there.
			}


			List<string> filesToCopyOver = new List<string>();
			this.GetRelativePaths(this.folder, null, filesToCopyOver);

			switch (this.platform)
			{
				case PlatformTarget.Python_PyGame: this.SerializePython(outputFolder, finalcode, filesToCopyOver); break;
				case PlatformTarget.JavaScript_Browser: this.SerializeJavaScript(outputFolder, finalcode, filesToCopyOver); break;
				default: throw new NotImplementedException();
			}
		}

		private void SerializePython(string folder, string finalCode, IList<string> filesToCopyOver)
		{
			System.IO.File.WriteAllText(System.IO.Path.Combine(folder, "game.py"), finalCode);
		}

		private void SerializeJavaScript(string folder, string finalCode, IList<string> filesToCopyOver)
		{
			Crayon.Translator.JavaScript.Browser.BrowserImplementation.GenerateHtmlFile(folder);
			System.IO.File.WriteAllText(System.IO.Path.Combine(folder, "code.js"), finalCode);
		}

		private string GetPlatformFolderName(PlatformTarget platform)
		{
			switch (platform)
			{
				case PlatformTarget.Python_PyGame: return "pygame";
				case PlatformTarget.JavaScript_Browser: return "javascript";
				case PlatformTarget.CSharp_Windows: return "windows";
				case PlatformTarget.CSharp_WindowsPhone: return "winphone";
				case PlatformTarget.CSharp_XBox: return "xbox";
				case PlatformTarget.Java_Android: return "android";
				case PlatformTarget.Java_Desktop: return "javaclient";
				case PlatformTarget.Java_Ouya: return "ouya";
				case PlatformTarget.Swift_iThing: return "iThing";
				default: return "unknown";
			}
		}

		private void GetRelativePaths(string root, string folder, List<string> output)
		{
			string thisFolder = folder != null ? System.IO.Path.Combine(root, folder) : root;
			foreach (string subfolder in System.IO.Directory.GetDirectories(thisFolder))
			{
				string lowername = subfolder.ToLowerInvariant();
				bool ignore = lowername == ".svn";
				if (!ignore)
				{
					GetRelativePaths(root, System.IO.Path.Combine(thisFolder, subfolder), output);
				}
			}

			foreach (string file in System.IO.Directory.GetFiles(thisFolder))
			{
				string extension = System.IO.Path.GetExtension(file).ToLowerInvariant();
				bool ignore = (extension == ".cry") ||
					file == "thumbs.db" ||
					file == ".ds_store";

				if (!ignore)
				{
					string fullFileName = folder == null ? file : System.IO.Path.Combine(folder, file);
					output.Add(fullFileName);
				}
			}
		}
	}
}
