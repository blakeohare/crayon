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
            Dictionary<ExportOptionKey, object> options)
        {
            throw new InvalidOperationException("This platform does not support direct export.");
        }

        public string GenerateCodeForStruct(string structName, List<Pastel.Nodes.PType> types, List<string> fieldNames)
        {
            List<string> lines = new List<string>();

            lines.Add("using System;");
            lines.Add("");
            lines.Add("public class " + structName);
            lines.Add("{");
            for (int i = 0; i < types.Count; ++i)
            {
                lines.Add("\tpublic " + this.TranslateType(types[i]) + " " + fieldNames[i] + ";");
            }
            lines.Add("}");
            lines.Add("");

            return string.Join("\n", lines);
        }
    }
}
