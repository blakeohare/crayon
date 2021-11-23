using Common;
using CommonUtil.Json;
using System.Collections.Generic;
using System.Linq;

namespace Build.BuildParseNodes
{
    // TODO: after a few versions have passed, remove all the hyphenated aliases
    internal static class JsonParserForBuild
    {
        internal static BuildRoot Parse(string file)
        {
            JsonParser parser = new JsonParser(file);
            IDictionary<string, object> rootDict = parser.ParseAsDictionary();
            JsonLookup root = new JsonLookup(rootDict);

            BuildRoot rootOut = new BuildRoot();
            rootOut.ProjectId = root.GetAsString("id");

            rootOut.ProgrammingLanguage = root.GetAsString("programmingLanguage") ?? root.GetAsString("programming-language");
            ParseBuildItem(rootOut, root);
            List<Target> targets = new List<Target>();
            foreach (JsonLookup targetRoot in root.GetAsList("targets")
                .OfType<IDictionary<string, object>>()
                .Select(t => new JsonLookup(t)))
            {
                Target target = new Target();
                target.Name = targetRoot.GetAsString("name");
                target.Platform = targetRoot.GetAsString("platform");
                ParseBuildItem(target, targetRoot);
                targets.Add(target);
            }
            rootOut.Targets = targets.ToArray();
            return rootOut;
        }

        private static void ParseBuildItem(BuildItem item, JsonLookup json)
        {
            item.CompilerLocale = json.GetAsString("compilerLocale") ?? json.GetAsString("compiler-locale");
            item.ProjectTitle = json.GetAsString("title");
            item.HasLegacyTitle = (json.GetAsString("defaultTitle") ?? json.GetAsString("default-title")) != null;
            item.Description = json.GetAsString("description");
            item.GuidSeed = json.GetAsString("guidSeed") ?? json.GetAsString("guid-seed");
            item.IosBundlePrefix = json.GetAsString("iosBundlePrefix") ?? json.GetAsString("ios-bundle-prefix");
            item.JavaPackage = json.GetAsString("javaPackage") ?? json.GetAsString("java-package");
            item.JsFilePrefix = json.GetAsString("jsFilePrefix") ?? json.GetAsString("js-file-prefix");
            object jsFullPage = json.Get("jsFullPage") ?? json.Get("js-full-page");
            item.JsFullPageRaw = jsFullPage == null ? (bool?)null : (bool)jsFullPage;
            item.LaunchScreen = json.GetAsString("launchScreen") ?? json.GetAsString("launch-screen");
            object jsMin = json.Get("jsMin") ?? json.Get("js-min");
            item.MinifiedRaw = jsMin == null ? (bool?)null : (bool)jsMin;
            item.Orientation = json.GetAsString("orientation");
            item.Output = json.GetAsString("output");
            item.Version = json.GetAsString("version");
            item.IconFilePaths = (json.GetAsList("icons") ?? new object[0]).OfType<string>().ToArray();
            item.HasLegacyIcon = json.GetAsString("icon") != null;
            item.DelegateMainTo = json.GetAsString("delegateMainTo") ?? json.GetAsString("delegate-main-to");
            item.EnvFile = json.GetAsString("envFile");

            object removeSymbols = json.Get("removeSymbols");
            item.RemoveSymbols = removeSymbols == null ? (bool?)null : (removeSymbols is bool && (bool)removeSymbols);

            List<string> fileDeps = new List<string>();
            foreach (string depPath in (json.GetAsList("deps") ?? new object[0]).OfType<string>())
            {
                if (depPath.StartsWith("http://") || depPath.StartsWith("https://"))
                {
                    throw new System.NotImplementedException(); // Remote dependencies no longer supported...for now.
                }
                else
                {
                    fileDeps.Add(depPath);
                }
            }

            item.LocalDeps = fileDeps
                .Select(t => t.Replace('\\', '/'))
                .Select(t => t.TrimEnd('/'))
                .Distinct()
                .ToArray();

            List<SourceItem> sourceDirectories = new List<SourceItem>();
            string source = json.GetAsString("source");

            if (source != null)
            {
                sourceDirectories.Add(new SourceItem() { Value = source });
            }
            else if (json.GetAsList("source") != null)
            {
                foreach (string sourceDir in json.GetAsList("source").OfType<string>())
                {
                    sourceDirectories.Add(new SourceItem() { Value = sourceDir });
                }
            }
            item.Sources = sourceDirectories.ToArray();

            List<BuildVar> buildVars = new List<BuildVar>();
            foreach (JsonLookup varJson in (json.GetAsList("vars") ?? new object[0])
                .OfType<IDictionary<string, object>>()
                .Select(t => new JsonLookup(t)))
            {
                BuildVar bv = new BuildVar() { Id = varJson.GetAsString("name") };
                object value = varJson.Get("value");
                if (value == null && varJson.GetAsString("env") != null)
                {
                    bv.Value = CommonUtil.Environment.EnvironmentVariables.Get(varJson.GetAsString("env")) ?? "";
                }
                else if (value == null && varJson.GetAsString("envFile") != null)
                {
                    bv.EnvFileReference = varJson.GetAsString("envFile");
                }
                else
                {
                    bv.Value = value;
                    bv.EnsureTypeValid();
                }
                buildVars.Add(bv);
            }
            item.Var = buildVars.ToArray();
        }
    }
}
