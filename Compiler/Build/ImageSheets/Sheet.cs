using System.Collections.Generic;

namespace Build.ImageSheets
{
    public class Sheet
    {
        public Sheet()
        {
            this.Chunks = new List<Chunk>();
        }

        public string ID { get; set; }
        public List<Chunk> Chunks { get; set; }
    }
}
