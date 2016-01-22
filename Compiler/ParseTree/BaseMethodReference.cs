﻿namespace Crayon.ParseTree
{
	internal class BaseMethodReference : Expression
	{
		public Token DotToken { get; set; }
		public Token StepToken { get; set; }
		public string ClassToWhichThisMethodRefers { get; set; }

		public BaseMethodReference(Token firstToken, Token dotToken, Token stepToken)
			: base(firstToken)
		{
			this.DotToken = dotToken;
			this.StepToken = stepToken;
		}

		public override Expression Resolve(Parser parser)
		{
			this.ClassToWhichThisMethodRefers = parser.CurrentClass.SubClasses[0].Value;
			return this;
		}

		public override void VariableUsagePass(Parser parser)
		{
		}

		public override void VariableIdAssignmentPass(Parser parser)
		{
		}
	}
}
