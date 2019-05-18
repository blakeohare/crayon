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

def lib_processutil_isSupported(vm, args):
  t = TODO("this")
  return buildBoolean(vm[13], t)

def lib_processutil_launchProcess(vm, args):
  bridge = args[0][1]
  bridge[3] = (PST_NoneListOfOne * 5)
  bridge[3][0] = True
  bridge[3][1] = 0
  bridge[3][2] = []
  bridge[3][3] = []
  bridge[3][4] = TODO("this")
  execName = args[1][1]
  argsRaw = args[2][1]
  isAsync = args[3][1]
  cb = args[4]
  argStrings = []
  i = 0
  while (i < argsRaw[1]):
    a = getItemFromList(argsRaw, i)
    argStrings.append(a[1])
    i += 1
  TODO("this")
  return vm[14]

def lib_processutil_readBridge(vm, args):
  bridge = args[0][1]
  outputList = args[1][1]
  type = args[2][1]
  mtx = bridge[3][4]
  if (type == 1):
    outputInt = TODO("this")
    addToList(outputList, buildInteger(vm[13], outputInt))
  else:
    output = []
    TODO("this")
    i = 0
    while (i < len(output)):
      addToList(outputList, buildString(vm[13], output[i]))
      i += 1
  return vm[14]
