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

def lib_imageresources_blit(vm, args):
  object1 = None
  objInstance1 = args[0][1]
  objInstance2 = args[1][1]
  libhelper_imageresources_imageResourceBlitImage(objInstance1[3][0], objInstance2[3][0], args[2][1], args[3][1], args[4][1], args[5][1], args[6][1], args[7][1])
  return vm[14]

def lib_imageresources_checkLoaderIsDone(vm, args):
  objInstance1 = args[0][1]
  objInstance2 = args[1][1]
  status = libhelper_imageresources_checkLoaderIsDone(objInstance1[3], objInstance2[3])
  return buildInteger(vm[13], status)

def lib_imageresources_flushImageChanges(vm, args):
  return vm[14]

def lib_imageresources_getManifestString(vm, args):
  return buildString(vm[13], libhelper_imageresources_getImageResourceManifestString())

def lib_imageresources_loadAsynchronous(vm, args):
  objInstance1 = args[0][1]
  filename = args[1][1]
  objInstance2 = args[2][1]
  objArray1 = (PST_NoneListOfOne * 3)
  objInstance1[3] = objArray1
  objArray2 = (PST_NoneListOfOne * 4)
  objArray2[2] = 0
  objInstance2[3] = objArray2
  libhelper_imageresources_imageLoadAsync(filename, objArray1, objArray2)
  return vm[14]

def lib_imageresources_nativeImageDataInit(vm, args):
  objInstance1 = args[0][1]
  nd = (PST_NoneListOfOne * 4)
  width = args[1][1]
  height = args[2][1]
  nd[0] = libhelper_imageresources_generateNativeBitmapOfSize(width, height)
  nd[1] = width
  nd[2] = height
  nd[3] = None
  objInstance1[3] = nd
  return vm[14]
