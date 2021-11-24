using System;
using System.Collections.Generic;
using System.Linq;

namespace AssemblyResolver
{
    public class AssemblyService : Wax.WaxService
    {
        private Dictionary<string, AssemblyFinder> assemblyFinderCache = new Dictionary<string, AssemblyFinder>();

        public AssemblyService() : base("assembly") { }

        private AssemblyFinder GetAssemblyFinder(string[] localDeps, string projectDir)
        {
            string finderKey = string.Join(';', localDeps) + ";PROJ:" + projectDir;
            if (!this.assemblyFinderCache.ContainsKey(finderKey))
            {
                this.assemblyFinderCache[finderKey] = new AssemblyFinder(localDeps, projectDir);
            }
            return this.assemblyFinderCache[finderKey];
        }

        public override void HandleRequest(
            Dictionary<string, object> request,
            Func<Dictionary<string, object>, bool> cb)
        {
            switch ((string)request["command"])
            {
                case "GetAssemblyMetadataFromAnyPossibleKey":
                    string locale = request.ContainsKey("locale") ? (string)request["locale"] : null;
                    string name = (string)request["name"];
                    string[] localDeps = (string[])request["localDeps"];
                    string projectDir = (string)request["projectDir"];
                    bool includeSource = (bool)request["includeSource"];
                    AssemblyFinder af = this.GetAssemblyFinder(localDeps, projectDir);
                    InternalAssemblyMetadata md = af.GetAssemblyMetadataFromAnyPossibleKey(locale == null ? name : (locale + ":" + name));
                    Dictionary<string, object> output = new Dictionary<string, object>();
                    if (md == null)
                    {
                        output["found"] = false;
                    }
                    else
                    {
                        output["found"] = true;
                        output["id"] = md.ID;
                        output["internalLocale"] = md.InternalLocale.ID;
                        output["supportedLocales"] = md.SupportedLocales.Select(loc => loc.ID).OrderBy(k => k).ToArray();
                        List<string> nameByLocale = new List<string>();
                        foreach (string loc in md.NameByLocale.Keys)
                        {
                            nameByLocale.Add(loc);
                            nameByLocale.Add(md.NameByLocale[loc]);
                        }
                        output["nameByLocale"] = nameByLocale.ToArray();
                        List<string> onlyImportableFrom = new List<string>();
                        output["onlyImportableFrom"] = md.OnlyImportableFrom;

                        if (includeSource)
                        {
                            Dictionary<string, string> code = md.GetSourceCode();
                            List<string> codeOut = new List<string>();
                            foreach (string file in code.Keys.OrderBy(k => k))
                            {
                                codeOut.Add(file);
                                codeOut.Add(code[file]);
                            }
                            output["sourceCode"] = codeOut.ToArray();
                        }
                    }

                    cb(output);
                    return;
            }
        }
    }
}
