using Pastel.Nodes;
using Pastel.Transpilers;
using System.Collections.Generic;
using System.Linq;

namespace Pastel
{
    public class PastelContext
    {
        private PastelCompiler compiler = null;
        public Language Language { get; private set; }
        private List<PastelCompiler> dependencies = new List<PastelCompiler>();
        private List<string> dependencyReferencePrefixes = new List<string>();
        private Dictionary<string, object> constants = new Dictionary<string, object>();
        private List<ExtensibleFunction> extensibleFunctions = new List<ExtensibleFunction>();
        private Dictionary<string, string> extensibleFunctionTranslations = new Dictionary<string, string>();

        internal AbstractTranslator Transpiler { get; private set; }
        public IInlineImportCodeLoader CodeLoader { get; private set; }

        public PastelContext(string languageId, IInlineImportCodeLoader codeLoader)
        {
            Language lang;
            switch (languageId)
            {
                case "C": lang = Language.C; break;
                case "CSHARP": lang = Language.CSHARP; break;
                case "JAVA": lang = Language.JAVA; break;
                case "JAVASCRIPT": lang = Language.JAVASCRIPT; break;
                case "PYTHON": lang = Language.PYTHON; break;
                default: throw new System.InvalidOperationException();
            }

            this.CodeLoader = codeLoader;
            this.Language = lang;
            this.Transpiler = LanguageUtil.GetTranspiler(lang);
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

        public PastelContext AddDependency(PastelContext context, string referencePrefix)
        {
            this.dependencies.Add(context.compiler);
            this.dependencyReferencePrefixes.Add(referencePrefix);
            return this;
        }

        public string GetDependencyPrefix(PastelContext context)
        {
            if (context == this) return null;
            for (int i = 0; i < this.dependencies.Count; ++i)
            {
                if (this.dependencies[i].Context == context)
                {
                    return this.dependencyReferencePrefixes[i];
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

        public PastelContext CompileCode(string filename, string code)
        {
            if (this.compiler == null)
            {
                this.compiler = new PastelCompiler(
                    this,
                    this.Language,
                    this.dependencies,
                    this.constants,
                    this.CodeLoader,
                    this.extensibleFunctions);
            }
            this.compiler.CompileBlobOfCode(filename, code);
            return this;
        }

        public PastelContext CompileFile(string filename)
        {
            return this.CompileCode(filename, this.CodeLoader.LoadCode(filename));
        }

        public PastelContext FinalizeCompilation()
        {
            this.compiler.Resolve();
            return this;
        }

        public Dictionary<string, string> GetCodeForStructs()
        {
            TranspilerContext ctx = this.GetTranspilerContext();
            Dictionary<string, string> output = new Dictionary<string, string>();
            foreach (StructDefinition sd in this.compiler.GetStructDefinitions())
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
            return this.compiler.GetFunctionCodeAsLookupTEMP(ctx, "");
        }

        public string GetCodeForFunctionDeclarations()
        {
            TranspilerContext ctx = this.GetTranspilerContext();
            return this.compiler.GetFunctionDeclarationsTEMP(ctx, "");
        }

        public string GetCodeForFunctions()
        {
            TranspilerContext ctx = this.GetTranspilerContext();
            Dictionary<string, string> output = this.GetCodeForFunctionsLookup();
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            string resourcePath = ctx.Transpiler.HelperCodeResourcePath;
            if (resourcePath != null)
            {
                string helperCode = PastelUtil.ReadAssemblyFileText(typeof(AbstractTranslator).Assembly, resourcePath);
                sb.Append(helperCode);
                sb.Append(ctx.Transpiler.NewLine);
            }

            foreach (string fnName in output.Keys.OrderBy(s => s))
            {
                sb.Append(output[fnName]);
                sb.Append(ctx.Transpiler.NewLine);
                sb.Append(ctx.Transpiler.NewLine);
            }
            return sb.ToString().Trim();
        }

        public string GetFunctionCodeForSpecificFunctionAndPopItFromFutureSerialization(string name, string swapOutWithNewNameOrNull)
        {
            TranspilerContext ctx = this.GetTranspilerContext();
            return this.compiler.GetFunctionCodeForSpecificFunctionAndPopItFromFutureSerializationTEMP(name, swapOutWithNewNameOrNull, ctx, "");
        }
    }
}
