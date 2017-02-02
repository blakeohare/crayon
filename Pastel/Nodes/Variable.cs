using System;
using System.Collections.Generic;

namespace Pastel.Nodes
{
    class Variable : Expression
    {
        public Variable(Token token) : base(token)
        { }

        public string Name { get { return this.FirstToken.Value; } }

        public override Expression ResolveNamesAndCullUnusedCode(PastelCompiler compiler)
        {
            string name = this.Name;
            if (compiler.ConstantDefinitions.ContainsKey(name))
            {
                // resolved by now
                InlineConstant constantValue = (InlineConstant)compiler.ConstantDefinitions[name].Value;
                return constantValue.CloneWithNewToken(this.FirstToken);
            }

            if (compiler.FunctionDefinitions.ContainsKey(name))
            {
                if (!compiler.ResolvedFunctions.Contains(name))
                {
                    compiler.ResolutionQueue.Enqueue(name);
                }

                return new FunctionReference(this.FirstToken, compiler.FunctionDefinitions[name]);
            }

            return this;
        }

        internal override void ResolveType(VariableScope varScope, PastelCompiler compiler)
        {
            PType type = varScope.GetTypeOfVariable(this.Name);
            this.ResolvedType = type;
            if (type == null)
            {
                throw new ParserException(this.FirstToken, "This variable is not defined.");
            }
        }
    }
}
