using Common;
using Localization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Parser.ParseTree
{
    public class FunctionDefinition : TopLevelConstruct
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
        public int FinalizedPC { get; set; }
        public int MemberID { get; set; }

        public FunctionDefinition(
            Token functionToken,
            LibraryMetadata library,
            TopLevelConstruct nullableOwner,
            bool isStaticMethod,
            Token nameToken,
            IList<Annotation> functionAnnotations,
            string namespyace,
            FileScope fileScope)
            : base(functionToken, nullableOwner, fileScope)
        {
            this.Library = library;
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

        public override string GetFullyQualifiedLocalizedName(Locale locale)
        {
            string name = this.NameToken.Value;
            if (this.Owner != null)
            {
                name = this.Owner.GetFullyQualifiedLocalizedName(locale) + "." + name;
            }
            return name;
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

        internal override void Resolve(ParserContext parser)
        {
            parser.ValueStackDepth = 0;

            this.FunctionID = parser.GetNextFunctionId();

            for (int i = 0; i < this.DefaultValues.Length; ++i)
            {
                if (this.DefaultValues[i] != null)
                {
                    this.DefaultValues[i] = this.DefaultValues[i].Resolve(parser);
                }

                TODO.RemoveAnnotationsFromParser();

                // Annotations not allowed in byte code mode
                if (this.ArgAnnotations[i] != null)
                {
                    throw new ParserException(this.ArgAnnotations[i].FirstToken, "Unexpected token: '@'");
                }
            }

            this.Code = Executable.Resolve(parser, this.Code).ToArray();

            if (this.Code.Length == 0 || !(this.Code[this.Code.Length - 1] is ReturnStatement))
            {
                List<Executable> newCode = new List<Executable>(this.Code);
                newCode.Add(new ReturnStatement(this.FirstToken, null, this));
                this.Code = newCode.ToArray();
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

        internal override void ResolveNames(ParserContext parser)
        {
            parser.CurrentCodeContainer = this;
            this.BatchExpressionNameResolver(parser, this.DefaultValues);
            this.BatchExecutableNameResolver(parser, this.Code);
            parser.CurrentCodeContainer = null;
        }

        internal void AllocateLocalScopeIds(ParserContext parser)
        {
            VariableIdAllocator variableIds = new VariableIdAllocator();
            for (int i = 0; i < this.ArgNames.Length; ++i)
            {
                variableIds.RegisterVariable(this.ArgNames[i].Value);
            }
            this.PerformLocalIdAllocation(parser, variableIds, VariableIdAllocPhase.REGISTER_AND_ALLOC);
            this.LocalScopeSize = variableIds.Size;
        }

        internal override void PerformLocalIdAllocation(ParserContext parser, VariableIdAllocator varIds, VariableIdAllocPhase phase)
        {
            foreach (Executable ex in this.Code)
            {
                ex.PerformLocalIdAllocation(parser, varIds, VariableIdAllocPhase.REGISTER_AND_ALLOC);
            }
        }

        internal override void GetAllVariablesReferenced(HashSet<Variable> vars)
        {
            // Currently only used to get the variables declared in a function in translation mode. This shouldn't
            // be called directly.
            throw new NotImplementedException();
        }
    }
}
