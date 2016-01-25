namespace Crayon.ParseTree
{
	internal class BracketIndex : Expression
	{
		public override bool CanAssignTo { get { return true; } }

		public Expression Root { get; set; }
		public Token BracketToken { get; private set; }
		public Expression Index { get; set; }

		public BracketIndex(Expression root, Token bracketToken, Expression index, Executable owner)
			: base(root.FirstToken, owner)
		{
			this.Root = root;
			this.BracketToken = bracketToken;
			this.Index = index;
		}

		internal override Expression Resolve(Parser parser)
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
								case Crayon.BuildContext.VarType.INT: return new IntegerConstant(this.FirstToken, buildVar.IntValue, this.FunctionOrClassOwner);
								case Crayon.BuildContext.VarType.FLOAT: return new FloatConstant(this.FirstToken, buildVar.FloatValue, this.FunctionOrClassOwner);
								case Crayon.BuildContext.VarType.STRING: return new StringConstant(this.FirstToken, buildVar.StringValue, this.FunctionOrClassOwner);
								case Crayon.BuildContext.VarType.BOOLEAN: return new BooleanConstant(this.FirstToken, buildVar.BoolValue, this.FunctionOrClassOwner);
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

		internal override void VariableUsagePass(Parser parser)
		{
			this.Root.VariableUsagePass(parser);
			this.Index.VariableUsagePass(parser);
		}

		internal override void VariableIdAssignmentPass(Parser parser)
		{
			this.Root.VariableIdAssignmentPass(parser);
			this.Index.VariableIdAssignmentPass(parser);
		}

		internal override Expression ResolveNames(Parser parser, System.Collections.Generic.Dictionary<string, Executable> lookup, string[] imports)
		{
			this.Root = this.Root.ResolveNames(parser, lookup, imports);
			this.Index = this.Index.ResolveNames(parser, lookup, imports);
			return this;
		}
	}
}
