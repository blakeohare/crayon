﻿using System.Collections.Generic;

namespace Crayon.ImageSheets
{
    internal class Sheet
    {
        public Sheet()
        {
            this.Chunks = new List<Chunk>();
        }

        public string ID { get; set; }
        public List<Chunk> Chunks { get; set; }
    }
}
