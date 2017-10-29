using System.Collections.Generic;
using System.Linq;

namespace Crayon.ParseTree
{
    internal class Annotation
    {
        public Token FirstToken { get; private set; }
        public Token TypeToken { get; private set; }
        public string Type { get; private set; }
        public Expression[] Args { get; private set; }

        public Annotation(Token firstToken, Token typeToken, IList<Expression> args)
        {
            this.FirstToken = firstToken;
            this.TypeToken = typeToken;
            this.Type = typeToken.Value;
            this.Args = args.ToArray();
        }

        public string GetSingleArgAsString(ParserContext parser)
        {
            if (this.Args.Length != 1) throw new ParserException(this.TypeToken, "This annotation requires exactly 1 arg.");
            StringConstant stringConstant = this.Args[0].Resolve(parser) as StringConstant;
            if (stringConstant == null) throw new ParserException(this.Args[0].FirstToken, "This annotation requires exactly 1 string arg.");
            return stringConstant.Value;
        }
    }
}
