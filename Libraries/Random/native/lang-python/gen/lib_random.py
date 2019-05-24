import base64
import code.vm as VM
import math
import os
import random
import sys
import time

def lib_random_random_bool(vm, args):
  if (random.random() < 0.5):
    return vm[15]
  return vm[16]

def lib_random_random_float(vm, args):
  return [4, random.random()]

def lib_random_random_int(vm, args):
  if ((args[0][0] != 3) or (args[1][0] != 3)):
    return vm[14]
  lower = args[0][1]
  upper = args[1][1]
  if (lower >= upper):
    return vm[14]
  value = int(((random.random() * (upper - lower))))
  return buildInteger(vm[13], (lower + value))
