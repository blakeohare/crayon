using System.Collections.Generic;

namespace Crayon.ParseTree
{
    internal class BaseMethodReference : Expression
    {
        internal override Expression PastelResolve(ParserContext parser)
        {
            throw new System.NotImplementedException();
        }

        public override bool CanAssignTo { get { return false; } }

        public Token DotToken { get; set; }
        public Token StepToken { get; set; }
        public ClassDefinition ClassToWhichThisMethodRefers { get; set; }
        public FunctionDefinition FunctionDefinition { get; set; }

        public BaseMethodReference(Token firstToken, Token dotToken, Token stepToken, TopLevelConstruct owner)
            : base(firstToken, owner)
        {
            this.DotToken = dotToken;
            this.StepToken = stepToken;
            ClassDefinition cd = null;
            if (owner is FunctionDefinition)
            {
                cd = (ClassDefinition)((FunctionDefinition)owner).Owner;
            }
            else if (owner is ClassDefinition)
            {
                cd = (ClassDefinition)owner;
            }
            else
            {
                throw new System.InvalidOperationException(); // this should not happen.
            }
            this.ClassToWhichThisMethodRefers = cd.BaseClass;
            this.FunctionDefinition = this.ClassToWhichThisMethodRefers.GetMethod(this.StepToken.Value, true);
            if (this.FunctionDefinition == null)
            {
                throw new ParserException(this.StepToken, "There is no method named '" + this.StepToken.Value + "' on any base class.");
            }
        }

        internal override Expression Resolve(ParserContext parser)
        {
            return this;
        }

        internal override Expression ResolveNames(ParserContext parser)
        {
            return this;
        }

        internal override void GetAllVariablesReferenced(HashSet<Variable> vars) { }
        internal override void PerformLocalIdAllocation(ParserContext parser, VariableIdAllocator varIds, VariableIdAllocPhase phase) { }
    }
}
