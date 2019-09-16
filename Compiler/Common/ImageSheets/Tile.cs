namespace Common.ImageSheets
{
    public class Tile
    {
        public string GeneratedFilename { get; set; }
        public CommonUtil.Images.Bitmap Bitmap { get; set; }
        public int ChunkX { get; set; }
        public int ChunkY { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int Bytes { get; set; }

        // JPEGs and standalone PNG's don't need to be serialized out to an in-memory bitmap and then re-encoded.
        public bool IsDirectCopy { get; set; }
        public FileOutput OriginalFile { get; set; }
    }
}
