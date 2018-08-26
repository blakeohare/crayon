using System;
using System.Collections.Generic;

namespace Parser.ParseTree
{
    public class CoreFunctionInvocation : Expression
    {
        public Expression[] Args { get; set; }
        public int FunctionId { get; set; }

        public override bool CanAssignTo { get { return false; } }

        public CoreFunctionInvocation(Token firstToken, Expression[] originalArgs, Node owner) :
            base(firstToken, owner)
        {
            if (originalArgs.Length == 0 || !(originalArgs[0] is StringConstant))
            {
                throw new ParserException(firstToken, "$$$ invocations must include a string constant containing the function name.");
            }

            this.FunctionId = CoreFunctionIDHelper.GetId(
                (StringConstant)originalArgs[0],
                this.Owner.FileScope.CompilationScope.Locale);
            List<Expression> args = new List<Expression>(originalArgs);
            args.RemoveAt(0);
            this.Args = args.ToArray();
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
            // created after the resolve name phase
            throw new NotImplementedException();
        }

        internal override void PerformLocalIdAllocation(ParserContext parser, VariableScope varIds, VariableIdAllocPhase phase)
        {
            if ((phase & VariableIdAllocPhase.ALLOC) != 0)
            {
                foreach (Expression arg in this.Args)
                {
                    arg.PerformLocalIdAllocation(parser, varIds, phase);
                }
            }
        }
    }
}
