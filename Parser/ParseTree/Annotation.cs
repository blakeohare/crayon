using Common;
using Localization;
using System.Collections.Generic;
using System.Linq;

namespace Parser.ParseTree
{
    public class Annotation
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
        
        public static Dictionary<Locale, string> GetNamesByLocale(Multimap<string, Annotation> annotations, int expectedSegments)
        {
            TODO.MoreExtensibleFormOfParsingAnnotations();

            Dictionary<Locale, string> output = new Dictionary<Locale, string>();
            if (annotations != null && annotations.ContainsKey("localized"))
            {
                foreach (Annotation annotation in annotations["localized"])
                {
                    if (annotation.Args.Length != 2 ||
                        !(annotation.Args[0] is StringConstant) ||
                        !(annotation.Args[1] is StringConstant))
                    {
                        throw new ParserException(
                            annotation.FirstToken,
                            "@localized argument must have 2 constant string arguments.");
                    }

                    string localeId = ((StringConstant)annotation.Args[0]).Value;
                    string name = ((StringConstant)annotation.Args[1]).Value;
                    int segmentCount = name.Contains('.') ? name.Split('.').Length : 1;
                    if (segmentCount != expectedSegments)
                    {
                        throw new ParserException(
                            annotation.Args[1].FirstToken, 
                            "@localized name must contain the same number of dotted segments as the original definition.");
                    }
                    Locale locale = Locale.Get(localeId);
                    output[locale] = name;
                }
            }
            return output;
        }
    }
}
