using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Crayon.ParseTree
{
    public abstract class SpecialEntity : Expression
    {
        internal override Expression PastelResolve(ParserContext parser)
        {
            throw new NotImplementedException();
        }

        public SpecialEntity(Token firstToken, TopLevelConstruct owner) : base(firstToken, owner)
        { }

        public override bool CanAssignTo { get { return false; } }
        internal override void GetAllVariablesReferenced(HashSet<Variable> vars) { }

        internal override Expression ResolveNames(ParserContext parser)
        {
            return this;
        }

        internal override Expression Resolve(ParserContext parser)
        {
            // Needs to be optimized out before resolving.
            throw new InvalidOperationException();
        }

        // No variables are assumed.
        internal override void PerformLocalIdAllocation(ParserContext parser, VariableIdAllocator varIds, VariableIdAllocPhase phase) { }

        internal class EnumMaxFunction : SpecialEntity
        {
            private EnumDefinition enumDef;
            public EnumMaxFunction(Token firstToken, EnumDefinition enumDef, TopLevelConstruct owner)
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
        }

        internal class EnumValuesFunction : SpecialEntity
        {
            private EnumDefinition enumDef;
            public EnumValuesFunction(Token firstToken, EnumDefinition enumDef, TopLevelConstruct owner)
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
        }
    }
}
