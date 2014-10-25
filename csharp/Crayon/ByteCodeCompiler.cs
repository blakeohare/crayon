using System;
using System.Collections.Generic;
using System.Linq;
using Crayon.ParseTree;

namespace Crayon
{
	internal class ByteCodeCompiler
	{
		public ByteBuffer GenerateByteCode(Parser parser, IList<Executable> lines)
		{
			ByteBuffer userCode = new ByteBuffer();

			this.Compile(parser, userCode, lines);
			userCode.Add(null, OpCode.RETURN_NULL);

			ByteBuffer literalsTable = parser.LiteralLookup.BuildByteCode();

			ByteBuffer tokenData = this.BuildTokenData(userCode);

			ByteBuffer fileContent = this.BuildFileContent(parser.GetFilesById());

			ByteBuffer switchStatements = this.BuildSwitchStatementTables(parser);

			ByteBuffer header = new Crayon.ByteBuffer();
			header.Concat(literalsTable);
			header.Concat(tokenData);
			header.Concat(fileContent);
			header.Concat(switchStatements);
			header.Add(null, OpCode.FINALIZE_INITIALIZATION);

			ByteBuffer output = new Crayon.ByteBuffer();
			output.Add(null, OpCode.USER_CODE_START, header.Size + 1);
			output.Concat(header);
			output.Concat(userCode);

			return output;
		}

		private ByteBuffer BuildSwitchStatementTables(Parser parser)
		{
			ByteBuffer output = new Crayon.ByteBuffer();
			List<Dictionary<int, int>> intSwitches = parser.GetIntegerSwitchStatements();
			for (int i = 0; i < intSwitches.Count; ++i)
			{
				List<int> args = new List<int>();
				Dictionary<int, int> lookup = intSwitches[i];
				foreach (int key in lookup.Keys) {
					int offset = lookup[key];
					args.Add(key);
					args.Add(offset);
				}

				output.Add(null, OpCode.BUILD_SWITCH_INT, args.ToArray());
			}

			List<Dictionary<string, int>> stringSwitches = parser.GetStringSwitchStatements();
			for (int i = 0; i < stringSwitches.Count; ++i)
			{
				Dictionary<string, int> lookup = stringSwitches[i];
				foreach (string key in lookup.Keys)
				{
					int offset = lookup[key];
					output.Add(null, OpCode.BUILD_SWITCH_STRING, key, i, offset);
				}
			}
			return output;
		}

		private ByteBuffer BuildFileContent(string[] filesById)
		{
			ByteBuffer output = new ByteBuffer();

			for (int i = 0; i < filesById.Length; ++i)
			{
				output.Add(null, OpCode.DEF_ORIGINAL_CODE, filesById[i], i);
			}

			return output;
		}

		private ByteBuffer BuildTokenData(ByteBuffer userCode)
		{
			Token[] tokens = userCode.ToTokenList().ToArray();
			int[][] rows = userCode.ToIntList().ToArray();

			int size = tokens.Length;
			ByteBuffer output = new ByteBuffer();
			// TODO: add command line flag for excluding token data. In that case, just return here.

			Token token;
			int[] row;
			for (int i = 0; i < size; ++i)
			{
				token = tokens[i];
				row = rows[i];

				if (row[0] == (int)OpCode.VARIABLE_STREAM)
				{
					bool toggled = false;
					for (int j = 1; j < row.Length; ++j)
					{
						if (toggled)
						{
							int line = row[j];
							int col = row[j + 1];
							int file = row[j + 2];

							output.Add(null, OpCode.TOKEN_DATA, i, line, col, file);

							j += 2;
						}
						else if (row[j] == -1)
						{
							toggled = true;
						}
					}
				}
				else if (token != null)
				{
					output.Add(null, OpCode.TOKEN_DATA, i, token.Line, token.Col, token.FileID);
				}
			}

			return output;
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
			else if (line is FunctionDefinition) this.CompileFunctionDefinition(parser, buffer, (FunctionDefinition)line, false);
			else if (line is Assignment) this.CompileAssignment(parser, buffer, (Assignment)line);
			else if (line is WhileLoop) this.CompileWhileLoop(parser, buffer, (WhileLoop)line);
			else if (line is BreakStatement) this.CompileBreakStatement(parser, buffer, (BreakStatement)line);
			else if (line is ContinueStatement) this.CompileContinueStatement(parser, buffer, (ContinueStatement)line);
			else if (line is ForLoop) this.CompileForLoop(parser, buffer, (ForLoop)line);
			else if (line is IfStatement) this.CompileIfStatement(parser, buffer, (IfStatement)line);
			else if (line is ReturnStatement) this.CompileReturnStatement(parser, buffer, (ReturnStatement)line);
			else if (line is ClassDefinition) this.CompileClass(parser, buffer, (ClassDefinition)line);
			else if (line is SwitchStatement) this.CompileSwitchStatement(parser, buffer, (SwitchStatement)line);
			else if (line is ForEachLoop) this.CompileForEachLoop(parser, buffer, (ForEachLoop)line);
			else throw new NotImplementedException("Invalid target for byte code compilation");
		}

		private void CompileForEachLoop(Parser parser, ByteBuffer buffer, ForEachLoop forEachLoop)
		{
			buffer.Add(null, OpCode.LITERAL, parser.GetIntConstant(0));
			buffer.Add(null, OpCode.LITERAL, parser.GetIntConstant(parser.GetId(forEachLoop.IterationVariable.Value)));
			this.CompileExpression(parser, buffer, forEachLoop.IterationExpression, true);
			buffer.Add(forEachLoop.IterationExpression.FirstToken, OpCode.VERIFY_IS_LIST);

			ByteBuffer body = new ByteBuffer();
			ByteBuffer body2 = new Crayon.ByteBuffer();

			this.Compile(parser, body2, forEachLoop.Body);

			body.Add(forEachLoop.FirstToken, OpCode.ITERATION_STEP, body2.Size + 1);
			
			body2.Add(null, OpCode.JUMP, -body2.Size - 2);
			body.Concat(body2);

			body.ResolveBreaks();
			body.ResolveContinues();

			buffer.Concat(body);
			buffer.Add(null, OpCode.POP); // list
			buffer.Add(null, OpCode.POP); // var ID
			buffer.Add(null, OpCode.POP); // index
		}

		private void CompileSwitchStatement(Parser parser, ByteBuffer buffer, SwitchStatement switchStatement)
		{
			this.CompileExpression(parser, buffer, switchStatement.Condition, true);

			ByteBuffer chunkBuffer = new ByteBuffer();

			Dictionary<int, int> chunkIdsToOffsets = new Dictionary<int, int>();
			Dictionary<int, int> integersToChunkIds = new Dictionary<int,int>();
			Dictionary<string, int> stringsToChunkIds = new Dictionary<string,int>();

			int defaultChunkId = -1;
			foreach (SwitchStatement.Chunk chunk in switchStatement.Chunks)
			{
				int chunkId = chunk.ID;

				if (chunk.Cases.Length == 1 && chunk.Cases[0] == null)
				{
					defaultChunkId = chunkId;
				}
				else
				{
					foreach (Expression expression in chunk.Cases)
					{
						if (switchStatement.UsesIntegers)
						{
							integersToChunkIds[((IntegerConstant)expression).Value] = chunkId;
						}
						else
						{
							stringsToChunkIds[((StringConstant)expression).Value] = chunkId;
						}
					}
				}

				chunkIdsToOffsets[chunkId] = chunkBuffer.Size;

				this.Compile(parser, chunkBuffer, chunk.Code);
			}

			chunkBuffer.ResolveBreaks();

			int switchId = parser.RegisterByteCodeSwitch(chunkIdsToOffsets, integersToChunkIds, stringsToChunkIds);

			int defaultOffsetLength = defaultChunkId == -1
				? chunkBuffer.Size
				: chunkIdsToOffsets[defaultChunkId];

			buffer.Add(switchStatement.FirstToken, switchStatement.UsesIntegers ? OpCode.SWITCH_INT : OpCode.SWITCH_STRING, switchId, defaultOffsetLength);
			buffer.Concat(chunkBuffer);
		}

		private void CompileClass(Parser parser, ByteBuffer buffer, ClassDefinition classDefinition)
		{
			ByteBuffer methodBuffer = new ByteBuffer();
			bool hasConstructor = classDefinition.Constructor != null;

			if (hasConstructor)
			{
				this.CompileConstructor(parser, methodBuffer, classDefinition.Constructor);
			}

			Dictionary<int, int> methodOffsets = new Dictionary<int, int>();
			Dictionary<int, int> argCount = new Dictionary<int,int>();
			List<int> methodIds = new List<int>();
			foreach (FunctionDefinition method in classDefinition.Methods)
			{
				int methodName = parser.GetId(method.NameToken.Value);
				if (methodOffsets.ContainsKey(methodName))
				{
					throw new ParserException(method.NameToken, "A method with this name already exists in this class.");
				}
				methodIds.Add(methodName);
				methodOffsets[methodName] = methodBuffer.Size;
				argCount[methodName] = method.ArgNames.Length;

				this.CompileFunctionDefinition(parser, methodBuffer, method, true);
			}

			List<int> args = new List<int>();
			// class name ID
			args.Add(parser.GetId(classDefinition.NameToken.Value));
			
			// subclass ID or -1
			if (classDefinition.SubClasses.Length > 1) throw new NotImplementedException("Interfaces not supported yet.");
			if (classDefinition.SubClasses.Length == 0)
			{
				args.Add(-1);
			}
			else
			{
				args.Add(parser.GetId(classDefinition.SubClasses[0].Value));
			}

			// has constructor? (the byte code is always next after the jump)
			args.Add(hasConstructor ? classDefinition.Constructor.Args.Length : -1);

			// how many methods?
			args.Add(methodIds.Count);

			// each method is a set of 3 integers...
			foreach (int methodId in methodIds)
			{
				// name of the method...
				args.Add(methodId);
				// PC offset...
				args.Add(methodOffsets[methodId] + 2);
				// Maximum number of args...
				args.Add(argCount[methodId]);
			}

			buffer.Add(classDefinition.FirstToken, OpCode.CLASS_DEFINITION, args.ToArray());
			buffer.Add(null, OpCode.JUMP, methodBuffer.Size);
			buffer.Concat(methodBuffer);
		}

		private void CompileConstructor(Parser parser, ByteBuffer buffer, ConstructorDefinition constructor)
		{
			// TODO: throw parser exception in the resolver if a return appears with any value
			this.CompileFunctionArgs(parser, buffer, constructor.Args, constructor.DefaultValues);

			if (constructor.BaseToken != null)
			{
				this.CompileExpressionList(parser, buffer, constructor.BaseArgs, true);
				buffer.Add(constructor.BaseToken, OpCode.CALL_BASE_CONSTRUCTOR, constructor.BaseArgs.Length);
			}

			this.Compile(parser, buffer, constructor.Code);
			buffer.Add(null, OpCode.RETURN_NULL);
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

		private BinaryOps ConvertOpString(Token token)
		{
			switch (token.Value)
			{
				case "++": return BinaryOps.ADDITION;
				case "+=": return BinaryOps.ADDITION;
				case "--": return BinaryOps.SUBTRACTION;
				case "-=": return BinaryOps.SUBTRACTION;
				case "*=": return BinaryOps.MULTIPLICATION;
				case "/=": return BinaryOps.DIVISION;
				case "%=": return BinaryOps.MODULO;
				case "&=": return BinaryOps.BITWISE_AND;
				case "|=": return BinaryOps.BITWISE_OR;
				case "^=": return BinaryOps.BITWISE_XOR;
				case "<<=": return BinaryOps.BIT_SHIFT_LEFT;
				case ">>=": return BinaryOps.BIT_SHIFT_RIGHT;
				default: throw new ParserException(token, "Unrecognized op.");
			}
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
					DotStep dotStep = (DotStep)assignment.Target;
					if (dotStep.Root is ThisKeyword)
					{
						this.CompileExpression(parser, buffer, assignment.Value, true);
						buffer.Add(assignment.AssignmentOpToken, OpCode.ASSIGN_THIS_STEP, parser.GetId(dotStep.StepToken.Value));
					}
					else
					{
						this.CompileExpression(parser, buffer, dotStep.Root, true);
						this.CompileExpression(parser, buffer, assignment.Value, true);
						buffer.Add(assignment.AssignmentOpToken, OpCode.ASSIGN_STEP, parser.GetId(dotStep.StepToken.Value));
					}
				}
				else
				{
					throw new Exception("This shouldn't happen.");
				}
			}
			else
			{
				BinaryOps op = this.ConvertOpString(assignment.AssignmentOpToken);
				if (assignment.Target is Variable)
				{
					Variable varTarget = (Variable)assignment.Target;
					buffer.Add(varTarget.FirstToken, OpCode.VARIABLE, parser.GetId(varTarget.Name));
					this.CompileExpression(parser, buffer, assignment.Value, true);
					buffer.Add(assignment.AssignmentOpToken, OpCode.BINARY_OP, (int)op);
					buffer.Add(assignment.Target.FirstToken, OpCode.ASSIGN_VAR, parser.GetId(varTarget.Name));
				}
				else if (assignment.Target is DotStep)
				{
					DotStep dotExpr = (DotStep)assignment.Target;
					int stepId = parser.GetId(dotExpr.StepToken.Value);
					this.CompileExpression(parser, buffer, dotExpr.Root, true);
					if (!(dotExpr.Root is ThisKeyword))
					{
						buffer.Add(null, OpCode.DUPLICATE_STACK_TOP, 1);
					}
					buffer.Add(dotExpr.DotToken, OpCode.DEREF_DOT, stepId);
					this.CompileExpression(parser, buffer, assignment.Value, true);
					buffer.Add(assignment.AssignmentOpToken, OpCode.BINARY_OP, (int)op);
					if (dotExpr.Root is ThisKeyword)
					{
						buffer.Add(assignment.AssignmentOpToken, OpCode.ASSIGN_THIS_STEP, stepId);
					}
					else
					{
						buffer.Add(assignment.AssignmentOpToken, OpCode.ASSIGN_STEP, stepId);
					}
				}
				else if (assignment.Target is BracketIndex)
				{
					BracketIndex indexExpr = (BracketIndex)assignment.Target;
					this.CompileExpression(parser, buffer, indexExpr.Root, true);
					this.CompileExpression(parser, buffer, indexExpr.Index, true);
					buffer.Add(null, OpCode.DUPLICATE_STACK_TOP, 2);
					buffer.Add(indexExpr.BracketToken, OpCode.INDEX);
					this.CompileExpression(parser, buffer, assignment.Value, true);
					buffer.Add(assignment.AssignmentOpToken, OpCode.BINARY_OP, (int)op);
					buffer.Add(assignment.AssignmentOpToken, OpCode.ASSIGN_INDEX);
				}
				else
				{
					throw new ParserException(assignment.AssignmentOpToken, "Assignment is not allowed on this sort of expression.");
				}
			}
		}

		private void CompileFunctionArgs(Parser parser, ByteBuffer buffer, IList<Token> argNames, IList<Expression> argValues)
		{
			for (int i = 0; i < argNames.Count; ++i)
			{
				string argName = argNames[i].Value;
				Expression defaultValue = argValues[i];
				if (defaultValue == null)
				{
					buffer.Add(argNames[i], OpCode.ASSIGN_FUNCTION_ARG, parser.GetId(argName), i);
				}
				else
				{
					ByteBuffer defaultValueCode = new ByteBuffer();

					this.CompileExpression(parser, defaultValueCode, defaultValue, true);
					defaultValueCode.Add(argNames[i], OpCode.ASSIGN_VAR, parser.GetId(argName));

					buffer.Add(defaultValue.FirstToken, OpCode.ASSIGN_FUNCTION_ARG_AND_JUMP, parser.GetId(argName), i, defaultValueCode.Size);
					buffer.Concat(defaultValueCode);
				}
			}
		}

		private void CompileFunctionDefinition(Parser parser, ByteBuffer buffer, FunctionDefinition funDef, bool isMethod)
		{
			ByteBuffer tBuffer = new ByteBuffer();

			this.CompileFunctionArgs(parser, tBuffer, funDef.ArgNames, funDef.DefaultValues);

			Compile(parser, tBuffer, funDef.Code);

			int offset = tBuffer.Size;

			buffer.Add(
				funDef.FirstToken,
				OpCode.FUNCTION_DEFINITION,
				parser.GetId(funDef.NameToken.Value), // local var to save in
				isMethod ? 0 : 2, // offset from current PC where code is located
				funDef.ArgNames.Length, // max number of args supplied
				isMethod ? 1 : 0); // is a method
			if (!isMethod)
			{
				buffer.Add(funDef.FirstToken, OpCode.ASSIGN_VAR, parser.GetId(funDef.NameToken.Value));
				buffer.Add(null, OpCode.JUMP, offset);
			}

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
			else if (expr is ThisKeyword) this.CompileThisKeyword(parser, buffer, (ThisKeyword)expr, outputUsed);
			else if (expr is Instantiate) this.CompileInstantiate(parser, buffer, (Instantiate)expr, outputUsed);
			else if (expr is DictionaryDefinition) this.CompileDictionaryDefinition(parser, buffer, (DictionaryDefinition)expr, outputUsed);
			else if (expr is BooleanCombination) this.CompileBooleanCombination(parser, buffer, (BooleanCombination)expr, outputUsed);
			else if (expr is BooleanNot) this.CompileBooleanNot(parser, buffer, (BooleanNot)expr, outputUsed);
			else if (expr is Ternary) this.CompileTernary(parser, buffer, (Ternary)expr, outputUsed);
			else throw new NotImplementedException();
		}

		private void CompileTernary(Parser parser, ByteBuffer buffer, Ternary ternary, bool outputUsed)
		{
			if (!outputUsed) throw new ParserException(ternary.FirstToken, "Cannot have this expression here.");

			this.CompileExpression(parser, buffer, ternary.Condition, true);
			ByteBuffer trueBuffer = new ByteBuffer();
			this.CompileExpression(parser, trueBuffer, ternary.TrueValue, true);
			ByteBuffer falseBuffer = new ByteBuffer();
			this.CompileExpression(parser, falseBuffer, ternary.FalseValue, true);
			trueBuffer.Add(null, OpCode.JUMP, falseBuffer.Size);
			buffer.Add(ternary.Condition.FirstToken, OpCode.JUMP_IF_FALSE, trueBuffer.Size);
			buffer.Concat(trueBuffer);
			buffer.Concat(falseBuffer);
		}

		private void CompileBooleanNot(Parser parser, ByteBuffer buffer, BooleanNot boolNot, bool outputUsed)
		{
			if (!outputUsed) throw new ParserException(boolNot.FirstToken, "Cannot have this expression here.");

			this.CompileExpression(parser, buffer, boolNot.Root, true);
			buffer.Add(boolNot.FirstToken, OpCode.BOOLEAN_NOT);
		}

		private void CompileBooleanCombination(Parser parser, ByteBuffer buffer, BooleanCombination boolComb, bool outputUsed)
		{
			if (!outputUsed) throw new ParserException(boolComb.FirstToken, "Cannot have this expression here.");

			ByteBuffer rightBuffer = new ByteBuffer();
			Expression[] expressions = boolComb.Expressions;
			this.CompileExpression(parser, rightBuffer, expressions[expressions.Length - 1], true);
			for (int i = expressions.Length - 2; i >= 0; --i)
			{
				ByteBuffer leftBuffer = new ByteBuffer();
				this.CompileExpression(parser, leftBuffer, expressions[i], true);
				Token op = boolComb.Ops[i];
				if (op.Value == "&&")
				{
					leftBuffer.Add(op, OpCode.JUMP_IF_FALSE_NO_POP, rightBuffer.Size);
				}
				else
				{
					leftBuffer.Add(op, OpCode.JUMP_IF_TRUE_NO_POP, rightBuffer.Size);
				}
				leftBuffer.Concat(rightBuffer);
				rightBuffer = leftBuffer;
			}

			buffer.Concat(rightBuffer);
		}

		private void CompileDictionaryDefinition(Parser parser, ByteBuffer buffer, DictionaryDefinition dictDef, bool outputUsed)
		{
			if (!outputUsed) throw new ParserException(dictDef.FirstToken, "Cannot have a dictionary all by itself.");

			int itemCount = dictDef.Keys.Length;
			List<Expression> expressionList = new List<Expression>();
			for (int i = 0; i < itemCount; ++i)
			{
				expressionList.Add(dictDef.Keys[i]);
				expressionList.Add(dictDef.Values[i]);
			}

			this.CompileExpressionList(parser, buffer, expressionList, true);

			buffer.Add(dictDef.FirstToken, OpCode.DEF_DICTIONARY, itemCount);
		}

		private void CompileInstantiate(Parser parser, ByteBuffer buffer, Instantiate instantiate, bool outputUsed)
		{
			this.CompileExpressionList(parser, buffer, instantiate.Args, true);
			buffer.Add(instantiate.NameToken, OpCode.CALL_CONSTRUCTOR, instantiate.Args.Length, parser.GetId(instantiate.NameToken.Value), outputUsed ? 1 : 0);
		}

		private void CompileThisKeyword(Parser parser, ByteBuffer buffer, ThisKeyword thisKeyword, bool outputUsed)
		{
			if (!outputUsed) throw new ParserException(thisKeyword.FirstToken, "This expression doesn't do anything.");

			buffer.Add(thisKeyword.FirstToken, OpCode.THIS);
		}

		private void CompileNullConstant(Parser parser, ByteBuffer buffer, NullConstant nullConstant, bool outputUsed)
		{
			if (!outputUsed) throw new ParserException(nullConstant.FirstToken, "This expression doesn't do anything.");

			buffer.Add(nullConstant.FirstToken, OpCode.LITERAL, parser.GetNullConstant());
		}

		private void CompileFloatConstant(Parser parser, ByteBuffer buffer, FloatConstant floatConstant, bool outputUsed)
		{
			if (!outputUsed) throw new ParserException(floatConstant.FirstToken, "This expression doesn't do anything.");
			buffer.Add(floatConstant.FirstToken, OpCode.LITERAL, parser.GetFloatConstant(floatConstant.Value));
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
					buffer.Add(increment.IncrementToken, OpCode.LITERAL, parser.GetIntConstant(1));
					buffer.Add(increment.IncrementToken, OpCode.BINARY_OP, increment.IsIncrement ? (int)BinaryOps.ADDITION : (int)BinaryOps.SUBTRACTION);
					buffer.Add(increment.IncrementToken, OpCode.DUPLICATE_STACK_TOP, 1);
					buffer.Add(variable.FirstToken, OpCode.ASSIGN_VAR, parser.GetId(variable.Name));
				}
				else
				{
					buffer.Add(increment.IncrementToken, OpCode.DUPLICATE_STACK_TOP, 1);
					buffer.Add(increment.IncrementToken, OpCode.LITERAL, parser.GetIntConstant(1));
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
			int argCount = sysFunc.Args.Length;
			string frameworkFunctionName = sysFunc.Name.Substring(1);
			FrameworkFunction ff = parser.GetFrameworkFunction(sysFunc.FirstToken, frameworkFunctionName);
			FrameworkFunctionUtil.VerifyArgsAsMuchAsPossible(sysFunc.FirstToken, ff, sysFunc.Args);
			buffer.Add(sysFunc.FirstToken, OpCode.CALL_FRAMEWORK_FUNCTION, (int)ff, outputUsed ? 1 : 0, argCount);
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
			buffer.Add(stringConstant.FirstToken, OpCode.LITERAL, parser.GetStringConstant(stringConstant.Value));
		}

		private void CompileBinaryOpChain(Parser parser, ByteBuffer buffer, BinaryOpChain opChain, bool outputUsed)
		{
			if (!outputUsed) throw new ParserException(opChain.FirstToken, "This expression isn't valid here.");

			this.CompileExpressionList(parser, buffer, new Expression[] { opChain.Left, opChain.Right }, true);

			Token opToken = opChain.Op;
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
				case "%": buffer.Add(opToken, OpCode.BINARY_OP, (int)BinaryOps.MODULO); break;
				case "!=": buffer.Add(opToken, OpCode.BINARY_OP, (int)BinaryOps.NOT_EQUALS); break;
				case "**": buffer.Add(opToken, OpCode.BINARY_OP, (int)BinaryOps.EXPONENT); break;
				case "|": buffer.Add(opToken, OpCode.BINARY_OP, (int)BinaryOps.BITWISE_OR); break;
				case "&": buffer.Add(opToken, OpCode.BINARY_OP, (int)BinaryOps.BITWISE_AND); break;
				case "^": buffer.Add(opToken, OpCode.BINARY_OP, (int)BinaryOps.BITWISE_XOR); break;
				case "<<": buffer.Add(opToken, OpCode.BINARY_OP, (int)BinaryOps.BIT_SHIFT_LEFT); break;
				case ">>": buffer.Add(opToken, OpCode.BINARY_OP, (int)BinaryOps.BIT_SHIFT_RIGHT); break;
				default: throw new NotImplementedException("Binary op: " + opChain.Op.Value);
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
			buffer.Add(boolConstant.FirstToken, OpCode.LITERAL, parser.GetBoolConstant(boolConstant.Value));
		}

		private void CompileVariable(Parser parser, ByteBuffer buffer, Variable variable, bool outputUsed)
		{
			if (!outputUsed) throw new ParserException(variable.FirstToken, "This expression does nothing.");
			buffer.Add(variable.FirstToken, OpCode.VARIABLE, parser.GetId(variable.Name));
		}

		private void CompileIntegerConstant(Parser parser, ByteBuffer buffer, IntegerConstant intConst, bool outputUsed)
		{
			if (!outputUsed) throw new ParserException(intConst.FirstToken, "This expression does nothing.");
			buffer.Add(intConst.FirstToken, OpCode.LITERAL, parser.GetIntConstant(intConst.Value));
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

				// Add the literal ID arg from the above literals to the arg list of a LITERAL_STREAM
				buffer.Add(
					expressions[0].FirstToken, 
					OpCode.LITERAL_STREAM, 
					exprBuffer.ToIntList().Reverse<int[]>().Select<int[], int>(args => args[1]).ToArray());
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
