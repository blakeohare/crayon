import base64
import code.vm as VM
import math
import os
import random
import sys
import time

PST_NoneListOfOne = [None]

def lib_graphics2d_addImageRenderEvent(vm, args):
  i = 0
  # get the drawing queue data
  drawQueueData = (args[0][1])[3]
  # expand the draw event queue
  eventQueue = drawQueueData[0]
  queueLength = drawQueueData[1]
  if (queueLength >= len(eventQueue)):
    eventQueue = lib_graphics2d_expandEventQueueCapacity(eventQueue)
    drawQueueData[0] = eventQueue
  drawQueueData[1] = (queueLength + 16)
  # expand (or create) the image native data queue
  imageNativeDataQueue = drawQueueData[2]
  imageNativeDataQueueSize = 0
  if (imageNativeDataQueue == None):
    imageNativeDataQueue = (PST_NoneListOfOne * 16)
  else:
    imageNativeDataQueueSize = drawQueueData[3]
  if (imageNativeDataQueueSize >= len(imageNativeDataQueue)):
    objArrayArray2 = (PST_NoneListOfOne * ((imageNativeDataQueueSize * 2) + 16))
    i = 0
    while (i < imageNativeDataQueueSize):
      objArrayArray2[i] = imageNativeDataQueue[i]
      i += 1
    imageNativeDataQueue = objArrayArray2
    drawQueueData[2] = imageNativeDataQueue
  drawQueueData[3] = (imageNativeDataQueueSize + 1)
  # Add the image to the image native data queue
  imageNativeData = (args[1][1])[3]
  imageNativeDataQueue[imageNativeDataQueueSize] = imageNativeData
  isValid = True
  isNoop = False
  # mark event as an Image event (6)
  eventQueue[queueLength] = 6
  # get/set the draw options mask
  flag = args[2][1]
  eventQueue[(queueLength | 1)] = flag
  # rotation
  if ((flag & 4) != 0):
    rotationValue = args[11]
    theta = 0.0
    if (rotationValue[0] == 4):
      theta = rotationValue[1]
    elif (rotationValue[0] == 3):
      theta += rotationValue[1]
    else:
      isValid = False
    eventQueue[(queueLength | 10)] = int((canonicalizeAngle(theta) * 1048576))
  # alpha
  if ((flag & 8) != 0):
    alphaValue = args[12]
    alpha = 0
    if (alphaValue[0] == 3):
      alpha = alphaValue[1]
    elif (alphaValue[0] == 4):
      alpha = int((0.5 + alphaValue[1]))
    else:
      isValid = False
    if (i > 254):
      eventQueue[(queueLength | 1)] = (flag - 8)
    elif (i < 0):
      isNoop = True
    else:
      eventQueue[(queueLength | 11)] = alpha
  # Copy values to event queue
  value = None
  i = 3
  while (i < 11):
    value = args[i]
    if (value[0] == 3):
      eventQueue[(queueLength + i - 1)] = value[1]
    elif (value[0] == 4):
      eventQueue[(queueLength + i - 1)] = int((0.5 + value[1]))
    else:
      isValid = False
    i += 1
  # slicing
  if ((flag & 1) != 0):
    actualWidth = imageNativeData[5]
    sourceX = eventQueue[(queueLength | 2)]
    sourceWidth = eventQueue[(queueLength | 4)]
    if ((sourceX < 0) or ((sourceX + sourceWidth) > actualWidth) or (sourceWidth < 0)):
      isValid = False
    elif (sourceWidth == 0):
      isNoop = True
    actualHeight = imageNativeData[6]
    sourceY = eventQueue[(queueLength | 3)]
    sourceHeight = eventQueue[(queueLength | 5)]
    if ((sourceY < 0) or ((sourceY + sourceHeight) > actualHeight) or (sourceHeight < 0)):
      isValid = False
    elif (sourceHeight == 0):
      isNoop = True
  # stretching
  if ((flag & 2) != 0):
    if (eventQueue[(queueLength | 6)] <= 0):
      if (eventQueue[(queueLength | 6)] < 0):
        isValid = False
      else:
        isNoop = True
    if (eventQueue[(queueLength | 7)] <= 0):
      if (eventQueue[(queueLength | 7)] < 0):
        isValid = False
      else:
        isNoop = True
  # Revert the operation if it is null or a no-op
  if (isNoop or not (isValid)):
    drawQueueData[1] = queueLength
    drawQueueData[3] = imageNativeDataQueueSize
  if (isValid or isNoop):
    return vm[15]
  return vm[16]

def lib_graphics2d_expandEventQueueCapacity(a):
  _len = len(a)
  output = (PST_NoneListOfOne * ((_len * 2) + 16))
  i = 0
  while (i < _len):
    output[i] = a[i]
    i += 1
  return output

def lib_graphics2d_flip(vm, args):
  bool1 = False
  bool2 = False
  i = 0
  objArray1 = None
  objArray2 = None
  object1 = None
  objInstance1 = None
  objInstance2 = None
  arg1 = args[0]
  arg2 = args[1]
  arg3 = args[2]
  arg4 = args[3]
  arg5 = args[4]
  arg6 = args[5]
  objInstance1 = arg1[1]
  objInstance2 = arg2[1]
  objArray1 = objInstance1[3]
  objArray2 = (PST_NoneListOfOne * 7)
  objInstance2[3] = objArray2
  bool1 = arg3[1]
  bool2 = arg4[1]
  i = 6
  while (i >= 0):
    objArray2[i] = objArray1[i]
    i -= 1
  objInstance1 = arg6[1]
  objArray1 = objInstance1[3]
  objInstance2 = arg2[1]
  objInstance2[3][0] = objArray1
  object1 = objArray1[3]
  object1 = TranslationHelper_globals['LIB:GAME:g2dflip'](object1, bool1, bool2)
  objArray1[3] = object1
  return arg2

def lib_graphics2d_initializeTexture(vm, args):
  arg1 = args[0]
  arg2 = args[1]
  arg3 = args[2]
  arg4 = args[3]
  arg5 = args[4]
  objInstance1 = arg1[1]
  objArray1 = (PST_NoneListOfOne * 7)
  objInstance1[3] = objArray1
  objInstance1 = arg2[1]
  objArray1[0] = objInstance1[3]
  list1 = arg3[1]
  value = getItemFromList(list1, 0)
  float1 = value[1]
  value = getItemFromList(list1, 2)
  float2 = value[1]
  objArray1[1] = float1
  objArray1[3] = float2
  value = getItemFromList(list1, 1)
  float1 = value[1]
  value = getItemFromList(list1, 3)
  float2 = value[1]
  objArray1[2] = float1
  objArray1[4] = float2
  objArray1[5] = arg4[1]
  objArray1[6] = arg5[1]
  return vm[14]

def lib_graphics2d_initializeTextureResource(vm, args):
  textureResourceInstance = args[0][1]
  textureResourceNativeData = (PST_NoneListOfOne * 6)
  textureResourceInstance[3] = textureResourceNativeData
  nativeImageDataInstance = args[2][1]
  nativeImageDataNativeData = nativeImageDataInstance[3]
  if args[1][1]:
    textureResourceNativeData[0] = False
    textureResourceNativeData[1] = False
    textureResourceNativeData[2] = -1
    textureResourceNativeData[3] = nativeImageDataNativeData[0]
    textureResourceNativeData[4] = nativeImageDataNativeData[1]
    textureResourceNativeData[5] = nativeImageDataNativeData[2]
  else:
    textureResourceNativeData[0] = False
    textureResourceNativeData[1] = True
    textureResourceNativeData[2] = -1
    textureResourceNativeData[3] = nativeImageDataNativeData[3]
    textureResourceNativeData[4] = nativeImageDataNativeData[4]
    textureResourceNativeData[5] = nativeImageDataNativeData[5]
  return vm[14]

def lib_graphics2d_isOpenGlBased(vm, args):
  return vm[16]

def lib_graphics2d_isPlatformUsingTextureAtlas(vm, args):
  return vm[16]

def lib_graphics2d_lineToQuad(vm, args):
  float1 = 0.0
  float2 = 0.0
  float3 = 0.0
  i = 0
  j = 0
  int1 = 0
  int2 = 0
  int3 = 0
  int4 = 0
  int5 = 0
  objInstance1 = args[0][1]
  objArray1 = objInstance1[3]
  intArray1 = objArray1[0]
  _len = (objArray1[1] - 16)
  int1 = intArray1[(_len + 1)]
  int2 = intArray1[(_len + 2)]
  int3 = intArray1[(_len + 3)]
  int4 = intArray1[(_len + 4)]
  int5 = intArray1[(_len + 5)]
  float1 = ((0.0 + int4) - int2)
  float2 = ((0.0 + int3) - int1)
  float3 = (1.0 * (float1) / (float2))
  float1 = (1.0 * (int5) / (2.0))
  if (float1 < 0.5):
    float1 = 1.0
  float2 = (1.0 * (float1) / (((((float3 * float3) + 1) ** 0.5))))
  float1 = (-float2 * float3)
  i = int(((int1 + float1) + 0.5))
  j = int(((int1 - float1) + 0.5))
  if (i == j):
    j += 1
  intArray1[(_len + 1)] = i
  intArray1[(_len + 3)] = j
  i = int(((int2 + float2) + 0.5))
  j = int(((int2 - float2) + 0.5))
  if (i == j):
    j += 1
  intArray1[(_len + 2)] = i
  intArray1[(_len + 4)] = j
  i = int(((int3 - float1) + 0.5))
  j = int(((int3 + float1) + 0.5))
  if (i == j):
    i += 1
  intArray1[(_len + 5)] = i
  intArray1[(_len + 7)] = j
  i = int(((int4 - float2) + 0.5))
  j = int(((int4 + float2) + 0.5))
  if (i == j):
    i += 1
  intArray1[(_len + 6)] = i
  intArray1[(_len + 8)] = j
  return vm[14]

def lib_graphics2d_renderQueueAction(vm, args):
  command = args[2][1]
  objInstance1 = args[0][1]
  objArray1 = objInstance1[3]
  if (objArray1 == None):
    objArray1 = (PST_NoneListOfOne * 5)
    objInstance1[3] = objArray1
  intArray1 = objArray1[0]
  if (intArray1 == None):
    intArray1 = []
    objArray1[0] = intArray1
    objArray1[1] = 0
    objArray1[2] = (PST_NoneListOfOne * 64)
    objArray1[3] = 0
    objArray1[4] = []
  intList1 = objArray1[4]
  if (command == 1):
    charList = args[1]
    if (charList[0] == 6):
      value = None
      list1 = charList[1]
      _len = len(list1)
      i = 0
      while (i < _len):
        value = list1[i]
        intList1.append(value[1])
        i += 1
    renderArgs = (PST_NoneListOfOne * 4)
    renderArgs[0] = intArray1
    renderArgs[1] = objArray1[1]
    renderArgs[2] = objArray1[2]
    renderArgs[3] = intList1
    callbackId = getNamedCallbackId(vm, "Game", "set-render-data")
    invokeNamedCallback(vm, callbackId, renderArgs)
  elif (command == 2):
    objArray1[1] = 0
    objArray1[3] = 0
    del (intList1)[:]
  return vm[14]

def lib_graphics2d_renderQueueValidateArgs(vm, args):
  o = args[0][1]
  drawQueueRawData = o[3]
  drawEvents = drawQueueRawData[0]
  length = drawQueueRawData[1]
  r = 0
  g = 0
  b = 0
  a = 0
  i = 0
  while (i < length):
    sc_0 = swlookup__lib_graphics2d_renderQueueValidateArgs__0.get(drawEvents[i], 5)
    if (sc_0 < 3):
      if (sc_0 == 0):
        r = drawEvents[(i | 5)]
        g = drawEvents[(i | 6)]
        b = drawEvents[(i | 7)]
        a = drawEvents[(i | 8)]
        if (r > 255):
          drawEvents[(i | 5)] = 255
        elif (r < 0):
          drawEvents[(i | 5)] = 0
        if (g > 255):
          drawEvents[(i | 6)] = 255
        elif (g < 0):
          drawEvents[(i | 6)] = 0
        if (b > 255):
          drawEvents[(i | 7)] = 255
        elif (b < 0):
          drawEvents[(i | 7)] = 0
        if (a > 255):
          drawEvents[(i | 8)] = 255
        elif (a < 0):
          drawEvents[(i | 8)] = 0
      elif (sc_0 == 1):
        r = drawEvents[(i | 6)]
        g = drawEvents[(i | 7)]
        b = drawEvents[(i | 8)]
        a = drawEvents[(i | 9)]
        if (r > 255):
          drawEvents[(i | 6)] = 255
        elif (r < 0):
          drawEvents[(i | 6)] = 0
        if (g > 255):
          drawEvents[(i | 7)] = 255
        elif (g < 0):
          drawEvents[(i | 7)] = 0
        if (b > 255):
          drawEvents[(i | 8)] = 255
        elif (b < 0):
          drawEvents[(i | 8)] = 0
        if (a > 255):
          drawEvents[(i | 9)] = 255
        elif (a < 0):
          drawEvents[(i | 9)] = 0
      else:
        r = drawEvents[(i | 7)]
        g = drawEvents[(i | 8)]
        b = drawEvents[(i | 9)]
        a = drawEvents[(i | 10)]
        if (r > 255):
          drawEvents[(i | 7)] = 255
        elif (r < 0):
          drawEvents[(i | 7)] = 0
        if (g > 255):
          drawEvents[(i | 8)] = 255
        elif (g < 0):
          drawEvents[(i | 8)] = 0
        if (b > 255):
          drawEvents[(i | 9)] = 255
        elif (b < 0):
          drawEvents[(i | 9)] = 0
        if (a > 255):
          drawEvents[(i | 10)] = 255
        elif (a < 0):
          drawEvents[(i | 10)] = 0
    elif (sc_0 == 3):
      r = drawEvents[(i | 9)]
      g = drawEvents[(i | 10)]
      b = drawEvents[(i | 11)]
      a = drawEvents[(i | 12)]
      if (r > 255):
        drawEvents[(i | 9)] = 255
      elif (r < 0):
        drawEvents[(i | 9)] = 0
      if (g > 255):
        drawEvents[(i | 10)] = 255
      elif (g < 0):
        drawEvents[(i | 10)] = 0
      if (b > 255):
        drawEvents[(i | 11)] = 255
      elif (b < 0):
        drawEvents[(i | 11)] = 0
      if (a > 255):
        drawEvents[(i | 12)] = 255
      elif (a < 0):
        drawEvents[(i | 12)] = 0
    elif (sc_0 == 4):
      r = drawEvents[(i | 10)]
      g = drawEvents[(i | 11)]
      b = drawEvents[(i | 12)]
      a = drawEvents[(i | 13)]
      if (r > 255):
        drawEvents[(i | 10)] = 255
      elif (r < 0):
        drawEvents[(i | 10)] = 0
      if (g > 255):
        drawEvents[(i | 11)] = 255
      elif (g < 0):
        drawEvents[(i | 11)] = 0
      if (b > 255):
        drawEvents[(i | 12)] = 255
      elif (b < 0):
        drawEvents[(i | 12)] = 0
      if (a > 255):
        drawEvents[(i | 13)] = 255
      elif (a < 0):
        drawEvents[(i | 13)] = 0
    i += 16
  return vm[14]

swlookup__lib_graphics2d_renderQueueValidateArgs__0 = { 1: 0, 2: 0, 3: 1, 4: 2, 5: 3, 8: 4 }

def lib_graphics2d_scale(vm, args):
  objArray1 = None
  objArray2 = None
  objInstance1 = None
  objInstance2 = None
  arg2 = args[1]
  arg3 = args[2]
  arg4 = args[3]
  arg5 = args[4]
  arg6 = args[5]
  int1 = arg3[1]
  int2 = arg4[1]
  objInstance1 = arg5[1]
  object1 = objInstance1[3][3]
  objInstance1 = arg6[1]
  objArray1 = (PST_NoneListOfOne * 6)
  objInstance1[3] = objArray1
  objArray1[0] = False
  objArray1[1] = True
  objArray1[2] = 0
  objArray1[3] = TranslationHelper_globals['LIB:GAME:g2dscale'](object1, int1, int2)
  objArray1[4] = int1
  objArray1[5] = int2
  objInstance2 = arg2[1]
  objArray1 = (PST_NoneListOfOne * 7)
  objInstance2[3] = objArray1
  objInstance2 = args[0][1]
  objArray2 = objInstance2[3]
  i = 4
  while (i >= 1):
    objArray1[i] = objArray2[i]
    i -= 1
  objArray1[5] = int1
  objArray1[6] = int2
  objInstance1 = arg6[1]
  objArray1[0] = objInstance1[3]
  return args[0]
