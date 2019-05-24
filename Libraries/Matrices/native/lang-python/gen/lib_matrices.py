import base64
import code.vm as VM
import math
import os
import random
import sys
import time

PST_NoneListOfOne = [None]

def lib_matrices_addMatrix(vm, args):
  obj = args[0][1]
  nd1 = obj[3]
  if not (args[2][1]):
    nd1[5] = "Input must be a matrix"
    return vm[14]
  left = nd1[0]
  width = nd1[1]
  height = nd1[2]
  obj = args[1][1]
  nd2 = obj[3]
  right = nd2[0]
  if ((nd2[1] != width) or (nd2[2] != height)):
    nd1[5] = "Matrices must be the same size."
    return vm[14]
  output = left
  isInline = (args[3][0] == 1)
  if isInline:
    nd1[4] = True
  elif not (args[4][1]):
    nd1[5] = "Output value must be a matrix"
    return vm[14]
  else:
    obj = args[3][1]
    nd3 = obj[3]
    output = nd3[0]
    if ((nd3[1] != width) or (nd3[2] != height)):
      nd1[5] = "Output matrix must have the same size as the inputs."
      return vm[14]
    nd3[4] = True
  length = (width * height)
  i = 0
  while (i < length):
    output[i] = (left[i] + right[i])
    i += 1
  return args[0]

def lib_matrices_copyFrom(vm, args):
  obj = args[0][1]
  nd1 = obj[3]
  obj = args[1][1]
  nd2 = obj[3]
  if not (args[2][1]):
    nd1[5] = "value was not a matrix"
    return vm[14]
  if ((nd1[1] != nd2[1]) or (nd1[2] != nd2[2])):
    nd1[5] = "Matrices were not the same size."
    return vm[14]
  target = nd1[0]
  source = nd2[0]
  _len = len(target)
  i = 0
  while (i < _len):
    target[i] = source[i]
    i += 1
  return args[0]

def lib_matrices_getError(vm, args):
  obj = args[0][1]
  return buildString(vm[13], obj[3][5])

def lib_matrices_getValue(vm, args):
  obj = args[0][1]
  nd = obj[3]
  if ((args[1][0] != 3) or (args[2][0] != 3)):
    nd[5] = "Invalid coordinates"
    return vm[14]
  x = args[1][1]
  y = args[2][1]
  width = nd[1]
  height = nd[2]
  if ((x < 0) or (x >= width) or (y < 0) or (y >= height)):
    nd[5] = "Coordinates out of range."
    return vm[14]
  valueArray = nd[3]
  if not (nd[4]):
    data = nd[0]
    length = (width * height)
    i = 0
    while (i < length):
      valueArray[i] = buildFloat(vm[13], data[i])
      i += 1
  return valueArray[((width * y) + x)]

def lib_matrices_initMatrix(vm, args):
  obj = args[0][1]
  nd = obj[3]
  if ((args[1][0] != 3) or (args[2][0] != 3)):
    nd[5] = "Width and height must be integers."
    return vm[15]
  width = args[1][1]
  height = args[2][1]
  size = (width * height)
  data = (PST_NoneListOfOne * size)
  nd[0] = data
  nd[1] = width
  nd[2] = height
  nd[3] = (PST_NoneListOfOne * size)
  nd[4] = False
  nd[5] = ""
  nd[6] = (PST_NoneListOfOne * size)
  i = 0
  while (i < size):
    data[i] = 0.0
    i += 1
  return vm[16]

def lib_matrices_multiplyMatrix(vm, args):
  obj = args[0][1]
  nd1 = obj[3]
  if not (args[2][1]):
    nd1[5] = "argument must be a matrix"
    return vm[14]
  obj = args[1][1]
  nd2 = obj[3]
  isInline = False
  if (args[3][0] == 1):
    isInline = True
  elif not (args[4][1]):
    nd1[5] = "output matrix was unrecognized type."
    return vm[14]
  m1width = nd1[1]
  m1height = nd1[2]
  m2width = nd2[1]
  m2height = nd2[2]
  m3width = m2width
  m3height = m1height
  if (m1width != m2height):
    nd1[5] = "Matrix size mismatch"
    return vm[14]
  m1data = nd1[0]
  m2data = nd2[0]
  nd3 = None
  if isInline:
    nd3 = nd1
    if (m2width != m2height):
      nd1[5] = "You can only multiply a matrix inline with a square matrix."
      return vm[14]
  else:
    obj = args[3][1]
    nd3 = obj[3]
    if ((nd3[1] != m3width) or (nd3[2] != m3height)):
      nd1[5] = "Output matrix is incorrect size."
      return vm[14]
  nd3[4] = True
  m3data = nd3[6]
  x = 0
  y = 0
  i = 0
  m1index = 0
  m2index = 0
  m3index = 0
  value = 0.0
  y = 0
  while (y < m3height):
    x = 0
    while (x < m3width):
      value = 0.0
      m1index = (y * m1height)
      m2index = x
      i = 0
      while (i < m1width):
        value += (m1data[m1index] * m2data[m2index])
        m1index += 1
        m2index += m2width
        i += 1
      m3data[m3index] = value
      m3index += 1
      x += 1
    y += 1
  t = nd3[0]
  nd3[0] = nd3[6]
  nd3[6] = t
  return args[0]

def lib_matrices_multiplyScalar(vm, args):
  obj = args[0][1]
  nd = obj[3]
  isInline = (args[2][0] == 1)
  m1data = nd[0]
  m2data = m1data
  if isInline:
    nd[4] = True
  elif not (args[3][1]):
    nd[5] = "output must be a matrix instance"
    return vm[14]
  else:
    obj = args[2][1]
    nd2 = obj[3]
    if ((nd[1] != nd2[1]) or (nd[2] != nd2[2])):
      nd[5] = "output matrix must be the same size."
      return vm[14]
    m2data = nd2[0]
    nd2[4] = True
  scalar = 0.0
  if (args[1][0] == 4):
    scalar = args[1][1]
  elif (args[1][0] == 3):
    scalar = (0.0 + args[1][1])
  else:
    nd[5] = "scalar must be a number"
    return vm[14]
  i = 0
  length = len(m1data)
  i = 0
  while (i < length):
    m2data[i] = (m1data[i] * scalar)
    i += 1
  return args[0]

def lib_matrices_setValue(vm, args):
  obj = args[0][1]
  nd = obj[3]
  if ((args[1][0] != 3) or (args[2][0] != 3)):
    nd[5] = "Invalid coordinates"
    return vm[14]
  x = args[1][1]
  y = args[2][1]
  width = nd[1]
  height = nd[2]
  if ((x < 0) or (x >= width) or (y < 0) or (y >= height)):
    nd[5] = "Coordinates out of range."
    return vm[14]
  value = 0.0
  if (args[3][0] == 4):
    value = args[3][1]
  elif (args[3][0] == 3):
    value = (0.0 + args[3][1])
  else:
    nd[5] = "Value must be a number."
    return vm[14]
  index = ((y * width) + x)
  data = nd[0]
  valueArray = nd[3]
  data[index] = value
  valueArray[index] = buildFloat(vm[13], value)
  return args[0]

def lib_matrices_toVector(vm, args):
  obj = args[0][1]
  nd = obj[3]
  data = nd[0]
  width = nd[1]
  height = nd[2]
  length = (width * height)
  if (args[1][0] != 6):
    nd[5] = "Output argument must be a list"
    return vm[14]
  output = args[1][1]
  while (output[1] < length):
    addToList(output, vm[14])
  value = 0.0
  toList = None
  i = 0
  while (i < length):
    value = data[i]
    if (value == 0):
      toList = vm[13][6]
    elif (value == 1):
      toList = vm[13][7]
    else:
      toList = [4, data[i]]
    output[2][i] = toList
    i += 1
  return args[1]
