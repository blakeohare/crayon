namespace Parser
{
    interface IAnnotationParser
    {
        AnnotationCollection ParseAnnotations(TokenStream tokens);
    }
}
