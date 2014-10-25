using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Crayon.Translator.JavaScript
{
	class JavaScriptPlatform : AbstractPlatform
	{
		private string jsFolderPrefix;

		public JavaScriptPlatform(bool isMin, string jsFolderPrefix)
			: base(isMin, new JavaScriptTranslator(), new JavaScriptSystemFunctionTranslator())
		{
			this.jsFolderPrefix = jsFolderPrefix;
		}

		public override bool IsAsync { get { return true; } }
		public override bool SupportsListClear { get { return false; } }
		public override bool IsStronglyTyped { get { return false; } }
		public override string OutputFolderName { get { return "javascript"; } }

		public override Dictionary<string, FileOutput> Package(string projectId, Dictionary<string, ParseTree.Executable[]> finalCode, List<string> filesToCopyOver, ICollection<ParseTree.StructDefinition> structDefinitions)
		{
			Dictionary<string, FileOutput> output = new Dictionary<string, FileOutput>();

			output["index.html"] = new FileOutput()
			{
				Type = FileOutputType.Text,
				TextContent = this.GenerateHtmlFile()
			};

			List<string> codeJs = new List<string>();

			codeJs.Add(Minify(Util.ReadFileInternally("Translator/JavaScript/Browser/game_code.js")));
			codeJs.Add(this.Translator.NL);
			codeJs.Add(Minify(Util.ReadFileInternally("Translator/JavaScript/Browser/interpreter_helpers.js")));
			codeJs.Add(this.Translator.NL);

			foreach (string component in finalCode.Keys)
			{
				this.Translator.Translate(codeJs, finalCode[component]);
				codeJs.Add(this.Translator.NL);
			}

			string codeJsText = Constants.DoReplacements(string.Join("", codeJs).Replace("%%%JS_FILE_PREFIX%%%", "'" + this.jsFolderPrefix + "'"));

			output["code.js"] = new FileOutput()
			{
				Type = FileOutputType.Text,
				TextContent = codeJsText
			};

			foreach (string file in filesToCopyOver)
			{
				output[file] = new FileOutput()
				{
					Type = FileOutputType.Copy,
					RelativeInputPath = file
				};
			}

			return output;
		}

		private string Minify(string data)
		{
			if (!this.IsMin)
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
				if (commentLoc != -1)
				{
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
		}

		public string GenerateHtmlFile()
		{
			List<string> output = new List<string>();

			output.Add("<!doctype html>\n<html lang=\"en-US\" dir=\"ltr\">");
			output.Add("<head>");
			output.Add("<meta charset=\"utf-8\">");
			output.Add("<title>Crayon JS output</title>");
			output.Add("<script type=\"text/javascript\" src=\"code.js\"></script>");
			output.Add("</head>");
			output.Add("<body onload=\"v_main()\">");
			output.Add(Util.ReadFileInternally("Translator/JavaScript/Browser/game_host_html.txt"));
			output.Add("</body>");
			output.Add("</html>");

			return string.Join(this.IsMin ? "" : "\n", output);
		}
	}
}
