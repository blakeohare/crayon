using Wax;

namespace Builder.ImageSheets
{
    class Image
    {
        public string Path { get; set; }
        public FileOutput File { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public bool IsJpeg { get; set; }

        public string Module
        {
            get
            {
                return this.Path.Split(':')[0];
            }
        }
    }
}
