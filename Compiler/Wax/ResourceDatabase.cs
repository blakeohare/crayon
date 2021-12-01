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
                if (dict == null) return new Dictionary<string, FileOutput>();
                return dict.Keys.ToDictionary(k => k, k => (FileOutput)dict[k]);
            }
            set
            {
                if (value == null) this.ClearValue("imageFiles");
                else this.SetDictionary("imageFiles", value.Keys.ToDictionary(k => k, k => (JsonBasedObject)value[k]));
            }
        }

        private FileOutput[] GetFileOutputList(string key)
        {
            if (this.IsFlattenedMode) throw new System.Exception(); // This should not happen
            return this.GetObjectsAsType<FileOutput>(key) ?? new FileOutput[0];
        }

        private void SetFileOutputList(string key, FileOutput[] value)
        {
            if (value == null) this.ClearValue(key);
            else this.SetObjects(key, value.Cast<JsonBasedObject>());
        }

        public FileOutput[] AudioResources { get { return this.GetFileOutputList("audioResources"); } set { this.SetFileOutputList("audioResource", value); } }
        public FileOutput[] ImageResources { get { return this.GetFileOutputList("imageResources"); } set { this.SetFileOutputList("imageResources", value); } }
        public FileOutput[] TextResources { get { return this.GetFileOutputList("textResources"); } set { this.SetFileOutputList("textResources", value); } }
        public FileOutput[] BinaryResources { get { return this.GetFileOutputList("binaryResources"); } set { this.SetFileOutputList("binaryResources", value); } }
        public FileOutput[] FontResources { get { return this.GetFileOutputList("fontResources"); } set { this.SetFileOutputList("fontResources", value); } }

        private bool IsFlattenedMode { get { return this.GetBoolean("isFlatMode"); } set { this.SetBoolean("isFlatMode", value); } }
        public string[] FlatFileNames { get { return this.GetStrings("flatFileNames"); } set { this.SetStrings("flatFileNames", value); } }
        public FileOutput[] FlatFiles { get { return this.GetObjectsAsType<FileOutput>("flatFiles"); } set { this.SetObjects("flatFiles", value); } }

        public string[] IgnoredFileWarnings {  get { return this.GetStrings("ignoredFileWarnings"); } set { this.SetStrings("ignoredFileWarnings", value); } }

        public void ConvertToFlattenedFileData()
        {
            Dictionary<string, FileOutput> flattenedFiles = new Dictionary<string, FileOutput>();
            foreach (FileOutput txtResource in this.TextResources)
            {
                flattenedFiles["res/" + txtResource.CanonicalFileName] = txtResource;
            }
            foreach (FileOutput sndResource in this.AudioResources)
            {
                flattenedFiles["res/" + sndResource.CanonicalFileName] = sndResource;
            }
            foreach (FileOutput fontResource in this.FontResources)
            {
                flattenedFiles["res/" + fontResource.CanonicalFileName] = fontResource;
            }
            foreach (FileOutput binResource in this.BinaryResources)
            {
                flattenedFiles["res/" + binResource.CanonicalFileName] = binResource;
            }
            foreach (string name in this.ImageResourceFiles.Keys)
            {
                FileOutput imgResource = this.ImageResourceFiles[name];
                flattenedFiles["res/" + name] = imgResource;
            }

            string[] keys = flattenedFiles.Keys.OrderBy(k => k).ToArray();
            FileOutput[] files = keys.Select(k => flattenedFiles[k]).ToArray();
            this.FlatFileNames = keys;
            this.FlatFiles = files;

            this.IsFlattenedMode = true;

            this.TextResources = null;
            this.AudioResources = null;
            this.FontResources = null;
            this.BinaryResources = null;
            this.ImageResourceFiles = null;
        }
    }
}
