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
            this.FirstToken = returnType.FirstToken;
            this.NameToken = nameToken;
            this.ReturnType = returnType;
            this.ArgTypes = argTypes.ToArray();
            this.ArgNames = argNames.ToArray();
            this.Code = code.ToArray();
        }

        public void ResolveNamesAndCullUnusedCode(PastelCompiler compiler)
        {
            this.Code = Executable.ResolveNamesAndCullUnusedCodeForBlock(this.Code, compiler).ToArray();
        }

        public void ResolveTypes(PastelCompiler compiler)
        {
            Dictionary<string, VariableDeclaration> globals = compiler.SharedScope == null ? compiler.Globals : compiler.SharedScope.Globals;

            VariableScope varScope = new VariableScope(this, globals);
            for (int i = 0; i < this.ArgTypes.Length; ++i)
            {
                varScope.DeclareVariables(this.ArgNames[i], this.ArgTypes[i]);
            }

            for (int i = 0; i < this.Code.Length; ++i)
            {
                this.Code[i].ResolveTypes(varScope, compiler);
            }
        }

        public void ResolveWithTypeContext(PastelCompiler compiler)
        {
            Executable.ResolveWithTypeContext(compiler, this.Code);
        }
    }
}
