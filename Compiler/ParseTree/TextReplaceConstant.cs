using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Crayon.ParseTree
{
    /*
     * This is a temporary entity.
     * It is used by the translator specifically to translate Crayon into Pastel and preserve text replacement constants
     * such as %%%THINGS_LIKE_THIS%%% that appear in the source code and are generally replaced before the parser runs.
     */
    class TextReplaceConstant : Expression
    {
        internal override Expression PastelResolve(Parser parser)
        {
            return this;
        }

        public Token NameToken { get; set; }
        public string Name { get { return this.NameToken.Value; } }

        public TextReplaceConstant(Token firstToken, Token nameToken, Executable owner) : base(firstToken, owner)
        {
            this.NameToken = nameToken;
        }

        public override bool CanAssignTo
        {
            get { return false; }
        }

        internal override void GetAllVariablesReferenced(HashSet<Variable> vars) { }

        internal override void PerformLocalIdAllocation(VariableIdAllocator varIds, VariableIdAllocPhase phase) { }

        internal override Expression Resolve(Parser parser)
        {
            return this;
        }

        internal override Expression ResolveNames(Parser parser, Dictionary<string, Executable> lookup, string[] imports)
        {
            return this;
        }
    }
}
