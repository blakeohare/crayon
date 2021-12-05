using System.Collections.Generic;
using System.Linq;
using Wax.Util.Disk;

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
                        this.binaryContentCache = this.Bitmap.SaveBytes(Wax.Util.Images.ImageFormat.PNG);
                    }
                    else if (this.BinaryContentB64 != null)
                    {
                        this.binaryContentCache = System.Convert.FromBase64String(this.BinaryContentB64);
                    }
                    else if (this.Type == FileOutputType.Text && this.TextContent != null)
                    {
                        this.binaryContentCache = System.Text.Encoding.UTF8.GetBytes(this.TextContent);
                    }
                    else if (this.Type == FileOutputType.Copy)
                    {
                        this.binaryContentCache = System.IO.File.ReadAllBytes(this.AbsoluteInputPath);
                    }
                    else
                    {
                        throw new System.Exception();
                    }
                }
                return this.binaryContentCache;
            }
            set
            {
                this.binaryContentCache = value;
                this.BinaryContentB64 = System.Convert.ToBase64String(value);
            }
        }

        private Wax.Util.Images.Bitmap bitmap = null;
        public Wax.Util.Images.Bitmap Bitmap
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

                case FileOutputType.Text:
                    if (this.BinaryContent != null) return this.BinaryContent;
                    if (this.BinaryContentB64 != null) return System.Convert.FromBase64String(this.BinaryContentB64);
                    if (this.TextContent != null)
                    {
                        this.BinaryContent = System.Text.Encoding.UTF8.GetBytes(this.TextContent);
                        return this.binaryContentCache;
                    }
                    throw new System.NotImplementedException();

                default:
                    throw new System.NotImplementedException();
            }
        }

        public override Dictionary<string, object> GetRawData()
        {
            if (this.Type == FileOutputType.Text)
            {
                return base.GetRawData();
            }

            if (this.BinaryContentB64 == null)
            {
                if (this.binaryContentCache == null)
                {
                    this.binaryContentCache = this.GetFinalBinaryContent();
                }
                this.BinaryContentB64 = System.Convert.ToBase64String(this.binaryContentCache);
            }
            return base.GetRawData();
        }
    }
}
