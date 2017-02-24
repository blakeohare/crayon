using System;
using System.Collections.Generic;

namespace Pastel.Nodes
{
    public class VariableDeclaration : Executable, ICompilationEntity
    {
        public CompilationEntityType EntityType
        {
            get
            {
                if (this.IsConstant) return CompilationEntityType.CONSTANT;
                if (this.IsGlobal) return CompilationEntityType.GLOBAL;
                throw new Exception(); // this shouldn't have been a top-level thing.
            }
        }

        public PType Type { get; set; }
        public Token VariableName { get; set; }
        public Token EqualsToken { get; set; }
        public Expression Value { get; set; }

        public bool IsConstant { get; set; }
        public bool IsGlobal { get; set; }

        public VariableDeclaration(
            PType type,
            Token variableName,
            Token equalsToken,
            Expression assignmentValue) : base(type.FirstToken)
        {
            this.Type = type;
            this.VariableName = variableName;
            this.EqualsToken = equalsToken;
            this.Value = assignmentValue;
        }

        public override IList<Executable> ResolveNamesAndCullUnusedCode(PastelCompiler compiler)
        {
            this.Value = this.Value.ResolveNamesAndCullUnusedCode(compiler);

            return Listify(this);
        }

        public void DoConstantResolutions(HashSet<string> cycleDetection, PastelCompiler compiler)
        {
            this.Value = this.Value.DoConstantResolution(cycleDetection, compiler);
        }

        internal override void ResolveTypes(VariableScope varScope, PastelCompiler compiler)
        {
            this.Value = this.Value.ResolveType(varScope, compiler);

            if (!PType.CheckAssignment(this.Type, this.Value.ResolvedType))
            {
                throw new ParserException(this.Value.FirstToken, "Cannot assign this type to a " + this.Type);
            }

            varScope.DeclareVariables(this.VariableName, this.Type);
        }

        internal override Executable ResolveWithTypeContext(PastelCompiler compiler)
        {
            if (this.Value != null)
            {
                this.Value = this.Value.ResolveWithTypeContext(compiler);
            }
            return this;
        }
    }
}
