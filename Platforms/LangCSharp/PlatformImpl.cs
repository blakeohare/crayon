using System;
using System.Collections.Generic;
using System.Text;
using Common;
using Pastel.Nodes;
using Platform;

namespace LangCSharp
{
    public class PlatformImpl : AbstractPlatform
    {
        public override string Name { get { return "lang-csharp"; } }
        public override string InheritsFrom { get { return null; } }
        public override string NL { get { return "\r\n"; } }

        public override Dictionary<string, FileOutput> Export(
            Dictionary<string, object[]> executablesPerCompilationUnit,
            object[] structDefinitions)
        {
            throw new NotImplementedException();
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

        public override string TranslateType(Pastel.Nodes.PType type)
        {
            switch (type.RootValue)
            {
                case "int":
                case "char":
                case "bool":
                case "double":
                case "string":
                case "object":
                case "void":
                    return type.RootValue;

                case "List":
                    return "List<" + this.TranslateType(type.Generics[0]) + ">";

                case "Dictionary":
                    return "Dictionary<" + this.TranslateType(type.Generics[0]) + ", " + this.TranslateType(type.Generics[1]) + ">";

                case "Array":
                    return this.TranslateType(type.Generics[0]) + "[]";

                default:
                    if (type.Generics.Length > 0)
                    {
                        throw new NotImplementedException();
                    }
                    return type.RootValue;
            }
        }

        public override Dictionary<string, FileOutput> ExportProject(
            IList<VariableDeclaration> globals,
            IList<StructDefinition> structDefinitions,
            IList<FunctionDefinition> functionDefinitions,
            Options options)
        {
            throw new InvalidOperationException("This platform does not support direct export.");
        }

        public override string GenerateCodeForStruct(StructDefinition structDef)
        {
            Pastel.Nodes.PType[] types = structDef.ArgTypes;
            Pastel.Token[] fieldNames = structDef.ArgNames;
            string name = structDef.NameToken.Value;
            List<string> lines = new List<string>();

            lines.Add("public class " + name);
            lines.Add("{");
            for (int i = 0; i < types.Length; ++i)
            {
                lines.Add("    public " + this.TranslateType(types[i]) + " " + fieldNames[i].Value + ";");
            }
            lines.Add("");

            StringBuilder constructorDeclaration = new StringBuilder();
            constructorDeclaration.Append("    public ");
            constructorDeclaration.Append(name);
            constructorDeclaration.Append('(');
            for (int i = 0; i < types.Length; ++i)
            {
                if (i > 0) constructorDeclaration.Append(", ");
                constructorDeclaration.Append(this.TranslateType(types[i]));
                constructorDeclaration.Append(' ');
                constructorDeclaration.Append(fieldNames[i].Value);
            }
            constructorDeclaration.Append(')');
            lines.Add(constructorDeclaration.ToString());
            lines.Add("    {");
            for (int i = 0; i < types.Length; ++i)
            {
                string fieldName = fieldNames[i].Value;
                lines.Add("        this." + fieldName + " = " + fieldName + ";");
            }
            lines.Add("    }");

            lines.Add("}");
            lines.Add("");

            return string.Join("\r\n", lines);
        }

        public override string GenerateCodeForFunction(AbstractTranslator translator, FunctionDefinition funcDef)
        {
            StringBuilder output = new StringBuilder();
            PType returnType = funcDef.ReturnType;
            string funcName = funcDef.NameToken.Value;
            PType[] argTypes = funcDef.ArgTypes;
            Pastel.Token[] argNames = funcDef.ArgNames;

            output.Append("public static ");
            output.Append(this.TranslateType(returnType));
            output.Append(" v_");
            output.Append(funcName);
            output.Append("(");
            for (int i = 0; i < argTypes.Length; ++i)
            {
                if (i > 0) output.Append(", ");
                output.Append(this.TranslateType(argTypes[i]));
                output.Append(" ");
                output.Append(argNames[i].Value);
            }
            output.Append(")");
            output.Append(this.NL);
            output.Append("{");
            output.Append(this.NL);
            translator.TabDepth = 1;
            translator.TranslateExecutables(output, funcDef.Code);
            translator.TabDepth = 0;
            output.Append("}");

            return string.Join("", output);
        }

        public override Dictionary<string, string> GenerateReplacementDictionary(Options options)
        {
            Dictionary<string, string> replacements = AbstractPlatform.GenerateGeneralReplacementsDictionary(options);
            replacements["PROJECT_GUID"] = "project guid goes here.";
            return replacements;
        }
    }
}
