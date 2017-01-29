using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pastel.Nodes;
using Common;

namespace Pastel
{
    public class PastelCompiler
    {
        public PastelCompiler(IDictionary<string, bool> constants, IInlineImportCodeLoader inlineImportCodeLoader)
        {
            this.interpreterParser = new PastelParser(constants, inlineImportCodeLoader);
        }

        private PastelParser interpreterParser;

        private Dictionary<string, StructDefinition> structDefinitions = new Dictionary<string, StructDefinition>();
        private Dictionary<string, Assignment> globals = new Dictionary<string, Assignment>();
        private Dictionary<string, FunctionDefinition> functionDefinitions = new Dictionary<string, FunctionDefinition>();
        
        public void CompileBlobOfCode(string name, string code)
        {
            ICompilationEntity[] entities = this.interpreterParser.ParseText(name, code);
            foreach (ICompilationEntity entity in entities)
            {
                switch (entity.EntityType)
                {
                    case CompilationEntityType.FUNCTION:
                        FunctionDefinition fnDef = (FunctionDefinition)entity;
                        string functionName = fnDef.NameToken.Value;
                        if (functionDefinitions.ContainsKey(functionName))
                        {
                            throw new ParserException(fnDef.FirstToken, "Multiple definitions of function: '" + functionName + "'");
                        }
                        functionDefinitions[functionName] = fnDef;
                        break;

                    default:
                        throw new NotImplementedException();
                }
            }
        }
    }
}
