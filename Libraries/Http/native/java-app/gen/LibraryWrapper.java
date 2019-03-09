package org.crayonlang.libraries.http;

import java.util.ArrayList;
import java.util.HashMap;
import org.crayonlang.interpreter.PlatformTranslationHelper;;
import org.crayonlang.interpreter.structs.*;
import org.crayonlang.interpreter.TranslationHelper;;

public final class LibraryWrapper {

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

  public static Value lib_http_fastEnsureAllBytes(VmContext vm, Value[] args) {
    if ((args[0].type == 6)) {
      ListImpl list1 = ((ListImpl) args[0].internalValue);
      int i = list1.size;
      int int1 = 0;
      int[] intArray1 = new int[i];
      Value value = null;
      while ((i > 0)) {
        i -= 1;
        value = list1.array[i];
        if ((value.type != 3)) {
          return vm.globalFalse;
        }
        int1 = ((int) value.internalValue);
        if ((int1 < 0)) {
          if ((int1 < -128)) {
            return vm.globalFalse;
          }
          int1 += 256;
        } else {
          if ((int1 >= 256)) {
            return vm.globalFalse;
          }
        }
        intArray1[i] = int1;
      }
      Object[] objArray1 = new Object[1];
      objArray1[0] = intArray1;
      ObjectInstance objInstance1 = ((ObjectInstance) args[1].internalValue);
      objInstance1.nativeData = objArray1;
      return vm.globalTrue;
    }
    return vm.globalFalse;
  }

  public static Value lib_http_getResponseBytes(VmContext vm, Value[] args) {
    Value outputListValue = args[1];
    ObjectInstance objInstance1 = ((ObjectInstance) args[0].internalValue);
    Object[] objArray1 = objInstance1.nativeData;
    ArrayList<Value> tList = new ArrayList<Value>();
    crayonlib.http.HttpHelper.getResponseBytes(objArray1[0], vm.globals.positiveIntegers, tList);
    ListImpl outputList = ((ListImpl) outputListValue.internalValue);
    Value t = org.crayonlang.interpreter.vm.CrayonWrapper.buildList(tList);
    ListImpl otherList = ((ListImpl) t.internalValue);
    outputList.capacity = otherList.capacity;
    outputList.array = otherList.array;
    outputList.size = tList.size();
    return outputListValue;
  }

  public static Value lib_http_pollRequest(VmContext vm, Value[] args) {
    ObjectInstance objInstance1 = ((ObjectInstance) args[0].internalValue);
    Object[] objArray1 = objInstance1.nativeData;
    if (crayonlib.http.HttpHelper.pollRequest(objArray1)) {
      return vm.globalTrue;
    }
    return vm.globalFalse;
  }

  public static Value lib_http_populateResponse(VmContext vm, Value[] args) {
    Value arg2 = args[1];
    Value arg3 = args[2];
    Value arg4 = args[3];
    ObjectInstance objInstance1 = ((ObjectInstance) args[0].internalValue);
    Object object1 = objInstance1.nativeData[0];
    Object[] objArray1 = new Object[1];
    ArrayList<String> stringList1 = new ArrayList<String>();
    crayonlib.http.HttpHelper.readResponseData(object1, PST_intBuffer16, PST_stringBuffer16, objArray1, stringList1);
    objInstance1 = ((ObjectInstance) arg2.internalValue);
    objInstance1.nativeData = objArray1;
    ListImpl outputList = ((ListImpl) arg3.internalValue);
    org.crayonlang.interpreter.vm.CrayonWrapper.addToList(outputList, org.crayonlang.interpreter.vm.CrayonWrapper.buildInteger(vm.globals, PST_intBuffer16[0]));
    org.crayonlang.interpreter.vm.CrayonWrapper.addToList(outputList, org.crayonlang.interpreter.vm.CrayonWrapper.buildString(vm.globals, PST_stringBuffer16[0]));
    Value value = vm.globalNull;
    Value value2 = vm.globalTrue;
    if ((PST_intBuffer16[1] == 0)) {
      value = org.crayonlang.interpreter.vm.CrayonWrapper.buildString(vm.globals, PST_stringBuffer16[1]);
      value2 = vm.globalFalse;
    }
    org.crayonlang.interpreter.vm.CrayonWrapper.addToList(outputList, value);
    org.crayonlang.interpreter.vm.CrayonWrapper.addToList(outputList, value2);
    ListImpl list1 = ((ListImpl) arg4.internalValue);
    int i = 0;
    while ((i < stringList1.size())) {
      org.crayonlang.interpreter.vm.CrayonWrapper.addToList(list1, org.crayonlang.interpreter.vm.CrayonWrapper.buildString(vm.globals, stringList1.get(i)));
      i += 1;
    }
    return vm.globalNull;
  }

  public static Value lib_http_sendRequest(VmContext vm, Value[] args) {
    Value body = args[5];
    ObjectInstance objInstance1 = ((ObjectInstance) args[0].internalValue);
    Object[] objArray1 = new Object[3];
    objInstance1.nativeData = objArray1;
    objArray1[2] = false;
    String method = ((String) args[2].internalValue);
    String url = ((String) args[3].internalValue);
    ArrayList<String> headers = new ArrayList<String>();
    ListImpl list1 = ((ListImpl) args[4].internalValue);
    int i = 0;
    while ((i < list1.size)) {
      headers.add(((String) list1.array[i].internalValue));
      i += 1;
    }
    Object bodyRawObject = body.internalValue;
    int bodyState = 0;
    if ((body.type == 5)) {
      bodyState = 1;
    } else {
      if ((body.type == 8)) {
        objInstance1 = ((ObjectInstance) bodyRawObject);
        bodyRawObject = objInstance1.nativeData[0];
        bodyState = 2;
      } else {
        bodyRawObject = null;
      }
    }
    boolean getResponseAsText = (((int) args[6].internalValue) == 1);
    if (((boolean) args[1].internalValue)) {
      crayonlib.http.HttpHelper.sendRequestAsync(objArray1, method, url, headers, bodyState, bodyRawObject, getResponseAsText, vm, args[8], (((ObjectInstance) args[9].internalValue)).nativeData);
    } else {
      int execId = ((int) args[7].internalValue);
      if (crayonlib.http.HttpHelper.sendRequestSync(objArray1, method, url, headers, bodyState, bodyRawObject, getResponseAsText)) {
        org.crayonlang.interpreter.vm.CrayonWrapper.vm_suspend_context_by_id(vm, execId, 1);
      }
    }
    return vm.globalNull;
  }
}
