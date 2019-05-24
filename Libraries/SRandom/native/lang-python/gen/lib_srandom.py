import base64
import code.vm as VM
import math
import os
import random
import sys
import time

def lib_srandom_getBoolean(vm, args):
  intPtr = args[0][1]
  value = 0
  value = (((intPtr[2][0][1] * 20077) + 12345) & 65535)
  intPtr[2][0] = buildInteger(vm[13], value)
  if ((value & 1) == 0):
    return vm[16]
  return vm[15]

def lib_srandom_getFloat(vm, args):
  intPtr = args[0][1]
  value1 = 0
  value1 = (((intPtr[2][0][1] * 20077) + 12345) & 65535)
  value2 = (((value1 * 20077) + 12345) & 65535)
  value3 = (((value2 * 20077) + 12345) & 65535)
  intPtr[2][0] = buildInteger(vm[13], value3)
  value1 = ((value1 >> 8) & 255)
  value2 = ((value2 >> 8) & 255)
  value3 = ((value3 >> 8) & 255)
  return buildFloat(vm[13], (1.0 * (((value1 << 16) | (value2 << 8) | value3)) / (16777216.0)))

def lib_srandom_getInteger(vm, args):
  intPtr = args[0][1]
  value1 = 0
  value1 = (((intPtr[2][0][1] * 20077) + 12345) & 65535)
  value2 = (((value1 * 20077) + 12345) & 65535)
  value3 = (((value2 * 20077) + 12345) & 65535)
  value4 = (((value3 * 20077) + 12345) & 65535)
  intPtr[2][0] = buildInteger(vm[13], value4)
  value1 = ((value1 >> 8) & 255)
  value2 = ((value2 >> 8) & 255)
  value3 = ((value3 >> 8) & 255)
  value4 = ((value4 >> 8) & 127)
  return buildInteger(vm[13], ((value4 << 24) | (value3 << 16) | (value2 << 8) | value1))
