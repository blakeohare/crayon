using System.Collections.Generic;
using System.Linq;

namespace Crayon.ParseTree
{
    internal class Instantiate : Expression
    {
        public override bool CanAssignTo { get { return false; } }

        public Token NameToken { get; private set; }
        public string Name { get; private set; }
        public Expression[] Args { get; private set; }
        public ClassDefinition Class { get; set; }

        public Instantiate(Token firstToken, Token firstClassNameToken, string name, IList<Expression> args, Executable owner)
            : base(firstToken, owner)
        {
            this.NameToken = firstClassNameToken;
            this.Name = name;
            this.Args = args.ToArray();
        }

        internal override Expression Resolve(Parser parser)
        {
            string className = this.NameToken.Value;

            if (parser.IsTranslateMode)
            {
                StructDefinition structDefinition = parser.GetStructDefinition(className);

                if (structDefinition != null)
                {
                    if (this.Args.Length != structDefinition.Fields.Length)
                    {
                        throw new ParserException(this.FirstToken, "Args length did not match struct field count for '" + structDefinition.Name.Value + "'.");
                    }

                    StructInstance si = new StructInstance(this.FirstToken, this.NameToken, this.Args, this.FunctionOrClassOwner);
                    si = (StructInstance)si.Resolve(parser);
                    return si;
                }
            }

            for (int i = 0; i < this.Args.Length; ++i)
            {
                this.Args[i] = this.Args[i].Resolve(parser);
            }

            if (this.Class == null)
            {
                throw new ParserException(this.FirstToken, "No class named '" + this.Name + "'");
            }

            if (this.Class.StaticToken != null)
            {
                throw new ParserException(this.FirstToken, "Cannot instantiate a static class.");
            }
            
            ConstructorDefinition cons = this.Class.Constructor;

            if (cons.PrivateAnnotation != null)
            {
                string errorMessage = "The constructor for " + this.Class.NameToken.Value + " is private and cannot be invoked from outside the class.";
                if (cons.PrivateAnnotation.Args.Length > 0)
                {
                    StringConstant stringMessage = cons.PrivateAnnotation.Args[0] as StringConstant;
                    if (stringMessage != null)
                    {
                        errorMessage += " " + stringMessage.Value.Trim();
                    }
                }

                throw new ParserException(this.FirstToken, errorMessage);
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
                throw new ParserException(this.FirstToken, message);
            }

            return this;
        }

        internal override void SetLocalIdPass(VariableIdAllocator varIds)
        {
            for (int i = 0; i < this.Args.Length; ++i)
            {
                this.Args[i].SetLocalIdPass(varIds);
            }
        }

        internal override Expression ResolveNames(Parser parser, Dictionary<string, Executable> lookup, string[] imports)
        {
            this.BatchExpressionNameResolver(parser, lookup, imports, this.Args);

            Executable ex = Expression.DoNameLookup(lookup, imports, this.Name);
            if (ex == null)
            {
                string name = this.Name;
                string message = "No class named '" + name + "' was found.";
                if (name.Contains('.'))
                {
                    message += " Did you forget to import a library?";
                }
                throw new ParserException(this.NameToken, message);
            }

            if (ex is ClassDefinition)
            {
                this.Class = (ClassDefinition)ex;
            }
            else
            {
                throw new ParserException(this.NameToken, "This is not a class.");
            }

            return this;
        }

        internal override void GetAllVariablesReferenced(HashSet<Variable> vars)
        {
            throw new System.NotImplementedException();
        }
    }
}
