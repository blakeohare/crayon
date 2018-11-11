namespace Pastel
{
    public class ExtensionMethodNotImplementedException : ParserException
    {
        internal ExtensionMethodNotImplementedException(Token throwToken, string message)
            : base(throwToken, message)
        { }
    }
}
