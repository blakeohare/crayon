﻿using Localization;
using Parser.Resolver;
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
        public AType FieldType { get; private set; }
        public ResolvedType ResolvedFieldType { get; private set; }

        public FieldDefinition(Token fieldToken, AType fieldType, Token nameToken, ClassDefinition owner, bool isStatic, AnnotationCollection annotations)
            : base(fieldToken, owner, owner.FileScope)
        {
            this.NameToken = nameToken;
            this.FieldType = fieldType;
            this.DefaultValue = null;
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
            if (this.DefaultValue != null)
            {
                parser.CurrentCodeContainer = this;
                this.DefaultValue = this.DefaultValue.ResolveEntityNames(parser);
                parser.CurrentCodeContainer = null;
            }
        }

        internal override void ResolveSignatureTypes(ParserContext parser, TypeResolver typeResolver)
        {
            this.ResolvedFieldType = typeResolver.ResolveType(this.FieldType);
            if (this.DefaultValue == null)
            {
                switch (this.ResolvedFieldType.Category)
                {
                    case ResolvedTypeCategory.INTEGER:
                        this.DefaultValue = new IntegerConstant(this.FirstToken, 0, this);
                        break;
                    case ResolvedTypeCategory.FLOAT:
                        this.DefaultValue = new FloatConstant(this.FirstToken, 0.0, this);
                        break;
                    case ResolvedTypeCategory.BOOLEAN:
                        this.DefaultValue = new BooleanConstant(this.FirstToken, false, this);
                        break;
                    default:
                        this.DefaultValue = new NullConstant(this.FirstToken, this);
                        break;
                }
            }
        }

        internal override void ResolveTypes(ParserContext parser, TypeResolver typeResolver)
        {
            this.DefaultValue.ResolveTypes(parser, typeResolver);
            this.DefaultValue.ResolvedType.EnsureCanAssignToA(this.DefaultValue.FirstToken, this.ResolvedFieldType);
        }

        internal void ResolveVariableOrigins(ParserContext parser)
        {
            if (this.DefaultValue != null)
            {
                VariableScope varScope = VariableScope.NewEmptyScope(this.CompilationScope.IsStaticallyTyped);
                this.DefaultValue.ResolveVariableOrigins(parser, varScope, VariableIdAllocPhase.REGISTER_AND_ALLOC);

                if (varScope.Size > 0)
                {
                    // Although if you manage to trigger this, I'd love to know how.
                    throw new ParserException(this, "Cannot declare a variable this way.");
                }

                Lambda.DoVarScopeIdAllocationForLambdaContainer(parser, varScope, this);
            }
        }
    }
}
