﻿using System.Collections.Generic;

namespace Crayon.ParseTree
{
    public abstract class Expression : Node
    {
        public Expression(Token firstToken, Executable owner)
            : base(firstToken, owner)
        {
            this.Annotations = null;
        }

        public abstract bool CanAssignTo { get; }

        internal abstract Expression Resolve(Parser parser);

        internal abstract Expression ResolveNames(Parser parser, Dictionary<string, Executable> lookup, string[] imports);

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
