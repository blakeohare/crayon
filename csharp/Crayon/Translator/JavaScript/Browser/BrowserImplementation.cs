using System.Collections.Generic;

namespace Crayon.Translator.JavaScript.Browser
{
	internal class BrowserImplementation : AbstractPlatformImplementation
	{
		public override string SerializeBoilerPlates(Parser parser)
		{
			List<string> output = new List<string>();
			output.Add(Util.ReadFileInternally("Translator/JavaScript/Browser/game_code.js"));
			output.Add("\r\n");
			output.Add(Util.ReadFileInternally("Translator/JavaScript/Browser/interpreter_helpers.js"));
			output.Add("\r\n");

			string code = string.Join("", output);
			code = Constants.DoReplacements(code, PlatformTarget.JavaScript_Browser);

			return code;
		}

		public static void GenerateHtmlFile(string folder)
		{
			List<string> output = new List<string>();

			output.Add("<!doctype html>\r\n");
			output.Add("<html lang=\"en-US\" dir=\"ltr\">\r\n");
			output.Add("<head>\r\n");
			output.Add("<meta charset=\"utf-8\">\r\n");
			output.Add("<title>Crayon JS output</title>\r\n");
			output.Add("<script type=\"text/javascript\" src=\"code.js\"></script>\r\n");
			output.Add("</head>\r\n");
			output.Add("<body onload=\"v_main()\">\r\n");
			output.Add(Util.ReadFileInternally("Translator/JavaScript/Browser/game_host.txt"));
			output.Add("</body>\r\n");
			output.Add("</html>\r\n");

			string htmlFile = string.Join("", output);

			System.IO.File.WriteAllText(System.IO.Path.Combine(folder, "index.html"), htmlFile);
		}
	}
}
