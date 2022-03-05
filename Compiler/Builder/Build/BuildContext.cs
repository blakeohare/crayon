using Builder.Build.BuildParseNodes;
using Builder.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wax.Util.Disk;

namespace Builder
{
    internal class BuildContext
    {
        public string ProjectID { get; set; }
        public string ProjectDirectory { get; set; }
        public string OutputFolder { get; set; }
        public string JsFilePrefix { get; set; }
        public string Platform { get; set; }
        public bool Minified { get; set; }
        public bool ReadableByteCode { get; set; }
        public string GuidSeed { get; set; }
        public string LaunchScreenPath { get; set; }
        public string ProjectTitle { get; set; }
        public Wax.Orientations Orientation { get; set; }
        public string[] LocalDeps { get; set; }
        public string IosBundlePrefix { get; set; }
        public string JavaPackage { get; private set; }
        public bool JsFullPage { get; set; }
        public Locale CompilerLocale { get; set; }
        public string[] IconFilePaths { get; set; }
        public string DelegateMainTo { get; set; }
        public bool RemoveSymbols { get; set; }
        public ProjectFilePath[] SourceFolders { get; set; }
        public Dictionary<string, BuildVar> BuildVariableLookup { get; set; }
        public string Version { get; set; }
        public string Description { get; set; }
        public ProgrammingLanguage RootProgrammingLanguage { get; set; }

        public bool SkipRun { get; set; }
        public Wax.ExtensionArg[] ExtensionArgs { get; set; }

        public DiskUtil DiskUtil { get; private set; }

        private static BuildRoot GetBuildRoot(string buildFile, string projectDir)
        {
            try
            {
                return JsonParserForBuild.Parse(buildFile, projectDir);
            }
            catch (Wax.Util.JsonParser.JsonParserException jpe)
            {
                throw new InvalidOperationException("Build file JSON syntax error: " + jpe.Message);
            }
        }

        public static BuildContext Parse(
            string projectDir,
            string buildFile,
            string nullableTargetName,
            IList<Wax.BuildArg> buildArgOverrides,
            IList<Wax.ExtensionArg> extensionArgOverrides,
            DiskUtil diskUtil)
        {
            BuildRoot buildInput = GetBuildRoot(buildFile, projectDir);

            Dictionary<string, Target> targetsByName = new Dictionary<string, Target>();
            foreach (Target target in buildInput.Targets)
            {
                string name = target.Name;
                if (name == null) throw new InvalidOperationException("A target in the build file is missing a name.");
                if (targetsByName.ContainsKey(name)) throw new InvalidOperationException("There are multiple build targets with the name '" + name + "'.");
                targetsByName[name] = target;
            }

            List<Wax.BuildArg> buildArgs = new List<Wax.BuildArg>();
            List<Wax.ExtensionArg> extensionArgs = new List<Wax.ExtensionArg>();
            List<BuildVar> buildVars = new List<BuildVar>();

            List<BuildItem> buildItems = new List<BuildItem>();
            if (nullableTargetName == null)
            {
                buildItems.Add(buildInput);
            }
            else
            {
                HashSet<string> targetsVisited = new HashSet<string>();
                string nextItem = nullableTargetName;
                Target walker;
                while (nextItem != null)
                {
                    if (targetsVisited.Contains(nextItem)) throw new InvalidOperationException("Build target inheritance loop for '" + nextItem + "' contains a cycle.");
                    targetsVisited.Add(nextItem);
                    if (!targetsByName.ContainsKey(nextItem)) throw new InvalidOperationException("There is no build target named: '" + nextItem + "'.");
                    walker = targetsByName[nextItem];
                    buildItems.Add(walker);
                    nextItem = walker.InheritFrom;
                }
                buildItems.Add(buildInput);
            }

            buildItems.Reverse(); // The attributes in the leaf node have precedence.

            foreach (BuildItem currentItem in buildItems)
            {
                buildArgs.AddRange(currentItem.BuildArgs);
                extensionArgs.AddRange(currentItem.ExtensionArgs);
                buildVars.AddRange(currentItem.BuildVars);
            }

            if (buildArgOverrides != null) buildArgs.AddRange(buildArgOverrides);
            if (extensionArgOverrides != null) extensionArgs.AddRange(extensionArgOverrides);

            PercentReplacer pr = new PercentReplacer()
                .AddReplacement("TARGET_NAME", nullableTargetName ?? "");

            // Do a first pass over all the build args to fetch anything that is used by %PERCENT_REPLACEMENT% and anything
            // that can be defined with multiple values in a list.
            List<string> sources = new List<string>();
            List<string> icons = new List<string>();
            string version = "1.0";
            Locale compilerLocale = Locale.Get("en");
            string projectId = null;
            ProgrammingLanguage programmingLanguage = ProgrammingLanguage.CRAYON;
            List<Wax.BuildArg> remainingBuildArgs = new List<Wax.BuildArg>();
            List<string> envFilesBuilder = new List<string>();
            List<string> deps = new List<string>();

            foreach (Wax.BuildArg buildArg in buildArgs)
            {
                switch (buildArg.Name)
                {
                    case "programmingLanguage":
                        ProgrammingLanguage? pl = ProgrammingLanguageParser.Parse(buildArg.Value);
                        if (pl == null) throw new InvalidOperationException("Invalid programming language in build file: '" + buildArg.Value + "'.");
                        programmingLanguage = pl.Value;
                        break;
                    case "version":
                        version = buildArg.Value;
                        break;
                    case "source":
                        sources.Add(buildArg.Value);
                        break;
                    case "icon":
                        icons.Add(buildArg.Value);
                        break;
                    case "compilerLocale":
                        compilerLocale = Locale.Get(buildArg.Value);
                        break;
                    case "id":
                        projectId = buildArg.Value;
                        break;
                    case "dependencies":
                        deps.Add(buildArg.Value);
                        break;
                    case "envFile":
                        throw new Exception(); // These are eliminated in the parsing phase.
                    default:
                        remainingBuildArgs.Add(buildArg);
                        break;
                }
            }

            if (projectId == null) throw new InvalidOperationException("The projectId is not defined in the build file.");
            if (projectId.Length == 0) throw new InvalidOperationException("projectId cannot be blank.");
            if (projectId.ToCharArray().Any(c => (c < 'a' || c > 'z') && (c < 'A' || c > 'Z') && (c < '0' || c > '9')))
            {
                throw new InvalidOperationException("projectId can only contain alphanumeric characters.");
            }
            if (projectId[0] >= '0' && projectId[0] <= '9') throw new InvalidOperationException("projectId cannot being with a number.");

            pr
                .AddReplacement("PROJECT_ID", projectId)
                .AddReplacement("COMPILER_LANGUAGE", programmingLanguage.ToString().ToUpperInvariant())
                .AddReplacement("VERSION", version)
                .AddReplacement("COMPILER_LOCALE", compilerLocale.ID.ToLowerInvariant());

            ProjectFilePath[] envFiles = ToFilePaths(projectDir, envFilesBuilder);

            BuildContext buildContext = new BuildContext()
            {
                ProjectID = projectId,
                ProjectDirectory = projectDir,
                SourceFolders = ToFilePaths(projectDir, sources),
                Version = version,
                RootProgrammingLanguage = programmingLanguage,
                IconFilePaths = icons
                    // TODO: icon values should be concatenated within a build target, but override previous targets
                    .Select(t => pr.Replace(t))
                    .Select(t => FileUtil.GetAbsolutePathFromRelativeOrAbsolutePath(projectDir, t))
                    .Select(t => FileUtil.GetCanonicalizeUniversalPath(t))
                    .Where(t =>
                    {
                        if (!System.IO.File.Exists(t)) throw new System.InvalidOperationException("The following icon path does not exist: '" + t + "'.");
                        return true;
                    })
                    .ToArray(),
                CompilerLocale = compilerLocale,
                LocalDeps = deps
                    .Select(t => Wax.Util.EnvironmentVariables.DoReplacementsInString(t))
                    .Select(t => pr.Replace(t))
                    .Select(t => FileUtil.GetCanonicalizeUniversalPath(t))
                    .ToArray(),
                ExtensionArgs = extensionArgs.Select(arg => { arg.Value = pr.Replace(arg.Value); return arg; }).ToArray(),
            };
            buildContext.DiskUtil = diskUtil;

            foreach (Wax.BuildArg buildArg in remainingBuildArgs)
            {
                string value = pr.Replace(buildArg.Value);
                switch (buildArg.Name)
                {
                    case "title": buildContext.ProjectTitle = pr.Replace(value); break;
                    case "description": buildContext.Description = pr.Replace(value); break;
                    case "orientation": buildContext.Orientation = ParseOrientations(value); break;
                    case "output": buildContext.OutputFolder = DiskUtil.JoinPathCanonical(projectDir, pr.Replace(value)); break;
                    case "delegateMainTo": buildContext.DelegateMainTo = value; break;
                    case "removeSymbols": buildContext.RemoveSymbols = GetBoolValue(value); break;
                    case "skipRun": buildContext.SkipRun = GetBoolValue(value); break;
                }
            }

            buildContext.BuildVariableLookup = buildVars.ToDictionary(bv => bv.ID);

            return buildContext;
        }

        private static bool GetBoolValue(string argValue)
        {
            if (argValue == null) return false;
            switch (argValue.ToUpperInvariant())
            {
                case "": // The presence of the argument indicates that it's intended to be set. e.g. -build:argName on the command line instead of -build:argName=1
                case "1":
                case "TRUE":
                case "YES":
                case "ON":
                case "AYE":
                    return true;
                default: return false;
            }
        }

        public async Task<Dictionary<string, string>> GetCodeFiles()
        {
            Dictionary<string, string> output = new Dictionary<string, string>();
            string fileExtension = ProgrammingLanguageParser.LangToFileExtension(this.RootProgrammingLanguage);
            foreach (ProjectFilePath sourceDir in this.SourceFolders)
            {
                string[] files = await FileUtil.GetAllAbsoluteFilePathsDescendentsOfAsync(sourceDir.AbsolutePath);
                foreach (string filepath in files)
                {
                    if (filepath.ToLowerInvariant().EndsWith(fileExtension))
                    {
                        string relativePath = FileUtil.ConvertAbsolutePathToRelativePath(
                            filepath,
                            this.ProjectDirectory);
                        output[relativePath] = await this.DiskUtil.FileReadText(filepath);
                    }
                }
            }
            return output;
        }


        private static string AbsoluteToRelativePath(string absolutePath, string relativeTo)
        {
            string[] absParts = absolutePath.Replace('\\', '/').TrimEnd('/').Split('/');
            string[] relativeToParts = relativeTo.Replace('\\', '/').TrimEnd('/').Split('/');
            int indexDiverge = 0;
            while (indexDiverge < absParts.Length && indexDiverge < relativeToParts.Length && absParts[indexDiverge] == relativeToParts[indexDiverge])
            {
                indexDiverge++;
            }

            int dotDots = relativeToParts.Length - indexDiverge;
            List<string> output = new List<string>();
            for (int i = 0; i < dotDots; ++i)
            {
                output.Add("..");
            }

            for (int i = indexDiverge; i < absParts.Length; ++i)
            {
                output.Add(absParts[i]);
            }

            return string.Join("/", output);
        }

        internal static ProjectFilePath[] ToFilePaths(string projectDir, IList<string> directories)
        {
            Dictionary<string, ProjectFilePath> paths = new Dictionary<string, ProjectFilePath>();

            foreach (string dir in directories)
            {
                string sourceDirValue = Wax.Util.EnvironmentVariables.DoReplacementsInString(dir);
                string relative = FileUtil.GetCanonicalizeUniversalPath(sourceDirValue);
                ProjectFilePath filePath = new ProjectFilePath(relative, projectDir);
                paths[filePath.AbsolutePath] = filePath;
            }

            List<ProjectFilePath> output = new List<ProjectFilePath>();
            foreach (string key in paths.Keys.OrderBy<string, string>(k => k))
            {
                output.Add(paths[key]);
            }
            return output.ToArray();
        }

        internal class PercentReplacer
        {
            private Dictionary<string, string> replacements = new Dictionary<string, string>();
            public PercentReplacer() { }
            public PercentReplacer AddReplacement(string oldValue, string newValue)
            {
                this.replacements[oldValue] = newValue;
                return this;
            }

            public string Replace(string value)
            {
                if (value == null || !value.Contains('%'))
                {
                    return value;
                }

                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                string[] parts = value.Split('%');
                sb.Append(parts[0]);
                string replacement;
                for (int i = 1; i < parts.Length - 1; ++i)
                {
                    if (this.replacements.TryGetValue(parts[i], out replacement))
                    {
                        sb.Append(replacement);
                        sb.Append(parts[++i]);
                    }
                    else
                    {
                        sb.Append('%');
                        sb.Append(parts[i]);
                    }
                }
                return sb.ToString();
            }
        }

        public static string GetValidatedCanonicalBuildFilePath(string originalBuildFilePath, Wax.WaxHub waxHub)
        {
            string validatedPath = GetValidatedCanonicalBuildFilePathImpl(originalBuildFilePath, waxHub);
            if (validatedPath == null)
            {
                throw new InvalidOperationException("Build file does not exist: " + originalBuildFilePath);
            }
            return validatedPath;
        }

        private static string GetValidatedCanonicalBuildFilePathImpl(string originalBuildFilePath, Wax.WaxHub waxHub)
        {
            string buildFilePath = originalBuildFilePath;

            buildFilePath = FileUtil.FinalizeTilde(buildFilePath);
            if (!buildFilePath.StartsWith("/") &&
                !(buildFilePath.Length > 1 && buildFilePath[1] == ':'))
            {
                // Build file will always be absolute. So make it absolute if it isn't already.
                buildFilePath = FileUtil.GetAbsolutePathFromRelativeOrAbsolutePath(buildFilePath);
            }

            if (!FileUtil.FileExists_DEPRECATED(buildFilePath))
            {
                return null;
            }

            return buildFilePath;
        }

        private static Wax.Orientations ParseOrientations(string rawValue)
        {
            bool down = false;
            bool up = false;
            bool left = false;
            bool right = false;
            if (rawValue == null || rawValue.Length == 0)
            {
                down = true;
            }
            else
            {
                foreach (string orientation in rawValue.Split(",", StringSplitOptions.RemoveEmptyEntries))
                {
                    switch (orientation.Trim())
                    {
                        case "portrait":
                            down = true;
                            break;
                        case "upsidedown":
                            up = true;
                            break;
                        case "landscape":
                            left = true;
                            right = true;
                            break;
                        case "landscapeleft":
                            left = true;
                            break;
                        case "landscaperight":
                            right = true;
                            break;
                        case "all":
                            down = true;
                            up = true;
                            left = true;
                            right = true;
                            break;
                        default:
                            throw new InvalidOperationException("Unrecognized screen orientation: '" + orientation + "'");
                    }
                }
            }

            return new Wax.Orientations()
            {
                SupportsPortrait = down,
                SupportsUpsideDown = up,
                SupportsLandscapeLeft = left,
                SupportsLandscapeRight = right,
            };
        }
    }
}
