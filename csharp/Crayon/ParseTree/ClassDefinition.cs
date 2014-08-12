using System.Collections.Generic;
using System.Linq;

namespace Crayon.ParseTree
{
	internal class ClassDefinition : Executable
	{
		public Token NameToken { get; private set; }
		public Token[] SubClasses { get; private set; }
		public FunctionDefinition[] Methods { get; private set; }

		public ClassDefinition(Token classToken, Token nameToken, IList<Token> subclasses, IList<FunctionDefinition> methods)
			: base(classToken)
		{
			this.NameToken = nameToken;
			this.SubClasses = subclasses.ToArray();
			this.Methods = methods.ToArray();
		}

		public override IList<Executable> Resolve(Parser parser)
		{
			// TODO: throw if name is a keyword
			// TODO: throw if base class is a keyword

			for (int i = 0; i < this.Methods.Length; ++i)
			{
				this.Methods[i] = (FunctionDefinition)this.Methods[i].Resolve(parser)[0];
			}

			return Listify(this);
		}
	}
}
