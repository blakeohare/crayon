using System.Collections.Generic;

namespace Crayon.ParseTree
{
	internal class BaseMethodReference : Expression
	{
		public override bool CanAssignTo { get { return false; } }

		public Token DotToken { get; set; }
		public Token StepToken { get; set; }
		public string ClassToWhichThisMethodRefers { get; set; }

		public BaseMethodReference(Token firstToken, Token dotToken, Token stepToken, Executable owner)
			: base(firstToken, owner)
		{
			this.DotToken = dotToken;
			this.StepToken = stepToken;
		}

		internal override Expression Resolve(Parser parser)
		{
			throw new System.NotImplementedException();
		}

		internal override void SetLocalIdPass(VariableIdAllocator varIds) { }

		internal override Expression ResolveNames(Parser parser, Dictionary<string, Executable> lookup, string[] imports)
		{
			return this;
		}
	}
}
