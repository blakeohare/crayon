using System.Collections.Generic;

namespace Pastel.Nodes
{
    internal class Variable : Expression
    {
        public readonly static HashSet<string> BANNED_NAMES = new HashSet<string>() {
            "len",
        };

        public Variable(Token token, ICompilationEntity owner) : base(token, owner)
        {
            this.ApplyPrefix = true;
            if (BANNED_NAMES.Contains(token.Value))
            {
                throw new ParserException(token, "The name '" + token.Value + "' is reserved for some platforms and cannot be used.");
            }
        }

        // Some generated code needs to namespace itself different to prevent collision with translated variables.
        // For example, some of the Python switch statement stuff uses temporary variables that are not in the original code.
        public bool ApplyPrefix { get; set; }

        public string Name { get { return this.FirstToken.Value; } }

        public override Expression ResolveNamesAndCullUnusedCode(PastelCompiler compiler)
        {
            string name = this.Name;

            InlineConstant constantValue = compiler.GetConstantDefinition(name);
            if (constantValue != null)
            {
                return constantValue.CloneWithNewToken(this.FirstToken);
            }

            if (name == "Core")
            {
                return new CoreNamespaceReference(this.FirstToken, this.Owner);
            }

            if (name == "Extension")
            {
                return new ExtensibleNamespaceReference(this.FirstToken, this.Owner);
            }

            FunctionDefinition functionDefinition = compiler.GetFunctionDefinition(name);
            if (functionDefinition != null)
            {
                return new FunctionReference(this.FirstToken, functionDefinition, this.Owner);
            }

            EnumDefinition enumDefinition = compiler.GetEnumDefinition(name);
            if (enumDefinition != null)
            {
                return new EnumReference(this.FirstToken, enumDefinition, this.Owner);
            }

            if (compiler.IncludedScopeNamespacesToIndex.ContainsKey(name))
            {
                int index = compiler.IncludedScopeNamespacesToIndex[name];
                PastelCompiler referencedScope = compiler.IncludedScopes[index];
                return new DependencyNamespaceReference(this.FirstToken, referencedScope, this.Owner);
            }

            return this;
        }

        internal override Expression ResolveType(VariableScope varScope, PastelCompiler compiler)
        {
            PType type = varScope.GetTypeOfVariable(this.Name);
            this.ResolvedType = type;
            if (type == null)
            {
                throw new ParserException(this.FirstToken, "The variable '" + this.Name + "' is not defined.");
            }

            return this;
        }

        internal override Expression ResolveWithTypeContext(PastelCompiler compiler)
        {
            return this;
        }
    }
}
