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

		internal override Expression Resolve(Parser parser)
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
						if (structDef == null)
						{
							throw new ParserException(this.Root.FirstToken, "The struct '" + structName + "' does not exist.");
						}
						if (!structDef.IndexByField.ContainsKey(step))
						{
							throw new ParserException(this.StepToken, "The struct '" + structDef.Name.Value + "' does not contain a field called '" + step + "'");
						}
						return new DotStepStruct(this.FirstToken, structDef, this);
					}
				}
			}

			if (this.Root is BaseKeyword)
			{
				return new BaseMethodReference(this.Root.FirstToken, this.DotToken, this.StepToken).Resolve(parser);
			}

			if (this.Root is StringConstant)
			{
				if (step == "join")
				{
					throw new ParserException(this.StepToken,
						"There is no join method on strings, you silly Python user. Did you mean to do list.join(string) instead?");
				}
				else if (step == "size")
				{
					throw new ParserException(this.StepToken, "String size is indicated by string.length.");
				}
				else if (step == "length")
				{
					int length = ((StringConstant)this.Root).Value.Length;
					return new IntegerConstant(this.FirstToken, length);
				}
			}

			return this;
		}

		internal override void VariableUsagePass(Parser parser)
		{
			this.Root.VariableUsagePass(parser);
		}

		internal override void VariableIdAssignmentPass(Parser parser)
		{
			this.Root.VariableIdAssignmentPass(parser);
		}
	}
}
