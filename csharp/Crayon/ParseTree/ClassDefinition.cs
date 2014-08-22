using System.Collections.Generic;
using System.Linq;

namespace Crayon.ParseTree
{
	internal class ClassDefinition : Executable
	{
		public Token NameToken { get; private set; }
		public Token[] SubClasses { get; private set; }
		public FunctionDefinition[] Methods { get; private set; }
		public ConstructorDefinition Constructor { get; private set; }

		public ClassDefinition(Token classToken, Token nameToken, IList<Token> subclasses, IList<FunctionDefinition> methods, ConstructorDefinition constructor)
			: base(classToken)
		{
			this.NameToken = nameToken;
			this.SubClasses = subclasses.ToArray();
			this.Methods = methods.ToArray();
			this.Constructor = constructor;
		}

		public override IList<Executable> Resolve(Parser parser)
		{
			if (parser.IsInClass)
			{
				throw new ParserException(this.FirstToken, "Nested classes aren't a thing, yet.");
			}

			if (Parser.IsReservedKeyword(this.NameToken.Value))
			{
				throw new ParserException(this.NameToken, "'" + this.NameToken.Value + "' is a reserved keyword.");
			}

			parser.IsInClass = true;
			for (int i = 0; i < this.Methods.Length; ++i)
			{
				this.Methods[i] = (FunctionDefinition)this.Methods[i].Resolve(parser)[0];
			}

			if (this.Constructor != null)
			{
				this.Constructor.Resolve(parser);
			}
			parser.IsInClass = false;

			return Listify(this);
		}
	}
}
