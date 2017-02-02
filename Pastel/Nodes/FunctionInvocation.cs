using System;
using System.Collections.Generic;
using System.Linq;

namespace Pastel.Nodes
{
    class FunctionInvocation : Expression
    {
        public Expression Root { get; set; }
        public Token OpenParenToken { get; set; }
        public Expression[] Args { get; set; }

        public FunctionInvocation(
            Expression root,
            Token openParen,
            IList<Expression> args) : base(root.FirstToken)
        {
            this.Root = root;
            this.OpenParenToken = openParen;
            this.Args = args.ToArray();
        }

        public Expression MaybeImmediatelyResolve(PastelParser parser)
        {
            if (this.Root is CompileTimeFunctionReference)
            {
                CompileTimeFunctionReference constFunc = (CompileTimeFunctionReference)this.Root;
                InlineConstant argName = (InlineConstant)this.Args[0];
                switch (constFunc.NameToken.Value)
                {
                    case "ext_boolean":
                        return new InlineConstant(
                            PType.BOOL,
                            this.FirstToken,
                            parser.GetParseTimeBooleanConstant(argName.Value.ToString()));

                    case "ext_integer":
                        return new InlineConstant(
                            PType.INT,
                            this.FirstToken,
                            parser.GetParseTimeIntegerConstant(argName.Value.ToString()));

                    default:
                        return this;
                }
            }
            return this;
        }

        public override Expression ResolveNamesAndCullUnusedCode(PastelCompiler compiler)
        {
            this.Root = this.Root.ResolveNamesAndCullUnusedCode(compiler);
            Expression.ResolveNamesAndCullUnusedCodeInPlace(this.Args, compiler);

            // TODO: check for core function reference
            return this;
        }
    }
}
