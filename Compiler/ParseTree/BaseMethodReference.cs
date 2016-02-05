using System.Collections.Generic;

namespace Crayon.ParseTree
{
	internal class BaseMethodReference : Expression
	{
		public override bool CanAssignTo { get { return false; } }

		public Token DotToken { get; set; }
		public Token StepToken { get; set; }
		public ClassDefinition ClassToWhichThisMethodRefers { get; set; }

		public BaseMethodReference(Token firstToken, Token dotToken, Token stepToken, Executable owner)
			: base(firstToken, owner)
		{
			this.DotToken = dotToken;
			this.StepToken = stepToken;
			ClassDefinition cd = null;
			if (owner is FunctionDefinition)
			{
				cd = (ClassDefinition)((FunctionDefinition)owner).FunctionOrClassOwner;
			}
			else if (owner is ClassDefinition)
			{
				cd = (ClassDefinition)owner;
			}
			else
			{
				throw new System.InvalidOperationException(); // this should not happen.
			}
			this.ClassToWhichThisMethodRefers = cd.BaseClass;
		}

		internal override Expression Resolve(Parser parser)
		{
			return this;
		}

		internal override void SetLocalIdPass(VariableIdAllocator varIds) { }

		internal override Expression ResolveNames(Parser parser, Dictionary<string, Executable> lookup, string[] imports)
		{
			return this;
		}
	}
}
