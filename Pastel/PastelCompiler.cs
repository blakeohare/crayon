using System;
using System.Collections.Generic;
using System.Linq;
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
        private Dictionary<string, EnumDefinition> enumDefinitions = new Dictionary<string, EnumDefinition>();
        private Dictionary<string, Assignment> globals = new Dictionary<string, Assignment>();
        private Dictionary<string, Assignment> constants = new Dictionary<string, Assignment>();
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

                    case CompilationEntityType.ENUM:
                        EnumDefinition enumDef = (EnumDefinition)entity;
                        string enumName = enumDef.NameToken.Value;
                        if (enumDefinitions.ContainsKey(enumName))
                        {
                            throw new ParserException(enumDef.FirstToken, "Multiple definitions of function: '" + enumName + "'");
                        }
                        enumDefinitions[enumName] = enumDef;
                        break;

                    case CompilationEntityType.CONSTANT:
                    case CompilationEntityType.GLOBAL:
                        Assignment assignment = (Assignment)entity;
                        string targetName = assignment.Target.FirstToken.Value;
                        Dictionary<string, Assignment> lookup = entity.EntityType == CompilationEntityType.CONSTANT
                            ? constants
                            : globals;
                        if (lookup.ContainsKey(targetName))
                        {
                            throw new ParserException(
                                assignment.FirstToken,
                                "Multiple definitions of : '" + targetName + "'");
                        }
                        lookup[targetName] = assignment;
                        break;

                    default:
                        throw new NotImplementedException();
                }
            }
        }
    }
}
