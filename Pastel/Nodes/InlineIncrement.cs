using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pastel.Nodes
{
    class InlineIncrement : Expression
    {
        public Token IncrementToken { get; set; }
        public Expression Expression { get; set; }
        public bool IsPrefix { get; set; }

        public InlineIncrement(Token firstToken, Token incrementToken, Expression root, bool isPrefix) : base(firstToken)
        {
            this.IncrementToken = incrementToken;
            this.Expression = root;
            this.IsPrefix = isPrefix;
        }

        public override Expression ResolveNamesAndCullUnusedCode(PastelCompiler compiler)
        {
            this.Expression = this.Expression.ResolveNamesAndCullUnusedCode(compiler);
            return this;
        }

        internal override Expression ResolveType(VariableScope varScope, PastelCompiler compiler)
        {
            this.Expression.ResolveType(varScope, compiler);
            if (!this.Expression.ResolvedType.IsIdentical(PType.INT))
            {
                throw new ParserException(this.IncrementToken, "++ and -- can only be applied to integer types.");
            }
            this.ResolvedType = PType.INT;
            return this;
        }
    }
}
