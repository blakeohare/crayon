using Common;
using Localization;
using Parser.ParseTree;
using System.Collections.Generic;
using System.Linq;

namespace Parser
{
    public class AnnotationCollection
    {
        private Multimap<string, Annotation> annotations = new Multimap<string, Annotation>();
        private ParserContext parser;

        public AnnotationCollection(ParserContext parser)
        {
            this.parser = parser;
        }

        public void Add(Annotation annotation)
        {
            this.annotations.Add(annotation.Type, annotation);
        }

        public void Validate()
        {

        }

        public bool IsPrivate()
        {
            string key = this.parser.Keywords.PRIVATE;
            if (this.annotations.ContainsKey(key))
            {
                Annotation firstPrivate = null;
                foreach (Annotation privateAnnotation in this.annotations[key])
                {
                    if (firstPrivate == null)
                    {
                        firstPrivate = privateAnnotation;
                    }
                    else
                    {
                        throw new ParserException(privateAnnotation.FirstToken, "Multiple @private annotations");
                    }
                }

                if (firstPrivate.Args.Length > 0)
                {
                    throw new ParserException(firstPrivate.FirstToken, "@private does not take any arguments.");
                }

                return true;
            }
            return false;
        }

        public Dictionary<Locale, string> GetNamesByLocale(int expectedSegments)
        {
            TODO.MoreExtensibleFormOfParsingAnnotations();

            Dictionary<Locale, string> output = new Dictionary<Locale, string>();
            if (annotations != null && this.annotations.ContainsKey("localized"))
            {
                foreach (Annotation annotation in this.annotations["localized"])
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
