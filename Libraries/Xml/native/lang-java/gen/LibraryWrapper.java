package org.crayonlang.libraries.xml;

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

  public static String lib_xml_ampUnescape(String value, HashMap<String, String> entityLookup) {
    String[] ampParts = PST_literalStringSplit(value, "&");
    int i = 1;
    while ((i < ampParts.length)) {
      String component = ampParts[i];
      int semicolon = component.indexOf(";");
      if ((semicolon != -1)) {
        String entityCode = component.substring(0, 0 + semicolon);
        String entityValue = lib_xml_getEntity(entityCode, entityLookup);
        if ((entityValue == null)) {
          entityValue = "&";
        } else {
          component = component.substring((semicolon + 1), (semicolon + 1) + ((component.length() - semicolon - 1)));
        }
        ampParts[i] = entityValue + component;
      }
      i += 1;
    }
    return String.join("", ampParts);
  }

  public static String lib_xml_error(String xml, int index, String msg) {
    String loc = "";
    if ((index < xml.length())) {
      int line = 1;
      int col = 0;
      int i = 0;
      while ((i <= index)) {
        if ((xml.charAt(i) == '\n')) {
          line += 1;
          col = 0;
        } else {
          col += 1;
        }
        i += 1;
      }
      loc = " on line " + Integer.toString(line) + ", col " + Integer.toString(col);
    }
    return "XML parse error" + loc + ": " + msg;
  }

  public static String lib_xml_getEntity(String code, HashMap<String, String> entityLookup) {
    if (entityLookup.containsKey(code)) {
      return entityLookup.get(code);
    }
    return null;
  }

  public static boolean lib_xml_isNext(String xml, int[] indexPtr, String value) {
    return PST_checkStringInString(xml, indexPtr[0], value);
  }

  public static Value lib_xml_parse(VmContext vm, Value[] args) {
    String xml = ((String) args[0].internalValue);
    ListImpl list1 = ((ListImpl) args[1].internalValue);
    ObjectInstance objInstance1 = ((ObjectInstance) args[2].internalValue);
    Object[] objArray1 = objInstance1.nativeData;
    if ((objArray1 == null)) {
      objArray1 = new Object[2];
      objInstance1.nativeData = objArray1;
      objArray1[0] = new HashMap<String, String>();
      objArray1[1] = new HashMap<Integer, Integer>();
    }
    ArrayList<Value> output = new ArrayList<Value>();
    String errMsg = lib_xml_parseImpl(vm, xml, PST_intBuffer16, output, ((HashMap<String, String>) objArray1[0]), ((HashMap<Integer, Integer>) objArray1[1]));
    if ((errMsg != null)) {
      return org.crayonlang.interpreter.vm.CrayonWrapper.buildString(vm.globals, errMsg);
    }
    ListImpl list2 = ((ListImpl) org.crayonlang.interpreter.vm.CrayonWrapper.buildList(output).internalValue);
    list1.size = list2.size;
    list1.capacity = list2.capacity;
    list1.array = list2.array;
    return vm.globalNull;
  }

  public static String lib_xml_parseElement(VmContext vm, String xml, int[] indexPtr, ArrayList<Value> output, HashMap<String, String> entityLookup, HashMap<Integer, Integer> stringEnders) {
    int length = xml.length();
    ArrayList<Value> attributeKeys = new ArrayList<Value>();
    ArrayList<Value> attributeValues = new ArrayList<Value>();
    ArrayList<Value> children = new ArrayList<Value>();
    ArrayList<Value> element = new ArrayList<Value>();
    String error = null;
    if (!lib_xml_popIfPresent(xml, indexPtr, "<")) {
      return lib_xml_error(xml, indexPtr[0], "Expected: '<'");
    }
    String name = lib_xml_popName(xml, indexPtr);
    lib_xml_skipWhitespace(xml, indexPtr);
    boolean hasClosingTag = true;
    while (true) {
      if ((indexPtr[0] >= length)) {
        return lib_xml_error(xml, length, "Unexpected EOF");
      }
      if (lib_xml_popIfPresent(xml, indexPtr, ">")) {
        break;
      }
      if (lib_xml_popIfPresent(xml, indexPtr, "/>")) {
        hasClosingTag = false;
        break;
      }
      String key = lib_xml_popName(xml, indexPtr);
      if ((key.length() == 0)) {
        return lib_xml_error(xml, indexPtr[0], "Expected attribute name.");
      }
      attributeKeys.add(org.crayonlang.interpreter.vm.CrayonWrapper.buildString(vm.globals, key));
      lib_xml_skipWhitespace(xml, indexPtr);
      if (!lib_xml_popIfPresent(xml, indexPtr, "=")) {
        return lib_xml_error(xml, indexPtr[0], "Expected: '='");
      }
      lib_xml_skipWhitespace(xml, indexPtr);
      error = lib_xml_popString(vm, xml, indexPtr, attributeValues, entityLookup, stringEnders);
      if ((error != null)) {
        return error;
      }
      lib_xml_skipWhitespace(xml, indexPtr);
    }
    if (hasClosingTag) {
      String close = "</" + name + ">";
      while (!lib_xml_popIfPresent(xml, indexPtr, close)) {
        if (lib_xml_isNext(xml, indexPtr, "</")) {
          error = lib_xml_error(xml, (indexPtr[0] - 2), "Unexpected close tag.");
        } else {
          if (lib_xml_isNext(xml, indexPtr, "<!--")) {
            error = lib_xml_skipComment(xml, indexPtr);
          } else {
            if (lib_xml_isNext(xml, indexPtr, "<")) {
              error = lib_xml_parseElement(vm, xml, indexPtr, children, entityLookup, stringEnders);
            } else {
              error = lib_xml_parseText(vm, xml, indexPtr, children, entityLookup);
            }
          }
        }
        if (((error == null) && (indexPtr[0] >= length))) {
          error = lib_xml_error(xml, length, "Unexpected EOF. Unclosed tag.");
        }
        if ((error != null)) {
          return error;
        }
      }
    }
    element.add(vm.globalTrue);
    element.add(org.crayonlang.interpreter.vm.CrayonWrapper.buildString(vm.globals, name));
    element.add(org.crayonlang.interpreter.vm.CrayonWrapper.buildList(attributeKeys));
    element.add(org.crayonlang.interpreter.vm.CrayonWrapper.buildList(attributeValues));
    element.add(org.crayonlang.interpreter.vm.CrayonWrapper.buildList(children));
    output.add(org.crayonlang.interpreter.vm.CrayonWrapper.buildList(element));
    return null;
  }

  public static String lib_xml_parseImpl(VmContext vm, String input, int[] indexPtr, ArrayList<Value> output, HashMap<String, String> entityLookup, HashMap<Integer, Integer> stringEnders) {
    if ((entityLookup.size() == 0)) {
      entityLookup.put("amp", "&");
      entityLookup.put("lt", "<");
      entityLookup.put("gt", ">");
      entityLookup.put("quot", "\"");
      entityLookup.put("apos", "'");
    }
    if ((stringEnders.size() == 0)) {
      stringEnders.put(((int)(' ')), 1);
      stringEnders.put(((int)('"')), 1);
      stringEnders.put(((int)('\'')), 1);
      stringEnders.put(((int)('<')), 1);
      stringEnders.put(((int)('>')), 1);
      stringEnders.put(((int)('\t')), 1);
      stringEnders.put(((int)('\r')), 1);
      stringEnders.put(((int)('\n')), 1);
      stringEnders.put(((int)('/')), 1);
    }
    indexPtr[0] = 0;
    lib_xml_skipWhitespace(input, indexPtr);
    if (lib_xml_popIfPresent(input, indexPtr, "<?xml")) {
      int newBegin = input.indexOf("?>");
      if ((newBegin == -1)) {
        return lib_xml_error(input, (indexPtr[0] - 5), "XML Declaration is not closed.");
      }
      indexPtr[0] = (newBegin + 2);
    }
    String error = lib_xml_skipStuff(input, indexPtr);
    if ((error != null)) {
      return error;
    }
    error = lib_xml_parseElement(vm, input, indexPtr, output, entityLookup, stringEnders);
    if ((error != null)) {
      return error;
    }
    lib_xml_skipStuff(input, indexPtr);
    if ((indexPtr[0] != input.length())) {
      return lib_xml_error(input, indexPtr[0], "Unexpected text.");
    }
    return null;
  }

  public static String lib_xml_parseText(VmContext vm, String xml, int[] indexPtr, ArrayList<Value> output, HashMap<String, String> entityLookup) {
    int length = xml.length();
    int start = indexPtr[0];
    int i = start;
    boolean ampFound = false;
    char c = ' ';
    while ((i < length)) {
      c = xml.charAt(i);
      if ((c == '<')) {
        break;
      } else {
        if ((c == '&')) {
          ampFound = true;
        }
      }
      i += 1;
    }
    if ((i > start)) {
      indexPtr[0] = i;
      String textValue = xml.substring(start, start + (i - start));
      if (ampFound) {
        textValue = lib_xml_ampUnescape(textValue, entityLookup);
      }
      ArrayList<Value> textElement = new ArrayList<Value>();
      textElement.add(vm.globalFalse);
      textElement.add(org.crayonlang.interpreter.vm.CrayonWrapper.buildString(vm.globals, textValue));
      output.add(org.crayonlang.interpreter.vm.CrayonWrapper.buildList(textElement));
    }
    return null;
  }

  public static boolean lib_xml_popIfPresent(String xml, int[] indexPtr, String s) {
    if (PST_checkStringInString(xml, indexPtr[0], s)) {
      indexPtr[0] = (indexPtr[0] + s.length());
      return true;
    }
    return false;
  }

  public static String lib_xml_popName(String xml, int[] indexPtr) {
    int length = xml.length();
    int i = indexPtr[0];
    int start = i;
    char c = ' ';
    while ((i < length)) {
      c = xml.charAt(i);
      if ((((c >= 'a') && (c <= 'z')) || ((c >= 'A') && (c <= 'Z')) || ((c >= '0') && (c <= '9')) || (c == '_') || (c == '.') || (c == ':') || (c == '-'))) {
      } else {
        break;
      }
      i += 1;
    }
    String output = xml.substring(start, start + (i - start));
    indexPtr[0] = i;
    return output;
  }

  public static String lib_xml_popString(VmContext vm, String xml, int[] indexPtr, ArrayList<Value> attributeValueOut, HashMap<String, String> entityLookup, HashMap<Integer, Integer> stringEnders) {
    int length = xml.length();
    int start = indexPtr[0];
    int end = length;
    int i = start;
    int stringType = ((int) xml.charAt(i));
    boolean unwrapped = ((stringType != ((int)('"'))) && (stringType != ((int)('\''))));
    boolean ampFound = false;
    int c = ((int)(' '));
    if (unwrapped) {
      while ((i < length)) {
        c = ((int) xml.charAt(i));
        if (stringEnders.containsKey(c)) {
          end = i;
          break;
        } else {
          if ((c == ((int)('&')))) {
            ampFound = true;
          }
        }
        i += 1;
      }
    } else {
      i += 1;
      start = i;
      while ((i < length)) {
        c = ((int) xml.charAt(i));
        if ((c == stringType)) {
          end = i;
          i += 1;
          break;
        } else {
          if ((c == ((int)('&')))) {
            ampFound = true;
          }
        }
        i += 1;
      }
    }
    indexPtr[0] = i;
    String output = xml.substring(start, start + (end - start));
    if (ampFound) {
      output = lib_xml_ampUnescape(output, entityLookup);
    }
    attributeValueOut.add(org.crayonlang.interpreter.vm.CrayonWrapper.buildString(vm.globals, output));
    return null;
  }

  public static String lib_xml_skipComment(String xml, int[] indexPtr) {
    if (lib_xml_popIfPresent(xml, indexPtr, "<!--")) {
      int i = xml.indexOf("-->", indexPtr[0]);
      if ((i == -1)) {
        return lib_xml_error(xml, (indexPtr[0] - 4), "Unclosed comment.");
      }
      indexPtr[0] = (i + 3);
    }
    return null;
  }

  public static String lib_xml_skipStuff(String xml, int[] indexPtr) {
    int index = (indexPtr[0] - 1);
    while ((index < indexPtr[0])) {
      index = indexPtr[0];
      lib_xml_skipWhitespace(xml, indexPtr);
      String error = lib_xml_skipComment(xml, indexPtr);
      if ((error != null)) {
        return error;
      }
    }
    return null;
  }

  public static int lib_xml_skipWhitespace(String xml, int[] indexPtr) {
    int length = xml.length();
    int i = indexPtr[0];
    while ((i < length)) {
      char c = xml.charAt(i);
      if (((c != ' ') && (c != '\t') && (c != '\n') && (c != '\r'))) {
        indexPtr[0] = i;
        return 0;
      }
      i += 1;
    }
    indexPtr[0] = i;
    return 0;
  }
}
