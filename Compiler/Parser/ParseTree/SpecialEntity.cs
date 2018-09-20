using Parser.Resolver;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Parser.ParseTree
{
    public abstract class SpecialEntity : Expression
    {
        public SpecialEntity(Token firstToken, Node owner) : base(firstToken, owner)
        { }

        public override bool CanAssignTo { get { return false; } }

        internal override IEnumerable<Expression> Descendants { get { return Expression.NO_DESCENDANTS; } }

        internal override Expression ResolveEntityNames(ParserContext parser)
        {
            return this;
        }

        internal override Expression Resolve(ParserContext parser)
        {
            // Needs to be optimized out before resolving.
            throw new InvalidOperationException();
        }

        // No variables are assumed.
        internal override void PerformLocalIdAllocation(ParserContext parser, VariableScope varIds, VariableIdAllocPhase phase) { }

        internal class EnumMaxFunction : SpecialEntity
        {
            private EnumDefinition enumDef;
            public EnumMaxFunction(Token firstToken, EnumDefinition enumDef, Node owner)
                : base(firstToken, owner)
            {
                this.enumDef = enumDef;
            }

            public int GetMax()
            {
                int[] values = EnumValuesFunction.GetEnumValues(this.enumDef);
                int max = values[0];
                for (int i = 1; i < values.Length; ++i)
                {
                    if (values[i] > max)
                    {
                        max = values[i];
                    }
                }
                return max;
            }

            internal override Expression ResolveTypes(ParserContext parser, TypeResolver typeResolver)
            {
                throw new System.NotImplementedException();
            }
        }

        internal class EnumValuesFunction : SpecialEntity
        {
            private EnumDefinition enumDef;
            public EnumValuesFunction(Token firstToken, EnumDefinition enumDef, Node owner)
                : base(firstToken, owner)
            {
                this.enumDef = enumDef;
            }

            internal static int[] GetEnumValues(EnumDefinition enumDef)
            {
                return enumDef.Items
                    .Select<Token, int>(token => enumDef.IntValue[token.Value])
                    .ToArray();
            }

            public int[] GetValues()
            {
                return GetEnumValues(this.enumDef);
            }

            internal override Expression ResolveTypes(ParserContext parser, TypeResolver typeResolver)
            {
                throw new System.NotImplementedException();
            }
        }
    }
}
