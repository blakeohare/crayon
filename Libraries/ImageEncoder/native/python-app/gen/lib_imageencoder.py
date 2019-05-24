import base64
import code.vm as VM
import math
import os
import random
import sys
import time

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
