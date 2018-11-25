using System;
using System.Collections.Generic;
using System.Linq;

namespace Pastel.Nodes
{
    internal class ExtensibleFunctionInvocation : Expression
    {
        public Expression[] Args { get; set; }
        public ExtensibleFunctionReference FunctionRef { get; set; }

        public ExtensibleFunctionInvocation(
            Token firstToken,
            ExtensibleFunctionReference functionRef,
            IList<Expression> args)
            : base(firstToken, functionRef.Owner)
        {
            this.FunctionRef = functionRef;
            this.Args = args.ToArray();
        }

        public override Expression ResolveNamesAndCullUnusedCode(PastelCompiler compiler)
        {
            throw new NotImplementedException();
        }

        internal override Expression ResolveType(VariableScope varScope, PastelCompiler compiler)
        {
            // Args already resolved by FunctionInvocation.ResolveType().

            string name = this.FunctionRef.Name;
            ExtensibleFunction extensibleFunction;
            if (!compiler.ExtensibleFunctions.TryGetValue(name, out extensibleFunction))
            {
                throw new ParserException(this.FirstToken, "Type information for '" + name + "' extensible function is not defined.");
            }
            this.ResolvedType = extensibleFunction.ReturnType;

            PType[] argTypes = extensibleFunction.ArgTypes;

            if (argTypes.Length != this.Args.Length)
            {
                throw new ParserException(this.FirstToken, "Incorrect number of args for this function. Expected " + argTypes.Length + " but instead found " + this.Args.Length + ".");
            }

            for (int i = 0; i < this.Args.Length; ++i)
            {
                if (!PType.CheckAssignment(compiler, argTypes[i], this.Args[i].ResolvedType))
                {
                    throw new ParserException(this.Args[i].FirstToken, "Invalid argument type. Expected '" + argTypes[i] + "' but found '" + this.Args[i].ResolvedType + "'.");
                }
            }

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
