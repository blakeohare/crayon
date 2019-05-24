import base64
import code.vm as VM
import math
import os
import random
import sys
import time

def lib_resources_getResourceData(vm, args):
  output = buildList(vm[9][2])
  vm[9][2] = None
  return output

def lib_resources_readText(vm, args):
  string1 = ResourceReader_readTextFile('res/text/' + args[0][1])
  return buildString(vm[13], string1)
