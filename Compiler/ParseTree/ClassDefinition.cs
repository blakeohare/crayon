using System.Collections.Generic;
using System.Linq;

namespace Crayon.ParseTree
{
	internal class ClassDefinition : Executable
	{
		public int ClassID { get; private set; }
		public Token NameToken { get; private set; }
		public Token[] SubClasses { get; private set; }
		public FunctionDefinition[] Methods { get; set; }
		public ConstructorDefinition Constructor { get; set; }
		public FieldDeclaration[] Fields { get; set; }
		public string Namespace { get; set; }

		// When a variable in this class is not locally defined, look for a fully qualified name that has one of these prefixes.

		private ClassDefinition baseClassInstance = null;

		public ClassDefinition(
			Token classToken, 
			Token nameToken, 
			IList<Token> subclasses,
			string namespyace, 
			Executable owner)
			: base(classToken, owner)
		{
			this.Namespace = namespyace;
			this.NameToken = nameToken;
			this.SubClasses = subclasses.ToArray();
		}

		internal override IList<Executable> Resolve(Parser parser)
		{
			this.ClassID = parser.GetClassId(this);

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

			parser.CurrentClass = this;
			for (int i = 0; i < this.Methods.Length; ++i)
			{
				this.Methods[i] = (FunctionDefinition)this.Methods[i].Resolve(parser)[0];
			}

			if (this.Constructor != null)
			{
				this.Constructor.Resolve(parser);
			}
			parser.CurrentClass = null;

			bool hasABaseClass = this.baseClassInstance != null;
			if (this.Constructor == null)
			{
				throw new ParserException(this.FirstToken, "Class is missing constructor.");
			}
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

			if (this.baseClassInstance != null)
			{
				parser.VerifySubclassDeclarationOrder(this, this.baseClassInstance);
			}


			return Listify(this);
		}

		internal override void VariableUsagePass(Parser parser)
		{
			if (this.Constructor != null)
			{
				this.Constructor.VariableUsagePass(parser);
			}

			foreach (FunctionDefinition func in this.Methods)
			{
				func.VariableUsagePass(parser);
			}
		}

		internal override void VariableIdAssignmentPass(Parser parser)
		{
			if (this.Constructor != null)
			{
				this.Constructor.VariableIdAssignmentPass(parser);
			}

			foreach (FunctionDefinition func in this.Methods)
			{
				func.VariableIdAssignmentPass(parser);
			}
		}
	}
}
