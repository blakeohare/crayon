import base64
import code.vm as VM
import math
import os
import random
import sys
import time

def always_false(): return False

def lib_gamegifcap_createGifContext(vm, args):
  ms = args[1][1]
  oi = args[0][1]
  oi[3][0] = {}
  return vm[14]

def lib_gamegifcap_isSupported(vm, args):
  if always_false():
    return vm[15]
  return vm[16]

def lib_gamegifcap_saveToDisk(vm, args):
  oi = args[0][1]
  ctx = oi[3][0]
  path = args[1][1]
  sc = 0
  return buildInteger(vm[13], sc)

def lib_gamegifcap_screenCap(vm, args):
  oiCtx = args[0][1]
  oiGw = args[1][1]
  sc = 0
  return buildInteger(vm[13], sc)

def lib_gamegifcap_setRecordSize(vm, args):
  oi = args[0][1]
  w = args[1][1]
  h = args[2][1]
  always_false()
  return vm[14]
