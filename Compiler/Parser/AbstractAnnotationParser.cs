namespace Parser
{
    internal abstract class AbstractAnnotationParser
    {
        internal abstract AnnotationCollection ParseAnnotations(TokenStream tokens);
    }
}
