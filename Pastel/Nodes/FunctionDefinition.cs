using System.Collections.Generic;
using System.Linq;

namespace Pastel.Nodes
{
    public class FunctionDefinition : ICompilationEntity
    {
        public CompilationEntityType EntityType { get { return CompilationEntityType.FUNCTION; } }

        public Token FirstToken { get; set; }
        public PType ReturnType { get; set; }
        public Token NameToken { get; set; }
        public PType[] ArgTypes { get; set; }
        public Token[] ArgNames { get; set; }
        public Executable[] Code { get; set; }

        public FunctionDefinition(
            Token nameToken,
            PType returnType,
            IList<PType> argTypes, 
            IList<Token> argNames, 
            IList<Executable> code) 
        {
            this.FirstToken = ReturnType.FirstToken;
            this.NameToken = nameToken;
            this.ReturnType = returnType;
            this.ArgTypes = argTypes.ToArray();
            this.ArgNames = argNames.ToArray();
            this.Code = code.ToArray();
        }
    }
}
