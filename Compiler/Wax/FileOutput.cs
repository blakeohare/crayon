using CommonUtil.Disk;

namespace Wax
{
    public enum FileOutputType
    {
        Text = 1,
        Binary = 2,
        Copy = 3,
        Image = 4,
    }

    public class FileOutput
    {
        public FileOutputType Type { get; set; }

        public string RelativeInputPath { get; set; }
        public string AbsoluteInputPath { get; set; }
        public string TextContent { get; set; }
        public byte[] BinaryContent { get; set; }
        public CommonUtil.Images.Bitmap Bitmap { get; set; }
        public bool IsLossy { get; set; } // e.g. JPEG's will change if re-encoded.

        // Original path relative to the root of the source directory.
        // This is the virtualized location of the embedded resource.
        public string OriginalPath { get; set; }

        // An auto-assigned filename that doesn't have special characters.
        public string CanonicalFileName { get; set; }

        // For text file types, ensure that there is no BOM in the output.
        // BOM's are automatically removed from .java files
        // TODO: change this to an enum: { DEFAULT, PRESENT, ABSENT }
        public bool TrimBomIfPresent { get; set; }

        public byte[] GetFinalBinaryContent()
        {
            switch (this.Type)
            {
                case FileOutputType.Binary: return this.BinaryContent;
                case FileOutputType.Copy: return FileUtil.ReadFileBytes(this.AbsoluteInputPath);
                case FileOutputType.Text: throw new System.NotImplementedException(); // TODO: return UTF-8 bytes
                default: throw new System.NotImplementedException();
            }
        }

        public static FileOutput OfString(string value)
        {
            return new FileOutput()
            {
                Type = FileOutputType.Text,
                TextContent = value,
            };
        }
    }
}
