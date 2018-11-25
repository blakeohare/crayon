using System.Collections.Generic;
using System.Linq;

namespace Pastel.Nodes
{
    internal class SwitchStatement : Executable
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

        public override Executable ResolveNamesAndCullUnusedCode(PastelCompiler compiler)
        {
            this.Condition = this.Condition.ResolveNamesAndCullUnusedCode(compiler);
            for (int i = 0; i < this.Chunks.Length; ++i)
            {
                SwitchChunk chunk = this.Chunks[i];
                for (int j = 0; j < chunk.Cases.Length; ++j)
                {
                    if (chunk.Cases[j] != null)
                    {
                        chunk.Cases[j] = chunk.Cases[j].ResolveNamesAndCullUnusedCode(compiler);
                    }
                }

                chunk.Code = Executable.ResolveNamesAndCullUnusedCodeForBlock(chunk.Code, compiler).ToArray();
            }
            return this;
        }

        internal override void ResolveTypes(VariableScope varScope, PastelCompiler compiler)
        {
            this.Condition = this.Condition.ResolveType(varScope, compiler);
            if (!this.Condition.ResolvedType.IsIdentical(compiler, PType.INT))
            {
                throw new ParserException(this.Condition.FirstToken, "Only ints can be used in switch statements.");
            }

            // consider it all one scope
            for (int i = 0; i < this.Chunks.Length; ++i)
            {
                SwitchChunk chunk = this.Chunks[i];
                for (int j = 0; j < chunk.Cases.Length; ++j)
                {
                    Expression ex = chunk.Cases[j];
                    if (ex != null)
                    {
                        ex = ex.ResolveType(varScope, compiler);
                        chunk.Cases[j] = ex;
                        if (ex.ResolvedType.RootValue != "int")
                        {
                            throw new ParserException(ex.FirstToken, "Only ints may be used.");
                        }
                    }
                }

                Executable.ResolveTypes(chunk.Code, varScope, compiler);
            }
        }

        internal override Executable ResolveWithTypeContext(PastelCompiler compiler)
        {
            this.Condition = this.Condition.ResolveWithTypeContext(compiler);
            for (int i = 0; i < this.Chunks.Length; ++i)
            {
                SwitchChunk chunk = this.Chunks[i];
                for (int j = 0; j < chunk.Cases.Length; ++j)
                {
                    if (chunk.Cases[j] != null)
                    {
                        chunk.Cases[j] = chunk.Cases[j].ResolveWithTypeContext(compiler);
                    }
                }
                Executable.ResolveWithTypeContext(compiler, chunk.Code);
            }
            return this;
        }
    }
}
