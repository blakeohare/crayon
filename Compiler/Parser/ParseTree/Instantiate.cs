﻿using System.Collections.Generic;
using System.Linq;

namespace Parser.ParseTree
{
    public class Instantiate : Expression
    {
        public override bool CanAssignTo { get { return false; } }

        public Token NameToken { get; private set; }
        public string Name { get; private set; }
        public Expression[] Args { get; private set; }
        public ClassDefinition Class { get; set; }
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

        internal override Expression Resolve(ParserContext parser)
        {
            for (int i = 0; i < this.Args.Length; ++i)
            {
                this.Args[i] = this.Args[i].Resolve(parser);
            }

            if (this.Class == null)
            {
                throw new ParserException(this, "No class named '" + this.Name + "'");
            }

            if (this.Class.StaticToken != null)
            {
                throw new ParserException(this, "Cannot instantiate a static class.");
            }

            ConstructorDefinition cons = this.Class.Constructor;

            if (cons.Annotations.IsPrivate())
            {
                bool isValidUsage =
                    this.Class == this.Owner || // used in a field where the owner is the class directly
                    this.Class == this.Owner.Owner; // used in a function where the owner is the method whose owner is the class.

                if (!isValidUsage)
                {
                    throw new ParserException(this, "The constructor for " + this.Class.NameToken.Value + " is private and cannot be invoked from outside the class.");
                }
            }

            if (this.Args.Length < cons.MinArgCount || this.Args.Length > cons.MaxArgCount)
            {
                string message = "This constructor has the wrong number of arguments. ";
                if (cons.MinArgCount == cons.MaxArgCount)
                {
                    message += "Expected " + cons.MinArgCount + " but found " + this.Args.Length;
                }
                else if (this.Args.Length < cons.MinArgCount)
                {
                    message += " At least " + cons.MinArgCount + " are required but found only " + this.Args.Length + ".";
                }
                else
                {
                    message += " At most " + cons.MaxArgCount + " are allowed but found " + this.Args.Length + ".";
                }
                throw new ParserException(this, message);
            }

            return this;
        }

        internal override Expression ResolveEntityNames(ParserContext parser)
        {
            this.BatchExpressionEntityNameResolver(parser, this.Args);
            this.Class = this.FileScope.DoClassLookup(this.Owner, this.NameToken, this.Name);
            return this;
        }

        internal override void PerformLocalIdAllocation(ParserContext parser, VariableScope varIds, VariableIdAllocPhase phase)
        {
            if ((phase & VariableIdAllocPhase.ALLOC) != 0)
            {
                foreach (Expression arg in this.Args)
                {
                    arg.PerformLocalIdAllocation(parser, varIds, phase);
                }
            }
        }
    }
}
