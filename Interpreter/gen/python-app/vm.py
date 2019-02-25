import base64
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

def addLiteralImpl(vm, row, stringArg):
  g = vm[13]
  type = row[0]
  if (type == 1):
    vm[4][4].append(g[0])
  elif (type == 2):
    vm[4][4].append(buildBoolean(g, (row[1] == 1)))
  elif (type == 3):
    vm[4][4].append(buildInteger(g, row[1]))
  elif (type == 4):
    vm[4][4].append(buildFloat(g, float(stringArg)))
  elif (type == 5):
    vm[4][4].append(buildCommonString(g, stringArg))
  elif (type == 9):
    index = len(vm[4][4])
    vm[4][4].append(buildCommonString(g, stringArg))
    vm[4][20][stringArg] = index
  elif (type == 10):
    cv = [False, row[1]]
    vm[4][4].append([10, cv])
  return 0

def addNameImpl(vm, nameValue):
  index = len(vm[4][1])
  vm[4][2][nameValue] = index
  vm[4][1].append(nameValue)
  if "length" == nameValue:
    vm[4][14] = index
  return 0

def addToList(list, item):
  list[2].append(item)
  list[1] += 1

def applyDebugSymbolData(vm, opArgs, stringData, recentlyDefinedFunction):
  return 0

def buildBoolean(g, value):
  if value:
    return g[1]
  return g[2]

def buildCommonString(g, s):
  value = None
  value = g[11].get(s, None)
  if (value == None):
    value = buildString(g, s)
    g[11][s] = value
  return value

def buildFloat(g, value):
  if (value == 0.0):
    return g[6]
  if (value == 1.0):
    return g[7]
  return [4, value]

def buildInteger(g, num):
  if (num < 0):
    if (num > -257):
      return g[10][-num]
  elif (num < 2049):
    return g[9][num]
  return [3, num]

def buildList(valueList):
  return buildListWithType(None, valueList)

def buildListWithType(type, valueList):
  return [6, [type, len(valueList), valueList]]

def buildNull(globals):
  return globals[0]

def buildRelayObj(type, iarg1, iarg2, iarg3, farg1, sarg1):
  return [type, iarg1, iarg2, iarg3, farg1, sarg1]

def buildString(g, s):
  if (len(s) == 0):
    return g[8]
  return [5, s]

def buildStringDictionary(globals, stringKeys, values):
  size = len(stringKeys)
  d = [size, 5, 0, None, {}, {}, [], []]
  k = None
  i = 0
  while (i < size):
    k = stringKeys[i]
    if (k in d[5]):
      d[7][d[5][k]] = values[i]
    else:
      d[5][k] = len(d[7])
      d[7].append(values[i])
      d[6].append(buildString(globals, k))
    i += 1
  d[0] = len(d[7])
  return [7, d]

def canAssignGenericToGeneric(vm, gen1, gen1Index, gen2, gen2Index, newIndexOut):
  if (gen2 == None):
    return True
  if (gen1 == None):
    return False
  t1 = gen1[gen1Index]
  t2 = gen2[gen2Index]
  sc_0 = swlookup__canAssignGenericToGeneric__0.get(t1, 6)
  if (sc_0 < 4):
    if (sc_0 < 2):
      if (sc_0 == 0):
        newIndexOut[0] = (gen1Index + 1)
        newIndexOut[1] = (gen2Index + 2)
        return (t2 == t1)
      else:
        newIndexOut[0] = (gen1Index + 1)
        newIndexOut[1] = (gen2Index + 2)
        return ((t2 == 3) or (t2 == 4))
    elif (sc_0 == 2):
      newIndexOut[0] = (gen1Index + 1)
      newIndexOut[1] = (gen2Index + 2)
      if (t2 != 8):
        return False
      c1 = gen1[(gen1Index + 1)]
      c2 = gen2[(gen2Index + 1)]
      if (c1 == c2):
        return True
      return isClassASubclassOf(vm, c1, c2)
    else:
      if (t2 != 6):
        return False
      return canAssignGenericToGeneric(vm, gen1, (gen1Index + 1), gen2, (gen2Index + 1), newIndexOut)
  elif (sc_0 == 4):
    if (t2 != 7):
      return False
    if not (canAssignGenericToGeneric(vm, gen1, (gen1Index + 1), gen2, (gen2Index + 1), newIndexOut)):
      return False
    return canAssignGenericToGeneric(vm, gen1, newIndexOut[0], gen2, newIndexOut[1], newIndexOut)
  elif (sc_0 == 5):
    if (t2 != 9):
      return False
    return False
  else:
    return False

swlookup__canAssignGenericToGeneric__0 = { 0: 0, 1: 0, 2: 0, 4: 0, 5: 0, 10: 0, 3: 1, 8: 2, 6: 3, 7: 4, 9: 5 }

def canAssignTypeToGeneric(vm, value, generics, genericIndex):
  sc_0 = swlookup__canAssignTypeToGeneric__0.get(value[0], 7)
  if (sc_0 < 4):
    if (sc_0 < 2):
      if (sc_0 == 0):
        sc_1 = swlookup__canAssignTypeToGeneric__1.get(generics[genericIndex], 1)
        if (sc_1 == 0):
          return value
        return None
      else:
        if (generics[genericIndex] == value[0]):
          return value
        return None
    elif (sc_0 == 2):
      if (generics[genericIndex] == 3):
        return value
      if (generics[genericIndex] == 4):
        return buildFloat(vm[13], (0.0 + value[1]))
      return None
    else:
      if (generics[genericIndex] == 4):
        return value
      return None
  elif (sc_0 < 6):
    if (sc_0 == 4):
      list = value[1]
      listType = list[0]
      genericIndex += 1
      if (listType == None):
        if ((generics[genericIndex] == 1) or (generics[genericIndex] == 0)):
          return value
        return None
      i = 0
      while (i < len(listType)):
        if (listType[i] != generics[(genericIndex + i)]):
          return None
        i += 1
      return value
    else:
      dict = value[1]
      j = genericIndex
      sc_2 = swlookup__canAssignTypeToGeneric__2.get(dict[1], 2)
      if (sc_2 == 0):
        if (generics[1] == dict[1]):
          j += 2
        else:
          return None
      elif (sc_2 == 1):
        if (generics[1] == 8):
          j += 3
        else:
          return None
      valueType = dict[3]
      if (valueType == None):
        if ((generics[j] == 0) or (generics[j] == 1)):
          return value
        return None
      k = 0
      while (k < len(valueType)):
        if (valueType[k] != generics[(j + k)]):
          return None
        k += 1
      return value
  elif (sc_0 == 6):
    if (generics[genericIndex] == 8):
      targetClassId = generics[(genericIndex + 1)]
      givenClassId = (value[1])[0]
      if (targetClassId == givenClassId):
        return value
      if isClassASubclassOf(vm, givenClassId, targetClassId):
        return value
    return None
  return None

swlookup__canAssignTypeToGeneric__1 = { 5: 0, 8: 0, 10: 0, 9: 0, 6: 0, 7: 0 }
swlookup__canAssignTypeToGeneric__2 = { 3: 0, 5: 0, 8: 1 }
swlookup__canAssignTypeToGeneric__0 = { 1: 0, 2: 1, 5: 1, 10: 1, 3: 2, 4: 3, 6: 4, 7: 5, 8: 6 }

def canonicalizeAngle(a):
  twopi = 6.28318530717958
  a = (a % twopi)
  if (a < 0):
    a += twopi
  return a

def canonicalizeListSliceArgs(outParams, beginValue, endValue, beginIndex, endIndex, stepAmount, length, isForward):
  if (beginValue == None):
    if isForward:
      beginIndex = 0
    else:
      beginIndex = (length - 1)
  if (endValue == None):
    if isForward:
      endIndex = length
    else:
      endIndex = (-1 - length)
  if (beginIndex < 0):
    beginIndex += length
  if (endIndex < 0):
    endIndex += length
  if ((beginIndex == 0) and (endIndex == length) and (stepAmount == 1)):
    return 2
  if isForward:
    if (beginIndex >= length):
      return 0
    if (beginIndex < 0):
      return 3
    if (endIndex < beginIndex):
      return 4
    if (beginIndex == endIndex):
      return 0
    if (endIndex > length):
      endIndex = length
  else:
    if (beginIndex < 0):
      return 0
    if (beginIndex >= length):
      return 3
    if (endIndex > beginIndex):
      return 4
    if (beginIndex == endIndex):
      return 0
    if (endIndex < -1):
      endIndex = -1
  outParams[0] = beginIndex
  outParams[1] = endIndex
  return 1

def classIdToString(vm, classId):
  return vm[4][9][classId][16]

def clearList(a):
  if (a[1] == 1):
    a[2].pop()
  else:
    a[2] = []
  a[1] = 0
  return 0

def cloneDictionary(original, clone):
  type = original[1]
  i = 0
  size = original[0]
  kInt = 0
  kString = None
  if (clone == None):
    clone = [0, type, original[2], original[3], {}, {}, [], []]
    if (type == 5):
      while (i < size):
        clone[5][original[6][i][1]] = i
        i += 1
    else:
      while (i < size):
        if (type == 8):
          kInt = (original[6][i][1])[1]
        else:
          kInt = original[6][i][1]
        clone[4][kInt] = i
        i += 1
    i = 0
    while (i < size):
      clone[6].append(original[6][i])
      clone[7].append(original[7][i])
      i += 1
  else:
    i = 0
    while (i < size):
      if (type == 5):
        kString = original[6][i][1]
        if (kString in clone[5]):
          clone[7][clone[5][kString]] = original[7][i]
        else:
          clone[5][kString] = len(clone[7])
          clone[7].append(original[7][i])
          clone[6].append(original[6][i])
      else:
        if (type == 3):
          kInt = original[6][i][1]
        else:
          kInt = (original[6][i][1])[1]
        if (kInt in clone[4]):
          clone[7][clone[4][kInt]] = original[7][i]
        else:
          clone[4][kInt] = len(clone[7])
          clone[7].append(original[7][i])
          clone[6].append(original[6][i])
      i += 1
  clone[0] = (len(clone[4]) + len(clone[5]))
  return clone

def createInstanceType(classId):
  o = [None, None]
  o[0] = 8
  o[1] = classId
  return o

def createVm(rawByteCode, resourceManifest):
  globals = initializeConstantValues()
  resources = resourceManagerInitialize(globals, resourceManifest)
  byteCode = initializeByteCode(rawByteCode)
  localsStack = (PST_NoneListOfOne * 10)
  localsStackSet = (PST_NoneListOfOne * 10)
  i = 0
  i = (len(localsStack) - 1)
  while (i >= 0):
    localsStack[i] = None
    localsStackSet[i] = 0
    i -= 1
  stack = [0, 1, 0, 0, None, False, None, 0, 0, 1, 0, None, None, None]
  executionContext = [0, stack, 0, 100, (PST_NoneListOfOne * 100), localsStack, localsStackSet, 1, 0, False, None, False, 0, None]
  executionContexts = {}
  executionContexts[0] = executionContext
  vm = [executionContexts, executionContext[0], byteCode, [(PST_NoneListOfOne * len(byteCode[0])), None, [], None, None, {}, {}], [None, [], {}, None, [], None, [], None, [], (PST_NoneListOfOne * 100), (PST_NoneListOfOne * 100), {}, None, {}, -1, (PST_NoneListOfOne * 10), 0, None, None, [0, 0, 0], {}, {}, None], 0, False, [], None, resources, [], [[], False, None, None], [[], {}], globals, globals[0], globals[1], globals[2]]
  return vm

def debuggerClearBreakpoint(vm, id):
  return 0

def debuggerFindPcForLine(vm, path, line):
  return -1

def debuggerSetBreakpoint(vm, path, line):
  return -1

def debugSetStepOverBreakpoint(vm):
  return False

def defOriginalCodeImpl(vm, row, fileContents):
  fileId = row[0]
  codeLookup = vm[3][2]
  while (len(codeLookup) <= fileId):
    codeLookup.append(None)
  codeLookup[fileId] = fileContents
  return 0

def dictKeyInfoToString(vm, dict):
  if (dict[1] == 5):
    return "string"
  if (dict[1] == 3):
    return "int"
  if (dict[2] == 0):
    return "instance"
  return classIdToString(vm, dict[2])

def doEqualityComparisonAndReturnCode(a, b):
  leftType = a[0]
  rightType = b[0]
  if (leftType == rightType):
    if (leftType < 6):
      if (a[1] == b[1]):
        return 1
      return 0
    output = 0
    sc_0 = swlookup__doEqualityComparisonAndReturnCode__0.get(leftType, 8)
    if (sc_0 < 5):
      if (sc_0 < 3):
        if (sc_0 == 0):
          output = 1
        elif (sc_0 == 1):
          if (a[1] == b[1]):
            output = 1
        elif (a[1] == b[1]):
          output = 1
      elif (sc_0 == 3):
        if (a[1] == b[1]):
          output = 1
      elif (a[1] == b[1]):
        output = 1
    elif (sc_0 < 7):
      if (sc_0 == 5):
        if a[1] is b[1]:
          output = 1
      else:
        f1 = a[1]
        f2 = b[1]
        if (f1[3] == f2[3]):
          if ((f1[0] == 2) or (f1[0] == 4)):
            if (doEqualityComparisonAndReturnCode(f1[1], f2[1]) == 1):
              output = 1
          else:
            output = 1
    elif (sc_0 == 7):
      c1 = a[1]
      c2 = b[1]
      if (c1[1] == c2[1]):
        output = 1
    else:
      output = 2
    return output
  if (rightType == 1):
    return 0
  if ((leftType == 3) and (rightType == 4)):
    if (a[1] == b[1]):
      return 1
  elif ((leftType == 4) and (rightType == 3)):
    if (a[1] == b[1]):
      return 1
  return 0

swlookup__doEqualityComparisonAndReturnCode__0 = { 1: 0, 3: 1, 4: 2, 2: 3, 5: 4, 6: 5, 7: 5, 8: 5, 9: 6, 10: 7 }

def doExponentMath(globals, b, e, preferInt):
  if (e == 0.0):
    if preferInt:
      return globals[4]
    return globals[7]
  if (b == 0.0):
    if preferInt:
      return globals[3]
    return globals[6]
  r = 0.0
  if (b < 0):
    if ((e >= 0) and (e < 1)):
      return None
    if ((e % 1.0) == 0.0):
      eInt = int(e)
      r = (0.0 + (b ** eInt))
    else:
      return None
  else:
    r = (b ** e)
  if preferInt:
    r = fixFuzzyFloatPrecision(r)
    if ((r % 1.0) == 0.0):
      return buildInteger(globals, int(r))
  return buildFloat(globals, r)

def encodeBreakpointData(vm, breakpoint, pc):
  return None

def errorResult(error):
  return [3, error, 0.0, 0, False, ""]

def EX_AssertionFailed(ec, exMsg):
  return generateException2(ec, 2, exMsg)

def EX_DivisionByZero(ec, exMsg):
  return generateException2(ec, 3, exMsg)

def EX_Fatal(ec, exMsg):
  return generateException2(ec, 0, exMsg)

def EX_IndexOutOfRange(ec, exMsg):
  return generateException2(ec, 4, exMsg)

def EX_InvalidArgument(ec, exMsg):
  return generateException2(ec, 5, exMsg)

def EX_InvalidAssignment(ec, exMsg):
  return generateException2(ec, 6, exMsg)

def EX_InvalidInvocation(ec, exMsg):
  return generateException2(ec, 7, exMsg)

def EX_InvalidKey(ec, exMsg):
  return generateException2(ec, 8, exMsg)

def EX_KeyNotFound(ec, exMsg):
  return generateException2(ec, 9, exMsg)

def EX_NullReference(ec, exMsg):
  return generateException2(ec, 10, exMsg)

def EX_UnassignedVariable(ec, exMsg):
  return generateException2(ec, 11, exMsg)

def EX_UnknownField(ec, exMsg):
  return generateException2(ec, 12, exMsg)

def EX_UnsupportedOperation(ec, exMsg):
  return generateException2(ec, 13, exMsg)

def finalizeInitializationImpl(vm, projectId, localeCount):
  vm[3][1] = vm[3][2][:]
  vm[3][2] = None
  vm[4][19][2] = localeCount
  vm[4][0] = vm[4][1][:]
  vm[4][3] = vm[4][4][:]
  vm[4][12] = primitiveMethodsInitializeLookup(vm[4][2])
  vm[8] = (PST_NoneListOfOne * len(vm[4][0]))
  vm[4][17] = projectId
  vm[4][1] = None
  vm[4][4] = None
  vm[6] = True
  return 0

def fixFuzzyFloatPrecision(x):
  if ((x % 1) != 0):
    u = (x % 1)
    if (u < 0):
      u += 1.0
    roundDown = False
    if (u > 0.9999999999):
      roundDown = True
      x += 0.1
    elif (u < 0.00000000002250000000):
      roundDown = True
    if roundDown:
      if (False or (x > 0)):
        x = (int(x) + 0.0)
      else:
        x = (int(x) - 1.0)
  return x

def generateEsfData(byteCodeLength, esfArgs):
  output = (PST_NoneListOfOne * byteCodeLength)
  esfTokenStack = []
  esfTokenStackTop = None
  esfArgIterator = 0
  esfArgLength = len(esfArgs)
  j = 0
  pc = 0
  while (pc < byteCodeLength):
    if ((esfArgIterator < esfArgLength) and (pc == esfArgs[esfArgIterator])):
      esfTokenStackTop = [None, None]
      j = 1
      while (j < 3):
        esfTokenStackTop[(j - 1)] = esfArgs[(esfArgIterator + j)]
        j += 1
      esfTokenStack.append(esfTokenStackTop)
      esfArgIterator += 3
    while ((esfTokenStackTop != None) and (esfTokenStackTop[1] <= pc)):
      esfTokenStack.pop()
      if (len(esfTokenStack) == 0):
        esfTokenStackTop = None
      else:
        esfTokenStackTop = esfTokenStack[(len(esfTokenStack) - 1)]
    output[pc] = esfTokenStackTop
    pc += 1
  return output

def generateException(vm, stack, pc, valueStackSize, ec, type, message):
  ec[2] = valueStackSize
  stack[0] = pc
  mn = vm[4][19]
  generateExceptionFunctionId = mn[1]
  functionInfo = vm[4][10][generateExceptionFunctionId]
  pc = functionInfo[2]
  if (len(ec[5]) <= (functionInfo[7] + stack[3])):
    increaseLocalsStackCapacity(ec, functionInfo[7])
  localsIndex = stack[3]
  localsStackSetToken = (ec[7] + 1)
  ec[7] = localsStackSetToken
  ec[5][localsIndex] = buildInteger(vm[13], type)
  ec[5][(localsIndex + 1)] = buildString(vm[13], message)
  ec[6][localsIndex] = localsStackSetToken
  ec[6][(localsIndex + 1)] = localsStackSetToken
  ec[1] = [(pc + 1), localsStackSetToken, stack[3], (stack[3] + functionInfo[7]), stack, False, None, valueStackSize, 0, (stack[9] + 1), 0, None, None, None]
  return [5, None, 0.0, 0, False, ""]

def generateException2(ec, exceptionType, exMsg):
  ec[13] = [1, exceptionType, exMsg, 0.0, None]
  return True

def generatePrimitiveMethodReference(lookup, globalNameId, context):
  functionId = resolvePrimitiveMethodName2(lookup, context[0], globalNameId)
  if (functionId < 0):
    return None
  return [9, [4, context, 0, functionId, None]]

def generateTokenListFromPcs(vm, pcs):
  output = []
  tokensByPc = vm[3][0]
  token = None
  i = 0
  while (i < len(pcs)):
    localTokens = tokensByPc[pcs[i]]
    if (localTokens == None):
      if (len(output) > 0):
        output.append(None)
    else:
      token = localTokens[0]
      output.append(token)
    i += 1
  return output

def getBinaryOpFromId(id):
  sc_0 = swlookup__getBinaryOpFromId__0.get(id, 15)
  if (sc_0 < 8):
    if (sc_0 < 4):
      if (sc_0 < 2):
        if (sc_0 == 0):
          return "+"
        else:
          return "-"
      elif (sc_0 == 2):
        return "*"
      else:
        return "/"
    elif (sc_0 < 6):
      if (sc_0 == 4):
        return "%"
      else:
        return "**"
    elif (sc_0 == 6):
      return "&"
    else:
      return "|"
  elif (sc_0 < 12):
    if (sc_0 < 10):
      if (sc_0 == 8):
        return "^"
      else:
        return "<<"
    elif (sc_0 == 10):
      return ">>"
    else:
      return "<"
  elif (sc_0 < 14):
    if (sc_0 == 12):
      return "<="
    else:
      return ">"
  elif (sc_0 == 14):
    return ">="
  else:
    return "unknown"

swlookup__getBinaryOpFromId__0 = { 0: 0, 1: 1, 2: 2, 3: 3, 4: 4, 5: 5, 6: 6, 7: 7, 8: 8, 9: 9, 10: 10, 11: 11, 12: 12, 13: 13, 14: 14 }

def getClassTable(vm, classId):
  oldTable = vm[4][9]
  oldLength = len(oldTable)
  if (classId < oldLength):
    return oldTable
  newLength = (oldLength * 2)
  if (classId >= newLength):
    newLength = (classId + 100)
  newTable = (PST_NoneListOfOne * newLength)
  i = (oldLength - 1)
  while (i >= 0):
    newTable[i] = oldTable[i]
    i -= 1
  vm[4][9] = newTable
  return newTable

def getExecutionContext(vm, id):
  if (id == -1):
    id = vm[1]
  if (id in vm[0]):
    return vm[0][id]
  return None

def getExponentErrorMsg(vm, b, e):
  return ''.join(["Invalid values for exponent computation. Base: ", valueToString(vm, b), ", Power: ", valueToString(vm, e)])

def getFloat(num):
  if (num[0] == 4):
    return num[1]
  return (num[1] + 0.0)

def getFunctionTable(vm, functionId):
  oldTable = vm[4][10]
  oldLength = len(oldTable)
  if (functionId < oldLength):
    return oldTable
  newLength = (oldLength * 2)
  if (functionId >= newLength):
    newLength = (functionId + 100)
  newTable = (PST_NoneListOfOne * newLength)
  i = 0
  while (i < oldLength):
    newTable[i] = oldTable[i]
    i += 1
  vm[4][10] = newTable
  return newTable

def getItemFromList(list, i):
  return list[2][i]

def getNamedCallbackId(vm, scope, functionName):
  return getNamedCallbackIdImpl(vm, scope, functionName, False)

def getNamedCallbackIdImpl(vm, scope, functionName, allocIfMissing):
  lookup = vm[12][1]
  idsForScope = None
  idsForScope = lookup.get(scope, None)
  if (idsForScope == None):
    idsForScope = {}
    lookup[scope] = idsForScope
  id = -1
  id = idsForScope.get(functionName, -1)
  if ((id == -1) and allocIfMissing):
    id = len(vm[12][0])
    vm[12][0].append(None)
    idsForScope[functionName] = id
  return id

def getNativeDataItem(objValue, index):
  obj = objValue[1]
  return obj[3][index]

def getTypeFromId(id):
  sc_0 = swlookup__getTypeFromId__0.get(id, 9)
  if (sc_0 < 5):
    if (sc_0 < 3):
      if (sc_0 == 0):
        return "null"
      elif (sc_0 == 1):
        return "boolean"
      else:
        return "integer"
    elif (sc_0 == 3):
      return "float"
    else:
      return "string"
  elif (sc_0 < 8):
    if (sc_0 == 5):
      return "list"
    elif (sc_0 == 6):
      return "dictionary"
    else:
      return "instance"
  elif (sc_0 == 8):
    return "function"
  return None

swlookup__getTypeFromId__0 = { 1: 0, 2: 1, 3: 2, 4: 3, 5: 4, 6: 5, 7: 6, 8: 7, 9: 8 }

def getVmReinvokeDelay(result):
  return result[2]

def getVmResultAssemblyInfo(result):
  return result[5]

def getVmResultExecId(result):
  return result[3]

def getVmResultStatus(result):
  return result[0]

def increaseListCapacity(list):
  pass

def increaseLocalsStackCapacity(ec, newScopeSize):
  oldLocals = ec[5]
  oldSetIndicator = ec[6]
  oldCapacity = len(oldLocals)
  newCapacity = ((oldCapacity * 2) + newScopeSize)
  newLocals = (PST_NoneListOfOne * newCapacity)
  newSetIndicator = (PST_NoneListOfOne * newCapacity)
  i = 0
  while (i < oldCapacity):
    newLocals[i] = oldLocals[i]
    newSetIndicator[i] = oldSetIndicator[i]
    i += 1
  ec[5] = newLocals
  ec[6] = newSetIndicator
  return 0

def initFileNameSymbolData(vm):
  symbolData = vm[3]
  if (symbolData == None):
    return 0
  if (symbolData[3] == None):
    i = 0
    filenames = (PST_NoneListOfOne * len(symbolData[1]))
    fileIdByPath = {}
    i = 0
    while (i < len(filenames)):
      sourceCode = symbolData[1][i]
      if (sourceCode != None):
        colon = sourceCode.find("\n")
        if (colon != -1):
          filename = sourceCode[0:0 + colon]
          filenames[i] = filename
          fileIdByPath[filename] = i
      i += 1
    symbolData[3] = filenames
    symbolData[4] = fileIdByPath
  return 0

def initializeByteCode(raw):
  index = [None]
  index[0] = 0
  length = len(raw)
  header = read_till(index, raw, length, "@")
  if (header != "CRAYON"):
    pass
  alphaNums = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz"
  opCount = read_integer(index, raw, length, alphaNums)
  ops = (PST_NoneListOfOne * opCount)
  iargs = (PST_NoneListOfOne * opCount)
  sargs = (PST_NoneListOfOne * opCount)
  c = " "
  argc = 0
  j = 0
  stringarg = None
  stringPresent = False
  iarg = 0
  iarglist = None
  i = 0
  i = 0
  while (i < opCount):
    c = raw[index[0]]
    index[0] = (index[0] + 1)
    argc = 0
    stringPresent = True
    if (c == "!"):
      argc = 1
    elif (c == "&"):
      argc = 2
    elif (c == "*"):
      argc = 3
    else:
      if (c != "~"):
        stringPresent = False
        index[0] = (index[0] - 1)
      argc = read_integer(index, raw, length, alphaNums)
    iarglist = (PST_NoneListOfOne * (argc - 1))
    j = 0
    while (j < argc):
      iarg = read_integer(index, raw, length, alphaNums)
      if (j == 0):
        ops[i] = iarg
      else:
        iarglist[(j - 1)] = iarg
      j += 1
    iargs[i] = iarglist
    if stringPresent:
      stringarg = read_string(index, raw, length, alphaNums)
    else:
      stringarg = None
    sargs[i] = stringarg
    i += 1
  hasBreakpoint = (PST_NoneListOfOne * opCount)
  breakpointInfo = (PST_NoneListOfOne * opCount)
  i = 0
  while (i < opCount):
    hasBreakpoint[i] = False
    breakpointInfo[i] = None
    i += 1
  return [ops, iargs, sargs, (PST_NoneListOfOne * opCount), (PST_NoneListOfOne * opCount), [hasBreakpoint, breakpointInfo, {}, 1, 0]]

def initializeClass(pc, vm, args, className):
  i = 0
  memberId = 0
  globalId = 0
  functionId = 0
  t = 0
  classId = args[0]
  baseClassId = args[1]
  globalNameId = args[2]
  constructorFunctionId = args[3]
  staticConstructorFunctionId = args[4]
  staticInitializationState = 0
  if (staticConstructorFunctionId == -1):
    staticInitializationState = 2
  staticFieldCount = args[5]
  assemblyId = args[6]
  staticFields = (PST_NoneListOfOne * staticFieldCount)
  i = 0
  while (i < staticFieldCount):
    staticFields[i] = vm[13][0]
    i += 1
  classInfo = [classId, globalNameId, baseClassId, assemblyId, staticInitializationState, staticFields, staticConstructorFunctionId, constructorFunctionId, 0, None, None, None, None, None, vm[4][21][classId], None, className]
  classTable = getClassTable(vm, classId)
  classTable[classId] = classInfo
  classChain = []
  classChain.append(classInfo)
  classIdWalker = baseClassId
  while (classIdWalker != -1):
    walkerClass = classTable[classIdWalker]
    classChain.append(walkerClass)
    classIdWalker = walkerClass[2]
  baseClass = None
  if (baseClassId != -1):
    baseClass = classChain[1]
  functionIds = []
  fieldInitializationCommand = []
  fieldInitializationLiteral = []
  fieldAccessModifier = []
  globalNameIdToMemberId = {}
  if (baseClass != None):
    i = 0
    while (i < baseClass[8]):
      functionIds.append(baseClass[9][i])
      fieldInitializationCommand.append(baseClass[10][i])
      fieldInitializationLiteral.append(baseClass[11][i])
      fieldAccessModifier.append(baseClass[12][i])
      i += 1
    keys = list(baseClass[13].keys())
    i = 0
    while (i < len(keys)):
      t = keys[i]
      globalNameIdToMemberId[t] = baseClass[13][t]
      i += 1
    keys = list(baseClass[14].keys())
    i = 0
    while (i < len(keys)):
      t = keys[i]
      classInfo[14][t] = baseClass[14][t]
      i += 1
  accessModifier = 0
  i = 7
  while (i < len(args)):
    memberId = args[(i + 1)]
    globalId = args[(i + 2)]
    accessModifier = args[(i + 5)]
    while (memberId >= len(functionIds)):
      functionIds.append(-1)
      fieldInitializationCommand.append(-1)
      fieldInitializationLiteral.append(None)
      fieldAccessModifier.append(0)
    globalNameIdToMemberId[globalId] = memberId
    fieldAccessModifier[memberId] = accessModifier
    if (args[i] == 0):
      fieldInitializationCommand[memberId] = args[(i + 3)]
      t = args[(i + 4)]
      if (t == -1):
        fieldInitializationLiteral[memberId] = vm[13][0]
      else:
        fieldInitializationLiteral[memberId] = vm[4][3][t]
    else:
      functionId = args[(i + 3)]
      functionIds[memberId] = functionId
    i += 6
  classInfo[9] = functionIds[:]
  classInfo[10] = fieldInitializationCommand[:]
  classInfo[11] = fieldInitializationLiteral[:]
  classInfo[12] = fieldAccessModifier[:]
  classInfo[8] = len(functionIds)
  classInfo[13] = globalNameIdToMemberId
  classInfo[15] = (PST_NoneListOfOne * classInfo[8])
  if (baseClass != None):
    i = 0
    while (i < len(baseClass[15])):
      classInfo[15][i] = baseClass[15][i]
      i += 1
  if "Core.Exception" == className:
    mn = vm[4][19]
    mn[0] = classId
  return 0

def initializeClassFieldTypeInfo(vm, opCodeRow):
  classInfo = vm[4][9][opCodeRow[0]]
  memberId = opCodeRow[1]
  _len = len(opCodeRow)
  typeInfo = (PST_NoneListOfOne * (_len - 2))
  i = 2
  while (i < _len):
    typeInfo[(i - 2)] = opCodeRow[i]
    i += 1
  classInfo[15][memberId] = typeInfo
  return 0

def initializeConstantValues():
  pos = (PST_NoneListOfOne * 2049)
  neg = (PST_NoneListOfOne * 257)
  i = 0
  while (i < 2049):
    pos[i] = [3, i]
    i += 1
  i = 1
  while (i < 257):
    neg[i] = [3, -i]
    i += 1
  neg[0] = pos[0]
  globals = [[1, None], [2, True], [2, False], pos[0], pos[1], neg[1], [4, 0.0], [4, 1.0], [5, ""], pos, neg, {}, [None], [None], [None], [None], [None], [None, None]]
  globals[11][""] = globals[8]
  globals[12][0] = 2
  globals[13][0] = 3
  globals[15][0] = 4
  globals[14][0] = 5
  globals[16][0] = 10
  globals[17][0] = 8
  globals[17][1] = 0
  return globals

def initializeFunction(vm, args, currentPc, stringArg):
  functionId = args[0]
  nameId = args[1]
  minArgCount = args[2]
  maxArgCount = args[3]
  functionType = args[4]
  classId = args[5]
  localsCount = args[6]
  numPcOffsetsForOptionalArgs = args[8]
  pcOffsetsForOptionalArgs = (PST_NoneListOfOne * (numPcOffsetsForOptionalArgs + 1))
  i = 0
  while (i < numPcOffsetsForOptionalArgs):
    pcOffsetsForOptionalArgs[(i + 1)] = args[(9 + i)]
    i += 1
  functionTable = getFunctionTable(vm, functionId)
  functionTable[functionId] = [functionId, nameId, currentPc, minArgCount, maxArgCount, functionType, classId, localsCount, pcOffsetsForOptionalArgs, stringArg, None]
  vm[4][22] = functionTable[functionId]
  if (nameId >= 0):
    name = vm[4][0][nameId]
    if "_LIB_CORE_list_filter" == name:
      vm[4][15][0] = functionId
    elif "_LIB_CORE_list_map" == name:
      vm[4][15][1] = functionId
    elif "_LIB_CORE_list_sort_by_key" == name:
      vm[4][15][2] = functionId
    elif "_LIB_CORE_invoke" == name:
      vm[4][15][3] = functionId
    elif "_LIB_CORE_generateException" == name:
      mn = vm[4][19]
      mn[1] = functionId
  return 0

def initializeIntSwitchStatement(vm, pc, args):
  output = {}
  i = 1
  while (i < len(args)):
    output[args[i]] = args[(i + 1)]
    i += 2
  vm[2][3][pc] = output
  return output

def initializeStringSwitchStatement(vm, pc, args):
  output = {}
  i = 1
  while (i < len(args)):
    s = vm[4][3][args[i]][1]
    output[s] = args[(i + 1)]
    i += 2
  vm[2][4][pc] = output
  return output

def initLocTable(vm, row):
  classId = row[0]
  memberCount = row[1]
  nameId = 0
  totalLocales = vm[4][19][2]
  lookup = {}
  i = 2
  while (i < len(row)):
    localeId = row[i]
    i += 1
    j = 0
    while (j < memberCount):
      nameId = row[(i + j)]
      if (nameId != -1):
        lookup[((nameId * totalLocales) + localeId)] = j
      j += 1
    i += memberCount
  vm[4][21][classId] = lookup
  return 0

def interpret(vm, executionContextId):
  output = interpretImpl(vm, executionContextId)
  while ((output[0] == 5) and (output[2] == 0)):
    output = interpretImpl(vm, executionContextId)
  return output

def interpreterFinished(vm, ec):
  if (ec != None):
    id = ec[0]
    if (id in vm[0]):
      vm[0].pop(id)
  return [1, None, 0.0, 0, False, ""]

def interpreterGetExecutionContext(vm, executionContextId):
  executionContexts = vm[0]
  if not ((executionContextId in executionContexts)):
    return None
  return executionContexts[executionContextId]

def interpretImpl(vm, executionContextId):
  metadata = vm[4]
  globals = vm[13]
  VALUE_NULL = globals[0]
  VALUE_TRUE = globals[1]
  VALUE_FALSE = globals[2]
  VALUE_INT_ONE = globals[4]
  VALUE_INT_ZERO = globals[3]
  VALUE_FLOAT_ZERO = globals[6]
  VALUE_FLOAT_ONE = globals[7]
  INTEGER_POSITIVE_CACHE = globals[9]
  INTEGER_NEGATIVE_CACHE = globals[10]
  executionContexts = vm[0]
  ec = interpreterGetExecutionContext(vm, executionContextId)
  if (ec == None):
    return interpreterFinished(vm, None)
  ec[8] += 1
  stack = ec[1]
  ops = vm[2][0]
  args = vm[2][1]
  stringArgs = vm[2][2]
  classTable = vm[4][9]
  functionTable = vm[4][10]
  literalTable = vm[4][3]
  identifiers = vm[4][0]
  valueStack = ec[4]
  valueStackSize = ec[2]
  valueStackCapacity = len(valueStack)
  hasInterrupt = False
  type = 0
  nameId = 0
  classId = 0
  functionId = 0
  localeId = 0
  classInfo = None
  _len = 0
  root = None
  row = None
  argCount = 0
  stringList = None
  returnValueUsed = False
  output = None
  functionInfo = None
  keyType = 0
  intKey = 0
  stringKey = None
  first = False
  primitiveMethodToCoreLibraryFallback = False
  bool1 = False
  bool2 = False
  staticConstructorNotInvoked = True
  int1 = 0
  int2 = 0
  int3 = 0
  i = 0
  j = 0
  float1 = 0.0
  float2 = 0.0
  float3 = 0.0
  floatList1 = [None, None]
  value = None
  value2 = None
  value3 = None
  string1 = None
  string2 = None
  objInstance1 = None
  objInstance2 = None
  list1 = None
  list2 = None
  valueList1 = None
  valueList2 = None
  dictImpl = None
  dictImpl2 = None
  stringList1 = None
  intList1 = None
  valueArray1 = None
  intArray1 = None
  intArray2 = None
  objArray1 = None
  functionPointer1 = None
  intIntDict1 = None
  stringIntDict1 = None
  stackFrame2 = None
  leftValue = None
  rightValue = None
  classValue = None
  arg1 = None
  arg2 = None
  arg3 = None
  tokenList = None
  globalNameIdToPrimitiveMethodName = vm[4][12]
  magicNumbers = vm[4][19]
  integerSwitchesByPc = vm[2][3]
  stringSwitchesByPc = vm[2][4]
  integerSwitch = None
  stringSwitch = None
  esfData = vm[4][18]
  closure = None
  parentClosure = None
  intBuffer = (PST_NoneListOfOne * 16)
  localsStack = ec[5]
  localsStackSet = ec[6]
  localsStackSetToken = stack[1]
  localsStackCapacity = len(localsStack)
  localsStackOffset = stack[2]
  funcArgs = vm[8]
  pc = stack[0]
  nativeFp = None
  debugData = vm[2][5]
  isBreakPointPresent = debugData[0]
  breakpointInfo = None
  debugBreakPointTemporaryDisable = False
  while True:
    row = args[pc]
    sc_0 = swlookup__interpretImpl__0.get(ops[pc], 66)
    if (sc_0 < 34):
      if (sc_0 < 17):
        if (sc_0 < 9):
          if (sc_0 < 5):
            if (sc_0 < 3):
              if (sc_0 == 0):
                # ADD_LITERAL
                addLiteralImpl(vm, row, stringArgs[pc])
              elif (sc_0 == 1):
                # ADD_NAME
                addNameImpl(vm, stringArgs[pc])
              else:
                # ARG_TYPE_VERIFY
                _len = row[0]
                i = 1
                j = 0
                while (j < _len):
                  j += 1
            elif (sc_0 == 3):
              # ASSIGN_CLOSURE
              valueStackSize -= 1
              value = valueStack[valueStackSize]
              i = row[0]
              if (stack[12] == None):
                closure = {}
                closure[-1] = [stack[6]]
                stack[12] = closure
                closure[i] = [value]
              else:
                closure = stack[12]
                if (i in closure):
                  closure[i][0] = value
                else:
                  closure[i] = [value]
            else:
              # ASSIGN_INDEX
              valueStackSize -= 3
              value = valueStack[(valueStackSize + 2)]
              value2 = valueStack[(valueStackSize + 1)]
              root = valueStack[valueStackSize]
              type = root[0]
              bool1 = (row[0] == 1)
              if (type == 6):
                if (value2[0] == 3):
                  i = value2[1]
                  list1 = root[1]
                  if (list1[0] != None):
                    value3 = canAssignTypeToGeneric(vm, value, list1[0], 0)
                    if (value3 == None):
                      hasInterrupt = EX_InvalidArgument(ec, ''.join(["Cannot convert a ", typeToStringFromValue(vm, value), " into a ", typeToString(vm, list1[0], 0)]))
                    value = value3
                  if not (hasInterrupt):
                    if (i >= list1[1]):
                      hasInterrupt = EX_IndexOutOfRange(ec, "Index is out of range.")
                    elif (i < 0):
                      i += list1[1]
                      if (i < 0):
                        hasInterrupt = EX_IndexOutOfRange(ec, "Index is out of range.")
                    if not (hasInterrupt):
                      list1[2][i] = value
                else:
                  hasInterrupt = EX_InvalidArgument(ec, "List index must be an integer.")
              elif (type == 7):
                dictImpl = root[1]
                if (dictImpl[3] != None):
                  value3 = canAssignTypeToGeneric(vm, value, dictImpl[3], 0)
                  if (value3 == None):
                    hasInterrupt = EX_InvalidArgument(ec, "Cannot assign a value to this dictionary of this type.")
                  else:
                    value = value3
                keyType = value2[0]
                if (keyType == 3):
                  intKey = value2[1]
                elif (keyType == 5):
                  stringKey = value2[1]
                elif (keyType == 8):
                  objInstance1 = value2[1]
                  intKey = objInstance1[1]
                else:
                  hasInterrupt = EX_InvalidArgument(ec, "Invalid key for a dictionary.")
                if not (hasInterrupt):
                  bool2 = (dictImpl[0] == 0)
                  if (dictImpl[1] != keyType):
                    if (dictImpl[3] != None):
                      string1 = ''.join(["Cannot assign a key of type ", typeToStringFromValue(vm, value2), " to a dictionary that requires key types of ", dictKeyInfoToString(vm, dictImpl), "."])
                      hasInterrupt = EX_InvalidKey(ec, string1)
                    elif not (bool2):
                      hasInterrupt = EX_InvalidKey(ec, "Cannot have multiple keys in one dictionary with different types.")
                  elif ((keyType == 8) and (dictImpl[2] > 0) and (objInstance1[0] != dictImpl[2])):
                    if isClassASubclassOf(vm, objInstance1[0], dictImpl[2]):
                      hasInterrupt = EX_InvalidKey(ec, "Cannot use this type of object as a key for this dictionary.")
                if not (hasInterrupt):
                  if (keyType == 5):
                    int1 = dictImpl[5].get(stringKey, -1)
                    if (int1 == -1):
                      dictImpl[5][stringKey] = dictImpl[0]
                      dictImpl[0] += 1
                      dictImpl[6].append(value2)
                      dictImpl[7].append(value)
                      if bool2:
                        dictImpl[1] = keyType
                    else:
                      dictImpl[7][int1] = value
                  else:
                    int1 = dictImpl[4].get(intKey, -1)
                    if (int1 == -1):
                      dictImpl[4][intKey] = dictImpl[0]
                      dictImpl[0] += 1
                      dictImpl[6].append(value2)
                      dictImpl[7].append(value)
                      if bool2:
                        dictImpl[1] = keyType
                    else:
                      dictImpl[7][int1] = value
              else:
                hasInterrupt = EX_UnsupportedOperation(ec, getTypeFromId(type) + " type does not support assigning to an index.")
              if bool1:
                valueStack[valueStackSize] = value
                valueStackSize += 1
          elif (sc_0 < 7):
            if (sc_0 == 5):
              # ASSIGN_STATIC_FIELD
              classInfo = classTable[row[0]]
              staticConstructorNotInvoked = True
              if (classInfo[4] < 2):
                stack[0] = pc
                stackFrame2 = maybeInvokeStaticConstructor(vm, ec, stack, classInfo, valueStackSize, PST_IntBuffer16)
                if (PST_IntBuffer16[0] == 1):
                  return generateException(vm, stack, pc, valueStackSize, ec, 0, "Static initialization loop detected. The class this field is a member of is not done being initialized.")
                if (stackFrame2 != None):
                  staticConstructorNotInvoked = False
                  stack = stackFrame2
                  pc = stack[0]
                  localsStackSetToken = stack[1]
                  localsStackOffset = stack[2]
              if staticConstructorNotInvoked:
                valueStackSize -= 1
                classInfo[5][row[1]] = valueStack[valueStackSize]
            else:
              # ASSIGN_FIELD
              valueStackSize -= 2
              value = valueStack[(valueStackSize + 1)]
              value2 = valueStack[valueStackSize]
              nameId = row[2]
              if (value2[0] == 8):
                objInstance1 = value2[1]
                classId = objInstance1[0]
                classInfo = classTable[classId]
                intIntDict1 = classInfo[14]
                if (row[5] == classId):
                  int1 = row[6]
                else:
                  int1 = intIntDict1.get(nameId, -1)
                  if (int1 != -1):
                    int3 = classInfo[12][int1]
                    if (int3 > 1):
                      if (int3 == 2):
                        if (classId != row[3]):
                          int1 = -2
                      else:
                        if ((int3 == 3) or (int3 == 5)):
                          if (classInfo[3] != row[4]):
                            int1 = -3
                        if ((int3 == 4) or (int3 == 5)):
                          i = row[3]
                          if (classId == i):
                            pass
                          else:
                            classInfo = classTable[classInfo[0]]
                            while ((classInfo[2] != -1) and (int1 < len(classTable[classInfo[2]][12]))):
                              classInfo = classTable[classInfo[2]]
                            j = classInfo[0]
                            if (j != i):
                              bool1 = False
                              while ((i != -1) and (classTable[i][2] != -1)):
                                i = classTable[i][2]
                                if (i == j):
                                  bool1 = True
                                  i = -1
                              if not (bool1):
                                int1 = -4
                          classInfo = classTable[classId]
                  row[5] = classId
                  row[6] = int1
                if (int1 > -1):
                  int2 = classInfo[9][int1]
                  if (int2 == -1):
                    intArray1 = classInfo[15][int1]
                    if (intArray1 == None):
                      objInstance1[2][int1] = value
                    else:
                      value2 = canAssignTypeToGeneric(vm, value, intArray1, 0)
                      if (value2 != None):
                        objInstance1[2][int1] = value2
                      else:
                        hasInterrupt = EX_InvalidArgument(ec, "Cannot assign this type to this field.")
                  else:
                    hasInterrupt = EX_InvalidArgument(ec, "Cannot override a method with assignment.")
                elif (int1 < -1):
                  string1 = identifiers[row[0]]
                  if (int1 == -2):
                    string2 = "private"
                  elif (int1 == -3):
                    string2 = "internal"
                  else:
                    string2 = "protected"
                  hasInterrupt = EX_UnknownField(ec, ''.join(["The field '", string1, "' is marked as ", string2, " and cannot be accessed from here."]))
                else:
                  hasInterrupt = EX_InvalidAssignment(ec, ''.join(["'", classInfo[16], "' instances do not have a field called '", metadata[0][row[0]], "'"]))
              else:
                hasInterrupt = EX_InvalidAssignment(ec, "Cannot assign to a field on this type.")
              if (row[1] == 1):
                valueStack[valueStackSize] = value
                valueStackSize += 1
          elif (sc_0 == 7):
            # ASSIGN_THIS_FIELD
            objInstance2 = stack[6][1]
            valueStackSize -= 1
            objInstance2[2][row[0]] = valueStack[valueStackSize]
          else:
            # ASSIGN_LOCAL
            i = (localsStackOffset + row[0])
            valueStackSize -= 1
            localsStack[i] = valueStack[valueStackSize]
            localsStackSet[i] = localsStackSetToken
        elif (sc_0 < 13):
          if (sc_0 < 11):
            if (sc_0 == 9):
              # BINARY_OP
              valueStackSize -= 1
              rightValue = valueStack[valueStackSize]
              leftValue = valueStack[(valueStackSize - 1)]
              sc_1 = swlookup__interpretImpl__1.get(((((leftValue[0] * 15) + row[0]) * 11) + rightValue[0]), 51)
              if (sc_1 < 26):
                if (sc_1 < 13):
                  if (sc_1 < 7):
                    if (sc_1 < 4):
                      if (sc_1 < 2):
                        if (sc_1 == 0):
                          # int ** int
                          value = doExponentMath(globals, (0.0 + leftValue[1]), (0.0 + rightValue[1]), True)
                          if (value == None):
                            hasInterrupt = EX_InvalidArgument(ec, getExponentErrorMsg(vm, leftValue, rightValue))
                        else:
                          # int ** float
                          value = doExponentMath(globals, (0.0 + leftValue[1]), rightValue[1], False)
                          if (value == None):
                            hasInterrupt = EX_InvalidArgument(ec, getExponentErrorMsg(vm, leftValue, rightValue))
                      elif (sc_1 == 2):
                        # float ** int
                        value = doExponentMath(globals, leftValue[1], (0.0 + rightValue[1]), False)
                        if (value == None):
                          hasInterrupt = EX_InvalidArgument(ec, getExponentErrorMsg(vm, leftValue, rightValue))
                      else:
                        # float ** float
                        value = doExponentMath(globals, leftValue[1], rightValue[1], False)
                        if (value == None):
                          hasInterrupt = EX_InvalidArgument(ec, getExponentErrorMsg(vm, leftValue, rightValue))
                    elif (sc_1 == 4):
                      # float % float
                      float1 = rightValue[1]
                      if (float1 == 0):
                        hasInterrupt = EX_DivisionByZero(ec, "Modulo by 0.")
                      else:
                        float3 = (leftValue[1] % float1)
                        if (float3 < 0):
                          float3 += float1
                        value = buildFloat(globals, float3)
                    elif (sc_1 == 5):
                      # float % int
                      int1 = rightValue[1]
                      if (int1 == 0):
                        hasInterrupt = EX_DivisionByZero(ec, "Modulo by 0.")
                      else:
                        float1 = (leftValue[1] % int1)
                        if (float1 < 0):
                          float1 += int1
                        value = buildFloat(globals, float1)
                    else:
                      # int % float
                      float3 = rightValue[1]
                      if (float3 == 0):
                        hasInterrupt = EX_DivisionByZero(ec, "Modulo by 0.")
                      else:
                        float1 = (leftValue[1] % float3)
                        if (float1 < 0):
                          float1 += float3
                        value = buildFloat(globals, float1)
                  elif (sc_1 < 10):
                    if (sc_1 == 7):
                      # int % int
                      int2 = rightValue[1]
                      if (int2 == 0):
                        hasInterrupt = EX_DivisionByZero(ec, "Modulo by 0.")
                      else:
                        int1 = (leftValue[1] % int2)
                        if (int1 < 0):
                          int1 += int2
                        value = buildInteger(globals, int1)
                    elif (sc_1 == 8):
                      # list + list
                      value = [6, valueConcatLists(leftValue[1], rightValue[1])]
                    else:
                      # int + int
                      int1 = (leftValue[1] + rightValue[1])
                      if (int1 < 0):
                        if (int1 > -257):
                          value = INTEGER_NEGATIVE_CACHE[-int1]
                        else:
                          value = [3, int1]
                      elif (int1 < 2049):
                        value = INTEGER_POSITIVE_CACHE[int1]
                      else:
                        value = [3, int1]
                  elif (sc_1 == 10):
                    # int - int
                    int1 = (leftValue[1] - rightValue[1])
                    if (int1 < 0):
                      if (int1 > -257):
                        value = INTEGER_NEGATIVE_CACHE[-int1]
                      else:
                        value = [3, int1]
                    elif (int1 < 2049):
                      value = INTEGER_POSITIVE_CACHE[int1]
                    else:
                      value = [3, int1]
                  elif (sc_1 == 11):
                    # int * int
                    int1 = (leftValue[1] * rightValue[1])
                    if (int1 < 0):
                      if (int1 > -257):
                        value = INTEGER_NEGATIVE_CACHE[-int1]
                      else:
                        value = [3, int1]
                    elif (int1 < 2049):
                      value = INTEGER_POSITIVE_CACHE[int1]
                    else:
                      value = [3, int1]
                  else:
                    # int / int
                    int1 = leftValue[1]
                    int2 = rightValue[1]
                    if (int2 == 0):
                      hasInterrupt = EX_DivisionByZero(ec, "Division by 0.")
                    elif (int1 == 0):
                      value = VALUE_INT_ZERO
                    else:
                      if ((int1 % int2) == 0):
                        int3 = (int1) // (int2)
                      elif (((int1 < 0)) != ((int2 < 0))):
                        float1 = (1 + (1.0 * ((-1.0 * int1)) / (int2)))
                        float1 -= (float1 % 1.0)
                        int3 = int((-float1))
                      else:
                        int3 = (int1) // (int2)
                      if (int3 < 0):
                        if (int3 > -257):
                          value = INTEGER_NEGATIVE_CACHE[-int3]
                        else:
                          value = [3, int3]
                      elif (int3 < 2049):
                        value = INTEGER_POSITIVE_CACHE[int3]
                      else:
                        value = [3, int3]
                elif (sc_1 < 20):
                  if (sc_1 < 17):
                    if (sc_1 < 15):
                      if (sc_1 == 13):
                        # float + int
                        value = buildFloat(globals, (leftValue[1] + rightValue[1]))
                      else:
                        # int + float
                        value = buildFloat(globals, (leftValue[1] + rightValue[1]))
                    elif (sc_1 == 15):
                      # float + float
                      float1 = (leftValue[1] + rightValue[1])
                      if (float1 == 0):
                        value = VALUE_FLOAT_ZERO
                      elif (float1 == 1):
                        value = VALUE_FLOAT_ONE
                      else:
                        value = [4, float1]
                    else:
                      # int - float
                      value = buildFloat(globals, (leftValue[1] - rightValue[1]))
                  elif (sc_1 == 17):
                    # float - int
                    value = buildFloat(globals, (leftValue[1] - rightValue[1]))
                  elif (sc_1 == 18):
                    # float - float
                    float1 = (leftValue[1] - rightValue[1])
                    if (float1 == 0):
                      value = VALUE_FLOAT_ZERO
                    elif (float1 == 1):
                      value = VALUE_FLOAT_ONE
                    else:
                      value = [4, float1]
                  else:
                    # float * int
                    value = buildFloat(globals, (leftValue[1] * rightValue[1]))
                elif (sc_1 < 23):
                  if (sc_1 == 20):
                    # int * float
                    value = buildFloat(globals, (leftValue[1] * rightValue[1]))
                  elif (sc_1 == 21):
                    # float * float
                    value = buildFloat(globals, (leftValue[1] * rightValue[1]))
                  else:
                    # int / float
                    float1 = rightValue[1]
                    if (float1 == 0):
                      hasInterrupt = EX_DivisionByZero(ec, "Division by 0.")
                    else:
                      value = buildFloat(globals, (1.0 * (leftValue[1]) / (float1)))
                elif (sc_1 == 23):
                  # float / int
                  int1 = rightValue[1]
                  if (int1 == 0):
                    hasInterrupt = EX_DivisionByZero(ec, "Division by 0.")
                  else:
                    value = buildFloat(globals, (1.0 * (leftValue[1]) / (int1)))
                elif (sc_1 == 24):
                  # float / float
                  float1 = rightValue[1]
                  if (float1 == 0):
                    hasInterrupt = EX_DivisionByZero(ec, "Division by 0.")
                  else:
                    value = buildFloat(globals, (1.0 * (leftValue[1]) / (float1)))
                else:
                  # int & int
                  value = buildInteger(globals, (leftValue[1] & rightValue[1]))
              elif (sc_1 < 39):
                if (sc_1 < 33):
                  if (sc_1 < 30):
                    if (sc_1 < 28):
                      if (sc_1 == 26):
                        # int | int
                        value = buildInteger(globals, (leftValue[1] | rightValue[1]))
                      else:
                        # int ^ int
                        value = buildInteger(globals, (leftValue[1] ^ rightValue[1]))
                    elif (sc_1 == 28):
                      # int << int
                      int1 = rightValue[1]
                      if (int1 < 0):
                        hasInterrupt = EX_InvalidArgument(ec, "Cannot bit shift by a negative number.")
                      else:
                        value = buildInteger(globals, (leftValue[1] << int1))
                    else:
                      # int >> int
                      int1 = rightValue[1]
                      if (int1 < 0):
                        hasInterrupt = EX_InvalidArgument(ec, "Cannot bit shift by a negative number.")
                      else:
                        value = buildInteger(globals, (leftValue[1] >> int1))
                  elif (sc_1 == 30):
                    # int < int
                    if (leftValue[1] < rightValue[1]):
                      value = VALUE_TRUE
                    else:
                      value = VALUE_FALSE
                  elif (sc_1 == 31):
                    # int <= int
                    if (leftValue[1] <= rightValue[1]):
                      value = VALUE_TRUE
                    else:
                      value = VALUE_FALSE
                  else:
                    # float < int
                    if (leftValue[1] < rightValue[1]):
                      value = VALUE_TRUE
                    else:
                      value = VALUE_FALSE
                elif (sc_1 < 36):
                  if (sc_1 == 33):
                    # float <= int
                    if (leftValue[1] <= rightValue[1]):
                      value = VALUE_TRUE
                    else:
                      value = VALUE_FALSE
                  elif (sc_1 == 34):
                    # int < float
                    if (leftValue[1] < rightValue[1]):
                      value = VALUE_TRUE
                    else:
                      value = VALUE_FALSE
                  else:
                    # int <= float
                    if (leftValue[1] <= rightValue[1]):
                      value = VALUE_TRUE
                    else:
                      value = VALUE_FALSE
                elif (sc_1 == 36):
                  # float < float
                  if (leftValue[1] < rightValue[1]):
                    value = VALUE_TRUE
                  else:
                    value = VALUE_FALSE
                elif (sc_1 == 37):
                  # float <= float
                  if (leftValue[1] <= rightValue[1]):
                    value = VALUE_TRUE
                  else:
                    value = VALUE_FALSE
                else:
                  # int >= int
                  if (leftValue[1] >= rightValue[1]):
                    value = VALUE_TRUE
                  else:
                    value = VALUE_FALSE
              elif (sc_1 < 46):
                if (sc_1 < 43):
                  if (sc_1 < 41):
                    if (sc_1 == 39):
                      # int > int
                      if (leftValue[1] > rightValue[1]):
                        value = VALUE_TRUE
                      else:
                        value = VALUE_FALSE
                    else:
                      # float >= int
                      if (leftValue[1] >= rightValue[1]):
                        value = VALUE_TRUE
                      else:
                        value = VALUE_FALSE
                  elif (sc_1 == 41):
                    # float > int
                    if (leftValue[1] > rightValue[1]):
                      value = VALUE_TRUE
                    else:
                      value = VALUE_FALSE
                  else:
                    # int >= float
                    if (leftValue[1] >= rightValue[1]):
                      value = VALUE_TRUE
                    else:
                      value = VALUE_FALSE
                elif (sc_1 == 43):
                  # int > float
                  if (leftValue[1] > rightValue[1]):
                    value = VALUE_TRUE
                  else:
                    value = VALUE_FALSE
                elif (sc_1 == 44):
                  # float >= float
                  if (leftValue[1] >= rightValue[1]):
                    value = VALUE_TRUE
                  else:
                    value = VALUE_FALSE
                else:
                  # float > float
                  if (leftValue[1] > rightValue[1]):
                    value = VALUE_TRUE
                  else:
                    value = VALUE_FALSE
              elif (sc_1 < 49):
                if (sc_1 == 46):
                  # string + string
                  value = [5, leftValue[1] + rightValue[1]]
                elif (sc_1 == 47):
                  # string * int
                  value = multiplyString(globals, leftValue, leftValue[1], rightValue[1])
                else:
                  # int * string
                  value = multiplyString(globals, rightValue, rightValue[1], leftValue[1])
              elif (sc_1 == 49):
                # list * int
                int1 = rightValue[1]
                if (int1 < 0):
                  hasInterrupt = EX_UnsupportedOperation(ec, "Cannot multiply list by negative number.")
                else:
                  value = [6, valueMultiplyList(leftValue[1], int1)]
              elif (sc_1 == 50):
                # int * list
                int1 = leftValue[1]
                if (int1 < 0):
                  hasInterrupt = EX_UnsupportedOperation(ec, "Cannot multiply list by negative number.")
                else:
                  value = [6, valueMultiplyList(rightValue[1], int1)]
              elif ((row[0] == 0) and (((leftValue[0] == 5) or (rightValue[0] == 5)))):
                value = [5, valueToString(vm, leftValue) + valueToString(vm, rightValue)]
              else:
                # unrecognized op
                hasInterrupt = EX_UnsupportedOperation(ec, ''.join(["The '", getBinaryOpFromId(row[0]), "' operator is not supported for these types: ", getTypeFromId(leftValue[0]), " and ", getTypeFromId(rightValue[0])]))
              valueStack[(valueStackSize - 1)] = value
            else:
              # BOOLEAN_NOT
              value = valueStack[(valueStackSize - 1)]
              if (value[0] != 2):
                hasInterrupt = EX_InvalidArgument(ec, "Boolean expected.")
              elif value[1]:
                valueStack[(valueStackSize - 1)] = VALUE_FALSE
              else:
                valueStack[(valueStackSize - 1)] = VALUE_TRUE
          elif (sc_0 == 11):
            # BREAK
            if (row[0] == 1):
              pc += row[1]
            else:
              intArray1 = esfData[pc]
              pc = (intArray1[1] - 1)
              valueStackSize = stack[7]
              stack[10] = 1
          else:
            # CALL_FUNCTION
            type = row[0]
            argCount = row[1]
            functionId = row[2]
            returnValueUsed = (row[3] == 1)
            classId = row[4]
            if ((type == 2) or (type == 6)):
              # constructor or static method
              classInfo = metadata[9][classId]
              staticConstructorNotInvoked = True
              if (classInfo[4] < 2):
                stack[0] = pc
                stackFrame2 = maybeInvokeStaticConstructor(vm, ec, stack, classInfo, valueStackSize, PST_IntBuffer16)
                if (PST_IntBuffer16[0] == 1):
                  return generateException(vm, stack, pc, valueStackSize, ec, 0, "Static initialization loop detected. The class this field is a member of is not done being initialized.")
                if (stackFrame2 != None):
                  staticConstructorNotInvoked = False
                  stack = stackFrame2
                  pc = stack[0]
                  localsStackSetToken = stack[1]
                  localsStackOffset = stack[2]
            else:
              staticConstructorNotInvoked = True
            if staticConstructorNotInvoked:
              bool1 = True
              # construct args array
              if (argCount == -1):
                valueStackSize -= 1
                value = valueStack[valueStackSize]
                if (value[0] == 1):
                  argCount = 0
                elif (value[0] == 6):
                  list1 = value[1]
                  argCount = list1[1]
                  i = (argCount - 1)
                  while (i >= 0):
                    funcArgs[i] = list1[2][i]
                    i -= 1
                else:
                  hasInterrupt = EX_InvalidArgument(ec, "Function pointers' .invoke method requires a list argument.")
              else:
                i = (argCount - 1)
                while (i >= 0):
                  valueStackSize -= 1
                  funcArgs[i] = valueStack[valueStackSize]
                  i -= 1
              if not (hasInterrupt):
                if (type == 3):
                  value = stack[6]
                  objInstance1 = value[1]
                  if (objInstance1[0] != classId):
                    int2 = row[5]
                    if (int2 != -1):
                      classInfo = classTable[objInstance1[0]]
                      functionId = classInfo[9][int2]
                elif (type == 5):
                  # field invocation
                  valueStackSize -= 1
                  value = valueStack[valueStackSize]
                  localeId = row[5]
                  sc_2 = swlookup__interpretImpl__2.get(value[0], 3)
                  if (sc_2 < 2):
                    if (sc_2 == 0):
                      hasInterrupt = EX_NullReference(ec, "Invoked method on null.")
                    else:
                      # field invoked on an object instance.
                      objInstance1 = value[1]
                      int1 = objInstance1[0]
                      classInfo = classTable[int1]
                      intIntDict1 = classInfo[14]
                      int1 = ((row[4] * magicNumbers[2]) + row[5])
                      i = intIntDict1.get(int1, -1)
                      if (i != -1):
                        int1 = intIntDict1[int1]
                        functionId = classInfo[9][int1]
                        if (functionId > 0):
                          type = 3
                        else:
                          value = objInstance1[2][int1]
                          type = 4
                          valueStack[valueStackSize] = value
                          valueStackSize += 1
                      else:
                        hasInterrupt = EX_UnknownField(ec, "Unknown field.")
                  elif (sc_2 == 2):
                    # field invocation on a class object instance.
                    functionId = resolvePrimitiveMethodName2(globalNameIdToPrimitiveMethodName, value[0], classId)
                    if (functionId < 0):
                      hasInterrupt = EX_InvalidInvocation(ec, "Class definitions do not have that method.")
                    else:
                      functionId = resolvePrimitiveMethodName2(globalNameIdToPrimitiveMethodName, value[0], classId)
                      if (functionId < 0):
                        hasInterrupt = EX_InvalidInvocation(ec, getTypeFromId(value[0]) + " does not have that method.")
                      elif (globalNameIdToPrimitiveMethodName[classId] == 8):
                        type = 6
                        classValue = value[1]
                        if classValue[0]:
                          hasInterrupt = EX_UnsupportedOperation(ec, "Cannot create an instance of an interface.")
                        else:
                          classId = classValue[1]
                          if not (returnValueUsed):
                            hasInterrupt = EX_UnsupportedOperation(ec, "Cannot create an instance and not use the output.")
                          else:
                            classInfo = metadata[9][classId]
                            functionId = classInfo[7]
                      else:
                        type = 9
                  else:
                    # primitive method suspected.
                    functionId = resolvePrimitiveMethodName2(globalNameIdToPrimitiveMethodName, value[0], classId)
                    if (functionId < 0):
                      hasInterrupt = EX_InvalidInvocation(ec, getTypeFromId(value[0]) + " does not have that method.")
                    else:
                      type = 9
              if ((type == 4) and not (hasInterrupt)):
                # pointer provided
                valueStackSize -= 1
                value = valueStack[valueStackSize]
                if (value[0] == 9):
                  functionPointer1 = value[1]
                  sc_3 = swlookup__interpretImpl__3.get(functionPointer1[0], 5)
                  if (sc_3 < 3):
                    if (sc_3 == 0):
                      # pointer to a function
                      functionId = functionPointer1[3]
                      type = 1
                    elif (sc_3 == 1):
                      # pointer to a method
                      functionId = functionPointer1[3]
                      value = functionPointer1[1]
                      type = 3
                    else:
                      # pointer to a static method
                      functionId = functionPointer1[3]
                      classId = functionPointer1[2]
                      type = 2
                  elif (sc_3 == 3):
                    # pointer to a primitive method
                    value = functionPointer1[1]
                    functionId = functionPointer1[3]
                    type = 9
                  elif (sc_3 == 4):
                    # lambda instance
                    value = functionPointer1[1]
                    functionId = functionPointer1[3]
                    type = 10
                    closure = functionPointer1[4]
                else:
                  hasInterrupt = EX_InvalidInvocation(ec, "This type cannot be invoked like a function.")
              if ((type == 9) and not (hasInterrupt)):
                # primitive method invocation
                output = VALUE_NULL
                primitiveMethodToCoreLibraryFallback = False
                sc_4 = swlookup__interpretImpl__4.get(value[0], 5)
                if (sc_4 < 3):
                  if (sc_4 == 0):
                    # ...on a string
                    string1 = value[1]
                    sc_5 = swlookup__interpretImpl__5.get(functionId, 12)
                    if (sc_5 < 7):
                      if (sc_5 < 4):
                        if (sc_5 < 2):
                          if (sc_5 == 0):
                            if (argCount != 1):
                              hasInterrupt = EX_InvalidArgument(ec, primitiveMethodWrongArgCountError("string contains method", 1, argCount))
                            else:
                              value2 = funcArgs[0]
                              if (value2[0] != 5):
                                hasInterrupt = EX_InvalidArgument(ec, "string contains method requires another string as input.")
                              elif (value2[1] in string1):
                                output = VALUE_TRUE
                              else:
                                output = VALUE_FALSE
                          elif (argCount != 1):
                            hasInterrupt = EX_InvalidArgument(ec, primitiveMethodWrongArgCountError("string endsWith method", 1, argCount))
                          else:
                            value2 = funcArgs[0]
                            if (value2[0] != 5):
                              hasInterrupt = EX_InvalidArgument(ec, "string endsWith method requires another string as input.")
                            elif string1.endswith(value2[1]):
                              output = VALUE_TRUE
                            else:
                              output = VALUE_FALSE
                        elif (sc_5 == 2):
                          if (argCount != 1):
                            hasInterrupt = EX_InvalidArgument(ec, primitiveMethodWrongArgCountError("string indexOf method", 1, argCount))
                          else:
                            value2 = funcArgs[0]
                            if (value2[0] != 5):
                              hasInterrupt = EX_InvalidArgument(ec, "string indexOf method requires another string as input.")
                            else:
                              output = buildInteger(globals, string1.find(value2[1]))
                        elif (argCount > 0):
                          hasInterrupt = EX_InvalidArgument(ec, primitiveMethodWrongArgCountError("string lower method", 0, argCount))
                        else:
                          output = buildString(globals, string1.lower())
                      elif (sc_5 == 4):
                        if (argCount > 0):
                          hasInterrupt = EX_InvalidArgument(ec, primitiveMethodWrongArgCountError("string ltrim method", 0, argCount))
                        else:
                          output = buildString(globals, string1.lstrip())
                      elif (sc_5 == 5):
                        if (argCount != 2):
                          hasInterrupt = EX_InvalidArgument(ec, primitiveMethodWrongArgCountError("string replace method", 2, argCount))
                        else:
                          value2 = funcArgs[0]
                          value3 = funcArgs[1]
                          if ((value2[0] != 5) or (value3[0] != 5)):
                            hasInterrupt = EX_InvalidArgument(ec, "string replace method requires 2 strings as input.")
                          else:
                            output = buildString(globals, string1.replace(value2[1], value3[1]))
                      elif (argCount > 0):
                        hasInterrupt = EX_InvalidArgument(ec, primitiveMethodWrongArgCountError("string reverse method", 0, argCount))
                      else:
                        output = buildString(globals, string1[::-1])
                    elif (sc_5 < 10):
                      if (sc_5 == 7):
                        if (argCount > 0):
                          hasInterrupt = EX_InvalidArgument(ec, primitiveMethodWrongArgCountError("string rtrim method", 0, argCount))
                        else:
                          output = buildString(globals, string1.rstrip())
                      elif (sc_5 == 8):
                        if (argCount != 1):
                          hasInterrupt = EX_InvalidArgument(ec, primitiveMethodWrongArgCountError("string split method", 1, argCount))
                        else:
                          value2 = funcArgs[0]
                          if (value2[0] != 5):
                            hasInterrupt = EX_InvalidArgument(ec, "string split method requires another string as input.")
                          else:
                            stringList = string1.split(value2[1])
                            _len = len(stringList)
                            list1 = makeEmptyList(globals[14], _len)
                            i = 0
                            while (i < _len):
                              list1[2].append(buildString(globals, stringList[i]))
                              i += 1
                            list1[1] = _len
                            output = [6, list1]
                      elif (argCount != 1):
                        hasInterrupt = EX_InvalidArgument(ec, primitiveMethodWrongArgCountError("string startsWith method", 1, argCount))
                      else:
                        value2 = funcArgs[0]
                        if (value2[0] != 5):
                          hasInterrupt = EX_InvalidArgument(ec, "string startsWith method requires another string as input.")
                        elif string1.startswith(value2[1]):
                          output = VALUE_TRUE
                        else:
                          output = VALUE_FALSE
                    elif (sc_5 == 10):
                      if (argCount > 0):
                        hasInterrupt = EX_InvalidArgument(ec, primitiveMethodWrongArgCountError("string trim method", 0, argCount))
                      else:
                        output = buildString(globals, string1.strip())
                    elif (sc_5 == 11):
                      if (argCount > 0):
                        hasInterrupt = EX_InvalidArgument(ec, primitiveMethodWrongArgCountError("string upper method", 0, argCount))
                      else:
                        output = buildString(globals, string1.upper())
                    else:
                      output = None
                  elif (sc_4 == 1):
                    # ...on a list
                    list1 = value[1]
                    sc_6 = swlookup__interpretImpl__6.get(functionId, 15)
                    if (sc_6 < 8):
                      if (sc_6 < 4):
                        if (sc_6 < 2):
                          if (sc_6 == 0):
                            if (argCount == 0):
                              hasInterrupt = EX_InvalidArgument(ec, "List add method requires at least one argument.")
                            else:
                              intArray1 = list1[0]
                              i = 0
                              while (i < argCount):
                                value = funcArgs[i]
                                if (intArray1 != None):
                                  value2 = canAssignTypeToGeneric(vm, value, intArray1, 0)
                                  if (value2 == None):
                                    hasInterrupt = EX_InvalidArgument(ec, ''.join(["Cannot convert a ", typeToStringFromValue(vm, value), " into a ", typeToString(vm, list1[0], 0)]))
                                  list1[2].append(value2)
                                else:
                                  list1[2].append(value)
                                i += 1
                              list1[1] += argCount
                              output = VALUE_NULL
                          elif (argCount > 0):
                            hasInterrupt = EX_InvalidArgument(ec, primitiveMethodWrongArgCountError("list choice method", 0, argCount))
                          else:
                            _len = list1[1]
                            if (_len == 0):
                              hasInterrupt = EX_UnsupportedOperation(ec, "Cannot use list.choice() method on an empty list.")
                            else:
                              i = int(((random.random() * _len)))
                              output = list1[2][i]
                        elif (sc_6 == 2):
                          if (argCount > 0):
                            hasInterrupt = EX_InvalidArgument(ec, primitiveMethodWrongArgCountError("list clear method", 0, argCount))
                          elif (list1[1] > 0):
                            if (list1[1] == 1):
                              list1[2].pop()
                            else:
                              list1[2] = []
                            list1[1] = 0
                        elif (argCount > 0):
                          hasInterrupt = EX_InvalidArgument(ec, primitiveMethodWrongArgCountError("list clone method", 0, argCount))
                        else:
                          _len = list1[1]
                          list2 = makeEmptyList(list1[0], _len)
                          i = 0
                          while (i < _len):
                            list2[2].append(list1[2][i])
                            i += 1
                          list2[1] = _len
                          output = [6, list2]
                      elif (sc_6 < 6):
                        if (sc_6 == 4):
                          if (argCount != 1):
                            hasInterrupt = EX_InvalidArgument(ec, primitiveMethodWrongArgCountError("list concat method", 1, argCount))
                          else:
                            value2 = funcArgs[0]
                            if (value2[0] != 6):
                              hasInterrupt = EX_InvalidArgument(ec, "list concat methods requires a list as an argument.")
                            else:
                              list2 = value2[1]
                              intArray1 = list1[0]
                              if ((intArray1 != None) and not (canAssignGenericToGeneric(vm, list2[0], 0, intArray1, 0, intBuffer))):
                                hasInterrupt = EX_InvalidArgument(ec, "Cannot concat a list: incompatible types.")
                              else:
                                if ((intArray1 != None) and (intArray1[0] == 4) and (list2[0][0] == 3)):
                                  bool1 = True
                                else:
                                  bool1 = False
                                _len = list2[1]
                                i = 0
                                while (i < _len):
                                  value = list2[2][i]
                                  if bool1:
                                    value = buildFloat(globals, (0.0 + value[1]))
                                  list1[2].append(value)
                                  i += 1
                                list1[1] += _len
                        elif (argCount != 1):
                          hasInterrupt = EX_InvalidArgument(ec, primitiveMethodWrongArgCountError("list contains method", 1, argCount))
                        else:
                          value2 = funcArgs[0]
                          _len = list1[1]
                          output = VALUE_FALSE
                          i = 0
                          while (i < _len):
                            value = list1[2][i]
                            if (doEqualityComparisonAndReturnCode(value2, value) == 1):
                              output = VALUE_TRUE
                              i = _len
                            i += 1
                      elif (sc_6 == 6):
                        if (argCount != 1):
                          hasInterrupt = EX_InvalidArgument(ec, primitiveMethodWrongArgCountError("list filter method", 1, argCount))
                        else:
                          value2 = funcArgs[0]
                          if (value2[0] != 9):
                            hasInterrupt = EX_InvalidArgument(ec, "list filter method requires a function pointer as its argument.")
                          else:
                            primitiveMethodToCoreLibraryFallback = True
                            functionId = metadata[15][0]
                            funcArgs[1] = value
                            argCount = 2
                            output = None
                      elif (argCount != 2):
                        hasInterrupt = EX_InvalidArgument(ec, primitiveMethodWrongArgCountError("list insert method", 1, argCount))
                      else:
                        value = funcArgs[0]
                        value2 = funcArgs[1]
                        if (value[0] != 3):
                          hasInterrupt = EX_InvalidArgument(ec, "First argument of list.insert must be an integer index.")
                        else:
                          intArray1 = list1[0]
                          if (intArray1 != None):
                            value3 = canAssignTypeToGeneric(vm, value2, intArray1, 0)
                            if (value3 == None):
                              hasInterrupt = EX_InvalidArgument(ec, "Cannot insert this type into this type of list.")
                            value2 = value3
                          if not (hasInterrupt):
                            int1 = value[1]
                            _len = list1[1]
                            if (int1 < 0):
                              int1 += _len
                            if (int1 == _len):
                              list1[2].append(value2)
                              list1[1] += 1
                            elif ((int1 < 0) or (int1 >= _len)):
                              hasInterrupt = EX_IndexOutOfRange(ec, "Index out of range.")
                            else:
                              list1[2].insert(int1, value2)
                              list1[1] += 1
                    elif (sc_6 < 12):
                      if (sc_6 < 10):
                        if (sc_6 == 8):
                          if (argCount != 1):
                            if (argCount == 0):
                              value2 = globals[8]
                            else:
                              hasInterrupt = EX_InvalidArgument(ec, primitiveMethodWrongArgCountError("list join method", 1, argCount))
                          else:
                            value2 = funcArgs[0]
                            if (value2[0] != 5):
                              hasInterrupt = EX_InvalidArgument(ec, "Argument of list.join needs to be a string.")
                          if not (hasInterrupt):
                            stringList1 = []
                            string1 = value2[1]
                            _len = list1[1]
                            i = 0
                            while (i < _len):
                              value = list1[2][i]
                              if (value[0] != 5):
                                string2 = valueToString(vm, value)
                              else:
                                string2 = value[1]
                              stringList1.append(string2)
                              i += 1
                            output = buildString(globals, string1.join(stringList1))
                        elif (argCount != 1):
                          hasInterrupt = EX_InvalidArgument(ec, primitiveMethodWrongArgCountError("list map method", 1, argCount))
                        else:
                          value2 = funcArgs[0]
                          if (value2[0] != 9):
                            hasInterrupt = EX_InvalidArgument(ec, "list map method requires a function pointer as its argument.")
                          else:
                            primitiveMethodToCoreLibraryFallback = True
                            functionId = metadata[15][1]
                            funcArgs[1] = value
                            argCount = 2
                            output = None
                      elif (sc_6 == 10):
                        if (argCount > 0):
                          hasInterrupt = EX_InvalidArgument(ec, primitiveMethodWrongArgCountError("list pop method", 0, argCount))
                        else:
                          _len = list1[1]
                          if (_len < 1):
                            hasInterrupt = EX_IndexOutOfRange(ec, "Cannot pop from an empty list.")
                          else:
                            _len -= 1
                            value = list1[2].pop()
                            if returnValueUsed:
                              output = value
                            list1[1] = _len
                      elif (argCount != 1):
                        hasInterrupt = EX_InvalidArgument(ec, primitiveMethodWrongArgCountError("list remove method", 1, argCount))
                      else:
                        value = funcArgs[0]
                        if (value[0] != 3):
                          hasInterrupt = EX_InvalidArgument(ec, "Argument of list.remove must be an integer index.")
                        else:
                          int1 = value[1]
                          _len = list1[1]
                          if (int1 < 0):
                            int1 += _len
                          if ((int1 < 0) or (int1 >= _len)):
                            hasInterrupt = EX_IndexOutOfRange(ec, "Index out of range.")
                          else:
                            if returnValueUsed:
                              output = list1[2][int1]
                            _len = (list1[1] - 1)
                            list1[1] = _len
                            del list1[2][int1]
                    elif (sc_6 < 14):
                      if (sc_6 == 12):
                        if (argCount > 0):
                          hasInterrupt = EX_InvalidArgument(ec, primitiveMethodWrongArgCountError("list reverse method", 0, argCount))
                        else:
                          list1[2].reverse()
                      elif (argCount > 0):
                        hasInterrupt = EX_InvalidArgument(ec, primitiveMethodWrongArgCountError("list shuffle method", 0, argCount))
                      else:
                        random.shuffle(list1[2])
                    elif (sc_6 == 14):
                      if (argCount == 0):
                        sortLists(list1, list1, PST_IntBuffer16)
                        if (PST_IntBuffer16[0] > 0):
                          hasInterrupt = EX_InvalidArgument(ec, "Invalid list to sort. All items must be numbers or all strings, but not mixed.")
                      elif (argCount == 1):
                        value2 = funcArgs[0]
                        if (value2[0] == 9):
                          primitiveMethodToCoreLibraryFallback = True
                          functionId = metadata[15][2]
                          funcArgs[1] = value
                          argCount = 2
                        else:
                          hasInterrupt = EX_InvalidArgument(ec, "list.sort(get_key_function) requires a function pointer as its argument.")
                        output = None
                    else:
                      output = None
                  else:
                    # ...on a dictionary
                    dictImpl = value[1]
                    sc_7 = swlookup__interpretImpl__7.get(functionId, 8)
                    if (sc_7 < 5):
                      if (sc_7 < 3):
                        if (sc_7 == 0):
                          if (argCount > 0):
                            hasInterrupt = EX_InvalidArgument(ec, primitiveMethodWrongArgCountError("dictionary clear method", 0, argCount))
                          elif (dictImpl[0] > 0):
                            dictImpl[4] = {}
                            dictImpl[5] = {}
                            del dictImpl[6][:]
                            del dictImpl[7][:]
                            dictImpl[0] = 0
                        elif (sc_7 == 1):
                          if (argCount > 0):
                            hasInterrupt = EX_InvalidArgument(ec, primitiveMethodWrongArgCountError("dictionary clone method", 0, argCount))
                          else:
                            output = [7, cloneDictionary(dictImpl, None)]
                        elif (argCount != 1):
                          hasInterrupt = EX_InvalidArgument(ec, primitiveMethodWrongArgCountError("dictionary contains method", 1, argCount))
                        else:
                          value = funcArgs[0]
                          output = VALUE_FALSE
                          if (value[0] == 5):
                            if (value[1] in dictImpl[5]):
                              output = VALUE_TRUE
                          else:
                            if (value[0] == 3):
                              i = value[1]
                            else:
                              i = (value[1])[1]
                            if (i in dictImpl[4]):
                              output = VALUE_TRUE
                      elif (sc_7 == 3):
                        if ((argCount != 1) and (argCount != 2)):
                          hasInterrupt = EX_InvalidArgument(ec, "Dictionary get method requires 1 or 2 arguments.")
                        else:
                          value = funcArgs[0]
                          sc_8 = swlookup__interpretImpl__8.get(value[0], 3)
                          if (sc_8 < 2):
                            if (sc_8 == 0):
                              int1 = value[1]
                              i = dictImpl[4].get(int1, -1)
                            else:
                              int1 = (value[1])[1]
                              i = dictImpl[4].get(int1, -1)
                          elif (sc_8 == 2):
                            string1 = value[1]
                            i = dictImpl[5].get(string1, -1)
                          if (i == -1):
                            if (argCount == 2):
                              output = funcArgs[1]
                            else:
                              output = VALUE_NULL
                          else:
                            output = dictImpl[7][i]
                      elif (argCount > 0):
                        hasInterrupt = EX_InvalidArgument(ec, primitiveMethodWrongArgCountError("dictionary keys method", 0, argCount))
                      else:
                        valueList1 = dictImpl[6]
                        _len = len(valueList1)
                        if (dictImpl[1] == 8):
                          intArray1 = [None, None]
                          intArray1[0] = 8
                          intArray1[0] = dictImpl[2]
                        else:
                          intArray1 = [None]
                          intArray1[0] = dictImpl[1]
                        list1 = makeEmptyList(intArray1, _len)
                        i = 0
                        while (i < _len):
                          list1[2].append(valueList1[i])
                          i += 1
                        list1[1] = _len
                        output = [6, list1]
                    elif (sc_7 < 7):
                      if (sc_7 == 5):
                        if (argCount != 1):
                          hasInterrupt = EX_InvalidArgument(ec, primitiveMethodWrongArgCountError("dictionary merge method", 1, argCount))
                        else:
                          value2 = funcArgs[0]
                          if (value2[0] != 7):
                            hasInterrupt = EX_InvalidArgument(ec, "dictionary merge method requires another dictionary as a parameeter.")
                          else:
                            dictImpl2 = value2[1]
                            if (dictImpl2[0] > 0):
                              if (dictImpl[0] == 0):
                                value[1] = cloneDictionary(dictImpl2, None)
                              elif (dictImpl2[1] != dictImpl[1]):
                                hasInterrupt = EX_InvalidKey(ec, "Dictionaries with different key types cannot be merged.")
                              elif ((dictImpl2[1] == 8) and (dictImpl2[2] != dictImpl[2]) and (dictImpl[2] != 0) and not (isClassASubclassOf(vm, dictImpl2[2], dictImpl[2]))):
                                hasInterrupt = EX_InvalidKey(ec, "Dictionary key types are incompatible.")
                              else:
                                if (dictImpl[3] == None):
                                  pass
                                elif (dictImpl2[3] == None):
                                  hasInterrupt = EX_InvalidKey(ec, "Dictionaries with different value types cannot be merged.")
                                elif not (canAssignGenericToGeneric(vm, dictImpl2[3], 0, dictImpl[3], 0, intBuffer)):
                                  hasInterrupt = EX_InvalidKey(ec, "The dictionary value types are incompatible.")
                                if not (hasInterrupt):
                                  cloneDictionary(dictImpl2, dictImpl)
                            output = VALUE_NULL
                      elif (argCount != 1):
                        hasInterrupt = EX_InvalidArgument(ec, primitiveMethodWrongArgCountError("dictionary remove method", 1, argCount))
                      else:
                        value2 = funcArgs[0]
                        bool2 = False
                        keyType = dictImpl[1]
                        if ((dictImpl[0] > 0) and (keyType == value2[0])):
                          if (keyType == 5):
                            stringKey = value2[1]
                            if (stringKey in dictImpl[5]):
                              i = dictImpl[5][stringKey]
                              bool2 = True
                          else:
                            if (keyType == 3):
                              intKey = value2[1]
                            else:
                              intKey = (value2[1])[1]
                            if (intKey in dictImpl[4]):
                              i = dictImpl[4][intKey]
                              bool2 = True
                          if bool2:
                            _len = (dictImpl[0] - 1)
                            dictImpl[0] = _len
                            if (i == _len):
                              if (keyType == 5):
                                dictImpl[5].pop(stringKey)
                              else:
                                dictImpl[4].pop(intKey)
                              del dictImpl[6][i]
                              del dictImpl[7][i]
                            else:
                              value = dictImpl[6][_len]
                              dictImpl[6][i] = value
                              dictImpl[7][i] = dictImpl[7][_len]
                              dictImpl[6].pop()
                              dictImpl[7].pop()
                              if (keyType == 5):
                                dictImpl[5].pop(stringKey)
                                stringKey = value[1]
                                dictImpl[5][stringKey] = i
                              else:
                                dictImpl[4].pop(intKey)
                                if (keyType == 3):
                                  intKey = value[1]
                                else:
                                  intKey = (value[1])[1]
                                dictImpl[4][intKey] = i
                        if not (bool2):
                          hasInterrupt = EX_KeyNotFound(ec, "dictionary does not contain the given key.")
                    elif (sc_7 == 7):
                      if (argCount > 0):
                        hasInterrupt = EX_InvalidArgument(ec, primitiveMethodWrongArgCountError("dictionary values method", 0, argCount))
                      else:
                        valueList1 = dictImpl[7]
                        _len = len(valueList1)
                        list1 = makeEmptyList(dictImpl[3], _len)
                        i = 0
                        while (i < _len):
                          addToList(list1, valueList1[i])
                          i += 1
                        output = [6, list1]
                    else:
                      output = None
                elif (sc_4 == 3):
                  # ...on a function pointer
                  functionPointer1 = value[1]
                  sc_9 = swlookup__interpretImpl__9.get(functionId, 4)
                  if (sc_9 < 3):
                    if (sc_9 == 0):
                      if (argCount > 0):
                        hasInterrupt = EX_InvalidArgument(ec, primitiveMethodWrongArgCountError("argCountMax method", 0, argCount))
                      else:
                        functionId = functionPointer1[3]
                        functionInfo = metadata[10][functionId]
                        output = buildInteger(globals, functionInfo[4])
                    elif (sc_9 == 1):
                      if (argCount > 0):
                        hasInterrupt = EX_InvalidArgument(ec, primitiveMethodWrongArgCountError("argCountMin method", 0, argCount))
                      else:
                        functionId = functionPointer1[3]
                        functionInfo = metadata[10][functionId]
                        output = buildInteger(globals, functionInfo[3])
                    else:
                      functionInfo = metadata[10][functionPointer1[3]]
                      output = buildString(globals, functionInfo[9])
                  elif (sc_9 == 3):
                    if (argCount == 1):
                      funcArgs[1] = funcArgs[0]
                    elif (argCount == 0):
                      funcArgs[1] = VALUE_NULL
                    else:
                      hasInterrupt = EX_InvalidArgument(ec, "invoke requires a list of arguments.")
                    funcArgs[0] = value
                    argCount = 2
                    primitiveMethodToCoreLibraryFallback = True
                    functionId = metadata[15][3]
                    output = None
                  else:
                    output = None
                elif (sc_4 == 4):
                  # ...on a class definition
                  classValue = value[1]
                  sc_10 = swlookup__interpretImpl__10.get(functionId, 2)
                  if (sc_10 == 0):
                    classInfo = metadata[9][classValue[1]]
                    output = buildString(globals, classInfo[16])
                  elif (sc_10 == 1):
                    if (argCount != 1):
                      hasInterrupt = EX_InvalidArgument(ec, primitiveMethodWrongArgCountError("class isA method", 1, argCount))
                    else:
                      int1 = classValue[1]
                      value = funcArgs[0]
                      if (value[0] != 10):
                        hasInterrupt = EX_InvalidArgument(ec, "class isA method requires another class reference.")
                      else:
                        classValue = value[1]
                        int2 = classValue[1]
                        output = VALUE_FALSE
                        if isClassASubclassOf(vm, int1, int2):
                          output = VALUE_TRUE
                  else:
                    output = None
                if not (hasInterrupt):
                  if (output == None):
                    if primitiveMethodToCoreLibraryFallback:
                      type = 1
                      bool1 = True
                    else:
                      hasInterrupt = EX_InvalidInvocation(ec, "primitive method not found.")
                  else:
                    if returnValueUsed:
                      if (valueStackSize == valueStackCapacity):
                        valueStack = valueStackIncreaseCapacity(ec)
                        valueStackCapacity = len(valueStack)
                      valueStack[valueStackSize] = output
                      valueStackSize += 1
                    bool1 = False
              if (bool1 and not (hasInterrupt)):
                # push a new frame to the stack
                stack[0] = pc
                bool1 = False
                sc_11 = swlookup__interpretImpl__11.get(type, 6)
                if (sc_11 < 4):
                  if (sc_11 < 2):
                    if (sc_11 == 0):
                      # function
                      functionInfo = functionTable[functionId]
                      pc = functionInfo[2]
                      value = None
                      classId = 0
                    else:
                      # lambda
                      pc = functionId
                      functionInfo = metadata[11][functionId]
                      value = None
                      classId = 0
                  elif (sc_11 == 2):
                    # static method
                    functionInfo = functionTable[functionId]
                    pc = functionInfo[2]
                    value = None
                    classId = 0
                  else:
                    # non-static method
                    functionInfo = functionTable[functionId]
                    pc = functionInfo[2]
                    classId = 0
                elif (sc_11 == 4):
                  # constructor
                  vm[5] += 1
                  classInfo = classTable[classId]
                  valueArray1 = (PST_NoneListOfOne * classInfo[8])
                  i = (len(valueArray1) - 1)
                  while (i >= 0):
                    sc_12 = swlookup__interpretImpl__12.get(classInfo[10][i], 3)
                    if (sc_12 < 2):
                      if (sc_12 == 0):
                        valueArray1[i] = classInfo[11][i]
                    elif (sc_12 == 2):
                      pass
                    i -= 1
                  objInstance1 = [classId, vm[5], valueArray1, None, None]
                  value = [8, objInstance1]
                  functionId = classInfo[7]
                  functionInfo = functionTable[functionId]
                  pc = functionInfo[2]
                  classId = 0
                  if returnValueUsed:
                    returnValueUsed = False
                    if (valueStackSize == valueStackCapacity):
                      valueStack = valueStackIncreaseCapacity(ec)
                      valueStackCapacity = len(valueStack)
                    valueStack[valueStackSize] = value
                    valueStackSize += 1
                elif (sc_11 == 5):
                  # base constructor
                  value = stack[6]
                  classInfo = classTable[classId]
                  functionId = classInfo[7]
                  functionInfo = functionTable[functionId]
                  pc = functionInfo[2]
                  classId = 0
                if ((argCount < functionInfo[3]) or (argCount > functionInfo[4])):
                  pc = stack[0]
                  hasInterrupt = EX_InvalidArgument(ec, "Incorrect number of args were passed to this function.")
                else:
                  int1 = functionInfo[7]
                  int2 = stack[3]
                  if (localsStackCapacity <= (int2 + int1)):
                    increaseLocalsStackCapacity(ec, int1)
                    localsStack = ec[5]
                    localsStackSet = ec[6]
                    localsStackCapacity = len(localsStack)
                  localsStackSetToken = (ec[7] + 1)
                  ec[7] = localsStackSetToken
                  if (localsStackSetToken > 2000000000):
                    resetLocalsStackTokens(ec, stack)
                    localsStackSetToken = 2
                  localsStackOffset = int2
                  if (type == 10):
                    value = closure[-1][0]
                  else:
                    closure = None
                  # invoke the function
                  stack = [pc, localsStackSetToken, localsStackOffset, (localsStackOffset + int1), stack, returnValueUsed, value, valueStackSize, 0, (stack[9] + 1), 0, None, closure, None]
                  i = 0
                  while (i < argCount):
                    int1 = (localsStackOffset + i)
                    localsStack[int1] = funcArgs[i]
                    localsStackSet[int1] = localsStackSetToken
                    i += 1
                  if (argCount != functionInfo[3]):
                    int1 = (argCount - functionInfo[3])
                    if (int1 > 0):
                      pc += functionInfo[8][int1]
                      stack[0] = pc
                  if (stack[9] > 1000):
                    hasInterrupt = EX_Fatal(ec, "Stack overflow.")
        elif (sc_0 < 15):
          if (sc_0 == 13):
            # CAST
            value = valueStack[(valueStackSize - 1)]
            value2 = canAssignTypeToGeneric(vm, value, row, 0)
            if (value2 == None):
              if ((value[0] == 4) and (row[0] == 3)):
                if (row[1] == 1):
                  float1 = value[1]
                  if ((float1 < 0) and ((float1 % 1) != 0)):
                    i = (int(float1) - 1)
                  else:
                    i = int(float1)
                  if (i < 0):
                    if (i > -257):
                      value2 = globals[10][-i]
                    else:
                      value2 = [3, i]
                  elif (i < 2049):
                    value2 = globals[9][i]
                  else:
                    value2 = [3, i]
              elif ((value[0] == 3) and (row[0] == 4)):
                int1 = value[1]
                if (int1 == 0):
                  value2 = VALUE_FLOAT_ZERO
                else:
                  value2 = [4, (0.0 + int1)]
              if (value2 != None):
                valueStack[(valueStackSize - 1)] = value2
            if (value2 == None):
              hasInterrupt = EX_InvalidArgument(ec, ''.join(["Cannot convert a ", typeToStringFromValue(vm, value), " to a ", typeToString(vm, row, 0)]))
            else:
              valueStack[(valueStackSize - 1)] = value2
          else:
            # CLASS_DEFINITION
            initializeClass(pc, vm, row, stringArgs[pc])
            classTable = metadata[9]
        elif (sc_0 == 15):
          # CNI_INVOKE
          nativeFp = metadata[13][row[0]]
          if (nativeFp == None):
            hasInterrupt = EX_InvalidInvocation(ec, "CNI method could not be found.")
          else:
            _len = row[1]
            valueStackSize -= _len
            valueArray1 = (PST_NoneListOfOne * _len)
            i = 0
            while (i < _len):
              valueArray1[i] = valueStack[(valueStackSize + i)]
              i += 1
            prepareToSuspend(ec, stack, valueStackSize, pc)
            value = nativeFp(vm, valueArray1)
            if ec[11]:
              ec[11] = False
              if (ec[12] == 1):
                return suspendInterpreter()
            if (row[2] == 1):
              if (valueStackSize == valueStackCapacity):
                valueStack = valueStackIncreaseCapacity(ec)
                valueStackCapacity = len(valueStack)
              valueStack[valueStackSize] = value
              valueStackSize += 1
        else:
          # CNI_REGISTER
          nativeFp = TranslationHelper_getFunction(stringArgs[pc])
          metadata[13][row[0]] = nativeFp
      elif (sc_0 < 26):
        if (sc_0 < 22):
          if (sc_0 < 20):
            if (sc_0 == 17):
              # COMMAND_LINE_ARGS
              if (valueStackSize == valueStackCapacity):
                valueStack = valueStackIncreaseCapacity(ec)
                valueStackCapacity = len(valueStack)
              list1 = makeEmptyList(globals[14], 3)
              i = 0
              while (i < len(vm[11][0])):
                addToList(list1, buildString(globals, vm[11][0][i]))
                i += 1
              valueStack[valueStackSize] = [6, list1]
              valueStackSize += 1
            elif (sc_0 == 18):
              # CONTINUE
              if (row[0] == 1):
                pc += row[1]
              else:
                intArray1 = esfData[pc]
                pc = (intArray1[1] - 1)
                valueStackSize = stack[7]
                stack[10] = 2
            else:
              # CORE_FUNCTION
              sc_13 = swlookup__interpretImpl__13.get(row[0], 41)
              if (sc_13 < 21):
                if (sc_13 < 11):
                  if (sc_13 < 6):
                    if (sc_13 < 3):
                      if (sc_13 == 0):
                        # parseInt
                        valueStackSize -= 1
                        arg1 = valueStack[valueStackSize]
                        output = VALUE_NULL
                        if (arg1[0] == 5):
                          string1 = (arg1[1]).strip()
                          if PST_isValidInteger(string1):
                            output = buildInteger(globals, int(string1))
                        else:
                          hasInterrupt = EX_InvalidArgument(ec, "parseInt requires a string argument.")
                      elif (sc_13 == 1):
                        # parseFloat
                        valueStackSize -= 1
                        arg1 = valueStack[valueStackSize]
                        output = VALUE_NULL
                        if (arg1[0] == 5):
                          string1 = (arg1[1]).strip()
                          PST_tryParseFloat(string1, floatList1)
                          if (floatList1[0] >= 0):
                            output = buildFloat(globals, floatList1[1])
                        else:
                          hasInterrupt = EX_InvalidArgument(ec, "parseFloat requires a string argument.")
                      else:
                        # print
                        valueStackSize -= 1
                        arg1 = valueStack[valueStackSize]
                        output = VALUE_NULL
                        printToStdOut(vm[11][2], valueToString(vm, arg1))
                    elif (sc_13 == 3):
                      # typeof
                      valueStackSize -= 1
                      arg1 = valueStack[valueStackSize]
                      output = buildInteger(globals, (arg1[0] - 1))
                    elif (sc_13 == 4):
                      # typeis
                      valueStackSize -= 1
                      arg1 = valueStack[valueStackSize]
                      int1 = arg1[0]
                      int2 = row[2]
                      output = VALUE_FALSE
                      while (int2 > 0):
                        if (row[(2 + int2)] == int1):
                          output = VALUE_TRUE
                          int2 = 0
                        else:
                          int2 -= 1
                    else:
                      # execId
                      output = buildInteger(globals, ec[0])
                  elif (sc_13 < 9):
                    if (sc_13 == 6):
                      # assert
                      valueStackSize -= 3
                      arg3 = valueStack[(valueStackSize + 2)]
                      arg2 = valueStack[(valueStackSize + 1)]
                      arg1 = valueStack[valueStackSize]
                      if (arg1[0] != 2):
                        hasInterrupt = EX_InvalidArgument(ec, "Assertion expression must be a boolean.")
                      elif arg1[1]:
                        output = VALUE_NULL
                      else:
                        string1 = valueToString(vm, arg2)
                        if arg3[1]:
                          string1 = "Assertion failed: " + string1
                        hasInterrupt = EX_AssertionFailed(ec, string1)
                    elif (sc_13 == 7):
                      # chr
                      valueStackSize -= 1
                      arg1 = valueStack[valueStackSize]
                      output = None
                      if (arg1[0] == 3):
                        int1 = arg1[1]
                        if ((int1 >= 0) and (int1 < 256)):
                          output = buildCommonString(globals, chr(int1))
                      if (output == None):
                        hasInterrupt = EX_InvalidArgument(ec, "chr requires an integer between 0 and 255.")
                    else:
                      # ord
                      valueStackSize -= 1
                      arg1 = valueStack[valueStackSize]
                      output = None
                      if (arg1[0] == 5):
                        string1 = arg1[1]
                        if (len(string1) == 1):
                          output = buildInteger(globals, ord(string1[0]))
                      if (output == None):
                        hasInterrupt = EX_InvalidArgument(ec, "ord requires a 1 character string.")
                  elif (sc_13 == 9):
                    # currentTime
                    output = buildFloat(globals, time.time())
                  else:
                    # sortList
                    valueStackSize -= 2
                    arg2 = valueStack[(valueStackSize + 1)]
                    arg1 = valueStack[valueStackSize]
                    output = VALUE_NULL
                    list1 = arg1[1]
                    list2 = arg2[1]
                    sortLists(list2, list1, PST_IntBuffer16)
                    if (PST_IntBuffer16[0] > 0):
                      hasInterrupt = EX_InvalidArgument(ec, "Invalid sort keys. Keys must be all numbers or all strings, but not mixed.")
                elif (sc_13 < 16):
                  if (sc_13 < 14):
                    if (sc_13 == 11):
                      # abs
                      valueStackSize -= 1
                      arg1 = valueStack[valueStackSize]
                      output = arg1
                      if (arg1[0] == 3):
                        if (arg1[1] < 0):
                          output = buildInteger(globals, -(arg1[1]))
                      elif (arg1[0] == 4):
                        if (arg1[1] < 0):
                          output = buildFloat(globals, -(arg1[1]))
                      else:
                        hasInterrupt = EX_InvalidArgument(ec, "abs requires a number as input.")
                    elif (sc_13 == 12):
                      # arcCos
                      valueStackSize -= 1
                      arg1 = valueStack[valueStackSize]
                      if (arg1[0] == 4):
                        float1 = arg1[1]
                      elif (arg1[0] == 3):
                        float1 = (0.0 + arg1[1])
                      else:
                        hasInterrupt = EX_InvalidArgument(ec, "arccos requires a number as input.")
                      if not (hasInterrupt):
                        if ((float1 < -1) or (float1 > 1)):
                          hasInterrupt = EX_InvalidArgument(ec, "arccos requires a number in the range of -1 to 1.")
                        else:
                          output = buildFloat(globals, math.acos(float1))
                    else:
                      # arcSin
                      valueStackSize -= 1
                      arg1 = valueStack[valueStackSize]
                      if (arg1[0] == 4):
                        float1 = arg1[1]
                      elif (arg1[0] == 3):
                        float1 = (0.0 + arg1[1])
                      else:
                        hasInterrupt = EX_InvalidArgument(ec, "arcsin requires a number as input.")
                      if not (hasInterrupt):
                        if ((float1 < -1) or (float1 > 1)):
                          hasInterrupt = EX_InvalidArgument(ec, "arcsin requires a number in the range of -1 to 1.")
                        else:
                          output = buildFloat(globals, math.asin(float1))
                  elif (sc_13 == 14):
                    # arcTan
                    valueStackSize -= 2
                    arg2 = valueStack[(valueStackSize + 1)]
                    arg1 = valueStack[valueStackSize]
                    bool1 = False
                    if (arg1[0] == 4):
                      float1 = arg1[1]
                    elif (arg1[0] == 3):
                      float1 = (0.0 + arg1[1])
                    else:
                      bool1 = True
                    if (arg2[0] == 4):
                      float2 = arg2[1]
                    elif (arg2[0] == 3):
                      float2 = (0.0 + arg2[1])
                    else:
                      bool1 = True
                    if bool1:
                      hasInterrupt = EX_InvalidArgument(ec, "arctan requires numeric arguments.")
                    else:
                      output = buildFloat(globals, math.atan2(float1, float2))
                  else:
                    # cos
                    valueStackSize -= 1
                    arg1 = valueStack[valueStackSize]
                    if (arg1[0] == 4):
                      float1 = arg1[1]
                      output = buildFloat(globals, math.cos(float1))
                    elif (arg1[0] == 3):
                      int1 = arg1[1]
                      output = buildFloat(globals, math.cos(int1))
                    else:
                      hasInterrupt = EX_InvalidArgument(ec, "cos requires a number argument.")
                elif (sc_13 < 19):
                  if (sc_13 == 16):
                    # ensureRange
                    valueStackSize -= 3
                    arg3 = valueStack[(valueStackSize + 2)]
                    arg2 = valueStack[(valueStackSize + 1)]
                    arg1 = valueStack[valueStackSize]
                    bool1 = False
                    if (arg2[0] == 4):
                      float2 = arg2[1]
                    elif (arg2[0] == 3):
                      float2 = (0.0 + arg2[1])
                    else:
                      bool1 = True
                    if (arg3[0] == 4):
                      float3 = arg3[1]
                    elif (arg3[0] == 3):
                      float3 = (0.0 + arg3[1])
                    else:
                      bool1 = True
                    if (not (bool1) and (float3 < float2)):
                      float1 = float3
                      float3 = float2
                      float2 = float1
                      value = arg2
                      arg2 = arg3
                      arg3 = value
                    if (arg1[0] == 4):
                      float1 = arg1[1]
                    elif (arg1[0] == 3):
                      float1 = (0.0 + arg1[1])
                    else:
                      bool1 = True
                    if bool1:
                      hasInterrupt = EX_InvalidArgument(ec, "ensureRange requires numeric arguments.")
                    elif (float1 < float2):
                      output = arg2
                    elif (float1 > float3):
                      output = arg3
                    else:
                      output = arg1
                  elif (sc_13 == 17):
                    # floor
                    valueStackSize -= 1
                    arg1 = valueStack[valueStackSize]
                    if (arg1[0] == 4):
                      float1 = arg1[1]
                      if ((float1 < 0) and ((float1 % 1) != 0)):
                        int1 = (int(float1) - 1)
                      else:
                        int1 = int(float1)
                      if (int1 < 2049):
                        if (int1 >= 0):
                          output = INTEGER_POSITIVE_CACHE[int1]
                        elif (int1 > -257):
                          output = INTEGER_NEGATIVE_CACHE[-int1]
                        else:
                          output = [3, int1]
                      else:
                        output = [3, int1]
                    elif (arg1[0] == 3):
                      output = arg1
                    else:
                      hasInterrupt = EX_InvalidArgument(ec, "floor expects a numeric argument.")
                  else:
                    # max
                    valueStackSize -= 2
                    arg2 = valueStack[(valueStackSize + 1)]
                    arg1 = valueStack[valueStackSize]
                    bool1 = False
                    if (arg1[0] == 4):
                      float1 = arg1[1]
                    elif (arg1[0] == 3):
                      float1 = (0.0 + arg1[1])
                    else:
                      bool1 = True
                    if (arg2[0] == 4):
                      float2 = arg2[1]
                    elif (arg2[0] == 3):
                      float2 = (0.0 + arg2[1])
                    else:
                      bool1 = True
                    if bool1:
                      hasInterrupt = EX_InvalidArgument(ec, "max requires numeric arguments.")
                    elif (float1 >= float2):
                      output = arg1
                    else:
                      output = arg2
                elif (sc_13 == 19):
                  # min
                  valueStackSize -= 2
                  arg2 = valueStack[(valueStackSize + 1)]
                  arg1 = valueStack[valueStackSize]
                  bool1 = False
                  if (arg1[0] == 4):
                    float1 = arg1[1]
                  elif (arg1[0] == 3):
                    float1 = (0.0 + arg1[1])
                  else:
                    bool1 = True
                  if (arg2[0] == 4):
                    float2 = arg2[1]
                  elif (arg2[0] == 3):
                    float2 = (0.0 + arg2[1])
                  else:
                    bool1 = True
                  if bool1:
                    hasInterrupt = EX_InvalidArgument(ec, "min requires numeric arguments.")
                  elif (float1 <= float2):
                    output = arg1
                  else:
                    output = arg2
                else:
                  # nativeInt
                  valueStackSize -= 2
                  arg2 = valueStack[(valueStackSize + 1)]
                  arg1 = valueStack[valueStackSize]
                  output = buildInteger(globals, (arg1[1])[3][arg2[1]])
              elif (sc_13 < 32):
                if (sc_13 < 27):
                  if (sc_13 < 24):
                    if (sc_13 == 21):
                      # nativeString
                      valueStackSize -= 3
                      arg3 = valueStack[(valueStackSize + 2)]
                      arg2 = valueStack[(valueStackSize + 1)]
                      arg1 = valueStack[valueStackSize]
                      string1 = (arg1[1])[3][arg2[1]]
                      if arg3[1]:
                        output = buildCommonString(globals, string1)
                      else:
                        output = buildString(globals, string1)
                    elif (sc_13 == 22):
                      # sign
                      valueStackSize -= 1
                      arg1 = valueStack[valueStackSize]
                      if (arg1[0] == 3):
                        float1 = (0.0 + (arg1[1]))
                      elif (arg1[0] == 4):
                        float1 = arg1[1]
                      else:
                        hasInterrupt = EX_InvalidArgument(ec, "sign requires a number as input.")
                      if (float1 == 0):
                        output = VALUE_INT_ZERO
                      elif (float1 > 0):
                        output = VALUE_INT_ONE
                      else:
                        output = INTEGER_NEGATIVE_CACHE[1]
                    else:
                      # sin
                      valueStackSize -= 1
                      arg1 = valueStack[valueStackSize]
                      if (arg1[0] == 4):
                        float1 = arg1[1]
                      elif (arg1[0] == 3):
                        float1 = (0.0 + arg1[1])
                      else:
                        hasInterrupt = EX_InvalidArgument(ec, "sin requires a number argument.")
                      output = buildFloat(globals, math.sin(float1))
                  elif (sc_13 == 24):
                    # tan
                    valueStackSize -= 1
                    arg1 = valueStack[valueStackSize]
                    if (arg1[0] == 4):
                      float1 = arg1[1]
                    elif (arg1[0] == 3):
                      float1 = (0.0 + arg1[1])
                    else:
                      hasInterrupt = EX_InvalidArgument(ec, "tan requires a number argument.")
                    if not (hasInterrupt):
                      float2 = math.cos(float1)
                      if (float2 < 0):
                        float2 = -float2
                      if (float2 < 0.00000000015):
                        hasInterrupt = EX_DivisionByZero(ec, "Tangent is undefined.")
                      else:
                        output = buildFloat(globals, math.tan(float1))
                  elif (sc_13 == 25):
                    # log
                    valueStackSize -= 2
                    arg2 = valueStack[(valueStackSize + 1)]
                    arg1 = valueStack[valueStackSize]
                    if (arg1[0] == 4):
                      float1 = arg1[1]
                    elif (arg1[0] == 3):
                      float1 = (0.0 + arg1[1])
                    else:
                      hasInterrupt = EX_InvalidArgument(ec, "logarithms require a number argument.")
                    if not (hasInterrupt):
                      if (float1 <= 0):
                        hasInterrupt = EX_InvalidArgument(ec, "logarithms require positive inputs.")
                      else:
                        output = buildFloat(globals, fixFuzzyFloatPrecision((math.log(float1) * arg2[1])))
                  else:
                    # intQueueClear
                    valueStackSize -= 1
                    arg1 = valueStack[valueStackSize]
                    output = VALUE_NULL
                    objInstance1 = arg1[1]
                    if (objInstance1[3] != None):
                      objInstance1[3][1] = 0
                elif (sc_13 < 30):
                  if (sc_13 == 27):
                    # intQueueWrite16
                    output = VALUE_NULL
                    int1 = row[2]
                    valueStackSize -= (int1 + 1)
                    value = valueStack[valueStackSize]
                    objArray1 = (value[1])[3]
                    intArray1 = objArray1[0]
                    _len = objArray1[1]
                    if (_len >= len(intArray1)):
                      intArray2 = (PST_NoneListOfOne * ((_len * 2) + 16))
                      j = 0
                      while (j < _len):
                        intArray2[j] = intArray1[j]
                        j += 1
                      intArray1 = intArray2
                      objArray1[0] = intArray1
                    objArray1[1] = (_len + 16)
                    i = (int1 - 1)
                    while (i >= 0):
                      value = valueStack[((valueStackSize + 1) + i)]
                      if (value[0] == 3):
                        intArray1[(_len + i)] = value[1]
                      elif (value[0] == 4):
                        float1 = (0.5 + value[1])
                        intArray1[(_len + i)] = int(float1)
                      else:
                        hasInterrupt = EX_InvalidArgument(ec, "Input must be integers.")
                        i = -1
                      i -= 1
                  elif (sc_13 == 28):
                    # execCounter
                    output = buildInteger(globals, ec[8])
                  else:
                    # sleep
                    valueStackSize -= 1
                    arg1 = valueStack[valueStackSize]
                    float1 = getFloat(arg1)
                    if (row[1] == 1):
                      if (valueStackSize == valueStackCapacity):
                        valueStack = valueStackIncreaseCapacity(ec)
                        valueStackCapacity = len(valueStack)
                      valueStack[valueStackSize] = VALUE_NULL
                      valueStackSize += 1
                    prepareToSuspend(ec, stack, valueStackSize, pc)
                    ec[13] = [3, 0, "", float1, None]
                    hasInterrupt = True
                elif (sc_13 == 30):
                  # projectId
                  output = buildCommonString(globals, metadata[17])
                else:
                  # isJavaScript
                  output = VALUE_FALSE
              elif (sc_13 < 37):
                if (sc_13 < 35):
                  if (sc_13 == 32):
                    # isAndroid
                    output = VALUE_FALSE
                  elif (sc_13 == 33):
                    # allocNativeData
                    valueStackSize -= 2
                    arg2 = valueStack[(valueStackSize + 1)]
                    arg1 = valueStack[valueStackSize]
                    objInstance1 = arg1[1]
                    int1 = arg2[1]
                    objArray1 = (PST_NoneListOfOne * int1)
                    objInstance1[3] = objArray1
                  else:
                    # setNativeData
                    valueStackSize -= 3
                    arg3 = valueStack[(valueStackSize + 2)]
                    arg2 = valueStack[(valueStackSize + 1)]
                    arg1 = valueStack[valueStackSize]
                    (arg1[1])[3][arg2[1]] = arg3[1]
                elif (sc_13 == 35):
                  # getExceptionTrace
                  valueStackSize -= 1
                  arg1 = valueStack[valueStackSize]
                  intList1 = getNativeDataItem(arg1, 1)
                  list1 = makeEmptyList(globals[14], 20)
                  output = [6, list1]
                  if (intList1 != None):
                    stringList1 = tokenHelperConvertPcsToStackTraceStrings(vm, intList1)
                    i = 0
                    while (i < len(stringList1)):
                      addToList(list1, buildString(globals, stringList1[i]))
                      i += 1
                    reverseList(list1)
                else:
                  # reflectAllClasses
                  output = Reflect_allClasses(vm)
              elif (sc_13 < 40):
                if (sc_13 == 37):
                  # reflectGetMethods
                  valueStackSize -= 1
                  arg1 = valueStack[valueStackSize]
                  output = Reflect_getMethods(vm, ec, arg1)
                  hasInterrupt = (ec[13] != None)
                elif (sc_13 == 38):
                  # reflectGetClass
                  valueStackSize -= 1
                  arg1 = valueStack[valueStackSize]
                  if (arg1[0] != 8):
                    hasInterrupt = EX_InvalidArgument(ec, "Cannot get class from non-instance types.")
                  else:
                    objInstance1 = arg1[1]
                    output = [10, [False, objInstance1[0]]]
                else:
                  # convertFloatArgsToInts
                  int1 = stack[3]
                  i = localsStackOffset
                  while (i < int1):
                    value = localsStack[i]
                    if (localsStackSet[i] != localsStackSetToken):
                      i += int1
                    elif (value[0] == 4):
                      float1 = value[1]
                      if ((float1 < 0) and ((float1 % 1) != 0)):
                        int2 = (int(float1) - 1)
                      else:
                        int2 = int(float1)
                      if ((int2 >= 0) and (int2 < 2049)):
                        localsStack[i] = INTEGER_POSITIVE_CACHE[int2]
                      else:
                        localsStack[i] = buildInteger(globals, int2)
                    i += 1
              elif (sc_13 == 40):
                # addShutdownHandler
                valueStackSize -= 1
                arg1 = valueStack[valueStackSize]
                vm[10].append(arg1)
              if (row[1] == 1):
                if (valueStackSize == valueStackCapacity):
                  valueStack = valueStackIncreaseCapacity(ec)
                  valueStackCapacity = len(valueStack)
                valueStack[valueStackSize] = output
                valueStackSize += 1
          elif (sc_0 == 20):
            # DEBUG_SYMBOLS
            applyDebugSymbolData(vm, row, stringArgs[pc], metadata[22])
          else:
            # DEF_DICT
            intIntDict1 = {}
            stringIntDict1 = {}
            valueList2 = []
            valueList1 = []
            _len = row[0]
            type = 3
            first = True
            i = _len
            while (i > 0):
              valueStackSize -= 2
              value = valueStack[(valueStackSize + 1)]
              value2 = valueStack[valueStackSize]
              if first:
                type = value2[0]
                first = False
              elif (type != value2[0]):
                hasInterrupt = EX_InvalidKey(ec, "Dictionary keys must be of the same type.")
              if not (hasInterrupt):
                if (type == 3):
                  intKey = value2[1]
                elif (type == 5):
                  stringKey = value2[1]
                elif (type == 8):
                  objInstance1 = value2[1]
                  intKey = objInstance1[1]
                else:
                  hasInterrupt = EX_InvalidKey(ec, "Only integers, strings, and objects can be used as dictionary keys.")
              if not (hasInterrupt):
                if (type == 5):
                  stringIntDict1[stringKey] = len(valueList1)
                else:
                  intIntDict1[intKey] = len(valueList1)
                valueList2.append(value2)
                valueList1.append(value)
                i -= 1
            if not (hasInterrupt):
              if (type == 5):
                i = len(stringIntDict1)
              else:
                i = len(intIntDict1)
              if (i != _len):
                hasInterrupt = EX_InvalidKey(ec, "Key collision")
            if not (hasInterrupt):
              i = row[1]
              classId = 0
              if (i > 0):
                type = row[2]
                if (type == 8):
                  classId = row[3]
                int1 = len(row)
                intArray1 = (PST_NoneListOfOne * (int1 - i))
                while (i < int1):
                  intArray1[(i - row[1])] = row[i]
                  i += 1
              else:
                intArray1 = None
              if (valueStackSize == valueStackCapacity):
                valueStack = valueStackIncreaseCapacity(ec)
                valueStackCapacity = len(valueStack)
              valueStack[valueStackSize] = [7, [_len, type, classId, intArray1, intIntDict1, stringIntDict1, valueList2, valueList1]]
              valueStackSize += 1
        elif (sc_0 < 24):
          if (sc_0 == 22):
            # DEF_LIST
            int1 = row[0]
            list1 = makeEmptyList(None, int1)
            if (row[1] != 0):
              list1[0] = (PST_NoneListOfOne * (len(row) - 1))
              i = 1
              while (i < len(row)):
                list1[0][(i - 1)] = row[i]
                i += 1
            list1[1] = int1
            while (int1 > 0):
              valueStackSize -= 1
              list1[2].append(valueStack[valueStackSize])
              int1 -= 1
            list1[2].reverse()
            value = [6, list1]
            if (valueStackSize == valueStackCapacity):
              valueStack = valueStackIncreaseCapacity(ec)
              valueStackCapacity = len(valueStack)
            valueStack[valueStackSize] = value
            valueStackSize += 1
          else:
            # DEF_ORIGINAL_CODE
            defOriginalCodeImpl(vm, row, stringArgs[pc])
        elif (sc_0 == 24):
          # DEREF_CLOSURE
          bool1 = True
          closure = stack[12]
          i = row[0]
          if ((closure != None) and (i in closure)):
            value = closure[i][0]
            if (value != None):
              bool1 = False
              if (valueStackSize == valueStackCapacity):
                valueStack = valueStackIncreaseCapacity(ec)
                valueStackCapacity = len(valueStack)
              valueStack[valueStackSize] = value
              valueStackSize += 1
          if bool1:
            hasInterrupt = EX_UnassignedVariable(ec, "Variable used before it was set.")
        else:
          # DEREF_DOT
          value = valueStack[(valueStackSize - 1)]
          nameId = row[0]
          int2 = row[1]
          sc_14 = swlookup__interpretImpl__14.get(value[0], 4)
          if (sc_14 < 3):
            if (sc_14 == 0):
              objInstance1 = value[1]
              classId = objInstance1[0]
              classInfo = classTable[classId]
              if (classId == row[4]):
                int1 = row[5]
              else:
                intIntDict1 = classInfo[14]
                int1 = intIntDict1.get(int2, -1)
                int3 = classInfo[12][int1]
                if (int3 > 1):
                  if (int3 == 2):
                    if (classId != row[2]):
                      int1 = -2
                  else:
                    if ((int3 == 3) or (int3 == 5)):
                      if (classInfo[3] != row[3]):
                        int1 = -3
                    if ((int3 == 4) or (int3 == 5)):
                      i = row[2]
                      if (classId == i):
                        pass
                      else:
                        classInfo = classTable[classInfo[0]]
                        while ((classInfo[2] != -1) and (int1 < len(classTable[classInfo[2]][12]))):
                          classInfo = classTable[classInfo[2]]
                        j = classInfo[0]
                        if (j != i):
                          bool1 = False
                          while ((i != -1) and (classTable[i][2] != -1)):
                            i = classTable[i][2]
                            if (i == j):
                              bool1 = True
                              i = -1
                          if not (bool1):
                            int1 = -4
                      classInfo = classTable[classId]
                row[4] = objInstance1[0]
                row[5] = int1
              if (int1 > -1):
                functionId = classInfo[9][int1]
                if (functionId == -1):
                  output = objInstance1[2][int1]
                else:
                  output = [9, [2, value, objInstance1[0], functionId, None]]
              else:
                output = None
            elif (sc_14 == 1):
              if (metadata[14] == nameId):
                output = buildInteger(globals, len((value[1])))
              else:
                output = None
            elif (metadata[14] == nameId):
              output = buildInteger(globals, (value[1])[1])
            else:
              output = None
          elif (sc_14 == 3):
            if (metadata[14] == nameId):
              output = buildInteger(globals, (value[1])[0])
            else:
              output = None
          elif (value[0] == 1):
            hasInterrupt = EX_NullReference(ec, "Derferenced a field from null.")
            output = VALUE_NULL
          else:
            output = None
          if (output == None):
            output = generatePrimitiveMethodReference(globalNameIdToPrimitiveMethodName, nameId, value)
            if (output == None):
              if (value[0] == 1):
                hasInterrupt = EX_NullReference(ec, "Tried to dereference a field on null.")
              elif ((value[0] == 8) and (int1 < -1)):
                string1 = identifiers[row[0]]
                if (int1 == -2):
                  string2 = "private"
                elif (int1 == -3):
                  string2 = "internal"
                else:
                  string2 = "protected"
                hasInterrupt = EX_UnknownField(ec, ''.join(["The field '", string1, "' is marked as ", string2, " and cannot be accessed from here."]))
              else:
                if (value[0] == 8):
                  classId = (value[1])[0]
                  classInfo = classTable[classId]
                  string1 = classInfo[16] + " instance"
                else:
                  string1 = getTypeFromId(value[0])
                hasInterrupt = EX_UnknownField(ec, string1 + " does not have that field.")
          valueStack[(valueStackSize - 1)] = output
      elif (sc_0 < 30):
        if (sc_0 < 28):
          if (sc_0 == 26):
            # DEREF_INSTANCE_FIELD
            objInstance1 = stack[6][1]
            value = objInstance1[2][row[0]]
            if (valueStackSize == valueStackCapacity):
              valueStack = valueStackIncreaseCapacity(ec)
              valueStackCapacity = len(valueStack)
            valueStack[valueStackSize] = value
            valueStackSize += 1
          else:
            # DEREF_STATIC_FIELD
            classInfo = classTable[row[0]]
            staticConstructorNotInvoked = True
            if (classInfo[4] < 2):
              stack[0] = pc
              stackFrame2 = maybeInvokeStaticConstructor(vm, ec, stack, classInfo, valueStackSize, PST_IntBuffer16)
              if (PST_IntBuffer16[0] == 1):
                return generateException(vm, stack, pc, valueStackSize, ec, 0, "Static initialization loop detected. The class this field is a member of is not done being initialized.")
              if (stackFrame2 != None):
                staticConstructorNotInvoked = False
                stack = stackFrame2
                pc = stack[0]
                localsStackSetToken = stack[1]
                localsStackOffset = stack[2]
            if staticConstructorNotInvoked:
              if (valueStackSize == valueStackCapacity):
                valueStack = valueStackIncreaseCapacity(ec)
                valueStackCapacity = len(valueStack)
              valueStack[valueStackSize] = classInfo[5][row[1]]
              valueStackSize += 1
        elif (sc_0 == 28):
          # DUPLICATE_STACK_TOP
          if (row[0] == 1):
            value = valueStack[(valueStackSize - 1)]
            if (valueStackSize == valueStackCapacity):
              valueStack = valueStackIncreaseCapacity(ec)
              valueStackCapacity = len(valueStack)
            valueStack[valueStackSize] = value
            valueStackSize += 1
          elif (row[0] == 2):
            if ((valueStackSize + 1) > valueStackCapacity):
              valueStackIncreaseCapacity(ec)
              valueStack = ec[4]
              valueStackCapacity = len(valueStack)
            valueStack[valueStackSize] = valueStack[(valueStackSize - 2)]
            valueStack[(valueStackSize + 1)] = valueStack[(valueStackSize - 1)]
            valueStackSize += 2
          else:
            hasInterrupt = EX_Fatal(ec, "?")
        else:
          # EQUALS
          valueStackSize -= 2
          rightValue = valueStack[(valueStackSize + 1)]
          leftValue = valueStack[valueStackSize]
          if (leftValue[0] == rightValue[0]):
            sc_15 = swlookup__interpretImpl__15.get(leftValue[0], 4)
            if (sc_15 < 3):
              if (sc_15 == 0):
                bool1 = True
              elif (sc_15 == 1):
                bool1 = (leftValue[1] == rightValue[1])
              else:
                bool1 = (leftValue[1] == rightValue[1])
            elif (sc_15 == 3):
              bool1 = (leftValue[1] == rightValue[1])
            else:
              bool1 = (doEqualityComparisonAndReturnCode(leftValue, rightValue) == 1)
          else:
            int1 = doEqualityComparisonAndReturnCode(leftValue, rightValue)
            if (int1 == 0):
              bool1 = False
            elif (int1 == 1):
              bool1 = True
            else:
              hasInterrupt = EX_UnsupportedOperation(ec, "== and != not defined here.")
          if (valueStackSize == valueStackCapacity):
            valueStack = valueStackIncreaseCapacity(ec)
            valueStackCapacity = len(valueStack)
          if (bool1 != ((row[0] == 1))):
            valueStack[valueStackSize] = VALUE_TRUE
          else:
            valueStack[valueStackSize] = VALUE_FALSE
          valueStackSize += 1
      elif (sc_0 < 32):
        if (sc_0 == 30):
          # ESF_LOOKUP
          esfData = generateEsfData(len(args), row)
          metadata[18] = esfData
        else:
          # EXCEPTION_HANDLED_TOGGLE
          ec[9] = (row[0] == 1)
      elif (sc_0 == 32):
        # FIELD_TYPE_INFO
        initializeClassFieldTypeInfo(vm, row)
      else:
        # FINALIZE_INITIALIZATION
        finalizeInitializationImpl(vm, stringArgs[pc], row[0])
        identifiers = vm[4][0]
        literalTable = vm[4][3]
        globalNameIdToPrimitiveMethodName = vm[4][12]
        funcArgs = vm[8]
    elif (sc_0 < 51):
      if (sc_0 < 43):
        if (sc_0 < 39):
          if (sc_0 < 37):
            if (sc_0 == 34):
              # FINALLY_END
              value = ec[10]
              if ((value == None) or ec[9]):
                sc_16 = swlookup__interpretImpl__16.get(stack[10], 4)
                if (sc_16 < 3):
                  if (sc_16 == 0):
                    ec[10] = None
                  elif (sc_16 == 1):
                    ec[10] = None
                    int1 = row[0]
                    if (int1 == 1):
                      pc += row[1]
                    elif (int1 == 2):
                      intArray1 = esfData[pc]
                      pc = intArray1[1]
                    else:
                      hasInterrupt = EX_Fatal(ec, "break exists without a loop")
                  else:
                    ec[10] = None
                    int1 = row[2]
                    if (int1 == 1):
                      pc += row[3]
                    elif (int1 == 2):
                      intArray1 = esfData[pc]
                      pc = intArray1[1]
                    else:
                      hasInterrupt = EX_Fatal(ec, "continue exists without a loop")
                elif (sc_16 == 3):
                  if (stack[8] != 0):
                    markClassAsInitialized(vm, stack, stack[8])
                  if stack[5]:
                    valueStackSize = stack[7]
                    value = stack[11]
                    stack = stack[4]
                    if (valueStackSize == valueStackCapacity):
                      valueStack = valueStackIncreaseCapacity(ec)
                      valueStackCapacity = len(valueStack)
                    valueStack[valueStackSize] = value
                    valueStackSize += 1
                  else:
                    valueStackSize = stack[7]
                    stack = stack[4]
                  pc = stack[0]
                  localsStackOffset = stack[2]
                  localsStackSetToken = stack[1]
              else:
                ec[9] = False
                stack[0] = pc
                intArray1 = esfData[pc]
                value = ec[10]
                objInstance1 = value[1]
                objArray1 = objInstance1[3]
                bool1 = True
                if (objArray1[0] != None):
                  bool1 = objArray1[0]
                intList1 = objArray1[1]
                while ((stack != None) and ((intArray1 == None) or bool1)):
                  stack = stack[4]
                  if (stack != None):
                    pc = stack[0]
                    intList1.append(pc)
                    intArray1 = esfData[pc]
                if (stack == None):
                  return uncaughtExceptionResult(vm, value)
                int1 = intArray1[0]
                if (int1 < pc):
                  int1 = intArray1[1]
                pc = (int1 - 1)
                stack[0] = pc
                localsStackOffset = stack[2]
                localsStackSetToken = stack[1]
                ec[1] = stack
                stack[10] = 0
                ec[2] = valueStackSize
            elif (sc_0 == 35):
              # FUNCTION_DEFINITION
              initializeFunction(vm, row, pc, stringArgs[pc])
              pc += row[7]
              functionTable = metadata[10]
            else:
              # INDEX
              valueStackSize -= 1
              value = valueStack[valueStackSize]
              root = valueStack[(valueStackSize - 1)]
              if (root[0] == 6):
                if (value[0] != 3):
                  hasInterrupt = EX_InvalidArgument(ec, "List index must be an integer.")
                else:
                  i = value[1]
                  list1 = root[1]
                  if (i < 0):
                    i += list1[1]
                  if ((i < 0) or (i >= list1[1])):
                    hasInterrupt = EX_IndexOutOfRange(ec, "List index is out of bounds")
                  else:
                    valueStack[(valueStackSize - 1)] = list1[2][i]
              elif (root[0] == 7):
                dictImpl = root[1]
                keyType = value[0]
                if (keyType != dictImpl[1]):
                  if (dictImpl[0] == 0):
                    hasInterrupt = EX_KeyNotFound(ec, "Key not found. Dictionary is empty.")
                  else:
                    hasInterrupt = EX_InvalidKey(ec, ''.join(["Incorrect key type. This dictionary contains ", getTypeFromId(dictImpl[1]), " keys. Provided key is a ", getTypeFromId(keyType), "."]))
                else:
                  if (keyType == 3):
                    intKey = value[1]
                  elif (keyType == 5):
                    stringKey = value[1]
                  elif (keyType == 8):
                    intKey = (value[1])[1]
                  elif (dictImpl[0] == 0):
                    hasInterrupt = EX_KeyNotFound(ec, "Key not found. Dictionary is empty.")
                  else:
                    hasInterrupt = EX_KeyNotFound(ec, "Key not found.")
                  if not (hasInterrupt):
                    if (keyType == 5):
                      stringIntDict1 = dictImpl[5]
                      int1 = stringIntDict1.get(stringKey, -1)
                      if (int1 == -1):
                        hasInterrupt = EX_KeyNotFound(ec, ''.join(["Key not found: '", stringKey, "'"]))
                      else:
                        valueStack[(valueStackSize - 1)] = dictImpl[7][int1]
                    else:
                      intIntDict1 = dictImpl[4]
                      int1 = intIntDict1.get(intKey, -1)
                      if (int1 == -1):
                        hasInterrupt = EX_KeyNotFound(ec, "Key not found.")
                      else:
                        valueStack[(valueStackSize - 1)] = dictImpl[7][intIntDict1[intKey]]
              elif (root[0] == 5):
                string1 = root[1]
                if (value[0] != 3):
                  hasInterrupt = EX_InvalidArgument(ec, "String indices must be integers.")
                else:
                  int1 = value[1]
                  if (int1 < 0):
                    int1 += len(string1)
                  if ((int1 < 0) or (int1 >= len(string1))):
                    hasInterrupt = EX_IndexOutOfRange(ec, "String index out of range.")
                  else:
                    valueStack[(valueStackSize - 1)] = buildCommonString(globals, string1[int1])
              else:
                hasInterrupt = EX_InvalidArgument(ec, "Cannot index into this type: " + getTypeFromId(root[0]))
          elif (sc_0 == 37):
            # IS_COMPARISON
            value = valueStack[(valueStackSize - 1)]
            output = VALUE_FALSE
            if (value[0] == 8):
              objInstance1 = value[1]
              if isClassASubclassOf(vm, objInstance1[0], row[0]):
                output = VALUE_TRUE
            valueStack[(valueStackSize - 1)] = output
          else:
            # ITERATION_STEP
            int1 = (localsStackOffset + row[2])
            value3 = localsStack[int1]
            i = value3[1]
            value = localsStack[(localsStackOffset + row[3])]
            if (value[0] == 6):
              list1 = value[1]
              _len = list1[1]
              bool1 = True
            else:
              string2 = value[1]
              _len = len(string2)
              bool1 = False
            if (i < _len):
              if bool1:
                value = list1[2][i]
              else:
                value = buildCommonString(globals, string2[i])
              int3 = (localsStackOffset + row[1])
              localsStackSet[int3] = localsStackSetToken
              localsStack[int3] = value
            else:
              pc += row[0]
            i += 1
            if (i < 2049):
              localsStack[int1] = INTEGER_POSITIVE_CACHE[i]
            else:
              localsStack[int1] = [3, i]
        elif (sc_0 < 41):
          if (sc_0 == 39):
            # JUMP
            pc += row[0]
          else:
            # JUMP_IF_EXCEPTION_OF_TYPE
            value = ec[10]
            objInstance1 = value[1]
            int1 = objInstance1[0]
            i = (len(row) - 1)
            while (i >= 2):
              if isClassASubclassOf(vm, int1, row[i]):
                i = 0
                pc += row[0]
                int2 = row[1]
                if (int2 > -1):
                  int1 = (localsStackOffset + int2)
                  localsStack[int1] = value
                  localsStackSet[int1] = localsStackSetToken
              i -= 1
        elif (sc_0 == 41):
          # JUMP_IF_FALSE
          valueStackSize -= 1
          value = valueStack[valueStackSize]
          if (value[0] != 2):
            hasInterrupt = EX_InvalidArgument(ec, "Boolean expected.")
          elif not (value[1]):
            pc += row[0]
        else:
          # JUMP_IF_FALSE_NON_POP
          value = valueStack[(valueStackSize - 1)]
          if (value[0] != 2):
            hasInterrupt = EX_InvalidArgument(ec, "Boolean expected.")
          elif value[1]:
            valueStackSize -= 1
          else:
            pc += row[0]
      elif (sc_0 < 47):
        if (sc_0 < 45):
          if (sc_0 == 43):
            # JUMP_IF_TRUE
            valueStackSize -= 1
            value = valueStack[valueStackSize]
            if (value[0] != 2):
              hasInterrupt = EX_InvalidArgument(ec, "Boolean expected.")
            elif value[1]:
              pc += row[0]
          else:
            # JUMP_IF_TRUE_NO_POP
            value = valueStack[(valueStackSize - 1)]
            if (value[0] != 2):
              hasInterrupt = EX_InvalidArgument(ec, "Boolean expected.")
            elif value[1]:
              pc += row[0]
            else:
              valueStackSize -= 1
        elif (sc_0 == 45):
          # LAMBDA
          if not ((pc in metadata[11])):
            int1 = (4 + row[4] + 1)
            _len = row[int1]
            intArray1 = (PST_NoneListOfOne * _len)
            i = 0
            while (i < _len):
              intArray1[i] = row[(int1 + i + 1)]
              i += 1
            _len = row[4]
            intArray2 = (PST_NoneListOfOne * _len)
            i = 0
            while (i < _len):
              intArray2[i] = row[(5 + i)]
              i += 1
            metadata[11][pc] = [pc, 0, pc, row[0], row[1], 5, 0, row[2], intArray2, "lambda", intArray1]
          closure = {}
          parentClosure = stack[12]
          if (parentClosure == None):
            parentClosure = {}
            stack[12] = parentClosure
            parentClosure[-1] = [stack[6]]
          closure[-1] = parentClosure[-1]
          functionInfo = metadata[11][pc]
          intArray1 = functionInfo[10]
          _len = len(intArray1)
          i = 0
          while (i < _len):
            j = intArray1[i]
            if (j in parentClosure):
              closure[j] = parentClosure[j]
            else:
              closure[j] = [None]
              parentClosure[j] = closure[j]
            i += 1
          if (valueStackSize == valueStackCapacity):
            valueStack = valueStackIncreaseCapacity(ec)
            valueStackCapacity = len(valueStack)
          valueStack[valueStackSize] = [9, [5, None, 0, pc, closure]]
          valueStackSize += 1
          pc += row[3]
        else:
          # LIB_DECLARATION
          prepareToSuspend(ec, stack, valueStackSize, pc)
          ec[13] = [4, row[0], stringArgs[pc], 0.0, None]
          hasInterrupt = True
      elif (sc_0 < 49):
        if (sc_0 == 47):
          # LIST_SLICE
          if (row[2] == 1):
            valueStackSize -= 1
            arg3 = valueStack[valueStackSize]
          else:
            arg3 = None
          if (row[1] == 1):
            valueStackSize -= 1
            arg2 = valueStack[valueStackSize]
          else:
            arg2 = None
          if (row[0] == 1):
            valueStackSize -= 1
            arg1 = valueStack[valueStackSize]
          else:
            arg1 = None
          value = valueStack[(valueStackSize - 1)]
          value = performListSlice(globals, ec, value, arg1, arg2, arg3)
          hasInterrupt = (ec[13] != None)
          if not (hasInterrupt):
            valueStack[(valueStackSize - 1)] = value
        else:
          # LITERAL
          if (valueStackSize == valueStackCapacity):
            valueStack = valueStackIncreaseCapacity(ec)
            valueStackCapacity = len(valueStack)
          valueStack[valueStackSize] = literalTable[row[0]]
          valueStackSize += 1
      elif (sc_0 == 49):
        # LITERAL_STREAM
        int1 = len(row)
        if ((valueStackSize + int1) > valueStackCapacity):
          while ((valueStackSize + int1) > valueStackCapacity):
            valueStackIncreaseCapacity(ec)
            valueStack = ec[4]
            valueStackCapacity = len(valueStack)
        i = (int1 - 1)
        while (i >= 0):
          valueStack[valueStackSize] = literalTable[row[i]]
          valueStackSize += 1
          i -= 1
      else:
        # LOCAL
        int1 = (localsStackOffset + row[0])
        if (localsStackSet[int1] == localsStackSetToken):
          if (valueStackSize == valueStackCapacity):
            valueStack = valueStackIncreaseCapacity(ec)
            valueStackCapacity = len(valueStack)
          valueStack[valueStackSize] = localsStack[int1]
          valueStackSize += 1
        else:
          hasInterrupt = EX_UnassignedVariable(ec, "Variable used before it was set.")
    elif (sc_0 < 59):
      if (sc_0 < 55):
        if (sc_0 < 53):
          if (sc_0 == 51):
            # LOC_TABLE
            initLocTable(vm, row)
          else:
            # NEGATIVE_SIGN
            value = valueStack[(valueStackSize - 1)]
            type = value[0]
            if (type == 3):
              valueStack[(valueStackSize - 1)] = buildInteger(globals, -(value[1]))
            elif (type == 4):
              valueStack[(valueStackSize - 1)] = buildFloat(globals, -(value[1]))
            else:
              hasInterrupt = EX_InvalidArgument(ec, ''.join(["Negative sign can only be applied to numbers. Found ", getTypeFromId(type), " instead."]))
        elif (sc_0 == 53):
          # POP
          valueStackSize -= 1
        else:
          # POP_IF_NULL_OR_JUMP
          value = valueStack[(valueStackSize - 1)]
          if (value[0] == 1):
            valueStackSize -= 1
          else:
            pc += row[0]
      elif (sc_0 < 57):
        if (sc_0 == 55):
          # PUSH_FUNC_REF
          value = None
          sc_17 = swlookup__interpretImpl__17.get(row[1], 3)
          if (sc_17 < 2):
            if (sc_17 == 0):
              value = [9, [1, None, 0, row[0], None]]
            else:
              value = [9, [2, stack[6], row[2], row[0], None]]
          elif (sc_17 == 2):
            classId = row[2]
            classInfo = classTable[classId]
            staticConstructorNotInvoked = True
            if (classInfo[4] < 2):
              stack[0] = pc
              stackFrame2 = maybeInvokeStaticConstructor(vm, ec, stack, classInfo, valueStackSize, PST_IntBuffer16)
              if (PST_IntBuffer16[0] == 1):
                return generateException(vm, stack, pc, valueStackSize, ec, 0, "Static initialization loop detected. The class this field is a member of is not done being initialized.")
              if (stackFrame2 != None):
                staticConstructorNotInvoked = False
                stack = stackFrame2
                pc = stack[0]
                localsStackSetToken = stack[1]
                localsStackOffset = stack[2]
            if staticConstructorNotInvoked:
              value = [9, [3, None, classId, row[0], None]]
            else:
              value = None
          if (value != None):
            if (valueStackSize == valueStackCapacity):
              valueStack = valueStackIncreaseCapacity(ec)
              valueStackCapacity = len(valueStack)
            valueStack[valueStackSize] = value
            valueStackSize += 1
        else:
          # RETURN
          if (esfData[pc] != None):
            intArray1 = esfData[pc]
            pc = (intArray1[1] - 1)
            if (row[0] == 0):
              stack[11] = VALUE_NULL
            else:
              stack[11] = valueStack[(valueStackSize - 1)]
            valueStackSize = stack[7]
            stack[10] = 3
          else:
            if (stack[4] == None):
              return interpreterFinished(vm, ec)
            if (stack[8] != 0):
              markClassAsInitialized(vm, stack, stack[8])
            if stack[5]:
              if (row[0] == 0):
                valueStackSize = stack[7]
                stack = stack[4]
                if (valueStackSize == valueStackCapacity):
                  valueStack = valueStackIncreaseCapacity(ec)
                  valueStackCapacity = len(valueStack)
                valueStack[valueStackSize] = VALUE_NULL
              else:
                value = valueStack[(valueStackSize - 1)]
                valueStackSize = stack[7]
                stack = stack[4]
                valueStack[valueStackSize] = value
              valueStackSize += 1
            else:
              valueStackSize = stack[7]
              stack = stack[4]
            pc = stack[0]
            localsStackOffset = stack[2]
            localsStackSetToken = stack[1]
      elif (sc_0 == 57):
        # STACK_INSERTION_FOR_INCREMENT
        if (valueStackSize == valueStackCapacity):
          valueStack = valueStackIncreaseCapacity(ec)
          valueStackCapacity = len(valueStack)
        valueStack[valueStackSize] = valueStack[(valueStackSize - 1)]
        valueStack[(valueStackSize - 1)] = valueStack[(valueStackSize - 2)]
        valueStack[(valueStackSize - 2)] = valueStack[(valueStackSize - 3)]
        valueStack[(valueStackSize - 3)] = valueStack[valueStackSize]
        valueStackSize += 1
      else:
        # STACK_SWAP_POP
        valueStackSize -= 1
        valueStack[(valueStackSize - 1)] = valueStack[valueStackSize]
    elif (sc_0 < 63):
      if (sc_0 < 61):
        if (sc_0 == 59):
          # SWITCH_INT
          valueStackSize -= 1
          value = valueStack[valueStackSize]
          if (value[0] == 3):
            intKey = value[1]
            integerSwitch = integerSwitchesByPc[pc]
            if (integerSwitch == None):
              integerSwitch = initializeIntSwitchStatement(vm, pc, row)
            i = integerSwitch.get(intKey, -1)
            if (i == -1):
              pc += row[0]
            else:
              pc += i
          else:
            hasInterrupt = EX_InvalidArgument(ec, "Switch statement expects an integer.")
        else:
          # SWITCH_STRING
          valueStackSize -= 1
          value = valueStack[valueStackSize]
          if (value[0] == 5):
            stringKey = value[1]
            stringSwitch = stringSwitchesByPc[pc]
            if (stringSwitch == None):
              stringSwitch = initializeStringSwitchStatement(vm, pc, row)
            i = stringSwitch.get(stringKey, -1)
            if (i == -1):
              pc += row[0]
            else:
              pc += i
          else:
            hasInterrupt = EX_InvalidArgument(ec, "Switch statement expects a string.")
      elif (sc_0 == 61):
        # THIS
        if (valueStackSize == valueStackCapacity):
          valueStack = valueStackIncreaseCapacity(ec)
          valueStackCapacity = len(valueStack)
        valueStack[valueStackSize] = stack[6]
        valueStackSize += 1
      else:
        # THROW
        valueStackSize -= 1
        value = valueStack[valueStackSize]
        bool2 = (value[0] == 8)
        if bool2:
          objInstance1 = value[1]
          if not (isClassASubclassOf(vm, objInstance1[0], magicNumbers[0])):
            bool2 = False
        if bool2:
          objArray1 = objInstance1[3]
          intList1 = []
          objArray1[1] = intList1
          if not (isPcFromCore(vm, pc)):
            intList1.append(pc)
          ec[10] = value
          ec[9] = False
          stack[0] = pc
          intArray1 = esfData[pc]
          value = ec[10]
          objInstance1 = value[1]
          objArray1 = objInstance1[3]
          bool1 = True
          if (objArray1[0] != None):
            bool1 = objArray1[0]
          intList1 = objArray1[1]
          while ((stack != None) and ((intArray1 == None) or bool1)):
            stack = stack[4]
            if (stack != None):
              pc = stack[0]
              intList1.append(pc)
              intArray1 = esfData[pc]
          if (stack == None):
            return uncaughtExceptionResult(vm, value)
          int1 = intArray1[0]
          if (int1 < pc):
            int1 = intArray1[1]
          pc = (int1 - 1)
          stack[0] = pc
          localsStackOffset = stack[2]
          localsStackSetToken = stack[1]
          ec[1] = stack
          stack[10] = 0
          ec[2] = valueStackSize
        else:
          hasInterrupt = EX_InvalidArgument(ec, "Thrown value is not an exception.")
    elif (sc_0 < 65):
      if (sc_0 == 63):
        # TOKEN_DATA
        tokenDataImpl(vm, row)
      else:
        # USER_CODE_START
        metadata[16] = row[0]
    elif (sc_0 == 65):
      # VERIFY_TYPE_IS_ITERABLE
      valueStackSize -= 1
      value = valueStack[valueStackSize]
      i = (localsStackOffset + row[0])
      localsStack[i] = value
      localsStackSet[i] = localsStackSetToken
      int1 = value[0]
      if ((int1 != 6) and (int1 != 5)):
        hasInterrupt = EX_InvalidArgument(ec, ''.join(["Expected an iterable type, such as a list or string. Found ", getTypeFromId(int1), " instead."]))
      i = (localsStackOffset + row[1])
      localsStack[i] = VALUE_INT_ZERO
      localsStackSet[i] = localsStackSetToken
    else:
      # THIS SHOULD NEVER HAPPEN
      return generateException(vm, stack, pc, valueStackSize, ec, 0, "Bad op code: " + str(ops[pc]))
    if hasInterrupt:
      interrupt = ec[13]
      ec[13] = None
      if (interrupt[0] == 1):
        return generateException(vm, stack, pc, valueStackSize, ec, interrupt[1], interrupt[2])
      if (interrupt[0] == 3):
        return [5, "", interrupt[3], 0, False, ""]
      if (interrupt[0] == 4):
        return [6, "", 0.0, 0, False, interrupt[2]]
    pc += 1

swlookup__interpretImpl__1 = { 553: 0, 554: 1, 718: 2, 719: 3, 708: 4, 707: 5, 543: 6, 542: 7, 996: 8, 498: 9, 509: 10, 520: 11, 531: 12, 663: 13, 499: 14, 664: 15, 510: 16, 674: 17, 675: 18, 685: 19, 521: 20, 686: 21, 532: 22, 696: 23, 697: 24, 564: 25, 575: 26, 586: 27, 597: 28, 608: 29, 619: 30, 630: 31, 784: 32, 795: 33, 620: 34, 631: 35, 785: 36, 796: 37, 652: 38, 641: 39, 817: 40, 806: 41, 653: 42, 642: 43, 818: 44, 807: 45, 830: 46, 850: 47, 522: 48, 1015: 49, 523: 50 }
swlookup__interpretImpl__2 = { 1: 0, 8: 1, 10: 2 }
swlookup__interpretImpl__3 = { 1: 0, 2: 1, 3: 2, 4: 3, 5: 4 }
swlookup__interpretImpl__5 = { 7: 0, 9: 1, 13: 2, 19: 3, 20: 4, 25: 5, 26: 6, 27: 7, 30: 8, 31: 9, 32: 10, 33: 11 }
swlookup__interpretImpl__6 = { 0: 0, 3: 1, 4: 2, 5: 3, 6: 4, 7: 5, 10: 6, 14: 7, 17: 8, 21: 9, 23: 10, 24: 11, 26: 12, 28: 13, 29: 14 }
swlookup__interpretImpl__8 = { 3: 0, 8: 1, 5: 2 }
swlookup__interpretImpl__7 = { 4: 0, 5: 1, 7: 2, 11: 3, 18: 4, 22: 5, 24: 6, 34: 7 }
swlookup__interpretImpl__9 = { 1: 0, 2: 1, 12: 2, 15: 3 }
swlookup__interpretImpl__10 = { 12: 0, 16: 1 }
swlookup__interpretImpl__4 = { 5: 0, 6: 1, 7: 2, 9: 3, 10: 4 }
swlookup__interpretImpl__12 = { 0: 0, 1: 1, 2: 2 }
swlookup__interpretImpl__11 = { 1: 0, 10: 1, 2: 2, 3: 3, 6: 4, 7: 5 }
swlookup__interpretImpl__13 = { 1: 0, 2: 1, 3: 2, 4: 3, 5: 4, 6: 5, 7: 6, 8: 7, 9: 8, 10: 9, 11: 10, 12: 11, 13: 12, 14: 13, 15: 14, 16: 15, 17: 16, 18: 17, 19: 18, 20: 19, 21: 20, 22: 21, 23: 22, 24: 23, 25: 24, 26: 25, 27: 26, 28: 27, 29: 28, 30: 29, 31: 30, 32: 31, 33: 32, 34: 33, 35: 34, 36: 35, 37: 36, 38: 37, 39: 38, 40: 39, 41: 40 }
swlookup__interpretImpl__14 = { 8: 0, 5: 1, 6: 2, 7: 3 }
swlookup__interpretImpl__15 = { 1: 0, 2: 1, 3: 2, 5: 3 }
swlookup__interpretImpl__16 = { 0: 0, 1: 1, 2: 2, 3: 3 }
swlookup__interpretImpl__17 = { 0: 0, 1: 1, 2: 2 }
swlookup__interpretImpl__0 = { 0: 0, 1: 1, 2: 2, 3: 3, 4: 4, 6: 5, 7: 6, 8: 7, 5: 8, 9: 9, 10: 10, 11: 11, 12: 12, 13: 13, 14: 14, 15: 15, 16: 16, 17: 17, 18: 18, 19: 19, 20: 20, 21: 21, 22: 22, 23: 23, 24: 24, 25: 25, 26: 26, 27: 27, 28: 28, 29: 29, 30: 30, 31: 31, 32: 32, 33: 33, 34: 34, 35: 35, 36: 36, 37: 37, 38: 38, 39: 39, 40: 40, 41: 41, 42: 42, 43: 43, 44: 44, 45: 45, 46: 46, 47: 47, 48: 48, 49: 49, 50: 50, 51: 51, 52: 52, 53: 53, 54: 54, 55: 55, 56: 56, 57: 57, 58: 58, 59: 59, 60: 60, 61: 61, 62: 62, 63: 63, 64: 64, 65: 65 }

def invokeNamedCallback(vm, id, args):
  cb = vm[12][0][id]
  return cb(args)

def isClassASubclassOf(vm, subClassId, parentClassId):
  if (subClassId == parentClassId):
    return True
  classTable = vm[4][9]
  classIdWalker = subClassId
  while (classIdWalker != -1):
    if (classIdWalker == parentClassId):
      return True
    classInfo = classTable[classIdWalker]
    classIdWalker = classInfo[2]
  return False

def isPcFromCore(vm, pc):
  if (vm[3] == None):
    return False
  tokens = vm[3][0][pc]
  if (tokens == None):
    return False
  token = tokens[0]
  filename = tokenHelperGetFileLine(vm, token[2], 0)
  return "[Core]" == filename

def isStringEqual(a, b):
  if (a == b):
    return True
  return False

def isVmResultRootExecContext(result):
  return result[4]

def makeEmptyList(type, capacity):
  return [type, 0, []]

def markClassAsInitialized(vm, stack, classId):
  classInfo = vm[4][9][stack[8]]
  classInfo[4] = 2
  vm[7].pop()
  return 0

def maybeInvokeStaticConstructor(vm, ec, stack, classInfo, valueStackSize, intOutParam):
  PST_IntBuffer16[0] = 0
  classId = classInfo[0]
  if (classInfo[4] == 1):
    classIdsBeingInitialized = vm[7]
    if (classIdsBeingInitialized[(len(classIdsBeingInitialized) - 1)] != classId):
      PST_IntBuffer16[0] = 1
    return None
  classInfo[4] = 1
  vm[7].append(classId)
  functionInfo = vm[4][10][classInfo[6]]
  stack[0] -= 1
  newFrameLocalsSize = functionInfo[7]
  currentFrameLocalsEnd = stack[3]
  if (len(ec[5]) <= (currentFrameLocalsEnd + newFrameLocalsSize)):
    increaseLocalsStackCapacity(ec, newFrameLocalsSize)
  if (ec[7] > 2000000000):
    resetLocalsStackTokens(ec, stack)
  ec[7] += 1
  return [functionInfo[2], ec[7], currentFrameLocalsEnd, (currentFrameLocalsEnd + newFrameLocalsSize), stack, False, None, valueStackSize, classId, (stack[9] + 1), 0, None, None, None]

def multiplyString(globals, strValue, str, n):
  if (n <= 2):
    if (n == 1):
      return strValue
    if (n == 2):
      return buildString(globals, str + str)
    return globals[8]
  builder = []
  while (n > 0):
    n -= 1
    builder.append(str)
  str = "".join(builder)
  return buildString(globals, str)

def nextPowerOf2(value):
  if (((value - 1) & value) == 0):
    return value
  output = 1
  while (output < value):
    output *= 2
  return output

def noop():
  return 0

def performListSlice(globals, ec, value, arg1, arg2, arg3):
  begin = 0
  end = 0
  step = 0
  length = 0
  i = 0
  isForward = False
  isString = False
  originalString = ""
  originalList = None
  outputList = None
  outputString = None
  status = 0
  if (arg3 != None):
    if (arg3[0] == 3):
      step = arg3[1]
      if (step == 0):
        status = 2
    else:
      status = 3
      step = 1
  else:
    step = 1
  isForward = (step > 0)
  if (arg2 != None):
    if (arg2[0] == 3):
      end = arg2[1]
    else:
      status = 3
  if (arg1 != None):
    if (arg1[0] == 3):
      begin = arg1[1]
    else:
      status = 3
  if (value[0] == 5):
    isString = True
    originalString = value[1]
    length = len(originalString)
  elif (value[0] == 6):
    isString = False
    originalList = value[1]
    length = originalList[1]
  else:
    EX_InvalidArgument(ec, ''.join(["Cannot apply slicing to ", getTypeFromId(value[0]), ". Must be string or list."]))
    return globals[0]
  if (status >= 2):
    msg = None
    if isString:
      msg = "String"
    else:
      msg = "List"
    if (status == 3):
      msg += " slice indexes must be integers. Found "
      if ((arg1 != None) and (arg1[0] != 3)):
        EX_InvalidArgument(ec, ''.join([msg, getTypeFromId(arg1[0]), " for begin index."]))
        return globals[0]
      if ((arg2 != None) and (arg2[0] != 3)):
        EX_InvalidArgument(ec, ''.join([msg, getTypeFromId(arg2[0]), " for end index."]))
        return globals[0]
      if ((arg3 != None) and (arg3[0] != 3)):
        EX_InvalidArgument(ec, ''.join([msg, getTypeFromId(arg3[0]), " for step amount."]))
        return globals[0]
      EX_InvalidArgument(ec, "Invalid slice arguments.")
      return globals[0]
    else:
      EX_InvalidArgument(ec, msg + " slice step cannot be 0.")
      return globals[0]
  status = canonicalizeListSliceArgs(PST_IntBuffer16, arg1, arg2, begin, end, step, length, isForward)
  if (status == 1):
    begin = PST_IntBuffer16[0]
    end = PST_IntBuffer16[1]
    if isString:
      outputString = []
      if isForward:
        if (step == 1):
          return buildString(globals, originalString[begin:begin + (end - begin)])
        else:
          while (begin < end):
            outputString.append(originalString[begin])
            begin += step
      else:
        while (begin > end):
          outputString.append(originalString[begin])
          begin += step
      value = buildString(globals, "".join(outputString))
    else:
      outputList = makeEmptyList(originalList[0], 10)
      if isForward:
        while (begin < end):
          addToList(outputList, originalList[2][begin])
          begin += step
      else:
        while (begin > end):
          addToList(outputList, originalList[2][begin])
          begin += step
      value = [6, outputList]
  elif (status == 0):
    if isString:
      value = globals[8]
    else:
      value = [6, makeEmptyList(originalList[0], 0)]
  elif (status == 2):
    if not (isString):
      outputList = makeEmptyList(originalList[0], length)
      i = 0
      while (i < length):
        addToList(outputList, originalList[2][i])
        i += 1
      value = [6, outputList]
  else:
    msg = None
    if isString:
      msg = "String"
    else:
      msg = "List"
    if (status == 3):
      msg += " slice begin index is out of range."
    elif isForward:
      msg += " slice begin index must occur before the end index when step is positive."
    else:
      msg += " slice begin index must occur after the end index when the step is negative."
    EX_IndexOutOfRange(ec, msg)
    return globals[0]
  return value

def prepareToSuspend(ec, stack, valueStackSize, currentPc):
  ec[1] = stack
  ec[2] = valueStackSize
  stack[0] = (currentPc + 1)
  return 0

def primitiveMethodsInitializeLookup(nameLookups):
  length = len(nameLookups)
  lookup = (PST_NoneListOfOne * length)
  i = 0
  while (i < length):
    lookup[i] = -1
    i += 1
  if ("add" in nameLookups):
    lookup[nameLookups["add"]] = 0
  if ("argCountMax" in nameLookups):
    lookup[nameLookups["argCountMax"]] = 1
  if ("argCountMin" in nameLookups):
    lookup[nameLookups["argCountMin"]] = 2
  if ("choice" in nameLookups):
    lookup[nameLookups["choice"]] = 3
  if ("clear" in nameLookups):
    lookup[nameLookups["clear"]] = 4
  if ("clone" in nameLookups):
    lookup[nameLookups["clone"]] = 5
  if ("concat" in nameLookups):
    lookup[nameLookups["concat"]] = 6
  if ("contains" in nameLookups):
    lookup[nameLookups["contains"]] = 7
  if ("createInstance" in nameLookups):
    lookup[nameLookups["createInstance"]] = 8
  if ("endsWith" in nameLookups):
    lookup[nameLookups["endsWith"]] = 9
  if ("filter" in nameLookups):
    lookup[nameLookups["filter"]] = 10
  if ("get" in nameLookups):
    lookup[nameLookups["get"]] = 11
  if ("getName" in nameLookups):
    lookup[nameLookups["getName"]] = 12
  if ("indexOf" in nameLookups):
    lookup[nameLookups["indexOf"]] = 13
  if ("insert" in nameLookups):
    lookup[nameLookups["insert"]] = 14
  if ("invoke" in nameLookups):
    lookup[nameLookups["invoke"]] = 15
  if ("isA" in nameLookups):
    lookup[nameLookups["isA"]] = 16
  if ("join" in nameLookups):
    lookup[nameLookups["join"]] = 17
  if ("keys" in nameLookups):
    lookup[nameLookups["keys"]] = 18
  if ("lower" in nameLookups):
    lookup[nameLookups["lower"]] = 19
  if ("ltrim" in nameLookups):
    lookup[nameLookups["ltrim"]] = 20
  if ("map" in nameLookups):
    lookup[nameLookups["map"]] = 21
  if ("merge" in nameLookups):
    lookup[nameLookups["merge"]] = 22
  if ("pop" in nameLookups):
    lookup[nameLookups["pop"]] = 23
  if ("remove" in nameLookups):
    lookup[nameLookups["remove"]] = 24
  if ("replace" in nameLookups):
    lookup[nameLookups["replace"]] = 25
  if ("reverse" in nameLookups):
    lookup[nameLookups["reverse"]] = 26
  if ("rtrim" in nameLookups):
    lookup[nameLookups["rtrim"]] = 27
  if ("shuffle" in nameLookups):
    lookup[nameLookups["shuffle"]] = 28
  if ("sort" in nameLookups):
    lookup[nameLookups["sort"]] = 29
  if ("split" in nameLookups):
    lookup[nameLookups["split"]] = 30
  if ("startsWith" in nameLookups):
    lookup[nameLookups["startsWith"]] = 31
  if ("trim" in nameLookups):
    lookup[nameLookups["trim"]] = 32
  if ("upper" in nameLookups):
    lookup[nameLookups["upper"]] = 33
  if ("values" in nameLookups):
    lookup[nameLookups["values"]] = 34
  return lookup

def primitiveMethodWrongArgCountError(name, expected, actual):
  output = ""
  if (expected == 0):
    output = name + " does not accept any arguments."
  elif (expected == 1):
    output = name + " accepts exactly 1 argument."
  else:
    output = ''.join([name, " requires ", str(expected), " arguments."])
  return ''.join([output, " Found: ", str(actual)])

def printToStdOut(prefix, line):
  if (prefix == None):
    print(line)
  else:
    canonical = line.replace("\r\n", "\n").replace("\r", "\n")
    lines = canonical.split("\n")
    i = 0
    while (i < len(lines)):
      print(''.join([prefix, ": ", lines[i]]))
      i += 1
  return 0

def qsortHelper(keyStringList, keyNumList, indices, isString, startIndex, endIndex):
  if ((endIndex - startIndex) <= 0):
    return 0
  if ((endIndex - startIndex) == 1):
    if sortHelperIsRevOrder(keyStringList, keyNumList, isString, startIndex, endIndex):
      sortHelperSwap(keyStringList, keyNumList, indices, isString, startIndex, endIndex)
    return 0
  mid = ((endIndex + startIndex) >> 1)
  sortHelperSwap(keyStringList, keyNumList, indices, isString, mid, startIndex)
  upperPointer = (endIndex + 1)
  lowerPointer = (startIndex + 1)
  while (upperPointer > lowerPointer):
    if sortHelperIsRevOrder(keyStringList, keyNumList, isString, startIndex, lowerPointer):
      lowerPointer += 1
    else:
      upperPointer -= 1
      sortHelperSwap(keyStringList, keyNumList, indices, isString, lowerPointer, upperPointer)
  midIndex = (lowerPointer - 1)
  sortHelperSwap(keyStringList, keyNumList, indices, isString, midIndex, startIndex)
  qsortHelper(keyStringList, keyNumList, indices, isString, startIndex, (midIndex - 1))
  qsortHelper(keyStringList, keyNumList, indices, isString, (midIndex + 1), endIndex)
  return 0

def queryValue(vm, execId, stackFrameOffset, steps):
  if (execId == -1):
    execId = vm[1]
  ec = vm[0][execId]
  stackFrame = ec[1]
  while (stackFrameOffset > 0):
    stackFrameOffset -= 1
    stackFrame = stackFrame[4]
  current = None
  i = 0
  j = 0
  len = len(steps)
  i = 0
  while (i < len(steps)):
    if ((current == None) and (i > 0)):
      return None
    step = steps[i]
    if isStringEqual(".", step):
      return None
    elif isStringEqual("this", step):
      current = stackFrame[6]
    elif isStringEqual("class", step):
      return None
    elif isStringEqual("local", step):
      i += 1
      step = steps[i]
      localNamesByFuncPc = vm[3][5]
      localNames = None
      if ((localNamesByFuncPc == None) or (len(localNamesByFuncPc) == 0)):
        return None
      j = stackFrame[0]
      while (j >= 0):
        if (j in localNamesByFuncPc):
          localNames = localNamesByFuncPc[j]
          j = -1
        j -= 1
      if (localNames == None):
        return None
      localId = -1
      if (localNames != None):
        j = 0
        while (j < len(localNames)):
          if isStringEqual(localNames[j], step):
            localId = j
            j = len(localNames)
          j += 1
      if (localId == -1):
        return None
      localOffset = (localId + stackFrame[2])
      if (ec[6][localOffset] != stackFrame[1]):
        return None
      current = ec[5][localOffset]
    elif isStringEqual("index", step):
      return None
    elif isStringEqual("key-int", step):
      return None
    elif isStringEqual("key-str", step):
      return None
    elif isStringEqual("key-obj", step):
      return None
    else:
      return None
    i += 1
  return current

def read_integer(pindex, raw, length, alphaNums):
  num = 0
  c = raw[pindex[0]]
  pindex[0] = (pindex[0] + 1)
  if (c == "%"):
    value = read_till(pindex, raw, length, "%")
    num = int(value)
  elif (c == "@"):
    num = read_integer(pindex, raw, length, alphaNums)
    num *= 62
    num += read_integer(pindex, raw, length, alphaNums)
  elif (c == "#"):
    num = read_integer(pindex, raw, length, alphaNums)
    num *= 62
    num += read_integer(pindex, raw, length, alphaNums)
    num *= 62
    num += read_integer(pindex, raw, length, alphaNums)
  elif (c == "^"):
    num = (-1 * read_integer(pindex, raw, length, alphaNums))
  else:
    # TODO: string.IndexOfChar(c)
    num = alphaNums.find(c)
    if (num == -1):
      pass
  return num

def read_string(pindex, raw, length, alphaNums):
  b64 = read_till(pindex, raw, length, "%")
  return PST_base64ToString(b64)

def read_till(index, raw, length, end):
  output = []
  ctn = True
  c = " "
  while ctn:
    c = raw[index[0]]
    if (c == end):
      ctn = False
    else:
      output.append(c)
    index[0] = (index[0] + 1)
  return ''.join(output)

def reallocIntArray(original, requiredCapacity):
  oldSize = len(original)
  size = oldSize
  while (size < requiredCapacity):
    size *= 2
  output = (PST_NoneListOfOne * size)
  i = 0
  while (i < oldSize):
    output[i] = original[i]
    i += 1
  return output

def Reflect_allClasses(vm):
  generics = [None]
  generics[0] = 10
  output = makeEmptyList(generics, 20)
  classTable = vm[4][9]
  i = 1
  while (i < len(classTable)):
    classInfo = classTable[i]
    if (classInfo == None):
      i = len(classTable)
    else:
      addToList(output, [10, [False, classInfo[0]]])
    i += 1
  return [6, output]

def Reflect_getMethods(vm, ec, methodSource):
  output = makeEmptyList(None, 8)
  if (methodSource[0] == 8):
    objInstance1 = methodSource[1]
    classInfo = vm[4][9][objInstance1[0]]
    i = 0
    while (i < len(classInfo[9])):
      functionId = classInfo[9][i]
      if (functionId != -1):
        addToList(output, [9, [2, methodSource, objInstance1[0], functionId, None]])
      i += 1
  else:
    classValue = methodSource[1]
    classInfo = vm[4][9][classValue[1]]
    EX_UnsupportedOperation(ec, "static method reflection not implemented yet.")
  return [6, output]

def registerNamedCallback(vm, scope, functionName, callback):
  id = getNamedCallbackIdImpl(vm, scope, functionName, True)
  vm[12][0][id] = callback
  return id

def resetLocalsStackTokens(ec, stack):
  localsStack = ec[5]
  localsStackSet = ec[6]
  i = stack[3]
  while (i < len(localsStackSet)):
    localsStackSet[i] = 0
    localsStack[i] = None
    i += 1
  stackWalker = stack
  while (stackWalker != None):
    token = stackWalker[1]
    stackWalker[1] = 1
    i = stackWalker[2]
    while (i < stackWalker[3]):
      if (localsStackSet[i] == token):
        localsStackSet[i] = 1
      else:
        localsStackSet[i] = 0
        localsStack[i] = None
      i += 1
    stackWalker = stackWalker[4]
  ec[7] = 1
  return -1

def resolvePrimitiveMethodName2(lookup, type, globalNameId):
  output = lookup[globalNameId]
  if (output != -1):
    sc_0 = swlookup__resolvePrimitiveMethodName2__0.get((type + (11 * output)), 1)
    if (sc_0 == 0):
      return output
    else:
      return -1
  return -1

swlookup__resolvePrimitiveMethodName2__0 = { 82: 0, 104: 0, 148: 0, 214: 0, 225: 0, 280: 0, 291: 0, 302: 0, 335: 0, 346: 0, 357: 0, 368: 0, 6: 0, 39: 0, 50: 0, 61: 0, 72: 0, 83: 0, 116: 0, 160: 0, 193: 0, 237: 0, 259: 0, 270: 0, 292: 0, 314: 0, 325: 0, 51: 0, 62: 0, 84: 0, 128: 0, 205: 0, 249: 0, 271: 0, 381: 0, 20: 0, 31: 0, 141: 0, 174: 0, 98: 0, 142: 0, 186: 0 }

def resource_manager_getResourceOfType(vm, userPath, type):
  db = vm[9]
  lookup = db[1]
  if (userPath in lookup):
    output = makeEmptyList(None, 2)
    file = lookup[userPath]
    if file[3] == type:
      addToList(output, vm[13][1])
      addToList(output, buildString(vm[13], file[1]))
    else:
      addToList(output, vm[13][2])
    return [6, output]
  return vm[13][0]

def resource_manager_populate_directory_lookup(dirs, path):
  parts = path.split("/")
  pathBuilder = ""
  file = ""
  i = 0
  while (i < len(parts)):
    file = parts[i]
    files = None
    if not ((pathBuilder in dirs)):
      files = []
      dirs[pathBuilder] = files
    else:
      files = dirs[pathBuilder]
    files.append(file)
    if (i > 0):
      pathBuilder = ''.join([pathBuilder, "/", file])
    else:
      pathBuilder = file
    i += 1
  return 0

def resourceManagerInitialize(globals, manifest):
  filesPerDirectoryBuilder = {}
  fileInfo = {}
  dataList = []
  items = manifest.split("\n")
  resourceInfo = None
  type = ""
  userPath = ""
  internalPath = ""
  argument = ""
  isText = False
  intType = 0
  i = 0
  while (i < len(items)):
    itemData = items[i].split(",")
    if (len(itemData) >= 3):
      type = itemData[0]
      isText = "TXT" == type
      if isText:
        intType = 1
      elif ("IMGSH" == type or "IMG" == type):
        intType = 2
      elif "SND" == type:
        intType = 3
      elif "TTF" == type:
        intType = 4
      else:
        intType = 5
      userPath = stringDecode(itemData[1])
      internalPath = itemData[2]
      argument = ""
      if (len(itemData) > 3):
        argument = stringDecode(itemData[3])
      resourceInfo = [userPath, internalPath, isText, type, argument]
      fileInfo[userPath] = resourceInfo
      resource_manager_populate_directory_lookup(filesPerDirectoryBuilder, userPath)
      dataList.append(buildString(globals, userPath))
      dataList.append(buildInteger(globals, intType))
      if (internalPath != None):
        dataList.append(buildString(globals, internalPath))
      else:
        dataList.append(globals[0])
    i += 1
  dirs = list(filesPerDirectoryBuilder.keys())
  filesPerDirectorySorted = {}
  i = 0
  while (i < len(dirs)):
    dir = dirs[i]
    unsortedDirs = filesPerDirectoryBuilder[dir]
    dirsSorted = unsortedDirs[:]
    dirsSorted = PST_sortedCopyOfList(dirsSorted)
    filesPerDirectorySorted[dir] = dirsSorted
    i += 1
  return [filesPerDirectorySorted, fileInfo, dataList]

def reverseList(list):
  list[2].reverse()

def runInterpreter(vm, executionContextId):
  result = interpret(vm, executionContextId)
  result[3] = executionContextId
  status = result[0]
  if (status == 1):
    if (executionContextId in vm[0]):
      vm[0].pop(executionContextId)
    runShutdownHandlers(vm)
  elif (status == 3):
    printToStdOut(vm[11][3], result[1])
    runShutdownHandlers(vm)
  if (executionContextId == 0):
    result[4] = True
  return result

def runInterpreterWithFunctionPointer(vm, fpValue, args):
  newId = (vm[1] + 1)
  vm[1] = newId
  argList = []
  i = 0
  while (i < len(args)):
    argList.append(args[i])
    i += 1
  locals = []
  localsSet = []
  valueStack = (PST_NoneListOfOne * 100)
  valueStack[0] = fpValue
  valueStack[1] = buildList(argList)
  stack = [(len(vm[2][0]) - 2), 1, 0, 0, None, False, None, 0, 0, 1, 0, None, None, None]
  executionContext = [newId, stack, 2, 100, valueStack, locals, localsSet, 1, 0, False, None, False, 0, None]
  vm[0][newId] = executionContext
  return runInterpreter(vm, newId)

def runShutdownHandlers(vm):
  while (len(vm[10]) > 0):
    handler = vm[10][0]
    del vm[10][0]
    runInterpreterWithFunctionPointer(vm, handler, [])
  return 0

def setItemInList(list, i, v):
  list[2][i] = v

def sortHelperIsRevOrder(keyStringList, keyNumList, isString, indexLeft, indexRight):
  if isString:
    return (keyStringList[indexLeft] > keyStringList[indexRight])
  return (keyNumList[indexLeft] > keyNumList[indexRight])

def sortHelperSwap(keyStringList, keyNumList, indices, isString, index1, index2):
  if (index1 == index2):
    return 0
  t = indices[index1]
  indices[index1] = indices[index2]
  indices[index2] = t
  if isString:
    s = keyStringList[index1]
    keyStringList[index1] = keyStringList[index2]
    keyStringList[index2] = s
  else:
    n = keyNumList[index1]
    keyNumList[index1] = keyNumList[index2]
    keyNumList[index2] = n
  return 0

def sortLists(keyList, parallelList, intOutParam):
  PST_IntBuffer16[0] = 0
  length = keyList[1]
  if (length < 2):
    return 0
  i = 0
  item = None
  item = keyList[2][0]
  isString = (item[0] == 5)
  stringKeys = None
  numKeys = None
  if isString:
    stringKeys = (PST_NoneListOfOne * length)
  else:
    numKeys = (PST_NoneListOfOne * length)
  indices = (PST_NoneListOfOne * length)
  originalOrder = (PST_NoneListOfOne * length)
  i = 0
  while (i < length):
    indices[i] = i
    originalOrder[i] = parallelList[2][i]
    item = keyList[2][i]
    sc_0 = swlookup__sortLists__0.get(item[0], 3)
    if (sc_0 < 2):
      if (sc_0 == 0):
        if isString:
          PST_IntBuffer16[0] = 1
          return 0
        numKeys[i] = item[1]
      else:
        if isString:
          PST_IntBuffer16[0] = 1
          return 0
        numKeys[i] = item[1]
    elif (sc_0 == 2):
      if not (isString):
        PST_IntBuffer16[0] = 1
        return 0
      stringKeys[i] = item[1]
    else:
      PST_IntBuffer16[0] = 1
      return 0
    i += 1
  qsortHelper(stringKeys, numKeys, indices, isString, 0, (length - 1))
  i = 0
  while (i < length):
    parallelList[2][i] = originalOrder[indices[i]]
    i += 1
  return 0

swlookup__sortLists__0 = { 3: 0, 4: 1, 5: 2 }

def stackItemIsLibrary(stackInfo):
  if (stackInfo[0] != "["):
    return False
  cIndex = stackInfo.find(":")
  return ((cIndex > 0) and (cIndex < stackInfo.find("]")))

def startVm(vm):
  return runInterpreter(vm, vm[1])

def stringDecode(encoded):
  if not (("%" in encoded)):
    length = len(encoded)
    per = "%"
    builder = []
    i = 0
    while (i < length):
      c = encoded[i]
      if ((c == per) and ((i + 2) < length)):
        builder.append(stringFromHex(''.join(["", encoded[(i + 1)], encoded[(i + 2)]])))
      else:
        builder.append("" + c)
      i += 1
    return "".join(builder)
  return encoded

def stringFromHex(encoded):
  encoded = encoded.upper()
  hex = "0123456789ABCDEF"
  output = []
  length = len(encoded)
  a = 0
  b = 0
  c = None
  i = 0
  while ((i + 1) < length):
    c = "" + encoded[i]
    a = hex.find(c)
    if (a == -1):
      return None
    c = "" + encoded[(i + 1)]
    b = hex.find(c)
    if (b == -1):
      return None
    a = ((a * 16) + b)
    output.append(chr(a))
    i += 2
  return "".join(output)

def suspendInterpreter():
  return [2, None, 0.0, 0, False, ""]

def tokenDataImpl(vm, row):
  tokensByPc = vm[3][0]
  pc = (row[0] + vm[4][16])
  line = row[1]
  col = row[2]
  file = row[3]
  tokens = tokensByPc[pc]
  if (tokens == None):
    tokens = []
    tokensByPc[pc] = tokens
  tokens.append([line, col, file])
  return 0

def tokenHelperConvertPcsToStackTraceStrings(vm, pcs):
  tokens = generateTokenListFromPcs(vm, pcs)
  files = vm[3][1]
  output = []
  i = 0
  while (i < len(tokens)):
    token = tokens[i]
    if (token == None):
      output.append("[No stack information]")
    else:
      line = token[0]
      col = token[1]
      fileData = files[token[2]]
      lines = fileData.split("\n")
      filename = lines[0]
      linevalue = lines[(line + 1)]
      output.append(''.join([filename, ", Line: ", str((line + 1)), ", Col: ", str((col + 1))]))
    i += 1
  return output

def tokenHelperGetFileLine(vm, fileId, lineNum):
  sourceCode = vm[3][1][fileId]
  if (sourceCode == None):
    return None
  return sourceCode.split("\n")[lineNum]

def tokenHelperGetFormattedPointerToToken(vm, token):
  line = tokenHelperGetFileLine(vm, token[2], (token[0] + 1))
  if (line == None):
    return None
  columnIndex = token[1]
  lineLength = len(line)
  line = line.lstrip()
  line = line.replace("\t", " ")
  offset = (lineLength - len(line))
  columnIndex -= offset
  line2 = ""
  while (columnIndex > 0):
    columnIndex -= 1
    line2 = line2 + " "
  line2 = line2 + "^"
  return ''.join([line, "\n", line2])

def tokenHelplerIsFilePathLibrary(vm, fileId, allFiles):
  filename = tokenHelperGetFileLine(vm, fileId, 0)
  return not (filename.lower().endswith(".cry"))

def typeInfoToString(vm, typeInfo, i):
  output = []
  typeToStringBuilder(vm, output, typeInfo, i)
  return "".join(output)

def typeToString(vm, typeInfo, i):
  sb = []
  typeToStringBuilder(vm, sb, typeInfo, i)
  return "".join(sb)

def typeToStringBuilder(vm, sb, typeInfo, i):
  sc_0 = swlookup__typeToStringBuilder__0.get(typeInfo[i], 11)
  if (sc_0 < 6):
    if (sc_0 < 3):
      if (sc_0 == 0):
        sb.append("void")
        return (i + 1)
      elif (sc_0 == 1):
        sb.append("object")
        return (i + 1)
      else:
        sb.append("int")
        return (i + 1)
    elif (sc_0 == 3):
      sb.append("float")
      return (i + 1)
    elif (sc_0 == 4):
      sb.append("bool")
      return (i + 1)
    else:
      sb.append("string")
      return (i + 1)
  elif (sc_0 < 9):
    if (sc_0 == 6):
      sb.append("List<")
      i = typeToStringBuilder(vm, sb, typeInfo, (i + 1))
      sb.append(">")
      return i
    elif (sc_0 == 7):
      sb.append("Dictionary<")
      i = typeToStringBuilder(vm, sb, typeInfo, (i + 1))
      sb.append(", ")
      i = typeToStringBuilder(vm, sb, typeInfo, i)
      sb.append(">")
      return i
    else:
      classId = typeInfo[(i + 1)]
      if (classId == 0):
        sb.append("object")
      else:
        classInfo = vm[4][9][classId]
        sb.append(classInfo[16])
      return (i + 2)
  elif (sc_0 == 9):
    sb.append("Class")
    return (i + 1)
  elif (sc_0 == 10):
    n = typeInfo[(i + 1)]
    optCount = typeInfo[(i + 2)]
    i += 2
    sb.append("function(")
    ret = []
    i = typeToStringBuilder(vm, ret, typeInfo, i)
    j = 1
    while (j < n):
      if (j > 1):
        sb.append(", ")
      i = typeToStringBuilder(vm, sb, typeInfo, i)
      j += 1
    if (n == 1):
      sb.append("void")
    sb.append(" => ")
    optStart = (n - optCount - 1)
    j = 0
    while (j < len(ret)):
      if (j >= optStart):
        sb.append("(opt) ")
      sb.append(ret[j])
      j += 1
    sb.append(")")
    return i
  else:
    sb.append("UNKNOWN")
    return (i + 1)

swlookup__typeToStringBuilder__0 = { -1: 0, 0: 1, 1: 1, 3: 2, 4: 3, 2: 4, 5: 5, 6: 6, 7: 7, 8: 8, 10: 9, 9: 10 }

def typeToStringFromValue(vm, value):
  sb = None
  sc_0 = swlookup__typeToStringFromValue__0.get(value[0], 10)
  if (sc_0 < 6):
    if (sc_0 < 3):
      if (sc_0 == 0):
        return "null"
      elif (sc_0 == 1):
        return "bool"
      else:
        return "int"
    elif (sc_0 == 3):
      return "float"
    elif (sc_0 == 4):
      return "string"
    else:
      return "class"
  elif (sc_0 < 9):
    if (sc_0 == 6):
      classId = (value[1])[0]
      classInfo = vm[4][9][classId]
      return classInfo[16]
    elif (sc_0 == 7):
      sb = []
      sb.append("List<")
      list = value[1]
      if (list[0] == None):
        sb.append("object")
      else:
        typeToStringBuilder(vm, sb, list[0], 0)
      sb.append(">")
      return "".join(sb)
    else:
      dict = value[1]
      sb = []
      sb.append("Dictionary<")
      sc_1 = swlookup__typeToStringFromValue__1.get(dict[1], 3)
      if (sc_1 < 2):
        if (sc_1 == 0):
          sb.append("int")
        else:
          sb.append("string")
      elif (sc_1 == 2):
        sb.append("object")
      else:
        sb.append("???")
      sb.append(", ")
      if (dict[3] == None):
        sb.append("object")
      else:
        typeToStringBuilder(vm, sb, dict[3], 0)
      sb.append(">")
      return "".join(sb)
  elif (sc_0 == 9):
    return "Function"
  else:
    return "Unknown"

swlookup__typeToStringFromValue__1 = { 3: 0, 5: 1, 8: 2 }
swlookup__typeToStringFromValue__0 = { 1: 0, 2: 1, 3: 2, 4: 3, 5: 4, 10: 5, 8: 6, 6: 7, 7: 8, 9: 9 }

def uncaughtExceptionResult(vm, exception):
  return [3, unrollExceptionOutput(vm, exception), 0.0, 0, False, ""]

def unrollExceptionOutput(vm, exceptionInstance):
  objInstance = exceptionInstance[1]
  classInfo = vm[4][9][objInstance[0]]
  pcs = objInstance[3][1]
  codeFormattedPointer = ""
  exceptionName = classInfo[16]
  message = valueToString(vm, objInstance[2][1])
  trace = tokenHelperConvertPcsToStackTraceStrings(vm, pcs)
  trace.pop()
  trace.append("Stack Trace:")
  trace.reverse()
  pcs.reverse()
  showLibStack = vm[11][1]
  if (not (showLibStack) and not (stackItemIsLibrary(trace[0]))):
    while stackItemIsLibrary(trace[(len(trace) - 1)]):
      trace.pop()
      pcs.pop()
  tokensAtPc = vm[3][0][pcs[(len(pcs) - 1)]]
  if (tokensAtPc != None):
    codeFormattedPointer = "\n\n" + tokenHelperGetFormattedPointerToToken(vm, tokensAtPc[0])
  stackTrace = "\n".join(trace)
  return ''.join([stackTrace, codeFormattedPointer, "\n", exceptionName, ": ", message])

def valueConcatLists(a, b):
  return [None, (a[1] + b[1]), a[2] + b[2]]

def valueMultiplyList(a, n):
  _len = (a[1] * n)
  output = makeEmptyList(a[0], _len)
  if (_len == 0):
    return output
  aLen = a[1]
  i = 0
  value = None
  if (aLen == 1):
    value = a[2][0]
    i = 0
    while (i < n):
      output[2].append(value)
      i += 1
  else:
    j = 0
    i = 0
    while (i < n):
      j = 0
      while (j < aLen):
        output[2].append(a[2][j])
        j += 1
      i += 1
  output[1] = _len
  return output

def valueStackIncreaseCapacity(ec):
  stack = ec[4]
  oldCapacity = len(stack)
  newCapacity = (oldCapacity * 2)
  newStack = (PST_NoneListOfOne * newCapacity)
  i = (oldCapacity - 1)
  while (i >= 0):
    newStack[i] = stack[i]
    i -= 1
  ec[4] = newStack
  return newStack

def valueToString(vm, wrappedValue):
  type = wrappedValue[0]
  if (type == 1):
    return "null"
  if (type == 2):
    if wrappedValue[1]:
      return "true"
    return "false"
  if (type == 4):
    floatStr = str(wrappedValue[1])
    if not (("." in floatStr)):
      floatStr += ".0"
    return floatStr
  if (type == 3):
    return str(wrappedValue[1])
  if (type == 5):
    return wrappedValue[1]
  if (type == 6):
    internalList = wrappedValue[1]
    output = "["
    i = 0
    while (i < internalList[1]):
      if (i > 0):
        output += ", "
      output += valueToString(vm, internalList[2][i])
      i += 1
    output += "]"
    return output
  if (type == 8):
    objInstance = wrappedValue[1]
    classId = objInstance[0]
    ptr = objInstance[1]
    classInfo = vm[4][9][classId]
    nameId = classInfo[1]
    className = vm[4][0][nameId]
    return ''.join(["Instance<", className, "#", str(ptr), ">"])
  if (type == 7):
    dict = wrappedValue[1]
    if (dict[0] == 0):
      return "{}"
    output = "{"
    keyList = dict[6]
    valueList = dict[7]
    i = 0
    while (i < dict[0]):
      if (i > 0):
        output += ", "
      output += ''.join([valueToString(vm, dict[6][i]), ": ", valueToString(vm, dict[7][i])])
      i += 1
    output += " }"
    return output
  if (type == 9):
    fp = wrappedValue[1]
    sc_0 = swlookup__valueToString__0.get(fp[0], 5)
    if (sc_0 < 3):
      if (sc_0 == 0):
        return "<FunctionPointer>"
      elif (sc_0 == 1):
        return "<ClassMethodPointer>"
      else:
        return "<ClassStaticMethodPointer>"
    elif (sc_0 == 3):
      return "<PrimitiveMethodPointer>"
    elif (sc_0 == 4):
      return "<Lambda>"
    else:
      return "<UnknownFunctionPointer>"
  return "<unknown>"

swlookup__valueToString__0 = { 1: 0, 2: 1, 3: 2, 4: 3, 5: 4 }

def vm_getCurrentExecutionContextId(vm):
  return vm[1]

def vm_suspend(vm, status):
  return vm_suspend_for_context(getExecutionContext(vm, -1), 1)

def vm_suspend_for_context(ec, status):
  ec[11] = True
  ec[12] = status
  return 0

def vm_suspend_with_status(vm, status):
  return vm_suspend_for_context(getExecutionContext(vm, -1), status)

def vmEnvSetCommandLineArgs(vm, args):
  vm[11][0] = args

def vmGetGlobals(vm):
  return vm[13]
