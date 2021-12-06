using System.Collections.Generic;
using System.Linq;

namespace Builder.ParseTree
{
    internal class Annotation : Node
    {
        public Token TypeToken { get; private set; }
        public string Type { get; private set; }
        public Expression[] Args { get; private set; }

        public Annotation(Token firstToken, Token typeToken)
            : base(firstToken, null)
        {
            this.TypeToken = typeToken;
            this.Type = typeToken.Value;
        }

        public void SetArgs(IList<Expression> args)
        {
            this.Args = args.ToArray();
        }

        internal string GetSingleArgAsString(ParserContext parser)
        {
            if (this.Args.Length != 1) throw new ParserException(this.TypeToken, "This annotation requires exactly 1 arg.");
            StringConstant stringConstant = this.Args[0].Resolve(parser) as StringConstant;
            if (stringConstant == null) throw new ParserException(this.Args[0], "This annotation requires exactly 1 string arg.");
            return stringConstant.Value;
        }
    }
}
