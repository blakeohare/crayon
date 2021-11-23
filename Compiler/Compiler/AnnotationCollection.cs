using Common;
using Common.Localization;
using Parser.ParseTree;
using System.Collections.Generic;

namespace Parser
{
    internal class AnnotationCollection
    {
        private Dictionary<string, List<Annotation>> annotations = new Dictionary<string, List<Annotation>>();
        private ParserContext parser;

        internal AnnotationCollection(ParserContext parser)
        {
            this.parser = parser;
        }

        public void Add(Annotation annotation)
        {
            string type = annotation.Type;
            if (!annotations.ContainsKey(type)) annotations[type] = new List<Annotation>();
            annotations[type].Add(annotation);
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
                        throw this.parser.GenerateParseError(
                            ErrorMessages.MULTIPLE_PRIVATE_ANNOTATIONS,
                            privateAnnotation.FirstToken);
                    }
                }

                if (firstPrivate.Args.Length > 0)
                {
                    throw this.parser.GenerateParseError(
                        ErrorMessages.PRIVATE_ANNOTATION_HAS_ARGUMENT,
                        firstPrivate.FirstToken);
                }

                return true;
            }

            return false;
        }

        // Note: the output of this function is currently treated as mutable.
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
                        throw this.parser.GenerateParseError(
                            ErrorMessages.LOCALIZED_ANNOTATION_ARGUMENT_MUST_HAVE_2_STRINGS,
                            annotation.FirstToken);
                    }

                    string localeId = ((StringConstant)annotation.Args[0]).Value;
                    string name = ((StringConstant)annotation.Args[1]).Value;
                    int segmentCount = name.Contains('.') ? name.Split('.').Length : 1;
                    if (segmentCount != expectedSegments)
                    {
                        throw this.parser.GenerateParseError(
                            ErrorMessages.LOCALIZED_ANNOTATION_MUST_CONTAIN_SAME_NUMBER_DOTTED_SEGMENTS,
                            annotation.FirstToken);
                    }
                    Locale locale = Locale.Get(localeId);
                    output[locale] = name;
                }
            }
            return output;
        }
    }
}
