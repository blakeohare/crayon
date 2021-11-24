using System.Collections.Generic;

namespace Wax
{
    /*
     * Database of non-code files to be copied.
     */
    public class ResourceDatabase
    {
        //public FileOutput ByteCodeFile { get; set; }
        public FileOutput ResourceManifestFile { get; set; }
        public FileOutput ImageResourceManifestFile { get; set; }

        public Dictionary<string, FileOutput> ImageResourceFiles { get; set; }

        public List<FileOutput> AudioResources { get; set; }
        public List<FileOutput> ImageResources { get; set; }
        public List<FileOutput> TextResources { get; set; }
        public List<FileOutput> BinaryResources { get; set; }
        public List<FileOutput> FontResources { get; set; }

        public ResourceDatabase()
        {
            this.AudioResources = new List<FileOutput>();
            this.BinaryResources = new List<FileOutput>();
            this.ImageResources = new List<FileOutput>();
            this.TextResources = new List<FileOutput>();
            this.FontResources = new List<FileOutput>();
        }

        public void GenerateResourceMapping()
        {
            List<string> manifest = new List<string>();
            int resourceId = 1;
            foreach (FileOutput textFile in this.TextResources)
            {
                textFile.CanonicalFileName = "txt" + (resourceId++) + ".txt";
                manifest.Add("TXT," + textFile.OriginalPath + "," + textFile.CanonicalFileName);
            }

            foreach (FileOutput imageFile in this.ImageResources)
            {
                manifest.Add("IMG," + imageFile.OriginalPath + ",");
            }

            foreach (FileOutput audioFile in this.AudioResources)
            {
                audioFile.CanonicalFileName = "snd" + (resourceId++) + ".ogg";
                // TODO: swap the order of the original path and the canonical name
                manifest.Add("SND," + audioFile.OriginalPath + "," + audioFile.CanonicalFileName);
            }

            foreach (FileOutput fontFile in this.FontResources)
            {
                fontFile.CanonicalFileName = "ttf" + (resourceId++) + ".ttf";
                manifest.Add("TTF," + fontFile.OriginalPath + "," + fontFile.CanonicalFileName);
            }

            this.ResourceManifestFile = new FileOutput()
            {
                Type = FileOutputType.Text,
                TextContent = string.Join("\n", manifest),
            };
        }

        public void PopulateFileOutputContextForCbx(Dictionary<string, FileOutput> output)
        {
            foreach (FileOutput txtResource in this.TextResources)
            {
                output["res/txt/" + txtResource.CanonicalFileName] = txtResource;
            }
            foreach (FileOutput sndResource in this.AudioResources)
            {
                output["res/snd/" + sndResource.CanonicalFileName] = sndResource;
            }
            foreach (FileOutput fontResource in this.FontResources)
            {
                output["res/ttf/" + fontResource.CanonicalFileName] = fontResource;
            }
            foreach (FileOutput binResource in this.BinaryResources)
            {
                output["res/bin/" + binResource.CanonicalFileName] = binResource;
            }

            foreach (string name in this.ImageResourceFiles.Keys)
            {
                FileOutput imgResource = this.ImageResourceFiles[name];
                output["res/img/" + name] = imgResource;
            }
        }
    }
}
