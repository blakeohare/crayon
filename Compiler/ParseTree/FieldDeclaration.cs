using System;
using System.Collections.Generic;

namespace Crayon.ParseTree
{
    internal class FieldDeclaration : Executable
    {
        public Token NameToken { get; set; }
        public Expression DefaultValue { get; set; }
        public bool IsStaticField { get; private set; }
        public int MemberID { get; set; }
        public int StaticMemberID { get; set; }

        public FieldDeclaration(Token fieldToken, Token nameToken, ClassDefinition owner, bool isStatic)
            : base(fieldToken, owner)
        {
            this.NameToken = nameToken;
            this.DefaultValue = new NullConstant(fieldToken, owner);
            this.IsStaticField = isStatic;
            this.MemberID = -1;
        }

        internal override IList<Executable> Resolve(Parser parser)
        {
            this.DefaultValue = this.DefaultValue.Resolve(parser);
            return Listify(this);
        }

        internal override Executable ResolveNames(Parser parser, Dictionary<string, Executable> lookup, string[] imports)
        {
            parser.CurrentCodeContainer = this;
            this.DefaultValue = this.DefaultValue.ResolveNames(parser, lookup, imports);
            parser.CurrentCodeContainer = null;

            return this;
        }

        internal override void GetAllVariablesReferenced(HashSet<Variable> vars) { }

        internal override void PerformLocalIdAllocation(Parser parser, VariableIdAllocator varIds, VariableIdAllocPhase phase)
        {
            // Throws if it finds any variable.
            this.DefaultValue.PerformLocalIdAllocation(parser, varIds, phase);
        }

        internal override Executable PastelResolve(Parser parser)
        {
            throw new NotImplementedException();
        }
    }
}
