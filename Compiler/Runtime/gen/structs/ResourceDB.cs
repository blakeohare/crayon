using Interpreter.Structs;
using System.Collections.Generic;
using System.Linq;

namespace Interpreter.Structs
{
    public class ResourceDB
    {
        public Dictionary<string, string[]> filesPerDirectory;
        public Dictionary<string, ResourceInfo> fileInfo;
        public List<Value> dataList;
        public string imageAtlasManifest;

        public ResourceDB(Dictionary<string, string[]> filesPerDirectory, Dictionary<string, ResourceInfo> fileInfo, List<Value> dataList, string imageAtlasManifest)
        {
            this.filesPerDirectory = filesPerDirectory;
            this.fileInfo = fileInfo;
            this.dataList = dataList;
            this.imageAtlasManifest = imageAtlasManifest;
        }
    }

}
