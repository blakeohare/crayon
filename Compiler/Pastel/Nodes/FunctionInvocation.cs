using System;
using System.Collections.Generic;
using System.Linq;

namespace Pastel.Nodes
{
    internal class FunctionInvocation : Expression
    {
        public Expression Root { get; set; }
        public Token OpenParenToken { get; set; }
        public Expression[] Args { get; set; }

        public FunctionInvocation(
            Expression root,
            Token openParen,
            IList<Expression> args) : base(root.FirstToken, root.Owner)
        {
            this.Root = root;
            this.OpenParenToken = openParen;
            this.Args = args.ToArray();
        }

        internal Expression MaybeImmediatelyResolve(PastelParser parser)
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
                            parser.GetParseTimeBooleanConstant(argName.Value.ToString()),
                            this.Owner);

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

            return this;
        }

        internal override Expression ResolveType(VariableScope varScope, PastelCompiler compiler)
        {
            for (int i = 0; i < this.Args.Length; ++i)
            {
                this.Args[i] = this.Args[i].ResolveType(varScope, compiler);
            }

            this.Root = this.Root.ResolveType(varScope, compiler);

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
                    if (!PType.CheckAssignment(compiler, expectedTypes[i], this.Args[i].ResolvedType))
                    {
                        throw new ParserException(this.Args[i].FirstToken, "Wrong function arg type. Cannot convert a " + this.Args[i].ResolvedType + " to a " + expectedTypes[i]);
                    }
                }

                this.ResolvedType = functionDefinition.ReturnType;
                return this;
            }
            else if (this.Root is CoreFunctionReference)
            {
                CoreFunctionReference nfr = (CoreFunctionReference)this.Root;
                CoreFunctionInvocation nfi;
                if (nfr.Context == null)
                {
                    nfi = new CoreFunctionInvocation(this.FirstToken, nfr.CoreFunctionId, this.Args, this.Owner);
                }
                else
                {
                    nfi = new CoreFunctionInvocation(this.FirstToken, nfr.CoreFunctionId, nfr.Context, this.Args, this.Owner);
                }

                return nfi.ResolveType(varScope, compiler);
            }
            else if (this.Root is ExtensibleFunctionReference)
            {
                return new ExtensibleFunctionInvocation(this.FirstToken, (ExtensibleFunctionReference)this.Root, this.Args).ResolveType(varScope, compiler);
            }
            else if (this.Root is ConstructorReference)
            {
                PType typeToConstruct = ((ConstructorReference)this.Root).TypeToConstruct;
                typeToConstruct.FinalizeType(compiler);
                return new ConstructorInvocation(this.FirstToken, typeToConstruct, this.Args, this.Owner);
            }
            else if (this.Root.ResolvedType.RootValue == "Func")
            {
                return new FunctionPointerInvocation(compiler, this.FirstToken, this.Root, this.Args);
            }
            else
            {
                throw new ParserException(this.OpenParenToken, "This expression cannot be invoked like a function.");
            }
        }

        internal override Expression ResolveWithTypeContext(PastelCompiler compiler)
        {
            this.Root = this.Root.ResolveWithTypeContext(compiler);

            if (this.Root is FunctionReference)
            {
                // this is okay.
            }
            else
            {
                throw new ParserException(this.OpenParenToken, "Cannot invoke this like a function.");
            }

            for (int i = 0; i < this.Args.Length; ++i)
            {
                this.Args[i] = this.Args[i].ResolveWithTypeContext(compiler);
            }
            return this;
        }
    }
}
