using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parser.ParseTree
{
    public class CniFunctionInvocation : Expression
    {
        public Expression[] Args { get; private set; }
        public string Name { get; private set; }

        public CniFunctionInvocation(Token firstToken, IList<Expression> args, Node owner)
            : base(firstToken, owner)
        {
            this.Name = firstToken.Value.Substring(1);
            this.Args = args.ToArray();
        }

        public override bool CanAssignTo { get { return false; } }

        internal override void PerformLocalIdAllocation(ParserContext parser, VariableScope varIds, VariableIdAllocPhase phase)
        {
            foreach (Expression expr in this.Args)
            {
                expr.PerformLocalIdAllocation(parser, varIds, phase);
            }
        }

        internal override Expression Resolve(ParserContext parser)
        {
            for (int i = 0; i < this.Args.Length; ++i)
            {
                this.Args[i] = this.Args[i].Resolve(parser);
            }
            return this;
        }

        internal override Expression ResolveEntityNames(ParserContext parser)
        {
            CniFunction func;
            if (!this.CompilationScope.CniFunctionsByName.TryGetValue(this.Name, out func))
            {
                throw new ParserException(this, "This CNI method has not been declared in this compilation scope's manifest.");
            }

            if (func.ArgCount != this.Args.Length)
            {
                throw new ParserException(this, "Incorrect number of args. Expected " + func.ArgCount + " but found " + this.Args.Length);
            }

            for (int i = 0; i < this.Args.Length; ++i)
            {
                this.Args[i] = this.Args[i].ResolveEntityNames(parser);
            }
            return this;
        }
    }
}
