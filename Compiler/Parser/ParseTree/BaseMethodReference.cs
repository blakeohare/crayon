using Parser.Resolver;
using System.Collections.Generic;

namespace Parser.ParseTree
{
    public class BaseMethodReference : Expression
    {
        public override bool CanAssignTo { get { return false; } }

        internal override IEnumerable<Expression> Descendants { get { return Expression.NO_DESCENDANTS; } }

        public Token DotToken { get; set; }
        public Token StepToken { get; set; }
        public ClassDefinition ClassToWhichThisMethodRefers { get; set; }
        public FunctionDefinition FunctionDefinition { get; set; }

        public BaseMethodReference(Token firstToken, Token dotToken, Token stepToken, Node owner)
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

        internal override Expression ResolveEntityNames(ParserContext parser)
        {
            return this;
        }

        internal override Expression ResolveTypes(ParserContext parser, TypeResolver typeResolver)
        {
            this.ResolvedType = ResolvedType.GetFunctionType(this.FunctionDefinition);
            return this;
        }

        internal override void PerformLocalIdAllocation(ParserContext parser, VariableScope varIds, VariableIdAllocPhase phase) { }
    }
}
