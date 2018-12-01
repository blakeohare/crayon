import base64
import code.vm as VM
import math
import os
import random
import sys
import time

PST_StringBuffer16 = [None] * 16
PST_IntBuffer16 = [0] * 16
PST_FloatBuffer16 = [0.0] * 16
PST_NoneListOfOne = [None]

PST_StringType = type('')
def PST_base64ToString(value):
  u_value = base64.b64decode(value)
  if type(u_value) == PST_StringType:
    return u_value
  return u_value.decode('utf8')

def PST_isValidInteger(value):
  if len(value) == 0: return False
  if value[0] == '-': value = value[1:]
  return value.isdigit()

def PST_sortedCopyOfList(t):
  t = t[:]
  t.sort()
  return t

def PST_tryParseFloat(value, floatOut):
  try:
    floatOut[1] = float(value)
    floatOut[0] = 1.0
  except:
    floatOut[0] = -1.0

def PST_stringCheckSlice(haystack, i, needle):
  return haystack[i:i + len(needle)] == needle

def always_true(): return True
def always_false(): return False

def lib_easing_apply_pts(vm, args):
  sampleValues = args[1][1]
  _len = sampleValues[1]
  samples = (PST_NoneListOfOne * _len)
  i = 0
  while (i < _len):
    samples[i] = sampleValues[2][i][1]
    i += 1
  samples[0] = 0.0
  samples[(_len - 1)] = 1.0
  o = args[0][1]
  o[4] = [_len, samples]
  return vm[13][0]

def lib_easing_interpolate(vm, args):
  arg2 = args[1]
  arg3 = args[2]
  arg4 = args[3]
  arg5 = args[4]
  arg6 = args[5]
  o = args[0][1]
  es = o[4]
  samples = es[1]
  _len = es[0]
  int1 = args[6][1]
  float1 = 0.0
  float2 = 0.0
  float3 = 0.0
  if (arg4[0] == 3):
    float1 = (0.0 + arg4[1])
  elif (arg4[0] == 4):
    float1 = arg4[1]
  else:
    return vm[13][0]
  if (arg5[0] == 3):
    float2 = (0.0 + arg5[1])
  elif (arg5[0] == 4):
    float2 = arg5[1]
  else:
    return vm[13][0]
  bool1 = False
  bool2 = False
  first = False
  if (int1 == 2):
    first = True
    if ((float1 * 2.0) > float2):
      float1 = ((float2 - float1) * 2)
      bool1 = True
      bool2 = True
    else:
      float1 *= 2.0
  elif (int1 == 1):
    float1 = (float2 - float1)
    bool1 = True
  if (float2 == 0):
    float1 = samples[0]
  else:
    if (float2 < 0):
      float2 = -float2
      float1 = -float1
    if (float1 >= float2):
      float1 = samples[(_len - 1)]
    elif (float1 < 0):
      float1 = samples[0]
    else:
      float1 = (1.0 * (float1) / (float2))
      if (_len > 2):
        float2 = (float1 * _len)
        index = int(float2)
        float2 -= index
        float1 = samples[index]
        if ((index < (_len - 1)) and (float2 > 0)):
          float3 = samples[(index + 1)]
          float1 = ((float1 * (1 - float2)) + (float3 * float2))
  if (arg2[0] == 3):
    float2 = (0.0 + arg2[1])
  elif (arg2[0] == 4):
    float2 = arg2[1]
  else:
    return vm[13][0]
  if (arg3[0] == 3):
    float3 = (0.0 + arg3[1])
  elif (arg3[0] == 4):
    float3 = arg3[1]
  else:
    return vm[13][0]
  if bool1:
    float1 = (1.0 - float1)
  if first:
    float1 *= 0.5
  if bool2:
    float1 += 0.5
  float1 = ((float1 * float3) + ((1 - float1) * float2))
  if ((arg6[0] == 2) and arg6[1]):
    return buildInteger(vm[13], int((float1 + 0.5)))
  return buildFloat(vm[13], float1)
