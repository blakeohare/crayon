namespace Crayon
{
    internal static class ParserUtil
    {
        public static string PopClassNameWithFirstTokenAlreadyPopped(TokenStream tokens, Token firstToken)
        {
            Parser.VerifyIdentifier(firstToken);
            string name = firstToken.Value;
            while (tokens.PopIfPresent("."))
            {
                Token nameNext = tokens.Pop();
                Parser.VerifyIdentifier(nameNext);
                name += "." + nameNext.Value;
            }
            return name;
        }

    }
}
