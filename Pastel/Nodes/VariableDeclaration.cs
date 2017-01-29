using System;
using System.Collections.Generic;

namespace Pastel.Nodes
{
    class VariableDeclaration : Executable, ICompilationEntity
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

        public override IList<Executable> NameResolution(Dictionary<string, FunctionDefinition> functionLookup, Dictionary<string, StructDefinition> structLookup)
        {
            throw new NotImplementedException();
        }

        public override void ResolveTypes()
        {
            throw new NotImplementedException();
        }
    }
}
