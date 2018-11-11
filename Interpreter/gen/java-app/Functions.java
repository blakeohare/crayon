private static java.util.Random PST_random = new java.util.Random();
private static final Object[] PST_emptyArrayObject = new Object[0];
private static final String[] PST_emptyArrayString = new String[0];
@SuppressWarnings("rawtypes")
private static final ArrayList[] PST_emptyArrayList = new ArrayList[0];
@SuppressWarnings("rawtypes")
private static final HashMap[] PST_emptyArrayMap = new HashMap[0];

private static final int[] PST_intBuffer16 = new int[16];
private static final double[] PST_floatBuffer16 = new double[16];
private static final String[] PST_stringBuffer16 = new String[16];

private static final java.nio.charset.Charset UTF8 = java.nio.charset.Charset.forName("UTF-8");
private static String PST_base64ToString(String b64Value) {
  int inputLength = b64Value.length();

  if (inputLength == 0) return "";
  while (inputLength > 0 && b64Value.charAt(inputLength - 1) == '=') {
    b64Value = b64Value.substring(0, --inputLength);
  }
  int bitsOfData = inputLength * 6;
  int outputLength = bitsOfData / 8;

  byte[] buffer = new byte[outputLength];
  char c;
  int charValue;
  for (int i = 0; i < inputLength; ++i) {
    c = b64Value.charAt(i);
    charValue = -1;
    switch (c) {
      case '=': break;
      case '+': charValue = 62;
      case '/': charValue = 63;
      default:
        if (c >= 'A' && c <= 'Z') {
          charValue = c - 'A';
        } else if (c >= 'a' && c <= 'z') {
          charValue = c - 'a' + 26;
        } else if (c >= '0' && c <= '9') {
          charValue = c - '0' + 52;
        }
        break;
    }

    if (charValue != -1) {
      int bitOffset = i * 6;
      int targetIndex = bitOffset / 8;
      int bitWithinByte = bitOffset % 8;
      switch (bitOffset % 8) {
        case 0:
          buffer[targetIndex] |= charValue << 2;
          break;
        case 2:
          buffer[targetIndex] |= charValue;
          break;
        case 4:
          buffer[targetIndex] |= charValue >> 2;
          if (targetIndex + 1 < outputLength)
            buffer[targetIndex + 1] |= charValue << 6;
          break;
        case 6:
          buffer[targetIndex] |= charValue >> 4;
          if (targetIndex + 1 < outputLength)
            buffer[targetIndex + 1] |= charValue << 4;
          break;
      }
    }
  }
  return new String(buffer, UTF8);
}

private static int[] PST_convertIntegerSetToArray(java.util.Set<Integer> original) {
  int[] output = new int[original.size()];
  int i = 0;
  for (int value : original) {
    output[i++] = value;
  }
  return output;
}

private static String[] PST_convertStringSetToArray(java.util.Set<String> original) {
  String[] output = new String[original.size()];
  int i = 0;
  for (String value : original) {
    output[i++] = value;
  }
  return output;
}

private static boolean PST_isValidInteger(String value) {
  try {
    Integer.parseInt(value);
  } catch (NumberFormatException nfe) {
    return false;
  }
  return true;
}

private static String PST_joinChars(ArrayList<Character> chars) {
  char[] output = new char[chars.size()];
  for (int i = output.length - 1; i >= 0; --i) {
    output[i] = chars.get(i);
  }
  return String.copyValueOf(output);
}

private static String PST_joinList(String sep, ArrayList<String> items) {
  int length = items.size();
  if (length < 2) {
    if (length == 0) return "";
    return items.get(0);
  }

  boolean useSeparator = sep.length() > 0;
  StringBuilder sb = new StringBuilder(useSeparator ? (length * 2 - 1) : length);
  sb.append(items.get(0));
  if (useSeparator) {
    for (int i = 1; i < length; ++i) {
      sb.append(sep);
      sb.append(items.get(i));
    }
  } else {
    for (int i = 1; i < length; ++i) {
      sb.append(items.get(i));
    }
  }

  return sb.toString();
}

private static <T> T PST_listPop(ArrayList<T> list) {
  return list.remove(list.size() - 1);
}

private static String[] PST_literalStringSplit(String original, String sep) {
  ArrayList<String> output = new ArrayList<String>();
  ArrayList<String> currentPiece = new ArrayList<String>();
  int length = original.length();
  int sepLength = sep.length();
  char firstSepChar = sep.charAt(0);
  char c;
  int j;
  boolean match;
  for (int i = 0; i < length; ++i) {
    c = original.charAt(i);
    match = false;
    if (c == firstSepChar) {
      match = true;
      for (j = 1; j < sepLength; ++j) {
        if (i + j < length ) {
          if (sep.charAt(j) != original.charAt(i + j)) {
            match = false;
            break;
          }
        } else {
          match = false;
        }
      }
    }

    if (match) {
      output.add(PST_joinList("", currentPiece));
      currentPiece.clear();
      i += sepLength - 1;
    } else {
      currentPiece.add("" + c);
    }
  }
  output.add(PST_joinList("", currentPiece));
  return output.toArray(new String[output.size()]);
}

private static String PST_reverseString(String original) {
  char[] output = original.toCharArray();
  int length = output.length;
  int lengthMinusOne = length - 1;
  char c;
  for (int i = length / 2 - 1; i >= 0; --i) {
    c = output[i];
    output[i] = output[lengthMinusOne - i];
    output[lengthMinusOne] = c;
  }
  return String.copyValueOf(output);
}

private static boolean PST_checkStringInString(String haystack, int index, String expectedValue) {
  int evLength = expectedValue.length();
  if (evLength + index > haystack.length()) return false;
  if (evLength == 0) return true;
  if (expectedValue.charAt(0) != haystack.charAt(index)) return false;
  if (expectedValue.charAt(evLength - 1) != haystack.charAt(index + evLength - 1)) return false;
  if (evLength <= 2) return true;
  for (int i = evLength - 2; i > 1; --i) {
    if (expectedValue.charAt(i) != haystack.charAt(index + i)) return false;
  }
  return true;
}

private static String PST_trimSide(String value, boolean isLeft) {
  int i = isLeft ? 0 : value.length() - 1;
  int end = isLeft ? value.length() : -1;
  int step = isLeft ? 1 : -1;
  char c;
  boolean trimming = true;
  while (trimming && i != end) {
    c = value.charAt(i);
    switch (c) {
      case ' ':
      case '\n':
      case '\t':
      case '\r':
        i += step;
        break;
      default:
        trimming = false;
        break;
    }
  }

  return isLeft ? value.substring(i) : value.substring(0, i + 1);
}

private static void PST_parseFloatOrReturnNull(double[] outParam, String rawValue) {
  try {
    outParam[1] = Double.parseDouble(rawValue);
    outParam[0] = 1;
  } catch (NumberFormatException nfe) {
    outParam[0] = -1;
  }
}

private static <T> ArrayList<T> PST_multiplyList(ArrayList<T> list, int n) {
  int len = list.size();
  ArrayList<T> output = new ArrayList<T>(len * n);
  if (len > 0) {
    if (len == 1) {
      T t = list.get(0);
      while (n --> 0) {
        output.add(t);
      }
    } else {
      while (n --> 0) {
        output.addAll(list);
      }
    }
  }
  return output;
}

private static <T> ArrayList<T> PST_concatLists(ArrayList<T> a, ArrayList<T> b) {
  ArrayList<T> output = new ArrayList(a.size() + b.size());
  output.addAll(a);
  output.addAll(b);
  return output;
}

private static <T> void PST_listShuffle(ArrayList<T> list) {
  int len = list.size();
  for (int i = len - 1; i >= 0; --i) {
    int ti = PST_random.nextInt(len);
    if (ti != i) {
      T t = list.get(ti);
      list.set(ti, list.get(i));
      list.set(i, t);
    }
  }
}

private static boolean[] PST_listToArrayBool(ArrayList<Boolean> list) {
  int length = list.size();
  boolean[] output = new boolean[length];
  for (int i = 0; i < length; ++i) output[i] = list.get(i);
  return output;
}

private static byte[] PST_listToArrayByte(ArrayList<Byte> list) {
  int length = list.size();
  byte[] output = new byte[length];
  for (int i = 0; i < length; ++i) output[i] = list.get(i);
  return output;
}

private static int[] PST_listToArrayInt(ArrayList<Integer> list) {
  int length = list.size();
  int[] output = new int[length];
  for (int i = 0; i < length; ++i) output[i] = list.get(i);
  return output;
}

private static double[] PST_listToArrayDouble(ArrayList<Double> list) {
  int length = list.size();
  double[] output = new double[length];
  for (int i = 0; i < length; ++i) output[i] = list.get(i);
  return output;
}

private static char[] PST_listToArrayChar(ArrayList<Character> list) {
  int length = list.size();
  char[] output = new char[length];
  for (int i = 0; i < length; ++i) output[i] = list.get(i);
  return output;
}

private static int[] PST_sortedCopyOfIntArray(int[] nums) {
  int[] output = java.util.Arrays.copyOf(nums, nums.length);
  java.util.Arrays.sort(output);
  return output;
}

private static String[] PST_sortedCopyOfStringArray(String[] values) {
  String[] output = java.util.Arrays.copyOf(values, values.length);
  java.util.Arrays.sort(output);
  return output;
}

public static int v_addLiteralImpl(VmContext v_vm, int[] v_row, String v_stringArg) {
  VmGlobals v_g = v_vm.globals;
  int v_type = v_row[0];
  if ((v_type == 1)) {
    v_vm.metadata.literalTableBuilder.add(v_g.valueNull);
  } else {
    if ((v_type == 2)) {
      v_vm.metadata.literalTableBuilder.add(v_buildBoolean(v_g, (v_row[1] == 1)));
    } else {
      if ((v_type == 3)) {
        v_vm.metadata.literalTableBuilder.add(v_buildInteger(v_g, v_row[1]));
      } else {
        if ((v_type == 4)) {
          v_vm.metadata.literalTableBuilder.add(v_buildFloat(v_g, Double.parseDouble(v_stringArg)));
        } else {
          if ((v_type == 5)) {
            v_vm.metadata.literalTableBuilder.add(v_buildCommonString(v_g, v_stringArg));
          } else {
            if ((v_type == 9)) {
              int v_index = v_vm.metadata.literalTableBuilder.size();
              v_vm.metadata.literalTableBuilder.add(v_buildCommonString(v_g, v_stringArg));
              v_vm.metadata.invFunctionNameLiterals.put(v_stringArg, v_index);
            } else {
              if ((v_type == 10)) {
                org.crayonlang.interpreter.structs.ClassValue v_cv = new org.crayonlang.interpreter.structs.ClassValue(false, v_row[1]);
                v_vm.metadata.literalTableBuilder.add(new Value(10, v_cv));
              }
            }
          }
        }
      }
    }
  }
  return 0;
}

public static int v_addNameImpl(VmContext v_vm, String v_nameValue) {
  int v_index = v_vm.metadata.identifiersBuilder.size();
  v_vm.metadata.invIdentifiers.put(v_nameValue, v_index);
  v_vm.metadata.identifiersBuilder.add(v_nameValue);
  if ("length".equals(v_nameValue)) {
    v_vm.metadata.lengthId = v_index;
  }
  return 0;
}

public static void v_addToList(ListImpl v_list, Value v_item) {
  if ((v_list.size == v_list.capacity)) {
    v_increaseListCapacity(v_list);
  }
  v_list.array[v_list.size] = v_item;
  v_list.size += 1;
}

public static int v_applyDebugSymbolData(VmContext v_vm, int[] v_opArgs, String v_stringData, FunctionInfo v_recentlyDefinedFunction) {
  return 0;
}

public static Value v_buildBoolean(VmGlobals v_g, boolean v_value) {
  if (v_value) {
    return v_g.boolTrue;
  }
  return v_g.boolFalse;
}

public static Value v_buildCommonString(VmGlobals v_g, String v_s) {
  Value v_value = null;
  Value dictLookup0 = v_g.commonStrings.get(v_s);
  v_value = dictLookup0 == null ? (v_g.commonStrings.containsKey(v_s) ? null : (null)) : dictLookup0;
  if ((v_value == null)) {
    v_value = v_buildString(v_g, v_s);
    v_g.commonStrings.put(v_s, v_value);
  }
  return v_value;
}

public static Value v_buildFloat(VmGlobals v_g, double v_value) {
  if ((v_value == 0.0)) {
    return v_g.floatZero;
  }
  if ((v_value == 1.0)) {
    return v_g.floatOne;
  }
  return new Value(4, v_value);
}

public static Value v_buildInteger(VmGlobals v_g, int v_num) {
  if ((v_num < 0)) {
    if ((v_num > -257)) {
      return v_g.negativeIntegers[-v_num];
    }
  } else {
    if ((v_num < 2049)) {
      return v_g.positiveIntegers[v_num];
    }
  }
  return new Value(v_num);
}

public static Value v_buildList(ArrayList<Value> v_valueList) {
  return v_buildListWithType(null, v_valueList);
}

public static Value v_buildListWithType(int[] v_type, ArrayList<Value> v_valueList) {
  int v_len = v_valueList.size();
  ListImpl v_output = v_makeEmptyList(v_type, v_len);
  int v_i = 0;
  while ((v_i < v_len)) {
    v_output.array[v_i] = v_valueList.get(v_i);
    v_i += 1;
  }
  v_output.size = v_len;
  return new Value(6, v_output);
}

public static Value v_buildNull(VmGlobals v_globals) {
  return v_globals.valueNull;
}

public static PlatformRelayObject v_buildRelayObj(int v_type, int v_iarg1, int v_iarg2, int v_iarg3, double v_farg1, String v_sarg1) {
  return new PlatformRelayObject(v_type, v_iarg1, v_iarg2, v_iarg3, v_farg1, v_sarg1);
}

public static Value v_buildString(VmGlobals v_g, String v_s) {
  if ((v_s.length() == 0)) {
    return v_g.stringEmpty;
  }
  return new Value(5, v_s);
}

public static Value v_buildStringDictionary(VmGlobals v_globals, String[] v_stringKeys, Value[] v_values) {
  int v_size = v_stringKeys.length;
  DictImpl v_d = new DictImpl(v_size, 5, 0, null, new HashMap<Integer, Integer>(), new HashMap<String, Integer>(), new ArrayList<Value>(), new ArrayList<Value>());
  String v_k = null;
  int v_i = 0;
  while ((v_i < v_size)) {
    v_k = v_stringKeys[v_i];
    if (v_d.stringToIndex.containsKey(v_k)) {
      v_d.values.set(v_d.stringToIndex.get(v_k), v_values[v_i]);
    } else {
      v_d.stringToIndex.put(v_k, v_d.values.size());
      v_d.values.add(v_values[v_i]);
      v_d.keys.add(v_buildString(v_globals, v_k));
    }
    v_i += 1;
  }
  v_d.size = v_d.values.size();
  return new Value(7, v_d);
}

public static boolean v_canAssignGenericToGeneric(VmContext v_vm, int[] v_gen1, int v_gen1Index, int[] v_gen2, int v_gen2Index, int[] v_newIndexOut) {
  if ((v_gen2 == null)) {
    return true;
  }
  if ((v_gen1 == null)) {
    return false;
  }
  int v_t1 = v_gen1[v_gen1Index];
  int v_t2 = v_gen2[v_gen2Index];
  switch (v_t1) {
    case 0:
      v_newIndexOut[0] = (v_gen1Index + 1);
      v_newIndexOut[1] = (v_gen2Index + 2);
      return (v_t2 == v_t1);
    case 1:
      v_newIndexOut[0] = (v_gen1Index + 1);
      v_newIndexOut[1] = (v_gen2Index + 2);
      return (v_t2 == v_t1);
    case 2:
      v_newIndexOut[0] = (v_gen1Index + 1);
      v_newIndexOut[1] = (v_gen2Index + 2);
      return (v_t2 == v_t1);
    case 4:
      v_newIndexOut[0] = (v_gen1Index + 1);
      v_newIndexOut[1] = (v_gen2Index + 2);
      return (v_t2 == v_t1);
    case 5:
      v_newIndexOut[0] = (v_gen1Index + 1);
      v_newIndexOut[1] = (v_gen2Index + 2);
      return (v_t2 == v_t1);
    case 10:
      v_newIndexOut[0] = (v_gen1Index + 1);
      v_newIndexOut[1] = (v_gen2Index + 2);
      return (v_t2 == v_t1);
    case 3:
      v_newIndexOut[0] = (v_gen1Index + 1);
      v_newIndexOut[1] = (v_gen2Index + 2);
      return ((v_t2 == 3) || (v_t2 == 4));
    case 8:
      v_newIndexOut[0] = (v_gen1Index + 1);
      v_newIndexOut[1] = (v_gen2Index + 2);
      if ((v_t2 != 8)) {
        return false;
      }
      int v_c1 = v_gen1[(v_gen1Index + 1)];
      int v_c2 = v_gen2[(v_gen2Index + 1)];
      if ((v_c1 == v_c2)) {
        return true;
      }
      return v_isClassASubclassOf(v_vm, v_c1, v_c2);
    case 6:
      if ((v_t2 != 6)) {
        return false;
      }
      return v_canAssignGenericToGeneric(v_vm, v_gen1, (v_gen1Index + 1), v_gen2, (v_gen2Index + 1), v_newIndexOut);
    case 7:
      if ((v_t2 != 7)) {
        return false;
      }
      if (!v_canAssignGenericToGeneric(v_vm, v_gen1, (v_gen1Index + 1), v_gen2, (v_gen2Index + 1), v_newIndexOut)) {
        return false;
      }
      return v_canAssignGenericToGeneric(v_vm, v_gen1, v_newIndexOut[0], v_gen2, v_newIndexOut[1], v_newIndexOut);
    case 9:
      if ((v_t2 != 9)) {
        return false;
      }
      return false;
    default:
      return false;
  }
}

public static Value v_canAssignTypeToGeneric(VmContext v_vm, Value v_value, int[] v_generics, int v_genericIndex) {
  switch (v_value.type) {
    case 1:
      switch (v_generics[v_genericIndex]) {
        case 5:
          return v_value;
        case 8:
          return v_value;
        case 10:
          return v_value;
        case 9:
          return v_value;
        case 6:
          return v_value;
        case 7:
          return v_value;
      }
      return null;
    case 2:
      if ((v_generics[v_genericIndex] == v_value.type)) {
        return v_value;
      }
      return null;
    case 5:
      if ((v_generics[v_genericIndex] == v_value.type)) {
        return v_value;
      }
      return null;
    case 10:
      if ((v_generics[v_genericIndex] == v_value.type)) {
        return v_value;
      }
      return null;
    case 3:
      if ((v_generics[v_genericIndex] == 3)) {
        return v_value;
      }
      if ((v_generics[v_genericIndex] == 4)) {
        return v_buildFloat(v_vm.globals, (0.0 + v_value.intValue));
      }
      return null;
    case 4:
      if ((v_generics[v_genericIndex] == 4)) {
        return v_value;
      }
      return null;
    case 6:
      ListImpl v_list = ((ListImpl) v_value.internalValue);
      int[] v_listType = v_list.type;
      v_genericIndex += 1;
      if ((v_listType == null)) {
        if (((v_generics[v_genericIndex] == 1) || (v_generics[v_genericIndex] == 0))) {
          return v_value;
        }
        return null;
      }
      int v_i = 0;
      while ((v_i < v_listType.length)) {
        if ((v_listType[v_i] != v_generics[(v_genericIndex + v_i)])) {
          return null;
        }
        v_i += 1;
      }
      return v_value;
    case 7:
      DictImpl v_dict = ((DictImpl) v_value.internalValue);
      int v_j = v_genericIndex;
      switch (v_dict.keyType) {
        case 3:
          if ((v_generics[1] == v_dict.keyType)) {
            v_j += 2;
          } else {
            return null;
          }
          break;
        case 5:
          if ((v_generics[1] == v_dict.keyType)) {
            v_j += 2;
          } else {
            return null;
          }
          break;
        case 8:
          if ((v_generics[1] == 8)) {
            v_j += 3;
          } else {
            return null;
          }
          break;
      }
      int[] v_valueType = v_dict.valueType;
      if ((v_valueType == null)) {
        if (((v_generics[v_j] == 0) || (v_generics[v_j] == 1))) {
          return v_value;
        }
        return null;
      }
      int v_k = 0;
      while ((v_k < v_valueType.length)) {
        if ((v_valueType[v_k] != v_generics[(v_j + v_k)])) {
          return null;
        }
        v_k += 1;
      }
      return v_value;
    case 8:
      if ((v_generics[v_genericIndex] == 8)) {
        int v_targetClassId = v_generics[(v_genericIndex + 1)];
        int v_givenClassId = (((ObjectInstance) v_value.internalValue)).classId;
        if ((v_targetClassId == v_givenClassId)) {
          return v_value;
        }
        if (v_isClassASubclassOf(v_vm, v_givenClassId, v_targetClassId)) {
          return v_value;
        }
      }
      return null;
  }
  return null;
}

public static double v_canonicalizeAngle(double v_a) {
  double v_twopi = 6.28318530717958;
  v_a = (v_a % v_twopi);
  if ((v_a < 0)) {
    v_a += v_twopi;
  }
  return v_a;
}

public static int v_canonicalizeListSliceArgs(int[] v_outParams, Value v_beginValue, Value v_endValue, int v_beginIndex, int v_endIndex, int v_stepAmount, int v_length, boolean v_isForward) {
  if ((v_beginValue == null)) {
    if (v_isForward) {
      v_beginIndex = 0;
    } else {
      v_beginIndex = (v_length - 1);
    }
  }
  if ((v_endValue == null)) {
    if (v_isForward) {
      v_endIndex = v_length;
    } else {
      v_endIndex = (-1 - v_length);
    }
  }
  if ((v_beginIndex < 0)) {
    v_beginIndex += v_length;
  }
  if ((v_endIndex < 0)) {
    v_endIndex += v_length;
  }
  if (((v_beginIndex == 0) && (v_endIndex == v_length) && (v_stepAmount == 1))) {
    return 2;
  }
  if (v_isForward) {
    if ((v_beginIndex >= v_length)) {
      return 0;
    }
    if ((v_beginIndex < 0)) {
      return 3;
    }
    if ((v_endIndex < v_beginIndex)) {
      return 4;
    }
    if ((v_beginIndex == v_endIndex)) {
      return 0;
    }
    if ((v_endIndex > v_length)) {
      v_endIndex = v_length;
    }
  } else {
    if ((v_beginIndex < 0)) {
      return 0;
    }
    if ((v_beginIndex >= v_length)) {
      return 3;
    }
    if ((v_endIndex > v_beginIndex)) {
      return 4;
    }
    if ((v_beginIndex == v_endIndex)) {
      return 0;
    }
    if ((v_endIndex < -1)) {
      v_endIndex = -1;
    }
  }
  v_outParams[0] = v_beginIndex;
  v_outParams[1] = v_endIndex;
  return 1;
}

public static String v_classIdToString(VmContext v_vm, int v_classId) {
  return v_vm.metadata.classTable[v_classId].fullyQualifiedName;
}

public static int v_clearList(ListImpl v_a) {
  int v_i = (v_a.size - 1);
  while ((v_i >= 0)) {
    v_a.array[v_i] = null;
    v_i -= 1;
  }
  v_a.size = 0;
  return 0;
}

public static DictImpl v_cloneDictionary(DictImpl v_original, DictImpl v_clone) {
  int v_type = v_original.keyType;
  int v_i = 0;
  int v_size = v_original.size;
  int v_kInt = 0;
  String v_kString = null;
  if ((v_clone == null)) {
    v_clone = new DictImpl(0, v_type, v_original.keyClassId, v_original.valueType, new HashMap<Integer, Integer>(), new HashMap<String, Integer>(), new ArrayList<Value>(), new ArrayList<Value>());
    if ((v_type == 5)) {
      while ((v_i < v_size)) {
        v_clone.stringToIndex.put(((String) v_original.keys.get(v_i).internalValue), v_i);
        v_i += 1;
      }
    } else {
      while ((v_i < v_size)) {
        if ((v_type == 8)) {
          v_kInt = (((ObjectInstance) v_original.keys.get(v_i).internalValue)).objectId;
        } else {
          v_kInt = v_original.keys.get(v_i).intValue;
        }
        v_clone.intToIndex.put(v_kInt, v_i);
        v_i += 1;
      }
    }
    v_i = 0;
    while ((v_i < v_size)) {
      v_clone.keys.add(v_original.keys.get(v_i));
      v_clone.values.add(v_original.values.get(v_i));
      v_i += 1;
    }
  } else {
    v_i = 0;
    while ((v_i < v_size)) {
      if ((v_type == 5)) {
        v_kString = ((String) v_original.keys.get(v_i).internalValue);
        if (v_clone.stringToIndex.containsKey(v_kString)) {
          v_clone.values.set(v_clone.stringToIndex.get(v_kString), v_original.values.get(v_i));
        } else {
          v_clone.stringToIndex.put(v_kString, v_clone.values.size());
          v_clone.values.add(v_original.values.get(v_i));
          v_clone.keys.add(v_original.keys.get(v_i));
        }
      } else {
        if ((v_type == 3)) {
          v_kInt = v_original.keys.get(v_i).intValue;
        } else {
          v_kInt = (((ObjectInstance) v_original.keys.get(v_i).internalValue)).objectId;
        }
        if (v_clone.intToIndex.containsKey(v_kInt)) {
          v_clone.values.set(v_clone.intToIndex.get(v_kInt), v_original.values.get(v_i));
        } else {
          v_clone.intToIndex.put(v_kInt, v_clone.values.size());
          v_clone.values.add(v_original.values.get(v_i));
          v_clone.keys.add(v_original.keys.get(v_i));
        }
      }
      v_i += 1;
    }
  }
  v_clone.size = (v_clone.intToIndex.size() + v_clone.stringToIndex.size());
  return v_clone;
}

public static int[] v_createInstanceType(int v_classId) {
  int[] v_o = new int[2];
  v_o[0] = 8;
  v_o[1] = v_classId;
  return v_o;
}

public static VmContext v_createVm(String v_rawByteCode, String v_resourceManifest) {
  VmGlobals v_globals = v_initializeConstantValues();
  ResourceDB v_resources = v_resourceManagerInitialize(v_globals, v_resourceManifest);
  Code v_byteCode = v_initializeByteCode(v_rawByteCode);
  Value[] v_localsStack = new Value[10];
  int[] v_localsStackSet = new int[10];
  int v_i = 0;
  v_i = (v_localsStack.length - 1);
  while ((v_i >= 0)) {
    v_localsStack[v_i] = null;
    v_localsStackSet[v_i] = 0;
    v_i -= 1;
  }
  StackFrame v_stack = new StackFrame(0, 1, 0, 0, null, false, null, 0, 0, 1, 0, null, null, null);
  ExecutionContext v_executionContext = new ExecutionContext(0, v_stack, 0, 100, new Value[100], v_localsStack, v_localsStackSet, 1, 0, false, null, false, 0, null);
  HashMap<Integer, ExecutionContext> v_executionContexts = new HashMap<Integer, ExecutionContext>();
  v_executionContexts.put(0, v_executionContext);
  VmContext v_vm = new VmContext(v_executionContexts, v_executionContext.id, v_byteCode, new SymbolData(new ArrayList[v_byteCode.ops.length], null, new ArrayList<String>(), null, null, new HashMap<Integer, ArrayList<String>>(), new HashMap<Integer, ArrayList<String>>()), new VmMetadata(null, new ArrayList<String>(), new HashMap<String, Integer>(), null, new ArrayList<Value>(), null, new ArrayList<HashMap<Integer, Integer>>(), null, new ArrayList<HashMap<String, Integer>>(), new ClassInfo[100], new FunctionInfo[100], new HashMap<Integer, FunctionInfo>(), null, new HashMap<Integer, java.lang.reflect.Method>(), -1, new int[10], 0, null, null, new MagicNumbers(0, 0, 0), new HashMap<String, Integer>(), new HashMap<Integer, HashMap<Integer, Integer>>(), null), 0, false, new ArrayList<Integer>(), null, v_resources, new ArrayList<Value>(), new VmEnvironment(new String[0], false, null, null), v_globals, v_globals.valueNull, v_globals.boolTrue, v_globals.boolFalse);
  return v_vm;
}

public static int v_debuggerClearBreakpoint(VmContext v_vm, int v_id) {
  return 0;
}

public static int v_debuggerFindPcForLine(VmContext v_vm, String v_path, int v_line) {
  return -1;
}

public static int v_debuggerSetBreakpoint(VmContext v_vm, String v_path, int v_line) {
  return -1;
}

public static boolean v_debugSetStepOverBreakpoint(VmContext v_vm) {
  return false;
}

public static int v_defOriginalCodeImpl(VmContext v_vm, int[] v_row, String v_fileContents) {
  int v_fileId = v_row[0];
  ArrayList<String> v_codeLookup = v_vm.symbolData.sourceCodeBuilder;
  while ((v_codeLookup.size() <= v_fileId)) {
    v_codeLookup.add(null);
  }
  v_codeLookup.set(v_fileId, v_fileContents);
  return 0;
}

public static String v_dictKeyInfoToString(VmContext v_vm, DictImpl v_dict) {
  if ((v_dict.keyType == 5)) {
    return "string";
  }
  if ((v_dict.keyType == 3)) {
    return "int";
  }
  if ((v_dict.keyClassId == 0)) {
    return "instance";
  }
  return v_classIdToString(v_vm, v_dict.keyClassId);
}

public static int v_doEqualityComparisonAndReturnCode(Value v_a, Value v_b) {
  int v_leftType = v_a.type;
  int v_rightType = v_b.type;
  if ((v_leftType == v_rightType)) {
    int v_output = 0;
    switch (v_leftType) {
      case 1:
        v_output = 1;
        break;
      case 3:
        if ((v_a.intValue == v_b.intValue)) {
          v_output = 1;
        }
        break;
      case 4:
        if ((((double) v_a.internalValue) == ((double) v_b.internalValue))) {
          v_output = 1;
        }
        break;
      case 2:
        if (((v_a.intValue == 1) == (v_b.intValue == 1))) {
          v_output = 1;
        }
        break;
      case 5:
        if (v_a.internalValue.equals(v_b.internalValue)) {
          v_output = 1;
        }
        break;
      case 6:
        if ((((Object) v_a.internalValue) == ((Object) v_b.internalValue))) {
          v_output = 1;
        }
        break;
      case 7:
        if ((((Object) v_a.internalValue) == ((Object) v_b.internalValue))) {
          v_output = 1;
        }
        break;
      case 8:
        if ((((Object) v_a.internalValue) == ((Object) v_b.internalValue))) {
          v_output = 1;
        }
        break;
      case 9:
        FunctionPointer v_f1 = ((FunctionPointer) v_a.internalValue);
        FunctionPointer v_f2 = ((FunctionPointer) v_b.internalValue);
        if ((v_f1.functionId == v_f2.functionId)) {
          if (((v_f1.type == 2) || (v_f1.type == 4))) {
            if ((v_doEqualityComparisonAndReturnCode(v_f1.context, v_f2.context) == 1)) {
              v_output = 1;
            }
          } else {
            v_output = 1;
          }
        }
        break;
      case 10:
        org.crayonlang.interpreter.structs.ClassValue v_c1 = ((org.crayonlang.interpreter.structs.ClassValue) v_a.internalValue);
        org.crayonlang.interpreter.structs.ClassValue v_c2 = ((org.crayonlang.interpreter.structs.ClassValue) v_b.internalValue);
        if ((v_c1.classId == v_c2.classId)) {
          v_output = 1;
        }
        break;
      default:
        v_output = 2;
        break;
    }
    return v_output;
  }
  if ((v_rightType == 1)) {
    return 0;
  }
  if (((v_leftType == 3) && (v_rightType == 4))) {
    if ((v_a.intValue == ((double) v_b.internalValue))) {
      return 1;
    }
  } else {
    if (((v_leftType == 4) && (v_rightType == 3))) {
      if ((((double) v_a.internalValue) == v_b.intValue)) {
        return 1;
      }
    }
  }
  return 0;
}

public static String v_encodeBreakpointData(VmContext v_vm, BreakpointInfo v_breakpoint, int v_pc) {
  return null;
}

public static InterpreterResult v_errorResult(String v_error) {
  return new InterpreterResult(3, v_error, 0.0, 0, false, "");
}

public static boolean v_EX_AssertionFailed(ExecutionContext v_ec, String v_exMsg) {
  return v_generateException2(v_ec, 2, v_exMsg);
}

public static boolean v_EX_DivisionByZero(ExecutionContext v_ec, String v_exMsg) {
  return v_generateException2(v_ec, 3, v_exMsg);
}

public static boolean v_EX_Fatal(ExecutionContext v_ec, String v_exMsg) {
  return v_generateException2(v_ec, 0, v_exMsg);
}

public static boolean v_EX_IndexOutOfRange(ExecutionContext v_ec, String v_exMsg) {
  return v_generateException2(v_ec, 4, v_exMsg);
}

public static boolean v_EX_InvalidArgument(ExecutionContext v_ec, String v_exMsg) {
  return v_generateException2(v_ec, 5, v_exMsg);
}

public static boolean v_EX_InvalidAssignment(ExecutionContext v_ec, String v_exMsg) {
  return v_generateException2(v_ec, 6, v_exMsg);
}

public static boolean v_EX_InvalidInvocation(ExecutionContext v_ec, String v_exMsg) {
  return v_generateException2(v_ec, 7, v_exMsg);
}

public static boolean v_EX_InvalidKey(ExecutionContext v_ec, String v_exMsg) {
  return v_generateException2(v_ec, 8, v_exMsg);
}

public static boolean v_EX_KeyNotFound(ExecutionContext v_ec, String v_exMsg) {
  return v_generateException2(v_ec, 9, v_exMsg);
}

public static boolean v_EX_NullReference(ExecutionContext v_ec, String v_exMsg) {
  return v_generateException2(v_ec, 10, v_exMsg);
}

public static boolean v_EX_UnassignedVariable(ExecutionContext v_ec, String v_exMsg) {
  return v_generateException2(v_ec, 11, v_exMsg);
}

public static boolean v_EX_UnknownField(ExecutionContext v_ec, String v_exMsg) {
  return v_generateException2(v_ec, 12, v_exMsg);
}

public static boolean v_EX_UnsupportedOperation(ExecutionContext v_ec, String v_exMsg) {
  return v_generateException2(v_ec, 13, v_exMsg);
}

public static int v_finalizeInitializationImpl(VmContext v_vm, String v_projectId, int v_localeCount) {
  v_vm.symbolData.sourceCode = v_vm.symbolData.sourceCodeBuilder.toArray(PST_emptyArrayString);
  v_vm.symbolData.sourceCodeBuilder = null;
  v_vm.metadata.magicNumbers.totalLocaleCount = v_localeCount;
  v_vm.metadata.identifiers = v_vm.metadata.identifiersBuilder.toArray(PST_emptyArrayString);
  v_vm.metadata.literalTable = v_vm.metadata.literalTableBuilder.toArray(Value.EMPTY_ARRAY);
  v_vm.metadata.globalNameIdToPrimitiveMethodName = v_primitiveMethodsInitializeLookup(v_vm.metadata.invIdentifiers);
  v_vm.funcArgs = new Value[v_vm.metadata.identifiers.length];
  v_vm.metadata.projectId = v_projectId;
  v_vm.metadata.identifiersBuilder = null;
  v_vm.metadata.literalTableBuilder = null;
  v_vm.initializationComplete = true;
  return 0;
}

public static double v_fixFuzzyFloatPrecision(double v_x) {
  if (((v_x % 1) != 0)) {
    double v_u = (v_x % 1);
    if ((v_u < 0)) {
      v_u += 1.0;
    }
    boolean v_roundDown = false;
    if ((v_u > 0.9999999999)) {
      v_roundDown = true;
      v_x += 0.1;
    } else {
      if ((v_u < 0.00000000002250000000)) {
        v_roundDown = true;
      }
    }
    if (v_roundDown) {
      if ((false || (v_x > 0))) {
        v_x = (((int) v_x) + 0.0);
      } else {
        v_x = (((int) v_x) - 1.0);
      }
    }
  }
  return v_x;
}

public static int[][] v_generateEsfData(int v_byteCodeLength, int[] v_esfArgs) {
  int[][] v_output = new int[v_byteCodeLength][];
  ArrayList<int[]> v_esfTokenStack = new ArrayList<int[]>();
  int[] v_esfTokenStackTop = null;
  int v_esfArgIterator = 0;
  int v_esfArgLength = v_esfArgs.length;
  int v_j = 0;
  int v_pc = 0;
  while ((v_pc < v_byteCodeLength)) {
    if (((v_esfArgIterator < v_esfArgLength) && (v_pc == v_esfArgs[v_esfArgIterator]))) {
      v_esfTokenStackTop = new int[2];
      v_j = 1;
      while ((v_j < 3)) {
        v_esfTokenStackTop[(v_j - 1)] = v_esfArgs[(v_esfArgIterator + v_j)];
        v_j += 1;
      }
      v_esfTokenStack.add(v_esfTokenStackTop);
      v_esfArgIterator += 3;
    }
    while (((v_esfTokenStackTop != null) && (v_esfTokenStackTop[1] <= v_pc))) {
      v_esfTokenStack.remove(v_esfTokenStack.size() - 1);
      if ((v_esfTokenStack.size() == 0)) {
        v_esfTokenStackTop = null;
      } else {
        v_esfTokenStackTop = v_esfTokenStack.get((v_esfTokenStack.size() - 1));
      }
    }
    v_output[v_pc] = v_esfTokenStackTop;
    v_pc += 1;
  }
  return v_output;
}

public static InterpreterResult v_generateException(VmContext v_vm, StackFrame v_stack, int v_pc, int v_valueStackSize, ExecutionContext v_ec, int v_type, String v_message) {
  v_ec.currentValueStackSize = v_valueStackSize;
  v_stack.pc = v_pc;
  MagicNumbers v_mn = v_vm.metadata.magicNumbers;
  int v_generateExceptionFunctionId = v_mn.coreGenerateExceptionFunctionId;
  FunctionInfo v_functionInfo = v_vm.metadata.functionTable[v_generateExceptionFunctionId];
  v_pc = v_functionInfo.pc;
  if ((v_ec.localsStack.length <= (v_functionInfo.localsSize + v_stack.localsStackOffsetEnd))) {
    v_increaseLocalsStackCapacity(v_ec, v_functionInfo.localsSize);
  }
  int v_localsIndex = v_stack.localsStackOffsetEnd;
  int v_localsStackSetToken = (v_ec.localsStackSetToken + 1);
  v_ec.localsStackSetToken = v_localsStackSetToken;
  v_ec.localsStack[v_localsIndex] = v_buildInteger(v_vm.globals, v_type);
  v_ec.localsStack[(v_localsIndex + 1)] = v_buildString(v_vm.globals, v_message);
  v_ec.localsStackSet[v_localsIndex] = v_localsStackSetToken;
  v_ec.localsStackSet[(v_localsIndex + 1)] = v_localsStackSetToken;
  v_ec.stackTop = new StackFrame((v_pc + 1), v_localsStackSetToken, v_stack.localsStackOffsetEnd, (v_stack.localsStackOffsetEnd + v_functionInfo.localsSize), v_stack, false, null, v_valueStackSize, 0, (v_stack.depth + 1), 0, null, null, null);
  return new InterpreterResult(5, null, 0.0, 0, false, "");
}

public static boolean v_generateException2(ExecutionContext v_ec, int v_exceptionType, String v_exMsg) {
  v_ec.activeInterrupt = new Interrupt(1, v_exceptionType, v_exMsg, 0.0, null);
  return true;
}

public static Value v_generatePrimitiveMethodReference(int[] v_lookup, int v_globalNameId, Value v_context) {
  int v_functionId = v_resolvePrimitiveMethodName2(v_lookup, v_context.type, v_globalNameId);
  if ((v_functionId < 0)) {
    return null;
  }
  return new Value(9, new FunctionPointer(4, v_context, 0, v_functionId, null));
}

public static ArrayList<Token> v_generateTokenListFromPcs(VmContext v_vm, ArrayList<Integer> v_pcs) {
  ArrayList<Token> v_output = new ArrayList<Token>();
  ArrayList<Token>[] v_tokensByPc = v_vm.symbolData.tokenData;
  Token v_token = null;
  int v_i = 0;
  while ((v_i < v_pcs.size())) {
    ArrayList<Token> v_localTokens = v_tokensByPc[v_pcs.get(v_i)];
    if ((v_localTokens == null)) {
      if ((v_output.size() > 0)) {
        v_output.add(null);
      }
    } else {
      v_token = v_localTokens.get(0);
      v_output.add(v_token);
    }
    v_i += 1;
  }
  return v_output;
}

public static String v_getBinaryOpFromId(int v_id) {
  switch (v_id) {
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

public static ClassInfo[] v_getClassTable(VmContext v_vm, int v_classId) {
  ClassInfo[] v_oldTable = v_vm.metadata.classTable;
  int v_oldLength = v_oldTable.length;
  if ((v_classId < v_oldLength)) {
    return v_oldTable;
  }
  int v_newLength = (v_oldLength * 2);
  if ((v_classId >= v_newLength)) {
    v_newLength = (v_classId + 100);
  }
  ClassInfo[] v_newTable = new ClassInfo[v_newLength];
  int v_i = (v_oldLength - 1);
  while ((v_i >= 0)) {
    v_newTable[v_i] = v_oldTable[v_i];
    v_i -= 1;
  }
  v_vm.metadata.classTable = v_newTable;
  return v_newTable;
}

public static ExecutionContext v_getExecutionContext(VmContext v_vm, int v_id) {
  if ((v_id == -1)) {
    v_id = v_vm.lastExecutionContextId;
  }
  if (v_vm.executionContexts.containsKey(v_id)) {
    return v_vm.executionContexts.get(v_id);
  }
  return null;
}

public static double v_getFloat(Value v_num) {
  if ((v_num.type == 4)) {
    return ((double) v_num.internalValue);
  }
  return (v_num.intValue + 0.0);
}

public static FunctionInfo[] v_getFunctionTable(VmContext v_vm, int v_functionId) {
  FunctionInfo[] v_oldTable = v_vm.metadata.functionTable;
  int v_oldLength = v_oldTable.length;
  if ((v_functionId < v_oldLength)) {
    return v_oldTable;
  }
  int v_newLength = (v_oldLength * 2);
  if ((v_functionId >= v_newLength)) {
    v_newLength = (v_functionId + 100);
  }
  FunctionInfo[] v_newTable = new FunctionInfo[v_newLength];
  int v_i = 0;
  while ((v_i < v_oldLength)) {
    v_newTable[v_i] = v_oldTable[v_i];
    v_i += 1;
  }
  v_vm.metadata.functionTable = v_newTable;
  return v_newTable;
}

public static Value v_getItemFromList(ListImpl v_list, int v_i) {
  return v_list.array[v_i];
}

public static Object v_getNativeDataItem(Value v_objValue, int v_index) {
  ObjectInstance v_obj = ((ObjectInstance) v_objValue.internalValue);
  return v_obj.nativeData[v_index];
}

public static String v_getTypeFromId(int v_id) {
  switch (v_id) {
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

public static double v_getVmReinvokeDelay(InterpreterResult v_result) {
  return v_result.reinvokeDelay;
}

public static String v_getVmResultAssemblyInfo(InterpreterResult v_result) {
  return v_result.loadAssemblyInformation;
}

public static int v_getVmResultExecId(InterpreterResult v_result) {
  return v_result.executionContextId;
}

public static int v_getVmResultStatus(InterpreterResult v_result) {
  return v_result.status;
}

public static void v_increaseListCapacity(ListImpl v_list) {
  int v_oldCapacity = v_list.capacity;
  int v_newCapacity = (v_oldCapacity * 2);
  if ((v_newCapacity < 8)) {
    v_newCapacity = 8;
  }
  Value[] v_newArr = new Value[v_newCapacity];
  Value[] v_oldArr = v_list.array;
  int v_i = 0;
  while ((v_i < v_oldCapacity)) {
    v_newArr[v_i] = v_oldArr[v_i];
    v_i += 1;
  }
  v_list.capacity = v_newCapacity;
  v_list.array = v_newArr;
}

public static int v_increaseLocalsStackCapacity(ExecutionContext v_ec, int v_newScopeSize) {
  Value[] v_oldLocals = v_ec.localsStack;
  int[] v_oldSetIndicator = v_ec.localsStackSet;
  int v_oldCapacity = v_oldLocals.length;
  int v_newCapacity = ((v_oldCapacity * 2) + v_newScopeSize);
  Value[] v_newLocals = new Value[v_newCapacity];
  int[] v_newSetIndicator = new int[v_newCapacity];
  int v_i = 0;
  while ((v_i < v_oldCapacity)) {
    v_newLocals[v_i] = v_oldLocals[v_i];
    v_newSetIndicator[v_i] = v_oldSetIndicator[v_i];
    v_i += 1;
  }
  v_ec.localsStack = v_newLocals;
  v_ec.localsStackSet = v_newSetIndicator;
  return 0;
}

public static int v_initFileNameSymbolData(VmContext v_vm) {
  SymbolData v_symbolData = v_vm.symbolData;
  if ((v_symbolData == null)) {
    return 0;
  }
  if ((v_symbolData.fileNameById == null)) {
    int v_i = 0;
    String[] v_filenames = new String[v_symbolData.sourceCode.length];
    HashMap<String, Integer> v_fileIdByPath = new HashMap<String, Integer>();
    v_i = 0;
    while ((v_i < v_filenames.length)) {
      String v_sourceCode = v_symbolData.sourceCode[v_i];
      if ((v_sourceCode != null)) {
        int v_colon = v_sourceCode.indexOf("\n");
        if ((v_colon != -1)) {
          String v_filename = v_sourceCode.substring(0, 0 + v_colon);
          v_filenames[v_i] = v_filename;
          v_fileIdByPath.put(v_filename, v_i);
        }
      }
      v_i += 1;
    }
    v_symbolData.fileNameById = v_filenames;
    v_symbolData.fileIdByName = v_fileIdByPath;
  }
  return 0;
}

public static Code v_initializeByteCode(String v_raw) {
  int[] v_index = new int[1];
  v_index[0] = 0;
  int v_length = v_raw.length();
  String v_header = v_read_till(v_index, v_raw, v_length, '@');
  if ((v_header != "CRAYON")) {
  }
  String v_alphaNums = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
  int v_opCount = v_read_integer(v_index, v_raw, v_length, v_alphaNums);
  int[] v_ops = new int[v_opCount];
  int[][] v_iargs = new int[v_opCount][];
  String[] v_sargs = new String[v_opCount];
  char v_c = ' ';
  int v_argc = 0;
  int v_j = 0;
  String v_stringarg = null;
  boolean v_stringPresent = false;
  int v_iarg = 0;
  int[] v_iarglist = null;
  int v_i = 0;
  v_i = 0;
  while ((v_i < v_opCount)) {
    v_c = v_raw.charAt(v_index[0]);
    v_index[0] = (v_index[0] + 1);
    v_argc = 0;
    v_stringPresent = true;
    if ((v_c == '!')) {
      v_argc = 1;
    } else {
      if ((v_c == '&')) {
        v_argc = 2;
      } else {
        if ((v_c == '*')) {
          v_argc = 3;
        } else {
          if ((v_c != '~')) {
            v_stringPresent = false;
            v_index[0] = (v_index[0] - 1);
          }
          v_argc = v_read_integer(v_index, v_raw, v_length, v_alphaNums);
        }
      }
    }
    v_iarglist = new int[(v_argc - 1)];
    v_j = 0;
    while ((v_j < v_argc)) {
      v_iarg = v_read_integer(v_index, v_raw, v_length, v_alphaNums);
      if ((v_j == 0)) {
        v_ops[v_i] = v_iarg;
      } else {
        v_iarglist[(v_j - 1)] = v_iarg;
      }
      v_j += 1;
    }
    v_iargs[v_i] = v_iarglist;
    if (v_stringPresent) {
      v_stringarg = v_read_string(v_index, v_raw, v_length, v_alphaNums);
    } else {
      v_stringarg = null;
    }
    v_sargs[v_i] = v_stringarg;
    v_i += 1;
  }
  boolean[] v_hasBreakpoint = new boolean[v_opCount];
  BreakpointInfo[] v_breakpointInfo = new BreakpointInfo[v_opCount];
  v_i = 0;
  while ((v_i < v_opCount)) {
    v_hasBreakpoint[v_i] = false;
    v_breakpointInfo[v_i] = null;
    v_i += 1;
  }
  return new Code(v_ops, v_iargs, v_sargs, new HashMap[v_opCount], new HashMap[v_opCount], new VmDebugData(v_hasBreakpoint, v_breakpointInfo, new HashMap<Integer, Integer>(), 1, 0));
}

public static int v_initializeClass(int v_pc, VmContext v_vm, int[] v_args, String v_className) {
  int v_i = 0;
  int v_memberId = 0;
  int v_globalId = 0;
  int v_functionId = 0;
  int v_t = 0;
  int v_classId = v_args[0];
  int v_baseClassId = v_args[1];
  int v_globalNameId = v_args[2];
  int v_constructorFunctionId = v_args[3];
  int v_staticConstructorFunctionId = v_args[4];
  int v_staticInitializationState = 0;
  if ((v_staticConstructorFunctionId == -1)) {
    v_staticInitializationState = 2;
  }
  int v_staticFieldCount = v_args[5];
  int v_assemblyId = v_args[6];
  Value[] v_staticFields = new Value[v_staticFieldCount];
  v_i = 0;
  while ((v_i < v_staticFieldCount)) {
    v_staticFields[v_i] = v_vm.globals.valueNull;
    v_i += 1;
  }
  ClassInfo v_classInfo = new ClassInfo(v_classId, v_globalNameId, v_baseClassId, v_assemblyId, v_staticInitializationState, v_staticFields, v_staticConstructorFunctionId, v_constructorFunctionId, 0, null, null, null, null, null, v_vm.metadata.classMemberLocalizerBuilder.get(v_classId), null, v_className);
  ClassInfo[] v_classTable = v_getClassTable(v_vm, v_classId);
  v_classTable[v_classId] = v_classInfo;
  ArrayList<ClassInfo> v_classChain = new ArrayList<ClassInfo>();
  v_classChain.add(v_classInfo);
  int v_classIdWalker = v_baseClassId;
  while ((v_classIdWalker != -1)) {
    ClassInfo v_walkerClass = v_classTable[v_classIdWalker];
    v_classChain.add(v_walkerClass);
    v_classIdWalker = v_walkerClass.baseClassId;
  }
  ClassInfo v_baseClass = null;
  if ((v_baseClassId != -1)) {
    v_baseClass = v_classChain.get(1);
  }
  ArrayList<Integer> v_functionIds = new ArrayList<Integer>();
  ArrayList<Integer> v_fieldInitializationCommand = new ArrayList<Integer>();
  ArrayList<Value> v_fieldInitializationLiteral = new ArrayList<Value>();
  ArrayList<Integer> v_fieldAccessModifier = new ArrayList<Integer>();
  HashMap<Integer, Integer> v_globalNameIdToMemberId = new HashMap<Integer, Integer>();
  if ((v_baseClass != null)) {
    v_i = 0;
    while ((v_i < v_baseClass.memberCount)) {
      v_functionIds.add(v_baseClass.functionIds[v_i]);
      v_fieldInitializationCommand.add(v_baseClass.fieldInitializationCommand[v_i]);
      v_fieldInitializationLiteral.add(v_baseClass.fieldInitializationLiteral[v_i]);
      v_fieldAccessModifier.add(v_baseClass.fieldAccessModifiers[v_i]);
      v_i += 1;
    }
    int[] v_keys = PST_convertIntegerSetToArray(v_baseClass.globalIdToMemberId.keySet());
    v_i = 0;
    while ((v_i < v_keys.length)) {
      v_t = v_keys[v_i];
      v_globalNameIdToMemberId.put(v_t, v_baseClass.globalIdToMemberId.get(v_t));
      v_i += 1;
    }
    v_keys = PST_convertIntegerSetToArray(v_baseClass.localeScopedNameIdToMemberId.keySet());
    v_i = 0;
    while ((v_i < v_keys.length)) {
      v_t = v_keys[v_i];
      v_classInfo.localeScopedNameIdToMemberId.put(v_t, v_baseClass.localeScopedNameIdToMemberId.get(v_t));
      v_i += 1;
    }
  }
  int v_accessModifier = 0;
  v_i = 7;
  while ((v_i < v_args.length)) {
    v_memberId = v_args[(v_i + 1)];
    v_globalId = v_args[(v_i + 2)];
    v_accessModifier = v_args[(v_i + 5)];
    while ((v_memberId >= v_functionIds.size())) {
      v_functionIds.add(-1);
      v_fieldInitializationCommand.add(-1);
      v_fieldInitializationLiteral.add(null);
      v_fieldAccessModifier.add(0);
    }
    v_globalNameIdToMemberId.put(v_globalId, v_memberId);
    v_fieldAccessModifier.set(v_memberId, v_accessModifier);
    if ((v_args[v_i] == 0)) {
      v_fieldInitializationCommand.set(v_memberId, v_args[(v_i + 3)]);
      v_t = v_args[(v_i + 4)];
      if ((v_t == -1)) {
        v_fieldInitializationLiteral.set(v_memberId, v_vm.globals.valueNull);
      } else {
        v_fieldInitializationLiteral.set(v_memberId, v_vm.metadata.literalTable[v_t]);
      }
    } else {
      v_functionId = v_args[(v_i + 3)];
      v_functionIds.set(v_memberId, v_functionId);
    }
    v_i += 6;
  }
  v_classInfo.functionIds = PST_listToArrayInt(v_functionIds);
  v_classInfo.fieldInitializationCommand = PST_listToArrayInt(v_fieldInitializationCommand);
  v_classInfo.fieldInitializationLiteral = v_fieldInitializationLiteral.toArray(Value.EMPTY_ARRAY);
  v_classInfo.fieldAccessModifiers = PST_listToArrayInt(v_fieldAccessModifier);
  v_classInfo.memberCount = v_functionIds.size();
  v_classInfo.globalIdToMemberId = v_globalNameIdToMemberId;
  v_classInfo.typeInfo = new int[v_classInfo.memberCount][];
  if ((v_baseClass != null)) {
    v_i = 0;
    while ((v_i < v_baseClass.typeInfo.length)) {
      v_classInfo.typeInfo[v_i] = v_baseClass.typeInfo[v_i];
      v_i += 1;
    }
  }
  if ("Core.Exception".equals(v_className)) {
    MagicNumbers v_mn = v_vm.metadata.magicNumbers;
    v_mn.coreExceptionClassId = v_classId;
  }
  return 0;
}

public static int v_initializeClassFieldTypeInfo(VmContext v_vm, int[] v_opCodeRow) {
  ClassInfo v_classInfo = v_vm.metadata.classTable[v_opCodeRow[0]];
  int v_memberId = v_opCodeRow[1];
  int v_len = v_opCodeRow.length;
  int[] v_typeInfo = new int[(v_len - 2)];
  int v_i = 2;
  while ((v_i < v_len)) {
    v_typeInfo[(v_i - 2)] = v_opCodeRow[v_i];
    v_i += 1;
  }
  v_classInfo.typeInfo[v_memberId] = v_typeInfo;
  return 0;
}

public static VmGlobals v_initializeConstantValues() {
  Value[] v_pos = new Value[2049];
  Value[] v_neg = new Value[257];
  int v_i = 0;
  while ((v_i < 2049)) {
    v_pos[v_i] = new Value(v_i);
    v_i += 1;
  }
  v_i = 1;
  while ((v_i < 257)) {
    v_neg[v_i] = new Value(-v_i);
    v_i += 1;
  }
  v_neg[0] = v_pos[0];
  VmGlobals v_globals = new VmGlobals(new Value(1, null), new Value(true), new Value(false), v_pos[0], v_pos[1], v_neg[1], new Value(4, 0.0), new Value(4, 1.0), new Value(5, ""), v_pos, v_neg, new HashMap<String, Value>(), new int[1], new int[1], new int[1], new int[1], new int[1], new int[2]);
  v_globals.commonStrings.put("", v_globals.stringEmpty);
  v_globals.booleanType[0] = 2;
  v_globals.intType[0] = 3;
  v_globals.floatType[0] = 4;
  v_globals.stringType[0] = 5;
  v_globals.classType[0] = 10;
  v_globals.anyInstanceType[0] = 8;
  v_globals.anyInstanceType[1] = 0;
  return v_globals;
}

public static int v_initializeFunction(VmContext v_vm, int[] v_args, int v_currentPc, String v_stringArg) {
  int v_functionId = v_args[0];
  int v_nameId = v_args[1];
  int v_minArgCount = v_args[2];
  int v_maxArgCount = v_args[3];
  int v_functionType = v_args[4];
  int v_classId = v_args[5];
  int v_localsCount = v_args[6];
  int v_numPcOffsetsForOptionalArgs = v_args[8];
  int[] v_pcOffsetsForOptionalArgs = new int[(v_numPcOffsetsForOptionalArgs + 1)];
  int v_i = 0;
  while ((v_i < v_numPcOffsetsForOptionalArgs)) {
    v_pcOffsetsForOptionalArgs[(v_i + 1)] = v_args[(9 + v_i)];
    v_i += 1;
  }
  FunctionInfo[] v_functionTable = v_getFunctionTable(v_vm, v_functionId);
  v_functionTable[v_functionId] = new FunctionInfo(v_functionId, v_nameId, v_currentPc, v_minArgCount, v_maxArgCount, v_functionType, v_classId, v_localsCount, v_pcOffsetsForOptionalArgs, v_stringArg, null);
  v_vm.metadata.mostRecentFunctionDef = v_functionTable[v_functionId];
  if ((v_nameId >= 0)) {
    String v_name = v_vm.metadata.identifiers[v_nameId];
    if ("_LIB_CORE_list_filter".equals(v_name)) {
      v_vm.metadata.primitiveMethodFunctionIdFallbackLookup[0] = v_functionId;
    } else {
      if ("_LIB_CORE_list_map".equals(v_name)) {
        v_vm.metadata.primitiveMethodFunctionIdFallbackLookup[1] = v_functionId;
      } else {
        if ("_LIB_CORE_list_sort_by_key".equals(v_name)) {
          v_vm.metadata.primitiveMethodFunctionIdFallbackLookup[2] = v_functionId;
        } else {
          if ("_LIB_CORE_invoke".equals(v_name)) {
            v_vm.metadata.primitiveMethodFunctionIdFallbackLookup[3] = v_functionId;
          } else {
            if ("_LIB_CORE_generateException".equals(v_name)) {
              MagicNumbers v_mn = v_vm.metadata.magicNumbers;
              v_mn.coreGenerateExceptionFunctionId = v_functionId;
            }
          }
        }
      }
    }
  }
  return 0;
}

public static HashMap<Integer, Integer> v_initializeIntSwitchStatement(VmContext v_vm, int v_pc, int[] v_args) {
  HashMap<Integer, Integer> v_output = new HashMap<Integer, Integer>();
  int v_i = 1;
  while ((v_i < v_args.length)) {
    v_output.put(v_args[v_i], v_args[(v_i + 1)]);
    v_i += 2;
  }
  v_vm.byteCode.integerSwitchesByPc[v_pc] = v_output;
  return v_output;
}

public static HashMap<String, Integer> v_initializeStringSwitchStatement(VmContext v_vm, int v_pc, int[] v_args) {
  HashMap<String, Integer> v_output = new HashMap<String, Integer>();
  int v_i = 1;
  while ((v_i < v_args.length)) {
    String v_s = ((String) v_vm.metadata.literalTable[v_args[v_i]].internalValue);
    v_output.put(v_s, v_args[(v_i + 1)]);
    v_i += 2;
  }
  v_vm.byteCode.stringSwitchesByPc[v_pc] = v_output;
  return v_output;
}

public static int v_initLocTable(VmContext v_vm, int[] v_row) {
  int v_classId = v_row[0];
  int v_memberCount = v_row[1];
  int v_nameId = 0;
  int v_totalLocales = v_vm.metadata.magicNumbers.totalLocaleCount;
  HashMap<Integer, Integer> v_lookup = new HashMap<Integer, Integer>();
  int v_i = 2;
  while ((v_i < v_row.length)) {
    int v_localeId = v_row[v_i];
    v_i += 1;
    int v_j = 0;
    while ((v_j < v_memberCount)) {
      v_nameId = v_row[(v_i + v_j)];
      if ((v_nameId != -1)) {
        v_lookup.put(((v_nameId * v_totalLocales) + v_localeId), v_j);
      }
      v_j += 1;
    }
    v_i += v_memberCount;
  }
  v_vm.metadata.classMemberLocalizerBuilder.put(v_classId, v_lookup);
  return 0;
}

public static InterpreterResult v_interpret(VmContext v_vm, int v_executionContextId) {
  InterpreterResult v_output = v_interpretImpl(v_vm, v_executionContextId);
  while (((v_output.status == 5) && (v_output.reinvokeDelay == 0))) {
    v_output = v_interpretImpl(v_vm, v_executionContextId);
  }
  return v_output;
}

public static InterpreterResult v_interpreterFinished(VmContext v_vm, ExecutionContext v_ec) {
  if ((v_ec != null)) {
    int v_id = v_ec.id;
    if (v_vm.executionContexts.containsKey(v_id)) {
      v_vm.executionContexts.remove(v_id);
    }
  }
  return new InterpreterResult(1, null, 0.0, 0, false, "");
}

public static ExecutionContext v_interpreterGetExecutionContext(VmContext v_vm, int v_executionContextId) {
  HashMap<Integer, ExecutionContext> v_executionContexts = v_vm.executionContexts;
  if (!v_executionContexts.containsKey(v_executionContextId)) {
    return null;
  }
  return v_executionContexts.get(v_executionContextId);
}

public static InterpreterResult v_interpretImpl(VmContext v_vm, int v_executionContextId) {
  VmMetadata v_metadata = v_vm.metadata;
  VmGlobals v_globals = v_vm.globals;
  Value v_VALUE_NULL = v_globals.valueNull;
  Value v_VALUE_TRUE = v_globals.boolTrue;
  Value v_VALUE_FALSE = v_globals.boolFalse;
  Value v_VALUE_INT_ONE = v_globals.intOne;
  Value v_VALUE_INT_ZERO = v_globals.intZero;
  Value v_VALUE_FLOAT_ZERO = v_globals.floatZero;
  Value v_VALUE_FLOAT_ONE = v_globals.floatOne;
  Value[] v_INTEGER_POSITIVE_CACHE = v_globals.positiveIntegers;
  Value[] v_INTEGER_NEGATIVE_CACHE = v_globals.negativeIntegers;
  HashMap<Integer, ExecutionContext> v_executionContexts = v_vm.executionContexts;
  ExecutionContext v_ec = v_interpreterGetExecutionContext(v_vm, v_executionContextId);
  if ((v_ec == null)) {
    return v_interpreterFinished(v_vm, null);
  }
  v_ec.executionCounter += 1;
  StackFrame v_stack = v_ec.stackTop;
  int[] v_ops = v_vm.byteCode.ops;
  int[][] v_args = v_vm.byteCode.args;
  String[] v_stringArgs = v_vm.byteCode.stringArgs;
  ClassInfo[] v_classTable = v_vm.metadata.classTable;
  FunctionInfo[] v_functionTable = v_vm.metadata.functionTable;
  Value[] v_literalTable = v_vm.metadata.literalTable;
  String[] v_identifiers = v_vm.metadata.identifiers;
  Value[] v_valueStack = v_ec.valueStack;
  int v_valueStackSize = v_ec.currentValueStackSize;
  int v_valueStackCapacity = v_valueStack.length;
  boolean v_hasInterrupt = false;
  int v_type = 0;
  int v_nameId = 0;
  int v_classId = 0;
  int v_functionId = 0;
  int v_localeId = 0;
  ClassInfo v_classInfo = null;
  int v_len = 0;
  Value v_root = null;
  int[] v_row = null;
  int v_argCount = 0;
  String[] v_stringList = null;
  boolean v_returnValueUsed = false;
  Value v_output = null;
  FunctionInfo v_functionInfo = null;
  int v_keyType = 0;
  int v_intKey = 0;
  String v_stringKey = null;
  boolean v_first = false;
  boolean v_primitiveMethodToCoreLibraryFallback = false;
  boolean v_bool1 = false;
  boolean v_bool2 = false;
  boolean v_staticConstructorNotInvoked = true;
  int v_int1 = 0;
  int v_int2 = 0;
  int v_int3 = 0;
  int v_i = 0;
  int v_j = 0;
  double v_float1 = 0.0;
  double v_float2 = 0.0;
  double v_float3 = 0.0;
  double[] v_floatList1 = new double[2];
  Value v_value = null;
  Value v_value2 = null;
  Value v_value3 = null;
  String v_string1 = null;
  String v_string2 = null;
  ObjectInstance v_objInstance1 = null;
  ObjectInstance v_objInstance2 = null;
  ListImpl v_list1 = null;
  ListImpl v_list2 = null;
  ArrayList<Value> v_valueList1 = null;
  ArrayList<Value> v_valueList2 = null;
  DictImpl v_dictImpl = null;
  DictImpl v_dictImpl2 = null;
  ArrayList<String> v_stringList1 = null;
  ArrayList<Integer> v_intList1 = null;
  Value[] v_valueArray1 = null;
  int[] v_intArray1 = null;
  int[] v_intArray2 = null;
  Object[] v_objArray1 = null;
  FunctionPointer v_functionPointer1 = null;
  HashMap<Integer, Integer> v_intIntDict1 = null;
  HashMap<String, Integer> v_stringIntDict1 = null;
  StackFrame v_stackFrame2 = null;
  Value v_leftValue = null;
  Value v_rightValue = null;
  org.crayonlang.interpreter.structs.ClassValue v_classValue = null;
  Value v_arg1 = null;
  Value v_arg2 = null;
  Value v_arg3 = null;
  ArrayList<Token> v_tokenList = null;
  int[] v_globalNameIdToPrimitiveMethodName = v_vm.metadata.globalNameIdToPrimitiveMethodName;
  MagicNumbers v_magicNumbers = v_vm.metadata.magicNumbers;
  HashMap<Integer, Integer>[] v_integerSwitchesByPc = v_vm.byteCode.integerSwitchesByPc;
  HashMap<String, Integer>[] v_stringSwitchesByPc = v_vm.byteCode.stringSwitchesByPc;
  HashMap<Integer, Integer> v_integerSwitch = null;
  HashMap<String, Integer> v_stringSwitch = null;
  int[][] v_esfData = v_vm.metadata.esfData;
  HashMap<Integer, ClosureValuePointer> v_closure = null;
  HashMap<Integer, ClosureValuePointer> v_parentClosure = null;
  int[] v_intBuffer = new int[16];
  Value[] v_localsStack = v_ec.localsStack;
  int[] v_localsStackSet = v_ec.localsStackSet;
  int v_localsStackSetToken = v_stack.localsStackSetToken;
  int v_localsStackCapacity = v_localsStack.length;
  int v_localsStackOffset = v_stack.localsStackOffset;
  Value[] v_funcArgs = v_vm.funcArgs;
  int v_pc = v_stack.pc;
  java.lang.reflect.Method v_nativeFp = null;
  VmDebugData v_debugData = v_vm.byteCode.debugData;
  boolean[] v_isBreakPointPresent = v_debugData.hasBreakpoint;
  BreakpointInfo v_breakpointInfo = null;
  boolean v_debugBreakPointTemporaryDisable = false;
  while (true) {
    v_row = v_args[v_pc];
    switch (v_ops[v_pc]) {
      case 0:
        // ADD_LITERAL;
        v_addLiteralImpl(v_vm, v_row, v_stringArgs[v_pc]);
        break;
      case 1:
        // ADD_NAME;
        v_addNameImpl(v_vm, v_stringArgs[v_pc]);
        break;
      case 2:
        // ARG_TYPE_VERIFY;
        v_len = v_row[0];
        v_i = 1;
        v_j = 0;
        while ((v_j < v_len)) {
          v_j += 1;
        }
        break;
      case 3:
        // ASSIGN_CLOSURE;
        v_value = v_valueStack[--v_valueStackSize];
        v_i = v_row[0];
        if ((v_stack.closureVariables == null)) {
          v_closure = new HashMap<Integer, ClosureValuePointer>();
          v_stack.closureVariables = v_closure;
          v_closure.put(v_i, new ClosureValuePointer(v_value));
        } else {
          v_closure = v_stack.closureVariables;
          if (v_closure.containsKey(v_i)) {
            v_closure.get(v_i).value = v_value;
          } else {
            v_closure.put(v_i, new ClosureValuePointer(v_value));
          }
        }
        break;
      case 4:
        // ASSIGN_INDEX;
        v_valueStackSize -= 3;
        v_value = v_valueStack[(v_valueStackSize + 2)];
        v_value2 = v_valueStack[(v_valueStackSize + 1)];
        v_root = v_valueStack[v_valueStackSize];
        v_type = v_root.type;
        v_bool1 = (v_row[0] == 1);
        if ((v_type == 6)) {
          if ((v_value2.type == 3)) {
            v_i = v_value2.intValue;
            v_list1 = ((ListImpl) v_root.internalValue);
            if ((v_list1.type != null)) {
              v_value3 = v_canAssignTypeToGeneric(v_vm, v_value, v_list1.type, 0);
              if ((v_value3 == null)) {
                v_hasInterrupt = v_EX_InvalidArgument(v_ec, "Cannot convert a " + v_typeToStringFromValue(v_vm, v_value) + " into a " + v_typeToString(v_vm, v_list1.type, 0));
              }
              v_value = v_value3;
            }
            if (!v_hasInterrupt) {
              if ((v_i >= v_list1.size)) {
                v_hasInterrupt = v_EX_IndexOutOfRange(v_ec, "Index is out of range.");
              } else {
                if ((v_i < 0)) {
                  v_i += v_list1.size;
                  if ((v_i < 0)) {
                    v_hasInterrupt = v_EX_IndexOutOfRange(v_ec, "Index is out of range.");
                  }
                }
              }
              if (!v_hasInterrupt) {
                v_list1.array[v_i] = v_value;
              }
            }
          } else {
            v_hasInterrupt = v_EX_InvalidArgument(v_ec, "List index must be an integer.");
          }
        } else {
          if ((v_type == 7)) {
            v_dictImpl = ((DictImpl) v_root.internalValue);
            if ((v_dictImpl.valueType != null)) {
              v_value3 = v_canAssignTypeToGeneric(v_vm, v_value, v_dictImpl.valueType, 0);
              if ((v_value3 == null)) {
                v_hasInterrupt = v_EX_InvalidArgument(v_ec, "Cannot assign a value to this dictionary of this type.");
              } else {
                v_value = v_value3;
              }
            }
            v_keyType = v_value2.type;
            if ((v_keyType == 3)) {
              v_intKey = v_value2.intValue;
            } else {
              if ((v_keyType == 5)) {
                v_stringKey = ((String) v_value2.internalValue);
              } else {
                if ((v_keyType == 8)) {
                  v_objInstance1 = ((ObjectInstance) v_value2.internalValue);
                  v_intKey = v_objInstance1.objectId;
                } else {
                  v_hasInterrupt = v_EX_InvalidArgument(v_ec, "Invalid key for a dictionary.");
                }
              }
            }
            if (!v_hasInterrupt) {
              v_bool2 = (v_dictImpl.size == 0);
              if ((v_dictImpl.keyType != v_keyType)) {
                if ((v_dictImpl.valueType != null)) {
                  v_string1 = "Cannot assign a key of type " + v_typeToStringFromValue(v_vm, v_value2) + " to a dictionary that requires key types of " + v_dictKeyInfoToString(v_vm, v_dictImpl) + ".";
                  v_hasInterrupt = v_EX_InvalidKey(v_ec, v_string1);
                } else {
                  if (!v_bool2) {
                    v_hasInterrupt = v_EX_InvalidKey(v_ec, "Cannot have multiple keys in one dictionary with different types.");
                  }
                }
              } else {
                if (((v_keyType == 8) && (v_dictImpl.keyClassId > 0) && (v_objInstance1.classId != v_dictImpl.keyClassId))) {
                  if (v_isClassASubclassOf(v_vm, v_objInstance1.classId, v_dictImpl.keyClassId)) {
                    v_hasInterrupt = v_EX_InvalidKey(v_ec, "Cannot use this type of object as a key for this dictionary.");
                  }
                }
              }
            }
            if (!v_hasInterrupt) {
              if ((v_keyType == 5)) {
                Integer dictLookup1 = v_dictImpl.stringToIndex.get(v_stringKey);
                v_int1 = dictLookup1 == null ? (v_dictImpl.stringToIndex.containsKey(v_stringKey) ? null : (-1)) : dictLookup1;
                if ((v_int1 == -1)) {
                  v_dictImpl.stringToIndex.put(v_stringKey, v_dictImpl.size);
                  v_dictImpl.size += 1;
                  v_dictImpl.keys.add(v_value2);
                  v_dictImpl.values.add(v_value);
                  if (v_bool2) {
                    v_dictImpl.keyType = v_keyType;
                  }
                } else {
                  v_dictImpl.values.set(v_int1, v_value);
                }
              } else {
                Integer dictLookup2 = v_dictImpl.intToIndex.get(v_intKey);
                v_int1 = dictLookup2 == null ? (-1) : dictLookup2;
                if ((v_int1 == -1)) {
                  v_dictImpl.intToIndex.put(v_intKey, v_dictImpl.size);
                  v_dictImpl.size += 1;
                  v_dictImpl.keys.add(v_value2);
                  v_dictImpl.values.add(v_value);
                  if (v_bool2) {
                    v_dictImpl.keyType = v_keyType;
                  }
                } else {
                  v_dictImpl.values.set(v_int1, v_value);
                }
              }
            }
          } else {
            v_hasInterrupt = v_EX_UnsupportedOperation(v_ec, v_getTypeFromId(v_type) + " type does not support assigning to an index.");
          }
        }
        if (v_bool1) {
          v_valueStack[v_valueStackSize] = v_value;
          v_valueStackSize += 1;
        }
        break;
      case 6:
        // ASSIGN_STATIC_FIELD;
        v_classInfo = v_classTable[v_row[0]];
        v_staticConstructorNotInvoked = true;
        if ((v_classInfo.staticInitializationState < 2)) {
          v_stack.pc = v_pc;
          v_stackFrame2 = v_maybeInvokeStaticConstructor(v_vm, v_ec, v_stack, v_classInfo, v_valueStackSize, PST_intBuffer16);
          if ((PST_intBuffer16[0] == 1)) {
            return v_generateException(v_vm, v_stack, v_pc, v_valueStackSize, v_ec, 0, "Static initialization loop detected. The class this field is a member of is not done being initialized.");
          }
          if ((v_stackFrame2 != null)) {
            v_staticConstructorNotInvoked = false;
            v_stack = v_stackFrame2;
            v_pc = v_stack.pc;
            v_localsStackSetToken = v_stack.localsStackSetToken;
            v_localsStackOffset = v_stack.localsStackOffset;
          }
        }
        if (v_staticConstructorNotInvoked) {
          v_valueStackSize -= 1;
          v_classInfo.staticFields[v_row[1]] = v_valueStack[v_valueStackSize];
        }
        break;
      case 7:
        // ASSIGN_FIELD;
        v_valueStackSize -= 2;
        v_value = v_valueStack[(v_valueStackSize + 1)];
        v_value2 = v_valueStack[v_valueStackSize];
        v_nameId = v_row[2];
        if ((v_value2.type == 8)) {
          v_objInstance1 = ((ObjectInstance) v_value2.internalValue);
          v_classId = v_objInstance1.classId;
          v_classInfo = v_classTable[v_classId];
          v_intIntDict1 = v_classInfo.localeScopedNameIdToMemberId;
          if ((v_row[5] == v_classId)) {
            v_int1 = v_row[6];
          } else {
            Integer dictLookup3 = v_intIntDict1.get(v_nameId);
            v_int1 = dictLookup3 == null ? (-1) : dictLookup3;
            if ((v_int1 != -1)) {
              v_int3 = v_classInfo.fieldAccessModifiers[v_int1];
              if ((v_int3 > 1)) {
                if ((v_int3 == 2)) {
                  if ((v_classId != v_row[3])) {
                    v_int1 = -2;
                  }
                } else {
                  if (((v_int3 == 3) || (v_int3 == 5))) {
                    if ((v_classInfo.assemblyId != v_row[4])) {
                      v_int1 = -3;
                    }
                  }
                  if (((v_int3 == 4) || (v_int3 == 5))) {
                    v_i = v_row[3];
                    if ((v_classId == v_i)) {
                    } else {
                      v_classInfo = v_classTable[v_classInfo.id];
                      while (((v_classInfo.baseClassId != -1) && (v_int1 < v_classTable[v_classInfo.baseClassId].fieldAccessModifiers.length))) {
                        v_classInfo = v_classTable[v_classInfo.baseClassId];
                      }
                      v_j = v_classInfo.id;
                      if ((v_j != v_i)) {
                        v_bool1 = false;
                        while (((v_i != -1) && (v_classTable[v_i].baseClassId != -1))) {
                          v_i = v_classTable[v_i].baseClassId;
                          if ((v_i == v_j)) {
                            v_bool1 = true;
                            v_i = -1;
                          }
                        }
                        if (!v_bool1) {
                          v_int1 = -4;
                        }
                      }
                    }
                    v_classInfo = v_classTable[v_classId];
                  }
                }
              }
            }
            v_row[5] = v_classId;
            v_row[6] = v_int1;
          }
          if ((v_int1 > -1)) {
            v_int2 = v_classInfo.functionIds[v_int1];
            if ((v_int2 == -1)) {
              v_intArray1 = v_classInfo.typeInfo[v_int1];
              if ((v_intArray1 == null)) {
                v_objInstance1.members[v_int1] = v_value;
              } else {
                v_value2 = v_canAssignTypeToGeneric(v_vm, v_value, v_intArray1, 0);
                if ((v_value2 != null)) {
                  v_objInstance1.members[v_int1] = v_value2;
                } else {
                  v_hasInterrupt = v_EX_InvalidArgument(v_ec, "Cannot assign this type to this field.");
                }
              }
            } else {
              v_hasInterrupt = v_EX_InvalidArgument(v_ec, "Cannot override a method with assignment.");
            }
          } else {
            if ((v_int1 < -1)) {
              v_string1 = v_identifiers[v_row[0]];
              if ((v_int1 == -2)) {
                v_string2 = "private";
              } else {
                if ((v_int1 == -3)) {
                  v_string2 = "internal";
                } else {
                  v_string2 = "protected";
                }
              }
              v_hasInterrupt = v_EX_UnknownField(v_ec, "The field '" + v_string1 + "' is marked as " + v_string2 + " and cannot be accessed from here.");
            } else {
              v_hasInterrupt = v_EX_InvalidAssignment(v_ec, "'" + v_classInfo.fullyQualifiedName + "' instances do not have a field called '" + v_metadata.identifiers[v_row[0]] + "'");
            }
          }
        } else {
          v_hasInterrupt = v_EX_InvalidAssignment(v_ec, "Cannot assign to a field on this type.");
        }
        if ((v_row[1] == 1)) {
          v_valueStack[v_valueStackSize++] = v_value;
        }
        break;
      case 8:
        // ASSIGN_THIS_FIELD;
        v_objInstance2 = ((ObjectInstance) v_stack.objectContext.internalValue);
        v_objInstance2.members[v_row[0]] = v_valueStack[--v_valueStackSize];
        break;
      case 5:
        // ASSIGN_LOCAL;
        v_i = (v_localsStackOffset + v_row[0]);
        v_localsStack[v_i] = v_valueStack[--v_valueStackSize];
        v_localsStackSet[v_i] = v_localsStackSetToken;
        break;
      case 9:
        // BINARY_OP;
        v_rightValue = v_valueStack[--v_valueStackSize];
        v_leftValue = v_valueStack[(v_valueStackSize - 1)];
        switch (((((v_leftValue.type * 15) + v_row[0]) * 11) + v_rightValue.type)) {
          case 553:
            // int ** int;
            if ((v_rightValue.intValue == 0)) {
              v_value = v_VALUE_INT_ONE;
            } else {
              if ((v_rightValue.intValue > 0)) {
                v_value = v_buildInteger(v_globals, ((int) Math.pow(v_leftValue.intValue, v_rightValue.intValue)));
              } else {
                v_value = v_buildFloat(v_globals, Math.pow(v_leftValue.intValue, v_rightValue.intValue));
              }
            }
            break;
          case 554:
            // int ** float;
            v_value = v_buildFloat(v_globals, (0.0 + Math.pow(v_leftValue.intValue, ((double) v_rightValue.internalValue))));
            break;
          case 718:
            // float ** int;
            v_value = v_buildFloat(v_globals, (0.0 + Math.pow(((double) v_leftValue.internalValue), v_rightValue.intValue)));
            break;
          case 719:
            // float ** float;
            v_value = v_buildFloat(v_globals, (0.0 + Math.pow(((double) v_leftValue.internalValue), ((double) v_rightValue.internalValue))));
            break;
          case 708:
            // float % float;
            v_float1 = ((double) v_rightValue.internalValue);
            if ((v_float1 == 0)) {
              v_hasInterrupt = v_EX_DivisionByZero(v_ec, "Modulo by 0.");
            } else {
              v_float3 = (((double) v_leftValue.internalValue) % v_float1);
              if ((v_float3 < 0)) {
                v_float3 += v_float1;
              }
              v_value = v_buildFloat(v_globals, v_float3);
            }
            break;
          case 707:
            // float % int;
            v_int1 = v_rightValue.intValue;
            if ((v_int1 == 0)) {
              v_hasInterrupt = v_EX_DivisionByZero(v_ec, "Modulo by 0.");
            } else {
              v_float1 = (((double) v_leftValue.internalValue) % v_int1);
              if ((v_float1 < 0)) {
                v_float1 += v_int1;
              }
              v_value = v_buildFloat(v_globals, v_float1);
            }
            break;
          case 543:
            // int % float;
            v_float3 = ((double) v_rightValue.internalValue);
            if ((v_float3 == 0)) {
              v_hasInterrupt = v_EX_DivisionByZero(v_ec, "Modulo by 0.");
            } else {
              v_float1 = (v_leftValue.intValue % v_float3);
              if ((v_float1 < 0)) {
                v_float1 += v_float3;
              }
              v_value = v_buildFloat(v_globals, v_float1);
            }
            break;
          case 542:
            // int % int;
            v_int2 = v_rightValue.intValue;
            if ((v_int2 == 0)) {
              v_hasInterrupt = v_EX_DivisionByZero(v_ec, "Modulo by 0.");
            } else {
              v_int1 = (v_leftValue.intValue % v_int2);
              if ((v_int1 < 0)) {
                v_int1 += v_int2;
              }
              v_value = v_buildInteger(v_globals, v_int1);
            }
            break;
          case 996:
            // list + list;
            v_value = new Value(6, v_valueConcatLists(((ListImpl) v_leftValue.internalValue), ((ListImpl) v_rightValue.internalValue)));
            break;
          case 498:
            // int + int;
            v_int1 = (v_leftValue.intValue + v_rightValue.intValue);
            if ((v_int1 < 0)) {
              if ((v_int1 > -257)) {
                v_value = v_INTEGER_NEGATIVE_CACHE[-v_int1];
              } else {
                v_value = new Value(v_int1);
              }
            } else {
              if ((v_int1 < 2049)) {
                v_value = v_INTEGER_POSITIVE_CACHE[v_int1];
              } else {
                v_value = new Value(v_int1);
              }
            }
            break;
          case 509:
            // int - int;
            v_int1 = (v_leftValue.intValue - v_rightValue.intValue);
            if ((v_int1 < 0)) {
              if ((v_int1 > -257)) {
                v_value = v_INTEGER_NEGATIVE_CACHE[-v_int1];
              } else {
                v_value = new Value(v_int1);
              }
            } else {
              if ((v_int1 < 2049)) {
                v_value = v_INTEGER_POSITIVE_CACHE[v_int1];
              } else {
                v_value = new Value(v_int1);
              }
            }
            break;
          case 520:
            // int * int;
            v_int1 = (v_leftValue.intValue * v_rightValue.intValue);
            if ((v_int1 < 0)) {
              if ((v_int1 > -257)) {
                v_value = v_INTEGER_NEGATIVE_CACHE[-v_int1];
              } else {
                v_value = new Value(v_int1);
              }
            } else {
              if ((v_int1 < 2049)) {
                v_value = v_INTEGER_POSITIVE_CACHE[v_int1];
              } else {
                v_value = new Value(v_int1);
              }
            }
            break;
          case 531:
            // int / int;
            v_int1 = v_leftValue.intValue;
            v_int2 = v_rightValue.intValue;
            if ((v_int2 == 0)) {
              v_hasInterrupt = v_EX_DivisionByZero(v_ec, "Division by 0.");
            } else {
              if ((v_int1 == 0)) {
                v_value = v_VALUE_INT_ZERO;
              } else {
                if (((v_int1 % v_int2) == 0)) {
                  v_int3 = v_int1 / v_int2;
                } else {
                  if ((((v_int1 < 0)) != ((v_int2 < 0)))) {
                    v_float1 = (1 + (-1.0 * v_int1) / v_int2);
                    v_float1 -= (v_float1 % 1.0);
                    v_int3 = ((int) (-v_float1));
                  } else {
                    v_int3 = v_int1 / v_int2;
                  }
                }
                if ((v_int3 < 0)) {
                  if ((v_int3 > -257)) {
                    v_value = v_INTEGER_NEGATIVE_CACHE[-v_int3];
                  } else {
                    v_value = new Value(v_int3);
                  }
                } else {
                  if ((v_int3 < 2049)) {
                    v_value = v_INTEGER_POSITIVE_CACHE[v_int3];
                  } else {
                    v_value = new Value(v_int3);
                  }
                }
              }
            }
            break;
          case 663:
            // float + int;
            v_value = v_buildFloat(v_globals, (((double) v_leftValue.internalValue) + v_rightValue.intValue));
            break;
          case 499:
            // int + float;
            v_value = v_buildFloat(v_globals, (v_leftValue.intValue + ((double) v_rightValue.internalValue)));
            break;
          case 664:
            // float + float;
            v_float1 = (((double) v_leftValue.internalValue) + ((double) v_rightValue.internalValue));
            if ((v_float1 == 0)) {
              v_value = v_VALUE_FLOAT_ZERO;
            } else {
              if ((v_float1 == 1)) {
                v_value = v_VALUE_FLOAT_ONE;
              } else {
                v_value = new Value(4, v_float1);
              }
            }
            break;
          case 510:
            // int - float;
            v_value = v_buildFloat(v_globals, (v_leftValue.intValue - ((double) v_rightValue.internalValue)));
            break;
          case 674:
            // float - int;
            v_value = v_buildFloat(v_globals, (((double) v_leftValue.internalValue) - v_rightValue.intValue));
            break;
          case 675:
            // float - float;
            v_float1 = (((double) v_leftValue.internalValue) - ((double) v_rightValue.internalValue));
            if ((v_float1 == 0)) {
              v_value = v_VALUE_FLOAT_ZERO;
            } else {
              if ((v_float1 == 1)) {
                v_value = v_VALUE_FLOAT_ONE;
              } else {
                v_value = new Value(4, v_float1);
              }
            }
            break;
          case 685:
            // float * int;
            v_value = v_buildFloat(v_globals, (((double) v_leftValue.internalValue) * v_rightValue.intValue));
            break;
          case 521:
            // int * float;
            v_value = v_buildFloat(v_globals, (v_leftValue.intValue * ((double) v_rightValue.internalValue)));
            break;
          case 686:
            // float * float;
            v_value = v_buildFloat(v_globals, (((double) v_leftValue.internalValue) * ((double) v_rightValue.internalValue)));
            break;
          case 532:
            // int / float;
            v_float1 = ((double) v_rightValue.internalValue);
            if ((v_float1 == 0)) {
              v_hasInterrupt = v_EX_DivisionByZero(v_ec, "Division by 0.");
            } else {
              v_value = v_buildFloat(v_globals, v_leftValue.intValue / v_float1);
            }
            break;
          case 696:
            // float / int;
            v_int1 = v_rightValue.intValue;
            if ((v_int1 == 0)) {
              v_hasInterrupt = v_EX_DivisionByZero(v_ec, "Division by 0.");
            } else {
              v_value = v_buildFloat(v_globals, ((double) v_leftValue.internalValue) / v_int1);
            }
            break;
          case 697:
            // float / float;
            v_float1 = ((double) v_rightValue.internalValue);
            if ((v_float1 == 0)) {
              v_hasInterrupt = v_EX_DivisionByZero(v_ec, "Division by 0.");
            } else {
              v_value = v_buildFloat(v_globals, ((double) v_leftValue.internalValue) / v_float1);
            }
            break;
          case 564:
            // int & int;
            v_value = v_buildInteger(v_globals, (v_leftValue.intValue & v_rightValue.intValue));
            break;
          case 575:
            // int | int;
            v_value = v_buildInteger(v_globals, (v_leftValue.intValue | v_rightValue.intValue));
            break;
          case 586:
            // int ^ int;
            v_value = v_buildInteger(v_globals, (v_leftValue.intValue ^ v_rightValue.intValue));
            break;
          case 597:
            // int << int;
            v_int1 = v_rightValue.intValue;
            if ((v_int1 < 0)) {
              v_hasInterrupt = v_EX_InvalidArgument(v_ec, "Cannot bit shift by a negative number.");
            } else {
              v_value = v_buildInteger(v_globals, (v_leftValue.intValue << v_int1));
            }
            break;
          case 608:
            // int >> int;
            v_int1 = v_rightValue.intValue;
            if ((v_int1 < 0)) {
              v_hasInterrupt = v_EX_InvalidArgument(v_ec, "Cannot bit shift by a negative number.");
            } else {
              v_value = v_buildInteger(v_globals, (v_leftValue.intValue >> v_int1));
            }
            break;
          case 619:
            // int < int;
            if ((v_leftValue.intValue < v_rightValue.intValue)) {
              v_value = v_VALUE_TRUE;
            } else {
              v_value = v_VALUE_FALSE;
            }
            break;
          case 630:
            // int <= int;
            if ((v_leftValue.intValue <= v_rightValue.intValue)) {
              v_value = v_VALUE_TRUE;
            } else {
              v_value = v_VALUE_FALSE;
            }
            break;
          case 784:
            // float < int;
            if ((((double) v_leftValue.internalValue) < v_rightValue.intValue)) {
              v_value = v_VALUE_TRUE;
            } else {
              v_value = v_VALUE_FALSE;
            }
            break;
          case 795:
            // float <= int;
            if ((((double) v_leftValue.internalValue) <= v_rightValue.intValue)) {
              v_value = v_VALUE_TRUE;
            } else {
              v_value = v_VALUE_FALSE;
            }
            break;
          case 620:
            // int < float;
            if ((v_leftValue.intValue < ((double) v_rightValue.internalValue))) {
              v_value = v_VALUE_TRUE;
            } else {
              v_value = v_VALUE_FALSE;
            }
            break;
          case 631:
            // int <= float;
            if ((v_leftValue.intValue <= ((double) v_rightValue.internalValue))) {
              v_value = v_VALUE_TRUE;
            } else {
              v_value = v_VALUE_FALSE;
            }
            break;
          case 785:
            // float < float;
            if ((((double) v_leftValue.internalValue) < ((double) v_rightValue.internalValue))) {
              v_value = v_VALUE_TRUE;
            } else {
              v_value = v_VALUE_FALSE;
            }
            break;
          case 796:
            // float <= float;
            if ((((double) v_leftValue.internalValue) <= ((double) v_rightValue.internalValue))) {
              v_value = v_VALUE_TRUE;
            } else {
              v_value = v_VALUE_FALSE;
            }
            break;
          case 652:
            // int >= int;
            if ((v_leftValue.intValue >= v_rightValue.intValue)) {
              v_value = v_VALUE_TRUE;
            } else {
              v_value = v_VALUE_FALSE;
            }
            break;
          case 641:
            // int > int;
            if ((v_leftValue.intValue > v_rightValue.intValue)) {
              v_value = v_VALUE_TRUE;
            } else {
              v_value = v_VALUE_FALSE;
            }
            break;
          case 817:
            // float >= int;
            if ((((double) v_leftValue.internalValue) >= v_rightValue.intValue)) {
              v_value = v_VALUE_TRUE;
            } else {
              v_value = v_VALUE_FALSE;
            }
            break;
          case 806:
            // float > int;
            if ((((double) v_leftValue.internalValue) > v_rightValue.intValue)) {
              v_value = v_VALUE_TRUE;
            } else {
              v_value = v_VALUE_FALSE;
            }
            break;
          case 653:
            // int >= float;
            if ((v_leftValue.intValue >= ((double) v_rightValue.internalValue))) {
              v_value = v_VALUE_TRUE;
            } else {
              v_value = v_VALUE_FALSE;
            }
            break;
          case 642:
            // int > float;
            if ((v_leftValue.intValue > ((double) v_rightValue.internalValue))) {
              v_value = v_VALUE_TRUE;
            } else {
              v_value = v_VALUE_FALSE;
            }
            break;
          case 818:
            // float >= float;
            if ((((double) v_leftValue.internalValue) >= ((double) v_rightValue.internalValue))) {
              v_value = v_VALUE_TRUE;
            } else {
              v_value = v_VALUE_FALSE;
            }
            break;
          case 807:
            // float > float;
            if ((((double) v_leftValue.internalValue) > ((double) v_rightValue.internalValue))) {
              v_value = v_VALUE_TRUE;
            } else {
              v_value = v_VALUE_FALSE;
            }
            break;
          case 830:
            // string + string;
            v_value = new Value(5, ((String) v_leftValue.internalValue) + ((String) v_rightValue.internalValue));
            break;
          case 850:
            // string * int;
            v_value = v_multiplyString(v_globals, v_leftValue, ((String) v_leftValue.internalValue), v_rightValue.intValue);
            break;
          case 522:
            // int * string;
            v_value = v_multiplyString(v_globals, v_rightValue, ((String) v_rightValue.internalValue), v_leftValue.intValue);
            break;
          case 1015:
            // list * int;
            v_int1 = v_rightValue.intValue;
            if ((v_int1 < 0)) {
              v_hasInterrupt = v_EX_UnsupportedOperation(v_ec, "Cannot multiply list by negative number.");
            } else {
              v_value = new Value(6, v_valueMultiplyList(((ListImpl) v_leftValue.internalValue), v_int1));
            }
            break;
          case 523:
            // int * list;
            v_int1 = v_leftValue.intValue;
            if ((v_int1 < 0)) {
              v_hasInterrupt = v_EX_UnsupportedOperation(v_ec, "Cannot multiply list by negative number.");
            } else {
              v_value = new Value(6, v_valueMultiplyList(((ListImpl) v_rightValue.internalValue), v_int1));
            }
            break;
          default:
            if (((v_row[0] == 0) && (((v_leftValue.type == 5) || (v_rightValue.type == 5))))) {
              v_value = new Value(5, v_valueToString(v_vm, v_leftValue) + v_valueToString(v_vm, v_rightValue));
            } else {
              // unrecognized op;
              v_hasInterrupt = v_EX_UnsupportedOperation(v_ec, "The '" + v_getBinaryOpFromId(v_row[0]) + "' operator is not supported for these types: " + v_getTypeFromId(v_leftValue.type) + " and " + v_getTypeFromId(v_rightValue.type));
            }
            break;
        }
        v_valueStack[(v_valueStackSize - 1)] = v_value;
        break;
      case 10:
        // BOOLEAN_NOT;
        v_value = v_valueStack[(v_valueStackSize - 1)];
        if ((v_value.type != 2)) {
          v_hasInterrupt = v_EX_InvalidArgument(v_ec, "Boolean expected.");
        } else {
          if ((v_value.intValue == 1)) {
            v_valueStack[(v_valueStackSize - 1)] = v_VALUE_FALSE;
          } else {
            v_valueStack[(v_valueStackSize - 1)] = v_VALUE_TRUE;
          }
        }
        break;
      case 11:
        // BREAK;
        if ((v_row[0] == 1)) {
          v_pc += v_row[1];
        } else {
          v_intArray1 = v_esfData[v_pc];
          v_pc = (v_intArray1[1] - 1);
          v_valueStackSize = v_stack.valueStackPopSize;
          v_stack.postFinallyBehavior = 1;
        }
        break;
      case 12:
        // CALL_FUNCTION;
        v_type = v_row[0];
        v_argCount = v_row[1];
        v_functionId = v_row[2];
        v_returnValueUsed = (v_row[3] == 1);
        v_classId = v_row[4];
        if (((v_type == 2) || (v_type == 6))) {
          // constructor or static method;
          v_classInfo = v_metadata.classTable[v_classId];
          v_staticConstructorNotInvoked = true;
          if ((v_classInfo.staticInitializationState < 2)) {
            v_stack.pc = v_pc;
            v_stackFrame2 = v_maybeInvokeStaticConstructor(v_vm, v_ec, v_stack, v_classInfo, v_valueStackSize, PST_intBuffer16);
            if ((PST_intBuffer16[0] == 1)) {
              return v_generateException(v_vm, v_stack, v_pc, v_valueStackSize, v_ec, 0, "Static initialization loop detected. The class this field is a member of is not done being initialized.");
            }
            if ((v_stackFrame2 != null)) {
              v_staticConstructorNotInvoked = false;
              v_stack = v_stackFrame2;
              v_pc = v_stack.pc;
              v_localsStackSetToken = v_stack.localsStackSetToken;
              v_localsStackOffset = v_stack.localsStackOffset;
            }
          }
        } else {
          v_staticConstructorNotInvoked = true;
        }
        if (v_staticConstructorNotInvoked) {
          v_bool1 = true;
          // construct args array;
          if ((v_argCount == -1)) {
            v_valueStackSize -= 1;
            v_value = v_valueStack[v_valueStackSize];
            if ((v_value.type == 1)) {
              v_argCount = 0;
            } else {
              if ((v_value.type == 6)) {
                v_list1 = ((ListImpl) v_value.internalValue);
                v_argCount = v_list1.size;
                v_i = (v_argCount - 1);
                while ((v_i >= 0)) {
                  v_funcArgs[v_i] = v_list1.array[v_i];
                  v_i -= 1;
                }
              } else {
                v_hasInterrupt = v_EX_InvalidArgument(v_ec, "Function pointers' .invoke method requires a list argument.");
              }
            }
          } else {
            v_i = (v_argCount - 1);
            while ((v_i >= 0)) {
              v_valueStackSize -= 1;
              v_funcArgs[v_i] = v_valueStack[v_valueStackSize];
              v_i -= 1;
            }
          }
          if (!v_hasInterrupt) {
            if ((v_type == 3)) {
              v_value = v_stack.objectContext;
              v_objInstance1 = ((ObjectInstance) v_value.internalValue);
              if ((v_objInstance1.classId != v_classId)) {
                v_int2 = v_row[5];
                if ((v_int2 != -1)) {
                  v_classInfo = v_classTable[v_objInstance1.classId];
                  v_functionId = v_classInfo.functionIds[v_int2];
                }
              }
            } else {
              if ((v_type == 5)) {
                // field invocation;
                v_valueStackSize -= 1;
                v_value = v_valueStack[v_valueStackSize];
                v_localeId = v_row[5];
                switch (v_value.type) {
                  case 1:
                    v_hasInterrupt = v_EX_NullReference(v_ec, "Invoked method on null.");
                    break;
                  case 8:
                    // field invoked on an object instance.;
                    v_objInstance1 = ((ObjectInstance) v_value.internalValue);
                    v_int1 = v_objInstance1.classId;
                    v_classInfo = v_classTable[v_int1];
                    v_intIntDict1 = v_classInfo.localeScopedNameIdToMemberId;
                    v_int1 = ((v_row[4] * v_magicNumbers.totalLocaleCount) + v_row[5]);
                    Integer dictLookup4 = v_intIntDict1.get(v_int1);
                    v_i = dictLookup4 == null ? (-1) : dictLookup4;
                    if ((v_i != -1)) {
                      v_int1 = v_intIntDict1.get(v_int1);
                      v_functionId = v_classInfo.functionIds[v_int1];
                      if ((v_functionId > 0)) {
                        v_type = 3;
                      } else {
                        v_value = v_objInstance1.members[v_int1];
                        v_type = 4;
                        v_valueStack[v_valueStackSize] = v_value;
                        v_valueStackSize += 1;
                      }
                    } else {
                      v_hasInterrupt = v_EX_UnknownField(v_ec, "Unknown field.");
                    }
                    break;
                  case 10:
                    // field invocation on a class object instance.;
                    v_functionId = v_resolvePrimitiveMethodName2(v_globalNameIdToPrimitiveMethodName, v_value.type, v_classId);
                    if ((v_functionId < 0)) {
                      v_hasInterrupt = v_EX_InvalidInvocation(v_ec, "Class definitions do not have that method.");
                    } else {
                      v_functionId = v_resolvePrimitiveMethodName2(v_globalNameIdToPrimitiveMethodName, v_value.type, v_classId);
                      if ((v_functionId < 0)) {
                        v_hasInterrupt = v_EX_InvalidInvocation(v_ec, v_getTypeFromId(v_value.type) + " does not have that method.");
                      } else {
                        if ((v_globalNameIdToPrimitiveMethodName[v_classId] == 8)) {
                          v_type = 6;
                          v_classValue = ((org.crayonlang.interpreter.structs.ClassValue) v_value.internalValue);
                          if (v_classValue.isInterface) {
                            v_hasInterrupt = v_EX_UnsupportedOperation(v_ec, "Cannot create an instance of an interface.");
                          } else {
                            v_classId = v_classValue.classId;
                            if (!v_returnValueUsed) {
                              v_hasInterrupt = v_EX_UnsupportedOperation(v_ec, "Cannot create an instance and not use the output.");
                            } else {
                              v_classInfo = v_metadata.classTable[v_classId];
                              v_functionId = v_classInfo.constructorFunctionId;
                            }
                          }
                        } else {
                          v_type = 9;
                        }
                      }
                    }
                    break;
                  default:
                    // primitive method suspected.;
                    v_functionId = v_resolvePrimitiveMethodName2(v_globalNameIdToPrimitiveMethodName, v_value.type, v_classId);
                    if ((v_functionId < 0)) {
                      v_hasInterrupt = v_EX_InvalidInvocation(v_ec, v_getTypeFromId(v_value.type) + " does not have that method.");
                    } else {
                      v_type = 9;
                    }
                    break;
                }
              }
            }
          }
          if (((v_type == 4) && !v_hasInterrupt)) {
            // pointer provided;
            v_valueStackSize -= 1;
            v_value = v_valueStack[v_valueStackSize];
            if ((v_value.type == 9)) {
              v_functionPointer1 = ((FunctionPointer) v_value.internalValue);
              switch (v_functionPointer1.type) {
                case 1:
                  // pointer to a function;
                  v_functionId = v_functionPointer1.functionId;
                  v_type = 1;
                  break;
                case 2:
                  // pointer to a method;
                  v_functionId = v_functionPointer1.functionId;
                  v_value = v_functionPointer1.context;
                  v_type = 3;
                  break;
                case 3:
                  // pointer to a static method;
                  v_functionId = v_functionPointer1.functionId;
                  v_classId = v_functionPointer1.classId;
                  v_type = 2;
                  break;
                case 4:
                  // pointer to a primitive method;
                  v_value = v_functionPointer1.context;
                  v_functionId = v_functionPointer1.functionId;
                  v_type = 9;
                  break;
                case 5:
                  // lambda instance;
                  v_value = v_functionPointer1.context;
                  v_functionId = v_functionPointer1.functionId;
                  v_type = 10;
                  v_closure = v_functionPointer1.closureVariables;
                  break;
              }
            } else {
              v_hasInterrupt = v_EX_InvalidInvocation(v_ec, "This type cannot be invoked like a function.");
            }
          }
          if (((v_type == 9) && !v_hasInterrupt)) {
            // primitive method invocation;
            v_output = v_VALUE_NULL;
            v_primitiveMethodToCoreLibraryFallback = false;
            switch (v_value.type) {
              case 5:
                // ...on a string;
                v_string1 = ((String) v_value.internalValue);
                switch (v_functionId) {
                  case 7:
                    if ((v_argCount != 1)) {
                      v_hasInterrupt = v_EX_InvalidArgument(v_ec, v_primitiveMethodWrongArgCountError("string contains method", 1, v_argCount));
                    } else {
                      v_value2 = v_funcArgs[0];
                      if ((v_value2.type != 5)) {
                        v_hasInterrupt = v_EX_InvalidArgument(v_ec, "string contains method requires another string as input.");
                      } else {
                        if (v_string1.contains(((String) v_value2.internalValue))) {
                          v_output = v_VALUE_TRUE;
                        } else {
                          v_output = v_VALUE_FALSE;
                        }
                      }
                    }
                    break;
                  case 9:
                    if ((v_argCount != 1)) {
                      v_hasInterrupt = v_EX_InvalidArgument(v_ec, v_primitiveMethodWrongArgCountError("string endsWith method", 1, v_argCount));
                    } else {
                      v_value2 = v_funcArgs[0];
                      if ((v_value2.type != 5)) {
                        v_hasInterrupt = v_EX_InvalidArgument(v_ec, "string endsWith method requires another string as input.");
                      } else {
                        if (v_string1.endsWith(((String) v_value2.internalValue))) {
                          v_output = v_VALUE_TRUE;
                        } else {
                          v_output = v_VALUE_FALSE;
                        }
                      }
                    }
                    break;
                  case 13:
                    if ((v_argCount != 1)) {
                      v_hasInterrupt = v_EX_InvalidArgument(v_ec, v_primitiveMethodWrongArgCountError("string indexOf method", 1, v_argCount));
                    } else {
                      v_value2 = v_funcArgs[0];
                      if ((v_value2.type != 5)) {
                        v_hasInterrupt = v_EX_InvalidArgument(v_ec, "string indexOf method requires another string as input.");
                      } else {
                        v_output = v_buildInteger(v_globals, v_string1.indexOf(((String) v_value2.internalValue)));
                      }
                    }
                    break;
                  case 19:
                    if ((v_argCount > 0)) {
                      v_hasInterrupt = v_EX_InvalidArgument(v_ec, v_primitiveMethodWrongArgCountError("string lower method", 0, v_argCount));
                    } else {
                      v_output = v_buildString(v_globals, v_string1.toLowerCase());
                    }
                    break;
                  case 20:
                    if ((v_argCount > 0)) {
                      v_hasInterrupt = v_EX_InvalidArgument(v_ec, v_primitiveMethodWrongArgCountError("string ltrim method", 0, v_argCount));
                    } else {
                      v_output = v_buildString(v_globals, PST_trimSide(v_string1, true));
                    }
                    break;
                  case 25:
                    if ((v_argCount != 2)) {
                      v_hasInterrupt = v_EX_InvalidArgument(v_ec, v_primitiveMethodWrongArgCountError("string replace method", 2, v_argCount));
                    } else {
                      v_value2 = v_funcArgs[0];
                      v_value3 = v_funcArgs[1];
                      if (((v_value2.type != 5) || (v_value3.type != 5))) {
                        v_hasInterrupt = v_EX_InvalidArgument(v_ec, "string replace method requires 2 strings as input.");
                      } else {
                        v_output = v_buildString(v_globals, v_string1.replace((CharSequence) ((String) v_value2.internalValue), (CharSequence) ((String) v_value3.internalValue)));
                      }
                    }
                    break;
                  case 26:
                    if ((v_argCount > 0)) {
                      v_hasInterrupt = v_EX_InvalidArgument(v_ec, v_primitiveMethodWrongArgCountError("string reverse method", 0, v_argCount));
                    } else {
                      v_output = v_buildString(v_globals, PST_reverseString(v_string1));
                    }
                    break;
                  case 27:
                    if ((v_argCount > 0)) {
                      v_hasInterrupt = v_EX_InvalidArgument(v_ec, v_primitiveMethodWrongArgCountError("string rtrim method", 0, v_argCount));
                    } else {
                      v_output = v_buildString(v_globals, PST_trimSide(v_string1, false));
                    }
                    break;
                  case 30:
                    if ((v_argCount != 1)) {
                      v_hasInterrupt = v_EX_InvalidArgument(v_ec, v_primitiveMethodWrongArgCountError("string split method", 1, v_argCount));
                    } else {
                      v_value2 = v_funcArgs[0];
                      if ((v_value2.type != 5)) {
                        v_hasInterrupt = v_EX_InvalidArgument(v_ec, "string split method requires another string as input.");
                      } else {
                        v_stringList = PST_literalStringSplit(v_string1, ((String) v_value2.internalValue));
                        v_len = v_stringList.length;
                        v_list1 = v_makeEmptyList(v_globals.stringType, v_len);
                        v_i = 0;
                        while ((v_i < v_len)) {
                          v_list1.array[v_i] = v_buildString(v_globals, v_stringList[v_i]);
                          v_i += 1;
                        }
                        v_list1.size = v_len;
                        v_output = new Value(6, v_list1);
                      }
                    }
                    break;
                  case 31:
                    if ((v_argCount != 1)) {
                      v_hasInterrupt = v_EX_InvalidArgument(v_ec, v_primitiveMethodWrongArgCountError("string startsWith method", 1, v_argCount));
                    } else {
                      v_value2 = v_funcArgs[0];
                      if ((v_value2.type != 5)) {
                        v_hasInterrupt = v_EX_InvalidArgument(v_ec, "string startsWith method requires another string as input.");
                      } else {
                        if (v_string1.startsWith(((String) v_value2.internalValue))) {
                          v_output = v_VALUE_TRUE;
                        } else {
                          v_output = v_VALUE_FALSE;
                        }
                      }
                    }
                    break;
                  case 32:
                    if ((v_argCount > 0)) {
                      v_hasInterrupt = v_EX_InvalidArgument(v_ec, v_primitiveMethodWrongArgCountError("string trim method", 0, v_argCount));
                    } else {
                      v_output = v_buildString(v_globals, v_string1.trim());
                    }
                    break;
                  case 33:
                    if ((v_argCount > 0)) {
                      v_hasInterrupt = v_EX_InvalidArgument(v_ec, v_primitiveMethodWrongArgCountError("string upper method", 0, v_argCount));
                    } else {
                      v_output = v_buildString(v_globals, v_string1.toUpperCase());
                    }
                    break;
                  default:
                    v_output = null;
                    break;
                }
                break;
              case 6:
                // ...on a list;
                v_list1 = ((ListImpl) v_value.internalValue);
                switch (v_functionId) {
                  case 0:
                    if ((v_argCount == 0)) {
                      v_hasInterrupt = v_EX_InvalidArgument(v_ec, "List add method requires at least one argument.");
                    } else {
                      v_intArray1 = v_list1.type;
                      while (((v_list1.size + v_argCount) > v_list1.capacity)) {
                        v_increaseListCapacity(v_list1);
                      }
                      v_int1 = v_list1.size;
                      v_i = 0;
                      while ((v_i < v_argCount)) {
                        v_value = v_funcArgs[v_i];
                        if ((v_intArray1 != null)) {
                          v_value2 = v_canAssignTypeToGeneric(v_vm, v_value, v_intArray1, 0);
                          if ((v_value2 == null)) {
                            v_hasInterrupt = v_EX_InvalidArgument(v_ec, "Cannot convert a " + v_typeToStringFromValue(v_vm, v_value) + " into a " + v_typeToString(v_vm, v_list1.type, 0));
                          }
                          v_list1.array[(v_int1 + v_i)] = v_value2;
                        } else {
                          v_list1.array[(v_int1 + v_i)] = v_value;
                        }
                        v_i += 1;
                      }
                      v_list1.size += v_argCount;
                      v_output = v_VALUE_NULL;
                    }
                    break;
                  case 3:
                    if ((v_argCount > 0)) {
                      v_hasInterrupt = v_EX_InvalidArgument(v_ec, v_primitiveMethodWrongArgCountError("list choice method", 0, v_argCount));
                    } else {
                      v_len = v_list1.size;
                      if ((v_len == 0)) {
                        v_hasInterrupt = v_EX_UnsupportedOperation(v_ec, "Cannot use list.choice() method on an empty list.");
                      } else {
                        v_i = ((int) ((PST_random.nextDouble() * v_len)));
                        v_output = v_list1.array[v_i];
                      }
                    }
                    break;
                  case 4:
                    if ((v_argCount > 0)) {
                      v_hasInterrupt = v_EX_InvalidArgument(v_ec, v_primitiveMethodWrongArgCountError("list clear method", 0, v_argCount));
                    } else {
                      if ((v_list1.size > 0)) {
                        v_i = (v_list1.size - 1);
                        while ((v_i >= 0)) {
                          v_list1.array[v_i] = null;
                          v_i -= 1;
                        }
                        v_list1.size = 0;
                      }
                    }
                    break;
                  case 5:
                    if ((v_argCount > 0)) {
                      v_hasInterrupt = v_EX_InvalidArgument(v_ec, v_primitiveMethodWrongArgCountError("list clone method", 0, v_argCount));
                    } else {
                      v_len = v_list1.size;
                      v_list2 = v_makeEmptyList(v_list1.type, v_len);
                      v_i = 0;
                      while ((v_i < v_len)) {
                        v_list2.array[v_i] = v_list1.array[v_i];
                        v_i += 1;
                      }
                      v_list2.size = v_len;
                      v_output = new Value(6, v_list2);
                    }
                    break;
                  case 6:
                    if ((v_argCount != 1)) {
                      v_hasInterrupt = v_EX_InvalidArgument(v_ec, v_primitiveMethodWrongArgCountError("list concat method", 1, v_argCount));
                    } else {
                      v_value2 = v_funcArgs[0];
                      if ((v_value2.type != 6)) {
                        v_hasInterrupt = v_EX_InvalidArgument(v_ec, "list concat methods requires a list as an argument.");
                      } else {
                        v_list2 = ((ListImpl) v_value2.internalValue);
                        v_intArray1 = v_list1.type;
                        if (((v_intArray1 != null) && !v_canAssignGenericToGeneric(v_vm, v_list2.type, 0, v_intArray1, 0, v_intBuffer))) {
                          v_hasInterrupt = v_EX_InvalidArgument(v_ec, "Cannot concat a list: incompatible types.");
                        } else {
                          if (((v_intArray1 != null) && (v_intArray1[0] == 4) && (v_list2.type[0] == 3))) {
                            v_bool1 = true;
                          } else {
                            v_bool1 = false;
                          }
                          v_len = v_list2.size;
                          v_int1 = v_list1.size;
                          while (((v_int1 + v_len) > v_list1.capacity)) {
                            v_increaseListCapacity(v_list1);
                          }
                          v_i = 0;
                          while ((v_i < v_len)) {
                            v_value = v_list2.array[v_i];
                            if (v_bool1) {
                              v_value = v_buildFloat(v_globals, (0.0 + v_value.intValue));
                            }
                            v_list1.array[(v_int1 + v_i)] = v_value;
                            v_i += 1;
                          }
                          v_list1.size += v_len;
                        }
                      }
                    }
                    break;
                  case 7:
                    if ((v_argCount != 1)) {
                      v_hasInterrupt = v_EX_InvalidArgument(v_ec, v_primitiveMethodWrongArgCountError("list contains method", 1, v_argCount));
                    } else {
                      v_value2 = v_funcArgs[0];
                      v_len = v_list1.size;
                      v_output = v_VALUE_FALSE;
                      v_i = 0;
                      while ((v_i < v_len)) {
                        v_value = v_list1.array[v_i];
                        if ((v_doEqualityComparisonAndReturnCode(v_value2, v_value) == 1)) {
                          v_output = v_VALUE_TRUE;
                          v_i = v_len;
                        }
                        v_i += 1;
                      }
                    }
                    break;
                  case 10:
                    if ((v_argCount != 1)) {
                      v_hasInterrupt = v_EX_InvalidArgument(v_ec, v_primitiveMethodWrongArgCountError("list filter method", 1, v_argCount));
                    } else {
                      v_value2 = v_funcArgs[0];
                      if ((v_value2.type != 9)) {
                        v_hasInterrupt = v_EX_InvalidArgument(v_ec, "list filter method requires a function pointer as its argument.");
                      } else {
                        v_primitiveMethodToCoreLibraryFallback = true;
                        v_functionId = v_metadata.primitiveMethodFunctionIdFallbackLookup[0];
                        v_funcArgs[1] = v_value;
                        v_argCount = 2;
                        v_output = null;
                      }
                    }
                    break;
                  case 14:
                    if ((v_argCount != 2)) {
                      v_hasInterrupt = v_EX_InvalidArgument(v_ec, v_primitiveMethodWrongArgCountError("list insert method", 1, v_argCount));
                    } else {
                      v_value = v_funcArgs[0];
                      v_value2 = v_funcArgs[1];
                      if ((v_value.type != 3)) {
                        v_hasInterrupt = v_EX_InvalidArgument(v_ec, "First argument of list.insert must be an integer index.");
                      } else {
                        v_intArray1 = v_list1.type;
                        if ((v_intArray1 != null)) {
                          v_value3 = v_canAssignTypeToGeneric(v_vm, v_value2, v_intArray1, 0);
                          if ((v_value3 == null)) {
                            v_hasInterrupt = v_EX_InvalidArgument(v_ec, "Cannot insert this type into this type of list.");
                          }
                          v_value2 = v_value3;
                        }
                        if (!v_hasInterrupt) {
                          if ((v_list1.size == v_list1.capacity)) {
                            v_increaseListCapacity(v_list1);
                          }
                          v_int1 = v_value.intValue;
                          v_len = v_list1.size;
                          if ((v_int1 < 0)) {
                            v_int1 += v_len;
                          }
                          if ((v_int1 == v_len)) {
                            v_list1.array[v_len] = v_value2;
                            v_list1.size += 1;
                          } else {
                            if (((v_int1 < 0) || (v_int1 >= v_len))) {
                              v_hasInterrupt = v_EX_IndexOutOfRange(v_ec, "Index out of range.");
                            } else {
                              v_i = v_int1;
                              while ((v_i < v_len)) {
                                v_value3 = v_list1.array[v_i];
                                v_list1.array[v_i] = v_value2;
                                v_value2 = v_value3;
                                v_i += 1;
                              }
                              v_list1.array[v_len] = v_value2;
                              v_list1.size += 1;
                            }
                          }
                        }
                      }
                    }
                    break;
                  case 17:
                    if ((v_argCount != 1)) {
                      if ((v_argCount == 0)) {
                        v_value2 = v_globals.stringEmpty;
                      } else {
                        v_hasInterrupt = v_EX_InvalidArgument(v_ec, v_primitiveMethodWrongArgCountError("list join method", 1, v_argCount));
                      }
                    } else {
                      v_value2 = v_funcArgs[0];
                      if ((v_value2.type != 5)) {
                        v_hasInterrupt = v_EX_InvalidArgument(v_ec, "Argument of list.join needs to be a string.");
                      }
                    }
                    if (!v_hasInterrupt) {
                      v_stringList1 = new ArrayList<String>();
                      v_string1 = ((String) v_value2.internalValue);
                      v_len = v_list1.size;
                      v_i = 0;
                      while ((v_i < v_len)) {
                        v_value = v_list1.array[v_i];
                        if ((v_value.type != 5)) {
                          v_string2 = v_valueToString(v_vm, v_value);
                        } else {
                          v_string2 = ((String) v_value.internalValue);
                        }
                        v_stringList1.add(v_string2);
                        v_i += 1;
                      }
                      v_output = v_buildString(v_globals, PST_joinList(v_string1, v_stringList1));
                    }
                    break;
                  case 21:
                    if ((v_argCount != 1)) {
                      v_hasInterrupt = v_EX_InvalidArgument(v_ec, v_primitiveMethodWrongArgCountError("list map method", 1, v_argCount));
                    } else {
                      v_value2 = v_funcArgs[0];
                      if ((v_value2.type != 9)) {
                        v_hasInterrupt = v_EX_InvalidArgument(v_ec, "list map method requires a function pointer as its argument.");
                      } else {
                        v_primitiveMethodToCoreLibraryFallback = true;
                        v_functionId = v_metadata.primitiveMethodFunctionIdFallbackLookup[1];
                        v_funcArgs[1] = v_value;
                        v_argCount = 2;
                        v_output = null;
                      }
                    }
                    break;
                  case 23:
                    if ((v_argCount > 0)) {
                      v_hasInterrupt = v_EX_InvalidArgument(v_ec, v_primitiveMethodWrongArgCountError("list pop method", 0, v_argCount));
                    } else {
                      v_len = v_list1.size;
                      if ((v_len < 1)) {
                        v_hasInterrupt = v_EX_IndexOutOfRange(v_ec, "Cannot pop from an empty list.");
                      } else {
                        v_len -= 1;
                        v_value = v_list1.array[v_len];
                        v_list1.array[v_len] = null;
                        if (v_returnValueUsed) {
                          v_output = v_value;
                        }
                        v_list1.size = v_len;
                      }
                    }
                    break;
                  case 24:
                    if ((v_argCount != 1)) {
                      v_hasInterrupt = v_EX_InvalidArgument(v_ec, v_primitiveMethodWrongArgCountError("list remove method", 1, v_argCount));
                    } else {
                      v_value = v_funcArgs[0];
                      if ((v_value.type != 3)) {
                        v_hasInterrupt = v_EX_InvalidArgument(v_ec, "Argument of list.remove must be an integer index.");
                      } else {
                        v_int1 = v_value.intValue;
                        v_len = v_list1.size;
                        if ((v_int1 < 0)) {
                          v_int1 += v_len;
                        }
                        if (((v_int1 < 0) || (v_int1 >= v_len))) {
                          v_hasInterrupt = v_EX_IndexOutOfRange(v_ec, "Index out of range.");
                        } else {
                          if (v_returnValueUsed) {
                            v_output = v_list1.array[v_int1];
                          }
                          v_len = (v_list1.size - 1);
                          v_list1.size = v_len;
                          v_i = v_int1;
                          while ((v_i < v_len)) {
                            v_list1.array[v_i] = v_list1.array[(v_i + 1)];
                            v_i += 1;
                          }
                          v_list1.array[v_len] = null;
                        }
                      }
                    }
                    break;
                  case 26:
                    if ((v_argCount > 0)) {
                      v_hasInterrupt = v_EX_InvalidArgument(v_ec, v_primitiveMethodWrongArgCountError("list reverse method", 0, v_argCount));
                    } else {
                      v_reverseList(v_list1);
                    }
                    break;
                  case 28:
                    if ((v_argCount > 0)) {
                      v_hasInterrupt = v_EX_InvalidArgument(v_ec, v_primitiveMethodWrongArgCountError("list shuffle method", 0, v_argCount));
                    } else {
                      v_len = v_list1.size;
                      v_i = 0;
                      while ((v_i < v_len)) {
                        v_j = ((int) ((PST_random.nextDouble() * v_len)));
                        v_value = v_list1.array[v_i];
                        v_list1.array[v_i] = v_list1.array[v_j];
                        v_list1.array[v_j] = v_value;
                        v_i += 1;
                      }
                    }
                    break;
                  case 29:
                    if ((v_argCount == 0)) {
                      v_sortLists(v_list1, v_list1, PST_intBuffer16);
                      if ((PST_intBuffer16[0] > 0)) {
                        v_hasInterrupt = v_EX_InvalidArgument(v_ec, "Invalid list to sort. All items must be numbers or all strings, but not mixed.");
                      }
                    } else {
                      if ((v_argCount == 1)) {
                        v_value2 = v_funcArgs[0];
                        if ((v_value2.type == 9)) {
                          v_primitiveMethodToCoreLibraryFallback = true;
                          v_functionId = v_metadata.primitiveMethodFunctionIdFallbackLookup[2];
                          v_funcArgs[1] = v_value;
                          v_argCount = 2;
                        } else {
                          v_hasInterrupt = v_EX_InvalidArgument(v_ec, "list.sort(get_key_function) requires a function pointer as its argument.");
                        }
                        v_output = null;
                      }
                    }
                    break;
                  default:
                    v_output = null;
                    break;
                }
                break;
              case 7:
                // ...on a dictionary;
                v_dictImpl = ((DictImpl) v_value.internalValue);
                switch (v_functionId) {
                  case 4:
                    if ((v_argCount > 0)) {
                      v_hasInterrupt = v_EX_InvalidArgument(v_ec, v_primitiveMethodWrongArgCountError("dictionary clear method", 0, v_argCount));
                    } else {
                      if ((v_dictImpl.size > 0)) {
                        v_dictImpl.intToIndex = new HashMap<Integer, Integer>();
                        v_dictImpl.stringToIndex = new HashMap<String, Integer>();
                        v_dictImpl.keys.clear();
                        v_dictImpl.values.clear();
                        v_dictImpl.size = 0;
                      }
                    }
                    break;
                  case 5:
                    if ((v_argCount > 0)) {
                      v_hasInterrupt = v_EX_InvalidArgument(v_ec, v_primitiveMethodWrongArgCountError("dictionary clone method", 0, v_argCount));
                    } else {
                      v_output = new Value(7, v_cloneDictionary(v_dictImpl, null));
                    }
                    break;
                  case 7:
                    if ((v_argCount != 1)) {
                      v_hasInterrupt = v_EX_InvalidArgument(v_ec, v_primitiveMethodWrongArgCountError("dictionary contains method", 1, v_argCount));
                    } else {
                      v_value = v_funcArgs[0];
                      v_output = v_VALUE_FALSE;
                      if ((v_value.type == 5)) {
                        if (v_dictImpl.stringToIndex.containsKey(((String) v_value.internalValue))) {
                          v_output = v_VALUE_TRUE;
                        }
                      } else {
                        if ((v_value.type == 3)) {
                          v_i = v_value.intValue;
                        } else {
                          v_i = (((ObjectInstance) v_value.internalValue)).objectId;
                        }
                        if (v_dictImpl.intToIndex.containsKey(v_i)) {
                          v_output = v_VALUE_TRUE;
                        }
                      }
                    }
                    break;
                  case 11:
                    if (((v_argCount != 1) && (v_argCount != 2))) {
                      v_hasInterrupt = v_EX_InvalidArgument(v_ec, "Dictionary get method requires 1 or 2 arguments.");
                    } else {
                      v_value = v_funcArgs[0];
                      switch (v_value.type) {
                        case 3:
                          v_int1 = v_value.intValue;
                          Integer dictLookup5 = v_dictImpl.intToIndex.get(v_int1);
                          v_i = dictLookup5 == null ? (-1) : dictLookup5;
                          break;
                        case 8:
                          v_int1 = (((ObjectInstance) v_value.internalValue)).objectId;
                          Integer dictLookup6 = v_dictImpl.intToIndex.get(v_int1);
                          v_i = dictLookup6 == null ? (-1) : dictLookup6;
                          break;
                        case 5:
                          v_string1 = ((String) v_value.internalValue);
                          Integer dictLookup7 = v_dictImpl.stringToIndex.get(v_string1);
                          v_i = dictLookup7 == null ? (v_dictImpl.stringToIndex.containsKey(v_string1) ? null : (-1)) : dictLookup7;
                          break;
                      }
                      if ((v_i == -1)) {
                        if ((v_argCount == 2)) {
                          v_output = v_funcArgs[1];
                        } else {
                          v_output = v_VALUE_NULL;
                        }
                      } else {
                        v_output = v_dictImpl.values.get(v_i);
                      }
                    }
                    break;
                  case 18:
                    if ((v_argCount > 0)) {
                      v_hasInterrupt = v_EX_InvalidArgument(v_ec, v_primitiveMethodWrongArgCountError("dictionary keys method", 0, v_argCount));
                    } else {
                      v_valueList1 = v_dictImpl.keys;
                      v_len = v_valueList1.size();
                      if ((v_dictImpl.keyType == 8)) {
                        v_intArray1 = new int[2];
                        v_intArray1[0] = 8;
                        v_intArray1[0] = v_dictImpl.keyClassId;
                      } else {
                        v_intArray1 = new int[1];
                        v_intArray1[0] = v_dictImpl.keyType;
                      }
                      v_list1 = v_makeEmptyList(v_intArray1, v_len);
                      v_i = 0;
                      while ((v_i < v_len)) {
                        v_list1.array[v_i] = v_valueList1.get(v_i);
                        v_i += 1;
                      }
                      v_list1.size = v_len;
                      v_output = new Value(6, v_list1);
                    }
                    break;
                  case 22:
                    if ((v_argCount != 1)) {
                      v_hasInterrupt = v_EX_InvalidArgument(v_ec, v_primitiveMethodWrongArgCountError("dictionary merge method", 1, v_argCount));
                    } else {
                      v_value2 = v_funcArgs[0];
                      if ((v_value2.type != 7)) {
                        v_hasInterrupt = v_EX_InvalidArgument(v_ec, "dictionary merge method requires another dictionary as a parameeter.");
                      } else {
                        v_dictImpl2 = ((DictImpl) v_value2.internalValue);
                        if ((v_dictImpl2.size > 0)) {
                          if ((v_dictImpl.size == 0)) {
                            v_value.internalValue = v_cloneDictionary(v_dictImpl2, null);
                          } else {
                            if ((v_dictImpl2.keyType != v_dictImpl.keyType)) {
                              v_hasInterrupt = v_EX_InvalidKey(v_ec, "Dictionaries with different key types cannot be merged.");
                            } else {
                              if (((v_dictImpl2.keyType == 8) && (v_dictImpl2.keyClassId != v_dictImpl.keyClassId) && (v_dictImpl.keyClassId != 0) && !v_isClassASubclassOf(v_vm, v_dictImpl2.keyClassId, v_dictImpl.keyClassId))) {
                                v_hasInterrupt = v_EX_InvalidKey(v_ec, "Dictionary key types are incompatible.");
                              } else {
                                if ((v_dictImpl.valueType == null)) {
                                } else {
                                  if ((v_dictImpl2.valueType == null)) {
                                    v_hasInterrupt = v_EX_InvalidKey(v_ec, "Dictionaries with different value types cannot be merged.");
                                  } else {
                                    if (!v_canAssignGenericToGeneric(v_vm, v_dictImpl2.valueType, 0, v_dictImpl.valueType, 0, v_intBuffer)) {
                                      v_hasInterrupt = v_EX_InvalidKey(v_ec, "The dictionary value types are incompatible.");
                                    }
                                  }
                                }
                                if (!v_hasInterrupt) {
                                  v_cloneDictionary(v_dictImpl2, v_dictImpl);
                                }
                              }
                            }
                          }
                        }
                        v_output = v_VALUE_NULL;
                      }
                    }
                    break;
                  case 24:
                    if ((v_argCount != 1)) {
                      v_hasInterrupt = v_EX_InvalidArgument(v_ec, v_primitiveMethodWrongArgCountError("dictionary remove method", 1, v_argCount));
                    } else {
                      v_value2 = v_funcArgs[0];
                      v_bool2 = false;
                      v_keyType = v_dictImpl.keyType;
                      if (((v_dictImpl.size > 0) && (v_keyType == v_value2.type))) {
                        if ((v_keyType == 5)) {
                          v_stringKey = ((String) v_value2.internalValue);
                          if (v_dictImpl.stringToIndex.containsKey(v_stringKey)) {
                            v_i = v_dictImpl.stringToIndex.get(v_stringKey);
                            v_bool2 = true;
                          }
                        } else {
                          if ((v_keyType == 3)) {
                            v_intKey = v_value2.intValue;
                          } else {
                            v_intKey = (((ObjectInstance) v_value2.internalValue)).objectId;
                          }
                          if (v_dictImpl.intToIndex.containsKey(v_intKey)) {
                            v_i = v_dictImpl.intToIndex.get(v_intKey);
                            v_bool2 = true;
                          }
                        }
                        if (v_bool2) {
                          v_len = (v_dictImpl.size - 1);
                          v_dictImpl.size = v_len;
                          if ((v_i == v_len)) {
                            if ((v_keyType == 5)) {
                              v_dictImpl.stringToIndex.remove(v_stringKey);
                            } else {
                              v_dictImpl.intToIndex.remove(v_intKey);
                            }
                            v_dictImpl.keys.remove(v_i);
                            v_dictImpl.values.remove(v_i);
                          } else {
                            v_value = v_dictImpl.keys.get(v_len);
                            v_dictImpl.keys.set(v_i, v_value);
                            v_dictImpl.values.set(v_i, v_dictImpl.values.get(v_len));
                            v_dictImpl.keys.remove(v_dictImpl.keys.size() - 1);
                            v_dictImpl.values.remove(v_dictImpl.values.size() - 1);
                            if ((v_keyType == 5)) {
                              v_dictImpl.stringToIndex.remove(v_stringKey);
                              v_stringKey = ((String) v_value.internalValue);
                              v_dictImpl.stringToIndex.put(v_stringKey, v_i);
                            } else {
                              v_dictImpl.intToIndex.remove(v_intKey);
                              if ((v_keyType == 3)) {
                                v_intKey = v_value.intValue;
                              } else {
                                v_intKey = (((ObjectInstance) v_value.internalValue)).objectId;
                              }
                              v_dictImpl.intToIndex.put(v_intKey, v_i);
                            }
                          }
                        }
                      }
                      if (!v_bool2) {
                        v_hasInterrupt = v_EX_KeyNotFound(v_ec, "dictionary does not contain the given key.");
                      }
                    }
                    break;
                  case 34:
                    if ((v_argCount > 0)) {
                      v_hasInterrupt = v_EX_InvalidArgument(v_ec, v_primitiveMethodWrongArgCountError("dictionary values method", 0, v_argCount));
                    } else {
                      v_valueList1 = v_dictImpl.values;
                      v_len = v_valueList1.size();
                      v_list1 = v_makeEmptyList(v_dictImpl.valueType, v_len);
                      v_i = 0;
                      while ((v_i < v_len)) {
                        v_addToList(v_list1, v_valueList1.get(v_i));
                        v_i += 1;
                      }
                      v_output = new Value(6, v_list1);
                    }
                    break;
                  default:
                    v_output = null;
                    break;
                }
                break;
              case 9:
                // ...on a function pointer;
                v_functionPointer1 = ((FunctionPointer) v_value.internalValue);
                switch (v_functionId) {
                  case 1:
                    if ((v_argCount > 0)) {
                      v_hasInterrupt = v_EX_InvalidArgument(v_ec, v_primitiveMethodWrongArgCountError("argCountMax method", 0, v_argCount));
                    } else {
                      v_functionId = v_functionPointer1.functionId;
                      v_functionInfo = v_metadata.functionTable[v_functionId];
                      v_output = v_buildInteger(v_globals, v_functionInfo.maxArgs);
                    }
                    break;
                  case 2:
                    if ((v_argCount > 0)) {
                      v_hasInterrupt = v_EX_InvalidArgument(v_ec, v_primitiveMethodWrongArgCountError("argCountMin method", 0, v_argCount));
                    } else {
                      v_functionId = v_functionPointer1.functionId;
                      v_functionInfo = v_metadata.functionTable[v_functionId];
                      v_output = v_buildInteger(v_globals, v_functionInfo.minArgs);
                    }
                    break;
                  case 12:
                    v_functionInfo = v_metadata.functionTable[v_functionPointer1.functionId];
                    v_output = v_buildString(v_globals, v_functionInfo.name);
                    break;
                  case 15:
                    if ((v_argCount == 1)) {
                      v_funcArgs[1] = v_funcArgs[0];
                    } else {
                      if ((v_argCount == 0)) {
                        v_funcArgs[1] = v_VALUE_NULL;
                      } else {
                        v_hasInterrupt = v_EX_InvalidArgument(v_ec, "invoke requires a list of arguments.");
                      }
                    }
                    v_funcArgs[0] = v_value;
                    v_argCount = 2;
                    v_primitiveMethodToCoreLibraryFallback = true;
                    v_functionId = v_metadata.primitiveMethodFunctionIdFallbackLookup[3];
                    v_output = null;
                    break;
                  default:
                    v_output = null;
                    break;
                }
                break;
              case 10:
                // ...on a class definition;
                v_classValue = ((org.crayonlang.interpreter.structs.ClassValue) v_value.internalValue);
                switch (v_functionId) {
                  case 12:
                    v_classInfo = v_metadata.classTable[v_classValue.classId];
                    v_output = v_buildString(v_globals, v_classInfo.fullyQualifiedName);
                    break;
                  case 16:
                    if ((v_argCount != 1)) {
                      v_hasInterrupt = v_EX_InvalidArgument(v_ec, v_primitiveMethodWrongArgCountError("class isA method", 1, v_argCount));
                    } else {
                      v_int1 = v_classValue.classId;
                      v_value = v_funcArgs[0];
                      if ((v_value.type != 10)) {
                        v_hasInterrupt = v_EX_InvalidArgument(v_ec, "class isA method requires another class reference.");
                      } else {
                        v_classValue = ((org.crayonlang.interpreter.structs.ClassValue) v_value.internalValue);
                        v_int2 = v_classValue.classId;
                        v_output = v_VALUE_FALSE;
                        if (v_isClassASubclassOf(v_vm, v_int1, v_int2)) {
                          v_output = v_VALUE_TRUE;
                        }
                      }
                    }
                    break;
                  default:
                    v_output = null;
                    break;
                }
                break;
            }
            if (!v_hasInterrupt) {
              if ((v_output == null)) {
                if (v_primitiveMethodToCoreLibraryFallback) {
                  v_type = 1;
                  v_bool1 = true;
                } else {
                  v_hasInterrupt = v_EX_InvalidInvocation(v_ec, "primitive method not found.");
                }
              } else {
                if (v_returnValueUsed) {
                  if ((v_valueStackSize == v_valueStackCapacity)) {
                    v_valueStack = v_valueStackIncreaseCapacity(v_ec);
                    v_valueStackCapacity = v_valueStack.length;
                  }
                  v_valueStack[v_valueStackSize] = v_output;
                  v_valueStackSize += 1;
                }
                v_bool1 = false;
              }
            }
          }
          if ((v_bool1 && !v_hasInterrupt)) {
            // push a new frame to the stack;
            v_stack.pc = v_pc;
            v_bool1 = false;
            switch (v_type) {
              case 1:
                // function;
                v_functionInfo = v_functionTable[v_functionId];
                v_pc = v_functionInfo.pc;
                v_value = null;
                v_classId = 0;
                break;
              case 10:
                // lambda;
                v_pc = v_functionId;
                v_functionInfo = v_metadata.lambdaTable.get(v_functionId);
                v_value = null;
                v_classId = 0;
                break;
              case 2:
                // static method;
                v_functionInfo = v_functionTable[v_functionId];
                v_pc = v_functionInfo.pc;
                v_value = null;
                v_classId = 0;
                break;
              case 3:
                // non-static method;
                v_functionInfo = v_functionTable[v_functionId];
                v_pc = v_functionInfo.pc;
                v_classId = 0;
                break;
              case 6:
                // constructor;
                v_vm.instanceCounter += 1;
                v_classInfo = v_classTable[v_classId];
                v_valueArray1 = new Value[v_classInfo.memberCount];
                v_i = (v_valueArray1.length - 1);
                while ((v_i >= 0)) {
                  switch (v_classInfo.fieldInitializationCommand[v_i]) {
                    case 0:
                      v_valueArray1[v_i] = v_classInfo.fieldInitializationLiteral[v_i];
                      break;
                    case 1:
                      break;
                    case 2:
                      break;
                  }
                  v_i -= 1;
                }
                v_objInstance1 = new ObjectInstance(v_classId, v_vm.instanceCounter, v_valueArray1, null, null);
                v_value = new Value(8, v_objInstance1);
                v_functionId = v_classInfo.constructorFunctionId;
                v_functionInfo = v_functionTable[v_functionId];
                v_pc = v_functionInfo.pc;
                v_classId = 0;
                if (v_returnValueUsed) {
                  v_returnValueUsed = false;
                  if ((v_valueStackSize == v_valueStackCapacity)) {
                    v_valueStack = v_valueStackIncreaseCapacity(v_ec);
                    v_valueStackCapacity = v_valueStack.length;
                  }
                  v_valueStack[v_valueStackSize] = v_value;
                  v_valueStackSize += 1;
                }
                break;
              case 7:
                // base constructor;
                v_value = v_stack.objectContext;
                v_classInfo = v_classTable[v_classId];
                v_functionId = v_classInfo.constructorFunctionId;
                v_functionInfo = v_functionTable[v_functionId];
                v_pc = v_functionInfo.pc;
                v_classId = 0;
                break;
            }
            if (((v_argCount < v_functionInfo.minArgs) || (v_argCount > v_functionInfo.maxArgs))) {
              v_pc = v_stack.pc;
              v_hasInterrupt = v_EX_InvalidArgument(v_ec, "Incorrect number of args were passed to this function.");
            } else {
              v_int1 = v_functionInfo.localsSize;
              v_int2 = v_stack.localsStackOffsetEnd;
              if ((v_localsStackCapacity <= (v_int2 + v_int1))) {
                v_increaseLocalsStackCapacity(v_ec, v_int1);
                v_localsStack = v_ec.localsStack;
                v_localsStackSet = v_ec.localsStackSet;
                v_localsStackCapacity = v_localsStack.length;
              }
              v_localsStackSetToken = (v_ec.localsStackSetToken + 1);
              v_ec.localsStackSetToken = v_localsStackSetToken;
              if ((v_localsStackSetToken > 2000000000)) {
                v_resetLocalsStackTokens(v_ec, v_stack);
                v_localsStackSetToken = 2;
              }
              v_localsStackOffset = v_int2;
              if ((v_type != 10)) {
                v_closure = null;
              }
              // invoke the function;
              v_stack = new StackFrame(v_pc, v_localsStackSetToken, v_localsStackOffset, (v_localsStackOffset + v_int1), v_stack, v_returnValueUsed, v_value, v_valueStackSize, 0, (v_stack.depth + 1), 0, null, v_closure, null);
              v_i = 0;
              while ((v_i < v_argCount)) {
                v_int1 = (v_localsStackOffset + v_i);
                v_localsStack[v_int1] = v_funcArgs[v_i];
                v_localsStackSet[v_int1] = v_localsStackSetToken;
                v_i += 1;
              }
              if ((v_argCount != v_functionInfo.minArgs)) {
                v_int1 = (v_argCount - v_functionInfo.minArgs);
                if ((v_int1 > 0)) {
                  v_pc += v_functionInfo.pcOffsetsForOptionalArgs[v_int1];
                  v_stack.pc = v_pc;
                }
              }
              if ((v_stack.depth > 1000)) {
                v_hasInterrupt = v_EX_Fatal(v_ec, "Stack overflow.");
              }
            }
          }
        }
        break;
      case 13:
        // CAST;
        v_value = v_valueStack[(v_valueStackSize - 1)];
        v_value2 = v_canAssignTypeToGeneric(v_vm, v_value, v_row, 0);
        if ((v_value2 == null)) {
          if (((v_value.type == 4) && (v_row[0] == 3))) {
            if ((v_row[1] == 1)) {
              v_float1 = ((double) v_value.internalValue);
              if (((v_float1 < 0) && ((v_float1 % 1) != 0))) {
                v_i = (((int) v_float1) - 1);
              } else {
                v_i = ((int) v_float1);
              }
              if ((v_i < 0)) {
                if ((v_i > -257)) {
                  v_value2 = v_globals.negativeIntegers[-v_i];
                } else {
                  v_value2 = new Value(v_i);
                }
              } else {
                if ((v_i < 2049)) {
                  v_value2 = v_globals.positiveIntegers[v_i];
                } else {
                  v_value2 = new Value(v_i);
                }
              }
            }
          } else {
            if (((v_value.type == 3) && (v_row[0] == 4))) {
              v_int1 = v_value.intValue;
              if ((v_int1 == 0)) {
                v_value2 = v_VALUE_FLOAT_ZERO;
              } else {
                v_value2 = new Value(4, (0.0 + v_int1));
              }
            }
          }
          if ((v_value2 != null)) {
            v_valueStack[(v_valueStackSize - 1)] = v_value2;
          }
        }
        if ((v_value2 == null)) {
          v_hasInterrupt = v_EX_InvalidArgument(v_ec, "Cannot convert a " + v_typeToStringFromValue(v_vm, v_value) + " to a " + v_typeToString(v_vm, v_row, 0));
        } else {
          v_valueStack[(v_valueStackSize - 1)] = v_value2;
        }
        break;
      case 14:
        // CLASS_DEFINITION;
        v_initializeClass(v_pc, v_vm, v_row, v_stringArgs[v_pc]);
        v_classTable = v_metadata.classTable;
        break;
      case 15:
        // CNI_INVOKE;
        v_nativeFp = v_metadata.cniFunctionsById.get(v_row[0]);
        if ((v_nativeFp == null)) {
          v_hasInterrupt = v_EX_InvalidInvocation(v_ec, "CNI method could not be found.");
        } else {
          v_len = v_row[1];
          v_valueStackSize -= v_len;
          v_valueArray1 = new Value[v_len];
          v_i = 0;
          while ((v_i < v_len)) {
            v_valueArray1[v_i] = v_valueStack[(v_valueStackSize + v_i)];
            v_i += 1;
          }
          v_prepareToSuspend(v_ec, v_stack, v_valueStackSize, v_pc);
          v_value = ((Value) TranslationHelper.invokeFunctionPointer(v_nativeFp, new Object[] {v_vm, v_valueArray1}));
          if (v_ec.executionStateChange) {
            v_ec.executionStateChange = false;
            if ((v_ec.executionStateChangeCommand == 1)) {
              return v_suspendInterpreter();
            }
          }
          if ((v_row[2] == 1)) {
            if ((v_valueStackSize == v_valueStackCapacity)) {
              v_valueStack = v_valueStackIncreaseCapacity(v_ec);
              v_valueStackCapacity = v_valueStack.length;
            }
            v_valueStack[v_valueStackSize] = v_value;
            v_valueStackSize += 1;
          }
        }
        break;
      case 16:
        // CNI_REGISTER;
        v_nativeFp = ((java.lang.reflect.Method) TranslationHelper.getFunction(v_stringArgs[v_pc]));
        v_metadata.cniFunctionsById.put(v_row[0], v_nativeFp);
        break;
      case 17:
        // COMMAND_LINE_ARGS;
        if ((v_valueStackSize == v_valueStackCapacity)) {
          v_valueStack = v_valueStackIncreaseCapacity(v_ec);
          v_valueStackCapacity = v_valueStack.length;
        }
        v_list1 = v_makeEmptyList(v_globals.stringType, 3);
        v_i = 0;
        while ((v_i < v_vm.environment.commandLineArgs.length)) {
          v_addToList(v_list1, v_buildString(v_globals, v_vm.environment.commandLineArgs[v_i]));
          v_i += 1;
        }
        v_valueStack[v_valueStackSize] = new Value(6, v_list1);
        v_valueStackSize += 1;
        break;
      case 18:
        // CONTINUE;
        if ((v_row[0] == 1)) {
          v_pc += v_row[1];
        } else {
          v_intArray1 = v_esfData[v_pc];
          v_pc = (v_intArray1[1] - 1);
          v_valueStackSize = v_stack.valueStackPopSize;
          v_stack.postFinallyBehavior = 2;
        }
        break;
      case 19:
        // CORE_FUNCTION;
        switch (v_row[0]) {
          case 1:
            // parseInt;
            v_arg1 = v_valueStack[--v_valueStackSize];
            v_output = v_VALUE_NULL;
            if ((v_arg1.type == 5)) {
              v_string1 = (((String) v_arg1.internalValue)).trim();
              if (PST_isValidInteger(v_string1)) {
                v_output = v_buildInteger(v_globals, Integer.parseInt(v_string1));
              }
            } else {
              v_hasInterrupt = v_EX_InvalidArgument(v_ec, "parseInt requires a string argument.");
            }
            break;
          case 2:
            // parseFloat;
            v_arg1 = v_valueStack[--v_valueStackSize];
            v_output = v_VALUE_NULL;
            if ((v_arg1.type == 5)) {
              v_string1 = (((String) v_arg1.internalValue)).trim();
              PST_parseFloatOrReturnNull(v_floatList1, v_string1);
              if ((v_floatList1[0] >= 0)) {
                v_output = v_buildFloat(v_globals, v_floatList1[1]);
              }
            } else {
              v_hasInterrupt = v_EX_InvalidArgument(v_ec, "parseFloat requires a string argument.");
            }
            break;
          case 3:
            // print;
            v_arg1 = v_valueStack[--v_valueStackSize];
            v_output = v_VALUE_NULL;
            v_printToStdOut(v_vm.environment.stdoutPrefix, v_valueToString(v_vm, v_arg1));
            break;
          case 4:
            // typeof;
            v_arg1 = v_valueStack[--v_valueStackSize];
            v_output = v_buildInteger(v_globals, (v_arg1.type - 1));
            break;
          case 5:
            // typeis;
            v_arg1 = v_valueStack[--v_valueStackSize];
            v_int1 = v_arg1.type;
            v_int2 = v_row[2];
            v_output = v_VALUE_FALSE;
            while ((v_int2 > 0)) {
              if ((v_row[(2 + v_int2)] == v_int1)) {
                v_output = v_VALUE_TRUE;
                v_int2 = 0;
              } else {
                v_int2 -= 1;
              }
            }
            break;
          case 6:
            // execId;
            v_output = v_buildInteger(v_globals, v_ec.id);
            break;
          case 7:
            // assert;
            v_valueStackSize -= 3;
            v_arg3 = v_valueStack[(v_valueStackSize + 2)];
            v_arg2 = v_valueStack[(v_valueStackSize + 1)];
            v_arg1 = v_valueStack[v_valueStackSize];
            if ((v_arg1.type != 2)) {
              v_hasInterrupt = v_EX_InvalidArgument(v_ec, "Assertion expression must be a boolean.");
            } else {
              if ((v_arg1.intValue == 1)) {
                v_output = v_VALUE_NULL;
              } else {
                v_string1 = v_valueToString(v_vm, v_arg2);
                if ((v_arg3.intValue == 1)) {
                  v_string1 = "Assertion failed: " + v_string1;
                }
                v_hasInterrupt = v_EX_AssertionFailed(v_ec, v_string1);
              }
            }
            break;
          case 8:
            // chr;
            v_arg1 = v_valueStack[--v_valueStackSize];
            v_output = null;
            if ((v_arg1.type == 3)) {
              v_int1 = v_arg1.intValue;
              if (((v_int1 >= 0) && (v_int1 < 256))) {
                v_output = v_buildCommonString(v_globals, ("" + Character.toString((char) v_int1)));
              }
            }
            if ((v_output == null)) {
              v_hasInterrupt = v_EX_InvalidArgument(v_ec, "chr requires an integer between 0 and 255.");
            }
            break;
          case 9:
            // ord;
            v_arg1 = v_valueStack[--v_valueStackSize];
            v_output = null;
            if ((v_arg1.type == 5)) {
              v_string1 = ((String) v_arg1.internalValue);
              if ((v_string1.length() == 1)) {
                v_output = v_buildInteger(v_globals, ((int) v_string1.charAt(0)));
              }
            }
            if ((v_output == null)) {
              v_hasInterrupt = v_EX_InvalidArgument(v_ec, "ord requires a 1 character string.");
            }
            break;
          case 10:
            // currentTime;
            v_output = v_buildFloat(v_globals, System.currentTimeMillis() / 1000.0);
            break;
          case 11:
            // sortList;
            v_valueStackSize -= 2;
            v_arg2 = v_valueStack[(v_valueStackSize + 1)];
            v_arg1 = v_valueStack[v_valueStackSize];
            v_output = v_VALUE_NULL;
            v_list1 = ((ListImpl) v_arg1.internalValue);
            v_list2 = ((ListImpl) v_arg2.internalValue);
            v_sortLists(v_list2, v_list1, PST_intBuffer16);
            if ((PST_intBuffer16[0] > 0)) {
              v_hasInterrupt = v_EX_InvalidArgument(v_ec, "Invalid sort keys. Keys must be all numbers or all strings, but not mixed.");
            }
            break;
          case 12:
            // abs;
            v_arg1 = v_valueStack[--v_valueStackSize];
            v_output = v_arg1;
            if ((v_arg1.type == 3)) {
              if ((v_arg1.intValue < 0)) {
                v_output = v_buildInteger(v_globals, -v_arg1.intValue);
              }
            } else {
              if ((v_arg1.type == 4)) {
                if ((((double) v_arg1.internalValue) < 0)) {
                  v_output = v_buildFloat(v_globals, -((double) v_arg1.internalValue));
                }
              } else {
                v_hasInterrupt = v_EX_InvalidArgument(v_ec, "abs requires a number as input.");
              }
            }
            break;
          case 13:
            // arcCos;
            v_arg1 = v_valueStack[--v_valueStackSize];
            if ((v_arg1.type == 4)) {
              v_float1 = ((double) v_arg1.internalValue);
            } else {
              if ((v_arg1.type == 3)) {
                v_float1 = (0.0 + v_arg1.intValue);
              } else {
                v_hasInterrupt = v_EX_InvalidArgument(v_ec, "arccos requires a number as input.");
              }
            }
            if (!v_hasInterrupt) {
              if (((v_float1 < -1) || (v_float1 > 1))) {
                v_hasInterrupt = v_EX_InvalidArgument(v_ec, "arccos requires a number in the range of -1 to 1.");
              } else {
                v_output = v_buildFloat(v_globals, Math.acos(v_float1));
              }
            }
            break;
          case 14:
            // arcSin;
            v_arg1 = v_valueStack[--v_valueStackSize];
            if ((v_arg1.type == 4)) {
              v_float1 = ((double) v_arg1.internalValue);
            } else {
              if ((v_arg1.type == 3)) {
                v_float1 = (0.0 + v_arg1.intValue);
              } else {
                v_hasInterrupt = v_EX_InvalidArgument(v_ec, "arcsin requires a number as input.");
              }
            }
            if (!v_hasInterrupt) {
              if (((v_float1 < -1) || (v_float1 > 1))) {
                v_hasInterrupt = v_EX_InvalidArgument(v_ec, "arcsin requires a number in the range of -1 to 1.");
              } else {
                v_output = v_buildFloat(v_globals, Math.asin(v_float1));
              }
            }
            break;
          case 15:
            // arcTan;
            v_valueStackSize -= 2;
            v_arg2 = v_valueStack[(v_valueStackSize + 1)];
            v_arg1 = v_valueStack[v_valueStackSize];
            v_bool1 = false;
            if ((v_arg1.type == 4)) {
              v_float1 = ((double) v_arg1.internalValue);
            } else {
              if ((v_arg1.type == 3)) {
                v_float1 = (0.0 + v_arg1.intValue);
              } else {
                v_bool1 = true;
              }
            }
            if ((v_arg2.type == 4)) {
              v_float2 = ((double) v_arg2.internalValue);
            } else {
              if ((v_arg2.type == 3)) {
                v_float2 = (0.0 + v_arg2.intValue);
              } else {
                v_bool1 = true;
              }
            }
            if (v_bool1) {
              v_hasInterrupt = v_EX_InvalidArgument(v_ec, "arctan requires numeric arguments.");
            } else {
              v_output = v_buildFloat(v_globals, Math.atan2(v_float1, v_float2));
            }
            break;
          case 16:
            // cos;
            v_arg1 = v_valueStack[--v_valueStackSize];
            if ((v_arg1.type == 4)) {
              v_float1 = ((double) v_arg1.internalValue);
              v_output = v_buildFloat(v_globals, Math.cos(v_float1));
            } else {
              if ((v_arg1.type == 3)) {
                v_int1 = v_arg1.intValue;
                v_output = v_buildFloat(v_globals, Math.cos(v_int1));
              } else {
                v_hasInterrupt = v_EX_InvalidArgument(v_ec, "cos requires a number argument.");
              }
            }
            break;
          case 17:
            // ensureRange;
            v_valueStackSize -= 3;
            v_arg3 = v_valueStack[(v_valueStackSize + 2)];
            v_arg2 = v_valueStack[(v_valueStackSize + 1)];
            v_arg1 = v_valueStack[v_valueStackSize];
            v_bool1 = false;
            if ((v_arg2.type == 4)) {
              v_float2 = ((double) v_arg2.internalValue);
            } else {
              if ((v_arg2.type == 3)) {
                v_float2 = (0.0 + v_arg2.intValue);
              } else {
                v_bool1 = true;
              }
            }
            if ((v_arg3.type == 4)) {
              v_float3 = ((double) v_arg3.internalValue);
            } else {
              if ((v_arg3.type == 3)) {
                v_float3 = (0.0 + v_arg3.intValue);
              } else {
                v_bool1 = true;
              }
            }
            if ((!v_bool1 && (v_float3 < v_float2))) {
              v_float1 = v_float3;
              v_float3 = v_float2;
              v_float2 = v_float1;
              v_value = v_arg2;
              v_arg2 = v_arg3;
              v_arg3 = v_value;
            }
            if ((v_arg1.type == 4)) {
              v_float1 = ((double) v_arg1.internalValue);
            } else {
              if ((v_arg1.type == 3)) {
                v_float1 = (0.0 + v_arg1.intValue);
              } else {
                v_bool1 = true;
              }
            }
            if (v_bool1) {
              v_hasInterrupt = v_EX_InvalidArgument(v_ec, "ensureRange requires numeric arguments.");
            } else {
              if ((v_float1 < v_float2)) {
                v_output = v_arg2;
              } else {
                if ((v_float1 > v_float3)) {
                  v_output = v_arg3;
                } else {
                  v_output = v_arg1;
                }
              }
            }
            break;
          case 18:
            // floor;
            v_arg1 = v_valueStack[--v_valueStackSize];
            if ((v_arg1.type == 4)) {
              v_float1 = ((double) v_arg1.internalValue);
              if (((v_float1 < 0) && ((v_float1 % 1) != 0))) {
                v_int1 = (((int) v_float1) - 1);
              } else {
                v_int1 = ((int) v_float1);
              }
              if ((v_int1 < 2049)) {
                if ((v_int1 >= 0)) {
                  v_output = v_INTEGER_POSITIVE_CACHE[v_int1];
                } else {
                  if ((v_int1 > -257)) {
                    v_output = v_INTEGER_NEGATIVE_CACHE[-v_int1];
                  } else {
                    v_output = new Value(v_int1);
                  }
                }
              } else {
                v_output = new Value(v_int1);
              }
            } else {
              if ((v_arg1.type == 3)) {
                v_output = v_arg1;
              } else {
                v_hasInterrupt = v_EX_InvalidArgument(v_ec, "floor expects a numeric argument.");
              }
            }
            break;
          case 19:
            // max;
            v_valueStackSize -= 2;
            v_arg2 = v_valueStack[(v_valueStackSize + 1)];
            v_arg1 = v_valueStack[v_valueStackSize];
            v_bool1 = false;
            if ((v_arg1.type == 4)) {
              v_float1 = ((double) v_arg1.internalValue);
            } else {
              if ((v_arg1.type == 3)) {
                v_float1 = (0.0 + v_arg1.intValue);
              } else {
                v_bool1 = true;
              }
            }
            if ((v_arg2.type == 4)) {
              v_float2 = ((double) v_arg2.internalValue);
            } else {
              if ((v_arg2.type == 3)) {
                v_float2 = (0.0 + v_arg2.intValue);
              } else {
                v_bool1 = true;
              }
            }
            if (v_bool1) {
              v_hasInterrupt = v_EX_InvalidArgument(v_ec, "max requires numeric arguments.");
            } else {
              if ((v_float1 >= v_float2)) {
                v_output = v_arg1;
              } else {
                v_output = v_arg2;
              }
            }
            break;
          case 20:
            // min;
            v_valueStackSize -= 2;
            v_arg2 = v_valueStack[(v_valueStackSize + 1)];
            v_arg1 = v_valueStack[v_valueStackSize];
            v_bool1 = false;
            if ((v_arg1.type == 4)) {
              v_float1 = ((double) v_arg1.internalValue);
            } else {
              if ((v_arg1.type == 3)) {
                v_float1 = (0.0 + v_arg1.intValue);
              } else {
                v_bool1 = true;
              }
            }
            if ((v_arg2.type == 4)) {
              v_float2 = ((double) v_arg2.internalValue);
            } else {
              if ((v_arg2.type == 3)) {
                v_float2 = (0.0 + v_arg2.intValue);
              } else {
                v_bool1 = true;
              }
            }
            if (v_bool1) {
              v_hasInterrupt = v_EX_InvalidArgument(v_ec, "min requires numeric arguments.");
            } else {
              if ((v_float1 <= v_float2)) {
                v_output = v_arg1;
              } else {
                v_output = v_arg2;
              }
            }
            break;
          case 21:
            // nativeInt;
            v_valueStackSize -= 2;
            v_arg2 = v_valueStack[(v_valueStackSize + 1)];
            v_arg1 = v_valueStack[v_valueStackSize];
            v_output = v_buildInteger(v_globals, ((int) (((ObjectInstance) v_arg1.internalValue)).nativeData[v_arg2.intValue]));
            break;
          case 22:
            // nativeString;
            v_valueStackSize -= 3;
            v_arg3 = v_valueStack[(v_valueStackSize + 2)];
            v_arg2 = v_valueStack[(v_valueStackSize + 1)];
            v_arg1 = v_valueStack[v_valueStackSize];
            v_string1 = ((String) (((ObjectInstance) v_arg1.internalValue)).nativeData[v_arg2.intValue]);
            if ((v_arg3.intValue == 1)) {
              v_output = v_buildCommonString(v_globals, v_string1);
            } else {
              v_output = v_buildString(v_globals, v_string1);
            }
            break;
          case 23:
            // sign;
            v_arg1 = v_valueStack[--v_valueStackSize];
            if ((v_arg1.type == 3)) {
              v_float1 = (0.0 + (v_arg1.intValue));
            } else {
              if ((v_arg1.type == 4)) {
                v_float1 = ((double) v_arg1.internalValue);
              } else {
                v_hasInterrupt = v_EX_InvalidArgument(v_ec, "sign requires a number as input.");
              }
            }
            if ((v_float1 == 0)) {
              v_output = v_VALUE_INT_ZERO;
            } else {
              if ((v_float1 > 0)) {
                v_output = v_VALUE_INT_ONE;
              } else {
                v_output = v_INTEGER_NEGATIVE_CACHE[1];
              }
            }
            break;
          case 24:
            // sin;
            v_arg1 = v_valueStack[--v_valueStackSize];
            if ((v_arg1.type == 4)) {
              v_float1 = ((double) v_arg1.internalValue);
            } else {
              if ((v_arg1.type == 3)) {
                v_float1 = (0.0 + v_arg1.intValue);
              } else {
                v_hasInterrupt = v_EX_InvalidArgument(v_ec, "sin requires a number argument.");
              }
            }
            v_output = v_buildFloat(v_globals, Math.sin(v_float1));
            break;
          case 25:
            // tan;
            v_arg1 = v_valueStack[--v_valueStackSize];
            if ((v_arg1.type == 4)) {
              v_float1 = ((double) v_arg1.internalValue);
            } else {
              if ((v_arg1.type == 3)) {
                v_float1 = (0.0 + v_arg1.intValue);
              } else {
                v_hasInterrupt = v_EX_InvalidArgument(v_ec, "tan requires a number argument.");
              }
            }
            if (!v_hasInterrupt) {
              v_float2 = Math.cos(v_float1);
              if ((v_float2 < 0)) {
                v_float2 = -v_float2;
              }
              if ((v_float2 < 0.00000000015)) {
                v_hasInterrupt = v_EX_DivisionByZero(v_ec, "Tangent is undefined.");
              } else {
                v_output = v_buildFloat(v_globals, Math.tan(v_float1));
              }
            }
            break;
          case 26:
            // log;
            v_valueStackSize -= 2;
            v_arg2 = v_valueStack[(v_valueStackSize + 1)];
            v_arg1 = v_valueStack[v_valueStackSize];
            if ((v_arg1.type == 4)) {
              v_float1 = ((double) v_arg1.internalValue);
            } else {
              if ((v_arg1.type == 3)) {
                v_float1 = (0.0 + v_arg1.intValue);
              } else {
                v_hasInterrupt = v_EX_InvalidArgument(v_ec, "logarithms require a number argument.");
              }
            }
            if (!v_hasInterrupt) {
              if ((v_float1 <= 0)) {
                v_hasInterrupt = v_EX_InvalidArgument(v_ec, "logarithms require positive inputs.");
              } else {
                v_output = v_buildFloat(v_globals, v_fixFuzzyFloatPrecision((Math.log(v_float1) * ((double) v_arg2.internalValue))));
              }
            }
            break;
          case 27:
            // intQueueClear;
            v_arg1 = v_valueStack[--v_valueStackSize];
            v_output = v_VALUE_NULL;
            v_objInstance1 = ((ObjectInstance) v_arg1.internalValue);
            if ((v_objInstance1.nativeData != null)) {
              v_objInstance1.nativeData[1] = 0;
            }
            break;
          case 28:
            // intQueueWrite16;
            v_output = v_VALUE_NULL;
            v_int1 = v_row[2];
            v_valueStackSize -= (v_int1 + 1);
            v_value = v_valueStack[v_valueStackSize];
            v_objArray1 = (((ObjectInstance) v_value.internalValue)).nativeData;
            v_intArray1 = ((int[]) v_objArray1[0]);
            v_len = ((int) v_objArray1[1]);
            if ((v_len >= v_intArray1.length)) {
              v_intArray2 = new int[((v_len * 2) + 16)];
              v_j = 0;
              while ((v_j < v_len)) {
                v_intArray2[v_j] = v_intArray1[v_j];
                v_j += 1;
              }
              v_intArray1 = v_intArray2;
              v_objArray1[0] = v_intArray1;
            }
            v_objArray1[1] = (v_len + 16);
            v_i = (v_int1 - 1);
            while ((v_i >= 0)) {
              v_value = v_valueStack[((v_valueStackSize + 1) + v_i)];
              if ((v_value.type == 3)) {
                v_intArray1[(v_len + v_i)] = v_value.intValue;
              } else {
                if ((v_value.type == 4)) {
                  v_float1 = (0.5 + ((double) v_value.internalValue));
                  v_intArray1[(v_len + v_i)] = ((int) v_float1);
                } else {
                  v_hasInterrupt = v_EX_InvalidArgument(v_ec, "Input must be integers.");
                  v_i = -1;
                }
              }
              v_i -= 1;
            }
            break;
          case 29:
            // execCounter;
            v_output = v_buildInteger(v_globals, v_ec.executionCounter);
            break;
          case 30:
            // sleep;
            v_arg1 = v_valueStack[--v_valueStackSize];
            v_float1 = v_getFloat(v_arg1);
            if ((v_row[1] == 1)) {
              if ((v_valueStackSize == v_valueStackCapacity)) {
                v_valueStack = v_valueStackIncreaseCapacity(v_ec);
                v_valueStackCapacity = v_valueStack.length;
              }
              v_valueStack[v_valueStackSize] = v_VALUE_NULL;
              v_valueStackSize += 1;
            }
            v_prepareToSuspend(v_ec, v_stack, v_valueStackSize, v_pc);
            v_ec.activeInterrupt = new Interrupt(3, 0, "", v_float1, null);
            v_hasInterrupt = true;
            break;
          case 31:
            // projectId;
            v_output = v_buildCommonString(v_globals, v_metadata.projectId);
            break;
          case 32:
            // isJavaScript;
            v_output = v_VALUE_FALSE;
            break;
          case 33:
            // isAndroid;
            v_output = v_VALUE_FALSE;
            break;
          case 34:
            // allocNativeData;
            v_valueStackSize -= 2;
            v_arg2 = v_valueStack[(v_valueStackSize + 1)];
            v_arg1 = v_valueStack[v_valueStackSize];
            v_objInstance1 = ((ObjectInstance) v_arg1.internalValue);
            v_int1 = v_arg2.intValue;
            v_objArray1 = new Object[v_int1];
            v_objInstance1.nativeData = v_objArray1;
            break;
          case 35:
            // setNativeData;
            v_valueStackSize -= 3;
            v_arg3 = v_valueStack[(v_valueStackSize + 2)];
            v_arg2 = v_valueStack[(v_valueStackSize + 1)];
            v_arg1 = v_valueStack[v_valueStackSize];
            (((ObjectInstance) v_arg1.internalValue)).nativeData[v_arg2.intValue] = v_arg3.internalValue;
            break;
          case 36:
            // getExceptionTrace;
            v_arg1 = v_valueStack[--v_valueStackSize];
            v_intList1 = ((ArrayList<Integer>) v_getNativeDataItem(v_arg1, 1));
            v_list1 = v_makeEmptyList(v_globals.stringType, 20);
            v_output = new Value(6, v_list1);
            if ((v_intList1 != null)) {
              v_stringList1 = v_tokenHelperConvertPcsToStackTraceStrings(v_vm, v_intList1);
              v_i = 0;
              while ((v_i < v_stringList1.size())) {
                v_addToList(v_list1, v_buildString(v_globals, v_stringList1.get(v_i)));
                v_i += 1;
              }
              v_reverseList(v_list1);
            }
            break;
          case 37:
            // reflectAllClasses;
            v_output = v_Reflect_allClasses(v_vm);
            break;
          case 38:
            // reflectGetMethods;
            v_arg1 = v_valueStack[--v_valueStackSize];
            v_output = v_Reflect_getMethods(v_vm, v_ec, v_arg1);
            v_hasInterrupt = (v_ec.activeInterrupt != null);
            break;
          case 39:
            // reflectGetClass;
            v_arg1 = v_valueStack[--v_valueStackSize];
            if ((v_arg1.type != 8)) {
              v_hasInterrupt = v_EX_InvalidArgument(v_ec, "Cannot get class from non-instance types.");
            } else {
              v_objInstance1 = ((ObjectInstance) v_arg1.internalValue);
              v_output = new Value(10, new org.crayonlang.interpreter.structs.ClassValue(false, v_objInstance1.classId));
            }
            break;
          case 40:
            // convertFloatArgsToInts;
            v_int1 = v_stack.localsStackOffsetEnd;
            v_i = v_localsStackOffset;
            while ((v_i < v_int1)) {
              v_value = v_localsStack[v_i];
              if ((v_localsStackSet[v_i] != v_localsStackSetToken)) {
                v_i += v_int1;
              } else {
                if ((v_value.type == 4)) {
                  v_float1 = ((double) v_value.internalValue);
                  if (((v_float1 < 0) && ((v_float1 % 1) != 0))) {
                    v_int2 = (((int) v_float1) - 1);
                  } else {
                    v_int2 = ((int) v_float1);
                  }
                  if (((v_int2 >= 0) && (v_int2 < 2049))) {
                    v_localsStack[v_i] = v_INTEGER_POSITIVE_CACHE[v_int2];
                  } else {
                    v_localsStack[v_i] = v_buildInteger(v_globals, v_int2);
                  }
                }
              }
              v_i += 1;
            }
            break;
          case 41:
            // addShutdownHandler;
            v_arg1 = v_valueStack[--v_valueStackSize];
            v_vm.shutdownHandlers.add(v_arg1);
            break;
        }
        if ((v_row[1] == 1)) {
          if ((v_valueStackSize == v_valueStackCapacity)) {
            v_valueStack = v_valueStackIncreaseCapacity(v_ec);
            v_valueStackCapacity = v_valueStack.length;
          }
          v_valueStack[v_valueStackSize] = v_output;
          v_valueStackSize += 1;
        }
        break;
      case 20:
        // DEBUG_SYMBOLS;
        v_applyDebugSymbolData(v_vm, v_row, v_stringArgs[v_pc], v_metadata.mostRecentFunctionDef);
        break;
      case 21:
        // DEF_DICT;
        v_intIntDict1 = new HashMap<Integer, Integer>();
        v_stringIntDict1 = new HashMap<String, Integer>();
        v_valueList2 = new ArrayList<Value>();
        v_valueList1 = new ArrayList<Value>();
        v_len = v_row[0];
        v_type = 3;
        v_first = true;
        v_i = v_len;
        while ((v_i > 0)) {
          v_valueStackSize -= 2;
          v_value = v_valueStack[(v_valueStackSize + 1)];
          v_value2 = v_valueStack[v_valueStackSize];
          if (v_first) {
            v_type = v_value2.type;
            v_first = false;
          } else {
            if ((v_type != v_value2.type)) {
              v_hasInterrupt = v_EX_InvalidKey(v_ec, "Dictionary keys must be of the same type.");
            }
          }
          if (!v_hasInterrupt) {
            if ((v_type == 3)) {
              v_intKey = v_value2.intValue;
            } else {
              if ((v_type == 5)) {
                v_stringKey = ((String) v_value2.internalValue);
              } else {
                if ((v_type == 8)) {
                  v_objInstance1 = ((ObjectInstance) v_value2.internalValue);
                  v_intKey = v_objInstance1.objectId;
                } else {
                  v_hasInterrupt = v_EX_InvalidKey(v_ec, "Only integers, strings, and objects can be used as dictionary keys.");
                }
              }
            }
          }
          if (!v_hasInterrupt) {
            if ((v_type == 5)) {
              v_stringIntDict1.put(v_stringKey, v_valueList1.size());
            } else {
              v_intIntDict1.put(v_intKey, v_valueList1.size());
            }
            v_valueList2.add(v_value2);
            v_valueList1.add(v_value);
            v_i -= 1;
          }
        }
        if (!v_hasInterrupt) {
          if ((v_type == 5)) {
            v_i = v_stringIntDict1.size();
          } else {
            v_i = v_intIntDict1.size();
          }
          if ((v_i != v_len)) {
            v_hasInterrupt = v_EX_InvalidKey(v_ec, "Key collision");
          }
        }
        if (!v_hasInterrupt) {
          v_i = v_row[1];
          v_classId = 0;
          if ((v_i > 0)) {
            v_type = v_row[2];
            if ((v_type == 8)) {
              v_classId = v_row[3];
            }
            v_int1 = v_row.length;
            v_intArray1 = new int[(v_int1 - v_i)];
            while ((v_i < v_int1)) {
              v_intArray1[(v_i - v_row[1])] = v_row[v_i];
              v_i += 1;
            }
          } else {
            v_intArray1 = null;
          }
          if ((v_valueStackSize == v_valueStackCapacity)) {
            v_valueStack = v_valueStackIncreaseCapacity(v_ec);
            v_valueStackCapacity = v_valueStack.length;
          }
          v_valueStack[v_valueStackSize] = new Value(7, new DictImpl(v_len, v_type, v_classId, v_intArray1, v_intIntDict1, v_stringIntDict1, v_valueList2, v_valueList1));
          v_valueStackSize += 1;
        }
        break;
      case 22:
        // DEF_LIST;
        v_int1 = v_row[0];
        v_list1 = v_makeEmptyList(null, v_int1);
        if ((v_row[1] != 0)) {
          v_list1.type = new int[(v_row.length - 1)];
          v_i = 1;
          while ((v_i < v_row.length)) {
            v_list1.type[(v_i - 1)] = v_row[v_i];
            v_i += 1;
          }
        }
        v_list1.size = v_int1;
        v_int2 = (v_valueStackSize - v_int1);
        v_i = 0;
        while ((v_i < v_int1)) {
          v_list1.array[v_i] = v_valueStack[(v_int2 + v_i)];
          v_i += 1;
        }
        v_valueStackSize -= v_int1;
        v_value = new Value(6, v_list1);
        if ((v_valueStackSize == v_valueStackCapacity)) {
          v_valueStack = v_valueStackIncreaseCapacity(v_ec);
          v_valueStackCapacity = v_valueStack.length;
        }
        v_valueStack[v_valueStackSize] = v_value;
        v_valueStackSize += 1;
        break;
      case 23:
        // DEF_ORIGINAL_CODE;
        v_defOriginalCodeImpl(v_vm, v_row, v_stringArgs[v_pc]);
        break;
      case 24:
        // DEREF_CLOSURE;
        v_bool1 = true;
        v_closure = v_stack.closureVariables;
        v_i = v_row[0];
        if (((v_closure != null) && v_closure.containsKey(v_i))) {
          v_value = v_closure.get(v_i).value;
          if ((v_value != null)) {
            v_bool1 = false;
            if ((v_valueStackSize == v_valueStackCapacity)) {
              v_valueStack = v_valueStackIncreaseCapacity(v_ec);
              v_valueStackCapacity = v_valueStack.length;
            }
            v_valueStack[v_valueStackSize++] = v_value;
          }
        }
        if (v_bool1) {
          v_hasInterrupt = v_EX_UnassignedVariable(v_ec, "Variable used before it was set.");
        }
        break;
      case 25:
        // DEREF_DOT;
        v_value = v_valueStack[(v_valueStackSize - 1)];
        v_nameId = v_row[0];
        v_int2 = v_row[1];
        switch (v_value.type) {
          case 8:
            v_objInstance1 = ((ObjectInstance) v_value.internalValue);
            v_classId = v_objInstance1.classId;
            v_classInfo = v_classTable[v_classId];
            if ((v_classId == v_row[4])) {
              v_int1 = v_row[5];
            } else {
              v_intIntDict1 = v_classInfo.localeScopedNameIdToMemberId;
              Integer dictLookup8 = v_intIntDict1.get(v_int2);
              v_int1 = dictLookup8 == null ? (-1) : dictLookup8;
              v_int3 = v_classInfo.fieldAccessModifiers[v_int1];
              if ((v_int3 > 1)) {
                if ((v_int3 == 2)) {
                  if ((v_classId != v_row[2])) {
                    v_int1 = -2;
                  }
                } else {
                  if (((v_int3 == 3) || (v_int3 == 5))) {
                    if ((v_classInfo.assemblyId != v_row[3])) {
                      v_int1 = -3;
                    }
                  }
                  if (((v_int3 == 4) || (v_int3 == 5))) {
                    v_i = v_row[2];
                    if ((v_classId == v_i)) {
                    } else {
                      v_classInfo = v_classTable[v_classInfo.id];
                      while (((v_classInfo.baseClassId != -1) && (v_int1 < v_classTable[v_classInfo.baseClassId].fieldAccessModifiers.length))) {
                        v_classInfo = v_classTable[v_classInfo.baseClassId];
                      }
                      v_j = v_classInfo.id;
                      if ((v_j != v_i)) {
                        v_bool1 = false;
                        while (((v_i != -1) && (v_classTable[v_i].baseClassId != -1))) {
                          v_i = v_classTable[v_i].baseClassId;
                          if ((v_i == v_j)) {
                            v_bool1 = true;
                            v_i = -1;
                          }
                        }
                        if (!v_bool1) {
                          v_int1 = -4;
                        }
                      }
                    }
                    v_classInfo = v_classTable[v_classId];
                  }
                }
              }
              v_row[4] = v_objInstance1.classId;
              v_row[5] = v_int1;
            }
            if ((v_int1 > -1)) {
              v_functionId = v_classInfo.functionIds[v_int1];
              if ((v_functionId == -1)) {
                v_output = v_objInstance1.members[v_int1];
              } else {
                v_output = new Value(9, new FunctionPointer(2, v_value, v_objInstance1.classId, v_functionId, null));
              }
            } else {
              v_output = null;
            }
            break;
          case 5:
            if ((v_metadata.lengthId == v_nameId)) {
              v_output = v_buildInteger(v_globals, (((String) v_value.internalValue)).length());
            } else {
              v_output = null;
            }
            break;
          case 6:
            if ((v_metadata.lengthId == v_nameId)) {
              v_output = v_buildInteger(v_globals, (((ListImpl) v_value.internalValue)).size);
            } else {
              v_output = null;
            }
            break;
          case 7:
            if ((v_metadata.lengthId == v_nameId)) {
              v_output = v_buildInteger(v_globals, (((DictImpl) v_value.internalValue)).size);
            } else {
              v_output = null;
            }
            break;
          default:
            if ((v_value.type == 1)) {
              v_hasInterrupt = v_EX_NullReference(v_ec, "Derferenced a field from null.");
              v_output = v_VALUE_NULL;
            } else {
              v_output = null;
            }
            break;
        }
        if ((v_output == null)) {
          v_output = v_generatePrimitiveMethodReference(v_globalNameIdToPrimitiveMethodName, v_nameId, v_value);
          if ((v_output == null)) {
            if ((v_value.type == 1)) {
              v_hasInterrupt = v_EX_NullReference(v_ec, "Tried to dereference a field on null.");
            } else {
              if (((v_value.type == 8) && (v_int1 < -1))) {
                v_string1 = v_identifiers[v_row[0]];
                if ((v_int1 == -2)) {
                  v_string2 = "private";
                } else {
                  if ((v_int1 == -3)) {
                    v_string2 = "internal";
                  } else {
                    v_string2 = "protected";
                  }
                }
                v_hasInterrupt = v_EX_UnknownField(v_ec, "The field '" + v_string1 + "' is marked as " + v_string2 + " and cannot be accessed from here.");
              } else {
                if ((v_value.type == 8)) {
                  v_classId = (((ObjectInstance) v_value.internalValue)).classId;
                  v_classInfo = v_classTable[v_classId];
                  v_string1 = v_classInfo.fullyQualifiedName + " instance";
                } else {
                  v_string1 = v_getTypeFromId(v_value.type);
                }
                v_hasInterrupt = v_EX_UnknownField(v_ec, v_string1 + " does not have that field.");
              }
            }
          }
        }
        v_valueStack[(v_valueStackSize - 1)] = v_output;
        break;
      case 26:
        // DEREF_INSTANCE_FIELD;
        v_value = v_stack.objectContext;
        v_objInstance1 = ((ObjectInstance) v_value.internalValue);
        v_value = v_objInstance1.members[v_row[0]];
        if ((v_valueStackSize == v_valueStackCapacity)) {
          v_valueStack = v_valueStackIncreaseCapacity(v_ec);
          v_valueStackCapacity = v_valueStack.length;
        }
        v_valueStack[v_valueStackSize++] = v_value;
        break;
      case 27:
        // DEREF_STATIC_FIELD;
        v_classInfo = v_classTable[v_row[0]];
        v_staticConstructorNotInvoked = true;
        if ((v_classInfo.staticInitializationState < 2)) {
          v_stack.pc = v_pc;
          v_stackFrame2 = v_maybeInvokeStaticConstructor(v_vm, v_ec, v_stack, v_classInfo, v_valueStackSize, PST_intBuffer16);
          if ((PST_intBuffer16[0] == 1)) {
            return v_generateException(v_vm, v_stack, v_pc, v_valueStackSize, v_ec, 0, "Static initialization loop detected. The class this field is a member of is not done being initialized.");
          }
          if ((v_stackFrame2 != null)) {
            v_staticConstructorNotInvoked = false;
            v_stack = v_stackFrame2;
            v_pc = v_stack.pc;
            v_localsStackSetToken = v_stack.localsStackSetToken;
            v_localsStackOffset = v_stack.localsStackOffset;
          }
        }
        if (v_staticConstructorNotInvoked) {
          if ((v_valueStackSize == v_valueStackCapacity)) {
            v_valueStack = v_valueStackIncreaseCapacity(v_ec);
            v_valueStackCapacity = v_valueStack.length;
          }
          v_valueStack[v_valueStackSize++] = v_classInfo.staticFields[v_row[1]];
        }
        break;
      case 28:
        // DUPLICATE_STACK_TOP;
        if ((v_row[0] == 1)) {
          v_value = v_valueStack[(v_valueStackSize - 1)];
          if ((v_valueStackSize == v_valueStackCapacity)) {
            v_valueStack = v_valueStackIncreaseCapacity(v_ec);
            v_valueStackCapacity = v_valueStack.length;
          }
          v_valueStack[v_valueStackSize++] = v_value;
        } else {
          if ((v_row[0] == 2)) {
            if (((v_valueStackSize + 1) > v_valueStackCapacity)) {
              v_valueStackIncreaseCapacity(v_ec);
              v_valueStack = v_ec.valueStack;
              v_valueStackCapacity = v_valueStack.length;
            }
            v_valueStack[v_valueStackSize] = v_valueStack[(v_valueStackSize - 2)];
            v_valueStack[(v_valueStackSize + 1)] = v_valueStack[(v_valueStackSize - 1)];
            v_valueStackSize += 2;
          } else {
            v_hasInterrupt = v_EX_Fatal(v_ec, "?");
          }
        }
        break;
      case 29:
        // EQUALS;
        v_valueStackSize -= 2;
        v_rightValue = v_valueStack[(v_valueStackSize + 1)];
        v_leftValue = v_valueStack[v_valueStackSize];
        if ((v_leftValue.type == v_rightValue.type)) {
          switch (v_leftValue.type) {
            case 1:
              v_bool1 = true;
              break;
            case 2:
              v_bool1 = ((v_leftValue.intValue == 1) == (v_rightValue.intValue == 1));
              break;
            case 3:
              v_bool1 = (v_leftValue.intValue == v_rightValue.intValue);
              break;
            case 5:
              v_bool1 = v_leftValue.internalValue.equals(v_rightValue.internalValue);
              break;
            default:
              v_bool1 = (v_doEqualityComparisonAndReturnCode(v_leftValue, v_rightValue) == 1);
              break;
          }
        } else {
          v_int1 = v_doEqualityComparisonAndReturnCode(v_leftValue, v_rightValue);
          if ((v_int1 == 0)) {
            v_bool1 = false;
          } else {
            if ((v_int1 == 1)) {
              v_bool1 = true;
            } else {
              v_hasInterrupt = v_EX_UnsupportedOperation(v_ec, "== and != not defined here.");
            }
          }
        }
        if ((v_valueStackSize == v_valueStackCapacity)) {
          v_valueStack = v_valueStackIncreaseCapacity(v_ec);
          v_valueStackCapacity = v_valueStack.length;
        }
        if ((v_bool1 != ((v_row[0] == 1)))) {
          v_valueStack[v_valueStackSize] = v_VALUE_TRUE;
        } else {
          v_valueStack[v_valueStackSize] = v_VALUE_FALSE;
        }
        v_valueStackSize += 1;
        break;
      case 30:
        // ESF_LOOKUP;
        v_esfData = v_generateEsfData(v_args.length, v_row);
        v_metadata.esfData = v_esfData;
        break;
      case 31:
        // EXCEPTION_HANDLED_TOGGLE;
        v_ec.activeExceptionHandled = (v_row[0] == 1);
        break;
      case 32:
        // FIELD_TYPE_INFO;
        v_initializeClassFieldTypeInfo(v_vm, v_row);
        break;
      case 33:
        // FINALIZE_INITIALIZATION;
        v_finalizeInitializationImpl(v_vm, v_stringArgs[v_pc], v_row[0]);
        v_identifiers = v_vm.metadata.identifiers;
        v_literalTable = v_vm.metadata.literalTable;
        v_globalNameIdToPrimitiveMethodName = v_vm.metadata.globalNameIdToPrimitiveMethodName;
        v_funcArgs = v_vm.funcArgs;
        break;
      case 34:
        // FINALLY_END;
        v_value = v_ec.activeException;
        if (((v_value == null) || v_ec.activeExceptionHandled)) {
          switch (v_stack.postFinallyBehavior) {
            case 0:
              v_ec.activeException = null;
              break;
            case 1:
              v_ec.activeException = null;
              v_int1 = v_row[0];
              if ((v_int1 == 1)) {
                v_pc += v_row[1];
              } else {
                if ((v_int1 == 2)) {
                  v_intArray1 = v_esfData[v_pc];
                  v_pc = v_intArray1[1];
                } else {
                  v_hasInterrupt = v_EX_Fatal(v_ec, "break exists without a loop");
                }
              }
              break;
            case 2:
              v_ec.activeException = null;
              v_int1 = v_row[2];
              if ((v_int1 == 1)) {
                v_pc += v_row[3];
              } else {
                if ((v_int1 == 2)) {
                  v_intArray1 = v_esfData[v_pc];
                  v_pc = v_intArray1[1];
                } else {
                  v_hasInterrupt = v_EX_Fatal(v_ec, "continue exists without a loop");
                }
              }
              break;
            case 3:
              if ((v_stack.markClassAsInitialized != 0)) {
                v_markClassAsInitialized(v_vm, v_stack, v_stack.markClassAsInitialized);
              }
              if (v_stack.returnValueUsed) {
                v_valueStackSize = v_stack.valueStackPopSize;
                v_value = v_stack.returnValueTempStorage;
                v_stack = v_stack.previous;
                if ((v_valueStackSize == v_valueStackCapacity)) {
                  v_valueStack = v_valueStackIncreaseCapacity(v_ec);
                  v_valueStackCapacity = v_valueStack.length;
                }
                v_valueStack[v_valueStackSize] = v_value;
                v_valueStackSize += 1;
              } else {
                v_valueStackSize = v_stack.valueStackPopSize;
                v_stack = v_stack.previous;
              }
              v_pc = v_stack.pc;
              v_localsStackOffset = v_stack.localsStackOffset;
              v_localsStackSetToken = v_stack.localsStackSetToken;
              break;
          }
        } else {
          v_ec.activeExceptionHandled = false;
          v_stack.pc = v_pc;
          v_intArray1 = v_esfData[v_pc];
          v_value = v_ec.activeException;
          v_objInstance1 = ((ObjectInstance) v_value.internalValue);
          v_objArray1 = v_objInstance1.nativeData;
          v_bool1 = true;
          if ((v_objArray1[0] != null)) {
            v_bool1 = ((boolean) v_objArray1[0]);
          }
          v_intList1 = ((ArrayList<Integer>) v_objArray1[1]);
          while (((v_stack != null) && ((v_intArray1 == null) || v_bool1))) {
            v_stack = v_stack.previous;
            if ((v_stack != null)) {
              v_pc = v_stack.pc;
              v_intList1.add(v_pc);
              v_intArray1 = v_esfData[v_pc];
            }
          }
          if ((v_stack == null)) {
            return v_uncaughtExceptionResult(v_vm, v_value);
          }
          v_int1 = v_intArray1[0];
          if ((v_int1 < v_pc)) {
            v_int1 = v_intArray1[1];
          }
          v_pc = (v_int1 - 1);
          v_stack.pc = v_pc;
          v_localsStackOffset = v_stack.localsStackOffset;
          v_localsStackSetToken = v_stack.localsStackSetToken;
          v_ec.stackTop = v_stack;
          v_stack.postFinallyBehavior = 0;
          v_ec.currentValueStackSize = v_valueStackSize;
          if ((false && (v_stack.debugStepTracker != null))) {
            v_hasInterrupt = true;
            v_ec.activeInterrupt = new Interrupt(5, 0, "", 0.0, v_stack.debugStepTracker);
          }
        }
        break;
      case 35:
        // FUNCTION_DEFINITION;
        v_initializeFunction(v_vm, v_row, v_pc, v_stringArgs[v_pc]);
        v_pc += v_row[7];
        v_functionTable = v_metadata.functionTable;
        break;
      case 36:
        // INDEX;
        v_value = v_valueStack[--v_valueStackSize];
        v_root = v_valueStack[(v_valueStackSize - 1)];
        if ((v_root.type == 6)) {
          if ((v_value.type != 3)) {
            v_hasInterrupt = v_EX_InvalidArgument(v_ec, "List index must be an integer.");
          } else {
            v_i = v_value.intValue;
            v_list1 = ((ListImpl) v_root.internalValue);
            if ((v_i < 0)) {
              v_i += v_list1.size;
            }
            if (((v_i < 0) || (v_i >= v_list1.size))) {
              v_hasInterrupt = v_EX_IndexOutOfRange(v_ec, "List index is out of bounds");
            } else {
              v_valueStack[(v_valueStackSize - 1)] = v_list1.array[v_i];
            }
          }
        } else {
          if ((v_root.type == 7)) {
            v_dictImpl = ((DictImpl) v_root.internalValue);
            v_keyType = v_value.type;
            if ((v_keyType != v_dictImpl.keyType)) {
              if ((v_dictImpl.size == 0)) {
                v_hasInterrupt = v_EX_KeyNotFound(v_ec, "Key not found. Dictionary is empty.");
              } else {
                v_hasInterrupt = v_EX_InvalidKey(v_ec, "Incorrect key type. This dictionary contains " + v_getTypeFromId(v_dictImpl.keyType) + " keys. Provided key is a " + v_getTypeFromId(v_keyType) + ".");
              }
            } else {
              if ((v_keyType == 3)) {
                v_intKey = v_value.intValue;
              } else {
                if ((v_keyType == 5)) {
                  v_stringKey = ((String) v_value.internalValue);
                } else {
                  if ((v_keyType == 8)) {
                    v_intKey = (((ObjectInstance) v_value.internalValue)).objectId;
                  } else {
                    if ((v_dictImpl.size == 0)) {
                      v_hasInterrupt = v_EX_KeyNotFound(v_ec, "Key not found. Dictionary is empty.");
                    } else {
                      v_hasInterrupt = v_EX_KeyNotFound(v_ec, "Key not found.");
                    }
                  }
                }
              }
              if (!v_hasInterrupt) {
                if ((v_keyType == 5)) {
                  v_stringIntDict1 = ((HashMap<String, Integer>) v_dictImpl.stringToIndex);
                  Integer dictLookup9 = v_stringIntDict1.get(v_stringKey);
                  v_int1 = dictLookup9 == null ? (v_stringIntDict1.containsKey(v_stringKey) ? null : (-1)) : dictLookup9;
                  if ((v_int1 == -1)) {
                    v_hasInterrupt = v_EX_KeyNotFound(v_ec, "Key not found: '" + v_stringKey + "'");
                  } else {
                    v_valueStack[(v_valueStackSize - 1)] = v_dictImpl.values.get(v_int1);
                  }
                } else {
                  v_intIntDict1 = ((HashMap<Integer, Integer>) v_dictImpl.intToIndex);
                  Integer dictLookup10 = v_intIntDict1.get(v_intKey);
                  v_int1 = dictLookup10 == null ? (-1) : dictLookup10;
                  if ((v_int1 == -1)) {
                    v_hasInterrupt = v_EX_KeyNotFound(v_ec, "Key not found.");
                  } else {
                    v_valueStack[(v_valueStackSize - 1)] = v_dictImpl.values.get(v_intIntDict1.get(v_intKey));
                  }
                }
              }
            }
          } else {
            if ((v_root.type == 5)) {
              v_string1 = ((String) v_root.internalValue);
              if ((v_value.type != 3)) {
                v_hasInterrupt = v_EX_InvalidArgument(v_ec, "String indices must be integers.");
              } else {
                v_int1 = v_value.intValue;
                if ((v_int1 < 0)) {
                  v_int1 += v_string1.length();
                }
                if (((v_int1 < 0) || (v_int1 >= v_string1.length()))) {
                  v_hasInterrupt = v_EX_IndexOutOfRange(v_ec, "String index out of range.");
                } else {
                  v_valueStack[(v_valueStackSize - 1)] = v_buildCommonString(v_globals, ("" + v_string1.charAt(v_int1)));
                }
              }
            } else {
              v_hasInterrupt = v_EX_InvalidArgument(v_ec, "Cannot index into this type: " + v_getTypeFromId(v_root.type));
            }
          }
        }
        break;
      case 37:
        // IS_COMPARISON;
        v_value = v_valueStack[(v_valueStackSize - 1)];
        v_output = v_VALUE_FALSE;
        if ((v_value.type == 8)) {
          v_objInstance1 = ((ObjectInstance) v_value.internalValue);
          if (v_isClassASubclassOf(v_vm, v_objInstance1.classId, v_row[0])) {
            v_output = v_VALUE_TRUE;
          }
        }
        v_valueStack[(v_valueStackSize - 1)] = v_output;
        break;
      case 38:
        // ITERATION_STEP;
        v_int1 = (v_localsStackOffset + v_row[2]);
        v_value3 = v_localsStack[v_int1];
        v_i = v_value3.intValue;
        v_value = v_localsStack[(v_localsStackOffset + v_row[3])];
        if ((v_value.type == 6)) {
          v_list1 = ((ListImpl) v_value.internalValue);
          v_len = v_list1.size;
          v_bool1 = true;
        } else {
          v_string2 = ((String) v_value.internalValue);
          v_len = v_string2.length();
          v_bool1 = false;
        }
        if ((v_i < v_len)) {
          if (v_bool1) {
            v_value = v_list1.array[v_i];
          } else {
            v_value = v_buildCommonString(v_globals, ("" + v_string2.charAt(v_i)));
          }
          v_int3 = (v_localsStackOffset + v_row[1]);
          v_localsStackSet[v_int3] = v_localsStackSetToken;
          v_localsStack[v_int3] = v_value;
        } else {
          v_pc += v_row[0];
        }
        v_i += 1;
        if ((v_i < 2049)) {
          v_localsStack[v_int1] = v_INTEGER_POSITIVE_CACHE[v_i];
        } else {
          v_localsStack[v_int1] = new Value(v_i);
        }
        break;
      case 39:
        // JUMP;
        v_pc += v_row[0];
        break;
      case 40:
        // JUMP_IF_EXCEPTION_OF_TYPE;
        v_value = v_ec.activeException;
        v_objInstance1 = ((ObjectInstance) v_value.internalValue);
        v_int1 = v_objInstance1.classId;
        v_i = (v_row.length - 1);
        while ((v_i >= 2)) {
          if (v_isClassASubclassOf(v_vm, v_int1, v_row[v_i])) {
            v_i = 0;
            v_pc += v_row[0];
            v_int2 = v_row[1];
            if ((v_int2 > -1)) {
              v_int1 = (v_localsStackOffset + v_int2);
              v_localsStack[v_int1] = v_value;
              v_localsStackSet[v_int1] = v_localsStackSetToken;
            }
          }
          v_i -= 1;
        }
        break;
      case 41:
        // JUMP_IF_FALSE;
        v_value = v_valueStack[--v_valueStackSize];
        if ((v_value.type != 2)) {
          v_hasInterrupt = v_EX_InvalidArgument(v_ec, "Boolean expected.");
        } else {
          if (!(v_value.intValue == 1)) {
            v_pc += v_row[0];
          }
        }
        break;
      case 42:
        // JUMP_IF_FALSE_NON_POP;
        v_value = v_valueStack[(v_valueStackSize - 1)];
        if ((v_value.type != 2)) {
          v_hasInterrupt = v_EX_InvalidArgument(v_ec, "Boolean expected.");
        } else {
          if ((v_value.intValue == 1)) {
            v_valueStackSize -= 1;
          } else {
            v_pc += v_row[0];
          }
        }
        break;
      case 43:
        // JUMP_IF_TRUE;
        v_value = v_valueStack[--v_valueStackSize];
        if ((v_value.type != 2)) {
          v_hasInterrupt = v_EX_InvalidArgument(v_ec, "Boolean expected.");
        } else {
          if ((v_value.intValue == 1)) {
            v_pc += v_row[0];
          }
        }
        break;
      case 44:
        // JUMP_IF_TRUE_NO_POP;
        v_value = v_valueStack[(v_valueStackSize - 1)];
        if ((v_value.type != 2)) {
          v_hasInterrupt = v_EX_InvalidArgument(v_ec, "Boolean expected.");
        } else {
          if ((v_value.intValue == 1)) {
            v_pc += v_row[0];
          } else {
            v_valueStackSize -= 1;
          }
        }
        break;
      case 45:
        // LAMBDA;
        if (!v_metadata.lambdaTable.containsKey(v_pc)) {
          v_int1 = (4 + v_row[4] + 1);
          v_len = v_row[v_int1];
          v_intArray1 = new int[v_len];
          v_i = 0;
          while ((v_i < v_len)) {
            v_intArray1[v_i] = v_row[(v_int1 + v_i + 1)];
            v_i += 1;
          }
          v_len = v_row[4];
          v_intArray2 = new int[v_len];
          v_i = 0;
          while ((v_i < v_len)) {
            v_intArray2[v_i] = v_row[(5 + v_i)];
            v_i += 1;
          }
          v_metadata.lambdaTable.put(v_pc, new FunctionInfo(v_pc, 0, v_pc, v_row[0], v_row[1], 5, 0, v_row[2], v_intArray2, "lambda", v_intArray1));
        }
        v_closure = new HashMap<Integer, ClosureValuePointer>();
        v_parentClosure = v_stack.closureVariables;
        if ((v_parentClosure == null)) {
          v_parentClosure = new HashMap<Integer, ClosureValuePointer>();
          v_stack.closureVariables = v_parentClosure;
        }
        v_functionInfo = v_metadata.lambdaTable.get(v_pc);
        v_intArray1 = v_functionInfo.closureIds;
        v_len = v_intArray1.length;
        v_i = 0;
        while ((v_i < v_len)) {
          v_j = v_intArray1[v_i];
          if (v_parentClosure.containsKey(v_j)) {
            v_closure.put(v_j, v_parentClosure.get(v_j));
          } else {
            v_closure.put(v_j, new ClosureValuePointer(null));
            v_parentClosure.put(v_j, v_closure.get(v_j));
          }
          v_i += 1;
        }
        if ((v_valueStackSize == v_valueStackCapacity)) {
          v_valueStack = v_valueStackIncreaseCapacity(v_ec);
          v_valueStackCapacity = v_valueStack.length;
        }
        v_valueStack[v_valueStackSize] = new Value(9, new FunctionPointer(5, null, 0, v_pc, v_closure));
        v_valueStackSize += 1;
        v_pc += v_row[3];
        break;
      case 46:
        // LIB_DECLARATION;
        v_prepareToSuspend(v_ec, v_stack, v_valueStackSize, v_pc);
        v_ec.activeInterrupt = new Interrupt(4, v_row[0], v_stringArgs[v_pc], 0.0, null);
        v_hasInterrupt = true;
        break;
      case 47:
        // LIST_SLICE;
        if ((v_row[2] == 1)) {
          v_valueStackSize -= 1;
          v_arg3 = v_valueStack[v_valueStackSize];
        } else {
          v_arg3 = null;
        }
        if ((v_row[1] == 1)) {
          v_valueStackSize -= 1;
          v_arg2 = v_valueStack[v_valueStackSize];
        } else {
          v_arg2 = null;
        }
        if ((v_row[0] == 1)) {
          v_valueStackSize -= 1;
          v_arg1 = v_valueStack[v_valueStackSize];
        } else {
          v_arg1 = null;
        }
        v_value = v_valueStack[(v_valueStackSize - 1)];
        v_value = v_performListSlice(v_globals, v_ec, v_value, v_arg1, v_arg2, v_arg3);
        v_hasInterrupt = (v_ec.activeInterrupt != null);
        if (!v_hasInterrupt) {
          v_valueStack[(v_valueStackSize - 1)] = v_value;
        }
        break;
      case 48:
        // LITERAL;
        if ((v_valueStackSize == v_valueStackCapacity)) {
          v_valueStack = v_valueStackIncreaseCapacity(v_ec);
          v_valueStackCapacity = v_valueStack.length;
        }
        v_valueStack[v_valueStackSize++] = v_literalTable[v_row[0]];
        break;
      case 49:
        // LITERAL_STREAM;
        v_int1 = v_row.length;
        if (((v_valueStackSize + v_int1) > v_valueStackCapacity)) {
          while (((v_valueStackSize + v_int1) > v_valueStackCapacity)) {
            v_valueStackIncreaseCapacity(v_ec);
            v_valueStack = v_ec.valueStack;
            v_valueStackCapacity = v_valueStack.length;
          }
        }
        v_i = v_int1;
        while ((--v_i >= 0)) {
          v_valueStack[v_valueStackSize++] = v_literalTable[v_row[v_i]];
        }
        break;
      case 50:
        // LOCAL;
        v_int1 = (v_localsStackOffset + v_row[0]);
        if ((v_localsStackSet[v_int1] == v_localsStackSetToken)) {
          if ((v_valueStackSize == v_valueStackCapacity)) {
            v_valueStack = v_valueStackIncreaseCapacity(v_ec);
            v_valueStackCapacity = v_valueStack.length;
          }
          v_valueStack[v_valueStackSize++] = v_localsStack[v_int1];
        } else {
          v_hasInterrupt = v_EX_UnassignedVariable(v_ec, "Variable used before it was set.");
        }
        break;
      case 51:
        // LOC_TABLE;
        v_initLocTable(v_vm, v_row);
        break;
      case 52:
        // NEGATIVE_SIGN;
        v_value = v_valueStack[(v_valueStackSize - 1)];
        v_type = v_value.type;
        if ((v_type == 3)) {
          v_valueStack[(v_valueStackSize - 1)] = v_buildInteger(v_globals, -v_value.intValue);
        } else {
          if ((v_type == 4)) {
            v_valueStack[(v_valueStackSize - 1)] = v_buildFloat(v_globals, -((double) v_value.internalValue));
          } else {
            v_hasInterrupt = v_EX_InvalidArgument(v_ec, "Negative sign can only be applied to numbers. Found " + v_getTypeFromId(v_type) + " instead.");
          }
        }
        break;
      case 53:
        // POP;
        v_valueStackSize -= 1;
        break;
      case 54:
        // POP_IF_NULL_OR_JUMP;
        v_value = v_valueStack[(v_valueStackSize - 1)];
        if ((v_value.type == 1)) {
          v_valueStackSize -= 1;
        } else {
          v_pc += v_row[0];
        }
        break;
      case 55:
        // PUSH_FUNC_REF;
        v_value = null;
        switch (v_row[1]) {
          case 0:
            v_value = new Value(9, new FunctionPointer(1, null, 0, v_row[0], null));
            break;
          case 1:
            v_value = new Value(9, new FunctionPointer(2, v_stack.objectContext, v_row[2], v_row[0], null));
            break;
          case 2:
            v_classId = v_row[2];
            v_classInfo = v_classTable[v_classId];
            v_staticConstructorNotInvoked = true;
            if ((v_classInfo.staticInitializationState < 2)) {
              v_stack.pc = v_pc;
              v_stackFrame2 = v_maybeInvokeStaticConstructor(v_vm, v_ec, v_stack, v_classInfo, v_valueStackSize, PST_intBuffer16);
              if ((PST_intBuffer16[0] == 1)) {
                return v_generateException(v_vm, v_stack, v_pc, v_valueStackSize, v_ec, 0, "Static initialization loop detected. The class this field is a member of is not done being initialized.");
              }
              if ((v_stackFrame2 != null)) {
                v_staticConstructorNotInvoked = false;
                v_stack = v_stackFrame2;
                v_pc = v_stack.pc;
                v_localsStackSetToken = v_stack.localsStackSetToken;
                v_localsStackOffset = v_stack.localsStackOffset;
              }
            }
            if (v_staticConstructorNotInvoked) {
              v_value = new Value(9, new FunctionPointer(3, null, v_classId, v_row[0], null));
            } else {
              v_value = null;
            }
            break;
        }
        if ((v_value != null)) {
          if ((v_valueStackSize == v_valueStackCapacity)) {
            v_valueStack = v_valueStackIncreaseCapacity(v_ec);
            v_valueStackCapacity = v_valueStack.length;
          }
          v_valueStack[v_valueStackSize] = v_value;
          v_valueStackSize += 1;
        }
        break;
      case 56:
        // RETURN;
        if ((v_esfData[v_pc] != null)) {
          v_intArray1 = v_esfData[v_pc];
          v_pc = (v_intArray1[1] - 1);
          if ((v_row[0] == 0)) {
            v_stack.returnValueTempStorage = v_VALUE_NULL;
          } else {
            v_stack.returnValueTempStorage = v_valueStack[(v_valueStackSize - 1)];
          }
          v_valueStackSize = v_stack.valueStackPopSize;
          v_stack.postFinallyBehavior = 3;
        } else {
          if ((v_stack.previous == null)) {
            return v_interpreterFinished(v_vm, v_ec);
          }
          if ((v_stack.markClassAsInitialized != 0)) {
            v_markClassAsInitialized(v_vm, v_stack, v_stack.markClassAsInitialized);
          }
          if (v_stack.returnValueUsed) {
            if ((v_row[0] == 0)) {
              v_valueStackSize = v_stack.valueStackPopSize;
              v_stack = v_stack.previous;
              if ((v_valueStackSize == v_valueStackCapacity)) {
                v_valueStack = v_valueStackIncreaseCapacity(v_ec);
                v_valueStackCapacity = v_valueStack.length;
              }
              v_valueStack[v_valueStackSize] = v_VALUE_NULL;
            } else {
              v_value = v_valueStack[(v_valueStackSize - 1)];
              v_valueStackSize = v_stack.valueStackPopSize;
              v_stack = v_stack.previous;
              v_valueStack[v_valueStackSize] = v_value;
            }
            v_valueStackSize += 1;
          } else {
            v_valueStackSize = v_stack.valueStackPopSize;
            v_stack = v_stack.previous;
          }
          v_pc = v_stack.pc;
          v_localsStackOffset = v_stack.localsStackOffset;
          v_localsStackSetToken = v_stack.localsStackSetToken;
          if ((false && (v_stack.debugStepTracker != null))) {
            v_hasInterrupt = true;
            v_ec.activeInterrupt = new Interrupt(5, 0, "", 0.0, v_stack.debugStepTracker);
          }
        }
        break;
      case 57:
        // STACK_INSERTION_FOR_INCREMENT;
        if ((v_valueStackSize == v_valueStackCapacity)) {
          v_valueStack = v_valueStackIncreaseCapacity(v_ec);
          v_valueStackCapacity = v_valueStack.length;
        }
        v_valueStack[v_valueStackSize] = v_valueStack[(v_valueStackSize - 1)];
        v_valueStack[(v_valueStackSize - 1)] = v_valueStack[(v_valueStackSize - 2)];
        v_valueStack[(v_valueStackSize - 2)] = v_valueStack[(v_valueStackSize - 3)];
        v_valueStack[(v_valueStackSize - 3)] = v_valueStack[v_valueStackSize];
        v_valueStackSize += 1;
        break;
      case 58:
        // STACK_SWAP_POP;
        v_valueStackSize -= 1;
        v_valueStack[(v_valueStackSize - 1)] = v_valueStack[v_valueStackSize];
        break;
      case 59:
        // SWITCH_INT;
        v_value = v_valueStack[--v_valueStackSize];
        if ((v_value.type == 3)) {
          v_intKey = v_value.intValue;
          v_integerSwitch = v_integerSwitchesByPc[v_pc];
          if ((v_integerSwitch == null)) {
            v_integerSwitch = v_initializeIntSwitchStatement(v_vm, v_pc, v_row);
          }
          Integer dictLookup11 = v_integerSwitch.get(v_intKey);
          v_i = dictLookup11 == null ? (-1) : dictLookup11;
          if ((v_i == -1)) {
            v_pc += v_row[0];
          } else {
            v_pc += v_i;
          }
        } else {
          v_hasInterrupt = v_EX_InvalidArgument(v_ec, "Switch statement expects an integer.");
        }
        break;
      case 60:
        // SWITCH_STRING;
        v_value = v_valueStack[--v_valueStackSize];
        if ((v_value.type == 5)) {
          v_stringKey = ((String) v_value.internalValue);
          v_stringSwitch = v_stringSwitchesByPc[v_pc];
          if ((v_stringSwitch == null)) {
            v_stringSwitch = v_initializeStringSwitchStatement(v_vm, v_pc, v_row);
          }
          Integer dictLookup12 = v_stringSwitch.get(v_stringKey);
          v_i = dictLookup12 == null ? (v_stringSwitch.containsKey(v_stringKey) ? null : (-1)) : dictLookup12;
          if ((v_i == -1)) {
            v_pc += v_row[0];
          } else {
            v_pc += v_i;
          }
        } else {
          v_hasInterrupt = v_EX_InvalidArgument(v_ec, "Switch statement expects a string.");
        }
        break;
      case 61:
        // THIS;
        if ((v_valueStackSize == v_valueStackCapacity)) {
          v_valueStack = v_valueStackIncreaseCapacity(v_ec);
          v_valueStackCapacity = v_valueStack.length;
        }
        v_valueStack[v_valueStackSize] = v_stack.objectContext;
        v_valueStackSize += 1;
        break;
      case 62:
        // THROW;
        v_valueStackSize -= 1;
        v_value = v_valueStack[v_valueStackSize];
        v_bool2 = (v_value.type == 8);
        if (v_bool2) {
          v_objInstance1 = ((ObjectInstance) v_value.internalValue);
          if (!v_isClassASubclassOf(v_vm, v_objInstance1.classId, v_magicNumbers.coreExceptionClassId)) {
            v_bool2 = false;
          }
        }
        if (v_bool2) {
          v_objArray1 = v_objInstance1.nativeData;
          v_intList1 = new ArrayList<Integer>();
          v_objArray1[1] = v_intList1;
          if (!v_isPcFromCore(v_vm, v_pc)) {
            v_intList1.add(v_pc);
          }
          v_ec.activeException = v_value;
          v_ec.activeExceptionHandled = false;
          v_stack.pc = v_pc;
          v_intArray1 = v_esfData[v_pc];
          v_value = v_ec.activeException;
          v_objInstance1 = ((ObjectInstance) v_value.internalValue);
          v_objArray1 = v_objInstance1.nativeData;
          v_bool1 = true;
          if ((v_objArray1[0] != null)) {
            v_bool1 = ((boolean) v_objArray1[0]);
          }
          v_intList1 = ((ArrayList<Integer>) v_objArray1[1]);
          while (((v_stack != null) && ((v_intArray1 == null) || v_bool1))) {
            v_stack = v_stack.previous;
            if ((v_stack != null)) {
              v_pc = v_stack.pc;
              v_intList1.add(v_pc);
              v_intArray1 = v_esfData[v_pc];
            }
          }
          if ((v_stack == null)) {
            return v_uncaughtExceptionResult(v_vm, v_value);
          }
          v_int1 = v_intArray1[0];
          if ((v_int1 < v_pc)) {
            v_int1 = v_intArray1[1];
          }
          v_pc = (v_int1 - 1);
          v_stack.pc = v_pc;
          v_localsStackOffset = v_stack.localsStackOffset;
          v_localsStackSetToken = v_stack.localsStackSetToken;
          v_ec.stackTop = v_stack;
          v_stack.postFinallyBehavior = 0;
          v_ec.currentValueStackSize = v_valueStackSize;
          if ((false && (v_stack.debugStepTracker != null))) {
            v_hasInterrupt = true;
            v_ec.activeInterrupt = new Interrupt(5, 0, "", 0.0, v_stack.debugStepTracker);
          }
        } else {
          v_hasInterrupt = v_EX_InvalidArgument(v_ec, "Thrown value is not an exception.");
        }
        break;
      case 63:
        // TOKEN_DATA;
        v_tokenDataImpl(v_vm, v_row);
        break;
      case 64:
        // USER_CODE_START;
        v_metadata.userCodeStart = v_row[0];
        break;
      case 65:
        // VERIFY_TYPE_IS_ITERABLE;
        v_value = v_valueStack[--v_valueStackSize];
        v_i = (v_localsStackOffset + v_row[0]);
        v_localsStack[v_i] = v_value;
        v_localsStackSet[v_i] = v_localsStackSetToken;
        v_int1 = v_value.type;
        if (((v_int1 != 6) && (v_int1 != 5))) {
          v_hasInterrupt = v_EX_InvalidArgument(v_ec, "Expected an iterable type, such as a list or string. Found " + v_getTypeFromId(v_int1) + " instead.");
        }
        v_i = (v_localsStackOffset + v_row[1]);
        v_localsStack[v_i] = v_VALUE_INT_ZERO;
        v_localsStackSet[v_i] = v_localsStackSetToken;
        break;
      default:
        // THIS SHOULD NEVER HAPPEN;
        return v_generateException(v_vm, v_stack, v_pc, v_valueStackSize, v_ec, 0, "Bad op code: " + Integer.toString(v_ops[v_pc]));
    }
    if (v_hasInterrupt) {
      Interrupt v_interrupt = v_ec.activeInterrupt;
      v_ec.activeInterrupt = null;
      if ((v_interrupt.type == 1)) {
        return v_generateException(v_vm, v_stack, v_pc, v_valueStackSize, v_ec, v_interrupt.exceptionType, v_interrupt.exceptionMessage);
      }
      if ((v_interrupt.type == 3)) {
        return new InterpreterResult(5, "", v_interrupt.sleepDurationSeconds, 0, false, "");
      }
      if ((v_interrupt.type == 4)) {
        return new InterpreterResult(6, "", 0.0, 0, false, v_interrupt.exceptionMessage);
      }
    }
    ++v_pc;
  }
}

public static boolean v_isClassASubclassOf(VmContext v_vm, int v_subClassId, int v_parentClassId) {
  if ((v_subClassId == v_parentClassId)) {
    return true;
  }
  ClassInfo[] v_classTable = v_vm.metadata.classTable;
  int v_classIdWalker = v_subClassId;
  while ((v_classIdWalker != -1)) {
    if ((v_classIdWalker == v_parentClassId)) {
      return true;
    }
    ClassInfo v_classInfo = v_classTable[v_classIdWalker];
    v_classIdWalker = v_classInfo.baseClassId;
  }
  return false;
}

public static boolean v_isPcFromCore(VmContext v_vm, int v_pc) {
  if ((v_vm.symbolData == null)) {
    return false;
  }
  ArrayList<Token> v_tokens = v_vm.symbolData.tokenData[v_pc];
  if ((v_tokens == null)) {
    return false;
  }
  Token v_token = v_tokens.get(0);
  String v_filename = v_tokenHelperGetFileLine(v_vm, v_token.fileId, 0);
  return "[Core]".equals(v_filename);
}

public static boolean v_isStringEqual(String v_a, String v_b) {
  if (((v_a == null) || (v_b == null))) {
    return ((v_a == null) && (v_b == null));
  }
  if (v_a.equals(v_b)) {
    return true;
  }
  return false;
}

public static boolean v_isVmResultRootExecContext(InterpreterResult v_result) {
  return v_result.isRootContext;
}

public static ListImpl v_makeEmptyList(int[] v_type, int v_capacity) {
  return new ListImpl(v_type, 0, v_capacity, new Value[v_capacity]);
}

public static int v_markClassAsInitialized(VmContext v_vm, StackFrame v_stack, int v_classId) {
  ClassInfo v_classInfo = v_vm.metadata.classTable[v_stack.markClassAsInitialized];
  v_classInfo.staticInitializationState = 2;
  v_vm.classStaticInitializationStack.remove(v_vm.classStaticInitializationStack.size() - 1);
  return 0;
}

public static StackFrame v_maybeInvokeStaticConstructor(VmContext v_vm, ExecutionContext v_ec, StackFrame v_stack, ClassInfo v_classInfo, int v_valueStackSize, int[] v_intOutParam) {
  PST_intBuffer16[0] = 0;
  int v_classId = v_classInfo.id;
  if ((v_classInfo.staticInitializationState == 1)) {
    ArrayList<Integer> v_classIdsBeingInitialized = v_vm.classStaticInitializationStack;
    if ((v_classIdsBeingInitialized.get((v_classIdsBeingInitialized.size() - 1)) != v_classId)) {
      PST_intBuffer16[0] = 1;
    }
    return null;
  }
  v_classInfo.staticInitializationState = 1;
  v_vm.classStaticInitializationStack.add(v_classId);
  FunctionInfo v_functionInfo = v_vm.metadata.functionTable[v_classInfo.staticConstructorFunctionId];
  v_stack.pc -= 1;
  int v_newFrameLocalsSize = v_functionInfo.localsSize;
  int v_currentFrameLocalsEnd = v_stack.localsStackOffsetEnd;
  if ((v_ec.localsStack.length <= (v_currentFrameLocalsEnd + v_newFrameLocalsSize))) {
    v_increaseLocalsStackCapacity(v_ec, v_newFrameLocalsSize);
  }
  if ((v_ec.localsStackSetToken > 2000000000)) {
    v_resetLocalsStackTokens(v_ec, v_stack);
  }
  v_ec.localsStackSetToken += 1;
  return new StackFrame(v_functionInfo.pc, v_ec.localsStackSetToken, v_currentFrameLocalsEnd, (v_currentFrameLocalsEnd + v_newFrameLocalsSize), v_stack, false, null, v_valueStackSize, v_classId, (v_stack.depth + 1), 0, null, null, null);
}

public static Value v_multiplyString(VmGlobals v_globals, Value v_strValue, String v_str, int v_n) {
  if ((v_n <= 2)) {
    if ((v_n == 1)) {
      return v_strValue;
    }
    if ((v_n == 2)) {
      return v_buildString(v_globals, v_str + v_str);
    }
    return v_globals.stringEmpty;
  }
  ArrayList<String> v_builder = new ArrayList<String>();
  while ((v_n > 0)) {
    v_n -= 1;
    v_builder.add(v_str);
  }
  v_str = PST_joinList("", v_builder);
  return v_buildString(v_globals, v_str);
}

public static int v_nextPowerOf2(int v_value) {
  if ((((v_value - 1) & v_value) == 0)) {
    return v_value;
  }
  int v_output = 1;
  while ((v_output < v_value)) {
    v_output *= 2;
  }
  return v_output;
}

public static int v_noop() {
  return 0;
}

public static Value v_performListSlice(VmGlobals v_globals, ExecutionContext v_ec, Value v_value, Value v_arg1, Value v_arg2, Value v_arg3) {
  int v_begin = 0;
  int v_end = 0;
  int v_step = 0;
  int v_length = 0;
  int v_i = 0;
  boolean v_isForward = false;
  boolean v_isString = false;
  String v_originalString = "";
  ListImpl v_originalList = null;
  ListImpl v_outputList = null;
  ArrayList<String> v_outputString = null;
  int v_status = 0;
  if ((v_arg3 != null)) {
    if ((v_arg3.type == 3)) {
      v_step = v_arg3.intValue;
      if ((v_step == 0)) {
        v_status = 2;
      }
    } else {
      v_status = 3;
      v_step = 1;
    }
  } else {
    v_step = 1;
  }
  v_isForward = (v_step > 0);
  if ((v_arg2 != null)) {
    if ((v_arg2.type == 3)) {
      v_end = v_arg2.intValue;
    } else {
      v_status = 3;
    }
  }
  if ((v_arg1 != null)) {
    if ((v_arg1.type == 3)) {
      v_begin = v_arg1.intValue;
    } else {
      v_status = 3;
    }
  }
  if ((v_value.type == 5)) {
    v_isString = true;
    v_originalString = ((String) v_value.internalValue);
    v_length = v_originalString.length();
  } else {
    if ((v_value.type == 6)) {
      v_isString = false;
      v_originalList = ((ListImpl) v_value.internalValue);
      v_length = v_originalList.size;
    } else {
      v_EX_InvalidArgument(v_ec, "Cannot apply slicing to " + v_getTypeFromId(v_value.type) + ". Must be string or list.");
      return v_globals.valueNull;
    }
  }
  if ((v_status >= 2)) {
    String v_msg = null;
    if (v_isString) {
      v_msg = "String";
    } else {
      v_msg = "List";
    }
    if ((v_status == 3)) {
      v_msg += " slice indexes must be integers. Found ";
      if (((v_arg1 != null) && (v_arg1.type != 3))) {
        v_EX_InvalidArgument(v_ec, v_msg + v_getTypeFromId(v_arg1.type) + " for begin index.");
        return v_globals.valueNull;
      }
      if (((v_arg2 != null) && (v_arg2.type != 3))) {
        v_EX_InvalidArgument(v_ec, v_msg + v_getTypeFromId(v_arg2.type) + " for end index.");
        return v_globals.valueNull;
      }
      if (((v_arg3 != null) && (v_arg3.type != 3))) {
        v_EX_InvalidArgument(v_ec, v_msg + v_getTypeFromId(v_arg3.type) + " for step amount.");
        return v_globals.valueNull;
      }
      v_EX_InvalidArgument(v_ec, "Invalid slice arguments.");
      return v_globals.valueNull;
    } else {
      v_EX_InvalidArgument(v_ec, v_msg + " slice step cannot be 0.");
      return v_globals.valueNull;
    }
  }
  v_status = v_canonicalizeListSliceArgs(PST_intBuffer16, v_arg1, v_arg2, v_begin, v_end, v_step, v_length, v_isForward);
  if ((v_status == 1)) {
    v_begin = PST_intBuffer16[0];
    v_end = PST_intBuffer16[1];
    if (v_isString) {
      v_outputString = new ArrayList<String>();
      if (v_isForward) {
        if ((v_step == 1)) {
          return v_buildString(v_globals, v_originalString.substring(v_begin, v_begin + (v_end - v_begin)));
        } else {
          while ((v_begin < v_end)) {
            v_outputString.add(("" + v_originalString.charAt(v_begin)));
            v_begin += v_step;
          }
        }
      } else {
        while ((v_begin > v_end)) {
          v_outputString.add(("" + v_originalString.charAt(v_begin)));
          v_begin += v_step;
        }
      }
      v_value = v_buildString(v_globals, PST_joinList("", v_outputString));
    } else {
      v_outputList = v_makeEmptyList(v_originalList.type, 10);
      if (v_isForward) {
        while ((v_begin < v_end)) {
          v_addToList(v_outputList, v_originalList.array[v_begin]);
          v_begin += v_step;
        }
      } else {
        while ((v_begin > v_end)) {
          v_addToList(v_outputList, v_originalList.array[v_begin]);
          v_begin += v_step;
        }
      }
      v_value = new Value(6, v_outputList);
    }
  } else {
    if ((v_status == 0)) {
      if (v_isString) {
        v_value = v_globals.stringEmpty;
      } else {
        v_value = new Value(6, v_makeEmptyList(v_originalList.type, 0));
      }
    } else {
      if ((v_status == 2)) {
        if (!v_isString) {
          v_outputList = v_makeEmptyList(v_originalList.type, v_length);
          v_i = 0;
          while ((v_i < v_length)) {
            v_addToList(v_outputList, v_originalList.array[v_i]);
            v_i += 1;
          }
          v_value = new Value(6, v_outputList);
        }
      } else {
        String v_msg = null;
        if (v_isString) {
          v_msg = "String";
        } else {
          v_msg = "List";
        }
        if ((v_status == 3)) {
          v_msg += " slice begin index is out of range.";
        } else {
          if (v_isForward) {
            v_msg += " slice begin index must occur before the end index when step is positive.";
          } else {
            v_msg += " slice begin index must occur after the end index when the step is negative.";
          }
        }
        v_EX_IndexOutOfRange(v_ec, v_msg);
        return v_globals.valueNull;
      }
    }
  }
  return v_value;
}

public static int v_prepareToSuspend(ExecutionContext v_ec, StackFrame v_stack, int v_valueStackSize, int v_currentPc) {
  v_ec.stackTop = v_stack;
  v_ec.currentValueStackSize = v_valueStackSize;
  v_stack.pc = (v_currentPc + 1);
  return 0;
}

public static int[] v_primitiveMethodsInitializeLookup(HashMap<String, Integer> v_nameLookups) {
  int v_length = v_nameLookups.size();
  int[] v_lookup = new int[v_length];
  int v_i = 0;
  while ((v_i < v_length)) {
    v_lookup[v_i] = -1;
    v_i += 1;
  }
  if (v_nameLookups.containsKey("add")) {
    v_lookup[v_nameLookups.get("add")] = 0;
  }
  if (v_nameLookups.containsKey("argCountMax")) {
    v_lookup[v_nameLookups.get("argCountMax")] = 1;
  }
  if (v_nameLookups.containsKey("argCountMin")) {
    v_lookup[v_nameLookups.get("argCountMin")] = 2;
  }
  if (v_nameLookups.containsKey("choice")) {
    v_lookup[v_nameLookups.get("choice")] = 3;
  }
  if (v_nameLookups.containsKey("clear")) {
    v_lookup[v_nameLookups.get("clear")] = 4;
  }
  if (v_nameLookups.containsKey("clone")) {
    v_lookup[v_nameLookups.get("clone")] = 5;
  }
  if (v_nameLookups.containsKey("concat")) {
    v_lookup[v_nameLookups.get("concat")] = 6;
  }
  if (v_nameLookups.containsKey("contains")) {
    v_lookup[v_nameLookups.get("contains")] = 7;
  }
  if (v_nameLookups.containsKey("createInstance")) {
    v_lookup[v_nameLookups.get("createInstance")] = 8;
  }
  if (v_nameLookups.containsKey("endsWith")) {
    v_lookup[v_nameLookups.get("endsWith")] = 9;
  }
  if (v_nameLookups.containsKey("filter")) {
    v_lookup[v_nameLookups.get("filter")] = 10;
  }
  if (v_nameLookups.containsKey("get")) {
    v_lookup[v_nameLookups.get("get")] = 11;
  }
  if (v_nameLookups.containsKey("getName")) {
    v_lookup[v_nameLookups.get("getName")] = 12;
  }
  if (v_nameLookups.containsKey("indexOf")) {
    v_lookup[v_nameLookups.get("indexOf")] = 13;
  }
  if (v_nameLookups.containsKey("insert")) {
    v_lookup[v_nameLookups.get("insert")] = 14;
  }
  if (v_nameLookups.containsKey("invoke")) {
    v_lookup[v_nameLookups.get("invoke")] = 15;
  }
  if (v_nameLookups.containsKey("isA")) {
    v_lookup[v_nameLookups.get("isA")] = 16;
  }
  if (v_nameLookups.containsKey("join")) {
    v_lookup[v_nameLookups.get("join")] = 17;
  }
  if (v_nameLookups.containsKey("keys")) {
    v_lookup[v_nameLookups.get("keys")] = 18;
  }
  if (v_nameLookups.containsKey("lower")) {
    v_lookup[v_nameLookups.get("lower")] = 19;
  }
  if (v_nameLookups.containsKey("ltrim")) {
    v_lookup[v_nameLookups.get("ltrim")] = 20;
  }
  if (v_nameLookups.containsKey("map")) {
    v_lookup[v_nameLookups.get("map")] = 21;
  }
  if (v_nameLookups.containsKey("merge")) {
    v_lookup[v_nameLookups.get("merge")] = 22;
  }
  if (v_nameLookups.containsKey("pop")) {
    v_lookup[v_nameLookups.get("pop")] = 23;
  }
  if (v_nameLookups.containsKey("remove")) {
    v_lookup[v_nameLookups.get("remove")] = 24;
  }
  if (v_nameLookups.containsKey("replace")) {
    v_lookup[v_nameLookups.get("replace")] = 25;
  }
  if (v_nameLookups.containsKey("reverse")) {
    v_lookup[v_nameLookups.get("reverse")] = 26;
  }
  if (v_nameLookups.containsKey("rtrim")) {
    v_lookup[v_nameLookups.get("rtrim")] = 27;
  }
  if (v_nameLookups.containsKey("shuffle")) {
    v_lookup[v_nameLookups.get("shuffle")] = 28;
  }
  if (v_nameLookups.containsKey("sort")) {
    v_lookup[v_nameLookups.get("sort")] = 29;
  }
  if (v_nameLookups.containsKey("split")) {
    v_lookup[v_nameLookups.get("split")] = 30;
  }
  if (v_nameLookups.containsKey("startsWith")) {
    v_lookup[v_nameLookups.get("startsWith")] = 31;
  }
  if (v_nameLookups.containsKey("trim")) {
    v_lookup[v_nameLookups.get("trim")] = 32;
  }
  if (v_nameLookups.containsKey("upper")) {
    v_lookup[v_nameLookups.get("upper")] = 33;
  }
  if (v_nameLookups.containsKey("values")) {
    v_lookup[v_nameLookups.get("values")] = 34;
  }
  return v_lookup;
}

public static String v_primitiveMethodWrongArgCountError(String v_name, int v_expected, int v_actual) {
  String v_output = "";
  if ((v_expected == 0)) {
    v_output = v_name + " does not accept any arguments.";
  } else {
    if ((v_expected == 1)) {
      v_output = v_name + " accepts exactly 1 argument.";
    } else {
      v_output = v_name + " requires " + Integer.toString(v_expected) + " arguments.";
    }
  }
  return v_output + " Found: " + Integer.toString(v_actual);
}

public static int v_printToStdOut(String v_prefix, String v_line) {
  if ((v_prefix == null)) {
    PlatformTranslationHelper.printStdOut(v_line);
  } else {
    String v_canonical = v_line.replace((CharSequence) "\r\n", (CharSequence) "\n").replace((CharSequence) "\r", (CharSequence) "\n");
    String[] v_lines = PST_literalStringSplit(v_canonical, "\n");
    int v_i = 0;
    while ((v_i < v_lines.length)) {
      PlatformTranslationHelper.printStdOut(v_prefix + ": " + v_lines[v_i]);
      v_i += 1;
    }
  }
  return 0;
}

public static int v_qsortHelper(String[] v_keyStringList, double[] v_keyNumList, int[] v_indices, boolean v_isString, int v_startIndex, int v_endIndex) {
  if (((v_endIndex - v_startIndex) <= 0)) {
    return 0;
  }
  if (((v_endIndex - v_startIndex) == 1)) {
    if (v_sortHelperIsRevOrder(v_keyStringList, v_keyNumList, v_isString, v_startIndex, v_endIndex)) {
      v_sortHelperSwap(v_keyStringList, v_keyNumList, v_indices, v_isString, v_startIndex, v_endIndex);
    }
    return 0;
  }
  int v_mid = ((v_endIndex + v_startIndex) >> 1);
  v_sortHelperSwap(v_keyStringList, v_keyNumList, v_indices, v_isString, v_mid, v_startIndex);
  int v_upperPointer = (v_endIndex + 1);
  int v_lowerPointer = (v_startIndex + 1);
  while ((v_upperPointer > v_lowerPointer)) {
    if (v_sortHelperIsRevOrder(v_keyStringList, v_keyNumList, v_isString, v_startIndex, v_lowerPointer)) {
      v_lowerPointer += 1;
    } else {
      v_upperPointer -= 1;
      v_sortHelperSwap(v_keyStringList, v_keyNumList, v_indices, v_isString, v_lowerPointer, v_upperPointer);
    }
  }
  int v_midIndex = (v_lowerPointer - 1);
  v_sortHelperSwap(v_keyStringList, v_keyNumList, v_indices, v_isString, v_midIndex, v_startIndex);
  v_qsortHelper(v_keyStringList, v_keyNumList, v_indices, v_isString, v_startIndex, (v_midIndex - 1));
  v_qsortHelper(v_keyStringList, v_keyNumList, v_indices, v_isString, (v_midIndex + 1), v_endIndex);
  return 0;
}

public static Value v_queryValue(VmContext v_vm, int v_execId, int v_stackFrameOffset, String[] v_steps) {
  if ((v_execId == -1)) {
    v_execId = v_vm.lastExecutionContextId;
  }
  ExecutionContext v_ec = v_vm.executionContexts.get(v_execId);
  StackFrame v_stackFrame = v_ec.stackTop;
  while ((v_stackFrameOffset > 0)) {
    v_stackFrameOffset -= 1;
    v_stackFrame = v_stackFrame.previous;
  }
  Value v_current = null;
  int v_i = 0;
  int v_j = 0;
  int v_len = v_steps.length;
  v_i = 0;
  while ((v_i < v_steps.length)) {
    if (((v_current == null) && (v_i > 0))) {
      return null;
    }
    String v_step = v_steps[v_i];
    if (v_isStringEqual(".", v_step)) {
      return null;
    } else {
      if (v_isStringEqual("this", v_step)) {
        v_current = v_stackFrame.objectContext;
      } else {
        if (v_isStringEqual("class", v_step)) {
          return null;
        } else {
          if (v_isStringEqual("local", v_step)) {
            v_i += 1;
            v_step = v_steps[v_i];
            HashMap<Integer, ArrayList<String>> v_localNamesByFuncPc = v_vm.symbolData.localVarNamesById;
            ArrayList<String> v_localNames = null;
            if (((v_localNamesByFuncPc == null) || (v_localNamesByFuncPc.size() == 0))) {
              return null;
            }
            v_j = v_stackFrame.pc;
            while ((v_j >= 0)) {
              if (v_localNamesByFuncPc.containsKey(v_j)) {
                v_localNames = v_localNamesByFuncPc.get(v_j);
                v_j = -1;
              }
              v_j -= 1;
            }
            if ((v_localNames == null)) {
              return null;
            }
            int v_localId = -1;
            if ((v_localNames != null)) {
              v_j = 0;
              while ((v_j < v_localNames.size())) {
                if (v_isStringEqual(v_localNames.get(v_j), v_step)) {
                  v_localId = v_j;
                  v_j = v_localNames.size();
                }
                v_j += 1;
              }
            }
            if ((v_localId == -1)) {
              return null;
            }
            int v_localOffset = (v_localId + v_stackFrame.localsStackOffset);
            if ((v_ec.localsStackSet[v_localOffset] != v_stackFrame.localsStackSetToken)) {
              return null;
            }
            v_current = v_ec.localsStack[v_localOffset];
          } else {
            if (v_isStringEqual("index", v_step)) {
              return null;
            } else {
              if (v_isStringEqual("key-int", v_step)) {
                return null;
              } else {
                if (v_isStringEqual("key-str", v_step)) {
                  return null;
                } else {
                  if (v_isStringEqual("key-obj", v_step)) {
                    return null;
                  } else {
                    return null;
                  }
                }
              }
            }
          }
        }
      }
    }
    v_i += 1;
  }
  return v_current;
}

public static int v_read_integer(int[] v_pindex, String v_raw, int v_length, String v_alphaNums) {
  int v_num = 0;
  char v_c = v_raw.charAt(v_pindex[0]);
  v_pindex[0] = (v_pindex[0] + 1);
  if ((v_c == '%')) {
    String v_value = v_read_till(v_pindex, v_raw, v_length, '%');
    v_num = Integer.parseInt(v_value);
  } else {
    if ((v_c == '@')) {
      v_num = v_read_integer(v_pindex, v_raw, v_length, v_alphaNums);
      v_num *= 62;
      v_num += v_read_integer(v_pindex, v_raw, v_length, v_alphaNums);
    } else {
      if ((v_c == '#')) {
        v_num = v_read_integer(v_pindex, v_raw, v_length, v_alphaNums);
        v_num *= 62;
        v_num += v_read_integer(v_pindex, v_raw, v_length, v_alphaNums);
        v_num *= 62;
        v_num += v_read_integer(v_pindex, v_raw, v_length, v_alphaNums);
      } else {
        if ((v_c == '^')) {
          v_num = (-1 * v_read_integer(v_pindex, v_raw, v_length, v_alphaNums));
        } else {
          // TODO: string.IndexOfChar(c);
          v_num = v_alphaNums.indexOf(("" + v_c));
          if ((v_num == -1)) {
          }
        }
      }
    }
  }
  return v_num;
}

public static String v_read_string(int[] v_pindex, String v_raw, int v_length, String v_alphaNums) {
  String v_b64 = v_read_till(v_pindex, v_raw, v_length, '%');
  return PST_base64ToString(v_b64);
}

public static String v_read_till(int[] v_index, String v_raw, int v_length, char v_end) {
  ArrayList<Character> v_output = new ArrayList<Character>();
  boolean v_ctn = true;
  char v_c = ' ';
  while (v_ctn) {
    v_c = v_raw.charAt(v_index[0]);
    if ((v_c == v_end)) {
      v_ctn = false;
    } else {
      v_output.add(v_c);
    }
    v_index[0] = (v_index[0] + 1);
  }
  return PST_joinChars(v_output);
}

public static int[] v_reallocIntArray(int[] v_original, int v_requiredCapacity) {
  int v_oldSize = v_original.length;
  int v_size = v_oldSize;
  while ((v_size < v_requiredCapacity)) {
    v_size *= 2;
  }
  int[] v_output = new int[v_size];
  int v_i = 0;
  while ((v_i < v_oldSize)) {
    v_output[v_i] = v_original[v_i];
    v_i += 1;
  }
  return v_output;
}

public static Value v_Reflect_allClasses(VmContext v_vm) {
  int[] v_generics = new int[1];
  v_generics[0] = 10;
  ListImpl v_output = v_makeEmptyList(v_generics, 20);
  ClassInfo[] v_classTable = v_vm.metadata.classTable;
  int v_i = 1;
  while ((v_i < v_classTable.length)) {
    ClassInfo v_classInfo = v_classTable[v_i];
    if ((v_classInfo == null)) {
      v_i = v_classTable.length;
    } else {
      v_addToList(v_output, new Value(10, new org.crayonlang.interpreter.structs.ClassValue(false, v_classInfo.id)));
    }
    v_i += 1;
  }
  return new Value(6, v_output);
}

public static Value v_Reflect_getMethods(VmContext v_vm, ExecutionContext v_ec, Value v_methodSource) {
  ListImpl v_output = v_makeEmptyList(null, 8);
  if ((v_methodSource.type == 8)) {
    ObjectInstance v_objInstance1 = ((ObjectInstance) v_methodSource.internalValue);
    ClassInfo v_classInfo = v_vm.metadata.classTable[v_objInstance1.classId];
    int v_i = 0;
    while ((v_i < v_classInfo.functionIds.length)) {
      int v_functionId = v_classInfo.functionIds[v_i];
      if ((v_functionId != -1)) {
        v_addToList(v_output, new Value(9, new FunctionPointer(2, v_methodSource, v_objInstance1.classId, v_functionId, null)));
      }
      v_i += 1;
    }
  } else {
    org.crayonlang.interpreter.structs.ClassValue v_classValue = ((org.crayonlang.interpreter.structs.ClassValue) v_methodSource.internalValue);
    ClassInfo v_classInfo = v_vm.metadata.classTable[v_classValue.classId];
    v_EX_UnsupportedOperation(v_ec, "static method reflection not implemented yet.");
  }
  return new Value(6, v_output);
}

public static int v_resetLocalsStackTokens(ExecutionContext v_ec, StackFrame v_stack) {
  Value[] v_localsStack = v_ec.localsStack;
  int[] v_localsStackSet = v_ec.localsStackSet;
  int v_i = v_stack.localsStackOffsetEnd;
  while ((v_i < v_localsStackSet.length)) {
    v_localsStackSet[v_i] = 0;
    v_localsStack[v_i] = null;
    v_i += 1;
  }
  StackFrame v_stackWalker = v_stack;
  while ((v_stackWalker != null)) {
    int v_token = v_stackWalker.localsStackSetToken;
    v_stackWalker.localsStackSetToken = 1;
    v_i = v_stackWalker.localsStackOffset;
    while ((v_i < v_stackWalker.localsStackOffsetEnd)) {
      if ((v_localsStackSet[v_i] == v_token)) {
        v_localsStackSet[v_i] = 1;
      } else {
        v_localsStackSet[v_i] = 0;
        v_localsStack[v_i] = null;
      }
      v_i += 1;
    }
    v_stackWalker = v_stackWalker.previous;
  }
  v_ec.localsStackSetToken = 1;
  return -1;
}

public static int v_resolvePrimitiveMethodName2(int[] v_lookup, int v_type, int v_globalNameId) {
  int v_output = v_lookup[v_globalNameId];
  if ((v_output != -1)) {
    switch ((v_type + (11 * v_output))) {
      case 82:
        return v_output;
      case 104:
        return v_output;
      case 148:
        return v_output;
      case 214:
        return v_output;
      case 225:
        return v_output;
      case 280:
        return v_output;
      case 291:
        return v_output;
      case 302:
        return v_output;
      case 335:
        return v_output;
      case 346:
        return v_output;
      case 357:
        return v_output;
      case 368:
        return v_output;
      case 6:
        return v_output;
      case 39:
        return v_output;
      case 50:
        return v_output;
      case 61:
        return v_output;
      case 72:
        return v_output;
      case 83:
        return v_output;
      case 116:
        return v_output;
      case 160:
        return v_output;
      case 193:
        return v_output;
      case 237:
        return v_output;
      case 259:
        return v_output;
      case 270:
        return v_output;
      case 292:
        return v_output;
      case 314:
        return v_output;
      case 325:
        return v_output;
      case 51:
        return v_output;
      case 62:
        return v_output;
      case 84:
        return v_output;
      case 128:
        return v_output;
      case 205:
        return v_output;
      case 249:
        return v_output;
      case 271:
        return v_output;
      case 381:
        return v_output;
      case 20:
        return v_output;
      case 31:
        return v_output;
      case 141:
        return v_output;
      case 174:
        return v_output;
      case 98:
        return v_output;
      case 142:
        return v_output;
      case 186:
        return v_output;
      default:
        return -1;
    }
  }
  return -1;
}

public static Value v_resource_manager_getResourceOfType(VmContext v_vm, String v_userPath, String v_type) {
  ResourceDB v_db = v_vm.resourceDatabase;
  HashMap<String, ResourceInfo> v_lookup = v_db.fileInfo;
  if (v_lookup.containsKey(v_userPath)) {
    ListImpl v_output = v_makeEmptyList(null, 2);
    ResourceInfo v_file = v_lookup.get(v_userPath);
    if (v_file.type.equals(v_type)) {
      v_addToList(v_output, v_vm.globals.boolTrue);
      v_addToList(v_output, v_buildString(v_vm.globals, v_file.internalPath));
    } else {
      v_addToList(v_output, v_vm.globals.boolFalse);
    }
    return new Value(6, v_output);
  }
  return v_vm.globals.valueNull;
}

public static int v_resource_manager_populate_directory_lookup(HashMap<String, ArrayList<String>> v_dirs, String v_path) {
  String[] v_parts = PST_literalStringSplit(v_path, "/");
  String v_pathBuilder = "";
  String v_file = "";
  int v_i = 0;
  while ((v_i < v_parts.length)) {
    v_file = v_parts[v_i];
    ArrayList<String> v_files = null;
    if (!v_dirs.containsKey(v_pathBuilder)) {
      v_files = new ArrayList<String>();
      v_dirs.put(v_pathBuilder, v_files);
    } else {
      v_files = v_dirs.get(v_pathBuilder);
    }
    v_files.add(v_file);
    if ((v_i > 0)) {
      v_pathBuilder = v_pathBuilder + "/" + v_file;
    } else {
      v_pathBuilder = v_file;
    }
    v_i += 1;
  }
  return 0;
}

public static ResourceDB v_resourceManagerInitialize(VmGlobals v_globals, String v_manifest) {
  HashMap<String, ArrayList<String>> v_filesPerDirectoryBuilder = new HashMap<String, ArrayList<String>>();
  HashMap<String, ResourceInfo> v_fileInfo = new HashMap<String, ResourceInfo>();
  ArrayList<Value> v_dataList = new ArrayList<Value>();
  String[] v_items = PST_literalStringSplit(v_manifest, "\n");
  ResourceInfo v_resourceInfo = null;
  String v_type = "";
  String v_userPath = "";
  String v_internalPath = "";
  String v_argument = "";
  boolean v_isText = false;
  int v_intType = 0;
  int v_i = 0;
  while ((v_i < v_items.length)) {
    String[] v_itemData = PST_literalStringSplit(v_items[v_i], ",");
    if ((v_itemData.length >= 3)) {
      v_type = v_itemData[0];
      v_isText = "TXT".equals(v_type);
      if (v_isText) {
        v_intType = 1;
      } else {
        if (("IMGSH".equals(v_type) || "IMG".equals(v_type))) {
          v_intType = 2;
        } else {
          if ("SND".equals(v_type)) {
            v_intType = 3;
          } else {
            if ("TTF".equals(v_type)) {
              v_intType = 4;
            } else {
              v_intType = 5;
            }
          }
        }
      }
      v_userPath = v_stringDecode(v_itemData[1]);
      v_internalPath = v_itemData[2];
      v_argument = "";
      if ((v_itemData.length > 3)) {
        v_argument = v_stringDecode(v_itemData[3]);
      }
      v_resourceInfo = new ResourceInfo(v_userPath, v_internalPath, v_isText, v_type, v_argument);
      v_fileInfo.put(v_userPath, v_resourceInfo);
      v_resource_manager_populate_directory_lookup(v_filesPerDirectoryBuilder, v_userPath);
      v_dataList.add(v_buildString(v_globals, v_userPath));
      v_dataList.add(v_buildInteger(v_globals, v_intType));
      if ((v_internalPath != null)) {
        v_dataList.add(v_buildString(v_globals, v_internalPath));
      } else {
        v_dataList.add(v_globals.valueNull);
      }
    }
    v_i += 1;
  }
  String[] v_dirs = PST_convertStringSetToArray(v_filesPerDirectoryBuilder.keySet());
  HashMap<String, String[]> v_filesPerDirectorySorted = new HashMap<String, String[]>();
  v_i = 0;
  while ((v_i < v_dirs.length)) {
    String v_dir = v_dirs[v_i];
    ArrayList<String> v_unsortedDirs = v_filesPerDirectoryBuilder.get(v_dir);
    String[] v_dirsSorted = v_unsortedDirs.toArray(PST_emptyArrayString);
    v_dirsSorted = PST_sortedCopyOfStringArray(v_dirsSorted);
    v_filesPerDirectorySorted.put(v_dir, v_dirsSorted);
    v_i += 1;
  }
  return new ResourceDB(v_filesPerDirectorySorted, v_fileInfo, v_dataList);
}

public static void v_reverseList(ListImpl v_list) {
  int v_len = v_list.size;
  Value v_t = null;
  int v_i2 = 0;
  Value[] v_arr = v_list.array;
  int v_i = (v_len >> 1);
  while ((v_i < v_len)) {
    v_i2 = (v_len - v_i - 1);
    v_t = v_arr[v_i];
    v_arr[v_i] = v_arr[v_i2];
    v_arr[v_i2] = v_t;
    v_i += 1;
  }
}

public static InterpreterResult v_runInterpreter(VmContext v_vm, int v_executionContextId) {
  InterpreterResult v_result = v_interpret(v_vm, v_executionContextId);
  v_result.executionContextId = v_executionContextId;
  int v_status = v_result.status;
  if ((v_status == 1)) {
    if (v_vm.executionContexts.containsKey(v_executionContextId)) {
      v_vm.executionContexts.remove(v_executionContextId);
    }
    v_runShutdownHandlers(v_vm);
  } else {
    if ((v_status == 3)) {
      v_printToStdOut(v_vm.environment.stacktracePrefix, v_result.errorMessage);
      v_runShutdownHandlers(v_vm);
    }
  }
  if ((v_executionContextId == 0)) {
    v_result.isRootContext = true;
  }
  return v_result;
}

public static InterpreterResult v_runInterpreterWithFunctionPointer(VmContext v_vm, Value v_fpValue, Value[] v_args) {
  int v_newId = (v_vm.lastExecutionContextId + 1);
  v_vm.lastExecutionContextId = v_newId;
  ArrayList<Value> v_argList = new ArrayList<Value>();
  int v_i = 0;
  while ((v_i < v_args.length)) {
    v_argList.add(v_args[v_i]);
    v_i += 1;
  }
  Value[] v_locals = new Value[0];
  int[] v_localsSet = new int[0];
  Value[] v_valueStack = new Value[100];
  v_valueStack[0] = v_fpValue;
  v_valueStack[1] = new Value(6, v_argList);
  StackFrame v_stack = new StackFrame((v_vm.byteCode.ops.length - 2), 1, 0, 0, null, false, null, 0, 0, 1, 0, null, null, null);
  ExecutionContext v_executionContext = new ExecutionContext(v_newId, v_stack, 2, 100, v_valueStack, v_locals, v_localsSet, 1, 0, false, null, false, 0, null);
  v_vm.executionContexts.put(v_newId, v_executionContext);
  return v_runInterpreter(v_vm, v_newId);
}

public static int v_runShutdownHandlers(VmContext v_vm) {
  while ((v_vm.shutdownHandlers.size() > 0)) {
    Value v_handler = v_vm.shutdownHandlers.get(0);
    v_vm.shutdownHandlers.remove(0);
    v_runInterpreterWithFunctionPointer(v_vm, v_handler, new Value[0]);
  }
  return 0;
}

public static void v_setItemInList(ListImpl v_list, int v_i, Value v_v) {
  v_list.array[v_i] = v_v;
}

public static boolean v_sortHelperIsRevOrder(String[] v_keyStringList, double[] v_keyNumList, boolean v_isString, int v_indexLeft, int v_indexRight) {
  if (v_isString) {
    return (v_keyStringList[v_indexLeft].compareTo(v_keyStringList[v_indexRight]) > 0);
  }
  return (v_keyNumList[v_indexLeft] > v_keyNumList[v_indexRight]);
}

public static int v_sortHelperSwap(String[] v_keyStringList, double[] v_keyNumList, int[] v_indices, boolean v_isString, int v_index1, int v_index2) {
  if ((v_index1 == v_index2)) {
    return 0;
  }
  int v_t = v_indices[v_index1];
  v_indices[v_index1] = v_indices[v_index2];
  v_indices[v_index2] = v_t;
  if (v_isString) {
    String v_s = v_keyStringList[v_index1];
    v_keyStringList[v_index1] = v_keyStringList[v_index2];
    v_keyStringList[v_index2] = v_s;
  } else {
    double v_n = v_keyNumList[v_index1];
    v_keyNumList[v_index1] = v_keyNumList[v_index2];
    v_keyNumList[v_index2] = v_n;
  }
  return 0;
}

public static int v_sortLists(ListImpl v_keyList, ListImpl v_parallelList, int[] v_intOutParam) {
  PST_intBuffer16[0] = 0;
  int v_length = v_keyList.size;
  if ((v_length < 2)) {
    return 0;
  }
  int v_i = 0;
  Value v_item = null;
  v_item = v_keyList.array[0];
  boolean v_isString = (v_item.type == 5);
  String[] v_stringKeys = null;
  double[] v_numKeys = null;
  if (v_isString) {
    v_stringKeys = new String[v_length];
  } else {
    v_numKeys = new double[v_length];
  }
  int[] v_indices = new int[v_length];
  Value[] v_originalOrder = new Value[v_length];
  v_i = 0;
  while ((v_i < v_length)) {
    v_indices[v_i] = v_i;
    v_originalOrder[v_i] = v_parallelList.array[v_i];
    v_item = v_keyList.array[v_i];
    switch (v_item.type) {
      case 3:
        if (v_isString) {
          PST_intBuffer16[0] = 1;
          return 0;
        }
        v_numKeys[v_i] = ((double) v_item.intValue);
        break;
      case 4:
        if (v_isString) {
          PST_intBuffer16[0] = 1;
          return 0;
        }
        v_numKeys[v_i] = ((double) v_item.internalValue);
        break;
      case 5:
        if (!v_isString) {
          PST_intBuffer16[0] = 1;
          return 0;
        }
        v_stringKeys[v_i] = ((String) v_item.internalValue);
        break;
      default:
        PST_intBuffer16[0] = 1;
        return 0;
    }
    v_i += 1;
  }
  v_qsortHelper(v_stringKeys, v_numKeys, v_indices, v_isString, 0, (v_length - 1));
  v_i = 0;
  while ((v_i < v_length)) {
    v_parallelList.array[v_i] = v_originalOrder[v_indices[v_i]];
    v_i += 1;
  }
  return 0;
}

public static boolean v_stackItemIsLibrary(String v_stackInfo) {
  if ((v_stackInfo.charAt(0) != '[')) {
    return false;
  }
  int v_cIndex = v_stackInfo.indexOf(":");
  return ((v_cIndex > 0) && (v_cIndex < v_stackInfo.indexOf("]")));
}

public static InterpreterResult v_startVm(VmContext v_vm) {
  return v_runInterpreter(v_vm, v_vm.lastExecutionContextId);
}

public static String v_stringDecode(String v_encoded) {
  if (!v_encoded.contains("%")) {
    int v_length = v_encoded.length();
    char v_per = '%';
    ArrayList<String> v_builder = new ArrayList<String>();
    int v_i = 0;
    while ((v_i < v_length)) {
      char v_c = v_encoded.charAt(v_i);
      if (((v_c == v_per) && ((v_i + 2) < v_length))) {
        v_builder.add(v_stringFromHex("" + ("" + v_encoded.charAt((v_i + 1))) + ("" + v_encoded.charAt((v_i + 2)))));
      } else {
        v_builder.add("" + ("" + v_c));
      }
      v_i += 1;
    }
    return PST_joinList("", v_builder);
  }
  return v_encoded;
}

public static String v_stringFromHex(String v_encoded) {
  v_encoded = v_encoded.toUpperCase();
  String v_hex = "0123456789ABCDEF";
  ArrayList<String> v_output = new ArrayList<String>();
  int v_length = v_encoded.length();
  int v_a = 0;
  int v_b = 0;
  String v_c = null;
  int v_i = 0;
  while (((v_i + 1) < v_length)) {
    v_c = "" + ("" + v_encoded.charAt(v_i));
    v_a = v_hex.indexOf(v_c);
    if ((v_a == -1)) {
      return null;
    }
    v_c = "" + ("" + v_encoded.charAt((v_i + 1)));
    v_b = v_hex.indexOf(v_c);
    if ((v_b == -1)) {
      return null;
    }
    v_a = ((v_a * 16) + v_b);
    v_output.add(("" + Character.toString((char) v_a)));
    v_i += 2;
  }
  return PST_joinList("", v_output);
}

public static InterpreterResult v_suspendInterpreter() {
  return new InterpreterResult(2, null, 0.0, 0, false, "");
}

public static int v_tokenDataImpl(VmContext v_vm, int[] v_row) {
  ArrayList<Token>[] v_tokensByPc = v_vm.symbolData.tokenData;
  int v_pc = (v_row[0] + v_vm.metadata.userCodeStart);
  int v_line = v_row[1];
  int v_col = v_row[2];
  int v_file = v_row[3];
  ArrayList<Token> v_tokens = v_tokensByPc[v_pc];
  if ((v_tokens == null)) {
    v_tokens = new ArrayList<Token>();
    v_tokensByPc[v_pc] = v_tokens;
  }
  v_tokens.add(new Token(v_line, v_col, v_file));
  return 0;
}

public static ArrayList<String> v_tokenHelperConvertPcsToStackTraceStrings(VmContext v_vm, ArrayList<Integer> v_pcs) {
  ArrayList<Token> v_tokens = v_generateTokenListFromPcs(v_vm, v_pcs);
  String[] v_files = v_vm.symbolData.sourceCode;
  ArrayList<String> v_output = new ArrayList<String>();
  int v_i = 0;
  while ((v_i < v_tokens.size())) {
    Token v_token = v_tokens.get(v_i);
    if ((v_token == null)) {
      v_output.add("[No stack information]");
    } else {
      int v_line = v_token.lineIndex;
      int v_col = v_token.colIndex;
      String v_fileData = v_files[v_token.fileId];
      String[] v_lines = PST_literalStringSplit(v_fileData, "\n");
      String v_filename = v_lines[0];
      String v_linevalue = v_lines[(v_line + 1)];
      v_output.add(v_filename + ", Line: " + Integer.toString((v_line + 1)) + ", Col: " + Integer.toString((v_col + 1)));
    }
    v_i += 1;
  }
  return v_output;
}

public static String v_tokenHelperGetFileLine(VmContext v_vm, int v_fileId, int v_lineNum) {
  String v_sourceCode = v_vm.symbolData.sourceCode[v_fileId];
  if ((v_sourceCode == null)) {
    return null;
  }
  return PST_literalStringSplit(v_sourceCode, "\n")[v_lineNum];
}

public static String v_tokenHelperGetFormattedPointerToToken(VmContext v_vm, Token v_token) {
  String v_line = v_tokenHelperGetFileLine(v_vm, v_token.fileId, (v_token.lineIndex + 1));
  if ((v_line == null)) {
    return null;
  }
  int v_columnIndex = v_token.colIndex;
  int v_lineLength = v_line.length();
  v_line = PST_trimSide(v_line, true);
  v_line = v_line.replace((CharSequence) "\t", (CharSequence) " ");
  int v_offset = (v_lineLength - v_line.length());
  v_columnIndex -= v_offset;
  String v_line2 = "";
  while ((v_columnIndex > 0)) {
    v_columnIndex -= 1;
    v_line2 = v_line2 + " ";
  }
  v_line2 = v_line2 + "^";
  return v_line + "\n" + v_line2;
}

public static boolean v_tokenHelplerIsFilePathLibrary(VmContext v_vm, int v_fileId, String[] v_allFiles) {
  String v_filename = v_tokenHelperGetFileLine(v_vm, v_fileId, 0);
  return !v_filename.toLowerCase().endsWith(".cry");
}

public static String v_typeInfoToString(VmContext v_vm, int[] v_typeInfo, int v_i) {
  ArrayList<String> v_output = new ArrayList<String>();
  v_typeToStringBuilder(v_vm, v_output, v_typeInfo, v_i);
  return PST_joinList("", v_output);
}

public static String v_typeToString(VmContext v_vm, int[] v_typeInfo, int v_i) {
  ArrayList<String> v_sb = new ArrayList<String>();
  v_typeToStringBuilder(v_vm, v_sb, v_typeInfo, v_i);
  return PST_joinList("", v_sb);
}

public static int v_typeToStringBuilder(VmContext v_vm, ArrayList<String> v_sb, int[] v_typeInfo, int v_i) {
  switch (v_typeInfo[v_i]) {
    case -1:
      v_sb.add("void");
      return (v_i + 1);
    case 0:
      v_sb.add("object");
      return (v_i + 1);
    case 1:
      v_sb.add("object");
      return (v_i + 1);
    case 3:
      v_sb.add("int");
      return (v_i + 1);
    case 4:
      v_sb.add("float");
      return (v_i + 1);
    case 2:
      v_sb.add("bool");
      return (v_i + 1);
    case 5:
      v_sb.add("string");
      return (v_i + 1);
    case 6:
      v_sb.add("List<");
      v_i = v_typeToStringBuilder(v_vm, v_sb, v_typeInfo, (v_i + 1));
      v_sb.add(">");
      return v_i;
    case 7:
      v_sb.add("Dictionary<");
      v_i = v_typeToStringBuilder(v_vm, v_sb, v_typeInfo, (v_i + 1));
      v_sb.add(", ");
      v_i = v_typeToStringBuilder(v_vm, v_sb, v_typeInfo, v_i);
      v_sb.add(">");
      return v_i;
    case 8:
      int v_classId = v_typeInfo[(v_i + 1)];
      if ((v_classId == 0)) {
        v_sb.add("object");
      } else {
        ClassInfo v_classInfo = v_vm.metadata.classTable[v_classId];
        v_sb.add(v_classInfo.fullyQualifiedName);
      }
      return (v_i + 2);
    case 10:
      v_sb.add("Class");
      return (v_i + 1);
    case 9:
      int v_n = v_typeInfo[(v_i + 1)];
      int v_optCount = v_typeInfo[(v_i + 2)];
      v_i += 2;
      v_sb.add("function(");
      ArrayList<String> v_ret = new ArrayList<String>();
      v_i = v_typeToStringBuilder(v_vm, v_ret, v_typeInfo, v_i);
      int v_j = 1;
      while ((v_j < v_n)) {
        if ((v_j > 1)) {
          v_sb.add(", ");
        }
        v_i = v_typeToStringBuilder(v_vm, v_sb, v_typeInfo, v_i);
        v_j += 1;
      }
      if ((v_n == 1)) {
        v_sb.add("void");
      }
      v_sb.add(" => ");
      int v_optStart = (v_n - v_optCount - 1);
      v_j = 0;
      while ((v_j < v_ret.size())) {
        if ((v_j >= v_optStart)) {
          v_sb.add("(opt) ");
        }
        v_sb.add(v_ret.get(v_j));
        v_j += 1;
      }
      v_sb.add(")");
      return v_i;
    default:
      v_sb.add("UNKNOWN");
      return (v_i + 1);
  }
}

public static String v_typeToStringFromValue(VmContext v_vm, Value v_value) {
  ArrayList<String> v_sb = null;
  switch (v_value.type) {
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
      int v_classId = (((ObjectInstance) v_value.internalValue)).classId;
      ClassInfo v_classInfo = v_vm.metadata.classTable[v_classId];
      return v_classInfo.fullyQualifiedName;
    case 6:
      v_sb = new ArrayList<String>();
      v_sb.add("List<");
      ListImpl v_list = ((ListImpl) v_value.internalValue);
      if ((v_list.type == null)) {
        v_sb.add("object");
      } else {
        v_typeToStringBuilder(v_vm, v_sb, v_list.type, 0);
      }
      v_sb.add(">");
      return PST_joinList("", v_sb);
    case 7:
      DictImpl v_dict = ((DictImpl) v_value.internalValue);
      v_sb = new ArrayList<String>();
      v_sb.add("Dictionary<");
      switch (v_dict.keyType) {
        case 3:
          v_sb.add("int");
          break;
        case 5:
          v_sb.add("string");
          break;
        case 8:
          v_sb.add("object");
          break;
        default:
          v_sb.add("???");
          break;
      }
      v_sb.add(", ");
      if ((v_dict.valueType == null)) {
        v_sb.add("object");
      } else {
        v_typeToStringBuilder(v_vm, v_sb, v_dict.valueType, 0);
      }
      v_sb.add(">");
      return PST_joinList("", v_sb);
    case 9:
      return "Function";
    default:
      return "Unknown";
  }
}

public static InterpreterResult v_uncaughtExceptionResult(VmContext v_vm, Value v_exception) {
  return new InterpreterResult(3, v_unrollExceptionOutput(v_vm, v_exception), 0.0, 0, false, "");
}

public static String v_unrollExceptionOutput(VmContext v_vm, Value v_exceptionInstance) {
  ObjectInstance v_objInstance = ((ObjectInstance) v_exceptionInstance.internalValue);
  ClassInfo v_classInfo = v_vm.metadata.classTable[v_objInstance.classId];
  ArrayList<Integer> v_pcs = ((ArrayList<Integer>) v_objInstance.nativeData[1]);
  String v_codeFormattedPointer = "";
  String v_exceptionName = v_classInfo.fullyQualifiedName;
  String v_message = v_valueToString(v_vm, v_objInstance.members[1]);
  ArrayList<String> v_trace = v_tokenHelperConvertPcsToStackTraceStrings(v_vm, v_pcs);
  v_trace.remove(v_trace.size() - 1);
  v_trace.add("Stack Trace:");
  java.util.Collections.reverse(v_trace);
  java.util.Collections.reverse(v_pcs);
  boolean v_showLibStack = v_vm.environment.showLibStack;
  if ((!v_showLibStack && !v_stackItemIsLibrary(v_trace.get(0)))) {
    while (v_stackItemIsLibrary(v_trace.get((v_trace.size() - 1)))) {
      v_trace.remove(v_trace.size() - 1);
      v_pcs.remove(v_pcs.size() - 1);
    }
  }
  ArrayList<Token> v_tokensAtPc = v_vm.symbolData.tokenData[v_pcs.get((v_pcs.size() - 1))];
  if ((v_tokensAtPc != null)) {
    v_codeFormattedPointer = "\n\n" + v_tokenHelperGetFormattedPointerToToken(v_vm, v_tokensAtPc.get(0));
  }
  String v_stackTrace = PST_joinList("\n", v_trace);
  return v_stackTrace + v_codeFormattedPointer + "\n" + v_exceptionName + ": " + v_message;
}

public static ListImpl v_valueConcatLists(ListImpl v_a, ListImpl v_b) {
  int v_aLen = v_a.size;
  int v_bLen = v_b.size;
  int v_size = (v_aLen + v_bLen);
  ListImpl v_c = new ListImpl(null, v_size, v_size, new Value[v_size]);
  int v_i = 0;
  while ((v_i < v_aLen)) {
    v_c.array[v_i] = v_a.array[v_i];
    v_i += 1;
  }
  v_i = 0;
  while ((v_i < v_bLen)) {
    v_c.array[(v_i + v_aLen)] = v_b.array[v_i];
    v_i += 1;
  }
  v_c.size = v_c.capacity;
  return v_c;
}

public static ListImpl v_valueMultiplyList(ListImpl v_a, int v_n) {
  int v_len = (v_a.size * v_n);
  ListImpl v_output = v_makeEmptyList(v_a.type, v_len);
  if ((v_len == 0)) {
    return v_output;
  }
  int v_aLen = v_a.size;
  int v_i = 0;
  Value v_value = null;
  if ((v_aLen == 1)) {
    v_value = v_a.array[0];
    v_i = 0;
    while ((v_i < v_n)) {
      v_output.array[v_i] = v_value;
      v_i += 1;
    }
  } else {
    int v_j = 0;
    v_i = 0;
    while ((v_i < v_n)) {
      v_j = 0;
      while ((v_j < v_aLen)) {
        v_output.array[((v_i * v_aLen) + v_j)] = v_a.array[v_j];
        v_j += 1;
      }
      v_i += 1;
    }
  }
  v_output.size = v_len;
  return v_output;
}

public static Value[] v_valueStackIncreaseCapacity(ExecutionContext v_ec) {
  Value[] v_stack = v_ec.valueStack;
  int v_oldCapacity = v_stack.length;
  int v_newCapacity = (v_oldCapacity * 2);
  Value[] v_newStack = new Value[v_newCapacity];
  int v_i = (v_oldCapacity - 1);
  while ((v_i >= 0)) {
    v_newStack[v_i] = v_stack[v_i];
    v_i -= 1;
  }
  v_ec.valueStack = v_newStack;
  return v_newStack;
}

public static String v_valueToString(VmContext v_vm, Value v_wrappedValue) {
  int v_type = v_wrappedValue.type;
  if ((v_type == 1)) {
    return "null";
  }
  if ((v_type == 2)) {
    if ((v_wrappedValue.intValue == 1)) {
      return "true";
    }
    return "false";
  }
  if ((v_type == 4)) {
    String v_floatStr = Double.toString(((double) v_wrappedValue.internalValue));
    if (!v_floatStr.contains(".")) {
      v_floatStr += ".0";
    }
    return v_floatStr;
  }
  if ((v_type == 3)) {
    return Integer.toString(v_wrappedValue.intValue);
  }
  if ((v_type == 5)) {
    return ((String) v_wrappedValue.internalValue);
  }
  if ((v_type == 6)) {
    ListImpl v_internalList = ((ListImpl) v_wrappedValue.internalValue);
    String v_output = "[";
    int v_i = 0;
    while ((v_i < v_internalList.size)) {
      if ((v_i > 0)) {
        v_output += ", ";
      }
      v_output += v_valueToString(v_vm, v_internalList.array[v_i]);
      v_i += 1;
    }
    v_output += "]";
    return v_output;
  }
  if ((v_type == 8)) {
    ObjectInstance v_objInstance = ((ObjectInstance) v_wrappedValue.internalValue);
    int v_classId = v_objInstance.classId;
    int v_ptr = v_objInstance.objectId;
    ClassInfo v_classInfo = v_vm.metadata.classTable[v_classId];
    int v_nameId = v_classInfo.nameId;
    String v_className = v_vm.metadata.identifiers[v_nameId];
    return "Instance<" + v_className + "#" + Integer.toString(v_ptr) + ">";
  }
  if ((v_type == 7)) {
    DictImpl v_dict = ((DictImpl) v_wrappedValue.internalValue);
    if ((v_dict.size == 0)) {
      return "{}";
    }
    String v_output = "{";
    ArrayList<Value> v_keyList = v_dict.keys;
    ArrayList<Value> v_valueList = v_dict.values;
    int v_i = 0;
    while ((v_i < v_dict.size)) {
      if ((v_i > 0)) {
        v_output += ", ";
      }
      v_output += v_valueToString(v_vm, v_dict.keys.get(v_i)) + ": " + v_valueToString(v_vm, v_dict.values.get(v_i));
      v_i += 1;
    }
    v_output += " }";
    return v_output;
  }
  return "<unknown>";
}

public static int v_vm_getCurrentExecutionContextId(VmContext v_vm) {
  return v_vm.lastExecutionContextId;
}

public static int v_vm_suspend(VmContext v_vm, int v_status) {
  return v_vm_suspend_for_context(v_getExecutionContext(v_vm, -1), 1);
}

public static int v_vm_suspend_for_context(ExecutionContext v_ec, int v_status) {
  v_ec.executionStateChange = true;
  v_ec.executionStateChangeCommand = v_status;
  return 0;
}

public static int v_vm_suspend_with_status(VmContext v_vm, int v_status) {
  return v_vm_suspend_for_context(v_getExecutionContext(v_vm, -1), v_status);
}

public static void v_vmEnvSetCommandLineArgs(VmContext v_vm, String[] v_args) {
  v_vm.environment.commandLineArgs = v_args;
}