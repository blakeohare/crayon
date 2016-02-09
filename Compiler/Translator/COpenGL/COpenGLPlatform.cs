using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crayon.ParseTree;

namespace Crayon.Translator.COpenGL
{
	class COpenGLPlatform : AbstractPlatform
	{
		public COpenGLPlatform()
			: base(LibraryConfig.PlatformId.C_OPENGL, LibraryConfig.LanguageId.C, false, new CTranslator(), new COpenGLSystemFunctionTranslator(), new COpenGlOpenGlTranslator(), null)
		{ }

		public override bool IsAsync { get { return false; } }
		public override bool SupportsListClear { get { return true; } }
		public override bool IsStronglyTyped { get { return true; } }
		public override bool ImagesLoadInstantly { get { return true; } }
		public override bool IsArraySameAsList { get { return false; } }
		public override string GeneratedFilesFolder { get { return "generated_files"; } }
		public override string PlatformShortId { get { return "copengl"; } }

		public override Dictionary<string, FileOutput> Package(BuildContext buildContext, string projectId, Dictionary<string, ParseTree.Executable[]> finalCode, List<string> filesToCopyOver, ICollection<ParseTree.StructDefinition> structDefinitions, string fileCopySourceRoot, SpriteSheetBuilder spriteSheet)
		{
			Dictionary<string, string> mainFile = new Dictionary<string, string>();
			
			string nl = this.Translator.NL;

			List<string> structSection = new List<string>();
			foreach (StructDefinition structDefinition in structDefinitions)
			{
				string name = structDefinition.Name.Value;
				structSection.Add("typedef struct " + name + " {");
				for (int i = 0; i < structDefinition.Fields.Length; ++i)
				{
					string fieldName = structDefinition.FieldsByIndex[i];
					Annotation type = structDefinition.Types[i];
					string typeValue = ((StringConstant)type.Args[0]).Value;
					string typeString = this.GetTypeStringFromAnnotation(structDefinition.Fields[i], typeValue, false, false);
					structSection.Add("\t" + typeString + " " + fieldName + "; /* " + typeValue + " */");
				}
				structSection.Add("} " + name + ";");
				structSection.Add("");
			}

			mainFile["structs"] = string.Join("\n", structSection);

			Dictionary<string, FileOutput> output = new Dictionary<string, FileOutput>();

			List<string> mainC = new List<string>();
			mainC.Add(Util.ReadFileInternally("Translator/COpenGL/Project/Header.c"));
			mainC.Add(mainFile["structs"]);
			mainC.Add(this.Translator.NL);
			this.Translator.TranslateGlobals(mainC, finalCode);
			mainC.Add(this.Translator.NL);
			this.Translator.TranslateFunctions(mainC, finalCode);
			mainC.Add(this.Translator.NL);
			mainC.Add(Util.ReadFileInternally("Translator/COpenGL/Project/Footer.c"));

			output["main.c"] = new FileOutput()
			{
				Type = FileOutputType.Text,
				TextContent = string.Join("", mainC)
			};

			return output;
		}

		public string GetTypeStringFromAnnotation(Token stringToken, string value, bool wrappedContext, bool dropGenerics)
		{
			AnnotatedType type = new AnnotatedType(stringToken, Tokenizer.Tokenize("type proxy", value, -1, false));
			return GetTypeStringFromAnnotation(type, wrappedContext, dropGenerics);
		}
		
		public string GetTypeStringFromAnnotation(AnnotatedType type, bool wrappedContext, bool dropGenerics)
		{
			string output;

			if (type.Name == "Array")
			{
				output = this.GetTypeStringFromAnnotation(type.Generics[0], false, dropGenerics);
				output += "*";
			}
			else
			{
				// TODO: there's going to have to be massive changes here. 
				// I'm likely just going to autogenerate all the generic types used as separate types.
				if (type.Generics.Length == 0)
				{
					output = TranslateType(type.Name);
				}
				else
				{
					output = "void*";
				}
			}
			return output;
		}

		public override string TranslateType(string original)
		{
			switch (original)
			{
				case "string": return "XString*";
				case "bool": return "int";
				case "int": return "int";
				case "char": return "char";
				case "object": return "void*";
				case "List": return "XList*";
				case "Dictionary": return "XDictionary*";
				default: return original;
			}
		}
	}
}
