using System.Collections.Generic;
using System.Linq;

namespace Crayon.Translator.JavaScript
{
    class JavaScriptPlatform : AbstractPlatform
	{
		private string jsFolderPrefix;

		public JavaScriptPlatform(bool isMin, string jsFolderPrefix)
			: base(PlatformId.JAVASCRIPT_CANVAS, LanguageId.JAVASCRIPT, isMin, new JavaScriptTranslator(), new JavaScriptSystemFunctionTranslator(), false)
		{
			this.jsFolderPrefix = jsFolderPrefix;
		}

		public override bool IsAsync { get { return true; } }
		public override bool SupportsListClear { get { return false; } }
		public override bool IsStronglyTyped { get { return false; } }
		public override bool ImagesLoadInstantly { get { return false; } }
		public override bool IsArraySameAsList { get { return true; } }
		public override string PlatformShortId { get { return "javascript"; } }
        
		public override Dictionary<string, FileOutput> Package(
            BuildContext buildContext,
            string projectId,
            Dictionary<string, ParseTree.Executable[]> finalCode,
            ICollection<ParseTree.StructDefinition> structDefinitions,
            string fileCopySourceRoot,
            ResourceDatabase resourceDatabase)
		{
			Dictionary<string, FileOutput> output = new Dictionary<string, FileOutput>();

            Dictionary<string, string> replacements = new Dictionary<string, string>()
            {
                { "JS_FILE_PREFIX", "'" + this.jsFolderPrefix + "'" },
                { "JS_BYTE_CODE", Util.ConvertStringValueToCode(resourceDatabase.ByteCodeFile.TextContent, true) },
                { "JS_RESOURCE_MANIFEST", Util.ConvertStringValueToCode(resourceDatabase.ResourceManifestFile.TextContent, true) },
                { "PROJECT_ID", projectId },

                // TODO: create a field in the build file
                { "BUILD_FILE_DEFINED_TITLE", "Untitled" },
            };

            output["index.html"] = new FileOutput()
            {
                Type = FileOutputType.Text,
                TextContent = Constants.DoReplacements(this.GenerateHtmlFile(), replacements),
			};

			List<string> codeJs = new List<string>();

			foreach (string jsFile in new string[] {
				"game_code.js",
				"input.js",
				"drawing.js",
				"sound.js",
				"file_io.js",
				"fake_disk.js",
				"gamepad.js",
				"http.js",
				"interpreter_helpers.js",
			})
			{
				codeJs.Add(Minify(Util.ReadResourceFileInternally("javascript/" + jsFile)));
				codeJs.Add(this.Translator.NL);
			}

			foreach (string component in finalCode.Keys)
			{
				this.Translator.Translate(codeJs, finalCode[component]);
				codeJs.Add(this.Translator.NL);
			}

			string codeJsText = Constants.DoReplacements(string.Join("", codeJs), replacements);

			if (this.IsMin)
			{
				codeJsText = JavaScriptMinifier.Minify(codeJsText);
			}

			output["code.js"] = new FileOutput()
			{
				Type = FileOutputType.Text,
				TextContent = codeJsText
			};

			output["bytecode.js"] = new FileOutput()
			{
				Type = FileOutputType.Text,
				TextContent = Constants.DoReplacements(Util.ReadResourceFileInternally("javascript/bytecode.js"), replacements),
			};
            
			Dictionary<string, string> textResources = new Dictionary<string, string>();
            foreach (FileOutput textFile in resourceDatabase.TextResources)
            {
                textResources["text/" + textFile.CanonicalFileName] = textFile.TextContent;
            }

            foreach (string filename in resourceDatabase.SpriteSheetFiles.Keys)
            {
                FileOutput imageSheetFile = resourceDatabase.SpriteSheetFiles[filename];
                output["resources/imagesheets/" + filename] = imageSheetFile;
            }

            textResources["image_sheet_manifest.txt"] = resourceDatabase.SpriteSheetManifestFile.TextContent;

            foreach (FileOutput audioFile in resourceDatabase.AudioResources)
            {
                output["resources/audio/" + audioFile.CanonicalFileName] = audioFile;
            }

            output["resources.js"] = new FileOutput()
            {
                TextContent = BuildTextResourcesCodeFile(textResources),
                Type = FileOutputType.Text
            };

            return output;
		}
        
		private string BuildTextResourcesCodeFile(Dictionary<string, string> files)
        {
            List<string> output = new List<string>();
			string[] keys = files.Keys.OrderBy<string, string>(s => s.ToLowerInvariant()).ToArray();
			for (int i = 0; i < keys.Length; ++i)
			{
				string filename = keys[i];
				output.Add("R.addTextRes(");
                output.Add(Util.ConvertStringValueToCode(filename, true));
                output.Add(", ");
                output.Add(Util.ConvertStringValueToCode(files[filename], true));
                output.Add(");\n");
			}
			return string.Join("", output);
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
            return Util.ReadResourceFileInternally("javascript/game_host_html.txt");
        }
	}
}
