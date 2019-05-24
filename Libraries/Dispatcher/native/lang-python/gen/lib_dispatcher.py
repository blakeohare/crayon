import base64
import code.vm as VM
import math
import os
import random
import sys
import threading
import time

def lib_dispatcher_flushNativeQueue(vm, args):
  nd = (args[0][1])[3]
  output = []
  lib_dispatcher_flushNativeQueueImpl(nd, output)
  if (len(output) == 0):
    return vm[14]
  return buildList(output)

def lib_dispatcher_initNativeQueue(vm, args):
  obj = args[0][1]
  nd = [None, None]
  nd[0] = threading.Lock()
  nd[1] = []
  obj[3] = nd
  return vm[14]
