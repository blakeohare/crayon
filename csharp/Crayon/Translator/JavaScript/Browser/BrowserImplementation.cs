using System.Collections.Generic;

namespace Crayon.Translator.JavaScript.Browser
{
	internal class BrowserImplementation : AbstractPlatformImplementation
	{
		private string jsFolderPrefix;

		public BrowserImplementation(bool minified, string jsFolderPrefix)
			: base(minified)
		{
			this.jsFolderPrefix = jsFolderPrefix;
		}

		public override string SerializeBoilerPlates(Parser parser)
		{
			List<string> output = new List<string>();
			string gameCode = Util.ReadFileInternally("Translator/JavaScript/Browser/game_code.js");
			gameCode = gameCode.Replace("%%%JS_FILE_PREFIX%%%", "'" + this.jsFolderPrefix + "'");
			output.Add(Minify(gameCode));
			if (this.IsFull) output.Add("\r\n");
			output.Add(Minify(Util.ReadFileInternally("Translator/JavaScript/Browser/interpreter_helpers.js")));
			if (this.IsFull) output.Add("\r\n");

			string code = string.Join("", output);
			code = Constants.DoReplacements(code, PlatformTarget.JavaScript_Browser);

			return code;
		}

		private string Minify(string data)
		{
			if (this.IsFull)
			{

				return data;
			}
			string[] lines = data.Split('\n');
			List<string> output = new List<string>();
			int counter = -1;
			foreach (string rawLine in lines)
			{
				string line = rawLine.Trim();
				int commentLoc = line.IndexOf("//");
				if (commentLoc != -1) {
					line = line.Substring(0, commentLoc);
				}

				line = line
					.Replace(" = ", "=")
					.Replace(", ", ",")
					.Replace(" + ", "+")
					.Replace(" == ", "==")
					.Replace(" += ", "+=")
					.Replace(" -= ", "-=")
					.Replace(" - ", "-")
					.Replace(" / ", "/")
					.Replace("function (", "function(")
					.Replace(") {", "){");
				if (counter-- > 0)
				{
					line += "\r\n";
				}
				output.Add(line);

			}

			return string.Join("", output);
			//return data;
		}

		public void GenerateHtmlFile(string folder)
		{
			List<string> output = new List<string>();

			output.Add("<!doctype html>\n<html lang=\"en-US\" dir=\"ltr\">");
			output.Add("<head>");
			output.Add("<meta charset=\"utf-8\">");
			output.Add("<title>Crayon JS output</title>");
			output.Add("<script type=\"text/javascript\" src=\"code.js\"></script>");
			output.Add("</head>");
			output.Add("<body onload=\"v_main()\">");
			output.Add(Util.ReadFileInternally("Translator/JavaScript/Browser/game_host.txt"));
			output.Add("</body>");
			output.Add("</html>");

			string htmlFile = string.Join(this.IsMin ? "" : "\n", output);

			System.IO.File.WriteAllText(System.IO.Path.Combine(folder, "index.html"), htmlFile);
		}
	}
}
