using System;
using System.Collections.Generic;
using System.Linq;

namespace Pastel.Nodes
{
    class IfStatement : Executable
    {
        public Expression Condition { get; set; }
        public Executable[] IfCode { get; set; }
        public Token ElseToken { get; set; }
        public Executable[] ElseCode { get; set; }

        public IfStatement(
            Token ifToken,
            Expression condition,
            IList<Executable> ifCode,
            Token elseToken,
            IList<Executable> elseCode) : base(ifToken)
        {
            this.Condition = condition;
            this.IfCode = ifCode.ToArray();
            this.ElseToken = elseToken;
            this.ElseCode = elseCode.ToArray();
        }

        public override IList<Executable> ResolveNamesAndCullUnusedCode(PastelCompiler compiler)
        {
            this.Condition = this.Condition.ResolveNamesAndCullUnusedCode(compiler);

            if (this.Condition is InlineConstant)
            {
                object value = ((InlineConstant)this.Condition).Value;
                if (value is bool)
                {
                    return Executable.ResolveNamesAndCullUnusedCodeForBlock(
                        ((bool)value) ? this.IfCode : this.ElseCode,
                        compiler);
                }
            }
            this.IfCode = Executable.ResolveNamesAndCullUnusedCodeForBlock(this.IfCode, compiler).ToArray();
            this.ElseCode = Executable.ResolveNamesAndCullUnusedCodeForBlock(this.ElseCode, compiler).ToArray();

            return Listify(this);
        }

        internal override void ResolveTypes(VariableScope varScope, PastelCompiler compiler)
        {
            this.Condition.ResolveType(varScope, compiler);
            if (this.Condition.ResolvedType.RootValue != "bool")
            {
                throw new ParserException(this.Condition.FirstToken, "Only booleans can be used in if statements.");
            }

            Executable.ResolveTypes(this.IfCode, new VariableScope(varScope), compiler);
            Executable.ResolveTypes(this.ElseCode, new VariableScope(varScope), compiler);
        }
    }
}
