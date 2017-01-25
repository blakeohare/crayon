using System.Collections.Generic;
using Crayon.ParseTree;

namespace Crayon.Translator
{
    internal abstract class AbstractSystemFunctionTranslator
    {
        public AbstractSystemFunctionTranslator() { }

        public AbstractPlatform Platform { get; set; }
        public AbstractTranslator Translator { get; set; }

        public void Translate(string tab, List<string> output, SystemFunctionCall functionCall)
        {
            Expression[] args = functionCall.Args;
            string fullName = functionCall.Name;
            string name = fullName.Substring(1);

            if (name.StartsWith("_lib_"))
            {
                output.Add(functionCall.AssociatedLibrary.TranslateNativeInvocation(functionCall.FirstToken, this.Platform, fullName, args));
                return;
            }

            switch (name)
            {
                case "_cast": VerifyCount(functionCall, 2); TranslateCast(output, (StringConstant)args[0], args[1]); break;
                case "_cast_to_list": VerifyCount(functionCall, 2); TranslateCastToList(output, (StringConstant)args[0], args[1]); break;
                case "_chr": VerifyCount(functionCall, 1); TranslateChr(output, args[0]); break;
                case "_command_line_args": VerifyCount(functionCall, 0); TranslateCommandLineArgs(output); break;
                case "_comment": VerifyCount(functionCall, 1); TranslateComment(output, (StringConstant)args[0]); break;
                case "_convert_list_to_array": VerifyCount(functionCall, 2); TranslateConvertListToArray(output, (StringConstant)args[0], args[1]); break;
                case "_dictionary_get_guaranteed": VerifyCount(functionCall, 2); TranslateDictionaryGetGuaranteed(output, args[0], args[1]); break;
                case "_dictionary_get_keys": VerifyCount(functionCall, 2); TranslateDictionaryGetKeys(output, ((StringConstant)args[0]).Value, args[1]); break;
                case "_dictionary_get_values": VerifyCount(functionCall, 1); TranslateDictionaryGetValues(output, args[0]); break;
                case "_dictionary_remove": VerifyCount(functionCall, 2); TranslateDictionaryRemove(output, args[0], args[1]); break;
                case "_dictionary_set": VerifyCount(functionCall, 3); TranslateDictionarySet(output, args[0], args[1], args[2]); break;
                case "_dictionary_size": VerifyCount(functionCall, 1); TranslateDictionarySize(output, args[0]); break;
                case "_enqueue_vm_resume": VerifyCount(functionCall, 2); TranslateEnqueueVmResume(output, args[0], args[1]); break;
                case "_int": VerifyCount(functionCall, 1); TranslateInt(output, args[0]); break;
                case "_new_array": VerifyCount(functionCall, 2); TranslateNewArray(output, (StringConstant)args[0], args[1]); break;
                case "_new_dictionary": VerifyCount(functionCall, 2); TranslateNewDictionary(output, (StringConstant)args[0], (StringConstant)args[1]); break;
                case "_new_list": VerifyCount(functionCall, 1); TranslateNewList(output, (StringConstant)args[0]); break;
                case "_new_list_of_size": VerifyCount(functionCall, 2); TranslateNewListOfSize(output, (StringConstant)args[0], args[1]); break;
                case "_ord": VerifyCount(functionCall, 1); TranslateOrd(output, args[0]); break;
                case "_parse_float": VerifyCount(functionCall, 2); TranslateParseFloat(output, args[0], args[1]); break;
                case "_parse_int": VerifyCount(functionCall, 1); TranslateParseInt(output, args[0]); break;
                case "_resource_get_manifest": VerifyCount(functionCall, 0); TranslateResourceGetManifest(output); break;
                case "_sorted_copy_of_int_array": VerifyCount(functionCall, 1); TranslateSortedCopyOfIntArray(output, args[0]); break;
                case "_sorted_copy_of_string_array": VerifyCount(functionCall, 1); TranslateSortedCopyOfStringArray(output, args[0]); break;
                case "_string_append": VerifyCount(functionCall, 2); TranslateStringAppend(output, args[0], args[1]); break;
                case "_string_as_char": VerifyCount(functionCall, 1); TranslateStringAsChar(output, (StringConstant)args[0]); break;
                case "_string_cast_strong": VerifyCount(functionCall, 1); TranslateStringCast(output, args[0], true); break;
                case "_string_cast_weak": VerifyCount(functionCall, 1); TranslateStringCast(output, args[0], false); break;
                case "_string_char_at": VerifyCount(functionCall, 2); TranslateStringCharAt(output, args[0], args[1]); break;
                case "_string_char_code_at": VerifyCount(functionCall, 2); TranslateStringCharCodeAt(output, args[0], args[1]); break;
                case "_string_compare_is_reverse": VerifyCount(functionCall, 2); TranslateStringCompareIsReverse(output, args[0], args[1]); break;
                case "_string_concat": VerifyCountAtLeast(functionCall, 2); TranslateStringConcat(output, args); break;
                case "_string_equals": VerifyCount(functionCall, 2); TranslateStringEquals(output, args[0], args[1]); break;
                case "_string_from_code": VerifyCount(functionCall, 1); TranslateStringFromCode(output, args[0]); break;
                case "_string_index_of": VerifyCount(functionCall, 2, 3); TranslateStringIndexOf(output, args[0], args[1], args.Length == 3 ? args[2] : null); break;
                case "_string_parse_float": VerifyCount(functionCall, 1); TranslateStringParseFloat(output, args[0]); break;
                case "_string_parse_int": VerifyCount(functionCall, 1); TranslateStringParseInt(output, args[0]); break;
                case "_string_substring": VerifyCount(functionCall, 2, 3); TranslateStringSubstring(output, args[0], args[1], args.Length > 2 ? args[2] : null); break;
                case "_string_substring_exists_at": VerifyCount(functionCall, 3); TranslateStringSubstringExistsAt(output, args[0], args[1], args[2]); break;
                case "_thread_sleep": VerifyCount(functionCall, 1); TranslateThreadSleep(output, args[0]); break;
                default:
                    //throw new ParserException(functionCall.FirstToken, "Unrecognized system method invocation: " + functionCall.Name);
                    // TODO: Eventually this will be removed and AssociatedLibrary will be set to Core and this entire switch statement can go away.
                    output.Add(functionCall.HACK_CoreLibraryReference.TranslateNativeInvocation(functionCall.FirstToken, this.Platform, fullName, args));
                    break;
            }
        }

        protected abstract void TranslateCast(List<string> output, StringConstant typeValue, Expression expression);
        protected abstract void TranslateCastToList(List<string> output, StringConstant typeValue, Expression enumerableThing);
        protected abstract void TranslateChr(List<string> output, Expression asciiValue);
        protected abstract void TranslateCommandLineArgs(List<string> output);
        protected abstract void TranslateComment(List<string> output, StringConstant commentValue);
        protected abstract void TranslateConvertListToArray(List<string> output, StringConstant type, Expression list);
        protected abstract void TranslateDictionaryGetGuaranteed(List<string> output, Expression dictionary, Expression key);
        protected abstract void TranslateDictionaryGetKeys(List<string> output, string keyType, Expression dictionary);
        protected abstract void TranslateDictionaryGetValues(List<string> output, Expression dictionary);
        protected abstract void TranslateDictionaryRemove(List<string> output, Expression dictionary, Expression key);
        protected abstract void TranslateDictionarySet(List<string> output, Expression dictionary, Expression key, Expression value);
        protected abstract void TranslateDictionarySize(List<string> output, Expression dictionary);
        protected abstract void TranslateEnqueueVmResume(List<string> output, Expression seconds, Expression executionContextId);
        protected abstract void TranslateInt(List<string> output, Expression value);
        protected abstract void TranslateIsWindowsProgram(List<string> output);
        protected abstract void TranslateNewArray(List<string> output, StringConstant type, Expression size);
        protected abstract void TranslateNewDictionary(List<string> output, StringConstant keyType, StringConstant valueType);
        protected abstract void TranslateNewList(List<string> output, StringConstant type);
        protected abstract void TranslateNewListOfSize(List<string> output, StringConstant type, Expression length);
        protected abstract void TranslateOrd(List<string> output, Expression character);
        protected abstract void TranslateParseFloat(List<string> output, Expression outParam, Expression rawString);
        protected abstract void TranslateParseInt(List<string> output, Expression rawString);
        protected abstract void TranslateResourceGetManifest(List<string> output);
        protected abstract void TranslateSortedCopyOfIntArray(List<string> output, Expression list);
        protected abstract void TranslateSortedCopyOfStringArray(List<string> output, Expression list);
        protected abstract void TranslateStringAsChar(List<string> output, StringConstant stringConstant);
        protected abstract void TranslateStringAppend(List<string> output, Expression target, Expression valueToAppend);
        protected abstract void TranslateStringCast(List<string> output, Expression thing, bool strongCast);
        protected abstract void TranslateStringCharAt(List<string> output, Expression stringValue, Expression index);
        protected abstract void TranslateStringCharCodeAt(List<string> output, Expression stringValue, Expression index);
        protected abstract void TranslateStringCompareIsReverse(List<string> output, Expression a, Expression b);
        protected abstract void TranslateStringConcat(List<string> output, Expression[] values);
        protected abstract void TranslateStringEquals(List<string> output, Expression aNonNull, Expression b);
        protected abstract void TranslateStringFromCode(List<string> output, Expression characterCode);
        protected abstract void TranslateStringIndexOf(List<string> output, Expression haystack, Expression needle, Expression optionalStartFrom);
        protected abstract void TranslateStringParseFloat(List<string> output, Expression stringValue);
        protected abstract void TranslateStringParseInt(List<string> output, Expression value);
        protected abstract void TranslateStringSubstring(List<string> output, Expression stringExpr, Expression startIndex, Expression optionalLength);
        protected abstract void TranslateStringSubstringExistsAt(List<string> output, Expression stringExpr, Expression lookFor, Expression index);
        protected abstract void TranslateThreadSleep(List<string> output, Expression timeDelaySeconds);

        private void VerifyCountAtLeast(SystemFunctionCall functionCall, int minArgCount)
        {
            if (functionCall.Args.Length < minArgCount)
            {
                throw new ParserException(functionCall.FirstToken, "Not enough args. Expected at least " + minArgCount);
            }
        }

        private void VerifyCount(SystemFunctionCall functionCall, int argCount1, params int[] orTheseArgCounts)
        {
            int count = functionCall.Args.Length;
            if (count == argCount1) return;
            foreach (int argCount in orTheseArgCounts)
            {
                if (argCount == count) return;
            }
            throw new ParserException(functionCall.FirstToken, "Wrong number of args.");
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
