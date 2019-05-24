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

def always_false(): return False

PST_NoneListOfOne = [None]

def lib_zip_ensureValidArchiveInfo(vm, args):
  sc = 0
  if (args[0][0] != 5):
    sc = 1
  if ((sc == 0) and (lib_zip_validateByteList(args[1], False) != None)):
    sc = 2
  return buildInteger(vm[13], sc)

def lib_zip_initAsyncCallback(scOut, nativeData, nativeZipArchive, vm, execContext):
  sc = 0
  if (nativeZipArchive == None):
    sc = 2
  setItemInList(scOut, 0, buildInteger(vm[13], sc))
  nativeData[0] = nativeZipArchive
  runInterpreter(vm, execContext)

def lib_zip_initializeZipReader(vm, args):
  sc = 0
  scOut = args[2][1]
  execId = args[3][1]
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
    else:
      sc = 0
    if always_false():
      sc = 3
      vm_suspend_context_by_id(vm, execId, 1)
  setItemInList(scOut, 0, buildInteger(vm[13], sc))
  return vm[14]

def lib_zip_readerPeekNextEntry(vm, args):
  obj = args[0][1]
  nd = obj[3]
  output = args[1][1]
  execId = args[2][1]
  boolOut = (PST_NoneListOfOne * 3)
  nameOut = [None]
  integers = []
  lib_zip_readNextZipEntryImpl(nd[0], nd[1], boolOut, nameOut, integers)
  if always_false():
    vm_suspend_context_by_id(vm, execId, 1)
    return vm[15]
  return lib_zip_readerPeekNextEntryCallback(not (boolOut[0]), boolOut[1], boolOut[2], nameOut[0], integers, nd, output, vm)

def lib_zip_readerPeekNextEntryCallback(problemsEncountered, foundAnything, isDirectory, name, bytesAsIntList, nativeData, output, vm):
  if problemsEncountered:
    return vm[16]
  nativeData[1] = (1 + nativeData[1])
  setItemInList(output, 0, buildBoolean(vm[13], foundAnything))
  if not (foundAnything):
    return vm[15]
  setItemInList(output, 1, buildString(vm[13], name))
  if isDirectory:
    setItemInList(output, 2, buildBoolean(vm[13], isDirectory))
    return vm[15]
  byteValues = getItemFromList(output, 3)[1]
  length = len(bytesAsIntList)
  i = 0
  positiveNumbers = vm[13][9]
  valuesOut = byteValues[2]
  i = 0
  while (i < length):
    valuesOut.append(positiveNumbers[bytesAsIntList[i]])
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
