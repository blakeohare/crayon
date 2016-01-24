using System.Collections.Generic;

namespace Crayon.ParseTree
{
	internal class FieldDeclaration : Executable
	{
		public Token NameToken { get; set; }
		public Expression DefaultValue { get; set; }

		public FieldDeclaration(Token fieldToken, Token nameToken, ClassDefinition owner)
			: base(fieldToken, owner)
		{
			this.NameToken = nameToken;
			this.DefaultValue = new NullConstant(fieldToken, owner);
		}

		internal override IList<Executable> Resolve(Parser parser)
		{
			this.DefaultValue = this.DefaultValue.Resolve(parser);
			return Listify(this);
		}

		internal override void VariableUsagePass(Parser parser) { }
		internal override void VariableIdAssignmentPass(Parser parser) { }
	}
}
