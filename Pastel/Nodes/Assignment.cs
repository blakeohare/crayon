using System;
using System.Collections.Generic;

namespace Pastel.Nodes
{
    class Assignment : Executable, ICompilationEntity
    {
        public CompilationEntityType EntityType { get { return CompilationEntityType.GLOBAL; } }

        public Expression Target { get; set; }
        public Token OpToken { get; set; }
        public Expression Value { get; set; }

        public Assignment(
            Expression target,
            Token opToken,
            Expression value) : base(target.FirstToken)
        {
            this.Target = target;
            this.OpToken = opToken;
            this.Value = value;
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
