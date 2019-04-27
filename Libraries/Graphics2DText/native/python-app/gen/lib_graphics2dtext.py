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

def lib_graphics2dtext_createNativeFont(vm, args):
  ints = vm[13][9]
  nf = args[0][1]
  nfOut = nf[3]
  fontType = args[1][1]
  fontPath = ""
  if (fontType == 0):
    fontType = args[2][1]
  else:
    fontPath = args[2][1]
    if (fontType == 1):
      res = resource_manager_getResourceOfType(vm, fontPath, "TTF")
      if (res[0] == 1):
        return ints[2]
      resList = res[1]
      if not (getItemFromList(resList, 0)[1]):
        return ints[2]
      fontPath = getItemFromList(resList, 1)[1]
  fontClass = 0
  fontSize = args[3][1]
  red = args[4][1]
  green = args[5][1]
  blue = args[6][1]
  styleBitmask = args[7][1]
  isBold = (styleBitmask & 1)
  isItalic = (styleBitmask & 2)
  nfOut[0] = lib_graphics2dtext_createNativeFont_impl(fontType, fontClass, fontPath, fontSize, (isBold > 0), (isItalic > 0))
  if (nfOut[0] == None):
    if (fontType == 3):
      return ints[1]
    return ints[2]
  return ints[0]

def lib_graphics2dtext_getNativeFontUniqueKey(vm, args):
  list1 = args[7][1]
  output = []
  output.extend((args[0], args[1], args[2], args[6]))
  list2 = buildList(output)[1]
  list1[2] = list2[2]
  list1[1] = list2[1]
  return vm[14]

def lib_graphics2dtext_glGenerateAndLoadTexture(vm, args):
  return vm[14]

def lib_graphics2dtext_glRenderCharTile(vm, args):
  return vm[15]

def lib_graphics2dtext_glRenderTextSurface(vm, args):
  return vm[14]

def lib_graphics2dtext_glSetNativeDataIntArray(vm, args):
  return vm[14]

def lib_graphics2dtext_isDynamicFontLoaded(vm, args):
  return vm[15]

def lib_graphics2dtext_isGlRenderer(vm, args):
  return vm[16]

def lib_graphics2dtext_isResourceAvailable(vm, args):
  path = args[0][1]
  res = resource_manager_getResourceOfType(vm, path, "TTF")
  if (res[0] == 1):
    return vm[16]
  resList = res[1]
  if not (getItemFromList(resList, 0)[1]):
    return vm[16]
  return vm[15]

def lib_graphics2dtext_isSystemFontPresent(vm, args):
  return buildBoolean(vm[13], lib_graphics2dtext_isSystemFontAvailable_impl(args[0][1]))

def lib_graphics2dtext_renderText(vm, args):
  sizeOut = args[0][1]
  textSurface = args[1][1]
  imageOut = textSurface[3]
  nativeFont = (args[2][1])[3][0]
  sourceType = args[3][1]
  fontClass = 0
  fontPath = ""
  if (sourceType == 0):
    fontClass = args[4][1]
  else:
    fontPath = args[4][1]
  fontSize = args[5][1]
  fontStyle = args[6][1]
  isBold = (fontStyle & 1)
  isItalic = (fontStyle & 2)
  red = args[7][1]
  green = args[8][1]
  blue = args[9][1]
  text = args[10][1]
  bmp = lib_graphics2dtext_renderText_impl(PST_IntBuffer16, nativeFont, red, green, blue, text)
  spoofedNativeData = (PST_NoneListOfOne * 4)
  spoofedNativeData[3] = bmp
  spoofedNativeData2 = [None]
  spoofedNativeData2[0] = spoofedNativeData
  imageOut[0] = spoofedNativeData2
  setItemInList(sizeOut, 0, buildInteger(vm[13], PST_IntBuffer16[0]))
  setItemInList(sizeOut, 1, buildInteger(vm[13], PST_IntBuffer16[1]))
  return vm[14]

def lib_graphics2dtext_simpleBlit(vm, args):
  nativeBlittableBitmap = (args[0][1])[3][0]
  drawQueueNativeData = (args[1][1])[3]
  alpha = args[4][1]
  eventQueue = drawQueueNativeData[0]
  index = (drawQueueNativeData[1] - 16)
  imageQueue = drawQueueNativeData[2]
  imageQueueLength = drawQueueNativeData[3]
  eventQueue[index] = 6
  eventQueue[(index | 1)] = 0
  eventQueue[(index | 8)] = args[2][1]
  eventQueue[(index | 9)] = args[3][1]
  if (len(imageQueue) == imageQueueLength):
    oldSize = len(imageQueue)
    newSize = (oldSize * 2)
    newImageQueue = (PST_NoneListOfOne * newSize)
    i = 0
    while (i < oldSize):
      newImageQueue[i] = imageQueue[i]
      i += 1
    imageQueue = newImageQueue
    drawQueueNativeData[2] = imageQueue
  imageQueue[imageQueueLength] = nativeBlittableBitmap
  drawQueueNativeData[3] = (imageQueueLength + 1)
  return vm[14]
