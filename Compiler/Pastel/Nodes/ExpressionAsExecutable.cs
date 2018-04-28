using System;

namespace Pastel.Nodes
{
    internal class ExpressionAsExecutable : Executable
    {
        public Expression Expression { get; set; }

        public ExpressionAsExecutable(Expression expression) : base(expression.FirstToken)
        {
            this.Expression = expression;
        }

        internal Executable[] ImmediateResolveMaybe(PastelParser parser)
        {
            if (this.Expression is FunctionInvocation)
            {
                if (((FunctionInvocation)this.Expression).Root is CompileTimeFunctionReference)
                {
                    FunctionInvocation functionInvocation = (FunctionInvocation)this.Expression;
                    CompileTimeFunctionReference compileTimeFunction = (CompileTimeFunctionReference)functionInvocation.Root;
                    switch (compileTimeFunction.NameToken.Value)
                    {
                        case "import":
                            string path = ((InlineConstant)functionInvocation.Args[0]).Value.ToString();
                            return parser.ParseImportedCode(path);

                        default:
                            throw new NotImplementedException();
                    }
                }
            }
            return null;
        }

        public override Executable ResolveNamesAndCullUnusedCode(PastelCompiler compiler)
        {
            this.Expression = this.Expression.ResolveNamesAndCullUnusedCode(compiler);
            return this;
        }

        internal override void ResolveTypes(VariableScope varScope, PastelCompiler compiler)
        {
            this.Expression = this.Expression.ResolveType(varScope, compiler);
        }

        internal override Executable ResolveWithTypeContext(PastelCompiler compiler)
        {
            this.Expression = this.Expression.ResolveWithTypeContext(compiler);
            return this;
        }
    }
}
