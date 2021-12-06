namespace Builder
{
    internal static class ProgrammingLanguageParser
    {
        internal static ProgrammingLanguage? Parse(string value)
        {
            switch (value.ToLowerInvariant())
            {
                case "acrylic": return ProgrammingLanguage.ACRYLIC;
                case "crayon": return ProgrammingLanguage.CRAYON;
                default: return null;
            }
        }

        internal static string LangToFileExtension(ProgrammingLanguage lang)
        {
            switch (lang)
            {
                case ProgrammingLanguage.ACRYLIC: return ".acr";
                case ProgrammingLanguage.CRAYON: return ".cry";
                default: throw new System.InvalidOperationException();
            }
        }
    }
}
