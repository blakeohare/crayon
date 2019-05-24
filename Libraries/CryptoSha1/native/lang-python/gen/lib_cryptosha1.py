import base64
import code.vm as VM
import math
import os
import random
import sys
import time

PST_NoneListOfOne = [None]

def lib_cryptosha1_bitShiftRight(value, amount):
  if (amount == 0):
    return value
  mask = 2147483647
  value = (value & lib_cryptosha1_uint32Hack(65535, 65535))
  if (value > 0):
    return (value >> amount)
  return (((value >> amount)) & ((mask >> ((amount - 1)))))

def lib_cryptosha1_bitwiseNot(x):
  return (-x - 1)

def lib_cryptosha1_createWordsForBlock(startIndex, byteList, mWords):
  i = 0
  while (i < 64):
    mWords[(i >> 2)] = (((byteList[(startIndex + i)] << 24)) | ((byteList[(startIndex + i + 1)] << 16)) | ((byteList[(startIndex + i + 2)] << 8)) | (byteList[(startIndex + i + 3)]))
    i += 4
  return 0

def lib_cryptosha1_digest(vm, args):
  obj = args[0][1]
  output = args[1][1]
  byteList = obj[3][0]
  resultBytes = lib_cryptosha1_digestImpl(byteList)
  i = 0
  while (i < 20):
    b = resultBytes[i]
    addToList(output, vm[13][9][b])
    i += 1
  return args[1]

def lib_cryptosha1_digestImpl(inputBytes):
  originalLength = (len(inputBytes) * 8)
  h0 = lib_cryptosha1_uint32Hack(26437, 8961)
  h1 = lib_cryptosha1_uint32Hack(61389, 43913)
  h2 = lib_cryptosha1_uint32Hack(39098, 56574)
  h3 = lib_cryptosha1_uint32Hack(4146, 21622)
  h4 = lib_cryptosha1_uint32Hack(50130, 57840)
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
  inputBytes.append(((originalLength >> 0) & 255))
  mWords = (PST_NoneListOfOne * 80)
  mask32 = lib_cryptosha1_uint32Hack(65535, 65535)
  f = 0
  temp = 0
  k = 0
  kValues = (PST_NoneListOfOne * 4)
  kValues[0] = lib_cryptosha1_uint32Hack(23170, 31129)
  kValues[1] = lib_cryptosha1_uint32Hack(28377, 60321)
  kValues[2] = lib_cryptosha1_uint32Hack(36635, 48348)
  kValues[3] = lib_cryptosha1_uint32Hack(51810, 49622)
  chunkIndex = 0
  while (chunkIndex < len(inputBytes)):
    lib_cryptosha1_createWordsForBlock(chunkIndex, inputBytes, mWords)
    i = 16
    while (i < 80):
      mWords[i] = lib_cryptosha1_leftRotate((mWords[(i - 3)] ^ mWords[(i - 8)] ^ mWords[(i - 14)] ^ mWords[(i - 16)]), 1)
      i += 1
    a = h0
    b = h1
    c = h2
    d = h3
    e = h4
    j = 0
    while (j < 80):
      if (j < 20):
        f = (((b & c)) | ((lib_cryptosha1_bitwiseNot(b) & d)))
        k = kValues[0]
      elif (j < 40):
        f = (b ^ c ^ d)
        k = kValues[1]
      elif (j < 60):
        f = (((b & c)) | ((b & d)) | ((c & d)))
        k = kValues[2]
      else:
        f = (b ^ c ^ d)
        k = kValues[3]
      temp = (lib_cryptosha1_leftRotate(a, 5) + f + e + k + mWords[j])
      e = d
      d = c
      c = lib_cryptosha1_leftRotate(b, 30)
      b = a
      a = (temp & mask32)
      j += 1
    h0 = (((h0 + a)) & mask32)
    h1 = (((h1 + b)) & mask32)
    h2 = (((h2 + c)) & mask32)
    h3 = (((h3 + d)) & mask32)
    h4 = (((h4 + e)) & mask32)
    chunkIndex += 64
  output = (PST_NoneListOfOne * 20)
  output[0] = ((h0 >> 24) & 255)
  output[1] = ((h0 >> 16) & 255)
  output[2] = ((h0 >> 8) & 255)
  output[3] = (h0 & 255)
  output[4] = ((h1 >> 24) & 255)
  output[5] = ((h1 >> 16) & 255)
  output[6] = ((h1 >> 8) & 255)
  output[7] = (h1 & 255)
  output[8] = ((h2 >> 24) & 255)
  output[9] = ((h2 >> 16) & 255)
  output[10] = ((h2 >> 8) & 255)
  output[11] = (h2 & 255)
  output[12] = ((h3 >> 24) & 255)
  output[13] = ((h3 >> 16) & 255)
  output[14] = ((h3 >> 8) & 255)
  output[15] = (h3 & 255)
  output[16] = ((h4 >> 24) & 255)
  output[17] = ((h4 >> 16) & 255)
  output[18] = ((h4 >> 8) & 255)
  output[19] = (h4 & 255)
  return output

def lib_cryptosha1_leftRotate(value, amt):
  if (amt == 0):
    return value
  a = (value << amt)
  b = lib_cryptosha1_bitShiftRight(value, (32 - amt))
  result = (a | b)
  return result

def lib_cryptosha1_uint32Hack(left, right):
  return (((left << 16)) | right)
