using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crayon.ParseTree;

namespace Crayon.Translator.CSharp
{
	class CSharpPlatform : AbstractPlatform
	{
		public CSharpPlatform()
			: base(false, new CSharpTranslator(), new CSharpSystemFunctionTranslator())
		{ }

		public override bool IsAsync { get { return true; } }
		public override bool SupportsListClear { get { return true; } }
		public override bool IsStronglyTyped { get { return true; } }
		public override bool UseFixedListArgConstruction { get { return true; } }
		public override bool IntIsFloor { get { return false; } }
		public override bool ImagesLoadInstantly { get { return true; } }
		public override bool ScreenBlocksExecution { get { return true; } }
		public override string OutputFolderName { get { return "csharpwindows"; } }

		public override Dictionary<string, FileOutput> Package(string projectId, Dictionary<string, Executable[]> finalCode, List<string> filesToCopyOver, ICollection<StructDefinition> structDefinitions, string inputFolder)
		{
			string guid = Guid.NewGuid().ToString();

			Dictionary<string, string> replacements = new Dictionary<string, string>() {
				{ "PROJECT_GUID", guid },
				{ "PROJECT_TITLE", projectId },
				{ "PROJECT_ID", projectId },
				{ "CURRENT_YEAR", DateTime.Now.Year.ToString() },
				{ "COPYRIGHT", "©" },
			};

			Dictionary<string, FileOutput> output = new Dictionary<string, FileOutput>();
			List<string> compileTargets = new List<string>();

			compileTargets.Add("Program.cs");
			output[projectId + "/Program.cs"] = new FileOutput()
			{
				Type = FileOutputType.Text,
				TextContent = Util.MassReplacements(
					Util.ReadFileInternally("Translator/CSharp/Project/ProgramCs.txt"),
					replacements)
			};

			compileTargets.Add("Properties\\AssemblyInfo.cs");

			output[projectId + "/Properties/AssemblyInfo.cs"] = new FileOutput()
			{
				Type = FileOutputType.Text,
				TextContent = Util.MassReplacements(
					Util.ReadFileInternally("Translator/CSharp/Project/AssemblyInfo.txt"),
					replacements)
			};

			compileTargets.Add("CrStack.cs");
			output[projectId + "/CrStack.cs"] = new FileOutput()
			{
				Type = FileOutputType.Text,
				TextContent = Util.MassReplacements(
					Util.ReadFileInternally("Translator/CSharp/Project/CrStack.txt"),
					replacements)
			};

			compileTargets.Add("TranslationHelper.cs");
			output[projectId + "/TranslationHelper.cs"] = new FileOutput()
			{
				Type = FileOutputType.Text,
				TextContent = Util.MassReplacements(
					Util.ReadFileInternally("Translator/CSharp/Project/TranslationHelper.txt"),
					replacements)
			};

			compileTargets.Add("ResourceReader.cs");
			output[projectId + "/ResourceReader.cs"] = new FileOutput()
			{
				Type = FileOutputType.Text,
				TextContent = Util.MassReplacements(
					Util.ReadFileInternally("Translator/CSharp/Project/ResourceReader.txt"),
					replacements)
			};

			compileTargets.Add("GameWindow.cs");
			output[projectId + "/GameWindow.cs"] = new FileOutput()
			{
				Type = FileOutputType.Text,
				TextContent = Util.MassReplacements(
					Util.ReadFileInternally("Translator/CSharp/Project/GameWindow.txt"),
					replacements)
			};

			compileTargets.Add("Image.cs");
			output[projectId + "/Image.cs"] = new FileOutput()
			{
				Type = FileOutputType.Text,
				TextContent = Util.MassReplacements(
					Util.ReadFileInternally("Translator/CSharp/Project/Image.txt"),
					replacements)
			};

			output[projectId + "/OpenTK.dll"] = new FileOutput()
			{
				Type = FileOutputType.Binary,
				BinaryContent = Util.ReadBytesInternally("Translator/CSharp/OpenTK/OpenTK.dll")
			};

			string crayonHeader = string.Join(this.Translator.NL, new string[] {
					"using System;",
					"using System.Collections.Generic;",
					"using System.Linq;",
					"",
					"namespace " + projectId,
					"{",
					""
				});

			string crayonWrapperHeader = string.Join(this.Translator.NL, new string[] {
					crayonHeader + "\tinternal partial class CrayonWrapper",
					"\t{",
					""
				});

			string crayonFooter = "}" + this.Translator.NL;
			string crayonWrapperFooter = "\t}" + this.Translator.NL + crayonFooter;

			string nl = this.Translator.NL;

			foreach (StructDefinition structDefinition in structDefinitions)
			{
				string structName = structDefinition.Name.Value;
				string filename = structName + ".cs";
				compileTargets.Add(filename);
				List<string> codeContents = new List<string>();
				codeContents.Add(crayonHeader);
				codeContents.Add("\tpublic class " + structName + nl);
				codeContents.Add("\t{" + nl);
				codeContents.Add("\t\tpublic " + structName + "(");
				List<string> types = new List<string>();
				for (int i = 0; i < structDefinition.FieldsByIndex.Length; ++i)
				{
					string type;
					Annotation typeAnnotation = structDefinition.Types[i];
					if (typeAnnotation == null)
					{
						type = "object";
					}
					else
					{
						type = this.GetTypeStringFromAnnotation(typeAnnotation.FirstToken, typeAnnotation.GetSingleArgAsString(null));
					}
					types.Add(type);

					if (i > 0) codeContents.Add(", ");
					codeContents.Add(type);
					codeContents.Add(" v_" + structDefinition.FieldsByIndex[i]);
				}
				codeContents.Add(")" + nl);

				codeContents.Add("\t\t{" + nl);

				for (int i = 0; i < structDefinition.FieldsByIndex.Length; ++i)
				{
					codeContents.Add("\t\t\tthis." + structDefinition.FieldsByIndex[i] + " = v_" + structDefinition.FieldsByIndex[i] + ";" + nl);
				}

				codeContents.Add("\t\t}" + nl + nl);
				for (int i = 0; i < structDefinition.FieldsByIndex.Length; ++i)
				{
					codeContents.Add("\t\tpublic ");
					codeContents.Add(types[i]);
					codeContents.Add(" " + structDefinition.FieldsByIndex[i] + ";" + nl);
				}

				codeContents.Add("\t}" + nl);

				codeContents.Add(crayonFooter);
				output[projectId + "/" + filename] = new FileOutput()
				{
					Type = FileOutputType.Text,
					TextContent = string.Join("", codeContents)
				};
			}

			foreach (string codefile in finalCode.Keys)
			{
				List<string> codeContents = new List<string>();
				
				codeContents.Add(crayonWrapperHeader);
				this.Translator.CurrentIndention = 2;

				this.Translator.Translate(codeContents, finalCode[codefile]);

				codeContents.Add(crayonWrapperFooter);

				string filename = codefile + ".cs";
				compileTargets.Add(filename);
				output[projectId + "/" + filename] = new FileOutput()
				{
					Type = FileOutputType.Text,
					TextContent = string.Join("", codeContents)
				};
			}

			List<string> embeddedResources = new List<string>();
			foreach (string file in filesToCopyOver)
			{
				string filename = "Files/" + file;
				embeddedResources.Add(filename);
				output[projectId + "/" + filename] = new FileOutput()
				{
					Type = FileOutputType.Copy,
					RelativeInputPath = file
				};
			}

			output[projectId + ".sln"] = new FileOutput()
			{
				Type = FileOutputType.Text,
				TextContent = Util.ReadFileInternally("Translator/CSharp/Project/SolutionFile.txt")
			};

			List<string> csprojFile = new List<string>();
			csprojFile.Add(Util.ReadFileInternally("Translator/CSharp/Project/ProjectFileHeader.txt"));
			foreach (string compileTarget in compileTargets)
			{
				csprojFile.Add("    <Compile Include=\"" + compileTarget.Replace('/', '\\') + "\" />\r\n");
			}
			csprojFile.Add("    <EmbeddedResource Include=\"ByteCode.txt\" />\r\n");

			foreach (string embeddedResource in embeddedResources)
			{
				csprojFile.Add("    <EmbeddedResource Include=\"" + embeddedResource.Replace('/', '\\') + "\" />\r\n");
			}
			csprojFile.Add(Util.ReadFileInternally("Translator/CSharp/Project/ProjectFileFooter.txt"));

			output[projectId + "/" + projectId + ".csproj"] = new FileOutput()
			{
				Type = FileOutputType.Text,
				TextContent = Util.MassReplacements(string.Join("", csprojFile), replacements)
			};

			output[projectId + "/ByteCode.txt"] = new FileOutput()
			{
				Type = FileOutputType.Text,
				TextContent = this.Context.ByteCodeString
			};

			return output;
		}

		public string GetTypeStringFromAnnotation(Annotation annotation)
		{
			return GetTypeStringFromAnnotation(annotation.FirstToken, annotation.GetSingleArgAsString(null));
		}

		public string GetTypeStringFromAnnotation(Token stringToken, string value)
		{
			AnnotatedType type = new AnnotatedType(stringToken, Tokenizer.Tokenize("type proxy", value, -1, false));
			return GetTypeStringFromAnnotation(type);
		}

		private string GetTypeStringFromAnnotation(AnnotatedType type)
		{
			string output;

			if (type.Name == "Array")
			{
				output = this.GetTypeStringFromAnnotation(type.Generics[0]);
				output += "[]";
			}
			else
			{
				output = TypeTranslation(type.Name);
				if (type.Generics.Length > 0)
				{
					output += "<";
					for (int i = 0; i < type.Generics.Length; ++i)
					{
						if (i > 0) output += ", ";
						output += this.GetTypeStringFromAnnotation(type.Generics[i]);
					}
					output += ">";
				}
			}
			return output;
		}

		public string TypeTranslation(string original)
		{
			if (original == "Stack") return "CrStack";
			return original;
		}
	}
}
