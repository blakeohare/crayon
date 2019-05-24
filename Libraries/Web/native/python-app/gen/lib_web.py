import base64
import code.vm as VM
import math
import os
import random
import sys
import time

def lib_web_launch_browser(vm, args):
  url = args[0][1]
  library_web_launch_browser(url)
  return vm[14]
