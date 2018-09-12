namespace Parser.Acrylic
{
    internal class AcrylicAnnotationParser : AbstractAnnotationParser
    {
        private ParserContext parser;
        public AcrylicAnnotationParser(ParserContext parser)
        {
            this.parser = parser;
        }

        internal override AnnotationCollection ParseAnnotations(TokenStream tokens)
        {
            throw new System.NotImplementedException();
        }
    }
}
