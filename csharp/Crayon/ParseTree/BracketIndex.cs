namespace Crayon.ParseTree
{
	internal class BracketIndex : Expression
	{
		public Expression Root { get; set; }
		public Token BracketToken { get; private set; }
		public Expression Index { get; set; }

		public BracketIndex(Expression root, Token bracketToken, Expression index)
			: base(root.FirstToken)
		{
			this.Root = root;
			this.BracketToken = bracketToken;
			this.Index = index;
		}

		public override Expression Resolve(Parser parser)
		{
			this.Root = this.Root.Resolve(parser);
			this.Index = this.Index.Resolve(parser);

			if (this.Root is CompileTimeDictionary)
			{
				// Swap out this bracketed expression with a fixed constant.
				CompileTimeDictionary root = (CompileTimeDictionary)this.Root;

				if (root.Type == "var")
				{
					if (this.Index is StringConstant)
					{
						string index = ((StringConstant)this.Index).Value;

						if (parser.BuildContext.BuildVariableLookup.ContainsKey(index))
						{
							Crayon.BuildContext.BuildVarCanonicalized buildVar = parser.BuildContext.BuildVariableLookup[index];
							switch (buildVar.Type)
							{
								case Crayon.BuildContext.VarType.INT: return new IntegerConstant(this.FirstToken, buildVar.IntValue);
								case Crayon.BuildContext.VarType.FLOAT: return new FloatConstant(this.FirstToken, buildVar.FloatValue);
								case Crayon.BuildContext.VarType.STRING: return new StringConstant(this.FirstToken, buildVar.StringValue);
								case Crayon.BuildContext.VarType.BOOLEAN: return new BooleanConstant(this.FirstToken, buildVar.BoolValue);
								default:
									throw new System.Exception("This should not happen."); // invalid types filtered during build context construction.

							}
						}
						else
						{
							throw new ParserException(this.Index.FirstToken, "The build variable with id '" + index + "' is not defined for this target.");
						}
					}
				}
				else
				{
					throw new System.Exception("Unknown compile time dictionary type.");
				}
			}

			return this;
		}
	}
}
