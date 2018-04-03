using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Parser.ParseTree
{
    public class IsComparison : Expression
    {
        internal override Expression PastelResolve(ParserContext parser)
        {
            throw new NotImplementedException();
        }

        public Expression Expression { get; set; }
        public Token IsToken { get; set; }
        public Token ClassToken { get; set; }
        public string ClassName { get; set; }
        public ClassDefinition ClassDefinition { get; set; }

        public override bool CanAssignTo { get { return false; } }

        public IsComparison(Expression root, Token isToken, Token firstClassToken, string classNameWithNamespace, TopLevelConstruct owner)
            : base(root.FirstToken, owner)
        {
            this.Expression = root;
            this.IsToken = isToken;
            this.ClassToken = firstClassToken;
            this.ClassName = classNameWithNamespace;
        }

        internal override Expression Resolve(ParserContext parser)
        {
            this.Expression.Resolve(parser);
            return this;
        }

        internal override Expression ResolveNames(ParserContext parser)
        {
            this.Expression.ResolveNames(parser);
            this.ClassDefinition = Node.DoClassLookup(this.Owner, this.ClassToken, this.ClassName);
            return this;
        }

        internal override void GetAllVariablesReferenced(HashSet<Variable> vars)
        {
            this.Expression.GetAllVariablesReferenced(vars);
        }

        internal override void PerformLocalIdAllocation(ParserContext parser, VariableIdAllocator varIds, VariableIdAllocPhase phase)
        {
            this.Expression.PerformLocalIdAllocation(parser, varIds, phase);
        }
    }
}
