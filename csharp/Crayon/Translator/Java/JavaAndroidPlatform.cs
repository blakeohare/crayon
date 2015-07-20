using System;
using System.Collections.Generic;
using System.Linq;
using Crayon.ParseTree;

namespace Crayon.Translator.Java
{
	internal class JavaAndroidPlatform : JavaPlatform
	{
		public JavaAndroidPlatform()
			: base(new JavaAndroidSystemFunctionTranslator())
		{
		}

		public override bool IsOpenGlBased { get { return true; } }
		public override string GeneratedFilesFolder { get { return "app/src/main/res/generated"; } }

		public override Dictionary<string, FileOutput> Package(
			BuildContext buildContext,
			string projectId,
			Dictionary<string, ParseTree.Executable[]> finalCode,
			List<string> filesToCopyOver,
			ICollection<ParseTree.StructDefinition> structDefinitions,
			string inputFolder,
			SpriteSheetBuilder spriteSheet)
		{
			Dictionary<string, FileOutput> output = new Dictionary<string, FileOutput>();
			string package = "com." + projectId.ToLowerInvariant() + ".app";
			string pathToJavaCode = "app/src/main/java/" + package.Replace('.', '/');

			Dictionary<string, string> replacements = new Dictionary<string, string>() {
				{ "PROJECT_TITLE", projectId },
				{ "PROJECT_ID", projectId },
				{ "PACKAGE", package },
				{ "CURRENT_YEAR", DateTime.Now.Year.ToString() },
				{ "COPYRIGHT", "©" },
			};

			foreach (string simpleCopyText in new string[] {
				"build.gradle|BuildGradle.txt",
				"gradle.properties|GradleProperties.txt",
				"gradlew|Gradlew.txt",
				"gradlew.bat|GradlewBat.txt",
				projectId + ".iml|ProjectIml.txt",
				"settings.gradle|SettingsGradle.txt",
				"gradle/wrapper/gradle-wrapper.jar|GradleWrapperJar",
				"gradle/wrapper/gradle-wrapper.properties|GradleWrapperProperties.txt",
				"app/app.iml|AppIml.txt",
				"app/build.gradle|AppBuildGradle.txt",
				"app/src/main/res/layout/activity_game.xml|ResLayoutActivityGameXml.txt",
				"app/src/main/res/values/attrs.xml|ResValuesAttrsXml.txt",
				"app/src/main/res/values/colors.xml|ResValuesColorsXml.txt",
				"app/src/main/res/values/strings.xml|ResValuesStringsXml.txt",
				"app/src/main/res/values/styles.xml|ResValuesStylesXml.txt",
				"app/src/main/res/values-v11/styles.xml|ResValuesV11StylesXml.txt",
				"app/src/main/res/drawable-hdpi/ic_launcher.png|ResDrawableHdpiIcLauncher.png",
				"app/src/main/res/drawable-mdpi/ic_launcher.png|ResDrawableMdpiIcLauncher.png",
				"app/src/main/res/drawable-xhdpi/ic_launcher.png|ResDrawableXhdpiIcLauncher.png",
				"app/src/main/res/drawable-xxhdpi/ic_launcher.png|ResDrawableXxhdpiIcLauncher.png",
				pathToJavaCode + "/GameActivity.java|JavaGameActivity.txt",
				pathToJavaCode + "/util/SystemUiHider.java|JavaUtilSystemUiHider.txt",
				pathToJavaCode + "/util/SystemUiHiderBase.java|JavaUtilSystemUiHiderBase.txt",
				pathToJavaCode + "/util/SystemUiHiderHoneycomb.java|JavaUtilSystemUiHiderHoneycomb.txt",
				"app/src/main/AndroidManifest.xml|AndroidManifestXml.txt",
				"app/src/main/ic_launcher-web.png|IcLauncherWeb.png",
			}) {
				string[] parts = simpleCopyText.Split('|');
				string target = parts[0];
				string source = parts[1];
				bool isBinary = !source.EndsWith(".txt");
				if (isBinary)
				{
					output[target] = new FileOutput()
					{
						Type = FileOutputType.Binary,
						BinaryContent = Util.ReadBytesInternally("Translator/Java/AndroidProject/" + source)
					};
				}
				else
				{
					string text = Util.ReadFileInternally("Translator/Java/AndroidProject/" + source);
					output[target] = new FileOutput()
					{
						Type = FileOutputType.Text,
						TextContent = Constants.DoReplacements(text, replacements)
					};
				}
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
				output[pathToJavaCode + "/" + basicFile + ".java"] = new FileOutput()
				{
					Type = FileOutputType.Text,
					TextContent = Util.MassReplacements(
						Util.ReadFileInternally("Translator/Java/Project/" + basicFile + ".txt"),
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

			output[pathToJavaCode + "/CrayonWrapper.java"] = new FileOutput()
			{
				Type = FileOutputType.Text,
				TextContent = Util.MassReplacements(string.Join("", crayonWrapper), replacements)
			};

			string crayonHeader = string.Join(this.Translator.NL, new string[] {
					"package " + package + ";",
					"",
					"import java.util.ArrayList;",
					"import java.util.HashMap;",
					"import java.util.Stack;",
					"",
					""
				});


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

				output[pathToJavaCode + "/" + filename] = new FileOutput()
				{
					Type = FileOutputType.Text,
					TextContent = fileContents
				};

				output["app/src/main/res/bytecode/ByteCode.txt"] = new FileOutput()
				{
					Type = FileOutputType.Text,
					TextContent = this.Context.ByteCodeString
				};
			}

			foreach (string file in filesToCopyOver)
			{
				System.Drawing.Bitmap bmpHack = null;
				if (file.ToLowerInvariant().EndsWith(".png"))
				{
					bmpHack = HackUtil.ReEncodePngImageForJava(System.IO.Path.Combine(inputFolder, file));
				}

				if (bmpHack != null)
				{
					output["app/src/main/res/images/" + file] = new FileOutput()
					{
						Type = FileOutputType.Image,
						Bitmap = bmpHack
					};
				}
				else
				{
					output["app/src/main/res/images/" + file] = new FileOutput()
					{
						Type = FileOutputType.Copy,
						RelativeInputPath = file
					};
				}
			}

			return output;
		}
	}
}
