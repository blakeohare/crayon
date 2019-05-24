import base64
import code.vm as VM
import math
import os
import random
import sys
import time

def lib_cryptocommon_addBytes(vm, args):
  obj = args[0][1]
  fromByteList = args[1][1]
  toByteList = obj[3][0]
  length = fromByteList[1]
  i = 0
  while (i < length):
    toByteList.append(fromByteList[2][i][1])
    i += 1
  return vm[16]

def lib_cryptocommon_initHash(vm, args):
  obj = args[0][1]
  obj[3] = [None]
  obj[3][0] = []
  return vm[14]
