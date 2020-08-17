namespace Build.BuildParseNodes
{
    public class BuildRoot : BuildItem
    {
        public Target[] Targets { get; set; }

        public string ProgrammingLanguage { get; set; }
    }

}
