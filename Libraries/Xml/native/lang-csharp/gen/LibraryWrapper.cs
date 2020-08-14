using Interpreter.Structs;
using System.Collections.Generic;
using System.Linq;

namespace Interpreter.Libraries.Xml
{
    public static class LibraryWrapper
    {
        private static readonly string[] PST_SplitSep = new string[1];

        private static string[] PST_StringSplit(string value, string sep)
        {
            if (sep.Length == 1) return value.Split(sep[0]);
            if (sep.Length == 0) return value.ToCharArray().Select<char, string>(c => "" + c).ToArray();
            PST_SplitSep[0] = sep;
            return value.Split(PST_SplitSep, System.StringSplitOptions.None);
        }

        private static bool PST_SubstringIsEqualTo(string haystack, int index, string needle)
        {
            int needleLength = needle.Length;
            if (index + needleLength > haystack.Length) return false;
            if (needleLength == 0) return true;
            if (haystack[index] != needle[0]) return false;
            if (needleLength == 1) return true;
            for (int i = 1; i < needleLength; ++i)
            {
                if (needle[i] != haystack[index + i]) return false;
            }
            return true;
        }

        private static readonly int[] PST_IntBuffer16 = new int[16];

        private static Dictionary<string, System.Func<object[], object>> PST_ExtCallbacks = new Dictionary<string, System.Func<object[], object>>();

        public static void PST_RegisterExtensibleCallback(string name, System.Func<object[], object> func) {
            PST_ExtCallbacks[name] = func;
        }

        public static string lib_xml_ampUnescape(string value, Dictionary<string, string> entityLookup)
        {
            string[] ampParts = PST_StringSplit(value, "&");
            int i = 1;
            while ((i < ampParts.Length))
            {
                string component = ampParts[i];
                int semicolon = component.IndexOf(";");
                if ((semicolon != -1))
                {
                    string entityCode = component.Substring(0, semicolon);
                    string entityValue = lib_xml_getEntity(entityCode, entityLookup);
                    if ((entityValue == null))
                    {
                        entityValue = "&";
                    }
                    else
                    {
                        component = component.Substring((semicolon + 1), ((component.Length - semicolon - 1)));
                    }
                    ampParts[i] = entityValue + component;
                }
                i += 1;
            }
            return string.Join("", ampParts);
        }

        public static string lib_xml_error(string xml, int index, string msg)
        {
            string loc = "";
            if ((index < xml.Length))
            {
                int line = 1;
                int col = 0;
                int i = 0;
                while ((i <= index))
                {
                    if ((xml[i] == '\n'))
                    {
                        line += 1;
                        col = 0;
                    }
                    else
                    {
                        col += 1;
                    }
                    i += 1;
                }
                loc = string.Join("", new string[] { " on line ",(line).ToString(),", col ",(col).ToString() });
            }
            return string.Join("", new string[] { "XML parse error",loc,": ",msg });
        }

        public static string lib_xml_getEntity(string code, Dictionary<string, string> entityLookup)
        {
            if (entityLookup.ContainsKey(code))
            {
                return entityLookup[code];
            }
            return null;
        }

        public static bool lib_xml_isNext(string xml, int[] indexPtr, string value)
        {
            return PST_SubstringIsEqualTo(xml, indexPtr[0], value);
        }

        public static Value lib_xml_parse(VmContext vm, Value[] args)
        {
            string xml = (string)args[0].internalValue;
            ListImpl list1 = (ListImpl)args[1].internalValue;
            ObjectInstance objInstance1 = (ObjectInstance)args[2].internalValue;
            object[] objArray1 = objInstance1.nativeData;
            if ((objArray1 == null))
            {
                objArray1 = new object[2];
                objInstance1.nativeData = objArray1;
                objArray1[0] = new Dictionary<string, string>();
                objArray1[1] = new Dictionary<int, int>();
            }
            List<Value> output = new List<Value>();
            string errMsg = lib_xml_parseImpl(vm, xml, PST_IntBuffer16, output, (Dictionary<string, string>)objArray1[0], (Dictionary<int, int>)objArray1[1]);
            if ((errMsg != null))
            {
                return Interpreter.Vm.CrayonWrapper.buildString(vm.globals, errMsg);
            }
            ListImpl list2 = (ListImpl)Interpreter.Vm.CrayonWrapper.buildList(output).internalValue;
            list1.size = list2.size;
            list1.capacity = list2.capacity;
            list1.array = list2.array;
            return vm.globalNull;
        }

        public static string lib_xml_parseElement(VmContext vm, string xml, int[] indexPtr, List<Value> output, Dictionary<string, string> entityLookup, Dictionary<int, int> stringEnders)
        {
            int length = xml.Length;
            List<Value> attributeKeys = new List<Value>();
            List<Value> attributeValues = new List<Value>();
            List<Value> children = new List<Value>();
            List<Value> element = new List<Value>();
            string error = null;
            if (!lib_xml_popIfPresent(xml, indexPtr, "<"))
            {
                return lib_xml_error(xml, indexPtr[0], "Expected: '<'");
            }
            string name = lib_xml_popName(xml, indexPtr);
            lib_xml_skipWhitespace(xml, indexPtr);
            bool hasClosingTag = true;
            while (true)
            {
                if ((indexPtr[0] >= length))
                {
                    return lib_xml_error(xml, length, "Unexpected EOF");
                }
                if (lib_xml_popIfPresent(xml, indexPtr, ">"))
                {
                    break;
                }
                if (lib_xml_popIfPresent(xml, indexPtr, "/>"))
                {
                    hasClosingTag = false;
                    break;
                }
                string key = lib_xml_popName(xml, indexPtr);
                if ((key.Length == 0))
                {
                    return lib_xml_error(xml, indexPtr[0], "Expected attribute name.");
                }
                attributeKeys.Add(Interpreter.Vm.CrayonWrapper.buildString(vm.globals, key));
                lib_xml_skipWhitespace(xml, indexPtr);
                if (!lib_xml_popIfPresent(xml, indexPtr, "="))
                {
                    return lib_xml_error(xml, indexPtr[0], "Expected: '='");
                }
                lib_xml_skipWhitespace(xml, indexPtr);
                error = lib_xml_popString(vm, xml, indexPtr, attributeValues, entityLookup, stringEnders);
                if ((error != null))
                {
                    return error;
                }
                lib_xml_skipWhitespace(xml, indexPtr);
            }
            if (hasClosingTag)
            {
                string close = string.Join("", new string[] { "</",name,">" });
                while (!lib_xml_popIfPresent(xml, indexPtr, close))
                {
                    if (lib_xml_isNext(xml, indexPtr, "</"))
                    {
                        error = lib_xml_error(xml, (indexPtr[0] - 2), "Unexpected close tag.");
                    }
                    else if (lib_xml_isNext(xml, indexPtr, "<!--"))
                    {
                        error = lib_xml_skipComment(xml, indexPtr);
                    }
                    else if (lib_xml_isNext(xml, indexPtr, "<"))
                    {
                        error = lib_xml_parseElement(vm, xml, indexPtr, children, entityLookup, stringEnders);
                    }
                    else
                    {
                        error = lib_xml_parseText(vm, xml, indexPtr, children, entityLookup);
                    }
                    if (((error == null) && (indexPtr[0] >= length)))
                    {
                        error = lib_xml_error(xml, length, "Unexpected EOF. Unclosed tag.");
                    }
                    if ((error != null))
                    {
                        return error;
                    }
                }
            }
            element.Add(vm.globalTrue);
            element.Add(Interpreter.Vm.CrayonWrapper.buildString(vm.globals, name));
            element.Add(Interpreter.Vm.CrayonWrapper.buildList(attributeKeys));
            element.Add(Interpreter.Vm.CrayonWrapper.buildList(attributeValues));
            element.Add(Interpreter.Vm.CrayonWrapper.buildList(children));
            output.Add(Interpreter.Vm.CrayonWrapper.buildList(element));
            return null;
        }

        public static string lib_xml_parseImpl(VmContext vm, string input, int[] indexPtr, List<Value> output, Dictionary<string, string> entityLookup, Dictionary<int, int> stringEnders)
        {
            if ((entityLookup.Count == 0))
            {
                entityLookup["amp"] = "&";
                entityLookup["lt"] = "<";
                entityLookup["gt"] = ">";
                entityLookup["quot"] = "\"";
                entityLookup["apos"] = "'";
            }
            if ((stringEnders.Count == 0))
            {
                stringEnders[((int)(' '))] = 1;
                stringEnders[((int)('"'))] = 1;
                stringEnders[((int)('\''))] = 1;
                stringEnders[((int)('<'))] = 1;
                stringEnders[((int)('>'))] = 1;
                stringEnders[((int)('\t'))] = 1;
                stringEnders[((int)('\r'))] = 1;
                stringEnders[((int)('\n'))] = 1;
                stringEnders[((int)('/'))] = 1;
            }
            indexPtr[0] = 0;
            lib_xml_skipWhitespace(input, indexPtr);
            if (lib_xml_popIfPresent(input, indexPtr, "<?xml"))
            {
                int newBegin = input.IndexOf("?>");
                if ((newBegin == -1))
                {
                    return lib_xml_error(input, (indexPtr[0] - 5), "XML Declaration is not closed.");
                }
                indexPtr[0] = (newBegin + 2);
            }
            string error = lib_xml_skipStuff(input, indexPtr);
            if ((error != null))
            {
                return error;
            }
            error = lib_xml_parseElement(vm, input, indexPtr, output, entityLookup, stringEnders);
            if ((error != null))
            {
                return error;
            }
            lib_xml_skipStuff(input, indexPtr);
            if ((indexPtr[0] != input.Length))
            {
                return lib_xml_error(input, indexPtr[0], "Unexpected text.");
            }
            return null;
        }

        public static string lib_xml_parseText(VmContext vm, string xml, int[] indexPtr, List<Value> output, Dictionary<string, string> entityLookup)
        {
            int length = xml.Length;
            int start = indexPtr[0];
            int i = start;
            bool ampFound = false;
            char c = ' ';
            while ((i < length))
            {
                c = xml[i];
                if ((c == '<'))
                {
                    break;
                }
                else if ((c == '&'))
                {
                    ampFound = true;
                }
                i += 1;
            }
            if ((i > start))
            {
                indexPtr[0] = i;
                string textValue = xml.Substring(start, (i - start));
                if (ampFound)
                {
                    textValue = lib_xml_ampUnescape(textValue, entityLookup);
                }
                List<Value> textElement = new List<Value>();
                textElement.Add(vm.globalFalse);
                textElement.Add(Interpreter.Vm.CrayonWrapper.buildString(vm.globals, textValue));
                output.Add(Interpreter.Vm.CrayonWrapper.buildList(textElement));
            }
            return null;
        }

        public static bool lib_xml_popIfPresent(string xml, int[] indexPtr, string s)
        {
            if (PST_SubstringIsEqualTo(xml, indexPtr[0], s))
            {
                indexPtr[0] = (indexPtr[0] + s.Length);
                return true;
            }
            return false;
        }

        public static string lib_xml_popName(string xml, int[] indexPtr)
        {
            int length = xml.Length;
            int i = indexPtr[0];
            int start = i;
            char c = ' ';
            while ((i < length))
            {
                c = xml[i];
                if ((((c >= 'a') && (c <= 'z')) || ((c >= 'A') && (c <= 'Z')) || ((c >= '0') && (c <= '9')) || (c == '_') || (c == '.') || (c == ':') || (c == '-')))
                {
                }
                else
                {
                    break;
                }
                i += 1;
            }
            string output = xml.Substring(start, (i - start));
            indexPtr[0] = i;
            return output;
        }

        public static string lib_xml_popString(VmContext vm, string xml, int[] indexPtr, List<Value> attributeValueOut, Dictionary<string, string> entityLookup, Dictionary<int, int> stringEnders)
        {
            int length = xml.Length;
            int start = indexPtr[0];
            int end = length;
            int i = start;
            int stringType = ((int) xml[i]);
            bool unwrapped = ((stringType != ((int)('"'))) && (stringType != ((int)('\''))));
            bool ampFound = false;
            int c = ((int)(' '));
            if (unwrapped)
            {
                while ((i < length))
                {
                    c = ((int) xml[i]);
                    if (stringEnders.ContainsKey(c))
                    {
                        end = i;
                        break;
                    }
                    else if ((c == ((int)('&'))))
                    {
                        ampFound = true;
                    }
                    i += 1;
                }
            }
            else
            {
                i += 1;
                start = i;
                while ((i < length))
                {
                    c = ((int) xml[i]);
                    if ((c == stringType))
                    {
                        end = i;
                        i += 1;
                        break;
                    }
                    else if ((c == ((int)('&'))))
                    {
                        ampFound = true;
                    }
                    i += 1;
                }
            }
            indexPtr[0] = i;
            string output = xml.Substring(start, (end - start));
            if (ampFound)
            {
                output = lib_xml_ampUnescape(output, entityLookup);
            }
            attributeValueOut.Add(Interpreter.Vm.CrayonWrapper.buildString(vm.globals, output));
            return null;
        }

        public static string lib_xml_skipComment(string xml, int[] indexPtr)
        {
            if (lib_xml_popIfPresent(xml, indexPtr, "<!--"))
            {
                int i = xml.IndexOf("-->", indexPtr[0]);
                if ((i == -1))
                {
                    return lib_xml_error(xml, (indexPtr[0] - 4), "Unclosed comment.");
                }
                indexPtr[0] = (i + 3);
            }
            return null;
        }

        public static string lib_xml_skipStuff(string xml, int[] indexPtr)
        {
            int index = (indexPtr[0] - 1);
            while ((index < indexPtr[0]))
            {
                index = indexPtr[0];
                lib_xml_skipWhitespace(xml, indexPtr);
                string error = lib_xml_skipComment(xml, indexPtr);
                if ((error != null))
                {
                    return error;
                }
            }
            return null;
        }

        public static int lib_xml_skipWhitespace(string xml, int[] indexPtr)
        {
            int length = xml.Length;
            int i = indexPtr[0];
            while ((i < length))
            {
                char c = xml[i];
                if (((c != ' ') && (c != '\t') && (c != '\n') && (c != '\r')))
                {
                    indexPtr[0] = i;
                    return 0;
                }
                i += 1;
            }
            indexPtr[0] = i;
            return 0;
        }
    }
}
