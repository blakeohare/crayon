using System.Collections.Generic;
using System.Linq;

namespace Crayon.ParseTree
{
	internal class DoWhileLoop : Executable
	{
		public Executable[] Code { get; private set; }
		public Expression Condition { get; private set; }

		public DoWhileLoop(Token doToken, IList<Executable> code, Expression condition)
			: base(doToken)
		{
			this.Code = code.ToArray();
			this.Condition = condition;
		}

		public override IList<Executable> Resolve(Parser parser)
		{
			this.Code = Resolve(parser, this.Code).ToArray();

			this.Condition = this.Condition.Resolve(parser);

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
