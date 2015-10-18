using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crayon.ParseTree;

namespace Crayon.Translator.Java
{
	internal abstract class JavaPlatform : AbstractPlatform
	{
		public override bool IsAsync { get { return true; } }
		public override bool SupportsListClear { get { return true; } }
		public override bool IsStronglyTyped { get { return true; } }
		public override bool IntIsFloor { get { return false; } }
		public override bool ImagesLoadInstantly { get { return true; } }
		public override bool ScreenBlocksExecution { get { return true; } }
		public override bool UseFixedListArgConstruction { get { return true; } }

		public JavaPlatform(JavaSystemFunctionTranslator systemFunctionTranslator, AbstractOpenGlTranslator openGlTranslator)
			: base(false, new JavaTranslator(), systemFunctionTranslator, openGlTranslator, null)
		{ }

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
