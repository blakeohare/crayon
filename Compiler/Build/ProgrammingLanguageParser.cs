namespace Build
{
    public static class ProgrammingLanguageParser
    {
        public static Common.ProgrammingLanguage? Parse(string value)
        {
            switch (value.ToLowerInvariant())
            {
                case "acrylic": return Common.ProgrammingLanguage.ACRYLIC;
                case "crayon": return Common.ProgrammingLanguage.CRAYON;
                case "python": return Common.ProgrammingLanguage.PYTHON;
                default: return null;
            }
        }

        public static string LangToFileExtension(Common.ProgrammingLanguage lang)
        {
            switch (lang)
            {
                case Common.ProgrammingLanguage.ACRYLIC: return ".acr";
                case Common.ProgrammingLanguage.CRAYON: return ".cry";
                case Common.ProgrammingLanguage.PYTHON: return ".py";
                default: throw new System.InvalidOperationException();
            }
        }
    }
}
