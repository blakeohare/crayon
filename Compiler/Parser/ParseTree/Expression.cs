using Parser.Resolver;
using System.Collections.Generic;

namespace Parser.ParseTree
{
    public abstract class Expression : Node
    {
        protected static Expression[] NO_DESCENDANTS = new Expression[0];

        public Expression(Token firstToken, Node owner)
            : base(firstToken, owner)
        {
            this.Annotations = null;
        }

        public virtual bool CanAssignTo { get { return false; } }

        public ResolvedType ResolvedType { get; set; }

        // Override and return true if this expression, when used in an inline function as an argument into a library/core
        // function, shouldn't pose any problems. Generally this would be inline constants, simple variables, or static fields.
        public virtual bool IsInlineCandidate { get { return false; } }

        internal abstract Expression ResolveEntityNames(ParserContext parser);

        internal abstract Expression Resolve(ParserContext parser);

        internal abstract Expression ResolveTypes(ParserContext parser, TypeResolver typeResolver);

        internal abstract void ResolveVariableOrigins(ParserContext parser, VariableScope varIds, VariableIdAllocPhase phase);

        public virtual bool IsLiteral { get { return false; } }

        internal abstract IEnumerable<Expression> Descendants { get; }

        internal Dictionary<string, Annotation> Annotations { get; set; }

        internal Annotation GetAnnotation(string type)
        {
            if (this.Annotations != null && this.Annotations.ContainsKey(type))
            {
                return this.Annotations[type];
            }
            return null;
        }

        internal IEnumerable<Expression> GetFlattenedDescendants()
        {
            List<Expression> output = new List<Expression>() { this };
            for (int i = 0; i < output.Count; ++i)
            {
                output.AddRange(output[i].Descendants);
            }
            return output;
        }
    }
}
