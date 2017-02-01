using System;
using System.Collections.Generic;
using System.Linq;

namespace Pastel.Nodes
{
    class SwitchStatement : Executable
    {
        public Expression Condition { get; set; }
        public SwitchChunk[] Chunks { get; set; }

        public SwitchStatement(Token switchToken, Expression condition, IList<SwitchChunk> chunks) : base(switchToken)
        {
            this.Condition = condition;
            this.Chunks = chunks.ToArray();
        }
        
        public class SwitchChunk
        {
            public Token[] CaseAndDefaultTokens { get; set; }
            public Expression[] Cases { get; set; }
            public bool HasDefault { get; set; }
            public Executable[] Code { get; set; }

            public SwitchChunk(IList<Token> caseAndDefaultTokens, IList<Expression> caseExpressionsOrNullForDefault, IList<Executable> code)
            {
                this.CaseAndDefaultTokens = caseAndDefaultTokens.ToArray();
                this.Cases = caseExpressionsOrNullForDefault.ToArray();
                this.Code = code.ToArray();

                for (int i = 0; i < this.Cases.Length - 1; ++i)
                {
                    if (this.Cases[i] == null)
                    {
                        throw new ParserException(caseAndDefaultTokens[i], "default cannot appear before other cases.");
                    }
                }

                this.HasDefault = this.Cases[this.Cases.Length - 1] == null;
            }
        }

        public override IList<Executable> ResolveNamesAndCullUnusedCode(PastelCompiler compiler)
        {
            throw new NotImplementedException();
        }
    }
}
