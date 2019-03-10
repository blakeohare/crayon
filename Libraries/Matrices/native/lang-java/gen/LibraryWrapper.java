package org.crayonlang.libraries.matrices;

import java.util.ArrayList;
import java.util.HashMap;
import org.crayonlang.interpreter.PlatformTranslationHelper;
import org.crayonlang.interpreter.structs.*;
import org.crayonlang.interpreter.TranslationHelper;

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

  public static Value lib_matrices_addMatrix(VmContext vm, Value[] args) {
    ObjectInstance obj = ((ObjectInstance) args[0].internalValue);
    Object[] nd1 = obj.nativeData;
    if (!((boolean) args[2].internalValue)) {
      nd1[5] = "Input must be a matrix";
      return vm.globalNull;
    }
    double[] left = ((double[]) nd1[0]);
    int width = ((int) nd1[1]);
    int height = ((int) nd1[2]);
    obj = ((ObjectInstance) args[1].internalValue);
    Object[] nd2 = obj.nativeData;
    double[] right = ((double[]) nd2[0]);
    if (((((int) nd2[1]) != width) || (((int) nd2[2]) != height))) {
      nd1[5] = "Matrices must be the same size.";
      return vm.globalNull;
    }
    double[] output = left;
    boolean isInline = (args[3].type == 1);
    if (isInline) {
      nd1[4] = true;
    } else {
      if (!((boolean) args[4].internalValue)) {
        nd1[5] = "Output value must be a matrix";
        return vm.globalNull;
      } else {
        obj = ((ObjectInstance) args[3].internalValue);
        Object[] nd3 = obj.nativeData;
        output = ((double[]) nd3[0]);
        if (((((int) nd3[1]) != width) || (((int) nd3[2]) != height))) {
          nd1[5] = "Output matrix must have the same size as the inputs.";
          return vm.globalNull;
        }
        nd3[4] = true;
      }
    }
    int length = (width * height);
    int i = 0;
    while ((i < length)) {
      output[i] = (left[i] + right[i]);
      i += 1;
    }
    return args[0];
  }

  public static Value lib_matrices_copyFrom(VmContext vm, Value[] args) {
    ObjectInstance obj = ((ObjectInstance) args[0].internalValue);
    Object[] nd1 = obj.nativeData;
    obj = ((ObjectInstance) args[1].internalValue);
    Object[] nd2 = obj.nativeData;
    if (!((boolean) args[2].internalValue)) {
      nd1[5] = "value was not a matrix";
      return vm.globalNull;
    }
    if (((((int) nd1[1]) != ((int) nd2[1])) || (((int) nd1[2]) != ((int) nd2[2])))) {
      nd1[5] = "Matrices were not the same size.";
      return vm.globalNull;
    }
    double[] target = ((double[]) nd1[0]);
    double[] source = ((double[]) nd2[0]);
    int _len = target.length;
    int i = 0;
    while ((i < _len)) {
      target[i] = source[i];
      i += 1;
    }
    return args[0];
  }

  public static Value lib_matrices_getError(VmContext vm, Value[] args) {
    ObjectInstance obj = ((ObjectInstance) args[0].internalValue);
    return org.crayonlang.interpreter.vm.CrayonWrapper.buildString(vm.globals, ((String) obj.nativeData[5]));
  }

  public static Value lib_matrices_getValue(VmContext vm, Value[] args) {
    ObjectInstance obj = ((ObjectInstance) args[0].internalValue);
    Object[] nd = obj.nativeData;
    if (((args[1].type != 3) || (args[2].type != 3))) {
      nd[5] = "Invalid coordinates";
      return vm.globalNull;
    }
    int x = ((int) args[1].internalValue);
    int y = ((int) args[2].internalValue);
    int width = ((int) nd[1]);
    int height = ((int) nd[2]);
    if (((x < 0) || (x >= width) || (y < 0) || (y >= height))) {
      nd[5] = "Coordinates out of range.";
      return vm.globalNull;
    }
    Value[] valueArray = ((Value[]) nd[3]);
    if (!((boolean) nd[4])) {
      double[] data = ((double[]) nd[0]);
      int length = (width * height);
      int i = 0;
      while ((i < length)) {
        valueArray[i] = org.crayonlang.interpreter.vm.CrayonWrapper.buildFloat(vm.globals, data[i]);
        i += 1;
      }
    }
    return valueArray[((width * y) + x)];
  }

  public static Value lib_matrices_initMatrix(VmContext vm, Value[] args) {
    ObjectInstance obj = ((ObjectInstance) args[0].internalValue);
    Object[] nd = obj.nativeData;
    if (((args[1].type != 3) || (args[2].type != 3))) {
      nd[5] = "Width and height must be integers.";
      return vm.globalTrue;
    }
    int width = ((int) args[1].internalValue);
    int height = ((int) args[2].internalValue);
    int size = (width * height);
    double[] data = new double[size];
    nd[0] = data;
    nd[1] = width;
    nd[2] = height;
    nd[3] = new Value[size];
    nd[4] = false;
    nd[5] = "";
    nd[6] = new double[size];
    int i = 0;
    while ((i < size)) {
      data[i] = 0.0;
      i += 1;
    }
    return vm.globalFalse;
  }

  public static Value lib_matrices_multiplyMatrix(VmContext vm, Value[] args) {
    ObjectInstance obj = ((ObjectInstance) args[0].internalValue);
    Object[] nd1 = obj.nativeData;
    if (!((boolean) args[2].internalValue)) {
      nd1[5] = "argument must be a matrix";
      return vm.globalNull;
    }
    obj = ((ObjectInstance) args[1].internalValue);
    Object[] nd2 = obj.nativeData;
    boolean isInline = false;
    if ((args[3].type == 1)) {
      isInline = true;
    } else {
      if (!((boolean) args[4].internalValue)) {
        nd1[5] = "output matrix was unrecognized type.";
        return vm.globalNull;
      }
    }
    int m1width = ((int) nd1[1]);
    int m1height = ((int) nd1[2]);
    int m2width = ((int) nd2[1]);
    int m2height = ((int) nd2[2]);
    int m3width = m2width;
    int m3height = m1height;
    if ((m1width != m2height)) {
      nd1[5] = "Matrix size mismatch";
      return vm.globalNull;
    }
    double[] m1data = ((double[]) nd1[0]);
    double[] m2data = ((double[]) nd2[0]);
    Object[] nd3 = null;
    if (isInline) {
      nd3 = nd1;
      if ((m2width != m2height)) {
        nd1[5] = "You can only multiply a matrix inline with a square matrix.";
        return vm.globalNull;
      }
    } else {
      obj = ((ObjectInstance) args[3].internalValue);
      nd3 = obj.nativeData;
      if (((((int) nd3[1]) != m3width) || (((int) nd3[2]) != m3height))) {
        nd1[5] = "Output matrix is incorrect size.";
        return vm.globalNull;
      }
    }
    nd3[4] = true;
    double[] m3data = ((double[]) nd3[6]);
    int x = 0;
    int y = 0;
    int i = 0;
    int m1index = 0;
    int m2index = 0;
    int m3index = 0;
    double value = 0.0;
    y = 0;
    while ((y < m3height)) {
      x = 0;
      while ((x < m3width)) {
        value = 0.0;
        m1index = (y * m1height);
        m2index = x;
        i = 0;
        while ((i < m1width)) {
          value += (m1data[m1index] * m2data[m2index]);
          m1index += 1;
          m2index += m2width;
          i += 1;
        }
        m3data[m3index] = value;
        m3index += 1;
        x += 1;
      }
      y += 1;
    }
    Object t = nd3[0];
    nd3[0] = nd3[6];
    nd3[6] = t;
    return args[0];
  }

  public static Value lib_matrices_multiplyScalar(VmContext vm, Value[] args) {
    ObjectInstance obj = ((ObjectInstance) args[0].internalValue);
    Object[] nd = obj.nativeData;
    boolean isInline = (args[2].type == 1);
    double[] m1data = ((double[]) nd[0]);
    double[] m2data = m1data;
    if (isInline) {
      nd[4] = true;
    } else {
      if (!((boolean) args[3].internalValue)) {
        nd[5] = "output must be a matrix instance";
        return vm.globalNull;
      } else {
        obj = ((ObjectInstance) args[2].internalValue);
        Object[] nd2 = obj.nativeData;
        if (((((int) nd[1]) != ((int) nd2[1])) || (((int) nd[2]) != ((int) nd2[2])))) {
          nd[5] = "output matrix must be the same size.";
          return vm.globalNull;
        }
        m2data = ((double[]) nd2[0]);
        nd2[4] = true;
      }
    }
    double scalar = 0.0;
    if ((args[1].type == 4)) {
      scalar = ((double) args[1].internalValue);
    } else {
      if ((args[1].type == 3)) {
        scalar = (0.0 + ((int) args[1].internalValue));
      } else {
        nd[5] = "scalar must be a number";
        return vm.globalNull;
      }
    }
    int i = 0;
    int length = m1data.length;
    i = 0;
    while ((i < length)) {
      m2data[i] = (m1data[i] * scalar);
      i += 1;
    }
    return args[0];
  }

  public static Value lib_matrices_setValue(VmContext vm, Value[] args) {
    ObjectInstance obj = ((ObjectInstance) args[0].internalValue);
    Object[] nd = obj.nativeData;
    if (((args[1].type != 3) || (args[2].type != 3))) {
      nd[5] = "Invalid coordinates";
      return vm.globalNull;
    }
    int x = ((int) args[1].internalValue);
    int y = ((int) args[2].internalValue);
    int width = ((int) nd[1]);
    int height = ((int) nd[2]);
    if (((x < 0) || (x >= width) || (y < 0) || (y >= height))) {
      nd[5] = "Coordinates out of range.";
      return vm.globalNull;
    }
    double value = 0.0;
    if ((args[3].type == 4)) {
      value = ((double) args[3].internalValue);
    } else {
      if ((args[3].type == 3)) {
        value = (0.0 + ((int) args[3].internalValue));
      } else {
        nd[5] = "Value must be a number.";
        return vm.globalNull;
      }
    }
    int index = ((y * width) + x);
    double[] data = ((double[]) nd[0]);
    Value[] valueArray = ((Value[]) nd[3]);
    data[index] = value;
    valueArray[index] = org.crayonlang.interpreter.vm.CrayonWrapper.buildFloat(vm.globals, value);
    return args[0];
  }

  public static Value lib_matrices_toVector(VmContext vm, Value[] args) {
    ObjectInstance obj = ((ObjectInstance) args[0].internalValue);
    Object[] nd = obj.nativeData;
    double[] data = ((double[]) nd[0]);
    int width = ((int) nd[1]);
    int height = ((int) nd[2]);
    int length = (width * height);
    if ((args[1].type != 6)) {
      nd[5] = "Output argument must be a list";
      return vm.globalNull;
    }
    ListImpl output = ((ListImpl) args[1].internalValue);
    while ((output.size < length)) {
      org.crayonlang.interpreter.vm.CrayonWrapper.addToList(output, vm.globalNull);
    }
    double value = 0.0;
    Value toList = null;
    int i = 0;
    while ((i < length)) {
      value = data[i];
      if ((value == 0)) {
        toList = vm.globals.floatZero;
      } else {
        if ((value == 1)) {
          toList = vm.globals.floatOne;
        } else {
          toList = new Value(4, data[i]);
        }
      }
      output.array[i] = toList;
      i += 1;
    }
    return args[1];
  }
}
