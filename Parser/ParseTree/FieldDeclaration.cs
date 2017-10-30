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

        public FieldDeclaration(Token fieldToken, Token nameToken, ClassDefinition owner, bool isStatic)
            : base(fieldToken, owner, owner.FileScope)
        {
            this.NameToken = nameToken;
            this.DefaultValue = new NullConstant(fieldToken, owner);
            this.IsStaticField = isStatic;
            this.MemberID = -1;
        }

        internal override void Resolve(ParserContext parser)
        {
            this.DefaultValue = this.DefaultValue.Resolve(parser);
        }

        internal override void ResolveNames(ParserContext parser)
        {
            parser.CurrentCodeContainer = this;
            this.DefaultValue = this.DefaultValue.ResolveNames(parser);
            parser.CurrentCodeContainer = null;
        }

        internal override void GetAllVariablesReferenced(HashSet<Variable> vars) { }

        internal override void PerformLocalIdAllocation(ParserContext parser, VariableIdAllocator varIds, VariableIdAllocPhase phase)
        {
            // Throws if it finds any variable.
            this.DefaultValue.PerformLocalIdAllocation(parser, varIds, phase);
        }
    }
}
