using Parser.Resolver;
using System.Collections.Generic;

namespace Parser.ParseTree
{
    public class ThisKeyword : Expression
    {
        internal override IEnumerable<Expression> Descendants { get { return Expression.NO_DESCENDANTS; } }

        public ThisKeyword(Token token, Node owner)
            : base(token, owner)
        { }

        internal override Expression Resolve(ParserContext parser)
        {
            return this;
        }

        internal static void CheckIfThisOrBaseIsValid(Expression thisOrBase, ParserContext parser)
        {
            TopLevelEntity container = thisOrBase.TopLevelEntity;
            string thisOrBaseString = thisOrBase.FirstToken.Value;

            FunctionDefinition funcDef = container as FunctionDefinition;
            if (funcDef != null)
            {
                if (funcDef.IsStaticMethod)
                {
                    throw new ParserException(thisOrBase, "Cannot use '" + thisOrBaseString + "' in a static method");
                }

                if (funcDef.Owner == null)
                {
                    throw new ParserException(thisOrBase, "Cannot use '" + thisOrBaseString + "' in a function that isn't a class method.");
                }
            }
            else
            {
                FieldDefinition fieldDef = container as FieldDefinition;
                if (fieldDef != null)
                {
                    if (fieldDef.IsStaticField)
                    {
                        throw new ParserException(thisOrBase, "Cannot use '" + thisOrBaseString + "' in a static field value.");
                    }
                }
                else
                {
                    ConstructorDefinition ctorDef = container as ConstructorDefinition;
                    if (ctorDef != null)
                    {
                        // TODO: This check is silly. Add an IsStatic field to ConstructorDefinition.
                        if (ctorDef == ((ClassDefinition)ctorDef.Owner).StaticConstructor)
                        {
                            throw new ParserException(thisOrBase, "Cannot use '" + thisOrBaseString + "' in a static constructor.");
                        }
                    }
                }
            }
        }

        internal override Expression ResolveEntityNames(ParserContext parser)
        {
            ThisKeyword.CheckIfThisOrBaseIsValid(this, parser);
            return this;
        }

        internal override Expression ResolveTypes(ParserContext parser, TypeResolver typeResolver)
        {
            Node walker = this.Owner;
            while (!(walker is ClassDefinition))
            {
                walker = walker.Owner;
                if (walker == null) throw new System.Exception(); // already verified exists in a ClassDefinition in ResolveEntityNames, so this should never happen.
            }

            this.ResolvedType = ResolvedType.GetInstanceType((ClassDefinition)walker);
            return this;
        }

        internal override void PerformLocalIdAllocation(ParserContext parser, VariableScope varIds, VariableIdAllocPhase phase) { }
    }
}
