using System.Collections.Generic;
using System.Linq;

namespace Crayon.ParseTree
{
	internal class FunctionDefinition : Executable
	{
		public Token NameToken { get; private set; }
		public Token[] ArgNames { get; set; }
		public Expression[] DefaultValues { get; set; }
		private int[] argVarIds = null;
		public Executable[] Code { get; set; }
		public Annotation[] ArgAnnotations { get; set; }
		private Dictionary<string, Annotation> annotations;
		public VariableIdAllocator VariableIds { get; private set; }
		public int NameGlobalID { get; set; }
		public string Namespace { get; set; }

		public FunctionDefinition(
			Token functionToken,
			Executable nullableOwner,
			Token nameToken,
			IList<Annotation> functionAnnotations,
			string namespyace)
			: base(functionToken, nullableOwner)
		{
			this.Namespace = namespyace;
			this.NameToken = nameToken;
			this.annotations = new Dictionary<string, Annotation>();
			foreach (Annotation annotation in functionAnnotations)
			{
				this.annotations[annotation.Type] = annotation;
			}
			this.VariableIds = new VariableIdAllocator();
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

			foreach (Token arg in this.ArgNames)
			{
				this.VariableIds.RegisterVariable(arg.Value);
			}

			foreach (Executable ex in this.Code)
			{
				ex.AssignVariablesToIds(this.VariableIds);
			}

			return Listify(this);
		}

		internal override void AssignVariablesToIds(VariableIdAllocator varIds)
		{
			varIds.RegisterVariable(this.NameToken.Value);
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

		internal override void VariableUsagePass(Parser parser)
		{
			for (int i = 0; i < this.ArgNames.Length; ++i)
			{
				Token arg = this.ArgNames[i];
				parser.VariableRegister(arg.Value, true, arg);
				Expression defaultValue = this.DefaultValues[i];
				if (defaultValue != null)
				{
					defaultValue.VariableUsagePass(parser);
				}
			}

			for (int i = 0; i < this.Code.Length; ++i)
			{
				this.Code[i].VariableUsagePass(parser);
			}
		}

		internal override void VariableIdAssignmentPass(Parser parser)
		{
			for (int i = 0; i < this.ArgNames.Length; ++i)
			{
				this.ArgVarIDs[i] = parser.VariableGetLocalAndGlobalIds(this.ArgNames[i].Value)[0];
				if (this.DefaultValues[i] != null)
				{
					this.DefaultValues[i].VariableIdAssignmentPass(parser);
				}
			}

			for (int i = 0; i < this.Code.Length; ++i)
			{
				this.Code[i].VariableIdAssignmentPass(parser);
			}
		}
	}
}
