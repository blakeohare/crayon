using System.Collections.Generic;
using System.Linq;

namespace Build.BuildParseNodes
{
    internal static class JsonParserForBuild
    {
        internal static BuildRoot Parse(string file)
        {
            Common.JsonParser parser = new Common.JsonParser(file);
            IDictionary<string, object> rootDict = parser.ParseAsDictionary();
            Common.JsonLookup root = new Common.JsonLookup(rootDict);

            BuildRoot rootOut = new BuildRoot();
            rootOut.ProjectName = root.GetAsString("id");

            rootOut.ProgrammingLanguage = root.GetAsString("programming-language");
            ParseBuildItem(rootOut, root);
            List<Target> targets = new List<Target>();
            foreach (Common.JsonLookup targetRoot in root.GetAsList("targets")
                .OfType<IDictionary<string, object>>()
                .Select(t => new Common.JsonLookup(t)))
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
            item.DefaultTitle = json.GetAsString("default-title");
            item.Description = json.GetAsString("description");
            item.GuidSeed = json.GetAsString("guid-seed");
            item.IconFilePath = json.GetAsString("icon");
            item.IosBundlePrefix = json.GetAsString("ios-bundle-prefix");
            item.JavaPackage = json.GetAsString("java-package");
            item.JsFilePrefix = json.GetAsString("js-file-prefix");
            item.JsFullPageRaw = json.Get("js-full-page") == null ? (bool?)null : json.GetAsBoolean("js-full-page");
            item.LaunchScreen = json.GetAsString("launch-screen");
            item.MinifiedRaw = json.Get("js-min") == null ? (bool?)null : json.GetAsBoolean("js-min");
            item.Orientation = json.GetAsString("orientation");
            item.Output = json.GetAsString("output");
            item.Version = json.GetAsString("version");
            item.IsCSharpCompatMode = json.GetAsBoolean("csharp-compat-mode");

            // TODO(json-build): change this to direct dependency references. For now, this will use direct dependencies, but walk up to the previous directory for compatibility.
            item.CrayonPath = (json.GetAsList("deps") ?? new object[0])
                .OfType<string>()
                .Select(t => t.Replace('\\', '/'))
                .Select(t => t.TrimEnd('/'))
                .Select(t => t.Contains('/') ? t.Substring(0, t.LastIndexOf('/')) : (t + "/.."))
                .Distinct()
                .ToArray();

            List<ImageSheet> imageSheets = new List<ImageSheet>();
            object[] imageSheetsRaw = json.GetAsList("image-sheets");
            if (imageSheetsRaw != null)
            {
                foreach (Common.JsonLookup imageSheetJson in imageSheetsRaw.OfType<IDictionary<string, object>>().Select(t => new Common.JsonLookup(t)))
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
                    Width = windowWidth,
                    Height = windowHeight
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
                .Select(t => new Common.JsonLookup(t)))
            {
                BuildVar bv = new BuildVar() { Id = varJson.GetAsString("name") };
                object value = varJson.Get("value");
                if (value == null && varJson.GetAsString("env") != null)
                {
                    bv.Value = Common.EnvironmentVariableUtil.GetVariable(varJson.GetAsString("env")) ?? "";
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
