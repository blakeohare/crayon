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

def lib_fileiocommon_directoryCreate(vm, args):
  bool1 = False
  i = 0
  int1 = 0
  stringList1 = None
  hostObject = args[0]
  path = args[1][1]
  if args[2][1]:
    int1 = 0
    if not (lib_fileiocommon_directoryExists(lib_fileiocommon_getDirRoot(path))):
      int1 = 4
    else:
      stringList1 = []
      bool1 = True
      while (bool1 and not (lib_fileiocommon_directoryExists(path))):
        stringList1.append(path)
        int1 = lib_fileiocommon_getDirParent(path, PST_StringBuffer16)
        path = PST_StringBuffer16[0]
        if (int1 != 0):
          bool1 = False
      if bool1:
        i = (len(stringList1) - 1)
        while (i >= 0):
          path = stringList1[i]
          int1 = lib_fileiocommon_createDirectory(path)
          if (int1 != 0):
            i = -1
          i -= 1
  else:
    int1 = lib_fileiocommon_createDirectory(path)
  return buildInteger(vm[13], int1)

def lib_fileiocommon_directoryDelete(vm, args):
  sc = lib_fileiocommon_deleteDirectory(args[1][1])
  return buildInteger(vm[13], sc)

def lib_fileiocommon_directoryList(vm, args):
  diskhost = args[0]
  path = args[1][1]
  useFullPath = args[2][1]
  outputList = args[3][1]
  stringList1 = []
  sc = lib_fileiocommon_getDirectoryList(path, useFullPath, stringList1)
  if (sc == 0):
    i = 0
    while (i < len(stringList1)):
      addToList(outputList, buildString(vm[13], stringList1[i]))
      i += 1
  return buildInteger(vm[13], sc)

def lib_fileiocommon_directoryMove(vm, args):
  statusCode = lib_fileiocommon_moveDirectory(args[1][1], args[2][1])
  return buildInteger(vm[13], statusCode)

def lib_fileiocommon_fileDelete(vm, args):
  statusCode = lib_fileiocommon_fileDelete(args[1][1])
  return buildInteger(vm[13], statusCode)

def lib_fileiocommon_fileInfo(vm, args):
  mask = args[2][1]
  lib_fileiocommon_getFileInfo(args[1][1], mask, PST_IntBuffer16, PST_FloatBuffer16)
  outputList = args[3][1]
  clearList(outputList)
  globals = vm[13]
  addToList(outputList, buildBoolean(globals, (PST_IntBuffer16[0] > 0)))
  addToList(outputList, buildBoolean(globals, (PST_IntBuffer16[1] > 0)))
  if ((mask & 1) != 0):
    addToList(outputList, buildInteger(globals, PST_IntBuffer16[2]))
  else:
    addToList(outputList, globals[0])
  if ((mask & 2) != 0):
    addToList(outputList, buildBoolean(globals, (PST_IntBuffer16[3] > 0)))
  else:
    addToList(outputList, globals[0])
  if ((mask & 4) != 0):
    addToList(outputList, buildFloat(globals, PST_FloatBuffer16[0]))
  else:
    addToList(outputList, globals[0])
  if ((mask & 8) != 0):
    addToList(outputList, buildFloat(globals, PST_FloatBuffer16[1]))
  else:
    addToList(outputList, globals[0])
  return args[3]

def lib_fileiocommon_fileMove(vm, args):
  statusCode = lib_fileiocommon_fileMove(args[1][1], args[2][1], args[3][1], args[4][1])
  return buildInteger(vm[13], statusCode)

def lib_fileiocommon_fileRead(vm, args):
  diskHostObject = args[0]
  sandboxedPath = args[1][1]
  readDataAsBytes = args[2][1]
  outputList = args[3][1]
  tList = []
  statusCode = lib_fileiocommon_fileRead(sandboxedPath, readDataAsBytes, PST_StringBuffer16, vm[13][9], tList)
  if ((statusCode == 0) and not (readDataAsBytes)):
    addToList(outputList, buildString(vm[13], PST_StringBuffer16[0]))
  else:
    outputList[2] = tList
    outputList[1] = len(tList)
  return buildInteger(vm[13], statusCode)

def lib_fileiocommon_fileWrite(vm, args):
  ints = vm[13][9]
  if (args[3][0] != 3):
    return ints[3]
  statusCode = 0
  contentString = None
  byteArrayRef = None
  format = args[3][1]
  if (format == 0):
    byteArrayRef = lib_fileiocommon_listToBytes(args[2][1])
    if (byteArrayRef == None):
      return ints[6]
  elif (args[2][0] != 5):
    return ints[6]
  else:
    contentString = args[2][1]
  if (statusCode == 0):
    statusCode = lib_fileiocommon_fileWrite(args[1][1], format, contentString, byteArrayRef)
  return buildInteger(vm[13], statusCode)

def lib_fileiocommon_getCurrentDirectory(vm, args):
  return buildString(vm[13], lib_fileiocommon_getCurrentDirectory())

def lib_fileiocommon_getDiskObject(diskObjectArg):
  objInst = diskObjectArg[1]
  return objInst[3][0]

def lib_fileiocommon_getUserDirectory(vm, args):
  return buildString(vm[13], lib_fileiocommon_getUserDirectory())

def lib_fileiocommon_initializeDisk(vm, args):
  objInstance1 = args[0][1]
  objArray1 = [None]
  objInstance1[3] = objArray1
  object1 = always_false()
  objArray1[0] = object1
  return vm[13][0]

def lib_fileiocommon_isWindows(vm, args):
  if lib_fileiocommon_isWindows():
    return vm[13][1]
  return vm[13][2]

def lib_fileiocommon_listToBytes(listOfMaybeInts):
  bytes = (PST_NoneListOfOne * len(listOfMaybeInts))
  intValue = None
  byteValue = 0
  i = (len(listOfMaybeInts) - 1)
  while (i >= 0):
    intValue = listOfMaybeInts[i]
    if (intValue[0] != 3):
      return None
    byteValue = intValue[1]
    if (byteValue >= 256):
      return None
    if (byteValue < 0):
      if (byteValue < -128):
        return None
      byteValue += 256
    bytes[i] = byteValue
    i -= 1
  return bytes

def lib_fileiocommon_textToLines(vm, args):
  lib_fileiocommon_textToLinesImpl(vm[13], args[0][1], args[1][1])
  return args[1]

def lib_fileiocommon_textToLinesImpl(globals, text, output):
  stringList = []
  lib_fileiocommon_textToLines(text, stringList)
  _len = len(stringList)
  i = 0
  while (i < _len):
    addToList(output, buildString(globals, stringList[i]))
    i += 1
  return 0
