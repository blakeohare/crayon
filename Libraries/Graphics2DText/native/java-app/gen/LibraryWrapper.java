package org.crayonlang.libraries.graphics2dtext;

import java.util.ArrayList;
import java.util.HashMap;
import org.crayonlang.interpreter.structs.*;

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

  public static Value lib_graphics2dtext_createNativeFont(VmContext vm, Value[] args) {
    Value[] ints = vm.globals.positiveIntegers;
    ObjectInstance nf = ((ObjectInstance) args[0].internalValue);
    Object[] nfOut = nf.nativeData;
    int fontType = ((int) args[1].internalValue);
    String fontPath = "";
    if ((fontType == 0)) {
      fontType = ((int) args[2].internalValue);
    } else {
      fontPath = ((String) args[2].internalValue);
      if ((fontType == 1)) {
        Value res = org.crayonlang.interpreter.vm.CrayonWrapper.resource_manager_getResourceOfType(vm, fontPath, "TTF");
        if ((res.type == 1)) {
          return ints[2];
        }
        ListImpl resList = ((ListImpl) res.internalValue);
        if (!(org.crayonlang.interpreter.vm.CrayonWrapper.getItemFromList(resList, 0).intValue == 1)) {
          return ints[2];
        }
        fontPath = ((String) org.crayonlang.interpreter.vm.CrayonWrapper.getItemFromList(resList, 1).internalValue);
      }
    }
    int fontClass = 0;
    int fontSize = ((int) args[3].internalValue);
    int red = ((int) args[4].internalValue);
    int green = ((int) args[5].internalValue);
    int blue = ((int) args[6].internalValue);
    int styleBitmask = ((int) args[7].internalValue);
    int isBold = (styleBitmask & 1);
    int isItalic = (styleBitmask & 2);
    nfOut[0] = Graphics2DTextHelper.createNativeFont(fontType, fontClass, fontPath, fontSize, (isBold > 0), (isItalic > 0));
    if ((nfOut[0] == null)) {
      if ((fontType == 3)) {
        return ints[1];
      }
      return ints[2];
    }
    return ints[0];
  }

  public static Value lib_graphics2dtext_getNativeFontUniqueKey(VmContext vm, Value[] args) {
    ListImpl list1 = ((ListImpl) args[7].internalValue);
    ArrayList<Value> output = new ArrayList<Value>();
    Graphics2DTextHelper.addAll(output, args[0], args[1], args[2], args[6]);
    ListImpl list2 = ((ListImpl) org.crayonlang.interpreter.vm.CrayonWrapper.buildList(output).internalValue);
    list1.array = list2.array;
    list1.capacity = list2.capacity;
    list1.size = list2.size;
    return vm.globalNull;
  }

  public static Value lib_graphics2dtext_glGenerateAndLoadTexture(VmContext vm, Value[] args) {
    return vm.globalNull;
  }

  public static Value lib_graphics2dtext_glRenderCharTile(VmContext vm, Value[] args) {
    return vm.globalTrue;
  }

  public static Value lib_graphics2dtext_glRenderTextSurface(VmContext vm, Value[] args) {
    return vm.globalNull;
  }

  public static Value lib_graphics2dtext_glSetNativeDataIntArray(VmContext vm, Value[] args) {
    return vm.globalNull;
  }

  public static Value lib_graphics2dtext_isDynamicFontLoaded(VmContext vm, Value[] args) {
    return vm.globalTrue;
  }

  public static Value lib_graphics2dtext_isGlRenderer(VmContext vm, Value[] args) {
    return vm.globalFalse;
  }

  public static Value lib_graphics2dtext_isResourceAvailable(VmContext vm, Value[] args) {
    String path = ((String) args[0].internalValue);
    Value res = org.crayonlang.interpreter.vm.CrayonWrapper.resource_manager_getResourceOfType(vm, path, "TTF");
    if ((res.type == 1)) {
      return vm.globalFalse;
    }
    ListImpl resList = ((ListImpl) res.internalValue);
    if (!(org.crayonlang.interpreter.vm.CrayonWrapper.getItemFromList(resList, 0).intValue == 1)) {
      return vm.globalFalse;
    }
    return vm.globalTrue;
  }

  public static Value lib_graphics2dtext_isSystemFontPresent(VmContext vm, Value[] args) {
    return org.crayonlang.interpreter.vm.CrayonWrapper.buildBoolean(vm.globals, Graphics2DTextHelper.isSystemFontAvailable(((String) args[0].internalValue)));
  }

  public static Value lib_graphics2dtext_renderText(VmContext vm, Value[] args) {
    ListImpl sizeOut = ((ListImpl) args[0].internalValue);
    ObjectInstance textSurface = ((ObjectInstance) args[1].internalValue);
    Object[] imageOut = textSurface.nativeData;
    Object nativeFont = (((ObjectInstance) args[2].internalValue)).nativeData[0];
    int sourceType = ((int) args[3].internalValue);
    int fontClass = 0;
    String fontPath = "";
    if ((sourceType == 0)) {
      fontClass = ((int) args[4].internalValue);
    } else {
      fontPath = ((String) args[4].internalValue);
    }
    int fontSize = ((int) args[5].internalValue);
    int fontStyle = ((int) args[6].internalValue);
    int isBold = (fontStyle & 1);
    int isItalic = (fontStyle & 2);
    int red = ((int) args[7].internalValue);
    int green = ((int) args[8].internalValue);
    int blue = ((int) args[9].internalValue);
    String text = ((String) args[10].internalValue);
    Object bmp = Graphics2DTextHelper.renderText(PST_intBuffer16, nativeFont, red, green, blue, text);
    Object[] spoofedNativeData = new Object[4];
    spoofedNativeData[3] = bmp;
    Object[] spoofedNativeData2 = new Object[1];
    spoofedNativeData2[0] = spoofedNativeData;
    imageOut[0] = spoofedNativeData2;
    org.crayonlang.interpreter.vm.CrayonWrapper.setItemInList(sizeOut, 0, org.crayonlang.interpreter.vm.CrayonWrapper.buildInteger(vm.globals, PST_intBuffer16[0]));
    org.crayonlang.interpreter.vm.CrayonWrapper.setItemInList(sizeOut, 1, org.crayonlang.interpreter.vm.CrayonWrapper.buildInteger(vm.globals, PST_intBuffer16[1]));
    return vm.globalNull;
  }

  public static Value lib_graphics2dtext_simpleBlit(VmContext vm, Value[] args) {
    Object nativeBlittableBitmap = (((ObjectInstance) args[0].internalValue)).nativeData[0];
    Object[] drawQueueNativeData = (((ObjectInstance) args[1].internalValue)).nativeData;
    int alpha = ((int) args[4].internalValue);
    int[] eventQueue = ((int[]) drawQueueNativeData[0]);
    int index = (((int) drawQueueNativeData[1]) - 16);
    Object[] imageQueue = ((Object[]) drawQueueNativeData[2]);
    int imageQueueLength = ((int) drawQueueNativeData[3]);
    eventQueue[index] = 6;
    eventQueue[(index | 1)] = 0;
    eventQueue[(index | 8)] = ((int) args[2].internalValue);
    eventQueue[(index | 9)] = ((int) args[3].internalValue);
    if ((imageQueue.length == imageQueueLength)) {
      int oldSize = imageQueue.length;
      int newSize = (oldSize * 2);
      Object[] newImageQueue = new Object[newSize];
      int i = 0;
      while ((i < oldSize)) {
        newImageQueue[i] = imageQueue[i];
        i += 1;
      }
      imageQueue = newImageQueue;
      drawQueueNativeData[2] = imageQueue;
    }
    imageQueue[imageQueueLength] = nativeBlittableBitmap;
    drawQueueNativeData[3] = (imageQueueLength + 1);
    return vm.globalNull;
  }
}
