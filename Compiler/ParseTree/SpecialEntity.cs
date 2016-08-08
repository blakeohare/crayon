using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Crayon.ParseTree
{
    internal abstract class SpecialEntity : Expression
    {
        public SpecialEntity(Token firstToken, Executable owner) : base(firstToken, owner)
        { }

        public override bool CanAssignTo { get { return false; } }
        internal override void GetAllVariablesReferenced(HashSet<Variable> vars) { }

        internal override Expression ResolveNames(Parser parser, Dictionary<string, Executable> lookup, string[] imports)
        {
            return this;
        }

        internal override Expression Resolve(Parser parser)
        {
            // Needs to be optimized out before resolving.
            throw new InvalidOperationException();
        }

        // No variables are assumed.
        internal override void PerformLocalIdAllocation(VariableIdAllocator varIds, VariableIdAllocPhase phase) { }

        internal class EnumMaxFunction : SpecialEntity
        {
            private EnumDefinition enumDef;
            public EnumMaxFunction(Token firstToken, EnumDefinition enumDef, Executable owner)
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
            public EnumValuesFunction(Token firstToken, EnumDefinition enumDef, Executable owner)
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
