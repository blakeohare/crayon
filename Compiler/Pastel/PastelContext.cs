using Pastel.Nodes;
using Pastel.Transpilers;
using System.Collections.Generic;
using System.Linq;

namespace Pastel
{
    public class PastelContext
    {
        private PastelCompiler lazyInitCompiler = null;
        private bool dependenciesFinalized = false;
        public Language Language { get; private set; }
        private List<PastelCompiler> dependencies = new List<PastelCompiler>();
        private List<string> dependencyReferenceExportPrefixes = new List<string>();
        private Dictionary<string, int> dependencyReferenceNamespacesToDependencyIndex = new Dictionary<string, int>();
        private Dictionary<string, object> constants = new Dictionary<string, object>();
        private List<ExtensibleFunction> extensibleFunctions = new List<ExtensibleFunction>();
        private Dictionary<string, string> extensibleFunctionTranslations = new Dictionary<string, string>();

        internal AbstractTranslator Transpiler { get; private set; }
        public IInlineImportCodeLoader CodeLoader { get; private set; }

        private string dir;

        public PastelContext(string dir, Language language, IInlineImportCodeLoader codeLoader)
        {
            this.dir = dir;
            this.CodeLoader = codeLoader;
            this.Language = language;
            this.Transpiler = LanguageUtil.GetTranspiler(language);
        }

        public PastelContext(string dir, string languageId, IInlineImportCodeLoader codeLoader)
            : this(dir, LanguageUtil.ParseLanguage(languageId), codeLoader)
        { }

        public override string ToString()
        {
            return "Pastel Context: " + dir;
        }

        // TODO: refactor this all into a platform capabilities object.
        public bool UsesStructDefinitions { get { return this.Transpiler.UsesStructDefinitions; } }
        public bool UsesFunctionDeclarations { get { return this.Transpiler.UsesFunctionDeclarations; } }
        public bool UsesStructDeclarations { get { return this.Transpiler.UsesStructDeclarations; } }

        private TranspilerContext tc = null;
        public TranspilerContext GetTranspilerContext()
        {
            if (this.tc == null)
            {
                this.tc = new TranspilerContext(this.Language, this.extensibleFunctionTranslations);
            }
            return this.tc;
        }

        public PastelContext AddDependency(PastelContext context, string pastelNamespace, string referencePrefix)
        {
            this.dependencyReferenceNamespacesToDependencyIndex[pastelNamespace] = this.dependencies.Count;
            this.dependencies.Add(context.GetCompiler());
            this.dependencyReferenceExportPrefixes.Add(referencePrefix);
            return this;
        }

        public PastelContext MarkDependenciesAsFinalized()
        {
            this.dependenciesFinalized = true;
            return this;
        }

        public string GetDependencyExportPrefix(PastelContext context)
        {
            if (context == this) return null;
            for (int i = 0; i < this.dependencies.Count; ++i)
            {
                if (this.dependencies[i].Context == context)
                {
                    return this.dependencyReferenceExportPrefixes[i];
                }
            }
            // This is a hard crash, not a ParserException, as this is currently only accessible when
            // you have a resolved function definition, and so it would be impossible to get that
            // reference if you didn't already list it as a dependency.
            throw new System.Exception("This is not a dependency of this context.");
        }

        public PastelContext SetConstant(string key, object value)
        {
            this.constants[key] = value;
            return this;
        }

        public PastelContext AddExtensibleFunction(ExtensibleFunction fn, string translation)
        {
            this.extensibleFunctions.Add(fn);
            this.extensibleFunctionTranslations[fn.Name] = translation;
            return this;
        }

        private PastelCompiler GetCompiler()
        {
            if (this.lazyInitCompiler == null)
            {
                if (!this.dependenciesFinalized)
                {
                    throw new System.Exception("Cannot get compiler before dependencies are finalized.");
                }
                this.lazyInitCompiler = new PastelCompiler(
                    this,
                    this.Language,
                    this.dependencies,
                    this.dependencyReferenceNamespacesToDependencyIndex,
                    this.constants,
                    this.CodeLoader,
                    this.extensibleFunctions);
            }
            return this.lazyInitCompiler;
        }

        public PastelContext CompileCode(string filename, string code)
        {
            this.GetCompiler().CompileBlobOfCode(filename, code);
            return this;
        }

        public PastelContext CompileFile(string filename)
        {
            return this.CompileCode(filename, this.CodeLoader.LoadCode(filename));
        }

        public PastelContext FinalizeCompilation()
        {
            this.GetCompiler().Resolve();
            return this;
        }

        public Dictionary<string, string> GetCodeForStructs()
        {
            TranspilerContext ctx = this.GetTranspilerContext();
            Dictionary<string, string> output = new Dictionary<string, string>();
            foreach (StructDefinition sd in this.GetCompiler().GetStructDefinitions())
            {
                this.Transpiler.GenerateCodeForStruct(ctx, sd);
                output[sd.NameToken.Value] = ctx.FlushAndClearBuffer();
            }
            return output;
        }

        public string GetCodeForStructDeclaration(string structName)
        {
            TranspilerContext ctx = this.GetTranspilerContext();
            this.Transpiler.GenerateCodeForStructDeclaration(ctx, structName);
            return ctx.FlushAndClearBuffer();
        }

        public Dictionary<string, string> GetCodeForFunctionsLookup()
        {
            TranspilerContext ctx = this.GetTranspilerContext();
            return this.GetCompiler().GetFunctionCodeAsLookupTEMP(ctx, "");
        }

        public string GetCodeForFunctionDeclarations()
        {
            TranspilerContext ctx = this.GetTranspilerContext();
            return this.GetCompiler().GetFunctionDeclarationsTEMP(ctx, "");
        }

        public string GetCodeForFunctions()
        {
            TranspilerContext ctx = this.GetTranspilerContext();
            Dictionary<string, string> output = this.GetCodeForFunctionsLookup();
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            bool isFirst = true;
            string resourcePath = ctx.Transpiler.HelperCodeResourcePath;
            if (resourcePath != null)
            {
                string helperCode = PastelUtil.ReadAssemblyFileText(typeof(AbstractTranslator).Assembly, resourcePath);
                sb.Append(helperCode.TrimEnd());
                isFirst = false;
            }

            foreach (string fnName in output.Keys.OrderBy(s => s))
            {
                if (isFirst)
                {
                    isFirst = false;
                }
                else
                {
                    sb.Append(ctx.Transpiler.NewLine);
                    sb.Append(ctx.Transpiler.NewLine);
                }
                sb.Append(output[fnName]);
            }
            return sb.ToString().Trim();
        }
    }
}
