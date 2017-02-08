using System;
using System.Collections.Generic;
using Common;
using Platform;

namespace GameCSharpOpenTk
{
    public class PlatformImpl : AbstractPlatform
    {
        public override string Name { get { return "game-csharp-opentk-cbx"; } }
        public override string InheritsFrom { get { return "lang-csharp"; } }

        public override Dictionary<string, FileOutput> Export(
            Dictionary<string, object[]> executablesPerCompilationUnit,
            object[] structDefinitions)
        {
            throw new NotImplementedException();
        }

        public override IDictionary<string, object> GetConstantFlags()
        {
            return new Dictionary<string, object>();
        }

        public override Dictionary<string, FileOutput> ExportProject(
            IList<Pastel.Nodes.VariableDeclaration> globals,
            IList<Pastel.Nodes.StructDefinition> structDefinitions,
            IList<Pastel.Nodes.FunctionDefinition> functionDefinitions,
            Dictionary<ExportOptionKey, object> options)
        {
            Dictionary<string, FileOutput> output = new Dictionary<string, FileOutput>();
            string projectId = options[ExportOptionKey.PROJECT_ID].ToString();
            string baseDir = projectId + "/";

            output[projectId + ".sln"] = new FileOutput()
            {
                Type = FileOutputType.Text,
                TextContent = "SLN file.",
            };

            output[baseDir + "Interpreter.csproj"] = new FileOutput()
            {
                Type = FileOutputType.Text,
                TextContent = "CSPROJ file.",
            };

            foreach (Pastel.Nodes.StructDefinition structDefinition in structDefinitions)
            {
                List<string> builder = new List<string>()
                {
                    "using System;",
                    "",
                    "namespace Interpreter.Structs",
                    "{",
                    "\tpublic class " + structDefinition.NameToken.Value,
                    "\t{",
                };

                for (int i = 0; i < structDefinition.ArgNames.Length; ++i)
                {
                    builder.Add("\t\tpublic object " + structDefinition.ArgNames[i].Value + ";");
                }

                builder.Add("\t}");
                builder.Add("}");
                builder.Add("");

                output[baseDir + "Structs/" + structDefinition.NameToken.Value + ".cs"] = new FileOutput()
                {
                    Type = FileOutputType.Text,
                    TextContent = string.Join("\n", builder)
                };
            }

            return output;
        }
    }
}
