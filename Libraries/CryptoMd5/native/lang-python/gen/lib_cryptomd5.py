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

def lib_md5_addBytes(vm, args):
  obj = args[0][1]
  fromByteList = args[1][1]
  toByteList = obj[3][0]
  length = fromByteList[1]
  i = 0
  while (i < length):
    toByteList.append(fromByteList[2][i][1])
    i += 1
  return vm[16]

def lib_md5_bitShiftRight(value, amount):
  if (amount == 0):
    return value
  if (value > 0):
    return (value >> amount)
  mask = 2147483647
  return (((value >> 1)) & ((mask >> ((amount - 1)))))

def lib_md5_bitwiseNot(x):
  return (-x - 1)

def lib_md5_digestMd5(vm, args):
  obj = args[0][1]
  output = args[1][1]
  byteList = obj[3][0]
  resultBytes = lib_md5_digestMd5Impl(byteList)
  i = 0
  while (i < 16):
    b = resultBytes[i]
    addToList(output, vm[13][9][b])
    i += 1
  return args[1]

def lib_md5_digestMd5Impl(inputBytes):
  s = (PST_NoneListOfOne * 64)
  K = (PST_NoneListOfOne * 64)
  s[0] = 7
  s[1] = 12
  s[2] = 17
  s[3] = 22
  s[4] = 7
  s[5] = 12
  s[6] = 17
  s[7] = 22
  s[8] = 7
  s[9] = 12
  s[10] = 17
  s[11] = 22
  s[12] = 7
  s[13] = 12
  s[14] = 17
  s[15] = 22
  s[16] = 5
  s[17] = 9
  s[18] = 14
  s[19] = 20
  s[20] = 5
  s[21] = 9
  s[22] = 14
  s[23] = 20
  s[24] = 5
  s[25] = 9
  s[26] = 14
  s[27] = 20
  s[28] = 5
  s[29] = 9
  s[30] = 14
  s[31] = 20
  s[32] = 4
  s[33] = 11
  s[34] = 16
  s[35] = 23
  s[36] = 4
  s[37] = 11
  s[38] = 16
  s[39] = 23
  s[40] = 4
  s[41] = 11
  s[42] = 16
  s[43] = 23
  s[44] = 4
  s[45] = 11
  s[46] = 16
  s[47] = 23
  s[48] = 6
  s[49] = 10
  s[50] = 15
  s[51] = 21
  s[52] = 6
  s[53] = 10
  s[54] = 15
  s[55] = 21
  s[56] = 6
  s[57] = 10
  s[58] = 15
  s[59] = 21
  s[60] = 6
  s[61] = 10
  s[62] = 15
  s[63] = 21
  K[0] = lib_md5_uint32Hack(55146, 42104)
  K[1] = lib_md5_uint32Hack(59591, 46934)
  K[2] = lib_md5_uint32Hack(9248, 28891)
  K[3] = lib_md5_uint32Hack(49597, 52974)
  K[4] = lib_md5_uint32Hack(62844, 4015)
  K[5] = lib_md5_uint32Hack(18311, 50730)
  K[6] = lib_md5_uint32Hack(43056, 17939)
  K[7] = lib_md5_uint32Hack(64838, 38145)
  K[8] = lib_md5_uint32Hack(27008, 39128)
  K[9] = lib_md5_uint32Hack(35652, 63407)
  K[10] = lib_md5_uint32Hack(65535, 23473)
  K[11] = lib_md5_uint32Hack(35164, 55230)
  K[12] = lib_md5_uint32Hack(27536, 4386)
  K[13] = lib_md5_uint32Hack(64920, 29075)
  K[14] = lib_md5_uint32Hack(42617, 17294)
  K[15] = lib_md5_uint32Hack(18868, 2081)
  K[16] = lib_md5_uint32Hack(63006, 9570)
  K[17] = lib_md5_uint32Hack(49216, 45888)
  K[18] = lib_md5_uint32Hack(9822, 23121)
  K[19] = lib_md5_uint32Hack(59830, 51114)
  K[20] = lib_md5_uint32Hack(54831, 4189)
  K[21] = lib_md5_uint32Hack(580, 5203)
  K[22] = lib_md5_uint32Hack(55457, 59009)
  K[23] = lib_md5_uint32Hack(59347, 64456)
  K[24] = lib_md5_uint32Hack(8673, 52710)
  K[25] = lib_md5_uint32Hack(49975, 2006)
  K[26] = lib_md5_uint32Hack(62677, 3463)
  K[27] = lib_md5_uint32Hack(17754, 5357)
  K[28] = lib_md5_uint32Hack(43491, 59653)
  K[29] = lib_md5_uint32Hack(64751, 41976)
  K[30] = lib_md5_uint32Hack(26479, 729)
  K[31] = lib_md5_uint32Hack(36138, 19594)
  K[32] = lib_md5_uint32Hack(65530, 14658)
  K[33] = lib_md5_uint32Hack(34673, 63105)
  K[34] = lib_md5_uint32Hack(28061, 24866)
  K[35] = lib_md5_uint32Hack(64997, 14348)
  K[36] = lib_md5_uint32Hack(42174, 59972)
  K[37] = lib_md5_uint32Hack(19422, 53161)
  K[38] = lib_md5_uint32Hack(63163, 19296)
  K[39] = lib_md5_uint32Hack(48831, 48240)
  K[40] = lib_md5_uint32Hack(10395, 32454)
  K[41] = lib_md5_uint32Hack(60065, 10234)
  K[42] = lib_md5_uint32Hack(54511, 12421)
  K[43] = lib_md5_uint32Hack(1160, 7429)
  K[44] = lib_md5_uint32Hack(55764, 53305)
  K[45] = lib_md5_uint32Hack(59099, 39397)
  K[46] = lib_md5_uint32Hack(8098, 31992)
  K[47] = lib_md5_uint32Hack(50348, 22117)
  K[48] = lib_md5_uint32Hack(62505, 8772)
  K[49] = lib_md5_uint32Hack(17194, 65431)
  K[50] = lib_md5_uint32Hack(43924, 9127)
  K[51] = lib_md5_uint32Hack(64659, 41017)
  K[52] = lib_md5_uint32Hack(25947, 22979)
  K[53] = lib_md5_uint32Hack(36620, 52370)
  K[54] = lib_md5_uint32Hack(65519, 62589)
  K[55] = lib_md5_uint32Hack(34180, 24017)
  K[56] = lib_md5_uint32Hack(28584, 32335)
  K[57] = lib_md5_uint32Hack(65068, 59104)
  K[58] = lib_md5_uint32Hack(41729, 17172)
  K[59] = lib_md5_uint32Hack(19976, 4513)
  K[60] = lib_md5_uint32Hack(63315, 32386)
  K[61] = lib_md5_uint32Hack(48442, 62005)
  K[62] = lib_md5_uint32Hack(10967, 53947)
  K[63] = lib_md5_uint32Hack(60294, 54161)
  a0 = lib_md5_uint32Hack(26437, 8961)
  b0 = lib_md5_uint32Hack(61389, 43913)
  c0 = lib_md5_uint32Hack(39098, 56574)
  d0 = lib_md5_uint32Hack(4146, 21622)
  originalLength = len(inputBytes)
  inputBytes.append(128)
  while ((len(inputBytes) % 64) != 56):
    inputBytes.append(0)
  inputBytes.append(0)
  inputBytes.append(0)
  inputBytes.append(0)
  inputBytes.append(0)
  inputBytes.append(((originalLength >> 24) & 255))
  inputBytes.append(((originalLength >> 16) & 255))
  inputBytes.append(((originalLength >> 8) & 255))
  inputBytes.append((originalLength & 255))
  M = (PST_NoneListOfOne * 16)
  m = 0
  mask32 = lib_md5_uint32Hack(65535, 65535)
  F = 0
  g = 0
  t = 0
  rotAmt = 0
  chunkIndex = 0
  while (chunkIndex < len(inputBytes)):
    j = 0
    while (j < 16):
      t = (chunkIndex + (j * 4))
      m = (inputBytes[t] << 24)
      m += (inputBytes[(t + 1)] << 16)
      m += (inputBytes[(t + 2)] << 8)
      m += inputBytes[(t + 3)]
      M[j] = m
      j += 1
    A = a0
    B = b0
    C = c0
    D = d0
    i = 0
    while (i < 64):
      if (i < 16):
        F = (((B & C)) | ((lib_md5_bitwiseNot(B) & D)))
        g = i
      elif (i < 32):
        F = (((D & B)) | ((lib_md5_bitwiseNot(D) & C)))
        g = ((((5 * i) + 1)) & 15)
      elif (i < 48):
        F = (B ^ C ^ D)
        g = ((((3 * i) + 5)) & 15)
      else:
        F = (C ^ ((B | lib_md5_bitwiseNot(D))))
        g = (((7 * i)) & 15)
      F = (((F + A + K[i] + M[g])) & mask32)
      A = D
      D = C
      C = B
      rotAmt = s[i]
      B = (B + ((((F << rotAmt)) | lib_md5_bitShiftRight(F, (32 - rotAmt)))))
      i += 1
    a0 = (((a0 + A)) & mask32)
    b0 = (((b0 + B)) & mask32)
    c0 = (((c0 + C)) & mask32)
    d0 = (((d0 + D)) & mask32)
    chunkIndex += 64
  output = (PST_NoneListOfOne * 16)
  output[0] = ((a0 >> 24) & 255)
  output[1] = ((a0 >> 16) & 255)
  output[2] = ((a0 >> 8) & 255)
  output[3] = (a0 & 255)
  output[4] = ((b0 >> 24) & 255)
  output[5] = ((b0 >> 16) & 255)
  output[6] = ((b0 >> 8) & 255)
  output[7] = (b0 & 255)
  output[8] = ((c0 >> 24) & 255)
  output[9] = ((c0 >> 16) & 255)
  output[10] = ((c0 >> 8) & 255)
  output[11] = (c0 & 255)
  output[12] = ((d0 >> 24) & 255)
  output[13] = ((d0 >> 16) & 255)
  output[14] = ((d0 >> 8) & 255)
  output[15] = (d0 & 255)
  return output

def lib_md5_initHash(vm, args):
  obj = args[0][1]
  obj[3] = [None]
  obj[3][0] = []
  return vm[14]

def lib_md5_uint32Hack(left, right):
  return (((left << 16)) | right)
