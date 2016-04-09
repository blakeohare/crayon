using System.Collections.Generic;
using System.Linq;

namespace Crayon.ParseTree
{
	internal class EnumDefinition : Executable
	{
		public string Name { get; private set; }
		public string Namespace { get; set; }
		public Token NameToken { get; private set; }
		public Token[] Items { get; private set; }
		public Expression[] Values { get; private set; }
		public Dictionary<string, int> IntValue { get; private set; }

		public EnumDefinition(Token enumToken, Token nameToken, string ns, IList<Token> items, IList<Expression> values, Executable owner)
			: base(enumToken, owner)
		{
			this.NameToken = nameToken;
			this.Name = nameToken.Value;
			this.Namespace = ns;
			this.Items = items.ToArray();
			this.Values = values.ToArray();
			this.IntValue = new Dictionary<string, int>();
		}

		internal override IList<Executable> Resolve(Parser parser)
		{
			if (!parser.IsTranslateMode)
			{
				int resolutionState = parser.ConstantAndEnumResolutionState[this];
				if (resolutionState == 2) return new Executable[0];
				if (resolutionState == 1)
				{
					throw new ParserException(this.FirstToken, "The resolution of this enum creates a cycle.");
				}
				parser.ConstantAndEnumResolutionState[this] = 1;
			}
			HashSet<int> consumed = new HashSet<int>();
			int[] valuesArray = new int[this.Items.Length];

			for (int i = 0; i < this.Items.Length; ++i)
			{
				string itemName = this.Items[i].Value;

				if (itemName == "length")
				{
					throw new ParserException(this.Items[i], "The name 'length' is not allowed as an enum value as it is a reserved field. In general, enum members should be in ALL CAPS anyway.");
				}

				if (this.IntValue.ContainsKey(itemName))
				{
					throw new ParserException(this.Items[i], "Duplicate item in same enum. ");
				}

				this.IntValue[itemName] = -1;

				if (this.Values[i] != null)
				{
					IntegerConstant ic = this.Values[i].Resolve(parser) as IntegerConstant;
					if (ic == null)
					{
						throw new ParserException(this.Values[i].FirstToken, "Enum values must be integers or left blank.");
					}
					this.Values[i] = ic;
					if (consumed.Contains(ic.Value))
					{
						throw new ParserException(this.Values[i].FirstToken, "This integer value has already been used in the same enum.");
					}

					consumed.Add(ic.Value);
					this.IntValue[itemName] = ic.Value;
				}
			}
			parser.ConstantAndEnumResolutionState[this] = 2;

			int next = 0;
			for (int i = 0; i < this.Items.Length; ++i)
			{
				if (this.Values[i] == null)
				{
					while (consumed.Contains(next))
					{
						++next;
					}

					this.IntValue[this.Items[i].Value] = next;
					consumed.Add(next);
				}
			}

			parser.AddEnumDefinition(this);

			return Executable.EMPTY_ARRAY;
		}

		internal override void CalculateLocalIdPass(VariableIdAllocator varIds) { }

		internal override void SetLocalIdPass(VariableIdAllocator varIds) { }

		internal override Executable ResolveNames(Parser parser, Dictionary<string, Executable> lookup, string[] imports)
		{
			this.BatchExpressionNameResolver(parser, lookup, imports, this.Values);
			return this;
		}

		internal override void GenerateGlobalNameIdManifest(VariableIdAllocator varIds)
		{
			throw new System.InvalidOperationException(); // should be resolved by now.
		}
	}
}
