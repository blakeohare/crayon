package org.crayonlang.libraries.xml;

import java.util.ArrayList;
import java.util.HashMap;
import org.crayonlang.interpreter.PlatformTranslationHelper;
import org.crayonlang.interpreter.structs.*;
import org.crayonlang.interpreter.TranslationHelper;

public final class LibraryWrapper {

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

  private static final int[] PST_intBuffer16 = new int[16];

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
