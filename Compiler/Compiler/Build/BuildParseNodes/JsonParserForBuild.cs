using System.Collections.Generic;
using System.Linq;

namespace Build.BuildParseNodes
{
    internal static class JsonParserForBuild
    {
        internal static BuildRoot Parse(string file, string projectDir)
        {
            Wax.Util.JsonParser parser = new Wax.Util.JsonParser(file);
            IDictionary<string, object> rootDict = parser.ParseAsDictionary();
            Wax.Util.JsonLookup rootData = new Wax.Util.JsonLookup(rootDict);

            BuildRoot rootOut = new BuildRoot();

            ParseBuildItem(rootOut, rootDict, projectDir);
            List<Target> targets = new List<Target>();
            foreach (IDictionary<string, object> targetRoot in rootData.GetAsList("targets").OfType<IDictionary<string, object>>())
            {
                Target target = new Target();
                ParseBuildItem(target, targetRoot, projectDir);
                targets.Add(target);
            }
            rootOut.Targets = targets.ToArray();
            return rootOut;
        }

        private static void ParseBuildItem(BuildItem item, IDictionary<string, object> jsonData, string projectDir)
        {
            List<BuildVar> buildVars = new List<BuildVar>();
            List<Wax.BuildArg> buildArgs = new List<Wax.BuildArg>();
            List<Wax.ExtensionArg> extensionArgs = new List<Wax.ExtensionArg>();
            List<BuildVar> envFileDefinedArgs = new List<BuildVar>();

            List<IDictionary<string, object>> rawVars = new List<IDictionary<string, object>>();
            foreach (string key in jsonData.Keys.OrderBy(k => k))
            {
                object valueObj = jsonData[key];
                string valueStr = ObjectToString(valueObj);

                if (valueObj == null) continue;
                switch (key)
                {
                    case "targets":
                        if (item is Target)
                        {
                            throw new System.InvalidOperationException("Cannot nest a 'targets' field within build targets.");
                        }
                        break;

                    case "vars":
                    case "extensions":
                        object[] vars = jsonData[key] as object[];
                        if (vars == null) throw new System.InvalidOperationException("'" + key + "' field in build file must be a list of objects");
                        rawVars.AddRange(vars.OfType<IDictionary<string, object>>());
                        if (rawVars.Count != vars.Length) throw new System.InvalidOperationException("'" + key + "' field in build file must be a list of objects");

                        foreach (Wax.Util.JsonLookup itemInfo in rawVars.Select(rv => new Wax.Util.JsonLookup(rv)))
                        {
                            string name = itemInfo.GetAsString("name");
                            object value = itemInfo.Get("value");

                            if (key == "vars")
                            {
                                string envName = itemInfo.GetAsString("env");
                                if (envName != null)
                                {
                                    value = value ?? Wax.Util.EnvironmentVariables.Get(envName);
                                }
                                if (name == null || name.Length == 0) throw new System.InvalidOperationException("There is a build variable with no name.");

                                buildVars.Add(new BuildVar(name, value));
                            }
                            else
                            {
                                string extension = itemInfo.GetAsString("extension");
                                string extArgValue = ObjectToString(value);
                                extensionArgs.Add(new Wax.ExtensionArg() { Extension = extension, Name = name, Value = extArgValue });
                            }
                        }
                        break;

                    case "name":
                        if (valueStr != null)
                        {
                            if (item is BuildRoot) throw new System.InvalidOperationException("Cannot set a name field on the root build data.");
                            ((Target)item).Name = valueStr;
                        }
                        break;

                    case "inheritFrom":
                        if (valueStr != null)
                        {
                            if (item is BuildRoot) throw new System.InvalidOperationException("Build file root scope cannot use the 'inheritFrom' attribute. All targets inherit from root by default.");
                            ((Target)item).InheritFrom = valueStr;
                        }
                        break;

                    default:
                        object[] valueObjs = valueObj as object[];
                        if (valueObjs == null)
                        {
                            valueObjs = new object[] { valueObj };
                        }

                        if (key == "envFile")
                        {
                            foreach (ProjectFilePath envFilePath in BuildContext.ToFilePaths(projectDir, valueObjs.OfType<string>().ToArray()))
                            {
                                if (System.IO.File.Exists(envFilePath.AbsolutePath))
                                {
                                    string envContents = System.IO.File.ReadAllText(envFilePath.AbsolutePath);
                                    IDictionary<string, object> envFileData = new Wax.Util.JsonParser(envContents).ParseAsDictionary();
                                    foreach (string envKey in envFileData.Keys)
                                    {
                                        envFileDefinedArgs.Add(new BuildVar(envKey, envFileData[envKey]));
                                    }
                                }
                                else
                                {
                                    throw new System.InvalidOperationException("The env file path '" + envFilePath + "' does not exist.");
                                }
                            }
                        }
                        else
                        {
                            buildArgs.AddRange(valueObjs
                                .Where(v => v != null)
                                .Select(v => ObjectToString(v))
                                .Where(s =>
                                {
                                    if (s == null) throw new System.InvalidOperationException("Unexpected value in build file for field '" + key + "'.");
                                    return true;
                                })
                                .Select(s => new Wax.BuildArg() { Name = key, Value = s }));
                        }
                        break;
                }
            }

            item.BuildArgs = buildArgs.ToArray();
            item.ExtensionArgs = extensionArgs.ToArray();

            // Flatten build vars and env file defined vars (with the latter taking precedence)
            Dictionary<string, BuildVar> flattenedVars = new Dictionary<string, BuildVar>();
            foreach (BuildVar buildVar in buildVars.Concat(envFileDefinedArgs))
            {
                flattenedVars[buildVar.ID] = buildVar;
            }
            item.BuildVars = flattenedVars.Values.ToArray();
        }

        private static string ObjectToString(object value)
        {
            if (value == null) return null;
            if (value is string || value is int || value is double) return value.ToString();
            if (value is bool) return (bool)value ? "true" : "false";
            return null;
        }
    }
}
