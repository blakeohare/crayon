using System.Collections.Generic;
using Crayon.ParseTree;

namespace Crayon.Translator
{
	internal abstract class AbstractSystemFunctionTranslator
	{
		public AbstractSystemFunctionTranslator() { }

		public AbstractPlatform Platform { get; set; }
		public AbstractTranslator Translator { get; set; }

		protected bool IsMin { get { return this.Platform.IsMin; } }
		
		protected string Shorten(string value)
		{
			if (this.Platform.IsMin) return value.Replace(" ", "");
			return value;
		}

		public void Translate(string tab, List<string> output, SystemFunctionCall functionCall)
		{
			Expression[] args = functionCall.Args;
			string name = functionCall.Name.Substring(1);
			switch (name)
			{
				case "_cos": VerifyCount(functionCall, 1); TranslateCos(output, args[0]); break;
				case "_sin": VerifyCount(functionCall, 1); TranslateSin(output, args[0]); break;
				case "_tan": VerifyCount(functionCall, 1); TranslateTan(output, args[0]); break;
				case "_arc_cos": VerifyCount(functionCall, 1); TranslateArcCos(output, args[0]); break;
				case "_arc_sin": VerifyCount(functionCall, 1); TranslateArcSin(output, args[0]); break;
				case "_arc_tan": VerifyCount(functionCall, 2); TranslateArcTan(output, args[0], args[1]); break;
				case "_array_get": VerifyCount(functionCall, 2); TranslateArrayGet(output, args[0], args[1]); break;
				case "_array_length": VerifyCount(functionCall, 1); TranslateArrayLength(output, args[0]); break;
				case "_array_set": VerifyCount(functionCall, 3); TranslateArraySet(output, args[0], args[1], args[2]); break;
				case "_begin_frame": VerifyCount(functionCall, 0); TranslateBeginFrame(output); break;
				case "_blit_image": VerifyCount(functionCall, 3); TranslateBlitImage(output, args[0], args[1], args[2]); break;
				case "_blit_image_partial": VerifyCount(functionCall, 7); TranslateBlitImagePartial(output, args[0], args[1], args[2], args[3], args[4], args[5], args[6]); break;
				case "_cast": VerifyCount(functionCall, 2); TranslateCast(output, (StringConstant)args[0], args[1]); break;
				case "_cast_to_list": VerifyCount(functionCall, 2); TranslateCastToList(output, (StringConstant)args[0], args[1]); break;
				case "_char_to_string": VerifyCount(functionCall, 1); TranslateCharToString(output, args[0]); break;
				case "_comment": VerifyCount(functionCall, 1); TranslateComment(output, (StringConstant)args[0]); break;
				case "_convert_list_to_array": VerifyCount(functionCall, 2); TranslateConvertListToArray(output, (StringConstant)args[0], args[1]); break;
				case "_current_time_seconds": VerifyCount(functionCall, 0); TranslateCurrentTimeSeconds(output); break;
				case "_dictionary_contains": VerifyCount(functionCall, 2); TranslateDictionaryContains(output, args[0], args[1]); break;
				case "_dictionary_get_guaranteed": VerifyCount(functionCall, 2); TranslateDictionaryGetGuaranteed(output, args[0], args[1]); break;
				case "_dictionary_get_keys": VerifyCount(functionCall, 1); TranslateDictionaryGetKeys(output, args[0]); break;
				case "_dictionary_get_values": VerifyCount(functionCall, 1); TranslateDictionaryGetValues(output, args[0]); break;
				case "_dictionary_remove": VerifyCount(functionCall, 2); TranslateDictionaryRemove(output, args[0], args[1]); break;
				case "_dictionary_set": VerifyCount(functionCall, 3); TranslateDictionarySet(output, args[0], args[1], args[2]); break;
				case "_dictionary_size": VerifyCount(functionCall, 1); TranslateDictionarySize(output, args[0]); break;
				case "_dot_equals": VerifyCount(functionCall, 2); TranslateDotEquals(output, args[0], args[1]); break;
				case "_download_image": VerifyCount(functionCall, 3); TranslateDownloadImage(output, args[0], args[1], ((BooleanConstant)args[2]).Value); break;
				case "_draw_ellipse": VerifyCount(functionCall, 8); TranslateDrawEllipse(output, args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7]); break;
				case "_draw_line": VerifyCount(functionCall, 9); TranslateDrawLine(output, args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8]); break;
				case "_draw_rectangle": VerifyCount(functionCall, 8); TranslateDrawRectangle(output, args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7]); break;
				case "_exponent": VerifyCount(functionCall, 2); TranslateExponent(output, args[0], args[1]); break;
				case "_fill_screen": VerifyCount(functionCall, 3); TranslateFillScreen(output, args[0], args[1], args[2]); break;
				case "_flip_image": VerifyCount(functionCall, 3); TranslateFlipImage(output, args[0], args[1], args[2]); break;
				case "_force_parens": VerifyCount(functionCall, 1); TranslateForceParens(output, args[0]); break;
				case "_get_events_raw_list": VerifyCount(functionCall, 0); TranslateGetEventsRawList(output); break;
				case "_get_program_data": VerifyCount(functionCall, 0); TranslateGetProgramData(output); break;
				case "_get_raw_byte_code_string": VerifyCount(functionCall, 0); TranslateGetRawByteCodeString(output, this.Platform.Context.ByteCodeString); break;
				case "_gl_load_texture": VerifyCount(functionCall, 1); TranslateGlLoadTexture(output, args[0]); break;
				case "_image_error_code": VerifyCount(functionCall, 1); TranslateImageErrorCode(output, args[0]); break;
				case "_image_load_from_user_data": VerifyCount(functionCall, 2); TranslateImageLoadFromUserData(output, args[0], args[1]); break;
				case "_image_loaded": VerifyCount(functionCall, 1); TranslateImageLoaded(output, args[0]); break;
				case "_image_native_bitmap_height": VerifyCount(functionCall, 1); TranslateImageNativeBitmapHeight(output, args[0]); break;
				case "_image_native_bitmap_width": VerifyCount(functionCall, 1); TranslateImageNativeBitmapWidth(output, args[0]); break;
				case "_image_sheet_perform_work_nugget_post_frame": VerifyCount(functionCall, 0); TranslateImageSheetPerformWorkNuggetPostFrame(output); break;
				case "_image_sheet_perform_work_nugget_pre_frame": VerifyCount(functionCall, 0); TranslateImageSheetPerformWorkNuggetPreFrame(output); break;
				case "_initialize_game_with_fps": VerifyCount(functionCall, 1); TranslateInitializeGameWithFps(output, args[0]); break;
				case "_initialize_screen": VerifyCount(functionCall, 4); TranslateInitializeScreen(output, args[0], args[1], args[2], args[3]); break;
				case "_int": VerifyCount(functionCall, 1); TranslateInt(output, args[0]); break;
				case "_is_valid_integer": VerifyCount(functionCall, 1); TranslateIsValidInteger(output, args[0]); break;
				case "_list_clear": VerifyCount(functionCall, 1); TranslateListClear(output, args[0]); break;
				case "_list_concat": VerifyCount(functionCall, 2); TranslateListConcat(output, args[0], args[1]); break;
				case "_list_get": VerifyCount(functionCall, 2); TranslateListGet(output, args[0], args[1]); break;
				case "_list_insert": VerifyCount(functionCall, 3); TranslateListInsert(output, args[0], args[1], args[2]); break;
				case "_list_join": VerifyCount(functionCall, 2); TranslateListJoin(output, args[0], args[1]); break;
				case "_list_join_chars": VerifyCount(functionCall, 1); TranslateListJoinChars(output, args[0]); break;
				case "_list_last_index": VerifyCount(functionCall, 1); TranslateListLastIndex(output, args[0]); break;
				case "_list_length": VerifyCount(functionCall, 1); TranslateListLength(output, args[0]); break;
				case "_list_pop": VerifyCount(functionCall, 1); TranslateListPop(output, args[0]); break;
				case "_list_push": VerifyCount(functionCall, 2); TranslateListPush(output, args[0], args[1]); break;
				case "_list_remove_at": VerifyCount(functionCall, 2); TranslateListRemoveAt(output, args[0], args[1]); break;
				case "_list_reverse_in_place": VerifyCount(functionCall, 1); TranslateListReverseInPlace(output, args[0]); break;
				case "_list_set": VerifyCount(functionCall, 3); TranslateListSet(output, args[0], args[1], args[2]); break;
				case "_list_shuffle_in_place": VerifyCount(functionCall, 1); TranslateListShuffleInPlace(output, args[0]); break;
				case "_multiply_list": VerifyCount(functionCall, 2); TranslateMultiplyList(output, args[0], args[1]); break;
				case "_new_array": VerifyCount(functionCall, 2); TranslateNewArray(output, (StringConstant)args[0], args[1]); break;
				case "_new_dictionary": VerifyCount(functionCall, 2); TranslateNewDictionary(output, (StringConstant)args[0], (StringConstant)args[1]); break;
				case "_new_list": VerifyCount(functionCall, 1); TranslateNewList(output, (StringConstant)args[0]); break;
				case "_new_list_of_size": VerifyCount(functionCall, 2); TranslateNewListOfSize(output, (StringConstant)args[0], args[1]); break;
				case "_new_stack": VerifyCount(functionCall, 1); TranslateNewStack(output, (StringConstant)args[0]); break;
				case "_parse_int": VerifyCount(functionCall, 1); TranslateParseInt(output, args[0]); break;
				case "_parse_json": VerifyCount(functionCall, 1); TranslateParseJson(output, args[0]); break;
				case "_pause_for_frame": VerifyCount(functionCall, 0); TranslatePauseForFrame(output); break;
				case "_print": VerifyCount(functionCall, 1); TranslatePrint(output, args[0]); break;
				case "_random_float": VerifyCount(functionCall, 0); TranslateRandomFloat(output); break;
				case "_read_local_tile_resource": VerifyCount(functionCall, 1); TranslateReadLocalTileResource(output, args[0]); break;
				case "_register_ticker": VerifyCount(functionCall, 0); TranslateRegisterTicker(output); break;
				case "_register_timeout": VerifyCount(functionCall, 0); TranslateRegisterTimeout(output); break;
				case "_resource_read_text_file": VerifyCount(functionCall, 1); TranslateResourceReadText(output, args[0]); break;
				case "_set_program_data": VerifyCount(functionCall, 1); TranslateSetProgramData(output, args[0]); break;
				case "_set_title": VerifyCount(functionCall, 1); TranslateSetTitle(output, args[0]); break;
				case "_sorted_copy_of_int_array": VerifyCount(functionCall, 1); TranslateSortedCopyOfIntArray(output, args[0]); break;
				case "_stack_get": VerifyCount(functionCall, 2); TranslateStackGet(output, args[0], args[1]); break;
				case "_stack_length": VerifyCount(functionCall, 1); TranslateStackLength(output, args[0]); break;
				case "_stack_pop": VerifyCount(functionCall, 1); TranslateStackPop(output, args[0]); break;
				case "_stack_push": VerifyCount(functionCall, 2); TranslateStackPush(output, args[0], args[1]); break;
				case "_stack_set": VerifyCount(functionCall, 3); TranslateStackSet(output, args[0], args[1], args[2]); break;
				case "_string_as_char": VerifyCount(functionCall, 1); TranslateStringAsChar(output, (StringConstant)args[0]); break;
				case "_string_cast_strong": VerifyCount(functionCall, 1); TranslateStringCast(output, args[0], true); break;
				case "_string_cast_weak": VerifyCount(functionCall, 1); TranslateStringCast(output, args[0], false); break;
				case "_string_char_at": VerifyCount(functionCall, 2); TranslateStringCharAt(output, args[0], args[1]); break;
				case "_string_compare": VerifyCount(functionCall, 2); TranslateStringCompare(output, args[0], args[1]); break;
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

		protected abstract void TranslateArcCos(List<string> output, Expression value);
		protected abstract void TranslateArcSin(List<string> output, Expression value);
		protected abstract void TranslateArcTan(List<string> output, Expression dy, Expression dx);
		protected abstract void TranslateArrayGet(List<string> output, Expression list, Expression index);
		protected abstract void TranslateArrayLength(List<string> output, Expression list);
		protected abstract void TranslateArraySet(List<string> output, Expression list, Expression index, Expression value);
		protected abstract void TranslateBeginFrame(List<string> output);
		protected abstract void TranslateBlitImage(List<string> output, Expression image, Expression x, Expression y);
		protected abstract void TranslateBlitImagePartial(List<string> output, Expression image, Expression targetX, Expression targetY, Expression sourceX, Expression sourceY, Expression width, Expression height);
		protected abstract void TranslateCast(List<string> output, StringConstant typeValue, Expression expression);
		protected abstract void TranslateCastToList(List<string> output, StringConstant typeValue, Expression enumerableThing);
		protected abstract void TranslateCharToString(List<string> output, Expression charValue);
		protected abstract void TranslateComment(List<string> output, StringConstant commentValue);
		protected abstract void TranslateConvertListToArray(List<string> output, StringConstant type, Expression list);
		protected abstract void TranslateCos(List<string> output, Expression value);
		protected abstract void TranslateCurrentTimeSeconds(List<string> output);
		protected abstract void TranslateDictionaryContains(List<string> output, Expression dictionary, Expression key);
		protected abstract void TranslateDictionaryGetGuaranteed(List<string> output, Expression dictionary, Expression key);
		protected abstract void TranslateDictionaryGetKeys(List<string> output, Expression dictionary);
		protected abstract void TranslateDictionaryGetValues(List<string> output, Expression dictionary);
		protected abstract void TranslateDictionaryRemove(List<string> output, Expression dictionary, Expression key);
		protected abstract void TranslateDictionarySet(List<string> output, Expression dictionary, Expression key, Expression value);
		protected abstract void TranslateDictionarySize(List<string> output, Expression dictionary);
		protected abstract void TranslateDotEquals(List<string> output, Expression root, Expression compareTo);
		protected abstract void TranslateDownloadImage(List<string> output, Expression key, Expression path, bool isLocalResource);
		protected abstract void TranslateDrawEllipse(List<string> output, Expression left, Expression top, Expression width, Expression height, Expression red, Expression green, Expression blue, Expression alpha);
		protected abstract void TranslateDrawLine(List<string> output, Expression ax, Expression ay, Expression bx, Expression by, Expression lineWidth, Expression red, Expression green, Expression blue, Expression alpha);
		protected abstract void TranslateDrawRectangle(List<string> output, Expression left, Expression top, Expression width, Expression height, Expression red, Expression green, Expression blue, Expression alpha);
		protected abstract void TranslateExponent(List<string> output, Expression baseNum, Expression powerNum);
		protected abstract void TranslateFillScreen(List<string> output, Expression red, Expression green, Expression blue);
		protected abstract void TranslateFlipImage(List<string> output, Expression image, Expression flipX, Expression flipY);
		protected abstract void TranslateForceParens(List<string> output, Expression expression);
		protected abstract void TranslateGetEventsRawList(List<string> output);
		protected abstract void TranslateGetProgramData(List<string> output);
		protected abstract void TranslateGetRawByteCodeString(List<string> output, string theString);
		protected abstract void TranslateGlLoadTexture(List<string> output, Expression platformBitmapResource);
		protected abstract void TranslateImageErrorCode(List<string> output, Expression imageKey);
		protected abstract void TranslateImageLoadFromUserData(List<string> output, Expression imageKey, Expression path);
		protected abstract void TranslateImageLoaded(List<string> output, Expression key);
		protected abstract void TranslateImageNativeBitmapHeight(List<string> output, Expression bitmap);
		protected abstract void TranslateImageNativeBitmapWidth(List<string> output, Expression bitmap);
		protected abstract void TranslateImageSheetPerformWorkNuggetPostFrame(List<string> output);
		protected abstract void TranslateImageSheetPerformWorkNuggetPreFrame(List<string> output);
		protected abstract void TranslateInitializeGameWithFps(List<string> output, Expression fps);
		protected abstract void TranslateInitializeScreen(List<string> output, Expression gameWidth, Expression gameHeight, Expression screenWidth, Expression screenHeight);
		protected abstract void TranslateInt(List<string> output, Expression value);
		protected abstract void TranslateIsValidInteger(List<string> output, Expression number);
		protected abstract void TranslateListClear(List<string> output, Expression list);
		protected abstract void TranslateListConcat(List<string> output, Expression listA, Expression listB);
		protected abstract void TranslateListGet(List<string> output, Expression list, Expression index);
		protected abstract void TranslateListInsert(List<string> output, Expression list, Expression index, Expression value);
		protected abstract void TranslateListJoin(List<string> output, Expression list, Expression sep);
		protected abstract void TranslateListJoinChars(List<string> output, Expression list);
		protected abstract void TranslateListLastIndex(List<string> output, Expression list);
		protected abstract void TranslateListLength(List<string> output, Expression list);
		protected abstract void TranslateListPop(List<string> output, Expression list);
		protected abstract void TranslateListPush(List<string> output, Expression list, Expression value);
		protected abstract void TranslateListRemoveAt(List<string> output, Expression list, Expression index);
		protected abstract void TranslateListReverseInPlace(List<string> output, Expression list);
		protected abstract void TranslateListSet(List<string> output, Expression list, Expression index, Expression value);
		protected abstract void TranslateListShuffleInPlace(List<string> output, Expression list);
		protected abstract void TranslateMultiplyList(List<string> output, Expression list, Expression num);
		protected abstract void TranslateNewArray(List<string> output, StringConstant type, Expression size);
		protected abstract void TranslateNewDictionary(List<string> output, StringConstant keyType, StringConstant valueType);
		protected abstract void TranslateNewList(List<string> output, StringConstant type);
		protected abstract void TranslateNewListOfSize(List<string> output, StringConstant type, Expression length);
		protected abstract void TranslateNewStack(List<string> output, StringConstant type);
		protected abstract void TranslateParseInt(List<string> output, Expression rawString);
		protected abstract void TranslateParseJson(List<string> output, Expression rawString);
		protected abstract void TranslatePauseForFrame(List<string> output);
		protected abstract void TranslatePrint(List<string> output, Expression message);
		protected abstract void TranslateRandomFloat(List<string> output);
		protected abstract void TranslateReadLocalTileResource(List<string> output, Expression tileGenName);
		protected abstract void TranslateRegisterTicker(List<string> output);
		protected abstract void TranslateRegisterTimeout(List<string> output);
		protected abstract void TranslateResourceReadText(List<string> output, Expression path);
		protected abstract void TranslateSetProgramData(List<string> output, Expression programData);
		protected abstract void TranslateSetTitle(List<string> output, Expression title);
		protected abstract void TranslateSin(List<string> output, Expression value);
		protected abstract void TranslateSortedCopyOfIntArray(List<string> output, Expression list);
		protected abstract void TranslateStackGet(List<string> output, Expression stack, Expression index);
		protected abstract void TranslateStackLength(List<string> output, Expression stack);
		protected abstract void TranslateStackPop(List<string> output, Expression stack);
		protected abstract void TranslateStackPush(List<string> output, Expression stack, Expression value);
		protected abstract void TranslateStackSet(List<string> output, Expression stack, Expression index, Expression value);
		protected abstract void TranslateStringAsChar(List<string> output, StringConstant stringConstant);
		protected abstract void TranslateStringCast(List<string> output, Expression thing, bool strongCast);
		protected abstract void TranslateStringCharAt(List<string> output, Expression stringValue, Expression index);
		protected abstract void TranslateStringCompare(List<string> output, Expression a, Expression b);
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
		protected abstract void TranslateTan(List<string> output, Expression value);
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
