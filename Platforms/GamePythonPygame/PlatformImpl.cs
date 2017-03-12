using System;
using System.Collections.Generic;
using Common;
using Pastel.Nodes;
using Platform;
using System.Text;

namespace GamePythonPygame
{
    public class PlatformImpl : AbstractPlatform
    {
        public override string Name { get { return "game-python-pygame-cbx"; } }
        public override string InheritsFrom { get { return "lang-python"; } }
        public override string NL { get { return "\n"; } }

        public PlatformImpl()
        {
            this.Translator = new PythonPygameTranslator(this);
        }

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
            IList<VariableDeclaration> globals,
            IList<StructDefinition> structDefinitions,
            IList<FunctionDefinition> functionDefinitions,
            IList<LibraryForExport> libraries,
            ResourceDatabase resourceDatabase,
            Options options,
            ILibraryNativeInvocationTranslatorProvider libraryNativeInvocationTranslatorProviderForPlatform)
        {
            Dictionary<string, FileOutput> output = new Dictionary<string, FileOutput>();

            List<string> runPy = new List<string>();

            runPy.Add(this.GenerateCodeForGlobalsDefinitions(this.Translator, globals));

            runPy.Add("");
            runPy.Add("v_main()");
            runPy.Add("");

            foreach (FunctionDefinition funcDef in functionDefinitions)
            {
                runPy.Add(this.GenerateCodeForFunction(this.Translator, funcDef));
            }

            output["run.py"] = new FileOutput()
            {
                Type = FileOutputType.Text,
                TextContent = string.Join("\n", runPy),
            };
            return output;
        }

        public override string GenerateCodeForFunction(AbstractTranslator translator, FunctionDefinition funcDef)
        {
            return this.ParentPlatform.GenerateCodeForFunction(this.Translator, funcDef);
        }

        public override string GenerateCodeForStruct(StructDefinition structDef)
        {
            return this.ParentPlatform.GenerateCodeForStruct(structDef);
        }

        public override Dictionary<string, string> GenerateReplacementDictionary(Options options, ResourceDatabase resDb)
        {
            return this.ParentPlatform.GenerateReplacementDictionary(options, resDb);
        }

        public override string GenerateCodeForGlobalsDefinitions(AbstractTranslator translator, IList<VariableDeclaration> globals)
        {
            return this.ParentPlatform.GenerateCodeForGlobalsDefinitions(translator, globals);
        }
    }
}
