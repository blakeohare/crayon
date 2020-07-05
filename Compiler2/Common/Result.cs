namespace Common
{
    public class Result
    {
        public bool IsOkay { get { return !this.HasErrors; } }
        public Error[] Errors { get; set; }
        public bool HasErrors { get { return this.Errors != null && this.Errors.Length > 0; } }
    }
}
