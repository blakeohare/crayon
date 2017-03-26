using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common;
using Pastel.Nodes;
using Platform;

namespace LangJava
{
    public class PlatformImpl : AbstractPlatform
    {
        public override string Name { get { return "lang-java"; } }
        public override string InheritsFrom { get { return null; } }
        public override string NL { get { return "\n"; } }

        public override Dictionary<string, FileOutput> ExportProject(IList<VariableDeclaration> globals, IList<StructDefinition> structDefinitions, IList<FunctionDefinition> functionDefinitions, IList<LibraryForExport> libraries, ResourceDatabase resourceDatabase, Options options, ILibraryNativeInvocationTranslatorProvider libraryNativeInvocationTranslatorProviderForPlatform)
        {
            throw new NotImplementedException();
        }

        public override string GenerateCodeForFunction(AbstractTranslator translator, FunctionDefinition funcDef)
        {
            throw new NotImplementedException();
        }

        public override string GenerateCodeForGlobalsDefinitions(AbstractTranslator translator, IList<VariableDeclaration> globals)
        {
            throw new NotImplementedException();
        }

        public override string GenerateCodeForStruct(StructDefinition structDef)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("public final class ");
            sb.Append(structDef.NameToken.Value);
            sb.Append(" {");
            sb.Append(this.NL);
            string[] types = structDef.ArgTypes.Select(type => this.TranslateType(type)).ToArray();
            string[] names = structDef.ArgNames.Select(token => token.Value).ToArray();
            int fieldCount = names.Length;
            for (int i = 0; i < fieldCount; ++i)
            {
                sb.Append("  public ");
                sb.Append(types[i]);
                sb.Append(' ');
                sb.Append(names[i]);
                sb.Append(';');
                sb.Append(this.NL);
            }
            sb.Append(this.NL);
            sb.Append("  public ");
            sb.Append(structDef.NameToken.Value);
            sb.Append('(');
            for (int i = 0; i < fieldCount; ++i)
            {
                if (i > 0) sb.Append(", ");
                sb.Append(types[i]);
                sb.Append(' ');
                sb.Append(names[i]);
            }
            sb.Append(") {");
            sb.Append(this.NL);
            for (int i = 0; i < fieldCount; ++i)
            {
                sb.Append("    this.");
                sb.Append(names[i]);
                sb.Append(" = ");
                sb.Append(names[i]);
                sb.Append(';');
                sb.Append(this.NL);
            }
            sb.Append("  }");
            sb.Append(this.NL);
            sb.Append("}");
            return sb.ToString();
        }

        public override Dictionary<string, string> GenerateReplacementDictionary(Options options, ResourceDatabase resDb)
        {
            return new Dictionary<string, string>();
        }

        public override IDictionary<string, object> GetConstantFlags()
        {
            return new Dictionary<string, object>();
        }

        public override string TranslateType(PType type)
        {
            switch (type.RootValue)
            {
                case "int": return "int";
                case "double": return "double";
                case "bool": return "boolean";
                case "object": return "Object";
                case "string": return "String";

                case "Array":
                    string innerType = this.TranslateType(type.Generics[0]);
                    return innerType + "[]";

                case "List":
                    return "ArrayList<" + this.TranslateInnerType(type.Generics[0]) + ">";

                case "Dictionary":
                    return "HashMap<" + this.TranslateInnerType(type.Generics[0]) + ", " + this.TranslateInnerType(type.Generics[1]) + ">";

                default:
                    char firstChar = type.RootValue[0];
                    if (firstChar >= 'A' && firstChar <= 'Z')
                    {
                        return type.RootValue;
                    }
                    throw new NotImplementedException();
            }
        }

        private string TranslateInnerType(PType type)
        {
            switch (type.RootValue)
            {
                case "int": return "Integer";
                case "double": return "Double";
                case "bool": return "Boolean";
                default:
                    return this.TranslateType(type);
            }
        }
    }
}
