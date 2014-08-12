namespace Crayon
{
	internal class Program
	{
		static void Main(string[] args)
		{
#if DEBUG
			string sourceFolder = "C:\\Users\\Blake\\Desktop\\flappy_bird\\source";
			string outputFolder = "C:\\Users\\Blake\\Desktop\\flappy_bird\\output";
#else
			if (args.Length != 2) throw new System.Exception("Invalid args");
			string sourceFolder = args[0];
			string outputFolder = args[1];
#endif

			PlatformTarget platform = PlatformTarget.JavaScript_Browser;
			Packager pkg = new Packager(platform, sourceFolder, outputFolder);
			pkg.Do();
		}
	}
}
