using System;
using System.Collections.Generic;
using System.Linq;

namespace Pastel.Nodes
{
    class EnumDefinition : ICompilationEntity
    {
        public CompilationEntityType EntityType { get { return CompilationEntityType.ENUM; } }

        public Token FirstToken { get; set; }
        public Token NameToken { get; set; }
        public Token[] ValueTokens { get; set; }
        public Expression[] ValueExpressions { get; set; }

        public EnumDefinition(Token enumToken, Token nameToken, IList<Token> valueTokens, IList<Expression> valueExpressions)
        {
            this.FirstToken = enumToken;
            this.NameToken = nameToken;
            this.ValueTokens = valueTokens.ToArray();
            this.ValueExpressions = valueExpressions.ToArray();
        }
    }
}
