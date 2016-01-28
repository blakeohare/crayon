using System.Collections.Generic;
using System.Linq;

namespace Crayon.ParseTree
{
	internal class ForLoop : Executable
	{
		public Executable[] Init { get; private set; }
		public Expression Condition { get; private set; }
		public Executable[] Step { get; private set; }
		public Executable[] Code { get; private set; }

		public ForLoop(Token forToken, IList<Executable> init, Expression condition, IList<Executable> step, IList<Executable> code, Executable owner)
			: base(forToken, owner)
		{
			this.Init = init.ToArray();
			this.Condition = condition ?? new BooleanConstant(forToken, true, owner);
			this.Step = step.ToArray();
			this.Code = code.ToArray();
		}

		internal override IList<Executable> Resolve(Parser parser)
		{
			this.Init = Resolve(parser, this.Init).ToArray();
			this.Condition = this.Condition.Resolve(parser);
			this.Step = Resolve(parser, this.Step).ToArray();
			this.Code = Resolve(parser, this.Code).ToArray();

			return Listify(this);
		}

		internal override void GetAllVariableNames(Dictionary<string, bool> lookup)
		{
			foreach (Executable init in this.Init)
			{
				init.GetAllVariableNames(lookup);
			}

			foreach (Executable step in this.Step)
			{
				step.GetAllVariableNames(lookup);
			}

			foreach (Executable line in this.Code)
			{
				line.GetAllVariableNames(lookup);
			}
		}

		internal override void GenerateGlobalNameIdManifest(VariableIdAllocator varIds)
		{
			foreach (Executable ex in this.Init.Concat<Executable>(this.Step).Concat<Executable>(this.Code))
			{
				ex.GenerateGlobalNameIdManifest(varIds);
			}
		}

		internal override void CalculateLocalIdPass(VariableIdAllocator varIds)
		{
			foreach (Executable ex in this.Init.Concat(this.Step).Concat(this.Code))
			{
				ex.CalculateLocalIdPass(varIds);
			}
		}

		internal override void SetLocalIdPass(VariableIdAllocator varIds)
		{
			foreach (Executable ex in this.Init)
			{
				ex.SetLocalIdPass(varIds);
			}
			this.Condition.SetLocalIdPass(varIds);
			foreach (Executable ex in this.Step.Concat(this.Code))
			{
				ex.SetLocalIdPass(varIds);
			}
		}

		internal override Executable ResolveNames(Parser parser, Dictionary<string, Executable> lookup, string[] imports)
		{
			this.BatchExecutableNameResolver(parser, lookup, imports, this.Init);
			this.Condition = this.Condition.ResolveNames(parser, lookup, imports);
			this.BatchExecutableNameResolver(parser, lookup, imports, this.Step);
			this.BatchExecutableNameResolver(parser, lookup, imports, this.Code);
			return this;
		}
	}
}
