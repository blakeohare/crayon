using System.Collections.Generic;
using System.Linq;

namespace Crayon.ParseTree
{
	internal class WhileLoop : Executable
	{
		public Expression Condition { get; private set; }
		public Executable[] Code { get; private set; }

		public WhileLoop(Token whileToken, Expression condition, IList<Executable> code)
			: base(whileToken)
		{
			this.Condition = condition;
			this.Code = code.ToArray();
		}

		public override IList<Executable> Resolve(Parser parser)
		{
			this.Condition = this.Condition.Resolve(parser);
			this.Code = Resolve(parser, this.Code).ToArray();
			return Listify(this);
		}

		public override void GetAllVariableNames(Dictionary<string, bool> lookup)
		{
			foreach (Executable line in this.Code)
			{
				line.GetAllVariableNames(lookup);
			}
		}
	}
}
