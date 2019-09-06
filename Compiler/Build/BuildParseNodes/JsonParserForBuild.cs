using Common;
using System.Collections.Generic;
using System.Linq;

namespace Build.BuildParseNodes
{
    internal static class JsonParserForBuild
    {
        internal static BuildRoot Parse(string file)
        {
            JsonParser parser = new JsonParser(file);
            IDictionary<string, object> rootDict = parser.ParseAsDictionary();
            JsonLookup root = new JsonLookup(rootDict);

            BuildRoot rootOut = new BuildRoot();
            rootOut.ProjectId = root.GetAsString("id");

            rootOut.ProgrammingLanguage = root.GetAsString("programming-language");
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

        private static void ParseBuildItem(BuildItem item, Common.JsonLookup json)
        {
            item.CompilerLocale = json.GetAsString("compiler-locale");
            item.ProjectTitle = json.GetAsString("title");
            item.HasLegacyTitle = json.GetAsString("default-title") != null;
            item.Description = json.GetAsString("description");
            item.GuidSeed = json.GetAsString("guid-seed");
            item.IosBundlePrefix = json.GetAsString("ios-bundle-prefix");
            item.JavaPackage = json.GetAsString("java-package");
            item.JsFilePrefix = json.GetAsString("js-file-prefix");
            item.JsFullPageRaw = json.Get("js-full-page") == null ? null : new NullableBoolean(json.GetAsBoolean("js-full-page"));
            item.LaunchScreen = json.GetAsString("launch-screen");
            item.MinifiedRaw = json.Get("js-min") == null ? null : new NullableBoolean(json.GetAsBoolean("js-min"));
            item.Orientation = json.GetAsString("orientation");
            item.Output = json.GetAsString("output");
            item.Version = json.GetAsString("version");
            item.IsCSharpCompatMode = json.GetAsBoolean("csharp-compat-mode");
            item.IconFilePaths = (json.GetAsList("icons") ?? new object[0]).OfType<string>().ToArray();
            item.HasLegacyIcon = json.GetAsString("icon") != null;
            item.DelegateMainTo = json.GetAsString("delegate-main-to");

            List<string> remoteDeps = new List<string>();
            List<string> fileDeps = new List<string>();
            foreach (string depPath in (json.GetAsList("deps") ?? new object[0]).OfType<string>())
            {
                if (depPath.StartsWith("http://") || depPath.StartsWith("https://"))
                {
                    remoteDeps.Add(depPath);
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

            item.RemoteDeps = remoteDeps.ToArray();

            List<ImageSheet> imageSheets = new List<ImageSheet>();
            object[] imageSheetsRaw = json.GetAsList("image-sheets");
            if (imageSheetsRaw != null)
            {
                foreach (JsonLookup imageSheetJson in imageSheetsRaw
                    .OfType<IDictionary<string, object>>()
                    .Select(t => new JsonLookup(t)))
                {
                    ImageSheet imgSheet = new ImageSheet();
                    imgSheet.Id = imageSheetJson.GetAsString("id");
                    imgSheet.Prefixes = new string[] { imageSheetJson.GetAsString("prefix") };
                    imageSheets.Add(imgSheet);
                }
            }
            item.ImageSheets = imageSheets.ToArray();

            int windowWidth = json.GetAsInteger("window-size.width", -1);
            int windowHeight = json.GetAsInteger("window-size.height", -1);
            if (windowWidth != -1 && windowHeight != -1)
            {
                item.WindowSize = new Size()
                {
                    Width = new NullableInteger(windowWidth),
                    Height = new NullableInteger(windowHeight)
                };
            }

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
            foreach (Common.JsonLookup varJson in (json.GetAsList("vars") ?? new object[0])
                .OfType<IDictionary<string, object>>()
                .Select(t => new JsonLookup(t)))
            {
                BuildVar bv = new BuildVar() { Id = varJson.GetAsString("name") };
                object value = varJson.Get("value");
                if (value == null && varJson.GetAsString("env") != null)
                {
                    bv.Value = EnvironmentVariableUtil.GetVariable(varJson.GetAsString("env")) ?? "";
                    bv.Type = VarType.STRING;
                }
                else
                {
                    bv.Value = value;
                    if (bv.Value is bool) bv.Type = VarType.BOOLEAN;
                    else if (bv.Value is int) bv.Type = VarType.INT;
                    else if (bv.Value is float || bv.Value is double) bv.Type = VarType.FLOAT;
                    else if (bv.Value is string) bv.Type = VarType.STRING;
                    else
                    {
                        throw new System.InvalidOperationException("Complex types are not allowed for build variables.");
                    }
                }
                buildVars.Add(bv);
            }
            item.Var = buildVars.ToArray();
        }
    }
}
