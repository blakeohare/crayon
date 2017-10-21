using System.Collections.Generic;

namespace Crayon.ParseTree
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

        internal abstract Expression Resolve(Parser parser);

        internal abstract Expression ResolveNames(Parser parser);

        public virtual bool IsLiteral { get { return false; } }

        internal Dictionary<string, Annotation> Annotations { get; set; }

        // To be overridden if necessary.
        internal override void GetAllVariableNames(Dictionary<string, bool> lookup)
        { }

        internal Annotation GetAnnotation(string type)
        {
            if (this.Annotations != null && this.Annotations.ContainsKey(type))
            {
                return this.Annotations[type];
            }
            return null;
        }

        internal abstract void GetAllVariablesReferenced(HashSet<Variable> vars);
        internal abstract Expression PastelResolve(Parser parser);
    }
}
