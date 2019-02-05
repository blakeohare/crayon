using Parser.Resolver;
using System.Collections.Generic;

namespace Parser.ParseTree
{
    public class Variable : Expression
    {
        public override bool IsInlineCandidate { get { return true; } }

        public override bool CanAssignTo { get { return true; } }

        public string Name { get; private set; }

        public VariableId LocalScopeId { get; set; }

        public Variable(Token token, string name, Node owner)
            : base(token, owner)
        {
            this.Name = name;
        }

        internal override IEnumerable<Expression> Descendants { get { return Expression.NO_DESCENDANTS; } }

        internal override Expression Resolve(ParserContext parser)
        {
            if (!parser.Keywords.IsValidVariable(this.Name))
            {
                throw new ParserException(this, "'" + this.Name + "' is a reserved keyword and cannot be used like this.");
            }

            return this;
        }

        internal override Expression ResolveEntityNames(ParserContext parser)
        {
            if (this.Name == "$var")
            {
                return new CompileTimeDictionary(this.FirstToken, "var", this.Owner);
            }

            if (this.Name == "$$$")
            {
                throw new ParserException(this, "Core function invocations cannot stand alone and must be immediately invoked.");
            }

            if (this.Name.StartsWith("$"))
            {
                throw new ParserException(this, "CNI functions must be invoked and cannot be used as function pointers.");
            }

            NamespaceReferenceTemplate nrt = this.Owner.FileScope.FileScopeEntityLookup.DoNamespaceLookup(this.Name, this.TopLevelEntity);
            if (nrt != null)
            {
                return new NamespaceReference(this.FirstToken, this.Owner, nrt);
            }

            TopLevelEntity exec = this.Owner.FileScope.FileScopeEntityLookup.DoEntityLookup(this.Name, this.Owner);

            if (exec != null)
            {
                if (!(this.Owner is ICodeContainer && ((ICodeContainer)this.Owner).ArgumentNameLookup.Contains(this.Name)))
                {
                    return ResolverPipeline.ConvertStaticReferenceToExpression(exec, this.FirstToken, this.Owner);
                }
            }

            return this;
        }

        internal override Expression ResolveTypes(ParserContext parser, TypeResolver typeResolver)
        {
            this.ResolvedType = this.LocalScopeId.ResolvedType;
            if (this.ResolvedType == null)
            {
                throw new ParserException(this, "This shouldn't happen");
            }
            return this;
        }

        internal override void ResolveVariableOrigins(ParserContext parser, VariableScope varIds, VariableIdAllocPhase phase)
        {
            if ((phase & VariableIdAllocPhase.ALLOC) != 0)
            {
                this.LocalScopeId = varIds.GetVarId(this.FirstToken);
                if (this.LocalScopeId == null)
                {
                    string name = this.FirstToken.Value;

                    if (parser.AssemblyManager.IsValidAssemblyNameFromLocale(this.Locale, name))
                    {
                        throw new ParserException(this, "'" + name + "' is referenced but not imported in this file.");
                    }

                    Node owner = this.Owner;
                    while (owner != null && !(owner is ClassDefinition))
                    {
                        owner = owner.Owner;
                    }

                    if (owner != null)
                    {
                        ClassDefinition cd = (ClassDefinition)owner;
                        foreach (FieldDefinition fd in cd.Fields)
                        {
                            if (fd.NameToken.Value == name)
                            {
                                string message = "'" + name + "' is used like a local variable but it is " + (fd.Modifiers.HasStatic ? "a static" : "an instance") + " field.";
                                message += " Did you mean '" + (fd.Modifiers.HasStatic ? cd.NameToken.Value : "this") + "." + name + "' instead of '" + name + "'?";
                                throw new ParserException(this, message);
                            }
                        }
                    }

                    // TODO: But if it's being called like a function then...
                    // - give a better error message "function 'foo' is not defined"
                    // - give an even better error message when there's a class or instance function with the same name
                    //   e.g. "'foo' is a static function and must be invoked with the class name: FooClass.foo(...)
                    // - if there's a method, suggest using "this."
                    // - if the variable name matches a library that is available, suggest it as a missing import.
                    throw new ParserException(this, "The variable '" + name + "' is used but is never assigned to.");
                }
            }
        }

        public override string ToString()
        {
            return "<Variable> " + this.Name;
        }
    }
}
