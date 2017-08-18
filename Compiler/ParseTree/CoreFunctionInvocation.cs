using System;
using System.Collections.Generic;

namespace Crayon.ParseTree
{
    internal class CoreFunctionInvocation : Expression
    {
        internal override Expression PastelResolve(Parser parser)
        {
            throw new NotImplementedException();
        }

        public Expression[] Args { get; set; }
        public int FunctionId { get; set; }

        public override bool CanAssignTo { get { return false; } }

        public CoreFunctionInvocation(Token firstToken, Expression[] originalArgs, Executable owner) :
            base(firstToken, owner)
        {
            if (originalArgs.Length == 0 || !(originalArgs[0] is StringConstant))
            {
                throw new ParserException(firstToken, "$$$ invocations must include a string constant containing the function name.");
            }

            this.FunctionId = CoreFunctionIDHelper.GetId((StringConstant)originalArgs[0]);
            List<Expression> args = new List<Expression>(originalArgs);
            args.RemoveAt(0);
            this.Args = args.ToArray();
        }

        internal override Expression Resolve(Parser parser)
        {
            for (int i = 0; i < this.Args.Length; ++i)
            {
                this.Args[i] = this.Args[i].Resolve(parser);
            }
            return this;
        }

        internal override Expression ResolveNames(Parser parser, Dictionary<string, Executable> lookup, string[] imports)
        {
            // created after the resolve name phase
            throw new NotImplementedException();
        }

        internal override void GetAllVariablesReferenced(HashSet<Variable> vars)
        {
            foreach (Expression arg in this.Args)
            {
                arg.GetAllVariablesReferenced(vars);
            }
        }

        internal override void PerformLocalIdAllocation(VariableIdAllocator varIds, VariableIdAllocPhase phase)
        {
            if ((phase & VariableIdAllocPhase.ALLOC) != 0)
            {
                foreach (Expression arg in this.Args)
                {
                    arg.PerformLocalIdAllocation(varIds, phase);
                }
            }
        }
    }
}
