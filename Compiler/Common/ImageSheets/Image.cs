namespace Common.ImageSheets
{
    public class Image
    {
        public Image(FileOutput file)
        {
            this.OriginalFile = file;
            this.Bitmap = file.Bitmap;
            this.OriginalPath = file.OriginalPath;
            this.Width = this.Bitmap.Width;
            this.Height = this.Bitmap.Height;
        }

        public FileOutput OriginalFile { get; set; }
        public string OriginalPath { get; set; }
        public SystemBitmap Bitmap { get; set; }
        public int ChunkX { get; set; }
        public int ChunkY { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }
}
