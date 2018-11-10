using System.Collections.Generic;
using System.Linq;

namespace Pastel.Nodes
{
    internal class EnumDefinition : ICompilationEntity
    {
        public CompilationEntityType EntityType { get { return CompilationEntityType.ENUM; } }

        public PastelContext Context { get; private set; }
        public Token FirstToken { get; set; }
        public Token NameToken { get; set; }
        public Token[] ValueTokens { get; set; }
        public Dictionary<string, Expression> ValuesByName { get; set; }

        public HashSet<string> UnresolvedValues = new HashSet<string>();

        public EnumDefinition(Token enumToken, Token nameToken, PastelContext context)
        {
            this.FirstToken = enumToken;
            this.NameToken = nameToken;
            this.Context = context;
        }

        internal void InitializeValues(IList<Token> valueTokens, IList<Expression> valueExpressions)
        {
            this.ValueTokens = valueTokens.ToArray();
            this.ValuesByName = new Dictionary<string, Expression>();
            int length = this.ValueTokens.Length;
            int highestValue = 0;
            bool highestSet = false;
            List<string> autoAssignMe = new List<string>();
            for (int i = 0; i < length; ++i)
            {
                string name = this.ValueTokens[i].Value;
                if (this.ValuesByName.ContainsKey(name))
                {
                    throw new ParserException(this.FirstToken, "The enum '" + this.NameToken.Value + "' has multiple definitions of '" + name + "'");
                }
                Expression expression = valueExpressions[i];
                if (expression == null)
                {
                    autoAssignMe.Add(name);
                }
                else
                {
                    this.ValuesByName[name] = expression;

                    if (expression is InlineConstant)
                    {
                        InlineConstant ic = (InlineConstant)expression;
                        if (ic.Value is int)
                        {
                            if (!highestSet || (int)ic.Value > highestValue)
                            {
                                highestValue = (int)ic.Value;
                                highestSet = true;
                            }
                        }
                        else
                        {
                            throw new ParserException(expression.FirstToken, "Only integers are allowed as enum values.");
                        }
                    }
                    else
                    {
                        this.UnresolvedValues.Add(name);
                    }
                }
            }

            // anything that doesn't have a value assigned to it, auto-assign incrementally from the highest value provided.
            foreach (string name in autoAssignMe)
            {
                this.ValuesByName[name] = new InlineConstant(PType.INT, this.FirstToken, highestValue++, this);
            }
        }

        public InlineConstant GetValue(Token name)
        {
            Expression value;
            if (this.ValuesByName.TryGetValue(name.Value, out value))
            {
                return (InlineConstant)value;
            }
            throw new ParserException(name, "The enum value '" + name.Value + "' does not exist in the definition of '" + this.NameToken.Value + "'.");
        }

        internal void DoConstantResolutions(HashSet<string> cycleDetection, PastelCompiler compiler)
        {
            string prefix = this.NameToken.Value + ".";
            foreach (string name in this.UnresolvedValues)
            {
                string cycleKey = prefix + name;
                if (cycleDetection.Contains(cycleKey))
                {
                    throw new ParserException(this.FirstToken, "This enum has a cycle in its value declarations in '" + name + "'");
                }
                cycleDetection.Add(cycleKey);

                InlineConstant ic = this.ValuesByName[cycleKey].DoConstantResolution(cycleDetection, compiler);
                if (!(ic.Value is int))
                {
                    throw new ParserException(ic.FirstToken, "Enum values must resolve into integers. This does not.");
                }

                this.ValuesByName[cycleKey] = ic;
                cycleDetection.Remove(cycleKey);
            }
        }
    }
}
