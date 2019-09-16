using CommonUtil.Disk;

namespace Common
{
    public enum FileOutputType
    {
        Text,
        Binary,
        Copy,
        Move,
        Image,

        // A stub indicating the original file path of a file that was once here but is no longer
        // present at this location. This is for things like images that are in image sheets.
        Ghost,
    }

    public class FileOutput
    {
        public FileOutputType Type { get; set; }

        public string RelativeInputPath { get; set; }
        public string AbsoluteInputPath { get; set; }
        public string TextContent { get; set; }
        public byte[] BinaryContent { get; set; }
        public SystemBitmap Bitmap { get; set; }
        public bool IsLossy { get; set; } // e.g. JPEG's will change if re-encoded.

        // Re-encode all text files as UTF-8 for consistent readback. If re-encoding fails, then this
        // is likely a binary format that was erroneously included as a text resource and will not be
        // useful to include in the project and thus the user can be warned.
        // Only set to true on Type == Text.
        public bool EnsureUtf8 { get; set; }

        // Original path relative to the root of the source directory.
        // This is the virtualized location of the embedded resource.
        // Ghost types will have this set.
        public string OriginalPath { get; set; }

        // Set on images that are included as a image sheet. Type is changed to Ghost after processing.
        public string ImageSheetId { get; set; }

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
