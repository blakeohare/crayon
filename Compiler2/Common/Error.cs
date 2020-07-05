namespace Common
{
    public class Error
    {
        public int Line { get; set; }
        public int Column { get; set; }
        public string FileName { get; set; }
        public string Message { get; set; }
        public bool HasLineInfo { get { return this.Line != 0; } }
    }
}
