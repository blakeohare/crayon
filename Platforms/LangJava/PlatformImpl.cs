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
            StringBuilder sb = new StringBuilder();

            sb.Append(translator.CurrentTab);
            sb.Append("public static ");
            sb.Append(this.TranslateType(funcDef.ReturnType));
            sb.Append(" v_");
            sb.Append(funcDef.NameToken.Value);
            sb.Append('(');
            Pastel.Token[] argNames = funcDef.ArgNames;
            PType[] argTypes = funcDef.ArgTypes;
            for (int i = 0; i < argTypes.Length; ++i)
            {
                if (i > 0) sb.Append(", ");
                sb.Append(this.TranslateType(argTypes[i]));
                sb.Append(" v_");
                sb.Append(argNames[i].Value);
            }
            sb.Append(") {");
            sb.Append(this.NL);
            translator.TabDepth++;
            translator.TranslateExecutables(sb, funcDef.Code);
            translator.TabDepth--;
            sb.Append(translator.CurrentTab);
            sb.Append('}');
            sb.Append(this.NL);

            return sb.ToString();
        }

        public override string GenerateCodeForGlobalsDefinitions(AbstractTranslator translator, IList<VariableDeclaration> globals)
        {
            List<string> lines = new List<string>()
            {
                "package org.crayonlang.interpreter;",
                "",
                "import java.util.HashMap;",
                "import org.crayonlang.interpreter.structs.Value;",
                "",
                "public final class VmGlobal {",
                "",
                "  private VmGlobal() {}",
                "",
            };
            foreach (VariableDeclaration varDecl in globals)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("  public static final ");
                sb.Append(this.TranslateType(varDecl.Type));
                sb.Append(' ');
                sb.Append(varDecl.VariableNameToken.Value);
                sb.Append(" = ");
                translator.TranslateExpression(sb, varDecl.Value);
                sb.Append(';');
                lines.Add(sb.ToString());
            }
            lines.Add("}");
            lines.Add("");
            return string.Join(this.NL, lines);
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
            string structCode = sb.ToString();

            List<string> structFileLines = new List<string>();
            structFileLines.Add("package org.crayonlang.interpreter.structs;");
            structFileLines.Add("");
            bool hasLists = structCode.Contains("public ArrayList<");
            bool hasDictionaries = structCode.Contains("public HashMap<");
            if (hasLists) structFileLines.Add("import java.util.ArrayList;");
            if (hasDictionaries) structFileLines.Add("import java.util.HashMap;");
            if (hasLists || hasDictionaries) structFileLines.Add("");
            structFileLines.Add(structCode);
            structFileLines.Add("");

            return string.Join(this.NL, structFileLines);
        }

        public override Dictionary<string, string> GenerateReplacementDictionary(Options options, ResourceDatabase resDb)
        {
            return new Dictionary<string, string>();
        }

        public override IDictionary<string, object> GetConstantFlags()
        {
            return new Dictionary<string, object>()
                {
                    { "IS_ASYNC", true },
                    { "PLATFORM_SUPPORTS_LIST_CLEAR", true },
                    { "STRONGLY_TYPED", true },
                    { "IS_ARRAY_SAME_AS_LIST", false },
                    { "IS_PYTHON", false },
                    { "IS_CHAR_A_NUMBER", true },
                    { "INT_IS_FLOOR", false },
                    { "IS_THREAD_BLOCKING_ALLOWED", true },
                };
        }

        public override string TranslateType(PType type)
        {
            return TranslateJavaType(type);
        }

        public static string TranslateJavaType(PType type)
        {
            switch (type.RootValue)
            {
                case "void": return "void";
                case "byte": return "byte";
                case "int": return "int";
                case "char": return "char";
                case "double": return "double";
                case "bool": return "boolean";
                case "object": return "Object";
                case "string": return "String";

                case "Array":
                    string innerType = TranslateJavaType(type.Generics[0]);
                    return innerType + "[]";

                case "List":
                    return "ArrayList<" + TranslateJavaNestedType(type.Generics[0]) + ">";

                case "Dictionary":
                    return "HashMap<" + TranslateJavaNestedType(type.Generics[0]) + ", " + TranslateJavaNestedType(type.Generics[1]) + ">";

                case "ClassValue":
                    // java.lang.ClassValue collision
                    return "org.crayonlang.interpreter.structs.ClassValue";

                default:
                    char firstChar = type.RootValue[0];
                    if (firstChar >= 'A' && firstChar <= 'Z')
                    {
                        return type.RootValue;
                    }
                    throw new NotImplementedException();
            }
        }

        public static string TranslateJavaNestedType(PType type)
        {
            switch (type.RootValue)
            {
                case "bool": return "Boolean";
                case "byte": return "Byte";
                case "char": return "Character";
                case "double": return "Double";
                case "int": return "Integer";
                default:
                    return TranslateJavaType(type);
            }
        }
    }
}
