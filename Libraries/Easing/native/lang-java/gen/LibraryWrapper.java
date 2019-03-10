package org.crayonlang.libraries.easing;

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

  public static Value lib_easing_apply_pts(VmContext vm, Value[] args) {
    ListImpl sampleValues = ((ListImpl) args[1].internalValue);
    int _len = sampleValues.size;
    double[] samples = new double[_len];
    int i = 0;
    while ((i < _len)) {
      samples[i] = ((double) sampleValues.array[i].internalValue);
      i += 1;
    }
    samples[0] = 0.0;
    samples[(_len - 1)] = 1.0;
    ObjectInstance o = ((ObjectInstance) args[0].internalValue);
    o.nativeObject = new EasingSampling(_len, samples);
    return vm.globals.valueNull;
  }

  public static Value lib_easing_interpolate(VmContext vm, Value[] args) {
    Value arg2 = args[1];
    Value arg3 = args[2];
    Value arg4 = args[3];
    Value arg5 = args[4];
    Value arg6 = args[5];
    ObjectInstance o = ((ObjectInstance) args[0].internalValue);
    EasingSampling es = ((EasingSampling) o.nativeObject);
    double[] samples = es.samples;
    int _len = es.sampleCount;
    int int1 = ((int) args[6].internalValue);
    double float1 = 0.0;
    double float2 = 0.0;
    double float3 = 0.0;
    if ((arg4.type == 3)) {
      float1 = (0.0 + ((int) arg4.internalValue));
    } else {
      if ((arg4.type == 4)) {
        float1 = ((double) arg4.internalValue);
      } else {
        return vm.globals.valueNull;
      }
    }
    if ((arg5.type == 3)) {
      float2 = (0.0 + ((int) arg5.internalValue));
    } else {
      if ((arg5.type == 4)) {
        float2 = ((double) arg5.internalValue);
      } else {
        return vm.globals.valueNull;
      }
    }
    boolean bool1 = false;
    boolean bool2 = false;
    boolean first = false;
    if ((int1 == 2)) {
      first = true;
      if (((float1 * 2.0) > float2)) {
        float1 = ((float2 - float1) * 2);
        bool1 = true;
        bool2 = true;
      } else {
        float1 *= 2.0;
      }
    } else {
      if ((int1 == 1)) {
        float1 = (float2 - float1);
        bool1 = true;
      }
    }
    if ((float2 == 0)) {
      float1 = samples[0];
    } else {
      if ((float2 < 0)) {
        float2 = -float2;
        float1 = -float1;
      }
      if ((float1 >= float2)) {
        float1 = samples[(_len - 1)];
      } else {
        if ((float1 < 0)) {
          float1 = samples[0];
        } else {
          float1 = float1 / float2;
          if ((_len > 2)) {
            float2 = (float1 * _len);
            int index = ((int) float2);
            float2 -= index;
            float1 = samples[index];
            if (((index < (_len - 1)) && (float2 > 0))) {
              float3 = samples[(index + 1)];
              float1 = ((float1 * (1 - float2)) + (float3 * float2));
            }
          }
        }
      }
    }
    if ((arg2.type == 3)) {
      float2 = (0.0 + ((int) arg2.internalValue));
    } else {
      if ((arg2.type == 4)) {
        float2 = ((double) arg2.internalValue);
      } else {
        return vm.globals.valueNull;
      }
    }
    if ((arg3.type == 3)) {
      float3 = (0.0 + ((int) arg3.internalValue));
    } else {
      if ((arg3.type == 4)) {
        float3 = ((double) arg3.internalValue);
      } else {
        return vm.globals.valueNull;
      }
    }
    if (bool1) {
      float1 = (1.0 - float1);
    }
    if (first) {
      float1 *= 0.5;
    }
    if (bool2) {
      float1 += 0.5;
    }
    float1 = ((float1 * float3) + ((1 - float1) * float2));
    if (((arg6.type == 2) && ((boolean) arg6.internalValue))) {
      return org.crayonlang.interpreter.vm.CrayonWrapper.buildInteger(vm.globals, ((int) (float1 + 0.5)));
    }
    return org.crayonlang.interpreter.vm.CrayonWrapper.buildFloat(vm.globals, float1);
  }
}
