using System.Collections.Generic;

namespace Crayon.ParseTree
{
    class CompileTimeDictionary : Expression
    {
        public override bool CanAssignTo { get { return false; } }

        public string Type { get; private set; }

        public CompileTimeDictionary(Token firstToken, string type, Executable owner)
            : base(firstToken, owner)
        {
            this.Type = type;
        }

        internal override Expression Resolve(Parser parser)
        {
            return this;
        }

        internal override void SetLocalIdPass(VariableIdAllocator varIds) { }

        internal override Expression ResolveNames(Parser parser, Dictionary<string, Executable> lookup, string[] imports)
        {
            return this;
        }

        internal override void GetAllVariablesReferenced(HashSet<Variable> vars) { }
    }
}
