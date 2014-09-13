using System.Collections.Generic;
using System.Linq;

namespace Crayon.ParseTree
{
	internal class IfStatement : Executable
	{
		public Expression Condition { get; private set; }
		public Executable[] TrueCode { get; private set; }
		public Executable[] FalseCode { get; private set; }

		public IfStatement(Token ifToken, Expression condition, IList<Executable> trueCode, IList<Executable> falseCode)
			: base(ifToken)
		{
			this.Condition = condition;
			this.TrueCode = trueCode.ToArray();
			this.FalseCode = falseCode.ToArray();
		}

		public override IList<Executable> Resolve(Parser parser)
		{
			this.Condition = this.Condition.Resolve(parser);

			this.TrueCode = Resolve(parser, this.TrueCode).ToArray();
			this.FalseCode = Resolve(parser, this.FalseCode).ToArray();

			BooleanConstant bc = this.Condition as BooleanConstant;
			if (bc != null)
			{
				return bc.Value ? this.TrueCode : this.FalseCode;
			}

			return Listify(this);
		}

		public override void GetAllVariableNames(Dictionary<string, bool> lookup)
		{
			foreach (Executable line in this.TrueCode)
			{
				line.GetAllVariableNames(lookup);
			}

			foreach (Executable line in this.FalseCode)
			{
				line.GetAllVariableNames(lookup);
			}
		}

		public override bool IsTerminator
		{
			get
			{
				return this.TrueCode.Length > 0 &&
					this.FalseCode.Length > 0 &&
					this.TrueCode[this.TrueCode.Length - 1].IsTerminator &&
					this.FalseCode[this.FalseCode.Length - 1].IsTerminator;
			}
		}
	}
}
