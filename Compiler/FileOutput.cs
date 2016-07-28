namespace Crayon
{
    enum FileOutputType
    {
        Text,
        Binary,
        Copy,
        Image,

        // A stub indicating the original file path of a file that was once here but is no longer
        // present at this location. This is for things like images that are in image sheets.
        Ghost,
    }

    class FileOutput
    {
        public FileOutputType Type { get; set; }

        public string RelativeInputPath { get; set; }
        public string TextContent { get; set; }
        public byte[] BinaryContent { get; set; }
        public SystemBitmap Bitmap { get; set; }

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
    }
}
