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

def lib_imageencoder_encodeToBytes(vm, args):
  platformBitmap = getNativeDataItem(args[0], 0)
  imageFormat = args[1][1]
  byteOutputList = []
  statusCode = lib_imageencoder_encodeImage(platformBitmap, imageFormat, byteOutputList, vm[13][9])
  length = len(byteOutputList)
  finalOutputList = args[2][1]
  i = 0
  while (i < length):
    addToList(finalOutputList, byteOutputList[i])
    i += 1
  return buildInteger(vm[13], statusCode)