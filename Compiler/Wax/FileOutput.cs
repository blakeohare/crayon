using CommonUtil.Disk;
using System.Collections.Generic;
using System.Linq;

namespace Wax
{
    public enum FileOutputType
    {
        Text,
        Binary,
        Copy,
        Image,
    }

    public class FileOutput : JsonBasedObject
    {
        private static Dictionary<string, FileOutputType> TYPE_LOOKUP;

        static FileOutput()
        {
            TYPE_LOOKUP = System.Enum.GetValues(typeof(FileOutputType))
                .Cast<FileOutputType>()
                .ToDictionary(t => t.ToString().ToLowerInvariant());
        }

        public FileOutput() : base() { }
        public FileOutput(IDictionary<string, object> data) : base(data) { }

        public FileOutputType Type { get { return TYPE_LOOKUP[this.GetString("type")]; } set { this.SetString("type", value.ToString().ToLowerInvariant()); } }

        public string RelativeInputPath { get { return this.GetString("relativeInputPath"); } set { this.SetString("relativeInputPath", value); } }
        public string AbsoluteInputPath { get { return this.GetString("absoluteInputPath"); } set { this.SetString("absoluteInputPath", value); } }
        public string TextContent { get { return this.GetString("textContent"); } set { this.SetString("textContent", value); } }

        // Original path relative to the root of the source directory.
        // This is the virtualized location of the embedded resource.
        public string OriginalPath { get { return this.GetString("originalPath"); } set { this.SetString("originalPath", value); } }

        // An auto-assigned filename that doesn't have special characters.
        public string CanonicalFileName { get { return this.GetString("canonicalFileName"); } set { this.SetString("canonicalFileName", value); } }

        // e.g. JPEG's will change if re-encoded.
        public bool IsLossy { get { return this.GetBoolean("isLossy"); } set { this.SetBoolean("isLossy", value); } }

        // For text file types, ensure that there is no BOM in the output.
        // BOM's are automatically removed from .java files
        // TODO: change this to an enum: { DEFAULT, PRESENT, ABSENT }
        public bool TrimBomIfPresent { get { return this.GetBoolean("trimBomIfPresent"); } set { this.SetBoolean("trimBomIfPresent", value); } }

        public string BinaryContentB64 { get { return this.GetString("contentB64"); } set { this.binaryContentCache = null; this.SetString("contentB64", value); } }

        private byte[] binaryContentCache = null;
        public byte[] BinaryContent
        {
            get
            {
                if (this.binaryContentCache == null)
                {
                    if (this.Type == FileOutputType.Image && this.Bitmap != null)
                    {
                        this.binaryContentCache = this.Bitmap.SaveBytes(CommonUtil.Images.ImageFormat.PNG);
                    }
                    else
                    {
                        this.binaryContentCache = CommonUtil.Base64.FromBase64ToBytes(this.BinaryContentB64);
                    }
                }
                return this.binaryContentCache;
            }
            set
            {
                this.binaryContentCache = value;
                this.BinaryContentB64 = CommonUtil.Base64.ToBase64(value);
            }
        }

        private CommonUtil.Images.Bitmap bitmap = null;
        public CommonUtil.Images.Bitmap Bitmap
        {
            get { return this.bitmap; }
            set
            {
                this.binaryContentCache = null;
                this.bitmap = value;
            }
        }

        public byte[] GetFinalBinaryContent()
        {
            switch (this.Type)
            {
                case FileOutputType.Binary:
                case FileOutputType.Image:
                    return this.BinaryContent;

                case FileOutputType.Copy:
                    return FileUtil.ReadFileBytes(this.AbsoluteInputPath);

                case FileOutputType.Text:  // TODO: return UTF-8 bytes
                default:
                    throw new System.NotImplementedException();
            }
        }
    }
}
