using Pastel.Nodes;
using Pastel.Transpilers;
using System.Collections.Generic;
using System.Linq;

namespace Pastel
{
    public class PastelContext
    {
        private PastelCompiler compiler = null;
        private Language language;
        private List<PastelCompiler> dependencies = new List<PastelCompiler>();
        private Dictionary<string, object> constants = new Dictionary<string, object>();
        private List<ExtensibleFunction> extensibleFunctions = new List<ExtensibleFunction>();
        private Dictionary<string, string> extensibleFunctionTranslations = new Dictionary<string, string>();

        internal AbstractTranslator Transpiler { get; private set; }
        public IInlineImportCodeLoader CodeLoader { get; private set; }

        public PastelContext(Language language, IInlineImportCodeLoader codeLoader)
        {
            this.CodeLoader = codeLoader;
            this.language = language;
            this.Transpiler = LanguageUtil.GetTranspiler(language);
        }

        private TranspilerContext tc = null;
        public TranspilerContext GetTranspilerContext()
        {
            if (this.tc == null)
            {
                this.tc = new TranspilerContext(this.language, this.extensibleFunctionTranslations);
            }
            return this.tc;
        }

        public PastelContext AddDependency(PastelContext context)
        {
            this.dependencies.Add(context.compiler);
            return this;
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

        public Dictionary<string, string> GetCodeForFunctionsLookup()
        {
            TranspilerContext ctx = this.GetTranspilerContext();
            Dictionary<string, string> output = new Dictionary<string, string>();
            foreach (FunctionDefinition fd in this.compiler.GetFunctionDefinitions())
            {
                this.Transpiler.GenerateCodeForFunction(ctx, fd);
                output[fd.NameToken.Value] = ctx.FlushAndClearBuffer();
            }
            return output;
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
            foreach (string fnName in output.Keys.OrderBy(s => s))
            {
                sb.Append(output[fnName]);
                sb.Append(ctx.Transpiler.NewLine);
                sb.Append(ctx.Transpiler.NewLine);
            }
            return sb.ToString().Trim();
        }

        public string GetCodeForGlobals()
        {
            TranspilerContext ctx = this.GetTranspilerContext();
            this.Transpiler.GenerateCodeForGlobalsDefinitions(ctx, this.compiler.GetGlobalsDefinitions());
            return ctx.FlushAndClearBuffer();
        }

        public string GetFunctionCodeForSpecificFunctionAndPopItFromFutureSerialization(string name, string swapOutWithNewNameOrNull)
        {
            TranspilerContext ctx = this.GetTranspilerContext();
            return this.compiler.GetFunctionCodeForSpecificFunctionAndPopItFromFutureSerializationTEMP(name, swapOutWithNewNameOrNull, ctx, "");
        }

        public string GetStringConstantTable()
        {
            TranspilerContext ctx = this.GetTranspilerContext();
            this.Transpiler.GenerateCodeForStringTable(ctx, ctx.StringTableBuilder);
            return ctx.FlushAndClearBuffer();
        }
    } 
}
