using System.Collections.Generic;
using System.Linq;

namespace Pastel.Nodes
{
    internal class WhileLoop : Executable
    {
        public Expression Condition { get; set; }
        public Executable[] Code { get; set; }

        public WhileLoop(
            Token whileToken,
            Expression condition,
            IList<Executable> code) : base(whileToken)
        {
            this.Condition = condition;
            this.Code = code.ToArray();
        }

        public override Executable ResolveNamesAndCullUnusedCode(PastelCompiler compiler)
        {
            this.Condition = this.Condition.ResolveNamesAndCullUnusedCode(compiler);
            this.Code = Executable.ResolveNamesAndCullUnusedCodeForBlock(this.Code, compiler).ToArray();
            return this;
        }

        internal override void ResolveTypes(VariableScope varScope, PastelCompiler compiler)
        {
            this.Condition = this.Condition.ResolveType(varScope, compiler);
            if (!this.Condition.ResolvedType.IsIdentical(compiler, PType.BOOL))
            {
                throw new ParserException(this.Condition.FirstToken, "While loop must have a boolean condition.");
            }

            Executable.ResolveTypes(this.Code, varScope, compiler);
        }

        internal override Executable ResolveWithTypeContext(PastelCompiler compiler)
        {
            this.Condition = this.Condition.ResolveWithTypeContext(compiler);
            Executable.ResolveWithTypeContext(compiler, this.Code);
            return this;
        }
    }
}
