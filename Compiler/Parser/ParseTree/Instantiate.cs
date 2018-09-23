using Parser.Resolver;
using System.Collections.Generic;
using System.Linq;

namespace Parser.ParseTree
{
    public class Instantiate : Expression
    {
        public Token NameToken { get; private set; }
        public string Name { get; private set; }
        public Expression[] Args { get; private set; }
        public ClassDefinition Class { get; set; }
        public ConstructorDefinition ConstructorReference { get; private set; }
        public AType[] Generics { get; set; }

        public Instantiate(
            Token firstToken,
            Token firstClassNameToken,
            string name,
            IList<AType> generics,
            IList<Expression> args,
            Node owner)
            : base(firstToken, owner)
        {
            this.NameToken = firstClassNameToken;
            this.Name = name;
            this.Args = args.ToArray();
            this.Generics = generics.ToArray();
        }

        internal override IEnumerable<Expression> Descendants { get { return this.Args; } }

        internal override Expression Resolve(ParserContext parser)
        {
            for (int i = 0; i < this.Args.Length; ++i)
            {
                this.Args[i] = this.Args[i].Resolve(parser);
            }

            return this;
        }

        internal override Expression ResolveEntityNames(ParserContext parser)
        {
            this.BatchExpressionEntityNameResolver(parser, this.Args);
            // TODO: localize or create a dummy stub in Core
            if (this.Name == "List")
            {
                return new ListDefinition(this.FirstToken, new List<Expression>(), this.Generics[0], this.Owner);
            }
            else if (this.Name == "Dictionary")
            {
                return new DictionaryDefinition(
                    this.FirstToken,
                    this.Generics[0], this.Generics[1],
                    new List<Expression>(), new List<Expression>(),
                    this.Owner);
            }
            this.Class = this.FileScope.DoClassLookup(this.Owner, this.NameToken, this.Name);

            if (this.Class == null)
            {
                throw new ParserException(this, "No class named '" + this.Name + "'");
            }

            if (this.Class.Modifiers.HasStatic)
            {
                throw new ParserException(this, "Cannot instantiate a static class.");
            }

            Node.EnsureAccessIsAllowed(this.FirstToken, this.Owner, this.Class);
            if (!Node.IsAccessAllowed(this.Owner, this.Class.Constructor))
            {
                // TODO: word this better. Maybe just say "this class' constructor is marked as private/protected/etc"
                throw new ParserException(this.FirstToken, "This class' constructor's access modifier does not allow it to be constructed here.");
            }

            this.ConstructorReference = this.Class.Constructor;
            int minArgCount = 0;
            int maxArgCount = 0;
            bool isPrivate = false;
            if (this.ConstructorReference != null)
            {
                minArgCount = this.ConstructorReference.MinArgCount;
                maxArgCount = this.ConstructorReference.MaxArgCount;
                isPrivate = this.ConstructorReference.Annotations.IsPrivate();
            }

            if (isPrivate)
            {
                bool isValidUsage =
                    this.Class == this.Owner || // used in a field where the owner is the class directly
                    this.Class == this.Owner.Owner; // used in a function where the owner is the method whose owner is the class.

                if (!isValidUsage)
                {
                    throw new ParserException(this, "The constructor for " + this.Class.NameToken.Value + " is private and cannot be invoked from outside the class.");
                }
            }

            if (this.Args.Length < minArgCount ||
                this.Args.Length > maxArgCount)
            {
                string message = "This constructor has the wrong number of arguments. ";
                if (minArgCount == maxArgCount)
                {
                    message += "Expected " + minArgCount + " but found " + this.Args.Length;
                }
                else if (this.Args.Length < minArgCount)
                {
                    message += " At least " + minArgCount + " are required but found only " + this.Args.Length + ".";
                }
                else
                {
                    message += " At most " + maxArgCount + " are allowed but found " + this.Args.Length + ".";
                }
                throw new ParserException(this, message);
            }

            return this;
        }

        internal override Expression ResolveTypes(ParserContext parser, TypeResolver typeResolver)
        {
            ClassDefinition cd = this.Class;

            if (this.ConstructorReference != null)
            {
                ResolvedType[] expectedArgTypes = this.ConstructorReference.ResolvedArgTypes;
                for (int i = 0; i < this.Args.Length; ++i)
                {
                    this.Args[i] = this.Args[i].ResolveTypes(parser, typeResolver);
                    if (!this.Args[i].ResolvedType.CanAssignToA(expectedArgTypes[i]))
                    {
                        throw new ParserException(this.Args[i], "Cannot pass an argument of this type.");
                    }
                }
            }

            this.ResolvedType = ResolvedType.GetInstanceType(cd);

            return this;
        }

        internal override void ResolveVariableOrigins(ParserContext parser, VariableScope varIds, VariableIdAllocPhase phase)
        {
            if ((phase & VariableIdAllocPhase.ALLOC) != 0)
            {
                foreach (Expression arg in this.Args)
                {
                    arg.ResolveVariableOrigins(parser, varIds, phase);
                }
            }
        }
    }
}
