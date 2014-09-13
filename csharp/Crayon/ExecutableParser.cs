using System;
using System.Collections.Generic;
using Crayon.ParseTree;

namespace Crayon
{
	internal static class ExecutableParser
	{
		private static readonly HashSet<string> ASSIGNMENT_OPS = new HashSet<string>("= += -= *= /= %= |= &= ^= <<= >>=".Split(' '));

		private static readonly HashSet<string> SYSTEM_LIBRARIES = new HashSet<string>("Game".Split(' '));

		public static Executable Parse(TokenStream tokens, bool simpleOnly, bool semicolonPresent, bool isRoot)
		{
			string value = tokens.PeekValue();

			if (!simpleOnly)
			{
				if (value == "struct")
				{
					if (!isRoot)
					{
						throw new ParserException(tokens.Peek(), "structs cannot be nested into any other construct.");
					}

					// struct is special. If you are not compiling in JS mode, 
					if (Parser.IsValidIdentifier(tokens.PeekValue(1)) && tokens.PeekValue(2) == "{")
					{
						return ParseStruct(tokens);
					}
				}

				if (!isRoot && (value == "function" || value == "class"))
				{
					throw new ParserException(tokens.Peek(), (value == "function" ? "Function" : "Class") + " definition cannot be nested in another construct.");
				}

				if (value == "import")
				{
					if (!isRoot)
					{
						throw new ParserException(tokens.Peek(), "File imports can only be made from the root of a file and cannot be nested inside functions/loops/etc.");
					}

					Token importToken = tokens.PopExpected("import");
					Token fileToken = tokens.Pop();
					char importPathFirstChar = fileToken.Value[0];
					if (importPathFirstChar != '\'' && importPathFirstChar != '"')
					{
						if (SYSTEM_LIBRARIES.Contains(fileToken.Value))
						{
							tokens.PopExpected(";");
							return new ImportStatement(importToken, fileToken, true);
						}
						else
						{
							throw new ParserException(fileToken, "Expected string path to file or system library name.");
						}
					}
					tokens.PopExpected(";");
					return new ImportStatement(importToken, fileToken, false);
				}

				if (value == "enum")
				{
					if (!isRoot)
					{
						throw new ParserException(tokens.Peek(), "Enums can only be defined from the root of a file and cannot be nested inside functions/loops/etc.");
					}

					return ParseEnumDefinition(tokens);
				}

				switch (value)
				{
					case "function": return ParseFunction(tokens);
					case "class": return ParseClassDefinition(tokens);
					case "enum": return ParseEnumDefinition(tokens);
					case "for": return ParseFor(tokens);
					case "while": return ParseWhile(tokens);
					case "do": return ParseDoWhile(tokens);
					case "switch": return ParseSwitch(tokens);
					case "if": return ParseIf(tokens);
					case "try": return ParseTry(tokens);
					case "return": return ParseReturn(tokens);
					case "break": return ParseBreak(tokens);
					case "continue": return ParseContinue(tokens);
					case "const": return ParseConst(tokens);
					case "constructor": return ParseConstructor(tokens);
					default: break;
				}
			}

			Expression expr = ExpressionParser.Parse(tokens);
			value = tokens.PeekValue();
			if (ASSIGNMENT_OPS.Contains(value))
			{
				Token assignment = tokens.Pop();
				Expression assignmentValue = ExpressionParser.Parse(tokens);
				if (semicolonPresent) tokens.PopExpected(";");
				return new Assignment(expr, assignment, assignment.Value, assignmentValue);
			}

			if (semicolonPresent)
			{
				tokens.PopExpected(";");
			}

			return new ExpressionAsExecutable(expr);
		}

		private static Executable ParseConstructor(TokenStream tokens)
		{
			Token constructorToken = tokens.PopExpected("constructor");
			tokens.PopExpected("(");
			List<Token> argNames = new List<Token>();
			List<Expression> argValues = new List<Expression>();
			while (!tokens.PopIfPresent(")"))
			{
				if (argNames.Count > 0)
				{
					tokens.PopExpected(",");
				}

				Token argName = tokens.Pop();
				Parser.VerifyIdentifier(argName);
				Expression defaultValue = null;
				if (tokens.PopIfPresent("="))
				{
					defaultValue = ExpressionParser.Parse(tokens);
				}

				argNames.Add(argName);
				argValues.Add(defaultValue);
			}

			List<Expression> baseArgs = new List<Expression>();
			Token baseToken = null;
			if (tokens.PopIfPresent(":"))
			{
				baseToken = tokens.PopExpected("base");
				tokens.PopExpected("(");
				while (!tokens.PopIfPresent(")"))
				{
					if (baseArgs.Count > 0)
					{
						tokens.PopExpected(",");
					}

					baseArgs.Add(ExpressionParser.Parse(tokens));
				}
			}

			IList<Executable> code = Parser.ParseBlock(tokens, true);

			return new ConstructorDefinition(constructorToken, argNames, argValues, baseArgs, code, baseToken);
		}

		private static Executable ParseConst(TokenStream tokens)
		{
			Token constToken = tokens.PopExpected("const");
			Token nameToken = tokens.Pop();
			Parser.VerifyIdentifier(nameToken);
			tokens.PopExpected("=");
			Expression expression = ExpressionParser.Parse(tokens);
			tokens.PopExpected(";");

			return new ConstStatement(constToken, nameToken, expression);
		}

		private static Executable ParseEnumDefinition(TokenStream tokens)
		{
			Token enumToken = tokens.PopExpected("enum");
			Token nameToken = tokens.Pop();
			Parser.VerifyIdentifier(nameToken);
			string name = nameToken.Value;
			tokens.PopExpected("{");
			bool nextForbidden = false;
			List<Token> items = new List<Token>();
			List<Expression> values = new List<Expression>();
			while (!tokens.PopIfPresent("}"))
			{
				if (nextForbidden) tokens.PopExpected("}"); // crash

				Token enumItem = tokens.Pop();
				Parser.VerifyIdentifier(enumItem);
				if (tokens.PopIfPresent("="))
				{
					values.Add(ExpressionParser.Parse(tokens));
				}
				else
				{
					values.Add(null);
				}
				nextForbidden = !tokens.PopIfPresent(",");
				items.Add(enumItem);
			}

			return new EnumDefinition(enumToken, nameToken, items, values);
		}

		private static Executable ParseClassDefinition(TokenStream tokens)
		{
			Token classToken = tokens.PopExpected("class");
			Token classNameToken = tokens.Pop();
			Parser.VerifyIdentifier(classNameToken);
			List<Token> baseClasses = new List<Token>();
			if (tokens.PopIfPresent(":"))
			{
				if (baseClasses.Count > 0)
				{
					tokens.PopExpected(",");
				}

				Token baseClass = tokens.Pop();
				Parser.VerifyIdentifier(baseClass);
				baseClasses.Add(baseClass);
			}
			tokens.PopExpected("{");
			List<FunctionDefinition> methods = new List<FunctionDefinition>();
			ConstructorDefinition constructorDef = null;
			while (!tokens.PopIfPresent("}"))
			{
				if (tokens.IsNext("function"))
				{
					methods.Add((FunctionDefinition)ExecutableParser.ParseFunction(tokens));
				}
				else if (tokens.IsNext("constructor"))
				{
					if (constructorDef != null)
					{
						throw new ParserException(tokens.Pop(), "Multiple constructors are not allowed. Use optional arguments.");
					}

					constructorDef = (ConstructorDefinition)ExecutableParser.ParseConstructor(tokens);
				}
				else
				{
					tokens.PopExpected("}");
				}
			}

			if (baseClasses.Count > 1) throw new ParserException(baseClasses[1], "Cannot support interfaces yet.");

			return new ClassDefinition(classToken, classNameToken, baseClasses, methods, constructorDef);
		}

		private static Executable ParseStruct(TokenStream tokens)
		{
			Token structToken = tokens.PopExpected("struct");
			Token structNameToken = tokens.Pop();
			Parser.VerifyIdentifier(structNameToken);

			tokens.PopExpected("{");

			List<Token> fieldTokens = new List<Token>();
			bool nextForbidden = false;
			while (!tokens.PopIfPresent("}"))
			{
				if (nextForbidden) tokens.PopExpected("}"); // crash

				Token fieldToken = tokens.Pop();
				Parser.VerifyIdentifier(fieldToken);
				nextForbidden = !tokens.PopIfPresent(",");
				fieldTokens.Add(fieldToken);
			}

			return new StructDefinition(structToken, structNameToken, fieldTokens);
		}

		private static Executable ParseFunction(TokenStream tokens)
		{
			Token functionToken = tokens.PopExpected("function");
			Token functionNameToken = tokens.Pop();
			Parser.VerifyIdentifier(functionNameToken);
			tokens.PopExpected("(");
			List<Token> argNames = new List<Token>();
			List<Expression> defaultValues = new List<Expression>();
			while (!tokens.PopIfPresent(")"))
			{
				if (argNames.Count > 0) tokens.PopExpected(",");
				Token argName = tokens.Pop();
				Expression defaultValue = null;
				Parser.VerifyIdentifier(argName);
				if (tokens.PopIfPresent("="))
				{
					defaultValue = ExpressionParser.Parse(tokens);
				}
				argNames.Add(argName);
				defaultValues.Add(defaultValue);
			}

			IList<Executable> code = Parser.ParseBlock(tokens, true);

			return new FunctionDefinition(functionToken, functionNameToken, argNames, defaultValues, code);
		}

		private static Executable ParseFor(TokenStream tokens)
		{
			Token forToken = tokens.PopExpected("for");
			tokens.PopExpected("(");
			List<Executable> init = new List<Executable>();
			while (!tokens.PopIfPresent(";"))
			{
				if (init.Count > 0) tokens.PopExpected(",");
				init.Add(Parse(tokens, true, false, false));
			}
			Expression condition = ExpressionParser.Parse(tokens);
			tokens.PopExpected(";");
			List<Executable> step = new List<Executable>();
			while (!tokens.PopIfPresent(")"))
			{
				if (step.Count > 0) tokens.PopExpected(",");
				step.Add(Parse(tokens, true, false, false));
			}

			IList<Executable> body = Parser.ParseBlock(tokens, false);

			return new ForLoop(forToken, init, condition, step, body);
		}

		private static Executable ParseWhile(TokenStream tokens)
		{
			Token whileToken = tokens.PopExpected("while");
			tokens.PopExpected("(");
			Expression condition = ExpressionParser.Parse(tokens);
			tokens.PopExpected(")");
			IList<Executable> body = Parser.ParseBlock(tokens, false);
			return new WhileLoop(whileToken, condition, body);
		}

		private static Executable ParseDoWhile(TokenStream tokens)
		{
			Token doToken = tokens.PopExpected("do");
			IList<Executable> body = Parser.ParseBlock(tokens, true);
			tokens.PopExpected("(");
			Expression condition = ExpressionParser.Parse(tokens);
			tokens.PopExpected(")");
			tokens.PopExpected(";");
			return new DoWhileLoop(doToken, body, condition);
		}

		private static Executable ParseSwitch(TokenStream tokens)
		{
			Token switchToken = tokens.PopExpected("switch");

			Expression explicitMax = null;
			Token explicitMaxToken = null;
			if (tokens.IsNext("{"))
			{
				explicitMaxToken = tokens.Pop();
				explicitMax = ExpressionParser.Parse(tokens);
				tokens.PopExpected("}");
			}

			tokens.PopExpected("(");
			Expression condition = ExpressionParser.Parse(tokens);
			tokens.PopExpected(")");
			tokens.PopExpected("{");
			List<List<Expression>> cases = new List<List<Expression>>();
			List<Token> firstTokens = new List<Token>();
			List<List<Executable>> code = new List<List<Executable>>();
			char state = '?'; // ? - first, O - code, A - case
			bool defaultEncountered = false;
			while (!tokens.PopIfPresent("}"))
			{
				if (tokens.IsNext("case"))
				{
					if (defaultEncountered)
					{
						throw new ParserException(tokens.Peek(), "default condition in a switch statement must be the last condition.");
					}

					Token caseToken = tokens.PopExpected("case");
					if (state != 'A')
					{
						cases.Add(new List<Expression>());
						firstTokens.Add(caseToken);
						code.Add(null);
						state = 'A';
					}
					cases[cases.Count - 1].Add(ExpressionParser.Parse(tokens));
					tokens.PopExpected(":");
				}
				else if (tokens.IsNext("default"))
				{
					Token defaultToken = tokens.PopExpected("default");
					if (state != 'A')
					{
						cases.Add(new List<Expression>());
						firstTokens.Add(defaultToken);
						code.Add(null);
						state = 'A';
					}
					cases[cases.Count - 1].Add(null);
					tokens.PopExpected(":");
					defaultEncountered = true;
				}
				else
				{
					if (state != 'O')
					{
						cases.Add(null);
						code.Add(new List<Executable>());
						state = 'O';
					}
					code[code.Count - 1].Add(ExecutableParser.Parse(tokens, false, true, false));
				}
			}

			return new SwitchStatement(switchToken, condition, firstTokens, cases, code, explicitMax, explicitMaxToken);
		}

		private static Executable ParseIf(TokenStream tokens)
		{
			Token ifToken = tokens.PopExpected("if");
			tokens.PopExpected("(");
			Expression condition = ExpressionParser.Parse(tokens);
			tokens.PopExpected(")");
			IList<Executable> body = Parser.ParseBlock(tokens, false);
			IList<Executable> elseBody;
			if (tokens.PopIfPresent("else"))
			{
				elseBody = Parser.ParseBlock(tokens, false);
			}
			else
			{
				elseBody = new Executable[0];
			}
			return new IfStatement(ifToken, condition, body, elseBody);
		}

		private static Executable ParseTry(TokenStream tokens)
		{
			Token tryToken = tokens.PopExpected("try");
			throw new NotImplementedException();
		}

		private static Executable ParseBreak(TokenStream tokens)
		{
			Token breakToken = tokens.PopExpected("break");
			tokens.PopExpected(";");
			return new BreakStatement(breakToken);
		}

		private static Executable ParseContinue(TokenStream tokens)
		{
			Token continueToken = tokens.PopExpected("continue");
			tokens.PopExpected(";");
			return new ContinueStatement(continueToken);
		}

		private static Executable ParseReturn(TokenStream tokens)
		{
			Token returnToken = tokens.PopExpected("return");
			Expression expr = null;
			if (!tokens.PopIfPresent(";"))
			{
				expr = ExpressionParser.Parse(tokens);
				tokens.PopExpected(";");
			}

			return new ReturnStatement(returnToken, expr);
		}
	}
}
