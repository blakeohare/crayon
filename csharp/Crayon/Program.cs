namespace Crayon
{
	internal class Program
	{
		static void Main(string[] args)
		{
#if DEBUG
			string sourceFolder = @"C:\things\projects\Crayon\Demos\AGameThatInvolvesFire\source";
			string outputFolder = @"C:\things\projects\Crayon\Demos\AGameThatInvolvesFire\output";
			string rawPlatform = "js";

#else
			if (args.Length != 3)
			{
				System.Console.WriteLine("Usage:");
				System.Console.WriteLine("  C:\\Things> crayon.exe sourcefolder outputfolder platform");
				System.Console.WriteLine("Platforms:");
				foreach (string line in PLATFORM_USAGE)
				{
					System.Console.WriteLine(line);
				}
			}
			string sourceFolder = args[0];
			string outputFolder = args[1];
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

			PlatformTarget platform;
			switch (rawPlatform.ToLowerInvariant())
			{
				case "js": platform = PlatformTarget.JavaScript_Browser; break;
				case "py": platform = PlatformTarget.Python_PyGame; break;
				default:
					System.Console.WriteLine("Platform must be one of the following:");
					foreach (string line in PLATFORM_USAGE)
					{
						System.Console.WriteLine(line);
					}
					return;
			}
			Packager pkg = new Packager(platform, sourceFolder, outputFolder);
			pkg.Do();
		}

		private static readonly string[] PLATFORM_USAGE = new string[] {
			"  js - HTML/JavaScript",
			"  py - Python (with PyGame)",
		};
	}
}
