using SimpleJson;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pastel
{
    public class Program
    {
        public static void PseudoMain(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Incorrect usage. Please provide a path to a Pastel project config file.");
                return;
            }

            string projectPath = args[0];
            if (!System.IO.File.Exists(projectPath))
            {
                Console.WriteLine("Project file does not exist: '" + projectPath + "'");
                return;
            }

            projectPath = System.IO.Path.GetFullPath(projectPath);

            BuildProject(projectPath);
        }

        private static void BuildProject(string projectPath)
        {
            ProjectConfig config = ProjectConfig.Parse(projectPath);
            if (config.Language == Language.NONE) throw new InvalidOperationException("Language not defined in " + projectPath);
            PastelContext context = CompilePastelContexts(config);
            GenerateFiles(config, context);
        }

        private static Dictionary<string, string> GenerateFiles(ProjectConfig config, PastelContext context)
        {
            Dictionary<string, string> output = new Dictionary<string, string>();
            if (context.UsesFunctionDeclarations)
            {
                string funcDeclarations = context.GetCodeForFunctionDeclarations();
                throw new NotImplementedException();
            }

            GenerateFunctionImplementation(config, context.GetCodeForFunctions());

            if (context.UsesStructDefinitions)
            {
                Dictionary<string, string> structDefinitions = context.GetCodeForStructs();
                string[] structNames = structDefinitions.Keys.OrderBy(k => k.ToLower()).ToArray();

                foreach (string structName in structNames)
                {
                    GenerateStructImplementation(config, structName, structDefinitions[structName]);
                }

                if (context.UsesStructDeclarations)
                {
                    Dictionary<string, string> structDeclarations = structNames.ToDictionary(k => context.GetCodeForStructDeclaration(k));

                    foreach (string structName in structNames)
                    {
                        output["struct_decl:" + structName] = structDeclarations[structName];
                    }
                }
            }
            return output;
        }

        private static void GenerateFunctionImplementation(ProjectConfig config, string funcCode)
        {
            Transpilers.AbstractTranslator transpiler = LanguageUtil.GetTranspiler(config.Language);
            funcCode = transpiler.WrapCodeForFunctions(config, funcCode);
            System.IO.File.WriteAllText(config.OutputFileFunctions, funcCode);
        }

        private static void GenerateStructImplementation(ProjectConfig config, string structName, string structCode)
        {
            // TODO(pastel-split): move this to platform specific code.
            // Currently only C# and Java hit this code path, but C code generation will be very different.
            Transpilers.AbstractTranslator transpiler = LanguageUtil.GetTranspiler(config.Language);
            structCode = transpiler.WrapCodeForStructs(config, structCode);
            string fileExtension = config.Language == Language.CSHARP ? ".cs" : ".java";
            string path = System.IO.Path.Combine(config.OutputDirStructs, structName + fileExtension);
            System.IO.File.WriteAllText(path, structCode);
        }

        private static PastelContext CompilePastelContexts(ProjectConfig rootConfig)
        {
            Dictionary<string, ProjectConfig> configsLookup = new Dictionary<string, ProjectConfig>();
            string[] contextPaths = GetContextsInDependencyOrder(rootConfig, configsLookup);
            Dictionary<string, PastelContext> contexts = new Dictionary<string, PastelContext>();
            foreach (string contextPath in contextPaths)
            {
                ProjectConfig config = configsLookup[contextPath];
                PastelContext context = GetContextForConfigImpl(config, contexts, new HashSet<string>());
                context.CompileCode(config.Source, System.IO.File.ReadAllText(config.Source));
                context.FinalizeCompilation();
            }
            return contexts[rootConfig.Path];
        }

        // The reason this order is needed is because the Compiler objects are required for
        // adding dependencies, but Compiler objects cannot be instantiated until the dependencies
        // are resolved.
        private static string[] GetContextsInDependencyOrder(ProjectConfig rootConfig, Dictionary<string, ProjectConfig> configLookupOut)
        {
            List<string> contextPaths = new List<string>();
            AddConfigsInDependencyOrder(rootConfig, contextPaths, configLookupOut);
            return contextPaths.ToArray();
        }

        private static void AddConfigsInDependencyOrder(ProjectConfig current, List<string> paths, Dictionary<string, ProjectConfig> configLookupOut)
        {
            if (configLookupOut.ContainsKey(current.Path)) return;

            foreach (ProjectConfig dep in current.DependenciesByPrefix.Values)
            {
                AddConfigsInDependencyOrder(dep, paths, configLookupOut);
            }

            if (configLookupOut.ContainsKey(current.Path)) throw new Exception(); // This shouldn't happen.

            paths.Add(current.Path);
            configLookupOut[current.Path] = current;
        }

        private static PastelContext GetContextForConfigImpl(
            ProjectConfig config,
            Dictionary<string, PastelContext> contexts,
            HashSet<string> recursionCheck)
        {
            if (contexts.ContainsKey(config.Path)) return contexts[config.Path];
            if (recursionCheck.Contains(config.Path))
            {
                throw new InvalidOperationException("Project config dependencies have a cycle involving: " + config.Path);
            }

            recursionCheck.Add(config.Path);

            ProjectConfig[] requiredScopes = config.DependenciesByPrefix.Values.ToArray();
            for (int i = 0; i < requiredScopes.Length; ++i)
            {
                string path = requiredScopes[i].Path;
                if (!contexts.ContainsKey(path))
                {
                    throw new Exception(); // This shouldn't happen. These are run in dependency order.
                }
            }

            string sourceRootDir = System.IO.Path.GetDirectoryName(config.Source);
            PastelContext context = new PastelContext(sourceRootDir, config.Language, new CodeLoader(sourceRootDir));

            foreach (string prefix in config.DependenciesByPrefix.Keys)
            {
                ProjectConfig depConfig = config.DependenciesByPrefix[prefix];

                // TODO: make this a little more generic for other possible languages
                string funcNs = depConfig.NamespaceForFunctions;
                string classWrapper = depConfig.WrappingClassNameForFunctions;
                string outputPrefix = "";
                if (funcNs != null) outputPrefix = funcNs + ".";
                if (classWrapper != null) outputPrefix += classWrapper + ".";

                context.AddDependency(contexts[depConfig.Path], prefix, outputPrefix);
            }
            context.MarkDependenciesAsFinalized();

            foreach (string constantName in config.Flags.Keys)
            {
                context.SetConstant(constantName, config.Flags[constantName]);
            }

            foreach (ExtensibleFunction exFn in config.GetExtensibleFunctions())
            {
                // TODO(pastel-split): Translation is already set on the extensible function in the
                // new codepath, so the 2nd parameter here ought to be removed.
                context.AddExtensibleFunction(exFn, exFn.Translation);
            }

            contexts[config.Path] = context;
            recursionCheck.Remove(config.Path);
            return context;
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
            string sourceDir = System.IO.Path.GetFullPath(System.IO.Path.GetDirectoryName(source));
            PastelContext pastelContext = new PastelContext(sourceDir, language, codeLoader);

            // TODO(pastel-split): this
            Dictionary<string, string> extensibleFunctionTranslations = new Dictionary<string, string>();
            List<ExtensibleFunction> extensibleFunctions = new List<ExtensibleFunction>();

            // TODO(pastel-split): load constants from manifest
            Dictionary<string, object> constants = new Dictionary<string, object>();

            return true;
        }
    }
}
