using Interpreter.Structs;
using System.Collections.Generic;
using System.Linq;

namespace Interpreter.Structs
{
    public class ResourceInfo
    {
        public string userPath;
        public string internalPath;
        public bool isText;
        public string type;
        public string manifestParam;

        public ResourceInfo(string userPath, string internalPath, bool isText, string type, string manifestParam)
        {
            this.userPath = userPath;
            this.internalPath = internalPath;
            this.isText = isText;
            this.type = type;
            this.manifestParam = manifestParam;
        }
    }

}
