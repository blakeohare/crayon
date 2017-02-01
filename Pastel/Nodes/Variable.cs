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
                return new FunctionReference(this.FirstToken, compiler.FunctionDefinitions[name]);
            }

            return this;
        }
    }
}
