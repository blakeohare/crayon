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
            string fullName = functionCall.Name;
            string name = fullName.Substring(1);

            if (name.StartsWith("_lib_"))
            {
                output.Add(functionCall.AssociatedLibrary.TranslateNativeInvocation(this.Platform, fullName, args));
                return;
            }

            switch (name)
            {
                // TODO: migrate this to the HTTP library
                case "_http_request": VerifyCount(functionCall, 9); TranslateHttpRequest(output, args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8]); break;

                // TODO: migrate these to the FileIO library
                case "_io_create_directory": VerifyCount(functionCall, 1); TranslateIoCreateDirectory(output, args[0]); break;
                case "_io_current_directory": VerifyCount(functionCall, 0); TranslateIoCurrentDirectory(output); break;
                case "_io_delete_directory": VerifyCount(functionCall, 2); TranslateIoDeleteDirectory(output, args[0], args[1]); break;
                case "_io_delete_file": VerifyCount(functionCall, 2); TranslateIoDeleteFile(output, args[0], args[1]); break;
                case "_io_does_path_exist": VerifyCount(functionCall, 4); TranslateIoDoesPathExist(output, args[0], args[1], args[2], args[3]); break;
                case "_io_file_read_text": VerifyCount(functionCall, 2); TranslateIoFileReadText(output, args[0], args[1]); break;
                case "_io_files_in_directory": VerifyCount(functionCall, 2); TranslateIoFilesInDirectory(output, args[0], args[1]); break;
                case "_io_file_write_text": VerifyCount(functionCall, 3); TranslateIoFileWriteText(output, args[0], args[1], args[2]); break;
                case "_is_windows_program": VerifyCount(functionCall, 0); TranslateIsWindowsProgram(output); break;

                // TODO: to the JSON library
                case "_parse_json": VerifyCount(functionCall, 1); TranslateParseJson(output, args[0]); break;

                // TODO: redo this too. preferably when you refactor the HTTP stuff that depends on this
                case "_async_message_queue_pump": VerifyCount(functionCall, 0); TranslateAsyncMessageQueuePump(output); break;

                case "_app_data_root": VerifyCount(functionCall, 0); TranslateAppDataRoot(output); break;
                case "_array_get": VerifyCount(functionCall, 2); TranslateArrayGet(output, args[0], args[1]); break;
                case "_array_length": VerifyCount(functionCall, 1); TranslateArrayLength(output, args[0]); break;
                case "_array_set": VerifyCount(functionCall, 3); TranslateArraySet(output, args[0], args[1], args[2]); break;
                case "_assert": VerifyCount(functionCall, 1); TranslateAssert(output, args[0]); break;
                case "_begin_frame": VerifyCount(functionCall, 0); TranslateBeginFrame(output); break;
                case "_byte_code_get_int_args": VerifyCount(functionCall, 0); TranslateByteCodeGetIntArgs(output); break;
                case "_byte_code_get_ops": VerifyCount(functionCall, 0); TranslateByteCodeGetOps(output); break;
                case "_byte_code_get_string_args": VerifyCount(functionCall, 0); TranslateByteCodeGetStringArgs(output); break;
                case "_byte_code_get_raw_string": VerifyCount(functionCall, 0); TranslateGetRawByteCodeString(output); break;
                case "_cast": VerifyCount(functionCall, 2); TranslateCast(output, (StringConstant)args[0], args[1]); break;
                case "_cast_to_list": VerifyCount(functionCall, 2); TranslateCastToList(output, (StringConstant)args[0], args[1]); break;
                case "_char_to_string": VerifyCount(functionCall, 1); TranslateCharToString(output, args[0]); break;
                case "_chr": VerifyCount(functionCall, 1); TranslateChr(output, args[0]); break;
                case "_comment": VerifyCount(functionCall, 1); TranslateComment(output, (StringConstant)args[0]); break;
                case "_convert_list_to_array": VerifyCount(functionCall, 2); TranslateConvertListToArray(output, (StringConstant)args[0], args[1]); break;
                case "_current_time_seconds": VerifyCount(functionCall, 0); TranslateCurrentTimeSeconds(output); break;
                case "_dictionary_contains": VerifyCount(functionCall, 2); TranslateDictionaryContains(output, args[0], args[1]); break;
                case "_dictionary_get_guaranteed": VerifyCount(functionCall, 2); TranslateDictionaryGetGuaranteed(output, args[0], args[1]); break;
                case "_dictionary_get_keys": VerifyCount(functionCall, 2); TranslateDictionaryGetKeys(output, ((StringConstant)args[0]).Value, args[1]); break;
                case "_dictionary_get_values": VerifyCount(functionCall, 1); TranslateDictionaryGetValues(output, args[0]); break;
                case "_dictionary_remove": VerifyCount(functionCall, 2); TranslateDictionaryRemove(output, args[0], args[1]); break;
                case "_dictionary_set": VerifyCount(functionCall, 3); TranslateDictionarySet(output, args[0], args[1], args[2]); break;
                case "_dictionary_size": VerifyCount(functionCall, 1); TranslateDictionarySize(output, args[0]); break;
                case "_dot_equals": VerifyCount(functionCall, 2); TranslateDotEquals(output, args[0], args[1]); break;
                case "_force_parens": VerifyCount(functionCall, 1); TranslateForceParens(output, args[0]); break;
                case "_get_program_data": VerifyCount(functionCall, 0); TranslateGetProgramData(output); break;
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
                case "_math_arc_cos": VerifyCount(functionCall, 1); TranslateArcCos(output, args[0]); break;
                case "_math_arc_sin": VerifyCount(functionCall, 1); TranslateArcSin(output, args[0]); break;
                case "_math_arc_tan": VerifyCount(functionCall, 2); TranslateArcTan(output, args[0], args[1]); break;
                case "_math_cos": VerifyCount(functionCall, 1); TranslateCos(output, args[0]); break;
                case "_math_pow": VerifyCount(functionCall, 2); TranslateExponent(output, args[0], args[1]); break;
                case "_math_sin": VerifyCount(functionCall, 1); TranslateSin(output, args[0]); break;
                case "_math_tan": VerifyCount(functionCall, 1); TranslateTan(output, args[0]); break;
                case "_multiply_list": VerifyCount(functionCall, 2); TranslateMultiplyList(output, args[0], args[1]); break;
                case "_new_array": VerifyCount(functionCall, 2); TranslateNewArray(output, (StringConstant)args[0], args[1]); break;
                case "_new_dictionary": VerifyCount(functionCall, 2); TranslateNewDictionary(output, (StringConstant)args[0], (StringConstant)args[1]); break;
                case "_new_list": VerifyCount(functionCall, 1); TranslateNewList(output, (StringConstant)args[0]); break;
                case "_new_list_of_size": VerifyCount(functionCall, 2); TranslateNewListOfSize(output, (StringConstant)args[0], args[1]); break;
                case "_ord": VerifyCount(functionCall, 1); TranslateOrd(output, args[0]); break;
                case "_parse_float": VerifyCount(functionCall, 2); TranslateParseFloat(output, args[0], args[1]); break;
                case "_parse_int": VerifyCount(functionCall, 1); TranslateParseInt(output, args[0]); break;
                case "_postfix_decrement": VerifyCount(functionCall, 1); TranslateIncrement(output, args[0], false, false); break;
                case "_postfix_increment": VerifyCount(functionCall, 1); TranslateIncrement(output, args[0], true, false); break;
                case "_prefix_decrement": VerifyCount(functionCall, 1); TranslateIncrement(output, args[0], false, true); break;
                case "_prefix_increment": VerifyCount(functionCall, 1); TranslateIncrement(output, args[0], true, true); break;
                case "_random_float": VerifyCount(functionCall, 0); TranslateRandomFloat(output); break;
                case "_resource_get_manifest": VerifyCount(functionCall, 0); TranslateResourceGetManifest(output); break;
                case "_resource_read_text_file": VerifyCount(functionCall, 1); TranslateResourceReadText(output, args[0]); break;
                case "_set_program_data": VerifyCount(functionCall, 1); TranslateSetProgramData(output, args[0]); break;
                case "_sorted_copy_of_int_array": VerifyCount(functionCall, 1); TranslateSortedCopyOfIntArray(output, args[0]); break;
                case "_sorted_copy_of_string_array": VerifyCount(functionCall, 1); TranslateSortedCopyOfStringArray(output, args[0]); break;
                case "_string_append": VerifyCount(functionCall, 2); TranslateStringAppend(output, args[0], args[1]); break;
                case "_string_as_char": VerifyCount(functionCall, 1); TranslateStringAsChar(output, (StringConstant)args[0]); break;
                case "_string_cast_strong": VerifyCount(functionCall, 1); TranslateStringCast(output, args[0], true); break;
                case "_string_cast_weak": VerifyCount(functionCall, 1); TranslateStringCast(output, args[0], false); break;
                case "_string_char_at": VerifyCount(functionCall, 2); TranslateStringCharAt(output, args[0], args[1]); break;
                case "_string_compare": VerifyCount(functionCall, 2); TranslateStringCompare(output, args[0], args[1]); break;
                case "_string_concat": VerifyCountAtLeast(functionCall, 2); TranslateStringConcat(output, args); break;
                case "_string_contains": VerifyCount(functionCall, 2); TranslateStringContains(output, args[0], args[1]); break;
                case "_string_endswith": VerifyCount(functionCall, 2); TranslateStringEndsWith(output, args[0], args[1]); break;
                case "_string_equals": VerifyCount(functionCall, 2); TranslateStringEquals(output, args[0], args[1]); break;
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
                case "_unsafe_float_division": VerifyCount(functionCall, 2); TranslateUnsafeFloatDivision(output, args[0], args[1]); break;
                case "_unsafe_integer_division": VerifyCount(functionCall, 2); TranslateUnsafeIntegerDivision(output, args[0], args[1]); break;
                default: throw new ParserException(functionCall.FirstToken, "Unrecognized system method invocation: " + functionCall.Name);
            }
        }

        protected abstract void TranslateAppDataRoot(List<string> output);
        protected abstract void TranslateAsyncMessageQueuePump(List<string> output);
        protected abstract void TranslateArcCos(List<string> output, Expression value);
        protected abstract void TranslateArcSin(List<string> output, Expression value);
        protected abstract void TranslateArcTan(List<string> output, Expression dy, Expression dx);
        protected abstract void TranslateArrayGet(List<string> output, Expression list, Expression index);
        protected abstract void TranslateArrayLength(List<string> output, Expression list);
        protected abstract void TranslateArraySet(List<string> output, Expression list, Expression index, Expression value);
        protected abstract void TranslateAssert(List<string> output, Expression message);
        protected abstract void TranslateBeginFrame(List<string> output);
        protected abstract void TranslateByteCodeGetIntArgs(List<string> output);
        protected abstract void TranslateByteCodeGetOps(List<string> output);
        protected abstract void TranslateByteCodeGetStringArgs(List<string> output);
        protected abstract void TranslateCast(List<string> output, StringConstant typeValue, Expression expression);
        protected abstract void TranslateCastToList(List<string> output, StringConstant typeValue, Expression enumerableThing);
        protected abstract void TranslateCharToString(List<string> output, Expression charValue);
        protected abstract void TranslateChr(List<string> output, Expression asciiValue);
        protected abstract void TranslateComment(List<string> output, StringConstant commentValue);
        protected abstract void TranslateConvertListToArray(List<string> output, StringConstant type, Expression list);
        protected abstract void TranslateCos(List<string> output, Expression value);
        protected abstract void TranslateCurrentTimeSeconds(List<string> output);
        protected abstract void TranslateDictionaryContains(List<string> output, Expression dictionary, Expression key);
        protected abstract void TranslateDictionaryGetGuaranteed(List<string> output, Expression dictionary, Expression key);
        protected abstract void TranslateDictionaryGetKeys(List<string> output, string keyType, Expression dictionary);
        protected abstract void TranslateDictionaryGetValues(List<string> output, Expression dictionary);
        protected abstract void TranslateDictionaryRemove(List<string> output, Expression dictionary, Expression key);
        protected abstract void TranslateDictionarySet(List<string> output, Expression dictionary, Expression key, Expression value);
        protected abstract void TranslateDictionarySize(List<string> output, Expression dictionary);
        protected abstract void TranslateDotEquals(List<string> output, Expression root, Expression compareTo);
        protected abstract void TranslateExponent(List<string> output, Expression baseNum, Expression powerNum);
        protected abstract void TranslateForceParens(List<string> output, Expression expression);
        protected abstract void TranslateGetProgramData(List<string> output);
        protected abstract void TranslateGetRawByteCodeString(List<string> output);
        protected abstract void TranslateHttpRequest(List<string> output, Expression httpRequest, Expression method, Expression url, Expression body, Expression userAgent, Expression contentType, Expression contentLength, Expression headerNameList, Expression headerValueList);
        protected abstract void TranslateInt(List<string> output, Expression value);
        protected abstract void TranslateIoCreateDirectory(List<string> output, Expression path);
        protected abstract void TranslateIoCurrentDirectory(List<string> output);
        protected abstract void TranslateIoDeleteDirectory(List<string> output, Expression path, Expression isRecursive);
        protected abstract void TranslateIoDeleteFile(List<string> output, Expression path, Expression isUserData);
        protected abstract void TranslateIoDoesPathExist(List<string> output, Expression canonicalizedPath, Expression directoriesOnly, Expression performCaseCheck, Expression isUserData);
        protected abstract void TranslateIoFileReadText(List<string> output, Expression path, Expression isUserData);
        protected abstract void TranslateIoFilesInDirectory(List<string> output, Expression verifiedCanonicalizedPath, Expression isUserData);
        protected abstract void TranslateIoFileWriteText(List<string> output, Expression path, Expression content, Expression isUserData);
        protected abstract void TranslateIsValidInteger(List<string> output, Expression number);
        protected abstract void TranslateIsWindowsProgram(List<string> output);
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
        protected abstract void TranslateOrd(List<string> output, Expression character);
        protected abstract void TranslateParseFloat(List<string> output, Expression outParam, Expression rawString);
        protected abstract void TranslateParseInt(List<string> output, Expression rawString);
        protected abstract void TranslateParseJson(List<string> output, Expression rawString);
        protected abstract void TranslateIncrement(List<string> output, Expression expression, bool increment, bool prefix);
        protected abstract void TranslateRandomFloat(List<string> output);
        protected abstract void TranslateResourceGetManifest(List<string> output);
        protected abstract void TranslateResourceReadText(List<string> output, Expression path);
        protected abstract void TranslateSetProgramData(List<string> output, Expression programData);
        protected abstract void TranslateSin(List<string> output, Expression value);
        protected abstract void TranslateSortedCopyOfIntArray(List<string> output, Expression list);
        protected abstract void TranslateSortedCopyOfStringArray(List<string> output, Expression list);
        protected abstract void TranslateStringAsChar(List<string> output, StringConstant stringConstant);
        protected abstract void TranslateStringAppend(List<string> output, Expression target, Expression valueToAppend);
        protected abstract void TranslateStringCast(List<string> output, Expression thing, bool strongCast);
        protected abstract void TranslateStringCharAt(List<string> output, Expression stringValue, Expression index);
        protected abstract void TranslateStringCompare(List<string> output, Expression a, Expression b);
        protected abstract void TranslateStringConcat(List<string> output, Expression[] values);
        protected abstract void TranslateStringContains(List<string> output, Expression haystack, Expression needle);
        protected abstract void TranslateStringEndsWith(List<string> output, Expression stringExpr, Expression findMe);
        protected abstract void TranslateStringEquals(List<string> output, Expression aNonNull, Expression b);
        protected abstract void TranslateStringFromCode(List<string> output, Expression characterCode);
        protected abstract void TranslateStringIndexOf(List<string> output, Expression haystack, Expression needle);
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
        protected abstract void TranslateUnsafeFloatDivision(List<string> output, Expression numerator, Expression denominator);
        protected abstract void TranslateUnsafeIntegerDivision(List<string> output, Expression numerator, Expression denominator);

        private void VerifyCountAtLeast(SystemFunctionCall functionCall, int minArgCount)
        {
            if (functionCall.Args.Length < minArgCount)
            {
                throw new ParserException(functionCall.FirstToken, "Not enough args. Expected at least " + minArgCount);
            }
        }

        private void VerifyCount(SystemFunctionCall functionCall, int argCount)
        {
            if (functionCall.Args.Length != argCount)
            {
                throw new ParserException(functionCall.FirstToken, "Wrong number of args. Expected: " + argCount);
            }
        }
    }
}