using System;
using System.Collections.Generic;

namespace Crayon.ParseTree
{
    internal class DotStepStruct : Expression
    {
        internal override Expression PastelResolve(Parser parser)
        {
            throw new NotImplementedException();
        }

        public override bool CanAssignTo { get { return true; } }

        public Token DotToken { get; private set; }
        public Expression RawRoot { get; private set; }
        public string RootVar { get; private set; }
        public string FieldName { get; private set; }
        public StructDefinition StructDefinition { get; private set; }

        public DotStepStruct(Token token, StructDefinition structDef, DotStep original, Executable owner)
            : base(token, owner)
        {
            this.DotToken = original.DotToken;
            this.RawRoot = original.Root;
            this.RootVar = "v_" + ((Variable)original.Root).Name.Split('$')[1];
            this.FieldName = original.StepToken.Value;
            this.StructDefinition = structDef;
        }

        internal override Expression Resolve(Parser parser)
        {
            return this;
        }

        internal override Expression ResolveNames(Parser parser, Dictionary<string, Executable> lookup, string[] imports)
        {
            throw new InvalidOperationException(); // translate mode only
        }

        internal override void GetAllVariablesReferenced(HashSet<Variable> vars) { }

        internal override void PerformLocalIdAllocation(VariableIdAllocator varIds, VariableIdAllocPhase phase)
        {
            throw new InvalidOperationException(); // translate mode only
        }
    }
}
