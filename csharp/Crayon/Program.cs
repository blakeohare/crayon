namespace Crayon
{
	internal class Program
	{
		static void Main(string[] args)
		{
#if DEBUG
			string sourceFolder = @"C:\things\projects\Crayon\Demos\Volcano\source";
			string outputFolder = @"C:\things\projects\Crayon\Demos\Volcano\output";
			string rawPlatform = "js";
			bool minified = false;
			string jsFolderRoot = "";
			string outputReadableByteCode = null;
			bool wrapOutput = true;

#else
			if (args.Length != 3 && args.Length != 4)
			{
				System.Console.WriteLine("Usage:");
				System.Console.WriteLine("  C:\\Things> crayon.exe sourcefolder outputfolder platform [jsfolderroot]");
				System.Console.WriteLine("Platforms:");
				foreach (string line in PLATFORM_USAGE)
				{
					System.Console.WriteLine(line);
					return;
				}
			}
			string sourceFolder = args[0];
			string outputFolder = args[1];
			string rawPlatform = args[2].ToLowerInvariant();
			string outputReadableByteCode = null; // TODO: expose this via a flag
			bool minified = false;
			string jsFolderRoot = "";
			if (args.Length == 4)
			{
				jsFolderRoot = args[3];
			}
#endif

			if (!System.IO.Directory.Exists(sourceFolder))
			{
				System.Console.WriteLine("Source folder could not be found.");
				return;
			}

			if (!System.IO.Directory.Exists(outputFolder))
			{
				System.Console.WriteLine("Output folder could not be found.");
				return;
			}

			AbstractPlatform platform;
			switch (rawPlatform.ToLowerInvariant())
			{
				case "java": platform = new Crayon.Translator.Java.JavaPlatform(); break;
				case "cswin": platform = new Crayon.Translator.CSharp.CSharpPlatform(); break;
				case "js": platform = new Crayon.Translator.JavaScript.JavaScriptPlatform(minified, jsFolderRoot); break;
				case "py": platform = new Crayon.Translator.Python.PythonPlatform(); break;
				default:
					System.Console.WriteLine("Platform must be one of the following:");
					foreach (string line in PLATFORM_USAGE)
					{
						System.Console.WriteLine(line);
					}
					return;
			}

			platform.Compile(sourceFolder, outputFolder, outputReadableByteCode, wrapOutput);
		}

		private static readonly string[] PLATFORM_USAGE = new string[] {
			"  cswin - C# (for Windows Client App)",
			"  js - HTML/JavaScript",
			"  java - Java (for Desktop)",
			"  py - Python (with PyGame)",
		};
	}
}
