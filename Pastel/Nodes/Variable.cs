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

            InlineConstant constantValue = compiler.GetConstantDefinition(name);
            if (constantValue != null)
            {
                return constantValue.CloneWithNewToken(this.FirstToken);
            }

            FunctionDefinition functionDefinition = compiler.GetFunctionDefinitionAndMaybeQueueForResolution(name);
            if (functionDefinition != null)
            {
                return new FunctionReference(this.FirstToken, functionDefinition);
            }

            return this;
        }

        internal override Expression ResolveType(VariableScope varScope, PastelCompiler compiler)
        {
            PType type = varScope.GetTypeOfVariable(this.Name);
            this.ResolvedType = type;
            if (type == null)
            {
                throw new ParserException(this.FirstToken, "The variable '" + this.Name + "'is not defined.");
            }
            return this;
        }
    }
}
