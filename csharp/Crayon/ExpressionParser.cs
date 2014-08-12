using System;
using System.Collections.Generic;
using Crayon.ParseTree;

namespace Crayon
{
	internal static class ExpressionParser
	{
		public static Expression Parse(TokenStream tokens)
		{
			return ParseTernary(tokens);
		}

		private static Expression ParseTernary(TokenStream tokens)
		{
			Expression root = ParseBooleanCombination(tokens);
			if (tokens.PopIfPresent("?"))
			{
				Expression trueExpr = ParseTernary(tokens);
				tokens.PopExpected(":");
				Expression falseExpr = ParseTernary(tokens);

				return new Ternary(root, trueExpr, falseExpr);
			}
			return root;
		}

		private static Expression ParseBooleanCombination(TokenStream tokens)
		{
			Expression expr = ParseBitwiseOp(tokens);
			string next = tokens.PeekValue();
			if (next == "||" || next == "&&")
			{
				List<Expression> expressions = new List<Expression>() { expr };
				List<Token> ops = new List<Token>();
				while (next == "||" || next == "&&")
				{
					ops.Add(tokens.Pop());
					expressions.Add(ParseBitwiseOp(tokens));
					next = tokens.PeekValue();
				}
				return new BooleanCombination(expressions, ops);
			}
			return expr;
		}

		private static Expression ParseBitwiseOp(TokenStream tokens)
		{
			Expression expr = ParseEqualityComparison(tokens);
			string next = tokens.PeekValue();
			if (next == "|" || next == "&" || next == "^")
			{
				Token bitwiseToken = tokens.Pop();
				Expression rightExpr = ParseBitwiseOp(tokens);
				return new BinaryOpChain(expr, bitwiseToken, rightExpr);
			}
			return expr;
		}

		private static Expression ParseEqualityComparison(TokenStream tokens)
		{
			Expression expr = ParseInequalityComparison(tokens);
			string next = tokens.PeekValue();
			if (next == "==" || next == "!=")
			{
				Token equalityToken = tokens.Pop();
				Expression rightExpr = ParseEqualityComparison(tokens);
				return new BinaryOpChain(expr, equalityToken, rightExpr);
			}
			return expr;
		}

		private static Expression ParseInequalityComparison(TokenStream tokens)
		{
			Expression expr = ParseBitShift(tokens);
			string next = tokens.PeekValue();
			if (next == "<" || next == ">" || next == "<=" || next == ">=")
			{
				// Don't allow chaining of inqeualities
				Token opToken = tokens.Pop();
				Expression rightExpr = ParseBitShift(tokens);
				return new BinaryOpChain(expr, opToken, rightExpr);
			}
			return expr;
		}

		private static Expression ParseBitShift(TokenStream tokens)
		{
			Expression expr = ParseAddition(tokens);
			string next = tokens.PeekValue();
			if (next == "<" || next == ">" || next == "<=" || next == ">=")
			{
				Token opToken = tokens.Pop();
				Expression rightExpr = ParseBitShift(tokens);
				return new BinaryOpChain(expr, opToken, rightExpr);
			}
			return expr;
		}

		private static Expression ParseAddition(TokenStream tokens)
		{
			Expression expr = ParseMultiplication(tokens);
			string next = tokens.PeekValue();
			if (next == "+" || next == "-")
			{
				List<Expression> expressions = new List<Expression>() { expr };
				List<Token> ops = new List<Token>();
				while (next == "+" || next == "-")
				{
					ops.Add(tokens.Pop());
					expressions.Add(ParseMultiplication(tokens));
					next = tokens.PeekValue();
				}

				return new BinaryOpChain(expressions, ops);
			}

			return expr;
		}

		private static readonly HashSet<string> MULTIPLICATION_OPS = new HashSet<string>("* / %".Split(' '));
		private static Expression ParseMultiplication(TokenStream tokens)
		{
			Expression expr = ParseNegate(tokens);
			string next = tokens.PeekValue();
			if (MULTIPLICATION_OPS.Contains(next))
			{
				List<Expression> expressions = new List<Expression>() { expr };
				List<Token> ops = new List<Token>();
				while (MULTIPLICATION_OPS.Contains(next))
				{
					ops.Add(tokens.Pop());
					expressions.Add(ParseNegate(tokens));
					next = tokens.PeekValue();
				}

				return new BinaryOpChain(expressions, ops);
			}

			return expr;
		}

		private static readonly HashSet<string> NEGATE_OPS = new HashSet<string>("! -".Split(' '));
		private static Expression ParseNegate(TokenStream tokens)
		{

			string next = tokens.PeekValue();
			if (NEGATE_OPS.Contains(next))
			{
				Token negateOp = tokens.Pop();
				Expression root = ParseNegate(tokens);
				if (negateOp.Value == "!") return new BooleanNot(negateOp, root);
				if (negateOp.Value == "-") return new NegativeSign(negateOp, root);
				throw new Exception("This shouldn't happen.");
			}

			return ParseExponents(tokens);
		}

		private static Expression ParseExponents(TokenStream tokens)
		{
			Expression expr = ParseIncrement(tokens);
			string next = tokens.PeekValue();
			if (next == "**")
			{
				List<Expression> expressions = new List<Expression>() { expr };
				List<Token> ops = new List<Token>();
				while (next == "**")
				{
					ops.Add(tokens.Pop());
					expressions.Add(ParseIncrement(tokens));
					next = tokens.PeekValue();
				}

				return new BinaryOpChain(expressions, ops);
			}

			return expr;
		}

		private static Expression ParseIncrement(TokenStream tokens)
		{
			Expression root;
			if (tokens.IsNext("++") || tokens.IsNext("--"))
			{
				Token incrementToken = tokens.Pop();
				root = ParseParenthesis(tokens);
				return new Increment(incrementToken, incrementToken, incrementToken.Value == "++", true, root);
			}

			root = ParseParenthesis(tokens);
			if (tokens.IsNext("++") || tokens.IsNext("--"))
			{
				Token incrementToken = tokens.Pop();
				return new Increment(root.FirstToken, incrementToken, incrementToken.Value == "++", false, root);
			}

			return root;
		}

		private static Expression ParseParenthesis(TokenStream tokens)
		{
			if (tokens.PopIfPresent("("))
			{
				Expression output = Parse(tokens);
				tokens.PopExpected(")");
				return output;
			}
			return ParseEntity(tokens);
		}

		private static readonly HashSet<char> VARIABLE_STARTER = new HashSet<char>("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ_$".ToCharArray());

		private static Expression ParseInstantiate(TokenStream tokens)
		{
			Token newToken = tokens.PopExpected("new");
			Token classNameToken = tokens.Pop();
			Parser.VerifyIdentifier(classNameToken);

			List<Expression> args = new List<Expression>();
			tokens.PopExpected("(");
			while (!tokens.PopIfPresent(")"))
			{
				if (args.Count > 0)
				{
					tokens.PopExpected(",");
				}
				args.Add(Parse(tokens));
			}

			return new Instantiate(newToken, classNameToken, args);
		}

		private static Expression ParseEntityWithoutSuffixChain(TokenStream tokens)
		{
			string next = tokens.PeekValue();

			if (next == "null") return new NullConstant(tokens.Pop());
			if (next == "true") return new BooleanConstant(tokens.Pop(), true);
			if (next == "false") return new BooleanConstant(tokens.Pop(), false);

			Token peekToken = tokens.Peek();
			if (next.StartsWith("'")) return new StringConstant(tokens.Pop(), StringConstant.ParseOutRawValue(peekToken));
			if (next.StartsWith("\"")) return new StringConstant(tokens.Pop(), StringConstant.ParseOutRawValue(peekToken));
			if (next == "new") return ParseInstantiate(tokens);

			char firstChar = next[0];
			if (VARIABLE_STARTER.Contains(firstChar))
			{
				Token varToken = tokens.Pop();
				return new Variable(varToken, varToken.Value);
			}

			if (firstChar == '[')
			{
				Token bracketToken = tokens.PopExpected("[");
				List<Expression> elements = new List<Expression>();
				bool previousHasCommaOrFirst = true;
				while (!tokens.PopIfPresent("]"))
				{
					if (!previousHasCommaOrFirst) tokens.PopExpected("]"); // throws appropriate error
					elements.Add(Parse(tokens));
					previousHasCommaOrFirst = tokens.PopIfPresent(",");
				}
				return new ListDefinition(bracketToken, elements);
			}

			if (firstChar == '{')
			{
				Token braceToken = tokens.PopExpected("{");
				List<Expression> keys = new List<Expression>();
				List<Expression> values = new List<Expression>();
				bool previousHasCommaOrFirst = true;
				while (!tokens.PopIfPresent("}"))
				{
					if (!previousHasCommaOrFirst) tokens.PopExpected("}"); // throws appropriate error
					keys.Add(Parse(tokens));
					tokens.PopExpected(":");
					values.Add(Parse(tokens));
					previousHasCommaOrFirst = tokens.PopIfPresent(",");
				}
				return new DictionaryDefinition(braceToken, keys, values);
			}

			if (next.Length > 2 && next.Substring(0, 2) == "0x")
			{
				Token intToken = tokens.Pop();
				int intValue = IntegerConstant.ParseIntConstant(intToken, intToken.Value);
				return new IntegerConstant(intToken, intValue);
			}

			if (Parser.IsInteger(next))
			{
				Token numberToken = tokens.Pop();
				string numberValue = numberToken.Value;

				if (tokens.IsNext("."))
				{
					Token decimalToken = tokens.Pop();
					if (decimalToken.HasWhitespacePrefix)
					{
						throw new ParserException(decimalToken, "Decimals cannot have whitespace before them.");
					}

					Token afterDecimal = tokens.Pop();
					if (afterDecimal.HasWhitespacePrefix) throw new ParserException(afterDecimal, "Cannot have whitespace after the decimal.");
					if (!Parser.IsInteger(afterDecimal.Value)) throw new ParserException(afterDecimal, "Decimal must be followed by an integer.");

					numberValue += "." + afterDecimal.Value;

					double floatValue = FloatConstant.ParseValue(numberToken, numberValue);
					return new FloatConstant(numberToken, floatValue);
				}

				int intValue = IntegerConstant.ParseIntConstant(numberToken, numberToken.Value);
				return new IntegerConstant(numberToken, intValue);
			}

			if (tokens.IsNext("."))
			{
				Token dotToken = tokens.PopExpected(".");
				string numberValue = "0.";
				Token postDecimal = tokens.Pop();
				if (postDecimal.HasWhitespacePrefix || !Parser.IsInteger(postDecimal.Value))
				{
					throw new ParserException(dotToken, "Unexpected dot.");
				}

				numberValue += postDecimal.Value;

				double floatValue;
				if (double.TryParse(numberValue, out floatValue))
				{
					return new FloatConstant(dotToken, floatValue);
				}

				throw new ParserException(dotToken, "Invalid float literal.");
			}

			throw new ParserException(tokens.Peek(), "Encountered unexpected token: '" + tokens.PeekValue() + "'");
		}

		private static Expression ParseEntity(TokenStream tokens)
		{
			Expression root = ParseEntityWithoutSuffixChain(tokens);

			bool anySuffixes = true;
			while (anySuffixes)
			{
				if (tokens.IsNext("."))
				{
					Token dotToken = tokens.Pop();
					Token stepToken = tokens.Pop();
					Parser.VerifyIdentifier(stepToken);
					root = new DotStep(root, dotToken, stepToken);
				}
				else if (tokens.IsNext("["))
				{
					Token openBracket = tokens.Pop();
					Expression index = Parse(tokens);
					tokens.PopExpected("]");
					root = new BracketIndex(root, openBracket, index);
				}
				else if (tokens.IsNext("("))
				{
					Token openParen = tokens.Pop();
					List<Expression> args = new List<Expression>();
					while (!tokens.PopIfPresent(")"))
					{
						if (args.Count > 0)
						{
							tokens.PopExpected(",");
						}

						args.Add(Parse(tokens));
					}
					root = new FunctionCall(root, openParen, args);
				}
				else
				{
					anySuffixes = false;
				}
			}
			return root;
		}
	}
}
