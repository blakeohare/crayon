using System.Collections.Generic;
using System.Linq;

namespace Crayon.ParseTree
{
    internal class FunctionDefinition : Executable
    {
        public int FunctionID { get; set; }
        public Token NameToken { get; private set; }
        public bool IsStaticMethod { get; private set; }
        public Token[] ArgNames { get; set; }
        public Expression[] DefaultValues { get; set; }
        private int[] argVarIds = null;
        public Executable[] Code { get; set; }
        public Annotation[] ArgAnnotations { get; set; }
        private Dictionary<string, Annotation> annotations;
        public int LocalScopeSize { get; set; }
        public string Namespace { get; set; }
        public int FinalizedPC { get; set; }
        public int MemberID { get; set; }

        public FunctionDefinition(
            Token functionToken,
            Executable nullableOwner,
            bool isStaticMethod,
            Token nameToken,
            IList<Annotation> functionAnnotations,
            string namespyace)
            : base(functionToken, nullableOwner)
        {
            this.IsStaticMethod = isStaticMethod;
            this.Namespace = namespyace;
            this.NameToken = nameToken;
            this.annotations = new Dictionary<string, Annotation>();
            foreach (Annotation annotation in functionAnnotations)
            {
                this.annotations[annotation.Type] = annotation;
            }
            this.MemberID = -1;
        }

        public int[] ArgVarIDs
        {
            get
            {
                if (this.argVarIds == null)
                {
                    this.argVarIds = new int[this.ArgNames.Length];
                }
                return this.argVarIds;
            }
        }

        public Annotation GetAnnotation(string type)
        {
            if (this.annotations.ContainsKey(type)) return this.annotations[type];
            return null;
        }

        internal override IList<Executable> Resolve(Parser parser)
        {
            this.FunctionID = parser.GetNextFunctionId();

            for (int i = 0; i < this.DefaultValues.Length; ++i)
            {
                if (this.DefaultValues[i] != null)
                {
                    this.DefaultValues[i] = this.DefaultValues[i].Resolve(parser);
                }

                // Annotations not allowed in byte code mode
                if (parser.NullablePlatform == null && this.ArgAnnotations[i] != null)
                {
                    throw new ParserException(this.ArgAnnotations[i].FirstToken, "Unexpected token: '@'");
                }
            }

            this.Code = Resolve(parser, this.Code).ToArray();

            if (this.Code.Length == 0 || !(this.Code[this.Code.Length - 1] is ReturnStatement))
            {
                List<Executable> newCode = new List<Executable>(this.Code);
                newCode.Add(new ReturnStatement(this.FirstToken, null, this.FunctionOrClassOwner));
                this.Code = newCode.ToArray();
            }

            return Listify(this);
        }

        internal override void GenerateGlobalNameIdManifest(VariableIdAllocator varIds)
        {
            varIds.RegisterVariable(this.NameToken.Value);
            foreach (Token argToken in this.ArgNames)
            {
                varIds.RegisterVariable(argToken.Value);
            }
            foreach (Executable line in this.Code)
            {
                line.GenerateGlobalNameIdManifest(varIds);
            }
        }

        internal override void GetAllVariableNames(Dictionary<string, bool> lookup)
        {
            foreach (Executable line in this.Code)
            {
                line.GetAllVariableNames(lookup);
            }
        }

        internal IList<string> GetVariableDeclarationList()
        {
            HashSet<string> dontRedeclareThese = new HashSet<string>();
            foreach (Token argNameToken in this.ArgNames)
            {
                dontRedeclareThese.Add(argNameToken.Value);
            }

            Dictionary<string, bool> variableNamesDict = new Dictionary<string, bool>();
            this.GetAllVariableNames(variableNamesDict);
            foreach (string variableName in variableNamesDict.Keys.ToArray())
            {
                if (dontRedeclareThese.Contains(variableName))
                {
                    variableNamesDict.Remove(variableName);
                }
            }

            return variableNamesDict.Keys.OrderBy<string, string>(s => s.ToLowerInvariant()).ToArray();
        }

        internal override void CalculateLocalIdPass(VariableIdAllocator varIds)
        {
            throw new System.InvalidOperationException(); // never call this directly on a function.
        }

        internal override void SetLocalIdPass(VariableIdAllocator varIds)
        {
            throw new System.InvalidOperationException(); // never call this directly on a function.
        }

        internal override Executable ResolveNames(Parser parser, Dictionary<string, Executable> lookup, string[] imports)
        {
            parser.CurrentCodeContainer = this;
            this.BatchExpressionNameResolver(parser, lookup, imports, this.DefaultValues);
            this.BatchExecutableNameResolver(parser, lookup, imports, this.Code);
            parser.CurrentCodeContainer = null;

            return this;
        }

        internal void AllocateLocalScopeIds()
        {
            VariableIdAllocator variableIds = new VariableIdAllocator();
            for (int i = 0; i < this.ArgNames.Length; ++i)
            {
                variableIds.RegisterVariable(this.ArgNames[i].Value);
            }

            foreach (Executable ex in this.Code)
            {
                ex.CalculateLocalIdPass(variableIds);
            }

            this.LocalScopeSize = variableIds.Size;

            foreach (Executable ex in this.Code)
            {
                ex.SetLocalIdPass(variableIds);
            }
        }

        internal override void GetAllVariablesReferenced(HashSet<Variable> vars)
        {
            // Currently only used to get the variables declared in a function in translation mode. This shouldn't
            // be called directly.
            throw new System.NotImplementedException();
        }
    }
}
