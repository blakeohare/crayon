using System.Collections.Generic;
using System.Linq;

namespace Crayon.ParseTree
{
	internal class ConstructorDefinition : Executable
	{
		public int FunctionID { get; private set; }
		public Executable[] Code { get; set; }
		public Token[] ArgNames { get; private set; }
		public Expression[] DefaultValues { get; private set; }
		public Expression[] BaseArgs { get; private set; }
		public Token BaseToken { get; private set; }
		public int LocalScopeSize { get; set; }
		public int MinArgCount { get; set; }
		public int MaxArgCount { get; set; }

		public ConstructorDefinition(Token constructorToken, IList<Token> args, IList<Expression> defaultValues, IList<Expression> baseArgs, IList<Executable> code, Token baseToken, Executable owner)
			: base(constructorToken, owner)
		{
			this.ArgNames = args.ToArray();
			//this.ArgVarIDs = new int[this.Args.Length];
			this.DefaultValues = defaultValues.ToArray();
			this.BaseArgs = baseArgs.ToArray();
			this.Code = code.ToArray();
			this.BaseToken = baseToken;

			// TODO: verify default args are at the end.
			this.MaxArgCount = this.ArgNames.Length;
			int minArgCount = 0;
			for (int i = 0; i < this.ArgNames.Length; ++i)
			{
				if (this.DefaultValues[i] != null)
				{
					minArgCount++;
				}
			}
			this.MinArgCount = minArgCount;
		}

		internal override IList<Executable> Resolve(Parser parser)
		{
			this.FunctionID = parser.GetNextFunctionId();

			for (int i = 0; i < this.ArgNames.Length; ++i)
			{
				if (this.DefaultValues[i] != null)
				{
					this.DefaultValues[i] = this.DefaultValues[i].Resolve(parser);
				}
			}

			for (int i = 0; i < this.BaseArgs.Length; ++i)
			{
				this.BaseArgs[i] = this.BaseArgs[i].Resolve(parser);
			}

			List<Executable> code = new List<Executable>();
			foreach (Executable line in this.Code)
			{
				code.AddRange(line.Resolve(parser));
			}
			this.Code = code.ToArray();

			return Listify(this);
		}

		internal override void GenerateGlobalNameIdManifest(VariableIdAllocator varIds)
		{
			foreach (Token argToken in this.ArgNames)
			{
				varIds.RegisterVariable(argToken.Value);
			}
			foreach (Executable line in this.Code)
			{
				line.GenerateGlobalNameIdManifest(varIds);
			}
		}

		internal override void CalculateLocalIdPass(VariableIdAllocator varIds)
		{
			throw new System.InvalidOperationException(); // never call this directly on a constructor.
		}

		internal override void SetLocalIdPass(VariableIdAllocator varIds)
		{
			throw new System.InvalidOperationException(); // never call this directly on a constructor.
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

			if (this.BaseArgs != null)
			{
				foreach (Expression ex in this.BaseArgs)
				{
					ex.SetLocalIdPass(variableIds);
				}
			}

			foreach (Executable ex in this.Code)
			{
				ex.SetLocalIdPass(variableIds);
			}
		}

		internal override Executable ResolveNames(Parser parser, Dictionary<string, Executable> lookup, string[] imports)
		{
			if (this.DefaultValues.Length > 0)
			{
				this.BatchExpressionNameResolver(parser, lookup, imports, this.DefaultValues);
			}

			if (this.BaseArgs.Length > 0)
			{
				this.BatchExpressionNameResolver(parser, lookup, imports, this.BaseArgs);
			}
			this.BatchExecutableNameResolver(parser, lookup, imports, this.Code);
			return this;
		}
	}
}
