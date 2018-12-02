package org.crayonlang.libraries.fileiocommon;

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

  public static Value lib_fileiocommon_directoryCreate(VmContext vm, Value[] args) {
    boolean bool1 = false;
    int i = 0;
    int int1 = 0;
    ArrayList<String> stringList1 = null;
    Value hostObject = args[0];
    String path = ((String) args[1].internalValue);
    if (((boolean) args[2].internalValue)) {
      int1 = 0;
      if (!FileIOHelper.directoryExists(FileIOHelper.getDirRoot(path))) {
        int1 = 4;
      } else {
        stringList1 = new ArrayList<String>();
        bool1 = true;
        while ((bool1 && !FileIOHelper.directoryExists(path))) {
          stringList1.add(path);
          int1 = FileIOHelper.getDirParent(path, PST_stringBuffer16);
          path = PST_stringBuffer16[0];
          if ((int1 != 0)) {
            bool1 = false;
          }
        }
        if (bool1) {
          i = (stringList1.size() - 1);
          while ((i >= 0)) {
            path = stringList1.get(i);
            int1 = FileIOHelper.createDirectory(path);
            if ((int1 != 0)) {
              i = -1;
            }
            i -= 1;
          }
        }
      }
    } else {
      int1 = FileIOHelper.createDirectory(path);
    }
    return org.crayonlang.interpreter.vm.CrayonWrapper.buildInteger(vm.globals, int1);
  }

  public static Value lib_fileiocommon_directoryDelete(VmContext vm, Value[] args) {
    int sc = FileIOHelper.deleteDirectory(((String) args[1].internalValue));
    return org.crayonlang.interpreter.vm.CrayonWrapper.buildInteger(vm.globals, sc);
  }

  public static Value lib_fileiocommon_directoryList(VmContext vm, Value[] args) {
    Value diskhost = args[0];
    String path = ((String) args[1].internalValue);
    boolean useFullPath = ((boolean) args[2].internalValue);
    ListImpl outputList = ((ListImpl) args[3].internalValue);
    ArrayList<String> stringList1 = new ArrayList<String>();
    int sc = FileIOHelper.getDirectoryList(path, useFullPath, stringList1);
    if ((sc == 0)) {
      int i = 0;
      while ((i < stringList1.size())) {
        org.crayonlang.interpreter.vm.CrayonWrapper.addToList(outputList, org.crayonlang.interpreter.vm.CrayonWrapper.buildString(vm.globals, stringList1.get(i)));
        i += 1;
      }
    }
    return org.crayonlang.interpreter.vm.CrayonWrapper.buildInteger(vm.globals, sc);
  }

  public static Value lib_fileiocommon_directoryMove(VmContext vm, Value[] args) {
    int statusCode = FileIOHelper.moveDirectory(((String) args[1].internalValue), ((String) args[2].internalValue));
    return org.crayonlang.interpreter.vm.CrayonWrapper.buildInteger(vm.globals, statusCode);
  }

  public static Value lib_fileiocommon_fileDelete(VmContext vm, Value[] args) {
    int statusCode = FileIOHelper.fileDelete(((String) args[1].internalValue));
    return org.crayonlang.interpreter.vm.CrayonWrapper.buildInteger(vm.globals, statusCode);
  }

  public static Value lib_fileiocommon_fileInfo(VmContext vm, Value[] args) {
    int mask = ((int) args[2].internalValue);
    FileIOHelper.getFileInfo(((String) args[1].internalValue), mask, PST_intBuffer16, PST_floatBuffer16);
    ListImpl outputList = ((ListImpl) args[3].internalValue);
    org.crayonlang.interpreter.vm.CrayonWrapper.clearList(outputList);
    VmGlobals globals = vm.globals;
    org.crayonlang.interpreter.vm.CrayonWrapper.addToList(outputList, org.crayonlang.interpreter.vm.CrayonWrapper.buildBoolean(globals, (PST_intBuffer16[0] > 0)));
    org.crayonlang.interpreter.vm.CrayonWrapper.addToList(outputList, org.crayonlang.interpreter.vm.CrayonWrapper.buildBoolean(globals, (PST_intBuffer16[1] > 0)));
    if (((mask & 1) != 0)) {
      org.crayonlang.interpreter.vm.CrayonWrapper.addToList(outputList, org.crayonlang.interpreter.vm.CrayonWrapper.buildInteger(globals, PST_intBuffer16[2]));
    } else {
      org.crayonlang.interpreter.vm.CrayonWrapper.addToList(outputList, globals.valueNull);
    }
    if (((mask & 2) != 0)) {
      org.crayonlang.interpreter.vm.CrayonWrapper.addToList(outputList, org.crayonlang.interpreter.vm.CrayonWrapper.buildBoolean(globals, (PST_intBuffer16[3] > 0)));
    } else {
      org.crayonlang.interpreter.vm.CrayonWrapper.addToList(outputList, globals.valueNull);
    }
    if (((mask & 4) != 0)) {
      org.crayonlang.interpreter.vm.CrayonWrapper.addToList(outputList, org.crayonlang.interpreter.vm.CrayonWrapper.buildFloat(globals, PST_floatBuffer16[0]));
    } else {
      org.crayonlang.interpreter.vm.CrayonWrapper.addToList(outputList, globals.valueNull);
    }
    if (((mask & 8) != 0)) {
      org.crayonlang.interpreter.vm.CrayonWrapper.addToList(outputList, org.crayonlang.interpreter.vm.CrayonWrapper.buildFloat(globals, PST_floatBuffer16[1]));
    } else {
      org.crayonlang.interpreter.vm.CrayonWrapper.addToList(outputList, globals.valueNull);
    }
    return args[3];
  }

  public static Value lib_fileiocommon_fileMove(VmContext vm, Value[] args) {
    int statusCode = FileIOHelper.fileMove(((String) args[1].internalValue), ((String) args[2].internalValue), ((boolean) args[3].internalValue), ((boolean) args[4].internalValue));
    return org.crayonlang.interpreter.vm.CrayonWrapper.buildInteger(vm.globals, statusCode);
  }

  public static Value lib_fileiocommon_fileRead(VmContext vm, Value[] args) {
    Value diskHostObject = args[0];
    String sandboxedPath = ((String) args[1].internalValue);
    boolean readDataAsBytes = ((boolean) args[2].internalValue);
    ListImpl outputList = ((ListImpl) args[3].internalValue);
    ArrayList<Value> tList = new ArrayList<Value>();
    int statusCode = FileIOHelper.fileRead(sandboxedPath, readDataAsBytes, PST_stringBuffer16, vm.globals.positiveIntegers, tList);
    if (((statusCode == 0) && !readDataAsBytes)) {
      org.crayonlang.interpreter.vm.CrayonWrapper.addToList(outputList, org.crayonlang.interpreter.vm.CrayonWrapper.buildString(vm.globals, PST_stringBuffer16[0]));
    } else {
      Value t = org.crayonlang.interpreter.vm.CrayonWrapper.buildList(tList);
      ListImpl tListImpl = ((ListImpl) t.internalValue);
      outputList.array = tListImpl.array;
      outputList.capacity = tListImpl.capacity;
      outputList.size = tList.size();
    }
    return org.crayonlang.interpreter.vm.CrayonWrapper.buildInteger(vm.globals, statusCode);
  }

  public static Value lib_fileiocommon_fileWrite(VmContext vm, Value[] args) {
    Value[] ints = vm.globals.positiveIntegers;
    if ((args[3].type != 3)) {
      return ints[3];
    }
    int statusCode = 0;
    String contentString = null;
    Object byteArrayRef = null;
    int format = ((int) args[3].internalValue);
    if ((format == 0)) {
      byteArrayRef = lib_fileiocommon_listToBytes(((ArrayList<Value>) args[2].internalValue));
      if ((byteArrayRef == null)) {
        return ints[6];
      }
    } else {
      if ((args[2].type != 5)) {
        return ints[6];
      } else {
        contentString = ((String) args[2].internalValue);
      }
    }
    if ((statusCode == 0)) {
      statusCode = FileIOHelper.fileWrite(((String) args[1].internalValue), format, contentString, byteArrayRef);
    }
    return org.crayonlang.interpreter.vm.CrayonWrapper.buildInteger(vm.globals, statusCode);
  }

  public static Value lib_fileiocommon_getCurrentDirectory(VmContext vm, Value[] args) {
    return org.crayonlang.interpreter.vm.CrayonWrapper.buildString(vm.globals, FileIOHelper.getCurrentDirectory());
  }

  public static Object lib_fileiocommon_getDiskObject(Value diskObjectArg) {
    ObjectInstance objInst = ((ObjectInstance) diskObjectArg.internalValue);
    return objInst.nativeData[0];
  }

  public static Value lib_fileiocommon_getUserDirectory(VmContext vm, Value[] args) {
    return org.crayonlang.interpreter.vm.CrayonWrapper.buildString(vm.globals, FileIOHelper.getUserDirectory());
  }

  public static Value lib_fileiocommon_initializeDisk(VmContext vm, Value[] args) {
    ObjectInstance objInstance1 = ((ObjectInstance) args[0].internalValue);
    Object[] objArray1 = new Object[1];
    objInstance1.nativeData = objArray1;
    Object object1 = TranslationHelper.alwaysFalse();
    objArray1[0] = object1;
    return vm.globals.valueNull;
  }

  public static Value lib_fileiocommon_isWindows(VmContext vm, Value[] args) {
    if (FileIOHelper.isWindows()) {
      return vm.globals.boolTrue;
    }
    return vm.globals.boolFalse;
  }

  public static Object lib_fileiocommon_listToBytes(ArrayList<Value> listOfMaybeInts) {
    byte[] bytes = new byte[listOfMaybeInts.size()];
    Value intValue = null;
    int byteValue = 0;
    int i = (listOfMaybeInts.size() - 1);
    while ((i >= 0)) {
      intValue = listOfMaybeInts.get(i);
      if ((intValue.type != 3)) {
        return null;
      }
      byteValue = ((int) intValue.internalValue);
      if ((byteValue >= 256)) {
        return null;
      }
      if ((byteValue < 0)) {
        if ((byteValue < -128)) {
          return null;
        }
        byteValue += 256;
      }
      bytes[i] = ((byte) byteValue);
      i -= 1;
    }
    return bytes;
  }

  public static Value lib_fileiocommon_textToLines(VmContext vm, Value[] args) {
    lib_fileiocommon_textToLinesImpl(vm.globals, ((String) args[0].internalValue), ((ListImpl) args[1].internalValue));
    return args[1];
  }

  public static int lib_fileiocommon_textToLinesImpl(VmGlobals globals, String text, ListImpl output) {
    ArrayList<String> stringList = new ArrayList<String>();
    FileIOHelper.textToLines(text, stringList);
    int _len = stringList.size();
    int i = 0;
    while ((i < _len)) {
      org.crayonlang.interpreter.vm.CrayonWrapper.addToList(output, org.crayonlang.interpreter.vm.CrayonWrapper.buildString(globals, stringList.get(i)));
      i += 1;
    }
    return 0;
  }
}
