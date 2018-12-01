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

def lib_srandom_getBoolean(vm, args):
  intPtr = args[0][1]
  value = 0
  value = (((intPtr[2][0][1] * 20077) + 12345) & 65535)
  intPtr[2][0] = buildInteger(vm[13], value)
  if ((value & 1) == 0):
    return vm[16]
  return vm[15]

def lib_srandom_getFloat(vm, args):
  intPtr = args[0][1]
  value1 = 0
  value1 = (((intPtr[2][0][1] * 20077) + 12345) & 65535)
  value2 = (((value1 * 20077) + 12345) & 65535)
  value3 = (((value2 * 20077) + 12345) & 65535)
  intPtr[2][0] = buildInteger(vm[13], value3)
  value1 = ((value1 >> 8) & 255)
  value2 = ((value2 >> 8) & 255)
  value3 = ((value3 >> 8) & 255)
  return buildFloat(vm[13], (1.0 * (((value1 << 16) | (value2 << 8) | value3)) / (16777216.0)))

def lib_srandom_getInteger(vm, args):
  intPtr = args[0][1]
  value1 = 0
  value1 = (((intPtr[2][0][1] * 20077) + 12345) & 65535)
  value2 = (((value1 * 20077) + 12345) & 65535)
  value3 = (((value2 * 20077) + 12345) & 65535)
  value4 = (((value3 * 20077) + 12345) & 65535)
  intPtr[2][0] = buildInteger(vm[13], value4)
  value1 = ((value1 >> 8) & 255)
  value2 = ((value2 >> 8) & 255)
  value3 = ((value3 >> 8) & 255)
  value4 = ((value4 >> 8) & 127)
  return buildInteger(vm[13], ((value4 << 24) | (value3 << 16) | (value2 << 8) | value1))
