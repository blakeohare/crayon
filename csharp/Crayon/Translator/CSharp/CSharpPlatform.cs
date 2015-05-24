using System;
using System.Collections.Generic;
using System.Linq;
using Crayon.ParseTree;

namespace Crayon.Translator.CSharp
{
	abstract class CSharpPlatform : AbstractPlatform
	{
		public CSharpPlatform(CSharpSystemFunctionTranslator systemFunctionTranslator)
			: base(false, new CSharpTranslator(), systemFunctionTranslator)
		{ }

		public override bool IsAsync { get { return true; } }
		public override bool SupportsListClear { get { return true; } }
		public override bool IsStronglyTyped { get { return true; } }
		public override bool UseFixedListArgConstruction { get { return true; } }
		public override bool IntIsFloor { get { return false; } }
		public override bool ImagesLoadInstantly { get { return true; } }
		public override bool ScreenBlocksExecution { get { return true; } }
		public override string GeneratedFilesFolder { get { return "%PROJECT_ID%/GeneratedFiles"; } }

		public abstract void PlatformSpecificFiles(
			string projectId,
			List<string> compileTargets,
			Dictionary<string, FileOutput> files,
			Dictionary<string, string> replacements);

		public abstract void ApplyPlatformSpecificReplacements(Dictionary<string, string> replacements);
		public abstract void AddPlatformSpecificSystemLibraries(HashSet<string> systemLibraries);

		public override Dictionary<string, FileOutput> Package(
			BuildContext buildContext,
			string projectId,
			Dictionary<string, Executable[]> finalCode,
			List<string> filesToCopyOver,
			ICollection<StructDefinition> structDefinitions,
			string inputFolder,
			SpriteSheetBuilder spriteSheet)
		{
			string guid = Guid.NewGuid().ToString();
			string guid2 = Guid.NewGuid().ToString();

			Dictionary<string, string> replacements = new Dictionary<string, string>() {
				{ "PROJECT_GUID", guid },
				{ "ASSEMBLY_GUID", guid2 },
				{ "PROJECT_TITLE", projectId },
				{ "PROJECT_ID", projectId },
				{ "CURRENT_YEAR", DateTime.Now.Year.ToString() },
				{ "COPYRIGHT", "©" },
				{ "EXTRA_DLLS", "" },
			};
			this.ApplyPlatformSpecificReplacements(replacements);

			HashSet<string> systemLibraries = new HashSet<string>(new string[] {
				"System",
				"System.Core",
				"System.Drawing",
				"System.Xml",
				"System.Xml.Linq",
				"Microsoft.CSharp"
			});

			this.AddPlatformSpecificSystemLibraries(systemLibraries);
			List<string> systemLibrariesStringBuilder = new List<string>();
			foreach (string library in systemLibraries.OrderBy<string, string>(s => s.ToLowerInvariant()))
			{
				systemLibrariesStringBuilder.Add("    <Reference Include=\"" + library + "\" />");
			}
			replacements["SYSTEM_LIBRARIES"] = string.Join("\r\n", systemLibrariesStringBuilder);

			Dictionary<string, FileOutput> output = new Dictionary<string, FileOutput>();
			List<string> compileTargets = new List<string>();

			// Get embedded resource list
			List<string> embeddedResources = this.GetEmbeddedResources(projectId, output, filesToCopyOver);

			// Code files that are templated
			Dictionary<string, string> directFileCopies = new Dictionary<string, string>()
			{
				{ "SolutionFile.txt", projectId + ".sln" },
				{ "ProjectFile.txt", projectId + ".csproj" },
				{ "ProgramCs.txt", "Program.cs" },
				{ "AssemblyInfo.txt", "Properties/AssemblyInfo.cs" },
				{ "CrStack.txt", "CrStack.cs" },
				{ "JsonParser.txt", "JsonParser.cs" },
				{ "TranslationHelper.txt", "TranslationHelper.cs" },
				{ "ResourceReader.txt", "ResourceReader.cs" },
				{ "AsyncMessageQueue.txt", "AsyncMessageQueue.cs" },
			};

			// Create a list of compiled C# files
			foreach (string finalFilePath in directFileCopies.Values.Where<string>(f => f.EndsWith(".cs")))
			{
				compileTargets.Add(finalFilePath.Replace('/', '\\'));
			}

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

			// Add files for specific C# platform
			this.PlatformSpecificFiles(projectId, compileTargets, output, replacements);

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

			// Create the actual compile targets in the .csproj file
			List<string> compileTargetCode = new List<string>();
			foreach (string compileTarget in compileTargets)
			{
				compileTargetCode.Add("    <Compile Include=\"" + compileTarget.Replace('/', '\\') + "\" />\r\n");
			}
			compileTargetCode.Add("    <EmbeddedResource Include=\"ByteCode.txt\" />\r\n");
			foreach (string embeddedResource in embeddedResources)
			{
				compileTargetCode.Add("    <EmbeddedResource Include=\"" + embeddedResource.Replace('/', '\\') + "\" />\r\n");
			}

			foreach (string spriteSheetImage in spriteSheet.FinalPaths)
			{
				// TODO: need a better system of putting things in predefined destinations, rather than hacking it between states
				// in this fashion.
				string path = spriteSheetImage.Substring("%PROJECT_ID%".Length + 1).Replace('/', '\\');
				compileTargetCode.Add("    <EmbeddedResource Include=\"" + path + "\" />\r\n");
			}

			replacements["COMPILE_TARGETS"] = string.Join("", compileTargetCode);

			// Copy templated files over with proper replacements
			foreach (string templateFile in directFileCopies.Keys)
			{
				string finalFilePath = directFileCopies[templateFile];
				string outputFilePath = finalFilePath.EndsWith(".sln") ? finalFilePath : (projectId + "/" + finalFilePath);
				output[outputFilePath] = new FileOutput()
				{
					Type = FileOutputType.Text,
					TextContent = Util.MassReplacements(
						Util.ReadFileInternally("Translator/CSharp/Project/" + templateFile),
						replacements)
				};
			}

			output[projectId + "/ByteCode.txt"] = new FileOutput()
			{
				Type = FileOutputType.Text,
				TextContent = this.Context.ByteCodeString
			};

			return output;
		}

		private List<string> GetEmbeddedResources(string projectId, Dictionary<string, FileOutput> output, List<string> filesToCopyOver)
		{
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
			return embeddedResources;
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
