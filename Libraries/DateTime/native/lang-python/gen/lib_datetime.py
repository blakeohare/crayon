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

def lib_datetime_getNativeTimezone(value):
  tzObj = value[1]
  if (tzObj[3] == None):
    return None
  return tzObj[3][0]

def lib_datetime_getOffsetFromUtcNow(vm, args):
  nativeTz = lib_datetime_getNativeTimezone(args[0])
  if (nativeTz == None):
    return vm[13][3]
  offset = lib_datetime_getOffsetFromUtcNow(nativeTz)
  return buildInteger(vm[13], offset)

def lib_datetime_initTimeZone(vm, args):
  timezone = args[0][1]
  timezone[3] = [None]
  nativeTzRef = None
  readableName = None
  offsetFromUtc = 0
  isDstObserved = 0
  fingerprint = None
  if (args[1][0] == 1):
    strOut = PST_StringBuffer16
    intOut = PST_IntBuffer16
    nativeTzRef = LIB$datetime_getDataForLocalTimeZone(strOut, intOut)
    readableName = strOut[0]
    fingerprint = strOut[1]
    offsetFromUtc = intOut[0]
    isDstObserved = intOut[1]
  else:
    return vm[14]
  timezone[3] = (PST_NoneListOfOne * 5)
  timezone[3][0] = nativeTzRef
  timezone[3][1] = readableName
  timezone[3][2] = offsetFromUtc
  timezone[3][3] = (isDstObserved == 1)
  timezone[3][4] = fingerprint
  values = []
  values.append(buildString(vm[13], readableName))
  values.append(buildInteger(vm[13], offsetFromUtc))
  values.append(buildBoolean(vm[13], (isDstObserved == 1)))
  values.append(buildString(vm[13], fingerprint))
  return buildList(values)

def lib_datetime_initTimeZoneList(vm, args):
  obj = args[0][1]
  obj[3] = [None]
  timezones = lib_datetime_initializeTimeZoneList()
  obj[3][0] = timezones
  length = len(timezones)
  return buildInteger(vm[13], length)

def lib_datetime_isDstOccurringAt(vm, args):
  nativeTz = lib_datetime_getNativeTimezone(args[0])
  unixtime = args[1][1]
  return buildBoolean(vm[13], lib_datetime_isDstOccurringAt(nativeTz, unixtime))

def lib_datetime_parseDate(vm, args):
  year = args[0][1]
  month = args[1][1]
  day = args[2][1]
  hour = args[3][1]
  minute = args[4][1]
  microseconds = args[5][1]
  nullableTimeZone = lib_datetime_getNativeTimezone(args[6])
  if ((year >= 1970) and (year < 2100) and (month >= 1) and (month <= 12) and (day >= 1) and (day <= 31) and (hour >= 0) and (hour < 24) and (minute >= 0) and (minute < 60) and (microseconds >= 0) and (microseconds < 60000000)):
    intOut = PST_IntBuffer16
    lib_datetime_parseDate(intOut, nullableTimeZone, year, month, day, hour, minute, microseconds)
    if (intOut[0] == 1):
      return buildInteger(vm[13], intOut[1])
  return vm[14]

def lib_datetime_unixToStructured(vm, args):
  unixTime = args[0][1]
  nullableTimeZone = lib_datetime_getNativeTimezone(args[1])
  output = []
  intOut = PST_IntBuffer16
  success = lib_datetime_unixToStructured(intOut, nullableTimeZone, unixTime)
  if not (success):
    return vm[14]
  i = 0
  while (i < 9):
    output.append(buildInteger(vm[13], intOut[i]))
    i += 1
  return buildList(output)
