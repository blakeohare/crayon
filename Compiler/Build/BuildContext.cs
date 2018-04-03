using Build.BuildParseNodes;
using Common;
using Localization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Build
{
    public class BuildContext
    {
        public string ProjectID { get; set; }
        public string ProjectDirectory { get; set; }
        public string OutputFolder { get; set; }
        public FilePath[] SourceFolders { get; set; }
        public Dictionary<string, string[]> ImageSheetPrefixesById { get; set; }
        public Dictionary<string, BuildVarCanonicalized> BuildVariableLookup { get; private set; }
        public string[] ImageSheetIds { get; set; }
        public string JsFilePrefix { get; set; }
        public string Platform { get; set; }
        public bool Minified { get; set; }
        public bool ReadableByteCode { get; set; }
        public string GuidSeed { get; set; }
        public string IconFilePath { get; set; }
        public string LaunchScreenPath { get; set; }
        public string DefaultTitle { get; set; }
        public string Orientation { get; set; }
        public string CrayonPath { get; set; }
        public string IosBundlePrefix { get; set; }
        public string JavaPackage { get; private set; }
        public bool JsFullPage { get; set; }
        public int? WindowWidth { get; set; }
        public int? WindowHeight { get; set; }
        public string Description { get; set; }
        public string Version { get; set; }
        public Locale CompilerLocale { get; set; }

        public static BuildContext Parse(string projectDir, string buildFile, string nullableTargetName)
        {
            BuildRoot buildInput = XmlParserForBuild.Parse(buildFile);
            BuildRoot flattened = buildInput;
            string platform = null;
            Dictionary<string, BuildVarCanonicalized> varLookup;
            if (nullableTargetName != null)
            {
                string targetName = nullableTargetName;
                Target desiredTarget = null;
                foreach (Target target in buildInput.Targets)
                {
                    if (target.Name == null) throw new InvalidOperationException("A target in the build file is missing a name.");
                    if (target.Platform == null) throw new InvalidOperationException("A target in the build file is missing a platform.");
                    if (target.Name == nullableTargetName)
                    {
                        desiredTarget = target;
                    }
                }

                if (desiredTarget == null)
                {
                    throw new InvalidOperationException("Build target does not exist in build file: '" + targetName + "'.");
                }

                varLookup = GenerateBuildVars(buildInput, desiredTarget, targetName);

                flattened.Sources = desiredTarget.SourcesNonNull.Union<SourceItem>(flattened.SourcesNonNull).ToArray();
                flattened.Output = FileUtil.GetCanonicalizeUniversalPath(DoReplacement(targetName, desiredTarget.Output ?? flattened.Output));
                flattened.ProjectName = DoReplacement(targetName, desiredTarget.ProjectName ?? flattened.ProjectName);
                flattened.JsFilePrefix = DoReplacement(targetName, desiredTarget.JsFilePrefix ?? flattened.JsFilePrefix);
                flattened.JsFullPage = desiredTarget.JsFullPage ?? flattened.JsFullPage;
                flattened.ImageSheets = MergeImageSheets(desiredTarget.ImageSheets, flattened.ImageSheets);
                flattened.MinifiedRaw = desiredTarget.MinifiedRaw ?? flattened.MinifiedRaw;
                flattened.ExportDebugByteCodeRaw = desiredTarget.ExportDebugByteCodeRaw ?? flattened.ExportDebugByteCodeRaw;
                flattened.GuidSeed = DoReplacement(targetName, desiredTarget.GuidSeed ?? flattened.GuidSeed);
                flattened.IconFilePath = DoReplacement(targetName, desiredTarget.IconFilePath ?? flattened.IconFilePath);
                flattened.LaunchScreen = DoReplacement(targetName, desiredTarget.LaunchScreen ?? flattened.LaunchScreen);
                flattened.DefaultTitle = DoReplacement(targetName, desiredTarget.DefaultTitle ?? flattened.DefaultTitle);
                flattened.Orientation = DoReplacement(targetName, desiredTarget.Orientation ?? flattened.Orientation);
                flattened.CrayonPath = CombineAndFlattenStringArrays(desiredTarget.CrayonPath, flattened.CrayonPath).Select(s => DoReplacement(targetName, s)).ToArray();
                flattened.Description = DoReplacement(targetName, desiredTarget.Description ?? flattened.Description);
                flattened.Version = DoReplacement(targetName, desiredTarget.Version ?? flattened.Version);
                flattened.WindowSize = Size.Merge(desiredTarget.WindowSize, flattened.WindowSize) ?? new Size();
                flattened.CompilerLocale = desiredTarget.CompilerLocale ?? flattened.CompilerLocale;
                flattened.Orientation = desiredTarget.Orientation ?? flattened.Orientation;

                platform = desiredTarget.Platform;
            }
            else
            {
                varLookup = GenerateBuildVars(buildInput, new Target(), null);
            }

            ImageSheet[] imageSheets = flattened.ImageSheets ?? new ImageSheet[0];

            return new BuildContext()
            {
                ProjectDirectory = projectDir,
                JsFilePrefix = flattened.JsFilePrefix,
                OutputFolder = flattened.Output,
                Platform = platform,
                ProjectID = flattened.ProjectName,
                Description = flattened.Description,
                Version = flattened.Version,
                SourceFolders = ToFilePaths(projectDir, flattened.Sources),
                ImageSheetPrefixesById = imageSheets.ToDictionary<ImageSheet, string, string[]>(s => s.Id, s => s.Prefixes),
                ImageSheetIds = imageSheets.Select<ImageSheet, string>(s => s.Id).ToArray(),
                Minified = flattened.Minified,
                ReadableByteCode = flattened.ExportDebugByteCode,
                BuildVariableLookup = varLookup,
                GuidSeed = flattened.GuidSeed,
                IconFilePath = flattened.IconFilePath,
                LaunchScreenPath = flattened.LaunchScreen,
                DefaultTitle = flattened.DefaultTitle,
                Orientation = flattened.Orientation,
                CrayonPath = flattened.CrayonPath == null ? "" : string.Join(";", flattened.CrayonPath),
                IosBundlePrefix = flattened.IosBundlePrefix,
                JavaPackage = flattened.JavaPackage,
                JsFullPage = Util.StringToBool(flattened.JsFullPage),
                WindowWidth = Util.ParseIntWithErrorNullOkay((flattened.WindowSize ?? new Size()).Width, "Invalid window width in build file."),
                WindowHeight = Util.ParseIntWithErrorNullOkay((flattened.WindowSize ?? new Size()).Height, "Invalid window height in build file."),
                CompilerLocale = Locale.Get((flattened.CompilerLocale ?? "en").Trim()),
            }.ValidateValues();
        }

        private static string[] CombineAndFlattenStringArrays(string[] a, string[] b)
        {
            List<string> output = new List<string>();
            if (a != null) output.AddRange(a);
            if (b != null) output.AddRange(b);
            return output.ToArray();
        }

        private static string ThrowError(string message)
        {
            throw new InvalidOperationException(message);
        }

        public BuildContext ValidateValues()
        {
            if (this.ProjectID == null) throw new InvalidOperationException("There is no <projectname> for this build target.");


            if (this.SourceFolders.Length == 0) throw new InvalidOperationException("There are no <source> paths for this build target.");
            if (this.OutputFolder == null) throw new InvalidOperationException("There is no <output> path for this build target.");

            foreach (char c in this.ProjectID)
            {
                if (!((c >= 'a' && c <= 'z') ||
                    (c >= 'A' && c <= 'Z') ||
                    (c >= '0' && c <= '9')))
                {
                    throw new InvalidOperationException("Project ID must be alphanumeric characters only (a-z, A-Z, 0-9)");
                }
            }

            string iconPathRaw = this.IconFilePath;
            if (iconPathRaw != null)
            {
                List<string> absoluteIconPaths = new List<string>();
                foreach (string iconFile in iconPathRaw.Split(','))
                {
                    string trimmedIconFile = iconFile.Trim();
                    if (!FileUtil.IsAbsolutePath(trimmedIconFile))
                    {
                        trimmedIconFile = FileUtil.JoinPath(this.ProjectDirectory, trimmedIconFile);
                    }
                    if (!FileUtil.FileExists(trimmedIconFile))
                    {
                        throw new InvalidOperationException("Icon file path does not exist: " + this.IconFilePath);
                    }
                    absoluteIconPaths.Add(trimmedIconFile);
                }
                this.IconFilePath = string.Join(",", absoluteIconPaths);
            }

            string launchScreenPath = this.LaunchScreenPath;
            if (launchScreenPath != null)
            {
                if (!FileUtil.IsAbsolutePath(launchScreenPath))
                {
                    launchScreenPath = FileUtil.JoinPath(this.ProjectDirectory, launchScreenPath);
                }
                if (!FileUtil.FileExists(launchScreenPath))
                {
                    throw new InvalidOperationException("Launch screen file path does not exist: " + this.LaunchScreenPath);
                }
            }

            return this;
        }

        private static FilePath[] ToFilePaths(string projectDir, SourceItem[] sourceDirs)
        {
            Dictionary<string, FilePath> paths = new Dictionary<string, FilePath>();

            foreach (SourceItem sourceDir in sourceDirs)
            {
                string relative = FileUtil.GetCanonicalizeUniversalPath(sourceDir.Value);
                FilePath filePath = new FilePath(relative, projectDir, sourceDir.Alias);
                paths[filePath.AbsolutePath] = filePath;
            }

            List<FilePath> output = new List<FilePath>();
            foreach (string key in paths.Keys.OrderBy<string, string>(k => k))
            {
                output.Add(paths[key]);
            }
            return output.ToArray();
        }

        private static string DoReplacement(string target, string value)
        {
            return value != null && value.Contains("%TARGET_NAME%")
                ? value.Replace("%TARGET_NAME%", target)
                : value;
        }

        private static Dictionary<string, BuildVarCanonicalized> GenerateBuildVars(BuildItem root, BuildItem target, string targetName)
        {
            Dictionary<string, BuildVar> firstPass = new Dictionary<string, BuildVar>();

            if (root.Var != null)
            {
                foreach (BuildVar rootVar in root.Var)
                {
                    if (rootVar.Id == null)
                    {
                        throw new InvalidOperationException("Build file contains a <var> without an id attribute.");
                    }
                    firstPass.Add(rootVar.Id, rootVar);
                }
            }

            if (target.Var != null)
            {
                foreach (BuildVar targetVar in target.Var)
                {
                    if (targetVar.Id == null)
                    {
                        throw new InvalidOperationException("Build file target contains a <var> without an id attribute.");
                    }
                    firstPass[targetVar.Id] = targetVar;
                }
            }

            Dictionary<string, BuildVarCanonicalized> output = new Dictionary<string, BuildVarCanonicalized>();

            foreach (BuildVar rawElement in firstPass.Values)
            {
                string id = rawElement.Id;
                string value = rawElement.Value;
                int intValue = 0;
                double floatValue = 0;
                bool boolValue = false;
                VarType type = VarType.BOOLEAN;
                switch ((rawElement.Type ?? "string").ToLowerInvariant())
                {
                    case "int":
                    case "integer":
                        type = VarType.INT;
                        break;
                    case "float":
                    case "double":
                        type = VarType.FLOAT;
                        break;
                    case "bool":
                    case "boolean":
                        type = VarType.BOOLEAN;
                        break;
                    case "string":
                        type = VarType.STRING;
                        break;
                    default:
                        throw new InvalidOperationException("Build file variable '" + id + "' contains an unrecognized type: '" + rawElement.Type + "'. Types must be 'string', 'integer', 'boolean', or 'float'.");
                }

                int score = (rawElement.EnvironmentVarValue != null ? 1 : 0)
                    + (rawElement.Value != null ? 1 : 0);

                if (score != 1)
                {
                    throw new InvalidOperationException("Build file variable '" + id + "' must contain either a <value> or a <env> content element but not both.");
                }

                if (value == null)
                {
                    value = System.Environment.GetEnvironmentVariable(rawElement.EnvironmentVarValue);
                    if (value == null)
                    {
                        throw new InvalidOperationException("Build file varaible '" + id + "' references an environment variable that is not set: '" + rawElement.EnvironmentVarValue + "'");
                    }
                    value = DoReplacement(targetName, value.Trim());
                }

                switch (type)
                {
                    case VarType.INT:
                        if (!int.TryParse(value, out intValue))
                        {
                            throw new InvalidOperationException("Build file variable: '" + id + "' contains an invalid integer value.");
                        }
                        break;
                    case VarType.FLOAT:
                        if (!Util.ParseDouble(value, out floatValue))
                        {
                            throw new InvalidOperationException("Build file variable: '" + id + "' contains an invalid float value.");
                        }
                        break;
                    case VarType.BOOLEAN:
                        switch (value.ToLowerInvariant())
                        {
                            case "0":
                            case "no":
                            case "false":
                            case "f":
                            case "n":
                                boolValue = false;
                                break;
                            case "1":
                            case "true":
                            case "t":
                            case "yes":
                            case "y":
                                boolValue = true;
                                break;
                            default:
                                throw new InvalidOperationException("Build file variable: '" + id + "' contains an invalid boolean valud.");
                        }
                        break;
                    case VarType.STRING:
                        break;

                    default:
                        break;
                }
                output[id] = new BuildVarCanonicalized()
                {
                    ID = id,
                    Type = type,
                    StringValue = value,
                    IntValue = intValue,
                    FloatValue = floatValue,
                    BoolValue = boolValue
                };
            }

            return output;
        }

        private static ImageSheet[] MergeImageSheets(ImageSheet[] originalSheets, ImageSheet[] newSheets)
        {
            Dictionary<string, List<string>> prefixDirectLookup = new Dictionary<string, List<string>>();
            List<string> order = new List<string>();

            originalSheets = originalSheets ?? new ImageSheet[0];
            newSheets = newSheets ?? new ImageSheet[0];

            foreach (ImageSheet sheet in originalSheets.Concat<ImageSheet>(newSheets))
            {
                if (sheet.Id == null)
                {
                    throw new InvalidOperationException("Image sheet is missing an ID.");
                }

                if (!prefixDirectLookup.ContainsKey(sheet.Id))
                {
                    prefixDirectLookup.Add(sheet.Id, new List<string>());
                    order.Add(sheet.Id);
                }
                prefixDirectLookup[sheet.Id].AddRange(sheet.Prefixes);
            }

            return order
                .Select<string, ImageSheet>(
                    id => new ImageSheet()
                    {
                        Id = id,
                        Prefixes = prefixDirectLookup[id].ToArray()
                    })
                .ToArray();
        }


        public static string GetValidatedCanonicalBuildFilePath(string originalBuildFilePath)
        {
            string buildFilePath = originalBuildFilePath;
            buildFilePath = FileUtil.FinalizeTilde(buildFilePath);
            if (!buildFilePath.StartsWith("/") &&
                !(buildFilePath.Length > 1 && buildFilePath[1] == ':'))
            {
                // Build file will always be absolute. So make it absolute if it isn't already.
                buildFilePath = FileUtil.GetAbsolutePathFromRelativeOrAbsolutePath(buildFilePath);
            }

            if (!FileUtil.FileExists(buildFilePath))
            {
                throw new InvalidOperationException("Build file does not exist: " + originalBuildFilePath);
            }

            return buildFilePath;
        }
    }
}
