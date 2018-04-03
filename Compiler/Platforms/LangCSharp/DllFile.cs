namespace LangCSharp
{
    /*
     * Converts an ExportEntity into an easily readable form.
     */
    public class DllFile
    {
        public DllFile(Platform.ExportEntity exportEntity)
        {
            this.HintPath = exportEntity.Values["hintpath"];
            this.FileOutput = exportEntity.FileOutput;
            this.IsStrongReference = exportEntity.Values.ContainsKey("name");
            if (this.IsStrongReference)
            {
                this.Name = exportEntity.Values["name"];
                this.Version = exportEntity.Values["version"];
                this.Culture = exportEntity.Values["culture"];
                this.PublicKeyToken = exportEntity.Values["token"];
                this.ProcessorArchitecture = exportEntity.Values["architecture"];
                this.SpecificVersion = exportEntity.Values["specificversion"].ToLower() == "true";
            }
        }

        public bool IsStrongReference { get; private set; }

        public string Name { get; private set; }
        public string Version { get; private set; }
        public string Culture { get; private set; }
        public string PublicKeyToken { get; private set; }
        public bool SpecificVersion { get; private set; }
        public string HintPath { get; private set; }
        public string ProcessorArchitecture { get; private set; }

        public Common.FileOutput FileOutput { get; private set; }
    }
}
