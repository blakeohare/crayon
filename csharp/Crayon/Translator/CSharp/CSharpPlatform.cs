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
		{
		}

		public override bool IsAsync { get { return true; } }
		public override string OutputFolderName { get { return "csharpwindows"; } }

		public override Dictionary<string, FileOutput> Package(string projectId, Dictionary<string, Executable[]> finalCode, List<string> filesToCopyOver, ICollection<StructDefinition> structDefinitions)
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

			string crayonWrapperHeader = string.Join(this.Translator.NL, new string[] {
					"using System;",
					"using System.Collections.Generic;",
					"",
					"namespace " + projectId,
					"{",
					"\tinternal partial class CrayonWrapper",
					"\t{",
					""
				});
			string crayonWrapperFooter = "\t}" + this.Translator.NL + "}" + this.Translator.NL;

			string nl = this.Translator.NL;

			foreach (StructDefinition structDefinition in structDefinitions)
			{
				string structName = structDefinition.Name.Value;
				string filename = structName + ".cs";
				compileTargets.Add(filename);
				List<string> codeContents = new List<string>();
				codeContents.Add(crayonWrapperHeader);
				codeContents.Add("\t\tpublic class " + structName + nl);
				codeContents.Add("\t\t{" + nl);
				codeContents.Add("\t\t\tpublic " + structName + "(");
				for (int i = 0; i < structDefinition.FieldsByIndex.Length; ++i)
				{
					if (i > 0) codeContents.Add(", ");
					codeContents.Add("object v_" + structDefinition.FieldsByIndex[i]);
				}
				codeContents.Add(")" + nl);

				codeContents.Add("\t\t\t{" + nl);

				for (int i = 0; i < structDefinition.FieldsByIndex.Length; ++i)
				{
					codeContents.Add("\t\t\t\tthis." + structDefinition.FieldsByIndex[i] + " = v_" + structDefinition.FieldsByIndex[i] + ";" + nl);
				}

				codeContents.Add("\t\t\t}" + nl + nl);
				for (int i = 0; i < structDefinition.FieldsByIndex.Length; ++i)
				{
					codeContents.Add("\t\t\tpublic object " + structDefinition.FieldsByIndex[i] + ";" + nl);
				}

				codeContents.Add("\t\t}" + nl);

				codeContents.Add(crayonWrapperFooter);
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
			foreach (string embeddedResource in embeddedResources)
			{
				csprojFile.Add("    <EmbeddedResource Include=\"" + embeddedResource.Replace('/', '\\') + "\" />\r\n");
			}
			csprojFile.Add(Util.ReadFileInternally("Translator/CSharp/Project/ProjectFileFooter.txt"));

			output[projectId + "/" + projectId + ".csproj"] = new FileOutput()
			{
				Type = FileOutputType.Text,
				TextContent = string.Join("", csprojFile)
			};

			return output;
		}
	}
}
