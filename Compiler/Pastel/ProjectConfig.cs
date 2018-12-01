using System;
using System.Collections.Generic;
using System.Linq;

namespace Pastel
{
    internal class ProjectConfig
    {
        public ProjectConfig()
        {
            this.Language = Language.NONE;
            this.Flags = new Dictionary<string, bool>();
            this.DependenciesByPrefix = new Dictionary<string, ProjectConfig>();
            this.ExtensionTypeDefinitions = new List<string>();
            this.ExtensionPlatformValues = new Dictionary<string, string>();
            this.Imports = new HashSet<string>();
        }

        public override string ToString()
        {
            string s = "Pastel Config";
            if (this.Path != null)
            {
                s += ": " + this.Path;
            }
            return s;
        }

        public string Path { get; set; } // reflects only the top level config, not any base
        public string Directory { get { return System.IO.Path.GetDirectoryName(this.Path); } }

        public Language Language { get; set; }
        public Dictionary<string, bool> Flags { get; set; }
        public string Source { get; set; }
        public List<string> ExtensionTypeDefinitions { get; set; }
        public Dictionary<string, string> ExtensionPlatformValues { get; set; }
        public Dictionary<string, ProjectConfig> DependenciesByPrefix { get; set; }
        public string OutputDirStructs { get; set; }
        public string OutputFileFunctions { get; set; }
        public string WrappingClassNameForFunctions { get; set; }
        public string NamespaceForStructs { get; set; }
        public string NamespaceForFunctions { get; set; }
        public HashSet<string> Imports { get; set; }

        public static ProjectConfig Parse(string path)
        {
            string configContents = System.IO.File.ReadAllText(path);
            ProjectConfig config = new ProjectConfig() { Path = path };
            ParseImpl(config, configContents, path);
            return config;
        }

        // multiple colons are valid, but no colon is not.
        // if the value contains a colon, then only split on the first one.
        // results should be trimmed.
        private static string[] SplitOnColon(string s)
        {
            int colonIndex = s.IndexOf(':');
            if (colonIndex == -1) return null;
            string key = s.Substring(0, colonIndex).Trim();
            string value = s.Substring(colonIndex + 1).Trim();
            return new string[] { key, value };
        }

        private static string CanonicalizeDirectory(string directory, string relativePath)
        {
            return System.IO.Path.GetFullPath(System.IO.Path.Combine(directory, relativePath));
        }

        private static void ParseImpl(ProjectConfig config, string configContents, string originalPath)
        {
            string directory = System.IO.Path.GetFullPath(System.IO.Path.GetDirectoryName(originalPath));
            string[] lines = configContents.Split('\n');
            for (int i = 0; i < lines.Length; ++i)
            {
                string line = lines[i].Trim();
                if (line.Length == 0 || line[0] == '#') continue;
                string[] parts = SplitOnColon(line);
                if (parts == null)
                {
                    throw new InvalidOperationException("Invalid syntax on line " + (i + 1) + " of " + originalPath + ":\n" + line);
                }

                string type = parts[0];
                string data = parts[1];
                switch (type)
                {
                    case "IMPORT":
                        string importedConfig = CanonicalizeDirectory(directory, data);
                        if (!System.IO.File.Exists(importedConfig))
                        {
                            throw new InvalidOperationException("Config file does not exist: " + importedConfig);
                        }
                        string importedConfigContents = System.IO.File.ReadAllText(importedConfig);
                        ParseImpl(config, importedConfigContents, importedConfig);
                        break;

                    case "LANGUAGE":
                        config.Language = LanguageUtil.ParseLanguage(data);
                        break;

                    case "SOURCE":
                        config.Source = CanonicalizeDirectory(directory, data);
                        break;

                    case "DEPENDENCY":
                        parts = SplitOnColon(data);
                        if (parts == null) throw new InvalidOperationException("Dependency requires a namespace in " + originalPath + " on line " + (i + 1) + ":\n" + line);
                        string depNamespace = parts[0];
                        string path = CanonicalizeDirectory(directory, parts[1]);
                        config.DependenciesByPrefix[depNamespace] = Parse(path);
                        break;

                    case "FLAG":
                        parts = SplitOnColon(data);
                        if (parts.Length != 2) throw new InvalidOperationException("Invalid flag definition: " + line);
                        bool flagValue;
                        switch (parts[1].ToLower())
                        {
                            case "true": flagValue = true; break;
                            case "false": flagValue = false; break;
                            default: throw new InvalidOperationException("Flags can only be 'true' or 'false'. Found: '" + parts[1].Trim() + "'");
                        }
                        config.Flags[parts[0]] = flagValue;
                        break;

                    case "OUTPUT-STRUCTS":
                        config.OutputDirStructs = CanonicalizeDirectory(directory, data);
                        break;

                    case "OUTPUT-FUNCTIONS":
                        config.OutputFileFunctions = CanonicalizeDirectory(directory, data);
                        break;

                    // TODO: I have not found value in distinguishing these two aside from this is just how
                    // I started things and have to maintain it. Merge the two namespaces and then change
                    // these to just "NAMESPACE".
                    case "NAMESPACE-FUNCTIONS":
                        config.NamespaceForFunctions = data;
                        break;
                    case "NAMESPACE-STRUCTS":
                        config.NamespaceForStructs = data;
                        break;

                    case "FUNCTION-WRAPPER-CLASS":
                        config.WrappingClassNameForFunctions = data;
                        break;

                    case "EXT":
                        parts = SplitOnColon(data);
                        config.ExtensionPlatformValues[parts[0]] = parts[1];
                        break;

                    case "EXT-TYPE":
                        config.ExtensionTypeDefinitions.Add(data);
                        break;

                    case "CODE-IMPORT":
                        config.Imports.Add(data);
                        break;

                    default:
                        throw new InvalidOperationException("Unrecognized project config command '" + type + "' on line " + (i + 1) + " of " + originalPath + ":\n" + line);
                }
            }
        }

        public List<ExtensibleFunction> GetExtensibleFunctions()
        {
            string typeDefinitionsRawCode = string.Join("\n", this.ExtensionTypeDefinitions);
            List<ExtensibleFunction> output = new List<ExtensibleFunction>();
            List<ExtensibleFunction> typeDefinitions = ExtensibleFunctionMetadataParser.Parse(this.Path, typeDefinitionsRawCode);
            Dictionary<string, ExtensibleFunction> typeDefinitionsLookup = new Dictionary<string, ExtensibleFunction>();
            foreach (ExtensibleFunction exFn in typeDefinitions)
            {
                typeDefinitionsLookup[exFn.Name] = exFn;
            }

            foreach (string exFunctionName in this.ExtensionPlatformValues.Keys.OrderBy(k => k))
            {
                if (!typeDefinitionsLookup.ContainsKey(exFunctionName))
                {
                    throw new InvalidOperationException("No type information defined for extensible function '" + exFunctionName + "'");
                }

                ExtensibleFunction exFn = typeDefinitionsLookup[exFunctionName];
                exFn.Translation = this.ExtensionPlatformValues[exFunctionName];
                output.Add(exFn);
            }
            return output;
        }
    }
}
