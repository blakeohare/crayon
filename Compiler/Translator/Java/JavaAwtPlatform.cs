using Crayon.ParseTree;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Crayon.Translator.Java
{
    internal class JavaAwtPlatform : JavaPlatform
	{
        public JavaAwtPlatform()
            : base(PlatformId.JAVA_AWT, new JavaAwtSystemFunctionTranslator(), null)
        { }

		public override string GeneratedFilesFolder { get { return "resources/generated"; } }
		public override string PlatformShortId { get { return "javaawt"; } }

		public override Dictionary<string, FileOutput> Package(
            BuildContext buildContext,
            string projectId,
            Dictionary<string, ParseTree.Executable[]> finalCode,
            ICollection<ParseTree.StructDefinition> structDefinitions,
            string fileCopySourceRoot,
            ResourceDatabase resourceDatabase)
		{
			Dictionary<string, FileOutput> output = new Dictionary<string, FileOutput>();
			string package = projectId.ToLowerInvariant();

			Dictionary<string, string> replacements = new Dictionary<string, string>() {
				{ "PROJECT_TITLE", projectId },
				{ "PROJECT_ID", projectId },
				{ "PACKAGE", package },
				{ "CURRENT_YEAR", DateTime.Now.Year.ToString() },
				{ "COPYRIGHT", "©" },
			};

			foreach (string awtFile in new string[] { 
				"AwtTranslationHelper",
			})
			{
				output["src/" + package + "/" + awtFile + ".java"] = new FileOutput()
				{
					Type = FileOutputType.Text,
					TextContent = Constants.DoReplacements(
						Util.ReadResourceFileInternally("java-awt/" + awtFile + ".txt"),
						replacements)
				};
			}

			foreach (string basicFile in new string[] { 
				"AsyncMessageQueue",
				"TranslationHelper",
				"Start",
				"GameWindow",
				"RenderEngine",
				"Image",
				"JsonParser"
			})
			{
				output["src/" + package + "/" + basicFile + ".java"] = new FileOutput()
				{
					Type = FileOutputType.Text,
					TextContent = Constants.DoReplacements(
						Util.ReadResourceFileInternally("java-common/" + basicFile + ".txt"),
						replacements)
				};
			}

			foreach (string basicFile in new string[] { 
				"AwtTranslationHelper",
			})
			{
				output["src/" + package + "/" + basicFile + ".java"] = new FileOutput()
				{
					Type = FileOutputType.Text,
					TextContent = Constants.DoReplacements(
						Util.ReadResourceFileInternally("java-awt/" + basicFile + ".txt"),
						replacements)
				};
			}

			string[] items = finalCode.Keys.OrderBy<string, string>(s => s.ToLowerInvariant()).ToArray();

			List<string> crayonWrapper = new List<string>();
			crayonWrapper.Add(string.Join("\n",
				"package %%%PACKAGE%%%;",
				"",
				"import java.util.ArrayList;",
				"import java.util.HashMap;",
				"import java.util.Stack;",
				"",
				"final class CrayonWrapper {",
				"    private CrayonWrapper() {}",
				""));

			this.Translator.CurrentIndention++;
			foreach (string finalCodeKey in items)
			{
				this.Translator.Translate(crayonWrapper, finalCode[finalCodeKey]);
			}
			this.Translator.CurrentIndention--;

			crayonWrapper.Add(string.Join("\n",
				"}",
				""));

			output["src/" + package + "/CrayonWrapper.java"] = new FileOutput()
			{
				Type = FileOutputType.Text,
				TextContent = Constants.DoReplacements(string.Join("", crayonWrapper), replacements)
			};

			string nl = this.Translator.NL;

			foreach (StructDefinition structDefinition in structDefinitions)
			{
				string structName = structDefinition.Name.Value;
				string filename = structName + ".java";

				List<string> codeContents = new List<string>();

				codeContents.Add("class " + structName + " {" + nl);
				codeContents.Add("    public " + structName + "(");
				List<string> types = new List<string>();
				for (int i = 0; i < structDefinition.FieldsByIndex.Length; ++i)
				{
					string type;
					Annotation typeAnnotation = structDefinition.Types[i];
					if (typeAnnotation == null)
					{
						throw new Exception("Are there any of these left?");
					}
					else
					{
						type = this.GetTypeStringFromAnnotation(typeAnnotation.FirstToken, typeAnnotation.GetSingleArgAsString(null), false, false);
					}
					types.Add(type);

					if (i > 0) codeContents.Add(", ");
					codeContents.Add(type);
					codeContents.Add(" v_" + structDefinition.FieldsByIndex[i]);
				}
				codeContents.Add(") {" + nl);

				for (int i = 0; i < structDefinition.FieldsByIndex.Length; ++i)
				{
					codeContents.Add("        this." + structDefinition.FieldsByIndex[i] + " = v_" + structDefinition.FieldsByIndex[i] + ";" + nl);
				}

				codeContents.Add("    }" + nl + nl);
				for (int i = 0; i < structDefinition.FieldsByIndex.Length; ++i)
				{
					codeContents.Add("    public ");
					codeContents.Add(types[i]);
					codeContents.Add(" " + structDefinition.FieldsByIndex[i] + ";" + nl);
				}

				codeContents.Add("}" + nl);

				string fileContents = string.Join("", codeContents);
				string header = "package " + package + ";" + nl + nl;

				bool useList = fileContents.Contains("ArrayList<");
				bool useHashMap = fileContents.Contains("HashMap<");
				bool useStack = fileContents.Contains("Stack<");
				if (useList || useHashMap || useStack)
				{
					if (useList) header += "import java.util.ArrayList;" + nl;
					if (useHashMap) header += "import java.util.HashMap;" + nl;
					if (useStack) header += "import java.util.Stack;" + nl;
					header += nl;
				}
				fileContents = header + fileContents;

				output["src/" + package + "/" + filename] = new FileOutput()
				{
					Type = FileOutputType.Text,
					TextContent = fileContents
				};
			}

            output["data/ByteCode.txt"] = resourceDatabase.ByteCodeFile;

            throw new System.NotImplementedException();
            /*
			foreach (string file in filesToCopyOver)
			{
                // TODO: This was fixed. Remove.
				SystemBitmap bmpHack = null;
				if (file.ToLowerInvariant().EndsWith(".png"))
				{
					bmpHack = new SystemBitmap(FileUtil.JoinPath(inputFolder, file));
				}

				if (bmpHack != null)
				{
					output["resources/" + file] = new FileOutput()
					{
						Type = FileOutputType.Image,
						Bitmap = bmpHack
					};
				}
				else
				{
					output["resources/" + file] = new FileOutput()
					{
						Type = FileOutputType.Copy,
						RelativeInputPath = file
					};
				}
			}//*/

			output["build.xml"] = new FileOutput()
			{
				Type = FileOutputType.Text,
				TextContent = Constants.DoReplacements(
					Util.ReadResourceFileInternally("java-common/BuildXml.txt"),
					replacements)
			};

			return output;
		}
	}
}
