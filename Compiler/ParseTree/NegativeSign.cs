using System.Collections.Generic;

namespace Crayon.ParseTree
{
    internal class NegativeSign : Expression
    {
        public override bool CanAssignTo { get { return false; } }

        public Expression Root { get; private set; }

        public NegativeSign(Token sign, Expression root, Executable owner)
            : base(sign, owner)
        {
            this.Root = root;
        }

        internal override Expression Resolve(Parser parser)
        {
            this.Root = this.Root.Resolve(parser);
            if (this.Root is IntegerConstant)
            {
                return new IntegerConstant(this.FirstToken, ((IntegerConstant)this.Root).Value * -1, this.FunctionOrClassOwner);
            }

            if (this.Root is FloatConstant)
            {
                return new FloatConstant(this.FirstToken, ((FloatConstant)this.Root).Value * -1, this.FunctionOrClassOwner);
            }

            return this;
        }

        internal override void SetLocalIdPass(VariableIdAllocator varIds)
        {
            this.Root.SetLocalIdPass(varIds);
        }

        internal override Expression ResolveNames(Parser parser, System.Collections.Generic.Dictionary<string, Executable> lookup, string[] imports)
        {
            this.Root = this.Root.ResolveNames(parser, lookup, imports);
            return this;
        }

        internal override void GetAllVariablesReferenced(HashSet<Variable> vars)
        {
            this.Root.GetAllVariablesReferenced(vars);
        }
    }
}
