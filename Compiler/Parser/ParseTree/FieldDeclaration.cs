using Localization;
using System.Collections.Generic;

namespace Parser.ParseTree
{
    public class FieldDeclaration : TopLevelConstruct
    {
        public Token NameToken { get; set; }
        public Expression DefaultValue { get; set; }
        public bool IsStaticField { get; private set; }
        public int MemberID { get; set; }
        public int StaticMemberID { get; set; }
        public AnnotationCollection Annotations { get; set; }

        public FieldDeclaration(Token fieldToken, Token nameToken, ClassDefinition owner, bool isStatic, AnnotationCollection annotations)
            : base(fieldToken, owner, owner.FileScope)
        {
            this.NameToken = nameToken;
            this.DefaultValue = new NullConstant(fieldToken, this);
            this.IsStaticField = isStatic;
            this.MemberID = -1;
            this.Annotations = annotations;
        }

        public override string GetFullyQualifiedLocalizedName(Locale locale)
        {
            string name = this.NameToken.Value;
            if (this.TopLevelEntity != null)
            {
                name = this.TopLevelEntity.GetFullyQualifiedLocalizedName(locale) + "." + name;
            }
            return name;
        }

        internal override void Resolve(ParserContext parser)
        {
            this.DefaultValue = this.DefaultValue.Resolve(parser);
        }

        internal override void ResolveEntityNames(ParserContext parser)
        {
            parser.CurrentCodeContainer = this;
            this.DefaultValue = this.DefaultValue.ResolveEntityNames(parser);
            parser.CurrentCodeContainer = null;
        }

        private static readonly VariableScope EMPTY_VAR_ALLOC = VariableScope.NewEmptyScope();
        internal void AllocateLocalScopeIds(ParserContext parser)
        {
            // Throws if it finds any variable.
            this.DefaultValue.PerformLocalIdAllocation(parser, EMPTY_VAR_ALLOC, VariableIdAllocPhase.ALLOC);
        }
    }
}
