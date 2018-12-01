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

def lib_imagewebresources_bytesToImage(vm, args):
  objInstance1 = args[0][1]
  object1 = objInstance1[3][0]
  list1 = args[1][1]
  value = getItemFromList(list1, 0)
  objArray1 = (PST_NoneListOfOne * 3)
  objInstance1 = value[1]
  objInstance1[3] = objArray1
  if lib_imagewebresources_imagedownloader_bytesToImage(object1, objArray1):
    width = buildInteger(vm[13], objArray1[1])
    height = buildInteger(vm[13], objArray1[2])
    list1[2][1] = width
    list1[2][2] = height
    return vm[15]
  return vm[16]

def lib_imagewebresources_jsDownload(vm, args):
  return vm[14]

def lib_imagewebresources_jsGetImage(vm, args):
  return vm[16]

def lib_imagewebresources_jsPoll(vm, args):
  return vm[16]
