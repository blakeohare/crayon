using System.Collections.Generic;
using System.Linq;

namespace Crayon.ParseTree
{
	internal class ClassDefinition : Executable
	{
		public int ClassID { get; private set; }
		public ClassDefinition BaseClass { get; private set; }
		public Token NameToken { get; private set; }
		public Token[] BaseClassTokens { get; private set; }
		public string[] BaseClassDeclarations { get; private set; }
		// TODO: interface definitions.
		public FunctionDefinition[] Methods { get; set; }
		public ConstructorDefinition Constructor { get; set; }
		public ConstructorDefinition StaticConstructor { get; set; }
		public FieldDeclaration[] Fields { get; set; }
		public string Namespace { get; set; }

		// When a variable in this class is not locally defined, look for a fully qualified name that has one of these prefixes.

		private Dictionary<string, FunctionDefinition> functionDefinitionsByName = null;
		private Dictionary<string, FieldDeclaration> fieldDeclarationsByName = null;

		private ClassDefinition baseClassInstance = null;

		public ClassDefinition(
			Token classToken, 
			Token nameToken, 
			IList<Token> subclassTokens,
			IList<string> subclassNames,
			string ns, 
			Executable owner)
			: base(classToken, owner)
		{
			this.Namespace = ns;
			this.NameToken = nameToken;
			this.BaseClassTokens = subclassTokens.ToArray();
			this.BaseClassDeclarations = subclassNames.ToArray();
		}

		public FunctionDefinition GetMethod(string name, bool walkUpBaseClasses)
		{
			if (this.functionDefinitionsByName == null)
			{
				foreach (FunctionDefinition fd in this.Methods)
				{
					this.functionDefinitionsByName[fd.NameToken.Value] = fd;
				}
			}

			if (this.functionDefinitionsByName.ContainsKey(name))
			{
				return this.functionDefinitionsByName[name];
			}

			if (walkUpBaseClasses && this.BaseClass != null)
			{
				return this.BaseClass.GetMethod(name, walkUpBaseClasses);
			}

			return null;
		}

		public FieldDeclaration GetField(string name, bool walkUpBaseClasses)
		{
			if (this.fieldDeclarationsByName == null)
			{
				foreach (FieldDeclaration fd in this.Fields)
				{
					this.fieldDeclarationsByName[fd.NameToken.Value] = fd;
				}
			}

			if (this.fieldDeclarationsByName.ContainsKey(name))
			{
				return this.fieldDeclarationsByName[name];
			}

			if (walkUpBaseClasses && this.BaseClass != null)
			{
				return this.BaseClass.GetField(name, walkUpBaseClasses);
			}

			return null;
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

			parser.CurrentClass = this;

			for (int i = 0; i < this.Fields.Length; ++i)
			{
				this.Fields[i] = (FieldDeclaration)this.Fields[i].Resolve(parser)[0];
			}

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

		public void ResolveBaseClasses(Dictionary<string, Executable> lookup, string[] imports)
		{
			List<ClassDefinition> baseClasses = new List<ClassDefinition>();
			List<Token> baseClassesTokens = new List<Token>();
			for (int i = 0; i < this.BaseClassDeclarations.Length; ++i)
			{
				string value = this.BaseClassDeclarations[i];
				Token token = this.BaseClassTokens[i];
				Executable baseClassInstance = Executable.DoNameLookup(lookup, imports, value);
				if (baseClassInstance == null)
				{
					throw new ParserException(token, "Class not found.");
				}

				if (!(baseClassInstance is ClassDefinition))
				{
					baseClasses.Add((ClassDefinition)baseClassInstance);
					baseClassesTokens.Add(token);
				}
				// TODO: else if (baseClassInstance is InterfaceDefinition) { ... }
				else
				{
					throw new ParserException(token, "This is not a class.");
				}
			}

			if (baseClasses.Count > 1)
			{
				throw new ParserException(baseClassesTokens[1], "Multiple base classes found. Did you mean to use an interface?");
			}

			if (baseClasses.Count == 1)
			{
				this.BaseClass = baseClasses[0];
			}
		}

		private string[] ExpandImportsToIncludeThis(string[] imports)
		{
			// Tack on this class as an import. Once classes/enums/constants can be nested inside other classes, it'll be important.
			string thisClassFullyQualified = this.Namespace;
			if (thisClassFullyQualified.Length > 0) thisClassFullyQualified += ".";
			thisClassFullyQualified += this.NameToken.Value;
			List<string> newImports = new List<string>(imports);
			newImports.Add(thisClassFullyQualified);
			return newImports.ToArray();
		}

		internal override Executable ResolveNames(Parser parser, Dictionary<string, Executable> lookup, string[] imports)
		{
			imports = this.ExpandImportsToIncludeThis(imports);

			foreach (FieldDeclaration fd in this.Fields)
			{
				fd.ResolveNames(parser, lookup, imports);
			}

			this.Constructor.ResolveNames(parser, lookup, imports);
			this.BatchExecutableNameResolver(parser, lookup, imports, this.Methods);

			return this;
		}

		public void VerifyNoBaseClassLoops()
		{
			if (this.BaseClass == null) return;

			HashSet<int> classIds = new HashSet<int>();
			ClassDefinition walker = this;
			while (walker != null)
			{
				if (classIds.Contains(walker.ClassID))
				{
					throw new ParserException(this.FirstToken, "This class' parent class hierarchy creates a cycle.");
				}
				classIds.Add(walker.ClassID);
				walker = walker.BaseClass;
			}
		}
	}
}
