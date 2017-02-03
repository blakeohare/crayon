using System.Collections.Generic;
using Common;

namespace Crayon.ParseTree
{
    internal class FloatConstant : Expression, IConstantValue
    {
        internal override Expression PastelResolve(Parser parser)
        {
            return this;
        }

        public override bool CanAssignTo { get { return false; } }

        public double Value { get; private set; }

        public override bool IsLiteral { get { return true; } }

        public FloatConstant(Token startValue, double value, Executable owner)
            : base(startValue, owner)
        {
            this.Value = value;
        }

        public static double ParseValue(Token firstToken, string fullValue)
        {
            double value;
            if (!Util.ParseDouble(fullValue, out value))
            {
                throw new ParserException(firstToken, "Invalid float literal.");
            }
            return value;
        }

        internal override Expression Resolve(Parser parser)
        {
            return this;
        }
        
        internal override Expression ResolveNames(Parser parser, System.Collections.Generic.Dictionary<string, Executable> lookup, string[] imports)
        {
            return this;
        }

        public Expression CloneValue(Token token, Executable owner)
        {
            return new FloatConstant(token, this.Value, owner);
        }

        internal override void GetAllVariablesReferenced(HashSet<Variable> vars) { }

        internal override void PerformLocalIdAllocation(VariableIdAllocator varIds, VariableIdAllocPhase phase) { }
    }
}
