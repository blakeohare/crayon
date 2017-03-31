using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pastel.Nodes
{
    public class LibraryNativeFunctionInvocation : Expression
    {
        public Expression[] Args { get; set; }
        public LibraryNativeFunctionReference LibraryNativeFunction { get; set; }

        public LibraryNativeFunctionInvocation(Token firstToken, LibraryNativeFunctionReference libraryNativeFunction, IList<Expression> args) : base(firstToken)
        {
            this.LibraryNativeFunction = libraryNativeFunction;
            this.Args = args.ToArray();
        }

        public override Expression ResolveNamesAndCullUnusedCode(PastelCompiler compiler)
        {
            throw new NotImplementedException();
        }

        internal override Expression ResolveType(VariableScope varScope, PastelCompiler compiler)
        {
            // Args already resolved by FunctionInvocation.ResolveType().

            string name = this.LibraryNativeFunction.Name;
            PType returnType;
            if (!compiler.LibraryNativeFunctionReferenceReturnTypes.TryGetValue(name, out returnType))
            {
                throw new ParserException(this.FirstToken, "Type information for '" + name + "' library function is not defined.");
            }
            this.ResolvedType = returnType;

            PType[] argTypes = compiler.LibraryNativeFunctionReferenceArgumentTypes[name];

            if (argTypes.Length != this.Args.Length)
            {
                throw new ParserException(this.FirstToken, "Incorrect number of args for this function. Expected " + argTypes.Length + " but instead found " + this.Args.Length + ".");
            }

            for (int i = 0; i < this.Args.Length; ++i)
            {
                if (!PType.CheckAssignment(argTypes[i], this.Args[i].ResolvedType))
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
