using System;
using System.Collections.Generic;
using System.Linq;
using Crayon.ParseTree;

namespace Crayon
{
	internal class ByteCodeCompiler
	{
		public ByteBuffer GenerateByteCode(Parser parser, IList<Executable> lines, IList<string> spriteSheetOpsStringArgs, IList<int[]> spriteSheetOpsIntArgs)
		{
			ByteBuffer userCode = new ByteBuffer();

			this.Compile(parser, userCode, lines);
			userCode.Add(null, OpCode.RETURN_NULL);

			ByteBuffer literalsTable = parser.LiteralLookup.BuildByteCode();

			ByteBuffer tokenData = this.BuildTokenData(userCode);

			ByteBuffer fileContent = this.BuildFileContent(parser.GetFilesById());

			ByteBuffer switchStatements = this.BuildSwitchStatementTables(parser);

			ByteBuffer spriteSheetStuff = this.BuildSpriteSheetStuff(spriteSheetOpsStringArgs, spriteSheetOpsIntArgs);

			ByteBuffer header = new Crayon.ByteBuffer();
			header.Concat(literalsTable);
			header.Concat(tokenData);
			header.Concat(fileContent);
			header.Concat(switchStatements);
			header.Concat(spriteSheetStuff);
			header.Add(null, OpCode.FINALIZE_INITIALIZATION);

			ByteBuffer output = new Crayon.ByteBuffer();
			output.Add(null, OpCode.USER_CODE_START, header.Size + 1, parser.VariableIds.Size);
			output.Concat(header);
			output.Concat(userCode);

			return output;
		}

		private ByteBuffer BuildSpriteSheetStuff(IList<string> stringArgs, IList<int[]> intArgs)
		{
			ByteBuffer buffer = new ByteBuffer();
			int length = stringArgs.Count;
			for (int i = 0; i < length; ++i)
			{
				buffer.Add(null, OpCode.SPRITE_SHEET_BUILDER, stringArgs[i], intArgs[i]);
			}
			return buffer;
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

		// Build the lookup table that maps PC's to tokens. There can be multiple tokens per PC, but it's up to the Op
		// to have a consistent convention to figure out the context of those tokens.
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

				if (token != null)
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
			else if (line is DoWhileLoop) this.CompileDoWhileLoop(parser, buffer, (DoWhileLoop)line);
			else throw new NotImplementedException("Invalid target for byte code compilation");
		}

		private void CompileForEachLoop(Parser parser, ByteBuffer buffer, ForEachLoop forEachLoop)
		{
			buffer.Add(null, OpCode.LITERAL, parser.GetIntConstant(0));
			buffer.Add(null, OpCode.LITERAL, parser.GetIntConstant(forEachLoop.IterationVariableId));
			this.CompileExpression(parser, buffer, forEachLoop.IterationExpression, true);
			buffer.Add(forEachLoop.IterationExpression.FirstToken, OpCode.VERIFY_TYPE_IS_ITERABLE);

			ByteBuffer body = new ByteBuffer();
			ByteBuffer body2 = new Crayon.ByteBuffer();

			this.Compile(parser, body2, forEachLoop.Code);

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

			int switchId = parser.RegisterByteCodeSwitch(switchStatement.FirstToken, chunkIdsToOffsets, integersToChunkIds, stringsToChunkIds, switchStatement.UsesIntegers);

			int defaultOffsetLength = defaultChunkId == -1
				? chunkBuffer.Size
				: chunkIdsToOffsets[defaultChunkId];

			buffer.Add(switchStatement.FirstToken, switchStatement.UsesIntegers ? OpCode.SWITCH_INT : OpCode.SWITCH_STRING, switchId, defaultOffsetLength);
			buffer.Concat(chunkBuffer);
		}

		private void CompileClass(Parser parser, ByteBuffer buffer, ClassDefinition classDefinition)
		{
			foreach (FunctionDefinition fd in classDefinition.Methods)
			{
				int pc = buffer.Size;
				fd.FinalizedPC = pc;
				this.CompileFunctionDefinition(parser, buffer, fd, true);
			}

			int classId = classDefinition.ClassID;
			int baseClassId = classDefinition.BaseClass != null ? classDefinition.BaseClass.ClassID : 0;
			int nameId = parser.GetId(classDefinition.NameToken.Value);
			int constructorId = classDefinition.Constructor.FunctionID;
			int staticConstructorId = classDefinition.StaticConstructor != null ? classDefinition.StaticConstructor.FunctionID : 0;
			
			FieldDeclaration[] staticFields = classDefinition.Fields.Where<FieldDeclaration>(fd => fd.IsStaticField).ToArray();
			FieldDeclaration[] regularFields = classDefinition.Fields.Where<FieldDeclaration>(fd => !fd.IsStaticField).ToArray();
			FunctionDefinition[] regularMethods = classDefinition.Methods.Where<FunctionDefinition>(fd => !fd.IsStaticMethod).ToArray();

			List<int> args = new List<int>()
			{
				classId,
				baseClassId,
				nameId,
				constructorId,
				staticConstructorId,
				staticFields.Length,
				regularFields.Length,
				regularMethods.Length,
			};

			foreach (FieldDeclaration field in regularFields) {
				args.Add(parser.GetId(field.NameToken.Value));
			}

			foreach (FunctionDefinition func in regularMethods)
			{
				args.Add(parser.GetId(func.NameToken.Value));
				args.Add(func.FunctionID);
			}

			buffer.Add(classDefinition.FirstToken, OpCode.CLASS_DEFINITION2, args.ToArray());
		}

		private void CompileConstructor(Parser parser, ByteBuffer buffer, ConstructorDefinition constructor)
		{
			// TODO: throw parser exception in the resolver if a return appears with any value
			
			ClassDefinition cd = (ClassDefinition)constructor.FunctionOrClassOwner;

			this.CompileFunctionArgs(parser, buffer, constructor.Args, constructor.DefaultValues, constructor.ArgVarIDs);

			if (constructor.BaseToken != null)
			{
				this.CompileExpressionList(parser, buffer, constructor.BaseArgs, true);
				buffer.Add(constructor.BaseToken, OpCode.CALL_BASE_CONSTRUCTOR, constructor.BaseArgs.Length);
			}

			foreach (FieldDeclaration fd in cd.Fields)
			{
				if (!fd.IsStaticField)
				{
					this.CompileExpression(parser, buffer, fd.DefaultValue, true);
					buffer.Add(fd.FirstToken, OpCode.ASSIGN_THIS_STEP, parser.GetId(fd.NameToken.Value));
				}
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
			codeBuffer.ResolveContinues(true); // resolve continues as jump-to-end before you add the step instructions.
			this.Compile(parser, codeBuffer, forLoop.Step);

			ByteBuffer forBuffer = new ByteBuffer();
			this.CompileExpression(parser, forBuffer, forLoop.Condition, true);
			forBuffer.Add(forLoop.Condition.FirstToken, OpCode.JUMP_IF_FALSE, codeBuffer.Size + 1); // +1 to go past the jump I'm about to add.

			forBuffer.Concat(codeBuffer);
			forBuffer.Add(null, OpCode.JUMP, -forBuffer.Size - 1);

			forBuffer.ResolveBreaks();

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

		private void CompileDoWhileLoop(Parser parser, ByteBuffer buffer, DoWhileLoop doWhileLoop)
		{
			ByteBuffer loopBody = new ByteBuffer();
			this.Compile(parser, loopBody, doWhileLoop.Code);
			loopBody.ResolveContinues(true); // continues should jump to the condition, hence the true.

			ByteBuffer condition = new ByteBuffer();
			this.CompileExpression(parser, condition, doWhileLoop.Condition, true);
			loopBody.Concat(condition);
			loopBody.Add(doWhileLoop.Condition.FirstToken, OpCode.JUMP_IF_TRUE, -loopBody.Size - 1);
			loopBody.ResolveBreaks();

			buffer.Concat(loopBody);
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
					int scopeId = varTarget.LocalScopeId == -1 ? varTarget.GlobalScopeId : varTarget.LocalScopeId;
					buffer.Add(assignment.AssignmentOpToken, OpCode.ASSIGN_VAR, scopeId);
				}
				else if (assignment.Target is BracketIndex)
				{
					BracketIndex bi = (BracketIndex)assignment.Target;
					// TODO: optimization opportunity: special op code when index is a string or int constant.

					this.CompileExpression(parser, buffer, bi.Root, true);
					this.CompileExpression(parser, buffer, bi.Index, true);
					this.CompileExpression(parser, buffer, assignment.Value, true);
					buffer.Add(assignment.AssignmentOpToken, OpCode.ASSIGN_INDEX, 0);
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
						buffer.Add(assignment.AssignmentOpToken, OpCode.ASSIGN_STEP, parser.GetId(dotStep.StepToken.Value), 0);
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
					int scopeId = varTarget.LocalScopeId;
					if (scopeId != -1)
					{
						buffer.Add(varTarget.FirstToken, OpCode.VARIABLE, parser.GetId(varTarget.Name), scopeId);
					}
					else // it's in the global scope
					{
						scopeId = varTarget.GlobalScopeId;
						buffer.Add(varTarget.FirstToken, OpCode.VARIABLE_GLOBAL, parser.GetId(varTarget.Name), scopeId);
					}
					this.CompileExpression(parser, buffer, assignment.Value, true);
					buffer.Add(assignment.AssignmentOpToken, OpCode.BINARY_OP, (int)op);
					buffer.Add(assignment.Target.FirstToken, OpCode.ASSIGN_VAR, scopeId);
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
						buffer.Add(assignment.AssignmentOpToken, OpCode.ASSIGN_STEP, stepId, 0);
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
					buffer.Add(assignment.AssignmentOpToken, OpCode.ASSIGN_INDEX, 0);
				}
				else
				{
					throw new ParserException(assignment.AssignmentOpToken, "Assignment is not allowed on this sort of expression.");
				}
			}
		}

		private void CompileFunctionArgs(Parser parser, ByteBuffer buffer, IList<Token> argNames, IList<Expression> argValues, IList<int> argVarIds)
		{
			for (int i = 0; i < argNames.Count; ++i)
			{
				string argName = argNames[i].Value;
				Expression defaultValue = argValues[i];
				int localVarId = argVarIds[i];
				if (defaultValue == null)
				{
					buffer.Add(argNames[i], OpCode.ASSIGN_FUNCTION_ARG, localVarId, i);
				}
				else
				{
					ByteBuffer defaultValueCode = new ByteBuffer();

					this.CompileExpression(parser, defaultValueCode, defaultValue, true);
					defaultValueCode.Add(argNames[i], OpCode.ASSIGN_VAR, localVarId);

					buffer.Add(defaultValue.FirstToken, OpCode.ASSIGN_FUNCTION_ARG_AND_JUMP, localVarId, i, defaultValueCode.Size);
					buffer.Concat(defaultValueCode);
				}
			}
		}

		private void CompileFunctionDefinition(Parser parser, ByteBuffer buffer, FunctionDefinition funDef, bool isMethod)
		{
			ByteBuffer tBuffer = new ByteBuffer();

			this.CompileFunctionArgs(parser, tBuffer, funDef.ArgNames, funDef.DefaultValues, funDef.ArgVarIDs);

			Compile(parser, tBuffer, funDef.Code);

			int offset = tBuffer.Size;

			int minArgCount = 0;
			for (int i = 0; i < funDef.DefaultValues.Length; ++i)
			{
				if (funDef.DefaultValues[i] != null)
				{
					break;
				}
				minArgCount++;
			}

			buffer.Add(
				funDef.FirstToken,
				OpCode.FUNCTION_DEFINITION2,
				funDef.FunctionID,
				parser.GetId(funDef.NameToken.Value), // local var to save in
				minArgCount,
				funDef.ArgNames.Length, // max number of args supplied
				isMethod ? (funDef.IsStaticMethod ? 2 : 1) : 0, // type (0 - function, 1 - method, 2 - static method)
				isMethod ? ((ClassDefinition)funDef.FunctionOrClassOwner).ClassID : 0,
				tBuffer.Size);

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
			else if (expr is CompileTimeDictionary) this.CompileCompileTimeDictionary((CompileTimeDictionary)expr);
			else if (expr is ListSlice) this.CompileListSlice(parser, buffer, (ListSlice)expr, outputUsed);
			else if (expr is NullCoalescer) this.CompileNullCoalescer(parser, buffer, (NullCoalescer)expr, outputUsed);
			else if (expr is BaseKeyword) this.CompileBaseKeyword(parser, buffer, (BaseKeyword)expr, outputUsed);
			else if (expr is BaseMethodReference) this.CompileBaseMethodReference(parser, buffer, (BaseMethodReference)expr, outputUsed);
			else if (expr is LibraryFunctionCall) this.CompileLibraryFunctionCall(parser, buffer, (LibraryFunctionCall)expr, outputUsed);
			else if (expr is FunctionReference) this.CompileFunctionReference(parser, buffer, (FunctionReference)expr, outputUsed);
			else throw new NotImplementedException();
		}

		private static void EnsureUsed(Token token, bool outputUsed)
		{
			if (!outputUsed)
			{
				throw new ParserException(token, "Cannot have this expression here. It does nothing. Did you mean to store this output into a variable or return it?");
			}
		}

		// Non-invoked function references.
		private void CompileFunctionReference(Parser parser, ByteBuffer buffer, FunctionReference funcRef, bool outputUsed)
		{
			EnsureUsed(funcRef.FirstToken, outputUsed);

			FunctionDefinition funcDef = funcRef.FunctionDefinition;

			int classIdStaticCheck = 0;
			int type = 0;
			if (funcDef.FunctionOrClassOwner is ClassDefinition && funcDef.IsStaticMethod)
			{
				if (funcDef.IsStaticMethod) {
					classIdStaticCheck = ((ClassDefinition)funcDef.FunctionOrClassOwner).ClassID ;
					type = 2;
				} else {
					type = 1;
				}
			}
			buffer.Add(funcRef.FirstToken, OpCode.PUSH_FUNC_REF,
				funcDef.FunctionID,
				type,
				classIdStaticCheck);
		}

		private void CompileBaseKeyword(Parser parser, ByteBuffer buffer, BaseKeyword baseKeyword, bool outputUsed)
		{
			throw new ParserException(baseKeyword.FirstToken, "Cannot have a reference to 'base' without invoking a field.");
		}

		private void CompileBaseMethodReference(Parser parser, ByteBuffer buffer, BaseMethodReference baseMethodReference, bool outputUsed)
		{
			EnsureUsed(baseMethodReference.FirstToken, outputUsed);
			int baseClassId = parser.GetClass(baseMethodReference.ClassToWhichThisMethodRefers).ClassID;
			buffer.Add(baseMethodReference.DotToken, OpCode.DEREF_DOT_ON_BASE, parser.GetId(baseMethodReference.StepToken.Value), baseClassId);
		}

		private void CompileCompileTimeDictionary(CompileTimeDictionary compileTimeDictionary)
		{
			if (compileTimeDictionary.Type == "var")
			{
				throw new ParserException(compileTimeDictionary.FirstToken, "$var is a compile-time dictionary and must be dereferenced with a hardcoded string constant.");
			}
			throw new Exception(); // should not happen.
		}

		private void CompileListSlice(Parser parser, ByteBuffer buffer, ListSlice listSlice, bool outputUsed)
		{
			EnsureUsed(listSlice.FirstToken, outputUsed);
			this.CompileExpression(parser, buffer, listSlice.Root, true);

			Expression step = listSlice.Items[2];
			bool isStep1 = step is IntegerConstant && ((IntegerConstant)step).Value == 1;

			int serializeThese = isStep1 ? 2 : 3;
			for (int i = 0; i < serializeThese; ++i)
			{
				Expression item = listSlice.Items[i];
				if (item != null)
				{
					this.CompileExpression(parser, buffer, item, true);
				}
			}

			bool firstIsPresent = listSlice.Items[0] != null;
			bool secondIsPresent = listSlice.Items[1] != null;

			buffer.Add(listSlice.BracketToken, OpCode.LIST_SLICE, new int[] { firstIsPresent ? 1 : 0, secondIsPresent ? 1 : 0, isStep1 ? 0 : 1 });
		}

		private void CompileTernary(Parser parser, ByteBuffer buffer, Ternary ternary, bool outputUsed)
		{
			EnsureUsed(ternary.FirstToken, outputUsed);

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

		private void CompileNullCoalescer(Parser parser, ByteBuffer buffer, NullCoalescer nullCoalescer, bool outputUsed)
		{
			EnsureUsed(nullCoalescer.FirstToken, outputUsed);

			this.CompileExpression(parser, buffer, nullCoalescer.PrimaryExpression, true);
			ByteBuffer secondaryExpression = new ByteBuffer();
			this.CompileExpression(parser, secondaryExpression, nullCoalescer.SecondaryExpression, true);
			buffer.Add(nullCoalescer.FirstToken, OpCode.POP_IF_NULL_OR_JUMP, secondaryExpression.Size);
			buffer.Concat(secondaryExpression);
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

			if (increment.Root is Variable)
			{
				// OpCode re-use be damned. This should be not one, but two top-level op codes.
				// INCREMENT_INLINE and INCREMENT_POP (depending on whether outputUsed is true)
				// In fact, the code here in its current form is actually WRONG because someString++ will have 
				// a '1' appended to it when it really should be an error if the variable is not an integer.
				// Same for the others below. Ideally the DUPLICATE_STACK_TOP op should be removed.
				Variable variable = (Variable)increment.Root;
				int scopeId = variable.LocalScopeId == -1 ? variable.GlobalScopeId : variable.LocalScopeId;
				this.CompileExpression(parser, buffer, increment.Root, true);
				if (increment.IsPrefix)
				{
					buffer.Add(increment.IncrementToken, OpCode.LITERAL, parser.GetIntConstant(1));
					buffer.Add(increment.IncrementToken, OpCode.BINARY_OP, increment.IsIncrement ? (int)BinaryOps.ADDITION : (int)BinaryOps.SUBTRACTION);
					buffer.Add(increment.IncrementToken, OpCode.DUPLICATE_STACK_TOP, 1);
					buffer.Add(variable.FirstToken, OpCode.ASSIGN_VAR, scopeId);
				}
				else
				{
					buffer.Add(increment.IncrementToken, OpCode.DUPLICATE_STACK_TOP, 1);
					buffer.Add(increment.IncrementToken, OpCode.LITERAL, parser.GetIntConstant(1));
					buffer.Add(increment.IncrementToken, OpCode.BINARY_OP, increment.IsIncrement ? (int)BinaryOps.ADDITION : (int)BinaryOps.SUBTRACTION);
					buffer.Add(variable.FirstToken, OpCode.ASSIGN_VAR, scopeId);
				}
			}
			else if (increment.Root is BracketIndex)
			{
				BracketIndex bracketIndex = (BracketIndex)increment.Root;
				this.CompileExpression(parser, buffer, bracketIndex.Root, true);
				this.CompileExpression(parser, buffer, bracketIndex.Index, true);
				buffer.Add(increment.IncrementToken, OpCode.DUPLICATE_STACK_TOP, 2);
				buffer.Add(bracketIndex.BracketToken, OpCode.INDEX);
				if (increment.IsPrefix)
				{
					buffer.Add(increment.IncrementToken, OpCode.LITERAL, parser.GetIntConstant(1));
					buffer.Add(increment.IncrementToken, OpCode.BINARY_OP, increment.IsIncrement ? (int)BinaryOps.ADDITION : (int)BinaryOps.SUBTRACTION);
					buffer.Add(increment.IncrementToken, OpCode.ASSIGN_INDEX, 1);
				}
				else
				{
					buffer.Add(increment.IncrementToken, OpCode.STACK_INSERTION_FOR_INCREMENT);
					buffer.Add(increment.IncrementToken, OpCode.LITERAL, parser.GetIntConstant(1));
					buffer.Add(increment.IncrementToken, OpCode.BINARY_OP, increment.IsIncrement ? (int)BinaryOps.ADDITION : (int)BinaryOps.SUBTRACTION);
					buffer.Add(increment.IncrementToken, OpCode.ASSIGN_INDEX, 0);
				}
			}
			else if (increment.Root is DotStep)
			{
				DotStep dotStep = (DotStep)increment.Root;
				this.CompileExpression(parser, buffer, dotStep.Root, true);
				buffer.Add(increment.IncrementToken, OpCode.DUPLICATE_STACK_TOP, 1);
				buffer.Add(dotStep.DotToken, OpCode.DEREF_DOT, parser.GetId(dotStep.StepToken.Value));
				if (increment.IsPrefix)
				{
					buffer.Add(increment.IncrementToken, OpCode.LITERAL, parser.GetIntConstant(1));
					buffer.Add(increment.IncrementToken, OpCode.BINARY_OP, increment.IsIncrement ? (int)BinaryOps.ADDITION : (int)BinaryOps.SUBTRACTION);
					buffer.Add(increment.IncrementToken, OpCode.ASSIGN_STEP, parser.GetId(dotStep.StepToken.Value), 1);
				}
				else
				{
					buffer.Add(increment.IncrementToken, OpCode.DUPLICATE_STACK_TOP, 2);
					buffer.Add(increment.IncrementToken, OpCode.LITERAL, parser.GetIntConstant(1));
					buffer.Add(increment.IncrementToken, OpCode.BINARY_OP, increment.IsIncrement ? (int)BinaryOps.ADDITION : (int)BinaryOps.SUBTRACTION);
					buffer.Add(increment.IncrementToken, OpCode.ASSIGN_STEP, parser.GetId(dotStep.StepToken.Value), 0);
					buffer.Add(increment.IncrementToken, OpCode.STACK_SWAP_POP);
				}
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

		private void CompileLibraryFunctionCall(Parser parser, ByteBuffer buffer, LibraryFunctionCall libFunc, bool outputUsed)
		{
			this.CompileExpressionList(parser, buffer, libFunc.Args, true);
			int argCount = libFunc.Args.Length;
			int id = parser.SystemLibraryManager.GetIdForFunction(libFunc.Name, libFunc.LibraryName);
			buffer.Add(libFunc.FirstToken, OpCode.CALL_LIB_FUNCTION, id, argCount, outputUsed ? 1 : 0);
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
			int primitiveMethodId = Constants.GetPrimitiveFieldId(dotStep.StepToken.Value);
			buffer.Add(dotStep.DotToken, OpCode.DEREF_DOT, parser.GetId(dotStep.StepToken.Value), primitiveMethodId);
		}

		private void CompileBooleanConstant(Parser parser, ByteBuffer buffer, BooleanConstant boolConstant, bool outputUsed)
		{
			if (!outputUsed) throw new ParserException(boolConstant.FirstToken, "This expression does nothing.");
			buffer.Add(boolConstant.FirstToken, OpCode.LITERAL, parser.GetBoolConstant(boolConstant.Value));
		}

		private void CompileVariable(Parser parser, ByteBuffer buffer, Variable variable, bool outputUsed)
		{
			if (!outputUsed) throw new ParserException(variable.FirstToken, "This expression does nothing.");
			int nameId = parser.GetId(variable.Name);
			Token token = variable.FirstToken;
			if (variable.LocalScopeId == -1)
			{
				buffer.Add(token, OpCode.VARIABLE_GLOBAL, nameId, variable.GlobalScopeId);
			}
			else if (variable.GlobalScopeId == -1)
			{
				buffer.Add(token, OpCode.VARIABLE, nameId, variable.LocalScopeId);
			}
			else
			{
				buffer.Add(token, OpCode.VARIABLE, nameId, variable.LocalScopeId, variable.GlobalScopeId);
			}
		}

		private void CompileIntegerConstant(Parser parser, ByteBuffer buffer, IntegerConstant intConst, bool outputUsed)
		{
			if (!outputUsed) throw new ParserException(intConst.FirstToken, "This expression does nothing.");
			buffer.Add(intConst.FirstToken, OpCode.LITERAL, parser.GetIntConstant(intConst.Value));
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

		public void CompileExpressionList(Parser parser, ByteBuffer buffer, IList<Expression> expressions, bool outputUsed)
		{
			if (expressions.Count == 0) return;
			if (expressions.Count == 1)
			{
				this.CompileExpression(parser, buffer, expressions[0], outputUsed);
				return;
			}

			List<Expression> literals = new List<Expression>();
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
				}
			}

			if (literals.Count > 0)
			{
				this.CompileLiteralStream(parser, buffer, literals, true);
				literals.Clear();
			}
		}

		private void CompileFunctionCall(Parser parser, ByteBuffer buffer, FunctionCall funCall, bool outputUsed)
		{
			this.CompileExpressionList(parser, buffer, funCall.Args, true);

			Expression root = funCall.Root;
			Variable rootVar = root as Variable;
			if (rootVar != null && rootVar.LocalScopeId == -1)
			{
				string functionName = rootVar.Name;
				buffer.Add(funCall.ParenToken, OpCode.CALL_FUNCTION_ON_GLOBAL, rootVar.GlobalScopeId, funCall.Args.Length, outputUsed ? 1 : 0);
			}
			else
			{
				this.CompileExpression(parser, buffer, funCall.Root, true);
				buffer.Add(funCall.ParenToken, OpCode.CALL_FUNCTION, funCall.Args.Length, outputUsed ? 1 : 0);
			}
		}
	}
}
