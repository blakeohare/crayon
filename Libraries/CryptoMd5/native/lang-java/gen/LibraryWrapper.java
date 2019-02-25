package org.crayonlang.libraries.cryptomd5;

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

  public static Value lib_md5_addBytes(VmContext vm, Value[] args) {
    ObjectInstance obj = ((ObjectInstance) args[0].internalValue);
    ListImpl fromByteList = ((ListImpl) args[1].internalValue);
    ArrayList<Integer> toByteList = ((ArrayList<Integer>) obj.nativeData[0]);
    int length = fromByteList.size;
    int i = 0;
    while ((i < length)) {
      toByteList.add(fromByteList.array[i].intValue);
      i += 1;
    }
    return vm.globalFalse;
  }

  public static int lib_md5_bitShiftRight(int value, int amount) {
    if ((amount == 0)) {
      return value;
    }
    if ((value > 0)) {
      return (value >> amount);
    }
    int mask = 2147483647;
    return (((value >> amount)) & ((mask >> ((amount - 1)))));
  }

  public static int lib_md5_bitwiseNot(int x) {
    return (-x - 1);
  }

  public static int lib_md5_createWordsForBlock(int startIndex, ArrayList<Integer> byteList, int[] mWords) {
    int i = 0;
    while ((i < 64)) {
      mWords[(i >> 2)] = (((byteList.get((startIndex + i)) << 24)) | ((byteList.get((startIndex + i + 1)) << 16)) | ((byteList.get((startIndex + i + 2)) << 8)) | (byteList.get((startIndex + i + 3))));
      i += 4;
    }
    return 0;
  }

  public static Value lib_md5_digestMd5(VmContext vm, Value[] args) {
    ObjectInstance obj = ((ObjectInstance) args[0].internalValue);
    ListImpl output = ((ListImpl) args[1].internalValue);
    ArrayList<Integer> byteList = ((ArrayList<Integer>) obj.nativeData[0]);
    int[] resultBytes = lib_md5_digestMd5Impl(byteList);
    int i = 0;
    while ((i < 16)) {
      int b = resultBytes[i];
      org.crayonlang.interpreter.vm.CrayonWrapper.addToList(output, vm.globals.positiveIntegers[b]);
      i += 1;
    }
    return args[1];
  }

  public static int[] lib_md5_digestMd5Impl(ArrayList<Integer> inputBytes) {
    int[] shiftTable = new int[64];
    int[] K = new int[64];
    int i = 0;
    while ((i < 16)) {
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
    K[0] = lib_md5_uint32Hack(55146, 42104);
    K[1] = lib_md5_uint32Hack(59591, 46934);
    K[2] = lib_md5_uint32Hack(9248, 28891);
    K[3] = lib_md5_uint32Hack(49597, 52974);
    K[4] = lib_md5_uint32Hack(62844, 4015);
    K[5] = lib_md5_uint32Hack(18311, 50730);
    K[6] = lib_md5_uint32Hack(43056, 17939);
    K[7] = lib_md5_uint32Hack(64838, 38145);
    K[8] = lib_md5_uint32Hack(27008, 39128);
    K[9] = lib_md5_uint32Hack(35652, 63407);
    K[10] = lib_md5_uint32Hack(65535, 23473);
    K[11] = lib_md5_uint32Hack(35164, 55230);
    K[12] = lib_md5_uint32Hack(27536, 4386);
    K[13] = lib_md5_uint32Hack(64920, 29075);
    K[14] = lib_md5_uint32Hack(42617, 17294);
    K[15] = lib_md5_uint32Hack(18868, 2081);
    K[16] = lib_md5_uint32Hack(63006, 9570);
    K[17] = lib_md5_uint32Hack(49216, 45888);
    K[18] = lib_md5_uint32Hack(9822, 23121);
    K[19] = lib_md5_uint32Hack(59830, 51114);
    K[20] = lib_md5_uint32Hack(54831, 4189);
    K[21] = lib_md5_uint32Hack(580, 5203);
    K[22] = lib_md5_uint32Hack(55457, 59009);
    K[23] = lib_md5_uint32Hack(59347, 64456);
    K[24] = lib_md5_uint32Hack(8673, 52710);
    K[25] = lib_md5_uint32Hack(49975, 2006);
    K[26] = lib_md5_uint32Hack(62677, 3463);
    K[27] = lib_md5_uint32Hack(17754, 5357);
    K[28] = lib_md5_uint32Hack(43491, 59653);
    K[29] = lib_md5_uint32Hack(64751, 41976);
    K[30] = lib_md5_uint32Hack(26479, 729);
    K[31] = lib_md5_uint32Hack(36138, 19594);
    K[32] = lib_md5_uint32Hack(65530, 14658);
    K[33] = lib_md5_uint32Hack(34673, 63105);
    K[34] = lib_md5_uint32Hack(28061, 24866);
    K[35] = lib_md5_uint32Hack(64997, 14348);
    K[36] = lib_md5_uint32Hack(42174, 59972);
    K[37] = lib_md5_uint32Hack(19422, 53161);
    K[38] = lib_md5_uint32Hack(63163, 19296);
    K[39] = lib_md5_uint32Hack(48831, 48240);
    K[40] = lib_md5_uint32Hack(10395, 32454);
    K[41] = lib_md5_uint32Hack(60065, 10234);
    K[42] = lib_md5_uint32Hack(54511, 12421);
    K[43] = lib_md5_uint32Hack(1160, 7429);
    K[44] = lib_md5_uint32Hack(55764, 53305);
    K[45] = lib_md5_uint32Hack(59099, 39397);
    K[46] = lib_md5_uint32Hack(8098, 31992);
    K[47] = lib_md5_uint32Hack(50348, 22117);
    K[48] = lib_md5_uint32Hack(62505, 8772);
    K[49] = lib_md5_uint32Hack(17194, 65431);
    K[50] = lib_md5_uint32Hack(43924, 9127);
    K[51] = lib_md5_uint32Hack(64659, 41017);
    K[52] = lib_md5_uint32Hack(25947, 22979);
    K[53] = lib_md5_uint32Hack(36620, 52370);
    K[54] = lib_md5_uint32Hack(65519, 62589);
    K[55] = lib_md5_uint32Hack(34180, 24017);
    K[56] = lib_md5_uint32Hack(28584, 32335);
    K[57] = lib_md5_uint32Hack(65068, 59104);
    K[58] = lib_md5_uint32Hack(41729, 17172);
    K[59] = lib_md5_uint32Hack(19976, 4513);
    K[60] = lib_md5_uint32Hack(63315, 32386);
    K[61] = lib_md5_uint32Hack(48442, 62005);
    K[62] = lib_md5_uint32Hack(10967, 53947);
    K[63] = lib_md5_uint32Hack(60294, 54161);
    int A = lib_md5_uint32Hack(26437, 8961);
    int B = lib_md5_uint32Hack(61389, 43913);
    int C = lib_md5_uint32Hack(39098, 56574);
    int D = lib_md5_uint32Hack(4146, 21622);
    int originalLength = inputBytes.size();
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
    inputBytes.add((originalLength & 255));
    int[] mWords = new int[16];
    int mask32 = lib_md5_uint32Hack(65535, 65535);
    int chunkIndex = 0;
    while ((chunkIndex < inputBytes.size())) {
      lib_md5_createWordsForBlock(chunkIndex, inputBytes, mWords);
      int A_init = A;
      int B_init = B;
      int C_init = C;
      int D_init = D;
      int j = 0;
      while ((j < 64)) {
        A = lib_md5_magicShuffle(mWords, K, shiftTable, mask32, A, B, C, D, j);
        D = lib_md5_magicShuffle(mWords, K, shiftTable, mask32, D, A, B, C, (j | 1));
        C = lib_md5_magicShuffle(mWords, K, shiftTable, mask32, C, D, A, B, (j | 2));
        B = lib_md5_magicShuffle(mWords, K, shiftTable, mask32, B, C, D, A, (j | 3));
        j += 4;
      }
      A = (((A_init + A)) & mask32);
      B = (((B_init + B)) & mask32);
      C = (((C_init + C)) & mask32);
      D = (((D_init + D)) & mask32);
      chunkIndex += 64;
    }
    int[] output = new int[16];
    output[0] = ((A >> 24) & 255);
    output[1] = ((A >> 16) & 255);
    output[2] = ((A >> 8) & 255);
    output[3] = (A & 255);
    output[4] = ((B >> 24) & 255);
    output[5] = ((B >> 16) & 255);
    output[6] = ((B >> 8) & 255);
    output[7] = (B & 255);
    output[8] = ((C >> 24) & 255);
    output[9] = ((C >> 16) & 255);
    output[10] = ((C >> 8) & 255);
    output[11] = (C & 255);
    output[12] = ((D >> 24) & 255);
    output[13] = ((D >> 16) & 255);
    output[14] = ((D >> 8) & 255);
    output[15] = (D & 255);
    return output;
  }

  public static Value lib_md5_initHash(VmContext vm, Value[] args) {
    ObjectInstance obj = ((ObjectInstance) args[0].internalValue);
    obj.nativeData = new Object[1];
    obj.nativeData[0] = new ArrayList<Integer>();
    return vm.globalNull;
  }

  public static int lib_md5_leftRotate(int value, int amt) {
    if ((amt == 0)) {
      return value;
    }
    int a = (value << amt);
    int b = lib_md5_bitShiftRight(value, (32 - amt));
    int result = (a | b);
    return result;
  }

  public static int lib_md5_magicShuffle(int[] mWords, int[] sineValues, int[] shiftValues, int mask32, int a, int b, int c, int d, int counter) {
    int roundNumber = (counter >> 4);
    int t = 0;
    int shiftAmount = shiftValues[counter];
    int sineValue = sineValues[counter];
    int mWord = 0;
    if ((roundNumber == 0)) {
      t = (((b & c)) | ((lib_md5_bitwiseNot(b) & d)));
      mWord = mWords[counter];
    } else {
      if ((roundNumber == 1)) {
        t = (((b & d)) | ((c & lib_md5_bitwiseNot(d))));
        mWord = mWords[((((5 * counter) + 1)) & 15)];
      } else {
        if ((roundNumber == 2)) {
          t = (b ^ c ^ d);
          mWord = mWords[((((3 * counter) + 5)) & 15)];
        } else {
          t = (c ^ ((b | lib_md5_bitwiseNot(d))));
          mWord = mWords[(((7 * counter)) & 15)];
        }
      }
    }
    t = (((a + t + mWord + sineValue)) & mask32);
    t = (b + lib_md5_leftRotate(t, shiftAmount));
    return (t & mask32);
  }

  public static int lib_md5_uint32Hack(int left, int right) {
    return (((left << 16)) | right);
  }
}
