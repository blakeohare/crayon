using System.Collections.Generic;

namespace Parser.ParseTree
{
    public abstract class Expression : Node
    {
        public Expression(Token firstToken, TopLevelConstruct owner)
            : base(firstToken, owner)
        {
            this.Annotations = null;
        }

        public abstract bool CanAssignTo { get; }

        // Override and return true if this expression, when used in an inline function as an argument into a library/core
        // function, shouldn't pose any problems. Generally this would be inline constants, simple variables, or static fields.
        public virtual bool IsInlineCandidate {  get { return false; } }

        internal abstract Expression ResolveEntityNames(ParserContext parser);

        internal abstract Expression Resolve(ParserContext parser);

        internal abstract void PerformLocalIdAllocation(ParserContext parser, VariableScope varIds, VariableIdAllocPhase phase);

        public virtual bool IsLiteral { get { return false; } }

        internal Dictionary<string, Annotation> Annotations { get; set; }

        internal Annotation GetAnnotation(string type)
        {
            if (this.Annotations != null && this.Annotations.ContainsKey(type))
            {
                return this.Annotations[type];
            }
            return null;
        }
    }
}
