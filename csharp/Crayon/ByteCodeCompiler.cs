using System;
using System.Collections.Generic;
using System.Linq;
using Crayon.ParseTree;

namespace Crayon
{
	internal class ByteCodeCompiler
	{
		public int[][] ByteCode { get; private set; }
		public Token[] TokenData { get; private set; }

		public void GenerateByteCode(Parser parser, IList<Executable> lines)
		{
			ByteBuffer buffer = new ByteBuffer();

			this.Compile(parser, buffer, lines);

			buffer.Add(null, OpCode.RETURN_NULL);

			this.ByteCode = buffer.ToIntList().ToArray();
			this.TokenData = buffer.ToTokenList().ToArray();
		}

		public void Compile(Parser parser, ByteBuffer buffer, IList<Executable> lines)
		{
			foreach (Executable line in lines)
			{
				this.Compile(parser, buffer, line);
			}
		}

		public void Compile(Parser parser, ByteBuffer buffer, Executable line)
		{
			if (line is ExpressionAsExecutable) this.CompileExpressionAsExecutable(parser, buffer, (ExpressionAsExecutable)line);
			else if (line is FunctionDefinition) this.CompileFunctionDefinition(parser, buffer, (FunctionDefinition)line);
			else if (line is Assignment) this.CompileAssignment(parser, buffer, (Assignment)line);
			else if (line is WhileLoop) this.CompileWhileLoop(parser, buffer, (WhileLoop)line);
			else if (line is BreakStatement) this.CompileBreakStatement(parser, buffer, (BreakStatement)line);
			else if (line is ContinueStatement) this.CompileContinueStatement(parser, buffer, (ContinueStatement)line);
			else if (line is ForLoop) this.CompileForLoop(parser, buffer, (ForLoop)line);
			else if (line is IfStatement) this.CompileIfStatement(parser, buffer, (IfStatement)line);
			else if (line is ReturnStatement) this.CompileReturnStatement(parser, buffer, (ReturnStatement)line);
			else throw new NotImplementedException("Invalid target for byte code compilation");
		}

		private void CompileReturnStatement(Parser parser, ByteBuffer buffer, ReturnStatement returnStatement)
		{
			if (returnStatement.Expression == null || returnStatement.Expression is NullConstant)
			{
				buffer.Add(returnStatement.FirstToken, OpCode.RETURN_NULL);
			}
			else
			{
				this.CompileExpression(parser, buffer, returnStatement.Expression, true);
				buffer.Add(returnStatement.FirstToken, OpCode.RETURN);
			}
		}

		private void CompileIfStatement(Parser parser, ByteBuffer buffer, IfStatement ifStatement)
		{
			this.CompileExpression(parser, buffer, ifStatement.Condition, true);
			ByteBuffer trueCode = new ByteBuffer();
			this.Compile(parser, trueCode, ifStatement.TrueCode);
			ByteBuffer falseCode = new ByteBuffer();
			this.Compile(parser, falseCode, ifStatement.FalseCode);

			if (falseCode.Size == 0)
			{
				if (trueCode.Size == 0) buffer.Add(ifStatement.Condition.FirstToken, OpCode.POP);
				else
				{
					buffer.Add(ifStatement.Condition.FirstToken, OpCode.JUMP_IF_FALSE, trueCode.Size);
					buffer.Concat(trueCode);
				}
			}
			else
			{
				trueCode.Add(null, OpCode.JUMP, falseCode.Size);
				buffer.Add(ifStatement.Condition.FirstToken, OpCode.JUMP_IF_FALSE, trueCode.Size);
				buffer.Concat(trueCode);
				buffer.Concat(falseCode);
			}
		}

		private void CompileBreakStatement(Parser parser, ByteBuffer buffer, BreakStatement breakStatement)
		{
			buffer.Add(breakStatement.FirstToken, OpCode.BREAK);
		}

		private void CompileContinueStatement(Parser parser, ByteBuffer buffer, ContinueStatement continueStatement)
		{
			buffer.Add(continueStatement.FirstToken, OpCode.CONTINUE);
		}

		private void CompileForLoop(Parser parser, ByteBuffer buffer, ForLoop forLoop)
		{
			this.Compile(parser, buffer, forLoop.Init);

			ByteBuffer codeBuffer = new ByteBuffer();
			this.Compile(parser, codeBuffer, forLoop.Code);
			this.Compile(parser, codeBuffer, forLoop.Step);

			ByteBuffer forBuffer = new ByteBuffer();
			this.CompileExpression(parser, forBuffer, forLoop.Condition, true);
			forBuffer.Add(forLoop.Condition.FirstToken, OpCode.JUMP_IF_FALSE, codeBuffer.Size + 1); // +1 to go past the jump I'm about to add.

			forBuffer.Concat(codeBuffer);
			forBuffer.Add(null, OpCode.JUMP, -forBuffer.Size - 1);

			forBuffer.ResolveBreaks();
			forBuffer.ResolveContinues();

			buffer.Concat(forBuffer);
		}

		private void CompileWhileLoop(Parser parser, ByteBuffer buffer, WhileLoop whileLoop)
		{
			ByteBuffer loopBody = new ByteBuffer();
			this.Compile(parser, loopBody, whileLoop.Code);
			ByteBuffer condition = new ByteBuffer();
			this.CompileExpression(parser, condition, whileLoop.Condition, true);

			condition.Add(whileLoop.Condition.FirstToken, OpCode.JUMP_IF_FALSE, loopBody.Size + 1);
			condition.Concat(loopBody);
			condition.Add(null, OpCode.JUMP, -condition.Size - 1);

			condition.ResolveBreaks();
			condition.ResolveContinues();

			buffer.Concat(condition);
		}

		private void CompileAssignment(Parser parser, ByteBuffer buffer, Assignment assignment)
		{
			if (assignment.AssignmentOp == "=")
			{
				if (assignment.Target is Variable)
				{
					Variable varTarget = (Variable)assignment.Target;
					this.CompileExpression(parser, buffer, assignment.Value, true);
					buffer.Add(assignment.AssignmentOpToken, OpCode.ASSIGN_VAR, parser.GetId(varTarget.Name));
				}
				else if (assignment.Target is BracketIndex)
				{
					BracketIndex bi = (BracketIndex)assignment.Target;
					// TODO: optimization opportunity: special op code when index is a string or int constant.

					this.CompileExpression(parser, buffer, bi.Root, true);
					this.CompileExpression(parser, buffer, bi.Index, true);
					this.CompileExpression(parser, buffer, assignment.Value, true);
					buffer.Add(assignment.AssignmentOpToken, OpCode.ASSIGN_INDEX);
				}
				else if (assignment.Target is DotStep)
				{
					throw new NotImplementedException();
				}
				else
				{
					throw new Exception("This shouldn't happen.");
				}
			}
			else
			{
				if (assignment.Target is Variable)
				{
					Variable varTarget = (Variable)assignment.Target;
					buffer.Add(varTarget.FirstToken, OpCode.VARIABLE, parser.GetId(varTarget.Name));
					this.CompileExpression(parser, buffer, assignment.Value, true);
					BinaryOps op;
					switch (assignment.AssignmentOp)
					{
						case "+=": op = BinaryOps.ADDITION; break;
						case "-=": op = BinaryOps.SUBTRACTION; break;
						case "*=": op = BinaryOps.MULTIPLICATION; break;
						default: throw new NotImplementedException("Need to do the rest of these.");
					}
					buffer.Add(assignment.AssignmentOpToken, OpCode.BINARY_OP, (int)op);
					buffer.Add(assignment.Target.FirstToken, OpCode.ASSIGN_VAR, parser.GetId(varTarget.Name));
				}
				else if (assignment.Target is DotStep)
				{
					throw new NotImplementedException();
				}
				else if (assignment.Target is BracketIndex)
				{
					throw new NotImplementedException();
				}
				else
				{
					throw new ParserException(assignment.AssignmentOpToken, "Assignment is not allowed on this sort of expression.");
				}
			}
		}

		private void CompileFunctionDefinition(Parser parser, ByteBuffer buffer, FunctionDefinition funDef)
		{
			// TODO: make this aware if it is a method or function

			ByteBuffer tBuffer = new ByteBuffer();

			for (int i = 0; i < funDef.ArgNames.Length; ++i)
			{
				string argName = funDef.ArgNames[i].Value;
				Expression defaultValue = funDef.DefaultValues[i];
				if (defaultValue == null)
				{
					tBuffer.Add(funDef.NameToken, OpCode.ASSIGN_FUNCTION_ARG, parser.GetId(argName), i);
				}
				else
				{
					ByteBuffer defaultValueCode = new ByteBuffer();

					this.CompileExpression(parser, defaultValueCode, defaultValue, true);

					tBuffer.Add(defaultValue.FirstToken, OpCode.ASSIGN_FUNCTION_ARG_AND_JUMP, parser.GetId(argName), i, defaultValueCode.Size);
					tBuffer.Concat(defaultValueCode);
				}
			}

			Compile(parser, tBuffer, funDef.Code);

			int offset = tBuffer.Size;

			buffer.Add(
				funDef.FirstToken,
				OpCode.FUNCTION_DEFINITION,
				parser.GetId(funDef.NameToken.Value), // local var to save in
				2, // offset from current PC where code is located
				funDef.ArgNames.Length); // max number of args supplied

			buffer.Add(funDef.FirstToken, OpCode.ASSIGN_VAR, parser.GetId(funDef.NameToken.Value));
			buffer.Add(null, OpCode.JUMP, offset);

			buffer.Concat(tBuffer);

		}

		private void CompileExpressionAsExecutable(Parser parser, ByteBuffer buffer, ExpressionAsExecutable expr)
		{
			this.CompileExpression(parser, buffer, expr.Expression, false);
		}

		private void CompileExpression(Parser parser, ByteBuffer buffer, Expression expr, bool outputUsed)
		{
			if (expr is FunctionCall) this.CompileFunctionCall(parser, buffer, (FunctionCall)expr, outputUsed);
			else if (expr is IntegerConstant) this.CompileIntegerConstant(parser, buffer, (IntegerConstant)expr, outputUsed);
			else if (expr is Variable) this.CompileVariable(parser, buffer, (Variable)expr, outputUsed);
			else if (expr is BooleanConstant) this.CompileBooleanConstant(parser, buffer, (BooleanConstant)expr, outputUsed);
			else if (expr is DotStep) this.CompileDotStep(parser, buffer, (DotStep)expr, outputUsed);
			else if (expr is BracketIndex) this.CompileBracketIndex(parser, buffer, (BracketIndex)expr, outputUsed);
			else if (expr is BinaryOpChain) this.CompileBinaryOpChain(parser, buffer, (BinaryOpChain)expr, outputUsed);
			else if (expr is StringConstant) this.CompileStringConstant(parser, buffer, (StringConstant)expr, outputUsed);
			else if (expr is NegativeSign) this.CompileNegativeSign(parser, buffer, (NegativeSign)expr, outputUsed);
			else if (expr is SystemFunctionCall) this.CompileSystemFunctionCall(parser, buffer, (SystemFunctionCall)expr, outputUsed);
			else if (expr is ListDefinition) this.CompileListDefinition(parser, buffer, (ListDefinition)expr, outputUsed);
			else if (expr is Increment) this.CompileIncrement(parser, buffer, (Increment)expr, outputUsed);
			else if (expr is FloatConstant) this.CompileFloatConstant(parser, buffer, (FloatConstant)expr, outputUsed);
			else if (expr is NullConstant) this.CompileNullConstant(parser, buffer, (NullConstant)expr, outputUsed);
			else throw new NotImplementedException();
		}

		private void CompileNullConstant(Parser parser, ByteBuffer buffer, NullConstant nullConstant, bool outputUsed)
		{
			if (!outputUsed) throw new ParserException(nullConstant.FirstToken, "This expression doesn't do anything.");

			buffer.Add(nullConstant.FirstToken, OpCode.LITERAL, (int)Types.NULL, 0);
		}

		private void CompileFloatConstant(Parser parser, ByteBuffer buffer, FloatConstant floatConstant, bool outputUsed)
		{
			if (!outputUsed) throw new ParserException(floatConstant.FirstToken, "This expression doesn't do anything.");
			int floatId = parser.GetFloatConstant(floatConstant.Value);
			buffer.Add(floatConstant.FirstToken, OpCode.LITERAL, (int)Types.FLOAT, floatId);
		}

		private void CompileIncrement(Parser parser, ByteBuffer buffer, Increment increment, bool outputUsed)
		{
			if (!outputUsed)
			{
				throw new Exception("This should have been optimized into a += or -=");
			}

			// TODO: once I optimize the addition of inline integers, use that op instead of the two ops
			if (increment.Root is Variable)
			{
				Variable variable = (Variable)increment.Root;
				this.CompileExpression(parser, buffer, increment.Root, true);
				if (increment.IsPrefix)
				{
					buffer.Add(increment.IncrementToken, OpCode.LITERAL, (int)Types.INTEGER, 1);
					buffer.Add(increment.IncrementToken, OpCode.BINARY_OP, increment.IsIncrement ? (int)BinaryOps.ADDITION : (int)BinaryOps.SUBTRACTION);
					buffer.Add(increment.IncrementToken, OpCode.DUPLICATE_STACK_TOP);
					buffer.Add(variable.FirstToken, OpCode.ASSIGN_VAR, parser.GetId(variable.Name));
				}
				else
				{
					buffer.Add(increment.IncrementToken, OpCode.DUPLICATE_STACK_TOP);
					buffer.Add(increment.IncrementToken, OpCode.LITERAL, (int)Types.INTEGER, 1);
					buffer.Add(increment.IncrementToken, OpCode.BINARY_OP, increment.IsIncrement ? (int)BinaryOps.ADDITION : (int)BinaryOps.SUBTRACTION);
					buffer.Add(variable.FirstToken, OpCode.ASSIGN_VAR, parser.GetId(variable.Name));
				}
			}
			else if (increment.Root is BracketIndex)
			{
				throw new NotImplementedException("++foo[key] and foo[key]++ are not implemented yet.");
			}
			else if (increment.Root is DotStep)
			{
				throw new NotImplementedException("++foo.field and foo.field++ are not implemented yet.");
			}
			else
			{
				throw new ParserException(increment.IncrementToken, "Cannot apply " + (increment.IsIncrement ? "++" : "--") + " to this sort of expression.");
			}
		}

		private void CompileListDefinition(Parser parser, ByteBuffer buffer, ListDefinition listDef, bool outputUsed)
		{
			if (!outputUsed) throw new ParserException(listDef.FirstToken, "List allocation made without storing it. This is likely a mistake.");
			foreach (Expression item in listDef.Items)
			{
				this.CompileExpression(parser, buffer, item, true);
			}
			buffer.Add(listDef.FirstToken, OpCode.DEF_LIST, listDef.Items.Length);
		}

		private void CompileSystemFunctionCall(Parser parser, ByteBuffer buffer, SystemFunctionCall sysFunc, bool outputUsed)
		{
			this.CompileExpressionList(parser, buffer, sysFunc.Args, true);

			string frameworkFunctionName = sysFunc.Name.Substring(1);
			FrameworkFunction ff = parser.GetFrameworkFunction(sysFunc.FirstToken, frameworkFunctionName);
			buffer.Add(sysFunc.FirstToken, OpCode.CALL_FRAMEWORK_FUNCTION, (int)ff, outputUsed ? 1 : 0);

		}

		private void CompileNegativeSign(Parser parser, ByteBuffer buffer, NegativeSign negativeSign, bool outputUsed)
		{
			if (!outputUsed) throw new ParserException(negativeSign.FirstToken, "This expression does nothing.");
			this.CompileExpression(parser, buffer, negativeSign.Root, true);
			buffer.Add(negativeSign.FirstToken, OpCode.NEGATIVE_SIGN);
		}

		private void CompileStringConstant(Parser parser, ByteBuffer buffer, StringConstant stringConstant, bool outputUsed)
		{
			if (!outputUsed) throw new ParserException(stringConstant.FirstToken, "This expression does nothing.");
			int stringId = parser.GetStringConstant(stringConstant.Value);
			buffer.Add(stringConstant.FirstToken, OpCode.LITERAL, (int)Types.STRING, stringId);
		}

		private void CompileBinaryOpChain(Parser parser, ByteBuffer buffer, BinaryOpChain opChain, bool outputUsed)
		{
			if (!outputUsed) throw new ParserException(opChain.FirstToken, "This expression isn't valid here.");

			this.CompileExpressionList(parser, buffer, new Expression[] { opChain.Expressions[0], opChain.Expressions[1] }, true);

			bool first = true;

			for (int i = 0; i < opChain.Ops.Length; ++i)
			{
				if (!first)
				{
					this.CompileExpression(parser, buffer, opChain.Expressions[i + 1], true);
				}
				else
				{
					first = false;
				}

				Token opToken = opChain.Ops[i];
				switch (opToken.Value)
				{
					case "+": buffer.Add(opToken, OpCode.BINARY_OP, (int)BinaryOps.ADDITION); break;
					case "<": buffer.Add(opToken, OpCode.BINARY_OP, (int)BinaryOps.LESS_THAN); break;
					case "==": buffer.Add(opToken, OpCode.BINARY_OP, (int)BinaryOps.EQUALS); break;
					case "<=": buffer.Add(opToken, OpCode.BINARY_OP, (int)BinaryOps.LESS_THAN_OR_EQUAL); break;
					case ">": buffer.Add(opToken, OpCode.BINARY_OP, (int)BinaryOps.GREATER_THAN); break;
					case ">=": buffer.Add(opToken, OpCode.BINARY_OP, (int)BinaryOps.GREATER_THAN_OR_EQUAL); break;
					case "-": buffer.Add(opToken, OpCode.BINARY_OP, (int)BinaryOps.SUBTRACTION); break;
					case "*": buffer.Add(opToken, OpCode.BINARY_OP, (int)BinaryOps.MULTIPLICATION); break;
					case "/": buffer.Add(opToken, OpCode.BINARY_OP, (int)BinaryOps.DIVISION); break;
					case "!=": buffer.Add(opToken, OpCode.BINARY_OP, (int)BinaryOps.NOT_EQUALS); break;
					default: throw new NotImplementedException("Binary op: " + opChain.Ops[i].Value);
				}
			}
			if (!outputUsed)
			{
				buffer.Add(null, OpCode.POP);
			}
		}

		private void CompileBracketIndex(Parser parser, ByteBuffer buffer, BracketIndex bracketIndex, bool outputUsed)
		{
			// TODO: do a bunch of optimizations here:
			// bracket index of integer, bracket index of string, bracket index of simple variable name, then bracket index of complex expression
			if (!outputUsed) throw new ParserException(bracketIndex.FirstToken, "This expression does nothing.");
			this.CompileExpression(parser, buffer, bracketIndex.Root, true);
			this.CompileExpression(parser, buffer, bracketIndex.Index, true);
			buffer.Add(bracketIndex.BracketToken, OpCode.INDEX);
		}

		private void CompileDotStep(Parser parser, ByteBuffer buffer, DotStep dotStep, bool outputUsed)
		{
			if (!outputUsed) throw new ParserException(dotStep.FirstToken, "This expression does nothing.");
			this.CompileExpression(parser, buffer, dotStep.Root, true);
			buffer.Add(dotStep.DotToken, OpCode.DEREF_DOT, parser.GetId(dotStep.StepToken.Value));
		}

		private void CompileBooleanConstant(Parser parser, ByteBuffer buffer, BooleanConstant boolConstant, bool outputUsed)
		{
			if (!outputUsed) throw new ParserException(boolConstant.FirstToken, "This expression does nothing.");
			buffer.Add(boolConstant.FirstToken, OpCode.LITERAL, (int)Types.BOOLEAN, boolConstant.Value ? 1 : 0);
		}

		private void CompileVariable(Parser parser, ByteBuffer buffer, Variable variable, bool outputUsed)
		{
			if (!outputUsed) throw new ParserException(variable.FirstToken, "This expression does nothing.");
			buffer.Add(variable.FirstToken, OpCode.VARIABLE, parser.GetId(variable.Name));
		}

		private void CompileIntegerConstant(Parser parser, ByteBuffer buffer, IntegerConstant intConst, bool outputUsed)
		{
			if (!outputUsed) throw new ParserException(intConst.FirstToken, "This expression does nothing.");
			buffer.Add(intConst.FirstToken, OpCode.LITERAL, (int)Types.INTEGER, intConst.Value);
		}

		private void CompileVariableStream(Parser parser, ByteBuffer buffer, IList<Expression> variables, bool outputUsed)
		{
			if (variables.Count == 1)
			{
				this.CompileExpression(parser, buffer, variables[0], outputUsed);
			}
			else
			{
				List<int> ids = new List<int>();
				List<int> tokenData = new List<int>();
				foreach (Variable v in variables.Cast<Variable>())
				{
					ids.Add(parser.GetId(v.Name));
					tokenData.Add(v.FirstToken.Line);
					tokenData.Add(v.FirstToken.Col);
					tokenData.Add(v.FirstToken.FileID);
				}

				ids.Add(-1);
				ids.AddRange(tokenData);

				buffer.Add(variables[0].FirstToken, OpCode.VARIABLE_STREAM, ids.ToArray());
			}
		}

		private void CompileLiteralStream(Parser parser, ByteBuffer buffer, IList<Expression> expressions, bool outputUsed)
		{
			if (expressions.Count == 1)
			{
				this.CompileExpression(parser, buffer, expressions[0], outputUsed);
			}
			else
			{
				ByteBuffer exprBuffer = new ByteBuffer();
				foreach (Expression expression in expressions)
				{
					this.CompileExpression(parser, exprBuffer, expression, outputUsed);
				}

				List<int> byteCodeArgs = new List<int>();
				foreach (int[] pairs in exprBuffer.ToIntList())
				{
					byteCodeArgs.Add(pairs[1]);
					byteCodeArgs.Add(pairs[2]);
				}

				buffer.Add(expressions[0].FirstToken, OpCode.LITERAL_STREAM, byteCodeArgs.ToArray());
			}
		}

		private const int EXPR_STREAM_OTHER = 1;
		private const int EXPR_STREAM_LITERAL = 2;
		private const int EXPR_STREAM_VARIABLE = 3;

		public void CompileExpressionList(Parser parser, ByteBuffer buffer, IList<Expression> expressions, bool outputUsed)
		{
			if (expressions.Count == 0) return;
			if (expressions.Count == 1)
			{
				this.CompileExpression(parser, buffer, expressions[0], outputUsed);
				return;
			}

			List<Expression> literals = new List<Expression>();
			List<Expression> variables = new List<Expression>();
			int mode = EXPR_STREAM_OTHER;

			for (int i = 0; i < expressions.Count; ++i)
			{
				Expression expr = expressions[i];
				bool modeChange = false;
				if (expr.IsLiteral)
				{
					if (mode == EXPR_STREAM_LITERAL)
					{
						literals.Add(expr);
					}
					else
					{
						mode = EXPR_STREAM_LITERAL;
						modeChange = true;
						--i;
					}
				}
				else if (expr is Variable)
				{
					if (mode == EXPR_STREAM_VARIABLE)
					{
						variables.Add(expr);
					}
					else
					{
						mode = EXPR_STREAM_VARIABLE;
						modeChange = true;
						--i;
					}
				}
				else
				{
					if (mode == EXPR_STREAM_OTHER)
					{
						this.CompileExpression(parser, buffer, expr, true);
					}
					else
					{
						mode = EXPR_STREAM_OTHER;
						modeChange = true;
						--i;
					}
				}

				if (modeChange)
				{
					if (literals.Count > 0)
					{
						this.CompileLiteralStream(parser, buffer, literals, true);
						literals.Clear();
					}
					else if (variables.Count > 0)
					{
						this.CompileVariableStream(parser, buffer, variables, true);
						variables.Clear();
					}
				}
			}

			if (literals.Count > 0)
			{
				this.CompileLiteralStream(parser, buffer, literals, true);
				literals.Clear();
			}
			else if (variables.Count > 0)
			{
				this.CompileVariableStream(parser, buffer, variables, true);
				variables.Clear();
			}
		}

		private void CompileFunctionCall(Parser parser, ByteBuffer buffer, FunctionCall funCall, bool outputUsed)
		{
			this.CompileExpressionList(parser, buffer, funCall.Args, true);

			Expression root = funCall.Root;
			Variable rootVar = root as Variable;
			if (rootVar != null)
			{
				string functionName = rootVar.Name;
				buffer.Add(funCall.ParenToken, OpCode.CALL_FUNCTION_ON_VARIABLE, parser.GetId(functionName), funCall.Args.Length, outputUsed ? 1 : 0);
			}
			else
			{
				this.CompileExpression(parser, buffer, funCall.Root, true);
				buffer.Add(funCall.ParenToken, OpCode.CALL_FUNCTION, funCall.Args.Length, outputUsed ? 1 : 0);
			}
		}
	}
}
