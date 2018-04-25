using Pastel.Nodes;
using Pastel.Transpilers;
using System.Collections.Generic;

namespace Pastel
{
    public class PastelContext
    {
        private PastelCompiler compiler = null;
        private Language language;
        private List<PastelCompiler> dependencies = new List<PastelCompiler>();
        private Dictionary<string, object> constants = new Dictionary<string, object>();
        private List<ExtensibleFunction> extensibleFunctions = new List<ExtensibleFunction>();

        internal AbstractTranslator Transpiler { get; private set; }
        public IInlineImportCodeLoader CodeLoader { get; private set; }

        // Remove this once direct dependence on Compiler is removed.
        public PastelCompiler CompilerDEPRECATED { get { return this.compiler; } }

        // TODO: eventually the transpilers should be internal and this will be an enum.
        public PastelContext(Language language, IInlineImportCodeLoader codeLoader)
        {
            this.CodeLoader = codeLoader;
            this.language = language;
            this.Transpiler = LanguageUtil.GetTranspiler(language);
        }

        public PastelContext AddDependency(PastelContext context)
        {
            this.dependencies.Add(context.compiler);
            return this;
        }

        // TODO: remove this once everything has been migrated to use PastelContext exclusively.
        public PastelContext AddDependencyTEMP(PastelCompiler compiler)
        {
            this.dependencies.Add(compiler);
            return this;
        }

        public PastelContext SetConstant(string key, object value)
        {
            this.constants[key] = value;
            return this;
        }

        public PastelContext AddExtensibleFunction(ExtensibleFunction fn)
        {
            this.extensibleFunctions.Add(fn);
            return this;
        }

        public PastelContext CompileCode(string filename, string code)
        {
            if (this.compiler == null)
            {
                this.compiler = new PastelCompiler(
                    this.language,
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

        public Dictionary<string, string> GetCodeForStructs(TranspilerContext ctx)
        {
            Dictionary<string, string> output = new Dictionary<string, string>();
            foreach (StructDefinition sd in this.compiler.GetStructDefinitions())
            {
                this.Transpiler.GenerateCodeForStruct(ctx, sd);
                output[sd.NameToken.Value] = ctx.FlushAndClearBuffer();
            }
            return output;
        }

        public Dictionary<string, string> GetCodeForFunctions(TranspilerContext ctx)
        {
            Dictionary<string, string> output = new Dictionary<string, string>();
            foreach (FunctionDefinition fd in this.compiler.GetFunctionDefinitions())
            {
                this.Transpiler.GenerateCodeForFunction(ctx, fd);
                output[fd.NameToken.Value] = ctx.FlushAndClearBuffer();
            }
            return output;
        }

        public string GetCodeForGlobals(TranspilerContext ctx)
        {
            this.Transpiler.GenerateCodeForGlobalsDefinitions(ctx, this.compiler.GetGlobalsDefinitions());
            return ctx.FlushAndClearBuffer();
        }
    }
}
