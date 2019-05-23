import array
import base64
import code.vm as VM
import io
import math
import os
import random
import sys
import time
import zipfile

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

def lib_zip_ensureValidArchiveInfo(vm, args):
  sc = 0
  if (args[0][0] != 5):
    sc = 1
  if ((sc == 0) and (lib_zip_validateByteList(args[1], False) != None)):
    sc = 2
  return buildInteger(vm[13], sc)

def lib_zip_initializeZipReader(vm, args):
  sc = 0
  byteArray = lib_zip_validateByteList(args[1], True)
  if (byteArray == None):
    sc = 1
  else:
    obj = args[0][1]
    obj[3] = [None, None]
    obj[3][0] = lib_zip_createZipReaderImpl(byteArray)
    obj[3][1] = 0
    if (obj[3][0] == None):
      sc = 2
  return buildInteger(vm[13], sc)

def lib_zip_readerPeekNextEntry(vm, args):
  obj = args[0][1]
  nd = obj[3]
  output = args[1][1]
  boolOut = (PST_NoneListOfOne * 3)
  nameOut = [None]
  integers = []
  lib_zip_readNextZipEntryImpl(nd[0], nd[1], boolOut, nameOut, integers)
  problemsEncountered = not (boolOut[0])
  foundAnything = boolOut[1]
  isDirectory = boolOut[2]
  if problemsEncountered:
    return vm[16]
  nd[1] = (1 + nd[1])
  setItemInList(output, 0, buildBoolean(vm[13], foundAnything))
  if not (foundAnything):
    return vm[15]
  setItemInList(output, 1, buildString(vm[13], nameOut[0]))
  if isDirectory:
    setItemInList(output, 2, buildBoolean(vm[13], isDirectory))
    return vm[15]
  byteValues = getItemFromList(output, 3)[1]
  length = len(integers)
  i = 0
  positiveNumbers = vm[13][9]
  valuesOut = byteValues[2]
  i = 0
  while (i < length):
    valuesOut.append(positiveNumbers[integers[i]])
    i += 1
  byteValues[1] = length
  return vm[15]

def lib_zip_validateByteList(byteListValue, convert):
  if (byteListValue[0] != 6):
    return None
  output = None
  bytes = byteListValue[1]
  length = bytes[1]
  if convert:
    output = (PST_NoneListOfOne * length)
  else:
    output = [None]
    output[0] = 1
  value = None
  b = 0
  i = 0
  while (i < length):
    value = bytes[2][i]
    if (value[0] != 3):
      return None
    b = value[1]
    if (b > 255):
      return None
    if (b < 0):
      if (b >= -128):
        b += 255
      else:
        return None
    if convert:
      output[i] = b
    i += 1
  return output
