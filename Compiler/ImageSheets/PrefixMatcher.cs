using System;
using System.Collections.Generic;
using Common;

namespace Crayon.ImageSheets
{
    class PrefixMatcher
    {
        private List<string> ids = new List<string>();
        private Dictionary<string, List<string>> prefixesById = new Dictionary<string, List<string>>();

        public PrefixMatcher() { }

        public void RegisterId(String id)
        {
            this.ids.Add(id);
            this.prefixesById.Add(id, new List<string>());
        }

        public void RegisterPrefix(String id, String prefix)
        {
            this.prefixesById[id].Add(prefix);
        }

        private string GetImageSheetIdMatch(string filename)
        {
            foreach (string id in this.ids)
            {
                foreach (string prefix in this.prefixesById[id])
                {
                    if (filename.Replace('\\', '/').StartsWith(prefix) || prefix == "*")
                    {
                        return id;
                    }
                }
            }
            return null;
        }

        public Dictionary<string, List<FileOutput>> MatchAndRemoveFiles(ResourceDatabase resDB)
        {
            Dictionary<string, List<FileOutput>> output = new Dictionary<string, List<FileOutput>>();

            foreach (string sheetId in this.ids)
            {
                output.Add(sheetId, new List<FileOutput>());
            }

            foreach (FileOutput file in resDB.ImageResources)
            {
                if (file.Type == FileOutputType.Image)
                {
                    string imageSheetId = this.GetImageSheetIdMatch(file.OriginalPath);
                    if (imageSheetId != null)
                    {
                        output[imageSheetId].Add(file);

                        // This "unregisters" the image file such that it won't be included in the project output.
                        file.ImageSheetId = imageSheetId;
                        file.Type = FileOutputType.Ghost;
                    }
                }
            }

            return output;
        }
    }
}
