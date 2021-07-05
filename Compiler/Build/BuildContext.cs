using Build.BuildParseNodes;
using Common;
using CommonUtil;
using CommonUtil.Disk;
using CommonUtil.Json;
using Localization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Build
{
    public class BuildContext
    {
        public BuildContext()
        {
            this.TopLevelAssembly = new AssemblyContext(this);
        }

        public AssemblyContext TopLevelAssembly { get; set; }
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
        public string Orientation { get; set; }
        public string[] LocalDeps { get; set; }
        public string IosBundlePrefix { get; set; }
        public string JavaPackage { get; private set; }
        public bool JsFullPage { get; set; }
        public Locale CompilerLocale { get; set; }
        public string[] IconFilePaths { get; set; }
        public string DelegateMainTo { get; set; }
        public bool RemoveSymbols { get; set; }

        private static Target FindTarget(string targetName, IList<Target> targets)
        {
            foreach (Target target in targets)
            {
                if (target.Name == null) throw new InvalidOperationException("A target in the build file is missing a name.");

                // CBX targets don't have a platform specified.
                if (target.Name != "cbx" && target.Platform == null) throw new InvalidOperationException("A target in the build file is missing a platform.");

                if (target.Name == targetName)
                {
                    return target;
                }
            }
            return null;
        }

        private static BuildRoot GetBuildRoot(string buildFile)
        {
            try
            {
                return JsonParserForBuild.Parse(buildFile);
            }
            catch (JsonParser.JsonParserException jpe)
            {
                throw new InvalidOperationException("Build file JSON syntax error: " + jpe.Message);
            }
        }

        public static BuildContext Parse(string projectDir, string buildFile, string nullableTargetName, bool useRelativePathsInErrors)
        {
            BuildRoot buildInput = GetBuildRoot(buildFile);
            string platform = null;
            Dictionary<string, BuildVarCanonicalized> varLookup;
            string targetName = nullableTargetName;
            Target desiredTarget = null;
            if (nullableTargetName != null)
            {
                desiredTarget = FindTarget(targetName, buildInput.Targets);

                if (desiredTarget == null)
                {
                    throw new InvalidOperationException("Build target does not exist in build file: '" + targetName + "'.");
                }

                platform = desiredTarget.Platform;
            }
            else
            {
                targetName = "cbx";
                desiredTarget = FindTarget(targetName, buildInput.Targets) ?? new Target();
            }

            Dictionary<string, string> replacements = new Dictionary<string, string>() {
                { "TARGET_NAME", targetName }
            };
            varLookup = BuildVarParser.GenerateBuildVars(projectDir, buildInput, desiredTarget, replacements);

            if (desiredTarget.HasLegacyIcon || buildInput.HasLegacyIcon)
            {
                // TODO: remove this in 2.2.0 or something
                throw new InvalidOperationException(
                    "This build file has a string property for an icon path. " +
                    "This has been changed to a JSON array of strings for icon paths with a key called \"icons\" instead of \"icon\". " +
                    "Please update your build file accordingly.");
            }

            if (desiredTarget.HasLegacyTitle || buildInput.HasLegacyTitle)
            {
                throw new InvalidOperationException("This build file has a \"default-title\" property, which was changed to just \"title\" in 2.1.0. Please update your build file accordingly.");
            }

            SourceItem[] sources = desiredTarget.SourcesNonNull.Union(buildInput.SourcesNonNull).ToArray();
            string output = desiredTarget.Output ?? buildInput.Output;
            string projectId = desiredTarget.ProjectId ?? buildInput.ProjectId;
            string version = desiredTarget.Version ?? buildInput.Version ?? "1.0";
            string jsFilePrefix = desiredTarget.JsFilePrefix ?? buildInput.JsFilePrefix;
            bool jsFullPage = NullableBoolean.ToBoolean(desiredTarget.JsFullPageRaw ?? buildInput.JsFullPageRaw, false);
            // TODO: maybe set this default value to true, although this does nothing as of now.
            bool minified = NullableBoolean.ToBoolean(desiredTarget.MinifiedRaw ?? buildInput.MinifiedRaw, false);
            bool exportDebugByteCode = BoolUtil.Parse(desiredTarget.ExportDebugByteCodeRaw ?? buildInput.ExportDebugByteCodeRaw);
            string guidSeed = desiredTarget.GuidSeed ?? buildInput.GuidSeed ?? "";
            // TODO: make this a string array.
            string[] iconFilePaths = CombineAndFlattenStringArrays(desiredTarget.IconFilePaths, buildInput.IconFilePaths);
            string launchScreen = desiredTarget.LaunchScreen ?? buildInput.LaunchScreen;
            string projectTitle = desiredTarget.ProjectTitle ?? buildInput.ProjectTitle;
            string orientation = desiredTarget.Orientation ?? buildInput.Orientation;
            string iosBundlePrefix = desiredTarget.IosBundlePrefix ?? buildInput.IosBundlePrefix;
            string javaPackage = desiredTarget.JavaPackage ?? buildInput.JavaPackage;
            string[] localDeps = CombineAndFlattenStringArrays(desiredTarget.LocalDeps, buildInput.LocalDeps);
            string description = desiredTarget.Description ?? buildInput.Description ?? "";
            string compilerLocale = desiredTarget.CompilerLocale ?? buildInput.CompilerLocale ?? "en";
            string programmingLanguage = buildInput.ProgrammingLanguage ?? "Crayon";
            string delegateMainTo = desiredTarget.DelegateMainTo ?? buildInput.DelegateMainTo;
            bool removeSymbols = NullableBoolean.ToBoolean(desiredTarget.RemoveSymbols ?? buildInput.RemoveSymbols, false);

            if (output == null)
            {
                throw new InvalidOperationException("No output directory defined.");
            }

            PercentReplacer pr = new PercentReplacer()
                .AddReplacement("COMPILER_VERSION", VersionInfo.VersionString)
                .AddReplacement("COMPILER_LANGUAGE", programmingLanguage)
                .AddReplacement("TARGET_NAME", targetName);

            version = pr.Replace(version);
            pr.AddReplacement("VERSION", version);

            compilerLocale = pr.Replace(compilerLocale);
            pr.AddReplacement("COMPILER_LOCALE", compilerLocale);

            output = FileUtil.GetCanonicalizeUniversalPath(pr.Replace(output));
            projectId = pr.Replace(projectId);
            jsFilePrefix = pr.Replace(jsFilePrefix);
            guidSeed = pr.Replace(guidSeed);
            iconFilePaths = iconFilePaths
                .Select(t => pr.Replace(t))
                .Select(t => FileUtil.GetAbsolutePathFromRelativeOrAbsolutePath(projectDir, t))
                .Select(t => FileUtil.GetCanonicalizeUniversalPath(t))
                .ToArray();
            launchScreen = pr.Replace(launchScreen);
            projectTitle = pr.Replace(projectTitle);
            orientation = pr.Replace(orientation);
            iosBundlePrefix = pr.Replace(iosBundlePrefix);
            javaPackage = pr.Replace(javaPackage);
            programmingLanguage = pr.Replace(programmingLanguage);
            localDeps = localDeps
                .Select(t => CommonUtil.Environment.EnvironmentVariables.DoReplacementsInString(t))
                .Select(t => pr.Replace(t))
                .Select(t => FileUtil.GetCanonicalizeUniversalPath(t))
                .ToArray();
            description = pr.Replace(description);

            BuildContext buildContext = new BuildContext()
            {
                ProjectDirectory = projectDir,
                JsFilePrefix = jsFilePrefix,
                OutputFolder = output,
                Platform = platform,
                ProjectID = projectId,
                Minified = minified,
                ReadableByteCode = exportDebugByteCode,
                GuidSeed = guidSeed,
                IconFilePaths = iconFilePaths,
                LaunchScreenPath = launchScreen,
                ProjectTitle = projectTitle,
                Orientation = orientation,
                LocalDeps = localDeps,
                IosBundlePrefix = iosBundlePrefix,
                JavaPackage = javaPackage,
                JsFullPage = jsFullPage,
                CompilerLocale = Locale.Get(compilerLocale),
                DelegateMainTo = delegateMainTo,
                RemoveSymbols = removeSymbols,
            };

            ProgrammingLanguage? nullableLanguage = ProgrammingLanguageParser.Parse(programmingLanguage);
            if (nullableLanguage == null)
            {
                throw new InvalidOperationException("Invalid programming language specified: '" + programmingLanguage + "'");
            }

            buildContext.TopLevelAssembly = new AssemblyContext(buildContext)
            {
                Description = description,
                Version = version,
                SourceFolders = ToFilePaths(projectDir, sources),
                BuildVariableLookup = varLookup,
                ProgrammingLanguage = nullableLanguage.Value,
            };

            return buildContext.ValidateValues(useRelativePathsInErrors);
        }

        private static string[] CombineAndFlattenStringArrays(string[] a, string[] b)
        {
            List<string> output = new List<string>();
            if (a != null) output.AddRange(a);
            if (b != null) output.AddRange(b);
            return output.ToArray();
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

        public BuildContext ValidateValues(bool useRelativePathsInErrors)
        {
            if (this.ProjectID == null) throw new InvalidOperationException("There is no project-id for this build target.");
            if (this.TopLevelAssembly.SourceFolders.Length == 0) throw new InvalidOperationException("There are no source paths for this build target.");
            if (this.OutputFolder == null) throw new InvalidOperationException("There is no output path for this build target.");

            foreach (char c in this.ProjectID)
            {
                if (!((c >= 'a' && c <= 'z') ||
                    (c >= 'A' && c <= 'Z') ||
                    (c >= '0' && c <= '9')))
                {
                    throw new InvalidOperationException("Project ID must be alphanumeric characters only (a-z, A-Z, 0-9)");
                }
            }

            string[] invalidIconPaths = this.IconFilePaths
                .Where(t => !FileUtil.FileExists(t))
                .Select(absPath => useRelativePathsInErrors ? AbsoluteToRelativePath(absPath, this.ProjectDirectory) : absPath)
                .ToArray();
            if (invalidIconPaths.Length > 0)
            {
                throw new InvalidOperationException("The following icon file paths do not exist: " + string.Join(",", invalidIconPaths));
            }

            string launchScreenPath = this.LaunchScreenPath;
            if (launchScreenPath != null)
            {
                if (!Path.IsAbsolute(launchScreenPath))
                {
                    launchScreenPath = FileUtil.JoinPath(this.ProjectDirectory, launchScreenPath);
                }
                if (!FileUtil.FileExists(launchScreenPath))
                {
                    throw new InvalidOperationException("Launch screen file path does not exist: " + this.LaunchScreenPath);
                }
            }

            List<string> newLocalDeps = new List<string>();
            foreach (string localDep in this.LocalDeps)
            {
                string manifestPath = localDep.EndsWith("/manifest.json")
                    ? localDep
                    : FileUtil.JoinAndCanonicalizePath(localDep, "manifest.json");

                string fullManifestPath = FileUtil.GetAbsolutePathFromRelativeOrAbsolutePath(this.ProjectDirectory, manifestPath);

                if (FileUtil.FileExists(fullManifestPath))
                {
                    newLocalDeps.Add(fullManifestPath.Substring(0, fullManifestPath.Length - "/manifest.json".Length));
                }
                else
                {
                    throw new InvalidOperationException("The path '" + localDep + "' does not point to a valid library with a manifest.json file. '" + fullManifestPath + "' does not exist.");
                }
            }

            this.LocalDeps = newLocalDeps.ToArray();

            return this;
        }

        private static FilePath[] ToFilePaths(string projectDir, SourceItem[] sourceDirs)
        {
            Dictionary<string, FilePath> paths = new Dictionary<string, FilePath>();

            foreach (SourceItem sourceDir in sourceDirs)
            {
                string sourceDirValue = CommonUtil.Environment.EnvironmentVariables.DoReplacementsInString(sourceDir.Value);
                string relative = FileUtil.GetCanonicalizeUniversalPath(sourceDirValue);
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

        private class PercentReplacer
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
