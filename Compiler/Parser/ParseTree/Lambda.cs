using System;
using System.Collections.Generic;
using System.Linq;

namespace Parser.ParseTree
{
    public class Lambda : Expression
    {
        public Token[] Args { get; private set; }
        public Executable[] Code { get; private set; }

        public override bool CanAssignTo { get { return false; } }

        public Lambda(Token firstToken, TopLevelConstruct owner, IList<Expression> args, IList<Executable> code)
            : base(firstToken, owner)
        {
            // TODO: ugh, change this at parse-time.
            this.Args = args.Select(arg => arg.FirstToken).ToArray();
            this.Code = code.ToArray();
        }
        
        internal override void PerformLocalIdAllocation(ParserContext parser, VariableIdAllocator varIds, VariableIdAllocPhase phase)
        {
            throw new NotImplementedException();
        }

        internal override Expression Resolve(ParserContext parser)
        {
            List<Executable> newCode = new List<Executable>();
            foreach (Executable ex in this.Code)
            {
                newCode.AddRange(ex.Resolve(parser));
            }
            this.Code = newCode.ToArray();

            return this;
        }

        internal override Expression ResolveEntityNames(ParserContext parser)
        {
            for (int i = 0; i < this.Code.Length; ++i)
            {
                this.Code[i] = this.Code[i].ResolveEntityNames(parser);
            }

            return this;
        }

        internal override void GetAllVariablesReferenced(HashSet<Variable> vars)
        {
            throw new NotImplementedException();
        }
    }
}
