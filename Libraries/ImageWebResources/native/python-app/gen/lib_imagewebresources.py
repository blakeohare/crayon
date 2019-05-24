import base64
import code.vm as VM
import math
import os
import random
import sys
import time

PST_NoneListOfOne = [None]

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
