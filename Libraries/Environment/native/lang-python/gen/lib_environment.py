import base64
import code.vm as VM
import math
import os
import random
import sys
import time

def lib_environment_get(vm, args):
  value = os.environ.get(args[0][1])
  if (value == None):
    return vm[14]
  return buildString(vm[13], value)
