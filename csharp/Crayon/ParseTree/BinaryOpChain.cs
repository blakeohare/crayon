using System;
using System.Collections.Generic;
using System.Linq;

namespace Crayon.ParseTree
{
	internal class BinaryOpChain : Expression
	{
		public Expression Left { get; private set; }
		public Expression Right { get; private set; }
		public Token Op { get; private set; }

		public BinaryOpChain(Expression left, Token op, Expression right)
			: base(left.FirstToken)
		{
			this.Left = left;
			this.Right = right;
			this.Op = op;
		}

		public static BinaryOpChain Build(IList<Expression> expressions, IList<Token> ops)
		{
			// TODO: don't pop from the front, you fool. that's silly slow. 
			List<Expression> mutableList = new List<Expression>(expressions);
			List<Token> mutableOps = new List<Token>(ops);
			Expression left = mutableList[0];
			Expression right = mutableList[1];
			
			// Pop! Pop! \o/
			mutableList.RemoveAt(0);
			mutableList.RemoveAt(0);

			Token op = mutableOps[0];
			mutableOps.RemoveAt(0);

			BinaryOpChain boc = new BinaryOpChain(left, op, right);
			while (mutableList.Count > 0)
			{
				right = mutableList[0];
				mutableList.RemoveAt(0);
				op = mutableOps[0];
				mutableOps.RemoveAt(0);
				boc = new BinaryOpChain(boc, op, right);
			}

			return boc;
		}

		private string GetType(Expression expr)
		{
			if (expr is IntegerConstant) return "int";
			if (expr is FloatConstant) return "float";
			if (expr is BooleanConstant) return "bool";
			if (expr is NullConstant) return "null";
			if (expr is StringConstant) return "string";
			return "other";
		}

		public override Expression Resolve(Parser parser)
		{
			if (this.Left.Resolve(parser) == null)
			{

			}
			this.Left = this.Left.Resolve(parser);
			this.Right = this.Right.Resolve(parser);
			string leftType = this.GetType(this.Left);
			string rightType = this.GetType(this.Right);
			return this.TryConsolidate(this.Left.FirstToken, leftType, this.Op, rightType, this.Left, this.Right);
		}

		private Expression TryConsolidate(Token firstToken, string leftType, Token opToken, string rightType, Expression left, Expression right)
		{
			// eliminate all non constants
			if (leftType == "other" || rightType == "other") return this;
			Token tk = firstToken;

			int leftInt, rightInt;
			double leftFloat, rightFloat;

			switch (leftType + " " + opToken.Value + " " + rightType)
			{
				case "null == null": return MakeBool(tk, true);
				case "null != null": return MakeBool(tk, false);

				case "bool == bool": return MakeBool(tk, GetBool(left) == GetBool(right));
				case "bool != bool": return MakeBool(tk, GetBool(left) != GetBool(right));

				case "int + int": return MakeInt(tk, GetInt(left) + GetInt(right));
				case "int - int": return MakeInt(tk, GetInt(left) - GetInt(right));
				case "int * int": return MakeInt(tk, GetInt(left) * GetInt(right));
				case "int / int": CheckZero(right); return MakeInt(tk, GetInt(left) / GetInt(right));
				case "int ** int":
					rightInt = GetInt(right);
					leftInt = GetInt(left);
					if (rightInt == 0) return MakeInt(tk, 1);
					if (rightInt > 0) return MakeInt(tk, (int)Math.Pow(leftInt, rightInt));
					return MakeFloat(tk, Math.Pow(leftInt, rightInt));
				case "int % int": CheckZero(right); return MakeInt(tk, GetInt(left) % GetInt(right));
				case "int & int": return MakeInt(tk, GetInt(left) & GetInt(right));
				case "int | int": return MakeInt(tk, GetInt(left) | GetInt(right));
				case "int ^ int": return MakeInt(tk, GetInt(left) ^ GetInt(right));
				case "int << int": return MakeInt(tk, GetInt(left) << GetInt(right));
				case "int >> int": return MakeInt(tk, GetInt(left) >> GetInt(right));
				case "int <= int": return MakeBool(tk, GetInt(left) <= GetInt(right));
				case "int >= int": return MakeBool(tk, GetInt(left) >= GetInt(right));
				case "int < int": return MakeBool(tk, GetInt(left) < GetInt(right));
				case "int > int": return MakeBool(tk, GetInt(left) > GetInt(right));
				case "int == int": return MakeBool(tk, GetInt(left) == GetInt(right));
				case "int != int": return MakeBool(tk, GetInt(left) != GetInt(right));

				case "int + float": return MakeFloat(tk, GetInt(left) + GetFloat(right));
				case "int - float": return MakeFloat(tk, GetInt(left) - GetFloat(right));
				case "int * float": return MakeFloat(tk, GetInt(left) * GetFloat(right));
				case "int / float": CheckZero(right); return MakeFloat(tk, GetInt(left) / GetFloat(right));
				case "int ** float":
					rightFloat = GetFloat(right);
					leftInt = GetInt(left);
					if (rightFloat == 0) return MakeFloat(tk, 1);
					return MakeFloat(tk, Math.Pow(leftInt, rightFloat));
				case "int % float": CheckZero(right); return MakeFloat(tk, GetInt(left) % GetFloat(right));
				case "int <= float": return MakeBool(tk, GetInt(left) <= GetFloat(right));
				case "int >= float": return MakeBool(tk, GetInt(left) >= GetFloat(right));
				case "int < float": return MakeBool(tk, GetInt(left) < GetFloat(right));
				case "int > float": return MakeBool(tk, GetInt(left) > GetFloat(right));
				case "int == float": return MakeBool(tk, GetInt(left) == GetFloat(right));
				case "int != float": return MakeBool(tk, GetInt(left) != GetFloat(right));

				case "float + int": return MakeFloat(tk, GetFloat(left) + GetInt(right));
				case "float - int": return MakeFloat(tk, GetFloat(left) - GetInt(right));
				case "float * int": return MakeFloat(tk, GetFloat(left) * GetInt(right));
				case "float / int": CheckZero(right); return MakeFloat(tk, GetFloat(left) / GetInt(right));
				case "float ** int":
					rightInt = GetInt(right);
					leftFloat = GetFloat(left);
					if (rightInt == 0) return MakeFloat(tk, 1);
					return MakeFloat(tk, Math.Pow(leftFloat, rightInt));
				case "float % int": CheckZero(right); return MakeFloat(tk, GetFloat(left) % GetInt(right));
				case "float <= int": return MakeBool(tk, GetFloat(left) <= GetInt(right));
				case "float >= int": return MakeBool(tk, GetFloat(left) >= GetInt(right));
				case "float < int": return MakeBool(tk, GetFloat(left) < GetInt(right));
				case "float > int": return MakeBool(tk, GetFloat(left) > GetInt(right));
				case "float == int": return MakeBool(tk, GetFloat(left) == GetInt(right));
				case "float != int": return MakeBool(tk, GetFloat(left) != GetInt(right));

				case "float + float": return MakeFloat(tk, GetFloat(left) + GetFloat(right));
				case "float - float": return MakeFloat(tk, GetFloat(left) - GetFloat(right));
				case "float * float": return MakeFloat(tk, GetFloat(left) * GetFloat(right));
				case "float / float": CheckZero(right); return MakeFloat(tk, GetFloat(left) / GetFloat(right));
				case "float ** float":
					rightFloat = GetFloat(right);
					leftFloat = GetFloat(left);
					if (rightFloat == 0) return MakeFloat(tk, 1);
					return MakeFloat(tk, Math.Pow(leftFloat, rightFloat));
				case "float % float": CheckZero(right); return MakeFloat(tk, GetFloat(left) % GetFloat(right));
				case "float <= float": return MakeBool(tk, GetFloat(left) <= GetFloat(right));
				case "float >= float": return MakeBool(tk, GetFloat(left) >= GetFloat(right));
				case "float < float": return MakeBool(tk, GetFloat(left) < GetFloat(right));
				case "float > float": return MakeBool(tk, GetFloat(left) > GetFloat(right));
				case "float == float": return MakeBool(tk, GetFloat(left) == GetFloat(right));
				case "float != float": return MakeBool(tk, GetFloat(left) != GetFloat(right));

				case "bool + string": return MakeString(tk, GetBool(left).ToString() + GetString(right));
				case "int + string": return MakeString(tk, GetInt(left).ToString() + GetString(right));
				case "float + string": return MakeString(tk, GetFloat(left).ToString() + GetString(right));

				case "string + bool": return MakeString(tk, GetString(left) + GetBool(right).ToString());
				case "string + int": return MakeString(tk, GetString(left) + GetInt(right).ToString());
				case "string + float": return MakeString(tk, GetString(left) + GetFloat(right).ToString());

				case "string + string": return MakeString(tk, GetString(left) + GetString(right));

				case "string * int":
				case "int * string":
					string stringValue = (leftType == "string") ? GetString(left) : GetString(right);
					int intValue = (leftType == "string") ? GetInt(right) : GetInt(left);

					// don't consolidate this operation if it's going to use a bunch of memory.
					if (intValue * stringValue.Length <= 100)
					{
						string output = "";
						while (intValue > 0)
						{
							output += stringValue;
							--intValue;
						}
						return MakeString(tk, output);
					}

					return this;

				default:
					throw new ParserException(opToken, "This operator is invalid for types: " + leftType + ", " + rightType + ".");
			}
		}

		private void CheckZero(Expression expr)
		{
			bool isZero = false;
			if (expr is IntegerConstant)
			{
				isZero = ((IntegerConstant)expr).Value == 0;
			}
			else
			{
				isZero = ((FloatConstant)expr).Value == 0;
			}
			if (isZero)
			{
				throw new ParserException(expr.FirstToken, "Division by 0 error.");
			}
		}

		private Expression MakeFloat(Token firstToken, double value)
		{
			return new FloatConstant(firstToken, value);
		}

		private Expression MakeInt(Token firstToken, int value)
		{
			return new IntegerConstant(firstToken, value);
		}

		private Expression MakeString(Token firstToken, string value)
		{
			return new StringConstant(firstToken, value);
		}

		private bool GetBool(Expression expr)
		{
			return ((BooleanConstant)expr).Value;
		}

		private int GetInt(Expression expr)
		{
			return ((IntegerConstant)expr).Value;
		}

		private double GetFloat(Expression expr)
		{
			return ((FloatConstant)expr).Value;
		}

		private string GetString(Expression expr)
		{
			return ((StringConstant)expr).Value;
		}

		private Expression MakeBool(Token firstToken, bool value)
		{
			return new BooleanConstant(firstToken, value);
		}
	}
}
