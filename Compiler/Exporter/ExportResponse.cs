namespace Exporter
{
    public class ExportResponse
    {
        public string CbxOutputPath { get; set; }
        public Common.Error[] Errors { get; set; }
        public bool HasErrors { get { return this.Errors != null && this.Errors.Length > 0; } }
    }
}
