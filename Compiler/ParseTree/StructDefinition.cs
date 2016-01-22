using System.Collections.Generic;
using System.Linq;

namespace Crayon.ParseTree
{
	internal class StructDefinition : Executable
	{
		public Token Name { get; private set; }
		public Token[] Fields { get; private set; }
		public Annotation[] Types { get; private set; }

		public StructDefinition(Token structToken, Token nameToken, IList<Token> fields, IList<Annotation> annotations)
			: base(structToken)
		{
			this.Name = nameToken;
			this.Fields = fields.ToArray();
			this.FieldsByIndex = fields.Select<Token, string>(t => t.Value).ToArray();
			this.IndexByField = new Dictionary<string, int>();
			for (int i = 0; i < this.FieldsByIndex.Length; ++i)
			{
				this.IndexByField[this.FieldsByIndex[i]] = i;
			}

			this.Types = annotations.ToArray();
		}

		public string[] FieldsByIndex { get; private set; }
		public Dictionary<string, int> IndexByField { get; private set; }

		internal override IList<Executable> Resolve(Parser parser)
		{
			if (parser.IsByteCodeMode)
			{
				throw new ParserException(this.FirstToken, "Structs are not allowed in byte code mode.");
			}

			parser.AddStructDefinition(this);

			return new Executable[0];
		}

		internal override void VariableUsagePass(Parser parser)
		{
		}

		internal override void VariableIdAssignmentPass(Parser parser)
		{
		}
	}
}
