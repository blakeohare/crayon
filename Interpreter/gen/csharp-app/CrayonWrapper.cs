using Interpreter.Structs;
using System.Collections.Generic;
using System.Linq;

namespace Interpreter.Vm
{
    public static class CrayonWrapper
    {
        private static readonly string[] PST_StringBuffer16 = new string[16];

        private static readonly int[] PST_IntBuffer16 = new int[16];

        public static string PST_StringReverse(string value)
        {
            if (value.Length < 2) return value;
            char[] chars = value.ToCharArray();
            return new string(chars.Reverse().ToArray());
        }

        private static readonly string[] PST_SplitSep = new string[1];

        private static string[] PST_StringSplit(string value, string sep)
        {
            if (sep.Length == 1) return value.Split(sep[0]);
            if (sep.Length == 0) return value.ToCharArray().Select<char, string>(c => "" + c).ToArray();
            PST_SplitSep[0] = sep;
            return value.Split(PST_SplitSep, System.StringSplitOptions.None);
        }

        private static readonly System.Random PST_Random = new System.Random();

        // TODO: use a model like parse float to avoid double parsing.
        public static bool PST_IsValidInteger(string value)
        {
            if (value.Length == 0) return false;
            char c = value[0];
            if (value.Length == 1) return c >= '0' && c <= '9';
            int length = value.Length;
            for (int i = c == '-' ? 1 : 0; i < length; ++i)
            {
                c = value[i];
                if (c < '0' || c > '9') return false;
            }
            return true;
        }

        public static void PST_ParseFloat(string strValue, double[] output)
        {
            double num = 0.0;
            output[0] = double.TryParse(strValue, out num) ? 1 : -1;
            output[1] = num;
        }

        private static readonly System.DateTime PST_UnixEpoch = new System.DateTime(1970, 1, 1);

        private static double PST_CurrentTime
        {
            get { return System.DateTime.UtcNow.Subtract(PST_UnixEpoch).TotalSeconds; }
        }

        private static Dictionary<string, System.Func<object[], object>> PST_ExtCallbacks = new Dictionary<string, System.Func<object[], object>>();

        private static string PST_Base64ToString(string b64Value)
        {
            byte[] utf8Bytes = System.Convert.FromBase64String(b64Value);
            string value = System.Text.Encoding.UTF8.GetString(utf8Bytes);
            return value;
        }

        private static string PST_FloatToString(double value)
        {
            string output = value.ToString();
            if (output[0] == '.') output = "0" + output;
            if (!output.Contains('.')) output += ".0";
            return output;
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

        public static void PST_RegisterExtensibleCallback(string name, System.Func<object[], object> func) {
            PST_ExtCallbacks[name] = func;
        }

        public static int addLiteralImpl(VmContext vm, int[] row, string stringArg)
        {
            VmGlobals g = vm.globals;
            int type = row[0];
            if ((type == 1))
            {
                vm.metadata.literalTableBuilder.Add(g.valueNull);
            }
            else if ((type == 2))
            {
                vm.metadata.literalTableBuilder.Add(buildBoolean(g, (row[1] == 1)));
            }
            else if ((type == 3))
            {
                vm.metadata.literalTableBuilder.Add(buildInteger(g, row[1]));
            }
            else if ((type == 4))
            {
                vm.metadata.literalTableBuilder.Add(buildFloat(g, double.Parse(stringArg)));
            }
            else if ((type == 5))
            {
                vm.metadata.literalTableBuilder.Add(buildCommonString(g, stringArg));
            }
            else if ((type == 9))
            {
                int index = vm.metadata.literalTableBuilder.Count;
                vm.metadata.literalTableBuilder.Add(buildCommonString(g, stringArg));
                vm.metadata.invFunctionNameLiterals[stringArg] = index;
            }
            else if ((type == 10))
            {
                ClassValue cv = new ClassValue(false, row[1]);
                vm.metadata.literalTableBuilder.Add(new Value(10, cv));
            }
            return 0;
        }

        public static int addNameImpl(VmContext vm, string nameValue)
        {
            int index = vm.metadata.identifiersBuilder.Count;
            vm.metadata.invIdentifiers[nameValue] = index;
            vm.metadata.identifiersBuilder.Add(nameValue);
            if ("length" == nameValue)
            {
                vm.metadata.lengthId = index;
            }
            return 0;
        }

        public static void addToList(ListImpl list, Value item)
        {
            if ((list.size == list.capacity))
            {
                increaseListCapacity(list);
            }
            list.array[list.size] = item;
            list.size += 1;
        }

        public static int applyDebugSymbolData(VmContext vm, int[] opArgs, string stringData, FunctionInfo recentlyDefinedFunction)
        {
            return 0;
        }

        public static Value buildBoolean(VmGlobals g, bool value)
        {
            if (value)
            {
                return g.boolTrue;
            }
            return g.boolFalse;
        }

        public static Value buildCommonString(VmGlobals g, string s)
        {
            Value value = null;
            if (!g.commonStrings.TryGetValue(s, out value)) value = null;
            if ((value == null))
            {
                value = buildString(g, s);
                g.commonStrings[s] = value;
            }
            return value;
        }

        public static Value buildFloat(VmGlobals g, double value)
        {
            if ((value == 0.0))
            {
                return g.floatZero;
            }
            if ((value == 1.0))
            {
                return g.floatOne;
            }
            return new Value(4, value);
        }

        public static Value buildInteger(VmGlobals g, int num)
        {
            if ((num < 0))
            {
                if ((num > -257))
                {
                    return g.negativeIntegers[-num];
                }
            }
            else if ((num < 2049))
            {
                return g.positiveIntegers[num];
            }
            return new Value(3, num);
        }

        public static Value buildList(List<Value> valueList)
        {
            return buildListWithType(null, valueList);
        }

        public static Value buildListWithType(int[] type, List<Value> valueList)
        {
            int _len = valueList.Count;
            ListImpl output = makeEmptyList(type, _len);
            int i = 0;
            while ((i < _len))
            {
                output.array[i] = valueList[i];
                i += 1;
            }
            output.size = _len;
            return new Value(6, output);
        }

        public static Value buildNull(VmGlobals globals)
        {
            return globals.valueNull;
        }

        public static PlatformRelayObject buildRelayObj(int type, int iarg1, int iarg2, int iarg3, double farg1, string sarg1)
        {
            return new PlatformRelayObject(type, iarg1, iarg2, iarg3, farg1, sarg1);
        }

        public static Value buildString(VmGlobals g, string s)
        {
            if ((s.Length == 0))
            {
                return g.stringEmpty;
            }
            return new Value(5, s);
        }

        public static Value buildStringDictionary(VmGlobals globals, string[] stringKeys, Value[] values)
        {
            int size = stringKeys.Length;
            DictImpl d = new DictImpl(size, 5, 0, null, new Dictionary<int, int>(), new Dictionary<string, int>(), new List<Value>(), new List<Value>());
            string k = null;
            int i = 0;
            while ((i < size))
            {
                k = stringKeys[i];
                if (d.stringToIndex.ContainsKey(k))
                {
                    d.values[d.stringToIndex[k]] = values[i];
                }
                else
                {
                    d.stringToIndex[k] = d.values.Count;
                    d.values.Add(values[i]);
                    d.keys.Add(buildString(globals, k));
                }
                i += 1;
            }
            d.size = d.values.Count;
            return new Value(7, d);
        }

        public static int byteObjToList(Value[] nums, ObjectInstance obj, ListImpl emptyList)
        {
            int[] bytes = (int[])obj.nativeData[0];
            int size = bytes.Length;
            emptyList.capacity = size;
            emptyList.size = size;
            Value[] arr = new Value[size];
            int i = 0;
            while ((i < size))
            {
                arr[i] = nums[bytes[i]];
                ++i;
            }
            emptyList.array = arr;
            return 0;
        }

        public static Value bytesToListValue(VmGlobals globals, int[] bytes)
        {
            List<Value> b = new List<Value>();
            int length = bytes.Length;
            Value[] nums = globals.positiveIntegers;
            int i = 0;
            while ((i < length))
            {
                b.Add(nums[bytes[i]]);
                i += 1;
            }
            return buildList(b);
        }

        public static bool canAssignGenericToGeneric(VmContext vm, int[] gen1, int gen1Index, int[] gen2, int gen2Index, int[] newIndexOut)
        {
            if ((gen2 == null))
            {
                return true;
            }
            if ((gen1 == null))
            {
                return false;
            }
            int t1 = gen1[gen1Index];
            int t2 = gen2[gen2Index];
            switch (t1)
            {
                case 0:
                    newIndexOut[0] = (gen1Index + 1);
                    newIndexOut[1] = (gen2Index + 2);
                    return (t2 == t1);
                case 1:
                    newIndexOut[0] = (gen1Index + 1);
                    newIndexOut[1] = (gen2Index + 2);
                    return (t2 == t1);
                case 2:
                    newIndexOut[0] = (gen1Index + 1);
                    newIndexOut[1] = (gen2Index + 2);
                    return (t2 == t1);
                case 4:
                    newIndexOut[0] = (gen1Index + 1);
                    newIndexOut[1] = (gen2Index + 2);
                    return (t2 == t1);
                case 5:
                    newIndexOut[0] = (gen1Index + 1);
                    newIndexOut[1] = (gen2Index + 2);
                    return (t2 == t1);
                case 10:
                    newIndexOut[0] = (gen1Index + 1);
                    newIndexOut[1] = (gen2Index + 2);
                    return (t2 == t1);
                case 3:
                    newIndexOut[0] = (gen1Index + 1);
                    newIndexOut[1] = (gen2Index + 2);
                    return ((t2 == 3) || (t2 == 4));
                case 8:
                    newIndexOut[0] = (gen1Index + 1);
                    newIndexOut[1] = (gen2Index + 2);
                    if ((t2 != 8))
                    {
                        return false;
                    }
                    int c1 = gen1[(gen1Index + 1)];
                    int c2 = gen2[(gen2Index + 1)];
                    if ((c1 == c2))
                    {
                        return true;
                    }
                    return isClassASubclassOf(vm, c1, c2);
                case 6:
                    if ((t2 != 6))
                    {
                        return false;
                    }
                    return canAssignGenericToGeneric(vm, gen1, (gen1Index + 1), gen2, (gen2Index + 1), newIndexOut);
                case 7:
                    if ((t2 != 7))
                    {
                        return false;
                    }
                    if (!canAssignGenericToGeneric(vm, gen1, (gen1Index + 1), gen2, (gen2Index + 1), newIndexOut))
                    {
                        return false;
                    }
                    return canAssignGenericToGeneric(vm, gen1, newIndexOut[0], gen2, newIndexOut[1], newIndexOut);
                case 9:
                    if ((t2 != 9))
                    {
                        return false;
                    }
                    return false;
                default:
                    return false;
            }
        }

        public static Value canAssignTypeToGeneric(VmContext vm, Value value, int[] generics, int genericIndex)
        {
            switch (value.type)
            {
                case 1:
                    switch (generics[genericIndex])
                    {
                        case 5:
                            return value;
                        case 8:
                            return value;
                        case 10:
                            return value;
                        case 9:
                            return value;
                        case 6:
                            return value;
                        case 7:
                            return value;
                    }
                    return null;
                case 2:
                    if ((generics[genericIndex] == value.type))
                    {
                        return value;
                    }
                    return null;
                case 5:
                    if ((generics[genericIndex] == value.type))
                    {
                        return value;
                    }
                    return null;
                case 10:
                    if ((generics[genericIndex] == value.type))
                    {
                        return value;
                    }
                    return null;
                case 3:
                    if ((generics[genericIndex] == 3))
                    {
                        return value;
                    }
                    if ((generics[genericIndex] == 4))
                    {
                        return buildFloat(vm.globals, (0.0 + (int)value.internalValue));
                    }
                    return null;
                case 4:
                    if ((generics[genericIndex] == 4))
                    {
                        return value;
                    }
                    return null;
                case 6:
                    ListImpl list = (ListImpl)value.internalValue;
                    int[] listType = list.type;
                    genericIndex += 1;
                    if ((listType == null))
                    {
                        if (((generics[genericIndex] == 1) || (generics[genericIndex] == 0)))
                        {
                            return value;
                        }
                        return null;
                    }
                    int i = 0;
                    while ((i < listType.Length))
                    {
                        if ((listType[i] != generics[(genericIndex + i)]))
                        {
                            return null;
                        }
                        i += 1;
                    }
                    return value;
                case 7:
                    DictImpl dict = (DictImpl)value.internalValue;
                    int j = genericIndex;
                    switch (dict.keyType)
                    {
                        case 3:
                            if ((generics[1] == dict.keyType))
                            {
                                j += 2;
                            }
                            else
                            {
                                return null;
                            }
                            break;
                        case 5:
                            if ((generics[1] == dict.keyType))
                            {
                                j += 2;
                            }
                            else
                            {
                                return null;
                            }
                            break;
                        case 8:
                            if ((generics[1] == 8))
                            {
                                j += 3;
                            }
                            else
                            {
                                return null;
                            }
                            break;
                    }
                    int[] valueType = dict.valueType;
                    if ((valueType == null))
                    {
                        if (((generics[j] == 0) || (generics[j] == 1)))
                        {
                            return value;
                        }
                        return null;
                    }
                    int k = 0;
                    while ((k < valueType.Length))
                    {
                        if ((valueType[k] != generics[(j + k)]))
                        {
                            return null;
                        }
                        k += 1;
                    }
                    return value;
                case 8:
                    if ((generics[genericIndex] == 8))
                    {
                        int targetClassId = generics[(genericIndex + 1)];
                        int givenClassId = ((ObjectInstance)value.internalValue).classId;
                        if ((targetClassId == givenClassId))
                        {
                            return value;
                        }
                        if (isClassASubclassOf(vm, givenClassId, targetClassId))
                        {
                            return value;
                        }
                    }
                    return null;
            }
            return null;
        }

        public static double canonicalizeAngle(double a)
        {
            double twopi = 6.28318530717958;
            a = (a % twopi);
            if ((a < 0))
            {
                a += twopi;
            }
            return a;
        }

        public static int canonicalizeListSliceArgs(int[] outParams, Value beginValue, Value endValue, int beginIndex, int endIndex, int stepAmount, int length, bool isForward)
        {
            if ((beginValue == null))
            {
                if (isForward)
                {
                    beginIndex = 0;
                }
                else
                {
                    beginIndex = (length - 1);
                }
            }
            if ((endValue == null))
            {
                if (isForward)
                {
                    endIndex = length;
                }
                else
                {
                    endIndex = (-1 - length);
                }
            }
            if ((beginIndex < 0))
            {
                beginIndex += length;
            }
            if ((endIndex < 0))
            {
                endIndex += length;
            }
            if (((beginIndex == 0) && (endIndex == length) && (stepAmount == 1)))
            {
                return 2;
            }
            if (isForward)
            {
                if ((beginIndex >= length))
                {
                    return 0;
                }
                if ((beginIndex < 0))
                {
                    return 3;
                }
                if ((endIndex < beginIndex))
                {
                    return 4;
                }
                if ((beginIndex == endIndex))
                {
                    return 0;
                }
                if ((endIndex > length))
                {
                    endIndex = length;
                }
            }
            else
            {
                if ((beginIndex < 0))
                {
                    return 0;
                }
                if ((beginIndex >= length))
                {
                    return 3;
                }
                if ((endIndex > beginIndex))
                {
                    return 4;
                }
                if ((beginIndex == endIndex))
                {
                    return 0;
                }
                if ((endIndex < -1))
                {
                    endIndex = -1;
                }
            }
            outParams[0] = beginIndex;
            outParams[1] = endIndex;
            return 1;
        }

        public static string classIdToString(VmContext vm, int classId)
        {
            return vm.metadata.classTable[classId].fullyQualifiedName;
        }

        public static int clearList(ListImpl a)
        {
            int i = (a.size - 1);
            while ((i >= 0))
            {
                a.array[i] = null;
                i -= 1;
            }
            a.size = 0;
            return 0;
        }

        public static DictImpl cloneDictionary(DictImpl original, DictImpl clone)
        {
            int type = original.keyType;
            int i = 0;
            int size = original.size;
            int kInt = 0;
            string kString = null;
            if ((clone == null))
            {
                clone = new DictImpl(0, type, original.keyClassId, original.valueType, new Dictionary<int, int>(), new Dictionary<string, int>(), new List<Value>(), new List<Value>());
                if ((type == 5))
                {
                    while ((i < size))
                    {
                        clone.stringToIndex[(string)original.keys[i].internalValue] = i;
                        i += 1;
                    }
                }
                else
                {
                    while ((i < size))
                    {
                        if ((type == 8))
                        {
                            kInt = ((ObjectInstance)original.keys[i].internalValue).objectId;
                        }
                        else
                        {
                            kInt = (int)original.keys[i].internalValue;
                        }
                        clone.intToIndex[kInt] = i;
                        i += 1;
                    }
                }
                i = 0;
                while ((i < size))
                {
                    clone.keys.Add(original.keys[i]);
                    clone.values.Add(original.values[i]);
                    i += 1;
                }
            }
            else
            {
                i = 0;
                while ((i < size))
                {
                    if ((type == 5))
                    {
                        kString = (string)original.keys[i].internalValue;
                        if (clone.stringToIndex.ContainsKey(kString))
                        {
                            clone.values[clone.stringToIndex[kString]] = original.values[i];
                        }
                        else
                        {
                            clone.stringToIndex[kString] = clone.values.Count;
                            clone.values.Add(original.values[i]);
                            clone.keys.Add(original.keys[i]);
                        }
                    }
                    else
                    {
                        if ((type == 3))
                        {
                            kInt = (int)original.keys[i].internalValue;
                        }
                        else
                        {
                            kInt = ((ObjectInstance)original.keys[i].internalValue).objectId;
                        }
                        if (clone.intToIndex.ContainsKey(kInt))
                        {
                            clone.values[clone.intToIndex[kInt]] = original.values[i];
                        }
                        else
                        {
                            clone.intToIndex[kInt] = clone.values.Count;
                            clone.values.Add(original.values[i]);
                            clone.keys.Add(original.keys[i]);
                        }
                    }
                    i += 1;
                }
            }
            clone.size = (clone.intToIndex.Count + clone.stringToIndex.Count);
            return clone;
        }

        public static int[] createInstanceType(int classId)
        {
            int[] o = new int[2];
            o[0] = 8;
            o[1] = classId;
            return o;
        }

        public static VmContext createVm(string rawByteCode, string resourceManifest, string imageAtlasManifest)
        {
            VmGlobals globals = initializeConstantValues();
            ResourceDB resources = resourceManagerInitialize(globals, resourceManifest, imageAtlasManifest);
            Code byteCode = initializeByteCode(rawByteCode);
            Value[] localsStack = new Value[10];
            int[] localsStackSet = new int[10];
            int i = 0;
            i = (localsStack.Length - 1);
            while ((i >= 0))
            {
                localsStack[i] = null;
                localsStackSet[i] = 0;
                i -= 1;
            }
            StackFrame stack = new StackFrame(0, 1, 0, 0, null, false, null, 0, 0, 1, 0, null, null, null);
            ExecutionContext executionContext = new ExecutionContext(0, stack, 0, 100, new Value[100], localsStack, localsStackSet, 1, 0, false, null, false, 0, null);
            Dictionary<int, ExecutionContext> executionContexts = new Dictionary<int, ExecutionContext>();
            executionContexts[0] = executionContext;
            VmContext vm = new VmContext(executionContexts, executionContext.id, byteCode, new SymbolData(new List<Token>[byteCode.ops.Length], null, new List<string>(), null, null, new Dictionary<int, List<string>>(), new Dictionary<int, List<string>>()), new VmMetadata(null, new List<string>(), new Dictionary<string, int>(), null, new List<Value>(), null, new List<Dictionary<int, int>>(), null, new List<Dictionary<string, int>>(), new ClassInfo[100], new FunctionInfo[100], new Dictionary<int, FunctionInfo>(), null, new Dictionary<int, System.Func<VmContext, Value[], Value>>(), -1, new int[10], 0, null, null, new MagicNumbers(0, 0, 0), new Dictionary<string, int>(), new Dictionary<int, Dictionary<int, int>>(), null), 0, false, new List<int>(), null, resources, new List<Value>(), new VmEnvironment(new string[0], false, null, null), new NamedCallbackStore(new List<System.Func<object[], object>>(), new Dictionary<string, Dictionary<string, int>>()), globals, globals.valueNull, globals.boolTrue, globals.boolFalse);
            return vm;
        }

        public static int crypto_bitShiftRight(int value, int amount)
        {
            if ((amount == 0))
            {
                return value;
            }
            value = (value & uint32Hack(65535, 65535));
            int mask = 2147483647;
            if ((value > 0))
            {
                return (value >> amount);
            }
            return (((value >> amount)) & ((mask >> ((amount - 1)))));
        }

        public static int crypto_bitwiseNot(int x)
        {
            return (-x - 1);
        }

        public static Value crypto_digest(VmGlobals globals, ListImpl bytes, int algo)
        {
            int[] byteArray = listImplToBytes(bytes);
            List<int> byteList = new List<int>();
            int i = 0;
            while ((i < byteArray.Length))
            {
                byteList.Add(byteArray[i]);
                i += 1;
            }
            if ((algo == 1))
            {
                return crypto_md5_digest(globals, byteList);
            }
            if ((algo == 2))
            {
                return crypto_sha1_digest(globals, byteList);
            }
            return globals.valueNull;
        }

        public static int crypto_leftRotate(int value, int amt)
        {
            if ((amt == 0))
            {
                return value;
            }
            int a = (value << amt);
            int b = crypto_bitShiftRight(value, (32 - amt));
            int result = (a | b);
            return result;
        }

        public static int crypto_md5_createWordsForBlock(int startIndex, List<int> byteList, int[] mWords)
        {
            int i = 0;
            while ((i < 64))
            {
                mWords[(i >> 2)] = ((byteList[(startIndex + i)]) | ((byteList[(startIndex + i + 1)] << 8)) | ((byteList[(startIndex + i + 2)] << 16)) | ((byteList[(startIndex + i + 3)] << 24)));
                i += 4;
            }
            return 0;
        }

        public static Value crypto_md5_digest(VmGlobals globals, List<int> inputBytes)
        {
            int originalLength = (inputBytes.Count * 8);
            int[] shiftTable = new int[64];
            int[] K = new int[64];
            int i = 0;
            while ((i < 16))
            {
                shiftTable[i] = 7;
                shiftTable[(i + 1)] = 12;
                shiftTable[(i + 2)] = 17;
                shiftTable[(i + 3)] = 22;
                shiftTable[(i + 16)] = 5;
                shiftTable[(i + 17)] = 9;
                shiftTable[(i + 18)] = 14;
                shiftTable[(i + 19)] = 20;
                shiftTable[(i + 32)] = 4;
                shiftTable[(i + 33)] = 11;
                shiftTable[(i + 34)] = 16;
                shiftTable[(i + 35)] = 23;
                shiftTable[(i + 48)] = 6;
                shiftTable[(i + 49)] = 10;
                shiftTable[(i + 50)] = 15;
                shiftTable[(i + 51)] = 21;
                i += 4;
            }
            K[0] = uint32Hack(55146, 42104);
            K[1] = uint32Hack(59591, 46934);
            K[2] = uint32Hack(9248, 28891);
            K[3] = uint32Hack(49597, 52974);
            K[4] = uint32Hack(62844, 4015);
            K[5] = uint32Hack(18311, 50730);
            K[6] = uint32Hack(43056, 17939);
            K[7] = uint32Hack(64838, 38145);
            K[8] = uint32Hack(27008, 39128);
            K[9] = uint32Hack(35652, 63407);
            K[10] = uint32Hack(65535, 23473);
            K[11] = uint32Hack(35164, 55230);
            K[12] = uint32Hack(27536, 4386);
            K[13] = uint32Hack(64920, 29075);
            K[14] = uint32Hack(42617, 17294);
            K[15] = uint32Hack(18868, 2081);
            K[16] = uint32Hack(63006, 9570);
            K[17] = uint32Hack(49216, 45888);
            K[18] = uint32Hack(9822, 23121);
            K[19] = uint32Hack(59830, 51114);
            K[20] = uint32Hack(54831, 4189);
            K[21] = uint32Hack(580, 5203);
            K[22] = uint32Hack(55457, 59009);
            K[23] = uint32Hack(59347, 64456);
            K[24] = uint32Hack(8673, 52710);
            K[25] = uint32Hack(49975, 2006);
            K[26] = uint32Hack(62677, 3463);
            K[27] = uint32Hack(17754, 5357);
            K[28] = uint32Hack(43491, 59653);
            K[29] = uint32Hack(64751, 41976);
            K[30] = uint32Hack(26479, 729);
            K[31] = uint32Hack(36138, 19594);
            K[32] = uint32Hack(65530, 14658);
            K[33] = uint32Hack(34673, 63105);
            K[34] = uint32Hack(28061, 24866);
            K[35] = uint32Hack(64997, 14348);
            K[36] = uint32Hack(42174, 59972);
            K[37] = uint32Hack(19422, 53161);
            K[38] = uint32Hack(63163, 19296);
            K[39] = uint32Hack(48831, 48240);
            K[40] = uint32Hack(10395, 32454);
            K[41] = uint32Hack(60065, 10234);
            K[42] = uint32Hack(54511, 12421);
            K[43] = uint32Hack(1160, 7429);
            K[44] = uint32Hack(55764, 53305);
            K[45] = uint32Hack(59099, 39397);
            K[46] = uint32Hack(8098, 31992);
            K[47] = uint32Hack(50348, 22117);
            K[48] = uint32Hack(62505, 8772);
            K[49] = uint32Hack(17194, 65431);
            K[50] = uint32Hack(43924, 9127);
            K[51] = uint32Hack(64659, 41017);
            K[52] = uint32Hack(25947, 22979);
            K[53] = uint32Hack(36620, 52370);
            K[54] = uint32Hack(65519, 62589);
            K[55] = uint32Hack(34180, 24017);
            K[56] = uint32Hack(28584, 32335);
            K[57] = uint32Hack(65068, 59104);
            K[58] = uint32Hack(41729, 17172);
            K[59] = uint32Hack(19976, 4513);
            K[60] = uint32Hack(63315, 32386);
            K[61] = uint32Hack(48442, 62005);
            K[62] = uint32Hack(10967, 53947);
            K[63] = uint32Hack(60294, 54161);
            int A = uint32Hack(26437, 8961);
            int B = uint32Hack(61389, 43913);
            int C = uint32Hack(39098, 56574);
            int D = uint32Hack(4146, 21622);
            inputBytes.Add(128);
            while (((inputBytes.Count % 64) != 56))
            {
                inputBytes.Add(0);
            }
            inputBytes.Add(((originalLength >> 0) & 255));
            inputBytes.Add(((originalLength >> 8) & 255));
            inputBytes.Add(((originalLength >> 16) & 255));
            inputBytes.Add(((originalLength >> 24) & 255));
            inputBytes.Add(0);
            inputBytes.Add(0);
            inputBytes.Add(0);
            inputBytes.Add(0);
            int[] mWords = new int[16];
            int mask32 = uint32Hack(65535, 65535);
            int chunkIndex = 0;
            while ((chunkIndex < inputBytes.Count))
            {
                crypto_md5_createWordsForBlock(chunkIndex, inputBytes, mWords);
                int A_init = A;
                int B_init = B;
                int C_init = C;
                int D_init = D;
                int j = 0;
                while ((j < 64))
                {
                    A = crypto_md5_magicShuffle(mWords, K, shiftTable, mask32, A, B, C, D, j);
                    D = crypto_md5_magicShuffle(mWords, K, shiftTable, mask32, D, A, B, C, (j | 1));
                    C = crypto_md5_magicShuffle(mWords, K, shiftTable, mask32, C, D, A, B, (j | 2));
                    B = crypto_md5_magicShuffle(mWords, K, shiftTable, mask32, B, C, D, A, (j | 3));
                    j += 4;
                }
                A = (((A_init + A)) & mask32);
                B = (((B_init + B)) & mask32);
                C = (((C_init + C)) & mask32);
                D = (((D_init + D)) & mask32);
                chunkIndex += 64;
            }
            List<Value> output = new List<Value>();
            output.Add(buildInteger(globals, (A & 255)));
            output.Add(buildInteger(globals, ((A >> 8) & 255)));
            output.Add(buildInteger(globals, ((A >> 16) & 255)));
            output.Add(buildInteger(globals, ((A >> 24) & 255)));
            output.Add(buildInteger(globals, (B & 255)));
            output.Add(buildInteger(globals, ((B >> 8) & 255)));
            output.Add(buildInteger(globals, ((B >> 16) & 255)));
            output.Add(buildInteger(globals, ((B >> 24) & 255)));
            output.Add(buildInteger(globals, (C & 255)));
            output.Add(buildInteger(globals, ((C >> 8) & 255)));
            output.Add(buildInteger(globals, ((C >> 16) & 255)));
            output.Add(buildInteger(globals, ((C >> 24) & 255)));
            output.Add(buildInteger(globals, (D & 255)));
            output.Add(buildInteger(globals, ((D >> 8) & 255)));
            output.Add(buildInteger(globals, ((D >> 16) & 255)));
            output.Add(buildInteger(globals, ((D >> 24) & 255)));
            return buildList(output);
        }

        public static int crypto_md5_magicShuffle(int[] mWords, int[] sineValues, int[] shiftValues, int mask32, int a, int b, int c, int d, int counter)
        {
            int roundNumber = (counter >> 4);
            int t = 0;
            int shiftAmount = shiftValues[counter];
            int sineValue = sineValues[counter];
            int mWord = 0;
            if ((roundNumber == 0))
            {
                t = (((b & c)) | ((crypto_bitwiseNot(b) & d)));
                mWord = mWords[counter];
            }
            else if ((roundNumber == 1))
            {
                t = (((b & d)) | ((c & crypto_bitwiseNot(d))));
                mWord = mWords[((((5 * counter) + 1)) & 15)];
            }
            else if ((roundNumber == 2))
            {
                t = (b ^ c ^ d);
                mWord = mWords[((((3 * counter) + 5)) & 15)];
            }
            else
            {
                t = (c ^ ((b | crypto_bitwiseNot(d))));
                mWord = mWords[(((7 * counter)) & 15)];
            }
            t = (((a + t + mWord + sineValue)) & mask32);
            t = (b + crypto_leftRotate(t, shiftAmount));
            return (t & mask32);
        }

        public static int crypto_sha1_createWordsForBlock(int startIndex, List<int> byteList, int[] mWords)
        {
            int i = 0;
            while ((i < 64))
            {
                mWords[(i >> 2)] = (((byteList[(startIndex + i)] << 24)) | ((byteList[(startIndex + i + 1)] << 16)) | ((byteList[(startIndex + i + 2)] << 8)) | (byteList[(startIndex + i + 3)]));
                i += 4;
            }
            return 0;
        }

        public static Value crypto_sha1_digest(VmGlobals globals, List<int> inputBytes)
        {
            int originalLength = (inputBytes.Count * 8);
            int h0 = uint32Hack(26437, 8961);
            int h1 = uint32Hack(61389, 43913);
            int h2 = uint32Hack(39098, 56574);
            int h3 = uint32Hack(4146, 21622);
            int h4 = uint32Hack(50130, 57840);
            inputBytes.Add(128);
            while (((inputBytes.Count % 64) != 56))
            {
                inputBytes.Add(0);
            }
            inputBytes.Add(0);
            inputBytes.Add(0);
            inputBytes.Add(0);
            inputBytes.Add(0);
            inputBytes.Add(((originalLength >> 24) & 255));
            inputBytes.Add(((originalLength >> 16) & 255));
            inputBytes.Add(((originalLength >> 8) & 255));
            inputBytes.Add(((originalLength >> 0) & 255));
            int[] mWords = new int[80];
            int mask32 = uint32Hack(65535, 65535);
            int f = 0;
            int temp = 0;
            int k = 0;
            int[] kValues = new int[4];
            kValues[0] = uint32Hack(23170, 31129);
            kValues[1] = uint32Hack(28377, 60321);
            kValues[2] = uint32Hack(36635, 48348);
            kValues[3] = uint32Hack(51810, 49622);
            int chunkIndex = 0;
            while ((chunkIndex < inputBytes.Count))
            {
                crypto_sha1_createWordsForBlock(chunkIndex, inputBytes, mWords);
                int i = 16;
                while ((i < 80))
                {
                    mWords[i] = crypto_leftRotate((mWords[(i - 3)] ^ mWords[(i - 8)] ^ mWords[(i - 14)] ^ mWords[(i - 16)]), 1);
                    i += 1;
                }
                int a = h0;
                int b = h1;
                int c = h2;
                int d = h3;
                int e = h4;
                int j = 0;
                while ((j < 80))
                {
                    if ((j < 20))
                    {
                        f = (((b & c)) | ((crypto_bitwiseNot(b) & d)));
                        k = kValues[0];
                    }
                    else if ((j < 40))
                    {
                        f = (b ^ c ^ d);
                        k = kValues[1];
                    }
                    else if ((j < 60))
                    {
                        f = (((b & c)) | ((b & d)) | ((c & d)));
                        k = kValues[2];
                    }
                    else
                    {
                        f = (b ^ c ^ d);
                        k = kValues[3];
                    }
                    temp = (crypto_leftRotate(a, 5) + f + e + k + mWords[j]);
                    e = d;
                    d = c;
                    c = crypto_leftRotate(b, 30);
                    b = a;
                    a = (temp & mask32);
                    j += 1;
                }
                h0 = (((h0 + a)) & mask32);
                h1 = (((h1 + b)) & mask32);
                h2 = (((h2 + c)) & mask32);
                h3 = (((h3 + d)) & mask32);
                h4 = (((h4 + e)) & mask32);
                chunkIndex += 64;
            }
            List<Value> output = new List<Value>();
            output.Add(buildInteger(globals, ((h0 >> 24) & 255)));
            output.Add(buildInteger(globals, ((h0 >> 16) & 255)));
            output.Add(buildInteger(globals, ((h0 >> 8) & 255)));
            output.Add(buildInteger(globals, (h0 & 255)));
            output.Add(buildInteger(globals, ((h1 >> 24) & 255)));
            output.Add(buildInteger(globals, ((h1 >> 16) & 255)));
            output.Add(buildInteger(globals, ((h1 >> 8) & 255)));
            output.Add(buildInteger(globals, (h1 & 255)));
            output.Add(buildInteger(globals, ((h2 >> 24) & 255)));
            output.Add(buildInteger(globals, ((h2 >> 16) & 255)));
            output.Add(buildInteger(globals, ((h2 >> 8) & 255)));
            output.Add(buildInteger(globals, (h2 & 255)));
            output.Add(buildInteger(globals, ((h3 >> 24) & 255)));
            output.Add(buildInteger(globals, ((h3 >> 16) & 255)));
            output.Add(buildInteger(globals, ((h3 >> 8) & 255)));
            output.Add(buildInteger(globals, (h3 & 255)));
            output.Add(buildInteger(globals, ((h4 >> 24) & 255)));
            output.Add(buildInteger(globals, ((h4 >> 16) & 255)));
            output.Add(buildInteger(globals, ((h4 >> 8) & 255)));
            output.Add(buildInteger(globals, (h4 & 255)));
            return buildList(output);
        }

        public static object DateTime_getNativeTimezone(Value value)
        {
            ObjectInstance tzObj = (ObjectInstance)value.internalValue;
            if ((tzObj.nativeData == null))
            {
                return null;
            }
            return tzObj.nativeData[0];
        }

        public static Value DateTime_getUtcOffsetAt(VmContext vm, Value arg1, Value arg2)
        {
            object nativeTz = DateTime_getNativeTimezone(arg1);
            int unixTime = (int)arg2.internalValue;
            int offsetSeconds = DateTimeHelper.GetUtcOffsetAt(nativeTz, unixTime);
            return buildInteger(vm.globals, offsetSeconds);
        }

        public static Value DateTime_initTimeZone(VmContext vm, Value arg1, Value arg2, Value arg3)
        {
            ObjectInstance timezone = (ObjectInstance)arg1.internalValue;
            timezone.nativeData = new object[1];
            object nativeTzRef = null;
            string readableName = null;
            int offsetFromUtc = 0;
            int isDstObserved = 0;
            string fingerprint = null;
            if ((arg2.type == 1))
            {
                string[] strOut = PST_StringBuffer16;
                int[] intOut = PST_IntBuffer16;
                nativeTzRef = DateTimeHelper.GetDataForLocalTimeZone(strOut, intOut);
                readableName = strOut[0];
                fingerprint = strOut[1];
                offsetFromUtc = intOut[0];
                isDstObserved = intOut[1];
            }
            else
            {
                return vm.globalNull;
            }
            timezone.nativeData = new object[5];
            timezone.nativeData[0] = nativeTzRef;
            timezone.nativeData[1] = readableName;
            timezone.nativeData[2] = offsetFromUtc;
            timezone.nativeData[3] = (isDstObserved == 1);
            timezone.nativeData[4] = fingerprint;
            List<Value> values = new List<Value>();
            values.Add(buildString(vm.globals, readableName));
            values.Add(buildInteger(vm.globals, offsetFromUtc));
            values.Add(buildBoolean(vm.globals, (isDstObserved == 1)));
            values.Add(buildString(vm.globals, fingerprint));
            return buildList(values);
        }

        public static Value DateTime_initTimeZoneList(VmContext vm, Value arg1)
        {
            ObjectInstance obj = (ObjectInstance)arg1.internalValue;
            obj.nativeData = new object[1];
            object[] timezones = DateTimeHelper.InitializeTimeZoneList();
            obj.nativeData[0] = timezones;
            int length = timezones.Length;
            return buildInteger(vm.globals, length);
        }

        public static Value DateTime_isDstOccurringAt(VmContext vm, Value arg1, Value arg2)
        {
            object nativeTz = DateTime_getNativeTimezone(arg1);
            int unixtime = (int)arg2.internalValue;
            return buildBoolean(vm.globals, DateTimeHelper.IsDstOccurringAt(nativeTz, unixtime));
        }

        public static Value DateTime_parseDate(VmContext vm, Value arg1, Value arg2, Value arg3, Value arg4, Value arg5, Value arg6, Value arg7)
        {
            int year = (int)arg1.internalValue;
            int month = (int)arg2.internalValue;
            int day = (int)arg3.internalValue;
            int hour = (int)arg4.internalValue;
            int minute = (int)arg5.internalValue;
            int microseconds = (int)arg6.internalValue;
            object nullableTimeZone = DateTime_getNativeTimezone(arg7);
            if (((year >= 1970) && (year < 2100) && (month >= 1) && (month <= 12) && (day >= 1) && (day <= 31) && (hour >= 0) && (hour < 24) && (minute >= 0) && (minute < 60) && (microseconds >= 0) && (microseconds < 60000000)))
            {
                int[] intOut = PST_IntBuffer16;
                DateTimeHelper.ParseDate(intOut, nullableTimeZone, year, month, day, hour, minute, microseconds);
                if ((intOut[0] == 1))
                {
                    double unixFloat = (intOut[1] + (intOut[2]) / (1000000.0));
                    return buildFloat(vm.globals, unixFloat);
                }
            }
            return vm.globalNull;
        }

        public static Value DateTime_unixToStructured(VmContext vm, Value arg1, Value arg2)
        {
            double unixTime = (double)arg1.internalValue;
            object nullableTimeZone = DateTime_getNativeTimezone(arg2);
            List<Value> output = new List<Value>();
            int[] intOut = PST_IntBuffer16;
            bool success = DateTimeHelper.UnixToStructured(intOut, nullableTimeZone, unixTime);
            if (!success)
            {
                return vm.globalNull;
            }
            int i = 0;
            while ((i < 9))
            {
                output.Add(buildInteger(vm.globals, intOut[i]));
                i += 1;
            }
            return buildList(output);
        }

        public static int debuggerClearBreakpoint(VmContext vm, int id)
        {
            return 0;
        }

        public static int debuggerFindPcForLine(VmContext vm, string path, int line)
        {
            return -1;
        }

        public static int debuggerSetBreakpoint(VmContext vm, string path, int line)
        {
            return -1;
        }

        public static bool debugSetStepOverBreakpoint(VmContext vm)
        {
            return false;
        }

        public static int defOriginalCodeImpl(VmContext vm, int[] row, string fileContents)
        {
            int fileId = row[0];
            List<string> codeLookup = vm.symbolData.sourceCodeBuilder;
            while ((codeLookup.Count <= fileId))
            {
                codeLookup.Add(null);
            }
            codeLookup[fileId] = fileContents;
            return 0;
        }

        public static string dictKeyInfoToString(VmContext vm, DictImpl dict)
        {
            if ((dict.keyType == 5))
            {
                return "string";
            }
            if ((dict.keyType == 3))
            {
                return "int";
            }
            if ((dict.keyClassId == 0))
            {
                return "instance";
            }
            return classIdToString(vm, dict.keyClassId);
        }

        public static int doEqualityComparisonAndReturnCode(Value a, Value b)
        {
            int leftType = a.type;
            int rightType = b.type;
            if ((leftType == rightType))
            {
                int output = 0;
                switch (leftType)
                {
                    case 1:
                        output = 1;
                        break;
                    case 3:
                        if (((int)a.internalValue == (int)b.internalValue))
                        {
                            output = 1;
                        }
                        break;
                    case 4:
                        if (((double)a.internalValue == (double)b.internalValue))
                        {
                            output = 1;
                        }
                        break;
                    case 2:
                        if (((bool)a.internalValue == (bool)b.internalValue))
                        {
                            output = 1;
                        }
                        break;
                    case 5:
                        if (((string)a.internalValue == (string)b.internalValue))
                        {
                            output = 1;
                        }
                        break;
                    case 6:
                        if (((object)a.internalValue == (object)b.internalValue))
                        {
                            output = 1;
                        }
                        break;
                    case 7:
                        if (((object)a.internalValue == (object)b.internalValue))
                        {
                            output = 1;
                        }
                        break;
                    case 8:
                        if (((object)a.internalValue == (object)b.internalValue))
                        {
                            output = 1;
                        }
                        break;
                    case 9:
                        FunctionPointer f1 = (FunctionPointer)a.internalValue;
                        FunctionPointer f2 = (FunctionPointer)b.internalValue;
                        if ((f1.functionId == f2.functionId))
                        {
                            if (((f1.type == 2) || (f1.type == 4)))
                            {
                                if ((doEqualityComparisonAndReturnCode(f1.context, f2.context) == 1))
                                {
                                    output = 1;
                                }
                            }
                            else
                            {
                                output = 1;
                            }
                        }
                        break;
                    case 10:
                        ClassValue c1 = (ClassValue)a.internalValue;
                        ClassValue c2 = (ClassValue)b.internalValue;
                        if ((c1.classId == c2.classId))
                        {
                            output = 1;
                        }
                        break;
                    default:
                        output = 2;
                        break;
                }
                return output;
            }
            if ((rightType == 1))
            {
                return 0;
            }
            if (((leftType == 3) && (rightType == 4)))
            {
                if (((int)a.internalValue == (double)b.internalValue))
                {
                    return 1;
                }
            }
            else if (((leftType == 4) && (rightType == 3)))
            {
                if (((double)a.internalValue == (int)b.internalValue))
                {
                    return 1;
                }
            }
            return 0;
        }

        public static Value doExponentMath(VmGlobals globals, double b, double e, bool preferInt)
        {
            if ((e == 0.0))
            {
                if (preferInt)
                {
                    return globals.intOne;
                }
                return globals.floatOne;
            }
            if ((b == 0.0))
            {
                if (preferInt)
                {
                    return globals.intZero;
                }
                return globals.floatZero;
            }
            double r = 0.0;
            if ((b < 0))
            {
                if (((e >= 0) && (e < 1)))
                {
                    return null;
                }
                if (((e % 1.0) == 0.0))
                {
                    int eInt = (int)e;
                    r = (0.0 + System.Math.Pow(b, eInt));
                }
                else
                {
                    return null;
                }
            }
            else
            {
                r = System.Math.Pow(b, e);
            }
            if (preferInt)
            {
                r = fixFuzzyFloatPrecision(r);
                if (((r % 1.0) == 0.0))
                {
                    return buildInteger(globals, (int)r);
                }
            }
            return buildFloat(globals, r);
        }

        public static string encodeBreakpointData(VmContext vm, BreakpointInfo breakpoint, int pc)
        {
            return null;
        }

        public static InterpreterResult errorResult(string error)
        {
            return new InterpreterResult(3, error, 0.0, 0, false, "");
        }

        public static bool EX_AssertionFailed(ExecutionContext ec, string exMsg)
        {
            return generateException2(ec, 2, exMsg);
        }

        public static bool EX_DivisionByZero(ExecutionContext ec, string exMsg)
        {
            return generateException2(ec, 3, exMsg);
        }

        public static bool EX_Fatal(ExecutionContext ec, string exMsg)
        {
            return generateException2(ec, 0, exMsg);
        }

        public static bool EX_IndexOutOfRange(ExecutionContext ec, string exMsg)
        {
            return generateException2(ec, 4, exMsg);
        }

        public static bool EX_InvalidArgument(ExecutionContext ec, string exMsg)
        {
            return generateException2(ec, 5, exMsg);
        }

        public static bool EX_InvalidAssignment(ExecutionContext ec, string exMsg)
        {
            return generateException2(ec, 6, exMsg);
        }

        public static bool EX_InvalidInvocation(ExecutionContext ec, string exMsg)
        {
            return generateException2(ec, 7, exMsg);
        }

        public static bool EX_InvalidKey(ExecutionContext ec, string exMsg)
        {
            return generateException2(ec, 8, exMsg);
        }

        public static bool EX_KeyNotFound(ExecutionContext ec, string exMsg)
        {
            return generateException2(ec, 9, exMsg);
        }

        public static bool EX_NullReference(ExecutionContext ec, string exMsg)
        {
            return generateException2(ec, 10, exMsg);
        }

        public static bool EX_UnassignedVariable(ExecutionContext ec, string exMsg)
        {
            return generateException2(ec, 11, exMsg);
        }

        public static bool EX_UnknownField(ExecutionContext ec, string exMsg)
        {
            return generateException2(ec, 12, exMsg);
        }

        public static bool EX_UnsupportedOperation(ExecutionContext ec, string exMsg)
        {
            return generateException2(ec, 13, exMsg);
        }

        public static int finalizeInitializationImpl(VmContext vm, string projectId, int localeCount)
        {
            vm.symbolData.sourceCode = vm.symbolData.sourceCodeBuilder.ToArray();
            vm.symbolData.sourceCodeBuilder = null;
            vm.metadata.magicNumbers.totalLocaleCount = localeCount;
            vm.metadata.identifiers = vm.metadata.identifiersBuilder.ToArray();
            vm.metadata.literalTable = vm.metadata.literalTableBuilder.ToArray();
            vm.metadata.globalNameIdToPrimitiveMethodName = primitiveMethodsInitializeLookup(vm.metadata.invIdentifiers);
            vm.funcArgs = new Value[vm.metadata.identifiers.Length];
            vm.metadata.projectId = projectId;
            vm.metadata.identifiersBuilder = null;
            vm.metadata.literalTableBuilder = null;
            vm.initializationComplete = true;
            return 0;
        }

        public static double fixFuzzyFloatPrecision(double x)
        {
            if (((x % 1) != 0))
            {
                double u = (x % 1);
                if ((u < 0))
                {
                    u += 1.0;
                }
                bool roundDown = false;
                if ((u > 0.9999999999))
                {
                    roundDown = true;
                    x += 0.1;
                }
                else if ((u < 0.00000000002250000000))
                {
                    roundDown = true;
                }
                if (roundDown)
                {
                    if ((false || (x > 0)))
                    {
                        x = ((int)x + 0.0);
                    }
                    else
                    {
                        x = ((int)x - 1.0);
                    }
                }
            }
            return x;
        }

        public static int[][] generateEsfData(int byteCodeLength, int[] esfArgs)
        {
            int[][] output = new int[byteCodeLength][];
            List<int[]> esfTokenStack = new List<int[]>();
            int[] esfTokenStackTop = null;
            int esfArgIterator = 0;
            int esfArgLength = esfArgs.Length;
            int j = 0;
            int pc = 0;
            while ((pc < byteCodeLength))
            {
                if (((esfArgIterator < esfArgLength) && (pc == esfArgs[esfArgIterator])))
                {
                    esfTokenStackTop = new int[2];
                    j = 1;
                    while ((j < 3))
                    {
                        esfTokenStackTop[(j - 1)] = esfArgs[(esfArgIterator + j)];
                        j += 1;
                    }
                    esfTokenStack.Add(esfTokenStackTop);
                    esfArgIterator += 3;
                }
                while (((esfTokenStackTop != null) && (esfTokenStackTop[1] <= pc)))
                {
                    esfTokenStack.RemoveAt(esfTokenStack.Count - 1);
                    if ((esfTokenStack.Count == 0))
                    {
                        esfTokenStackTop = null;
                    }
                    else
                    {
                        esfTokenStackTop = esfTokenStack[(esfTokenStack.Count - 1)];
                    }
                }
                output[pc] = esfTokenStackTop;
                pc += 1;
            }
            return output;
        }

        public static InterpreterResult generateException(VmContext vm, StackFrame stack, int pc, int valueStackSize, ExecutionContext ec, int type, string message)
        {
            ec.currentValueStackSize = valueStackSize;
            stack.pc = pc;
            MagicNumbers mn = vm.metadata.magicNumbers;
            int generateExceptionFunctionId = mn.coreGenerateExceptionFunctionId;
            FunctionInfo functionInfo = vm.metadata.functionTable[generateExceptionFunctionId];
            pc = functionInfo.pc;
            if ((ec.localsStack.Length <= (functionInfo.localsSize + stack.localsStackOffsetEnd)))
            {
                increaseLocalsStackCapacity(ec, functionInfo.localsSize);
            }
            int localsIndex = stack.localsStackOffsetEnd;
            int localsStackSetToken = (ec.localsStackSetToken + 1);
            ec.localsStackSetToken = localsStackSetToken;
            ec.localsStack[localsIndex] = buildInteger(vm.globals, type);
            ec.localsStack[(localsIndex + 1)] = buildString(vm.globals, message);
            ec.localsStackSet[localsIndex] = localsStackSetToken;
            ec.localsStackSet[(localsIndex + 1)] = localsStackSetToken;
            ec.stackTop = new StackFrame((pc + 1), localsStackSetToken, stack.localsStackOffsetEnd, (stack.localsStackOffsetEnd + functionInfo.localsSize), stack, false, null, valueStackSize, 0, (stack.depth + 1), 0, null, null, null);
            return new InterpreterResult(5, null, 0.0, 0, false, "");
        }

        public static bool generateException2(ExecutionContext ec, int exceptionType, string exMsg)
        {
            ec.activeInterrupt = new Interrupt(1, exceptionType, exMsg, 0.0, null);
            return true;
        }

        public static Value generatePrimitiveMethodReference(int[] lookup, int globalNameId, Value context)
        {
            int functionId = resolvePrimitiveMethodName2(lookup, context.type, globalNameId);
            if ((functionId < 0))
            {
                return null;
            }
            return new Value(9, new FunctionPointer(4, context, 0, functionId, null));
        }

        public static List<Token> generateTokenListFromPcs(VmContext vm, List<int> pcs)
        {
            List<Token> output = new List<Token>();
            List<Token>[] tokensByPc = vm.symbolData.tokenData;
            Token token = null;
            int i = 0;
            while ((i < pcs.Count))
            {
                List<Token> localTokens = tokensByPc[pcs[i]];
                if ((localTokens == null))
                {
                    if ((output.Count > 0))
                    {
                        output.Add(null);
                    }
                }
                else
                {
                    token = localTokens[0];
                    output.Add(token);
                }
                i += 1;
            }
            return output;
        }

        public static string getBinaryOpFromId(int id)
        {
            switch (id)
            {
                case 0:
                    return "+";
                case 1:
                    return "-";
                case 2:
                    return "*";
                case 3:
                    return "/";
                case 4:
                    return "%";
                case 5:
                    return "**";
                case 6:
                    return "&";
                case 7:
                    return "|";
                case 8:
                    return "^";
                case 9:
                    return "<<";
                case 10:
                    return ">>";
                case 11:
                    return "<";
                case 12:
                    return "<=";
                case 13:
                    return ">";
                case 14:
                    return ">=";
                default:
                    return "unknown";
            }
        }

        public static ClassInfo[] getClassTable(VmContext vm, int classId)
        {
            ClassInfo[] oldTable = vm.metadata.classTable;
            int oldLength = oldTable.Length;
            if ((classId < oldLength))
            {
                return oldTable;
            }
            int newLength = (oldLength * 2);
            if ((classId >= newLength))
            {
                newLength = (classId + 100);
            }
            ClassInfo[] newTable = new ClassInfo[newLength];
            int i = (oldLength - 1);
            while ((i >= 0))
            {
                newTable[i] = oldTable[i];
                i -= 1;
            }
            vm.metadata.classTable = newTable;
            return newTable;
        }

        public static ExecutionContext getExecutionContext(VmContext vm, int id)
        {
            if (vm.executionContexts.ContainsKey(id))
            {
                return vm.executionContexts[id];
            }
            return null;
        }

        public static string getExponentErrorMsg(VmContext vm, Value b, Value e)
        {
            return string.Join("", new string[] { "Invalid values for exponent computation. Base: ",valueToString(vm, b),", Power: ",valueToString(vm, e) });
        }

        public static double getFloat(Value num)
        {
            if ((num.type == 4))
            {
                return (double)num.internalValue;
            }
            return ((int)num.internalValue + 0.0);
        }

        public static FunctionInfo[] getFunctionTable(VmContext vm, int functionId)
        {
            FunctionInfo[] oldTable = vm.metadata.functionTable;
            int oldLength = oldTable.Length;
            if ((functionId < oldLength))
            {
                return oldTable;
            }
            int newLength = (oldLength * 2);
            if ((functionId >= newLength))
            {
                newLength = (functionId + 100);
            }
            FunctionInfo[] newTable = new FunctionInfo[newLength];
            int i = 0;
            while ((i < oldLength))
            {
                newTable[i] = oldTable[i];
                i += 1;
            }
            vm.metadata.functionTable = newTable;
            return newTable;
        }

        public static Value getItemFromList(ListImpl list, int i)
        {
            return list.array[i];
        }

        public static int getNamedCallbackId(VmContext vm, string scope, string functionName)
        {
            return getNamedCallbackIdImpl(vm, scope, functionName, false);
        }

        public static int getNamedCallbackIdImpl(VmContext vm, string scope, string functionName, bool allocIfMissing)
        {
            Dictionary<string, Dictionary<string, int>> lookup = vm.namedCallbacks.callbackIdLookup;
            Dictionary<string, int> idsForScope = null;
            if (!lookup.TryGetValue(scope, out idsForScope)) idsForScope = null;
            if ((idsForScope == null))
            {
                idsForScope = new Dictionary<string, int>();
                lookup[scope] = idsForScope;
            }
            int id = -1;
            if (!idsForScope.TryGetValue(functionName, out id)) id = -1;
            if (((id == -1) && allocIfMissing))
            {
                id = vm.namedCallbacks.callbacksById.Count;
                vm.namedCallbacks.callbacksById.Add(null);
                idsForScope[functionName] = id;
            }
            return id;
        }

        public static object getNativeDataItem(Value objValue, int index)
        {
            ObjectInstance obj = (ObjectInstance)objValue.internalValue;
            return obj.nativeData[index];
        }

        public static string getTypeFromId(int id)
        {
            switch (id)
            {
                case 1:
                    return "null";
                case 2:
                    return "boolean";
                case 3:
                    return "integer";
                case 4:
                    return "float";
                case 5:
                    return "string";
                case 6:
                    return "list";
                case 7:
                    return "dictionary";
                case 8:
                    return "instance";
                case 9:
                    return "function";
            }
            return null;
        }

        public static double getVmReinvokeDelay(InterpreterResult result)
        {
            return result.reinvokeDelay;
        }

        public static string getVmResultAssemblyInfo(InterpreterResult result)
        {
            return result.loadAssemblyInformation;
        }

        public static int getVmResultExecId(InterpreterResult result)
        {
            return result.executionContextId;
        }

        public static int getVmResultStatus(InterpreterResult result)
        {
            return result.status;
        }

        public static int ImageHelper_fromBytes(VmGlobals globals, ObjectInstance bmp, bool isB64, Value rawData, ListImpl sizeOut, Value callback)
        {
            object data = null;
            if (isB64)
            {
                if ((rawData.type != 5))
                {
                    return 1;
                }
                data = rawData.internalValue;
            }
            else
            {
                if ((rawData.type != 6))
                {
                    return 1;
                }
                data = listImplToBytes((ListImpl)rawData.internalValue);
                if ((data == null))
                {
                    return 1;
                }
            }
            int[] sizeOutInt = new int[2];
            bmp.nativeData = new object[1];
            bool isSync = ImageUtil.FromBytes((int[])data, sizeOutInt, bmp.nativeData);
            if (isSync)
            {
                if ((bmp.nativeData[0] == null))
                {
                    return 2;
                }
                addToList(sizeOut, buildInteger(globals, sizeOutInt[0]));
                addToList(sizeOut, buildInteger(globals, sizeOutInt[1]));
                return 0;
            }
            return 3;
        }

        public static void ImageHelper_GetChunkSync(ObjectInstance o, int cid)
        {
            o.nativeData = new object[1];
            o.nativeData[0] = ImageUtil.GetChunk(cid);
        }

        public static int ImageHelper_GetPixel(Value[] nums, ObjectInstance bmp, ObjectInstance edit, Value xv, Value yv, ListImpl pOut, int[] arr)
        {
            if (((xv.type != 3) || (yv.type != 3)))
            {
                return 1;
            }
            object e = null;
            if ((edit != null))
            {
                e = edit.nativeData;
            }
            ImageUtil.GetPixel(bmp.nativeData[0], e, (int)xv.internalValue, (int)yv.internalValue, arr);
            if ((arr[4] == 0))
            {
                return 2;
            }
            if ((pOut.capacity < 4))
            {
                pOut.capacity = 4;
                pOut.array = new Value[4];
            }
            pOut.size = 4;
            pOut.array[0] = nums[arr[0]];
            pOut.array[1] = nums[arr[1]];
            pOut.array[2] = nums[arr[2]];
            pOut.array[3] = nums[arr[3]];
            return 0;
        }

        public static void ImageHelper_ImageBlit(ObjectInstance target, ObjectInstance src, int sx, int sy, int sw, int sh, int tx, int ty, int tw, int th)
        {
            ImageUtil.Blit(target.nativeData[0], src.nativeData[0], sx, sy, sw, sh, tx, ty, tw, th);
        }

        public static void ImageHelper_ImageCreate(ObjectInstance o, int w, int h)
        {
            o.nativeData = new object[1];
            o.nativeData[0] = ImageUtil.NewBitmap(w, h);
        }

        public static Value ImageHelper_ImageEncode(VmGlobals globals, object bmp, int format)
        {
            bool[] o = new bool[1];
            object result = ImageUtil.Encode(bmp, format, o);
            if (o[0])
            {
                return buildString(globals, (string)result);
            }
            return bytesToListValue(globals, (int[])result);
        }

        public static void ImageHelper_LoadChunk(int chunkId, ListImpl allChunkIds, Value loadedCallback)
        {
            int size = allChunkIds.size;
            int[] chunkIds = new int[size];
            int i = 0;
            while ((i < size))
            {
                chunkIds[i] = (int)allChunkIds.array[i].internalValue;
                ++i;
            }
            ImageUtil.ChunkLoadAsync(chunkId, chunkIds, loadedCallback);
        }

        public static void ImageHelper_Scale(ObjectInstance src, ObjectInstance dest, int newWidth, int newHeight, int algo)
        {
            dest.nativeData = new object[1];
            dest.nativeData[0] = ImageUtil.Scale(src.nativeData[0], newWidth, newHeight, algo);
        }

        public static void ImageHelper_SessionFinish(ObjectInstance edit, ObjectInstance bmp)
        {
            ImageUtil.EndEditSession(edit.nativeData[0], bmp.nativeData[0]);
        }

        public static void ImageHelper_SessionStart(ObjectInstance edit, ObjectInstance bmp)
        {
            edit.nativeData = new object[1];
            edit.nativeData[0] = ImageUtil.StartEditSession(bmp.nativeData[0]);
        }

        public static int ImageHelper_SetPixel(ObjectInstance edit, Value xv1, Value yv1, Value xv2, Value yv2, Value rOrList, Value gv, Value bv, Value av)
        {
            if (((xv1.type != 3) || (yv1.type != 3)))
            {
                return 1;
            }
            if (((xv2.type != 3) || (yv2.type != 3)))
            {
                return 1;
            }
            int r = 0;
            int g = 0;
            int b = 0;
            int a = 255;
            if ((rOrList.type == 6))
            {
                ListImpl color = (ListImpl)rOrList.internalValue;
                r = color.size;
                if ((r == 4))
                {
                    av = color.array[3];
                }
                else if ((r != 3))
                {
                    return 5;
                }
                rOrList = color.array[0];
                gv = color.array[1];
                bv = color.array[2];
            }
            else if ((rOrList.type != 3))
            {
                return 3;
            }
            if (((rOrList.type != 3) || (gv.type != 3) || (bv.type != 3) || (av.type != 3)))
            {
                return 3;
            }
            r = (int)rOrList.internalValue;
            g = (int)gv.internalValue;
            b = (int)bv.internalValue;
            a = (int)av.internalValue;
            if (((r < 0) || (r > 255) || (g < 0) || (g > 255) || (b < 0) || (b > 255) || (a < 0) || (a > 255)))
            {
                return 4;
            }
            bool outOfRange = ImageUtil.SetPixel(edit.nativeData[0], (int)xv1.internalValue, (int)yv1.internalValue, (int)xv2.internalValue, (int)yv2.internalValue, r, g, b, a);
            if (outOfRange)
            {
                return 2;
            }
            return 0;
        }

        public static void increaseListCapacity(ListImpl list)
        {
            int oldCapacity = list.capacity;
            int newCapacity = (oldCapacity * 2);
            if ((newCapacity < 8))
            {
                newCapacity = 8;
            }
            Value[] newArr = new Value[newCapacity];
            Value[] oldArr = list.array;
            int i = 0;
            while ((i < oldCapacity))
            {
                newArr[i] = oldArr[i];
                i += 1;
            }
            list.capacity = newCapacity;
            list.array = newArr;
        }

        public static int increaseLocalsStackCapacity(ExecutionContext ec, int newScopeSize)
        {
            Value[] oldLocals = ec.localsStack;
            int[] oldSetIndicator = ec.localsStackSet;
            int oldCapacity = oldLocals.Length;
            int newCapacity = ((oldCapacity * 2) + newScopeSize);
            Value[] newLocals = new Value[newCapacity];
            int[] newSetIndicator = new int[newCapacity];
            int i = 0;
            while ((i < oldCapacity))
            {
                newLocals[i] = oldLocals[i];
                newSetIndicator[i] = oldSetIndicator[i];
                i += 1;
            }
            ec.localsStack = newLocals;
            ec.localsStackSet = newSetIndicator;
            return 0;
        }

        public static int initFileNameSymbolData(VmContext vm)
        {
            SymbolData symbolData = vm.symbolData;
            if ((symbolData == null))
            {
                return 0;
            }
            if ((symbolData.fileNameById == null))
            {
                int i = 0;
                string[] filenames = new string[symbolData.sourceCode.Length];
                Dictionary<string, int> fileIdByPath = new Dictionary<string, int>();
                i = 0;
                while ((i < filenames.Length))
                {
                    string sourceCode = symbolData.sourceCode[i];
                    if ((sourceCode != null))
                    {
                        int colon = sourceCode.IndexOf("\n");
                        if ((colon != -1))
                        {
                            string filename = sourceCode.Substring(0, colon);
                            filenames[i] = filename;
                            fileIdByPath[filename] = i;
                        }
                    }
                    i += 1;
                }
                symbolData.fileNameById = filenames;
                symbolData.fileIdByName = fileIdByPath;
            }
            return 0;
        }

        public static Code initializeByteCode(string raw)
        {
            int[] index = new int[1];
            index[0] = 0;
            int length = raw.Length;
            string header = read_till(index, raw, length, '@');
            if ((header != "CRAYON"))
            {
            }
            string alphaNums = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
            int opCount = read_integer(index, raw, length, alphaNums);
            int[] ops = new int[opCount];
            int[][] iargs = new int[opCount][];
            string[] sargs = new string[opCount];
            char c = ' ';
            int argc = 0;
            int j = 0;
            string stringarg = null;
            bool stringPresent = false;
            int iarg = 0;
            int[] iarglist = null;
            int i = 0;
            i = 0;
            while ((i < opCount))
            {
                c = raw[index[0]];
                index[0] = (index[0] + 1);
                argc = 0;
                stringPresent = true;
                if ((c == '!'))
                {
                    argc = 1;
                }
                else if ((c == '&'))
                {
                    argc = 2;
                }
                else if ((c == '*'))
                {
                    argc = 3;
                }
                else
                {
                    if ((c != '~'))
                    {
                        stringPresent = false;
                        index[0] = (index[0] - 1);
                    }
                    argc = read_integer(index, raw, length, alphaNums);
                }
                iarglist = new int[(argc - 1)];
                j = 0;
                while ((j < argc))
                {
                    iarg = read_integer(index, raw, length, alphaNums);
                    if ((j == 0))
                    {
                        ops[i] = iarg;
                    }
                    else
                    {
                        iarglist[(j - 1)] = iarg;
                    }
                    j += 1;
                }
                iargs[i] = iarglist;
                if (stringPresent)
                {
                    stringarg = read_string(index, raw, length, alphaNums);
                }
                else
                {
                    stringarg = null;
                }
                sargs[i] = stringarg;
                i += 1;
            }
            bool[] hasBreakpoint = new bool[opCount];
            BreakpointInfo[] breakpointInfo = new BreakpointInfo[opCount];
            i = 0;
            while ((i < opCount))
            {
                hasBreakpoint[i] = false;
                breakpointInfo[i] = null;
                i += 1;
            }
            return new Code(ops, iargs, sargs, new Dictionary<int, int>[opCount], new Dictionary<string, int>[opCount], new VmDebugData(hasBreakpoint, breakpointInfo, new Dictionary<int, int>(), 1, 0));
        }

        public static int initializeClass(int pc, VmContext vm, int[] args, string className)
        {
            int i = 0;
            int memberId = 0;
            int globalId = 0;
            int functionId = 0;
            int t = 0;
            int classId = args[0];
            int baseClassId = args[1];
            int globalNameId = args[2];
            int constructorFunctionId = args[3];
            int staticConstructorFunctionId = args[4];
            int staticInitializationState = 0;
            if ((staticConstructorFunctionId == -1))
            {
                staticInitializationState = 2;
            }
            int staticFieldCount = args[5];
            int assemblyId = args[6];
            Value[] staticFields = new Value[staticFieldCount];
            i = 0;
            while ((i < staticFieldCount))
            {
                staticFields[i] = vm.globals.valueNull;
                i += 1;
            }
            ClassInfo classInfo = new ClassInfo(classId, globalNameId, baseClassId, assemblyId, staticInitializationState, staticFields, staticConstructorFunctionId, constructorFunctionId, 0, null, null, null, null, null, vm.metadata.classMemberLocalizerBuilder[classId], null, className);
            ClassInfo[] classTable = getClassTable(vm, classId);
            classTable[classId] = classInfo;
            List<ClassInfo> classChain = new List<ClassInfo>();
            classChain.Add(classInfo);
            int classIdWalker = baseClassId;
            while ((classIdWalker != -1))
            {
                ClassInfo walkerClass = classTable[classIdWalker];
                classChain.Add(walkerClass);
                classIdWalker = walkerClass.baseClassId;
            }
            ClassInfo baseClass = null;
            if ((baseClassId != -1))
            {
                baseClass = classChain[1];
            }
            List<int> functionIds = new List<int>();
            List<int> fieldInitializationCommand = new List<int>();
            List<Value> fieldInitializationLiteral = new List<Value>();
            List<int> fieldAccessModifier = new List<int>();
            Dictionary<int, int> globalNameIdToMemberId = new Dictionary<int, int>();
            if ((baseClass != null))
            {
                i = 0;
                while ((i < baseClass.memberCount))
                {
                    functionIds.Add(baseClass.functionIds[i]);
                    fieldInitializationCommand.Add(baseClass.fieldInitializationCommand[i]);
                    fieldInitializationLiteral.Add(baseClass.fieldInitializationLiteral[i]);
                    fieldAccessModifier.Add(baseClass.fieldAccessModifiers[i]);
                    i += 1;
                }
                int[] keys = baseClass.globalIdToMemberId.Keys.ToArray();
                i = 0;
                while ((i < keys.Length))
                {
                    t = keys[i];
                    globalNameIdToMemberId[t] = baseClass.globalIdToMemberId[t];
                    i += 1;
                }
                keys = baseClass.localeScopedNameIdToMemberId.Keys.ToArray();
                i = 0;
                while ((i < keys.Length))
                {
                    t = keys[i];
                    classInfo.localeScopedNameIdToMemberId[t] = baseClass.localeScopedNameIdToMemberId[t];
                    i += 1;
                }
            }
            int accessModifier = 0;
            i = 7;
            while ((i < args.Length))
            {
                memberId = args[(i + 1)];
                globalId = args[(i + 2)];
                accessModifier = args[(i + 5)];
                while ((memberId >= functionIds.Count))
                {
                    functionIds.Add(-1);
                    fieldInitializationCommand.Add(-1);
                    fieldInitializationLiteral.Add(null);
                    fieldAccessModifier.Add(0);
                }
                globalNameIdToMemberId[globalId] = memberId;
                fieldAccessModifier[memberId] = accessModifier;
                if ((args[i] == 0))
                {
                    fieldInitializationCommand[memberId] = args[(i + 3)];
                    t = args[(i + 4)];
                    if ((t == -1))
                    {
                        fieldInitializationLiteral[memberId] = vm.globals.valueNull;
                    }
                    else
                    {
                        fieldInitializationLiteral[memberId] = vm.metadata.literalTable[t];
                    }
                }
                else
                {
                    functionId = args[(i + 3)];
                    functionIds[memberId] = functionId;
                }
                i += 6;
            }
            classInfo.functionIds = functionIds.ToArray();
            classInfo.fieldInitializationCommand = fieldInitializationCommand.ToArray();
            classInfo.fieldInitializationLiteral = fieldInitializationLiteral.ToArray();
            classInfo.fieldAccessModifiers = fieldAccessModifier.ToArray();
            classInfo.memberCount = functionIds.Count;
            classInfo.globalIdToMemberId = globalNameIdToMemberId;
            classInfo.typeInfo = new int[classInfo.memberCount][];
            if ((baseClass != null))
            {
                i = 0;
                while ((i < baseClass.typeInfo.Length))
                {
                    classInfo.typeInfo[i] = baseClass.typeInfo[i];
                    i += 1;
                }
            }
            if ("Core.Exception" == className)
            {
                MagicNumbers mn = vm.metadata.magicNumbers;
                mn.coreExceptionClassId = classId;
            }
            return 0;
        }

        public static int initializeClassFieldTypeInfo(VmContext vm, int[] opCodeRow)
        {
            ClassInfo classInfo = vm.metadata.classTable[opCodeRow[0]];
            int memberId = opCodeRow[1];
            int _len = opCodeRow.Length;
            int[] typeInfo = new int[(_len - 2)];
            int i = 2;
            while ((i < _len))
            {
                typeInfo[(i - 2)] = opCodeRow[i];
                i += 1;
            }
            classInfo.typeInfo[memberId] = typeInfo;
            return 0;
        }

        public static VmGlobals initializeConstantValues()
        {
            Value[] pos = new Value[2049];
            Value[] neg = new Value[257];
            int i = 0;
            while ((i < 2049))
            {
                pos[i] = new Value(3, i);
                i += 1;
            }
            i = 1;
            while ((i < 257))
            {
                neg[i] = new Value(3, -i);
                i += 1;
            }
            neg[0] = pos[0];
            VmGlobals globals = new VmGlobals(new Value(1, null), new Value(2, true), new Value(2, false), pos[0], pos[1], neg[1], new Value(4, 0.0), new Value(4, 1.0), new Value(5, ""), pos, neg, new Dictionary<string, Value>(), new int[1], new int[1], new int[1], new int[1], new int[1], new int[2]);
            globals.commonStrings[""] = globals.stringEmpty;
            globals.booleanType[0] = 2;
            globals.intType[0] = 3;
            globals.floatType[0] = 4;
            globals.stringType[0] = 5;
            globals.classType[0] = 10;
            globals.anyInstanceType[0] = 8;
            globals.anyInstanceType[1] = 0;
            return globals;
        }

        public static int initializeFunction(VmContext vm, int[] args, int currentPc, string stringArg)
        {
            int functionId = args[0];
            int nameId = args[1];
            int minArgCount = args[2];
            int maxArgCount = args[3];
            int functionType = args[4];
            int classId = args[5];
            int localsCount = args[6];
            int numPcOffsetsForOptionalArgs = args[8];
            int[] pcOffsetsForOptionalArgs = new int[(numPcOffsetsForOptionalArgs + 1)];
            int i = 0;
            while ((i < numPcOffsetsForOptionalArgs))
            {
                pcOffsetsForOptionalArgs[(i + 1)] = args[(9 + i)];
                i += 1;
            }
            FunctionInfo[] functionTable = getFunctionTable(vm, functionId);
            functionTable[functionId] = new FunctionInfo(functionId, nameId, currentPc, minArgCount, maxArgCount, functionType, classId, localsCount, pcOffsetsForOptionalArgs, stringArg, null);
            vm.metadata.mostRecentFunctionDef = functionTable[functionId];
            if ((nameId >= 0))
            {
                string name = vm.metadata.identifiers[nameId];
                if ("_LIB_CORE_list_filter" == name)
                {
                    vm.metadata.primitiveMethodFunctionIdFallbackLookup[0] = functionId;
                }
                else if ("_LIB_CORE_list_map" == name)
                {
                    vm.metadata.primitiveMethodFunctionIdFallbackLookup[1] = functionId;
                }
                else if ("_LIB_CORE_list_sort_by_key" == name)
                {
                    vm.metadata.primitiveMethodFunctionIdFallbackLookup[2] = functionId;
                }
                else if ("_LIB_CORE_invoke" == name)
                {
                    vm.metadata.primitiveMethodFunctionIdFallbackLookup[3] = functionId;
                }
                else if ("_LIB_CORE_generateException" == name)
                {
                    MagicNumbers mn = vm.metadata.magicNumbers;
                    mn.coreGenerateExceptionFunctionId = functionId;
                }
            }
            return 0;
        }

        public static Dictionary<int, int> initializeIntSwitchStatement(VmContext vm, int pc, int[] args)
        {
            Dictionary<int, int> output = new Dictionary<int, int>();
            int i = 1;
            while ((i < args.Length))
            {
                output[args[i]] = args[(i + 1)];
                i += 2;
            }
            vm.byteCode.integerSwitchesByPc[pc] = output;
            return output;
        }

        public static Dictionary<string, int> initializeStringSwitchStatement(VmContext vm, int pc, int[] args)
        {
            Dictionary<string, int> output = new Dictionary<string, int>();
            int i = 1;
            while ((i < args.Length))
            {
                string s = (string)vm.metadata.literalTable[args[i]].internalValue;
                output[s] = args[(i + 1)];
                i += 2;
            }
            vm.byteCode.stringSwitchesByPc[pc] = output;
            return output;
        }

        public static int initLocTable(VmContext vm, int[] row)
        {
            int classId = row[0];
            int memberCount = row[1];
            int nameId = 0;
            int totalLocales = vm.metadata.magicNumbers.totalLocaleCount;
            Dictionary<int, int> lookup = new Dictionary<int, int>();
            int i = 2;
            while ((i < row.Length))
            {
                int localeId = row[i];
                i += 1;
                int j = 0;
                while ((j < memberCount))
                {
                    nameId = row[(i + j)];
                    if ((nameId != -1))
                    {
                        lookup[((nameId * totalLocales) + localeId)] = j;
                    }
                    j += 1;
                }
                i += memberCount;
            }
            vm.metadata.classMemberLocalizerBuilder[classId] = lookup;
            return 0;
        }

        public static InterpreterResult interpret(VmContext vm, int executionContextId)
        {
            InterpreterResult output = interpretImpl(vm, executionContextId);
            while (((output.status == 5) && (output.reinvokeDelay == 0)))
            {
                output = interpretImpl(vm, executionContextId);
            }
            return output;
        }

        public static InterpreterResult interpreterFinished(VmContext vm, ExecutionContext ec)
        {
            if ((ec != null))
            {
                int id = ec.id;
                if (vm.executionContexts.ContainsKey(id))
                {
                    vm.executionContexts.Remove(id);
                }
            }
            return new InterpreterResult(1, null, 0.0, 0, false, "");
        }

        public static ExecutionContext interpreterGetExecutionContext(VmContext vm, int executionContextId)
        {
            Dictionary<int, ExecutionContext> executionContexts = vm.executionContexts;
            if (!executionContexts.ContainsKey(executionContextId))
            {
                return null;
            }
            return executionContexts[executionContextId];
        }

        public static InterpreterResult interpretImpl(VmContext vm, int executionContextId)
        {
            VmMetadata metadata = vm.metadata;
            VmGlobals globals = vm.globals;
            Value VALUE_NULL = globals.valueNull;
            Value VALUE_TRUE = globals.boolTrue;
            Value VALUE_FALSE = globals.boolFalse;
            Value VALUE_INT_ONE = globals.intOne;
            Value VALUE_INT_ZERO = globals.intZero;
            Value VALUE_FLOAT_ZERO = globals.floatZero;
            Value VALUE_FLOAT_ONE = globals.floatOne;
            Value[] INTEGER_POSITIVE_CACHE = globals.positiveIntegers;
            Value[] INTEGER_NEGATIVE_CACHE = globals.negativeIntegers;
            Dictionary<int, ExecutionContext> executionContexts = vm.executionContexts;
            ExecutionContext ec = interpreterGetExecutionContext(vm, executionContextId);
            if ((ec == null))
            {
                return interpreterFinished(vm, null);
            }
            ec.executionCounter += 1;
            StackFrame stack = ec.stackTop;
            int[] ops = vm.byteCode.ops;
            int[][] args = vm.byteCode.args;
            string[] stringArgs = vm.byteCode.stringArgs;
            ClassInfo[] classTable = vm.metadata.classTable;
            FunctionInfo[] functionTable = vm.metadata.functionTable;
            Value[] literalTable = vm.metadata.literalTable;
            string[] identifiers = vm.metadata.identifiers;
            Value[] valueStack = ec.valueStack;
            int valueStackSize = ec.currentValueStackSize;
            int valueStackCapacity = valueStack.Length;
            bool hasInterrupt = false;
            int type = 0;
            int nameId = 0;
            int classId = 0;
            int functionId = 0;
            int localeId = 0;
            ClassInfo classInfo = null;
            int _len = 0;
            Value root = null;
            int[] row = null;
            int argCount = 0;
            string[] stringList = null;
            bool returnValueUsed = false;
            Value output = null;
            FunctionInfo functionInfo = null;
            int keyType = 0;
            int intKey = 0;
            string stringKey = null;
            bool first = false;
            bool primitiveMethodToCoreLibraryFallback = false;
            bool bool1 = false;
            bool bool2 = false;
            bool staticConstructorNotInvoked = true;
            int int1 = 0;
            int int2 = 0;
            int int3 = 0;
            int i = 0;
            int j = 0;
            double float1 = 0.0;
            double float2 = 0.0;
            double float3 = 0.0;
            double[] floatList1 = new double[2];
            Value value = null;
            Value value2 = null;
            Value value3 = null;
            string string1 = null;
            string string2 = null;
            ObjectInstance objInstance1 = null;
            ObjectInstance objInstance2 = null;
            object obj1 = null;
            ListImpl list1 = null;
            ListImpl list2 = null;
            List<Value> valueList1 = null;
            List<Value> valueList2 = null;
            DictImpl dictImpl = null;
            DictImpl dictImpl2 = null;
            List<string> stringList1 = null;
            List<int> intList1 = null;
            Value[] valueArray1 = null;
            int[] intArray1 = null;
            int[] intArray2 = null;
            object[] objArray1 = null;
            FunctionPointer functionPointer1 = null;
            Dictionary<int, int> intIntDict1 = null;
            Dictionary<string, int> stringIntDict1 = null;
            StackFrame stackFrame2 = null;
            Value leftValue = null;
            Value rightValue = null;
            ClassValue classValue = null;
            Value arg1 = null;
            Value arg2 = null;
            Value arg3 = null;
            Value arg4 = null;
            Value arg5 = null;
            Value arg6 = null;
            Value arg7 = null;
            Value arg8 = null;
            Value arg9 = null;
            Value arg10 = null;
            List<Token> tokenList = null;
            int[] globalNameIdToPrimitiveMethodName = vm.metadata.globalNameIdToPrimitiveMethodName;
            MagicNumbers magicNumbers = vm.metadata.magicNumbers;
            Dictionary<int, int>[] integerSwitchesByPc = vm.byteCode.integerSwitchesByPc;
            Dictionary<string, int>[] stringSwitchesByPc = vm.byteCode.stringSwitchesByPc;
            Dictionary<int, int> integerSwitch = null;
            Dictionary<string, int> stringSwitch = null;
            int[][] esfData = vm.metadata.esfData;
            Dictionary<int, ClosureValuePointer> closure = null;
            Dictionary<int, ClosureValuePointer> parentClosure = null;
            int[] intBuffer = new int[16];
            Value[] localsStack = ec.localsStack;
            int[] localsStackSet = ec.localsStackSet;
            int localsStackSetToken = stack.localsStackSetToken;
            int localsStackCapacity = localsStack.Length;
            int localsStackOffset = stack.localsStackOffset;
            Value[] funcArgs = vm.funcArgs;
            int pc = stack.pc;
            System.Func<VmContext, Value[], Value> nativeFp = null;
            VmDebugData debugData = vm.byteCode.debugData;
            bool[] isBreakPointPresent = debugData.hasBreakpoint;
            BreakpointInfo breakpointInfo = null;
            bool debugBreakPointTemporaryDisable = false;
            while (true)
            {
                row = args[pc];
                switch (ops[pc])
                {
                    case 0:
                        // ADD_LITERAL;
                        addLiteralImpl(vm, row, stringArgs[pc]);
                        break;
                    case 1:
                        // ADD_NAME;
                        addNameImpl(vm, stringArgs[pc]);
                        break;
                    case 2:
                        // ARG_TYPE_VERIFY;
                        _len = row[0];
                        i = 1;
                        j = 0;
                        while ((j < _len))
                        {
                            j += 1;
                        }
                        break;
                    case 3:
                        // ASSIGN_CLOSURE;
                        value = valueStack[--valueStackSize];
                        i = row[0];
                        if ((stack.closureVariables == null))
                        {
                            closure = new Dictionary<int, ClosureValuePointer>();
                            closure[-1] = new ClosureValuePointer(stack.objectContext);
                            stack.closureVariables = closure;
                            closure[i] = new ClosureValuePointer(value);
                        }
                        else
                        {
                            closure = stack.closureVariables;
                            if (closure.ContainsKey(i))
                            {
                                closure[i].value = value;
                            }
                            else
                            {
                                closure[i] = new ClosureValuePointer(value);
                            }
                        }
                        break;
                    case 4:
                        // ASSIGN_INDEX;
                        valueStackSize -= 3;
                        value = valueStack[(valueStackSize + 2)];
                        value2 = valueStack[(valueStackSize + 1)];
                        root = valueStack[valueStackSize];
                        type = root.type;
                        bool1 = (row[0] == 1);
                        if ((type == 6))
                        {
                            if ((value2.type == 3))
                            {
                                i = (int)value2.internalValue;
                                list1 = (ListImpl)root.internalValue;
                                if ((list1.type != null))
                                {
                                    value3 = canAssignTypeToGeneric(vm, value, list1.type, 0);
                                    if ((value3 == null))
                                    {
                                        hasInterrupt = EX_InvalidArgument(ec, string.Join("", new string[] { "Cannot convert a ",typeToStringFromValue(vm, value)," into a ",typeToString(vm, list1.type, 0) }));
                                    }
                                    value = value3;
                                }
                                if (!hasInterrupt)
                                {
                                    if ((i >= list1.size))
                                    {
                                        hasInterrupt = EX_IndexOutOfRange(ec, "Index is out of range.");
                                    }
                                    else if ((i < 0))
                                    {
                                        i += list1.size;
                                        if ((i < 0))
                                        {
                                            hasInterrupt = EX_IndexOutOfRange(ec, "Index is out of range.");
                                        }
                                    }
                                    if (!hasInterrupt)
                                    {
                                        list1.array[i] = value;
                                    }
                                }
                            }
                            else
                            {
                                hasInterrupt = EX_InvalidArgument(ec, "List index must be an integer.");
                            }
                        }
                        else if ((type == 7))
                        {
                            dictImpl = (DictImpl)root.internalValue;
                            if ((dictImpl.valueType != null))
                            {
                                value3 = canAssignTypeToGeneric(vm, value, dictImpl.valueType, 0);
                                if ((value3 == null))
                                {
                                    hasInterrupt = EX_InvalidArgument(ec, "Cannot assign a value to this dictionary of this type.");
                                }
                                else
                                {
                                    value = value3;
                                }
                            }
                            keyType = value2.type;
                            if ((keyType == 3))
                            {
                                intKey = (int)value2.internalValue;
                            }
                            else if ((keyType == 5))
                            {
                                stringKey = (string)value2.internalValue;
                            }
                            else if ((keyType == 8))
                            {
                                objInstance1 = (ObjectInstance)value2.internalValue;
                                intKey = objInstance1.objectId;
                            }
                            else
                            {
                                hasInterrupt = EX_InvalidArgument(ec, "Invalid key for a dictionary.");
                            }
                            if (!hasInterrupt)
                            {
                                bool2 = (dictImpl.size == 0);
                                if ((dictImpl.keyType != keyType))
                                {
                                    if ((dictImpl.valueType != null))
                                    {
                                        string1 = string.Join("", new string[] { "Cannot assign a key of type ",typeToStringFromValue(vm, value2)," to a dictionary that requires key types of ",dictKeyInfoToString(vm, dictImpl),"." });
                                        hasInterrupt = EX_InvalidKey(ec, string1);
                                    }
                                    else if (!bool2)
                                    {
                                        hasInterrupt = EX_InvalidKey(ec, "Cannot have multiple keys in one dictionary with different types.");
                                    }
                                }
                                else if (((keyType == 8) && (dictImpl.keyClassId > 0) && (objInstance1.classId != dictImpl.keyClassId)))
                                {
                                    if (isClassASubclassOf(vm, objInstance1.classId, dictImpl.keyClassId))
                                    {
                                        hasInterrupt = EX_InvalidKey(ec, "Cannot use this type of object as a key for this dictionary.");
                                    }
                                }
                            }
                            if (!hasInterrupt)
                            {
                                if ((keyType == 5))
                                {
                                    if (!dictImpl.stringToIndex.TryGetValue(stringKey, out int1)) int1 = -1;
                                    if ((int1 == -1))
                                    {
                                        dictImpl.stringToIndex[stringKey] = dictImpl.size;
                                        dictImpl.size += 1;
                                        dictImpl.keys.Add(value2);
                                        dictImpl.values.Add(value);
                                        if (bool2)
                                        {
                                            dictImpl.keyType = keyType;
                                        }
                                    }
                                    else
                                    {
                                        dictImpl.values[int1] = value;
                                    }
                                }
                                else
                                {
                                    if (!dictImpl.intToIndex.TryGetValue(intKey, out int1)) int1 = -1;
                                    if ((int1 == -1))
                                    {
                                        dictImpl.intToIndex[intKey] = dictImpl.size;
                                        dictImpl.size += 1;
                                        dictImpl.keys.Add(value2);
                                        dictImpl.values.Add(value);
                                        if (bool2)
                                        {
                                            dictImpl.keyType = keyType;
                                        }
                                    }
                                    else
                                    {
                                        dictImpl.values[int1] = value;
                                    }
                                }
                            }
                        }
                        else
                        {
                            hasInterrupt = EX_UnsupportedOperation(ec, getTypeFromId(type) + " type does not support assigning to an index.");
                        }
                        if (bool1)
                        {
                            valueStack[valueStackSize] = value;
                            valueStackSize += 1;
                        }
                        break;
                    case 6:
                        // ASSIGN_STATIC_FIELD;
                        classInfo = classTable[row[0]];
                        staticConstructorNotInvoked = true;
                        if ((classInfo.staticInitializationState < 2))
                        {
                            stack.pc = pc;
                            stackFrame2 = maybeInvokeStaticConstructor(vm, ec, stack, classInfo, valueStackSize, PST_IntBuffer16);
                            if ((PST_IntBuffer16[0] == 1))
                            {
                                return generateException(vm, stack, pc, valueStackSize, ec, 0, "Static initialization loop detected. The class this field is a member of is not done being initialized.");
                            }
                            if ((stackFrame2 != null))
                            {
                                staticConstructorNotInvoked = false;
                                stack = stackFrame2;
                                pc = stack.pc;
                                localsStackSetToken = stack.localsStackSetToken;
                                localsStackOffset = stack.localsStackOffset;
                            }
                        }
                        if (staticConstructorNotInvoked)
                        {
                            valueStackSize -= 1;
                            classInfo.staticFields[row[1]] = valueStack[valueStackSize];
                        }
                        break;
                    case 7:
                        // ASSIGN_FIELD;
                        valueStackSize -= 2;
                        value = valueStack[(valueStackSize + 1)];
                        value2 = valueStack[valueStackSize];
                        nameId = row[2];
                        if ((value2.type == 8))
                        {
                            objInstance1 = (ObjectInstance)value2.internalValue;
                            classId = objInstance1.classId;
                            classInfo = classTable[classId];
                            intIntDict1 = classInfo.localeScopedNameIdToMemberId;
                            if ((row[5] == classId))
                            {
                                int1 = row[6];
                            }
                            else
                            {
                                if (!intIntDict1.TryGetValue(nameId, out int1)) int1 = -1;
                                if ((int1 != -1))
                                {
                                    int3 = classInfo.fieldAccessModifiers[int1];
                                    if ((int3 > 1))
                                    {
                                        if ((int3 == 2))
                                        {
                                            if ((classId != row[3]))
                                            {
                                                int1 = -2;
                                            }
                                        }
                                        else
                                        {
                                            if (((int3 == 3) || (int3 == 5)))
                                            {
                                                if ((classInfo.assemblyId != row[4]))
                                                {
                                                    int1 = -3;
                                                }
                                            }
                                            if (((int3 == 4) || (int3 == 5)))
                                            {
                                                i = row[3];
                                                if ((classId == i))
                                                {
                                                }
                                                else
                                                {
                                                    classInfo = classTable[classInfo.id];
                                                    while (((classInfo.baseClassId != -1) && (int1 < classTable[classInfo.baseClassId].fieldAccessModifiers.Length)))
                                                    {
                                                        classInfo = classTable[classInfo.baseClassId];
                                                    }
                                                    j = classInfo.id;
                                                    if ((j != i))
                                                    {
                                                        bool1 = false;
                                                        while (((i != -1) && (classTable[i].baseClassId != -1)))
                                                        {
                                                            i = classTable[i].baseClassId;
                                                            if ((i == j))
                                                            {
                                                                bool1 = true;
                                                                i = -1;
                                                            }
                                                        }
                                                        if (!bool1)
                                                        {
                                                            int1 = -4;
                                                        }
                                                    }
                                                }
                                                classInfo = classTable[classId];
                                            }
                                        }
                                    }
                                }
                                row[5] = classId;
                                row[6] = int1;
                            }
                            if ((int1 > -1))
                            {
                                int2 = classInfo.functionIds[int1];
                                if ((int2 == -1))
                                {
                                    intArray1 = classInfo.typeInfo[int1];
                                    if ((intArray1 == null))
                                    {
                                        objInstance1.members[int1] = value;
                                    }
                                    else
                                    {
                                        value2 = canAssignTypeToGeneric(vm, value, intArray1, 0);
                                        if ((value2 != null))
                                        {
                                            objInstance1.members[int1] = value2;
                                        }
                                        else
                                        {
                                            hasInterrupt = EX_InvalidArgument(ec, "Cannot assign this type to this field.");
                                        }
                                    }
                                }
                                else
                                {
                                    hasInterrupt = EX_InvalidArgument(ec, "Cannot override a method with assignment.");
                                }
                            }
                            else if ((int1 < -1))
                            {
                                string1 = identifiers[row[0]];
                                if ((int1 == -2))
                                {
                                    string2 = "private";
                                }
                                else if ((int1 == -3))
                                {
                                    string2 = "internal";
                                }
                                else
                                {
                                    string2 = "protected";
                                }
                                hasInterrupt = EX_UnknownField(ec, string.Join("", new string[] { "The field '",string1,"' is marked as ",string2," and cannot be accessed from here." }));
                            }
                            else
                            {
                                hasInterrupt = EX_InvalidAssignment(ec, string.Join("", new string[] { "'",classInfo.fullyQualifiedName,"' instances do not have a field called '",metadata.identifiers[row[0]],"'" }));
                            }
                        }
                        else if ((value2.type == 1))
                        {
                            hasInterrupt = EX_NullReference(ec, "Cannot assign to a field on null.");
                        }
                        else
                        {
                            hasInterrupt = EX_InvalidAssignment(ec, "Cannot assign to a field on this type.");
                        }
                        if ((row[1] == 1))
                        {
                            valueStack[valueStackSize++] = value;
                        }
                        break;
                    case 8:
                        // ASSIGN_THIS_FIELD;
                        objInstance2 = (ObjectInstance)stack.objectContext.internalValue;
                        objInstance2.members[row[0]] = valueStack[--valueStackSize];
                        break;
                    case 5:
                        // ASSIGN_LOCAL;
                        i = (localsStackOffset + row[0]);
                        localsStack[i] = valueStack[--valueStackSize];
                        localsStackSet[i] = localsStackSetToken;
                        break;
                    case 9:
                        // BINARY_OP;
                        rightValue = valueStack[--valueStackSize];
                        leftValue = valueStack[(valueStackSize - 1)];
                        switch (((((leftValue.type * 15) + row[0]) * 11) + rightValue.type))
                        {
                            case 553:
                                // int ** int;
                                value = doExponentMath(globals, (0.0 + (int)leftValue.internalValue), (0.0 + (int)rightValue.internalValue), false);
                                if ((value == null))
                                {
                                    hasInterrupt = EX_InvalidArgument(ec, getExponentErrorMsg(vm, leftValue, rightValue));
                                }
                                break;
                            case 554:
                                // int ** float;
                                value = doExponentMath(globals, (0.0 + (int)leftValue.internalValue), (double)rightValue.internalValue, false);
                                if ((value == null))
                                {
                                    hasInterrupt = EX_InvalidArgument(ec, getExponentErrorMsg(vm, leftValue, rightValue));
                                }
                                break;
                            case 718:
                                // float ** int;
                                value = doExponentMath(globals, (double)leftValue.internalValue, (0.0 + (int)rightValue.internalValue), false);
                                if ((value == null))
                                {
                                    hasInterrupt = EX_InvalidArgument(ec, getExponentErrorMsg(vm, leftValue, rightValue));
                                }
                                break;
                            case 719:
                                // float ** float;
                                value = doExponentMath(globals, (double)leftValue.internalValue, (double)rightValue.internalValue, false);
                                if ((value == null))
                                {
                                    hasInterrupt = EX_InvalidArgument(ec, getExponentErrorMsg(vm, leftValue, rightValue));
                                }
                                break;
                            case 708:
                                // float % float;
                                float1 = (double)rightValue.internalValue;
                                if ((float1 == 0))
                                {
                                    hasInterrupt = EX_DivisionByZero(ec, "Modulo by 0.");
                                }
                                else
                                {
                                    float3 = ((double)leftValue.internalValue % float1);
                                    if ((float3 < 0))
                                    {
                                        float3 += float1;
                                    }
                                    value = buildFloat(globals, float3);
                                }
                                break;
                            case 707:
                                // float % int;
                                int1 = (int)rightValue.internalValue;
                                if ((int1 == 0))
                                {
                                    hasInterrupt = EX_DivisionByZero(ec, "Modulo by 0.");
                                }
                                else
                                {
                                    float1 = ((double)leftValue.internalValue % int1);
                                    if ((float1 < 0))
                                    {
                                        float1 += int1;
                                    }
                                    value = buildFloat(globals, float1);
                                }
                                break;
                            case 543:
                                // int % float;
                                float3 = (double)rightValue.internalValue;
                                if ((float3 == 0))
                                {
                                    hasInterrupt = EX_DivisionByZero(ec, "Modulo by 0.");
                                }
                                else
                                {
                                    float1 = ((int)leftValue.internalValue % float3);
                                    if ((float1 < 0))
                                    {
                                        float1 += float3;
                                    }
                                    value = buildFloat(globals, float1);
                                }
                                break;
                            case 542:
                                // int % int;
                                int2 = (int)rightValue.internalValue;
                                if ((int2 == 0))
                                {
                                    hasInterrupt = EX_DivisionByZero(ec, "Modulo by 0.");
                                }
                                else
                                {
                                    int1 = ((int)leftValue.internalValue % int2);
                                    if ((int1 < 0))
                                    {
                                        int1 += int2;
                                    }
                                    value = buildInteger(globals, int1);
                                }
                                break;
                            case 996:
                                // list + list;
                                value = new Value(6, valueConcatLists((ListImpl)leftValue.internalValue, (ListImpl)rightValue.internalValue));
                                break;
                            case 498:
                                // int + int;
                                int1 = ((int)leftValue.internalValue + (int)rightValue.internalValue);
                                if ((int1 < 0))
                                {
                                    if ((int1 > -257))
                                    {
                                        value = INTEGER_NEGATIVE_CACHE[-int1];
                                    }
                                    else
                                    {
                                        value = new Value(3, int1);
                                    }
                                }
                                else if ((int1 < 2049))
                                {
                                    value = INTEGER_POSITIVE_CACHE[int1];
                                }
                                else
                                {
                                    value = new Value(3, int1);
                                }
                                break;
                            case 509:
                                // int - int;
                                int1 = ((int)leftValue.internalValue - (int)rightValue.internalValue);
                                if ((int1 < 0))
                                {
                                    if ((int1 > -257))
                                    {
                                        value = INTEGER_NEGATIVE_CACHE[-int1];
                                    }
                                    else
                                    {
                                        value = new Value(3, int1);
                                    }
                                }
                                else if ((int1 < 2049))
                                {
                                    value = INTEGER_POSITIVE_CACHE[int1];
                                }
                                else
                                {
                                    value = new Value(3, int1);
                                }
                                break;
                            case 520:
                                // int * int;
                                int1 = ((int)leftValue.internalValue * (int)rightValue.internalValue);
                                if ((int1 < 0))
                                {
                                    if ((int1 > -257))
                                    {
                                        value = INTEGER_NEGATIVE_CACHE[-int1];
                                    }
                                    else
                                    {
                                        value = new Value(3, int1);
                                    }
                                }
                                else if ((int1 < 2049))
                                {
                                    value = INTEGER_POSITIVE_CACHE[int1];
                                }
                                else
                                {
                                    value = new Value(3, int1);
                                }
                                break;
                            case 531:
                                // int / int;
                                int1 = (int)leftValue.internalValue;
                                int2 = (int)rightValue.internalValue;
                                if ((int2 == 0))
                                {
                                    hasInterrupt = EX_DivisionByZero(ec, "Division by 0.");
                                }
                                else if ((int1 == 0))
                                {
                                    value = VALUE_INT_ZERO;
                                }
                                else
                                {
                                    if (((int1 % int2) == 0))
                                    {
                                        int3 = (int1) / (int2);
                                    }
                                    else if ((((int1 < 0)) != ((int2 < 0))))
                                    {
                                        float1 = (1 + ((-1.0 * int1)) / (int2));
                                        float1 -= (float1 % 1.0);
                                        int3 = (int)(-float1);
                                    }
                                    else
                                    {
                                        int3 = (int1) / (int2);
                                    }
                                    if ((int3 < 0))
                                    {
                                        if ((int3 > -257))
                                        {
                                            value = INTEGER_NEGATIVE_CACHE[-int3];
                                        }
                                        else
                                        {
                                            value = new Value(3, int3);
                                        }
                                    }
                                    else if ((int3 < 2049))
                                    {
                                        value = INTEGER_POSITIVE_CACHE[int3];
                                    }
                                    else
                                    {
                                        value = new Value(3, int3);
                                    }
                                }
                                break;
                            case 663:
                                // float + int;
                                value = buildFloat(globals, ((double)leftValue.internalValue + (int)rightValue.internalValue));
                                break;
                            case 499:
                                // int + float;
                                value = buildFloat(globals, ((int)leftValue.internalValue + (double)rightValue.internalValue));
                                break;
                            case 664:
                                // float + float;
                                float1 = ((double)leftValue.internalValue + (double)rightValue.internalValue);
                                if ((float1 == 0))
                                {
                                    value = VALUE_FLOAT_ZERO;
                                }
                                else if ((float1 == 1))
                                {
                                    value = VALUE_FLOAT_ONE;
                                }
                                else
                                {
                                    value = new Value(4, float1);
                                }
                                break;
                            case 510:
                                // int - float;
                                value = buildFloat(globals, ((int)leftValue.internalValue - (double)rightValue.internalValue));
                                break;
                            case 674:
                                // float - int;
                                value = buildFloat(globals, ((double)leftValue.internalValue - (int)rightValue.internalValue));
                                break;
                            case 675:
                                // float - float;
                                float1 = ((double)leftValue.internalValue - (double)rightValue.internalValue);
                                if ((float1 == 0))
                                {
                                    value = VALUE_FLOAT_ZERO;
                                }
                                else if ((float1 == 1))
                                {
                                    value = VALUE_FLOAT_ONE;
                                }
                                else
                                {
                                    value = new Value(4, float1);
                                }
                                break;
                            case 685:
                                // float * int;
                                value = buildFloat(globals, ((double)leftValue.internalValue * (int)rightValue.internalValue));
                                break;
                            case 521:
                                // int * float;
                                value = buildFloat(globals, ((int)leftValue.internalValue * (double)rightValue.internalValue));
                                break;
                            case 686:
                                // float * float;
                                value = buildFloat(globals, ((double)leftValue.internalValue * (double)rightValue.internalValue));
                                break;
                            case 532:
                                // int / float;
                                float1 = (double)rightValue.internalValue;
                                if ((float1 == 0))
                                {
                                    hasInterrupt = EX_DivisionByZero(ec, "Division by 0.");
                                }
                                else
                                {
                                    value = buildFloat(globals, ((int)leftValue.internalValue) / (float1));
                                }
                                break;
                            case 696:
                                // float / int;
                                int1 = (int)rightValue.internalValue;
                                if ((int1 == 0))
                                {
                                    hasInterrupt = EX_DivisionByZero(ec, "Division by 0.");
                                }
                                else
                                {
                                    value = buildFloat(globals, ((double)leftValue.internalValue) / (int1));
                                }
                                break;
                            case 697:
                                // float / float;
                                float1 = (double)rightValue.internalValue;
                                if ((float1 == 0))
                                {
                                    hasInterrupt = EX_DivisionByZero(ec, "Division by 0.");
                                }
                                else
                                {
                                    value = buildFloat(globals, ((double)leftValue.internalValue) / (float1));
                                }
                                break;
                            case 564:
                                // int & int;
                                value = buildInteger(globals, ((int)leftValue.internalValue & (int)rightValue.internalValue));
                                break;
                            case 575:
                                // int | int;
                                value = buildInteger(globals, ((int)leftValue.internalValue | (int)rightValue.internalValue));
                                break;
                            case 586:
                                // int ^ int;
                                value = buildInteger(globals, ((int)leftValue.internalValue ^ (int)rightValue.internalValue));
                                break;
                            case 597:
                                // int << int;
                                int1 = (int)rightValue.internalValue;
                                if ((int1 < 0))
                                {
                                    hasInterrupt = EX_InvalidArgument(ec, "Cannot bit shift by a negative number.");
                                }
                                else
                                {
                                    value = buildInteger(globals, ((int)leftValue.internalValue << int1));
                                }
                                break;
                            case 608:
                                // int >> int;
                                int1 = (int)rightValue.internalValue;
                                if ((int1 < 0))
                                {
                                    hasInterrupt = EX_InvalidArgument(ec, "Cannot bit shift by a negative number.");
                                }
                                else
                                {
                                    value = buildInteger(globals, ((int)leftValue.internalValue >> int1));
                                }
                                break;
                            case 619:
                                // int < int;
                                if (((int)leftValue.internalValue < (int)rightValue.internalValue))
                                {
                                    value = VALUE_TRUE;
                                }
                                else
                                {
                                    value = VALUE_FALSE;
                                }
                                break;
                            case 630:
                                // int <= int;
                                if (((int)leftValue.internalValue <= (int)rightValue.internalValue))
                                {
                                    value = VALUE_TRUE;
                                }
                                else
                                {
                                    value = VALUE_FALSE;
                                }
                                break;
                            case 784:
                                // float < int;
                                if (((double)leftValue.internalValue < (int)rightValue.internalValue))
                                {
                                    value = VALUE_TRUE;
                                }
                                else
                                {
                                    value = VALUE_FALSE;
                                }
                                break;
                            case 795:
                                // float <= int;
                                if (((double)leftValue.internalValue <= (int)rightValue.internalValue))
                                {
                                    value = VALUE_TRUE;
                                }
                                else
                                {
                                    value = VALUE_FALSE;
                                }
                                break;
                            case 620:
                                // int < float;
                                if (((int)leftValue.internalValue < (double)rightValue.internalValue))
                                {
                                    value = VALUE_TRUE;
                                }
                                else
                                {
                                    value = VALUE_FALSE;
                                }
                                break;
                            case 631:
                                // int <= float;
                                if (((int)leftValue.internalValue <= (double)rightValue.internalValue))
                                {
                                    value = VALUE_TRUE;
                                }
                                else
                                {
                                    value = VALUE_FALSE;
                                }
                                break;
                            case 785:
                                // float < float;
                                if (((double)leftValue.internalValue < (double)rightValue.internalValue))
                                {
                                    value = VALUE_TRUE;
                                }
                                else
                                {
                                    value = VALUE_FALSE;
                                }
                                break;
                            case 796:
                                // float <= float;
                                if (((double)leftValue.internalValue <= (double)rightValue.internalValue))
                                {
                                    value = VALUE_TRUE;
                                }
                                else
                                {
                                    value = VALUE_FALSE;
                                }
                                break;
                            case 652:
                                // int >= int;
                                if (((int)leftValue.internalValue >= (int)rightValue.internalValue))
                                {
                                    value = VALUE_TRUE;
                                }
                                else
                                {
                                    value = VALUE_FALSE;
                                }
                                break;
                            case 641:
                                // int > int;
                                if (((int)leftValue.internalValue > (int)rightValue.internalValue))
                                {
                                    value = VALUE_TRUE;
                                }
                                else
                                {
                                    value = VALUE_FALSE;
                                }
                                break;
                            case 817:
                                // float >= int;
                                if (((double)leftValue.internalValue >= (int)rightValue.internalValue))
                                {
                                    value = VALUE_TRUE;
                                }
                                else
                                {
                                    value = VALUE_FALSE;
                                }
                                break;
                            case 806:
                                // float > int;
                                if (((double)leftValue.internalValue > (int)rightValue.internalValue))
                                {
                                    value = VALUE_TRUE;
                                }
                                else
                                {
                                    value = VALUE_FALSE;
                                }
                                break;
                            case 653:
                                // int >= float;
                                if (((int)leftValue.internalValue >= (double)rightValue.internalValue))
                                {
                                    value = VALUE_TRUE;
                                }
                                else
                                {
                                    value = VALUE_FALSE;
                                }
                                break;
                            case 642:
                                // int > float;
                                if (((int)leftValue.internalValue > (double)rightValue.internalValue))
                                {
                                    value = VALUE_TRUE;
                                }
                                else
                                {
                                    value = VALUE_FALSE;
                                }
                                break;
                            case 818:
                                // float >= float;
                                if (((double)leftValue.internalValue >= (double)rightValue.internalValue))
                                {
                                    value = VALUE_TRUE;
                                }
                                else
                                {
                                    value = VALUE_FALSE;
                                }
                                break;
                            case 807:
                                // float > float;
                                if (((double)leftValue.internalValue > (double)rightValue.internalValue))
                                {
                                    value = VALUE_TRUE;
                                }
                                else
                                {
                                    value = VALUE_FALSE;
                                }
                                break;
                            case 830:
                                // string + string;
                                value = new Value(5, (string)leftValue.internalValue + (string)rightValue.internalValue);
                                break;
                            case 850:
                                // string * int;
                                value = multiplyString(globals, leftValue, (string)leftValue.internalValue, (int)rightValue.internalValue);
                                break;
                            case 522:
                                // int * string;
                                value = multiplyString(globals, rightValue, (string)rightValue.internalValue, (int)leftValue.internalValue);
                                break;
                            case 1015:
                                // list * int;
                                int1 = (int)rightValue.internalValue;
                                if ((int1 < 0))
                                {
                                    hasInterrupt = EX_UnsupportedOperation(ec, "Cannot multiply list by negative number.");
                                }
                                else
                                {
                                    value = new Value(6, valueMultiplyList((ListImpl)leftValue.internalValue, int1));
                                }
                                break;
                            case 523:
                                // int * list;
                                int1 = (int)leftValue.internalValue;
                                if ((int1 < 0))
                                {
                                    hasInterrupt = EX_UnsupportedOperation(ec, "Cannot multiply list by negative number.");
                                }
                                else
                                {
                                    value = new Value(6, valueMultiplyList((ListImpl)rightValue.internalValue, int1));
                                }
                                break;
                            default:
                                if (((row[0] == 0) && (((leftValue.type == 5) || (rightValue.type == 5)))))
                                {
                                    value = new Value(5, valueToString(vm, leftValue) + valueToString(vm, rightValue));
                                }
                                else
                                {
                                    // unrecognized op;
                                    hasInterrupt = EX_UnsupportedOperation(ec, string.Join("", new string[] { "The '",getBinaryOpFromId(row[0]),"' operator is not supported for these types: ",getTypeFromId(leftValue.type)," and ",getTypeFromId(rightValue.type) }));
                                }
                                break;
                        }
                        valueStack[(valueStackSize - 1)] = value;
                        break;
                    case 10:
                        // BOOLEAN_NOT;
                        value = valueStack[(valueStackSize - 1)];
                        if ((value.type != 2))
                        {
                            hasInterrupt = EX_InvalidArgument(ec, "Boolean expected.");
                        }
                        else if ((bool)value.internalValue)
                        {
                            valueStack[(valueStackSize - 1)] = VALUE_FALSE;
                        }
                        else
                        {
                            valueStack[(valueStackSize - 1)] = VALUE_TRUE;
                        }
                        break;
                    case 11:
                        // BREAK;
                        if ((row[0] == 1))
                        {
                            pc += row[1];
                        }
                        else
                        {
                            intArray1 = esfData[pc];
                            pc = (intArray1[1] - 1);
                            valueStackSize = stack.valueStackPopSize;
                            stack.postFinallyBehavior = 1;
                        }
                        break;
                    case 12:
                        // CALL_FUNCTION;
                        type = row[0];
                        argCount = row[1];
                        functionId = row[2];
                        returnValueUsed = (row[3] == 1);
                        classId = row[4];
                        if (((type == 2) || (type == 6)))
                        {
                            // constructor or static method;
                            classInfo = metadata.classTable[classId];
                            staticConstructorNotInvoked = true;
                            if ((classInfo.staticInitializationState < 2))
                            {
                                stack.pc = pc;
                                stackFrame2 = maybeInvokeStaticConstructor(vm, ec, stack, classInfo, valueStackSize, PST_IntBuffer16);
                                if ((PST_IntBuffer16[0] == 1))
                                {
                                    return generateException(vm, stack, pc, valueStackSize, ec, 0, "Static initialization loop detected. The class this field is a member of is not done being initialized.");
                                }
                                if ((stackFrame2 != null))
                                {
                                    staticConstructorNotInvoked = false;
                                    stack = stackFrame2;
                                    pc = stack.pc;
                                    localsStackSetToken = stack.localsStackSetToken;
                                    localsStackOffset = stack.localsStackOffset;
                                }
                            }
                        }
                        else
                        {
                            staticConstructorNotInvoked = true;
                        }
                        if (staticConstructorNotInvoked)
                        {
                            bool1 = true;
                            // construct args array;
                            if ((argCount == -1))
                            {
                                valueStackSize -= 1;
                                value = valueStack[valueStackSize];
                                if ((value.type == 1))
                                {
                                    argCount = 0;
                                }
                                else if ((value.type == 6))
                                {
                                    list1 = (ListImpl)value.internalValue;
                                    argCount = list1.size;
                                    i = (argCount - 1);
                                    while ((i >= 0))
                                    {
                                        funcArgs[i] = list1.array[i];
                                        i -= 1;
                                    }
                                }
                                else
                                {
                                    hasInterrupt = EX_InvalidArgument(ec, "Function pointers' .invoke method requires a list argument.");
                                }
                            }
                            else
                            {
                                i = (argCount - 1);
                                while ((i >= 0))
                                {
                                    valueStackSize -= 1;
                                    funcArgs[i] = valueStack[valueStackSize];
                                    i -= 1;
                                }
                            }
                            if (!hasInterrupt)
                            {
                                if ((type == 3))
                                {
                                    value = stack.objectContext;
                                    objInstance1 = (ObjectInstance)value.internalValue;
                                    if ((objInstance1.classId != classId))
                                    {
                                        int2 = row[5];
                                        if ((int2 != -1))
                                        {
                                            classInfo = classTable[objInstance1.classId];
                                            functionId = classInfo.functionIds[int2];
                                        }
                                    }
                                }
                                else if ((type == 5))
                                {
                                    // field invocation;
                                    valueStackSize -= 1;
                                    value = valueStack[valueStackSize];
                                    localeId = row[5];
                                    switch (value.type)
                                    {
                                        case 1:
                                            hasInterrupt = EX_NullReference(ec, "Invoked method on null.");
                                            break;
                                        case 8:
                                            // field invoked on an object instance.;
                                            objInstance1 = (ObjectInstance)value.internalValue;
                                            int1 = objInstance1.classId;
                                            classInfo = classTable[int1];
                                            intIntDict1 = classInfo.localeScopedNameIdToMemberId;
                                            int1 = ((row[4] * magicNumbers.totalLocaleCount) + row[5]);
                                            if (!intIntDict1.TryGetValue(int1, out i)) i = -1;
                                            if ((i != -1))
                                            {
                                                int1 = intIntDict1[int1];
                                                functionId = classInfo.functionIds[int1];
                                                if ((functionId > 0))
                                                {
                                                    type = 3;
                                                }
                                                else
                                                {
                                                    value = objInstance1.members[int1];
                                                    type = 4;
                                                    valueStack[valueStackSize] = value;
                                                    valueStackSize += 1;
                                                }
                                            }
                                            else
                                            {
                                                hasInterrupt = EX_UnknownField(ec, "Unknown field.");
                                            }
                                            break;
                                        case 10:
                                            // field invocation on a class object instance.;
                                            functionId = resolvePrimitiveMethodName2(globalNameIdToPrimitiveMethodName, value.type, classId);
                                            if ((functionId < 0))
                                            {
                                                hasInterrupt = EX_InvalidInvocation(ec, "Class definitions do not have that method.");
                                            }
                                            else
                                            {
                                                functionId = resolvePrimitiveMethodName2(globalNameIdToPrimitiveMethodName, value.type, classId);
                                                if ((functionId < 0))
                                                {
                                                    hasInterrupt = EX_InvalidInvocation(ec, getTypeFromId(value.type) + " does not have that method.");
                                                }
                                                else if ((globalNameIdToPrimitiveMethodName[classId] == 8))
                                                {
                                                    type = 6;
                                                    classValue = (ClassValue)value.internalValue;
                                                    if (classValue.isInterface)
                                                    {
                                                        hasInterrupt = EX_UnsupportedOperation(ec, "Cannot create an instance of an interface.");
                                                    }
                                                    else
                                                    {
                                                        classId = classValue.classId;
                                                        if (!returnValueUsed)
                                                        {
                                                            hasInterrupt = EX_UnsupportedOperation(ec, "Cannot create an instance and not use the output.");
                                                        }
                                                        else
                                                        {
                                                            classInfo = metadata.classTable[classId];
                                                            functionId = classInfo.constructorFunctionId;
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    type = 9;
                                                }
                                            }
                                            break;
                                        default:
                                            // primitive method suspected.;
                                            functionId = resolvePrimitiveMethodName2(globalNameIdToPrimitiveMethodName, value.type, classId);
                                            if ((functionId < 0))
                                            {
                                                hasInterrupt = EX_InvalidInvocation(ec, getTypeFromId(value.type) + " does not have that method.");
                                            }
                                            else
                                            {
                                                type = 9;
                                            }
                                            break;
                                    }
                                }
                            }
                            if (((type == 4) && !hasInterrupt))
                            {
                                // pointer provided;
                                valueStackSize -= 1;
                                value = valueStack[valueStackSize];
                                if ((value.type == 9))
                                {
                                    functionPointer1 = (FunctionPointer)value.internalValue;
                                    switch (functionPointer1.type)
                                    {
                                        case 1:
                                            // pointer to a function;
                                            functionId = functionPointer1.functionId;
                                            type = 1;
                                            break;
                                        case 2:
                                            // pointer to a method;
                                            functionId = functionPointer1.functionId;
                                            value = functionPointer1.context;
                                            type = 3;
                                            break;
                                        case 3:
                                            // pointer to a static method;
                                            functionId = functionPointer1.functionId;
                                            classId = functionPointer1.classId;
                                            type = 2;
                                            break;
                                        case 4:
                                            // pointer to a primitive method;
                                            value = functionPointer1.context;
                                            functionId = functionPointer1.functionId;
                                            type = 9;
                                            break;
                                        case 5:
                                            // lambda instance;
                                            value = functionPointer1.context;
                                            functionId = functionPointer1.functionId;
                                            type = 10;
                                            closure = functionPointer1.closureVariables;
                                            break;
                                    }
                                }
                                else
                                {
                                    hasInterrupt = EX_InvalidInvocation(ec, "This type cannot be invoked like a function.");
                                }
                            }
                            if (((type == 9) && !hasInterrupt))
                            {
                                // primitive method invocation;
                                output = VALUE_NULL;
                                primitiveMethodToCoreLibraryFallback = false;
                                switch (value.type)
                                {
                                    case 5:
                                        // ...on a string;
                                        string1 = (string)value.internalValue;
                                        switch (functionId)
                                        {
                                            case 7:
                                                if ((argCount != 1))
                                                {
                                                    hasInterrupt = EX_InvalidArgument(ec, primitiveMethodWrongArgCountError("string contains method", 1, argCount));
                                                }
                                                else
                                                {
                                                    value2 = funcArgs[0];
                                                    if ((value2.type != 5))
                                                    {
                                                        hasInterrupt = EX_InvalidArgument(ec, "string contains method requires another string as input.");
                                                    }
                                                    else if (string1.Contains((string)value2.internalValue))
                                                    {
                                                        output = VALUE_TRUE;
                                                    }
                                                    else
                                                    {
                                                        output = VALUE_FALSE;
                                                    }
                                                }
                                                break;
                                            case 9:
                                                if ((argCount != 1))
                                                {
                                                    hasInterrupt = EX_InvalidArgument(ec, primitiveMethodWrongArgCountError("string endsWith method", 1, argCount));
                                                }
                                                else
                                                {
                                                    value2 = funcArgs[0];
                                                    if ((value2.type != 5))
                                                    {
                                                        hasInterrupt = EX_InvalidArgument(ec, "string endsWith method requires another string as input.");
                                                    }
                                                    else if (string1.EndsWith((string)value2.internalValue))
                                                    {
                                                        output = VALUE_TRUE;
                                                    }
                                                    else
                                                    {
                                                        output = VALUE_FALSE;
                                                    }
                                                }
                                                break;
                                            case 13:
                                                if (((argCount < 1) || (argCount > 2)))
                                                {
                                                    hasInterrupt = EX_InvalidArgument(ec, primitiveMethodWrongArgCountError("string indexOf method", 1, argCount));
                                                }
                                                else
                                                {
                                                    value2 = funcArgs[0];
                                                    if ((value2.type != 5))
                                                    {
                                                        hasInterrupt = EX_InvalidArgument(ec, "string indexOf method requires another string as input.");
                                                    }
                                                    else if ((argCount == 1))
                                                    {
                                                        output = buildInteger(globals, string1.IndexOf((string)value2.internalValue));
                                                    }
                                                    else if ((funcArgs[1].type != 3))
                                                    {
                                                        hasInterrupt = EX_InvalidArgument(ec, "string indexOf method requires an integer as its second argument.");
                                                    }
                                                    else
                                                    {
                                                        int1 = (int)funcArgs[1].internalValue;
                                                        if (((int1 < 0) || (int1 >= string1.Length)))
                                                        {
                                                            hasInterrupt = EX_IndexOutOfRange(ec, "String index is out of bounds.");
                                                        }
                                                        else
                                                        {
                                                            output = buildInteger(globals, string1.IndexOf((string)value2.internalValue, int1));
                                                        }
                                                    }
                                                }
                                                break;
                                            case 19:
                                                if ((argCount > 0))
                                                {
                                                    hasInterrupt = EX_InvalidArgument(ec, primitiveMethodWrongArgCountError("string lower method", 0, argCount));
                                                }
                                                else
                                                {
                                                    output = buildString(globals, string1.ToLower());
                                                }
                                                break;
                                            case 20:
                                                if ((argCount > 0))
                                                {
                                                    hasInterrupt = EX_InvalidArgument(ec, primitiveMethodWrongArgCountError("string ltrim method", 0, argCount));
                                                }
                                                else
                                                {
                                                    output = buildString(globals, string1.TrimStart());
                                                }
                                                break;
                                            case 25:
                                                if ((argCount != 2))
                                                {
                                                    hasInterrupt = EX_InvalidArgument(ec, primitiveMethodWrongArgCountError("string replace method", 2, argCount));
                                                }
                                                else
                                                {
                                                    value2 = funcArgs[0];
                                                    value3 = funcArgs[1];
                                                    if (((value2.type != 5) || (value3.type != 5)))
                                                    {
                                                        hasInterrupt = EX_InvalidArgument(ec, "string replace method requires 2 strings as input.");
                                                    }
                                                    else
                                                    {
                                                        output = buildString(globals, string1.Replace((string)value2.internalValue, (string)value3.internalValue));
                                                    }
                                                }
                                                break;
                                            case 26:
                                                if ((argCount > 0))
                                                {
                                                    hasInterrupt = EX_InvalidArgument(ec, primitiveMethodWrongArgCountError("string reverse method", 0, argCount));
                                                }
                                                else
                                                {
                                                    output = buildString(globals, PST_StringReverse(string1));
                                                }
                                                break;
                                            case 27:
                                                if ((argCount > 0))
                                                {
                                                    hasInterrupt = EX_InvalidArgument(ec, primitiveMethodWrongArgCountError("string rtrim method", 0, argCount));
                                                }
                                                else
                                                {
                                                    output = buildString(globals, string1.TrimEnd());
                                                }
                                                break;
                                            case 30:
                                                if ((argCount != 1))
                                                {
                                                    hasInterrupt = EX_InvalidArgument(ec, primitiveMethodWrongArgCountError("string split method", 1, argCount));
                                                }
                                                else
                                                {
                                                    value2 = funcArgs[0];
                                                    if ((value2.type != 5))
                                                    {
                                                        hasInterrupt = EX_InvalidArgument(ec, "string split method requires another string as input.");
                                                    }
                                                    else
                                                    {
                                                        stringList = PST_StringSplit(string1, (string)value2.internalValue);
                                                        _len = stringList.Length;
                                                        list1 = makeEmptyList(globals.stringType, _len);
                                                        i = 0;
                                                        while ((i < _len))
                                                        {
                                                            list1.array[i] = buildString(globals, stringList[i]);
                                                            i += 1;
                                                        }
                                                        list1.size = _len;
                                                        output = new Value(6, list1);
                                                    }
                                                }
                                                break;
                                            case 31:
                                                if ((argCount != 1))
                                                {
                                                    hasInterrupt = EX_InvalidArgument(ec, primitiveMethodWrongArgCountError("string startsWith method", 1, argCount));
                                                }
                                                else
                                                {
                                                    value2 = funcArgs[0];
                                                    if ((value2.type != 5))
                                                    {
                                                        hasInterrupt = EX_InvalidArgument(ec, "string startsWith method requires another string as input.");
                                                    }
                                                    else if (string1.StartsWith((string)value2.internalValue))
                                                    {
                                                        output = VALUE_TRUE;
                                                    }
                                                    else
                                                    {
                                                        output = VALUE_FALSE;
                                                    }
                                                }
                                                break;
                                            case 32:
                                                if ((argCount > 0))
                                                {
                                                    hasInterrupt = EX_InvalidArgument(ec, primitiveMethodWrongArgCountError("string trim method", 0, argCount));
                                                }
                                                else
                                                {
                                                    output = buildString(globals, string1.Trim());
                                                }
                                                break;
                                            case 33:
                                                if ((argCount > 0))
                                                {
                                                    hasInterrupt = EX_InvalidArgument(ec, primitiveMethodWrongArgCountError("string upper method", 0, argCount));
                                                }
                                                else
                                                {
                                                    output = buildString(globals, string1.ToUpper());
                                                }
                                                break;
                                            default:
                                                output = null;
                                                break;
                                        }
                                        break;
                                    case 6:
                                        // ...on a list;
                                        list1 = (ListImpl)value.internalValue;
                                        switch (functionId)
                                        {
                                            case 0:
                                                if ((argCount == 0))
                                                {
                                                    hasInterrupt = EX_InvalidArgument(ec, "List add method requires at least one argument.");
                                                }
                                                else
                                                {
                                                    intArray1 = list1.type;
                                                    while (((list1.size + argCount) > list1.capacity))
                                                    {
                                                        increaseListCapacity(list1);
                                                    }
                                                    int1 = list1.size;
                                                    i = 0;
                                                    while ((i < argCount))
                                                    {
                                                        value = funcArgs[i];
                                                        if ((intArray1 != null))
                                                        {
                                                            value2 = canAssignTypeToGeneric(vm, value, intArray1, 0);
                                                            if ((value2 == null))
                                                            {
                                                                hasInterrupt = EX_InvalidArgument(ec, string.Join("", new string[] { "Cannot convert a ",typeToStringFromValue(vm, value)," into a ",typeToString(vm, list1.type, 0) }));
                                                            }
                                                            list1.array[(int1 + i)] = value2;
                                                        }
                                                        else
                                                        {
                                                            list1.array[(int1 + i)] = value;
                                                        }
                                                        i += 1;
                                                    }
                                                    list1.size += argCount;
                                                    output = VALUE_NULL;
                                                }
                                                break;
                                            case 3:
                                                if ((argCount > 0))
                                                {
                                                    hasInterrupt = EX_InvalidArgument(ec, primitiveMethodWrongArgCountError("list choice method", 0, argCount));
                                                }
                                                else
                                                {
                                                    _len = list1.size;
                                                    if ((_len == 0))
                                                    {
                                                        hasInterrupt = EX_UnsupportedOperation(ec, "Cannot use list.choice() method on an empty list.");
                                                    }
                                                    else
                                                    {
                                                        i = (int)((PST_Random.NextDouble() * _len));
                                                        output = list1.array[i];
                                                    }
                                                }
                                                break;
                                            case 4:
                                                if ((argCount > 0))
                                                {
                                                    hasInterrupt = EX_InvalidArgument(ec, primitiveMethodWrongArgCountError("list clear method", 0, argCount));
                                                }
                                                else if ((list1.size > 0))
                                                {
                                                    i = (list1.size - 1);
                                                    while ((i >= 0))
                                                    {
                                                        list1.array[i] = null;
                                                        i -= 1;
                                                    }
                                                    list1.size = 0;
                                                }
                                                break;
                                            case 5:
                                                if ((argCount > 0))
                                                {
                                                    hasInterrupt = EX_InvalidArgument(ec, primitiveMethodWrongArgCountError("list clone method", 0, argCount));
                                                }
                                                else
                                                {
                                                    _len = list1.size;
                                                    list2 = makeEmptyList(list1.type, _len);
                                                    i = 0;
                                                    while ((i < _len))
                                                    {
                                                        list2.array[i] = list1.array[i];
                                                        i += 1;
                                                    }
                                                    list2.size = _len;
                                                    output = new Value(6, list2);
                                                }
                                                break;
                                            case 6:
                                                if ((argCount != 1))
                                                {
                                                    hasInterrupt = EX_InvalidArgument(ec, primitiveMethodWrongArgCountError("list concat method", 1, argCount));
                                                }
                                                else
                                                {
                                                    value2 = funcArgs[0];
                                                    if ((value2.type != 6))
                                                    {
                                                        hasInterrupt = EX_InvalidArgument(ec, "list concat methods requires a list as an argument.");
                                                    }
                                                    else
                                                    {
                                                        list2 = (ListImpl)value2.internalValue;
                                                        intArray1 = list1.type;
                                                        if (((intArray1 != null) && !canAssignGenericToGeneric(vm, list2.type, 0, intArray1, 0, intBuffer)))
                                                        {
                                                            hasInterrupt = EX_InvalidArgument(ec, "Cannot concat a list: incompatible types.");
                                                        }
                                                        else
                                                        {
                                                            if (((intArray1 != null) && (intArray1[0] == 4) && (list2.type[0] == 3)))
                                                            {
                                                                bool1 = true;
                                                            }
                                                            else
                                                            {
                                                                bool1 = false;
                                                            }
                                                            _len = list2.size;
                                                            int1 = list1.size;
                                                            while (((int1 + _len) > list1.capacity))
                                                            {
                                                                increaseListCapacity(list1);
                                                            }
                                                            i = 0;
                                                            while ((i < _len))
                                                            {
                                                                value = list2.array[i];
                                                                if (bool1)
                                                                {
                                                                    value = buildFloat(globals, (0.0 + (int)value.internalValue));
                                                                }
                                                                list1.array[(int1 + i)] = value;
                                                                i += 1;
                                                            }
                                                            list1.size += _len;
                                                        }
                                                    }
                                                }
                                                break;
                                            case 7:
                                                if ((argCount != 1))
                                                {
                                                    hasInterrupt = EX_InvalidArgument(ec, primitiveMethodWrongArgCountError("list contains method", 1, argCount));
                                                }
                                                else
                                                {
                                                    value2 = funcArgs[0];
                                                    _len = list1.size;
                                                    output = VALUE_FALSE;
                                                    i = 0;
                                                    while ((i < _len))
                                                    {
                                                        value = list1.array[i];
                                                        if ((doEqualityComparisonAndReturnCode(value2, value) == 1))
                                                        {
                                                            output = VALUE_TRUE;
                                                            i = _len;
                                                        }
                                                        i += 1;
                                                    }
                                                }
                                                break;
                                            case 10:
                                                if ((argCount != 1))
                                                {
                                                    hasInterrupt = EX_InvalidArgument(ec, primitiveMethodWrongArgCountError("list filter method", 1, argCount));
                                                }
                                                else
                                                {
                                                    value2 = funcArgs[0];
                                                    if ((value2.type != 9))
                                                    {
                                                        hasInterrupt = EX_InvalidArgument(ec, "list filter method requires a function pointer as its argument.");
                                                    }
                                                    else
                                                    {
                                                        primitiveMethodToCoreLibraryFallback = true;
                                                        functionId = metadata.primitiveMethodFunctionIdFallbackLookup[0];
                                                        funcArgs[1] = value;
                                                        argCount = 2;
                                                        output = null;
                                                    }
                                                }
                                                break;
                                            case 14:
                                                if ((argCount != 2))
                                                {
                                                    hasInterrupt = EX_InvalidArgument(ec, primitiveMethodWrongArgCountError("list insert method", 1, argCount));
                                                }
                                                else
                                                {
                                                    value = funcArgs[0];
                                                    value2 = funcArgs[1];
                                                    if ((value.type != 3))
                                                    {
                                                        hasInterrupt = EX_InvalidArgument(ec, "First argument of list.insert must be an integer index.");
                                                    }
                                                    else
                                                    {
                                                        intArray1 = list1.type;
                                                        if ((intArray1 != null))
                                                        {
                                                            value3 = canAssignTypeToGeneric(vm, value2, intArray1, 0);
                                                            if ((value3 == null))
                                                            {
                                                                hasInterrupt = EX_InvalidArgument(ec, "Cannot insert this type into this type of list.");
                                                            }
                                                            value2 = value3;
                                                        }
                                                        if (!hasInterrupt)
                                                        {
                                                            if ((list1.size == list1.capacity))
                                                            {
                                                                increaseListCapacity(list1);
                                                            }
                                                            int1 = (int)value.internalValue;
                                                            _len = list1.size;
                                                            if ((int1 < 0))
                                                            {
                                                                int1 += _len;
                                                            }
                                                            if ((int1 == _len))
                                                            {
                                                                list1.array[_len] = value2;
                                                                list1.size += 1;
                                                            }
                                                            else if (((int1 < 0) || (int1 >= _len)))
                                                            {
                                                                hasInterrupt = EX_IndexOutOfRange(ec, "Index out of range.");
                                                            }
                                                            else
                                                            {
                                                                i = int1;
                                                                while ((i < _len))
                                                                {
                                                                    value3 = list1.array[i];
                                                                    list1.array[i] = value2;
                                                                    value2 = value3;
                                                                    i += 1;
                                                                }
                                                                list1.array[_len] = value2;
                                                                list1.size += 1;
                                                            }
                                                        }
                                                    }
                                                }
                                                break;
                                            case 17:
                                                if ((argCount != 1))
                                                {
                                                    if ((argCount == 0))
                                                    {
                                                        value2 = globals.stringEmpty;
                                                    }
                                                    else
                                                    {
                                                        hasInterrupt = EX_InvalidArgument(ec, primitiveMethodWrongArgCountError("list join method", 1, argCount));
                                                    }
                                                }
                                                else
                                                {
                                                    value2 = funcArgs[0];
                                                    if ((value2.type != 5))
                                                    {
                                                        hasInterrupt = EX_InvalidArgument(ec, "Argument of list.join needs to be a string.");
                                                    }
                                                }
                                                if (!hasInterrupt)
                                                {
                                                    stringList1 = new List<string>();
                                                    string1 = (string)value2.internalValue;
                                                    _len = list1.size;
                                                    i = 0;
                                                    while ((i < _len))
                                                    {
                                                        value = list1.array[i];
                                                        if ((value.type != 5))
                                                        {
                                                            string2 = valueToString(vm, value);
                                                        }
                                                        else
                                                        {
                                                            string2 = (string)value.internalValue;
                                                        }
                                                        stringList1.Add(string2);
                                                        i += 1;
                                                    }
                                                    output = buildString(globals, string.Join(string1, stringList1));
                                                }
                                                break;
                                            case 21:
                                                if ((argCount != 1))
                                                {
                                                    hasInterrupt = EX_InvalidArgument(ec, primitiveMethodWrongArgCountError("list map method", 1, argCount));
                                                }
                                                else
                                                {
                                                    value2 = funcArgs[0];
                                                    if ((value2.type != 9))
                                                    {
                                                        hasInterrupt = EX_InvalidArgument(ec, "list map method requires a function pointer as its argument.");
                                                    }
                                                    else
                                                    {
                                                        primitiveMethodToCoreLibraryFallback = true;
                                                        functionId = metadata.primitiveMethodFunctionIdFallbackLookup[1];
                                                        funcArgs[1] = value;
                                                        argCount = 2;
                                                        output = null;
                                                    }
                                                }
                                                break;
                                            case 23:
                                                if ((argCount > 0))
                                                {
                                                    hasInterrupt = EX_InvalidArgument(ec, primitiveMethodWrongArgCountError("list pop method", 0, argCount));
                                                }
                                                else
                                                {
                                                    _len = list1.size;
                                                    if ((_len < 1))
                                                    {
                                                        hasInterrupt = EX_IndexOutOfRange(ec, "Cannot pop from an empty list.");
                                                    }
                                                    else
                                                    {
                                                        _len -= 1;
                                                        value = list1.array[_len];
                                                        list1.array[_len] = null;
                                                        if (returnValueUsed)
                                                        {
                                                            output = value;
                                                        }
                                                        list1.size = _len;
                                                    }
                                                }
                                                break;
                                            case 24:
                                                if ((argCount != 1))
                                                {
                                                    hasInterrupt = EX_InvalidArgument(ec, primitiveMethodWrongArgCountError("list remove method", 1, argCount));
                                                }
                                                else
                                                {
                                                    value = funcArgs[0];
                                                    if ((value.type != 3))
                                                    {
                                                        hasInterrupt = EX_InvalidArgument(ec, "Argument of list.remove must be an integer index.");
                                                    }
                                                    else
                                                    {
                                                        int1 = (int)value.internalValue;
                                                        _len = list1.size;
                                                        if ((int1 < 0))
                                                        {
                                                            int1 += _len;
                                                        }
                                                        if (((int1 < 0) || (int1 >= _len)))
                                                        {
                                                            hasInterrupt = EX_IndexOutOfRange(ec, "Index out of range.");
                                                        }
                                                        else
                                                        {
                                                            if (returnValueUsed)
                                                            {
                                                                output = list1.array[int1];
                                                            }
                                                            _len = (list1.size - 1);
                                                            list1.size = _len;
                                                            i = int1;
                                                            while ((i < _len))
                                                            {
                                                                list1.array[i] = list1.array[(i + 1)];
                                                                i += 1;
                                                            }
                                                            list1.array[_len] = null;
                                                        }
                                                    }
                                                }
                                                break;
                                            case 26:
                                                if ((argCount > 0))
                                                {
                                                    hasInterrupt = EX_InvalidArgument(ec, primitiveMethodWrongArgCountError("list reverse method", 0, argCount));
                                                }
                                                else
                                                {
                                                    reverseList(list1);
                                                }
                                                break;
                                            case 28:
                                                if ((argCount > 0))
                                                {
                                                    hasInterrupt = EX_InvalidArgument(ec, primitiveMethodWrongArgCountError("list shuffle method", 0, argCount));
                                                }
                                                else
                                                {
                                                    _len = list1.size;
                                                    i = 0;
                                                    while ((i < _len))
                                                    {
                                                        j = (int)((PST_Random.NextDouble() * _len));
                                                        value = list1.array[i];
                                                        list1.array[i] = list1.array[j];
                                                        list1.array[j] = value;
                                                        i += 1;
                                                    }
                                                }
                                                break;
                                            case 29:
                                                if ((argCount == 0))
                                                {
                                                    sortLists(list1, list1, PST_IntBuffer16);
                                                    if ((PST_IntBuffer16[0] > 0))
                                                    {
                                                        hasInterrupt = EX_InvalidArgument(ec, "Invalid list to sort. All items must be numbers or all strings, but not mixed.");
                                                    }
                                                }
                                                else if ((argCount == 1))
                                                {
                                                    value2 = funcArgs[0];
                                                    if ((value2.type == 9))
                                                    {
                                                        primitiveMethodToCoreLibraryFallback = true;
                                                        functionId = metadata.primitiveMethodFunctionIdFallbackLookup[2];
                                                        funcArgs[1] = value;
                                                        argCount = 2;
                                                    }
                                                    else
                                                    {
                                                        hasInterrupt = EX_InvalidArgument(ec, "list.sort(get_key_function) requires a function pointer as its argument.");
                                                    }
                                                    output = null;
                                                }
                                                break;
                                            default:
                                                output = null;
                                                break;
                                        }
                                        break;
                                    case 7:
                                        // ...on a dictionary;
                                        dictImpl = (DictImpl)value.internalValue;
                                        switch (functionId)
                                        {
                                            case 4:
                                                if ((argCount > 0))
                                                {
                                                    hasInterrupt = EX_InvalidArgument(ec, primitiveMethodWrongArgCountError("dictionary clear method", 0, argCount));
                                                }
                                                else if ((dictImpl.size > 0))
                                                {
                                                    dictImpl.intToIndex = new Dictionary<int, int>();
                                                    dictImpl.stringToIndex = new Dictionary<string, int>();
                                                    dictImpl.keys.Clear();
                                                    dictImpl.values.Clear();
                                                    dictImpl.size = 0;
                                                }
                                                break;
                                            case 5:
                                                if ((argCount > 0))
                                                {
                                                    hasInterrupt = EX_InvalidArgument(ec, primitiveMethodWrongArgCountError("dictionary clone method", 0, argCount));
                                                }
                                                else
                                                {
                                                    output = new Value(7, cloneDictionary(dictImpl, null));
                                                }
                                                break;
                                            case 7:
                                                if ((argCount != 1))
                                                {
                                                    hasInterrupt = EX_InvalidArgument(ec, primitiveMethodWrongArgCountError("dictionary contains method", 1, argCount));
                                                }
                                                else
                                                {
                                                    value = funcArgs[0];
                                                    output = VALUE_FALSE;
                                                    if ((value.type == 5))
                                                    {
                                                        if (dictImpl.stringToIndex.ContainsKey((string)value.internalValue))
                                                        {
                                                            output = VALUE_TRUE;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        if ((value.type == 3))
                                                        {
                                                            i = (int)value.internalValue;
                                                        }
                                                        else
                                                        {
                                                            i = ((ObjectInstance)value.internalValue).objectId;
                                                        }
                                                        if (dictImpl.intToIndex.ContainsKey(i))
                                                        {
                                                            output = VALUE_TRUE;
                                                        }
                                                    }
                                                }
                                                break;
                                            case 11:
                                                if (((argCount != 1) && (argCount != 2)))
                                                {
                                                    hasInterrupt = EX_InvalidArgument(ec, "Dictionary get method requires 1 or 2 arguments.");
                                                }
                                                else
                                                {
                                                    value = funcArgs[0];
                                                    switch (value.type)
                                                    {
                                                        case 3:
                                                            int1 = (int)value.internalValue;
                                                            if (!dictImpl.intToIndex.TryGetValue(int1, out i)) i = -1;
                                                            break;
                                                        case 8:
                                                            int1 = ((ObjectInstance)value.internalValue).objectId;
                                                            if (!dictImpl.intToIndex.TryGetValue(int1, out i)) i = -1;
                                                            break;
                                                        case 5:
                                                            string1 = (string)value.internalValue;
                                                            if (!dictImpl.stringToIndex.TryGetValue(string1, out i)) i = -1;
                                                            break;
                                                    }
                                                    if ((i == -1))
                                                    {
                                                        if ((argCount == 2))
                                                        {
                                                            output = funcArgs[1];
                                                        }
                                                        else
                                                        {
                                                            output = VALUE_NULL;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        output = dictImpl.values[i];
                                                    }
                                                }
                                                break;
                                            case 18:
                                                if ((argCount > 0))
                                                {
                                                    hasInterrupt = EX_InvalidArgument(ec, primitiveMethodWrongArgCountError("dictionary keys method", 0, argCount));
                                                }
                                                else
                                                {
                                                    valueList1 = dictImpl.keys;
                                                    _len = valueList1.Count;
                                                    if ((dictImpl.keyType == 8))
                                                    {
                                                        intArray1 = new int[2];
                                                        intArray1[0] = 8;
                                                        intArray1[0] = dictImpl.keyClassId;
                                                    }
                                                    else
                                                    {
                                                        intArray1 = new int[1];
                                                        intArray1[0] = dictImpl.keyType;
                                                    }
                                                    list1 = makeEmptyList(intArray1, _len);
                                                    i = 0;
                                                    while ((i < _len))
                                                    {
                                                        list1.array[i] = valueList1[i];
                                                        i += 1;
                                                    }
                                                    list1.size = _len;
                                                    output = new Value(6, list1);
                                                }
                                                break;
                                            case 22:
                                                if ((argCount != 1))
                                                {
                                                    hasInterrupt = EX_InvalidArgument(ec, primitiveMethodWrongArgCountError("dictionary merge method", 1, argCount));
                                                }
                                                else
                                                {
                                                    value2 = funcArgs[0];
                                                    if ((value2.type != 7))
                                                    {
                                                        hasInterrupt = EX_InvalidArgument(ec, "dictionary merge method requires another dictionary as a parameeter.");
                                                    }
                                                    else
                                                    {
                                                        dictImpl2 = (DictImpl)value2.internalValue;
                                                        if ((dictImpl2.size > 0))
                                                        {
                                                            if ((dictImpl.size == 0))
                                                            {
                                                                value.internalValue = cloneDictionary(dictImpl2, null);
                                                            }
                                                            else if ((dictImpl2.keyType != dictImpl.keyType))
                                                            {
                                                                hasInterrupt = EX_InvalidKey(ec, "Dictionaries with different key types cannot be merged.");
                                                            }
                                                            else if (((dictImpl2.keyType == 8) && (dictImpl2.keyClassId != dictImpl.keyClassId) && (dictImpl.keyClassId != 0) && !isClassASubclassOf(vm, dictImpl2.keyClassId, dictImpl.keyClassId)))
                                                            {
                                                                hasInterrupt = EX_InvalidKey(ec, "Dictionary key types are incompatible.");
                                                            }
                                                            else
                                                            {
                                                                if ((dictImpl.valueType == null))
                                                                {
                                                                }
                                                                else if ((dictImpl2.valueType == null))
                                                                {
                                                                    hasInterrupt = EX_InvalidKey(ec, "Dictionaries with different value types cannot be merged.");
                                                                }
                                                                else if (!canAssignGenericToGeneric(vm, dictImpl2.valueType, 0, dictImpl.valueType, 0, intBuffer))
                                                                {
                                                                    hasInterrupt = EX_InvalidKey(ec, "The dictionary value types are incompatible.");
                                                                }
                                                                if (!hasInterrupt)
                                                                {
                                                                    cloneDictionary(dictImpl2, dictImpl);
                                                                }
                                                            }
                                                        }
                                                        output = VALUE_NULL;
                                                    }
                                                }
                                                break;
                                            case 24:
                                                if ((argCount != 1))
                                                {
                                                    hasInterrupt = EX_InvalidArgument(ec, primitiveMethodWrongArgCountError("dictionary remove method", 1, argCount));
                                                }
                                                else
                                                {
                                                    value2 = funcArgs[0];
                                                    bool2 = false;
                                                    keyType = dictImpl.keyType;
                                                    if (((dictImpl.size > 0) && (keyType == value2.type)))
                                                    {
                                                        if ((keyType == 5))
                                                        {
                                                            stringKey = (string)value2.internalValue;
                                                            if (dictImpl.stringToIndex.ContainsKey(stringKey))
                                                            {
                                                                i = dictImpl.stringToIndex[stringKey];
                                                                bool2 = true;
                                                            }
                                                        }
                                                        else
                                                        {
                                                            if ((keyType == 3))
                                                            {
                                                                intKey = (int)value2.internalValue;
                                                            }
                                                            else
                                                            {
                                                                intKey = ((ObjectInstance)value2.internalValue).objectId;
                                                            }
                                                            if (dictImpl.intToIndex.ContainsKey(intKey))
                                                            {
                                                                i = dictImpl.intToIndex[intKey];
                                                                bool2 = true;
                                                            }
                                                        }
                                                        if (bool2)
                                                        {
                                                            _len = (dictImpl.size - 1);
                                                            dictImpl.size = _len;
                                                            if ((i == _len))
                                                            {
                                                                if ((keyType == 5))
                                                                {
                                                                    dictImpl.stringToIndex.Remove(stringKey);
                                                                }
                                                                else
                                                                {
                                                                    dictImpl.intToIndex.Remove(intKey);
                                                                }
                                                                dictImpl.keys.RemoveAt(i);
                                                                dictImpl.values.RemoveAt(i);
                                                            }
                                                            else
                                                            {
                                                                value = dictImpl.keys[_len];
                                                                dictImpl.keys[i] = value;
                                                                dictImpl.values[i] = dictImpl.values[_len];
                                                                dictImpl.keys.RemoveAt(dictImpl.keys.Count - 1);
                                                                dictImpl.values.RemoveAt(dictImpl.values.Count - 1);
                                                                if ((keyType == 5))
                                                                {
                                                                    dictImpl.stringToIndex.Remove(stringKey);
                                                                    stringKey = (string)value.internalValue;
                                                                    dictImpl.stringToIndex[stringKey] = i;
                                                                }
                                                                else
                                                                {
                                                                    dictImpl.intToIndex.Remove(intKey);
                                                                    if ((keyType == 3))
                                                                    {
                                                                        intKey = (int)value.internalValue;
                                                                    }
                                                                    else
                                                                    {
                                                                        intKey = ((ObjectInstance)value.internalValue).objectId;
                                                                    }
                                                                    dictImpl.intToIndex[intKey] = i;
                                                                }
                                                            }
                                                        }
                                                    }
                                                    if (!bool2)
                                                    {
                                                        hasInterrupt = EX_KeyNotFound(ec, "dictionary does not contain the given key.");
                                                    }
                                                }
                                                break;
                                            case 34:
                                                if ((argCount > 0))
                                                {
                                                    hasInterrupt = EX_InvalidArgument(ec, primitiveMethodWrongArgCountError("dictionary values method", 0, argCount));
                                                }
                                                else
                                                {
                                                    valueList1 = dictImpl.values;
                                                    _len = valueList1.Count;
                                                    list1 = makeEmptyList(dictImpl.valueType, _len);
                                                    i = 0;
                                                    while ((i < _len))
                                                    {
                                                        addToList(list1, valueList1[i]);
                                                        i += 1;
                                                    }
                                                    output = new Value(6, list1);
                                                }
                                                break;
                                            default:
                                                output = null;
                                                break;
                                        }
                                        break;
                                    case 9:
                                        // ...on a function pointer;
                                        functionPointer1 = (FunctionPointer)value.internalValue;
                                        switch (functionId)
                                        {
                                            case 1:
                                                if ((argCount > 0))
                                                {
                                                    hasInterrupt = EX_InvalidArgument(ec, primitiveMethodWrongArgCountError("argCountMax method", 0, argCount));
                                                }
                                                else
                                                {
                                                    functionId = functionPointer1.functionId;
                                                    functionInfo = metadata.functionTable[functionId];
                                                    output = buildInteger(globals, functionInfo.maxArgs);
                                                }
                                                break;
                                            case 2:
                                                if ((argCount > 0))
                                                {
                                                    hasInterrupt = EX_InvalidArgument(ec, primitiveMethodWrongArgCountError("argCountMin method", 0, argCount));
                                                }
                                                else
                                                {
                                                    functionId = functionPointer1.functionId;
                                                    functionInfo = metadata.functionTable[functionId];
                                                    output = buildInteger(globals, functionInfo.minArgs);
                                                }
                                                break;
                                            case 12:
                                                functionInfo = metadata.functionTable[functionPointer1.functionId];
                                                output = buildString(globals, functionInfo.name);
                                                break;
                                            case 15:
                                                if ((argCount == 1))
                                                {
                                                    funcArgs[1] = funcArgs[0];
                                                }
                                                else if ((argCount == 0))
                                                {
                                                    funcArgs[1] = VALUE_NULL;
                                                }
                                                else
                                                {
                                                    hasInterrupt = EX_InvalidArgument(ec, "invoke requires a list of arguments.");
                                                }
                                                funcArgs[0] = value;
                                                argCount = 2;
                                                primitiveMethodToCoreLibraryFallback = true;
                                                functionId = metadata.primitiveMethodFunctionIdFallbackLookup[3];
                                                output = null;
                                                break;
                                            default:
                                                output = null;
                                                break;
                                        }
                                        break;
                                    case 10:
                                        // ...on a class definition;
                                        classValue = (ClassValue)value.internalValue;
                                        switch (functionId)
                                        {
                                            case 12:
                                                classInfo = metadata.classTable[classValue.classId];
                                                output = buildString(globals, classInfo.fullyQualifiedName);
                                                break;
                                            case 16:
                                                if ((argCount != 1))
                                                {
                                                    hasInterrupt = EX_InvalidArgument(ec, primitiveMethodWrongArgCountError("class isA method", 1, argCount));
                                                }
                                                else
                                                {
                                                    int1 = classValue.classId;
                                                    value = funcArgs[0];
                                                    if ((value.type != 10))
                                                    {
                                                        hasInterrupt = EX_InvalidArgument(ec, "class isA method requires another class reference.");
                                                    }
                                                    else
                                                    {
                                                        classValue = (ClassValue)value.internalValue;
                                                        int2 = classValue.classId;
                                                        output = VALUE_FALSE;
                                                        if (isClassASubclassOf(vm, int1, int2))
                                                        {
                                                            output = VALUE_TRUE;
                                                        }
                                                    }
                                                }
                                                break;
                                            default:
                                                output = null;
                                                break;
                                        }
                                        break;
                                }
                                if (!hasInterrupt)
                                {
                                    if ((output == null))
                                    {
                                        if (primitiveMethodToCoreLibraryFallback)
                                        {
                                            type = 1;
                                            bool1 = true;
                                        }
                                        else
                                        {
                                            hasInterrupt = EX_InvalidInvocation(ec, "primitive method not found.");
                                        }
                                    }
                                    else
                                    {
                                        if (returnValueUsed)
                                        {
                                            if ((valueStackSize == valueStackCapacity))
                                            {
                                                valueStack = valueStackIncreaseCapacity(ec);
                                                valueStackCapacity = valueStack.Length;
                                            }
                                            valueStack[valueStackSize] = output;
                                            valueStackSize += 1;
                                        }
                                        bool1 = false;
                                    }
                                }
                            }
                            if ((bool1 && !hasInterrupt))
                            {
                                // push a new frame to the stack;
                                stack.pc = pc;
                                bool1 = false;
                                switch (type)
                                {
                                    case 1:
                                        // function;
                                        functionInfo = functionTable[functionId];
                                        pc = functionInfo.pc;
                                        value = null;
                                        classId = 0;
                                        break;
                                    case 10:
                                        // lambda;
                                        pc = functionId;
                                        functionInfo = metadata.lambdaTable[functionId];
                                        value = null;
                                        classId = 0;
                                        break;
                                    case 2:
                                        // static method;
                                        functionInfo = functionTable[functionId];
                                        pc = functionInfo.pc;
                                        value = null;
                                        classId = 0;
                                        break;
                                    case 3:
                                        // non-static method;
                                        functionInfo = functionTable[functionId];
                                        pc = functionInfo.pc;
                                        classId = 0;
                                        break;
                                    case 6:
                                        // constructor;
                                        vm.instanceCounter += 1;
                                        classInfo = classTable[classId];
                                        valueArray1 = new Value[classInfo.memberCount];
                                        i = (valueArray1.Length - 1);
                                        while ((i >= 0))
                                        {
                                            switch (classInfo.fieldInitializationCommand[i])
                                            {
                                                case 0:
                                                    valueArray1[i] = classInfo.fieldInitializationLiteral[i];
                                                    break;
                                                case 1:
                                                    break;
                                                case 2:
                                                    break;
                                            }
                                            i -= 1;
                                        }
                                        objInstance1 = new ObjectInstance(classId, vm.instanceCounter, valueArray1, null, null);
                                        value = new Value(8, objInstance1);
                                        functionId = classInfo.constructorFunctionId;
                                        functionInfo = functionTable[functionId];
                                        pc = functionInfo.pc;
                                        classId = 0;
                                        if (returnValueUsed)
                                        {
                                            returnValueUsed = false;
                                            if ((valueStackSize == valueStackCapacity))
                                            {
                                                valueStack = valueStackIncreaseCapacity(ec);
                                                valueStackCapacity = valueStack.Length;
                                            }
                                            valueStack[valueStackSize] = value;
                                            valueStackSize += 1;
                                        }
                                        break;
                                    case 7:
                                        // base constructor;
                                        value = stack.objectContext;
                                        classInfo = classTable[classId];
                                        functionId = classInfo.constructorFunctionId;
                                        functionInfo = functionTable[functionId];
                                        pc = functionInfo.pc;
                                        classId = 0;
                                        break;
                                }
                                if (((argCount < functionInfo.minArgs) || (argCount > functionInfo.maxArgs)))
                                {
                                    pc = stack.pc;
                                    hasInterrupt = EX_InvalidArgument(ec, "Incorrect number of args were passed to this function.");
                                }
                                else
                                {
                                    int1 = functionInfo.localsSize;
                                    int2 = stack.localsStackOffsetEnd;
                                    if ((localsStackCapacity <= (int2 + int1)))
                                    {
                                        increaseLocalsStackCapacity(ec, int1);
                                        localsStack = ec.localsStack;
                                        localsStackSet = ec.localsStackSet;
                                        localsStackCapacity = localsStack.Length;
                                    }
                                    localsStackSetToken = (ec.localsStackSetToken + 1);
                                    ec.localsStackSetToken = localsStackSetToken;
                                    if ((localsStackSetToken > 2000000000))
                                    {
                                        resetLocalsStackTokens(ec, stack);
                                        localsStackSetToken = 2;
                                    }
                                    localsStackOffset = int2;
                                    if ((type == 10))
                                    {
                                        value = closure[-1].value;
                                    }
                                    else
                                    {
                                        closure = null;
                                    }
                                    // invoke the function;
                                    stack = new StackFrame(pc, localsStackSetToken, localsStackOffset, (localsStackOffset + int1), stack, returnValueUsed, value, valueStackSize, 0, (stack.depth + 1), 0, null, closure, null);
                                    i = 0;
                                    while ((i < argCount))
                                    {
                                        int1 = (localsStackOffset + i);
                                        localsStack[int1] = funcArgs[i];
                                        localsStackSet[int1] = localsStackSetToken;
                                        i += 1;
                                    }
                                    if ((argCount != functionInfo.minArgs))
                                    {
                                        int1 = (argCount - functionInfo.minArgs);
                                        if ((int1 > 0))
                                        {
                                            pc += functionInfo.pcOffsetsForOptionalArgs[int1];
                                            stack.pc = pc;
                                        }
                                    }
                                    if ((stack.depth > 1000))
                                    {
                                        hasInterrupt = EX_Fatal(ec, "Stack overflow.");
                                    }
                                }
                            }
                        }
                        break;
                    case 13:
                        // CAST;
                        value = valueStack[(valueStackSize - 1)];
                        value2 = canAssignTypeToGeneric(vm, value, row, 0);
                        if ((value2 == null))
                        {
                            if (((value.type == 4) && (row[0] == 3)))
                            {
                                if ((row[1] == 1))
                                {
                                    float1 = (double)value.internalValue;
                                    if (((float1 < 0) && ((float1 % 1) != 0)))
                                    {
                                        i = ((int)float1 - 1);
                                    }
                                    else
                                    {
                                        i = (int)float1;
                                    }
                                    if ((i < 0))
                                    {
                                        if ((i > -257))
                                        {
                                            value2 = globals.negativeIntegers[-i];
                                        }
                                        else
                                        {
                                            value2 = new Value(3, i);
                                        }
                                    }
                                    else if ((i < 2049))
                                    {
                                        value2 = globals.positiveIntegers[i];
                                    }
                                    else
                                    {
                                        value2 = new Value(3, i);
                                    }
                                }
                            }
                            else if (((value.type == 3) && (row[0] == 4)))
                            {
                                int1 = (int)value.internalValue;
                                if ((int1 == 0))
                                {
                                    value2 = VALUE_FLOAT_ZERO;
                                }
                                else
                                {
                                    value2 = new Value(4, (0.0 + int1));
                                }
                            }
                            if ((value2 != null))
                            {
                                valueStack[(valueStackSize - 1)] = value2;
                            }
                        }
                        if ((value2 == null))
                        {
                            hasInterrupt = EX_InvalidArgument(ec, string.Join("", new string[] { "Cannot convert a ",typeToStringFromValue(vm, value)," to a ",typeToString(vm, row, 0) }));
                        }
                        else
                        {
                            valueStack[(valueStackSize - 1)] = value2;
                        }
                        break;
                    case 14:
                        // CLASS_DEFINITION;
                        initializeClass(pc, vm, row, stringArgs[pc]);
                        classTable = metadata.classTable;
                        break;
                    case 15:
                        // CNI_INVOKE;
                        nativeFp = metadata.cniFunctionsById[row[0]];
                        if ((nativeFp == null))
                        {
                            hasInterrupt = EX_InvalidInvocation(ec, "CNI method could not be found.");
                        }
                        else
                        {
                            _len = row[1];
                            valueStackSize -= _len;
                            valueArray1 = new Value[_len];
                            i = 0;
                            while ((i < _len))
                            {
                                valueArray1[i] = valueStack[(valueStackSize + i)];
                                i += 1;
                            }
                            prepareToSuspend(ec, stack, valueStackSize, pc);
                            value = nativeFp(vm, valueArray1);
                            if ((row[2] == 1))
                            {
                                if ((valueStackSize == valueStackCapacity))
                                {
                                    valueStack = valueStackIncreaseCapacity(ec);
                                    valueStackCapacity = valueStack.Length;
                                }
                                valueStack[valueStackSize] = value;
                                valueStackSize += 1;
                            }
                            if (ec.executionStateChange)
                            {
                                prepareToSuspend(ec, stack, valueStackSize, pc);
                                ec.executionStateChange = false;
                                if ((ec.executionStateChangeCommand == 1))
                                {
                                    return suspendInterpreter();
                                }
                            }
                        }
                        break;
                    case 16:
                        // CNI_REGISTER;
                        nativeFp = (System.Func<VmContext, Value[], Value>)TranslationHelper.GetFunctionPointer(stringArgs[pc]);
                        metadata.cniFunctionsById[row[0]] = nativeFp;
                        break;
                    case 17:
                        // COMMAND_LINE_ARGS;
                        if ((valueStackSize == valueStackCapacity))
                        {
                            valueStack = valueStackIncreaseCapacity(ec);
                            valueStackCapacity = valueStack.Length;
                        }
                        list1 = makeEmptyList(globals.stringType, 3);
                        i = 0;
                        while ((i < vm.environment.commandLineArgs.Length))
                        {
                            addToList(list1, buildString(globals, vm.environment.commandLineArgs[i]));
                            i += 1;
                        }
                        valueStack[valueStackSize] = new Value(6, list1);
                        valueStackSize += 1;
                        break;
                    case 18:
                        // CONTINUE;
                        if ((row[0] == 1))
                        {
                            pc += row[1];
                        }
                        else
                        {
                            intArray1 = esfData[pc];
                            pc = (intArray1[1] - 1);
                            valueStackSize = stack.valueStackPopSize;
                            stack.postFinallyBehavior = 2;
                        }
                        break;
                    case 19:
                        // CORE_FUNCTION;
                        switch (row[0])
                        {
                            case 1:
                                // parseInt;
                                arg1 = valueStack[--valueStackSize];
                                output = VALUE_NULL;
                                if ((arg1.type == 5))
                                {
                                    string1 = ((string)arg1.internalValue).Trim();
                                    if (PST_IsValidInteger(string1))
                                    {
                                        output = buildInteger(globals, int.Parse(string1));
                                    }
                                }
                                else
                                {
                                    hasInterrupt = EX_InvalidArgument(ec, "parseInt requires a string argument.");
                                }
                                break;
                            case 2:
                                // parseFloat;
                                arg1 = valueStack[--valueStackSize];
                                output = VALUE_NULL;
                                if ((arg1.type == 5))
                                {
                                    string1 = ((string)arg1.internalValue).Trim();
                                    PST_ParseFloat(string1, floatList1);
                                    if ((floatList1[0] >= 0))
                                    {
                                        output = buildFloat(globals, floatList1[1]);
                                    }
                                }
                                else
                                {
                                    hasInterrupt = EX_InvalidArgument(ec, "parseFloat requires a string argument.");
                                }
                                break;
                            case 3:
                                // print;
                                arg1 = valueStack[--valueStackSize];
                                output = VALUE_NULL;
                                printToStdOut(vm.environment.stdoutPrefix, valueToString(vm, arg1));
                                break;
                            case 4:
                                // typeof;
                                arg1 = valueStack[--valueStackSize];
                                output = buildInteger(globals, (arg1.type - 1));
                                break;
                            case 5:
                                // typeis;
                                arg1 = valueStack[--valueStackSize];
                                int1 = arg1.type;
                                int2 = row[2];
                                output = VALUE_FALSE;
                                while ((int2 > 0))
                                {
                                    if ((row[(2 + int2)] == int1))
                                    {
                                        output = VALUE_TRUE;
                                        int2 = 0;
                                    }
                                    else
                                    {
                                        int2 -= 1;
                                    }
                                }
                                break;
                            case 6:
                                // execId;
                                output = buildInteger(globals, ec.id);
                                break;
                            case 7:
                                // assert;
                                valueStackSize -= 3;
                                arg3 = valueStack[(valueStackSize + 2)];
                                arg2 = valueStack[(valueStackSize + 1)];
                                arg1 = valueStack[valueStackSize];
                                if ((arg1.type != 2))
                                {
                                    hasInterrupt = EX_InvalidArgument(ec, "Assertion expression must be a boolean.");
                                }
                                else if ((bool)arg1.internalValue)
                                {
                                    output = VALUE_NULL;
                                }
                                else
                                {
                                    string1 = valueToString(vm, arg2);
                                    if ((bool)arg3.internalValue)
                                    {
                                        string1 = "Assertion failed: " + string1;
                                    }
                                    hasInterrupt = EX_AssertionFailed(ec, string1);
                                }
                                break;
                            case 8:
                                // chr;
                                arg1 = valueStack[--valueStackSize];
                                output = null;
                                if ((arg1.type == 3))
                                {
                                    int1 = (int)arg1.internalValue;
                                    if (((int1 >= 0) && (int1 < 256)))
                                    {
                                        output = buildCommonString(globals, ((char) int1).ToString());
                                    }
                                }
                                if ((output == null))
                                {
                                    hasInterrupt = EX_InvalidArgument(ec, "chr requires an integer between 0 and 255.");
                                }
                                break;
                            case 9:
                                // ord;
                                arg1 = valueStack[--valueStackSize];
                                output = null;
                                if ((arg1.type == 5))
                                {
                                    string1 = (string)arg1.internalValue;
                                    if ((string1.Length == 1))
                                    {
                                        output = buildInteger(globals, ((int) string1[0]));
                                    }
                                }
                                if ((output == null))
                                {
                                    hasInterrupt = EX_InvalidArgument(ec, "ord requires a 1 character string.");
                                }
                                break;
                            case 10:
                                // currentTime;
                                output = buildFloat(globals, PST_CurrentTime);
                                break;
                            case 11:
                                // sortList;
                                valueStackSize -= 2;
                                arg2 = valueStack[(valueStackSize + 1)];
                                arg1 = valueStack[valueStackSize];
                                output = VALUE_NULL;
                                list1 = (ListImpl)arg1.internalValue;
                                list2 = (ListImpl)arg2.internalValue;
                                sortLists(list2, list1, PST_IntBuffer16);
                                if ((PST_IntBuffer16[0] > 0))
                                {
                                    hasInterrupt = EX_InvalidArgument(ec, "Invalid sort keys. Keys must be all numbers or all strings, but not mixed.");
                                }
                                break;
                            case 12:
                                // abs;
                                arg1 = valueStack[--valueStackSize];
                                output = arg1;
                                if ((arg1.type == 3))
                                {
                                    if (((int)arg1.internalValue < 0))
                                    {
                                        output = buildInteger(globals, -(int)arg1.internalValue);
                                    }
                                }
                                else if ((arg1.type == 4))
                                {
                                    if (((double)arg1.internalValue < 0))
                                    {
                                        output = buildFloat(globals, -(double)arg1.internalValue);
                                    }
                                }
                                else
                                {
                                    hasInterrupt = EX_InvalidArgument(ec, "abs requires a number as input.");
                                }
                                break;
                            case 13:
                                // arcCos;
                                arg1 = valueStack[--valueStackSize];
                                if ((arg1.type == 4))
                                {
                                    float1 = (double)arg1.internalValue;
                                }
                                else if ((arg1.type == 3))
                                {
                                    float1 = (0.0 + (int)arg1.internalValue);
                                }
                                else
                                {
                                    hasInterrupt = EX_InvalidArgument(ec, "arccos requires a number as input.");
                                }
                                if (!hasInterrupt)
                                {
                                    if (((float1 < -1) || (float1 > 1)))
                                    {
                                        hasInterrupt = EX_InvalidArgument(ec, "arccos requires a number in the range of -1 to 1.");
                                    }
                                    else
                                    {
                                        output = buildFloat(globals, System.Math.Acos(float1));
                                    }
                                }
                                break;
                            case 14:
                                // arcSin;
                                arg1 = valueStack[--valueStackSize];
                                if ((arg1.type == 4))
                                {
                                    float1 = (double)arg1.internalValue;
                                }
                                else if ((arg1.type == 3))
                                {
                                    float1 = (0.0 + (int)arg1.internalValue);
                                }
                                else
                                {
                                    hasInterrupt = EX_InvalidArgument(ec, "arcsin requires a number as input.");
                                }
                                if (!hasInterrupt)
                                {
                                    if (((float1 < -1) || (float1 > 1)))
                                    {
                                        hasInterrupt = EX_InvalidArgument(ec, "arcsin requires a number in the range of -1 to 1.");
                                    }
                                    else
                                    {
                                        output = buildFloat(globals, System.Math.Asin(float1));
                                    }
                                }
                                break;
                            case 15:
                                // arcTan;
                                valueStackSize -= 2;
                                arg2 = valueStack[(valueStackSize + 1)];
                                arg1 = valueStack[valueStackSize];
                                bool1 = false;
                                if ((arg1.type == 4))
                                {
                                    float1 = (double)arg1.internalValue;
                                }
                                else if ((arg1.type == 3))
                                {
                                    float1 = (0.0 + (int)arg1.internalValue);
                                }
                                else
                                {
                                    bool1 = true;
                                }
                                if ((arg2.type == 4))
                                {
                                    float2 = (double)arg2.internalValue;
                                }
                                else if ((arg2.type == 3))
                                {
                                    float2 = (0.0 + (int)arg2.internalValue);
                                }
                                else
                                {
                                    bool1 = true;
                                }
                                if (bool1)
                                {
                                    hasInterrupt = EX_InvalidArgument(ec, "arctan requires numeric arguments.");
                                }
                                else
                                {
                                    output = buildFloat(globals, System.Math.Atan2(float1, float2));
                                }
                                break;
                            case 16:
                                // cos;
                                arg1 = valueStack[--valueStackSize];
                                if ((arg1.type == 4))
                                {
                                    float1 = (double)arg1.internalValue;
                                    output = buildFloat(globals, System.Math.Cos(float1));
                                }
                                else if ((arg1.type == 3))
                                {
                                    int1 = (int)arg1.internalValue;
                                    output = buildFloat(globals, System.Math.Cos(int1));
                                }
                                else
                                {
                                    hasInterrupt = EX_InvalidArgument(ec, "cos requires a number argument.");
                                }
                                break;
                            case 17:
                                // ensureRange;
                                valueStackSize -= 3;
                                arg3 = valueStack[(valueStackSize + 2)];
                                arg2 = valueStack[(valueStackSize + 1)];
                                arg1 = valueStack[valueStackSize];
                                bool1 = false;
                                if ((arg2.type == 4))
                                {
                                    float2 = (double)arg2.internalValue;
                                }
                                else if ((arg2.type == 3))
                                {
                                    float2 = (0.0 + (int)arg2.internalValue);
                                }
                                else
                                {
                                    bool1 = true;
                                }
                                if ((arg3.type == 4))
                                {
                                    float3 = (double)arg3.internalValue;
                                }
                                else if ((arg3.type == 3))
                                {
                                    float3 = (0.0 + (int)arg3.internalValue);
                                }
                                else
                                {
                                    bool1 = true;
                                }
                                if ((!bool1 && (float3 < float2)))
                                {
                                    float1 = float3;
                                    float3 = float2;
                                    float2 = float1;
                                    value = arg2;
                                    arg2 = arg3;
                                    arg3 = value;
                                }
                                if ((arg1.type == 4))
                                {
                                    float1 = (double)arg1.internalValue;
                                }
                                else if ((arg1.type == 3))
                                {
                                    float1 = (0.0 + (int)arg1.internalValue);
                                }
                                else
                                {
                                    bool1 = true;
                                }
                                if (bool1)
                                {
                                    hasInterrupt = EX_InvalidArgument(ec, "ensureRange requires numeric arguments.");
                                }
                                else if ((float1 < float2))
                                {
                                    output = arg2;
                                }
                                else if ((float1 > float3))
                                {
                                    output = arg3;
                                }
                                else
                                {
                                    output = arg1;
                                }
                                break;
                            case 18:
                                // floor;
                                arg1 = valueStack[--valueStackSize];
                                if ((arg1.type == 4))
                                {
                                    float1 = (double)arg1.internalValue;
                                    if (((float1 < 0) && ((float1 % 1) != 0)))
                                    {
                                        int1 = ((int)float1 - 1);
                                    }
                                    else
                                    {
                                        int1 = (int)float1;
                                    }
                                    if ((int1 < 2049))
                                    {
                                        if ((int1 >= 0))
                                        {
                                            output = INTEGER_POSITIVE_CACHE[int1];
                                        }
                                        else if ((int1 > -257))
                                        {
                                            output = INTEGER_NEGATIVE_CACHE[-int1];
                                        }
                                        else
                                        {
                                            output = new Value(3, int1);
                                        }
                                    }
                                    else
                                    {
                                        output = new Value(3, int1);
                                    }
                                }
                                else if ((arg1.type == 3))
                                {
                                    output = arg1;
                                }
                                else
                                {
                                    hasInterrupt = EX_InvalidArgument(ec, "floor expects a numeric argument.");
                                }
                                break;
                            case 19:
                                // max;
                                valueStackSize -= 2;
                                arg2 = valueStack[(valueStackSize + 1)];
                                arg1 = valueStack[valueStackSize];
                                bool1 = false;
                                if ((arg1.type == 4))
                                {
                                    float1 = (double)arg1.internalValue;
                                }
                                else if ((arg1.type == 3))
                                {
                                    float1 = (0.0 + (int)arg1.internalValue);
                                }
                                else
                                {
                                    bool1 = true;
                                }
                                if ((arg2.type == 4))
                                {
                                    float2 = (double)arg2.internalValue;
                                }
                                else if ((arg2.type == 3))
                                {
                                    float2 = (0.0 + (int)arg2.internalValue);
                                }
                                else
                                {
                                    bool1 = true;
                                }
                                if (bool1)
                                {
                                    hasInterrupt = EX_InvalidArgument(ec, "max requires numeric arguments.");
                                }
                                else if ((float1 >= float2))
                                {
                                    output = arg1;
                                }
                                else
                                {
                                    output = arg2;
                                }
                                break;
                            case 20:
                                // min;
                                valueStackSize -= 2;
                                arg2 = valueStack[(valueStackSize + 1)];
                                arg1 = valueStack[valueStackSize];
                                bool1 = false;
                                if ((arg1.type == 4))
                                {
                                    float1 = (double)arg1.internalValue;
                                }
                                else if ((arg1.type == 3))
                                {
                                    float1 = (0.0 + (int)arg1.internalValue);
                                }
                                else
                                {
                                    bool1 = true;
                                }
                                if ((arg2.type == 4))
                                {
                                    float2 = (double)arg2.internalValue;
                                }
                                else if ((arg2.type == 3))
                                {
                                    float2 = (0.0 + (int)arg2.internalValue);
                                }
                                else
                                {
                                    bool1 = true;
                                }
                                if (bool1)
                                {
                                    hasInterrupt = EX_InvalidArgument(ec, "min requires numeric arguments.");
                                }
                                else if ((float1 <= float2))
                                {
                                    output = arg1;
                                }
                                else
                                {
                                    output = arg2;
                                }
                                break;
                            case 21:
                                // nativeInt;
                                valueStackSize -= 2;
                                arg2 = valueStack[(valueStackSize + 1)];
                                arg1 = valueStack[valueStackSize];
                                output = buildInteger(globals, (int)((ObjectInstance)arg1.internalValue).nativeData[(int)arg2.internalValue]);
                                break;
                            case 22:
                                // nativeString;
                                valueStackSize -= 3;
                                arg3 = valueStack[(valueStackSize + 2)];
                                arg2 = valueStack[(valueStackSize + 1)];
                                arg1 = valueStack[valueStackSize];
                                string1 = (string)((ObjectInstance)arg1.internalValue).nativeData[(int)arg2.internalValue];
                                if ((bool)arg3.internalValue)
                                {
                                    output = buildCommonString(globals, string1);
                                }
                                else
                                {
                                    output = buildString(globals, string1);
                                }
                                break;
                            case 23:
                                // sign;
                                arg1 = valueStack[--valueStackSize];
                                if ((arg1.type == 3))
                                {
                                    float1 = (0.0 + ((int)arg1.internalValue));
                                }
                                else if ((arg1.type == 4))
                                {
                                    float1 = (double)arg1.internalValue;
                                }
                                else
                                {
                                    hasInterrupt = EX_InvalidArgument(ec, "sign requires a number as input.");
                                }
                                if ((float1 == 0))
                                {
                                    output = VALUE_INT_ZERO;
                                }
                                else if ((float1 > 0))
                                {
                                    output = VALUE_INT_ONE;
                                }
                                else
                                {
                                    output = INTEGER_NEGATIVE_CACHE[1];
                                }
                                break;
                            case 24:
                                // sin;
                                arg1 = valueStack[--valueStackSize];
                                if ((arg1.type == 4))
                                {
                                    float1 = (double)arg1.internalValue;
                                }
                                else if ((arg1.type == 3))
                                {
                                    float1 = (0.0 + (int)arg1.internalValue);
                                }
                                else
                                {
                                    hasInterrupt = EX_InvalidArgument(ec, "sin requires a number argument.");
                                }
                                output = buildFloat(globals, System.Math.Sin(float1));
                                break;
                            case 25:
                                // tan;
                                arg1 = valueStack[--valueStackSize];
                                if ((arg1.type == 4))
                                {
                                    float1 = (double)arg1.internalValue;
                                }
                                else if ((arg1.type == 3))
                                {
                                    float1 = (0.0 + (int)arg1.internalValue);
                                }
                                else
                                {
                                    hasInterrupt = EX_InvalidArgument(ec, "tan requires a number argument.");
                                }
                                if (!hasInterrupt)
                                {
                                    float2 = System.Math.Cos(float1);
                                    if ((float2 < 0))
                                    {
                                        float2 = -float2;
                                    }
                                    if ((float2 < 0.00000000015))
                                    {
                                        hasInterrupt = EX_DivisionByZero(ec, "Tangent is undefined.");
                                    }
                                    else
                                    {
                                        output = buildFloat(globals, System.Math.Tan(float1));
                                    }
                                }
                                break;
                            case 26:
                                // log;
                                valueStackSize -= 2;
                                arg2 = valueStack[(valueStackSize + 1)];
                                arg1 = valueStack[valueStackSize];
                                if ((arg1.type == 4))
                                {
                                    float1 = (double)arg1.internalValue;
                                }
                                else if ((arg1.type == 3))
                                {
                                    float1 = (0.0 + (int)arg1.internalValue);
                                }
                                else
                                {
                                    hasInterrupt = EX_InvalidArgument(ec, "logarithms require a number argument.");
                                }
                                if (!hasInterrupt)
                                {
                                    if ((float1 <= 0))
                                    {
                                        hasInterrupt = EX_InvalidArgument(ec, "logarithms require positive inputs.");
                                    }
                                    else
                                    {
                                        output = buildFloat(globals, fixFuzzyFloatPrecision((System.Math.Log(float1) * (double)arg2.internalValue)));
                                    }
                                }
                                break;
                            case 27:
                                // intQueueClear;
                                arg1 = valueStack[--valueStackSize];
                                output = VALUE_NULL;
                                objInstance1 = (ObjectInstance)arg1.internalValue;
                                if ((objInstance1.nativeData != null))
                                {
                                    objInstance1.nativeData[1] = 0;
                                }
                                break;
                            case 28:
                                // intQueueWrite16;
                                output = VALUE_NULL;
                                int1 = row[2];
                                valueStackSize -= (int1 + 1);
                                value = valueStack[valueStackSize];
                                objArray1 = ((ObjectInstance)value.internalValue).nativeData;
                                intArray1 = (int[])objArray1[0];
                                _len = (int)objArray1[1];
                                if ((_len >= intArray1.Length))
                                {
                                    intArray2 = new int[((_len * 2) + 16)];
                                    j = 0;
                                    while ((j < _len))
                                    {
                                        intArray2[j] = intArray1[j];
                                        j += 1;
                                    }
                                    intArray1 = intArray2;
                                    objArray1[0] = intArray1;
                                }
                                objArray1[1] = (_len + 16);
                                i = (int1 - 1);
                                while ((i >= 0))
                                {
                                    value = valueStack[((valueStackSize + 1) + i)];
                                    if ((value.type == 3))
                                    {
                                        intArray1[(_len + i)] = (int)value.internalValue;
                                    }
                                    else if ((value.type == 4))
                                    {
                                        float1 = (0.5 + (double)value.internalValue);
                                        intArray1[(_len + i)] = (int)float1;
                                    }
                                    else
                                    {
                                        hasInterrupt = EX_InvalidArgument(ec, "Input must be integers.");
                                        i = -1;
                                    }
                                    i -= 1;
                                }
                                break;
                            case 29:
                                // execCounter;
                                output = buildInteger(globals, ec.executionCounter);
                                break;
                            case 30:
                                // sleep;
                                arg1 = valueStack[--valueStackSize];
                                float1 = getFloat(arg1);
                                if ((row[1] == 1))
                                {
                                    if ((valueStackSize == valueStackCapacity))
                                    {
                                        valueStack = valueStackIncreaseCapacity(ec);
                                        valueStackCapacity = valueStack.Length;
                                    }
                                    valueStack[valueStackSize] = VALUE_NULL;
                                    valueStackSize += 1;
                                }
                                prepareToSuspend(ec, stack, valueStackSize, pc);
                                ec.activeInterrupt = new Interrupt(3, 0, "", float1, null);
                                hasInterrupt = true;
                                break;
                            case 31:
                                // projectId;
                                output = buildCommonString(globals, metadata.projectId);
                                break;
                            case 32:
                                // isJavaScript;
                                output = VALUE_FALSE;
                                break;
                            case 33:
                                // isAndroid;
                                output = VALUE_FALSE;
                                break;
                            case 34:
                                // allocNativeData;
                                valueStackSize -= 2;
                                arg2 = valueStack[(valueStackSize + 1)];
                                arg1 = valueStack[valueStackSize];
                                objInstance1 = (ObjectInstance)arg1.internalValue;
                                int1 = (int)arg2.internalValue;
                                objArray1 = new object[int1];
                                objInstance1.nativeData = objArray1;
                                break;
                            case 35:
                                // setNativeData;
                                valueStackSize -= 3;
                                arg3 = valueStack[(valueStackSize + 2)];
                                arg2 = valueStack[(valueStackSize + 1)];
                                arg1 = valueStack[valueStackSize];
                                ((ObjectInstance)arg1.internalValue).nativeData[(int)arg2.internalValue] = arg3.internalValue;
                                break;
                            case 36:
                                // getExceptionTrace;
                                arg1 = valueStack[--valueStackSize];
                                intList1 = (List<int>)getNativeDataItem(arg1, 1);
                                list1 = makeEmptyList(globals.stringType, 20);
                                output = new Value(6, list1);
                                if ((intList1 != null))
                                {
                                    stringList1 = tokenHelperConvertPcsToStackTraceStrings(vm, intList1);
                                    i = 0;
                                    while ((i < stringList1.Count))
                                    {
                                        addToList(list1, buildString(globals, stringList1[i]));
                                        i += 1;
                                    }
                                    reverseList(list1);
                                }
                                break;
                            case 37:
                                // reflectAllClasses;
                                output = Reflect_allClasses(vm);
                                break;
                            case 38:
                                // reflectGetMethods;
                                arg1 = valueStack[--valueStackSize];
                                output = Reflect_getMethods(vm, ec, arg1);
                                hasInterrupt = (ec.activeInterrupt != null);
                                break;
                            case 39:
                                // reflectGetClass;
                                arg1 = valueStack[--valueStackSize];
                                if ((arg1.type != 8))
                                {
                                    hasInterrupt = EX_InvalidArgument(ec, "Cannot get class from non-instance types.");
                                }
                                else
                                {
                                    objInstance1 = (ObjectInstance)arg1.internalValue;
                                    output = new Value(10, new ClassValue(false, objInstance1.classId));
                                }
                                break;
                            case 40:
                                // convertFloatArgsToInts;
                                int1 = stack.localsStackOffsetEnd;
                                i = localsStackOffset;
                                while ((i < int1))
                                {
                                    value = localsStack[i];
                                    if ((localsStackSet[i] != localsStackSetToken))
                                    {
                                        i += int1;
                                    }
                                    else if ((value.type == 4))
                                    {
                                        float1 = (double)value.internalValue;
                                        if (((float1 < 0) && ((float1 % 1) != 0)))
                                        {
                                            int2 = ((int)float1 - 1);
                                        }
                                        else
                                        {
                                            int2 = (int)float1;
                                        }
                                        if (((int2 >= 0) && (int2 < 2049)))
                                        {
                                            localsStack[i] = INTEGER_POSITIVE_CACHE[int2];
                                        }
                                        else
                                        {
                                            localsStack[i] = buildInteger(globals, int2);
                                        }
                                    }
                                    i += 1;
                                }
                                break;
                            case 41:
                                // addShutdownHandler;
                                arg1 = valueStack[--valueStackSize];
                                vm.shutdownHandlers.Add(arg1);
                                break;
                            case 42:
                                // nativeTunnelSend;
                                valueStackSize -= 2;
                                arg2 = valueStack[(valueStackSize + 1)];
                                arg1 = valueStack[valueStackSize];
                                objArray1 = new object[2];
                                objArray1[0] = arg1.internalValue;
                                objArray1[1] = arg2.internalValue;
                                obj1 = (PST_ExtCallbacks.ContainsKey("nativeTunnelSend") ? PST_ExtCallbacks["nativeTunnelSend"].Invoke(objArray1) : null);
                                int1 = 0;
                                if ((obj1 != null))
                                {
                                    int1 = (int)obj1;
                                }
                                output = buildInteger(globals, int1);
                                break;
                            case 43:
                                // nativeTunnelRecv;
                                arg1 = valueStack[--valueStackSize];
                                list1 = (ListImpl)arg1.internalValue;
                                objArray1 = new object[4];
                                objArray1[3] = false;
                                obj1 = (PST_ExtCallbacks.ContainsKey("nativeTunnelRecv") ? PST_ExtCallbacks["nativeTunnelRecv"].Invoke(objArray1) : null);
                                bool1 = false;
                                if ((obj1 != null))
                                {
                                    bool1 = (bool)obj1;
                                }
                                output = buildBoolean(globals, bool1);
                                if (bool1)
                                {
                                    value = buildInteger(globals, (int)objArray1[0]);
                                    value2 = buildInteger(globals, (int)objArray1[1]);
                                    value3 = buildString(globals, (string)objArray1[2]);
                                    rightValue = buildBoolean(globals, (bool)objArray1[3]);
                                    list1.array[0] = value;
                                    list1.array[1] = value2;
                                    list1.array[2] = value3;
                                    list1.array[3] = rightValue;
                                }
                                break;
                            case 44:
                                // ipcNamedPipeIsSupported;
                                output = buildBoolean(globals, IpcNamedPipe_isSupported());
                                break;
                            case 45:
                                // ipcNamedPipeCreate;
                                valueStackSize -= 2;
                                arg2 = valueStack[(valueStackSize + 1)];
                                arg1 = valueStack[valueStackSize];
                                string1 = IpcNamedPipe_create(arg1, arg2);
                                if ((string1 == null))
                                {
                                    output = globals.valueNull;
                                }
                                else
                                {
                                    output = buildString(globals, string1);
                                }
                                break;
                            case 46:
                                // ipcNamedPipeSend;
                                valueStackSize -= 2;
                                arg2 = valueStack[(valueStackSize + 1)];
                                arg1 = valueStack[valueStackSize];
                                string1 = IpcNamedPipe_send(arg1, arg2);
                                if ((string1 == null))
                                {
                                    output = globals.valueNull;
                                }
                                else
                                {
                                    output = buildString(globals, string1);
                                }
                                break;
                            case 47:
                                // ipcNamedPipeFlush;
                                arg1 = valueStack[--valueStackSize];
                                string1 = IpcNamedPipe_flush(arg1);
                                if ((string1 == null))
                                {
                                    output = globals.valueNull;
                                }
                                else
                                {
                                    output = buildString(globals, string1);
                                }
                                break;
                            case 48:
                                // randomFloat;
                                output = new Value(4, PST_Random.NextDouble());
                                break;
                            case 49:
                                // randomInt;
                                valueStackSize -= 2;
                                arg2 = valueStack[(valueStackSize + 1)];
                                arg1 = valueStack[valueStackSize];
                                if (((arg1.type != 3) || (arg2.type != 3)))
                                {
                                    output = vm.globalNull;
                                }
                                else
                                {
                                    int1 = (int)arg1.internalValue;
                                    int2 = (int)arg2.internalValue;
                                    if ((int1 >= int2))
                                    {
                                        output = vm.globalNull;
                                    }
                                    else
                                    {
                                        int3 = (int)((PST_Random.NextDouble() * (int2 - int1)));
                                        output = buildInteger(vm.globals, (int1 + int3));
                                    }
                                }
                                break;
                            case 50:
                                // resourceGetManifest;
                                output = buildList(vm.resourceDatabase.dataList);
                                break;
                            case 51:
                                // resourceGetText;
                                arg1 = valueStack[--valueStackSize];
                                output = buildString(globals, Interpreter.ResourceReader.ReadTextResource((string)arg1.internalValue));
                                break;
                            case 52:
                                // environmentGetVariable;
                                arg1 = valueStack[--valueStackSize];
                                string1 = System.Environment.GetEnvironmentVariable((string)arg1.internalValue);
                                if ((string1 != null))
                                {
                                    output = buildString(globals, string1);
                                }
                                break;
                            case 53:
                                // srandomPopulateQueue;
                                valueStackSize -= 3;
                                arg3 = valueStack[(valueStackSize + 2)];
                                arg2 = valueStack[(valueStackSize + 1)];
                                arg1 = valueStack[valueStackSize];
                                output = buildInteger(globals, SRandomQueuePopulate(globals, (int)arg1.internalValue, (ListImpl)arg2.internalValue, (int)arg3.internalValue));
                                break;
                            case 54:
                                // dateTimeGetUtcOffsetAt;
                                valueStackSize -= 2;
                                arg2 = valueStack[(valueStackSize + 1)];
                                arg1 = valueStack[valueStackSize];
                                output = DateTime_getUtcOffsetAt(vm, arg1, arg2);
                                break;
                            case 55:
                                // dateTimeInitTimeZone;
                                valueStackSize -= 3;
                                arg3 = valueStack[(valueStackSize + 2)];
                                arg2 = valueStack[(valueStackSize + 1)];
                                arg1 = valueStack[valueStackSize];
                                output = DateTime_initTimeZone(vm, arg1, arg2, arg3);
                                break;
                            case 56:
                                // dateTimeInitTimeZoneList;
                                arg1 = valueStack[--valueStackSize];
                                output = DateTime_initTimeZoneList(vm, arg1);
                                break;
                            case 57:
                                // dateTimeIsDstOccurringAt;
                                valueStackSize -= 2;
                                arg2 = valueStack[(valueStackSize + 1)];
                                arg1 = valueStack[valueStackSize];
                                output = DateTime_isDstOccurringAt(vm, arg1, arg2);
                                break;
                            case 58:
                                // dateTimeParseDate;
                                valueStackSize -= 7;
                                output = DateTime_parseDate(vm, valueStack[valueStackSize], valueStack[(valueStackSize + 1)], valueStack[(valueStackSize + 2)], valueStack[(valueStackSize + 3)], valueStack[(valueStackSize + 4)], valueStack[(valueStackSize + 5)], valueStack[(valueStackSize + 6)]);
                                break;
                            case 59:
                                // dateTimeUnixToStructured;
                                valueStackSize -= 2;
                                arg2 = valueStack[(valueStackSize + 1)];
                                arg1 = valueStack[valueStackSize];
                                output = DateTime_unixToStructured(vm, arg1, arg2);
                                break;
                            case 60:
                                // ipcNamedPipeServerCreate;
                                valueStackSize -= 5;
                                arg5 = valueStack[(valueStackSize + 4)];
                                arg4 = valueStack[(valueStackSize + 3)];
                                arg3 = valueStack[(valueStackSize + 2)];
                                arg2 = valueStack[(valueStackSize + 1)];
                                arg1 = valueStack[valueStackSize];
                                string1 = IpcNamedPipeServer_create(arg1, arg2, arg3, arg4, arg5);
                                if ((string1 == null))
                                {
                                    output = globals.valueNull;
                                }
                                else
                                {
                                    output = buildString(globals, string1);
                                }
                                break;
                            case 61:
                                // ipcNamedPipeServerClose;
                                arg1 = valueStack[--valueStackSize];
                                string1 = IpcNamedPipeServer_close(arg1);
                                if ((string1 == null))
                                {
                                    output = globals.valueNull;
                                }
                                else
                                {
                                    output = buildString(globals, string1);
                                }
                                break;
                            case 62:
                                // jsInteropSupported;
                                output = buildBoolean(globals, false);
                                break;
                            case 63:
                                // jsInteropInvoke;
                                valueStackSize -= 3;
                                break;
                            case 64:
                                // jsInteropRegisterCallback;
                                valueStackSize -= 3;
                                break;
                            case 65:
                                // jsInteropCallbackReturn;
                                valueStackSize -= 2;
                                break;
                            case 66:
                                // imageCreate;
                                valueStackSize -= 3;
                                arg3 = valueStack[(valueStackSize + 2)];
                                arg2 = valueStack[(valueStackSize + 1)];
                                arg1 = valueStack[valueStackSize];
                                ImageHelper_ImageCreate((ObjectInstance)arg1.internalValue, (int)arg2.internalValue, (int)arg3.internalValue);
                                output = VALUE_NULL;
                                break;
                            case 67:
                                // imageGetPixel;
                                valueStackSize -= 5;
                                arg5 = valueStack[(valueStackSize + 4)];
                                arg4 = valueStack[(valueStackSize + 3)];
                                arg3 = valueStack[(valueStackSize + 2)];
                                arg2 = valueStack[(valueStackSize + 1)];
                                arg1 = valueStack[valueStackSize];
                                objInstance1 = null;
                                if ((arg2.type != 1))
                                {
                                    objInstance1 = (ObjectInstance)arg2.internalValue;
                                }
                                int1 = ImageHelper_GetPixel(globals.positiveIntegers, (ObjectInstance)arg1.internalValue, objInstance1, arg3, arg4, (ListImpl)arg5.internalValue, intBuffer);
                                output = globals.positiveIntegers[int1];
                                break;
                            case 68:
                                // imageSetPixel;
                                valueStackSize -= 9;
                                arg9 = valueStack[(valueStackSize + 8)];
                                arg8 = valueStack[(valueStackSize + 7)];
                                arg7 = valueStack[(valueStackSize + 6)];
                                arg6 = valueStack[(valueStackSize + 5)];
                                arg5 = valueStack[(valueStackSize + 4)];
                                arg4 = valueStack[(valueStackSize + 3)];
                                arg3 = valueStack[(valueStackSize + 2)];
                                arg2 = valueStack[(valueStackSize + 1)];
                                arg1 = valueStack[valueStackSize];
                                int1 = ImageHelper_SetPixel((ObjectInstance)arg1.internalValue, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9);
                                output = globals.positiveIntegers[int1];
                                break;
                            case 69:
                                // imageScale;
                                valueStackSize -= 5;
                                arg5 = valueStack[(valueStackSize + 4)];
                                arg4 = valueStack[(valueStackSize + 3)];
                                arg3 = valueStack[(valueStackSize + 2)];
                                arg2 = valueStack[(valueStackSize + 1)];
                                arg1 = valueStack[valueStackSize];
                                ImageHelper_Scale((ObjectInstance)arg1.internalValue, (ObjectInstance)arg2.internalValue, (int)arg3.internalValue, (int)arg4.internalValue, (int)arg5.internalValue);
                                output = VALUE_NULL;
                                break;
                            case 70:
                                // imageSessionStart;
                                valueStackSize -= 2;
                                arg2 = valueStack[(valueStackSize + 1)];
                                arg1 = valueStack[valueStackSize];
                                ImageHelper_SessionStart((ObjectInstance)arg1.internalValue, (ObjectInstance)arg2.internalValue);
                                output = VALUE_NULL;
                                break;
                            case 71:
                                // imageSessionFinish;
                                valueStackSize -= 2;
                                arg2 = valueStack[(valueStackSize + 1)];
                                arg1 = valueStack[valueStackSize];
                                ImageHelper_SessionFinish((ObjectInstance)arg1.internalValue, (ObjectInstance)arg2.internalValue);
                                output = VALUE_NULL;
                                break;
                            case 72:
                                // imageBlit;
                                valueStackSize -= 10;
                                arg10 = valueStack[(valueStackSize + 9)];
                                arg9 = valueStack[(valueStackSize + 8)];
                                arg8 = valueStack[(valueStackSize + 7)];
                                arg7 = valueStack[(valueStackSize + 6)];
                                arg6 = valueStack[(valueStackSize + 5)];
                                arg5 = valueStack[(valueStackSize + 4)];
                                arg4 = valueStack[(valueStackSize + 3)];
                                arg3 = valueStack[(valueStackSize + 2)];
                                arg2 = valueStack[(valueStackSize + 1)];
                                arg1 = valueStack[valueStackSize];
                                ImageHelper_ImageBlit((ObjectInstance)arg1.internalValue, (ObjectInstance)arg2.internalValue, (int)arg3.internalValue, (int)arg4.internalValue, (int)arg5.internalValue, (int)arg6.internalValue, (int)arg7.internalValue, (int)arg8.internalValue, (int)arg9.internalValue, (int)arg10.internalValue);
                                output = VALUE_NULL;
                                break;
                            case 73:
                                // imageAtlasManifest;
                                output = buildString(globals, vm.resourceDatabase.imageAtlasManifest);
                                break;
                            case 74:
                                // imageLoadChunk;
                                valueStackSize -= 3;
                                arg3 = valueStack[(valueStackSize + 2)];
                                arg2 = valueStack[(valueStackSize + 1)];
                                arg1 = valueStack[valueStackSize];
                                ImageHelper_LoadChunk((int)arg1.internalValue, (ListImpl)arg2.internalValue, arg3);
                                output = VALUE_NULL;
                                break;
                            case 75:
                                // imageGetChunkSync;
                                valueStackSize -= 2;
                                arg2 = valueStack[(valueStackSize + 1)];
                                arg1 = valueStack[valueStackSize];
                                ImageHelper_GetChunkSync((ObjectInstance)arg1.internalValue, (int)arg2.internalValue);
                                output = VALUE_NULL;
                                break;
                            case 76:
                                // makeByteList;
                                valueStackSize -= 2;
                                arg2 = valueStack[(valueStackSize + 1)];
                                arg1 = valueStack[valueStackSize];
                                output = buildBoolean(globals, makeByteListNativeData((ObjectInstance)arg1.internalValue, arg2));
                                break;
                            case 77:
                                // bytesObjToList;
                                valueStackSize -= 2;
                                arg2 = valueStack[(valueStackSize + 1)];
                                arg1 = valueStack[valueStackSize];
                                byteObjToList(globals.positiveIntegers, (ObjectInstance)arg1.internalValue, (ListImpl)arg2.internalValue);
                                output = VALUE_NULL;
                                break;
                            case 78:
                                // httpSend;
                                valueStackSize -= 9;
                                arg9 = valueStack[(valueStackSize + 8)];
                                arg8 = valueStack[(valueStackSize + 7)];
                                arg7 = valueStack[(valueStackSize + 6)];
                                arg6 = valueStack[(valueStackSize + 5)];
                                arg5 = valueStack[(valueStackSize + 4)];
                                arg4 = valueStack[(valueStackSize + 3)];
                                arg3 = valueStack[(valueStackSize + 2)];
                                arg2 = valueStack[(valueStackSize + 1)];
                                arg1 = valueStack[valueStackSize];
                                intArray1 = null;
                                string1 = null;
                                int1 = (int)arg5.internalValue;
                                if ((int1 == 1))
                                {
                                    string1 = (string)arg7.internalValue;
                                }
                                else if ((int1 == 2))
                                {
                                    intArray1 = (int[])((ObjectInstance)arg7.internalValue).nativeData[0];
                                }
                                list1 = (ListImpl)arg8.internalValue;
                                stringList = new string[list1.size];
                                i = 0;
                                while ((i < list1.size))
                                {
                                    stringList[i] = (string)list1.array[i].internalValue;
                                    i += 1;
                                }
                                objInstance1 = (ObjectInstance)arg9.internalValue;
                                objInstance1.nativeData = new object[1];
                                CoreFunctions.HttpSend(arg1, arg2, (string)arg3.internalValue, (string)arg4.internalValue, (string)arg6.internalValue, intArray1, string1, stringList, arg9, objInstance1.nativeData);
                                output = VALUE_NULL;
                                break;
                            case 79:
                                // imageFromBytes;
                                valueStackSize -= 4;
                                arg4 = valueStack[(valueStackSize + 3)];
                                arg3 = valueStack[(valueStackSize + 2)];
                                arg2 = valueStack[(valueStackSize + 1)];
                                arg1 = valueStack[valueStackSize];
                                output = buildInteger(globals, ImageHelper_fromBytes(globals, (ObjectInstance)arg1.internalValue, false, arg2, (ListImpl)arg3.internalValue, arg4));
                                break;
                            case 80:
                                // imageB64BytesPreferred;
                                output = buildBoolean(globals, false);
                                break;
                            case 81:
                                // imageEncode;
                                valueStackSize -= 2;
                                arg2 = valueStack[(valueStackSize + 1)];
                                arg1 = valueStack[valueStackSize];
                                value = ImageHelper_ImageEncode(globals, ((ObjectInstance)arg1.internalValue).nativeData[0], (int)arg2.internalValue);
                                valueList1 = new List<Value>();
                                valueList1.Add(buildBoolean(globals, (value.type == 5)));
                                valueList1.Add(value);
                                output = buildList(valueList1);
                                break;
                            case 82:
                                // argVerifyIntRange;
                                _len = (int)valueStack[(valueStackSize - 1)].internalValue;
                                int1 = (valueStackSize - _len - 1);
                                int2 = (int)valueStack[(int1 - 2)].internalValue;
                                int3 = (int)valueStack[(int1 - 1)].internalValue;
                                valueStackSize = (int1 - 3);
                                bool1 = false;
                                i = 0;
                                while ((i < _len))
                                {
                                    value = valueStack[(int1 + i)];
                                    if ((value.type != 3))
                                    {
                                        bool1 = true;
                                    }
                                    else
                                    {
                                        j = (int)value.internalValue;
                                        if (((j < int2) || (j > int3)))
                                        {
                                            bool1 = true;
                                        }
                                    }
                                    i += 1;
                                }
                                if (bool1)
                                {
                                    hasInterrupt = EX_InvalidArgument(ec, (string)valueStack[valueStackSize].internalValue);
                                }
                                break;
                            case 83:
                                // argVerifyNums;
                                arg1 = valueStack[--valueStackSize];
                                _len = (int)arg1.internalValue;
                                int1 = (valueStackSize - _len);
                                valueStackSize = (int1 - 1);
                                i = 0;
                                while ((i < _len))
                                {
                                    value = valueStack[(int1 + i)];
                                    if (((value.type != 3) && (value.type != 4)))
                                    {
                                        i += _len;
                                        hasInterrupt = EX_InvalidArgument(ec, (string)valueStack[valueStackSize].internalValue);
                                    }
                                    i += 1;
                                }
                                break;
                            case 84:
                                // xmlParse;
                                arg1 = valueStack[--valueStackSize];
                                output = xml_parse(vm, (string)arg1.internalValue);
                                break;
                            case 85:
                                // launchBrowser;
                                arg1 = valueStack[--valueStackSize];
                                System.Diagnostics.Process.Start((string)arg1.internalValue);
                                output = vm.globalNull;
                                break;
                            case 86:
                                // cryptoDigest;
                                valueStackSize -= 2;
                                arg2 = valueStack[(valueStackSize + 1)];
                                arg1 = valueStack[valueStackSize];
                                output = crypto_digest(globals, (ListImpl)arg1.internalValue, (int)arg2.internalValue);
                                break;
                        }
                        if ((row[1] == 1))
                        {
                            if ((valueStackSize == valueStackCapacity))
                            {
                                valueStack = valueStackIncreaseCapacity(ec);
                                valueStackCapacity = valueStack.Length;
                            }
                            valueStack[valueStackSize] = output;
                            valueStackSize += 1;
                        }
                        break;
                    case 20:
                        // DEBUG_SYMBOLS;
                        applyDebugSymbolData(vm, row, stringArgs[pc], metadata.mostRecentFunctionDef);
                        break;
                    case 21:
                        // DEF_DICT;
                        intIntDict1 = new Dictionary<int, int>();
                        stringIntDict1 = new Dictionary<string, int>();
                        valueList2 = new List<Value>();
                        valueList1 = new List<Value>();
                        _len = row[0];
                        type = 3;
                        first = true;
                        i = _len;
                        while ((i > 0))
                        {
                            valueStackSize -= 2;
                            value = valueStack[(valueStackSize + 1)];
                            value2 = valueStack[valueStackSize];
                            if (first)
                            {
                                type = value2.type;
                                first = false;
                            }
                            else if ((type != value2.type))
                            {
                                hasInterrupt = EX_InvalidKey(ec, "Dictionary keys must be of the same type.");
                            }
                            if (!hasInterrupt)
                            {
                                if ((type == 3))
                                {
                                    intKey = (int)value2.internalValue;
                                }
                                else if ((type == 5))
                                {
                                    stringKey = (string)value2.internalValue;
                                }
                                else if ((type == 8))
                                {
                                    objInstance1 = (ObjectInstance)value2.internalValue;
                                    intKey = objInstance1.objectId;
                                }
                                else
                                {
                                    hasInterrupt = EX_InvalidKey(ec, "Only integers, strings, and objects can be used as dictionary keys.");
                                }
                            }
                            if (!hasInterrupt)
                            {
                                if ((type == 5))
                                {
                                    stringIntDict1[stringKey] = valueList1.Count;
                                }
                                else
                                {
                                    intIntDict1[intKey] = valueList1.Count;
                                }
                                valueList2.Add(value2);
                                valueList1.Add(value);
                                i -= 1;
                            }
                        }
                        if (!hasInterrupt)
                        {
                            if ((type == 5))
                            {
                                i = stringIntDict1.Count;
                            }
                            else
                            {
                                i = intIntDict1.Count;
                            }
                            if ((i != _len))
                            {
                                hasInterrupt = EX_InvalidKey(ec, "Key collision");
                            }
                        }
                        if (!hasInterrupt)
                        {
                            i = row[1];
                            classId = 0;
                            if ((i > 0))
                            {
                                type = row[2];
                                if ((type == 8))
                                {
                                    classId = row[3];
                                }
                                int1 = row.Length;
                                intArray1 = new int[(int1 - i)];
                                while ((i < int1))
                                {
                                    intArray1[(i - row[1])] = row[i];
                                    i += 1;
                                }
                            }
                            else
                            {
                                intArray1 = null;
                            }
                            if ((valueStackSize == valueStackCapacity))
                            {
                                valueStack = valueStackIncreaseCapacity(ec);
                                valueStackCapacity = valueStack.Length;
                            }
                            valueStack[valueStackSize] = new Value(7, new DictImpl(_len, type, classId, intArray1, intIntDict1, stringIntDict1, valueList2, valueList1));
                            valueStackSize += 1;
                        }
                        break;
                    case 22:
                        // DEF_LIST;
                        int1 = row[0];
                        list1 = makeEmptyList(null, int1);
                        if ((row[1] != 0))
                        {
                            list1.type = new int[(row.Length - 1)];
                            i = 1;
                            while ((i < row.Length))
                            {
                                list1.type[(i - 1)] = row[i];
                                i += 1;
                            }
                        }
                        list1.size = int1;
                        int2 = (valueStackSize - int1);
                        i = 0;
                        while ((i < int1))
                        {
                            list1.array[i] = valueStack[(int2 + i)];
                            i += 1;
                        }
                        valueStackSize -= int1;
                        value = new Value(6, list1);
                        if ((valueStackSize == valueStackCapacity))
                        {
                            valueStack = valueStackIncreaseCapacity(ec);
                            valueStackCapacity = valueStack.Length;
                        }
                        valueStack[valueStackSize] = value;
                        valueStackSize += 1;
                        break;
                    case 23:
                        // DEF_ORIGINAL_CODE;
                        defOriginalCodeImpl(vm, row, stringArgs[pc]);
                        break;
                    case 24:
                        // DEREF_CLOSURE;
                        bool1 = true;
                        closure = stack.closureVariables;
                        i = row[0];
                        if (((closure != null) && closure.ContainsKey(i)))
                        {
                            value = closure[i].value;
                            if ((value != null))
                            {
                                bool1 = false;
                                if ((valueStackSize == valueStackCapacity))
                                {
                                    valueStack = valueStackIncreaseCapacity(ec);
                                    valueStackCapacity = valueStack.Length;
                                }
                                valueStack[valueStackSize++] = value;
                            }
                        }
                        if (bool1)
                        {
                            hasInterrupt = EX_UnassignedVariable(ec, "Variable used before it was set.");
                        }
                        break;
                    case 25:
                        // DEREF_DOT;
                        value = valueStack[(valueStackSize - 1)];
                        nameId = row[0];
                        int2 = row[1];
                        switch (value.type)
                        {
                            case 8:
                                objInstance1 = (ObjectInstance)value.internalValue;
                                classId = objInstance1.classId;
                                classInfo = classTable[classId];
                                if ((classId == row[4]))
                                {
                                    int1 = row[5];
                                }
                                else
                                {
                                    intIntDict1 = classInfo.localeScopedNameIdToMemberId;
                                    if (!intIntDict1.TryGetValue(int2, out int1)) int1 = -1;
                                    if ((int1 != -1))
                                    {
                                        int3 = classInfo.fieldAccessModifiers[int1];
                                        if ((int3 > 1))
                                        {
                                            if ((int3 == 2))
                                            {
                                                if ((classId != row[2]))
                                                {
                                                    int1 = -2;
                                                }
                                            }
                                            else
                                            {
                                                if (((int3 == 3) || (int3 == 5)))
                                                {
                                                    if ((classInfo.assemblyId != row[3]))
                                                    {
                                                        int1 = -3;
                                                    }
                                                }
                                                if (((int3 == 4) || (int3 == 5)))
                                                {
                                                    i = row[2];
                                                    if ((classId == i))
                                                    {
                                                    }
                                                    else
                                                    {
                                                        classInfo = classTable[classInfo.id];
                                                        while (((classInfo.baseClassId != -1) && (int1 < classTable[classInfo.baseClassId].fieldAccessModifiers.Length)))
                                                        {
                                                            classInfo = classTable[classInfo.baseClassId];
                                                        }
                                                        j = classInfo.id;
                                                        if ((j != i))
                                                        {
                                                            bool1 = false;
                                                            while (((i != -1) && (classTable[i].baseClassId != -1)))
                                                            {
                                                                i = classTable[i].baseClassId;
                                                                if ((i == j))
                                                                {
                                                                    bool1 = true;
                                                                    i = -1;
                                                                }
                                                            }
                                                            if (!bool1)
                                                            {
                                                                int1 = -4;
                                                            }
                                                        }
                                                    }
                                                    classInfo = classTable[classId];
                                                }
                                            }
                                        }
                                        row[4] = objInstance1.classId;
                                        row[5] = int1;
                                    }
                                }
                                if ((int1 > -1))
                                {
                                    functionId = classInfo.functionIds[int1];
                                    if ((functionId == -1))
                                    {
                                        output = objInstance1.members[int1];
                                    }
                                    else
                                    {
                                        output = new Value(9, new FunctionPointer(2, value, objInstance1.classId, functionId, null));
                                    }
                                }
                                else
                                {
                                    output = null;
                                }
                                break;
                            case 5:
                                if ((metadata.lengthId == nameId))
                                {
                                    output = buildInteger(globals, ((string)value.internalValue).Length);
                                }
                                else
                                {
                                    output = null;
                                }
                                break;
                            case 6:
                                if ((metadata.lengthId == nameId))
                                {
                                    output = buildInteger(globals, ((ListImpl)value.internalValue).size);
                                }
                                else
                                {
                                    output = null;
                                }
                                break;
                            case 7:
                                if ((metadata.lengthId == nameId))
                                {
                                    output = buildInteger(globals, ((DictImpl)value.internalValue).size);
                                }
                                else
                                {
                                    output = null;
                                }
                                break;
                            default:
                                if ((value.type == 1))
                                {
                                    hasInterrupt = EX_NullReference(ec, "Derferenced a field from null.");
                                    output = VALUE_NULL;
                                }
                                else
                                {
                                    output = null;
                                }
                                break;
                        }
                        if ((output == null))
                        {
                            output = generatePrimitiveMethodReference(globalNameIdToPrimitiveMethodName, nameId, value);
                            if ((output == null))
                            {
                                if ((value.type == 1))
                                {
                                    hasInterrupt = EX_NullReference(ec, "Tried to dereference a field on null.");
                                }
                                else if (((value.type == 8) && (int1 < -1)))
                                {
                                    string1 = identifiers[row[0]];
                                    if ((int1 == -2))
                                    {
                                        string2 = "private";
                                    }
                                    else if ((int1 == -3))
                                    {
                                        string2 = "internal";
                                    }
                                    else
                                    {
                                        string2 = "protected";
                                    }
                                    hasInterrupt = EX_UnknownField(ec, string.Join("", new string[] { "The field '",string1,"' is marked as ",string2," and cannot be accessed from here." }));
                                }
                                else
                                {
                                    if ((value.type == 8))
                                    {
                                        classId = ((ObjectInstance)value.internalValue).classId;
                                        classInfo = classTable[classId];
                                        string1 = classInfo.fullyQualifiedName + " instance";
                                    }
                                    else
                                    {
                                        string1 = getTypeFromId(value.type);
                                    }
                                    hasInterrupt = EX_UnknownField(ec, string.Join("", new string[] { string1," does not have a field called '",vm.metadata.identifiers[row[0]],"'." }));
                                }
                            }
                        }
                        valueStack[(valueStackSize - 1)] = output;
                        break;
                    case 26:
                        // DEREF_INSTANCE_FIELD;
                        objInstance1 = (ObjectInstance)stack.objectContext.internalValue;
                        value = objInstance1.members[row[0]];
                        if ((valueStackSize == valueStackCapacity))
                        {
                            valueStack = valueStackIncreaseCapacity(ec);
                            valueStackCapacity = valueStack.Length;
                        }
                        valueStack[valueStackSize++] = value;
                        break;
                    case 27:
                        // DEREF_STATIC_FIELD;
                        classInfo = classTable[row[0]];
                        staticConstructorNotInvoked = true;
                        if ((classInfo.staticInitializationState < 2))
                        {
                            stack.pc = pc;
                            stackFrame2 = maybeInvokeStaticConstructor(vm, ec, stack, classInfo, valueStackSize, PST_IntBuffer16);
                            if ((PST_IntBuffer16[0] == 1))
                            {
                                return generateException(vm, stack, pc, valueStackSize, ec, 0, "Static initialization loop detected. The class this field is a member of is not done being initialized.");
                            }
                            if ((stackFrame2 != null))
                            {
                                staticConstructorNotInvoked = false;
                                stack = stackFrame2;
                                pc = stack.pc;
                                localsStackSetToken = stack.localsStackSetToken;
                                localsStackOffset = stack.localsStackOffset;
                            }
                        }
                        if (staticConstructorNotInvoked)
                        {
                            if ((valueStackSize == valueStackCapacity))
                            {
                                valueStack = valueStackIncreaseCapacity(ec);
                                valueStackCapacity = valueStack.Length;
                            }
                            valueStack[valueStackSize++] = classInfo.staticFields[row[1]];
                        }
                        break;
                    case 28:
                        // DUPLICATE_STACK_TOP;
                        if ((row[0] == 1))
                        {
                            value = valueStack[(valueStackSize - 1)];
                            if ((valueStackSize == valueStackCapacity))
                            {
                                valueStack = valueStackIncreaseCapacity(ec);
                                valueStackCapacity = valueStack.Length;
                            }
                            valueStack[valueStackSize++] = value;
                        }
                        else if ((row[0] == 2))
                        {
                            if (((valueStackSize + 1) > valueStackCapacity))
                            {
                                valueStackIncreaseCapacity(ec);
                                valueStack = ec.valueStack;
                                valueStackCapacity = valueStack.Length;
                            }
                            valueStack[valueStackSize] = valueStack[(valueStackSize - 2)];
                            valueStack[(valueStackSize + 1)] = valueStack[(valueStackSize - 1)];
                            valueStackSize += 2;
                        }
                        else
                        {
                            hasInterrupt = EX_Fatal(ec, "?");
                        }
                        break;
                    case 29:
                        // EQUALS;
                        valueStackSize -= 2;
                        rightValue = valueStack[(valueStackSize + 1)];
                        leftValue = valueStack[valueStackSize];
                        if ((leftValue.type == rightValue.type))
                        {
                            switch (leftValue.type)
                            {
                                case 1:
                                    bool1 = true;
                                    break;
                                case 2:
                                    bool1 = ((bool)leftValue.internalValue == (bool)rightValue.internalValue);
                                    break;
                                case 3:
                                    bool1 = ((int)leftValue.internalValue == (int)rightValue.internalValue);
                                    break;
                                case 5:
                                    bool1 = ((string)leftValue.internalValue == (string)rightValue.internalValue);
                                    break;
                                default:
                                    bool1 = (doEqualityComparisonAndReturnCode(leftValue, rightValue) == 1);
                                    break;
                            }
                        }
                        else
                        {
                            int1 = doEqualityComparisonAndReturnCode(leftValue, rightValue);
                            if ((int1 == 0))
                            {
                                bool1 = false;
                            }
                            else if ((int1 == 1))
                            {
                                bool1 = true;
                            }
                            else
                            {
                                hasInterrupt = EX_UnsupportedOperation(ec, "== and != not defined here.");
                            }
                        }
                        if ((valueStackSize == valueStackCapacity))
                        {
                            valueStack = valueStackIncreaseCapacity(ec);
                            valueStackCapacity = valueStack.Length;
                        }
                        if ((bool1 != ((row[0] == 1))))
                        {
                            valueStack[valueStackSize] = VALUE_TRUE;
                        }
                        else
                        {
                            valueStack[valueStackSize] = VALUE_FALSE;
                        }
                        valueStackSize += 1;
                        break;
                    case 30:
                        // ESF_LOOKUP;
                        esfData = generateEsfData(args.Length, row);
                        metadata.esfData = esfData;
                        break;
                    case 31:
                        // EXCEPTION_HANDLED_TOGGLE;
                        ec.activeExceptionHandled = (row[0] == 1);
                        break;
                    case 32:
                        // FIELD_TYPE_INFO;
                        initializeClassFieldTypeInfo(vm, row);
                        break;
                    case 33:
                        // FINALIZE_INITIALIZATION;
                        finalizeInitializationImpl(vm, stringArgs[pc], row[0]);
                        identifiers = vm.metadata.identifiers;
                        literalTable = vm.metadata.literalTable;
                        globalNameIdToPrimitiveMethodName = vm.metadata.globalNameIdToPrimitiveMethodName;
                        funcArgs = vm.funcArgs;
                        break;
                    case 34:
                        // FINALLY_END;
                        value = ec.activeException;
                        if (((value == null) || ec.activeExceptionHandled))
                        {
                            switch (stack.postFinallyBehavior)
                            {
                                case 0:
                                    ec.activeException = null;
                                    break;
                                case 1:
                                    ec.activeException = null;
                                    int1 = row[0];
                                    if ((int1 == 1))
                                    {
                                        pc += row[1];
                                    }
                                    else if ((int1 == 2))
                                    {
                                        intArray1 = esfData[pc];
                                        pc = intArray1[1];
                                    }
                                    else
                                    {
                                        hasInterrupt = EX_Fatal(ec, "break exists without a loop");
                                    }
                                    break;
                                case 2:
                                    ec.activeException = null;
                                    int1 = row[2];
                                    if ((int1 == 1))
                                    {
                                        pc += row[3];
                                    }
                                    else if ((int1 == 2))
                                    {
                                        intArray1 = esfData[pc];
                                        pc = intArray1[1];
                                    }
                                    else
                                    {
                                        hasInterrupt = EX_Fatal(ec, "continue exists without a loop");
                                    }
                                    break;
                                case 3:
                                    if ((stack.markClassAsInitialized != 0))
                                    {
                                        markClassAsInitialized(vm, stack, stack.markClassAsInitialized);
                                    }
                                    if (stack.returnValueUsed)
                                    {
                                        valueStackSize = stack.valueStackPopSize;
                                        value = stack.returnValueTempStorage;
                                        stack = stack.previous;
                                        if ((valueStackSize == valueStackCapacity))
                                        {
                                            valueStack = valueStackIncreaseCapacity(ec);
                                            valueStackCapacity = valueStack.Length;
                                        }
                                        valueStack[valueStackSize] = value;
                                        valueStackSize += 1;
                                    }
                                    else
                                    {
                                        valueStackSize = stack.valueStackPopSize;
                                        stack = stack.previous;
                                    }
                                    pc = stack.pc;
                                    localsStackOffset = stack.localsStackOffset;
                                    localsStackSetToken = stack.localsStackSetToken;
                                    break;
                            }
                        }
                        else
                        {
                            ec.activeExceptionHandled = false;
                            stack.pc = pc;
                            intArray1 = esfData[pc];
                            value = ec.activeException;
                            objInstance1 = (ObjectInstance)value.internalValue;
                            objArray1 = objInstance1.nativeData;
                            bool1 = true;
                            if ((objArray1[0] != null))
                            {
                                bool1 = (bool)objArray1[0];
                            }
                            intList1 = (List<int>)objArray1[1];
                            while (((stack != null) && ((intArray1 == null) || bool1)))
                            {
                                stack = stack.previous;
                                if ((stack != null))
                                {
                                    pc = stack.pc;
                                    intList1.Add(pc);
                                    intArray1 = esfData[pc];
                                }
                            }
                            if ((stack == null))
                            {
                                return uncaughtExceptionResult(vm, value);
                            }
                            int1 = intArray1[0];
                            if ((int1 < pc))
                            {
                                int1 = intArray1[1];
                            }
                            pc = (int1 - 1);
                            stack.pc = pc;
                            localsStackOffset = stack.localsStackOffset;
                            localsStackSetToken = stack.localsStackSetToken;
                            ec.stackTop = stack;
                            stack.postFinallyBehavior = 0;
                            ec.currentValueStackSize = valueStackSize;
                        }
                        break;
                    case 35:
                        // FUNCTION_DEFINITION;
                        initializeFunction(vm, row, pc, stringArgs[pc]);
                        pc += row[7];
                        functionTable = metadata.functionTable;
                        break;
                    case 36:
                        // INDEX;
                        value = valueStack[--valueStackSize];
                        root = valueStack[(valueStackSize - 1)];
                        if ((root.type == 6))
                        {
                            if ((value.type != 3))
                            {
                                hasInterrupt = EX_InvalidArgument(ec, "List index must be an integer.");
                            }
                            else
                            {
                                i = (int)value.internalValue;
                                list1 = (ListImpl)root.internalValue;
                                if ((i < 0))
                                {
                                    i += list1.size;
                                }
                                if (((i < 0) || (i >= list1.size)))
                                {
                                    hasInterrupt = EX_IndexOutOfRange(ec, "List index is out of bounds");
                                }
                                else
                                {
                                    valueStack[(valueStackSize - 1)] = list1.array[i];
                                }
                            }
                        }
                        else if ((root.type == 7))
                        {
                            dictImpl = (DictImpl)root.internalValue;
                            keyType = value.type;
                            if ((keyType != dictImpl.keyType))
                            {
                                if ((dictImpl.size == 0))
                                {
                                    hasInterrupt = EX_KeyNotFound(ec, "Key not found. Dictionary is empty.");
                                }
                                else
                                {
                                    hasInterrupt = EX_InvalidKey(ec, string.Join("", new string[] { "Incorrect key type. This dictionary contains ",getTypeFromId(dictImpl.keyType)," keys. Provided key is a ",getTypeFromId(keyType),"." }));
                                }
                            }
                            else
                            {
                                if ((keyType == 3))
                                {
                                    intKey = (int)value.internalValue;
                                }
                                else if ((keyType == 5))
                                {
                                    stringKey = (string)value.internalValue;
                                }
                                else if ((keyType == 8))
                                {
                                    intKey = ((ObjectInstance)value.internalValue).objectId;
                                }
                                else if ((dictImpl.size == 0))
                                {
                                    hasInterrupt = EX_KeyNotFound(ec, "Key not found. Dictionary is empty.");
                                }
                                else
                                {
                                    hasInterrupt = EX_KeyNotFound(ec, "Key not found.");
                                }
                                if (!hasInterrupt)
                                {
                                    if ((keyType == 5))
                                    {
                                        stringIntDict1 = (Dictionary<string, int>)dictImpl.stringToIndex;
                                        if (!stringIntDict1.TryGetValue(stringKey, out int1)) int1 = -1;
                                        if ((int1 == -1))
                                        {
                                            hasInterrupt = EX_KeyNotFound(ec, string.Join("", new string[] { "Key not found: '",stringKey,"'" }));
                                        }
                                        else
                                        {
                                            valueStack[(valueStackSize - 1)] = dictImpl.values[int1];
                                        }
                                    }
                                    else
                                    {
                                        intIntDict1 = (Dictionary<int, int>)dictImpl.intToIndex;
                                        if (!intIntDict1.TryGetValue(intKey, out int1)) int1 = -1;
                                        if ((int1 == -1))
                                        {
                                            hasInterrupt = EX_KeyNotFound(ec, "Key not found.");
                                        }
                                        else
                                        {
                                            valueStack[(valueStackSize - 1)] = dictImpl.values[intIntDict1[intKey]];
                                        }
                                    }
                                }
                            }
                        }
                        else if ((root.type == 5))
                        {
                            string1 = (string)root.internalValue;
                            if ((value.type != 3))
                            {
                                hasInterrupt = EX_InvalidArgument(ec, "String indices must be integers.");
                            }
                            else
                            {
                                int1 = (int)value.internalValue;
                                if ((int1 < 0))
                                {
                                    int1 += string1.Length;
                                }
                                if (((int1 < 0) || (int1 >= string1.Length)))
                                {
                                    hasInterrupt = EX_IndexOutOfRange(ec, "String index out of range.");
                                }
                                else
                                {
                                    valueStack[(valueStackSize - 1)] = buildCommonString(globals, string1[int1].ToString());
                                }
                            }
                        }
                        else
                        {
                            hasInterrupt = EX_InvalidArgument(ec, "Cannot index into this type: " + getTypeFromId(root.type));
                        }
                        break;
                    case 37:
                        // IS_COMPARISON;
                        value = valueStack[(valueStackSize - 1)];
                        output = VALUE_FALSE;
                        if ((value.type == 8))
                        {
                            objInstance1 = (ObjectInstance)value.internalValue;
                            if (isClassASubclassOf(vm, objInstance1.classId, row[0]))
                            {
                                output = VALUE_TRUE;
                            }
                        }
                        valueStack[(valueStackSize - 1)] = output;
                        break;
                    case 38:
                        // ITERATION_STEP;
                        int1 = (localsStackOffset + row[2]);
                        value3 = localsStack[int1];
                        i = (int)value3.internalValue;
                        value = localsStack[(localsStackOffset + row[3])];
                        if ((value.type == 6))
                        {
                            list1 = (ListImpl)value.internalValue;
                            _len = list1.size;
                            bool1 = true;
                        }
                        else
                        {
                            string2 = (string)value.internalValue;
                            _len = string2.Length;
                            bool1 = false;
                        }
                        if ((i < _len))
                        {
                            if (bool1)
                            {
                                value = list1.array[i];
                            }
                            else
                            {
                                value = buildCommonString(globals, string2[i].ToString());
                            }
                            int3 = (localsStackOffset + row[1]);
                            localsStackSet[int3] = localsStackSetToken;
                            localsStack[int3] = value;
                        }
                        else
                        {
                            pc += row[0];
                        }
                        i += 1;
                        if ((i < 2049))
                        {
                            localsStack[int1] = INTEGER_POSITIVE_CACHE[i];
                        }
                        else
                        {
                            localsStack[int1] = new Value(3, i);
                        }
                        break;
                    case 39:
                        // JUMP;
                        pc += row[0];
                        break;
                    case 40:
                        // JUMP_IF_EXCEPTION_OF_TYPE;
                        value = ec.activeException;
                        objInstance1 = (ObjectInstance)value.internalValue;
                        int1 = objInstance1.classId;
                        i = (row.Length - 1);
                        while ((i >= 2))
                        {
                            if (isClassASubclassOf(vm, int1, row[i]))
                            {
                                i = 0;
                                pc += row[0];
                                int2 = row[1];
                                if ((int2 > -1))
                                {
                                    int1 = (localsStackOffset + int2);
                                    localsStack[int1] = value;
                                    localsStackSet[int1] = localsStackSetToken;
                                }
                            }
                            i -= 1;
                        }
                        break;
                    case 41:
                        // JUMP_IF_FALSE;
                        value = valueStack[--valueStackSize];
                        if ((value.type != 2))
                        {
                            hasInterrupt = EX_InvalidArgument(ec, "Boolean expected.");
                        }
                        else if (!(bool)value.internalValue)
                        {
                            pc += row[0];
                        }
                        break;
                    case 42:
                        // JUMP_IF_FALSE_NON_POP;
                        value = valueStack[(valueStackSize - 1)];
                        if ((value.type != 2))
                        {
                            hasInterrupt = EX_InvalidArgument(ec, "Boolean expected.");
                        }
                        else if ((bool)value.internalValue)
                        {
                            valueStackSize -= 1;
                        }
                        else
                        {
                            pc += row[0];
                        }
                        break;
                    case 43:
                        // JUMP_IF_TRUE;
                        value = valueStack[--valueStackSize];
                        if ((value.type != 2))
                        {
                            hasInterrupt = EX_InvalidArgument(ec, "Boolean expected.");
                        }
                        else if ((bool)value.internalValue)
                        {
                            pc += row[0];
                        }
                        break;
                    case 44:
                        // JUMP_IF_TRUE_NO_POP;
                        value = valueStack[(valueStackSize - 1)];
                        if ((value.type != 2))
                        {
                            hasInterrupt = EX_InvalidArgument(ec, "Boolean expected.");
                        }
                        else if ((bool)value.internalValue)
                        {
                            pc += row[0];
                        }
                        else
                        {
                            valueStackSize -= 1;
                        }
                        break;
                    case 45:
                        // LAMBDA;
                        if (!metadata.lambdaTable.ContainsKey(pc))
                        {
                            int1 = (4 + row[4] + 1);
                            _len = row[int1];
                            intArray1 = new int[_len];
                            i = 0;
                            while ((i < _len))
                            {
                                intArray1[i] = row[(int1 + i + 1)];
                                i += 1;
                            }
                            _len = row[4];
                            intArray2 = new int[_len];
                            i = 0;
                            while ((i < _len))
                            {
                                intArray2[i] = row[(5 + i)];
                                i += 1;
                            }
                            metadata.lambdaTable[pc] = new FunctionInfo(pc, 0, pc, row[0], row[1], 5, 0, row[2], intArray2, "lambda", intArray1);
                        }
                        closure = new Dictionary<int, ClosureValuePointer>();
                        parentClosure = stack.closureVariables;
                        if ((parentClosure == null))
                        {
                            parentClosure = new Dictionary<int, ClosureValuePointer>();
                            stack.closureVariables = parentClosure;
                            parentClosure[-1] = new ClosureValuePointer(stack.objectContext);
                        }
                        closure[-1] = parentClosure[-1];
                        functionInfo = metadata.lambdaTable[pc];
                        intArray1 = functionInfo.closureIds;
                        _len = intArray1.Length;
                        i = 0;
                        while ((i < _len))
                        {
                            j = intArray1[i];
                            if (parentClosure.ContainsKey(j))
                            {
                                closure[j] = parentClosure[j];
                            }
                            else
                            {
                                closure[j] = new ClosureValuePointer(null);
                                parentClosure[j] = closure[j];
                            }
                            i += 1;
                        }
                        if ((valueStackSize == valueStackCapacity))
                        {
                            valueStack = valueStackIncreaseCapacity(ec);
                            valueStackCapacity = valueStack.Length;
                        }
                        valueStack[valueStackSize] = new Value(9, new FunctionPointer(5, null, 0, pc, closure));
                        valueStackSize += 1;
                        pc += row[3];
                        break;
                    case 46:
                        // LIB_DECLARATION;
                        prepareToSuspend(ec, stack, valueStackSize, pc);
                        ec.activeInterrupt = new Interrupt(4, row[0], stringArgs[pc], 0.0, null);
                        hasInterrupt = true;
                        break;
                    case 47:
                        // LIST_SLICE;
                        if ((row[2] == 1))
                        {
                            valueStackSize -= 1;
                            arg3 = valueStack[valueStackSize];
                        }
                        else
                        {
                            arg3 = null;
                        }
                        if ((row[1] == 1))
                        {
                            valueStackSize -= 1;
                            arg2 = valueStack[valueStackSize];
                        }
                        else
                        {
                            arg2 = null;
                        }
                        if ((row[0] == 1))
                        {
                            valueStackSize -= 1;
                            arg1 = valueStack[valueStackSize];
                        }
                        else
                        {
                            arg1 = null;
                        }
                        value = valueStack[(valueStackSize - 1)];
                        value = performListSlice(globals, ec, value, arg1, arg2, arg3);
                        hasInterrupt = (ec.activeInterrupt != null);
                        if (!hasInterrupt)
                        {
                            valueStack[(valueStackSize - 1)] = value;
                        }
                        break;
                    case 48:
                        // LITERAL;
                        if ((valueStackSize == valueStackCapacity))
                        {
                            valueStack = valueStackIncreaseCapacity(ec);
                            valueStackCapacity = valueStack.Length;
                        }
                        valueStack[valueStackSize++] = literalTable[row[0]];
                        break;
                    case 49:
                        // LITERAL_STREAM;
                        int1 = row.Length;
                        if (((valueStackSize + int1) > valueStackCapacity))
                        {
                            while (((valueStackSize + int1) > valueStackCapacity))
                            {
                                valueStackIncreaseCapacity(ec);
                                valueStack = ec.valueStack;
                                valueStackCapacity = valueStack.Length;
                            }
                        }
                        i = int1;
                        while ((--i >= 0))
                        {
                            valueStack[valueStackSize++] = literalTable[row[i]];
                        }
                        break;
                    case 50:
                        // LOCAL;
                        int1 = (localsStackOffset + row[0]);
                        if ((localsStackSet[int1] == localsStackSetToken))
                        {
                            if ((valueStackSize == valueStackCapacity))
                            {
                                valueStack = valueStackIncreaseCapacity(ec);
                                valueStackCapacity = valueStack.Length;
                            }
                            valueStack[valueStackSize++] = localsStack[int1];
                        }
                        else
                        {
                            string1 = string.Join("", new string[] { "Variable used before it was set: '",vm.metadata.identifiers[row[1]],"'" });
                            hasInterrupt = EX_UnassignedVariable(ec, string1);
                        }
                        break;
                    case 51:
                        // LOC_TABLE;
                        initLocTable(vm, row);
                        break;
                    case 52:
                        // NEGATIVE_SIGN;
                        value = valueStack[(valueStackSize - 1)];
                        type = value.type;
                        if ((type == 3))
                        {
                            valueStack[(valueStackSize - 1)] = buildInteger(globals, -(int)value.internalValue);
                        }
                        else if ((type == 4))
                        {
                            valueStack[(valueStackSize - 1)] = buildFloat(globals, -(double)value.internalValue);
                        }
                        else
                        {
                            hasInterrupt = EX_InvalidArgument(ec, string.Join("", new string[] { "Negative sign can only be applied to numbers. Found ",getTypeFromId(type)," instead." }));
                        }
                        break;
                    case 53:
                        // POP;
                        valueStackSize -= 1;
                        break;
                    case 54:
                        // POP_IF_NULL_OR_JUMP;
                        value = valueStack[(valueStackSize - 1)];
                        if ((value.type == 1))
                        {
                            valueStackSize -= 1;
                        }
                        else
                        {
                            pc += row[0];
                        }
                        break;
                    case 55:
                        // PUSH_FUNC_REF;
                        value = null;
                        switch (row[1])
                        {
                            case 0:
                                value = new Value(9, new FunctionPointer(1, null, 0, row[0], null));
                                break;
                            case 1:
                                value = new Value(9, new FunctionPointer(2, stack.objectContext, row[2], row[0], null));
                                break;
                            case 2:
                                classId = row[2];
                                classInfo = classTable[classId];
                                staticConstructorNotInvoked = true;
                                if ((classInfo.staticInitializationState < 2))
                                {
                                    stack.pc = pc;
                                    stackFrame2 = maybeInvokeStaticConstructor(vm, ec, stack, classInfo, valueStackSize, PST_IntBuffer16);
                                    if ((PST_IntBuffer16[0] == 1))
                                    {
                                        return generateException(vm, stack, pc, valueStackSize, ec, 0, "Static initialization loop detected. The class this field is a member of is not done being initialized.");
                                    }
                                    if ((stackFrame2 != null))
                                    {
                                        staticConstructorNotInvoked = false;
                                        stack = stackFrame2;
                                        pc = stack.pc;
                                        localsStackSetToken = stack.localsStackSetToken;
                                        localsStackOffset = stack.localsStackOffset;
                                    }
                                }
                                if (staticConstructorNotInvoked)
                                {
                                    value = new Value(9, new FunctionPointer(3, null, classId, row[0], null));
                                }
                                else
                                {
                                    value = null;
                                }
                                break;
                        }
                        if ((value != null))
                        {
                            if ((valueStackSize == valueStackCapacity))
                            {
                                valueStack = valueStackIncreaseCapacity(ec);
                                valueStackCapacity = valueStack.Length;
                            }
                            valueStack[valueStackSize] = value;
                            valueStackSize += 1;
                        }
                        break;
                    case 56:
                        // RETURN;
                        if ((esfData[pc] != null))
                        {
                            intArray1 = esfData[pc];
                            pc = (intArray1[1] - 1);
                            if ((row[0] == 0))
                            {
                                stack.returnValueTempStorage = VALUE_NULL;
                            }
                            else
                            {
                                stack.returnValueTempStorage = valueStack[(valueStackSize - 1)];
                            }
                            valueStackSize = stack.valueStackPopSize;
                            stack.postFinallyBehavior = 3;
                        }
                        else
                        {
                            if ((stack.previous == null))
                            {
                                return interpreterFinished(vm, ec);
                            }
                            if ((stack.markClassAsInitialized != 0))
                            {
                                markClassAsInitialized(vm, stack, stack.markClassAsInitialized);
                            }
                            if (stack.returnValueUsed)
                            {
                                if ((row[0] == 0))
                                {
                                    valueStackSize = stack.valueStackPopSize;
                                    stack = stack.previous;
                                    if ((valueStackSize == valueStackCapacity))
                                    {
                                        valueStack = valueStackIncreaseCapacity(ec);
                                        valueStackCapacity = valueStack.Length;
                                    }
                                    valueStack[valueStackSize] = VALUE_NULL;
                                }
                                else
                                {
                                    value = valueStack[(valueStackSize - 1)];
                                    valueStackSize = stack.valueStackPopSize;
                                    stack = stack.previous;
                                    valueStack[valueStackSize] = value;
                                }
                                valueStackSize += 1;
                            }
                            else
                            {
                                valueStackSize = stack.valueStackPopSize;
                                stack = stack.previous;
                            }
                            pc = stack.pc;
                            localsStackOffset = stack.localsStackOffset;
                            localsStackSetToken = stack.localsStackSetToken;
                        }
                        break;
                    case 57:
                        // STACK_INSERTION_FOR_INCREMENT;
                        if ((valueStackSize == valueStackCapacity))
                        {
                            valueStack = valueStackIncreaseCapacity(ec);
                            valueStackCapacity = valueStack.Length;
                        }
                        valueStack[valueStackSize] = valueStack[(valueStackSize - 1)];
                        valueStack[(valueStackSize - 1)] = valueStack[(valueStackSize - 2)];
                        valueStack[(valueStackSize - 2)] = valueStack[(valueStackSize - 3)];
                        valueStack[(valueStackSize - 3)] = valueStack[valueStackSize];
                        valueStackSize += 1;
                        break;
                    case 58:
                        // STACK_SWAP_POP;
                        valueStackSize -= 1;
                        valueStack[(valueStackSize - 1)] = valueStack[valueStackSize];
                        break;
                    case 59:
                        // SWITCH_INT;
                        value = valueStack[--valueStackSize];
                        if ((value.type == 3))
                        {
                            intKey = (int)value.internalValue;
                            integerSwitch = integerSwitchesByPc[pc];
                            if ((integerSwitch == null))
                            {
                                integerSwitch = initializeIntSwitchStatement(vm, pc, row);
                            }
                            if (!integerSwitch.TryGetValue(intKey, out i)) i = -1;
                            if ((i == -1))
                            {
                                pc += row[0];
                            }
                            else
                            {
                                pc += i;
                            }
                        }
                        else
                        {
                            hasInterrupt = EX_InvalidArgument(ec, "Switch statement expects an integer.");
                        }
                        break;
                    case 60:
                        // SWITCH_STRING;
                        value = valueStack[--valueStackSize];
                        if ((value.type == 5))
                        {
                            stringKey = (string)value.internalValue;
                            stringSwitch = stringSwitchesByPc[pc];
                            if ((stringSwitch == null))
                            {
                                stringSwitch = initializeStringSwitchStatement(vm, pc, row);
                            }
                            if (!stringSwitch.TryGetValue(stringKey, out i)) i = -1;
                            if ((i == -1))
                            {
                                pc += row[0];
                            }
                            else
                            {
                                pc += i;
                            }
                        }
                        else
                        {
                            hasInterrupt = EX_InvalidArgument(ec, "Switch statement expects a string.");
                        }
                        break;
                    case 61:
                        // THIS;
                        if ((valueStackSize == valueStackCapacity))
                        {
                            valueStack = valueStackIncreaseCapacity(ec);
                            valueStackCapacity = valueStack.Length;
                        }
                        valueStack[valueStackSize] = stack.objectContext;
                        valueStackSize += 1;
                        break;
                    case 62:
                        // THROW;
                        valueStackSize -= 1;
                        value = valueStack[valueStackSize];
                        bool2 = (value.type == 8);
                        if (bool2)
                        {
                            objInstance1 = (ObjectInstance)value.internalValue;
                            if (!isClassASubclassOf(vm, objInstance1.classId, magicNumbers.coreExceptionClassId))
                            {
                                bool2 = false;
                            }
                        }
                        if (bool2)
                        {
                            objArray1 = objInstance1.nativeData;
                            intList1 = new List<int>();
                            objArray1[1] = intList1;
                            if (!isPcFromCore(vm, pc))
                            {
                                intList1.Add(pc);
                            }
                            ec.activeException = value;
                            ec.activeExceptionHandled = false;
                            stack.pc = pc;
                            intArray1 = esfData[pc];
                            value = ec.activeException;
                            objInstance1 = (ObjectInstance)value.internalValue;
                            objArray1 = objInstance1.nativeData;
                            bool1 = true;
                            if ((objArray1[0] != null))
                            {
                                bool1 = (bool)objArray1[0];
                            }
                            intList1 = (List<int>)objArray1[1];
                            while (((stack != null) && ((intArray1 == null) || bool1)))
                            {
                                stack = stack.previous;
                                if ((stack != null))
                                {
                                    pc = stack.pc;
                                    intList1.Add(pc);
                                    intArray1 = esfData[pc];
                                }
                            }
                            if ((stack == null))
                            {
                                return uncaughtExceptionResult(vm, value);
                            }
                            int1 = intArray1[0];
                            if ((int1 < pc))
                            {
                                int1 = intArray1[1];
                            }
                            pc = (int1 - 1);
                            stack.pc = pc;
                            localsStackOffset = stack.localsStackOffset;
                            localsStackSetToken = stack.localsStackSetToken;
                            ec.stackTop = stack;
                            stack.postFinallyBehavior = 0;
                            ec.currentValueStackSize = valueStackSize;
                        }
                        else
                        {
                            hasInterrupt = EX_InvalidArgument(ec, "Thrown value is not an exception.");
                        }
                        break;
                    case 63:
                        // TOKEN_DATA;
                        tokenDataImpl(vm, row);
                        break;
                    case 64:
                        // USER_CODE_START;
                        metadata.userCodeStart = row[0];
                        break;
                    case 65:
                        // VERIFY_TYPE_IS_ITERABLE;
                        value = valueStack[--valueStackSize];
                        i = (localsStackOffset + row[0]);
                        localsStack[i] = value;
                        localsStackSet[i] = localsStackSetToken;
                        int1 = value.type;
                        if (((int1 != 6) && (int1 != 5)))
                        {
                            hasInterrupt = EX_InvalidArgument(ec, string.Join("", new string[] { "Expected an iterable type, such as a list or string. Found ",getTypeFromId(int1)," instead." }));
                        }
                        i = (localsStackOffset + row[1]);
                        localsStack[i] = VALUE_INT_ZERO;
                        localsStackSet[i] = localsStackSetToken;
                        break;
                    default:
                        // THIS SHOULD NEVER HAPPEN;
                        return generateException(vm, stack, pc, valueStackSize, ec, 0, "Bad op code: " + (ops[pc]).ToString());
                }
                if (hasInterrupt)
                {
                    Interrupt interrupt = ec.activeInterrupt;
                    ec.activeInterrupt = null;
                    if ((interrupt.type == 1))
                    {
                        return generateException(vm, stack, pc, valueStackSize, ec, interrupt.exceptionType, interrupt.exceptionMessage);
                    }
                    if ((interrupt.type == 3))
                    {
                        return new InterpreterResult(5, "", interrupt.sleepDurationSeconds, 0, false, "");
                    }
                    if ((interrupt.type == 4))
                    {
                        return new InterpreterResult(6, "", 0.0, 0, false, interrupt.exceptionMessage);
                    }
                }
                ++pc;
            }
        }

        public static object invokeNamedCallback(VmContext vm, int id, object[] args)
        {
            System.Func<object[], object> cb = vm.namedCallbacks.callbacksById[id];
            return cb(args);
        }

        public static string IpcNamedPipe_create(Value objValue, Value nameValue)
        {
            ObjectInstance obj = (ObjectInstance)objValue.internalValue;
            obj.nativeData = new object[1];
            obj.nativeData[0] = CoreFunctions.NamedPipeCreate((string)nameValue.internalValue);
            return null;
        }

        public static string IpcNamedPipe_flush(Value objValue)
        {
            object pipe = getNativeDataItem(objValue, 0);
            return CoreFunctions.NamedPipeFlush(pipe);
        }

        public static bool IpcNamedPipe_isSupported()
        {
            return true;
        }

        public static string IpcNamedPipe_send(Value objValue, Value strValue)
        {
            object pipe = getNativeDataItem(objValue, 0);
            return CoreFunctions.NamedPipeWriteLine(pipe, (string)strValue.internalValue);
        }

        public static string IpcNamedPipeServer_close(Value objValue)
        {
            ObjectInstance obj = (ObjectInstance)objValue.internalValue;
            object pipe = obj.nativeData[0];
            return CoreFunctions.NamedPipeServerClose(pipe);
        }

        public static string IpcNamedPipeServer_create(Value objValue, Value nameValue, Value startFn, Value dataFn, Value closeFn)
        {
            ObjectInstance obj = (ObjectInstance)objValue.internalValue;
            obj.nativeData = new object[1];
            obj.nativeData[0] = CoreFunctions.NamedPipeServerCreate((string)nameValue.internalValue, startFn, dataFn, closeFn);
            return null;
        }

        public static bool isClassASubclassOf(VmContext vm, int subClassId, int parentClassId)
        {
            if ((subClassId == parentClassId))
            {
                return true;
            }
            ClassInfo[] classTable = vm.metadata.classTable;
            int classIdWalker = subClassId;
            while ((classIdWalker != -1))
            {
                if ((classIdWalker == parentClassId))
                {
                    return true;
                }
                ClassInfo classInfo = classTable[classIdWalker];
                classIdWalker = classInfo.baseClassId;
            }
            return false;
        }

        public static bool isPcFromCore(VmContext vm, int pc)
        {
            if ((vm.symbolData == null))
            {
                return false;
            }
            List<Token> tokens = vm.symbolData.tokenData[pc];
            if ((tokens == null))
            {
                return false;
            }
            Token token = tokens[0];
            string filename = tokenHelperGetFileLine(vm, token.fileId, 0);
            return "[Core]" == filename;
        }

        public static bool isStringEqual(string a, string b)
        {
            if (((string)a == (string)b))
            {
                return true;
            }
            return false;
        }

        public static bool isVmResultRootExecContext(InterpreterResult result)
        {
            return result.isRootContext;
        }

        public static int[] listImplToBytes(ListImpl list)
        {
            int size = list.size;
            int[] bytes = new int[size];
            Value nv = null;
            int n = 0;
            Value[] values = list.array;
            int i = 0;
            while ((i < size))
            {
                nv = values[i];
                if ((nv.type != 3))
                {
                    return null;
                }
                n = (int)nv.internalValue;
                if (((n < 0) || (n > 255)))
                {
                    return null;
                }
                bytes[i] = n;
                ++i;
            }
            return bytes;
        }

        public static bool makeByteListNativeData(ObjectInstance obj, Value vList)
        {
            if ((vList.type != 6))
            {
                return false;
            }
            ListImpl list = (ListImpl)vList.internalValue;
            int[] bytes = listImplToBytes(list);
            if ((bytes == null))
            {
                return false;
            }
            obj.nativeData = new object[1];
            obj.nativeData[0] = bytes;
            return true;
        }

        public static ListImpl makeEmptyList(int[] type, int capacity)
        {
            return new ListImpl(type, 0, capacity, new Value[capacity]);
        }

        public static int markClassAsInitialized(VmContext vm, StackFrame stack, int classId)
        {
            ClassInfo classInfo = vm.metadata.classTable[stack.markClassAsInitialized];
            classInfo.staticInitializationState = 2;
            vm.classStaticInitializationStack.RemoveAt(vm.classStaticInitializationStack.Count - 1);
            return 0;
        }

        public static StackFrame maybeInvokeStaticConstructor(VmContext vm, ExecutionContext ec, StackFrame stack, ClassInfo classInfo, int valueStackSize, int[] intOutParam)
        {
            PST_IntBuffer16[0] = 0;
            int classId = classInfo.id;
            if ((classInfo.staticInitializationState == 1))
            {
                List<int> classIdsBeingInitialized = vm.classStaticInitializationStack;
                if ((classIdsBeingInitialized[(classIdsBeingInitialized.Count - 1)] != classId))
                {
                    PST_IntBuffer16[0] = 1;
                }
                return null;
            }
            classInfo.staticInitializationState = 1;
            vm.classStaticInitializationStack.Add(classId);
            FunctionInfo functionInfo = vm.metadata.functionTable[classInfo.staticConstructorFunctionId];
            stack.pc -= 1;
            int newFrameLocalsSize = functionInfo.localsSize;
            int currentFrameLocalsEnd = stack.localsStackOffsetEnd;
            if ((ec.localsStack.Length <= (currentFrameLocalsEnd + newFrameLocalsSize)))
            {
                increaseLocalsStackCapacity(ec, newFrameLocalsSize);
            }
            if ((ec.localsStackSetToken > 2000000000))
            {
                resetLocalsStackTokens(ec, stack);
            }
            ec.localsStackSetToken += 1;
            return new StackFrame(functionInfo.pc, ec.localsStackSetToken, currentFrameLocalsEnd, (currentFrameLocalsEnd + newFrameLocalsSize), stack, false, null, valueStackSize, classId, (stack.depth + 1), 0, null, null, null);
        }

        public static Value multiplyString(VmGlobals globals, Value strValue, string str, int n)
        {
            if ((n <= 2))
            {
                if ((n == 1))
                {
                    return strValue;
                }
                if ((n == 2))
                {
                    return buildString(globals, str + str);
                }
                return globals.stringEmpty;
            }
            List<string> builder = new List<string>();
            while ((n > 0))
            {
                n -= 1;
                builder.Add(str);
            }
            str = string.Join("", builder);
            return buildString(globals, str);
        }

        public static int nextPowerOf2(int value)
        {
            if ((((value - 1) & value) == 0))
            {
                return value;
            }
            int output = 1;
            while ((output < value))
            {
                output *= 2;
            }
            return output;
        }

        public static int noop()
        {
            return 0;
        }

        public static Value performListSlice(VmGlobals globals, ExecutionContext ec, Value value, Value arg1, Value arg2, Value arg3)
        {
            int begin = 0;
            int end = 0;
            int step = 0;
            int length = 0;
            int i = 0;
            bool isForward = false;
            bool isString = false;
            string originalString = "";
            ListImpl originalList = null;
            ListImpl outputList = null;
            List<string> outputString = null;
            int status = 0;
            if ((arg3 != null))
            {
                if ((arg3.type == 3))
                {
                    step = (int)arg3.internalValue;
                    if ((step == 0))
                    {
                        status = 2;
                    }
                }
                else
                {
                    status = 3;
                    step = 1;
                }
            }
            else
            {
                step = 1;
            }
            isForward = (step > 0);
            if ((arg2 != null))
            {
                if ((arg2.type == 3))
                {
                    end = (int)arg2.internalValue;
                }
                else
                {
                    status = 3;
                }
            }
            if ((arg1 != null))
            {
                if ((arg1.type == 3))
                {
                    begin = (int)arg1.internalValue;
                }
                else
                {
                    status = 3;
                }
            }
            if ((value.type == 5))
            {
                isString = true;
                originalString = (string)value.internalValue;
                length = originalString.Length;
            }
            else if ((value.type == 6))
            {
                isString = false;
                originalList = (ListImpl)value.internalValue;
                length = originalList.size;
            }
            else
            {
                EX_InvalidArgument(ec, string.Join("", new string[] { "Cannot apply slicing to ",getTypeFromId(value.type),". Must be string or list." }));
                return globals.valueNull;
            }
            if ((status >= 2))
            {
                string msg = null;
                if (isString)
                {
                    msg = "String";
                }
                else
                {
                    msg = "List";
                }
                if ((status == 3))
                {
                    msg += " slice indexes must be integers. Found ";
                    if (((arg1 != null) && (arg1.type != 3)))
                    {
                        EX_InvalidArgument(ec, string.Join("", new string[] { msg,getTypeFromId(arg1.type)," for begin index." }));
                        return globals.valueNull;
                    }
                    if (((arg2 != null) && (arg2.type != 3)))
                    {
                        EX_InvalidArgument(ec, string.Join("", new string[] { msg,getTypeFromId(arg2.type)," for end index." }));
                        return globals.valueNull;
                    }
                    if (((arg3 != null) && (arg3.type != 3)))
                    {
                        EX_InvalidArgument(ec, string.Join("", new string[] { msg,getTypeFromId(arg3.type)," for step amount." }));
                        return globals.valueNull;
                    }
                    EX_InvalidArgument(ec, "Invalid slice arguments.");
                    return globals.valueNull;
                }
                else
                {
                    EX_InvalidArgument(ec, msg + " slice step cannot be 0.");
                    return globals.valueNull;
                }
            }
            status = canonicalizeListSliceArgs(PST_IntBuffer16, arg1, arg2, begin, end, step, length, isForward);
            if ((status == 1))
            {
                begin = PST_IntBuffer16[0];
                end = PST_IntBuffer16[1];
                if (isString)
                {
                    outputString = new List<string>();
                    if (isForward)
                    {
                        if ((step == 1))
                        {
                            return buildString(globals, originalString.Substring(begin, (end - begin)));
                        }
                        else
                        {
                            while ((begin < end))
                            {
                                outputString.Add(originalString[begin].ToString());
                                begin += step;
                            }
                        }
                    }
                    else
                    {
                        while ((begin > end))
                        {
                            outputString.Add(originalString[begin].ToString());
                            begin += step;
                        }
                    }
                    value = buildString(globals, string.Join("", outputString));
                }
                else
                {
                    outputList = makeEmptyList(originalList.type, 10);
                    if (isForward)
                    {
                        while ((begin < end))
                        {
                            addToList(outputList, originalList.array[begin]);
                            begin += step;
                        }
                    }
                    else
                    {
                        while ((begin > end))
                        {
                            addToList(outputList, originalList.array[begin]);
                            begin += step;
                        }
                    }
                    value = new Value(6, outputList);
                }
            }
            else if ((status == 0))
            {
                if (isString)
                {
                    value = globals.stringEmpty;
                }
                else
                {
                    value = new Value(6, makeEmptyList(originalList.type, 0));
                }
            }
            else if ((status == 2))
            {
                if (!isString)
                {
                    outputList = makeEmptyList(originalList.type, length);
                    i = 0;
                    while ((i < length))
                    {
                        addToList(outputList, originalList.array[i]);
                        i += 1;
                    }
                    value = new Value(6, outputList);
                }
            }
            else
            {
                string msg = null;
                if (isString)
                {
                    msg = "String";
                }
                else
                {
                    msg = "List";
                }
                if ((status == 3))
                {
                    msg += " slice begin index is out of range.";
                }
                else if (isForward)
                {
                    msg += " slice begin index must occur before the end index when step is positive.";
                }
                else
                {
                    msg += " slice begin index must occur after the end index when the step is negative.";
                }
                EX_IndexOutOfRange(ec, msg);
                return globals.valueNull;
            }
            return value;
        }

        public static int prepareToSuspend(ExecutionContext ec, StackFrame stack, int valueStackSize, int currentPc)
        {
            ec.stackTop = stack;
            ec.currentValueStackSize = valueStackSize;
            stack.pc = (currentPc + 1);
            return 0;
        }

        public static int[] primitiveMethodsInitializeLookup(Dictionary<string, int> nameLookups)
        {
            int length = nameLookups.Count;
            int[] lookup = new int[length];
            int i = 0;
            while ((i < length))
            {
                lookup[i] = -1;
                i += 1;
            }
            if (nameLookups.ContainsKey("add"))
            {
                lookup[nameLookups["add"]] = 0;
            }
            if (nameLookups.ContainsKey("argCountMax"))
            {
                lookup[nameLookups["argCountMax"]] = 1;
            }
            if (nameLookups.ContainsKey("argCountMin"))
            {
                lookup[nameLookups["argCountMin"]] = 2;
            }
            if (nameLookups.ContainsKey("choice"))
            {
                lookup[nameLookups["choice"]] = 3;
            }
            if (nameLookups.ContainsKey("clear"))
            {
                lookup[nameLookups["clear"]] = 4;
            }
            if (nameLookups.ContainsKey("clone"))
            {
                lookup[nameLookups["clone"]] = 5;
            }
            if (nameLookups.ContainsKey("concat"))
            {
                lookup[nameLookups["concat"]] = 6;
            }
            if (nameLookups.ContainsKey("contains"))
            {
                lookup[nameLookups["contains"]] = 7;
            }
            if (nameLookups.ContainsKey("createInstance"))
            {
                lookup[nameLookups["createInstance"]] = 8;
            }
            if (nameLookups.ContainsKey("endsWith"))
            {
                lookup[nameLookups["endsWith"]] = 9;
            }
            if (nameLookups.ContainsKey("filter"))
            {
                lookup[nameLookups["filter"]] = 10;
            }
            if (nameLookups.ContainsKey("get"))
            {
                lookup[nameLookups["get"]] = 11;
            }
            if (nameLookups.ContainsKey("getName"))
            {
                lookup[nameLookups["getName"]] = 12;
            }
            if (nameLookups.ContainsKey("indexOf"))
            {
                lookup[nameLookups["indexOf"]] = 13;
            }
            if (nameLookups.ContainsKey("insert"))
            {
                lookup[nameLookups["insert"]] = 14;
            }
            if (nameLookups.ContainsKey("invoke"))
            {
                lookup[nameLookups["invoke"]] = 15;
            }
            if (nameLookups.ContainsKey("isA"))
            {
                lookup[nameLookups["isA"]] = 16;
            }
            if (nameLookups.ContainsKey("join"))
            {
                lookup[nameLookups["join"]] = 17;
            }
            if (nameLookups.ContainsKey("keys"))
            {
                lookup[nameLookups["keys"]] = 18;
            }
            if (nameLookups.ContainsKey("lower"))
            {
                lookup[nameLookups["lower"]] = 19;
            }
            if (nameLookups.ContainsKey("ltrim"))
            {
                lookup[nameLookups["ltrim"]] = 20;
            }
            if (nameLookups.ContainsKey("map"))
            {
                lookup[nameLookups["map"]] = 21;
            }
            if (nameLookups.ContainsKey("merge"))
            {
                lookup[nameLookups["merge"]] = 22;
            }
            if (nameLookups.ContainsKey("pop"))
            {
                lookup[nameLookups["pop"]] = 23;
            }
            if (nameLookups.ContainsKey("remove"))
            {
                lookup[nameLookups["remove"]] = 24;
            }
            if (nameLookups.ContainsKey("replace"))
            {
                lookup[nameLookups["replace"]] = 25;
            }
            if (nameLookups.ContainsKey("reverse"))
            {
                lookup[nameLookups["reverse"]] = 26;
            }
            if (nameLookups.ContainsKey("rtrim"))
            {
                lookup[nameLookups["rtrim"]] = 27;
            }
            if (nameLookups.ContainsKey("shuffle"))
            {
                lookup[nameLookups["shuffle"]] = 28;
            }
            if (nameLookups.ContainsKey("sort"))
            {
                lookup[nameLookups["sort"]] = 29;
            }
            if (nameLookups.ContainsKey("split"))
            {
                lookup[nameLookups["split"]] = 30;
            }
            if (nameLookups.ContainsKey("startsWith"))
            {
                lookup[nameLookups["startsWith"]] = 31;
            }
            if (nameLookups.ContainsKey("trim"))
            {
                lookup[nameLookups["trim"]] = 32;
            }
            if (nameLookups.ContainsKey("upper"))
            {
                lookup[nameLookups["upper"]] = 33;
            }
            if (nameLookups.ContainsKey("values"))
            {
                lookup[nameLookups["values"]] = 34;
            }
            return lookup;
        }

        public static string primitiveMethodWrongArgCountError(string name, int expected, int actual)
        {
            string output = "";
            if ((expected == 0))
            {
                output = name + " does not accept any arguments.";
            }
            else if ((expected == 1))
            {
                output = name + " accepts exactly 1 argument.";
            }
            else
            {
                output = string.Join("", new string[] { name," requires ",(expected).ToString()," arguments." });
            }
            return string.Join("", new string[] { output," Found: ",(actual).ToString() });
        }

        public static int printToStdOut(string prefix, string line)
        {
            if ((prefix == null))
            {
                PlatformTranslationHelper.PrintStdOut(line);
            }
            else
            {
                string canonical = line.Replace("\r\n", "\n").Replace("\r", "\n");
                string[] lines = PST_StringSplit(canonical, "\n");
                int i = 0;
                while ((i < lines.Length))
                {
                    PlatformTranslationHelper.PrintStdOut(string.Join("", new string[] { prefix,": ",lines[i] }));
                    i += 1;
                }
            }
            return 0;
        }

        public static int qsortHelper(string[] keyStringList, double[] keyNumList, int[] indices, bool isString, int startIndex, int endIndex)
        {
            if (((endIndex - startIndex) <= 0))
            {
                return 0;
            }
            if (((endIndex - startIndex) == 1))
            {
                if (sortHelperIsRevOrder(keyStringList, keyNumList, isString, startIndex, endIndex))
                {
                    sortHelperSwap(keyStringList, keyNumList, indices, isString, startIndex, endIndex);
                }
                return 0;
            }
            int mid = ((endIndex + startIndex) >> 1);
            sortHelperSwap(keyStringList, keyNumList, indices, isString, mid, startIndex);
            int upperPointer = (endIndex + 1);
            int lowerPointer = (startIndex + 1);
            while ((upperPointer > lowerPointer))
            {
                if (sortHelperIsRevOrder(keyStringList, keyNumList, isString, startIndex, lowerPointer))
                {
                    lowerPointer += 1;
                }
                else
                {
                    upperPointer -= 1;
                    sortHelperSwap(keyStringList, keyNumList, indices, isString, lowerPointer, upperPointer);
                }
            }
            int midIndex = (lowerPointer - 1);
            sortHelperSwap(keyStringList, keyNumList, indices, isString, midIndex, startIndex);
            qsortHelper(keyStringList, keyNumList, indices, isString, startIndex, (midIndex - 1));
            qsortHelper(keyStringList, keyNumList, indices, isString, (midIndex + 1), endIndex);
            return 0;
        }

        public static Value queryValue(VmContext vm, int execId, int stackFrameOffset, string[] steps)
        {
            if ((execId == -1))
            {
                execId = vm.lastExecutionContextId;
            }
            ExecutionContext ec = vm.executionContexts[execId];
            StackFrame stackFrame = ec.stackTop;
            while ((stackFrameOffset > 0))
            {
                stackFrameOffset -= 1;
                stackFrame = stackFrame.previous;
            }
            Value current = null;
            int i = 0;
            int j = 0;
            int len = steps.Length;
            i = 0;
            while ((i < steps.Length))
            {
                if (((current == null) && (i > 0)))
                {
                    return null;
                }
                string step = steps[i];
                if (isStringEqual(".", step))
                {
                    return null;
                }
                else if (isStringEqual("this", step))
                {
                    current = stackFrame.objectContext;
                }
                else if (isStringEqual("class", step))
                {
                    return null;
                }
                else if (isStringEqual("local", step))
                {
                    i += 1;
                    step = steps[i];
                    Dictionary<int, List<string>> localNamesByFuncPc = vm.symbolData.localVarNamesById;
                    List<string> localNames = null;
                    if (((localNamesByFuncPc == null) || (localNamesByFuncPc.Count == 0)))
                    {
                        return null;
                    }
                    j = stackFrame.pc;
                    while ((j >= 0))
                    {
                        if (localNamesByFuncPc.ContainsKey(j))
                        {
                            localNames = localNamesByFuncPc[j];
                            j = -1;
                        }
                        j -= 1;
                    }
                    if ((localNames == null))
                    {
                        return null;
                    }
                    int localId = -1;
                    if ((localNames != null))
                    {
                        j = 0;
                        while ((j < localNames.Count))
                        {
                            if (isStringEqual(localNames[j], step))
                            {
                                localId = j;
                                j = localNames.Count;
                            }
                            j += 1;
                        }
                    }
                    if ((localId == -1))
                    {
                        return null;
                    }
                    int localOffset = (localId + stackFrame.localsStackOffset);
                    if ((ec.localsStackSet[localOffset] != stackFrame.localsStackSetToken))
                    {
                        return null;
                    }
                    current = ec.localsStack[localOffset];
                }
                else if (isStringEqual("index", step))
                {
                    return null;
                }
                else if (isStringEqual("key-int", step))
                {
                    return null;
                }
                else if (isStringEqual("key-str", step))
                {
                    return null;
                }
                else if (isStringEqual("key-obj", step))
                {
                    return null;
                }
                else
                {
                    return null;
                }
                i += 1;
            }
            return current;
        }

        public static int read_integer(int[] pindex, string raw, int length, string alphaNums)
        {
            int num = 0;
            char c = raw[pindex[0]];
            pindex[0] = (pindex[0] + 1);
            if ((c == '%'))
            {
                string value = read_till(pindex, raw, length, '%');
                num = int.Parse(value);
            }
            else if ((c == '@'))
            {
                num = read_integer(pindex, raw, length, alphaNums);
                num *= 62;
                num += read_integer(pindex, raw, length, alphaNums);
            }
            else if ((c == '#'))
            {
                num = read_integer(pindex, raw, length, alphaNums);
                num *= 62;
                num += read_integer(pindex, raw, length, alphaNums);
                num *= 62;
                num += read_integer(pindex, raw, length, alphaNums);
            }
            else if ((c == '^'))
            {
                num = (-1 * read_integer(pindex, raw, length, alphaNums));
            }
            else
            {
                // TODO: string.IndexOfChar(c);
                num = alphaNums.IndexOf(c.ToString());
                if ((num == -1))
                {
                }
            }
            return num;
        }

        public static string read_string(int[] pindex, string raw, int length, string alphaNums)
        {
            string b64 = read_till(pindex, raw, length, '%');
            return PST_Base64ToString(b64);
        }

        public static string read_till(int[] index, string raw, int length, char end)
        {
            List<char> output = new List<char>();
            bool ctn = true;
            char c = ' ';
            while (ctn)
            {
                c = raw[index[0]];
                if ((c == end))
                {
                    ctn = false;
                }
                else
                {
                    output.Add(c);
                }
                index[0] = (index[0] + 1);
            }
            return new string(output.ToArray());
        }

        public static int[] reallocIntArray(int[] original, int requiredCapacity)
        {
            int oldSize = original.Length;
            int size = oldSize;
            while ((size < requiredCapacity))
            {
                size *= 2;
            }
            int[] output = new int[size];
            int i = 0;
            while ((i < oldSize))
            {
                output[i] = original[i];
                i += 1;
            }
            return output;
        }

        public static Value Reflect_allClasses(VmContext vm)
        {
            int[] generics = new int[1];
            generics[0] = 10;
            ListImpl output = makeEmptyList(generics, 20);
            ClassInfo[] classTable = vm.metadata.classTable;
            int i = 1;
            while ((i < classTable.Length))
            {
                ClassInfo classInfo = classTable[i];
                if ((classInfo == null))
                {
                    i = classTable.Length;
                }
                else
                {
                    addToList(output, new Value(10, new ClassValue(false, classInfo.id)));
                }
                i += 1;
            }
            return new Value(6, output);
        }

        public static Value Reflect_getMethods(VmContext vm, ExecutionContext ec, Value methodSource)
        {
            ListImpl output = makeEmptyList(null, 8);
            if ((methodSource.type == 8))
            {
                ObjectInstance objInstance1 = (ObjectInstance)methodSource.internalValue;
                ClassInfo classInfo = vm.metadata.classTable[objInstance1.classId];
                int i = 0;
                while ((i < classInfo.functionIds.Length))
                {
                    int functionId = classInfo.functionIds[i];
                    if ((functionId != -1))
                    {
                        addToList(output, new Value(9, new FunctionPointer(2, methodSource, objInstance1.classId, functionId, null)));
                    }
                    i += 1;
                }
            }
            else
            {
                ClassValue classValue = (ClassValue)methodSource.internalValue;
                ClassInfo classInfo = vm.metadata.classTable[classValue.classId];
                EX_UnsupportedOperation(ec, "static method reflection not implemented yet.");
            }
            return new Value(6, output);
        }

        public static int registerNamedCallback(VmContext vm, string scope, string functionName, System.Func<object[], object> callback)
        {
            int id = getNamedCallbackIdImpl(vm, scope, functionName, true);
            vm.namedCallbacks.callbacksById[id] = callback;
            return id;
        }

        public static int resetLocalsStackTokens(ExecutionContext ec, StackFrame stack)
        {
            Value[] localsStack = ec.localsStack;
            int[] localsStackSet = ec.localsStackSet;
            int i = stack.localsStackOffsetEnd;
            while ((i < localsStackSet.Length))
            {
                localsStackSet[i] = 0;
                localsStack[i] = null;
                i += 1;
            }
            StackFrame stackWalker = stack;
            while ((stackWalker != null))
            {
                int token = stackWalker.localsStackSetToken;
                stackWalker.localsStackSetToken = 1;
                i = stackWalker.localsStackOffset;
                while ((i < stackWalker.localsStackOffsetEnd))
                {
                    if ((localsStackSet[i] == token))
                    {
                        localsStackSet[i] = 1;
                    }
                    else
                    {
                        localsStackSet[i] = 0;
                        localsStack[i] = null;
                    }
                    i += 1;
                }
                stackWalker = stackWalker.previous;
            }
            ec.localsStackSetToken = 1;
            return -1;
        }

        public static int resolvePrimitiveMethodName2(int[] lookup, int type, int globalNameId)
        {
            int output = lookup[globalNameId];
            if ((output != -1))
            {
                switch ((type + (11 * output)))
                {
                    case 82:
                        return output;
                    case 104:
                        return output;
                    case 148:
                        return output;
                    case 214:
                        return output;
                    case 225:
                        return output;
                    case 280:
                        return output;
                    case 291:
                        return output;
                    case 302:
                        return output;
                    case 335:
                        return output;
                    case 346:
                        return output;
                    case 357:
                        return output;
                    case 368:
                        return output;
                    case 6:
                        return output;
                    case 39:
                        return output;
                    case 50:
                        return output;
                    case 61:
                        return output;
                    case 72:
                        return output;
                    case 83:
                        return output;
                    case 116:
                        return output;
                    case 160:
                        return output;
                    case 193:
                        return output;
                    case 237:
                        return output;
                    case 259:
                        return output;
                    case 270:
                        return output;
                    case 292:
                        return output;
                    case 314:
                        return output;
                    case 325:
                        return output;
                    case 51:
                        return output;
                    case 62:
                        return output;
                    case 84:
                        return output;
                    case 128:
                        return output;
                    case 205:
                        return output;
                    case 249:
                        return output;
                    case 271:
                        return output;
                    case 381:
                        return output;
                    case 20:
                        return output;
                    case 31:
                        return output;
                    case 141:
                        return output;
                    case 174:
                        return output;
                    case 98:
                        return output;
                    case 142:
                        return output;
                    case 186:
                        return output;
                    default:
                        return -1;
                }
            }
            return -1;
        }

        public static Value resource_manager_getResourceOfType(VmContext vm, string userPath, string type)
        {
            ResourceDB db = vm.resourceDatabase;
            Dictionary<string, ResourceInfo> lookup = db.fileInfo;
            if (lookup.ContainsKey(userPath))
            {
                ListImpl output = makeEmptyList(null, 2);
                ResourceInfo file = lookup[userPath];
                if (file.type == type)
                {
                    addToList(output, vm.globals.boolTrue);
                    addToList(output, buildString(vm.globals, file.internalPath));
                }
                else
                {
                    addToList(output, vm.globals.boolFalse);
                }
                return new Value(6, output);
            }
            return vm.globals.valueNull;
        }

        public static int resource_manager_populate_directory_lookup(Dictionary<string, List<string>> dirs, string path)
        {
            string[] parts = PST_StringSplit(path, "/");
            string pathBuilder = "";
            string file = "";
            int i = 0;
            while ((i < parts.Length))
            {
                file = parts[i];
                List<string> files = null;
                if (!dirs.ContainsKey(pathBuilder))
                {
                    files = new List<string>();
                    dirs[pathBuilder] = files;
                }
                else
                {
                    files = dirs[pathBuilder];
                }
                files.Add(file);
                if ((i > 0))
                {
                    pathBuilder = string.Join("", new string[] { pathBuilder,"/",file });
                }
                else
                {
                    pathBuilder = file;
                }
                i += 1;
            }
            return 0;
        }

        public static ResourceDB resourceManagerInitialize(VmGlobals globals, string manifest, string imageAtlasManifest)
        {
            Dictionary<string, List<string>> filesPerDirectoryBuilder = new Dictionary<string, List<string>>();
            Dictionary<string, ResourceInfo> fileInfo = new Dictionary<string, ResourceInfo>();
            List<Value> dataList = new List<Value>();
            string[] items = PST_StringSplit(manifest, "\n");
            ResourceInfo resourceInfo = null;
            string type = "";
            string userPath = "";
            string internalPath = "";
            string argument = "";
            bool isText = false;
            int intType = 0;
            int i = 0;
            while ((i < items.Length))
            {
                string[] itemData = PST_StringSplit(items[i], ",");
                if ((itemData.Length >= 3))
                {
                    type = itemData[0];
                    isText = "TXT" == type;
                    if (isText)
                    {
                        intType = 1;
                    }
                    else if (("IMGSH" == type || "IMG" == type))
                    {
                        intType = 2;
                    }
                    else if ("SND" == type)
                    {
                        intType = 3;
                    }
                    else if ("TTF" == type)
                    {
                        intType = 4;
                    }
                    else
                    {
                        intType = 5;
                    }
                    userPath = stringDecode(itemData[1]);
                    internalPath = itemData[2];
                    argument = "";
                    if ((itemData.Length > 3))
                    {
                        argument = stringDecode(itemData[3]);
                    }
                    resourceInfo = new ResourceInfo(userPath, internalPath, isText, type, argument);
                    fileInfo[userPath] = resourceInfo;
                    resource_manager_populate_directory_lookup(filesPerDirectoryBuilder, userPath);
                    dataList.Add(buildString(globals, userPath));
                    dataList.Add(buildInteger(globals, intType));
                    if ((internalPath != null))
                    {
                        dataList.Add(buildString(globals, internalPath));
                    }
                    else
                    {
                        dataList.Add(globals.valueNull);
                    }
                }
                i += 1;
            }
            string[] dirs = filesPerDirectoryBuilder.Keys.ToArray();
            Dictionary<string, string[]> filesPerDirectorySorted = new Dictionary<string, string[]>();
            i = 0;
            while ((i < dirs.Length))
            {
                string dir = dirs[i];
                List<string> unsortedDirs = filesPerDirectoryBuilder[dir];
                string[] dirsSorted = unsortedDirs.ToArray();
                dirsSorted = dirsSorted.OrderBy<string, string>(_PST_GEN_arg => _PST_GEN_arg).ToArray();
                filesPerDirectorySorted[dir] = dirsSorted;
                i += 1;
            }
            return new ResourceDB(filesPerDirectorySorted, fileInfo, dataList, imageAtlasManifest);
        }

        public static void reverseList(ListImpl list)
        {
            int _len = list.size;
            Value t = null;
            int i2 = 0;
            Value[] arr = list.array;
            int i = (_len >> 1);
            while ((i < _len))
            {
                i2 = (_len - i - 1);
                t = arr[i];
                arr[i] = arr[i2];
                arr[i2] = t;
                i += 1;
            }
        }

        public static InterpreterResult runInterpreter(VmContext vm, int executionContextId)
        {
            InterpreterResult result = interpret(vm, executionContextId);
            result.executionContextId = executionContextId;
            int status = result.status;
            if ((status == 1))
            {
                if (vm.executionContexts.ContainsKey(executionContextId))
                {
                    vm.executionContexts.Remove(executionContextId);
                }
                runShutdownHandlers(vm);
            }
            else if ((status == 3))
            {
                printToStdOut(vm.environment.stacktracePrefix, result.errorMessage);
                runShutdownHandlers(vm);
            }
            if ((executionContextId == 0))
            {
                result.isRootContext = true;
            }
            return result;
        }

        public static InterpreterResult runInterpreterWithFunctionPointer(VmContext vm, Value fpValue, Value[] args)
        {
            int newId = (vm.lastExecutionContextId + 1);
            vm.lastExecutionContextId = newId;
            List<Value> argList = new List<Value>();
            int i = 0;
            while ((i < args.Length))
            {
                argList.Add(args[i]);
                i += 1;
            }
            Value[] locals = new Value[0];
            int[] localsSet = new int[0];
            Value[] valueStack = new Value[100];
            valueStack[0] = fpValue;
            valueStack[1] = buildList(argList);
            StackFrame stack = new StackFrame((vm.byteCode.ops.Length - 2), 1, 0, 0, null, false, null, 0, 0, 1, 0, null, null, null);
            ExecutionContext executionContext = new ExecutionContext(newId, stack, 2, 100, valueStack, locals, localsSet, 1, 0, false, null, false, 0, null);
            vm.executionContexts[newId] = executionContext;
            return runInterpreter(vm, newId);
        }

        public static int runShutdownHandlers(VmContext vm)
        {
            while ((vm.shutdownHandlers.Count > 0))
            {
                Value handler = vm.shutdownHandlers[0];
                vm.shutdownHandlers.RemoveAt(0);
                runInterpreterWithFunctionPointer(vm, handler, new Value[0]);
            }
            return 0;
        }

        public static void setItemInList(ListImpl list, int i, Value v)
        {
            list.array[i] = v;
        }

        public static bool sortHelperIsRevOrder(string[] keyStringList, double[] keyNumList, bool isString, int indexLeft, int indexRight)
        {
            if (isString)
            {
                return (keyStringList[indexLeft].CompareTo(keyStringList[indexRight]) == 1);
            }
            return (keyNumList[indexLeft] > keyNumList[indexRight]);
        }

        public static int sortHelperSwap(string[] keyStringList, double[] keyNumList, int[] indices, bool isString, int index1, int index2)
        {
            if ((index1 == index2))
            {
                return 0;
            }
            int t = indices[index1];
            indices[index1] = indices[index2];
            indices[index2] = t;
            if (isString)
            {
                string s = keyStringList[index1];
                keyStringList[index1] = keyStringList[index2];
                keyStringList[index2] = s;
            }
            else
            {
                double n = keyNumList[index1];
                keyNumList[index1] = keyNumList[index2];
                keyNumList[index2] = n;
            }
            return 0;
        }

        public static int sortLists(ListImpl keyList, ListImpl parallelList, int[] intOutParam)
        {
            PST_IntBuffer16[0] = 0;
            int length = keyList.size;
            if ((length < 2))
            {
                return 0;
            }
            int i = 0;
            Value item = null;
            item = keyList.array[0];
            bool isString = (item.type == 5);
            string[] stringKeys = null;
            double[] numKeys = null;
            if (isString)
            {
                stringKeys = new string[length];
            }
            else
            {
                numKeys = new double[length];
            }
            int[] indices = new int[length];
            Value[] originalOrder = new Value[length];
            i = 0;
            while ((i < length))
            {
                indices[i] = i;
                originalOrder[i] = parallelList.array[i];
                item = keyList.array[i];
                switch (item.type)
                {
                    case 3:
                        if (isString)
                        {
                            PST_IntBuffer16[0] = 1;
                            return 0;
                        }
                        numKeys[i] = (double)(int)item.internalValue;
                        break;
                    case 4:
                        if (isString)
                        {
                            PST_IntBuffer16[0] = 1;
                            return 0;
                        }
                        numKeys[i] = (double)item.internalValue;
                        break;
                    case 5:
                        if (!isString)
                        {
                            PST_IntBuffer16[0] = 1;
                            return 0;
                        }
                        stringKeys[i] = (string)item.internalValue;
                        break;
                    default:
                        PST_IntBuffer16[0] = 1;
                        return 0;
                }
                i += 1;
            }
            qsortHelper(stringKeys, numKeys, indices, isString, 0, (length - 1));
            i = 0;
            while ((i < length))
            {
                parallelList.array[i] = originalOrder[indices[i]];
                i += 1;
            }
            return 0;
        }

        public static int SRandomQueuePopulate(VmGlobals globals, int seed, ListImpl queue, int size)
        {
            int sign = 1;
            int num = 0;
            while ((size > 0))
            {
                size -= 1;
                if (((seed & 2) == 0))
                {
                    sign = -1;
                }
                else
                {
                    sign = 1;
                }
                num = ((seed >> 8) & 255);
                seed = (((seed * 20077) + 12345) & 65535);
                num = ((num * 256) + ((seed >> 8) & 255));
                seed = ((((seed * 20077) + 12345)) & 65535);
                num = ((num * 256) + ((seed >> 8) & 255));
                seed = ((((seed * 20077) + 12345)) & 65535);
                num = ((num * 256) + ((seed >> 8) & 255));
                seed = ((((seed * 20077) + 12345)) & 65535);
                addToList(queue, buildInteger(globals, (sign * num)));
            }
            return seed;
        }

        public static bool stackItemIsLibrary(string stackInfo)
        {
            if ((stackInfo[0] != '['))
            {
                return false;
            }
            int cIndex = stackInfo.IndexOf(":");
            return ((cIndex > 0) && (cIndex < stackInfo.IndexOf("]")));
        }

        public static InterpreterResult startVm(VmContext vm)
        {
            return runInterpreter(vm, vm.lastExecutionContextId);
        }

        public static string stringDecode(string encoded)
        {
            if (!encoded.Contains("%"))
            {
                int length = encoded.Length;
                char per = '%';
                List<string> builder = new List<string>();
                int i = 0;
                while ((i < length))
                {
                    char c = encoded[i];
                    if (((c == per) && ((i + 2) < length)))
                    {
                        builder.Add(stringFromHex(string.Join("", new string[] { "",encoded[(i + 1)].ToString(),encoded[(i + 2)].ToString() })));
                    }
                    else
                    {
                        builder.Add("" + c.ToString());
                    }
                    i += 1;
                }
                return string.Join("", builder);
            }
            return encoded;
        }

        public static string stringFromHex(string encoded)
        {
            encoded = encoded.ToUpper();
            string hex = "0123456789ABCDEF";
            List<string> output = new List<string>();
            int length = encoded.Length;
            int a = 0;
            int b = 0;
            string c = null;
            int i = 0;
            while (((i + 1) < length))
            {
                c = "" + encoded[i].ToString();
                a = hex.IndexOf(c);
                if ((a == -1))
                {
                    return null;
                }
                c = "" + encoded[(i + 1)].ToString();
                b = hex.IndexOf(c);
                if ((b == -1))
                {
                    return null;
                }
                a = ((a * 16) + b);
                output.Add(((char) a).ToString());
                i += 2;
            }
            return string.Join("", output);
        }

        public static InterpreterResult suspendInterpreter()
        {
            return new InterpreterResult(2, null, 0.0, 0, false, "");
        }

        public static int tokenDataImpl(VmContext vm, int[] row)
        {
            List<Token>[] tokensByPc = vm.symbolData.tokenData;
            int pc = (row[0] + vm.metadata.userCodeStart);
            int line = row[1];
            int col = row[2];
            int file = row[3];
            List<Token> tokens = tokensByPc[pc];
            if ((tokens == null))
            {
                tokens = new List<Token>();
                tokensByPc[pc] = tokens;
            }
            tokens.Add(new Token(line, col, file));
            return 0;
        }

        public static List<string> tokenHelperConvertPcsToStackTraceStrings(VmContext vm, List<int> pcs)
        {
            List<Token> tokens = generateTokenListFromPcs(vm, pcs);
            string[] files = vm.symbolData.sourceCode;
            List<string> output = new List<string>();
            int i = 0;
            while ((i < tokens.Count))
            {
                Token token = tokens[i];
                if ((token == null))
                {
                    output.Add("[No stack information]");
                }
                else
                {
                    int line = token.lineIndex;
                    int col = token.colIndex;
                    string fileData = files[token.fileId];
                    string[] lines = PST_StringSplit(fileData, "\n");
                    string filename = lines[0];
                    string linevalue = lines[(line + 1)];
                    output.Add(string.Join("", new string[] { filename,", Line: ",((line + 1)).ToString(),", Col: ",((col + 1)).ToString() }));
                }
                i += 1;
            }
            return output;
        }

        public static string tokenHelperGetFileLine(VmContext vm, int fileId, int lineNum)
        {
            string sourceCode = vm.symbolData.sourceCode[fileId];
            if ((sourceCode == null))
            {
                return null;
            }
            return PST_StringSplit(sourceCode, "\n")[lineNum];
        }

        public static string tokenHelperGetFormattedPointerToToken(VmContext vm, Token token)
        {
            string line = tokenHelperGetFileLine(vm, token.fileId, (token.lineIndex + 1));
            if ((line == null))
            {
                return null;
            }
            int columnIndex = token.colIndex;
            int lineLength = line.Length;
            line = line.TrimStart();
            line = line.Replace("\t", " ");
            int offset = (lineLength - line.Length);
            columnIndex -= offset;
            string line2 = "";
            while ((columnIndex > 0))
            {
                columnIndex -= 1;
                line2 = line2 + " ";
            }
            line2 = line2 + "^";
            return string.Join("", new string[] { line,"\n",line2 });
        }

        public static bool tokenHelplerIsFilePathLibrary(VmContext vm, int fileId, string[] allFiles)
        {
            string filename = tokenHelperGetFileLine(vm, fileId, 0);
            return !filename.ToLower().EndsWith(".cry");
        }

        public static string typeInfoToString(VmContext vm, int[] typeInfo, int i)
        {
            List<string> output = new List<string>();
            typeToStringBuilder(vm, output, typeInfo, i);
            return string.Join("", output);
        }

        public static string typeToString(VmContext vm, int[] typeInfo, int i)
        {
            List<string> sb = new List<string>();
            typeToStringBuilder(vm, sb, typeInfo, i);
            return string.Join("", sb);
        }

        public static int typeToStringBuilder(VmContext vm, List<string> sb, int[] typeInfo, int i)
        {
            switch (typeInfo[i])
            {
                case -1:
                    sb.Add("void");
                    return (i + 1);
                case 0:
                    sb.Add("object");
                    return (i + 1);
                case 1:
                    sb.Add("object");
                    return (i + 1);
                case 3:
                    sb.Add("int");
                    return (i + 1);
                case 4:
                    sb.Add("float");
                    return (i + 1);
                case 2:
                    sb.Add("bool");
                    return (i + 1);
                case 5:
                    sb.Add("string");
                    return (i + 1);
                case 6:
                    sb.Add("List<");
                    i = typeToStringBuilder(vm, sb, typeInfo, (i + 1));
                    sb.Add(">");
                    return i;
                case 7:
                    sb.Add("Dictionary<");
                    i = typeToStringBuilder(vm, sb, typeInfo, (i + 1));
                    sb.Add(", ");
                    i = typeToStringBuilder(vm, sb, typeInfo, i);
                    sb.Add(">");
                    return i;
                case 8:
                    int classId = typeInfo[(i + 1)];
                    if ((classId == 0))
                    {
                        sb.Add("object");
                    }
                    else
                    {
                        ClassInfo classInfo = vm.metadata.classTable[classId];
                        sb.Add(classInfo.fullyQualifiedName);
                    }
                    return (i + 2);
                case 10:
                    sb.Add("Class");
                    return (i + 1);
                case 9:
                    int n = typeInfo[(i + 1)];
                    int optCount = typeInfo[(i + 2)];
                    i += 2;
                    sb.Add("function(");
                    List<string> ret = new List<string>();
                    i = typeToStringBuilder(vm, ret, typeInfo, i);
                    int j = 1;
                    while ((j < n))
                    {
                        if ((j > 1))
                        {
                            sb.Add(", ");
                        }
                        i = typeToStringBuilder(vm, sb, typeInfo, i);
                        j += 1;
                    }
                    if ((n == 1))
                    {
                        sb.Add("void");
                    }
                    sb.Add(" => ");
                    int optStart = (n - optCount - 1);
                    j = 0;
                    while ((j < ret.Count))
                    {
                        if ((j >= optStart))
                        {
                            sb.Add("(opt) ");
                        }
                        sb.Add(ret[j]);
                        j += 1;
                    }
                    sb.Add(")");
                    return i;
                default:
                    sb.Add("UNKNOWN");
                    return (i + 1);
            }
        }

        public static string typeToStringFromValue(VmContext vm, Value value)
        {
            List<string> sb = null;
            switch (value.type)
            {
                case 1:
                    return "null";
                case 2:
                    return "bool";
                case 3:
                    return "int";
                case 4:
                    return "float";
                case 5:
                    return "string";
                case 10:
                    return "class";
                case 8:
                    int classId = ((ObjectInstance)value.internalValue).classId;
                    ClassInfo classInfo = vm.metadata.classTable[classId];
                    return classInfo.fullyQualifiedName;
                case 6:
                    sb = new List<string>();
                    sb.Add("List<");
                    ListImpl list = (ListImpl)value.internalValue;
                    if ((list.type == null))
                    {
                        sb.Add("object");
                    }
                    else
                    {
                        typeToStringBuilder(vm, sb, list.type, 0);
                    }
                    sb.Add(">");
                    return string.Join("", sb);
                case 7:
                    DictImpl dict = (DictImpl)value.internalValue;
                    sb = new List<string>();
                    sb.Add("Dictionary<");
                    switch (dict.keyType)
                    {
                        case 3:
                            sb.Add("int");
                            break;
                        case 5:
                            sb.Add("string");
                            break;
                        case 8:
                            sb.Add("object");
                            break;
                        default:
                            sb.Add("???");
                            break;
                    }
                    sb.Add(", ");
                    if ((dict.valueType == null))
                    {
                        sb.Add("object");
                    }
                    else
                    {
                        typeToStringBuilder(vm, sb, dict.valueType, 0);
                    }
                    sb.Add(">");
                    return string.Join("", sb);
                case 9:
                    return "Function";
                default:
                    return "Unknown";
            }
        }

        public static int uint32Hack(int left, int right)
        {
            return (((left << 16)) | right);
        }

        public static InterpreterResult uncaughtExceptionResult(VmContext vm, Value exception)
        {
            return new InterpreterResult(3, unrollExceptionOutput(vm, exception), 0.0, 0, false, "");
        }

        public static string unrollExceptionOutput(VmContext vm, Value exceptionInstance)
        {
            ObjectInstance objInstance = (ObjectInstance)exceptionInstance.internalValue;
            ClassInfo classInfo = vm.metadata.classTable[objInstance.classId];
            List<int> pcs = (List<int>)objInstance.nativeData[1];
            string codeFormattedPointer = "";
            string exceptionName = classInfo.fullyQualifiedName;
            string message = valueToString(vm, objInstance.members[1]);
            List<string> trace = tokenHelperConvertPcsToStackTraceStrings(vm, pcs);
            trace.RemoveAt(trace.Count - 1);
            trace.Add("Stack Trace:");
            trace.Reverse();
            pcs.Reverse();
            bool showLibStack = vm.environment.showLibStack;
            if ((!showLibStack && !stackItemIsLibrary(trace[0])))
            {
                while (stackItemIsLibrary(trace[(trace.Count - 1)]))
                {
                    trace.RemoveAt(trace.Count - 1);
                    pcs.RemoveAt(pcs.Count - 1);
                }
            }
            List<Token> tokensAtPc = vm.symbolData.tokenData[pcs[(pcs.Count - 1)]];
            if ((tokensAtPc != null))
            {
                codeFormattedPointer = "\n\n" + tokenHelperGetFormattedPointerToToken(vm, tokensAtPc[0]);
            }
            string stackTrace = string.Join("\n", trace);
            return string.Join("", new string[] { stackTrace,codeFormattedPointer,"\n",exceptionName,": ",message });
        }

        public static ListImpl valueConcatLists(ListImpl a, ListImpl b)
        {
            int aLen = a.size;
            int bLen = b.size;
            int size = (aLen + bLen);
            ListImpl c = new ListImpl(null, size, size, new Value[size]);
            int i = 0;
            while ((i < aLen))
            {
                c.array[i] = a.array[i];
                i += 1;
            }
            i = 0;
            while ((i < bLen))
            {
                c.array[(i + aLen)] = b.array[i];
                i += 1;
            }
            c.size = c.capacity;
            return c;
        }

        public static ListImpl valueMultiplyList(ListImpl a, int n)
        {
            int _len = (a.size * n);
            ListImpl output = makeEmptyList(a.type, _len);
            if ((_len == 0))
            {
                return output;
            }
            int aLen = a.size;
            int i = 0;
            Value value = null;
            if ((aLen == 1))
            {
                value = a.array[0];
                i = 0;
                while ((i < n))
                {
                    output.array[i] = value;
                    i += 1;
                }
            }
            else
            {
                int j = 0;
                i = 0;
                while ((i < n))
                {
                    j = 0;
                    while ((j < aLen))
                    {
                        output.array[((i * aLen) + j)] = a.array[j];
                        j += 1;
                    }
                    i += 1;
                }
            }
            output.size = _len;
            return output;
        }

        public static Value[] valueStackIncreaseCapacity(ExecutionContext ec)
        {
            Value[] stack = ec.valueStack;
            int oldCapacity = stack.Length;
            int newCapacity = (oldCapacity * 2);
            Value[] newStack = new Value[newCapacity];
            int i = (oldCapacity - 1);
            while ((i >= 0))
            {
                newStack[i] = stack[i];
                i -= 1;
            }
            ec.valueStack = newStack;
            return newStack;
        }

        public static string valueToString(VmContext vm, Value wrappedValue)
        {
            int type = wrappedValue.type;
            if ((type == 1))
            {
                return "null";
            }
            if ((type == 2))
            {
                if ((bool)wrappedValue.internalValue)
                {
                    return "true";
                }
                return "false";
            }
            if ((type == 4))
            {
                string floatStr = PST_FloatToString((double)wrappedValue.internalValue);
                if (!floatStr.Contains("."))
                {
                    floatStr += ".0";
                }
                return floatStr;
            }
            if ((type == 3))
            {
                return ((int)wrappedValue.internalValue).ToString();
            }
            if ((type == 5))
            {
                return (string)wrappedValue.internalValue;
            }
            if ((type == 6))
            {
                ListImpl internalList = (ListImpl)wrappedValue.internalValue;
                string output = "[";
                int i = 0;
                while ((i < internalList.size))
                {
                    if ((i > 0))
                    {
                        output += ", ";
                    }
                    output += valueToString(vm, internalList.array[i]);
                    i += 1;
                }
                output += "]";
                return output;
            }
            if ((type == 8))
            {
                ObjectInstance objInstance = (ObjectInstance)wrappedValue.internalValue;
                int classId = objInstance.classId;
                int ptr = objInstance.objectId;
                ClassInfo classInfo = vm.metadata.classTable[classId];
                int nameId = classInfo.nameId;
                string className = vm.metadata.identifiers[nameId];
                return string.Join("", new string[] { "Instance<",className,"#",(ptr).ToString(),">" });
            }
            if ((type == 7))
            {
                DictImpl dict = (DictImpl)wrappedValue.internalValue;
                if ((dict.size == 0))
                {
                    return "{}";
                }
                string output = "{";
                List<Value> keyList = dict.keys;
                List<Value> valueList = dict.values;
                int i = 0;
                while ((i < dict.size))
                {
                    if ((i > 0))
                    {
                        output += ", ";
                    }
                    output += string.Join("", new string[] { valueToString(vm, dict.keys[i]),": ",valueToString(vm, dict.values[i]) });
                    i += 1;
                }
                output += " }";
                return output;
            }
            if ((type == 9))
            {
                FunctionPointer fp = (FunctionPointer)wrappedValue.internalValue;
                switch (fp.type)
                {
                    case 1:
                        return "<FunctionPointer>";
                    case 2:
                        return "<ClassMethodPointer>";
                    case 3:
                        return "<ClassStaticMethodPointer>";
                    case 4:
                        return "<PrimitiveMethodPointer>";
                    case 5:
                        return "<Lambda>";
                    default:
                        return "<UnknownFunctionPointer>";
                }
            }
            return "<unknown>";
        }

        public static int vm_getCurrentExecutionContextId(VmContext vm)
        {
            return vm.lastExecutionContextId;
        }

        public static int vm_suspend_context_by_id(VmContext vm, int execId, int status)
        {
            return vm_suspend_for_context(getExecutionContext(vm, execId), 1);
        }

        public static int vm_suspend_for_context(ExecutionContext ec, int status)
        {
            ec.executionStateChange = true;
            ec.executionStateChangeCommand = status;
            return 0;
        }

        public static int vm_suspend_with_status_by_id(VmContext vm, int execId, int status)
        {
            return vm_suspend_for_context(getExecutionContext(vm, execId), status);
        }

        public static void vmEnableLibStackTrace(VmContext vm)
        {
            vm.environment.showLibStack = true;
        }

        public static void vmEnvSetCommandLineArgs(VmContext vm, string[] args)
        {
            vm.environment.commandLineArgs = args;
        }

        public static VmGlobals vmGetGlobals(VmContext vm)
        {
            return vm.globals;
        }

        public static string xml_ampUnescape(string value, Dictionary<string, string> entityLookup)
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
                    string entityValue = xml_getEntity(entityCode, entityLookup);
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

        public static string xml_error(string xml, int index, string msg)
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

        public static string xml_getEntity(string code, Dictionary<string, string> entityLookup)
        {
            if (entityLookup.ContainsKey(code))
            {
                return entityLookup[code];
            }
            return null;
        }

        public static bool xml_isNext(string xml, int[] indexPtr, string value)
        {
            return PST_SubstringIsEqualTo(xml, indexPtr[0], value);
        }

        public static Value xml_parse(VmContext vm, string xml)
        {
            Dictionary<string, string> entityLookup = new Dictionary<string, string>();
            entityLookup["amp"] = "&";
            entityLookup["lt"] = "<";
            entityLookup["gt"] = ">";
            entityLookup["quot"] = "\"";
            entityLookup["apos"] = "'";
            Dictionary<int, int> stringEnders = new Dictionary<int, int>();
            stringEnders[((int)(' '))] = 1;
            stringEnders[((int)('"'))] = 1;
            stringEnders[((int)('\''))] = 1;
            stringEnders[((int)('<'))] = 1;
            stringEnders[((int)('>'))] = 1;
            stringEnders[((int)('\t'))] = 1;
            stringEnders[((int)('\r'))] = 1;
            stringEnders[((int)('\n'))] = 1;
            stringEnders[((int)('/'))] = 1;
            List<Value> output = new List<Value>();
            string errMsg = xml_parseImpl(vm, xml, new int[1], output, entityLookup, stringEnders);
            if ((errMsg != null))
            {
                return buildString(vm.globals, errMsg);
            }
            return buildList(output);
        }

        public static string xml_parseElement(VmContext vm, string xml, int[] indexPtr, List<Value> output, Dictionary<string, string> entityLookup, Dictionary<int, int> stringEnders)
        {
            int length = xml.Length;
            List<Value> attributeKeys = new List<Value>();
            List<Value> attributeValues = new List<Value>();
            List<Value> children = new List<Value>();
            List<Value> element = new List<Value>();
            string error = null;
            if (!xml_popIfPresent(xml, indexPtr, "<"))
            {
                return xml_error(xml, indexPtr[0], "Expected: '<'");
            }
            string name = xml_popName(xml, indexPtr);
            xml_skipWhitespace(xml, indexPtr);
            bool hasClosingTag = true;
            while (true)
            {
                if ((indexPtr[0] >= length))
                {
                    return xml_error(xml, length, "Unexpected EOF");
                }
                if (xml_popIfPresent(xml, indexPtr, ">"))
                {
                    break;
                }
                if (xml_popIfPresent(xml, indexPtr, "/>"))
                {
                    hasClosingTag = false;
                    break;
                }
                string key = xml_popName(xml, indexPtr);
                if ((key.Length == 0))
                {
                    return xml_error(xml, indexPtr[0], "Expected attribute name.");
                }
                attributeKeys.Add(buildString(vm.globals, key));
                xml_skipWhitespace(xml, indexPtr);
                if (!xml_popIfPresent(xml, indexPtr, "="))
                {
                    return xml_error(xml, indexPtr[0], "Expected: '='");
                }
                xml_skipWhitespace(xml, indexPtr);
                error = xml_popString(vm, xml, indexPtr, attributeValues, entityLookup, stringEnders);
                if ((error != null))
                {
                    return error;
                }
                xml_skipWhitespace(xml, indexPtr);
            }
            if (hasClosingTag)
            {
                string close = string.Join("", new string[] { "</",name,">" });
                while (!xml_popIfPresent(xml, indexPtr, close))
                {
                    if (xml_isNext(xml, indexPtr, "</"))
                    {
                        error = xml_error(xml, (indexPtr[0] - 2), "Unexpected close tag.");
                    }
                    else if (xml_isNext(xml, indexPtr, "<!--"))
                    {
                        error = xml_skipComment(xml, indexPtr);
                    }
                    else if (xml_isNext(xml, indexPtr, "<"))
                    {
                        error = xml_parseElement(vm, xml, indexPtr, children, entityLookup, stringEnders);
                    }
                    else
                    {
                        error = xml_parseText(vm, xml, indexPtr, children, entityLookup);
                    }
                    if (((error == null) && (indexPtr[0] >= length)))
                    {
                        error = xml_error(xml, length, "Unexpected EOF. Unclosed tag.");
                    }
                    if ((error != null))
                    {
                        return error;
                    }
                }
            }
            element.Add(vm.globalTrue);
            element.Add(buildString(vm.globals, name));
            element.Add(buildList(attributeKeys));
            element.Add(buildList(attributeValues));
            element.Add(buildList(children));
            output.Add(buildList(element));
            return null;
        }

        public static string xml_parseImpl(VmContext vm, string input, int[] indexPtr, List<Value> output, Dictionary<string, string> entityLookup, Dictionary<int, int> stringEnders)
        {
            indexPtr[0] = 0;
            xml_skipWhitespace(input, indexPtr);
            if (xml_popIfPresent(input, indexPtr, "<?xml"))
            {
                int newBegin = input.IndexOf("?>");
                if ((newBegin == -1))
                {
                    return xml_error(input, (indexPtr[0] - 5), "XML Declaration is not closed.");
                }
                indexPtr[0] = (newBegin + 2);
            }
            string error = xml_skipStuff(input, indexPtr);
            if ((error != null))
            {
                return error;
            }
            error = xml_parseElement(vm, input, indexPtr, output, entityLookup, stringEnders);
            if ((error != null))
            {
                return error;
            }
            xml_skipStuff(input, indexPtr);
            if ((indexPtr[0] != input.Length))
            {
                return xml_error(input, indexPtr[0], "Unexpected text.");
            }
            return null;
        }

        public static string xml_parseText(VmContext vm, string xml, int[] indexPtr, List<Value> output, Dictionary<string, string> entityLookup)
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
                    textValue = xml_ampUnescape(textValue, entityLookup);
                }
                List<Value> textElement = new List<Value>();
                textElement.Add(vm.globalFalse);
                textElement.Add(buildString(vm.globals, textValue));
                output.Add(buildList(textElement));
            }
            return null;
        }

        public static bool xml_popIfPresent(string xml, int[] indexPtr, string s)
        {
            if (PST_SubstringIsEqualTo(xml, indexPtr[0], s))
            {
                indexPtr[0] = (indexPtr[0] + s.Length);
                return true;
            }
            return false;
        }

        public static string xml_popName(string xml, int[] indexPtr)
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

        public static string xml_popString(VmContext vm, string xml, int[] indexPtr, List<Value> attributeValueOut, Dictionary<string, string> entityLookup, Dictionary<int, int> stringEnders)
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
                output = xml_ampUnescape(output, entityLookup);
            }
            attributeValueOut.Add(buildString(vm.globals, output));
            return null;
        }

        public static string xml_skipComment(string xml, int[] indexPtr)
        {
            if (xml_popIfPresent(xml, indexPtr, "<!--"))
            {
                int i = xml.IndexOf("-->", indexPtr[0]);
                if ((i == -1))
                {
                    return xml_error(xml, (indexPtr[0] - 4), "Unclosed comment.");
                }
                indexPtr[0] = (i + 3);
            }
            return null;
        }

        public static string xml_skipStuff(string xml, int[] indexPtr)
        {
            int index = (indexPtr[0] - 1);
            while ((index < indexPtr[0]))
            {
                index = indexPtr[0];
                xml_skipWhitespace(xml, indexPtr);
                string error = xml_skipComment(xml, indexPtr);
                if ((error != null))
                {
                    return error;
                }
            }
            return null;
        }

        public static int xml_skipWhitespace(string xml, int[] indexPtr)
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
