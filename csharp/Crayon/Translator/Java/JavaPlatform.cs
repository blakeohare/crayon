using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crayon.ParseTree;

namespace Crayon.Translator.Java
{
	internal class JavaPlatform : AbstractPlatform
	{
		public override bool IsAsync { get { return true; } }
		public override string OutputFolderName { get { return "java"; } }
		public override bool SupportsListClear { get { return true; } }
		public override bool IsStronglyTyped { get { return true; } }
		public override bool IntIsFloor { get { return false; } }
		public override bool UseFixedListArgConstruction { get { return true; } }

		public JavaPlatform()
			: base(false, new JavaTranslator(), new JavaSystemFunctionTranslator())
		{ }

		public override Dictionary<string, FileOutput> Package(string projectId, Dictionary<string, ParseTree.Executable[]> finalCode, List<string> filesToCopyOver, ICollection<ParseTree.StructDefinition> structDefinitions)
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

			output["src/" + package + "/TranslationHelper.java"] = new FileOutput()
			{
				Type = FileOutputType.Text,
				TextContent = Util.MassReplacements(
					Util.ReadFileInternally("Translator/Java/Project/TranslationHelper.txt"),
					replacements)
			};

			output["src/" + package + "/Start.java"] = new FileOutput()
			{
				Type = FileOutputType.Text,
				TextContent = Util.MassReplacements(
					Util.ReadFileInternally("Translator/Java/Project/Start.txt"),
					replacements)
			};

			output["src/" + package + "/GameWindow.java"] = new FileOutput()
			{
				Type = FileOutputType.Text,
				TextContent = Util.MassReplacements(
					Util.ReadFileInternally("Translator/Java/Project/GameWindow.txt"),
					replacements)
			};

			output["src/" + package + "/RenderEngine.java"] = new FileOutput()
			{
				Type = FileOutputType.Text,
				TextContent = Util.MassReplacements(
					Util.ReadFileInternally("Translator/Java/Project/RenderEngine.txt"),
					replacements)
			};

			string[] items = finalCode.Keys.OrderBy<string, string>(s => s.ToLowerInvariant()).ToArray();

			List<string> crayonWrapper = new List<string>();
			crayonWrapper.Add(string.Join("\n",
				"package %%%PACKAGE%%%;",
				"",
				"import java.util.ArrayList;",
				"import java.util.HashMap;",
				"import java.util.Stack;",
				"import java.util.regex.Pattern;",
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
				
				output["src/" + package + "/" + filename] = new FileOutput()
				{
					Type = FileOutputType.Text,
					TextContent = fileContents
				};

				output["data/ByteCode.txt"] = new FileOutput()
				{
					Type = FileOutputType.Text,
					TextContent = this.Context.ByteCodeString
				};
			}

			return output;
		}

		public string TranslateType(string original, bool wrapped)
		{
			switch (original) {
				case "string": return "String";
				case "bool": return wrapped ? "Boolean" : "boolean";
				case "int": return wrapped ? "Integer" : "int";
				case "char": return wrapped ? "Character" : "char";
				case "object": return "Object";
				case "List": return "ArrayList";
				case "Dictionary": return "HashMap";
				default: return original;
			}
		}

		public string GetTypeStringFromString(string rawValue, bool wrappedContext, bool dropGenerics)
		{
			return this.GetTypeStringFromAnnotation(null, rawValue, wrappedContext, dropGenerics);
		}

		public string GetTypeStringFromAnnotation(Annotation annotation, bool wrappedContext, bool dropGenerics)
		{
			return GetTypeStringFromAnnotation(annotation.FirstToken, annotation.GetSingleArgAsString(null), wrappedContext, dropGenerics);
		}

		public string GetTypeStringFromAnnotation(Token stringToken, string value, bool wrappedContext, bool dropGenerics)
		{
			AnnotatedType type = new AnnotatedType(stringToken, Tokenizer.Tokenize("type proxy", value, -1, false));
			return GetTypeStringFromAnnotation(type, wrappedContext, dropGenerics);
		}

		private string GetTypeStringFromAnnotation(AnnotatedType type, bool wrappedContext, bool dropGenerics)
		{
			string output;

			if (type.Name == "Array")
			{
				output = this.GetTypeStringFromAnnotation(type.Generics[0], false, dropGenerics);
				output += "[]";
			}
			else
			{
				output = TranslateType(type.Name, wrappedContext);
				if (type.Generics.Length > 0 && !dropGenerics)
				{
					output += "<";
					for (int i = 0; i < type.Generics.Length; ++i)
					{
						if (i > 0) output += ", ";
						output += this.GetTypeStringFromAnnotation(type.Generics[i], true, false);
					}
					output += ">";
				}
			}
			return output;
		}
	}
}
