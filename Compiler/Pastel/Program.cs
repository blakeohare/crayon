using SimpleJson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pastel
{
    public class Program
    {
        public static void PseudoMain(string[] args)
        {

        }

        private class CodeLoader : IInlineImportCodeLoader
        {
            private string root;
            public CodeLoader(string root)
            {
                this.root = root;
            }

            public string LoadCode(string path)
            {
                path = System.IO.Path.Combine(this.root, path);
                path = System.IO.Path.GetFullPath(path);
                return System.IO.File.ReadAllText(path);
            }
        }

        public static bool ExportForTarget(string pastelProj, string targetName)
        {
            if (!System.IO.File.Exists(pastelProj)) return false;
            string pastelProjTextRaw = System.IO.File.ReadAllText(pastelProj);
            IDictionary<string, object> jsonDict = new JsonParser(pastelProjTextRaw).ParseAsDictionary();
            JsonLookup jsonRoot = new JsonLookup(jsonDict);
            JsonLookup jsonTarget = jsonRoot
                .GetAsList("targets")
                .OfType<IDictionary<string, object>>()
                .Select(t => new JsonLookup(t))
                .Where(t => t.GetAsString("name") == targetName)
                .FirstOrDefault();
            if (jsonTarget == null) return false;

            string source = jsonTarget.GetAsString("source") ?? jsonRoot.GetAsString("source");
            string output = jsonTarget.GetAsString("output") ?? jsonRoot.GetAsString("output");
            string language = jsonTarget.GetAsString("language") ?? jsonRoot.GetAsString("language");

            CodeLoader codeLoader = new CodeLoader(System.IO.Path.GetFileName(source));
            PastelContext pastelContext = new PastelContext(language, codeLoader);
            
            // TODO(pastel-split): this
            Dictionary<string, string> extensibleFunctionTranslations = new Dictionary<string, string>();
            List<ExtensibleFunction> extensibleFunctions = new List<ExtensibleFunction>();

            // TODO(pastel-split): load constants from manifest
            Dictionary<string, object> constants = new Dictionary<string, object>();

            return true;
        }
    }
}
