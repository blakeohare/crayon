using System;
using System.Collections.Generic;
using System.Linq;
using Crayon.ParseTree;

namespace Crayon.Translator.CSharp
{
	abstract class CSharpPlatform : AbstractPlatform
	{
		public CSharpPlatform(CSharpSystemFunctionTranslator systemFunctionTranslator, Crayon.Translator.AbstractOpenGlTranslator openGlTranslator)
			: base(PlatformId.CSHARP_OPENTK, LanguageId.CSHARP, false, new CSharpTranslator(), systemFunctionTranslator, openGlTranslator)
		{ }

		public override bool IsAsync { get { return true; } }
		public override bool SupportsListClear { get { return true; } }
		public override bool IsStronglyTyped { get { return true; } }
		public override bool ImagesLoadInstantly { get { return true; } }
		public override string GeneratedFilesFolder { get { return "%PROJECT_ID%/GeneratedFiles"; } }
		public override bool IsArraySameAsList { get { return false; } }

		public abstract void PlatformSpecificFiles(
			string projectId,
			List<string> compileTargets,
			Dictionary<string, FileOutput> files,
			Dictionary<string, string> replacements,
            SpriteSheetBuilder spriteSheet);

		public abstract void ApplyPlatformSpecificReplacements(Dictionary<string, string> replacements);
		public abstract void AddPlatformSpecificSystemLibraries(HashSet<string> systemLibraries);
        
        private static string GetGuid(string seed, string salt)
        {
            seed = seed ?? (DateTime.Now.Ticks.ToString() + new Random().NextDouble());

            byte[] seedBytes = (seed + salt).ToCharArray().Select<char, byte>(c => (byte) c).ToArray();
            byte[] hash = System.Security.Cryptography.SHA1.Create().ComputeHash(seedBytes);

            List<string> output = new List<string>();
            for (int i = 0; i < 16; ++i)
            {
                if (i == 4 || i == 6 || i == 8 || i == 10)
                {
                    output.Add("-");
                }
                char a = "0123456789ABCDEF"[hash[i] & 15];
                char b = "0123456789ABCDEF"[hash[i] >> 4];
                output.Add("" + a + b);
            }
            return string.Join("", output);
        }

        protected virtual List<string> FilterEmbeddedResources(List<string> embeddedResources)
        {
            // Override this no-op method if necessary.

            // Namely this was added so that Xamarin projects (where an embedded resource compilation action makes little sense)
            // can take things out of here and put them in their respective platform-specific destination.
            return embeddedResources;
        }

        public override Dictionary<string, FileOutput> Package(
			BuildContext buildContext,
			string projectId,
			Dictionary<string, Executable[]> finalCode,
			List<string> filesToCopyOver,
			ICollection<StructDefinition> structDefinitions,
			string inputFolder,
			SpriteSheetBuilder spriteSheet)
		{
			Dictionary<string, string> replacements = new Dictionary<string, string>() {
				{ "PROJECT_GUID", GetGuid(buildContext.GuidSeed, "@@project").ToUpper() },
				{ "ASSEMBLY_GUID", GetGuid(buildContext.GuidSeed, "@@assembly").ToLower() },
				{ "PROJECT_TITLE", projectId },
				{ "PROJECT_ID", projectId },
				{ "CURRENT_YEAR", DateTime.Now.Year.ToString() },
				{ "COPYRIGHT", "©" },
				{ "EXTRA_DLLS", "" },
				{ "PROJECT_FILE_EXTRA", "" },
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

            embeddedResources = this.FilterEmbeddedResources(embeddedResources);

			// Code files that are templated
			Dictionary<string, string> directFileCopies = new Dictionary<string, string>()
			{
				{ "SolutionFile.txt", projectId + ".sln" },
				{ "ProjectFile.txt", projectId + ".csproj" },
				{ "JsonParser.txt", "JsonParser.cs" },
				{ "TranslationHelper.txt", "TranslationHelper.cs" },
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

			List<string> compileTargetCode = new List<string>();

			compileTargetCode.Add("    \r\n");
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
					TextContent = Constants.DoReplacements(
						Util.ReadResourceFileInternally("csharp-common/" + templateFile),
						replacements)
				};
			}

			// Add files for specific C# platform
			this.PlatformSpecificFiles(projectId, compileTargets, output, replacements, spriteSheet);

            this.ApplyPlatformSpecificOverrides(projectId, output);
            
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
			AnnotatedType type = new AnnotatedType(stringToken, new TokenStream(Tokenizer.Tokenize("type proxy", value, -1, false)));
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
				output = type.Name;
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

        public virtual void ApplyPlatformSpecificOverrides(string projectId, Dictionary<string, FileOutput> files) { }
    }
}
