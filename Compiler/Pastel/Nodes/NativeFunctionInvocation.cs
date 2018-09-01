using System;
using System.Collections.Generic;
using System.Linq;

namespace Pastel.Nodes
{
    internal class NativeFunctionInvocation : Expression
    {
        public NativeFunction Function { get; set; }
        public Expression[] Args { get; set; }

        public NativeFunctionInvocation(Token firstToken, NativeFunction function, IList<Expression> args) : base(firstToken)
        {
            this.Function = function;
            this.Args = args.ToArray();
        }

        public NativeFunctionInvocation(Token firstToken, NativeFunction function, Expression context, IList<Expression> args)
           : this(firstToken, function, PushInFront(context, args))
        { }

        private static IList<Expression> PushInFront(Expression ex, IList<Expression> others)
        {
            List<Expression> expressions = new List<Expression>() { ex };
            expressions.AddRange(others);
            return expressions;
        }

        public override Expression ResolveNamesAndCullUnusedCode(PastelCompiler compiler)
        {
            throw new NotImplementedException();
        }

        internal override Expression ResolveType(VariableScope varScope, PastelCompiler compiler)
        {
            // The args were already resolved.
            // This ensures that they match the native function definition

            PType[] expectedTypes = NativeFunctionUtil.GetNativeFunctionArgTypes(this.Function);
            bool[] isArgRepeated = NativeFunctionUtil.GetNativeFunctionIsArgTypeRepeated(this.Function);

            switch (this.Function)
            {
                case NativeFunction.FORCE_PARENS:
                    if (this.Args.Length != 1) throw new ParserException(this.FirstToken, "Expected 1 arg.");

                    return new ForcedParenthesis(this.FirstToken, this.Args[0]);
            }

            Dictionary<string, PType> templateLookup = new Dictionary<string, PType>();

            int verificationLength = expectedTypes.Length;
            if (verificationLength > 0 && isArgRepeated[isArgRepeated.Length - 1])
            {
                verificationLength--;
            }

            for (int i = 0; i < verificationLength; ++i)
            {
                if (!PType.CheckAssignmentWithTemplateOutput(expectedTypes[i], this.Args[i].ResolvedType, templateLookup))
                {
                    throw new ParserException(this.Args[i].FirstToken, "Incorrect type. Expected " + expectedTypes[i] + " but found " + this.Args[i].ResolvedType + ".");
                }
            }

            if (expectedTypes.Length < this.Args.Length)
            {
                if (isArgRepeated[isArgRepeated.Length - 1])
                {
                    PType expectedType = expectedTypes[expectedTypes.Length - 1];
                    for (int i = expectedTypes.Length; i < this.Args.Length; ++i)
                    {
                        if (!PType.CheckAssignment(expectedType, this.Args[i].ResolvedType))
                        {
                            throw new ParserException(this.Args[i].FirstToken, "Incorrect type. Expected " + expectedTypes[i] + " but found " + this.Args[i].ResolvedType + ".");
                        }
                    }
                }
                else
                {
                    throw new ParserException(this.FirstToken, "Too many arguments.");
                }
            }

            PType returnType = NativeFunctionUtil.GetNativeFunctionReturnType(this.Function);

            if (returnType.HasTemplates)
            {
                returnType = returnType.ResolveTemplates(templateLookup);
            }

            this.ResolvedType = returnType;

            return this;
        }

        internal override Expression ResolveWithTypeContext(PastelCompiler compiler)
        {
            for (int i = 0; i < this.Args.Length; ++i)
            {
                this.Args[i] = this.Args[i].ResolveWithTypeContext(compiler);
            }
            return this;
        }
    }
}
