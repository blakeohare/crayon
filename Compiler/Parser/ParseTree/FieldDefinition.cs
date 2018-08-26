using Localization;
using System.Collections.Generic;

namespace Parser.ParseTree
{
    public class FieldDefinition : TopLevelEntity, ICodeContainer
    {
        public Token NameToken { get; set; }
        public Expression DefaultValue { get; set; }
        public bool IsStaticField { get; private set; }
        public int MemberID { get; set; }
        public int StaticMemberID { get; set; }
        public AnnotationCollection Annotations { get; set; }
        public List<Lambda> Lambdas { get; private set; }

        public FieldDefinition(Token fieldToken, Token nameToken, ClassDefinition owner, bool isStatic, AnnotationCollection annotations)
            : base(fieldToken, owner, owner.FileScope)
        {
            this.NameToken = nameToken;
            this.DefaultValue = new NullConstant(fieldToken, this);
            this.IsStaticField = isStatic;
            this.MemberID = -1;
            this.Annotations = annotations;
            this.Lambdas = new List<Lambda>();
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

        internal void AllocateLocalScopeIds(ParserContext parser)
        {
            VariableScope varScope = VariableScope.NewEmptyScope();
            this.DefaultValue.PerformLocalIdAllocation(parser, varScope, VariableIdAllocPhase.REGISTER_AND_ALLOC);

            if (varScope.Size > 0)
            {
                // Although if you manage to trigger this, I'd love to know how.
                throw new ParserException(this, "Cannot declare a variable this way.");
            }

            Lambda.DoVarScopeIdAllocationForLambdaContainer(parser, varScope, this);
        }
    }
}
