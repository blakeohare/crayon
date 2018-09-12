namespace Parser.Acrylic
{
    internal class AnnotationParser : IAnnotationParser
    {
        private ParserContext parser;
        public AnnotationParser(ParserContext parser)
        {
            this.parser = parser;
        }

        public AnnotationCollection ParseAnnotations(TokenStream tokens)
        {
            throw new System.NotImplementedException();
        }
    }
}
