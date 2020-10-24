using Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Build.ImageSheets
{
    class Image2
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
