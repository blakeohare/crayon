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
            throw new NotImplementedException();
        }

        public override Dictionary<string, string> GenerateReplacementDictionary(Options options, ResourceDatabase resDb)
        {
            return new Dictionary<string, string>();
        }

        public override IDictionary<string, object> GetConstantFlags()
        {
            return new Dictionary<string, object>();
        }
    }
}
