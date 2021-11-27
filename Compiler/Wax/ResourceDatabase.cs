using System.Collections.Generic;
using System.Linq;

namespace Wax
{
    /*
     * Database of non-code files to be copied.
     */
    public class ResourceDatabase : JsonBasedObject
    {
        public ResourceDatabase() : base() { }
        public ResourceDatabase(IDictionary<string, object> data) : base(data) { }

        public FileOutput ResourceManifestFile { get { return this.GetObjectAsType<FileOutput>("manifest"); } set { this.SetObject("manifest", value); } }
        public FileOutput ImageResourceManifestFile { get { return this.GetObjectAsType<FileOutput>("imageManifest"); } set { this.SetObject("imageManifest", value); } }

        public Dictionary<string, FileOutput> ImageResourceFiles
        {
            get
            {
                Dictionary<string, JsonBasedObject> dict = this.GetDictionary("imageFiles");
                return dict.Keys.ToDictionary(k => k, k => (FileOutput)dict[k]);
            }
            set
            {
                this.SetDictionary("imageFiles", value.Keys.ToDictionary(k => k, k => (JsonBasedObject)value[k]));
            }
        }

        private FileOutput[] GetFileOutputList(string key) { return this.GetObjectsAsType<FileOutput>(key) ?? new FileOutput[0]; }

        public FileOutput[] AudioResources { get { return this.GetFileOutputList("audioResources"); } set { this.SetObjects("audioResource", value.Cast<JsonBasedObject>()); } }
        public FileOutput[] ImageResources { get { return this.GetFileOutputList("imageResources"); } set { this.SetObjects("imageResources", value.Cast<JsonBasedObject>()); } }
        public FileOutput[] TextResources { get { return this.GetFileOutputList("textResources"); } set { this.SetObjects("textResources", value.Cast<JsonBasedObject>()); } }
        public FileOutput[] BinaryResources { get { return this.GetFileOutputList("binaryResources"); } set { this.SetObjects("binaryResources", value.Cast<JsonBasedObject>()); } }
        public FileOutput[] FontResources { get { return this.GetFileOutputList("fontResources"); } set { this.SetObjects("fontResources", value.Cast<JsonBasedObject>()); } }

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
