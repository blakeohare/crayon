import base64
import code.vm as VM
import math
import os
import random
import sys
import time

PST_StringBuffer16 = [None] * 16
PST_IntBuffer16 = [0] * 16
PST_FloatBuffer16 = [0.0] * 16
PST_NoneListOfOne = [None]

PST_StringType = type('')
def PST_base64ToString(value):
  u_value = base64.b64decode(value)
  if type(u_value) == PST_StringType:
    return u_value
  return u_value.decode('utf8')

def PST_isValidInteger(value):
  if len(value) == 0: return False
  if value[0] == '-': value = value[1:]
  return value.isdigit()

def PST_sortedCopyOfList(t):
  t = t[:]
  t.sort()
  return t

def PST_tryParseFloat(value, floatOut):
  try:
    floatOut[1] = float(value)
    floatOut[0] = 1.0
  except:
    floatOut[0] = -1.0

def PST_stringCheckSlice(haystack, i, needle):
  return haystack[i:i + len(needle)] == needle

def always_true(): return True
def always_false(): return False

def lib_xml_ampUnescape(value, entityLookup):
  ampParts = value.split("&")
  i = 1
  while (i < len(ampParts)):
    component = ampParts[i]
    semicolon = component.find(";")
    if (semicolon != -1):
      entityCode = component[0:0 + semicolon]
      entityValue = lib_xml_getEntity(entityCode, entityLookup)
      if (entityValue == None):
        entityValue = "&"
      else:
        component = component[(semicolon + 1):(semicolon + 1) + ((len(component) - semicolon - 1))]
      ampParts[i] = entityValue + component
    i += 1
  return ("").join(ampParts)

def lib_xml_error(xml, index, msg):
  loc = ""
  if (index < len(xml)):
    line = 1
    col = 0
    i = 0
    while (i <= index):
      if (xml[i] == "\n"):
        line += 1
        col = 0
      else:
        col += 1
      i += 1
    loc = ''.join([" on line ", str(line), ", col ", str(col)])
  return ''.join(["XML parse error", loc, ": ", msg])

def lib_xml_getEntity(code, entityLookup):
  if (code in entityLookup):
    return entityLookup[code]
  return None

def lib_xml_isNext(xml, indexPtr, value):
  return PST_stringCheckSlice(xml, indexPtr[0], value)

def lib_xml_parse(vm, args):
  xml = args[0][1]
  list1 = args[1][1]
  objInstance1 = args[2][1]
  objArray1 = objInstance1[3]
  if (objArray1 == None):
    objArray1 = [None, None]
    objInstance1[3] = objArray1
    objArray1[0] = {}
    objArray1[1] = {}
  output = []
  errMsg = lib_xml_parseImpl(vm, xml, PST_IntBuffer16, output, objArray1[0], objArray1[1])
  if (errMsg != None):
    return buildString(vm[13], errMsg)
  list2 = buildList(output)[1]
  list1[1] = list2[1]
  list1[2] = list2[2]
  return vm[14]

def lib_xml_parseElement(vm, xml, indexPtr, output, entityLookup, stringEnders):
  length = len(xml)
  attributeKeys = []
  attributeValues = []
  children = []
  element = []
  error = None
  if not (lib_xml_popIfPresent(xml, indexPtr, "<")):
    return lib_xml_error(xml, indexPtr[0], "Expected: '<'")
  name = lib_xml_popName(xml, indexPtr)
  lib_xml_skipWhitespace(xml, indexPtr)
  hasClosingTag = True
  while True:
    if (indexPtr[0] >= length):
      return lib_xml_error(xml, length, "Unexpected EOF")
    if lib_xml_popIfPresent(xml, indexPtr, ">"):
      break
    if lib_xml_popIfPresent(xml, indexPtr, "/>"):
      hasClosingTag = False
      break
    key = lib_xml_popName(xml, indexPtr)
    if (len(key) == 0):
      return lib_xml_error(xml, indexPtr[0], "Expected attribute name.")
    attributeKeys.append(buildString(vm[13], key))
    lib_xml_skipWhitespace(xml, indexPtr)
    if not (lib_xml_popIfPresent(xml, indexPtr, "=")):
      return lib_xml_error(xml, indexPtr[0], "Expected: '='")
    lib_xml_skipWhitespace(xml, indexPtr)
    error = lib_xml_popString(vm, xml, indexPtr, attributeValues, entityLookup, stringEnders)
    if (error != None):
      return error
    lib_xml_skipWhitespace(xml, indexPtr)
  if hasClosingTag:
    close = ''.join(["</", name, ">"])
    while not (lib_xml_popIfPresent(xml, indexPtr, close)):
      if lib_xml_isNext(xml, indexPtr, "</"):
        error = lib_xml_error(xml, (indexPtr[0] - 2), "Unexpected close tag.")
      elif lib_xml_isNext(xml, indexPtr, "<!--"):
        error = lib_xml_skipComment(xml, indexPtr)
      elif lib_xml_isNext(xml, indexPtr, "<"):
        error = lib_xml_parseElement(vm, xml, indexPtr, children, entityLookup, stringEnders)
      else:
        error = lib_xml_parseText(vm, xml, indexPtr, children, entityLookup)
      if ((error == None) and (indexPtr[0] >= length)):
        error = lib_xml_error(xml, length, "Unexpected EOF. Unclosed tag.")
      if (error != None):
        return error
  element.append(vm[15])
  element.append(buildString(vm[13], name))
  element.append(buildList(attributeKeys))
  element.append(buildList(attributeValues))
  element.append(buildList(children))
  output.append(buildList(element))
  return None

def lib_xml_parseImpl(vm, input, indexPtr, output, entityLookup, stringEnders):
  if (len(entityLookup) == 0):
    entityLookup["amp"] = "&"
    entityLookup["lt"] = "<"
    entityLookup["gt"] = ">"
    entityLookup["quot"] = "\""
    entityLookup["apos"] = "'"
  if (len(stringEnders) == 0):
    stringEnders[ord(" ")] = 1
    stringEnders[ord("\"")] = 1
    stringEnders[ord("'")] = 1
    stringEnders[ord("<")] = 1
    stringEnders[ord(">")] = 1
    stringEnders[ord("\t")] = 1
    stringEnders[ord("\r")] = 1
    stringEnders[ord("\n")] = 1
    stringEnders[ord("/")] = 1
  indexPtr[0] = 0
  lib_xml_skipWhitespace(input, indexPtr)
  if lib_xml_popIfPresent(input, indexPtr, "<?xml"):
    newBegin = input.find("?>")
    if (newBegin == -1):
      return lib_xml_error(input, (indexPtr[0] - 5), "XML Declaration is not closed.")
    indexPtr[0] = (newBegin + 2)
  error = lib_xml_skipStuff(input, indexPtr)
  if (error != None):
    return error
  error = lib_xml_parseElement(vm, input, indexPtr, output, entityLookup, stringEnders)
  if (error != None):
    return error
  lib_xml_skipStuff(input, indexPtr)
  if (indexPtr[0] != len(input)):
    return lib_xml_error(input, indexPtr[0], "Unexpected text.")
  return None

def lib_xml_parseText(vm, xml, indexPtr, output, entityLookup):
  length = len(xml)
  start = indexPtr[0]
  i = start
  ampFound = False
  c = " "
  while (i < length):
    c = xml[i]
    if (c == "<"):
      break
    elif (c == "&"):
      ampFound = True
    i += 1
  if (i > start):
    indexPtr[0] = i
    textValue = xml[start:start + (i - start)]
    if ampFound:
      textValue = lib_xml_ampUnescape(textValue, entityLookup)
    textElement = []
    textElement.append(vm[16])
    textElement.append(buildString(vm[13], textValue))
    output.append(buildList(textElement))
  return None

def lib_xml_popIfPresent(xml, indexPtr, s):
  if PST_stringCheckSlice(xml, indexPtr[0], s):
    indexPtr[0] = (indexPtr[0] + len(s))
    return True
  return False

def lib_xml_popName(xml, indexPtr):
  length = len(xml)
  i = indexPtr[0]
  start = i
  c = " "
  while (i < length):
    c = xml[i]
    if (((c >= "a") and (c <= "z")) or ((c >= "A") and (c <= "Z")) or ((c >= "0") and (c <= "9")) or (c == "_") or (c == ".") or (c == ":") or (c == "-")):
      pass
    else:
      break
    i += 1
  output = xml[start:start + (i - start)]
  indexPtr[0] = i
  return output

def lib_xml_popString(vm, xml, indexPtr, attributeValueOut, entityLookup, stringEnders):
  length = len(xml)
  start = indexPtr[0]
  end = length
  i = start
  stringType = ord(xml[i])
  unwrapped = ((stringType != ord("\"")) and (stringType != ord("'")))
  ampFound = False
  c = ord(" ")
  if unwrapped:
    while (i < length):
      c = ord(xml[i])
      if (c in stringEnders):
        end = i
        break
      elif (c == ord("&")):
        ampFound = True
      i += 1
  else:
    i += 1
    start = i
    while (i < length):
      c = ord(xml[i])
      if (c == stringType):
        end = i
        i += 1
        break
      elif (c == ord("&")):
        ampFound = True
      i += 1
  indexPtr[0] = i
  output = xml[start:start + (end - start)]
  if ampFound:
    output = lib_xml_ampUnescape(output, entityLookup)
  attributeValueOut.append(buildString(vm[13], output))
  return None

def lib_xml_skipComment(xml, indexPtr):
  if lib_xml_popIfPresent(xml, indexPtr, "<!--"):
    i = xml.find("-->", indexPtr[0])
    if (i == -1):
      return lib_xml_error(xml, (indexPtr[0] - 4), "Unclosed comment.")
    indexPtr[0] = (i + 3)
  return None

def lib_xml_skipStuff(xml, indexPtr):
  index = (indexPtr[0] - 1)
  while (index < indexPtr[0]):
    index = indexPtr[0]
    lib_xml_skipWhitespace(xml, indexPtr)
    error = lib_xml_skipComment(xml, indexPtr)
    if (error != None):
      return error
  return None

def lib_xml_skipWhitespace(xml, indexPtr):
  length = len(xml)
  i = indexPtr[0]
  while (i < length):
    c = xml[i]
    if ((c != " ") and (c != "\t") and (c != "\n") and (c != "\r")):
      indexPtr[0] = i
      return 0
    i += 1
  indexPtr[0] = i
  return 0
