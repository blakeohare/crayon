using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crayon.ParseTree;

namespace Crayon
{
	internal static class FrameworkFunctionUtil
	{
		private static readonly HashSet<string> THESE_MAKE_BOOLEANS = new HashSet<string>("== != >= <= < >".Split(' '));
		
		public static readonly Dictionary<string, FrameworkFunction> FF_LOOKUP;
		static FrameworkFunctionUtil()
		{
			Dictionary<string, FrameworkFunction> ffLookup = new Dictionary<string, FrameworkFunction>();
			foreach (object name in Enum.GetValues(typeof(FrameworkFunction)))
			{
				FrameworkFunction ff = (FrameworkFunction)name;
				ffLookup[ff.ToString().ToLowerInvariant()] = ff;
			}
			FF_LOOKUP = ffLookup;
		}

		// TODO: move this in the resolver
		public static void VerifyArgsAsMuchAsPossible(Token throwToken, FrameworkFunction frameworkFunction, Expression[] args)
		{
			Types[] argTypes = BuildKnownTypeList(args);
			switch (frameworkFunction)
			{
				case FrameworkFunction.ABS:
					VerifyLength(throwToken, frameworkFunction, 1, args);
					VerifyTypes(throwToken, frameworkFunction, args, argTypes, "NUMBER");
					break;
				case FrameworkFunction.ARCTAN2:
					VerifyLength(throwToken, frameworkFunction, 2, args);
					VerifyTypes(throwToken, frameworkFunction, args, argTypes, "NUMBER NUMBER");
					break;
				case FrameworkFunction.ASSERT:
					VerifyLength(throwToken, frameworkFunction, 2, args);
					VerifyTypes(throwToken, frameworkFunction, args, argTypes, "BOOLEAN STRING");
					break;
				case FrameworkFunction.CHR:
					VerifyLength(throwToken, frameworkFunction, 1, args);
					VerifyTypes(throwToken, frameworkFunction, args, argTypes, "INTEGER");
					break;
				case FrameworkFunction.COS:
					VerifyLength(throwToken, frameworkFunction, 1, args);
					VerifyTypes(throwToken, frameworkFunction, args, argTypes, "NUMBER");
					break;
				case FrameworkFunction.CURRENT_TIME:
					VerifyLength(throwToken, frameworkFunction, 0, args);
					break;
				case FrameworkFunction.FLOOR:
					VerifyLength(throwToken, frameworkFunction, 1, args);
					VerifyTypes(throwToken, frameworkFunction, args, argTypes, "NUMBER");
					break;
				case FrameworkFunction.GAME_CLOCK_TICK:
					VerifyLength(throwToken, frameworkFunction, 0, args);
					break;
				case FrameworkFunction.GAME_INITIALIZE:
					VerifyLength(throwToken, frameworkFunction, 1, args);
					VerifyTypes(throwToken, frameworkFunction, args, argTypes, "INTEGER");
					break;
				case FrameworkFunction.GAME_INITIALIZE_SCREEN:
					VerifyLength(throwToken, frameworkFunction, 2, args);
					VerifyTypes(throwToken, frameworkFunction, args, argTypes, "INTEGER INTEGER");
					break;
				case FrameworkFunction.GAME_INITIALIZE_SCREEN_SCALED:
					VerifyLength(throwToken, frameworkFunction, 4, args);
					VerifyTypes(throwToken, frameworkFunction, args, argTypes, "INTEGER INTEGER INTEGER INTEGER");
					break;
				case FrameworkFunction.GAME_PUMP_EVENTS:
					VerifyLength(throwToken, frameworkFunction, 0, args);
					break;
				case FrameworkFunction.GAME_SET_TITLE:
					VerifyLength(throwToken, frameworkFunction, 1, args);
					VerifyTypes(throwToken, frameworkFunction, args, argTypes, "STRING");
					break;
				case FrameworkFunction.GFX_BLIT_IMAGE:
					VerifyLength(throwToken, frameworkFunction, 3, args);
					VerifyTypes(throwToken, frameworkFunction, args, argTypes, "INSTANCE INTEGER INTEGER");
					break;
				case FrameworkFunction.GFX_BLIT_IMAGE_PARTIAL:
					VerifyLength(throwToken, frameworkFunction, 7, args);
					VerifyTypes(throwToken, frameworkFunction, args, argTypes, "INSTANCE INTEGER INTEGER INTEGER INTEGER INTEGER INTEGER");
					break;
				case FrameworkFunction.GFX_BLIT_IMAGE_PARTIAL_AT_SCALE:
					VerifyLength(throwToken, frameworkFunction, 9, args);
					VerifyTypes(throwToken, frameworkFunction, args, argTypes, "INSTANCE INTEGER INTEGER INTEGER INTEGER INTEGER INTEGER INTEGER INTEGER");
					break;
				case FrameworkFunction.GFX_DRAW_ELLIPSE:
					VerifyLength(throwToken, frameworkFunction, 8, args);
					VerifyTypes(throwToken, frameworkFunction, args, argTypes, "INTEGER INTEGER INTEGER INTEGER INTEGER INTEGER INTEGER INTEGER");
					break;
				case FrameworkFunction.GFX_DRAW_LINE:
					VerifyLength(throwToken, frameworkFunction, 9, args);
					VerifyTypes(throwToken, frameworkFunction, args, argTypes, "INTEGER INTEGER INTEGER INTEGER INTEGER INTEGER INTEGER INTEGER INTEGER");
					break;
				case FrameworkFunction.GFX_DRAW_RECTANGLE:
					VerifyLength(throwToken, frameworkFunction, 8, args);
					VerifyTypes(throwToken, frameworkFunction, args, argTypes, "INTEGER INTEGER INTEGER INTEGER INTEGER INTEGER INTEGER INTEGER");
					break;
				case FrameworkFunction.GFX_FILL_SCREEN:
					VerifyLength(throwToken, frameworkFunction, 3, args);
					VerifyTypes(throwToken, frameworkFunction, args, argTypes, "INTEGER INTEGER INTEGER");
					break;
				case FrameworkFunction.GFX_FLIP_IMAGE:
					VerifyLength(throwToken, frameworkFunction, 3, args);
					VerifyTypes(throwToken, frameworkFunction, args, argTypes, "INSTANCE BOOLEAN BOOLEAN");
					break;
				case FrameworkFunction.GAMEPAD_GET_DEVICES:
					VerifyLength(throwToken, frameworkFunction, 0, args);
					break;
				case FrameworkFunction.GFX_IMAGE_GET:
					VerifyLength(throwToken, frameworkFunction, 1, args);
					VerifyTypes(throwToken, frameworkFunction, args, argTypes, "STRING");
					break;
				case FrameworkFunction.GFX_IMAGE_LOAD_FROM_RESOURCE:
					VerifyLength(throwToken, frameworkFunction, 2, args);
					VerifyTypes(throwToken, frameworkFunction, args, argTypes, "STRING STRING");
					break;
				case FrameworkFunction.GFX_IMAGE_POP_SCALE:
					VerifyLength(throwToken, frameworkFunction, 0, args);
					break;
				case FrameworkFunction.GFX_IMAGE_PUSH_SCALE:
					VerifyLength(throwToken, frameworkFunction, 1, args);
					VerifyTypes(throwToken, frameworkFunction, args, argTypes, "INTEGER");
					break;
				case FrameworkFunction.GFX_IMAGE_SHEET_LOAD:
					VerifyLength(throwToken, frameworkFunction, 1, args);
					break;
				case FrameworkFunction.GFX_IMAGE_SHEET_LOAD_PROGRESS:
					VerifyLength(throwToken, frameworkFunction, 1, args);
					break;
				case FrameworkFunction.GFX_IMAGE_SHEET_LOADED:
					VerifyLength(throwToken, frameworkFunction, 1, args);
					break;
				case FrameworkFunction.GFX_IS_IMAGE_LOADED:
					VerifyLength(throwToken, frameworkFunction, 1, args);
					VerifyTypes(throwToken, frameworkFunction, args, argTypes, "STRING");
					break;
				case FrameworkFunction.HTTP_REQUEST:
					VerifyLength(throwToken, frameworkFunction, 4, args);
					VerifyTypes(throwToken, frameworkFunction, args, argTypes, "STRING STRING DICTIONARY? STRING?");
					break;
				case FrameworkFunction.IO_CURRENT_DIRECTORY:
					VerifyLength(throwToken, frameworkFunction, 0, args);
					break;
				case FrameworkFunction.IO_DIRECTORY_LISTING:
					VerifyLength(throwToken, frameworkFunction, 1, args);
					VerifyTypes(throwToken, frameworkFunction, args, argTypes, "STRING");
					break;
				case FrameworkFunction.IO_FILE_READ_TEXT:
					VerifyLength(throwToken, frameworkFunction, 1, args);
					VerifyTypes(throwToken, frameworkFunction, args, argTypes, "STRING");
					break;
				case FrameworkFunction.IO_FILE_WRITE_TEXT:
					VerifyLength(throwToken, frameworkFunction, 2, args);
					VerifyTypes(throwToken, frameworkFunction, args, argTypes, "STRING STRING");
					break;
				case FrameworkFunction.IO_IS_PATH_DIRECTORY:
					VerifyLength(throwToken, frameworkFunction, 1, args);
					VerifyTypes(throwToken, frameworkFunction, args, argTypes, "STRING");
					break;
				case FrameworkFunction.IO_PATH_EXISTS:
					VerifyLength(throwToken, frameworkFunction, 1, args);
					VerifyTypes(throwToken, frameworkFunction, args, argTypes, "STRING");
					break;
				case FrameworkFunction.IO_PATH_JOIN:
					VerifyLength(throwToken, frameworkFunction, 1, args);
					VerifyTypes(throwToken, frameworkFunction, args, argTypes, "LIST");
					break;
				case FrameworkFunction.ORD:
					VerifyLength(throwToken, frameworkFunction, 1, args);
					VerifyTypes(throwToken, frameworkFunction, args, argTypes, "STRING");
					break;
				case FrameworkFunction.PARSE_FLOAT:
					VerifyLength(throwToken, frameworkFunction, 1, args);
					VerifyTypes(throwToken, frameworkFunction, args, argTypes, "STRING");
					break;
				case FrameworkFunction.PARSE_INT:
					VerifyLength(throwToken, frameworkFunction, 1, args);
					VerifyTypes(throwToken, frameworkFunction, args, argTypes, "STRING");
					break;
				case FrameworkFunction.PARSE_JSON:
					VerifyLength(throwToken, frameworkFunction, 1, args);
					VerifyTypes(throwToken, frameworkFunction, args, argTypes, "STRING");
					break;
				case FrameworkFunction.PRINT:
					// TODO: allow print to take multiple arguments.
					VerifyLength(throwToken, frameworkFunction, 1, args);
					break;
				case FrameworkFunction.RANDOM:
					VerifyLength(throwToken, frameworkFunction, 0, args);
					break;
				case FrameworkFunction.RESOURCE_READ_TEXT:
					VerifyLength(throwToken, frameworkFunction, 1, args);
					break;
				case FrameworkFunction.SFX_GET_SOUND:
					VerifyLength(throwToken, frameworkFunction, 1, args);
					VerifyTypes(throwToken, frameworkFunction, args, argTypes, "STRING");
					break;
				case FrameworkFunction.SFX_IS_SOUND_LOADED:
					VerifyLength(throwToken, frameworkFunction, 1, args);
					VerifyTypes(throwToken, frameworkFunction, args, argTypes, "STRING");
					break;
				case FrameworkFunction.SFX_LOAD_SOUND_FROM_RESOURCE:
					VerifyLength(throwToken, frameworkFunction, 2, args);
					VerifyTypes(throwToken, frameworkFunction, args, argTypes, "STRING STRING");
					break;
				case FrameworkFunction.SFX_PLAY_SOUND:
					VerifyLength(throwToken, frameworkFunction, 1, args);
					VerifyTypes(throwToken, frameworkFunction, args, argTypes, "INSTANCE");
					break;
				case FrameworkFunction.SIN:
					VerifyLength(throwToken, frameworkFunction, 1, args);
					VerifyTypes(throwToken, frameworkFunction, args, argTypes, "NUMBER");
					break;
				case FrameworkFunction.TYPEOF:
					VerifyLength(throwToken, frameworkFunction, 1, args);
					break;
				case FrameworkFunction.USER_DATA_DELETE_DIRECTORY:
					VerifyLength(throwToken, frameworkFunction, 2, args);
					VerifyTypes(throwToken, frameworkFunction, args, argTypes, "STRING BOOLEAN");
					break;
				case FrameworkFunction.USER_DATA_DELETE_FILE:
					VerifyLength(throwToken, frameworkFunction, 1, args);
					VerifyTypes(throwToken, frameworkFunction, args, argTypes, "STRING");
					break;
				case FrameworkFunction.USER_DATA_DIRECTORY_LISTING:
					VerifyLength(throwToken, frameworkFunction, 1, args);
					VerifyTypes(throwToken, frameworkFunction, args, argTypes, "STRING");
					break;
				case FrameworkFunction.USER_DATA_FILE_READ_TEXT:
					VerifyLength(throwToken, frameworkFunction, 1, args);
					VerifyTypes(throwToken, frameworkFunction, args, argTypes, "STRING");
					break;
				case FrameworkFunction.USER_DATA_FILE_WRITE_TEXT:
					VerifyLength(throwToken, frameworkFunction, 2, args);
					VerifyTypes(throwToken, frameworkFunction, args, argTypes, "STRING STRING");
					break;
				case FrameworkFunction.USER_DATA_IS_PATH_DIRECTORY:
					VerifyLength(throwToken, frameworkFunction, 1, args);
					VerifyTypes(throwToken, frameworkFunction, args, argTypes, "STRING");
					break;
				case FrameworkFunction.USER_DATA_MAKE_DIRECTORY:
					VerifyLength(throwToken, frameworkFunction, 1, args);
					VerifyTypes(throwToken, frameworkFunction, args, argTypes, "STRING");
					break;
				case FrameworkFunction.USER_DATA_PATH_EXISTS:
					VerifyLength(throwToken, frameworkFunction, 1, args);
					VerifyTypes(throwToken, frameworkFunction, args, argTypes, "STRING");
					break;
				default: 
					throw new NotImplementedException("Implement arg check for " + frameworkFunction);
			}
		}

		private static void VerifyLength(Token token, FrameworkFunction ff, int expected, Expression[] args)
		{
			if (args.Length != expected)
			{
				throw new ParserException(token, "Incorrect argument length for " + ff.ToString().ToLower() + ". " + expected + " arg" + (expected == 1 ? "" : "s") + " expected. Found " + args.Length + ".");
			}
		}

		private static void VerifyTypes(Token throwToken, FrameworkFunction ff, Expression[] args, Types[] argTypes, string encodedTypes)
		{
			string[] types = encodedTypes.Split(' ');
#if DEBUG
			if (types.Length != args.Length) throw new Exception("Mismatch. VerifyLength is verifying the wrong amount or bad encodedTypes args.");
#endif
			for (int i = 0; i < args.Length; ++i)
			{
				Expression arg = args[i];
				Types argType = argTypes[i];
				Token token = arg.FirstToken;
				string type = types[i];
				bool nullable = false;
				if (type.EndsWith("?"))
				{
					type = type.Substring(0, type.Length - 1);
					nullable = true;
				}
				if (argType == Types.NULL && nullable) continue;
				if (type == "OBJECT") continue;
				if (argType == Types.NATIVE_OBJECT) continue;
				if (type == "NUMBER" && (argType == Types.INTEGER || argType == Types.FLOAT)) continue;
				if (type == argType.ToString()) continue;
				throw new ParserException(throwToken, "Invalid type for argument #" + (i + 1) + " of " + ff.ToString().ToLower() + ". Expected " + type.ToLower() + " but found " + argType.ToString().ToLower());
			}
		}

		private static Types[] BuildKnownTypeList(IList<Expression> expressionList)
		{
			// NATIVE_OBJECT is repurposed here to indicate UNKNOWN
			List<Types> types = new List<Types>();
			foreach (Expression expr in expressionList)
			{
				if (expr.IsLiteral)
				{
					if (expr is IntegerConstant) types.Add(Types.INTEGER);
					else if (expr is FloatConstant) types.Add(Types.FLOAT);
					else if (expr is StringConstant) types.Add(Types.STRING);
					else if (expr is NullConstant) types.Add(Types.NULL);
					else if (expr is BooleanConstant) types.Add(Types.BOOLEAN);
					else
					{
						throw new Exception("Did I miss one?");
					}
				}
				else if (expr is Instantiate)
				{
					types.Add(Types.INSTANCE);
				}
				else
				{
					if (expr is BooleanCombination || expr is BooleanNot)
					{
						types.Add(Types.BOOLEAN);
					}
					else if (expr is BinaryOpChain)
					{
						BinaryOpChain chain = (BinaryOpChain)expr;
						if (THESE_MAKE_BOOLEANS.Contains(chain.Op.Value))
						{
							types.Add(Types.BOOLEAN);
						}
						else
						{
							types.Add(Types.NATIVE_OBJECT);
						}
					}
					else
					{
						types.Add(Types.NATIVE_OBJECT);
					}
				}
			}
			return types.ToArray();
		}
	}
}
