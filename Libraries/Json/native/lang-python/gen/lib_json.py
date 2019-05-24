import base64
import code.vm as VM
import math
import os
import random
import sys
import time

def lib_json_parse(vm, args):
  raw = args[0][1]
  if (len(raw) > 0):
    output = lib_json_parse_impl(vm[13], raw)
    if (output != None):
      return output
  return vm[14]
