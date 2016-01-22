using System.Collections.Generic;
using System.Linq;

namespace Crayon.ParseTree
{
	internal class EnumDefinition : Executable
	{
		public string Name { get; private set; }
		public Token NameToken { get; private set; }
		public Token[] Items { get; private set; }
		public Expression[] Values { get; private set; }
		public Dictionary<string, int> IntValue { get; private set; }

		public EnumDefinition(Token enumToken, Token nameToken, IList<Token> items, IList<Expression> values)
			: base(enumToken)
		{
			this.NameToken = nameToken;
			this.Name = nameToken.Value;
			this.Items = items.ToArray();
			this.Values = values.ToArray();
			this.IntValue = new Dictionary<string, int>();
		}

		internal override IList<Executable> Resolve(Parser parser)
		{
			HashSet<int> consumed = new HashSet<int>();
			int[] valuesArray = new int[this.Items.Length];

			for (int i = 0; i < this.Items.Length; ++i)
			{
				string itemName = this.Items[i].Value;

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
					if (ic.Value < 0)
					{
						throw new ParserException(this.Values[i].FirstToken, "Only non-negative values may be used for enum values.");
					}

					consumed.Add(ic.Value);
					this.IntValue[itemName] = ic.Value;
				}
			}

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

		internal override void VariableUsagePass(Parser parser)
		{
		}

		internal override void VariableIdAssignmentPass(Parser parser)
		{
		}
	}
}
