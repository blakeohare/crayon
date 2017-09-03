using System;
using System.Collections.Generic;

namespace Crayon.ParseTree
{
    internal class ConstReference : Expression
    {
        internal override Expression PastelResolve(Parser parser)
        {
            throw new NotImplementedException();
        }

        public override bool CanAssignTo { get { return false; } }

        public ConstStatement ConstStatement { get; private set; }

        public ConstReference(Token token, ConstStatement con, Executable owner)
            : base(token, owner)
        {
            this.ConstStatement = con;
        }

        internal override Expression Resolve(Parser parser)
        {
            if (parser.ConstantAndEnumResolutionState[this.ConstStatement] != ConstantResolutionState.RESOLVED)
            {
                this.ConstStatement.Resolve(parser);
            }

            IConstantValue value = this.ConstStatement.Expression as IConstantValue;
            if (value == null)
            {
                throw new ParserException(this.ConstStatement.FirstToken, "Could not resolve this expression into a constant value.");
            }
            return value.CloneValue(this.FirstToken, this.FunctionOrClassOwner);
        }

        internal override Expression ResolveNames(Parser parser, Dictionary<string, Executable> lookup, string[] imports)
        {
            throw new NotImplementedException();
        }

        internal override void GetAllVariablesReferenced(HashSet<Variable> vars) { }
        internal override void PerformLocalIdAllocation(Parser parser, VariableIdAllocator varIds, VariableIdAllocPhase phase) { }
    }
}
