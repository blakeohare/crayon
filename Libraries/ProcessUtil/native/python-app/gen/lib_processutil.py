import base64
import code.vm as VM
import math
import os
import random
import subprocess
import sys
import threading
import time

def always_true(): return True

PST_NoneListOfOne = [None]

def lib_processutil_isSupported(vm, args):
  t = always_true()
  return buildBoolean(vm[13], t)

def lib_processutil_launchProcess(vm, args):
  bridge = args[0][1]
  bridge[3] = (PST_NoneListOfOne * 5)
  bridge[3][0] = True
  bridge[3][1] = 0
  bridge[3][2] = []
  bridge[3][3] = []
  bridge[3][4] = threading.Lock()
  execName = args[1][1]
  argsRaw = args[2][1]
  isAsync = args[3][1]
  cb = args[4]
  dispatcherQueue = args[5][1]
  argStrings = []
  i = 0
  while (i < argsRaw[1]):
    a = getItemFromList(argsRaw, i)
    argStrings.append(a[1])
    i += 1
  _ProcessUtilHelper_launch_process_impl(bridge[3], execName, argStrings, isAsync, cb, dispatcherQueue[3])
  return vm[14]

def lib_processutil_readBridge(vm, args):
  bridge = args[0][1]
  outputList = args[1][1]
  type = args[2][1]
  mtx = bridge[3][4]
  if (type == 1):
    outputInt = _ProcessUtilHelper_read_bridge_int(mtx, bridge[3], type)
    addToList(outputList, buildInteger(vm[13], outputInt))
  else:
    output = []
    _ProcessUtilHelper_read_bridge_strings(mtx, bridge[3], type, output)
    i = 0
    while (i < len(output)):
      addToList(outputList, buildString(vm[13], output[i]))
      i += 1
  return vm[14]
