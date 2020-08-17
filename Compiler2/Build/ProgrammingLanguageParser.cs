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
                default: return null;
            }
        }
    }
}
