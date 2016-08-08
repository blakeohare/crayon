using System;
using System.Collections.Generic;
using System.Linq;

namespace Crayon.ParseTree
{
    internal class StructDefinition : Executable
    {
        public Token Name { get; private set; }
        public Token[] Fields { get; private set; }
        public Annotation[] Types { get; private set; }

        public StructDefinition(Token structToken, Token nameToken, IList<Token> fields, IList<Annotation> annotations, Executable owner)
            : base(structToken, owner)
        {
            this.Name = nameToken;
            this.Fields = fields.ToArray();
            this.FieldsByIndex = fields.Select<Token, string>(t => t.Value).ToArray();
            this.IndexByField = new Dictionary<string, int>();
            for (int i = 0; i < this.FieldsByIndex.Length; ++i)
            {
                this.IndexByField[this.FieldsByIndex[i]] = i;
            }

            this.Types = annotations.ToArray();
        }

        public string[] FieldsByIndex { get; private set; }
        public Dictionary<string, int> IndexByField { get; private set; }

        internal override IList<Executable> Resolve(Parser parser)
        {
            if (parser.IsByteCodeMode)
            {
                throw new ParserException(this.FirstToken, "Structs are not allowed in byte code mode.");
            }

            parser.AddStructDefinition(this);

            return new Executable[0];
        }

        internal override Executable ResolveNames(Parser parser, Dictionary<string, Executable> lookup, string[] imports)
        {
            throw new System.NotImplementedException();
        }

        internal override void GetAllVariablesReferenced(HashSet<Variable> vars) { }

        internal override void PerformLocalIdAllocation(VariableIdAllocator varIds, VariableIdAllocPhase phase)
        {
            throw new InvalidOperationException(); // translate mode only
        }
    }
}
