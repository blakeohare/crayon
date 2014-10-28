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
				TextContent = Util.MassReplacements(string.Join("", crayonWrapper), replacements)
			};

			return output;
		}

		public string TranslateType(string original, bool wrapped)
		{
			switch (original) {
				case "string": return "String";
				case "bool": return wrapped ? "Boolean" : "boolean";
				case "int": return wrapped ? "Integer" : "int";
				case "char": return wrapped ? "Char" : "char";
				case "List": return "ArrayList";
				case "Dictionary": return "HashMap";
				default: return original;
			}
		}

		public string GetTypeStringFromString(string rawValue, bool wrappedContext)
		{
			return this.GetTypeStringFromAnnotation(null, rawValue, wrappedContext);
		}

		public string GetTypeStringFromAnnotation(Annotation annotation, bool wrappedContext)
		{
			return GetTypeStringFromAnnotation(annotation.FirstToken, annotation.GetSingleArgAsString(null), wrappedContext);
		}

		public string GetTypeStringFromAnnotation(Token stringToken, string value, bool wrappedContext)
		{
			AnnotatedType type = new AnnotatedType(stringToken, Tokenizer.Tokenize("type proxy", value, -1, false));
			return GetTypeStringFromAnnotation(type, wrappedContext);
		}

		private string GetTypeStringFromAnnotation(AnnotatedType type, bool wrappedContext)
		{
			string output;

			if (type.Name == "Array")
			{
				output = this.GetTypeStringFromAnnotation(type.Generics[0], false);
				output += "[]";
			}
			else
			{
				output = TranslateType(type.Name, wrappedContext);
				if (type.Generics.Length > 0)
				{
					output += "<";
					for (int i = 0; i < type.Generics.Length; ++i)
					{
						if (i > 0) output += ", ";
						output += this.GetTypeStringFromAnnotation(type.Generics[i], true);
					}
					output += ">";
				}
			}
			return output;
		}
	}
}
