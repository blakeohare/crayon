using System.Collections.Generic;

namespace Crayon.ParseTree
{
	internal class FieldDeclaration : Executable
	{
		public Token NameToken { get; set; }
		public Expression DefaultValue { get; set; }
		public bool IsStaticField { get; private set; }

		public FieldDeclaration(Token fieldToken, Token nameToken, ClassDefinition owner, bool isStatic)
			: base(fieldToken, owner)
		{
			this.NameToken = nameToken;
			this.DefaultValue = new NullConstant(fieldToken, owner);
			this.IsStaticField = isStatic;
		}

		internal override IList<Executable> Resolve(Parser parser)
		{
			this.DefaultValue = this.DefaultValue.Resolve(parser);
			return Listify(this);
		}

		internal override void VariableUsagePass(Parser parser) { }
		internal override void VariableIdAssignmentPass(Parser parser) { }

		internal override Executable ResolveNames(Parser parser, Dictionary<string, Executable> lookup, string[] imports)
		{
			this.DefaultValue = this.DefaultValue.ResolveNames(parser, lookup, imports);
			return this;
		}
	}
}
