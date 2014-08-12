using System.Linq;

namespace Crayon.ParseTree
{
	internal class DotStep : Expression
	{
		public Expression Root { get; set; }
		public Token DotToken { get; private set; }
		public Token StepToken { get; private set; }

		public DotStep(Expression root, Token dotToken, Token stepToken)
			: base(root.FirstToken)
		{
			this.Root = root;
			this.DotToken = dotToken;
			this.StepToken = stepToken;
		}

		public override Expression Resolve(Parser parser)
		{
			this.Root = this.Root.Resolve(parser);

			string step = this.StepToken.Value;
			Variable variable = this.Root as Variable;
			if (variable != null)
			{
				string varName = variable.Name;

				EnumDefinition enumDef = parser.GetEnumDefinition(varName);
				if (enumDef != null)
				{
					if (enumDef.IntValue.ContainsKey(step))
					{
						return new IntegerConstant(this.FirstToken, enumDef.IntValue[step]);
					}
					throw new ParserException(this.StepToken, "The enum '" + variable.Name + "' does not contain a definition for '" + step + "'");
				}

				if (parser.IsTranslateMode && varName.Contains('$'))
				{
					string[] parts = varName.Split('$');
					if (parts.Length == 2 && parts[0].Length > 0 && parts[1].Length > 0)
					{
						// struct casting
						string structName = parts[0];
						string realVarName = parts[1];
						StructDefinition structDef = parser.GetStructDefinition(structName);
						if (structDef.IndexByField.ContainsKey(step))
						{
							int index = structDef.IndexByField[step];
							Variable newRoot = new Variable(this.FirstToken, realVarName);
							BracketIndex bi = new BracketIndex(newRoot, DotToken, new IntegerConstant(this.StepToken, index));
							bi = (BracketIndex)bi.Resolve(parser);
							return bi;
						}
						else
						{
							throw new ParserException(this.StepToken, "The struct '" + structDef.Name.Value + "' does not contain a field called '" + step + "'");
						}
					}
				}
			}

			return this;
		}
	}
}
