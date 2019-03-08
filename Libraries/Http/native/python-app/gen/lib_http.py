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

def lib_http_fastEnsureAllBytes(vm, args):
  if (args[0][0] == 6):
    list1 = args[0][1]
    i = list1[1]
    int1 = 0
    intArray1 = (PST_NoneListOfOne * i)
    value = None
    while (i > 0):
      i -= 1
      value = list1[2][i]
      if (value[0] != 3):
        return vm[16]
      int1 = value[1]
      if (int1 < 0):
        if (int1 < -128):
          return vm[16]
        int1 += 256
      elif (int1 >= 256):
        return vm[16]
      intArray1[i] = int1
    objArray1 = [None]
    objArray1[0] = intArray1
    objInstance1 = args[1][1]
    objInstance1[3] = objArray1
    return vm[15]
  return vm[16]

def lib_http_getResponseBytes(vm, args):
  outputListValue = args[1]
  objInstance1 = args[0][1]
  objArray1 = objInstance1[3]
  tList = []
  lib_http_getResponseBytes(objArray1[0], vm[13][9], tList)
  outputList = outputListValue[1]
  outputList[2] = tList
  outputList[1] = len(tList)
  return outputListValue

def lib_http_pollRequest(vm, args):
  objInstance1 = args[0][1]
  objArray1 = objInstance1[3]
  if lib_http_pollRequest(objArray1):
    return vm[15]
  return vm[16]

def lib_http_populateResponse(vm, args):
  arg2 = args[1]
  arg3 = args[2]
  arg4 = args[3]
  objInstance1 = args[0][1]
  object1 = objInstance1[3][0]
  objArray1 = [None]
  stringList1 = []
  lib_http_readResponseData(object1, PST_IntBuffer16, PST_StringBuffer16, objArray1, stringList1)
  objInstance1 = arg2[1]
  objInstance1[3] = objArray1
  outputList = arg3[1]
  addToList(outputList, buildInteger(vm[13], PST_IntBuffer16[0]))
  addToList(outputList, buildString(vm[13], PST_StringBuffer16[0]))
  value = vm[14]
  value2 = vm[15]
  if (PST_IntBuffer16[1] == 0):
    value = buildString(vm[13], PST_StringBuffer16[1])
    value2 = vm[16]
  addToList(outputList, value)
  addToList(outputList, value2)
  list1 = arg4[1]
  i = 0
  while (i < len(stringList1)):
    addToList(list1, buildString(vm[13], stringList1[i]))
    i += 1
  return vm[14]

def lib_http_sendRequest(vm, args):
  body = args[5]
  objInstance1 = args[0][1]
  objArray1 = (PST_NoneListOfOne * 3)
  objInstance1[3] = objArray1
  objArray1[2] = False
  method = args[2][1]
  url = args[3][1]
  headers = []
  list1 = args[4][1]
  i = 0
  while (i < list1[1]):
    headers.append(list1[2][i][1])
    i += 1
  bodyRawObject = body[1]
  bodyState = 0
  if (body[0] == 5):
    bodyState = 1
  elif (body[0] == 8):
    objInstance1 = bodyRawObject
    bodyRawObject = objInstance1[3][0]
    bodyState = 2
  else:
    bodyRawObject = None
  getResponseAsText = (args[6][1] == 1)
  if args[1][1]:
    lib_http_sendRequestAsync(objArray1, method, url, headers, bodyState, bodyRawObject, getResponseAsText)
  else:
    execId = args[7][1]
    if lib_http_sendRequestSync(objArray1, method, url, headers, bodyState, bodyRawObject, getResponseAsText):
      vm_suspend_context_by_id(vm, execId, 1)
  return vm[14]
