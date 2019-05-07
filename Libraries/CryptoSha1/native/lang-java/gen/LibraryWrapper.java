package org.crayonlang.libraries.cryptosha1;

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

    int[] buffer = new int[outputLength];
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
    for (int i = 0; i < buffer.length; ++i) buffer[i] &= 255;

    // new String(buffer, UTF8) computes garbage surrogate pair values, so we must do this manually.
    java.util.ArrayList<Integer> codePoints = new java.util.ArrayList<Integer>();
    int b, cp;
    for (int i = 0; i < buffer.length; ++i) {
      b = buffer[i];
      if ((b & 0x80) == 0) {
        cp = b;
      } else if ((b & 0xe0) == 0xc0) {
        cp = ((b & 0x1f) << 6) + (buffer[i + 1] & 0x3f);
        ++i;
      } else if ((b & 0xf0) == 0xe0) {
        cp = ((b & 0x0f) << 12) + ((buffer[i + 1] & 0x3f) << 6) + (buffer[i + 2] & 0x3f);
        i += 2;
      } else {
        cp = ((b & 0x07) << 18) + ((buffer[i + 1] & 0x3f) << 12) + ((buffer[i + 2] & 0x3f) << 6) + (buffer[i + 3] & 0x3f);
        i += 3;
      }
      codePoints.add(cp);
    }
    StringBuilder sb = new StringBuilder();
    for (int i = 0; i < codePoints.size(); ++i) {
      cp = codePoints.get(i);
      if (cp < 0x10000) {
        sb.append((char)cp);
      } else {
        cp -= 0x10000;
        int s1 = 0xd800 | ((cp >> 10) & 0x03ff);
        int s2 = 0xdc00 | (cp & 0x03ff);
        sb.append(new String(new char[] { (char) s1, (char) s2 }));
      }
    }
    return sb.toString();
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
    int half = length / 2;
    char c;
    for (int i = 0; i < half; ++i) {
      c = output[i];
      output[i] = output[lengthMinusOne - i];
      output[lengthMinusOne - i] = c;
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

  public static int lib_cryptosha1_bitShiftRight(int value, int amount) {
    if ((amount == 0)) {
      return value;
    }
    int mask = 2147483647;
    value = (value & lib_cryptosha1_uint32Hack(65535, 65535));
    if ((value > 0)) {
      return (value >> amount);
    }
    return (((value >> amount)) & ((mask >> ((amount - 1)))));
  }

  public static int lib_cryptosha1_bitwiseNot(int x) {
    return (-x - 1);
  }

  public static int lib_cryptosha1_createWordsForBlock(int startIndex, ArrayList<Integer> byteList, int[] mWords) {
    int i = 0;
    while ((i < 64)) {
      mWords[(i >> 2)] = (((byteList.get((startIndex + i)) << 24)) | ((byteList.get((startIndex + i + 1)) << 16)) | ((byteList.get((startIndex + i + 2)) << 8)) | (byteList.get((startIndex + i + 3))));
      i += 4;
    }
    return 0;
  }

  public static Value lib_cryptosha1_digest(VmContext vm, Value[] args) {
    ObjectInstance obj = ((ObjectInstance) args[0].internalValue);
    ListImpl output = ((ListImpl) args[1].internalValue);
    ArrayList<Integer> byteList = ((ArrayList<Integer>) obj.nativeData[0]);
    int[] resultBytes = lib_cryptosha1_digestImpl(byteList);
    int i = 0;
    while ((i < 20)) {
      int b = resultBytes[i];
      org.crayonlang.interpreter.vm.CrayonWrapper.addToList(output, vm.globals.positiveIntegers[b]);
      i += 1;
    }
    return args[1];
  }

  public static int[] lib_cryptosha1_digestImpl(ArrayList<Integer> inputBytes) {
    int originalLength = (inputBytes.size() * 8);
    int h0 = lib_cryptosha1_uint32Hack(26437, 8961);
    int h1 = lib_cryptosha1_uint32Hack(61389, 43913);
    int h2 = lib_cryptosha1_uint32Hack(39098, 56574);
    int h3 = lib_cryptosha1_uint32Hack(4146, 21622);
    int h4 = lib_cryptosha1_uint32Hack(50130, 57840);
    inputBytes.add(128);
    while (((inputBytes.size() % 64) != 56)) {
      inputBytes.add(0);
    }
    inputBytes.add(0);
    inputBytes.add(0);
    inputBytes.add(0);
    inputBytes.add(0);
    inputBytes.add(((originalLength >> 24) & 255));
    inputBytes.add(((originalLength >> 16) & 255));
    inputBytes.add(((originalLength >> 8) & 255));
    inputBytes.add(((originalLength >> 0) & 255));
    int[] mWords = new int[80];
    int mask32 = lib_cryptosha1_uint32Hack(65535, 65535);
    int f = 0;
    int temp = 0;
    int k = 0;
    int[] kValues = new int[4];
    kValues[0] = lib_cryptosha1_uint32Hack(23170, 31129);
    kValues[1] = lib_cryptosha1_uint32Hack(28377, 60321);
    kValues[2] = lib_cryptosha1_uint32Hack(36635, 48348);
    kValues[3] = lib_cryptosha1_uint32Hack(51810, 49622);
    int chunkIndex = 0;
    while ((chunkIndex < inputBytes.size())) {
      lib_cryptosha1_createWordsForBlock(chunkIndex, inputBytes, mWords);
      int i = 16;
      while ((i < 80)) {
        mWords[i] = lib_cryptosha1_leftRotate((mWords[(i - 3)] ^ mWords[(i - 8)] ^ mWords[(i - 14)] ^ mWords[(i - 16)]), 1);
        i += 1;
      }
      int a = h0;
      int b = h1;
      int c = h2;
      int d = h3;
      int e = h4;
      int j = 0;
      while ((j < 80)) {
        if ((j < 20)) {
          f = (((b & c)) | ((lib_cryptosha1_bitwiseNot(b) & d)));
          k = kValues[0];
        } else {
          if ((j < 40)) {
            f = (b ^ c ^ d);
            k = kValues[1];
          } else {
            if ((j < 60)) {
              f = (((b & c)) | ((b & d)) | ((c & d)));
              k = kValues[2];
            } else {
              f = (b ^ c ^ d);
              k = kValues[3];
            }
          }
        }
        temp = (lib_cryptosha1_leftRotate(a, 5) + f + e + k + mWords[j]);
        e = d;
        d = c;
        c = lib_cryptosha1_leftRotate(b, 30);
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
    int[] output = new int[20];
    output[0] = ((h0 >> 24) & 255);
    output[1] = ((h0 >> 16) & 255);
    output[2] = ((h0 >> 8) & 255);
    output[3] = (h0 & 255);
    output[4] = ((h1 >> 24) & 255);
    output[5] = ((h1 >> 16) & 255);
    output[6] = ((h1 >> 8) & 255);
    output[7] = (h1 & 255);
    output[8] = ((h2 >> 24) & 255);
    output[9] = ((h2 >> 16) & 255);
    output[10] = ((h2 >> 8) & 255);
    output[11] = (h2 & 255);
    output[12] = ((h3 >> 24) & 255);
    output[13] = ((h3 >> 16) & 255);
    output[14] = ((h3 >> 8) & 255);
    output[15] = (h3 & 255);
    output[16] = ((h4 >> 24) & 255);
    output[17] = ((h4 >> 16) & 255);
    output[18] = ((h4 >> 8) & 255);
    output[19] = (h4 & 255);
    return output;
  }

  public static int lib_cryptosha1_leftRotate(int value, int amt) {
    if ((amt == 0)) {
      return value;
    }
    int a = (value << amt);
    int b = lib_cryptosha1_bitShiftRight(value, (32 - amt));
    int result = (a | b);
    return result;
  }

  public static int lib_cryptosha1_uint32Hack(int left, int right) {
    return (((left << 16)) | right);
  }
}
