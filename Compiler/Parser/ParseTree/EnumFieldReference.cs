using Parser.Resolver;
using System;
using System.Collections.Generic;

namespace Parser.ParseTree
{
    internal class EnumFieldReference : Expression
    {
        public EnumDefinition EnumDefinition { get; private set; }
        public string Field { get; private set; }
        public Token FieldToken { get; private set; }

        public EnumFieldReference(Token firstToken, EnumDefinition enumDef, Token fieldToken, Node owner)
            : base(firstToken, owner)
        {
            this.EnumDefinition = enumDef;
            this.FieldToken = fieldToken;
            this.Field = fieldToken.Value;
            this.ResolvedType = ResolvedType.INTEGER;
        }

        internal override IEnumerable<Expression> Descendants { get { return Expression.NO_DESCENDANTS; } }

        internal override Expression ResolveEntityNames(ParserContext parser)
        {
            throw new NotImplementedException();
        }

        internal override void ResolveVariableOrigins(ParserContext parser, VariableScope varIds, VariableIdAllocPhase phase)
        { }

        internal override Expression ResolveTypes(ParserContext parser, TypeResolver typeResolver)
        {
            return this;
        }

        internal override Expression Resolve(ParserContext parser)
        {
            ConstantResolutionState resolutionState = parser.ConstantAndEnumResolutionState[this.EnumDefinition];
            if (resolutionState != ConstantResolutionState.RESOLVED)
            {
                this.EnumDefinition.Resolve(parser);
            }

            if (this.EnumDefinition.IntValue.ContainsKey(this.Field))
            {
                return new IntegerConstant(this.FirstToken, this.EnumDefinition.IntValue[this.Field], this.Owner);
            }
            throw new ParserException(this.FieldToken, "The enum '" + this.EnumDefinition.Name + "' does not contain a definition for '" + this.Field + "'");
        }
    }
}
