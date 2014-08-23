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

		private ClassDefinition baseClassInstance = null;

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

			if (this.SubClasses.Length > 0)
			{
				ClassDefinition baseClassDef = parser.GetClass(this.SubClasses[0].Value);
				if (baseClassDef == null)
				{
					throw new ParserException(this.SubClasses[0], "The class '" + this.SubClasses[0].Value + "' could not be found.");
				}
				this.baseClassInstance = baseClassDef;
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

			bool hasABaseClass = this.baseClassInstance != null;
			bool callsBaseConstructor = this.Constructor.BaseToken != null;
			if (hasABaseClass && callsBaseConstructor)
			{
				Expression[] defaultValues = this.baseClassInstance.Constructor.DefaultValues;
				int maxValues = defaultValues.Length;
				int minValues = 0;
				for (int i = 0; i < maxValues; ++i)
				{
					if (defaultValues[i] == null) minValues++;
					else break;
				}
				int baseArgs = this.Constructor.BaseArgs.Length;
				if (baseArgs < minValues || baseArgs > maxValues)
				{
					throw new ParserException(this.Constructor.BaseToken, "Invalid number of arguments passed to base constructor.");
				}
			}
			else if (hasABaseClass && !callsBaseConstructor)
			{
				if (this.baseClassInstance.Constructor != null)
				{
					throw new ParserException(this.FirstToken, "The base class of this class has a constructor which must be called.");
				}
			}
			else if (!hasABaseClass && callsBaseConstructor)
			{
				throw new ParserException(this.Constructor.BaseToken, "Cannot call base constructor without a base class.");
			}
			else // (!hasABaseClass && !callsBaseConstructor)
			{
				// yeah, that's fine.
			}


			return Listify(this);
		}
	}
}
