namespace Build
{
    public static class ProgrammingLanguageParser
    {
        public static ProgrammingLanguage? Parse(string value)
        {
            switch (value.ToLowerInvariant())
            {
                case "acrylic": return ProgrammingLanguage.ACRYLIC;
                case "crayon": return ProgrammingLanguage.CRAYON;
                default: return null;
            }
        }
    }

    public enum ProgrammingLanguage
    {
        ACRYLIC,
        CRAYON,
    }
}
