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

def lib_textencoding_convertBytesToText(vm, args):
  if (args[0][0] != 6):
    return buildInteger(vm[13], 2)
  byteList = args[0][1]
  format = args[1][1]
  output = args[2][1]
  strOut = PST_StringBuffer16
  length = byteList[1]
  unwrappedBytes = (PST_NoneListOfOne * length)
  i = 0
  value = None
  while (i < length):
    value = byteList[2][i]
    if (value[0] != 3):
      return buildInteger(vm[13], 3)
    unwrappedBytes[i] = value[1]
    i += 1
  sc = lib_textencoding_bytesToText(unwrappedBytes, format, strOut)
  if (sc == 0):
    addToList(output, buildString(vm[13], strOut[0]))
  return buildInteger(vm[13], sc)

def lib_textencoding_convertTextToBytes(vm, args):
  value = args[0][1]
  format = args[1][1]
  includeBom = args[2][1]
  output = args[3][1]
  byteList = []
  intOut = PST_IntBuffer16
  sc = lib_textencoding_textToBytes(value, includeBom, format, byteList, vm[13][9], intOut)
  swapWordSize = intOut[0]
  if (swapWordSize != 0):
    i = 0
    j = 0
    length = len(byteList)
    swap = None
    half = (swapWordSize >> 1)
    k = 0
    while (i < length):
      k = (i + swapWordSize - 1)
      j = 0
      while (j < half):
        swap = byteList[(i + j)]
        byteList[(i + j)] = byteList[(k - j)]
        byteList[(k - j)] = swap
        j += 1
      i += swapWordSize
  if (sc == 0):
    addToList(output, buildList(byteList))
  return buildInteger(vm[13], sc)
