using System.Collections.Generic;
using Crayon.ParseTree;

namespace Crayon.Translator
{
	internal abstract class AbstractSystemFunctionTranslator
	{
		public AbstractSystemFunctionTranslator(AbstractPlatformImplementation platform)
		{
			this.Platform = platform;
		}

		internal AbstractPlatformImplementation Platform { get; private set; }

		internal AbstractTranslator Translator { get; set; }

		public bool IsMin { get { return this.Translator.IsMin; } }

		protected string Shorten(string value)
		{
			if (this.IsMin) return value.Replace(" ", "");
			return value;
		}

		public void Translate(string tab, List<string> output, SystemFunctionCall functionCall)
		{
			Expression[] args = functionCall.Args;
			string name = functionCall.Name.Substring(1);
			switch (name)
			{
				case "_insert_platform_code": VerifyCount(functionCall, 1); TranslateInsertFrameworkCode(tab, output, ((StringConstant)args[0]).Value); break;

				case "_begin_frame": VerifyCount(functionCall, 0); TranslateBeginFrame(output); break;
				case "_comment": VerifyCount(functionCall, 1); TranslateComment(output, args[0]); break;
				case "_dictionary_contains": VerifyCount(functionCall, 2); TranslateDictionaryContains(output, args[0], args[1]); break;
				case "_dictionary_get": VerifyCount(functionCall, 3); TranslateDictionaryGet(output, args[0], args[1], args[2]); break;
				case "_dictionary_get_keys": VerifyCount(functionCall, 1); TranslateDictionaryGetKeys(output, args[0]); break;
				case "_dictionary_get_values": VerifyCount(functionCall, 1); TranslateDictionaryGetValues(output, args[0]); break;
				case "_dictionary_remove": VerifyCount(functionCall, 2); TranslateDictionaryRemove(output, args[0], args[1]); break;
				case "_dictionary_set": VerifyCount(functionCall, 3); TranslateDictionarySet(output, args[0], args[1], args[2]); break;
				case "_dictionary_size": VerifyCount(functionCall, 1); TranslateDictionarySize(output, args[0]); break;
				case "_exponent": VerifyCount(functionCall, 2); TranslateExponent(output, args[0], args[1]); break;
				case "_int": VerifyCount(functionCall, 1); TranslateInt(output, args[0]); break;
				case "_list_concat": VerifyCount(functionCall, 2); TranslateListConcat(output, args[0], args[1]); break;
				case "_list_get": VerifyCount(functionCall, 2); TranslateListGet(output, args[0], args[1]); break;
				case "_list_insert": VerifyCount(functionCall, 3); TranslateListInsert(output, args[0], args[1], args[2]); break;
				case "_list_join": VerifyCount(functionCall, 2); TranslateListJoin(output, args[0], args[1]); break;
				case "_list_last_index": VerifyCount(functionCall, 1); TranslateListLastIndex(output, args[0]); break;
				case "_list_length": VerifyCount(functionCall, 1); TranslateListLength(output, args[0]); break;
				case "_list_new": VerifyCount(functionCall, 1); TranslateListNew(output, args[0]); break;
				case "_list_pop": VerifyCount(functionCall, 1); TranslateListPop(output, args[0]); break;
				case "_list_push": VerifyCount(functionCall, 2); TranslateListPush(output, args[0], args[1]); break;
				case "_list_remove_at": VerifyCount(functionCall, 2); TranslateListRemoveAt(output, args[0], args[1]); break;
				case "_list_reverse_in_place": VerifyCount(functionCall, 1); TranslateListReverseInPlace(output, args[0]); break;
				case "_list_set": VerifyCount(functionCall, 3); TranslateListSet(output, args[0], args[1], args[2]); break;
				case "_list_shuffle_in_place": VerifyCount(functionCall, 1); TranslateListShuffleInPlace(output, args[0]); break;
				case "_pause_for_frame": VerifyCount(functionCall, 0); TranslatePauseForFrame(output); break;
				case "_print": VerifyCount(functionCall, 1); TranslatePrint(output, args[0]); break;
				case "_register_ticker": VerifyCount(functionCall, 0); TranslateRegisterTicker(output); break;
				case "_register_timeout": VerifyCount(functionCall, 0); TranslateRegisterTimeout(output); break;
				case "_string_cast_strong": VerifyCount(functionCall, 1); TranslateStringCast(output, args[0], true); break;
				case "_string_cast_weak": VerifyCount(functionCall, 1); TranslateStringCast(output, args[0], false); break;
				case "_string_char_at": VerifyCount(functionCall, 2); TranslateStringCharAt(output, args[0], args[1]); break;
				case "_string_contains": VerifyCount(functionCall, 2); TranslateStringContains(output, args[0], args[1]); break;
				case "_string_endswith": VerifyCount(functionCall, 2); TranslateStringEndsWith(output, args[0], args[1]); break;
				case "_string_from_code": VerifyCount(functionCall, 1); TranslateStringFromCode(output, args[0]); break;
				case "_string_index_of": VerifyCount(functionCall, 2); TranslateStringIndexOf(output, args[0], args[1]); break;
				case "_string_length": VerifyCount(functionCall, 1); TranslateStringLength(output, args[0]); break;
				case "_string_lower": VerifyCount(functionCall, 1); TranslateStringLower(output, args[0]); break;
				case "_string_parse_float": VerifyCount(functionCall, 1); TranslateStringParseFloat(output, args[0]); break;
				case "_string_parse_int": VerifyCount(functionCall, 1); TranslateStringParseInt(output, args[0]); break;
				case "_string_reverse": VerifyCount(functionCall, 1); TranslateStringReverse(output, args[0]); break;
				case "_string_replace": VerifyCount(functionCall, 3); TranslateStringReplace(output, args[0], args[1], args[2]); break;
				case "_string_split": VerifyCount(functionCall, 2); TranslateStringSplit(output, args[0], args[1]); break;
				case "_string_startswith": VerifyCount(functionCall, 2); TranslateStringStartsWith(output, args[0], args[1]); break;
				case "_string_trim": VerifyCount(functionCall, 1); TranslateStringTrim(output, args[0]); break;
				case "_string_upper": VerifyCount(functionCall, 1); TranslateStringUpper(output, args[0]); break;
				case "_unregister_ticker": VerifyCount(functionCall, 0); TranslateUnregisterTicker(output); break;
				case "_unsafe_float_division": VerifyCount(functionCall, 2); TranslateUnsafeFloatDivision(output, args[0], args[1]); break;
				case "_unsafe_integer_division": VerifyCount(functionCall, 2); TranslateUnsafeIntegerDivision(output, args[0], args[1]); break;
				default: throw new ParserException(functionCall.FirstToken, "Unrecognized system method invocation: " + functionCall.Name);
			}
		}

		protected abstract void TranslateBeginFrame(List<string> output);
		protected abstract void TranslateComment(List<string> output, Expression commentValue);
		protected abstract void TranslateDictionaryContains(List<string> output, Expression dictionary, Expression key);
		protected abstract void TranslateDictionaryGet(List<string> output, Expression dictionary, Expression key, Expression defaultValue);
		protected abstract void TranslateDictionaryGetKeys(List<string> output, Expression dictionary);
		protected abstract void TranslateDictionaryGetValues(List<string> output, Expression dictionary);
		protected abstract void TranslateDictionaryRemove(List<string> output, Expression dictionary, Expression key);
		protected abstract void TranslateDictionarySet(List<string> output, Expression dict, Expression key, Expression value);
		protected abstract void TranslateDictionarySize(List<string> output, Expression dictionary);
		protected abstract void TranslateExponent(List<string> output, Expression baseNum, Expression powerNum);
		protected abstract void TranslateInsertFrameworkCode(string tab, List<string> output, string id);
		protected abstract void TranslateInt(List<string> output, Expression value);
		protected abstract void TranslateListConcat(List<string> output, Expression listA, Expression listB);
		protected abstract void TranslateListGet(List<string> output, Expression list, Expression index);
		protected abstract void TranslateListInsert(List<string> output, Expression list, Expression index, Expression value);
		protected abstract void TranslateListJoin(List<string> output, Expression list, Expression sep);
		protected abstract void TranslateListLastIndex(List<string> output, Expression list);
		protected abstract void TranslateListLength(List<string> output, Expression list);
		protected abstract void TranslateListNew(List<string> output, Expression length);
		protected abstract void TranslateListPop(List<string> output, Expression list);
		protected abstract void TranslateListPush(List<string> output, Expression list, Expression value);
		protected abstract void TranslateListRemoveAt(List<string> output, Expression list, Expression index);
		protected abstract void TranslateListReverseInPlace(List<string> output, Expression listVar);
		protected abstract void TranslateListSet(List<string> output, Expression list, Expression index, Expression value);
		protected abstract void TranslateListShuffleInPlace(List<string> output, Expression list);
		protected abstract void TranslatePauseForFrame(List<string> output);
		protected abstract void TranslatePrint(List<string> output, Expression message);
		protected abstract void TranslateRegisterTicker(List<string> output);
		protected abstract void TranslateRegisterTimeout(List<string> output);
		protected abstract void TranslateStringCast(List<string> output, Expression thing, bool strongCast);
		protected abstract void TranslateStringCharAt(List<string> output, Expression stringValue, Expression index);
		protected abstract void TranslateStringContains(List<string> output, Expression haystack, Expression needle);
		protected abstract void TranslateStringEndsWith(List<string> output, Expression stringExpr, Expression findMe);
		protected abstract void TranslateStringFromCode(List<string> output, Expression characterCode);
		protected abstract void TranslateStringIndexOf(List<string> output, ParseTree.Expression haystack, ParseTree.Expression needle);
		protected abstract void TranslateStringLength(List<string> output, Expression stringValue);
		protected abstract void TranslateStringLower(List<string> output, Expression stringValue);
		protected abstract void TranslateStringParseFloat(List<string> output, Expression stringValue);
		protected abstract void TranslateStringParseInt(List<string> output, Expression value);
		protected abstract void TranslateStringReplace(List<string> output, Expression stringValue, Expression findMe, Expression replaceWith);
		protected abstract void TranslateStringReverse(List<string> output, Expression stringValue);
		protected abstract void TranslateStringSplit(List<string> output, Expression stringExpr, Expression sep);
		protected abstract void TranslateStringStartsWith(List<string> output, Expression stringExpr, Expression findMe);
		protected abstract void TranslateStringTrim(List<string> output, Expression stringValue);
		protected abstract void TranslateStringUpper(List<string> output, Expression stringValue);
		protected abstract void TranslateUnregisterTicker(List<string> output);
		protected abstract void TranslateUnsafeFloatDivision(List<string> output, Expression numerator, Expression denominator);
		protected abstract void TranslateUnsafeIntegerDivision(List<string> output, Expression numerator, Expression denominator);

		private void VerifyCount(SystemFunctionCall functionCall, int argCount)
		{
			if (functionCall.Args.Length != argCount)
			{
				throw new ParserException(functionCall.FirstToken, "Wrong number of args. Expected: " + argCount);
			}
		}
	}
}
