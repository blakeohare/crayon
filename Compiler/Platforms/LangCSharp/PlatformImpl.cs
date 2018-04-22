using Common;
using Pastel.Nodes;
using Pastel.Transpilers;
using Platform;
using System;
using System.Collections.Generic;
using System.Text;

namespace LangCSharp
{
    public class PlatformImpl : AbstractPlatform
    {
        public override string Name { get { return "lang-csharp"; } }
        public override string InheritsFrom { get { return null; } }
        public override string NL { get { return "\r\n"; } }

        public PlatformImpl() : base()
        {
            this.ContextFreePlatformImpl = new ContextFreeLangCSharpPlatform();
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
                    { "HAS_INCREMENT", true },
                };
        }

        public override void ExportStandaloneVm(
            Dictionary<string, FileOutput> output,
            IList<VariableDeclaration> globals,
            IList<StructDefinition> structDefinitions,
            IList<FunctionDefinition> functionDefinitions,
            IList<LibraryForExport> everyLibrary,
            ILibraryNativeInvocationTranslatorProvider libraryNativeInvocationTranslatorProviderForPlatform)
        {
            throw new NotImplementedException();
        }

        public override void ExportProject(
            Dictionary<string, FileOutput> output,
            IList<VariableDeclaration> globals,
            IList<StructDefinition> structDefinitions,
            IList<FunctionDefinition> functionDefinitions,
            IList<LibraryForExport> libraries,
            ResourceDatabase resourceDatabase,
            Options options,
            ILibraryNativeInvocationTranslatorProvider libraryNativeInvocationTranslatorProviderForPlatform)
        {
            throw new InvalidOperationException("This platform does not support direct export.");
        }

        public override void GenerateCodeForStruct(TranspilerContext sb, AbstractTranslator translator, StructDefinition structDef)
        {
            PType[] types = structDef.ArgTypes;
            Pastel.Token[] fieldNames = structDef.ArgNames;
            string name = structDef.NameToken.Value;
            List<string> lines = new List<string>();

            lines.Add("public class " + name);
            lines.Add("{");
            for (int i = 0; i < types.Length; ++i)
            {
                lines.Add("    public " + translator.TranslateType(types[i]) + " " + fieldNames[i].Value + ";");
            }
            lines.Add("");

            StringBuilder constructorDeclaration = new StringBuilder();
            constructorDeclaration.Append("    public ");
            constructorDeclaration.Append(name);
            constructorDeclaration.Append('(');
            for (int i = 0; i < types.Length; ++i)
            {
                if (i > 0) constructorDeclaration.Append(", ");
                constructorDeclaration.Append(translator.TranslateType(types[i]));
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

            // TODO: rewrite this function to use the string builder inline and use this.NL
            sb.Append(string.Join("\r\n", lines));
        }

        public override void GenerateCodeForFunction(TranspilerContext output, AbstractTranslator translator, FunctionDefinition funcDef)
        {
            PType returnType = funcDef.ReturnType;
            string funcName = funcDef.NameToken.Value;
            PType[] argTypes = funcDef.ArgTypes;
            Pastel.Token[] argNames = funcDef.ArgNames;

            output.Append("public static ");
            output.Append(translator.TranslateType(returnType));
            output.Append(" v_");
            output.Append(funcName);
            output.Append("(");
            for (int i = 0; i < argTypes.Length; ++i)
            {
                if (i > 0) output.Append(", ");
                output.Append(translator.TranslateType(argTypes[i]));
                output.Append(" v_");
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
        }

        public override Dictionary<string, string> GenerateReplacementDictionary(Options options, ResourceDatabase resDb)
        {
            Dictionary<string, string> replacements = AbstractPlatform.GenerateGeneralReplacementsDictionary(options);
            replacements["PROJECT_GUID"] = "project guid goes here.";
            return replacements;
        }

        public override void GenerateCodeForGlobalsDefinitions(TranspilerContext output, AbstractTranslator translator, IList<VariableDeclaration> globals)
        {
            output.Append("    public static class Globals");
            output.Append(this.NL);
            output.Append("    {");
            output.Append(this.NL);
            translator.TabDepth = 0;
            foreach (VariableDeclaration vd in globals)
            {
                output.Append("        public static ");
                translator.TranslateVariableDeclaration(output, vd);
            }
            output.Append("    }");
        }
    }
}
