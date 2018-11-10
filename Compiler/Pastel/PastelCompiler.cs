using Pastel.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pastel
{
    internal class PastelCompiler
    {
        internal PastelCompiler[] IncludedScopes { get; private set; }

        internal IDictionary<string, ExtensibleFunction> ExtensibleFunctions { get; private set; }

        internal Transpilers.AbstractTranslator Transpiler { get; set; }

        public IInlineImportCodeLoader CodeLoader { get; private set; }

        public PastelCompiler(
            PastelContext context,
            Language language,
            IList<PastelCompiler> includedScopes,
            IDictionary<string, object> constants,
            IInlineImportCodeLoader inlineImportCodeLoader,
            ICollection<ExtensibleFunction> extensibleFunctions)
        {
            Dictionary<string, object> langConstants = LanguageUtil.GetLanguageConstants(language);
            Dictionary<string, object> flattenedConstants = new Dictionary<string, object>(langConstants);
            foreach (string key in constants.Keys)
            {
                flattenedConstants[key] = constants[key];
            }

            this.CodeLoader = inlineImportCodeLoader;
            this.Transpiler = LanguageUtil.GetTranspiler(language);
            this.IncludedScopes = includedScopes.ToArray();
            this.ExtensibleFunctions = extensibleFunctions == null
                ? new Dictionary<string, ExtensibleFunction>()
                : extensibleFunctions.ToDictionary(ef => ef.Name);
            this.StructDefinitions = new Dictionary<string, StructDefinition>();
            this.EnumDefinitions = new Dictionary<string, EnumDefinition>();
            this.ConstantDefinitions = new Dictionary<string, VariableDeclaration>();
            this.FunctionDefinitions = new Dictionary<string, FunctionDefinition>();
            this.interpreterParser = new PastelParser(context, flattenedConstants, inlineImportCodeLoader);
        }

        private PastelParser interpreterParser;

        public Dictionary<string, StructDefinition> StructDefinitions { get; set; }
        internal Dictionary<string, EnumDefinition> EnumDefinitions { get; set; }
        internal Dictionary<string, VariableDeclaration> ConstantDefinitions { get; set; }
        public Dictionary<string, FunctionDefinition> FunctionDefinitions { get; set; }

        public StructDefinition[] GetStructDefinitions()
        {
            return this.StructDefinitions.Keys
                .OrderBy(k => k)
                .Select(key => this.StructDefinitions[key])
                .ToArray();
        }

        public FunctionDefinition[] GetFunctionDefinitions()
        {
            return this.FunctionDefinitions.Keys
                .OrderBy(k => k)
                .Select(key => this.FunctionDefinitions[key])
                .ToArray();
        }

        internal HashSet<string> ResolvedFunctions { get; set; }
        internal Queue<string> ResolutionQueue { get; set; }

        internal InlineConstant GetConstantDefinition(string name)
        {
            if (this.ConstantDefinitions.ContainsKey(name))
            {
                return (InlineConstant)this.ConstantDefinitions[name].Value;
            }

            foreach (PastelCompiler includedScope in this.IncludedScopes)
            {
                if (includedScope.ConstantDefinitions.ContainsKey(name))
                {
                    return (InlineConstant)includedScope.ConstantDefinitions[name].Value;
                }
            }

            return null;
        }

        internal EnumDefinition GetEnumDefinition(string name)
        {
            if (this.EnumDefinitions.ContainsKey(name))
            {
                return this.EnumDefinitions[name];
            }

            foreach (PastelCompiler includedScope in this.IncludedScopes)
            {
                if (includedScope.EnumDefinitions.ContainsKey(name))
                {
                    return includedScope.EnumDefinitions[name];
                }
            }

            return null;
        }

        internal StructDefinition GetStructDefinition(string name)
        {
            if (this.StructDefinitions.ContainsKey(name))
            {
                return this.StructDefinitions[name];
            }

            foreach (PastelCompiler includedScope in this.IncludedScopes)
            {
                if (includedScope.StructDefinitions.ContainsKey(name))
                {
                    return includedScope.StructDefinitions[name];
                }
            }

            return null;
        }

        internal FunctionDefinition GetFunctionDefinitionAndMaybeQueueForResolution(string name)
        {
            if (this.FunctionDefinitions.ContainsKey(name))
            {
                return this.FunctionDefinitions[name];
            }

            foreach (PastelCompiler includedScope in this.IncludedScopes)
            {
                if (includedScope.FunctionDefinitions.ContainsKey(name))
                {
                    return includedScope.FunctionDefinitions[name];
                }
            }

            return null;
        }

        public void CompileBlobOfCode(string name, string code)
        {
            ICompilationEntity[] entities = this.interpreterParser.ParseText(name, code);
            foreach (ICompilationEntity entity in entities)
            {
                switch (entity.EntityType)
                {
                    case CompilationEntityType.FUNCTION:
                        FunctionDefinition fnDef = (FunctionDefinition)entity;
                        string functionName = fnDef.NameToken.Value;
                        if (this.FunctionDefinitions.ContainsKey(functionName))
                        {
                            throw new ParserException(fnDef.FirstToken, "Multiple definitions of function: '" + functionName + "'");
                        }
                        this.FunctionDefinitions[functionName] = fnDef;
                        break;

                    case CompilationEntityType.STRUCT:
                        StructDefinition structDef = (StructDefinition)entity;
                        string structName = structDef.NameToken.Value;
                        if (this.StructDefinitions.ContainsKey(structName))
                        {
                            throw new ParserException(structDef.FirstToken, "Multiple definitions of function: '" + structName + "'");
                        }
                        this.StructDefinitions[structName] = structDef;
                        break;

                    case CompilationEntityType.ENUM:
                        EnumDefinition enumDef = (EnumDefinition)entity;
                        string enumName = enumDef.NameToken.Value;
                        if (this.EnumDefinitions.ContainsKey(enumName))
                        {
                            throw new ParserException(enumDef.FirstToken, "Multiple definitions of function: '" + enumName + "'");
                        }
                        this.EnumDefinitions[enumName] = enumDef;
                        break;

                    case CompilationEntityType.CONSTANT:
                        VariableDeclaration assignment = (VariableDeclaration)entity;
                        string targetName = assignment.VariableNameToken.Value;
                        Dictionary<string, VariableDeclaration> lookup = this.ConstantDefinitions;
                        if (lookup.ContainsKey(targetName))
                        {
                            throw new ParserException(
                                assignment.FirstToken,
                                "Multiple definitions of : '" + targetName + "'");
                        }
                        lookup[targetName] = assignment;
                        break;

                    default:
                        throw new NotImplementedException();
                }
            }
        }

        public void Resolve()
        {
            this.ResolveConstants();
            this.ResolveNamesAndCullUnusedCode();
            this.ResolveTypes();
            this.ResolveWithTypeContext();
        }

        private void ResolveConstants()
        {
            HashSet<string> cycleDetection = new HashSet<string>();
            foreach (EnumDefinition enumDef in this.EnumDefinitions.Values)
            {
                if (enumDef.UnresolvedValues.Count > 0)
                {
                    enumDef.DoConstantResolutions(cycleDetection, this);
                }
            }

            foreach (VariableDeclaration constDef in this.ConstantDefinitions.Values)
            {
                if (!(constDef.Value is InlineConstant))
                {
                    string name = constDef.VariableNameToken.Value;
                    cycleDetection.Add(name);
                    constDef.DoConstantResolutions(cycleDetection, this);
                    cycleDetection.Remove(name);
                }
            }
        }

        private void ResolveNamesAndCullUnusedCode()
        {
            this.ResolvedFunctions = new HashSet<string>();
            this.ResolutionQueue = new Queue<string>();

            foreach (string functionName in this.FunctionDefinitions.Keys)
            {
                this.ResolutionQueue.Enqueue(functionName);
            }

            while (this.ResolutionQueue.Count > 0)
            {
                string functionName = this.ResolutionQueue.Dequeue();
                if (!this.ResolvedFunctions.Contains(functionName)) // multiple invocations in a function will put it in the queue multiple times.
                {
                    this.ResolvedFunctions.Add(functionName);
                    this.FunctionDefinitions[functionName].ResolveNamesAndCullUnusedCode(this);
                }
            }

            List<string> unusedFunctions = new List<string>();
            foreach (string functionName in this.FunctionDefinitions.Keys)
            {
                if (!this.ResolvedFunctions.Contains(functionName))
                {
                    unusedFunctions.Add(functionName);
                }
            }

            Dictionary<string, FunctionDefinition> finalFunctions = new Dictionary<string, FunctionDefinition>();
            foreach (string usedFunctionName in this.ResolvedFunctions)
            {
                finalFunctions[usedFunctionName] = this.FunctionDefinitions[usedFunctionName];
            }
            this.FunctionDefinitions = finalFunctions;
        }

        private void ResolveTypes()
        {
            string[] functionNames = this.FunctionDefinitions.Keys.OrderBy(s => s).ToArray();
            foreach (string functionName in functionNames)
            {
                FunctionDefinition functionDefinition = this.FunctionDefinitions[functionName];
                functionDefinition.ResolveTypes(this);
            }
        }

        private void ResolveWithTypeContext()
        {
            string[] functionNames = this.FunctionDefinitions.Keys.OrderBy<string, string>(s => s).ToArray();
            foreach (string functionName in functionNames)
            {
                FunctionDefinition functionDefinition = this.FunctionDefinitions[functionName];
                functionDefinition.ResolveWithTypeContext(this);
            }
        }

        // Delete once migrated to PastelContext
        internal Dictionary<string, string> GetStructCodeByClassTEMP(Transpilers.TranspilerContext ctx, string indent)
        {
            Dictionary<string, string> output = new Dictionary<string, string>();
            foreach (StructDefinition sd in this.GetStructDefinitions())
            {
                string name = sd.NameToken.Value;
                ctx.Transpiler.GenerateCodeForStruct(ctx, sd);
                output[name] = ctx.FlushAndClearBuffer();
            }
            return output;
        }

        // Delete once migrated to PastelContext
        internal string GetFunctionDeclarationsTEMP(Transpilers.TranspilerContext ctx, string indent)
        {
            foreach (FunctionDefinition fd in this.GetFunctionDefinitions())
            {
                if (!alreadySerializedFunctions.Contains(fd))
                {
                    ctx.Transpiler.GenerateCodeForFunctionDeclaration(ctx, fd);
                    ctx.Append(ctx.Transpiler.NewLine);
                }
            }

            return Indent(ctx.FlushAndClearBuffer().Trim(), ctx.Transpiler.NewLine, indent);
        }

        // Delete once migrated to PastelContext
        internal string GetFunctionCodeTEMP(Transpilers.TranspilerContext ctx, string indent)
        {
            foreach (FunctionDefinition fd in this.GetFunctionDefinitions())
            {
                if (!alreadySerializedFunctions.Contains(fd))
                {
                    this.Transpiler.GenerateCodeForFunction(ctx, fd);
                    ctx.Append(this.Transpiler.NewLine);
                }
            }

            return Indent(ctx.FlushAndClearBuffer().Trim(), this.Transpiler.NewLine, indent);
        }
        internal Dictionary<string, string> GetFunctionCodeAsLookupTEMP(Transpilers.TranspilerContext ctx, string indent)
        {
            Dictionary<string, string> output = new Dictionary<string, string>();
            foreach (FunctionDefinition fd in this.GetFunctionDefinitions())
            {
                if (!alreadySerializedFunctions.Contains(fd))
                {
                    ctx.Transpiler.GenerateCodeForFunction(ctx, fd);
                    output[fd.NameToken.Value] = Indent(ctx.FlushAndClearBuffer().Trim(), ctx.Transpiler.NewLine, indent);
                }
            }

            return output;
        }

        private HashSet<FunctionDefinition> alreadySerializedFunctions = new HashSet<FunctionDefinition>();
        internal string GetFunctionCodeForSpecificFunctionAndPopItFromFutureSerializationTEMP(
            string name,
            string swapOutWithNewNameOrNull,
            Transpilers.TranspilerContext ctx,
            string indent)
        {
            FunctionDefinition fd = this.GetFunctionDefinitions().Where(f => f.NameToken.Value == name).FirstOrDefault();
            if (fd == null || this.alreadySerializedFunctions.Contains(fd))
            {
                return null;
            }

            this.alreadySerializedFunctions.Add(fd);

            if (swapOutWithNewNameOrNull != null)
            {
                fd.NameToken = Token.CreateDummyToken(swapOutWithNewNameOrNull);
            }

            this.Transpiler.GenerateCodeForFunction(ctx, fd);
            return Indent(ctx.FlushAndClearBuffer().Trim(), this.Transpiler.NewLine, indent);
        }

        private static string Indent(string code, string newline, string indent)
        {
            if (indent.Length == 0) return code;

            return string.Join(newline, code
                .Split('\n')
                .Select(s => s.Trim())
                .Select(s => s.Length > 0 ? indent + s : ""));
        }
    }
}
