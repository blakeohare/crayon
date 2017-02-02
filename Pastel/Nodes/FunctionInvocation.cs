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

        internal override void ResolveType(VariableScope varScope, PastelCompiler compiler)
        {
            for (int i = 0; i < this.Args.Length; ++i)
            {
                this.Args[i].ResolveType(varScope, compiler);
            }

            if (this.Root is FunctionReference)
            {
                FunctionDefinition functionDefinition = ((FunctionReference)this.Root).Function;
                PType[] expectedTypes = functionDefinition.ArgTypes;
                if (expectedTypes.Length != this.Args.Length)
                {
                    throw new ParserException(this.OpenParenToken, "This function invocation has the wrong number of parameters. Expected " + expectedTypes.Length + " but found " + this.Args.Length + ".");
                }

                for (int i = 0; i < this.Args.Length; ++i)
                {
                    if (!expectedTypes[i].IsParentOf(this.Args[i].ResolvedType))
                    {
                        throw new ParserException(this.Args[i].FirstToken, "Wrong function arg type. Cannot convert a " + this.Args[i].ResolvedType + " to a " + expectedTypes[i]);
                    }
                }

                this.ResolvedType = functionDefinition.ReturnType;
            }
            else if (this.Root is DotField)
            {
                DotField dotField = (DotField)this.Root;
                dotField.ResolveType(varScope, compiler);
                switch (dotField.NativeFunctionId)
                {
                    case NativeFunction.NONE: throw new ParserException(this.FirstToken, "Unable to resolve this function.");
                    case NativeFunction.DICTIONARY_CONTAINS_KEY: this.ResolvedType = PType.BOOL; return;
                    default: throw new NotImplementedException();
                }
            }
            else if (this.Root is ConstructorReference)
            {
                this.ResolvedType = ((ConstructorReference)this.Root).TypeToConstruct;
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}
