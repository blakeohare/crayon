package org.crayonlang.interpreter.vm;

import java.util.ArrayList;
import java.util.HashMap;
import org.crayonlang.interpreter.PlatformTranslationHelper;;
import org.crayonlang.interpreter.structs.*;
import org.crayonlang.interpreter.TranslationHelper;;

public final class CrayonWrapper {

  private static java.util.Random PST_random = new java.util.Random();
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

  public static int addLiteralImpl(VmContext vm, int[] row, String stringArg) {
    VmGlobals g = vm.globals;
    int type = row[0];
    if ((type == 1)) {
      vm.metadata.literalTableBuilder.add(g.valueNull);
    } else {
      if ((type == 2)) {
        vm.metadata.literalTableBuilder.add(buildBoolean(g, (row[1] == 1)));
      } else {
        if ((type == 3)) {
          vm.metadata.literalTableBuilder.add(buildInteger(g, row[1]));
        } else {
          if ((type == 4)) {
            vm.metadata.literalTableBuilder.add(buildFloat(g, Double.parseDouble(stringArg)));
          } else {
            if ((type == 5)) {
              vm.metadata.literalTableBuilder.add(buildCommonString(g, stringArg));
            } else {
              if ((type == 9)) {
                int index = vm.metadata.literalTableBuilder.size();
                vm.metadata.literalTableBuilder.add(buildCommonString(g, stringArg));
                vm.metadata.invFunctionNameLiterals.put(stringArg, index);
              } else {
                if ((type == 10)) {
                  org.crayonlang.interpreter.structs.ClassValue cv = new org.crayonlang.interpreter.structs.ClassValue(false, row[1]);
                  vm.metadata.literalTableBuilder.add(new Value(10, cv));
                }
              }
            }
          }
        }
      }
    }
    return 0;
  }

  public static int addNameImpl(VmContext vm, String nameValue) {
    int index = vm.metadata.identifiersBuilder.size();
    vm.metadata.invIdentifiers.put(nameValue, index);
    vm.metadata.identifiersBuilder.add(nameValue);
    if ("length".equals(nameValue)) {
      vm.metadata.lengthId = index;
    }
    return 0;
  }

  public static void addToList(ListImpl list, Value item) {
    if ((list.size == list.capacity)) {
      increaseListCapacity(list);
    }
    list.array[list.size] = item;
    list.size += 1;
  }

  public static int applyDebugSymbolData(VmContext vm, int[] opArgs, String stringData, FunctionInfo recentlyDefinedFunction) {
    return 0;
  }

  public static Value buildBoolean(VmGlobals g, boolean value) {
    if (value) {
      return g.boolTrue;
    }
    return g.boolFalse;
  }

  public static Value buildCommonString(VmGlobals g, String s) {
    Value value = null;
    Value dictLookup0 = g.commonStrings.get(s);
    value = dictLookup0 == null ? (g.commonStrings.containsKey(s) ? null : (null)) : dictLookup0;
    if ((value == null)) {
      value = buildString(g, s);
      g.commonStrings.put(s, value);
    }
    return value;
  }

  public static Value buildFloat(VmGlobals g, double value) {
    if ((value == 0.0)) {
      return g.floatZero;
    }
    if ((value == 1.0)) {
      return g.floatOne;
    }
    return new Value(4, value);
  }

  public static Value buildInteger(VmGlobals g, int num) {
    if ((num < 0)) {
      if ((num > -257)) {
        return g.negativeIntegers[-num];
      }
    } else {
      if ((num < 2049)) {
        return g.positiveIntegers[num];
      }
    }
    return new Value(num);
  }

  public static Value buildList(ArrayList<Value> valueList) {
    return buildListWithType(null, valueList);
  }

  public static Value buildListWithType(int[] type, ArrayList<Value> valueList) {
    int _len = valueList.size();
    ListImpl output = makeEmptyList(type, _len);
    int i = 0;
    while ((i < _len)) {
      output.array[i] = valueList.get(i);
      i += 1;
    }
    output.size = _len;
    return new Value(6, output);
  }

  public static Value buildNull(VmGlobals globals) {
    return globals.valueNull;
  }

  public static PlatformRelayObject buildRelayObj(int type, int iarg1, int iarg2, int iarg3, double farg1, String sarg1) {
    return new PlatformRelayObject(type, iarg1, iarg2, iarg3, farg1, sarg1);
  }

  public static Value buildString(VmGlobals g, String s) {
    if ((s.length() == 0)) {
      return g.stringEmpty;
    }
    return new Value(5, s);
  }

  public static Value buildStringDictionary(VmGlobals globals, String[] stringKeys, Value[] values) {
    int size = stringKeys.length;
    DictImpl d = new DictImpl(size, 5, 0, null, new HashMap<Integer, Integer>(), new HashMap<String, Integer>(), new ArrayList<Value>(), new ArrayList<Value>());
    String k = null;
    int i = 0;
    while ((i < size)) {
      k = stringKeys[i];
      if (d.stringToIndex.containsKey(k)) {
        d.values.set(d.stringToIndex.get(k), values[i]);
      } else {
        d.stringToIndex.put(k, d.values.size());
        d.values.add(values[i]);
        d.keys.add(buildString(globals, k));
      }
      i += 1;
    }
    d.size = d.values.size();
    return new Value(7, d);
  }

  public static boolean canAssignGenericToGeneric(VmContext vm, int[] gen1, int gen1Index, int[] gen2, int gen2Index, int[] newIndexOut) {
    if ((gen2 == null)) {
      return true;
    }
    if ((gen1 == null)) {
      return false;
    }
    int t1 = gen1[gen1Index];
    int t2 = gen2[gen2Index];
    switch (t1) {
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
        if ((t2 != 8)) {
          return false;
        }
        int c1 = gen1[(gen1Index + 1)];
        int c2 = gen2[(gen2Index + 1)];
        if ((c1 == c2)) {
          return true;
        }
        return isClassASubclassOf(vm, c1, c2);
      case 6:
        if ((t2 != 6)) {
          return false;
        }
        return canAssignGenericToGeneric(vm, gen1, (gen1Index + 1), gen2, (gen2Index + 1), newIndexOut);
      case 7:
        if ((t2 != 7)) {
          return false;
        }
        if (!canAssignGenericToGeneric(vm, gen1, (gen1Index + 1), gen2, (gen2Index + 1), newIndexOut)) {
          return false;
        }
        return canAssignGenericToGeneric(vm, gen1, newIndexOut[0], gen2, newIndexOut[1], newIndexOut);
      case 9:
        if ((t2 != 9)) {
          return false;
        }
        return false;
      default:
        return false;
    }
  }

  public static Value canAssignTypeToGeneric(VmContext vm, Value value, int[] generics, int genericIndex) {
    switch (value.type) {
      case 1:
        switch (generics[genericIndex]) {
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
        if ((generics[genericIndex] == value.type)) {
          return value;
        }
        return null;
      case 5:
        if ((generics[genericIndex] == value.type)) {
          return value;
        }
        return null;
      case 10:
        if ((generics[genericIndex] == value.type)) {
          return value;
        }
        return null;
      case 3:
        if ((generics[genericIndex] == 3)) {
          return value;
        }
        if ((generics[genericIndex] == 4)) {
          return buildFloat(vm.globals, (0.0 + value.intValue));
        }
        return null;
      case 4:
        if ((generics[genericIndex] == 4)) {
          return value;
        }
        return null;
      case 6:
        ListImpl list = ((ListImpl) value.internalValue);
        int[] listType = list.type;
        genericIndex += 1;
        if ((listType == null)) {
          if (((generics[genericIndex] == 1) || (generics[genericIndex] == 0))) {
            return value;
          }
          return null;
        }
        int i = 0;
        while ((i < listType.length)) {
          if ((listType[i] != generics[(genericIndex + i)])) {
            return null;
          }
          i += 1;
        }
        return value;
      case 7:
        DictImpl dict = ((DictImpl) value.internalValue);
        int j = genericIndex;
        switch (dict.keyType) {
          case 3:
            if ((generics[1] == dict.keyType)) {
              j += 2;
            } else {
              return null;
            }
            break;
          case 5:
            if ((generics[1] == dict.keyType)) {
              j += 2;
            } else {
              return null;
            }
            break;
          case 8:
            if ((generics[1] == 8)) {
              j += 3;
            } else {
              return null;
            }
            break;
        }
        int[] valueType = dict.valueType;
        if ((valueType == null)) {
          if (((generics[j] == 0) || (generics[j] == 1))) {
            return value;
          }
          return null;
        }
        int k = 0;
        while ((k < valueType.length)) {
          if ((valueType[k] != generics[(j + k)])) {
            return null;
          }
          k += 1;
        }
        return value;
      case 8:
        if ((generics[genericIndex] == 8)) {
          int targetClassId = generics[(genericIndex + 1)];
          int givenClassId = (((ObjectInstance) value.internalValue)).classId;
          if ((targetClassId == givenClassId)) {
            return value;
          }
          if (isClassASubclassOf(vm, givenClassId, targetClassId)) {
            return value;
          }
        }
        return null;
    }
    return null;
  }

  public static double canonicalizeAngle(double a) {
    double twopi = 6.28318530717958;
    a = (a % twopi);
    if ((a < 0)) {
      a += twopi;
    }
    return a;
  }

  public static int canonicalizeListSliceArgs(int[] outParams, Value beginValue, Value endValue, int beginIndex, int endIndex, int stepAmount, int length, boolean isForward) {
    if ((beginValue == null)) {
      if (isForward) {
        beginIndex = 0;
      } else {
        beginIndex = (length - 1);
      }
    }
    if ((endValue == null)) {
      if (isForward) {
        endIndex = length;
      } else {
        endIndex = (-1 - length);
      }
    }
    if ((beginIndex < 0)) {
      beginIndex += length;
    }
    if ((endIndex < 0)) {
      endIndex += length;
    }
    if (((beginIndex == 0) && (endIndex == length) && (stepAmount == 1))) {
      return 2;
    }
    if (isForward) {
      if ((beginIndex >= length)) {
        return 0;
      }
      if ((beginIndex < 0)) {
        return 3;
      }
      if ((endIndex < beginIndex)) {
        return 4;
      }
      if ((beginIndex == endIndex)) {
        return 0;
      }
      if ((endIndex > length)) {
        endIndex = length;
      }
    } else {
      if ((beginIndex < 0)) {
        return 0;
      }
      if ((beginIndex >= length)) {
        return 3;
      }
      if ((endIndex > beginIndex)) {
        return 4;
      }
      if ((beginIndex == endIndex)) {
        return 0;
      }
      if ((endIndex < -1)) {
        endIndex = -1;
      }
    }
    outParams[0] = beginIndex;
    outParams[1] = endIndex;
    return 1;
  }

  public static String classIdToString(VmContext vm, int classId) {
    return vm.metadata.classTable[classId].fullyQualifiedName;
  }

  public static int clearList(ListImpl a) {
    int i = (a.size - 1);
    while ((i >= 0)) {
      a.array[i] = null;
      i -= 1;
    }
    a.size = 0;
    return 0;
  }

  public static DictImpl cloneDictionary(DictImpl original, DictImpl clone) {
    int type = original.keyType;
    int i = 0;
    int size = original.size;
    int kInt = 0;
    String kString = null;
    if ((clone == null)) {
      clone = new DictImpl(0, type, original.keyClassId, original.valueType, new HashMap<Integer, Integer>(), new HashMap<String, Integer>(), new ArrayList<Value>(), new ArrayList<Value>());
      if ((type == 5)) {
        while ((i < size)) {
          clone.stringToIndex.put(((String) original.keys.get(i).internalValue), i);
          i += 1;
        }
      } else {
        while ((i < size)) {
          if ((type == 8)) {
            kInt = (((ObjectInstance) original.keys.get(i).internalValue)).objectId;
          } else {
            kInt = original.keys.get(i).intValue;
          }
          clone.intToIndex.put(kInt, i);
          i += 1;
        }
      }
      i = 0;
      while ((i < size)) {
        clone.keys.add(original.keys.get(i));
        clone.values.add(original.values.get(i));
        i += 1;
      }
    } else {
      i = 0;
      while ((i < size)) {
        if ((type == 5)) {
          kString = ((String) original.keys.get(i).internalValue);
          if (clone.stringToIndex.containsKey(kString)) {
            clone.values.set(clone.stringToIndex.get(kString), original.values.get(i));
          } else {
            clone.stringToIndex.put(kString, clone.values.size());
            clone.values.add(original.values.get(i));
            clone.keys.add(original.keys.get(i));
          }
        } else {
          if ((type == 3)) {
            kInt = original.keys.get(i).intValue;
          } else {
            kInt = (((ObjectInstance) original.keys.get(i).internalValue)).objectId;
          }
          if (clone.intToIndex.containsKey(kInt)) {
            clone.values.set(clone.intToIndex.get(kInt), original.values.get(i));
          } else {
            clone.intToIndex.put(kInt, clone.values.size());
            clone.values.add(original.values.get(i));
            clone.keys.add(original.keys.get(i));
          }
        }
        i += 1;
      }
    }
    clone.size = (clone.intToIndex.size() + clone.stringToIndex.size());
    return clone;
  }

  public static int[] createInstanceType(int classId) {
    int[] o = new int[2];
    o[0] = 8;
    o[1] = classId;
    return o;
  }

  public static VmContext createVm(String rawByteCode, String resourceManifest) {
    VmGlobals globals = initializeConstantValues();
    ResourceDB resources = resourceManagerInitialize(globals, resourceManifest);
    Code byteCode = initializeByteCode(rawByteCode);
    Value[] localsStack = new Value[10];
    int[] localsStackSet = new int[10];
    int i = 0;
    i = (localsStack.length - 1);
    while ((i >= 0)) {
      localsStack[i] = null;
      localsStackSet[i] = 0;
      i -= 1;
    }
    StackFrame stack = new StackFrame(0, 1, 0, 0, null, false, null, 0, 0, 1, 0, null, null, null);
    ExecutionContext executionContext = new ExecutionContext(0, stack, 0, 100, new Value[100], localsStack, localsStackSet, 1, 0, false, null, false, 0, null);
    HashMap<Integer, ExecutionContext> executionContexts = new HashMap<Integer, ExecutionContext>();
    executionContexts.put(0, executionContext);
    VmContext vm = new VmContext(executionContexts, executionContext.id, byteCode, new SymbolData(new ArrayList[byteCode.ops.length], null, new ArrayList<String>(), null, null, new HashMap<Integer, ArrayList<String>>(), new HashMap<Integer, ArrayList<String>>()), new VmMetadata(null, new ArrayList<String>(), new HashMap<String, Integer>(), null, new ArrayList<Value>(), null, new ArrayList<HashMap<Integer, Integer>>(), null, new ArrayList<HashMap<String, Integer>>(), new ClassInfo[100], new FunctionInfo[100], new HashMap<Integer, FunctionInfo>(), null, new HashMap<Integer, java.lang.reflect.Method>(), -1, new int[10], 0, null, null, new MagicNumbers(0, 0, 0), new HashMap<String, Integer>(), new HashMap<Integer, HashMap<Integer, Integer>>(), null), 0, false, new ArrayList<Integer>(), null, resources, new ArrayList<Value>(), new VmEnvironment(new String[0], false, null, null), new NamedCallbackStore(new ArrayList<java.lang.reflect.Method>(), new HashMap<String, HashMap<String, Integer>>()), globals, globals.valueNull, globals.boolTrue, globals.boolFalse);
    return vm;
  }

  public static int debuggerClearBreakpoint(VmContext vm, int id) {
    return 0;
  }

  public static int debuggerFindPcForLine(VmContext vm, String path, int line) {
    return -1;
  }

  public static int debuggerSetBreakpoint(VmContext vm, String path, int line) {
    return -1;
  }

  public static boolean debugSetStepOverBreakpoint(VmContext vm) {
    return false;
  }

  public static int defOriginalCodeImpl(VmContext vm, int[] row, String fileContents) {
    int fileId = row[0];
    ArrayList<String> codeLookup = vm.symbolData.sourceCodeBuilder;
    while ((codeLookup.size() <= fileId)) {
      codeLookup.add(null);
    }
    codeLookup.set(fileId, fileContents);
    return 0;
  }

  public static String dictKeyInfoToString(VmContext vm, DictImpl dict) {
    if ((dict.keyType == 5)) {
      return "string";
    }
    if ((dict.keyType == 3)) {
      return "int";
    }
    if ((dict.keyClassId == 0)) {
      return "instance";
    }
    return classIdToString(vm, dict.keyClassId);
  }

  public static int doEqualityComparisonAndReturnCode(Value a, Value b) {
    int leftType = a.type;
    int rightType = b.type;
    if ((leftType == rightType)) {
      int output = 0;
      switch (leftType) {
        case 1:
          output = 1;
          break;
        case 3:
          if ((a.intValue == b.intValue)) {
            output = 1;
          }
          break;
        case 4:
          if ((((double) a.internalValue) == ((double) b.internalValue))) {
            output = 1;
          }
          break;
        case 2:
          if (((a.intValue == 1) == (b.intValue == 1))) {
            output = 1;
          }
          break;
        case 5:
          if ((((String) a.internalValue) == ((String) b.internalValue))) {
            output = 1;
          }
          break;
        case 6:
          if ((((Object) a.internalValue) == ((Object) b.internalValue))) {
            output = 1;
          }
          break;
        case 7:
          if ((((Object) a.internalValue) == ((Object) b.internalValue))) {
            output = 1;
          }
          break;
        case 8:
          if ((((Object) a.internalValue) == ((Object) b.internalValue))) {
            output = 1;
          }
          break;
        case 9:
          FunctionPointer f1 = ((FunctionPointer) a.internalValue);
          FunctionPointer f2 = ((FunctionPointer) b.internalValue);
          if ((f1.functionId == f2.functionId)) {
            if (((f1.type == 2) || (f1.type == 4))) {
              if ((doEqualityComparisonAndReturnCode(f1.context, f2.context) == 1)) {
                output = 1;
              }
            } else {
              output = 1;
            }
          }
          break;
        case 10:
          org.crayonlang.interpreter.structs.ClassValue c1 = ((org.crayonlang.interpreter.structs.ClassValue) a.internalValue);
          org.crayonlang.interpreter.structs.ClassValue c2 = ((org.crayonlang.interpreter.structs.ClassValue) b.internalValue);
          if ((c1.classId == c2.classId)) {
            output = 1;
          }
          break;
        default:
          output = 2;
          break;
      }
      return output;
    }
    if ((rightType == 1)) {
      return 0;
    }
    if (((leftType == 3) && (rightType == 4))) {
      if ((a.intValue == ((double) b.internalValue))) {
        return 1;
      }
    } else {
      if (((leftType == 4) && (rightType == 3))) {
        if ((((double) a.internalValue) == b.intValue)) {
          return 1;
        }
      }
    }
    return 0;
  }

  public static String encodeBreakpointData(VmContext vm, BreakpointInfo breakpoint, int pc) {
    return null;
  }

  public static InterpreterResult errorResult(String error) {
    return new InterpreterResult(3, error, 0.0, 0, false, "");
  }

  public static boolean EX_AssertionFailed(ExecutionContext ec, String exMsg) {
    return generateException2(ec, 2, exMsg);
  }

  public static boolean EX_DivisionByZero(ExecutionContext ec, String exMsg) {
    return generateException2(ec, 3, exMsg);
  }

  public static boolean EX_Fatal(ExecutionContext ec, String exMsg) {
    return generateException2(ec, 0, exMsg);
  }

  public static boolean EX_IndexOutOfRange(ExecutionContext ec, String exMsg) {
    return generateException2(ec, 4, exMsg);
  }

  public static boolean EX_InvalidArgument(ExecutionContext ec, String exMsg) {
    return generateException2(ec, 5, exMsg);
  }

  public static boolean EX_InvalidAssignment(ExecutionContext ec, String exMsg) {
    return generateException2(ec, 6, exMsg);
  }

  public static boolean EX_InvalidInvocation(ExecutionContext ec, String exMsg) {
    return generateException2(ec, 7, exMsg);
  }

  public static boolean EX_InvalidKey(ExecutionContext ec, String exMsg) {
    return generateException2(ec, 8, exMsg);
  }

  public static boolean EX_KeyNotFound(ExecutionContext ec, String exMsg) {
    return generateException2(ec, 9, exMsg);
  }

  public static boolean EX_NullReference(ExecutionContext ec, String exMsg) {
    return generateException2(ec, 10, exMsg);
  }

  public static boolean EX_UnassignedVariable(ExecutionContext ec, String exMsg) {
    return generateException2(ec, 11, exMsg);
  }

  public static boolean EX_UnknownField(ExecutionContext ec, String exMsg) {
    return generateException2(ec, 12, exMsg);
  }

  public static boolean EX_UnsupportedOperation(ExecutionContext ec, String exMsg) {
    return generateException2(ec, 13, exMsg);
  }

  public static int finalizeInitializationImpl(VmContext vm, String projectId, int localeCount) {
    vm.symbolData.sourceCode = vm.symbolData.sourceCodeBuilder.toArray(PST_emptyArrayString);
    vm.symbolData.sourceCodeBuilder = null;
    vm.metadata.magicNumbers.totalLocaleCount = localeCount;
    vm.metadata.identifiers = vm.metadata.identifiersBuilder.toArray(PST_emptyArrayString);
    vm.metadata.literalTable = vm.metadata.literalTableBuilder.toArray(Value.EMPTY_ARRAY);
    vm.metadata.globalNameIdToPrimitiveMethodName = primitiveMethodsInitializeLookup(vm.metadata.invIdentifiers);
    vm.funcArgs = new Value[vm.metadata.identifiers.length];
    vm.metadata.projectId = projectId;
    vm.metadata.identifiersBuilder = null;
    vm.metadata.literalTableBuilder = null;
    vm.initializationComplete = true;
    return 0;
  }

  public static double fixFuzzyFloatPrecision(double x) {
    if (((x % 1) != 0)) {
      double u = (x % 1);
      if ((u < 0)) {
        u += 1.0;
      }
      boolean roundDown = false;
      if ((u > 0.9999999999)) {
        roundDown = true;
        x += 0.1;
      } else {
        if ((u < 0.00000000002250000000)) {
          roundDown = true;
        }
      }
      if (roundDown) {
        if ((false || (x > 0))) {
          x = (((int) x) + 0.0);
        } else {
          x = (((int) x) - 1.0);
        }
      }
    }
    return x;
  }

  public static int[][] generateEsfData(int byteCodeLength, int[] esfArgs) {
    int[][] output = new int[byteCodeLength][];
    ArrayList<int[]> esfTokenStack = new ArrayList<int[]>();
    int[] esfTokenStackTop = null;
    int esfArgIterator = 0;
    int esfArgLength = esfArgs.length;
    int j = 0;
    int pc = 0;
    while ((pc < byteCodeLength)) {
      if (((esfArgIterator < esfArgLength) && (pc == esfArgs[esfArgIterator]))) {
        esfTokenStackTop = new int[2];
        j = 1;
        while ((j < 3)) {
          esfTokenStackTop[(j - 1)] = esfArgs[(esfArgIterator + j)];
          j += 1;
        }
        esfTokenStack.add(esfTokenStackTop);
        esfArgIterator += 3;
      }
      while (((esfTokenStackTop != null) && (esfTokenStackTop[1] <= pc))) {
        esfTokenStack.remove(esfTokenStack.size() - 1);
        if ((esfTokenStack.size() == 0)) {
          esfTokenStackTop = null;
        } else {
          esfTokenStackTop = esfTokenStack.get((esfTokenStack.size() - 1));
        }
      }
      output[pc] = esfTokenStackTop;
      pc += 1;
    }
    return output;
  }

  public static InterpreterResult generateException(VmContext vm, StackFrame stack, int pc, int valueStackSize, ExecutionContext ec, int type, String message) {
    ec.currentValueStackSize = valueStackSize;
    stack.pc = pc;
    MagicNumbers mn = vm.metadata.magicNumbers;
    int generateExceptionFunctionId = mn.coreGenerateExceptionFunctionId;
    FunctionInfo functionInfo = vm.metadata.functionTable[generateExceptionFunctionId];
    pc = functionInfo.pc;
    if ((ec.localsStack.length <= (functionInfo.localsSize + stack.localsStackOffsetEnd))) {
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

  public static boolean generateException2(ExecutionContext ec, int exceptionType, String exMsg) {
    ec.activeInterrupt = new Interrupt(1, exceptionType, exMsg, 0.0, null);
    return true;
  }

  public static Value generatePrimitiveMethodReference(int[] lookup, int globalNameId, Value context) {
    int functionId = resolvePrimitiveMethodName2(lookup, context.type, globalNameId);
    if ((functionId < 0)) {
      return null;
    }
    return new Value(9, new FunctionPointer(4, context, 0, functionId, null));
  }

  public static ArrayList<Token> generateTokenListFromPcs(VmContext vm, ArrayList<Integer> pcs) {
    ArrayList<Token> output = new ArrayList<Token>();
    ArrayList<Token>[] tokensByPc = vm.symbolData.tokenData;
    Token token = null;
    int i = 0;
    while ((i < pcs.size())) {
      ArrayList<Token> localTokens = tokensByPc[pcs.get(i)];
      if ((localTokens == null)) {
        if ((output.size() > 0)) {
          output.add(null);
        }
      } else {
        token = localTokens.get(0);
        output.add(token);
      }
      i += 1;
    }
    return output;
  }

  public static String getBinaryOpFromId(int id) {
    switch (id) {
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

  public static ClassInfo[] getClassTable(VmContext vm, int classId) {
    ClassInfo[] oldTable = vm.metadata.classTable;
    int oldLength = oldTable.length;
    if ((classId < oldLength)) {
      return oldTable;
    }
    int newLength = (oldLength * 2);
    if ((classId >= newLength)) {
      newLength = (classId + 100);
    }
    ClassInfo[] newTable = new ClassInfo[newLength];
    int i = (oldLength - 1);
    while ((i >= 0)) {
      newTable[i] = oldTable[i];
      i -= 1;
    }
    vm.metadata.classTable = newTable;
    return newTable;
  }

  public static ExecutionContext getExecutionContext(VmContext vm, int id) {
    if ((id == -1)) {
      id = vm.lastExecutionContextId;
    }
    if (vm.executionContexts.containsKey(id)) {
      return vm.executionContexts.get(id);
    }
    return null;
  }

  public static double getFloat(Value num) {
    if ((num.type == 4)) {
      return ((double) num.internalValue);
    }
    return (num.intValue + 0.0);
  }

  public static FunctionInfo[] getFunctionTable(VmContext vm, int functionId) {
    FunctionInfo[] oldTable = vm.metadata.functionTable;
    int oldLength = oldTable.length;
    if ((functionId < oldLength)) {
      return oldTable;
    }
    int newLength = (oldLength * 2);
    if ((functionId >= newLength)) {
      newLength = (functionId + 100);
    }
    FunctionInfo[] newTable = new FunctionInfo[newLength];
    int i = 0;
    while ((i < oldLength)) {
      newTable[i] = oldTable[i];
      i += 1;
    }
    vm.metadata.functionTable = newTable;
    return newTable;
  }

  public static Value getItemFromList(ListImpl list, int i) {
    return list.array[i];
  }

  public static int getNamedCallbackId(VmContext vm, String scope, String functionName) {
    return getNamedCallbackIdImpl(vm, scope, functionName, false);
  }

  public static int getNamedCallbackIdImpl(VmContext vm, String scope, String functionName, boolean allocIfMissing) {
    HashMap<String, HashMap<String, Integer>> lookup = vm.namedCallbacks.callbackIdLookup;
    HashMap<String, Integer> idsForScope = null;
    HashMap<String, Integer> dictLookup1 = lookup.get(scope);
    idsForScope = dictLookup1 == null ? (lookup.containsKey(scope) ? null : (null)) : dictLookup1;
    if ((idsForScope == null)) {
      idsForScope = new HashMap<String, Integer>();
      lookup.put(scope, idsForScope);
    }
    int id = -1;
    Integer dictLookup2 = idsForScope.get(functionName);
    id = dictLookup2 == null ? (idsForScope.containsKey(functionName) ? null : (-1)) : dictLookup2;
    if (((id == -1) && allocIfMissing)) {
      id = vm.namedCallbacks.callbacksById.size();
      vm.namedCallbacks.callbacksById.add(null);
      idsForScope.put(functionName, id);
    }
    return id;
  }

  public static Object getNativeDataItem(Value objValue, int index) {
    ObjectInstance obj = ((ObjectInstance) objValue.internalValue);
    return obj.nativeData[index];
  }

  public static String getTypeFromId(int id) {
    switch (id) {
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

  public static double getVmReinvokeDelay(InterpreterResult result) {
    return result.reinvokeDelay;
  }

  public static String getVmResultAssemblyInfo(InterpreterResult result) {
    return result.loadAssemblyInformation;
  }

  public static int getVmResultExecId(InterpreterResult result) {
    return result.executionContextId;
  }

  public static int getVmResultStatus(InterpreterResult result) {
    return result.status;
  }

  public static void increaseListCapacity(ListImpl list) {
    int oldCapacity = list.capacity;
    int newCapacity = (oldCapacity * 2);
    if ((newCapacity < 8)) {
      newCapacity = 8;
    }
    Value[] newArr = new Value[newCapacity];
    Value[] oldArr = list.array;
    int i = 0;
    while ((i < oldCapacity)) {
      newArr[i] = oldArr[i];
      i += 1;
    }
    list.capacity = newCapacity;
    list.array = newArr;
  }

  public static int increaseLocalsStackCapacity(ExecutionContext ec, int newScopeSize) {
    Value[] oldLocals = ec.localsStack;
    int[] oldSetIndicator = ec.localsStackSet;
    int oldCapacity = oldLocals.length;
    int newCapacity = ((oldCapacity * 2) + newScopeSize);
    Value[] newLocals = new Value[newCapacity];
    int[] newSetIndicator = new int[newCapacity];
    int i = 0;
    while ((i < oldCapacity)) {
      newLocals[i] = oldLocals[i];
      newSetIndicator[i] = oldSetIndicator[i];
      i += 1;
    }
    ec.localsStack = newLocals;
    ec.localsStackSet = newSetIndicator;
    return 0;
  }

  public static int initFileNameSymbolData(VmContext vm) {
    SymbolData symbolData = vm.symbolData;
    if ((symbolData == null)) {
      return 0;
    }
    if ((symbolData.fileNameById == null)) {
      int i = 0;
      String[] filenames = new String[symbolData.sourceCode.length];
      HashMap<String, Integer> fileIdByPath = new HashMap<String, Integer>();
      i = 0;
      while ((i < filenames.length)) {
        String sourceCode = symbolData.sourceCode[i];
        if ((sourceCode != null)) {
          int colon = sourceCode.indexOf("\n");
          if ((colon != -1)) {
            String filename = sourceCode.substring(0, 0 + colon);
            filenames[i] = filename;
            fileIdByPath.put(filename, i);
          }
        }
        i += 1;
      }
      symbolData.fileNameById = filenames;
      symbolData.fileIdByName = fileIdByPath;
    }
    return 0;
  }

  public static Code initializeByteCode(String raw) {
    int[] index = new int[1];
    index[0] = 0;
    int length = raw.length();
    String header = read_till(index, raw, length, '@');
    if ((header != "CRAYON")) {
    }
    String alphaNums = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
    int opCount = read_integer(index, raw, length, alphaNums);
    int[] ops = new int[opCount];
    int[][] iargs = new int[opCount][];
    String[] sargs = new String[opCount];
    char c = ' ';
    int argc = 0;
    int j = 0;
    String stringarg = null;
    boolean stringPresent = false;
    int iarg = 0;
    int[] iarglist = null;
    int i = 0;
    i = 0;
    while ((i < opCount)) {
      c = raw.charAt(index[0]);
      index[0] = (index[0] + 1);
      argc = 0;
      stringPresent = true;
      if ((c == '!')) {
        argc = 1;
      } else {
        if ((c == '&')) {
          argc = 2;
        } else {
          if ((c == '*')) {
            argc = 3;
          } else {
            if ((c != '~')) {
              stringPresent = false;
              index[0] = (index[0] - 1);
            }
            argc = read_integer(index, raw, length, alphaNums);
          }
        }
      }
      iarglist = new int[(argc - 1)];
      j = 0;
      while ((j < argc)) {
        iarg = read_integer(index, raw, length, alphaNums);
        if ((j == 0)) {
          ops[i] = iarg;
        } else {
          iarglist[(j - 1)] = iarg;
        }
        j += 1;
      }
      iargs[i] = iarglist;
      if (stringPresent) {
        stringarg = read_string(index, raw, length, alphaNums);
      } else {
        stringarg = null;
      }
      sargs[i] = stringarg;
      i += 1;
    }
    boolean[] hasBreakpoint = new boolean[opCount];
    BreakpointInfo[] breakpointInfo = new BreakpointInfo[opCount];
    i = 0;
    while ((i < opCount)) {
      hasBreakpoint[i] = false;
      breakpointInfo[i] = null;
      i += 1;
    }
    return new Code(ops, iargs, sargs, new HashMap[opCount], new HashMap[opCount], new VmDebugData(hasBreakpoint, breakpointInfo, new HashMap<Integer, Integer>(), 1, 0));
  }

  public static int initializeClass(int pc, VmContext vm, int[] args, String className) {
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
    if ((staticConstructorFunctionId == -1)) {
      staticInitializationState = 2;
    }
    int staticFieldCount = args[5];
    int assemblyId = args[6];
    Value[] staticFields = new Value[staticFieldCount];
    i = 0;
    while ((i < staticFieldCount)) {
      staticFields[i] = vm.globals.valueNull;
      i += 1;
    }
    ClassInfo classInfo = new ClassInfo(classId, globalNameId, baseClassId, assemblyId, staticInitializationState, staticFields, staticConstructorFunctionId, constructorFunctionId, 0, null, null, null, null, null, vm.metadata.classMemberLocalizerBuilder.get(classId), null, className);
    ClassInfo[] classTable = getClassTable(vm, classId);
    classTable[classId] = classInfo;
    ArrayList<ClassInfo> classChain = new ArrayList<ClassInfo>();
    classChain.add(classInfo);
    int classIdWalker = baseClassId;
    while ((classIdWalker != -1)) {
      ClassInfo walkerClass = classTable[classIdWalker];
      classChain.add(walkerClass);
      classIdWalker = walkerClass.baseClassId;
    }
    ClassInfo baseClass = null;
    if ((baseClassId != -1)) {
      baseClass = classChain.get(1);
    }
    ArrayList<Integer> functionIds = new ArrayList<Integer>();
    ArrayList<Integer> fieldInitializationCommand = new ArrayList<Integer>();
    ArrayList<Value> fieldInitializationLiteral = new ArrayList<Value>();
    ArrayList<Integer> fieldAccessModifier = new ArrayList<Integer>();
    HashMap<Integer, Integer> globalNameIdToMemberId = new HashMap<Integer, Integer>();
    if ((baseClass != null)) {
      i = 0;
      while ((i < baseClass.memberCount)) {
        functionIds.add(baseClass.functionIds[i]);
        fieldInitializationCommand.add(baseClass.fieldInitializationCommand[i]);
        fieldInitializationLiteral.add(baseClass.fieldInitializationLiteral[i]);
        fieldAccessModifier.add(baseClass.fieldAccessModifiers[i]);
        i += 1;
      }
      int[] keys = PST_convertIntegerSetToArray(baseClass.globalIdToMemberId.keySet());
      i = 0;
      while ((i < keys.length)) {
        t = keys[i];
        globalNameIdToMemberId.put(t, baseClass.globalIdToMemberId.get(t));
        i += 1;
      }
      keys = PST_convertIntegerSetToArray(baseClass.localeScopedNameIdToMemberId.keySet());
      i = 0;
      while ((i < keys.length)) {
        t = keys[i];
        classInfo.localeScopedNameIdToMemberId.put(t, baseClass.localeScopedNameIdToMemberId.get(t));
        i += 1;
      }
    }
    int accessModifier = 0;
    i = 7;
    while ((i < args.length)) {
      memberId = args[(i + 1)];
      globalId = args[(i + 2)];
      accessModifier = args[(i + 5)];
      while ((memberId >= functionIds.size())) {
        functionIds.add(-1);
        fieldInitializationCommand.add(-1);
        fieldInitializationLiteral.add(null);
        fieldAccessModifier.add(0);
      }
      globalNameIdToMemberId.put(globalId, memberId);
      fieldAccessModifier.set(memberId, accessModifier);
      if ((args[i] == 0)) {
        fieldInitializationCommand.set(memberId, args[(i + 3)]);
        t = args[(i + 4)];
        if ((t == -1)) {
          fieldInitializationLiteral.set(memberId, vm.globals.valueNull);
        } else {
          fieldInitializationLiteral.set(memberId, vm.metadata.literalTable[t]);
        }
      } else {
        functionId = args[(i + 3)];
        functionIds.set(memberId, functionId);
      }
      i += 6;
    }
    classInfo.functionIds = PST_listToArrayInt(functionIds);
    classInfo.fieldInitializationCommand = PST_listToArrayInt(fieldInitializationCommand);
    classInfo.fieldInitializationLiteral = fieldInitializationLiteral.toArray(Value.EMPTY_ARRAY);
    classInfo.fieldAccessModifiers = PST_listToArrayInt(fieldAccessModifier);
    classInfo.memberCount = functionIds.size();
    classInfo.globalIdToMemberId = globalNameIdToMemberId;
    classInfo.typeInfo = new int[classInfo.memberCount][];
    if ((baseClass != null)) {
      i = 0;
      while ((i < baseClass.typeInfo.length)) {
        classInfo.typeInfo[i] = baseClass.typeInfo[i];
        i += 1;
      }
    }
    if ("Core.Exception".equals(className)) {
      MagicNumbers mn = vm.metadata.magicNumbers;
      mn.coreExceptionClassId = classId;
    }
    return 0;
  }

  public static int initializeClassFieldTypeInfo(VmContext vm, int[] opCodeRow) {
    ClassInfo classInfo = vm.metadata.classTable[opCodeRow[0]];
    int memberId = opCodeRow[1];
    int _len = opCodeRow.length;
    int[] typeInfo = new int[(_len - 2)];
    int i = 2;
    while ((i < _len)) {
      typeInfo[(i - 2)] = opCodeRow[i];
      i += 1;
    }
    classInfo.typeInfo[memberId] = typeInfo;
    return 0;
  }

  public static VmGlobals initializeConstantValues() {
    Value[] pos = new Value[2049];
    Value[] neg = new Value[257];
    int i = 0;
    while ((i < 2049)) {
      pos[i] = new Value(i);
      i += 1;
    }
    i = 1;
    while ((i < 257)) {
      neg[i] = new Value(-i);
      i += 1;
    }
    neg[0] = pos[0];
    VmGlobals globals = new VmGlobals(new Value(1, null), new Value(true), new Value(false), pos[0], pos[1], neg[1], new Value(4, 0.0), new Value(4, 1.0), new Value(5, ""), pos, neg, new HashMap<String, Value>(), new int[1], new int[1], new int[1], new int[1], new int[1], new int[2]);
    globals.commonStrings.put("", globals.stringEmpty);
    globals.booleanType[0] = 2;
    globals.intType[0] = 3;
    globals.floatType[0] = 4;
    globals.stringType[0] = 5;
    globals.classType[0] = 10;
    globals.anyInstanceType[0] = 8;
    globals.anyInstanceType[1] = 0;
    return globals;
  }

  public static int initializeFunction(VmContext vm, int[] args, int currentPc, String stringArg) {
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
    while ((i < numPcOffsetsForOptionalArgs)) {
      pcOffsetsForOptionalArgs[(i + 1)] = args[(9 + i)];
      i += 1;
    }
    FunctionInfo[] functionTable = getFunctionTable(vm, functionId);
    functionTable[functionId] = new FunctionInfo(functionId, nameId, currentPc, minArgCount, maxArgCount, functionType, classId, localsCount, pcOffsetsForOptionalArgs, stringArg, null);
    vm.metadata.mostRecentFunctionDef = functionTable[functionId];
    if ((nameId >= 0)) {
      String name = vm.metadata.identifiers[nameId];
      if ("_LIB_CORE_list_filter".equals(name)) {
        vm.metadata.primitiveMethodFunctionIdFallbackLookup[0] = functionId;
      } else {
        if ("_LIB_CORE_list_map".equals(name)) {
          vm.metadata.primitiveMethodFunctionIdFallbackLookup[1] = functionId;
        } else {
          if ("_LIB_CORE_list_sort_by_key".equals(name)) {
            vm.metadata.primitiveMethodFunctionIdFallbackLookup[2] = functionId;
          } else {
            if ("_LIB_CORE_invoke".equals(name)) {
              vm.metadata.primitiveMethodFunctionIdFallbackLookup[3] = functionId;
            } else {
              if ("_LIB_CORE_generateException".equals(name)) {
                MagicNumbers mn = vm.metadata.magicNumbers;
                mn.coreGenerateExceptionFunctionId = functionId;
              }
            }
          }
        }
      }
    }
    return 0;
  }

  public static HashMap<Integer, Integer> initializeIntSwitchStatement(VmContext vm, int pc, int[] args) {
    HashMap<Integer, Integer> output = new HashMap<Integer, Integer>();
    int i = 1;
    while ((i < args.length)) {
      output.put(args[i], args[(i + 1)]);
      i += 2;
    }
    vm.byteCode.integerSwitchesByPc[pc] = output;
    return output;
  }

  public static HashMap<String, Integer> initializeStringSwitchStatement(VmContext vm, int pc, int[] args) {
    HashMap<String, Integer> output = new HashMap<String, Integer>();
    int i = 1;
    while ((i < args.length)) {
      String s = ((String) vm.metadata.literalTable[args[i]].internalValue);
      output.put(s, args[(i + 1)]);
      i += 2;
    }
    vm.byteCode.stringSwitchesByPc[pc] = output;
    return output;
  }

  public static int initLocTable(VmContext vm, int[] row) {
    int classId = row[0];
    int memberCount = row[1];
    int nameId = 0;
    int totalLocales = vm.metadata.magicNumbers.totalLocaleCount;
    HashMap<Integer, Integer> lookup = new HashMap<Integer, Integer>();
    int i = 2;
    while ((i < row.length)) {
      int localeId = row[i];
      i += 1;
      int j = 0;
      while ((j < memberCount)) {
        nameId = row[(i + j)];
        if ((nameId != -1)) {
          lookup.put(((nameId * totalLocales) + localeId), j);
        }
        j += 1;
      }
      i += memberCount;
    }
    vm.metadata.classMemberLocalizerBuilder.put(classId, lookup);
    return 0;
  }

  public static InterpreterResult interpret(VmContext vm, int executionContextId) {
    InterpreterResult output = interpretImpl(vm, executionContextId);
    while (((output.status == 5) && (output.reinvokeDelay == 0))) {
      output = interpretImpl(vm, executionContextId);
    }
    return output;
  }

  public static InterpreterResult interpreterFinished(VmContext vm, ExecutionContext ec) {
    if ((ec != null)) {
      int id = ec.id;
      if (vm.executionContexts.containsKey(id)) {
        vm.executionContexts.remove(id);
      }
    }
    return new InterpreterResult(1, null, 0.0, 0, false, "");
  }

  public static ExecutionContext interpreterGetExecutionContext(VmContext vm, int executionContextId) {
    HashMap<Integer, ExecutionContext> executionContexts = vm.executionContexts;
    if (!executionContexts.containsKey(executionContextId)) {
      return null;
    }
    return executionContexts.get(executionContextId);
  }

  public static InterpreterResult interpretImpl(VmContext vm, int executionContextId) {
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
    HashMap<Integer, ExecutionContext> executionContexts = vm.executionContexts;
    ExecutionContext ec = interpreterGetExecutionContext(vm, executionContextId);
    if ((ec == null)) {
      return interpreterFinished(vm, null);
    }
    ec.executionCounter += 1;
    StackFrame stack = ec.stackTop;
    int[] ops = vm.byteCode.ops;
    int[][] args = vm.byteCode.args;
    String[] stringArgs = vm.byteCode.stringArgs;
    ClassInfo[] classTable = vm.metadata.classTable;
    FunctionInfo[] functionTable = vm.metadata.functionTable;
    Value[] literalTable = vm.metadata.literalTable;
    String[] identifiers = vm.metadata.identifiers;
    Value[] valueStack = ec.valueStack;
    int valueStackSize = ec.currentValueStackSize;
    int valueStackCapacity = valueStack.length;
    boolean hasInterrupt = false;
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
    String[] stringList = null;
    boolean returnValueUsed = false;
    Value output = null;
    FunctionInfo functionInfo = null;
    int keyType = 0;
    int intKey = 0;
    String stringKey = null;
    boolean first = false;
    boolean primitiveMethodToCoreLibraryFallback = false;
    boolean bool1 = false;
    boolean bool2 = false;
    boolean staticConstructorNotInvoked = true;
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
    String string1 = null;
    String string2 = null;
    ObjectInstance objInstance1 = null;
    ObjectInstance objInstance2 = null;
    ListImpl list1 = null;
    ListImpl list2 = null;
    ArrayList<Value> valueList1 = null;
    ArrayList<Value> valueList2 = null;
    DictImpl dictImpl = null;
    DictImpl dictImpl2 = null;
    ArrayList<String> stringList1 = null;
    ArrayList<Integer> intList1 = null;
    Value[] valueArray1 = null;
    int[] intArray1 = null;
    int[] intArray2 = null;
    Object[] objArray1 = null;
    FunctionPointer functionPointer1 = null;
    HashMap<Integer, Integer> intIntDict1 = null;
    HashMap<String, Integer> stringIntDict1 = null;
    StackFrame stackFrame2 = null;
    Value leftValue = null;
    Value rightValue = null;
    org.crayonlang.interpreter.structs.ClassValue classValue = null;
    Value arg1 = null;
    Value arg2 = null;
    Value arg3 = null;
    ArrayList<Token> tokenList = null;
    int[] globalNameIdToPrimitiveMethodName = vm.metadata.globalNameIdToPrimitiveMethodName;
    MagicNumbers magicNumbers = vm.metadata.magicNumbers;
    HashMap<Integer, Integer>[] integerSwitchesByPc = vm.byteCode.integerSwitchesByPc;
    HashMap<String, Integer>[] stringSwitchesByPc = vm.byteCode.stringSwitchesByPc;
    HashMap<Integer, Integer> integerSwitch = null;
    HashMap<String, Integer> stringSwitch = null;
    int[][] esfData = vm.metadata.esfData;
    HashMap<Integer, ClosureValuePointer> closure = null;
    HashMap<Integer, ClosureValuePointer> parentClosure = null;
    int[] intBuffer = new int[16];
    Value[] localsStack = ec.localsStack;
    int[] localsStackSet = ec.localsStackSet;
    int localsStackSetToken = stack.localsStackSetToken;
    int localsStackCapacity = localsStack.length;
    int localsStackOffset = stack.localsStackOffset;
    Value[] funcArgs = vm.funcArgs;
    int pc = stack.pc;
    java.lang.reflect.Method nativeFp = null;
    VmDebugData debugData = vm.byteCode.debugData;
    boolean[] isBreakPointPresent = debugData.hasBreakpoint;
    BreakpointInfo breakpointInfo = null;
    boolean debugBreakPointTemporaryDisable = false;
    while (true) {
      row = args[pc];
      switch (ops[pc]) {
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
          while ((j < _len)) {
            j += 1;
          }
          break;
        case 3:
          // ASSIGN_CLOSURE;
          value = valueStack[--valueStackSize];
          i = row[0];
          if ((stack.closureVariables == null)) {
            closure = new HashMap<Integer, ClosureValuePointer>();
            stack.closureVariables = closure;
            closure.put(i, new ClosureValuePointer(value));
          } else {
            closure = stack.closureVariables;
            if (closure.containsKey(i)) {
              closure.get(i).value = value;
            } else {
              closure.put(i, new ClosureValuePointer(value));
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
          if ((type == 6)) {
            if ((value2.type == 3)) {
              i = value2.intValue;
              list1 = ((ListImpl) root.internalValue);
              if ((list1.type != null)) {
                value3 = canAssignTypeToGeneric(vm, value, list1.type, 0);
                if ((value3 == null)) {
                  hasInterrupt = EX_InvalidArgument(ec, "Cannot convert a " + typeToStringFromValue(vm, value) + " into a " + typeToString(vm, list1.type, 0));
                }
                value = value3;
              }
              if (!hasInterrupt) {
                if ((i >= list1.size)) {
                  hasInterrupt = EX_IndexOutOfRange(ec, "Index is out of range.");
                } else {
                  if ((i < 0)) {
                    i += list1.size;
                    if ((i < 0)) {
                      hasInterrupt = EX_IndexOutOfRange(ec, "Index is out of range.");
                    }
                  }
                }
                if (!hasInterrupt) {
                  list1.array[i] = value;
                }
              }
            } else {
              hasInterrupt = EX_InvalidArgument(ec, "List index must be an integer.");
            }
          } else {
            if ((type == 7)) {
              dictImpl = ((DictImpl) root.internalValue);
              if ((dictImpl.valueType != null)) {
                value3 = canAssignTypeToGeneric(vm, value, dictImpl.valueType, 0);
                if ((value3 == null)) {
                  hasInterrupt = EX_InvalidArgument(ec, "Cannot assign a value to this dictionary of this type.");
                } else {
                  value = value3;
                }
              }
              keyType = value2.type;
              if ((keyType == 3)) {
                intKey = value2.intValue;
              } else {
                if ((keyType == 5)) {
                  stringKey = ((String) value2.internalValue);
                } else {
                  if ((keyType == 8)) {
                    objInstance1 = ((ObjectInstance) value2.internalValue);
                    intKey = objInstance1.objectId;
                  } else {
                    hasInterrupt = EX_InvalidArgument(ec, "Invalid key for a dictionary.");
                  }
                }
              }
              if (!hasInterrupt) {
                bool2 = (dictImpl.size == 0);
                if ((dictImpl.keyType != keyType)) {
                  if ((dictImpl.valueType != null)) {
                    string1 = "Cannot assign a key of type " + typeToStringFromValue(vm, value2) + " to a dictionary that requires key types of " + dictKeyInfoToString(vm, dictImpl) + ".";
                    hasInterrupt = EX_InvalidKey(ec, string1);
                  } else {
                    if (!bool2) {
                      hasInterrupt = EX_InvalidKey(ec, "Cannot have multiple keys in one dictionary with different types.");
                    }
                  }
                } else {
                  if (((keyType == 8) && (dictImpl.keyClassId > 0) && (objInstance1.classId != dictImpl.keyClassId))) {
                    if (isClassASubclassOf(vm, objInstance1.classId, dictImpl.keyClassId)) {
                      hasInterrupt = EX_InvalidKey(ec, "Cannot use this type of object as a key for this dictionary.");
                    }
                  }
                }
              }
              if (!hasInterrupt) {
                if ((keyType == 5)) {
                  Integer dictLookup3 = dictImpl.stringToIndex.get(stringKey);
                  int1 = dictLookup3 == null ? (dictImpl.stringToIndex.containsKey(stringKey) ? null : (-1)) : dictLookup3;
                  if ((int1 == -1)) {
                    dictImpl.stringToIndex.put(stringKey, dictImpl.size);
                    dictImpl.size += 1;
                    dictImpl.keys.add(value2);
                    dictImpl.values.add(value);
                    if (bool2) {
                      dictImpl.keyType = keyType;
                    }
                  } else {
                    dictImpl.values.set(int1, value);
                  }
                } else {
                  Integer dictLookup4 = dictImpl.intToIndex.get(intKey);
                  int1 = dictLookup4 == null ? (-1) : dictLookup4;
                  if ((int1 == -1)) {
                    dictImpl.intToIndex.put(intKey, dictImpl.size);
                    dictImpl.size += 1;
                    dictImpl.keys.add(value2);
                    dictImpl.values.add(value);
                    if (bool2) {
                      dictImpl.keyType = keyType;
                    }
                  } else {
                    dictImpl.values.set(int1, value);
                  }
                }
              }
            } else {
              hasInterrupt = EX_UnsupportedOperation(ec, getTypeFromId(type) + " type does not support assigning to an index.");
            }
          }
          if (bool1) {
            valueStack[valueStackSize] = value;
            valueStackSize += 1;
          }
          break;
        case 6:
          // ASSIGN_STATIC_FIELD;
          classInfo = classTable[row[0]];
          staticConstructorNotInvoked = true;
          if ((classInfo.staticInitializationState < 2)) {
            stack.pc = pc;
            stackFrame2 = maybeInvokeStaticConstructor(vm, ec, stack, classInfo, valueStackSize, PST_intBuffer16);
            if ((PST_intBuffer16[0] == 1)) {
              return generateException(vm, stack, pc, valueStackSize, ec, 0, "Static initialization loop detected. The class this field is a member of is not done being initialized.");
            }
            if ((stackFrame2 != null)) {
              staticConstructorNotInvoked = false;
              stack = stackFrame2;
              pc = stack.pc;
              localsStackSetToken = stack.localsStackSetToken;
              localsStackOffset = stack.localsStackOffset;
            }
          }
          if (staticConstructorNotInvoked) {
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
          if ((value2.type == 8)) {
            objInstance1 = ((ObjectInstance) value2.internalValue);
            classId = objInstance1.classId;
            classInfo = classTable[classId];
            intIntDict1 = classInfo.localeScopedNameIdToMemberId;
            if ((row[5] == classId)) {
              int1 = row[6];
            } else {
              Integer dictLookup5 = intIntDict1.get(nameId);
              int1 = dictLookup5 == null ? (-1) : dictLookup5;
              if ((int1 != -1)) {
                int3 = classInfo.fieldAccessModifiers[int1];
                if ((int3 > 1)) {
                  if ((int3 == 2)) {
                    if ((classId != row[3])) {
                      int1 = -2;
                    }
                  } else {
                    if (((int3 == 3) || (int3 == 5))) {
                      if ((classInfo.assemblyId != row[4])) {
                        int1 = -3;
                      }
                    }
                    if (((int3 == 4) || (int3 == 5))) {
                      i = row[3];
                      if ((classId == i)) {
                      } else {
                        classInfo = classTable[classInfo.id];
                        while (((classInfo.baseClassId != -1) && (int1 < classTable[classInfo.baseClassId].fieldAccessModifiers.length))) {
                          classInfo = classTable[classInfo.baseClassId];
                        }
                        j = classInfo.id;
                        if ((j != i)) {
                          bool1 = false;
                          while (((i != -1) && (classTable[i].baseClassId != -1))) {
                            i = classTable[i].baseClassId;
                            if ((i == j)) {
                              bool1 = true;
                              i = -1;
                            }
                          }
                          if (!bool1) {
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
            if ((int1 > -1)) {
              int2 = classInfo.functionIds[int1];
              if ((int2 == -1)) {
                intArray1 = classInfo.typeInfo[int1];
                if ((intArray1 == null)) {
                  objInstance1.members[int1] = value;
                } else {
                  value2 = canAssignTypeToGeneric(vm, value, intArray1, 0);
                  if ((value2 != null)) {
                    objInstance1.members[int1] = value2;
                  } else {
                    hasInterrupt = EX_InvalidArgument(ec, "Cannot assign this type to this field.");
                  }
                }
              } else {
                hasInterrupt = EX_InvalidArgument(ec, "Cannot override a method with assignment.");
              }
            } else {
              if ((int1 < -1)) {
                string1 = identifiers[row[0]];
                if ((int1 == -2)) {
                  string2 = "private";
                } else {
                  if ((int1 == -3)) {
                    string2 = "internal";
                  } else {
                    string2 = "protected";
                  }
                }
                hasInterrupt = EX_UnknownField(ec, "The field '" + string1 + "' is marked as " + string2 + " and cannot be accessed from here.");
              } else {
                hasInterrupt = EX_InvalidAssignment(ec, "'" + classInfo.fullyQualifiedName + "' instances do not have a field called '" + metadata.identifiers[row[0]] + "'");
              }
            }
          } else {
            hasInterrupt = EX_InvalidAssignment(ec, "Cannot assign to a field on this type.");
          }
          if ((row[1] == 1)) {
            valueStack[valueStackSize++] = value;
          }
          break;
        case 8:
          // ASSIGN_THIS_FIELD;
          objInstance2 = ((ObjectInstance) stack.objectContext.internalValue);
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
          switch (((((leftValue.type * 15) + row[0]) * 11) + rightValue.type)) {
            case 553:
              // int ** int;
              if ((rightValue.intValue == 0)) {
                value = VALUE_INT_ONE;
              } else {
                if ((rightValue.intValue > 0)) {
                  value = buildInteger(globals, ((int) Math.pow(leftValue.intValue, rightValue.intValue)));
                } else {
                  value = buildFloat(globals, Math.pow(leftValue.intValue, rightValue.intValue));
                }
              }
              break;
            case 554:
              // int ** float;
              value = buildFloat(globals, (0.0 + Math.pow(leftValue.intValue, ((double) rightValue.internalValue))));
              break;
            case 718:
              // float ** int;
              value = buildFloat(globals, (0.0 + Math.pow(((double) leftValue.internalValue), rightValue.intValue)));
              break;
            case 719:
              // float ** float;
              value = buildFloat(globals, (0.0 + Math.pow(((double) leftValue.internalValue), ((double) rightValue.internalValue))));
              break;
            case 708:
              // float % float;
              float1 = ((double) rightValue.internalValue);
              if ((float1 == 0)) {
                hasInterrupt = EX_DivisionByZero(ec, "Modulo by 0.");
              } else {
                float3 = (((double) leftValue.internalValue) % float1);
                if ((float3 < 0)) {
                  float3 += float1;
                }
                value = buildFloat(globals, float3);
              }
              break;
            case 707:
              // float % int;
              int1 = rightValue.intValue;
              if ((int1 == 0)) {
                hasInterrupt = EX_DivisionByZero(ec, "Modulo by 0.");
              } else {
                float1 = (((double) leftValue.internalValue) % int1);
                if ((float1 < 0)) {
                  float1 += int1;
                }
                value = buildFloat(globals, float1);
              }
              break;
            case 543:
              // int % float;
              float3 = ((double) rightValue.internalValue);
              if ((float3 == 0)) {
                hasInterrupt = EX_DivisionByZero(ec, "Modulo by 0.");
              } else {
                float1 = (leftValue.intValue % float3);
                if ((float1 < 0)) {
                  float1 += float3;
                }
                value = buildFloat(globals, float1);
              }
              break;
            case 542:
              // int % int;
              int2 = rightValue.intValue;
              if ((int2 == 0)) {
                hasInterrupt = EX_DivisionByZero(ec, "Modulo by 0.");
              } else {
                int1 = (leftValue.intValue % int2);
                if ((int1 < 0)) {
                  int1 += int2;
                }
                value = buildInteger(globals, int1);
              }
              break;
            case 996:
              // list + list;
              value = new Value(6, valueConcatLists(((ListImpl) leftValue.internalValue), ((ListImpl) rightValue.internalValue)));
              break;
            case 498:
              // int + int;
              int1 = (leftValue.intValue + rightValue.intValue);
              if ((int1 < 0)) {
                if ((int1 > -257)) {
                  value = INTEGER_NEGATIVE_CACHE[-int1];
                } else {
                  value = new Value(int1);
                }
              } else {
                if ((int1 < 2049)) {
                  value = INTEGER_POSITIVE_CACHE[int1];
                } else {
                  value = new Value(int1);
                }
              }
              break;
            case 509:
              // int - int;
              int1 = (leftValue.intValue - rightValue.intValue);
              if ((int1 < 0)) {
                if ((int1 > -257)) {
                  value = INTEGER_NEGATIVE_CACHE[-int1];
                } else {
                  value = new Value(int1);
                }
              } else {
                if ((int1 < 2049)) {
                  value = INTEGER_POSITIVE_CACHE[int1];
                } else {
                  value = new Value(int1);
                }
              }
              break;
            case 520:
              // int * int;
              int1 = (leftValue.intValue * rightValue.intValue);
              if ((int1 < 0)) {
                if ((int1 > -257)) {
                  value = INTEGER_NEGATIVE_CACHE[-int1];
                } else {
                  value = new Value(int1);
                }
              } else {
                if ((int1 < 2049)) {
                  value = INTEGER_POSITIVE_CACHE[int1];
                } else {
                  value = new Value(int1);
                }
              }
              break;
            case 531:
              // int / int;
              int1 = leftValue.intValue;
              int2 = rightValue.intValue;
              if ((int2 == 0)) {
                hasInterrupt = EX_DivisionByZero(ec, "Division by 0.");
              } else {
                if ((int1 == 0)) {
                  value = VALUE_INT_ZERO;
                } else {
                  if (((int1 % int2) == 0)) {
                    int3 = int1 / int2;
                  } else {
                    if ((((int1 < 0)) != ((int2 < 0)))) {
                      float1 = (1 + (-1.0 * int1) / int2);
                      float1 -= (float1 % 1.0);
                      int3 = ((int) (-float1));
                    } else {
                      int3 = int1 / int2;
                    }
                  }
                  if ((int3 < 0)) {
                    if ((int3 > -257)) {
                      value = INTEGER_NEGATIVE_CACHE[-int3];
                    } else {
                      value = new Value(int3);
                    }
                  } else {
                    if ((int3 < 2049)) {
                      value = INTEGER_POSITIVE_CACHE[int3];
                    } else {
                      value = new Value(int3);
                    }
                  }
                }
              }
              break;
            case 663:
              // float + int;
              value = buildFloat(globals, (((double) leftValue.internalValue) + rightValue.intValue));
              break;
            case 499:
              // int + float;
              value = buildFloat(globals, (leftValue.intValue + ((double) rightValue.internalValue)));
              break;
            case 664:
              // float + float;
              float1 = (((double) leftValue.internalValue) + ((double) rightValue.internalValue));
              if ((float1 == 0)) {
                value = VALUE_FLOAT_ZERO;
              } else {
                if ((float1 == 1)) {
                  value = VALUE_FLOAT_ONE;
                } else {
                  value = new Value(4, float1);
                }
              }
              break;
            case 510:
              // int - float;
              value = buildFloat(globals, (leftValue.intValue - ((double) rightValue.internalValue)));
              break;
            case 674:
              // float - int;
              value = buildFloat(globals, (((double) leftValue.internalValue) - rightValue.intValue));
              break;
            case 675:
              // float - float;
              float1 = (((double) leftValue.internalValue) - ((double) rightValue.internalValue));
              if ((float1 == 0)) {
                value = VALUE_FLOAT_ZERO;
              } else {
                if ((float1 == 1)) {
                  value = VALUE_FLOAT_ONE;
                } else {
                  value = new Value(4, float1);
                }
              }
              break;
            case 685:
              // float * int;
              value = buildFloat(globals, (((double) leftValue.internalValue) * rightValue.intValue));
              break;
            case 521:
              // int * float;
              value = buildFloat(globals, (leftValue.intValue * ((double) rightValue.internalValue)));
              break;
            case 686:
              // float * float;
              value = buildFloat(globals, (((double) leftValue.internalValue) * ((double) rightValue.internalValue)));
              break;
            case 532:
              // int / float;
              float1 = ((double) rightValue.internalValue);
              if ((float1 == 0)) {
                hasInterrupt = EX_DivisionByZero(ec, "Division by 0.");
              } else {
                value = buildFloat(globals, leftValue.intValue / float1);
              }
              break;
            case 696:
              // float / int;
              int1 = rightValue.intValue;
              if ((int1 == 0)) {
                hasInterrupt = EX_DivisionByZero(ec, "Division by 0.");
              } else {
                value = buildFloat(globals, ((double) leftValue.internalValue) / int1);
              }
              break;
            case 697:
              // float / float;
              float1 = ((double) rightValue.internalValue);
              if ((float1 == 0)) {
                hasInterrupt = EX_DivisionByZero(ec, "Division by 0.");
              } else {
                value = buildFloat(globals, ((double) leftValue.internalValue) / float1);
              }
              break;
            case 564:
              // int & int;
              value = buildInteger(globals, (leftValue.intValue & rightValue.intValue));
              break;
            case 575:
              // int | int;
              value = buildInteger(globals, (leftValue.intValue | rightValue.intValue));
              break;
            case 586:
              // int ^ int;
              value = buildInteger(globals, (leftValue.intValue ^ rightValue.intValue));
              break;
            case 597:
              // int << int;
              int1 = rightValue.intValue;
              if ((int1 < 0)) {
                hasInterrupt = EX_InvalidArgument(ec, "Cannot bit shift by a negative number.");
              } else {
                value = buildInteger(globals, (leftValue.intValue << int1));
              }
              break;
            case 608:
              // int >> int;
              int1 = rightValue.intValue;
              if ((int1 < 0)) {
                hasInterrupt = EX_InvalidArgument(ec, "Cannot bit shift by a negative number.");
              } else {
                value = buildInteger(globals, (leftValue.intValue >> int1));
              }
              break;
            case 619:
              // int < int;
              if ((leftValue.intValue < rightValue.intValue)) {
                value = VALUE_TRUE;
              } else {
                value = VALUE_FALSE;
              }
              break;
            case 630:
              // int <= int;
              if ((leftValue.intValue <= rightValue.intValue)) {
                value = VALUE_TRUE;
              } else {
                value = VALUE_FALSE;
              }
              break;
            case 784:
              // float < int;
              if ((((double) leftValue.internalValue) < rightValue.intValue)) {
                value = VALUE_TRUE;
              } else {
                value = VALUE_FALSE;
              }
              break;
            case 795:
              // float <= int;
              if ((((double) leftValue.internalValue) <= rightValue.intValue)) {
                value = VALUE_TRUE;
              } else {
                value = VALUE_FALSE;
              }
              break;
            case 620:
              // int < float;
              if ((leftValue.intValue < ((double) rightValue.internalValue))) {
                value = VALUE_TRUE;
              } else {
                value = VALUE_FALSE;
              }
              break;
            case 631:
              // int <= float;
              if ((leftValue.intValue <= ((double) rightValue.internalValue))) {
                value = VALUE_TRUE;
              } else {
                value = VALUE_FALSE;
              }
              break;
            case 785:
              // float < float;
              if ((((double) leftValue.internalValue) < ((double) rightValue.internalValue))) {
                value = VALUE_TRUE;
              } else {
                value = VALUE_FALSE;
              }
              break;
            case 796:
              // float <= float;
              if ((((double) leftValue.internalValue) <= ((double) rightValue.internalValue))) {
                value = VALUE_TRUE;
              } else {
                value = VALUE_FALSE;
              }
              break;
            case 652:
              // int >= int;
              if ((leftValue.intValue >= rightValue.intValue)) {
                value = VALUE_TRUE;
              } else {
                value = VALUE_FALSE;
              }
              break;
            case 641:
              // int > int;
              if ((leftValue.intValue > rightValue.intValue)) {
                value = VALUE_TRUE;
              } else {
                value = VALUE_FALSE;
              }
              break;
            case 817:
              // float >= int;
              if ((((double) leftValue.internalValue) >= rightValue.intValue)) {
                value = VALUE_TRUE;
              } else {
                value = VALUE_FALSE;
              }
              break;
            case 806:
              // float > int;
              if ((((double) leftValue.internalValue) > rightValue.intValue)) {
                value = VALUE_TRUE;
              } else {
                value = VALUE_FALSE;
              }
              break;
            case 653:
              // int >= float;
              if ((leftValue.intValue >= ((double) rightValue.internalValue))) {
                value = VALUE_TRUE;
              } else {
                value = VALUE_FALSE;
              }
              break;
            case 642:
              // int > float;
              if ((leftValue.intValue > ((double) rightValue.internalValue))) {
                value = VALUE_TRUE;
              } else {
                value = VALUE_FALSE;
              }
              break;
            case 818:
              // float >= float;
              if ((((double) leftValue.internalValue) >= ((double) rightValue.internalValue))) {
                value = VALUE_TRUE;
              } else {
                value = VALUE_FALSE;
              }
              break;
            case 807:
              // float > float;
              if ((((double) leftValue.internalValue) > ((double) rightValue.internalValue))) {
                value = VALUE_TRUE;
              } else {
                value = VALUE_FALSE;
              }
              break;
            case 830:
              // string + string;
              value = new Value(5, ((String) leftValue.internalValue) + ((String) rightValue.internalValue));
              break;
            case 850:
              // string * int;
              value = multiplyString(globals, leftValue, ((String) leftValue.internalValue), rightValue.intValue);
              break;
            case 522:
              // int * string;
              value = multiplyString(globals, rightValue, ((String) rightValue.internalValue), leftValue.intValue);
              break;
            case 1015:
              // list * int;
              int1 = rightValue.intValue;
              if ((int1 < 0)) {
                hasInterrupt = EX_UnsupportedOperation(ec, "Cannot multiply list by negative number.");
              } else {
                value = new Value(6, valueMultiplyList(((ListImpl) leftValue.internalValue), int1));
              }
              break;
            case 523:
              // int * list;
              int1 = leftValue.intValue;
              if ((int1 < 0)) {
                hasInterrupt = EX_UnsupportedOperation(ec, "Cannot multiply list by negative number.");
              } else {
                value = new Value(6, valueMultiplyList(((ListImpl) rightValue.internalValue), int1));
              }
              break;
            default:
              if (((row[0] == 0) && (((leftValue.type == 5) || (rightValue.type == 5))))) {
                value = new Value(5, valueToString(vm, leftValue) + valueToString(vm, rightValue));
              } else {
                // unrecognized op;
                hasInterrupt = EX_UnsupportedOperation(ec, "The '" + getBinaryOpFromId(row[0]) + "' operator is not supported for these types: " + getTypeFromId(leftValue.type) + " and " + getTypeFromId(rightValue.type));
              }
              break;
          }
          valueStack[(valueStackSize - 1)] = value;
          break;
        case 10:
          // BOOLEAN_NOT;
          value = valueStack[(valueStackSize - 1)];
          if ((value.type != 2)) {
            hasInterrupt = EX_InvalidArgument(ec, "Boolean expected.");
          } else {
            if ((value.intValue == 1)) {
              valueStack[(valueStackSize - 1)] = VALUE_FALSE;
            } else {
              valueStack[(valueStackSize - 1)] = VALUE_TRUE;
            }
          }
          break;
        case 11:
          // BREAK;
          if ((row[0] == 1)) {
            pc += row[1];
          } else {
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
          if (((type == 2) || (type == 6))) {
            // constructor or static method;
            classInfo = metadata.classTable[classId];
            staticConstructorNotInvoked = true;
            if ((classInfo.staticInitializationState < 2)) {
              stack.pc = pc;
              stackFrame2 = maybeInvokeStaticConstructor(vm, ec, stack, classInfo, valueStackSize, PST_intBuffer16);
              if ((PST_intBuffer16[0] == 1)) {
                return generateException(vm, stack, pc, valueStackSize, ec, 0, "Static initialization loop detected. The class this field is a member of is not done being initialized.");
              }
              if ((stackFrame2 != null)) {
                staticConstructorNotInvoked = false;
                stack = stackFrame2;
                pc = stack.pc;
                localsStackSetToken = stack.localsStackSetToken;
                localsStackOffset = stack.localsStackOffset;
              }
            }
          } else {
            staticConstructorNotInvoked = true;
          }
          if (staticConstructorNotInvoked) {
            bool1 = true;
            // construct args array;
            if ((argCount == -1)) {
              valueStackSize -= 1;
              value = valueStack[valueStackSize];
              if ((value.type == 1)) {
                argCount = 0;
              } else {
                if ((value.type == 6)) {
                  list1 = ((ListImpl) value.internalValue);
                  argCount = list1.size;
                  i = (argCount - 1);
                  while ((i >= 0)) {
                    funcArgs[i] = list1.array[i];
                    i -= 1;
                  }
                } else {
                  hasInterrupt = EX_InvalidArgument(ec, "Function pointers' .invoke method requires a list argument.");
                }
              }
            } else {
              i = (argCount - 1);
              while ((i >= 0)) {
                valueStackSize -= 1;
                funcArgs[i] = valueStack[valueStackSize];
                i -= 1;
              }
            }
            if (!hasInterrupt) {
              if ((type == 3)) {
                value = stack.objectContext;
                objInstance1 = ((ObjectInstance) value.internalValue);
                if ((objInstance1.classId != classId)) {
                  int2 = row[5];
                  if ((int2 != -1)) {
                    classInfo = classTable[objInstance1.classId];
                    functionId = classInfo.functionIds[int2];
                  }
                }
              } else {
                if ((type == 5)) {
                  // field invocation;
                  valueStackSize -= 1;
                  value = valueStack[valueStackSize];
                  localeId = row[5];
                  switch (value.type) {
                    case 1:
                      hasInterrupt = EX_NullReference(ec, "Invoked method on null.");
                      break;
                    case 8:
                      // field invoked on an object instance.;
                      objInstance1 = ((ObjectInstance) value.internalValue);
                      int1 = objInstance1.classId;
                      classInfo = classTable[int1];
                      intIntDict1 = classInfo.localeScopedNameIdToMemberId;
                      int1 = ((row[4] * magicNumbers.totalLocaleCount) + row[5]);
                      Integer dictLookup6 = intIntDict1.get(int1);
                      i = dictLookup6 == null ? (-1) : dictLookup6;
                      if ((i != -1)) {
                        int1 = intIntDict1.get(int1);
                        functionId = classInfo.functionIds[int1];
                        if ((functionId > 0)) {
                          type = 3;
                        } else {
                          value = objInstance1.members[int1];
                          type = 4;
                          valueStack[valueStackSize] = value;
                          valueStackSize += 1;
                        }
                      } else {
                        hasInterrupt = EX_UnknownField(ec, "Unknown field.");
                      }
                      break;
                    case 10:
                      // field invocation on a class object instance.;
                      functionId = resolvePrimitiveMethodName2(globalNameIdToPrimitiveMethodName, value.type, classId);
                      if ((functionId < 0)) {
                        hasInterrupt = EX_InvalidInvocation(ec, "Class definitions do not have that method.");
                      } else {
                        functionId = resolvePrimitiveMethodName2(globalNameIdToPrimitiveMethodName, value.type, classId);
                        if ((functionId < 0)) {
                          hasInterrupt = EX_InvalidInvocation(ec, getTypeFromId(value.type) + " does not have that method.");
                        } else {
                          if ((globalNameIdToPrimitiveMethodName[classId] == 8)) {
                            type = 6;
                            classValue = ((org.crayonlang.interpreter.structs.ClassValue) value.internalValue);
                            if (classValue.isInterface) {
                              hasInterrupt = EX_UnsupportedOperation(ec, "Cannot create an instance of an interface.");
                            } else {
                              classId = classValue.classId;
                              if (!returnValueUsed) {
                                hasInterrupt = EX_UnsupportedOperation(ec, "Cannot create an instance and not use the output.");
                              } else {
                                classInfo = metadata.classTable[classId];
                                functionId = classInfo.constructorFunctionId;
                              }
                            }
                          } else {
                            type = 9;
                          }
                        }
                      }
                      break;
                    default:
                      // primitive method suspected.;
                      functionId = resolvePrimitiveMethodName2(globalNameIdToPrimitiveMethodName, value.type, classId);
                      if ((functionId < 0)) {
                        hasInterrupt = EX_InvalidInvocation(ec, getTypeFromId(value.type) + " does not have that method.");
                      } else {
                        type = 9;
                      }
                      break;
                  }
                }
              }
            }
            if (((type == 4) && !hasInterrupt)) {
              // pointer provided;
              valueStackSize -= 1;
              value = valueStack[valueStackSize];
              if ((value.type == 9)) {
                functionPointer1 = ((FunctionPointer) value.internalValue);
                switch (functionPointer1.type) {
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
              } else {
                hasInterrupt = EX_InvalidInvocation(ec, "This type cannot be invoked like a function.");
              }
            }
            if (((type == 9) && !hasInterrupt)) {
              // primitive method invocation;
              output = VALUE_NULL;
              primitiveMethodToCoreLibraryFallback = false;
              switch (value.type) {
                case 5:
                  // ...on a string;
                  string1 = ((String) value.internalValue);
                  switch (functionId) {
                    case 7:
                      if ((argCount != 1)) {
                        hasInterrupt = EX_InvalidArgument(ec, primitiveMethodWrongArgCountError("string contains method", 1, argCount));
                      } else {
                        value2 = funcArgs[0];
                        if ((value2.type != 5)) {
                          hasInterrupt = EX_InvalidArgument(ec, "string contains method requires another string as input.");
                        } else {
                          if (string1.contains(((String) value2.internalValue))) {
                            output = VALUE_TRUE;
                          } else {
                            output = VALUE_FALSE;
                          }
                        }
                      }
                      break;
                    case 9:
                      if ((argCount != 1)) {
                        hasInterrupt = EX_InvalidArgument(ec, primitiveMethodWrongArgCountError("string endsWith method", 1, argCount));
                      } else {
                        value2 = funcArgs[0];
                        if ((value2.type != 5)) {
                          hasInterrupt = EX_InvalidArgument(ec, "string endsWith method requires another string as input.");
                        } else {
                          if (string1.endsWith(((String) value2.internalValue))) {
                            output = VALUE_TRUE;
                          } else {
                            output = VALUE_FALSE;
                          }
                        }
                      }
                      break;
                    case 13:
                      if ((argCount != 1)) {
                        hasInterrupt = EX_InvalidArgument(ec, primitiveMethodWrongArgCountError("string indexOf method", 1, argCount));
                      } else {
                        value2 = funcArgs[0];
                        if ((value2.type != 5)) {
                          hasInterrupt = EX_InvalidArgument(ec, "string indexOf method requires another string as input.");
                        } else {
                          output = buildInteger(globals, string1.indexOf(((String) value2.internalValue)));
                        }
                      }
                      break;
                    case 19:
                      if ((argCount > 0)) {
                        hasInterrupt = EX_InvalidArgument(ec, primitiveMethodWrongArgCountError("string lower method", 0, argCount));
                      } else {
                        output = buildString(globals, string1.toLowerCase());
                      }
                      break;
                    case 20:
                      if ((argCount > 0)) {
                        hasInterrupt = EX_InvalidArgument(ec, primitiveMethodWrongArgCountError("string ltrim method", 0, argCount));
                      } else {
                        output = buildString(globals, PST_trimSide(string1, true));
                      }
                      break;
                    case 25:
                      if ((argCount != 2)) {
                        hasInterrupt = EX_InvalidArgument(ec, primitiveMethodWrongArgCountError("string replace method", 2, argCount));
                      } else {
                        value2 = funcArgs[0];
                        value3 = funcArgs[1];
                        if (((value2.type != 5) || (value3.type != 5))) {
                          hasInterrupt = EX_InvalidArgument(ec, "string replace method requires 2 strings as input.");
                        } else {
                          output = buildString(globals, string1.replace((CharSequence) ((String) value2.internalValue), (CharSequence) ((String) value3.internalValue)));
                        }
                      }
                      break;
                    case 26:
                      if ((argCount > 0)) {
                        hasInterrupt = EX_InvalidArgument(ec, primitiveMethodWrongArgCountError("string reverse method", 0, argCount));
                      } else {
                        output = buildString(globals, PST_reverseString(string1));
                      }
                      break;
                    case 27:
                      if ((argCount > 0)) {
                        hasInterrupt = EX_InvalidArgument(ec, primitiveMethodWrongArgCountError("string rtrim method", 0, argCount));
                      } else {
                        output = buildString(globals, PST_trimSide(string1, false));
                      }
                      break;
                    case 30:
                      if ((argCount != 1)) {
                        hasInterrupt = EX_InvalidArgument(ec, primitiveMethodWrongArgCountError("string split method", 1, argCount));
                      } else {
                        value2 = funcArgs[0];
                        if ((value2.type != 5)) {
                          hasInterrupt = EX_InvalidArgument(ec, "string split method requires another string as input.");
                        } else {
                          stringList = PST_literalStringSplit(string1, ((String) value2.internalValue));
                          _len = stringList.length;
                          list1 = makeEmptyList(globals.stringType, _len);
                          i = 0;
                          while ((i < _len)) {
                            list1.array[i] = buildString(globals, stringList[i]);
                            i += 1;
                          }
                          list1.size = _len;
                          output = new Value(6, list1);
                        }
                      }
                      break;
                    case 31:
                      if ((argCount != 1)) {
                        hasInterrupt = EX_InvalidArgument(ec, primitiveMethodWrongArgCountError("string startsWith method", 1, argCount));
                      } else {
                        value2 = funcArgs[0];
                        if ((value2.type != 5)) {
                          hasInterrupt = EX_InvalidArgument(ec, "string startsWith method requires another string as input.");
                        } else {
                          if (string1.startsWith(((String) value2.internalValue))) {
                            output = VALUE_TRUE;
                          } else {
                            output = VALUE_FALSE;
                          }
                        }
                      }
                      break;
                    case 32:
                      if ((argCount > 0)) {
                        hasInterrupt = EX_InvalidArgument(ec, primitiveMethodWrongArgCountError("string trim method", 0, argCount));
                      } else {
                        output = buildString(globals, string1.trim());
                      }
                      break;
                    case 33:
                      if ((argCount > 0)) {
                        hasInterrupt = EX_InvalidArgument(ec, primitiveMethodWrongArgCountError("string upper method", 0, argCount));
                      } else {
                        output = buildString(globals, string1.toUpperCase());
                      }
                      break;
                    default:
                      output = null;
                      break;
                  }
                  break;
                case 6:
                  // ...on a list;
                  list1 = ((ListImpl) value.internalValue);
                  switch (functionId) {
                    case 0:
                      if ((argCount == 0)) {
                        hasInterrupt = EX_InvalidArgument(ec, "List add method requires at least one argument.");
                      } else {
                        intArray1 = list1.type;
                        while (((list1.size + argCount) > list1.capacity)) {
                          increaseListCapacity(list1);
                        }
                        int1 = list1.size;
                        i = 0;
                        while ((i < argCount)) {
                          value = funcArgs[i];
                          if ((intArray1 != null)) {
                            value2 = canAssignTypeToGeneric(vm, value, intArray1, 0);
                            if ((value2 == null)) {
                              hasInterrupt = EX_InvalidArgument(ec, "Cannot convert a " + typeToStringFromValue(vm, value) + " into a " + typeToString(vm, list1.type, 0));
                            }
                            list1.array[(int1 + i)] = value2;
                          } else {
                            list1.array[(int1 + i)] = value;
                          }
                          i += 1;
                        }
                        list1.size += argCount;
                        output = VALUE_NULL;
                      }
                      break;
                    case 3:
                      if ((argCount > 0)) {
                        hasInterrupt = EX_InvalidArgument(ec, primitiveMethodWrongArgCountError("list choice method", 0, argCount));
                      } else {
                        _len = list1.size;
                        if ((_len == 0)) {
                          hasInterrupt = EX_UnsupportedOperation(ec, "Cannot use list.choice() method on an empty list.");
                        } else {
                          i = ((int) ((PST_random.nextDouble() * _len)));
                          output = list1.array[i];
                        }
                      }
                      break;
                    case 4:
                      if ((argCount > 0)) {
                        hasInterrupt = EX_InvalidArgument(ec, primitiveMethodWrongArgCountError("list clear method", 0, argCount));
                      } else {
                        if ((list1.size > 0)) {
                          i = (list1.size - 1);
                          while ((i >= 0)) {
                            list1.array[i] = null;
                            i -= 1;
                          }
                          list1.size = 0;
                        }
                      }
                      break;
                    case 5:
                      if ((argCount > 0)) {
                        hasInterrupt = EX_InvalidArgument(ec, primitiveMethodWrongArgCountError("list clone method", 0, argCount));
                      } else {
                        _len = list1.size;
                        list2 = makeEmptyList(list1.type, _len);
                        i = 0;
                        while ((i < _len)) {
                          list2.array[i] = list1.array[i];
                          i += 1;
                        }
                        list2.size = _len;
                        output = new Value(6, list2);
                      }
                      break;
                    case 6:
                      if ((argCount != 1)) {
                        hasInterrupt = EX_InvalidArgument(ec, primitiveMethodWrongArgCountError("list concat method", 1, argCount));
                      } else {
                        value2 = funcArgs[0];
                        if ((value2.type != 6)) {
                          hasInterrupt = EX_InvalidArgument(ec, "list concat methods requires a list as an argument.");
                        } else {
                          list2 = ((ListImpl) value2.internalValue);
                          intArray1 = list1.type;
                          if (((intArray1 != null) && !canAssignGenericToGeneric(vm, list2.type, 0, intArray1, 0, intBuffer))) {
                            hasInterrupt = EX_InvalidArgument(ec, "Cannot concat a list: incompatible types.");
                          } else {
                            if (((intArray1 != null) && (intArray1[0] == 4) && (list2.type[0] == 3))) {
                              bool1 = true;
                            } else {
                              bool1 = false;
                            }
                            _len = list2.size;
                            int1 = list1.size;
                            while (((int1 + _len) > list1.capacity)) {
                              increaseListCapacity(list1);
                            }
                            i = 0;
                            while ((i < _len)) {
                              value = list2.array[i];
                              if (bool1) {
                                value = buildFloat(globals, (0.0 + value.intValue));
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
                      if ((argCount != 1)) {
                        hasInterrupt = EX_InvalidArgument(ec, primitiveMethodWrongArgCountError("list contains method", 1, argCount));
                      } else {
                        value2 = funcArgs[0];
                        _len = list1.size;
                        output = VALUE_FALSE;
                        i = 0;
                        while ((i < _len)) {
                          value = list1.array[i];
                          if ((doEqualityComparisonAndReturnCode(value2, value) == 1)) {
                            output = VALUE_TRUE;
                            i = _len;
                          }
                          i += 1;
                        }
                      }
                      break;
                    case 10:
                      if ((argCount != 1)) {
                        hasInterrupt = EX_InvalidArgument(ec, primitiveMethodWrongArgCountError("list filter method", 1, argCount));
                      } else {
                        value2 = funcArgs[0];
                        if ((value2.type != 9)) {
                          hasInterrupt = EX_InvalidArgument(ec, "list filter method requires a function pointer as its argument.");
                        } else {
                          primitiveMethodToCoreLibraryFallback = true;
                          functionId = metadata.primitiveMethodFunctionIdFallbackLookup[0];
                          funcArgs[1] = value;
                          argCount = 2;
                          output = null;
                        }
                      }
                      break;
                    case 14:
                      if ((argCount != 2)) {
                        hasInterrupt = EX_InvalidArgument(ec, primitiveMethodWrongArgCountError("list insert method", 1, argCount));
                      } else {
                        value = funcArgs[0];
                        value2 = funcArgs[1];
                        if ((value.type != 3)) {
                          hasInterrupt = EX_InvalidArgument(ec, "First argument of list.insert must be an integer index.");
                        } else {
                          intArray1 = list1.type;
                          if ((intArray1 != null)) {
                            value3 = canAssignTypeToGeneric(vm, value2, intArray1, 0);
                            if ((value3 == null)) {
                              hasInterrupt = EX_InvalidArgument(ec, "Cannot insert this type into this type of list.");
                            }
                            value2 = value3;
                          }
                          if (!hasInterrupt) {
                            if ((list1.size == list1.capacity)) {
                              increaseListCapacity(list1);
                            }
                            int1 = value.intValue;
                            _len = list1.size;
                            if ((int1 < 0)) {
                              int1 += _len;
                            }
                            if ((int1 == _len)) {
                              list1.array[_len] = value2;
                              list1.size += 1;
                            } else {
                              if (((int1 < 0) || (int1 >= _len))) {
                                hasInterrupt = EX_IndexOutOfRange(ec, "Index out of range.");
                              } else {
                                i = int1;
                                while ((i < _len)) {
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
                      }
                      break;
                    case 17:
                      if ((argCount != 1)) {
                        if ((argCount == 0)) {
                          value2 = globals.stringEmpty;
                        } else {
                          hasInterrupt = EX_InvalidArgument(ec, primitiveMethodWrongArgCountError("list join method", 1, argCount));
                        }
                      } else {
                        value2 = funcArgs[0];
                        if ((value2.type != 5)) {
                          hasInterrupt = EX_InvalidArgument(ec, "Argument of list.join needs to be a string.");
                        }
                      }
                      if (!hasInterrupt) {
                        stringList1 = new ArrayList<String>();
                        string1 = ((String) value2.internalValue);
                        _len = list1.size;
                        i = 0;
                        while ((i < _len)) {
                          value = list1.array[i];
                          if ((value.type != 5)) {
                            string2 = valueToString(vm, value);
                          } else {
                            string2 = ((String) value.internalValue);
                          }
                          stringList1.add(string2);
                          i += 1;
                        }
                        output = buildString(globals, PST_joinList(string1, stringList1));
                      }
                      break;
                    case 21:
                      if ((argCount != 1)) {
                        hasInterrupt = EX_InvalidArgument(ec, primitiveMethodWrongArgCountError("list map method", 1, argCount));
                      } else {
                        value2 = funcArgs[0];
                        if ((value2.type != 9)) {
                          hasInterrupt = EX_InvalidArgument(ec, "list map method requires a function pointer as its argument.");
                        } else {
                          primitiveMethodToCoreLibraryFallback = true;
                          functionId = metadata.primitiveMethodFunctionIdFallbackLookup[1];
                          funcArgs[1] = value;
                          argCount = 2;
                          output = null;
                        }
                      }
                      break;
                    case 23:
                      if ((argCount > 0)) {
                        hasInterrupt = EX_InvalidArgument(ec, primitiveMethodWrongArgCountError("list pop method", 0, argCount));
                      } else {
                        _len = list1.size;
                        if ((_len < 1)) {
                          hasInterrupt = EX_IndexOutOfRange(ec, "Cannot pop from an empty list.");
                        } else {
                          _len -= 1;
                          value = list1.array[_len];
                          list1.array[_len] = null;
                          if (returnValueUsed) {
                            output = value;
                          }
                          list1.size = _len;
                        }
                      }
                      break;
                    case 24:
                      if ((argCount != 1)) {
                        hasInterrupt = EX_InvalidArgument(ec, primitiveMethodWrongArgCountError("list remove method", 1, argCount));
                      } else {
                        value = funcArgs[0];
                        if ((value.type != 3)) {
                          hasInterrupt = EX_InvalidArgument(ec, "Argument of list.remove must be an integer index.");
                        } else {
                          int1 = value.intValue;
                          _len = list1.size;
                          if ((int1 < 0)) {
                            int1 += _len;
                          }
                          if (((int1 < 0) || (int1 >= _len))) {
                            hasInterrupt = EX_IndexOutOfRange(ec, "Index out of range.");
                          } else {
                            if (returnValueUsed) {
                              output = list1.array[int1];
                            }
                            _len = (list1.size - 1);
                            list1.size = _len;
                            i = int1;
                            while ((i < _len)) {
                              list1.array[i] = list1.array[(i + 1)];
                              i += 1;
                            }
                            list1.array[_len] = null;
                          }
                        }
                      }
                      break;
                    case 26:
                      if ((argCount > 0)) {
                        hasInterrupt = EX_InvalidArgument(ec, primitiveMethodWrongArgCountError("list reverse method", 0, argCount));
                      } else {
                        reverseList(list1);
                      }
                      break;
                    case 28:
                      if ((argCount > 0)) {
                        hasInterrupt = EX_InvalidArgument(ec, primitiveMethodWrongArgCountError("list shuffle method", 0, argCount));
                      } else {
                        _len = list1.size;
                        i = 0;
                        while ((i < _len)) {
                          j = ((int) ((PST_random.nextDouble() * _len)));
                          value = list1.array[i];
                          list1.array[i] = list1.array[j];
                          list1.array[j] = value;
                          i += 1;
                        }
                      }
                      break;
                    case 29:
                      if ((argCount == 0)) {
                        sortLists(list1, list1, PST_intBuffer16);
                        if ((PST_intBuffer16[0] > 0)) {
                          hasInterrupt = EX_InvalidArgument(ec, "Invalid list to sort. All items must be numbers or all strings, but not mixed.");
                        }
                      } else {
                        if ((argCount == 1)) {
                          value2 = funcArgs[0];
                          if ((value2.type == 9)) {
                            primitiveMethodToCoreLibraryFallback = true;
                            functionId = metadata.primitiveMethodFunctionIdFallbackLookup[2];
                            funcArgs[1] = value;
                            argCount = 2;
                          } else {
                            hasInterrupt = EX_InvalidArgument(ec, "list.sort(get_key_function) requires a function pointer as its argument.");
                          }
                          output = null;
                        }
                      }
                      break;
                    default:
                      output = null;
                      break;
                  }
                  break;
                case 7:
                  // ...on a dictionary;
                  dictImpl = ((DictImpl) value.internalValue);
                  switch (functionId) {
                    case 4:
                      if ((argCount > 0)) {
                        hasInterrupt = EX_InvalidArgument(ec, primitiveMethodWrongArgCountError("dictionary clear method", 0, argCount));
                      } else {
                        if ((dictImpl.size > 0)) {
                          dictImpl.intToIndex = new HashMap<Integer, Integer>();
                          dictImpl.stringToIndex = new HashMap<String, Integer>();
                          dictImpl.keys.clear();
                          dictImpl.values.clear();
                          dictImpl.size = 0;
                        }
                      }
                      break;
                    case 5:
                      if ((argCount > 0)) {
                        hasInterrupt = EX_InvalidArgument(ec, primitiveMethodWrongArgCountError("dictionary clone method", 0, argCount));
                      } else {
                        output = new Value(7, cloneDictionary(dictImpl, null));
                      }
                      break;
                    case 7:
                      if ((argCount != 1)) {
                        hasInterrupt = EX_InvalidArgument(ec, primitiveMethodWrongArgCountError("dictionary contains method", 1, argCount));
                      } else {
                        value = funcArgs[0];
                        output = VALUE_FALSE;
                        if ((value.type == 5)) {
                          if (dictImpl.stringToIndex.containsKey(((String) value.internalValue))) {
                            output = VALUE_TRUE;
                          }
                        } else {
                          if ((value.type == 3)) {
                            i = value.intValue;
                          } else {
                            i = (((ObjectInstance) value.internalValue)).objectId;
                          }
                          if (dictImpl.intToIndex.containsKey(i)) {
                            output = VALUE_TRUE;
                          }
                        }
                      }
                      break;
                    case 11:
                      if (((argCount != 1) && (argCount != 2))) {
                        hasInterrupt = EX_InvalidArgument(ec, "Dictionary get method requires 1 or 2 arguments.");
                      } else {
                        value = funcArgs[0];
                        switch (value.type) {
                          case 3:
                            int1 = value.intValue;
                            Integer dictLookup7 = dictImpl.intToIndex.get(int1);
                            i = dictLookup7 == null ? (-1) : dictLookup7;
                            break;
                          case 8:
                            int1 = (((ObjectInstance) value.internalValue)).objectId;
                            Integer dictLookup8 = dictImpl.intToIndex.get(int1);
                            i = dictLookup8 == null ? (-1) : dictLookup8;
                            break;
                          case 5:
                            string1 = ((String) value.internalValue);
                            Integer dictLookup9 = dictImpl.stringToIndex.get(string1);
                            i = dictLookup9 == null ? (dictImpl.stringToIndex.containsKey(string1) ? null : (-1)) : dictLookup9;
                            break;
                        }
                        if ((i == -1)) {
                          if ((argCount == 2)) {
                            output = funcArgs[1];
                          } else {
                            output = VALUE_NULL;
                          }
                        } else {
                          output = dictImpl.values.get(i);
                        }
                      }
                      break;
                    case 18:
                      if ((argCount > 0)) {
                        hasInterrupt = EX_InvalidArgument(ec, primitiveMethodWrongArgCountError("dictionary keys method", 0, argCount));
                      } else {
                        valueList1 = dictImpl.keys;
                        _len = valueList1.size();
                        if ((dictImpl.keyType == 8)) {
                          intArray1 = new int[2];
                          intArray1[0] = 8;
                          intArray1[0] = dictImpl.keyClassId;
                        } else {
                          intArray1 = new int[1];
                          intArray1[0] = dictImpl.keyType;
                        }
                        list1 = makeEmptyList(intArray1, _len);
                        i = 0;
                        while ((i < _len)) {
                          list1.array[i] = valueList1.get(i);
                          i += 1;
                        }
                        list1.size = _len;
                        output = new Value(6, list1);
                      }
                      break;
                    case 22:
                      if ((argCount != 1)) {
                        hasInterrupt = EX_InvalidArgument(ec, primitiveMethodWrongArgCountError("dictionary merge method", 1, argCount));
                      } else {
                        value2 = funcArgs[0];
                        if ((value2.type != 7)) {
                          hasInterrupt = EX_InvalidArgument(ec, "dictionary merge method requires another dictionary as a parameeter.");
                        } else {
                          dictImpl2 = ((DictImpl) value2.internalValue);
                          if ((dictImpl2.size > 0)) {
                            if ((dictImpl.size == 0)) {
                              value.internalValue = cloneDictionary(dictImpl2, null);
                            } else {
                              if ((dictImpl2.keyType != dictImpl.keyType)) {
                                hasInterrupt = EX_InvalidKey(ec, "Dictionaries with different key types cannot be merged.");
                              } else {
                                if (((dictImpl2.keyType == 8) && (dictImpl2.keyClassId != dictImpl.keyClassId) && (dictImpl.keyClassId != 0) && !isClassASubclassOf(vm, dictImpl2.keyClassId, dictImpl.keyClassId))) {
                                  hasInterrupt = EX_InvalidKey(ec, "Dictionary key types are incompatible.");
                                } else {
                                  if ((dictImpl.valueType == null)) {
                                  } else {
                                    if ((dictImpl2.valueType == null)) {
                                      hasInterrupt = EX_InvalidKey(ec, "Dictionaries with different value types cannot be merged.");
                                    } else {
                                      if (!canAssignGenericToGeneric(vm, dictImpl2.valueType, 0, dictImpl.valueType, 0, intBuffer)) {
                                        hasInterrupt = EX_InvalidKey(ec, "The dictionary value types are incompatible.");
                                      }
                                    }
                                  }
                                  if (!hasInterrupt) {
                                    cloneDictionary(dictImpl2, dictImpl);
                                  }
                                }
                              }
                            }
                          }
                          output = VALUE_NULL;
                        }
                      }
                      break;
                    case 24:
                      if ((argCount != 1)) {
                        hasInterrupt = EX_InvalidArgument(ec, primitiveMethodWrongArgCountError("dictionary remove method", 1, argCount));
                      } else {
                        value2 = funcArgs[0];
                        bool2 = false;
                        keyType = dictImpl.keyType;
                        if (((dictImpl.size > 0) && (keyType == value2.type))) {
                          if ((keyType == 5)) {
                            stringKey = ((String) value2.internalValue);
                            if (dictImpl.stringToIndex.containsKey(stringKey)) {
                              i = dictImpl.stringToIndex.get(stringKey);
                              bool2 = true;
                            }
                          } else {
                            if ((keyType == 3)) {
                              intKey = value2.intValue;
                            } else {
                              intKey = (((ObjectInstance) value2.internalValue)).objectId;
                            }
                            if (dictImpl.intToIndex.containsKey(intKey)) {
                              i = dictImpl.intToIndex.get(intKey);
                              bool2 = true;
                            }
                          }
                          if (bool2) {
                            _len = (dictImpl.size - 1);
                            dictImpl.size = _len;
                            if ((i == _len)) {
                              if ((keyType == 5)) {
                                dictImpl.stringToIndex.remove(stringKey);
                              } else {
                                dictImpl.intToIndex.remove(intKey);
                              }
                              dictImpl.keys.remove(i);
                              dictImpl.values.remove(i);
                            } else {
                              value = dictImpl.keys.get(_len);
                              dictImpl.keys.set(i, value);
                              dictImpl.values.set(i, dictImpl.values.get(_len));
                              dictImpl.keys.remove(dictImpl.keys.size() - 1);
                              dictImpl.values.remove(dictImpl.values.size() - 1);
                              if ((keyType == 5)) {
                                dictImpl.stringToIndex.remove(stringKey);
                                stringKey = ((String) value.internalValue);
                                dictImpl.stringToIndex.put(stringKey, i);
                              } else {
                                dictImpl.intToIndex.remove(intKey);
                                if ((keyType == 3)) {
                                  intKey = value.intValue;
                                } else {
                                  intKey = (((ObjectInstance) value.internalValue)).objectId;
                                }
                                dictImpl.intToIndex.put(intKey, i);
                              }
                            }
                          }
                        }
                        if (!bool2) {
                          hasInterrupt = EX_KeyNotFound(ec, "dictionary does not contain the given key.");
                        }
                      }
                      break;
                    case 34:
                      if ((argCount > 0)) {
                        hasInterrupt = EX_InvalidArgument(ec, primitiveMethodWrongArgCountError("dictionary values method", 0, argCount));
                      } else {
                        valueList1 = dictImpl.values;
                        _len = valueList1.size();
                        list1 = makeEmptyList(dictImpl.valueType, _len);
                        i = 0;
                        while ((i < _len)) {
                          addToList(list1, valueList1.get(i));
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
                  functionPointer1 = ((FunctionPointer) value.internalValue);
                  switch (functionId) {
                    case 1:
                      if ((argCount > 0)) {
                        hasInterrupt = EX_InvalidArgument(ec, primitiveMethodWrongArgCountError("argCountMax method", 0, argCount));
                      } else {
                        functionId = functionPointer1.functionId;
                        functionInfo = metadata.functionTable[functionId];
                        output = buildInteger(globals, functionInfo.maxArgs);
                      }
                      break;
                    case 2:
                      if ((argCount > 0)) {
                        hasInterrupt = EX_InvalidArgument(ec, primitiveMethodWrongArgCountError("argCountMin method", 0, argCount));
                      } else {
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
                      if ((argCount == 1)) {
                        funcArgs[1] = funcArgs[0];
                      } else {
                        if ((argCount == 0)) {
                          funcArgs[1] = VALUE_NULL;
                        } else {
                          hasInterrupt = EX_InvalidArgument(ec, "invoke requires a list of arguments.");
                        }
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
                  classValue = ((org.crayonlang.interpreter.structs.ClassValue) value.internalValue);
                  switch (functionId) {
                    case 12:
                      classInfo = metadata.classTable[classValue.classId];
                      output = buildString(globals, classInfo.fullyQualifiedName);
                      break;
                    case 16:
                      if ((argCount != 1)) {
                        hasInterrupt = EX_InvalidArgument(ec, primitiveMethodWrongArgCountError("class isA method", 1, argCount));
                      } else {
                        int1 = classValue.classId;
                        value = funcArgs[0];
                        if ((value.type != 10)) {
                          hasInterrupt = EX_InvalidArgument(ec, "class isA method requires another class reference.");
                        } else {
                          classValue = ((org.crayonlang.interpreter.structs.ClassValue) value.internalValue);
                          int2 = classValue.classId;
                          output = VALUE_FALSE;
                          if (isClassASubclassOf(vm, int1, int2)) {
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
              if (!hasInterrupt) {
                if ((output == null)) {
                  if (primitiveMethodToCoreLibraryFallback) {
                    type = 1;
                    bool1 = true;
                  } else {
                    hasInterrupt = EX_InvalidInvocation(ec, "primitive method not found.");
                  }
                } else {
                  if (returnValueUsed) {
                    if ((valueStackSize == valueStackCapacity)) {
                      valueStack = valueStackIncreaseCapacity(ec);
                      valueStackCapacity = valueStack.length;
                    }
                    valueStack[valueStackSize] = output;
                    valueStackSize += 1;
                  }
                  bool1 = false;
                }
              }
            }
            if ((bool1 && !hasInterrupt)) {
              // push a new frame to the stack;
              stack.pc = pc;
              bool1 = false;
              switch (type) {
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
                  functionInfo = metadata.lambdaTable.get(functionId);
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
                  i = (valueArray1.length - 1);
                  while ((i >= 0)) {
                    switch (classInfo.fieldInitializationCommand[i]) {
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
                  if (returnValueUsed) {
                    returnValueUsed = false;
                    if ((valueStackSize == valueStackCapacity)) {
                      valueStack = valueStackIncreaseCapacity(ec);
                      valueStackCapacity = valueStack.length;
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
              if (((argCount < functionInfo.minArgs) || (argCount > functionInfo.maxArgs))) {
                pc = stack.pc;
                hasInterrupt = EX_InvalidArgument(ec, "Incorrect number of args were passed to this function.");
              } else {
                int1 = functionInfo.localsSize;
                int2 = stack.localsStackOffsetEnd;
                if ((localsStackCapacity <= (int2 + int1))) {
                  increaseLocalsStackCapacity(ec, int1);
                  localsStack = ec.localsStack;
                  localsStackSet = ec.localsStackSet;
                  localsStackCapacity = localsStack.length;
                }
                localsStackSetToken = (ec.localsStackSetToken + 1);
                ec.localsStackSetToken = localsStackSetToken;
                if ((localsStackSetToken > 2000000000)) {
                  resetLocalsStackTokens(ec, stack);
                  localsStackSetToken = 2;
                }
                localsStackOffset = int2;
                if ((type != 10)) {
                  closure = null;
                }
                // invoke the function;
                stack = new StackFrame(pc, localsStackSetToken, localsStackOffset, (localsStackOffset + int1), stack, returnValueUsed, value, valueStackSize, 0, (stack.depth + 1), 0, null, closure, null);
                i = 0;
                while ((i < argCount)) {
                  int1 = (localsStackOffset + i);
                  localsStack[int1] = funcArgs[i];
                  localsStackSet[int1] = localsStackSetToken;
                  i += 1;
                }
                if ((argCount != functionInfo.minArgs)) {
                  int1 = (argCount - functionInfo.minArgs);
                  if ((int1 > 0)) {
                    pc += functionInfo.pcOffsetsForOptionalArgs[int1];
                    stack.pc = pc;
                  }
                }
                if ((stack.depth > 1000)) {
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
          if ((value2 == null)) {
            if (((value.type == 4) && (row[0] == 3))) {
              if ((row[1] == 1)) {
                float1 = ((double) value.internalValue);
                if (((float1 < 0) && ((float1 % 1) != 0))) {
                  i = (((int) float1) - 1);
                } else {
                  i = ((int) float1);
                }
                if ((i < 0)) {
                  if ((i > -257)) {
                    value2 = globals.negativeIntegers[-i];
                  } else {
                    value2 = new Value(i);
                  }
                } else {
                  if ((i < 2049)) {
                    value2 = globals.positiveIntegers[i];
                  } else {
                    value2 = new Value(i);
                  }
                }
              }
            } else {
              if (((value.type == 3) && (row[0] == 4))) {
                int1 = value.intValue;
                if ((int1 == 0)) {
                  value2 = VALUE_FLOAT_ZERO;
                } else {
                  value2 = new Value(4, (0.0 + int1));
                }
              }
            }
            if ((value2 != null)) {
              valueStack[(valueStackSize - 1)] = value2;
            }
          }
          if ((value2 == null)) {
            hasInterrupt = EX_InvalidArgument(ec, "Cannot convert a " + typeToStringFromValue(vm, value) + " to a " + typeToString(vm, row, 0));
          } else {
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
          nativeFp = metadata.cniFunctionsById.get(row[0]);
          if ((nativeFp == null)) {
            hasInterrupt = EX_InvalidInvocation(ec, "CNI method could not be found.");
          } else {
            _len = row[1];
            valueStackSize -= _len;
            valueArray1 = new Value[_len];
            i = 0;
            while ((i < _len)) {
              valueArray1[i] = valueStack[(valueStackSize + i)];
              i += 1;
            }
            prepareToSuspend(ec, stack, valueStackSize, pc);
            value = ((Value) TranslationHelper.invokeFunctionPointer(nativeFp, new Object[] {vm, valueArray1}));
            if (ec.executionStateChange) {
              ec.executionStateChange = false;
              if ((ec.executionStateChangeCommand == 1)) {
                return suspendInterpreter();
              }
            }
            if ((row[2] == 1)) {
              if ((valueStackSize == valueStackCapacity)) {
                valueStack = valueStackIncreaseCapacity(ec);
                valueStackCapacity = valueStack.length;
              }
              valueStack[valueStackSize] = value;
              valueStackSize += 1;
            }
          }
          break;
        case 16:
          // CNI_REGISTER;
          nativeFp = ((java.lang.reflect.Method) TranslationHelper.getFunction(stringArgs[pc]));
          metadata.cniFunctionsById.put(row[0], nativeFp);
          break;
        case 17:
          // COMMAND_LINE_ARGS;
          if ((valueStackSize == valueStackCapacity)) {
            valueStack = valueStackIncreaseCapacity(ec);
            valueStackCapacity = valueStack.length;
          }
          list1 = makeEmptyList(globals.stringType, 3);
          i = 0;
          while ((i < vm.environment.commandLineArgs.length)) {
            addToList(list1, buildString(globals, vm.environment.commandLineArgs[i]));
            i += 1;
          }
          valueStack[valueStackSize] = new Value(6, list1);
          valueStackSize += 1;
          break;
        case 18:
          // CONTINUE;
          if ((row[0] == 1)) {
            pc += row[1];
          } else {
            intArray1 = esfData[pc];
            pc = (intArray1[1] - 1);
            valueStackSize = stack.valueStackPopSize;
            stack.postFinallyBehavior = 2;
          }
          break;
        case 19:
          // CORE_FUNCTION;
          switch (row[0]) {
            case 1:
              // parseInt;
              arg1 = valueStack[--valueStackSize];
              output = VALUE_NULL;
              if ((arg1.type == 5)) {
                string1 = (((String) arg1.internalValue)).trim();
                if (PST_isValidInteger(string1)) {
                  output = buildInteger(globals, Integer.parseInt(string1));
                }
              } else {
                hasInterrupt = EX_InvalidArgument(ec, "parseInt requires a string argument.");
              }
              break;
            case 2:
              // parseFloat;
              arg1 = valueStack[--valueStackSize];
              output = VALUE_NULL;
              if ((arg1.type == 5)) {
                string1 = (((String) arg1.internalValue)).trim();
                PST_parseFloatOrReturnNull(floatList1, string1);
                if ((floatList1[0] >= 0)) {
                  output = buildFloat(globals, floatList1[1]);
                }
              } else {
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
              while ((int2 > 0)) {
                if ((row[(2 + int2)] == int1)) {
                  output = VALUE_TRUE;
                  int2 = 0;
                } else {
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
              if ((arg1.type != 2)) {
                hasInterrupt = EX_InvalidArgument(ec, "Assertion expression must be a boolean.");
              } else {
                if ((arg1.intValue == 1)) {
                  output = VALUE_NULL;
                } else {
                  string1 = valueToString(vm, arg2);
                  if ((arg3.intValue == 1)) {
                    string1 = "Assertion failed: " + string1;
                  }
                  hasInterrupt = EX_AssertionFailed(ec, string1);
                }
              }
              break;
            case 8:
              // chr;
              arg1 = valueStack[--valueStackSize];
              output = null;
              if ((arg1.type == 3)) {
                int1 = arg1.intValue;
                if (((int1 >= 0) && (int1 < 256))) {
                  output = buildCommonString(globals, ("" + Character.toString((char) int1)));
                }
              }
              if ((output == null)) {
                hasInterrupt = EX_InvalidArgument(ec, "chr requires an integer between 0 and 255.");
              }
              break;
            case 9:
              // ord;
              arg1 = valueStack[--valueStackSize];
              output = null;
              if ((arg1.type == 5)) {
                string1 = ((String) arg1.internalValue);
                if ((string1.length() == 1)) {
                  output = buildInteger(globals, ((int) string1.charAt(0)));
                }
              }
              if ((output == null)) {
                hasInterrupt = EX_InvalidArgument(ec, "ord requires a 1 character string.");
              }
              break;
            case 10:
              // currentTime;
              output = buildFloat(globals, System.currentTimeMillis() / 1000.0);
              break;
            case 11:
              // sortList;
              valueStackSize -= 2;
              arg2 = valueStack[(valueStackSize + 1)];
              arg1 = valueStack[valueStackSize];
              output = VALUE_NULL;
              list1 = ((ListImpl) arg1.internalValue);
              list2 = ((ListImpl) arg2.internalValue);
              sortLists(list2, list1, PST_intBuffer16);
              if ((PST_intBuffer16[0] > 0)) {
                hasInterrupt = EX_InvalidArgument(ec, "Invalid sort keys. Keys must be all numbers or all strings, but not mixed.");
              }
              break;
            case 12:
              // abs;
              arg1 = valueStack[--valueStackSize];
              output = arg1;
              if ((arg1.type == 3)) {
                if ((arg1.intValue < 0)) {
                  output = buildInteger(globals, -arg1.intValue);
                }
              } else {
                if ((arg1.type == 4)) {
                  if ((((double) arg1.internalValue) < 0)) {
                    output = buildFloat(globals, -((double) arg1.internalValue));
                  }
                } else {
                  hasInterrupt = EX_InvalidArgument(ec, "abs requires a number as input.");
                }
              }
              break;
            case 13:
              // arcCos;
              arg1 = valueStack[--valueStackSize];
              if ((arg1.type == 4)) {
                float1 = ((double) arg1.internalValue);
              } else {
                if ((arg1.type == 3)) {
                  float1 = (0.0 + arg1.intValue);
                } else {
                  hasInterrupt = EX_InvalidArgument(ec, "arccos requires a number as input.");
                }
              }
              if (!hasInterrupt) {
                if (((float1 < -1) || (float1 > 1))) {
                  hasInterrupt = EX_InvalidArgument(ec, "arccos requires a number in the range of -1 to 1.");
                } else {
                  output = buildFloat(globals, Math.acos(float1));
                }
              }
              break;
            case 14:
              // arcSin;
              arg1 = valueStack[--valueStackSize];
              if ((arg1.type == 4)) {
                float1 = ((double) arg1.internalValue);
              } else {
                if ((arg1.type == 3)) {
                  float1 = (0.0 + arg1.intValue);
                } else {
                  hasInterrupt = EX_InvalidArgument(ec, "arcsin requires a number as input.");
                }
              }
              if (!hasInterrupt) {
                if (((float1 < -1) || (float1 > 1))) {
                  hasInterrupt = EX_InvalidArgument(ec, "arcsin requires a number in the range of -1 to 1.");
                } else {
                  output = buildFloat(globals, Math.asin(float1));
                }
              }
              break;
            case 15:
              // arcTan;
              valueStackSize -= 2;
              arg2 = valueStack[(valueStackSize + 1)];
              arg1 = valueStack[valueStackSize];
              bool1 = false;
              if ((arg1.type == 4)) {
                float1 = ((double) arg1.internalValue);
              } else {
                if ((arg1.type == 3)) {
                  float1 = (0.0 + arg1.intValue);
                } else {
                  bool1 = true;
                }
              }
              if ((arg2.type == 4)) {
                float2 = ((double) arg2.internalValue);
              } else {
                if ((arg2.type == 3)) {
                  float2 = (0.0 + arg2.intValue);
                } else {
                  bool1 = true;
                }
              }
              if (bool1) {
                hasInterrupt = EX_InvalidArgument(ec, "arctan requires numeric arguments.");
              } else {
                output = buildFloat(globals, Math.atan2(float1, float2));
              }
              break;
            case 16:
              // cos;
              arg1 = valueStack[--valueStackSize];
              if ((arg1.type == 4)) {
                float1 = ((double) arg1.internalValue);
                output = buildFloat(globals, Math.cos(float1));
              } else {
                if ((arg1.type == 3)) {
                  int1 = arg1.intValue;
                  output = buildFloat(globals, Math.cos(int1));
                } else {
                  hasInterrupt = EX_InvalidArgument(ec, "cos requires a number argument.");
                }
              }
              break;
            case 17:
              // ensureRange;
              valueStackSize -= 3;
              arg3 = valueStack[(valueStackSize + 2)];
              arg2 = valueStack[(valueStackSize + 1)];
              arg1 = valueStack[valueStackSize];
              bool1 = false;
              if ((arg2.type == 4)) {
                float2 = ((double) arg2.internalValue);
              } else {
                if ((arg2.type == 3)) {
                  float2 = (0.0 + arg2.intValue);
                } else {
                  bool1 = true;
                }
              }
              if ((arg3.type == 4)) {
                float3 = ((double) arg3.internalValue);
              } else {
                if ((arg3.type == 3)) {
                  float3 = (0.0 + arg3.intValue);
                } else {
                  bool1 = true;
                }
              }
              if ((!bool1 && (float3 < float2))) {
                float1 = float3;
                float3 = float2;
                float2 = float1;
                value = arg2;
                arg2 = arg3;
                arg3 = value;
              }
              if ((arg1.type == 4)) {
                float1 = ((double) arg1.internalValue);
              } else {
                if ((arg1.type == 3)) {
                  float1 = (0.0 + arg1.intValue);
                } else {
                  bool1 = true;
                }
              }
              if (bool1) {
                hasInterrupt = EX_InvalidArgument(ec, "ensureRange requires numeric arguments.");
              } else {
                if ((float1 < float2)) {
                  output = arg2;
                } else {
                  if ((float1 > float3)) {
                    output = arg3;
                  } else {
                    output = arg1;
                  }
                }
              }
              break;
            case 18:
              // floor;
              arg1 = valueStack[--valueStackSize];
              if ((arg1.type == 4)) {
                float1 = ((double) arg1.internalValue);
                if (((float1 < 0) && ((float1 % 1) != 0))) {
                  int1 = (((int) float1) - 1);
                } else {
                  int1 = ((int) float1);
                }
                if ((int1 < 2049)) {
                  if ((int1 >= 0)) {
                    output = INTEGER_POSITIVE_CACHE[int1];
                  } else {
                    if ((int1 > -257)) {
                      output = INTEGER_NEGATIVE_CACHE[-int1];
                    } else {
                      output = new Value(int1);
                    }
                  }
                } else {
                  output = new Value(int1);
                }
              } else {
                if ((arg1.type == 3)) {
                  output = arg1;
                } else {
                  hasInterrupt = EX_InvalidArgument(ec, "floor expects a numeric argument.");
                }
              }
              break;
            case 19:
              // max;
              valueStackSize -= 2;
              arg2 = valueStack[(valueStackSize + 1)];
              arg1 = valueStack[valueStackSize];
              bool1 = false;
              if ((arg1.type == 4)) {
                float1 = ((double) arg1.internalValue);
              } else {
                if ((arg1.type == 3)) {
                  float1 = (0.0 + arg1.intValue);
                } else {
                  bool1 = true;
                }
              }
              if ((arg2.type == 4)) {
                float2 = ((double) arg2.internalValue);
              } else {
                if ((arg2.type == 3)) {
                  float2 = (0.0 + arg2.intValue);
                } else {
                  bool1 = true;
                }
              }
              if (bool1) {
                hasInterrupt = EX_InvalidArgument(ec, "max requires numeric arguments.");
              } else {
                if ((float1 >= float2)) {
                  output = arg1;
                } else {
                  output = arg2;
                }
              }
              break;
            case 20:
              // min;
              valueStackSize -= 2;
              arg2 = valueStack[(valueStackSize + 1)];
              arg1 = valueStack[valueStackSize];
              bool1 = false;
              if ((arg1.type == 4)) {
                float1 = ((double) arg1.internalValue);
              } else {
                if ((arg1.type == 3)) {
                  float1 = (0.0 + arg1.intValue);
                } else {
                  bool1 = true;
                }
              }
              if ((arg2.type == 4)) {
                float2 = ((double) arg2.internalValue);
              } else {
                if ((arg2.type == 3)) {
                  float2 = (0.0 + arg2.intValue);
                } else {
                  bool1 = true;
                }
              }
              if (bool1) {
                hasInterrupt = EX_InvalidArgument(ec, "min requires numeric arguments.");
              } else {
                if ((float1 <= float2)) {
                  output = arg1;
                } else {
                  output = arg2;
                }
              }
              break;
            case 21:
              // nativeInt;
              valueStackSize -= 2;
              arg2 = valueStack[(valueStackSize + 1)];
              arg1 = valueStack[valueStackSize];
              output = buildInteger(globals, ((int) (((ObjectInstance) arg1.internalValue)).nativeData[arg2.intValue]));
              break;
            case 22:
              // nativeString;
              valueStackSize -= 3;
              arg3 = valueStack[(valueStackSize + 2)];
              arg2 = valueStack[(valueStackSize + 1)];
              arg1 = valueStack[valueStackSize];
              string1 = ((String) (((ObjectInstance) arg1.internalValue)).nativeData[arg2.intValue]);
              if ((arg3.intValue == 1)) {
                output = buildCommonString(globals, string1);
              } else {
                output = buildString(globals, string1);
              }
              break;
            case 23:
              // sign;
              arg1 = valueStack[--valueStackSize];
              if ((arg1.type == 3)) {
                float1 = (0.0 + (arg1.intValue));
              } else {
                if ((arg1.type == 4)) {
                  float1 = ((double) arg1.internalValue);
                } else {
                  hasInterrupt = EX_InvalidArgument(ec, "sign requires a number as input.");
                }
              }
              if ((float1 == 0)) {
                output = VALUE_INT_ZERO;
              } else {
                if ((float1 > 0)) {
                  output = VALUE_INT_ONE;
                } else {
                  output = INTEGER_NEGATIVE_CACHE[1];
                }
              }
              break;
            case 24:
              // sin;
              arg1 = valueStack[--valueStackSize];
              if ((arg1.type == 4)) {
                float1 = ((double) arg1.internalValue);
              } else {
                if ((arg1.type == 3)) {
                  float1 = (0.0 + arg1.intValue);
                } else {
                  hasInterrupt = EX_InvalidArgument(ec, "sin requires a number argument.");
                }
              }
              output = buildFloat(globals, Math.sin(float1));
              break;
            case 25:
              // tan;
              arg1 = valueStack[--valueStackSize];
              if ((arg1.type == 4)) {
                float1 = ((double) arg1.internalValue);
              } else {
                if ((arg1.type == 3)) {
                  float1 = (0.0 + arg1.intValue);
                } else {
                  hasInterrupt = EX_InvalidArgument(ec, "tan requires a number argument.");
                }
              }
              if (!hasInterrupt) {
                float2 = Math.cos(float1);
                if ((float2 < 0)) {
                  float2 = -float2;
                }
                if ((float2 < 0.00000000015)) {
                  hasInterrupt = EX_DivisionByZero(ec, "Tangent is undefined.");
                } else {
                  output = buildFloat(globals, Math.tan(float1));
                }
              }
              break;
            case 26:
              // log;
              valueStackSize -= 2;
              arg2 = valueStack[(valueStackSize + 1)];
              arg1 = valueStack[valueStackSize];
              if ((arg1.type == 4)) {
                float1 = ((double) arg1.internalValue);
              } else {
                if ((arg1.type == 3)) {
                  float1 = (0.0 + arg1.intValue);
                } else {
                  hasInterrupt = EX_InvalidArgument(ec, "logarithms require a number argument.");
                }
              }
              if (!hasInterrupt) {
                if ((float1 <= 0)) {
                  hasInterrupt = EX_InvalidArgument(ec, "logarithms require positive inputs.");
                } else {
                  output = buildFloat(globals, fixFuzzyFloatPrecision((Math.log(float1) * ((double) arg2.internalValue))));
                }
              }
              break;
            case 27:
              // intQueueClear;
              arg1 = valueStack[--valueStackSize];
              output = VALUE_NULL;
              objInstance1 = ((ObjectInstance) arg1.internalValue);
              if ((objInstance1.nativeData != null)) {
                objInstance1.nativeData[1] = 0;
              }
              break;
            case 28:
              // intQueueWrite16;
              output = VALUE_NULL;
              int1 = row[2];
              valueStackSize -= (int1 + 1);
              value = valueStack[valueStackSize];
              objArray1 = (((ObjectInstance) value.internalValue)).nativeData;
              intArray1 = ((int[]) objArray1[0]);
              _len = ((int) objArray1[1]);
              if ((_len >= intArray1.length)) {
                intArray2 = new int[((_len * 2) + 16)];
                j = 0;
                while ((j < _len)) {
                  intArray2[j] = intArray1[j];
                  j += 1;
                }
                intArray1 = intArray2;
                objArray1[0] = intArray1;
              }
              objArray1[1] = (_len + 16);
              i = (int1 - 1);
              while ((i >= 0)) {
                value = valueStack[((valueStackSize + 1) + i)];
                if ((value.type == 3)) {
                  intArray1[(_len + i)] = value.intValue;
                } else {
                  if ((value.type == 4)) {
                    float1 = (0.5 + ((double) value.internalValue));
                    intArray1[(_len + i)] = ((int) float1);
                  } else {
                    hasInterrupt = EX_InvalidArgument(ec, "Input must be integers.");
                    i = -1;
                  }
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
              if ((row[1] == 1)) {
                if ((valueStackSize == valueStackCapacity)) {
                  valueStack = valueStackIncreaseCapacity(ec);
                  valueStackCapacity = valueStack.length;
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
              objInstance1 = ((ObjectInstance) arg1.internalValue);
              int1 = arg2.intValue;
              objArray1 = new Object[int1];
              objInstance1.nativeData = objArray1;
              break;
            case 35:
              // setNativeData;
              valueStackSize -= 3;
              arg3 = valueStack[(valueStackSize + 2)];
              arg2 = valueStack[(valueStackSize + 1)];
              arg1 = valueStack[valueStackSize];
              (((ObjectInstance) arg1.internalValue)).nativeData[arg2.intValue] = arg3.internalValue;
              break;
            case 36:
              // getExceptionTrace;
              arg1 = valueStack[--valueStackSize];
              intList1 = ((ArrayList<Integer>) getNativeDataItem(arg1, 1));
              list1 = makeEmptyList(globals.stringType, 20);
              output = new Value(6, list1);
              if ((intList1 != null)) {
                stringList1 = tokenHelperConvertPcsToStackTraceStrings(vm, intList1);
                i = 0;
                while ((i < stringList1.size())) {
                  addToList(list1, buildString(globals, stringList1.get(i)));
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
              if ((arg1.type != 8)) {
                hasInterrupt = EX_InvalidArgument(ec, "Cannot get class from non-instance types.");
              } else {
                objInstance1 = ((ObjectInstance) arg1.internalValue);
                output = new Value(10, new org.crayonlang.interpreter.structs.ClassValue(false, objInstance1.classId));
              }
              break;
            case 40:
              // convertFloatArgsToInts;
              int1 = stack.localsStackOffsetEnd;
              i = localsStackOffset;
              while ((i < int1)) {
                value = localsStack[i];
                if ((localsStackSet[i] != localsStackSetToken)) {
                  i += int1;
                } else {
                  if ((value.type == 4)) {
                    float1 = ((double) value.internalValue);
                    if (((float1 < 0) && ((float1 % 1) != 0))) {
                      int2 = (((int) float1) - 1);
                    } else {
                      int2 = ((int) float1);
                    }
                    if (((int2 >= 0) && (int2 < 2049))) {
                      localsStack[i] = INTEGER_POSITIVE_CACHE[int2];
                    } else {
                      localsStack[i] = buildInteger(globals, int2);
                    }
                  }
                }
                i += 1;
              }
              break;
            case 41:
              // addShutdownHandler;
              arg1 = valueStack[--valueStackSize];
              vm.shutdownHandlers.add(arg1);
              break;
          }
          if ((row[1] == 1)) {
            if ((valueStackSize == valueStackCapacity)) {
              valueStack = valueStackIncreaseCapacity(ec);
              valueStackCapacity = valueStack.length;
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
          intIntDict1 = new HashMap<Integer, Integer>();
          stringIntDict1 = new HashMap<String, Integer>();
          valueList2 = new ArrayList<Value>();
          valueList1 = new ArrayList<Value>();
          _len = row[0];
          type = 3;
          first = true;
          i = _len;
          while ((i > 0)) {
            valueStackSize -= 2;
            value = valueStack[(valueStackSize + 1)];
            value2 = valueStack[valueStackSize];
            if (first) {
              type = value2.type;
              first = false;
            } else {
              if ((type != value2.type)) {
                hasInterrupt = EX_InvalidKey(ec, "Dictionary keys must be of the same type.");
              }
            }
            if (!hasInterrupt) {
              if ((type == 3)) {
                intKey = value2.intValue;
              } else {
                if ((type == 5)) {
                  stringKey = ((String) value2.internalValue);
                } else {
                  if ((type == 8)) {
                    objInstance1 = ((ObjectInstance) value2.internalValue);
                    intKey = objInstance1.objectId;
                  } else {
                    hasInterrupt = EX_InvalidKey(ec, "Only integers, strings, and objects can be used as dictionary keys.");
                  }
                }
              }
            }
            if (!hasInterrupt) {
              if ((type == 5)) {
                stringIntDict1.put(stringKey, valueList1.size());
              } else {
                intIntDict1.put(intKey, valueList1.size());
              }
              valueList2.add(value2);
              valueList1.add(value);
              i -= 1;
            }
          }
          if (!hasInterrupt) {
            if ((type == 5)) {
              i = stringIntDict1.size();
            } else {
              i = intIntDict1.size();
            }
            if ((i != _len)) {
              hasInterrupt = EX_InvalidKey(ec, "Key collision");
            }
          }
          if (!hasInterrupt) {
            i = row[1];
            classId = 0;
            if ((i > 0)) {
              type = row[2];
              if ((type == 8)) {
                classId = row[3];
              }
              int1 = row.length;
              intArray1 = new int[(int1 - i)];
              while ((i < int1)) {
                intArray1[(i - row[1])] = row[i];
                i += 1;
              }
            } else {
              intArray1 = null;
            }
            if ((valueStackSize == valueStackCapacity)) {
              valueStack = valueStackIncreaseCapacity(ec);
              valueStackCapacity = valueStack.length;
            }
            valueStack[valueStackSize] = new Value(7, new DictImpl(_len, type, classId, intArray1, intIntDict1, stringIntDict1, valueList2, valueList1));
            valueStackSize += 1;
          }
          break;
        case 22:
          // DEF_LIST;
          int1 = row[0];
          list1 = makeEmptyList(null, int1);
          if ((row[1] != 0)) {
            list1.type = new int[(row.length - 1)];
            i = 1;
            while ((i < row.length)) {
              list1.type[(i - 1)] = row[i];
              i += 1;
            }
          }
          list1.size = int1;
          int2 = (valueStackSize - int1);
          i = 0;
          while ((i < int1)) {
            list1.array[i] = valueStack[(int2 + i)];
            i += 1;
          }
          valueStackSize -= int1;
          value = new Value(6, list1);
          if ((valueStackSize == valueStackCapacity)) {
            valueStack = valueStackIncreaseCapacity(ec);
            valueStackCapacity = valueStack.length;
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
          if (((closure != null) && closure.containsKey(i))) {
            value = closure.get(i).value;
            if ((value != null)) {
              bool1 = false;
              if ((valueStackSize == valueStackCapacity)) {
                valueStack = valueStackIncreaseCapacity(ec);
                valueStackCapacity = valueStack.length;
              }
              valueStack[valueStackSize++] = value;
            }
          }
          if (bool1) {
            hasInterrupt = EX_UnassignedVariable(ec, "Variable used before it was set.");
          }
          break;
        case 25:
          // DEREF_DOT;
          value = valueStack[(valueStackSize - 1)];
          nameId = row[0];
          int2 = row[1];
          switch (value.type) {
            case 8:
              objInstance1 = ((ObjectInstance) value.internalValue);
              classId = objInstance1.classId;
              classInfo = classTable[classId];
              if ((classId == row[4])) {
                int1 = row[5];
              } else {
                intIntDict1 = classInfo.localeScopedNameIdToMemberId;
                Integer dictLookup10 = intIntDict1.get(int2);
                int1 = dictLookup10 == null ? (-1) : dictLookup10;
                int3 = classInfo.fieldAccessModifiers[int1];
                if ((int3 > 1)) {
                  if ((int3 == 2)) {
                    if ((classId != row[2])) {
                      int1 = -2;
                    }
                  } else {
                    if (((int3 == 3) || (int3 == 5))) {
                      if ((classInfo.assemblyId != row[3])) {
                        int1 = -3;
                      }
                    }
                    if (((int3 == 4) || (int3 == 5))) {
                      i = row[2];
                      if ((classId == i)) {
                      } else {
                        classInfo = classTable[classInfo.id];
                        while (((classInfo.baseClassId != -1) && (int1 < classTable[classInfo.baseClassId].fieldAccessModifiers.length))) {
                          classInfo = classTable[classInfo.baseClassId];
                        }
                        j = classInfo.id;
                        if ((j != i)) {
                          bool1 = false;
                          while (((i != -1) && (classTable[i].baseClassId != -1))) {
                            i = classTable[i].baseClassId;
                            if ((i == j)) {
                              bool1 = true;
                              i = -1;
                            }
                          }
                          if (!bool1) {
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
              if ((int1 > -1)) {
                functionId = classInfo.functionIds[int1];
                if ((functionId == -1)) {
                  output = objInstance1.members[int1];
                } else {
                  output = new Value(9, new FunctionPointer(2, value, objInstance1.classId, functionId, null));
                }
              } else {
                output = null;
              }
              break;
            case 5:
              if ((metadata.lengthId == nameId)) {
                output = buildInteger(globals, (((String) value.internalValue)).length());
              } else {
                output = null;
              }
              break;
            case 6:
              if ((metadata.lengthId == nameId)) {
                output = buildInteger(globals, (((ListImpl) value.internalValue)).size);
              } else {
                output = null;
              }
              break;
            case 7:
              if ((metadata.lengthId == nameId)) {
                output = buildInteger(globals, (((DictImpl) value.internalValue)).size);
              } else {
                output = null;
              }
              break;
            default:
              if ((value.type == 1)) {
                hasInterrupt = EX_NullReference(ec, "Derferenced a field from null.");
                output = VALUE_NULL;
              } else {
                output = null;
              }
              break;
          }
          if ((output == null)) {
            output = generatePrimitiveMethodReference(globalNameIdToPrimitiveMethodName, nameId, value);
            if ((output == null)) {
              if ((value.type == 1)) {
                hasInterrupt = EX_NullReference(ec, "Tried to dereference a field on null.");
              } else {
                if (((value.type == 8) && (int1 < -1))) {
                  string1 = identifiers[row[0]];
                  if ((int1 == -2)) {
                    string2 = "private";
                  } else {
                    if ((int1 == -3)) {
                      string2 = "internal";
                    } else {
                      string2 = "protected";
                    }
                  }
                  hasInterrupt = EX_UnknownField(ec, "The field '" + string1 + "' is marked as " + string2 + " and cannot be accessed from here.");
                } else {
                  if ((value.type == 8)) {
                    classId = (((ObjectInstance) value.internalValue)).classId;
                    classInfo = classTable[classId];
                    string1 = classInfo.fullyQualifiedName + " instance";
                  } else {
                    string1 = getTypeFromId(value.type);
                  }
                  hasInterrupt = EX_UnknownField(ec, string1 + " does not have that field.");
                }
              }
            }
          }
          valueStack[(valueStackSize - 1)] = output;
          break;
        case 26:
          // DEREF_INSTANCE_FIELD;
          value = stack.objectContext;
          objInstance1 = ((ObjectInstance) value.internalValue);
          value = objInstance1.members[row[0]];
          if ((valueStackSize == valueStackCapacity)) {
            valueStack = valueStackIncreaseCapacity(ec);
            valueStackCapacity = valueStack.length;
          }
          valueStack[valueStackSize++] = value;
          break;
        case 27:
          // DEREF_STATIC_FIELD;
          classInfo = classTable[row[0]];
          staticConstructorNotInvoked = true;
          if ((classInfo.staticInitializationState < 2)) {
            stack.pc = pc;
            stackFrame2 = maybeInvokeStaticConstructor(vm, ec, stack, classInfo, valueStackSize, PST_intBuffer16);
            if ((PST_intBuffer16[0] == 1)) {
              return generateException(vm, stack, pc, valueStackSize, ec, 0, "Static initialization loop detected. The class this field is a member of is not done being initialized.");
            }
            if ((stackFrame2 != null)) {
              staticConstructorNotInvoked = false;
              stack = stackFrame2;
              pc = stack.pc;
              localsStackSetToken = stack.localsStackSetToken;
              localsStackOffset = stack.localsStackOffset;
            }
          }
          if (staticConstructorNotInvoked) {
            if ((valueStackSize == valueStackCapacity)) {
              valueStack = valueStackIncreaseCapacity(ec);
              valueStackCapacity = valueStack.length;
            }
            valueStack[valueStackSize++] = classInfo.staticFields[row[1]];
          }
          break;
        case 28:
          // DUPLICATE_STACK_TOP;
          if ((row[0] == 1)) {
            value = valueStack[(valueStackSize - 1)];
            if ((valueStackSize == valueStackCapacity)) {
              valueStack = valueStackIncreaseCapacity(ec);
              valueStackCapacity = valueStack.length;
            }
            valueStack[valueStackSize++] = value;
          } else {
            if ((row[0] == 2)) {
              if (((valueStackSize + 1) > valueStackCapacity)) {
                valueStackIncreaseCapacity(ec);
                valueStack = ec.valueStack;
                valueStackCapacity = valueStack.length;
              }
              valueStack[valueStackSize] = valueStack[(valueStackSize - 2)];
              valueStack[(valueStackSize + 1)] = valueStack[(valueStackSize - 1)];
              valueStackSize += 2;
            } else {
              hasInterrupt = EX_Fatal(ec, "?");
            }
          }
          break;
        case 29:
          // EQUALS;
          valueStackSize -= 2;
          rightValue = valueStack[(valueStackSize + 1)];
          leftValue = valueStack[valueStackSize];
          if ((leftValue.type == rightValue.type)) {
            switch (leftValue.type) {
              case 1:
                bool1 = true;
                break;
              case 2:
                bool1 = ((leftValue.intValue == 1) == (rightValue.intValue == 1));
                break;
              case 3:
                bool1 = (leftValue.intValue == rightValue.intValue);
                break;
              case 5:
                bool1 = (((String) leftValue.internalValue) == ((String) rightValue.internalValue));
                break;
              default:
                bool1 = (doEqualityComparisonAndReturnCode(leftValue, rightValue) == 1);
                break;
            }
          } else {
            int1 = doEqualityComparisonAndReturnCode(leftValue, rightValue);
            if ((int1 == 0)) {
              bool1 = false;
            } else {
              if ((int1 == 1)) {
                bool1 = true;
              } else {
                hasInterrupt = EX_UnsupportedOperation(ec, "== and != not defined here.");
              }
            }
          }
          if ((valueStackSize == valueStackCapacity)) {
            valueStack = valueStackIncreaseCapacity(ec);
            valueStackCapacity = valueStack.length;
          }
          if ((bool1 != ((row[0] == 1)))) {
            valueStack[valueStackSize] = VALUE_TRUE;
          } else {
            valueStack[valueStackSize] = VALUE_FALSE;
          }
          valueStackSize += 1;
          break;
        case 30:
          // ESF_LOOKUP;
          esfData = generateEsfData(args.length, row);
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
          if (((value == null) || ec.activeExceptionHandled)) {
            switch (stack.postFinallyBehavior) {
              case 0:
                ec.activeException = null;
                break;
              case 1:
                ec.activeException = null;
                int1 = row[0];
                if ((int1 == 1)) {
                  pc += row[1];
                } else {
                  if ((int1 == 2)) {
                    intArray1 = esfData[pc];
                    pc = intArray1[1];
                  } else {
                    hasInterrupt = EX_Fatal(ec, "break exists without a loop");
                  }
                }
                break;
              case 2:
                ec.activeException = null;
                int1 = row[2];
                if ((int1 == 1)) {
                  pc += row[3];
                } else {
                  if ((int1 == 2)) {
                    intArray1 = esfData[pc];
                    pc = intArray1[1];
                  } else {
                    hasInterrupt = EX_Fatal(ec, "continue exists without a loop");
                  }
                }
                break;
              case 3:
                if ((stack.markClassAsInitialized != 0)) {
                  markClassAsInitialized(vm, stack, stack.markClassAsInitialized);
                }
                if (stack.returnValueUsed) {
                  valueStackSize = stack.valueStackPopSize;
                  value = stack.returnValueTempStorage;
                  stack = stack.previous;
                  if ((valueStackSize == valueStackCapacity)) {
                    valueStack = valueStackIncreaseCapacity(ec);
                    valueStackCapacity = valueStack.length;
                  }
                  valueStack[valueStackSize] = value;
                  valueStackSize += 1;
                } else {
                  valueStackSize = stack.valueStackPopSize;
                  stack = stack.previous;
                }
                pc = stack.pc;
                localsStackOffset = stack.localsStackOffset;
                localsStackSetToken = stack.localsStackSetToken;
                break;
            }
          } else {
            ec.activeExceptionHandled = false;
            stack.pc = pc;
            intArray1 = esfData[pc];
            value = ec.activeException;
            objInstance1 = ((ObjectInstance) value.internalValue);
            objArray1 = objInstance1.nativeData;
            bool1 = true;
            if ((objArray1[0] != null)) {
              bool1 = ((boolean) objArray1[0]);
            }
            intList1 = ((ArrayList<Integer>) objArray1[1]);
            while (((stack != null) && ((intArray1 == null) || bool1))) {
              stack = stack.previous;
              if ((stack != null)) {
                pc = stack.pc;
                intList1.add(pc);
                intArray1 = esfData[pc];
              }
            }
            if ((stack == null)) {
              return uncaughtExceptionResult(vm, value);
            }
            int1 = intArray1[0];
            if ((int1 < pc)) {
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
          if ((root.type == 6)) {
            if ((value.type != 3)) {
              hasInterrupt = EX_InvalidArgument(ec, "List index must be an integer.");
            } else {
              i = value.intValue;
              list1 = ((ListImpl) root.internalValue);
              if ((i < 0)) {
                i += list1.size;
              }
              if (((i < 0) || (i >= list1.size))) {
                hasInterrupt = EX_IndexOutOfRange(ec, "List index is out of bounds");
              } else {
                valueStack[(valueStackSize - 1)] = list1.array[i];
              }
            }
          } else {
            if ((root.type == 7)) {
              dictImpl = ((DictImpl) root.internalValue);
              keyType = value.type;
              if ((keyType != dictImpl.keyType)) {
                if ((dictImpl.size == 0)) {
                  hasInterrupt = EX_KeyNotFound(ec, "Key not found. Dictionary is empty.");
                } else {
                  hasInterrupt = EX_InvalidKey(ec, "Incorrect key type. This dictionary contains " + getTypeFromId(dictImpl.keyType) + " keys. Provided key is a " + getTypeFromId(keyType) + ".");
                }
              } else {
                if ((keyType == 3)) {
                  intKey = value.intValue;
                } else {
                  if ((keyType == 5)) {
                    stringKey = ((String) value.internalValue);
                  } else {
                    if ((keyType == 8)) {
                      intKey = (((ObjectInstance) value.internalValue)).objectId;
                    } else {
                      if ((dictImpl.size == 0)) {
                        hasInterrupt = EX_KeyNotFound(ec, "Key not found. Dictionary is empty.");
                      } else {
                        hasInterrupt = EX_KeyNotFound(ec, "Key not found.");
                      }
                    }
                  }
                }
                if (!hasInterrupt) {
                  if ((keyType == 5)) {
                    stringIntDict1 = ((HashMap<String, Integer>) dictImpl.stringToIndex);
                    Integer dictLookup11 = stringIntDict1.get(stringKey);
                    int1 = dictLookup11 == null ? (stringIntDict1.containsKey(stringKey) ? null : (-1)) : dictLookup11;
                    if ((int1 == -1)) {
                      hasInterrupt = EX_KeyNotFound(ec, "Key not found: '" + stringKey + "'");
                    } else {
                      valueStack[(valueStackSize - 1)] = dictImpl.values.get(int1);
                    }
                  } else {
                    intIntDict1 = ((HashMap<Integer, Integer>) dictImpl.intToIndex);
                    Integer dictLookup12 = intIntDict1.get(intKey);
                    int1 = dictLookup12 == null ? (-1) : dictLookup12;
                    if ((int1 == -1)) {
                      hasInterrupt = EX_KeyNotFound(ec, "Key not found.");
                    } else {
                      valueStack[(valueStackSize - 1)] = dictImpl.values.get(intIntDict1.get(intKey));
                    }
                  }
                }
              }
            } else {
              if ((root.type == 5)) {
                string1 = ((String) root.internalValue);
                if ((value.type != 3)) {
                  hasInterrupt = EX_InvalidArgument(ec, "String indices must be integers.");
                } else {
                  int1 = value.intValue;
                  if ((int1 < 0)) {
                    int1 += string1.length();
                  }
                  if (((int1 < 0) || (int1 >= string1.length()))) {
                    hasInterrupt = EX_IndexOutOfRange(ec, "String index out of range.");
                  } else {
                    valueStack[(valueStackSize - 1)] = buildCommonString(globals, ("" + string1.charAt(int1)));
                  }
                }
              } else {
                hasInterrupt = EX_InvalidArgument(ec, "Cannot index into this type: " + getTypeFromId(root.type));
              }
            }
          }
          break;
        case 37:
          // IS_COMPARISON;
          value = valueStack[(valueStackSize - 1)];
          output = VALUE_FALSE;
          if ((value.type == 8)) {
            objInstance1 = ((ObjectInstance) value.internalValue);
            if (isClassASubclassOf(vm, objInstance1.classId, row[0])) {
              output = VALUE_TRUE;
            }
          }
          valueStack[(valueStackSize - 1)] = output;
          break;
        case 38:
          // ITERATION_STEP;
          int1 = (localsStackOffset + row[2]);
          value3 = localsStack[int1];
          i = value3.intValue;
          value = localsStack[(localsStackOffset + row[3])];
          if ((value.type == 6)) {
            list1 = ((ListImpl) value.internalValue);
            _len = list1.size;
            bool1 = true;
          } else {
            string2 = ((String) value.internalValue);
            _len = string2.length();
            bool1 = false;
          }
          if ((i < _len)) {
            if (bool1) {
              value = list1.array[i];
            } else {
              value = buildCommonString(globals, ("" + string2.charAt(i)));
            }
            int3 = (localsStackOffset + row[1]);
            localsStackSet[int3] = localsStackSetToken;
            localsStack[int3] = value;
          } else {
            pc += row[0];
          }
          i += 1;
          if ((i < 2049)) {
            localsStack[int1] = INTEGER_POSITIVE_CACHE[i];
          } else {
            localsStack[int1] = new Value(i);
          }
          break;
        case 39:
          // JUMP;
          pc += row[0];
          break;
        case 40:
          // JUMP_IF_EXCEPTION_OF_TYPE;
          value = ec.activeException;
          objInstance1 = ((ObjectInstance) value.internalValue);
          int1 = objInstance1.classId;
          i = (row.length - 1);
          while ((i >= 2)) {
            if (isClassASubclassOf(vm, int1, row[i])) {
              i = 0;
              pc += row[0];
              int2 = row[1];
              if ((int2 > -1)) {
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
          if ((value.type != 2)) {
            hasInterrupt = EX_InvalidArgument(ec, "Boolean expected.");
          } else {
            if (!(value.intValue == 1)) {
              pc += row[0];
            }
          }
          break;
        case 42:
          // JUMP_IF_FALSE_NON_POP;
          value = valueStack[(valueStackSize - 1)];
          if ((value.type != 2)) {
            hasInterrupt = EX_InvalidArgument(ec, "Boolean expected.");
          } else {
            if ((value.intValue == 1)) {
              valueStackSize -= 1;
            } else {
              pc += row[0];
            }
          }
          break;
        case 43:
          // JUMP_IF_TRUE;
          value = valueStack[--valueStackSize];
          if ((value.type != 2)) {
            hasInterrupt = EX_InvalidArgument(ec, "Boolean expected.");
          } else {
            if ((value.intValue == 1)) {
              pc += row[0];
            }
          }
          break;
        case 44:
          // JUMP_IF_TRUE_NO_POP;
          value = valueStack[(valueStackSize - 1)];
          if ((value.type != 2)) {
            hasInterrupt = EX_InvalidArgument(ec, "Boolean expected.");
          } else {
            if ((value.intValue == 1)) {
              pc += row[0];
            } else {
              valueStackSize -= 1;
            }
          }
          break;
        case 45:
          // LAMBDA;
          if (!metadata.lambdaTable.containsKey(pc)) {
            int1 = (4 + row[4] + 1);
            _len = row[int1];
            intArray1 = new int[_len];
            i = 0;
            while ((i < _len)) {
              intArray1[i] = row[(int1 + i + 1)];
              i += 1;
            }
            _len = row[4];
            intArray2 = new int[_len];
            i = 0;
            while ((i < _len)) {
              intArray2[i] = row[(5 + i)];
              i += 1;
            }
            metadata.lambdaTable.put(pc, new FunctionInfo(pc, 0, pc, row[0], row[1], 5, 0, row[2], intArray2, "lambda", intArray1));
          }
          closure = new HashMap<Integer, ClosureValuePointer>();
          parentClosure = stack.closureVariables;
          if ((parentClosure == null)) {
            parentClosure = new HashMap<Integer, ClosureValuePointer>();
            stack.closureVariables = parentClosure;
          }
          functionInfo = metadata.lambdaTable.get(pc);
          intArray1 = functionInfo.closureIds;
          _len = intArray1.length;
          i = 0;
          while ((i < _len)) {
            j = intArray1[i];
            if (parentClosure.containsKey(j)) {
              closure.put(j, parentClosure.get(j));
            } else {
              closure.put(j, new ClosureValuePointer(null));
              parentClosure.put(j, closure.get(j));
            }
            i += 1;
          }
          if ((valueStackSize == valueStackCapacity)) {
            valueStack = valueStackIncreaseCapacity(ec);
            valueStackCapacity = valueStack.length;
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
          if ((row[2] == 1)) {
            valueStackSize -= 1;
            arg3 = valueStack[valueStackSize];
          } else {
            arg3 = null;
          }
          if ((row[1] == 1)) {
            valueStackSize -= 1;
            arg2 = valueStack[valueStackSize];
          } else {
            arg2 = null;
          }
          if ((row[0] == 1)) {
            valueStackSize -= 1;
            arg1 = valueStack[valueStackSize];
          } else {
            arg1 = null;
          }
          value = valueStack[(valueStackSize - 1)];
          value = performListSlice(globals, ec, value, arg1, arg2, arg3);
          hasInterrupt = (ec.activeInterrupt != null);
          if (!hasInterrupt) {
            valueStack[(valueStackSize - 1)] = value;
          }
          break;
        case 48:
          // LITERAL;
          if ((valueStackSize == valueStackCapacity)) {
            valueStack = valueStackIncreaseCapacity(ec);
            valueStackCapacity = valueStack.length;
          }
          valueStack[valueStackSize++] = literalTable[row[0]];
          break;
        case 49:
          // LITERAL_STREAM;
          int1 = row.length;
          if (((valueStackSize + int1) > valueStackCapacity)) {
            while (((valueStackSize + int1) > valueStackCapacity)) {
              valueStackIncreaseCapacity(ec);
              valueStack = ec.valueStack;
              valueStackCapacity = valueStack.length;
            }
          }
          i = int1;
          while ((--i >= 0)) {
            valueStack[valueStackSize++] = literalTable[row[i]];
          }
          break;
        case 50:
          // LOCAL;
          int1 = (localsStackOffset + row[0]);
          if ((localsStackSet[int1] == localsStackSetToken)) {
            if ((valueStackSize == valueStackCapacity)) {
              valueStack = valueStackIncreaseCapacity(ec);
              valueStackCapacity = valueStack.length;
            }
            valueStack[valueStackSize++] = localsStack[int1];
          } else {
            hasInterrupt = EX_UnassignedVariable(ec, "Variable used before it was set.");
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
          if ((type == 3)) {
            valueStack[(valueStackSize - 1)] = buildInteger(globals, -value.intValue);
          } else {
            if ((type == 4)) {
              valueStack[(valueStackSize - 1)] = buildFloat(globals, -((double) value.internalValue));
            } else {
              hasInterrupt = EX_InvalidArgument(ec, "Negative sign can only be applied to numbers. Found " + getTypeFromId(type) + " instead.");
            }
          }
          break;
        case 53:
          // POP;
          valueStackSize -= 1;
          break;
        case 54:
          // POP_IF_NULL_OR_JUMP;
          value = valueStack[(valueStackSize - 1)];
          if ((value.type == 1)) {
            valueStackSize -= 1;
          } else {
            pc += row[0];
          }
          break;
        case 55:
          // PUSH_FUNC_REF;
          value = null;
          switch (row[1]) {
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
              if ((classInfo.staticInitializationState < 2)) {
                stack.pc = pc;
                stackFrame2 = maybeInvokeStaticConstructor(vm, ec, stack, classInfo, valueStackSize, PST_intBuffer16);
                if ((PST_intBuffer16[0] == 1)) {
                  return generateException(vm, stack, pc, valueStackSize, ec, 0, "Static initialization loop detected. The class this field is a member of is not done being initialized.");
                }
                if ((stackFrame2 != null)) {
                  staticConstructorNotInvoked = false;
                  stack = stackFrame2;
                  pc = stack.pc;
                  localsStackSetToken = stack.localsStackSetToken;
                  localsStackOffset = stack.localsStackOffset;
                }
              }
              if (staticConstructorNotInvoked) {
                value = new Value(9, new FunctionPointer(3, null, classId, row[0], null));
              } else {
                value = null;
              }
              break;
          }
          if ((value != null)) {
            if ((valueStackSize == valueStackCapacity)) {
              valueStack = valueStackIncreaseCapacity(ec);
              valueStackCapacity = valueStack.length;
            }
            valueStack[valueStackSize] = value;
            valueStackSize += 1;
          }
          break;
        case 56:
          // RETURN;
          if ((esfData[pc] != null)) {
            intArray1 = esfData[pc];
            pc = (intArray1[1] - 1);
            if ((row[0] == 0)) {
              stack.returnValueTempStorage = VALUE_NULL;
            } else {
              stack.returnValueTempStorage = valueStack[(valueStackSize - 1)];
            }
            valueStackSize = stack.valueStackPopSize;
            stack.postFinallyBehavior = 3;
          } else {
            if ((stack.previous == null)) {
              return interpreterFinished(vm, ec);
            }
            if ((stack.markClassAsInitialized != 0)) {
              markClassAsInitialized(vm, stack, stack.markClassAsInitialized);
            }
            if (stack.returnValueUsed) {
              if ((row[0] == 0)) {
                valueStackSize = stack.valueStackPopSize;
                stack = stack.previous;
                if ((valueStackSize == valueStackCapacity)) {
                  valueStack = valueStackIncreaseCapacity(ec);
                  valueStackCapacity = valueStack.length;
                }
                valueStack[valueStackSize] = VALUE_NULL;
              } else {
                value = valueStack[(valueStackSize - 1)];
                valueStackSize = stack.valueStackPopSize;
                stack = stack.previous;
                valueStack[valueStackSize] = value;
              }
              valueStackSize += 1;
            } else {
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
          if ((valueStackSize == valueStackCapacity)) {
            valueStack = valueStackIncreaseCapacity(ec);
            valueStackCapacity = valueStack.length;
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
          if ((value.type == 3)) {
            intKey = value.intValue;
            integerSwitch = integerSwitchesByPc[pc];
            if ((integerSwitch == null)) {
              integerSwitch = initializeIntSwitchStatement(vm, pc, row);
            }
            Integer dictLookup13 = integerSwitch.get(intKey);
            i = dictLookup13 == null ? (-1) : dictLookup13;
            if ((i == -1)) {
              pc += row[0];
            } else {
              pc += i;
            }
          } else {
            hasInterrupt = EX_InvalidArgument(ec, "Switch statement expects an integer.");
          }
          break;
        case 60:
          // SWITCH_STRING;
          value = valueStack[--valueStackSize];
          if ((value.type == 5)) {
            stringKey = ((String) value.internalValue);
            stringSwitch = stringSwitchesByPc[pc];
            if ((stringSwitch == null)) {
              stringSwitch = initializeStringSwitchStatement(vm, pc, row);
            }
            Integer dictLookup14 = stringSwitch.get(stringKey);
            i = dictLookup14 == null ? (stringSwitch.containsKey(stringKey) ? null : (-1)) : dictLookup14;
            if ((i == -1)) {
              pc += row[0];
            } else {
              pc += i;
            }
          } else {
            hasInterrupt = EX_InvalidArgument(ec, "Switch statement expects a string.");
          }
          break;
        case 61:
          // THIS;
          if ((valueStackSize == valueStackCapacity)) {
            valueStack = valueStackIncreaseCapacity(ec);
            valueStackCapacity = valueStack.length;
          }
          valueStack[valueStackSize] = stack.objectContext;
          valueStackSize += 1;
          break;
        case 62:
          // THROW;
          valueStackSize -= 1;
          value = valueStack[valueStackSize];
          bool2 = (value.type == 8);
          if (bool2) {
            objInstance1 = ((ObjectInstance) value.internalValue);
            if (!isClassASubclassOf(vm, objInstance1.classId, magicNumbers.coreExceptionClassId)) {
              bool2 = false;
            }
          }
          if (bool2) {
            objArray1 = objInstance1.nativeData;
            intList1 = new ArrayList<Integer>();
            objArray1[1] = intList1;
            if (!isPcFromCore(vm, pc)) {
              intList1.add(pc);
            }
            ec.activeException = value;
            ec.activeExceptionHandled = false;
            stack.pc = pc;
            intArray1 = esfData[pc];
            value = ec.activeException;
            objInstance1 = ((ObjectInstance) value.internalValue);
            objArray1 = objInstance1.nativeData;
            bool1 = true;
            if ((objArray1[0] != null)) {
              bool1 = ((boolean) objArray1[0]);
            }
            intList1 = ((ArrayList<Integer>) objArray1[1]);
            while (((stack != null) && ((intArray1 == null) || bool1))) {
              stack = stack.previous;
              if ((stack != null)) {
                pc = stack.pc;
                intList1.add(pc);
                intArray1 = esfData[pc];
              }
            }
            if ((stack == null)) {
              return uncaughtExceptionResult(vm, value);
            }
            int1 = intArray1[0];
            if ((int1 < pc)) {
              int1 = intArray1[1];
            }
            pc = (int1 - 1);
            stack.pc = pc;
            localsStackOffset = stack.localsStackOffset;
            localsStackSetToken = stack.localsStackSetToken;
            ec.stackTop = stack;
            stack.postFinallyBehavior = 0;
            ec.currentValueStackSize = valueStackSize;
          } else {
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
          if (((int1 != 6) && (int1 != 5))) {
            hasInterrupt = EX_InvalidArgument(ec, "Expected an iterable type, such as a list or string. Found " + getTypeFromId(int1) + " instead.");
          }
          i = (localsStackOffset + row[1]);
          localsStack[i] = VALUE_INT_ZERO;
          localsStackSet[i] = localsStackSetToken;
          break;
        default:
          // THIS SHOULD NEVER HAPPEN;
          return generateException(vm, stack, pc, valueStackSize, ec, 0, "Bad op code: " + Integer.toString(ops[pc]));
      }
      if (hasInterrupt) {
        Interrupt interrupt = ec.activeInterrupt;
        ec.activeInterrupt = null;
        if ((interrupt.type == 1)) {
          return generateException(vm, stack, pc, valueStackSize, ec, interrupt.exceptionType, interrupt.exceptionMessage);
        }
        if ((interrupt.type == 3)) {
          return new InterpreterResult(5, "", interrupt.sleepDurationSeconds, 0, false, "");
        }
        if ((interrupt.type == 4)) {
          return new InterpreterResult(6, "", 0.0, 0, false, interrupt.exceptionMessage);
        }
      }
      ++pc;
    }
  }

  public static Object invokeNamedCallback(VmContext vm, int id, Object[] args) {
    java.lang.reflect.Method cb = vm.namedCallbacks.callbacksById.get(id);
    return ((Object) TranslationHelper.invokeFunctionPointer(cb, new Object[] {args}));
  }

  public static boolean isClassASubclassOf(VmContext vm, int subClassId, int parentClassId) {
    if ((subClassId == parentClassId)) {
      return true;
    }
    ClassInfo[] classTable = vm.metadata.classTable;
    int classIdWalker = subClassId;
    while ((classIdWalker != -1)) {
      if ((classIdWalker == parentClassId)) {
        return true;
      }
      ClassInfo classInfo = classTable[classIdWalker];
      classIdWalker = classInfo.baseClassId;
    }
    return false;
  }

  public static boolean isPcFromCore(VmContext vm, int pc) {
    if ((vm.symbolData == null)) {
      return false;
    }
    ArrayList<Token> tokens = vm.symbolData.tokenData[pc];
    if ((tokens == null)) {
      return false;
    }
    Token token = tokens.get(0);
    String filename = tokenHelperGetFileLine(vm, token.fileId, 0);
    return "[Core]".equals(filename);
  }

  public static boolean isStringEqual(String a, String b) {
    if ((((String) a) == ((String) b))) {
      return true;
    }
    return false;
  }

  public static boolean isVmResultRootExecContext(InterpreterResult result) {
    return result.isRootContext;
  }

  public static ListImpl makeEmptyList(int[] type, int capacity) {
    return new ListImpl(type, 0, capacity, new Value[capacity]);
  }

  public static int markClassAsInitialized(VmContext vm, StackFrame stack, int classId) {
    ClassInfo classInfo = vm.metadata.classTable[stack.markClassAsInitialized];
    classInfo.staticInitializationState = 2;
    vm.classStaticInitializationStack.remove(vm.classStaticInitializationStack.size() - 1);
    return 0;
  }

  public static StackFrame maybeInvokeStaticConstructor(VmContext vm, ExecutionContext ec, StackFrame stack, ClassInfo classInfo, int valueStackSize, int[] intOutParam) {
    PST_intBuffer16[0] = 0;
    int classId = classInfo.id;
    if ((classInfo.staticInitializationState == 1)) {
      ArrayList<Integer> classIdsBeingInitialized = vm.classStaticInitializationStack;
      if ((classIdsBeingInitialized.get((classIdsBeingInitialized.size() - 1)) != classId)) {
        PST_intBuffer16[0] = 1;
      }
      return null;
    }
    classInfo.staticInitializationState = 1;
    vm.classStaticInitializationStack.add(classId);
    FunctionInfo functionInfo = vm.metadata.functionTable[classInfo.staticConstructorFunctionId];
    stack.pc -= 1;
    int newFrameLocalsSize = functionInfo.localsSize;
    int currentFrameLocalsEnd = stack.localsStackOffsetEnd;
    if ((ec.localsStack.length <= (currentFrameLocalsEnd + newFrameLocalsSize))) {
      increaseLocalsStackCapacity(ec, newFrameLocalsSize);
    }
    if ((ec.localsStackSetToken > 2000000000)) {
      resetLocalsStackTokens(ec, stack);
    }
    ec.localsStackSetToken += 1;
    return new StackFrame(functionInfo.pc, ec.localsStackSetToken, currentFrameLocalsEnd, (currentFrameLocalsEnd + newFrameLocalsSize), stack, false, null, valueStackSize, classId, (stack.depth + 1), 0, null, null, null);
  }

  public static Value multiplyString(VmGlobals globals, Value strValue, String str, int n) {
    if ((n <= 2)) {
      if ((n == 1)) {
        return strValue;
      }
      if ((n == 2)) {
        return buildString(globals, str + str);
      }
      return globals.stringEmpty;
    }
    ArrayList<String> builder = new ArrayList<String>();
    while ((n > 0)) {
      n -= 1;
      builder.add(str);
    }
    str = PST_joinList("", builder);
    return buildString(globals, str);
  }

  public static int nextPowerOf2(int value) {
    if ((((value - 1) & value) == 0)) {
      return value;
    }
    int output = 1;
    while ((output < value)) {
      output *= 2;
    }
    return output;
  }

  public static int noop() {
    return 0;
  }

  public static Value performListSlice(VmGlobals globals, ExecutionContext ec, Value value, Value arg1, Value arg2, Value arg3) {
    int begin = 0;
    int end = 0;
    int step = 0;
    int length = 0;
    int i = 0;
    boolean isForward = false;
    boolean isString = false;
    String originalString = "";
    ListImpl originalList = null;
    ListImpl outputList = null;
    ArrayList<String> outputString = null;
    int status = 0;
    if ((arg3 != null)) {
      if ((arg3.type == 3)) {
        step = arg3.intValue;
        if ((step == 0)) {
          status = 2;
        }
      } else {
        status = 3;
        step = 1;
      }
    } else {
      step = 1;
    }
    isForward = (step > 0);
    if ((arg2 != null)) {
      if ((arg2.type == 3)) {
        end = arg2.intValue;
      } else {
        status = 3;
      }
    }
    if ((arg1 != null)) {
      if ((arg1.type == 3)) {
        begin = arg1.intValue;
      } else {
        status = 3;
      }
    }
    if ((value.type == 5)) {
      isString = true;
      originalString = ((String) value.internalValue);
      length = originalString.length();
    } else {
      if ((value.type == 6)) {
        isString = false;
        originalList = ((ListImpl) value.internalValue);
        length = originalList.size;
      } else {
        EX_InvalidArgument(ec, "Cannot apply slicing to " + getTypeFromId(value.type) + ". Must be string or list.");
        return globals.valueNull;
      }
    }
    if ((status >= 2)) {
      String msg = null;
      if (isString) {
        msg = "String";
      } else {
        msg = "List";
      }
      if ((status == 3)) {
        msg += " slice indexes must be integers. Found ";
        if (((arg1 != null) && (arg1.type != 3))) {
          EX_InvalidArgument(ec, msg + getTypeFromId(arg1.type) + " for begin index.");
          return globals.valueNull;
        }
        if (((arg2 != null) && (arg2.type != 3))) {
          EX_InvalidArgument(ec, msg + getTypeFromId(arg2.type) + " for end index.");
          return globals.valueNull;
        }
        if (((arg3 != null) && (arg3.type != 3))) {
          EX_InvalidArgument(ec, msg + getTypeFromId(arg3.type) + " for step amount.");
          return globals.valueNull;
        }
        EX_InvalidArgument(ec, "Invalid slice arguments.");
        return globals.valueNull;
      } else {
        EX_InvalidArgument(ec, msg + " slice step cannot be 0.");
        return globals.valueNull;
      }
    }
    status = canonicalizeListSliceArgs(PST_intBuffer16, arg1, arg2, begin, end, step, length, isForward);
    if ((status == 1)) {
      begin = PST_intBuffer16[0];
      end = PST_intBuffer16[1];
      if (isString) {
        outputString = new ArrayList<String>();
        if (isForward) {
          if ((step == 1)) {
            return buildString(globals, originalString.substring(begin, begin + (end - begin)));
          } else {
            while ((begin < end)) {
              outputString.add(("" + originalString.charAt(begin)));
              begin += step;
            }
          }
        } else {
          while ((begin > end)) {
            outputString.add(("" + originalString.charAt(begin)));
            begin += step;
          }
        }
        value = buildString(globals, PST_joinList("", outputString));
      } else {
        outputList = makeEmptyList(originalList.type, 10);
        if (isForward) {
          while ((begin < end)) {
            addToList(outputList, originalList.array[begin]);
            begin += step;
          }
        } else {
          while ((begin > end)) {
            addToList(outputList, originalList.array[begin]);
            begin += step;
          }
        }
        value = new Value(6, outputList);
      }
    } else {
      if ((status == 0)) {
        if (isString) {
          value = globals.stringEmpty;
        } else {
          value = new Value(6, makeEmptyList(originalList.type, 0));
        }
      } else {
        if ((status == 2)) {
          if (!isString) {
            outputList = makeEmptyList(originalList.type, length);
            i = 0;
            while ((i < length)) {
              addToList(outputList, originalList.array[i]);
              i += 1;
            }
            value = new Value(6, outputList);
          }
        } else {
          String msg = null;
          if (isString) {
            msg = "String";
          } else {
            msg = "List";
          }
          if ((status == 3)) {
            msg += " slice begin index is out of range.";
          } else {
            if (isForward) {
              msg += " slice begin index must occur before the end index when step is positive.";
            } else {
              msg += " slice begin index must occur after the end index when the step is negative.";
            }
          }
          EX_IndexOutOfRange(ec, msg);
          return globals.valueNull;
        }
      }
    }
    return value;
  }

  public static int prepareToSuspend(ExecutionContext ec, StackFrame stack, int valueStackSize, int currentPc) {
    ec.stackTop = stack;
    ec.currentValueStackSize = valueStackSize;
    stack.pc = (currentPc + 1);
    return 0;
  }

  public static int[] primitiveMethodsInitializeLookup(HashMap<String, Integer> nameLookups) {
    int length = nameLookups.size();
    int[] lookup = new int[length];
    int i = 0;
    while ((i < length)) {
      lookup[i] = -1;
      i += 1;
    }
    if (nameLookups.containsKey("add")) {
      lookup[nameLookups.get("add")] = 0;
    }
    if (nameLookups.containsKey("argCountMax")) {
      lookup[nameLookups.get("argCountMax")] = 1;
    }
    if (nameLookups.containsKey("argCountMin")) {
      lookup[nameLookups.get("argCountMin")] = 2;
    }
    if (nameLookups.containsKey("choice")) {
      lookup[nameLookups.get("choice")] = 3;
    }
    if (nameLookups.containsKey("clear")) {
      lookup[nameLookups.get("clear")] = 4;
    }
    if (nameLookups.containsKey("clone")) {
      lookup[nameLookups.get("clone")] = 5;
    }
    if (nameLookups.containsKey("concat")) {
      lookup[nameLookups.get("concat")] = 6;
    }
    if (nameLookups.containsKey("contains")) {
      lookup[nameLookups.get("contains")] = 7;
    }
    if (nameLookups.containsKey("createInstance")) {
      lookup[nameLookups.get("createInstance")] = 8;
    }
    if (nameLookups.containsKey("endsWith")) {
      lookup[nameLookups.get("endsWith")] = 9;
    }
    if (nameLookups.containsKey("filter")) {
      lookup[nameLookups.get("filter")] = 10;
    }
    if (nameLookups.containsKey("get")) {
      lookup[nameLookups.get("get")] = 11;
    }
    if (nameLookups.containsKey("getName")) {
      lookup[nameLookups.get("getName")] = 12;
    }
    if (nameLookups.containsKey("indexOf")) {
      lookup[nameLookups.get("indexOf")] = 13;
    }
    if (nameLookups.containsKey("insert")) {
      lookup[nameLookups.get("insert")] = 14;
    }
    if (nameLookups.containsKey("invoke")) {
      lookup[nameLookups.get("invoke")] = 15;
    }
    if (nameLookups.containsKey("isA")) {
      lookup[nameLookups.get("isA")] = 16;
    }
    if (nameLookups.containsKey("join")) {
      lookup[nameLookups.get("join")] = 17;
    }
    if (nameLookups.containsKey("keys")) {
      lookup[nameLookups.get("keys")] = 18;
    }
    if (nameLookups.containsKey("lower")) {
      lookup[nameLookups.get("lower")] = 19;
    }
    if (nameLookups.containsKey("ltrim")) {
      lookup[nameLookups.get("ltrim")] = 20;
    }
    if (nameLookups.containsKey("map")) {
      lookup[nameLookups.get("map")] = 21;
    }
    if (nameLookups.containsKey("merge")) {
      lookup[nameLookups.get("merge")] = 22;
    }
    if (nameLookups.containsKey("pop")) {
      lookup[nameLookups.get("pop")] = 23;
    }
    if (nameLookups.containsKey("remove")) {
      lookup[nameLookups.get("remove")] = 24;
    }
    if (nameLookups.containsKey("replace")) {
      lookup[nameLookups.get("replace")] = 25;
    }
    if (nameLookups.containsKey("reverse")) {
      lookup[nameLookups.get("reverse")] = 26;
    }
    if (nameLookups.containsKey("rtrim")) {
      lookup[nameLookups.get("rtrim")] = 27;
    }
    if (nameLookups.containsKey("shuffle")) {
      lookup[nameLookups.get("shuffle")] = 28;
    }
    if (nameLookups.containsKey("sort")) {
      lookup[nameLookups.get("sort")] = 29;
    }
    if (nameLookups.containsKey("split")) {
      lookup[nameLookups.get("split")] = 30;
    }
    if (nameLookups.containsKey("startsWith")) {
      lookup[nameLookups.get("startsWith")] = 31;
    }
    if (nameLookups.containsKey("trim")) {
      lookup[nameLookups.get("trim")] = 32;
    }
    if (nameLookups.containsKey("upper")) {
      lookup[nameLookups.get("upper")] = 33;
    }
    if (nameLookups.containsKey("values")) {
      lookup[nameLookups.get("values")] = 34;
    }
    return lookup;
  }

  public static String primitiveMethodWrongArgCountError(String name, int expected, int actual) {
    String output = "";
    if ((expected == 0)) {
      output = name + " does not accept any arguments.";
    } else {
      if ((expected == 1)) {
        output = name + " accepts exactly 1 argument.";
      } else {
        output = name + " requires " + Integer.toString(expected) + " arguments.";
      }
    }
    return output + " Found: " + Integer.toString(actual);
  }

  public static int printToStdOut(String prefix, String line) {
    if ((prefix == null)) {
      PlatformTranslationHelper.printStdOut(line);
    } else {
      String canonical = line.replace((CharSequence) "\r\n", (CharSequence) "\n").replace((CharSequence) "\r", (CharSequence) "\n");
      String[] lines = PST_literalStringSplit(canonical, "\n");
      int i = 0;
      while ((i < lines.length)) {
        PlatformTranslationHelper.printStdOut(prefix + ": " + lines[i]);
        i += 1;
      }
    }
    return 0;
  }

  public static int qsortHelper(String[] keyStringList, double[] keyNumList, int[] indices, boolean isString, int startIndex, int endIndex) {
    if (((endIndex - startIndex) <= 0)) {
      return 0;
    }
    if (((endIndex - startIndex) == 1)) {
      if (sortHelperIsRevOrder(keyStringList, keyNumList, isString, startIndex, endIndex)) {
        sortHelperSwap(keyStringList, keyNumList, indices, isString, startIndex, endIndex);
      }
      return 0;
    }
    int mid = ((endIndex + startIndex) >> 1);
    sortHelperSwap(keyStringList, keyNumList, indices, isString, mid, startIndex);
    int upperPointer = (endIndex + 1);
    int lowerPointer = (startIndex + 1);
    while ((upperPointer > lowerPointer)) {
      if (sortHelperIsRevOrder(keyStringList, keyNumList, isString, startIndex, lowerPointer)) {
        lowerPointer += 1;
      } else {
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

  public static Value queryValue(VmContext vm, int execId, int stackFrameOffset, String[] steps) {
    if ((execId == -1)) {
      execId = vm.lastExecutionContextId;
    }
    ExecutionContext ec = vm.executionContexts.get(execId);
    StackFrame stackFrame = ec.stackTop;
    while ((stackFrameOffset > 0)) {
      stackFrameOffset -= 1;
      stackFrame = stackFrame.previous;
    }
    Value current = null;
    int i = 0;
    int j = 0;
    int len = steps.length;
    i = 0;
    while ((i < steps.length)) {
      if (((current == null) && (i > 0))) {
        return null;
      }
      String step = steps[i];
      if (isStringEqual(".", step)) {
        return null;
      } else {
        if (isStringEqual("this", step)) {
          current = stackFrame.objectContext;
        } else {
          if (isStringEqual("class", step)) {
            return null;
          } else {
            if (isStringEqual("local", step)) {
              i += 1;
              step = steps[i];
              HashMap<Integer, ArrayList<String>> localNamesByFuncPc = vm.symbolData.localVarNamesById;
              ArrayList<String> localNames = null;
              if (((localNamesByFuncPc == null) || (localNamesByFuncPc.size() == 0))) {
                return null;
              }
              j = stackFrame.pc;
              while ((j >= 0)) {
                if (localNamesByFuncPc.containsKey(j)) {
                  localNames = localNamesByFuncPc.get(j);
                  j = -1;
                }
                j -= 1;
              }
              if ((localNames == null)) {
                return null;
              }
              int localId = -1;
              if ((localNames != null)) {
                j = 0;
                while ((j < localNames.size())) {
                  if (isStringEqual(localNames.get(j), step)) {
                    localId = j;
                    j = localNames.size();
                  }
                  j += 1;
                }
              }
              if ((localId == -1)) {
                return null;
              }
              int localOffset = (localId + stackFrame.localsStackOffset);
              if ((ec.localsStackSet[localOffset] != stackFrame.localsStackSetToken)) {
                return null;
              }
              current = ec.localsStack[localOffset];
            } else {
              if (isStringEqual("index", step)) {
                return null;
              } else {
                if (isStringEqual("key-int", step)) {
                  return null;
                } else {
                  if (isStringEqual("key-str", step)) {
                    return null;
                  } else {
                    if (isStringEqual("key-obj", step)) {
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
      i += 1;
    }
    return current;
  }

  public static int read_integer(int[] pindex, String raw, int length, String alphaNums) {
    int num = 0;
    char c = raw.charAt(pindex[0]);
    pindex[0] = (pindex[0] + 1);
    if ((c == '%')) {
      String value = read_till(pindex, raw, length, '%');
      num = Integer.parseInt(value);
    } else {
      if ((c == '@')) {
        num = read_integer(pindex, raw, length, alphaNums);
        num *= 62;
        num += read_integer(pindex, raw, length, alphaNums);
      } else {
        if ((c == '#')) {
          num = read_integer(pindex, raw, length, alphaNums);
          num *= 62;
          num += read_integer(pindex, raw, length, alphaNums);
          num *= 62;
          num += read_integer(pindex, raw, length, alphaNums);
        } else {
          if ((c == '^')) {
            num = (-1 * read_integer(pindex, raw, length, alphaNums));
          } else {
            // TODO: string.IndexOfChar(c);
            num = alphaNums.indexOf(("" + c));
            if ((num == -1)) {
            }
          }
        }
      }
    }
    return num;
  }

  public static String read_string(int[] pindex, String raw, int length, String alphaNums) {
    String b64 = read_till(pindex, raw, length, '%');
    return PST_base64ToString(b64);
  }

  public static String read_till(int[] index, String raw, int length, char end) {
    ArrayList<Character> output = new ArrayList<Character>();
    boolean ctn = true;
    char c = ' ';
    while (ctn) {
      c = raw.charAt(index[0]);
      if ((c == end)) {
        ctn = false;
      } else {
        output.add(c);
      }
      index[0] = (index[0] + 1);
    }
    return PST_joinChars(output);
  }

  public static int[] reallocIntArray(int[] original, int requiredCapacity) {
    int oldSize = original.length;
    int size = oldSize;
    while ((size < requiredCapacity)) {
      size *= 2;
    }
    int[] output = new int[size];
    int i = 0;
    while ((i < oldSize)) {
      output[i] = original[i];
      i += 1;
    }
    return output;
  }

  public static Value Reflect_allClasses(VmContext vm) {
    int[] generics = new int[1];
    generics[0] = 10;
    ListImpl output = makeEmptyList(generics, 20);
    ClassInfo[] classTable = vm.metadata.classTable;
    int i = 1;
    while ((i < classTable.length)) {
      ClassInfo classInfo = classTable[i];
      if ((classInfo == null)) {
        i = classTable.length;
      } else {
        addToList(output, new Value(10, new org.crayonlang.interpreter.structs.ClassValue(false, classInfo.id)));
      }
      i += 1;
    }
    return new Value(6, output);
  }

  public static Value Reflect_getMethods(VmContext vm, ExecutionContext ec, Value methodSource) {
    ListImpl output = makeEmptyList(null, 8);
    if ((methodSource.type == 8)) {
      ObjectInstance objInstance1 = ((ObjectInstance) methodSource.internalValue);
      ClassInfo classInfo = vm.metadata.classTable[objInstance1.classId];
      int i = 0;
      while ((i < classInfo.functionIds.length)) {
        int functionId = classInfo.functionIds[i];
        if ((functionId != -1)) {
          addToList(output, new Value(9, new FunctionPointer(2, methodSource, objInstance1.classId, functionId, null)));
        }
        i += 1;
      }
    } else {
      org.crayonlang.interpreter.structs.ClassValue classValue = ((org.crayonlang.interpreter.structs.ClassValue) methodSource.internalValue);
      ClassInfo classInfo = vm.metadata.classTable[classValue.classId];
      EX_UnsupportedOperation(ec, "static method reflection not implemented yet.");
    }
    return new Value(6, output);
  }

  public static int registerNamedCallback(VmContext vm, String scope, String functionName, java.lang.reflect.Method callback) {
    int id = getNamedCallbackIdImpl(vm, scope, functionName, true);
    vm.namedCallbacks.callbacksById.set(id, callback);
    return id;
  }

  public static int resetLocalsStackTokens(ExecutionContext ec, StackFrame stack) {
    Value[] localsStack = ec.localsStack;
    int[] localsStackSet = ec.localsStackSet;
    int i = stack.localsStackOffsetEnd;
    while ((i < localsStackSet.length)) {
      localsStackSet[i] = 0;
      localsStack[i] = null;
      i += 1;
    }
    StackFrame stackWalker = stack;
    while ((stackWalker != null)) {
      int token = stackWalker.localsStackSetToken;
      stackWalker.localsStackSetToken = 1;
      i = stackWalker.localsStackOffset;
      while ((i < stackWalker.localsStackOffsetEnd)) {
        if ((localsStackSet[i] == token)) {
          localsStackSet[i] = 1;
        } else {
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

  public static int resolvePrimitiveMethodName2(int[] lookup, int type, int globalNameId) {
    int output = lookup[globalNameId];
    if ((output != -1)) {
      switch ((type + (11 * output))) {
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

  public static Value resource_manager_getResourceOfType(VmContext vm, String userPath, String type) {
    ResourceDB db = vm.resourceDatabase;
    HashMap<String, ResourceInfo> lookup = db.fileInfo;
    if (lookup.containsKey(userPath)) {
      ListImpl output = makeEmptyList(null, 2);
      ResourceInfo file = lookup.get(userPath);
      if (file.type.equals(type)) {
        addToList(output, vm.globals.boolTrue);
        addToList(output, buildString(vm.globals, file.internalPath));
      } else {
        addToList(output, vm.globals.boolFalse);
      }
      return new Value(6, output);
    }
    return vm.globals.valueNull;
  }

  public static int resource_manager_populate_directory_lookup(HashMap<String, ArrayList<String>> dirs, String path) {
    String[] parts = PST_literalStringSplit(path, "/");
    String pathBuilder = "";
    String file = "";
    int i = 0;
    while ((i < parts.length)) {
      file = parts[i];
      ArrayList<String> files = null;
      if (!dirs.containsKey(pathBuilder)) {
        files = new ArrayList<String>();
        dirs.put(pathBuilder, files);
      } else {
        files = dirs.get(pathBuilder);
      }
      files.add(file);
      if ((i > 0)) {
        pathBuilder = pathBuilder + "/" + file;
      } else {
        pathBuilder = file;
      }
      i += 1;
    }
    return 0;
  }

  public static ResourceDB resourceManagerInitialize(VmGlobals globals, String manifest) {
    HashMap<String, ArrayList<String>> filesPerDirectoryBuilder = new HashMap<String, ArrayList<String>>();
    HashMap<String, ResourceInfo> fileInfo = new HashMap<String, ResourceInfo>();
    ArrayList<Value> dataList = new ArrayList<Value>();
    String[] items = PST_literalStringSplit(manifest, "\n");
    ResourceInfo resourceInfo = null;
    String type = "";
    String userPath = "";
    String internalPath = "";
    String argument = "";
    boolean isText = false;
    int intType = 0;
    int i = 0;
    while ((i < items.length)) {
      String[] itemData = PST_literalStringSplit(items[i], ",");
      if ((itemData.length >= 3)) {
        type = itemData[0];
        isText = "TXT".equals(type);
        if (isText) {
          intType = 1;
        } else {
          if (("IMGSH".equals(type) || "IMG".equals(type))) {
            intType = 2;
          } else {
            if ("SND".equals(type)) {
              intType = 3;
            } else {
              if ("TTF".equals(type)) {
                intType = 4;
              } else {
                intType = 5;
              }
            }
          }
        }
        userPath = stringDecode(itemData[1]);
        internalPath = itemData[2];
        argument = "";
        if ((itemData.length > 3)) {
          argument = stringDecode(itemData[3]);
        }
        resourceInfo = new ResourceInfo(userPath, internalPath, isText, type, argument);
        fileInfo.put(userPath, resourceInfo);
        resource_manager_populate_directory_lookup(filesPerDirectoryBuilder, userPath);
        dataList.add(buildString(globals, userPath));
        dataList.add(buildInteger(globals, intType));
        if ((internalPath != null)) {
          dataList.add(buildString(globals, internalPath));
        } else {
          dataList.add(globals.valueNull);
        }
      }
      i += 1;
    }
    String[] dirs = PST_convertStringSetToArray(filesPerDirectoryBuilder.keySet());
    HashMap<String, String[]> filesPerDirectorySorted = new HashMap<String, String[]>();
    i = 0;
    while ((i < dirs.length)) {
      String dir = dirs[i];
      ArrayList<String> unsortedDirs = filesPerDirectoryBuilder.get(dir);
      String[] dirsSorted = unsortedDirs.toArray(PST_emptyArrayString);
      dirsSorted = PST_sortedCopyOfStringArray(dirsSorted);
      filesPerDirectorySorted.put(dir, dirsSorted);
      i += 1;
    }
    return new ResourceDB(filesPerDirectorySorted, fileInfo, dataList);
  }

  public static void reverseList(ListImpl list) {
    int _len = list.size;
    Value t = null;
    int i2 = 0;
    Value[] arr = list.array;
    int i = (_len >> 1);
    while ((i < _len)) {
      i2 = (_len - i - 1);
      t = arr[i];
      arr[i] = arr[i2];
      arr[i2] = t;
      i += 1;
    }
  }

  public static InterpreterResult runInterpreter(VmContext vm, int executionContextId) {
    InterpreterResult result = interpret(vm, executionContextId);
    result.executionContextId = executionContextId;
    int status = result.status;
    if ((status == 1)) {
      if (vm.executionContexts.containsKey(executionContextId)) {
        vm.executionContexts.remove(executionContextId);
      }
      runShutdownHandlers(vm);
    } else {
      if ((status == 3)) {
        printToStdOut(vm.environment.stacktracePrefix, result.errorMessage);
        runShutdownHandlers(vm);
      }
    }
    if ((executionContextId == 0)) {
      result.isRootContext = true;
    }
    return result;
  }

  public static InterpreterResult runInterpreterWithFunctionPointer(VmContext vm, Value fpValue, Value[] args) {
    int newId = (vm.lastExecutionContextId + 1);
    vm.lastExecutionContextId = newId;
    ArrayList<Value> argList = new ArrayList<Value>();
    int i = 0;
    while ((i < args.length)) {
      argList.add(args[i]);
      i += 1;
    }
    Value[] locals = new Value[0];
    int[] localsSet = new int[0];
    Value[] valueStack = new Value[100];
    valueStack[0] = fpValue;
    valueStack[1] = buildList(argList);
    StackFrame stack = new StackFrame((vm.byteCode.ops.length - 2), 1, 0, 0, null, false, null, 0, 0, 1, 0, null, null, null);
    ExecutionContext executionContext = new ExecutionContext(newId, stack, 2, 100, valueStack, locals, localsSet, 1, 0, false, null, false, 0, null);
    vm.executionContexts.put(newId, executionContext);
    return runInterpreter(vm, newId);
  }

  public static int runShutdownHandlers(VmContext vm) {
    while ((vm.shutdownHandlers.size() > 0)) {
      Value handler = vm.shutdownHandlers.get(0);
      vm.shutdownHandlers.remove(0);
      runInterpreterWithFunctionPointer(vm, handler, new Value[0]);
    }
    return 0;
  }

  public static void setItemInList(ListImpl list, int i, Value v) {
    list.array[i] = v;
  }

  public static boolean sortHelperIsRevOrder(String[] keyStringList, double[] keyNumList, boolean isString, int indexLeft, int indexRight) {
    if (isString) {
      return (keyStringList[indexLeft].compareTo(keyStringList[indexRight]) > 0);
    }
    return (keyNumList[indexLeft] > keyNumList[indexRight]);
  }

  public static int sortHelperSwap(String[] keyStringList, double[] keyNumList, int[] indices, boolean isString, int index1, int index2) {
    if ((index1 == index2)) {
      return 0;
    }
    int t = indices[index1];
    indices[index1] = indices[index2];
    indices[index2] = t;
    if (isString) {
      String s = keyStringList[index1];
      keyStringList[index1] = keyStringList[index2];
      keyStringList[index2] = s;
    } else {
      double n = keyNumList[index1];
      keyNumList[index1] = keyNumList[index2];
      keyNumList[index2] = n;
    }
    return 0;
  }

  public static int sortLists(ListImpl keyList, ListImpl parallelList, int[] intOutParam) {
    PST_intBuffer16[0] = 0;
    int length = keyList.size;
    if ((length < 2)) {
      return 0;
    }
    int i = 0;
    Value item = null;
    item = keyList.array[0];
    boolean isString = (item.type == 5);
    String[] stringKeys = null;
    double[] numKeys = null;
    if (isString) {
      stringKeys = new String[length];
    } else {
      numKeys = new double[length];
    }
    int[] indices = new int[length];
    Value[] originalOrder = new Value[length];
    i = 0;
    while ((i < length)) {
      indices[i] = i;
      originalOrder[i] = parallelList.array[i];
      item = keyList.array[i];
      switch (item.type) {
        case 3:
          if (isString) {
            PST_intBuffer16[0] = 1;
            return 0;
          }
          numKeys[i] = ((double) item.intValue);
          break;
        case 4:
          if (isString) {
            PST_intBuffer16[0] = 1;
            return 0;
          }
          numKeys[i] = ((double) item.internalValue);
          break;
        case 5:
          if (!isString) {
            PST_intBuffer16[0] = 1;
            return 0;
          }
          stringKeys[i] = ((String) item.internalValue);
          break;
        default:
          PST_intBuffer16[0] = 1;
          return 0;
      }
      i += 1;
    }
    qsortHelper(stringKeys, numKeys, indices, isString, 0, (length - 1));
    i = 0;
    while ((i < length)) {
      parallelList.array[i] = originalOrder[indices[i]];
      i += 1;
    }
    return 0;
  }

  public static boolean stackItemIsLibrary(String stackInfo) {
    if ((stackInfo.charAt(0) != '[')) {
      return false;
    }
    int cIndex = stackInfo.indexOf(":");
    return ((cIndex > 0) && (cIndex < stackInfo.indexOf("]")));
  }

  public static InterpreterResult startVm(VmContext vm) {
    return runInterpreter(vm, vm.lastExecutionContextId);
  }

  public static String stringDecode(String encoded) {
    if (!encoded.contains("%")) {
      int length = encoded.length();
      char per = '%';
      ArrayList<String> builder = new ArrayList<String>();
      int i = 0;
      while ((i < length)) {
        char c = encoded.charAt(i);
        if (((c == per) && ((i + 2) < length))) {
          builder.add(stringFromHex("" + ("" + encoded.charAt((i + 1))) + ("" + encoded.charAt((i + 2)))));
        } else {
          builder.add("" + ("" + c));
        }
        i += 1;
      }
      return PST_joinList("", builder);
    }
    return encoded;
  }

  public static String stringFromHex(String encoded) {
    encoded = encoded.toUpperCase();
    String hex = "0123456789ABCDEF";
    ArrayList<String> output = new ArrayList<String>();
    int length = encoded.length();
    int a = 0;
    int b = 0;
    String c = null;
    int i = 0;
    while (((i + 1) < length)) {
      c = "" + ("" + encoded.charAt(i));
      a = hex.indexOf(c);
      if ((a == -1)) {
        return null;
      }
      c = "" + ("" + encoded.charAt((i + 1)));
      b = hex.indexOf(c);
      if ((b == -1)) {
        return null;
      }
      a = ((a * 16) + b);
      output.add(("" + Character.toString((char) a)));
      i += 2;
    }
    return PST_joinList("", output);
  }

  public static InterpreterResult suspendInterpreter() {
    return new InterpreterResult(2, null, 0.0, 0, false, "");
  }

  public static int tokenDataImpl(VmContext vm, int[] row) {
    ArrayList<Token>[] tokensByPc = vm.symbolData.tokenData;
    int pc = (row[0] + vm.metadata.userCodeStart);
    int line = row[1];
    int col = row[2];
    int file = row[3];
    ArrayList<Token> tokens = tokensByPc[pc];
    if ((tokens == null)) {
      tokens = new ArrayList<Token>();
      tokensByPc[pc] = tokens;
    }
    tokens.add(new Token(line, col, file));
    return 0;
  }

  public static ArrayList<String> tokenHelperConvertPcsToStackTraceStrings(VmContext vm, ArrayList<Integer> pcs) {
    ArrayList<Token> tokens = generateTokenListFromPcs(vm, pcs);
    String[] files = vm.symbolData.sourceCode;
    ArrayList<String> output = new ArrayList<String>();
    int i = 0;
    while ((i < tokens.size())) {
      Token token = tokens.get(i);
      if ((token == null)) {
        output.add("[No stack information]");
      } else {
        int line = token.lineIndex;
        int col = token.colIndex;
        String fileData = files[token.fileId];
        String[] lines = PST_literalStringSplit(fileData, "\n");
        String filename = lines[0];
        String linevalue = lines[(line + 1)];
        output.add(filename + ", Line: " + Integer.toString((line + 1)) + ", Col: " + Integer.toString((col + 1)));
      }
      i += 1;
    }
    return output;
  }

  public static String tokenHelperGetFileLine(VmContext vm, int fileId, int lineNum) {
    String sourceCode = vm.symbolData.sourceCode[fileId];
    if ((sourceCode == null)) {
      return null;
    }
    return PST_literalStringSplit(sourceCode, "\n")[lineNum];
  }

  public static String tokenHelperGetFormattedPointerToToken(VmContext vm, Token token) {
    String line = tokenHelperGetFileLine(vm, token.fileId, (token.lineIndex + 1));
    if ((line == null)) {
      return null;
    }
    int columnIndex = token.colIndex;
    int lineLength = line.length();
    line = PST_trimSide(line, true);
    line = line.replace((CharSequence) "\t", (CharSequence) " ");
    int offset = (lineLength - line.length());
    columnIndex -= offset;
    String line2 = "";
    while ((columnIndex > 0)) {
      columnIndex -= 1;
      line2 = line2 + " ";
    }
    line2 = line2 + "^";
    return line + "\n" + line2;
  }

  public static boolean tokenHelplerIsFilePathLibrary(VmContext vm, int fileId, String[] allFiles) {
    String filename = tokenHelperGetFileLine(vm, fileId, 0);
    return !filename.toLowerCase().endsWith(".cry");
  }

  public static String typeInfoToString(VmContext vm, int[] typeInfo, int i) {
    ArrayList<String> output = new ArrayList<String>();
    typeToStringBuilder(vm, output, typeInfo, i);
    return PST_joinList("", output);
  }

  public static String typeToString(VmContext vm, int[] typeInfo, int i) {
    ArrayList<String> sb = new ArrayList<String>();
    typeToStringBuilder(vm, sb, typeInfo, i);
    return PST_joinList("", sb);
  }

  public static int typeToStringBuilder(VmContext vm, ArrayList<String> sb, int[] typeInfo, int i) {
    switch (typeInfo[i]) {
      case -1:
        sb.add("void");
        return (i + 1);
      case 0:
        sb.add("object");
        return (i + 1);
      case 1:
        sb.add("object");
        return (i + 1);
      case 3:
        sb.add("int");
        return (i + 1);
      case 4:
        sb.add("float");
        return (i + 1);
      case 2:
        sb.add("bool");
        return (i + 1);
      case 5:
        sb.add("string");
        return (i + 1);
      case 6:
        sb.add("List<");
        i = typeToStringBuilder(vm, sb, typeInfo, (i + 1));
        sb.add(">");
        return i;
      case 7:
        sb.add("Dictionary<");
        i = typeToStringBuilder(vm, sb, typeInfo, (i + 1));
        sb.add(", ");
        i = typeToStringBuilder(vm, sb, typeInfo, i);
        sb.add(">");
        return i;
      case 8:
        int classId = typeInfo[(i + 1)];
        if ((classId == 0)) {
          sb.add("object");
        } else {
          ClassInfo classInfo = vm.metadata.classTable[classId];
          sb.add(classInfo.fullyQualifiedName);
        }
        return (i + 2);
      case 10:
        sb.add("Class");
        return (i + 1);
      case 9:
        int n = typeInfo[(i + 1)];
        int optCount = typeInfo[(i + 2)];
        i += 2;
        sb.add("function(");
        ArrayList<String> ret = new ArrayList<String>();
        i = typeToStringBuilder(vm, ret, typeInfo, i);
        int j = 1;
        while ((j < n)) {
          if ((j > 1)) {
            sb.add(", ");
          }
          i = typeToStringBuilder(vm, sb, typeInfo, i);
          j += 1;
        }
        if ((n == 1)) {
          sb.add("void");
        }
        sb.add(" => ");
        int optStart = (n - optCount - 1);
        j = 0;
        while ((j < ret.size())) {
          if ((j >= optStart)) {
            sb.add("(opt) ");
          }
          sb.add(ret.get(j));
          j += 1;
        }
        sb.add(")");
        return i;
      default:
        sb.add("UNKNOWN");
        return (i + 1);
    }
  }

  public static String typeToStringFromValue(VmContext vm, Value value) {
    ArrayList<String> sb = null;
    switch (value.type) {
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
        int classId = (((ObjectInstance) value.internalValue)).classId;
        ClassInfo classInfo = vm.metadata.classTable[classId];
        return classInfo.fullyQualifiedName;
      case 6:
        sb = new ArrayList<String>();
        sb.add("List<");
        ListImpl list = ((ListImpl) value.internalValue);
        if ((list.type == null)) {
          sb.add("object");
        } else {
          typeToStringBuilder(vm, sb, list.type, 0);
        }
        sb.add(">");
        return PST_joinList("", sb);
      case 7:
        DictImpl dict = ((DictImpl) value.internalValue);
        sb = new ArrayList<String>();
        sb.add("Dictionary<");
        switch (dict.keyType) {
          case 3:
            sb.add("int");
            break;
          case 5:
            sb.add("string");
            break;
          case 8:
            sb.add("object");
            break;
          default:
            sb.add("???");
            break;
        }
        sb.add(", ");
        if ((dict.valueType == null)) {
          sb.add("object");
        } else {
          typeToStringBuilder(vm, sb, dict.valueType, 0);
        }
        sb.add(">");
        return PST_joinList("", sb);
      case 9:
        return "Function";
      default:
        return "Unknown";
    }
  }

  public static InterpreterResult uncaughtExceptionResult(VmContext vm, Value exception) {
    return new InterpreterResult(3, unrollExceptionOutput(vm, exception), 0.0, 0, false, "");
  }

  public static String unrollExceptionOutput(VmContext vm, Value exceptionInstance) {
    ObjectInstance objInstance = ((ObjectInstance) exceptionInstance.internalValue);
    ClassInfo classInfo = vm.metadata.classTable[objInstance.classId];
    ArrayList<Integer> pcs = ((ArrayList<Integer>) objInstance.nativeData[1]);
    String codeFormattedPointer = "";
    String exceptionName = classInfo.fullyQualifiedName;
    String message = valueToString(vm, objInstance.members[1]);
    ArrayList<String> trace = tokenHelperConvertPcsToStackTraceStrings(vm, pcs);
    trace.remove(trace.size() - 1);
    trace.add("Stack Trace:");
    java.util.Collections.reverse(trace);
    java.util.Collections.reverse(pcs);
    boolean showLibStack = vm.environment.showLibStack;
    if ((!showLibStack && !stackItemIsLibrary(trace.get(0)))) {
      while (stackItemIsLibrary(trace.get((trace.size() - 1)))) {
        trace.remove(trace.size() - 1);
        pcs.remove(pcs.size() - 1);
      }
    }
    ArrayList<Token> tokensAtPc = vm.symbolData.tokenData[pcs.get((pcs.size() - 1))];
    if ((tokensAtPc != null)) {
      codeFormattedPointer = "\n\n" + tokenHelperGetFormattedPointerToToken(vm, tokensAtPc.get(0));
    }
    String stackTrace = PST_joinList("\n", trace);
    return stackTrace + codeFormattedPointer + "\n" + exceptionName + ": " + message;
  }

  public static ListImpl valueConcatLists(ListImpl a, ListImpl b) {
    int aLen = a.size;
    int bLen = b.size;
    int size = (aLen + bLen);
    ListImpl c = new ListImpl(null, size, size, new Value[size]);
    int i = 0;
    while ((i < aLen)) {
      c.array[i] = a.array[i];
      i += 1;
    }
    i = 0;
    while ((i < bLen)) {
      c.array[(i + aLen)] = b.array[i];
      i += 1;
    }
    c.size = c.capacity;
    return c;
  }

  public static ListImpl valueMultiplyList(ListImpl a, int n) {
    int _len = (a.size * n);
    ListImpl output = makeEmptyList(a.type, _len);
    if ((_len == 0)) {
      return output;
    }
    int aLen = a.size;
    int i = 0;
    Value value = null;
    if ((aLen == 1)) {
      value = a.array[0];
      i = 0;
      while ((i < n)) {
        output.array[i] = value;
        i += 1;
      }
    } else {
      int j = 0;
      i = 0;
      while ((i < n)) {
        j = 0;
        while ((j < aLen)) {
          output.array[((i * aLen) + j)] = a.array[j];
          j += 1;
        }
        i += 1;
      }
    }
    output.size = _len;
    return output;
  }

  public static Value[] valueStackIncreaseCapacity(ExecutionContext ec) {
    Value[] stack = ec.valueStack;
    int oldCapacity = stack.length;
    int newCapacity = (oldCapacity * 2);
    Value[] newStack = new Value[newCapacity];
    int i = (oldCapacity - 1);
    while ((i >= 0)) {
      newStack[i] = stack[i];
      i -= 1;
    }
    ec.valueStack = newStack;
    return newStack;
  }

  public static String valueToString(VmContext vm, Value wrappedValue) {
    int type = wrappedValue.type;
    if ((type == 1)) {
      return "null";
    }
    if ((type == 2)) {
      if ((wrappedValue.intValue == 1)) {
        return "true";
      }
      return "false";
    }
    if ((type == 4)) {
      String floatStr = Double.toString(((double) wrappedValue.internalValue));
      if (!floatStr.contains(".")) {
        floatStr += ".0";
      }
      return floatStr;
    }
    if ((type == 3)) {
      return Integer.toString(wrappedValue.intValue);
    }
    if ((type == 5)) {
      return ((String) wrappedValue.internalValue);
    }
    if ((type == 6)) {
      ListImpl internalList = ((ListImpl) wrappedValue.internalValue);
      String output = "[";
      int i = 0;
      while ((i < internalList.size)) {
        if ((i > 0)) {
          output += ", ";
        }
        output += valueToString(vm, internalList.array[i]);
        i += 1;
      }
      output += "]";
      return output;
    }
    if ((type == 8)) {
      ObjectInstance objInstance = ((ObjectInstance) wrappedValue.internalValue);
      int classId = objInstance.classId;
      int ptr = objInstance.objectId;
      ClassInfo classInfo = vm.metadata.classTable[classId];
      int nameId = classInfo.nameId;
      String className = vm.metadata.identifiers[nameId];
      return "Instance<" + className + "#" + Integer.toString(ptr) + ">";
    }
    if ((type == 7)) {
      DictImpl dict = ((DictImpl) wrappedValue.internalValue);
      if ((dict.size == 0)) {
        return "{}";
      }
      String output = "{";
      ArrayList<Value> keyList = dict.keys;
      ArrayList<Value> valueList = dict.values;
      int i = 0;
      while ((i < dict.size)) {
        if ((i > 0)) {
          output += ", ";
        }
        output += valueToString(vm, dict.keys.get(i)) + ": " + valueToString(vm, dict.values.get(i));
        i += 1;
      }
      output += " }";
      return output;
    }
    if ((type == 9)) {
      FunctionPointer fp = ((FunctionPointer) wrappedValue.internalValue);
      switch (fp.type) {
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

  public static int vm_getCurrentExecutionContextId(VmContext vm) {
    return vm.lastExecutionContextId;
  }

  public static int vm_suspend(VmContext vm, int status) {
    return vm_suspend_for_context(getExecutionContext(vm, -1), 1);
  }

  public static int vm_suspend_for_context(ExecutionContext ec, int status) {
    ec.executionStateChange = true;
    ec.executionStateChangeCommand = status;
    return 0;
  }

  public static int vm_suspend_with_status(VmContext vm, int status) {
    return vm_suspend_for_context(getExecutionContext(vm, -1), status);
  }

  public static void vmEnvSetCommandLineArgs(VmContext vm, String[] args) {
    vm.environment.commandLineArgs = args;
  }

  public static VmGlobals vmGetGlobals(VmContext vm) {
    return vm.globals;
  }
}
