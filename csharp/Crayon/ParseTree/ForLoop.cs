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

		public ForLoop(Token forToken, IList<Executable> init, Expression condition, IList<Executable> step, IList<Executable> code)
			: base(forToken)
		{
			this.Init = init.ToArray();
			this.Condition = condition;
			this.Step = step.ToArray();
			this.Code = code.ToArray();
		}

		public override IList<Executable> Resolve(Parser parser)
		{
			this.Init = Resolve(parser, this.Init).ToArray();
			this.Condition = this.Condition.Resolve(parser);
			this.Step = Resolve(parser, this.Step).ToArray();
			this.Code = Resolve(parser, this.Code).ToArray();

			return Listify(this);
		}

		public override void GetAllVariableNames(Dictionary<string, bool> lookup)
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
	}
}
