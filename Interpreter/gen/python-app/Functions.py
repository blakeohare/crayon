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

def v_addLiteralImpl(v_vm, v_row, v_stringArg):
  v_g = v_vm[12]
  v_type = v_row[0]
  if (v_type == 1):
    v_vm[4][4].append(v_g[0])
  elif (v_type == 2):
    v_vm[4][4].append(v_buildBoolean(v_g, (v_row[1] == 1)))
  elif (v_type == 3):
    v_vm[4][4].append(v_buildInteger(v_g, v_row[1]))
  elif (v_type == 4):
    v_vm[4][4].append(v_buildFloat(v_g, float(v_stringArg)))
  elif (v_type == 5):
    v_vm[4][4].append(v_buildCommonString(v_g, v_stringArg))
  elif (v_type == 9):
    v_index = len(v_vm[4][4])
    v_vm[4][4].append(v_buildCommonString(v_g, v_stringArg))
    v_vm[4][20][v_stringArg] = v_index
  elif (v_type == 10):
    v_cv = [False, v_row[1]]
    v_vm[4][4].append([10, v_cv])
  return 0

def v_addNameImpl(v_vm, v_nameValue):
  v_index = len(v_vm[4][1])
  v_vm[4][2][v_nameValue] = v_index
  v_vm[4][1].append(v_nameValue)
  if "length" == v_nameValue:
    v_vm[4][14] = v_index
  return 0

def v_addToList(v_list, v_item):
  v_list[2].append(v_item)
  v_list[1] += 1

def v_applyDebugSymbolData(v_vm, v_opArgs, v_stringData, v_recentlyDefinedFunction):
  return 0

def v_buildBoolean(v_g, v_value):
  if v_value:
    return v_g[1]
  return v_g[2]

def v_buildCommonString(v_g, v_s):
  v_value = None
  v_value = v_g[11].get(v_s, None)
  if (v_value == None):
    v_value = v_buildString(v_g, v_s)
    v_g[11][v_s] = v_value
  return v_value

def v_buildFloat(v_g, v_value):
  if (v_value == 0.0):
    return v_g[6]
  if (v_value == 1.0):
    return v_g[7]
  return [4, v_value]

def v_buildInteger(v_g, v_num):
  if (v_num < 0):
    if (v_num > -257):
      return v_g[10][-v_num]
  elif (v_num < 2049):
    return v_g[9][v_num]
  return [3, v_num]

def v_buildList(v_valueList):
  return v_buildListWithType(None, v_valueList)

def v_buildListWithType(v_type, v_valueList):
  return [6, [v_type, len(v_valueList), v_valueList]]

def v_buildNull(v_globals):
  return v_globals[0]

def v_buildRelayObj(v_type, v_iarg1, v_iarg2, v_iarg3, v_farg1, v_sarg1):
  return [v_type, v_iarg1, v_iarg2, v_iarg3, v_farg1, v_sarg1]

def v_buildString(v_g, v_s):
  if (len(v_s) == 0):
    return v_g[8]
  return [5, v_s]

def v_buildStringDictionary(v_globals, v_stringKeys, v_values):
  v_size = len(v_stringKeys)
  v_d = [v_size, 5, 0, None, {}, {}, [], []]
  v_k = None
  v_i = 0
  while (v_i < v_size):
    v_k = v_stringKeys[v_i]
    if (v_k in v_d[5]):
      v_d[7][v_d[5][v_k]] = v_values[v_i]
    else:
      v_d[5][v_k] = len(v_d[7])
      v_d[7].append(v_values[v_i])
      v_d[6].append(v_buildString(v_globals, v_k))
    v_i += 1
  v_d[0] = len(v_d[7])
  return [7, v_d]

def v_canAssignGenericToGeneric(v_vm, v_gen1, v_gen1Index, v_gen2, v_gen2Index, v_newIndexOut):
  if (v_gen2 == None):
    return True
  if (v_gen1 == None):
    return False
  v_t1 = v_gen1[v_gen1Index]
  v_t2 = v_gen2[v_gen2Index]
  sc_0 = swlookup__canAssignGenericToGeneric__0.get(v_t1, 6)
  if (sc_0 < 4):
    if (sc_0 < 2):
      if (sc_0 == 0):
        v_newIndexOut[0] = (v_gen1Index + 1)
        v_newIndexOut[1] = (v_gen2Index + 2)
        return (v_t2 == v_t1)
      else:
        v_newIndexOut[0] = (v_gen1Index + 1)
        v_newIndexOut[1] = (v_gen2Index + 2)
        return ((v_t2 == 3) or (v_t2 == 4))
    elif (sc_0 == 2):
      v_newIndexOut[0] = (v_gen1Index + 1)
      v_newIndexOut[1] = (v_gen2Index + 2)
      if (v_t2 != 8):
        return False
      v_c1 = v_gen1[(v_gen1Index + 1)]
      v_c2 = v_gen2[(v_gen2Index + 1)]
      if (v_c1 == v_c2):
        return True
      return v_isClassASubclassOf(v_vm, v_c1, v_c2)
    else:
      if (v_t2 != 6):
        return False
      return v_canAssignGenericToGeneric(v_vm, v_gen1, (v_gen1Index + 1), v_gen2, (v_gen2Index + 1), v_newIndexOut)
  elif (sc_0 == 4):
    if (v_t2 != 7):
      return False
    if not (v_canAssignGenericToGeneric(v_vm, v_gen1, (v_gen1Index + 1), v_gen2, (v_gen2Index + 1), v_newIndexOut)):
      return False
    return v_canAssignGenericToGeneric(v_vm, v_gen1, v_newIndexOut[0], v_gen2, v_newIndexOut[1], v_newIndexOut)
  elif (sc_0 == 5):
    if (v_t2 != 9):
      return False
    return False
  else:
    return False

swlookup__canAssignGenericToGeneric__0 = { 0: 0, 1: 0, 2: 0, 4: 0, 5: 0, 10: 0, 3: 1, 8: 2, 6: 3, 7: 4, 9: 5 }

def v_canAssignTypeToGeneric(v_vm, v_value, v_generics, v_genericIndex):
  sc_0 = swlookup__canAssignTypeToGeneric__0.get(v_value[0], 7)
  if (sc_0 < 4):
    if (sc_0 < 2):
      if (sc_0 == 0):
        sc_1 = swlookup__canAssignTypeToGeneric__1.get(v_generics[v_genericIndex], 1)
        if (sc_1 == 0):
          return v_value
        return None
      else:
        if (v_generics[v_genericIndex] == v_value[0]):
          return v_value
        return None
    elif (sc_0 == 2):
      if (v_generics[v_genericIndex] == 3):
        return v_value
      if (v_generics[v_genericIndex] == 4):
        return v_buildFloat(v_vm[12], (0.0 + v_value[1]))
      return None
    else:
      if (v_generics[v_genericIndex] == 4):
        return v_value
      return None
  elif (sc_0 < 6):
    if (sc_0 == 4):
      v_list = v_value[1]
      v_listType = v_list[0]
      v_genericIndex += 1
      if (v_listType == None):
        if ((v_generics[v_genericIndex] == 1) or (v_generics[v_genericIndex] == 0)):
          return v_value
        return None
      v_i = 0
      while (v_i < len(v_listType)):
        if (v_listType[v_i] != v_generics[(v_genericIndex + v_i)]):
          return None
        v_i += 1
      return v_value
    else:
      v_dict = v_value[1]
      v_j = v_genericIndex
      sc_2 = swlookup__canAssignTypeToGeneric__2.get(v_dict[1], 2)
      if (sc_2 == 0):
        if (v_generics[1] == v_dict[1]):
          v_j += 2
        else:
          return None
      elif (sc_2 == 1):
        if (v_generics[1] == 8):
          v_j += 3
        else:
          return None
      v_valueType = v_dict[3]
      if (v_valueType == None):
        if ((v_generics[v_j] == 0) or (v_generics[v_j] == 1)):
          return v_value
        return None
      v_k = 0
      while (v_k < len(v_valueType)):
        if (v_valueType[v_k] != v_generics[(v_j + v_k)]):
          return None
        v_k += 1
      return v_value
  elif (sc_0 == 6):
    if (v_generics[v_genericIndex] == 8):
      v_targetClassId = v_generics[(v_genericIndex + 1)]
      v_givenClassId = (v_value[1])[0]
      if (v_targetClassId == v_givenClassId):
        return v_value
      if v_isClassASubclassOf(v_vm, v_givenClassId, v_targetClassId):
        return v_value
    return None
  return None

swlookup__canAssignTypeToGeneric__1 = { 5: 0, 8: 0, 10: 0, 9: 0, 6: 0, 7: 0 }
swlookup__canAssignTypeToGeneric__2 = { 3: 0, 5: 0, 8: 1 }
swlookup__canAssignTypeToGeneric__0 = { 1: 0, 2: 1, 5: 1, 10: 1, 3: 2, 4: 3, 6: 4, 7: 5, 8: 6 }

def v_canonicalizeAngle(v_a):
  v_twopi = 6.28318530717958
  v_a = (v_a % v_twopi)
  if (v_a < 0):
    v_a += v_twopi
  return v_a

def v_canonicalizeListSliceArgs(v_outParams, v_beginValue, v_endValue, v_beginIndex, v_endIndex, v_stepAmount, v_length, v_isForward):
  if (v_beginValue == None):
    if v_isForward:
      v_beginIndex = 0
    else:
      v_beginIndex = (v_length - 1)
  if (v_endValue == None):
    if v_isForward:
      v_endIndex = v_length
    else:
      v_endIndex = (-1 - v_length)
  if (v_beginIndex < 0):
    v_beginIndex += v_length
  if (v_endIndex < 0):
    v_endIndex += v_length
  if ((v_beginIndex == 0) and (v_endIndex == v_length) and (v_stepAmount == 1)):
    return 2
  if v_isForward:
    if (v_beginIndex >= v_length):
      return 0
    if (v_beginIndex < 0):
      return 3
    if (v_endIndex < v_beginIndex):
      return 4
    if (v_beginIndex == v_endIndex):
      return 0
    if (v_endIndex > v_length):
      v_endIndex = v_length
  else:
    if (v_beginIndex < 0):
      return 0
    if (v_beginIndex >= v_length):
      return 3
    if (v_endIndex > v_beginIndex):
      return 4
    if (v_beginIndex == v_endIndex):
      return 0
    if (v_endIndex < -1):
      v_endIndex = -1
  v_outParams[0] = v_beginIndex
  v_outParams[1] = v_endIndex
  return 1

def v_classIdToString(v_vm, v_classId):
  return v_vm[4][9][v_classId][16]

def v_clearList(v_a):
  if (v_a[1] == 1):
    v_a[2].pop()
  else:
    v_a[2] = []
  v_a[1] = 0
  return 0

def v_cloneDictionary(v_original, v_clone):
  v_type = v_original[1]
  v_i = 0
  v_size = v_original[0]
  v_kInt = 0
  v_kString = None
  if (v_clone == None):
    v_clone = [0, v_type, v_original[2], v_original[3], {}, {}, [], []]
    if (v_type == 5):
      while (v_i < v_size):
        v_clone[5][v_original[6][v_i][1]] = v_i
        v_i += 1
    else:
      while (v_i < v_size):
        if (v_type == 8):
          v_kInt = (v_original[6][v_i][1])[1]
        else:
          v_kInt = v_original[6][v_i][1]
        v_clone[4][v_kInt] = v_i
        v_i += 1
    v_i = 0
    while (v_i < v_size):
      v_clone[6].append(v_original[6][v_i])
      v_clone[7].append(v_original[7][v_i])
      v_i += 1
  else:
    v_i = 0
    while (v_i < v_size):
      if (v_type == 5):
        v_kString = v_original[6][v_i][1]
        if (v_kString in v_clone[5]):
          v_clone[7][v_clone[5][v_kString]] = v_original[7][v_i]
        else:
          v_clone[5][v_kString] = len(v_clone[7])
          v_clone[7].append(v_original[7][v_i])
          v_clone[6].append(v_original[6][v_i])
      else:
        if (v_type == 3):
          v_kInt = v_original[6][v_i][1]
        else:
          v_kInt = (v_original[6][v_i][1])[1]
        if (v_kInt in v_clone[4]):
          v_clone[7][v_clone[4][v_kInt]] = v_original[7][v_i]
        else:
          v_clone[4][v_kInt] = len(v_clone[7])
          v_clone[7].append(v_original[7][v_i])
          v_clone[6].append(v_original[6][v_i])
      v_i += 1
  v_clone[0] = (len(v_clone[4]) + len(v_clone[5]))
  return v_clone

def v_createInstanceType(v_classId):
  v_o = [None, None]
  v_o[0] = 8
  v_o[1] = v_classId
  return v_o

def v_createVm(v_rawByteCode, v_resourceManifest):
  v_globals = v_initializeConstantValues()
  v_resources = v_resourceManagerInitialize(v_globals, v_resourceManifest)
  v_byteCode = v_initializeByteCode(v_rawByteCode)
  v_localsStack = (PST_NoneListOfOne * 10)
  v_localsStackSet = (PST_NoneListOfOne * 10)
  v_i = 0
  v_i = (len(v_localsStack) - 1)
  while (v_i >= 0):
    v_localsStack[v_i] = None
    v_localsStackSet[v_i] = 0
    v_i -= 1
  v_stack = [0, 1, 0, 0, None, False, None, 0, 0, 1, 0, None, None, None]
  v_executionContext = [0, v_stack, 0, 100, (PST_NoneListOfOne * 100), v_localsStack, v_localsStackSet, 1, 0, False, None, False, 0, None]
  v_executionContexts = {}
  v_executionContexts[0] = v_executionContext
  v_vm = [v_executionContexts, v_executionContext[0], v_byteCode, [(PST_NoneListOfOne * len(v_byteCode[0])), None, [], None, None, {}, {}], [None, [], {}, None, [], None, [], None, [], (PST_NoneListOfOne * 100), (PST_NoneListOfOne * 100), {}, None, {}, -1, (PST_NoneListOfOne * 10), 0, None, None, [0, 0, 0], {}, {}, None], 0, False, [], None, v_resources, [], [[], False, None, None], v_globals, v_globals[0], v_globals[1], v_globals[2]]
  return v_vm

def v_debuggerClearBreakpoint(v_vm, v_id):
  return 0

def v_debuggerFindPcForLine(v_vm, v_path, v_line):
  return -1

def v_debuggerSetBreakpoint(v_vm, v_path, v_line):
  return -1

def v_debugSetStepOverBreakpoint(v_vm):
  return False

def v_defOriginalCodeImpl(v_vm, v_row, v_fileContents):
  v_fileId = v_row[0]
  v_codeLookup = v_vm[3][2]
  while (len(v_codeLookup) <= v_fileId):
    v_codeLookup.append(None)
  v_codeLookup[v_fileId] = v_fileContents
  return 0

def v_dictKeyInfoToString(v_vm, v_dict):
  if (v_dict[1] == 5):
    return "string"
  if (v_dict[1] == 3):
    return "int"
  if (v_dict[2] == 0):
    return "instance"
  return v_classIdToString(v_vm, v_dict[2])

def v_doEqualityComparisonAndReturnCode(v_a, v_b):
  v_leftType = v_a[0]
  v_rightType = v_b[0]
  if (v_leftType == v_rightType):
    if (v_leftType < 6):
      if (v_a[1] == v_b[1]):
        return 1
      return 0
    v_output = 0
    sc_0 = swlookup__doEqualityComparisonAndReturnCode__0.get(v_leftType, 8)
    if (sc_0 < 5):
      if (sc_0 < 3):
        if (sc_0 == 0):
          v_output = 1
        elif (sc_0 == 1):
          if (v_a[1] == v_b[1]):
            v_output = 1
        elif (v_a[1] == v_b[1]):
          v_output = 1
      elif (sc_0 == 3):
        if (v_a[1] == v_b[1]):
          v_output = 1
      elif (v_a[1] == v_b[1]):
        v_output = 1
    elif (sc_0 < 7):
      if (sc_0 == 5):
        if v_a[1] is v_b[1]:
          v_output = 1
      else:
        v_f1 = v_a[1]
        v_f2 = v_b[1]
        if (v_f1[3] == v_f2[3]):
          if ((v_f1[0] == 2) or (v_f1[0] == 4)):
            if (v_doEqualityComparisonAndReturnCode(v_f1[1], v_f2[1]) == 1):
              v_output = 1
          else:
            v_output = 1
    elif (sc_0 == 7):
      v_c1 = v_a[1]
      v_c2 = v_b[1]
      if (v_c1[1] == v_c2[1]):
        v_output = 1
    else:
      v_output = 2
    return v_output
  if (v_rightType == 1):
    return 0
  if ((v_leftType == 3) and (v_rightType == 4)):
    if (v_a[1] == v_b[1]):
      return 1
  elif ((v_leftType == 4) and (v_rightType == 3)):
    if (v_a[1] == v_b[1]):
      return 1
  return 0

swlookup__doEqualityComparisonAndReturnCode__0 = { 1: 0, 3: 1, 4: 2, 2: 3, 5: 4, 6: 5, 7: 5, 8: 5, 9: 6, 10: 7 }

def v_encodeBreakpointData(v_vm, v_breakpoint, v_pc):
  return None

def v_errorResult(v_error):
  return [3, v_error, 0.0, 0, False, ""]

def v_EX_AssertionFailed(v_ec, v_exMsg):
  return v_generateException2(v_ec, 2, v_exMsg)

def v_EX_DivisionByZero(v_ec, v_exMsg):
  return v_generateException2(v_ec, 3, v_exMsg)

def v_EX_Fatal(v_ec, v_exMsg):
  return v_generateException2(v_ec, 0, v_exMsg)

def v_EX_IndexOutOfRange(v_ec, v_exMsg):
  return v_generateException2(v_ec, 4, v_exMsg)

def v_EX_InvalidArgument(v_ec, v_exMsg):
  return v_generateException2(v_ec, 5, v_exMsg)

def v_EX_InvalidAssignment(v_ec, v_exMsg):
  return v_generateException2(v_ec, 6, v_exMsg)

def v_EX_InvalidInvocation(v_ec, v_exMsg):
  return v_generateException2(v_ec, 7, v_exMsg)

def v_EX_InvalidKey(v_ec, v_exMsg):
  return v_generateException2(v_ec, 8, v_exMsg)

def v_EX_KeyNotFound(v_ec, v_exMsg):
  return v_generateException2(v_ec, 9, v_exMsg)

def v_EX_NullReference(v_ec, v_exMsg):
  return v_generateException2(v_ec, 10, v_exMsg)

def v_EX_UnassignedVariable(v_ec, v_exMsg):
  return v_generateException2(v_ec, 11, v_exMsg)

def v_EX_UnknownField(v_ec, v_exMsg):
  return v_generateException2(v_ec, 12, v_exMsg)

def v_EX_UnsupportedOperation(v_ec, v_exMsg):
  return v_generateException2(v_ec, 13, v_exMsg)

def v_finalizeInitializationImpl(v_vm, v_projectId, v_localeCount):
  v_vm[3][1] = v_vm[3][2][:]
  v_vm[3][2] = None
  v_vm[4][19][2] = v_localeCount
  v_vm[4][0] = v_vm[4][1][:]
  v_vm[4][3] = v_vm[4][4][:]
  v_vm[4][12] = v_primitiveMethodsInitializeLookup(v_vm[4][2])
  v_vm[8] = (PST_NoneListOfOne * len(v_vm[4][0]))
  v_vm[4][17] = v_projectId
  v_vm[4][1] = None
  v_vm[4][4] = None
  v_vm[6] = True
  return 0

def v_fixFuzzyFloatPrecision(v_x):
  if ((v_x % 1) != 0):
    v_u = (v_x % 1)
    if (v_u < 0):
      v_u += 1.0
    v_roundDown = False
    if (v_u > 0.9999999999):
      v_roundDown = True
      v_x += 0.1
    elif (v_u < 0.00000000002250000000):
      v_roundDown = True
    if v_roundDown:
      if (False or (v_x > 0)):
        v_x = (int(v_x) + 0.0)
      else:
        v_x = (int(v_x) - 1.0)
  return v_x

def v_generateEsfData(v_byteCodeLength, v_esfArgs):
  v_output = (PST_NoneListOfOne * v_byteCodeLength)
  v_esfTokenStack = []
  v_esfTokenStackTop = None
  v_esfArgIterator = 0
  v_esfArgLength = len(v_esfArgs)
  v_j = 0
  v_pc = 0
  while (v_pc < v_byteCodeLength):
    if ((v_esfArgIterator < v_esfArgLength) and (v_pc == v_esfArgs[v_esfArgIterator])):
      v_esfTokenStackTop = [None, None]
      v_j = 1
      while (v_j < 3):
        v_esfTokenStackTop[(v_j - 1)] = v_esfArgs[(v_esfArgIterator + v_j)]
        v_j += 1
      v_esfTokenStack.append(v_esfTokenStackTop)
      v_esfArgIterator += 3
    while ((v_esfTokenStackTop != None) and (v_esfTokenStackTop[1] <= v_pc)):
      v_esfTokenStack.pop()
      if (len(v_esfTokenStack) == 0):
        v_esfTokenStackTop = None
      else:
        v_esfTokenStackTop = v_esfTokenStack[(len(v_esfTokenStack) - 1)]
    v_output[v_pc] = v_esfTokenStackTop
    v_pc += 1
  return v_output

def v_generateException(v_vm, v_stack, v_pc, v_valueStackSize, v_ec, v_type, v_message):
  v_ec[2] = v_valueStackSize
  v_stack[0] = v_pc
  v_mn = v_vm[4][19]
  v_generateExceptionFunctionId = v_mn[1]
  v_functionInfo = v_vm[4][10][v_generateExceptionFunctionId]
  v_pc = v_functionInfo[2]
  if (len(v_ec[5]) <= (v_functionInfo[7] + v_stack[3])):
    v_increaseLocalsStackCapacity(v_ec, v_functionInfo[7])
  v_localsIndex = v_stack[3]
  v_localsStackSetToken = (v_ec[7] + 1)
  v_ec[7] = v_localsStackSetToken
  v_ec[5][v_localsIndex] = v_buildInteger(v_vm[12], v_type)
  v_ec[5][(v_localsIndex + 1)] = v_buildString(v_vm[12], v_message)
  v_ec[6][v_localsIndex] = v_localsStackSetToken
  v_ec[6][(v_localsIndex + 1)] = v_localsStackSetToken
  v_ec[1] = [(v_pc + 1), v_localsStackSetToken, v_stack[3], (v_stack[3] + v_functionInfo[7]), v_stack, False, None, v_valueStackSize, 0, (v_stack[9] + 1), 0, None, None, None]
  return [5, None, 0.0, 0, False, ""]

def v_generateException2(v_ec, v_exceptionType, v_exMsg):
  v_ec[13] = [1, v_exceptionType, v_exMsg, 0.0, None]
  return True

def v_generatePrimitiveMethodReference(v_lookup, v_globalNameId, v_context):
  v_functionId = v_resolvePrimitiveMethodName2(v_lookup, v_context[0], v_globalNameId)
  if (v_functionId < 0):
    return None
  return [9, [4, v_context, 0, v_functionId, None]]

def v_generateTokenListFromPcs(v_vm, v_pcs):
  v_output = []
  v_tokensByPc = v_vm[3][0]
  v_token = None
  v_i = 0
  while (v_i < len(v_pcs)):
    v_localTokens = v_tokensByPc[v_pcs[v_i]]
    if (v_localTokens == None):
      if (len(v_output) > 0):
        v_output.append(None)
    else:
      v_token = v_localTokens[0]
      v_output.append(v_token)
    v_i += 1
  return v_output

def v_getBinaryOpFromId(v_id):
  sc_0 = swlookup__getBinaryOpFromId__0.get(v_id, 15)
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

def v_getClassTable(v_vm, v_classId):
  v_oldTable = v_vm[4][9]
  v_oldLength = len(v_oldTable)
  if (v_classId < v_oldLength):
    return v_oldTable
  v_newLength = (v_oldLength * 2)
  if (v_classId >= v_newLength):
    v_newLength = (v_classId + 100)
  v_newTable = (PST_NoneListOfOne * v_newLength)
  v_i = (v_oldLength - 1)
  while (v_i >= 0):
    v_newTable[v_i] = v_oldTable[v_i]
    v_i -= 1
  v_vm[4][9] = v_newTable
  return v_newTable

def v_getExecutionContext(v_vm, v_id):
  if (v_id == -1):
    v_id = v_vm[1]
  if (v_id in v_vm[0]):
    return v_vm[0][v_id]
  return None

def v_getFloat(v_num):
  if (v_num[0] == 4):
    return v_num[1]
  return (v_num[1] + 0.0)

def v_getFunctionTable(v_vm, v_functionId):
  v_oldTable = v_vm[4][10]
  v_oldLength = len(v_oldTable)
  if (v_functionId < v_oldLength):
    return v_oldTable
  v_newLength = (v_oldLength * 2)
  if (v_functionId >= v_newLength):
    v_newLength = (v_functionId + 100)
  v_newTable = (PST_NoneListOfOne * v_newLength)
  v_i = 0
  while (v_i < v_oldLength):
    v_newTable[v_i] = v_oldTable[v_i]
    v_i += 1
  v_vm[4][10] = v_newTable
  return v_newTable

def v_getItemFromList(v_list, v_i):
  return v_list[2][v_i]

def v_getNativeDataItem(v_objValue, v_index):
  v_obj = v_objValue[1]
  return v_obj[3][v_index]

def v_getTypeFromId(v_id):
  sc_0 = swlookup__getTypeFromId__0.get(v_id, 9)
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

def v_getVmReinvokeDelay(v_result):
  return v_result[2]

def v_getVmResultAssemblyInfo(v_result):
  return v_result[5]

def v_getVmResultExecId(v_result):
  return v_result[3]

def v_getVmResultStatus(v_result):
  return v_result[0]

def v_increaseListCapacity(v_list):
  pass

def v_increaseLocalsStackCapacity(v_ec, v_newScopeSize):
  v_oldLocals = v_ec[5]
  v_oldSetIndicator = v_ec[6]
  v_oldCapacity = len(v_oldLocals)
  v_newCapacity = ((v_oldCapacity * 2) + v_newScopeSize)
  v_newLocals = (PST_NoneListOfOne * v_newCapacity)
  v_newSetIndicator = (PST_NoneListOfOne * v_newCapacity)
  v_i = 0
  while (v_i < v_oldCapacity):
    v_newLocals[v_i] = v_oldLocals[v_i]
    v_newSetIndicator[v_i] = v_oldSetIndicator[v_i]
    v_i += 1
  v_ec[5] = v_newLocals
  v_ec[6] = v_newSetIndicator
  return 0

def v_initFileNameSymbolData(v_vm):
  v_symbolData = v_vm[3]
  if (v_symbolData == None):
    return 0
  if (v_symbolData[3] == None):
    v_i = 0
    v_filenames = (PST_NoneListOfOne * len(v_symbolData[1]))
    v_fileIdByPath = {}
    v_i = 0
    while (v_i < len(v_filenames)):
      v_sourceCode = v_symbolData[1][v_i]
      if (v_sourceCode != None):
        v_colon = v_sourceCode.find("\n")
        if (v_colon != -1):
          v_filename = v_sourceCode[0:0 + v_colon]
          v_filenames[v_i] = v_filename
          v_fileIdByPath[v_filename] = v_i
      v_i += 1
    v_symbolData[3] = v_filenames
    v_symbolData[4] = v_fileIdByPath
  return 0

def v_initializeByteCode(v_raw):
  v_index = [None]
  v_index[0] = 0
  v_length = len(v_raw)
  v_header = v_read_till(v_index, v_raw, v_length, "@")
  if (v_header != "CRAYON"):
    pass
  v_alphaNums = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz"
  v_opCount = v_read_integer(v_index, v_raw, v_length, v_alphaNums)
  v_ops = (PST_NoneListOfOne * v_opCount)
  v_iargs = (PST_NoneListOfOne * v_opCount)
  v_sargs = (PST_NoneListOfOne * v_opCount)
  v_c = " "
  v_argc = 0
  v_j = 0
  v_stringarg = None
  v_stringPresent = False
  v_iarg = 0
  v_iarglist = None
  v_i = 0
  v_i = 0
  while (v_i < v_opCount):
    v_c = v_raw[v_index[0]]
    v_index[0] = (v_index[0] + 1)
    v_argc = 0
    v_stringPresent = True
    if (v_c == "!"):
      v_argc = 1
    elif (v_c == "&"):
      v_argc = 2
    elif (v_c == "*"):
      v_argc = 3
    else:
      if (v_c != "~"):
        v_stringPresent = False
        v_index[0] = (v_index[0] - 1)
      v_argc = v_read_integer(v_index, v_raw, v_length, v_alphaNums)
    v_iarglist = (PST_NoneListOfOne * (v_argc - 1))
    v_j = 0
    while (v_j < v_argc):
      v_iarg = v_read_integer(v_index, v_raw, v_length, v_alphaNums)
      if (v_j == 0):
        v_ops[v_i] = v_iarg
      else:
        v_iarglist[(v_j - 1)] = v_iarg
      v_j += 1
    v_iargs[v_i] = v_iarglist
    if v_stringPresent:
      v_stringarg = v_read_string(v_index, v_raw, v_length, v_alphaNums)
    else:
      v_stringarg = None
    v_sargs[v_i] = v_stringarg
    v_i += 1
  v_hasBreakpoint = (PST_NoneListOfOne * v_opCount)
  v_breakpointInfo = (PST_NoneListOfOne * v_opCount)
  v_i = 0
  while (v_i < v_opCount):
    v_hasBreakpoint[v_i] = False
    v_breakpointInfo[v_i] = None
    v_i += 1
  return [v_ops, v_iargs, v_sargs, (PST_NoneListOfOne * v_opCount), (PST_NoneListOfOne * v_opCount), [v_hasBreakpoint, v_breakpointInfo, {}, 1, 0]]

def v_initializeClass(v_pc, v_vm, v_args, v_className):
  v_i = 0
  v_memberId = 0
  v_globalId = 0
  v_functionId = 0
  v_t = 0
  v_classId = v_args[0]
  v_baseClassId = v_args[1]
  v_globalNameId = v_args[2]
  v_constructorFunctionId = v_args[3]
  v_staticConstructorFunctionId = v_args[4]
  v_staticInitializationState = 0
  if (v_staticConstructorFunctionId == -1):
    v_staticInitializationState = 2
  v_staticFieldCount = v_args[5]
  v_assemblyId = v_args[6]
  v_staticFields = (PST_NoneListOfOne * v_staticFieldCount)
  v_i = 0
  while (v_i < v_staticFieldCount):
    v_staticFields[v_i] = v_vm[12][0]
    v_i += 1
  v_classInfo = [v_classId, v_globalNameId, v_baseClassId, v_assemblyId, v_staticInitializationState, v_staticFields, v_staticConstructorFunctionId, v_constructorFunctionId, 0, None, None, None, None, None, v_vm[4][21][v_classId], None, v_className]
  v_classTable = v_getClassTable(v_vm, v_classId)
  v_classTable[v_classId] = v_classInfo
  v_classChain = []
  v_classChain.append(v_classInfo)
  v_classIdWalker = v_baseClassId
  while (v_classIdWalker != -1):
    v_walkerClass = v_classTable[v_classIdWalker]
    v_classChain.append(v_walkerClass)
    v_classIdWalker = v_walkerClass[2]
  v_baseClass = None
  if (v_baseClassId != -1):
    v_baseClass = v_classChain[1]
  v_functionIds = []
  v_fieldInitializationCommand = []
  v_fieldInitializationLiteral = []
  v_fieldAccessModifier = []
  v_globalNameIdToMemberId = {}
  if (v_baseClass != None):
    v_i = 0
    while (v_i < v_baseClass[8]):
      v_functionIds.append(v_baseClass[9][v_i])
      v_fieldInitializationCommand.append(v_baseClass[10][v_i])
      v_fieldInitializationLiteral.append(v_baseClass[11][v_i])
      v_fieldAccessModifier.append(v_baseClass[12][v_i])
      v_i += 1
    v_keys = list(v_baseClass[13].keys())
    v_i = 0
    while (v_i < len(v_keys)):
      v_t = v_keys[v_i]
      v_globalNameIdToMemberId[v_t] = v_baseClass[13][v_t]
      v_i += 1
    v_keys = list(v_baseClass[14].keys())
    v_i = 0
    while (v_i < len(v_keys)):
      v_t = v_keys[v_i]
      v_classInfo[14][v_t] = v_baseClass[14][v_t]
      v_i += 1
  v_accessModifier = 0
  v_i = 7
  while (v_i < len(v_args)):
    v_memberId = v_args[(v_i + 1)]
    v_globalId = v_args[(v_i + 2)]
    v_accessModifier = v_args[(v_i + 5)]
    while (v_memberId >= len(v_functionIds)):
      v_functionIds.append(-1)
      v_fieldInitializationCommand.append(-1)
      v_fieldInitializationLiteral.append(None)
      v_fieldAccessModifier.append(0)
    v_globalNameIdToMemberId[v_globalId] = v_memberId
    v_fieldAccessModifier[v_memberId] = v_accessModifier
    if (v_args[v_i] == 0):
      v_fieldInitializationCommand[v_memberId] = v_args[(v_i + 3)]
      v_t = v_args[(v_i + 4)]
      if (v_t == -1):
        v_fieldInitializationLiteral[v_memberId] = v_vm[12][0]
      else:
        v_fieldInitializationLiteral[v_memberId] = v_vm[4][3][v_t]
    else:
      v_functionId = v_args[(v_i + 3)]
      v_functionIds[v_memberId] = v_functionId
    v_i += 6
  v_classInfo[9] = v_functionIds[:]
  v_classInfo[10] = v_fieldInitializationCommand[:]
  v_classInfo[11] = v_fieldInitializationLiteral[:]
  v_classInfo[12] = v_fieldAccessModifier[:]
  v_classInfo[8] = len(v_functionIds)
  v_classInfo[13] = v_globalNameIdToMemberId
  v_classInfo[15] = (PST_NoneListOfOne * v_classInfo[8])
  if (v_baseClass != None):
    v_i = 0
    while (v_i < len(v_baseClass[15])):
      v_classInfo[15][v_i] = v_baseClass[15][v_i]
      v_i += 1
  if "Core.Exception" == v_className:
    v_mn = v_vm[4][19]
    v_mn[0] = v_classId
  return 0

def v_initializeClassFieldTypeInfo(v_vm, v_opCodeRow):
  v_classInfo = v_vm[4][9][v_opCodeRow[0]]
  v_memberId = v_opCodeRow[1]
  v_len = len(v_opCodeRow)
  v_typeInfo = (PST_NoneListOfOne * (v_len - 2))
  v_i = 2
  while (v_i < v_len):
    v_typeInfo[(v_i - 2)] = v_opCodeRow[v_i]
    v_i += 1
  v_classInfo[15][v_memberId] = v_typeInfo
  return 0

def v_initializeConstantValues():
  v_pos = (PST_NoneListOfOne * 2049)
  v_neg = (PST_NoneListOfOne * 257)
  v_i = 0
  while (v_i < 2049):
    v_pos[v_i] = [3, v_i]
    v_i += 1
  v_i = 1
  while (v_i < 257):
    v_neg[v_i] = [3, -v_i]
    v_i += 1
  v_neg[0] = v_pos[0]
  v_globals = [[1, None], [2, True], [2, False], v_pos[0], v_pos[1], v_neg[1], [4, 0.0], [4, 1.0], [5, ""], v_pos, v_neg, {}, [None], [None], [None], [None], [None], [None, None]]
  v_globals[11][""] = v_globals[8]
  v_globals[12][0] = 2
  v_globals[13][0] = 3
  v_globals[15][0] = 4
  v_globals[14][0] = 5
  v_globals[16][0] = 10
  v_globals[17][0] = 8
  v_globals[17][1] = 0
  return v_globals

def v_initializeFunction(v_vm, v_args, v_currentPc, v_stringArg):
  v_functionId = v_args[0]
  v_nameId = v_args[1]
  v_minArgCount = v_args[2]
  v_maxArgCount = v_args[3]
  v_functionType = v_args[4]
  v_classId = v_args[5]
  v_localsCount = v_args[6]
  v_numPcOffsetsForOptionalArgs = v_args[8]
  v_pcOffsetsForOptionalArgs = (PST_NoneListOfOne * (v_numPcOffsetsForOptionalArgs + 1))
  v_i = 0
  while (v_i < v_numPcOffsetsForOptionalArgs):
    v_pcOffsetsForOptionalArgs[(v_i + 1)] = v_args[(9 + v_i)]
    v_i += 1
  v_functionTable = v_getFunctionTable(v_vm, v_functionId)
  v_functionTable[v_functionId] = [v_functionId, v_nameId, v_currentPc, v_minArgCount, v_maxArgCount, v_functionType, v_classId, v_localsCount, v_pcOffsetsForOptionalArgs, v_stringArg, None]
  v_vm[4][22] = v_functionTable[v_functionId]
  if (v_nameId >= 0):
    v_name = v_vm[4][0][v_nameId]
    if "_LIB_CORE_list_filter" == v_name:
      v_vm[4][15][0] = v_functionId
    elif "_LIB_CORE_list_map" == v_name:
      v_vm[4][15][1] = v_functionId
    elif "_LIB_CORE_list_sort_by_key" == v_name:
      v_vm[4][15][2] = v_functionId
    elif "_LIB_CORE_invoke" == v_name:
      v_vm[4][15][3] = v_functionId
    elif "_LIB_CORE_generateException" == v_name:
      v_mn = v_vm[4][19]
      v_mn[1] = v_functionId
  return 0

def v_initializeIntSwitchStatement(v_vm, v_pc, v_args):
  v_output = {}
  v_i = 1
  while (v_i < len(v_args)):
    v_output[v_args[v_i]] = v_args[(v_i + 1)]
    v_i += 2
  v_vm[2][3][v_pc] = v_output
  return v_output

def v_initializeStringSwitchStatement(v_vm, v_pc, v_args):
  v_output = {}
  v_i = 1
  while (v_i < len(v_args)):
    v_s = v_vm[4][3][v_args[v_i]][1]
    v_output[v_s] = v_args[(v_i + 1)]
    v_i += 2
  v_vm[2][4][v_pc] = v_output
  return v_output

def v_initLocTable(v_vm, v_row):
  v_classId = v_row[0]
  v_memberCount = v_row[1]
  v_nameId = 0
  v_totalLocales = v_vm[4][19][2]
  v_lookup = {}
  v_i = 2
  while (v_i < len(v_row)):
    v_localeId = v_row[v_i]
    v_i += 1
    v_j = 0
    while (v_j < v_memberCount):
      v_nameId = v_row[(v_i + v_j)]
      if (v_nameId != -1):
        v_lookup[((v_nameId * v_totalLocales) + v_localeId)] = v_j
      v_j += 1
    v_i += v_memberCount
  v_vm[4][21][v_classId] = v_lookup
  return 0

def v_interpret(v_vm, v_executionContextId):
  v_output = v_interpretImpl(v_vm, v_executionContextId)
  while ((v_output[0] == 5) and (v_output[2] == 0)):
    v_output = v_interpretImpl(v_vm, v_executionContextId)
  return v_output

def v_interpreterFinished(v_vm, v_ec):
  if (v_ec != None):
    v_id = v_ec[0]
    if (v_id in v_vm[0]):
      v_vm[0].pop(v_id)
  return [1, None, 0.0, 0, False, ""]

def v_interpreterGetExecutionContext(v_vm, v_executionContextId):
  v_executionContexts = v_vm[0]
  if not ((v_executionContextId in v_executionContexts)):
    return None
  return v_executionContexts[v_executionContextId]

def v_interpretImpl(v_vm, v_executionContextId):
  v_metadata = v_vm[4]
  v_globals = v_vm[12]
  v_VALUE_NULL = v_globals[0]
  v_VALUE_TRUE = v_globals[1]
  v_VALUE_FALSE = v_globals[2]
  v_VALUE_INT_ONE = v_globals[4]
  v_VALUE_INT_ZERO = v_globals[3]
  v_VALUE_FLOAT_ZERO = v_globals[6]
  v_VALUE_FLOAT_ONE = v_globals[7]
  v_INTEGER_POSITIVE_CACHE = v_globals[9]
  v_INTEGER_NEGATIVE_CACHE = v_globals[10]
  v_executionContexts = v_vm[0]
  v_ec = v_interpreterGetExecutionContext(v_vm, v_executionContextId)
  if (v_ec == None):
    return v_interpreterFinished(v_vm, None)
  v_ec[8] += 1
  v_stack = v_ec[1]
  v_ops = v_vm[2][0]
  v_args = v_vm[2][1]
  v_stringArgs = v_vm[2][2]
  v_classTable = v_vm[4][9]
  v_functionTable = v_vm[4][10]
  v_literalTable = v_vm[4][3]
  v_identifiers = v_vm[4][0]
  v_valueStack = v_ec[4]
  v_valueStackSize = v_ec[2]
  v_valueStackCapacity = len(v_valueStack)
  v_hasInterrupt = False
  v_type = 0
  v_nameId = 0
  v_classId = 0
  v_functionId = 0
  v_localeId = 0
  v_classInfo = None
  v_len = 0
  v_root = None
  v_row = None
  v_argCount = 0
  v_stringList = None
  v_returnValueUsed = False
  v_output = None
  v_functionInfo = None
  v_keyType = 0
  v_intKey = 0
  v_stringKey = None
  v_first = False
  v_primitiveMethodToCoreLibraryFallback = False
  v_bool1 = False
  v_bool2 = False
  v_staticConstructorNotInvoked = True
  v_int1 = 0
  v_int2 = 0
  v_int3 = 0
  v_i = 0
  v_j = 0
  v_float1 = 0.0
  v_float2 = 0.0
  v_float3 = 0.0
  v_floatList1 = [None, None]
  v_value = None
  v_value2 = None
  v_value3 = None
  v_string1 = None
  v_string2 = None
  v_objInstance1 = None
  v_objInstance2 = None
  v_list1 = None
  v_list2 = None
  v_valueList1 = None
  v_valueList2 = None
  v_dictImpl = None
  v_dictImpl2 = None
  v_stringList1 = None
  v_intList1 = None
  v_valueArray1 = None
  v_intArray1 = None
  v_intArray2 = None
  v_objArray1 = None
  v_functionPointer1 = None
  v_intIntDict1 = None
  v_stringIntDict1 = None
  v_stackFrame2 = None
  v_leftValue = None
  v_rightValue = None
  v_classValue = None
  v_arg1 = None
  v_arg2 = None
  v_arg3 = None
  v_tokenList = None
  v_globalNameIdToPrimitiveMethodName = v_vm[4][12]
  v_magicNumbers = v_vm[4][19]
  v_integerSwitchesByPc = v_vm[2][3]
  v_stringSwitchesByPc = v_vm[2][4]
  v_integerSwitch = None
  v_stringSwitch = None
  v_esfData = v_vm[4][18]
  v_closure = None
  v_parentClosure = None
  v_intBuffer = (PST_NoneListOfOne * 16)
  v_localsStack = v_ec[5]
  v_localsStackSet = v_ec[6]
  v_localsStackSetToken = v_stack[1]
  v_localsStackCapacity = len(v_localsStack)
  v_localsStackOffset = v_stack[2]
  v_funcArgs = v_vm[8]
  v_pc = v_stack[0]
  v_nativeFp = None
  v_debugData = v_vm[2][5]
  v_isBreakPointPresent = v_debugData[0]
  v_breakpointInfo = None
  v_debugBreakPointTemporaryDisable = False
  while True:
    v_row = v_args[v_pc]
    sc_0 = swlookup__interpretImpl__0.get(v_ops[v_pc], 66)
    if (sc_0 < 34):
      if (sc_0 < 17):
        if (sc_0 < 9):
          if (sc_0 < 5):
            if (sc_0 < 3):
              if (sc_0 == 0):
                # ADD_LITERAL
                v_addLiteralImpl(v_vm, v_row, v_stringArgs[v_pc])
              elif (sc_0 == 1):
                # ADD_NAME
                v_addNameImpl(v_vm, v_stringArgs[v_pc])
              else:
                # ARG_TYPE_VERIFY
                v_len = v_row[0]
                v_i = 1
                v_j = 0
                while (v_j < v_len):
                  v_j += 1
            elif (sc_0 == 3):
              # ASSIGN_CLOSURE
              v_valueStackSize -= 1
              v_value = v_valueStack[v_valueStackSize]
              v_i = v_row[0]
              if (v_stack[12] == None):
                v_closure = {}
                v_stack[12] = v_closure
                v_closure[v_i] = [v_value]
              else:
                v_closure = v_stack[12]
                if (v_i in v_closure):
                  v_closure[v_i][0] = v_value
                else:
                  v_closure[v_i] = [v_value]
            else:
              # ASSIGN_INDEX
              v_valueStackSize -= 3
              v_value = v_valueStack[(v_valueStackSize + 2)]
              v_value2 = v_valueStack[(v_valueStackSize + 1)]
              v_root = v_valueStack[v_valueStackSize]
              v_type = v_root[0]
              v_bool1 = (v_row[0] == 1)
              if (v_type == 6):
                if (v_value2[0] == 3):
                  v_i = v_value2[1]
                  v_list1 = v_root[1]
                  if (v_list1[0] != None):
                    v_value3 = v_canAssignTypeToGeneric(v_vm, v_value, v_list1[0], 0)
                    if (v_value3 == None):
                      v_hasInterrupt = v_EX_InvalidArgument(v_ec, ''.join(["Cannot convert a ", v_typeToStringFromValue(v_vm, v_value), " into a ", v_typeToString(v_vm, v_list1[0], 0)]))
                    v_value = v_value3
                  if not (v_hasInterrupt):
                    if (v_i >= v_list1[1]):
                      v_hasInterrupt = v_EX_IndexOutOfRange(v_ec, "Index is out of range.")
                    elif (v_i < 0):
                      v_i += v_list1[1]
                      if (v_i < 0):
                        v_hasInterrupt = v_EX_IndexOutOfRange(v_ec, "Index is out of range.")
                    if not (v_hasInterrupt):
                      v_list1[2][v_i] = v_value
                else:
                  v_hasInterrupt = v_EX_InvalidArgument(v_ec, "List index must be an integer.")
              elif (v_type == 7):
                v_dictImpl = v_root[1]
                if (v_dictImpl[3] != None):
                  v_value3 = v_canAssignTypeToGeneric(v_vm, v_value, v_dictImpl[3], 0)
                  if (v_value3 == None):
                    v_hasInterrupt = v_EX_InvalidArgument(v_ec, "Cannot assign a value to this dictionary of this type.")
                  else:
                    v_value = v_value3
                v_keyType = v_value2[0]
                if (v_keyType == 3):
                  v_intKey = v_value2[1]
                elif (v_keyType == 5):
                  v_stringKey = v_value2[1]
                elif (v_keyType == 8):
                  v_objInstance1 = v_value2[1]
                  v_intKey = v_objInstance1[1]
                else:
                  v_hasInterrupt = v_EX_InvalidArgument(v_ec, "Invalid key for a dictionary.")
                if not (v_hasInterrupt):
                  v_bool2 = (v_dictImpl[0] == 0)
                  if (v_dictImpl[1] != v_keyType):
                    if (v_dictImpl[3] != None):
                      v_string1 = ''.join(["Cannot assign a key of type ", v_typeToStringFromValue(v_vm, v_value2), " to a dictionary that requires key types of ", v_dictKeyInfoToString(v_vm, v_dictImpl), "."])
                      v_hasInterrupt = v_EX_InvalidKey(v_ec, v_string1)
                    elif not (v_bool2):
                      v_hasInterrupt = v_EX_InvalidKey(v_ec, "Cannot have multiple keys in one dictionary with different types.")
                  elif ((v_keyType == 8) and (v_dictImpl[2] > 0) and (v_objInstance1[0] != v_dictImpl[2])):
                    if v_isClassASubclassOf(v_vm, v_objInstance1[0], v_dictImpl[2]):
                      v_hasInterrupt = v_EX_InvalidKey(v_ec, "Cannot use this type of object as a key for this dictionary.")
                if not (v_hasInterrupt):
                  if (v_keyType == 5):
                    v_int1 = v_dictImpl[5].get(v_stringKey, -1)
                    if (v_int1 == -1):
                      v_dictImpl[5][v_stringKey] = v_dictImpl[0]
                      v_dictImpl[0] += 1
                      v_dictImpl[6].append(v_value2)
                      v_dictImpl[7].append(v_value)
                      if v_bool2:
                        v_dictImpl[1] = v_keyType
                    else:
                      v_dictImpl[7][v_int1] = v_value
                  else:
                    v_int1 = v_dictImpl[4].get(v_intKey, -1)
                    if (v_int1 == -1):
                      v_dictImpl[4][v_intKey] = v_dictImpl[0]
                      v_dictImpl[0] += 1
                      v_dictImpl[6].append(v_value2)
                      v_dictImpl[7].append(v_value)
                      if v_bool2:
                        v_dictImpl[1] = v_keyType
                    else:
                      v_dictImpl[7][v_int1] = v_value
              else:
                v_hasInterrupt = v_EX_UnsupportedOperation(v_ec, v_getTypeFromId(v_type) + " type does not support assigning to an index.")
              if v_bool1:
                v_valueStack[v_valueStackSize] = v_value
                v_valueStackSize += 1
          elif (sc_0 < 7):
            if (sc_0 == 5):
              # ASSIGN_STATIC_FIELD
              v_classInfo = v_classTable[v_row[0]]
              v_staticConstructorNotInvoked = True
              if (v_classInfo[4] < 2):
                v_stack[0] = v_pc
                v_stackFrame2 = v_maybeInvokeStaticConstructor(v_vm, v_ec, v_stack, v_classInfo, v_valueStackSize, PST_IntBuffer16)
                if (PST_IntBuffer16[0] == 1):
                  return v_generateException(v_vm, v_stack, v_pc, v_valueStackSize, v_ec, 0, "Static initialization loop detected. The class this field is a member of is not done being initialized.")
                if (v_stackFrame2 != None):
                  v_staticConstructorNotInvoked = False
                  v_stack = v_stackFrame2
                  v_pc = v_stack[0]
                  v_localsStackSetToken = v_stack[1]
                  v_localsStackOffset = v_stack[2]
              if v_staticConstructorNotInvoked:
                v_valueStackSize -= 1
                v_classInfo[5][v_row[1]] = v_valueStack[v_valueStackSize]
            else:
              # ASSIGN_FIELD
              v_valueStackSize -= 2
              v_value = v_valueStack[(v_valueStackSize + 1)]
              v_value2 = v_valueStack[v_valueStackSize]
              v_nameId = v_row[2]
              if (v_value2[0] == 8):
                v_objInstance1 = v_value2[1]
                v_classId = v_objInstance1[0]
                v_classInfo = v_classTable[v_classId]
                v_intIntDict1 = v_classInfo[14]
                if (v_row[5] == v_classId):
                  v_int1 = v_row[6]
                else:
                  v_int1 = v_intIntDict1.get(v_nameId, -1)
                  if (v_int1 != -1):
                    v_int3 = v_classInfo[12][v_int1]
                    if (v_int3 > 1):
                      if (v_int3 == 2):
                        if (v_classId != v_row[3]):
                          v_int1 = -2
                      else:
                        if ((v_int3 == 3) or (v_int3 == 5)):
                          if (v_classInfo[3] != v_row[4]):
                            v_int1 = -3
                        if ((v_int3 == 4) or (v_int3 == 5)):
                          v_i = v_row[3]
                          if (v_classId == v_i):
                            pass
                          else:
                            v_classInfo = v_classTable[v_classInfo[0]]
                            while ((v_classInfo[2] != -1) and (v_int1 < len(v_classTable[v_classInfo[2]][12]))):
                              v_classInfo = v_classTable[v_classInfo[2]]
                            v_j = v_classInfo[0]
                            if (v_j != v_i):
                              v_bool1 = False
                              while ((v_i != -1) and (v_classTable[v_i][2] != -1)):
                                v_i = v_classTable[v_i][2]
                                if (v_i == v_j):
                                  v_bool1 = True
                                  v_i = -1
                              if not (v_bool1):
                                v_int1 = -4
                          v_classInfo = v_classTable[v_classId]
                  v_row[5] = v_classId
                  v_row[6] = v_int1
                if (v_int1 > -1):
                  v_int2 = v_classInfo[9][v_int1]
                  if (v_int2 == -1):
                    v_intArray1 = v_classInfo[15][v_int1]
                    if (v_intArray1 == None):
                      v_objInstance1[2][v_int1] = v_value
                    else:
                      v_value2 = v_canAssignTypeToGeneric(v_vm, v_value, v_intArray1, 0)
                      if (v_value2 != None):
                        v_objInstance1[2][v_int1] = v_value2
                      else:
                        v_hasInterrupt = v_EX_InvalidArgument(v_ec, "Cannot assign this type to this field.")
                  else:
                    v_hasInterrupt = v_EX_InvalidArgument(v_ec, "Cannot override a method with assignment.")
                elif (v_int1 < -1):
                  v_string1 = v_identifiers[v_row[0]]
                  if (v_int1 == -2):
                    v_string2 = "private"
                  elif (v_int1 == -3):
                    v_string2 = "internal"
                  else:
                    v_string2 = "protected"
                  v_hasInterrupt = v_EX_UnknownField(v_ec, ''.join(["The field '", v_string1, "' is marked as ", v_string2, " and cannot be accessed from here."]))
                else:
                  v_hasInterrupt = v_EX_InvalidAssignment(v_ec, ''.join(["'", v_classInfo[16], "' instances do not have a field called '", v_metadata[0][v_row[0]], "'"]))
              else:
                v_hasInterrupt = v_EX_InvalidAssignment(v_ec, "Cannot assign to a field on this type.")
              if (v_row[1] == 1):
                v_valueStack[v_valueStackSize] = v_value
                v_valueStackSize += 1
          elif (sc_0 == 7):
            # ASSIGN_THIS_FIELD
            v_objInstance2 = v_stack[6][1]
            v_valueStackSize -= 1
            v_objInstance2[2][v_row[0]] = v_valueStack[v_valueStackSize]
          else:
            # ASSIGN_LOCAL
            v_i = (v_localsStackOffset + v_row[0])
            v_valueStackSize -= 1
            v_localsStack[v_i] = v_valueStack[v_valueStackSize]
            v_localsStackSet[v_i] = v_localsStackSetToken
        elif (sc_0 < 13):
          if (sc_0 < 11):
            if (sc_0 == 9):
              # BINARY_OP
              v_valueStackSize -= 1
              v_rightValue = v_valueStack[v_valueStackSize]
              v_leftValue = v_valueStack[(v_valueStackSize - 1)]
              sc_1 = swlookup__interpretImpl__1.get(((((v_leftValue[0] * 15) + v_row[0]) * 11) + v_rightValue[0]), 51)
              if (sc_1 < 26):
                if (sc_1 < 13):
                  if (sc_1 < 7):
                    if (sc_1 < 4):
                      if (sc_1 < 2):
                        if (sc_1 == 0):
                          # int ** int
                          if (v_rightValue[1] == 0):
                            v_value = v_VALUE_INT_ONE
                          elif (v_rightValue[1] > 0):
                            v_value = v_buildInteger(v_globals, int((v_leftValue[1] ** v_rightValue[1])))
                          else:
                            v_value = v_buildFloat(v_globals, (v_leftValue[1] ** v_rightValue[1]))
                        else:
                          # int ** float
                          v_value = v_buildFloat(v_globals, (0.0 + (v_leftValue[1] ** v_rightValue[1])))
                      elif (sc_1 == 2):
                        # float ** int
                        v_value = v_buildFloat(v_globals, (0.0 + (v_leftValue[1] ** v_rightValue[1])))
                      else:
                        # float ** float
                        v_value = v_buildFloat(v_globals, (0.0 + (v_leftValue[1] ** v_rightValue[1])))
                    elif (sc_1 == 4):
                      # float % float
                      v_float1 = v_rightValue[1]
                      if (v_float1 == 0):
                        v_hasInterrupt = v_EX_DivisionByZero(v_ec, "Modulo by 0.")
                      else:
                        v_float3 = (v_leftValue[1] % v_float1)
                        if (v_float3 < 0):
                          v_float3 += v_float1
                        v_value = v_buildFloat(v_globals, v_float3)
                    elif (sc_1 == 5):
                      # float % int
                      v_int1 = v_rightValue[1]
                      if (v_int1 == 0):
                        v_hasInterrupt = v_EX_DivisionByZero(v_ec, "Modulo by 0.")
                      else:
                        v_float1 = (v_leftValue[1] % v_int1)
                        if (v_float1 < 0):
                          v_float1 += v_int1
                        v_value = v_buildFloat(v_globals, v_float1)
                    else:
                      # int % float
                      v_float3 = v_rightValue[1]
                      if (v_float3 == 0):
                        v_hasInterrupt = v_EX_DivisionByZero(v_ec, "Modulo by 0.")
                      else:
                        v_float1 = (v_leftValue[1] % v_float3)
                        if (v_float1 < 0):
                          v_float1 += v_float3
                        v_value = v_buildFloat(v_globals, v_float1)
                  elif (sc_1 < 10):
                    if (sc_1 == 7):
                      # int % int
                      v_int2 = v_rightValue[1]
                      if (v_int2 == 0):
                        v_hasInterrupt = v_EX_DivisionByZero(v_ec, "Modulo by 0.")
                      else:
                        v_int1 = (v_leftValue[1] % v_int2)
                        if (v_int1 < 0):
                          v_int1 += v_int2
                        v_value = v_buildInteger(v_globals, v_int1)
                    elif (sc_1 == 8):
                      # list + list
                      v_value = [6, v_valueConcatLists(v_leftValue[1], v_rightValue[1])]
                    else:
                      # int + int
                      v_int1 = (v_leftValue[1] + v_rightValue[1])
                      if (v_int1 < 0):
                        if (v_int1 > -257):
                          v_value = v_INTEGER_NEGATIVE_CACHE[-v_int1]
                        else:
                          v_value = [3, v_int1]
                      elif (v_int1 < 2049):
                        v_value = v_INTEGER_POSITIVE_CACHE[v_int1]
                      else:
                        v_value = [3, v_int1]
                  elif (sc_1 == 10):
                    # int - int
                    v_int1 = (v_leftValue[1] - v_rightValue[1])
                    if (v_int1 < 0):
                      if (v_int1 > -257):
                        v_value = v_INTEGER_NEGATIVE_CACHE[-v_int1]
                      else:
                        v_value = [3, v_int1]
                    elif (v_int1 < 2049):
                      v_value = v_INTEGER_POSITIVE_CACHE[v_int1]
                    else:
                      v_value = [3, v_int1]
                  elif (sc_1 == 11):
                    # int * int
                    v_int1 = (v_leftValue[1] * v_rightValue[1])
                    if (v_int1 < 0):
                      if (v_int1 > -257):
                        v_value = v_INTEGER_NEGATIVE_CACHE[-v_int1]
                      else:
                        v_value = [3, v_int1]
                    elif (v_int1 < 2049):
                      v_value = v_INTEGER_POSITIVE_CACHE[v_int1]
                    else:
                      v_value = [3, v_int1]
                  else:
                    # int / int
                    v_int1 = v_leftValue[1]
                    v_int2 = v_rightValue[1]
                    if (v_int2 == 0):
                      v_hasInterrupt = v_EX_DivisionByZero(v_ec, "Division by 0.")
                    elif (v_int1 == 0):
                      v_value = v_VALUE_INT_ZERO
                    else:
                      if ((v_int1 % v_int2) == 0):
                        v_int3 = (v_int1) // (v_int2)
                      elif (((v_int1 < 0)) != ((v_int2 < 0))):
                        v_float1 = (1 + (1.0 * ((-1.0 * v_int1)) / (v_int2)))
                        v_float1 -= (v_float1 % 1.0)
                        v_int3 = int((-v_float1))
                      else:
                        v_int3 = (v_int1) // (v_int2)
                      if (v_int3 < 0):
                        if (v_int3 > -257):
                          v_value = v_INTEGER_NEGATIVE_CACHE[-v_int3]
                        else:
                          v_value = [3, v_int3]
                      elif (v_int3 < 2049):
                        v_value = v_INTEGER_POSITIVE_CACHE[v_int3]
                      else:
                        v_value = [3, v_int3]
                elif (sc_1 < 20):
                  if (sc_1 < 17):
                    if (sc_1 < 15):
                      if (sc_1 == 13):
                        # float + int
                        v_value = v_buildFloat(v_globals, (v_leftValue[1] + v_rightValue[1]))
                      else:
                        # int + float
                        v_value = v_buildFloat(v_globals, (v_leftValue[1] + v_rightValue[1]))
                    elif (sc_1 == 15):
                      # float + float
                      v_float1 = (v_leftValue[1] + v_rightValue[1])
                      if (v_float1 == 0):
                        v_value = v_VALUE_FLOAT_ZERO
                      elif (v_float1 == 1):
                        v_value = v_VALUE_FLOAT_ONE
                      else:
                        v_value = [4, v_float1]
                    else:
                      # int - float
                      v_value = v_buildFloat(v_globals, (v_leftValue[1] - v_rightValue[1]))
                  elif (sc_1 == 17):
                    # float - int
                    v_value = v_buildFloat(v_globals, (v_leftValue[1] - v_rightValue[1]))
                  elif (sc_1 == 18):
                    # float - float
                    v_float1 = (v_leftValue[1] - v_rightValue[1])
                    if (v_float1 == 0):
                      v_value = v_VALUE_FLOAT_ZERO
                    elif (v_float1 == 1):
                      v_value = v_VALUE_FLOAT_ONE
                    else:
                      v_value = [4, v_float1]
                  else:
                    # float * int
                    v_value = v_buildFloat(v_globals, (v_leftValue[1] * v_rightValue[1]))
                elif (sc_1 < 23):
                  if (sc_1 == 20):
                    # int * float
                    v_value = v_buildFloat(v_globals, (v_leftValue[1] * v_rightValue[1]))
                  elif (sc_1 == 21):
                    # float * float
                    v_value = v_buildFloat(v_globals, (v_leftValue[1] * v_rightValue[1]))
                  else:
                    # int / float
                    v_float1 = v_rightValue[1]
                    if (v_float1 == 0):
                      v_hasInterrupt = v_EX_DivisionByZero(v_ec, "Division by 0.")
                    else:
                      v_value = v_buildFloat(v_globals, (1.0 * (v_leftValue[1]) / (v_float1)))
                elif (sc_1 == 23):
                  # float / int
                  v_int1 = v_rightValue[1]
                  if (v_int1 == 0):
                    v_hasInterrupt = v_EX_DivisionByZero(v_ec, "Division by 0.")
                  else:
                    v_value = v_buildFloat(v_globals, (1.0 * (v_leftValue[1]) / (v_int1)))
                elif (sc_1 == 24):
                  # float / float
                  v_float1 = v_rightValue[1]
                  if (v_float1 == 0):
                    v_hasInterrupt = v_EX_DivisionByZero(v_ec, "Division by 0.")
                  else:
                    v_value = v_buildFloat(v_globals, (1.0 * (v_leftValue[1]) / (v_float1)))
                else:
                  # int & int
                  v_value = v_buildInteger(v_globals, (v_leftValue[1] & v_rightValue[1]))
              elif (sc_1 < 39):
                if (sc_1 < 33):
                  if (sc_1 < 30):
                    if (sc_1 < 28):
                      if (sc_1 == 26):
                        # int | int
                        v_value = v_buildInteger(v_globals, (v_leftValue[1] | v_rightValue[1]))
                      else:
                        # int ^ int
                        v_value = v_buildInteger(v_globals, (v_leftValue[1] ^ v_rightValue[1]))
                    elif (sc_1 == 28):
                      # int << int
                      v_int1 = v_rightValue[1]
                      if (v_int1 < 0):
                        v_hasInterrupt = v_EX_InvalidArgument(v_ec, "Cannot bit shift by a negative number.")
                      else:
                        v_value = v_buildInteger(v_globals, (v_leftValue[1] << v_int1))
                    else:
                      # int >> int
                      v_int1 = v_rightValue[1]
                      if (v_int1 < 0):
                        v_hasInterrupt = v_EX_InvalidArgument(v_ec, "Cannot bit shift by a negative number.")
                      else:
                        v_value = v_buildInteger(v_globals, (v_leftValue[1] >> v_int1))
                  elif (sc_1 == 30):
                    # int < int
                    if (v_leftValue[1] < v_rightValue[1]):
                      v_value = v_VALUE_TRUE
                    else:
                      v_value = v_VALUE_FALSE
                  elif (sc_1 == 31):
                    # int <= int
                    if (v_leftValue[1] <= v_rightValue[1]):
                      v_value = v_VALUE_TRUE
                    else:
                      v_value = v_VALUE_FALSE
                  else:
                    # float < int
                    if (v_leftValue[1] < v_rightValue[1]):
                      v_value = v_VALUE_TRUE
                    else:
                      v_value = v_VALUE_FALSE
                elif (sc_1 < 36):
                  if (sc_1 == 33):
                    # float <= int
                    if (v_leftValue[1] <= v_rightValue[1]):
                      v_value = v_VALUE_TRUE
                    else:
                      v_value = v_VALUE_FALSE
                  elif (sc_1 == 34):
                    # int < float
                    if (v_leftValue[1] < v_rightValue[1]):
                      v_value = v_VALUE_TRUE
                    else:
                      v_value = v_VALUE_FALSE
                  else:
                    # int <= float
                    if (v_leftValue[1] <= v_rightValue[1]):
                      v_value = v_VALUE_TRUE
                    else:
                      v_value = v_VALUE_FALSE
                elif (sc_1 == 36):
                  # float < float
                  if (v_leftValue[1] < v_rightValue[1]):
                    v_value = v_VALUE_TRUE
                  else:
                    v_value = v_VALUE_FALSE
                elif (sc_1 == 37):
                  # float <= float
                  if (v_leftValue[1] <= v_rightValue[1]):
                    v_value = v_VALUE_TRUE
                  else:
                    v_value = v_VALUE_FALSE
                else:
                  # int >= int
                  if (v_leftValue[1] >= v_rightValue[1]):
                    v_value = v_VALUE_TRUE
                  else:
                    v_value = v_VALUE_FALSE
              elif (sc_1 < 46):
                if (sc_1 < 43):
                  if (sc_1 < 41):
                    if (sc_1 == 39):
                      # int > int
                      if (v_leftValue[1] > v_rightValue[1]):
                        v_value = v_VALUE_TRUE
                      else:
                        v_value = v_VALUE_FALSE
                    else:
                      # float >= int
                      if (v_leftValue[1] >= v_rightValue[1]):
                        v_value = v_VALUE_TRUE
                      else:
                        v_value = v_VALUE_FALSE
                  elif (sc_1 == 41):
                    # float > int
                    if (v_leftValue[1] > v_rightValue[1]):
                      v_value = v_VALUE_TRUE
                    else:
                      v_value = v_VALUE_FALSE
                  else:
                    # int >= float
                    if (v_leftValue[1] >= v_rightValue[1]):
                      v_value = v_VALUE_TRUE
                    else:
                      v_value = v_VALUE_FALSE
                elif (sc_1 == 43):
                  # int > float
                  if (v_leftValue[1] > v_rightValue[1]):
                    v_value = v_VALUE_TRUE
                  else:
                    v_value = v_VALUE_FALSE
                elif (sc_1 == 44):
                  # float >= float
                  if (v_leftValue[1] >= v_rightValue[1]):
                    v_value = v_VALUE_TRUE
                  else:
                    v_value = v_VALUE_FALSE
                else:
                  # float > float
                  if (v_leftValue[1] > v_rightValue[1]):
                    v_value = v_VALUE_TRUE
                  else:
                    v_value = v_VALUE_FALSE
              elif (sc_1 < 49):
                if (sc_1 == 46):
                  # string + string
                  v_value = [5, v_leftValue[1] + v_rightValue[1]]
                elif (sc_1 == 47):
                  # string * int
                  v_value = v_multiplyString(v_globals, v_leftValue, v_leftValue[1], v_rightValue[1])
                else:
                  # int * string
                  v_value = v_multiplyString(v_globals, v_rightValue, v_rightValue[1], v_leftValue[1])
              elif (sc_1 == 49):
                # list * int
                v_int1 = v_rightValue[1]
                if (v_int1 < 0):
                  v_hasInterrupt = v_EX_UnsupportedOperation(v_ec, "Cannot multiply list by negative number.")
                else:
                  v_value = [6, v_valueMultiplyList(v_leftValue[1], v_int1)]
              elif (sc_1 == 50):
                # int * list
                v_int1 = v_leftValue[1]
                if (v_int1 < 0):
                  v_hasInterrupt = v_EX_UnsupportedOperation(v_ec, "Cannot multiply list by negative number.")
                else:
                  v_value = [6, v_valueMultiplyList(v_rightValue[1], v_int1)]
              elif ((v_row[0] == 0) and (((v_leftValue[0] == 5) or (v_rightValue[0] == 5)))):
                v_value = [5, v_valueToString(v_vm, v_leftValue) + v_valueToString(v_vm, v_rightValue)]
              else:
                # unrecognized op
                v_hasInterrupt = v_EX_UnsupportedOperation(v_ec, ''.join(["The '", v_getBinaryOpFromId(v_row[0]), "' operator is not supported for these types: ", v_getTypeFromId(v_leftValue[0]), " and ", v_getTypeFromId(v_rightValue[0])]))
              v_valueStack[(v_valueStackSize - 1)] = v_value
            else:
              # BOOLEAN_NOT
              v_value = v_valueStack[(v_valueStackSize - 1)]
              if (v_value[0] != 2):
                v_hasInterrupt = v_EX_InvalidArgument(v_ec, "Boolean expected.")
              elif v_value[1]:
                v_valueStack[(v_valueStackSize - 1)] = v_VALUE_FALSE
              else:
                v_valueStack[(v_valueStackSize - 1)] = v_VALUE_TRUE
          elif (sc_0 == 11):
            # BREAK
            if (v_row[0] == 1):
              v_pc += v_row[1]
            else:
              v_intArray1 = v_esfData[v_pc]
              v_pc = (v_intArray1[1] - 1)
              v_valueStackSize = v_stack[7]
              v_stack[10] = 1
          else:
            # CALL_FUNCTION
            v_type = v_row[0]
            v_argCount = v_row[1]
            v_functionId = v_row[2]
            v_returnValueUsed = (v_row[3] == 1)
            v_classId = v_row[4]
            if ((v_type == 2) or (v_type == 6)):
              # constructor or static method
              v_classInfo = v_metadata[9][v_classId]
              v_staticConstructorNotInvoked = True
              if (v_classInfo[4] < 2):
                v_stack[0] = v_pc
                v_stackFrame2 = v_maybeInvokeStaticConstructor(v_vm, v_ec, v_stack, v_classInfo, v_valueStackSize, PST_IntBuffer16)
                if (PST_IntBuffer16[0] == 1):
                  return v_generateException(v_vm, v_stack, v_pc, v_valueStackSize, v_ec, 0, "Static initialization loop detected. The class this field is a member of is not done being initialized.")
                if (v_stackFrame2 != None):
                  v_staticConstructorNotInvoked = False
                  v_stack = v_stackFrame2
                  v_pc = v_stack[0]
                  v_localsStackSetToken = v_stack[1]
                  v_localsStackOffset = v_stack[2]
            else:
              v_staticConstructorNotInvoked = True
            if v_staticConstructorNotInvoked:
              v_bool1 = True
              # construct args array
              if (v_argCount == -1):
                v_valueStackSize -= 1
                v_value = v_valueStack[v_valueStackSize]
                if (v_value[0] == 1):
                  v_argCount = 0
                elif (v_value[0] == 6):
                  v_list1 = v_value[1]
                  v_argCount = v_list1[1]
                  v_i = (v_argCount - 1)
                  while (v_i >= 0):
                    v_funcArgs[v_i] = v_list1[2][v_i]
                    v_i -= 1
                else:
                  v_hasInterrupt = v_EX_InvalidArgument(v_ec, "Function pointers' .invoke method requires a list argument.")
              else:
                v_i = (v_argCount - 1)
                while (v_i >= 0):
                  v_valueStackSize -= 1
                  v_funcArgs[v_i] = v_valueStack[v_valueStackSize]
                  v_i -= 1
              if not (v_hasInterrupt):
                if (v_type == 3):
                  v_value = v_stack[6]
                  v_objInstance1 = v_value[1]
                  if (v_objInstance1[0] != v_classId):
                    v_int2 = v_row[5]
                    if (v_int2 != -1):
                      v_classInfo = v_classTable[v_objInstance1[0]]
                      v_functionId = v_classInfo[9][v_int2]
                elif (v_type == 5):
                  # field invocation
                  v_valueStackSize -= 1
                  v_value = v_valueStack[v_valueStackSize]
                  v_localeId = v_row[5]
                  sc_2 = swlookup__interpretImpl__2.get(v_value[0], 3)
                  if (sc_2 < 2):
                    if (sc_2 == 0):
                      v_hasInterrupt = v_EX_NullReference(v_ec, "Invoked method on null.")
                    else:
                      # field invoked on an object instance.
                      v_objInstance1 = v_value[1]
                      v_int1 = v_objInstance1[0]
                      v_classInfo = v_classTable[v_int1]
                      v_intIntDict1 = v_classInfo[14]
                      v_int1 = ((v_row[4] * v_magicNumbers[2]) + v_row[5])
                      v_i = v_intIntDict1.get(v_int1, -1)
                      if (v_i != -1):
                        v_int1 = v_intIntDict1[v_int1]
                        v_functionId = v_classInfo[9][v_int1]
                        if (v_functionId > 0):
                          v_type = 3
                        else:
                          v_value = v_objInstance1[2][v_int1]
                          v_type = 4
                          v_valueStack[v_valueStackSize] = v_value
                          v_valueStackSize += 1
                      else:
                        v_hasInterrupt = v_EX_UnknownField(v_ec, "Unknown field.")
                  elif (sc_2 == 2):
                    # field invocation on a class object instance.
                    v_functionId = v_resolvePrimitiveMethodName2(v_globalNameIdToPrimitiveMethodName, v_value[0], v_classId)
                    if (v_functionId < 0):
                      v_hasInterrupt = v_EX_InvalidInvocation(v_ec, "Class definitions do not have that method.")
                    else:
                      v_functionId = v_resolvePrimitiveMethodName2(v_globalNameIdToPrimitiveMethodName, v_value[0], v_classId)
                      if (v_functionId < 0):
                        v_hasInterrupt = v_EX_InvalidInvocation(v_ec, v_getTypeFromId(v_value[0]) + " does not have that method.")
                      elif (v_globalNameIdToPrimitiveMethodName[v_classId] == 8):
                        v_type = 6
                        v_classValue = v_value[1]
                        if v_classValue[0]:
                          v_hasInterrupt = v_EX_UnsupportedOperation(v_ec, "Cannot create an instance of an interface.")
                        else:
                          v_classId = v_classValue[1]
                          if not (v_returnValueUsed):
                            v_hasInterrupt = v_EX_UnsupportedOperation(v_ec, "Cannot create an instance and not use the output.")
                          else:
                            v_classInfo = v_metadata[9][v_classId]
                            v_functionId = v_classInfo[7]
                      else:
                        v_type = 9
                  else:
                    # primitive method suspected.
                    v_functionId = v_resolvePrimitiveMethodName2(v_globalNameIdToPrimitiveMethodName, v_value[0], v_classId)
                    if (v_functionId < 0):
                      v_hasInterrupt = v_EX_InvalidInvocation(v_ec, v_getTypeFromId(v_value[0]) + " does not have that method.")
                    else:
                      v_type = 9
              if ((v_type == 4) and not (v_hasInterrupt)):
                # pointer provided
                v_valueStackSize -= 1
                v_value = v_valueStack[v_valueStackSize]
                if (v_value[0] == 9):
                  v_functionPointer1 = v_value[1]
                  sc_3 = swlookup__interpretImpl__3.get(v_functionPointer1[0], 5)
                  if (sc_3 < 3):
                    if (sc_3 == 0):
                      # pointer to a function
                      v_functionId = v_functionPointer1[3]
                      v_type = 1
                    elif (sc_3 == 1):
                      # pointer to a method
                      v_functionId = v_functionPointer1[3]
                      v_value = v_functionPointer1[1]
                      v_type = 3
                    else:
                      # pointer to a static method
                      v_functionId = v_functionPointer1[3]
                      v_classId = v_functionPointer1[2]
                      v_type = 2
                  elif (sc_3 == 3):
                    # pointer to a primitive method
                    v_value = v_functionPointer1[1]
                    v_functionId = v_functionPointer1[3]
                    v_type = 9
                  elif (sc_3 == 4):
                    # lambda instance
                    v_value = v_functionPointer1[1]
                    v_functionId = v_functionPointer1[3]
                    v_type = 10
                    v_closure = v_functionPointer1[4]
                else:
                  v_hasInterrupt = v_EX_InvalidInvocation(v_ec, "This type cannot be invoked like a function.")
              if ((v_type == 9) and not (v_hasInterrupt)):
                # primitive method invocation
                v_output = v_VALUE_NULL
                v_primitiveMethodToCoreLibraryFallback = False
                sc_4 = swlookup__interpretImpl__4.get(v_value[0], 5)
                if (sc_4 < 3):
                  if (sc_4 == 0):
                    # ...on a string
                    v_string1 = v_value[1]
                    sc_5 = swlookup__interpretImpl__5.get(v_functionId, 12)
                    if (sc_5 < 7):
                      if (sc_5 < 4):
                        if (sc_5 < 2):
                          if (sc_5 == 0):
                            if (v_argCount != 1):
                              v_hasInterrupt = v_EX_InvalidArgument(v_ec, v_primitiveMethodWrongArgCountError("string contains method", 1, v_argCount))
                            else:
                              v_value2 = v_funcArgs[0]
                              if (v_value2[0] != 5):
                                v_hasInterrupt = v_EX_InvalidArgument(v_ec, "string contains method requires another string as input.")
                              elif (v_value2[1] in v_string1):
                                v_output = v_VALUE_TRUE
                              else:
                                v_output = v_VALUE_FALSE
                          elif (v_argCount != 1):
                            v_hasInterrupt = v_EX_InvalidArgument(v_ec, v_primitiveMethodWrongArgCountError("string endsWith method", 1, v_argCount))
                          else:
                            v_value2 = v_funcArgs[0]
                            if (v_value2[0] != 5):
                              v_hasInterrupt = v_EX_InvalidArgument(v_ec, "string endsWith method requires another string as input.")
                            elif v_string1.endswith(v_value2[1]):
                              v_output = v_VALUE_TRUE
                            else:
                              v_output = v_VALUE_FALSE
                        elif (sc_5 == 2):
                          if (v_argCount != 1):
                            v_hasInterrupt = v_EX_InvalidArgument(v_ec, v_primitiveMethodWrongArgCountError("string indexOf method", 1, v_argCount))
                          else:
                            v_value2 = v_funcArgs[0]
                            if (v_value2[0] != 5):
                              v_hasInterrupt = v_EX_InvalidArgument(v_ec, "string indexOf method requires another string as input.")
                            else:
                              v_output = v_buildInteger(v_globals, v_string1.find(v_value2[1]))
                        elif (v_argCount > 0):
                          v_hasInterrupt = v_EX_InvalidArgument(v_ec, v_primitiveMethodWrongArgCountError("string lower method", 0, v_argCount))
                        else:
                          v_output = v_buildString(v_globals, v_string1.lower())
                      elif (sc_5 == 4):
                        if (v_argCount > 0):
                          v_hasInterrupt = v_EX_InvalidArgument(v_ec, v_primitiveMethodWrongArgCountError("string ltrim method", 0, v_argCount))
                        else:
                          v_output = v_buildString(v_globals, v_string1.lstrip())
                      elif (sc_5 == 5):
                        if (v_argCount != 2):
                          v_hasInterrupt = v_EX_InvalidArgument(v_ec, v_primitiveMethodWrongArgCountError("string replace method", 2, v_argCount))
                        else:
                          v_value2 = v_funcArgs[0]
                          v_value3 = v_funcArgs[1]
                          if ((v_value2[0] != 5) or (v_value3[0] != 5)):
                            v_hasInterrupt = v_EX_InvalidArgument(v_ec, "string replace method requires 2 strings as input.")
                          else:
                            v_output = v_buildString(v_globals, v_string1.replace(v_value2[1], v_value3[1]))
                      elif (v_argCount > 0):
                        v_hasInterrupt = v_EX_InvalidArgument(v_ec, v_primitiveMethodWrongArgCountError("string reverse method", 0, v_argCount))
                      else:
                        v_output = v_buildString(v_globals, v_string1[::-1])
                    elif (sc_5 < 10):
                      if (sc_5 == 7):
                        if (v_argCount > 0):
                          v_hasInterrupt = v_EX_InvalidArgument(v_ec, v_primitiveMethodWrongArgCountError("string rtrim method", 0, v_argCount))
                        else:
                          v_output = v_buildString(v_globals, v_string1.rstrip())
                      elif (sc_5 == 8):
                        if (v_argCount != 1):
                          v_hasInterrupt = v_EX_InvalidArgument(v_ec, v_primitiveMethodWrongArgCountError("string split method", 1, v_argCount))
                        else:
                          v_value2 = v_funcArgs[0]
                          if (v_value2[0] != 5):
                            v_hasInterrupt = v_EX_InvalidArgument(v_ec, "string split method requires another string as input.")
                          else:
                            v_stringList = v_string1.split(v_value2[1])
                            v_len = len(v_stringList)
                            v_list1 = v_makeEmptyList(v_globals[14], v_len)
                            v_i = 0
                            while (v_i < v_len):
                              v_list1[2].append(v_buildString(v_globals, v_stringList[v_i]))
                              v_i += 1
                            v_list1[1] = v_len
                            v_output = [6, v_list1]
                      elif (v_argCount != 1):
                        v_hasInterrupt = v_EX_InvalidArgument(v_ec, v_primitiveMethodWrongArgCountError("string startsWith method", 1, v_argCount))
                      else:
                        v_value2 = v_funcArgs[0]
                        if (v_value2[0] != 5):
                          v_hasInterrupt = v_EX_InvalidArgument(v_ec, "string startsWith method requires another string as input.")
                        elif v_string1.startswith(v_value2[1]):
                          v_output = v_VALUE_TRUE
                        else:
                          v_output = v_VALUE_FALSE
                    elif (sc_5 == 10):
                      if (v_argCount > 0):
                        v_hasInterrupt = v_EX_InvalidArgument(v_ec, v_primitiveMethodWrongArgCountError("string trim method", 0, v_argCount))
                      else:
                        v_output = v_buildString(v_globals, v_string1.strip())
                    elif (sc_5 == 11):
                      if (v_argCount > 0):
                        v_hasInterrupt = v_EX_InvalidArgument(v_ec, v_primitiveMethodWrongArgCountError("string upper method", 0, v_argCount))
                      else:
                        v_output = v_buildString(v_globals, v_string1.upper())
                    else:
                      v_output = None
                  elif (sc_4 == 1):
                    # ...on a list
                    v_list1 = v_value[1]
                    sc_6 = swlookup__interpretImpl__6.get(v_functionId, 15)
                    if (sc_6 < 8):
                      if (sc_6 < 4):
                        if (sc_6 < 2):
                          if (sc_6 == 0):
                            if (v_argCount == 0):
                              v_hasInterrupt = v_EX_InvalidArgument(v_ec, "List add method requires at least one argument.")
                            else:
                              v_intArray1 = v_list1[0]
                              v_i = 0
                              while (v_i < v_argCount):
                                v_value = v_funcArgs[v_i]
                                if (v_intArray1 != None):
                                  v_value2 = v_canAssignTypeToGeneric(v_vm, v_value, v_intArray1, 0)
                                  if (v_value2 == None):
                                    v_hasInterrupt = v_EX_InvalidArgument(v_ec, ''.join(["Cannot convert a ", v_typeToStringFromValue(v_vm, v_value), " into a ", v_typeToString(v_vm, v_list1[0], 0)]))
                                  v_list1[2].append(v_value2)
                                else:
                                  v_list1[2].append(v_value)
                                v_i += 1
                              v_list1[1] += v_argCount
                              v_output = v_VALUE_NULL
                          elif (v_argCount > 0):
                            v_hasInterrupt = v_EX_InvalidArgument(v_ec, v_primitiveMethodWrongArgCountError("list choice method", 0, v_argCount))
                          else:
                            v_len = v_list1[1]
                            if (v_len == 0):
                              v_hasInterrupt = v_EX_UnsupportedOperation(v_ec, "Cannot use list.choice() method on an empty list.")
                            else:
                              v_i = int(((random.random() * v_len)))
                              v_output = v_list1[2][v_i]
                        elif (sc_6 == 2):
                          if (v_argCount > 0):
                            v_hasInterrupt = v_EX_InvalidArgument(v_ec, v_primitiveMethodWrongArgCountError("list clear method", 0, v_argCount))
                          elif (v_list1[1] > 0):
                            if (v_list1[1] == 1):
                              v_list1[2].pop()
                            else:
                              v_list1[2] = []
                            v_list1[1] = 0
                        elif (v_argCount > 0):
                          v_hasInterrupt = v_EX_InvalidArgument(v_ec, v_primitiveMethodWrongArgCountError("list clone method", 0, v_argCount))
                        else:
                          v_len = v_list1[1]
                          v_list2 = v_makeEmptyList(v_list1[0], v_len)
                          v_i = 0
                          while (v_i < v_len):
                            v_list2[2].append(v_list1[2][v_i])
                            v_i += 1
                          v_list2[1] = v_len
                          v_output = [6, v_list2]
                      elif (sc_6 < 6):
                        if (sc_6 == 4):
                          if (v_argCount != 1):
                            v_hasInterrupt = v_EX_InvalidArgument(v_ec, v_primitiveMethodWrongArgCountError("list concat method", 1, v_argCount))
                          else:
                            v_value2 = v_funcArgs[0]
                            if (v_value2[0] != 6):
                              v_hasInterrupt = v_EX_InvalidArgument(v_ec, "list concat methods requires a list as an argument.")
                            else:
                              v_list2 = v_value2[1]
                              v_intArray1 = v_list1[0]
                              if ((v_intArray1 != None) and not (v_canAssignGenericToGeneric(v_vm, v_list2[0], 0, v_intArray1, 0, v_intBuffer))):
                                v_hasInterrupt = v_EX_InvalidArgument(v_ec, "Cannot concat a list: incompatible types.")
                              else:
                                if ((v_intArray1 != None) and (v_intArray1[0] == 4) and (v_list2[0][0] == 3)):
                                  v_bool1 = True
                                else:
                                  v_bool1 = False
                                v_len = v_list2[1]
                                v_i = 0
                                while (v_i < v_len):
                                  v_value = v_list2[2][v_i]
                                  if v_bool1:
                                    v_value = v_buildFloat(v_globals, (0.0 + v_value[1]))
                                  v_list1[2].append(v_value)
                                  v_i += 1
                        elif (v_argCount != 1):
                          v_hasInterrupt = v_EX_InvalidArgument(v_ec, v_primitiveMethodWrongArgCountError("list contains method", 1, v_argCount))
                        else:
                          v_value2 = v_funcArgs[0]
                          v_len = v_list1[1]
                          v_output = v_VALUE_FALSE
                          v_i = 0
                          while (v_i < v_len):
                            v_value = v_list1[2][v_i]
                            if (v_doEqualityComparisonAndReturnCode(v_value2, v_value) == 1):
                              v_output = v_VALUE_TRUE
                              v_i = v_len
                            v_i += 1
                      elif (sc_6 == 6):
                        if (v_argCount != 1):
                          v_hasInterrupt = v_EX_InvalidArgument(v_ec, v_primitiveMethodWrongArgCountError("list filter method", 1, v_argCount))
                        else:
                          v_value2 = v_funcArgs[0]
                          if (v_value2[0] != 9):
                            v_hasInterrupt = v_EX_InvalidArgument(v_ec, "list filter method requires a function pointer as its argument.")
                          else:
                            v_primitiveMethodToCoreLibraryFallback = True
                            v_functionId = v_metadata[15][0]
                            v_funcArgs[1] = v_value
                            v_argCount = 2
                            v_output = None
                      elif (v_argCount != 2):
                        v_hasInterrupt = v_EX_InvalidArgument(v_ec, v_primitiveMethodWrongArgCountError("list insert method", 1, v_argCount))
                      else:
                        v_value = v_funcArgs[0]
                        v_value2 = v_funcArgs[1]
                        if (v_value[0] != 3):
                          v_hasInterrupt = v_EX_InvalidArgument(v_ec, "First argument of list.insert must be an integer index.")
                        else:
                          v_intArray1 = v_list1[0]
                          if (v_intArray1 != None):
                            v_value3 = v_canAssignTypeToGeneric(v_vm, v_value2, v_intArray1, 0)
                            if (v_value3 == None):
                              v_hasInterrupt = v_EX_InvalidArgument(v_ec, "Cannot insert this type into this type of list.")
                            v_value2 = v_value3
                          if not (v_hasInterrupt):
                            v_int1 = v_value[1]
                            v_len = v_list1[1]
                            if (v_int1 < 0):
                              v_int1 += v_len
                            if (v_int1 == v_len):
                              v_list1[2].append(v_value2)
                              v_list1[1] += 1
                            elif ((v_int1 < 0) or (v_int1 >= v_len)):
                              v_hasInterrupt = v_EX_IndexOutOfRange(v_ec, "Index out of range.")
                            else:
                              v_list1[2].insert(v_int1, v_value2)
                              v_list1[1] += 1
                    elif (sc_6 < 12):
                      if (sc_6 < 10):
                        if (sc_6 == 8):
                          if (v_argCount != 1):
                            if (v_argCount == 0):
                              v_value2 = v_globals[8]
                            else:
                              v_hasInterrupt = v_EX_InvalidArgument(v_ec, v_primitiveMethodWrongArgCountError("list join method", 1, v_argCount))
                          else:
                            v_value2 = v_funcArgs[0]
                            if (v_value2[0] != 5):
                              v_hasInterrupt = v_EX_InvalidArgument(v_ec, "Argument of list.join needs to be a string.")
                          if not (v_hasInterrupt):
                            v_stringList1 = []
                            v_string1 = v_value2[1]
                            v_len = v_list1[1]
                            v_i = 0
                            while (v_i < v_len):
                              v_value = v_list1[2][v_i]
                              if (v_value[0] != 5):
                                v_string2 = v_valueToString(v_vm, v_value)
                              else:
                                v_string2 = v_value[1]
                              v_stringList1.append(v_string2)
                              v_i += 1
                            v_output = v_buildString(v_globals, v_string1.join(v_stringList1))
                        elif (v_argCount != 1):
                          v_hasInterrupt = v_EX_InvalidArgument(v_ec, v_primitiveMethodWrongArgCountError("list map method", 1, v_argCount))
                        else:
                          v_value2 = v_funcArgs[0]
                          if (v_value2[0] != 9):
                            v_hasInterrupt = v_EX_InvalidArgument(v_ec, "list map method requires a function pointer as its argument.")
                          else:
                            v_primitiveMethodToCoreLibraryFallback = True
                            v_functionId = v_metadata[15][1]
                            v_funcArgs[1] = v_value
                            v_argCount = 2
                            v_output = None
                      elif (sc_6 == 10):
                        if (v_argCount > 0):
                          v_hasInterrupt = v_EX_InvalidArgument(v_ec, v_primitiveMethodWrongArgCountError("list pop method", 0, v_argCount))
                        else:
                          v_len = v_list1[1]
                          if (v_len < 1):
                            v_hasInterrupt = v_EX_IndexOutOfRange(v_ec, "Cannot pop from an empty list.")
                          else:
                            v_len -= 1
                            v_value = v_list1[2].pop()
                            if v_returnValueUsed:
                              v_output = v_value
                            v_list1[1] = v_len
                      elif (v_argCount != 1):
                        v_hasInterrupt = v_EX_InvalidArgument(v_ec, v_primitiveMethodWrongArgCountError("list remove method", 1, v_argCount))
                      else:
                        v_value = v_funcArgs[0]
                        if (v_value[0] != 3):
                          v_hasInterrupt = v_EX_InvalidArgument(v_ec, "Argument of list.remove must be an integer index.")
                        else:
                          v_int1 = v_value[1]
                          v_len = v_list1[1]
                          if (v_int1 < 0):
                            v_int1 += v_len
                          if ((v_int1 < 0) or (v_int1 >= v_len)):
                            v_hasInterrupt = v_EX_IndexOutOfRange(v_ec, "Index out of range.")
                          else:
                            if v_returnValueUsed:
                              v_output = v_list1[2][v_int1]
                            v_len = (v_list1[1] - 1)
                            v_list1[1] = v_len
                            del v_list1[2][v_int1]
                    elif (sc_6 < 14):
                      if (sc_6 == 12):
                        if (v_argCount > 0):
                          v_hasInterrupt = v_EX_InvalidArgument(v_ec, v_primitiveMethodWrongArgCountError("list reverse method", 0, v_argCount))
                        else:
                          v_list1[2].reverse()
                      elif (v_argCount > 0):
                        v_hasInterrupt = v_EX_InvalidArgument(v_ec, v_primitiveMethodWrongArgCountError("list shuffle method", 0, v_argCount))
                      else:
                        random.shuffle(v_list1[2])
                    elif (sc_6 == 14):
                      if (v_argCount == 0):
                        v_sortLists(v_list1, v_list1, PST_IntBuffer16)
                        if (PST_IntBuffer16[0] > 0):
                          v_hasInterrupt = v_EX_InvalidArgument(v_ec, "Invalid list to sort. All items must be numbers or all strings, but not mixed.")
                      elif (v_argCount == 1):
                        v_value2 = v_funcArgs[0]
                        if (v_value2[0] == 9):
                          v_primitiveMethodToCoreLibraryFallback = True
                          v_functionId = v_metadata[15][2]
                          v_funcArgs[1] = v_value
                          v_argCount = 2
                        else:
                          v_hasInterrupt = v_EX_InvalidArgument(v_ec, "list.sort(get_key_function) requires a function pointer as its argument.")
                        v_output = None
                    else:
                      v_output = None
                  else:
                    # ...on a dictionary
                    v_dictImpl = v_value[1]
                    sc_7 = swlookup__interpretImpl__7.get(v_functionId, 8)
                    if (sc_7 < 5):
                      if (sc_7 < 3):
                        if (sc_7 == 0):
                          if (v_argCount > 0):
                            v_hasInterrupt = v_EX_InvalidArgument(v_ec, v_primitiveMethodWrongArgCountError("dictionary clear method", 0, v_argCount))
                          elif (v_dictImpl[0] > 0):
                            v_dictImpl[4] = {}
                            v_dictImpl[5] = {}
                            del v_dictImpl[6][:]
                            del v_dictImpl[7][:]
                            v_dictImpl[0] = 0
                        elif (sc_7 == 1):
                          if (v_argCount > 0):
                            v_hasInterrupt = v_EX_InvalidArgument(v_ec, v_primitiveMethodWrongArgCountError("dictionary clone method", 0, v_argCount))
                          else:
                            v_output = [7, v_cloneDictionary(v_dictImpl, None)]
                        elif (v_argCount != 1):
                          v_hasInterrupt = v_EX_InvalidArgument(v_ec, v_primitiveMethodWrongArgCountError("dictionary contains method", 1, v_argCount))
                        else:
                          v_value = v_funcArgs[0]
                          v_output = v_VALUE_FALSE
                          if (v_value[0] == 5):
                            if (v_value[1] in v_dictImpl[5]):
                              v_output = v_VALUE_TRUE
                          else:
                            if (v_value[0] == 3):
                              v_i = v_value[1]
                            else:
                              v_i = (v_value[1])[1]
                            if (v_i in v_dictImpl[4]):
                              v_output = v_VALUE_TRUE
                      elif (sc_7 == 3):
                        if ((v_argCount != 1) and (v_argCount != 2)):
                          v_hasInterrupt = v_EX_InvalidArgument(v_ec, "Dictionary get method requires 1 or 2 arguments.")
                        else:
                          v_value = v_funcArgs[0]
                          sc_8 = swlookup__interpretImpl__8.get(v_value[0], 3)
                          if (sc_8 < 2):
                            if (sc_8 == 0):
                              v_int1 = v_value[1]
                              v_i = v_dictImpl[4].get(v_int1, -1)
                            else:
                              v_int1 = (v_value[1])[1]
                              v_i = v_dictImpl[4].get(v_int1, -1)
                          elif (sc_8 == 2):
                            v_string1 = v_value[1]
                            v_i = v_dictImpl[5].get(v_string1, -1)
                          if (v_i == -1):
                            if (v_argCount == 2):
                              v_output = v_funcArgs[1]
                            else:
                              v_output = v_VALUE_NULL
                          else:
                            v_output = v_dictImpl[7][v_i]
                      elif (v_argCount > 0):
                        v_hasInterrupt = v_EX_InvalidArgument(v_ec, v_primitiveMethodWrongArgCountError("dictionary keys method", 0, v_argCount))
                      else:
                        v_valueList1 = v_dictImpl[6]
                        v_len = len(v_valueList1)
                        if (v_dictImpl[1] == 8):
                          v_intArray1 = [None, None]
                          v_intArray1[0] = 8
                          v_intArray1[0] = v_dictImpl[2]
                        else:
                          v_intArray1 = [None]
                          v_intArray1[0] = v_dictImpl[1]
                        v_list1 = v_makeEmptyList(v_intArray1, v_len)
                        v_i = 0
                        while (v_i < v_len):
                          v_list1[2].append(v_valueList1[v_i])
                          v_i += 1
                        v_list1[1] = v_len
                        v_output = [6, v_list1]
                    elif (sc_7 < 7):
                      if (sc_7 == 5):
                        if (v_argCount != 1):
                          v_hasInterrupt = v_EX_InvalidArgument(v_ec, v_primitiveMethodWrongArgCountError("dictionary merge method", 1, v_argCount))
                        else:
                          v_value2 = v_funcArgs[0]
                          if (v_value2[0] != 7):
                            v_hasInterrupt = v_EX_InvalidArgument(v_ec, "dictionary merge method requires another dictionary as a parameeter.")
                          else:
                            v_dictImpl2 = v_value2[1]
                            if (v_dictImpl2[0] > 0):
                              if (v_dictImpl[0] == 0):
                                v_value[1] = v_cloneDictionary(v_dictImpl2, None)
                              elif (v_dictImpl2[1] != v_dictImpl[1]):
                                v_hasInterrupt = v_EX_InvalidKey(v_ec, "Dictionaries with different key types cannot be merged.")
                              elif ((v_dictImpl2[1] == 8) and (v_dictImpl2[2] != v_dictImpl[2]) and (v_dictImpl[2] != 0) and not (v_isClassASubclassOf(v_vm, v_dictImpl2[2], v_dictImpl[2]))):
                                v_hasInterrupt = v_EX_InvalidKey(v_ec, "Dictionary key types are incompatible.")
                              else:
                                if (v_dictImpl[3] == None):
                                  pass
                                elif (v_dictImpl2[3] == None):
                                  v_hasInterrupt = v_EX_InvalidKey(v_ec, "Dictionaries with different value types cannot be merged.")
                                elif not (v_canAssignGenericToGeneric(v_vm, v_dictImpl2[3], 0, v_dictImpl[3], 0, v_intBuffer)):
                                  v_hasInterrupt = v_EX_InvalidKey(v_ec, "The dictionary value types are incompatible.")
                                if not (v_hasInterrupt):
                                  v_cloneDictionary(v_dictImpl2, v_dictImpl)
                            v_output = v_VALUE_NULL
                      elif (v_argCount != 1):
                        v_hasInterrupt = v_EX_InvalidArgument(v_ec, v_primitiveMethodWrongArgCountError("dictionary remove method", 1, v_argCount))
                      else:
                        v_value2 = v_funcArgs[0]
                        v_bool2 = False
                        v_keyType = v_dictImpl[1]
                        if ((v_dictImpl[0] > 0) and (v_keyType == v_value2[0])):
                          if (v_keyType == 5):
                            v_stringKey = v_value2[1]
                            if (v_stringKey in v_dictImpl[5]):
                              v_i = v_dictImpl[5][v_stringKey]
                              v_bool2 = True
                          else:
                            if (v_keyType == 3):
                              v_intKey = v_value2[1]
                            else:
                              v_intKey = (v_value2[1])[1]
                            if (v_intKey in v_dictImpl[4]):
                              v_i = v_dictImpl[4][v_intKey]
                              v_bool2 = True
                          if v_bool2:
                            v_len = (v_dictImpl[0] - 1)
                            v_dictImpl[0] = v_len
                            if (v_i == v_len):
                              if (v_keyType == 5):
                                v_dictImpl[5].pop(v_stringKey)
                              else:
                                v_dictImpl[4].pop(v_intKey)
                              del v_dictImpl[6][v_i]
                              del v_dictImpl[7][v_i]
                            else:
                              v_value = v_dictImpl[6][v_len]
                              v_dictImpl[6][v_i] = v_value
                              v_dictImpl[7][v_i] = v_dictImpl[7][v_len]
                              v_dictImpl[6].pop()
                              v_dictImpl[7].pop()
                              if (v_keyType == 5):
                                v_dictImpl[5].pop(v_stringKey)
                                v_stringKey = v_value[1]
                                v_dictImpl[5][v_stringKey] = v_i
                              else:
                                v_dictImpl[4].pop(v_intKey)
                                if (v_keyType == 3):
                                  v_intKey = v_value[1]
                                else:
                                  v_intKey = (v_value[1])[1]
                                v_dictImpl[4][v_intKey] = v_i
                        if not (v_bool2):
                          v_hasInterrupt = v_EX_KeyNotFound(v_ec, "dictionary does not contain the given key.")
                    elif (sc_7 == 7):
                      if (v_argCount > 0):
                        v_hasInterrupt = v_EX_InvalidArgument(v_ec, v_primitiveMethodWrongArgCountError("dictionary values method", 0, v_argCount))
                      else:
                        v_valueList1 = v_dictImpl[7]
                        v_len = len(v_valueList1)
                        v_list1 = v_makeEmptyList(v_dictImpl[3], v_len)
                        v_i = 0
                        while (v_i < v_len):
                          v_addToList(v_list1, v_valueList1[v_i])
                          v_i += 1
                        v_output = [6, v_list1]
                    else:
                      v_output = None
                elif (sc_4 == 3):
                  # ...on a function pointer
                  v_functionPointer1 = v_value[1]
                  sc_9 = swlookup__interpretImpl__9.get(v_functionId, 4)
                  if (sc_9 < 3):
                    if (sc_9 == 0):
                      if (v_argCount > 0):
                        v_hasInterrupt = v_EX_InvalidArgument(v_ec, v_primitiveMethodWrongArgCountError("argCountMax method", 0, v_argCount))
                      else:
                        v_functionId = v_functionPointer1[3]
                        v_functionInfo = v_metadata[10][v_functionId]
                        v_output = v_buildInteger(v_globals, v_functionInfo[4])
                    elif (sc_9 == 1):
                      if (v_argCount > 0):
                        v_hasInterrupt = v_EX_InvalidArgument(v_ec, v_primitiveMethodWrongArgCountError("argCountMin method", 0, v_argCount))
                      else:
                        v_functionId = v_functionPointer1[3]
                        v_functionInfo = v_metadata[10][v_functionId]
                        v_output = v_buildInteger(v_globals, v_functionInfo[3])
                    else:
                      v_functionInfo = v_metadata[10][v_functionPointer1[3]]
                      v_output = v_buildString(v_globals, v_functionInfo[9])
                  elif (sc_9 == 3):
                    if (v_argCount == 1):
                      v_funcArgs[1] = v_funcArgs[0]
                    elif (v_argCount == 0):
                      v_funcArgs[1] = v_VALUE_NULL
                    else:
                      v_hasInterrupt = v_EX_InvalidArgument(v_ec, "invoke requires a list of arguments.")
                    v_funcArgs[0] = v_value
                    v_argCount = 2
                    v_primitiveMethodToCoreLibraryFallback = True
                    v_functionId = v_metadata[15][3]
                    v_output = None
                  else:
                    v_output = None
                elif (sc_4 == 4):
                  # ...on a class definition
                  v_classValue = v_value[1]
                  sc_10 = swlookup__interpretImpl__10.get(v_functionId, 2)
                  if (sc_10 == 0):
                    v_classInfo = v_metadata[9][v_classValue[1]]
                    v_output = v_buildString(v_globals, v_classInfo[16])
                  elif (sc_10 == 1):
                    if (v_argCount != 1):
                      v_hasInterrupt = v_EX_InvalidArgument(v_ec, v_primitiveMethodWrongArgCountError("class isA method", 1, v_argCount))
                    else:
                      v_int1 = v_classValue[1]
                      v_value = v_funcArgs[0]
                      if (v_value[0] != 10):
                        v_hasInterrupt = v_EX_InvalidArgument(v_ec, "class isA method requires another class reference.")
                      else:
                        v_classValue = v_value[1]
                        v_int2 = v_classValue[1]
                        v_output = v_VALUE_FALSE
                        if v_isClassASubclassOf(v_vm, v_int1, v_int2):
                          v_output = v_VALUE_TRUE
                  else:
                    v_output = None
                if not (v_hasInterrupt):
                  if (v_output == None):
                    if v_primitiveMethodToCoreLibraryFallback:
                      v_type = 1
                      v_bool1 = True
                    else:
                      v_hasInterrupt = v_EX_InvalidInvocation(v_ec, "primitive method not found.")
                  else:
                    if v_returnValueUsed:
                      if (v_valueStackSize == v_valueStackCapacity):
                        v_valueStack = v_valueStackIncreaseCapacity(v_ec)
                        v_valueStackCapacity = len(v_valueStack)
                      v_valueStack[v_valueStackSize] = v_output
                      v_valueStackSize += 1
                    v_bool1 = False
              if (v_bool1 and not (v_hasInterrupt)):
                # push a new frame to the stack
                v_stack[0] = v_pc
                v_bool1 = False
                sc_11 = swlookup__interpretImpl__11.get(v_type, 6)
                if (sc_11 < 4):
                  if (sc_11 < 2):
                    if (sc_11 == 0):
                      # function
                      v_functionInfo = v_functionTable[v_functionId]
                      v_pc = v_functionInfo[2]
                      v_value = None
                      v_classId = 0
                    else:
                      # lambda
                      v_pc = v_functionId
                      v_functionInfo = v_metadata[11][v_functionId]
                      v_value = None
                      v_classId = 0
                  elif (sc_11 == 2):
                    # static method
                    v_functionInfo = v_functionTable[v_functionId]
                    v_pc = v_functionInfo[2]
                    v_value = None
                    v_classId = 0
                  else:
                    # non-static method
                    v_functionInfo = v_functionTable[v_functionId]
                    v_pc = v_functionInfo[2]
                    v_classId = 0
                elif (sc_11 == 4):
                  # constructor
                  v_vm[5] += 1
                  v_classInfo = v_classTable[v_classId]
                  v_valueArray1 = (PST_NoneListOfOne * v_classInfo[8])
                  v_i = (len(v_valueArray1) - 1)
                  while (v_i >= 0):
                    sc_12 = swlookup__interpretImpl__12.get(v_classInfo[10][v_i], 3)
                    if (sc_12 < 2):
                      if (sc_12 == 0):
                        v_valueArray1[v_i] = v_classInfo[11][v_i]
                    elif (sc_12 == 2):
                      pass
                    v_i -= 1
                  v_objInstance1 = [v_classId, v_vm[5], v_valueArray1, None, None]
                  v_value = [8, v_objInstance1]
                  v_functionId = v_classInfo[7]
                  v_functionInfo = v_functionTable[v_functionId]
                  v_pc = v_functionInfo[2]
                  v_classId = 0
                  if v_returnValueUsed:
                    v_returnValueUsed = False
                    if (v_valueStackSize == v_valueStackCapacity):
                      v_valueStack = v_valueStackIncreaseCapacity(v_ec)
                      v_valueStackCapacity = len(v_valueStack)
                    v_valueStack[v_valueStackSize] = v_value
                    v_valueStackSize += 1
                elif (sc_11 == 5):
                  # base constructor
                  v_value = v_stack[6]
                  v_classInfo = v_classTable[v_classId]
                  v_functionId = v_classInfo[7]
                  v_functionInfo = v_functionTable[v_functionId]
                  v_pc = v_functionInfo[2]
                  v_classId = 0
                if ((v_argCount < v_functionInfo[3]) or (v_argCount > v_functionInfo[4])):
                  v_pc = v_stack[0]
                  v_hasInterrupt = v_EX_InvalidArgument(v_ec, "Incorrect number of args were passed to this function.")
                else:
                  v_int1 = v_functionInfo[7]
                  v_int2 = v_stack[3]
                  if (v_localsStackCapacity <= (v_int2 + v_int1)):
                    v_increaseLocalsStackCapacity(v_ec, v_int1)
                    v_localsStack = v_ec[5]
                    v_localsStackSet = v_ec[6]
                    v_localsStackCapacity = len(v_localsStack)
                  v_localsStackSetToken = (v_ec[7] + 1)
                  v_ec[7] = v_localsStackSetToken
                  if (v_localsStackSetToken > 2000000000):
                    v_resetLocalsStackTokens(v_ec, v_stack)
                    v_localsStackSetToken = 2
                  v_localsStackOffset = v_int2
                  if (v_type != 10):
                    v_closure = None
                  # invoke the function
                  v_stack = [v_pc, v_localsStackSetToken, v_localsStackOffset, (v_localsStackOffset + v_int1), v_stack, v_returnValueUsed, v_value, v_valueStackSize, 0, (v_stack[9] + 1), 0, None, v_closure, None]
                  v_i = 0
                  while (v_i < v_argCount):
                    v_int1 = (v_localsStackOffset + v_i)
                    v_localsStack[v_int1] = v_funcArgs[v_i]
                    v_localsStackSet[v_int1] = v_localsStackSetToken
                    v_i += 1
                  if (v_argCount != v_functionInfo[3]):
                    v_int1 = (v_argCount - v_functionInfo[3])
                    if (v_int1 > 0):
                      v_pc += v_functionInfo[8][v_int1]
                      v_stack[0] = v_pc
                  if (v_stack[9] > 1000):
                    v_hasInterrupt = v_EX_Fatal(v_ec, "Stack overflow.")
        elif (sc_0 < 15):
          if (sc_0 == 13):
            # CAST
            v_value = v_valueStack[(v_valueStackSize - 1)]
            v_value2 = v_canAssignTypeToGeneric(v_vm, v_value, v_row, 0)
            if (v_value2 == None):
              if ((v_value[0] == 4) and (v_row[0] == 3)):
                if (v_row[1] == 1):
                  v_float1 = v_value[1]
                  if ((v_float1 < 0) and ((v_float1 % 1) != 0)):
                    v_i = (int(v_float1) - 1)
                  else:
                    v_i = int(v_float1)
                  if (v_i < 0):
                    if (v_i > -257):
                      v_value2 = v_globals[10][-v_i]
                    else:
                      v_value2 = [3, v_i]
                  elif (v_i < 2049):
                    v_value2 = v_globals[9][v_i]
                  else:
                    v_value2 = [3, v_i]
              elif ((v_value[0] == 3) and (v_row[0] == 4)):
                v_int1 = v_value[1]
                if (v_int1 == 0):
                  v_value2 = v_VALUE_FLOAT_ZERO
                else:
                  v_value2 = [4, (0.0 + v_int1)]
              if (v_value2 != None):
                v_valueStack[(v_valueStackSize - 1)] = v_value2
            if (v_value2 == None):
              v_hasInterrupt = v_EX_InvalidArgument(v_ec, ''.join(["Cannot convert a ", v_typeToStringFromValue(v_vm, v_value), " to a ", v_typeToString(v_vm, v_row, 0)]))
            else:
              v_valueStack[(v_valueStackSize - 1)] = v_value2
          else:
            # CLASS_DEFINITION
            v_initializeClass(v_pc, v_vm, v_row, v_stringArgs[v_pc])
            v_classTable = v_metadata[9]
        elif (sc_0 == 15):
          # CNI_INVOKE
          v_nativeFp = v_metadata[13][v_row[0]]
          if (v_nativeFp == None):
            v_hasInterrupt = v_EX_InvalidInvocation(v_ec, "CNI method could not be found.")
          else:
            v_len = v_row[1]
            v_valueStackSize -= v_len
            v_valueArray1 = (PST_NoneListOfOne * v_len)
            v_i = 0
            while (v_i < v_len):
              v_valueArray1[v_i] = v_valueStack[(v_valueStackSize + v_i)]
              v_i += 1
            v_prepareToSuspend(v_ec, v_stack, v_valueStackSize, v_pc)
            v_value = v_nativeFp(v_vm, v_valueArray1)
            if v_ec[11]:
              v_ec[11] = False
              if (v_ec[12] == 1):
                return v_suspendInterpreter()
            if (v_row[2] == 1):
              if (v_valueStackSize == v_valueStackCapacity):
                v_valueStack = v_valueStackIncreaseCapacity(v_ec)
                v_valueStackCapacity = len(v_valueStack)
              v_valueStack[v_valueStackSize] = v_value
              v_valueStackSize += 1
        else:
          # CNI_REGISTER
          v_nativeFp = TranslationHelper_getFunction(v_stringArgs[v_pc])
          v_metadata[13][v_row[0]] = v_nativeFp
      elif (sc_0 < 26):
        if (sc_0 < 22):
          if (sc_0 < 20):
            if (sc_0 == 17):
              # COMMAND_LINE_ARGS
              if (v_valueStackSize == v_valueStackCapacity):
                v_valueStack = v_valueStackIncreaseCapacity(v_ec)
                v_valueStackCapacity = len(v_valueStack)
              v_list1 = v_makeEmptyList(v_globals[14], 3)
              v_i = 0
              while (v_i < len(v_vm[11][0])):
                v_addToList(v_list1, v_buildString(v_globals, v_vm[11][0][v_i]))
                v_i += 1
              v_valueStack[v_valueStackSize] = [6, v_list1]
              v_valueStackSize += 1
            elif (sc_0 == 18):
              # CONTINUE
              if (v_row[0] == 1):
                v_pc += v_row[1]
              else:
                v_intArray1 = v_esfData[v_pc]
                v_pc = (v_intArray1[1] - 1)
                v_valueStackSize = v_stack[7]
                v_stack[10] = 2
            else:
              # CORE_FUNCTION
              sc_13 = swlookup__interpretImpl__13.get(v_row[0], 41)
              if (sc_13 < 21):
                if (sc_13 < 11):
                  if (sc_13 < 6):
                    if (sc_13 < 3):
                      if (sc_13 == 0):
                        # parseInt
                        v_valueStackSize -= 1
                        v_arg1 = v_valueStack[v_valueStackSize]
                        v_output = v_VALUE_NULL
                        if (v_arg1[0] == 5):
                          v_string1 = (v_arg1[1]).strip()
                          if PST_isValidInteger(v_string1):
                            v_output = v_buildInteger(v_globals, int(v_string1))
                        else:
                          v_hasInterrupt = v_EX_InvalidArgument(v_ec, "parseInt requires a string argument.")
                      elif (sc_13 == 1):
                        # parseFloat
                        v_valueStackSize -= 1
                        v_arg1 = v_valueStack[v_valueStackSize]
                        v_output = v_VALUE_NULL
                        if (v_arg1[0] == 5):
                          v_string1 = (v_arg1[1]).strip()
                          PST_tryParseFloat(v_string1, v_floatList1)
                          if (v_floatList1[0] >= 0):
                            v_output = v_buildFloat(v_globals, v_floatList1[1])
                        else:
                          v_hasInterrupt = v_EX_InvalidArgument(v_ec, "parseFloat requires a string argument.")
                      else:
                        # print
                        v_valueStackSize -= 1
                        v_arg1 = v_valueStack[v_valueStackSize]
                        v_output = v_VALUE_NULL
                        v_printToStdOut(v_vm[11][2], v_valueToString(v_vm, v_arg1))
                    elif (sc_13 == 3):
                      # typeof
                      v_valueStackSize -= 1
                      v_arg1 = v_valueStack[v_valueStackSize]
                      v_output = v_buildInteger(v_globals, (v_arg1[0] - 1))
                    elif (sc_13 == 4):
                      # typeis
                      v_valueStackSize -= 1
                      v_arg1 = v_valueStack[v_valueStackSize]
                      v_int1 = v_arg1[0]
                      v_int2 = v_row[2]
                      v_output = v_VALUE_FALSE
                      while (v_int2 > 0):
                        if (v_row[(2 + v_int2)] == v_int1):
                          v_output = v_VALUE_TRUE
                          v_int2 = 0
                        else:
                          v_int2 -= 1
                    else:
                      # execId
                      v_output = v_buildInteger(v_globals, v_ec[0])
                  elif (sc_13 < 9):
                    if (sc_13 == 6):
                      # assert
                      v_valueStackSize -= 3
                      v_arg3 = v_valueStack[(v_valueStackSize + 2)]
                      v_arg2 = v_valueStack[(v_valueStackSize + 1)]
                      v_arg1 = v_valueStack[v_valueStackSize]
                      if (v_arg1[0] != 2):
                        v_hasInterrupt = v_EX_InvalidArgument(v_ec, "Assertion expression must be a boolean.")
                      elif v_arg1[1]:
                        v_output = v_VALUE_NULL
                      else:
                        v_string1 = v_valueToString(v_vm, v_arg2)
                        if v_arg3[1]:
                          v_string1 = "Assertion failed: " + v_string1
                        v_hasInterrupt = v_EX_AssertionFailed(v_ec, v_string1)
                    elif (sc_13 == 7):
                      # chr
                      v_valueStackSize -= 1
                      v_arg1 = v_valueStack[v_valueStackSize]
                      v_output = None
                      if (v_arg1[0] == 3):
                        v_int1 = v_arg1[1]
                        if ((v_int1 >= 0) and (v_int1 < 256)):
                          v_output = v_buildCommonString(v_globals, chr(v_int1))
                      if (v_output == None):
                        v_hasInterrupt = v_EX_InvalidArgument(v_ec, "chr requires an integer between 0 and 255.")
                    else:
                      # ord
                      v_valueStackSize -= 1
                      v_arg1 = v_valueStack[v_valueStackSize]
                      v_output = None
                      if (v_arg1[0] == 5):
                        v_string1 = v_arg1[1]
                        if (len(v_string1) == 1):
                          v_output = v_buildInteger(v_globals, ord(v_string1[0]))
                      if (v_output == None):
                        v_hasInterrupt = v_EX_InvalidArgument(v_ec, "ord requires a 1 character string.")
                  elif (sc_13 == 9):
                    # currentTime
                    v_output = v_buildFloat(v_globals, time.time())
                  else:
                    # sortList
                    v_valueStackSize -= 2
                    v_arg2 = v_valueStack[(v_valueStackSize + 1)]
                    v_arg1 = v_valueStack[v_valueStackSize]
                    v_output = v_VALUE_NULL
                    v_list1 = v_arg1[1]
                    v_list2 = v_arg2[1]
                    v_sortLists(v_list2, v_list1, PST_IntBuffer16)
                    if (PST_IntBuffer16[0] > 0):
                      v_hasInterrupt = v_EX_InvalidArgument(v_ec, "Invalid sort keys. Keys must be all numbers or all strings, but not mixed.")
                elif (sc_13 < 16):
                  if (sc_13 < 14):
                    if (sc_13 == 11):
                      # abs
                      v_valueStackSize -= 1
                      v_arg1 = v_valueStack[v_valueStackSize]
                      v_output = v_arg1
                      if (v_arg1[0] == 3):
                        if (v_arg1[1] < 0):
                          v_output = v_buildInteger(v_globals, -(v_arg1[1]))
                      elif (v_arg1[0] == 4):
                        if (v_arg1[1] < 0):
                          v_output = v_buildFloat(v_globals, -(v_arg1[1]))
                      else:
                        v_hasInterrupt = v_EX_InvalidArgument(v_ec, "abs requires a number as input.")
                    elif (sc_13 == 12):
                      # arcCos
                      v_valueStackSize -= 1
                      v_arg1 = v_valueStack[v_valueStackSize]
                      if (v_arg1[0] == 4):
                        v_float1 = v_arg1[1]
                      elif (v_arg1[0] == 3):
                        v_float1 = (0.0 + v_arg1[1])
                      else:
                        v_hasInterrupt = v_EX_InvalidArgument(v_ec, "arccos requires a number as input.")
                      if not (v_hasInterrupt):
                        if ((v_float1 < -1) or (v_float1 > 1)):
                          v_hasInterrupt = v_EX_InvalidArgument(v_ec, "arccos requires a number in the range of -1 to 1.")
                        else:
                          v_output = v_buildFloat(v_globals, math.acos(v_float1))
                    else:
                      # arcSin
                      v_valueStackSize -= 1
                      v_arg1 = v_valueStack[v_valueStackSize]
                      if (v_arg1[0] == 4):
                        v_float1 = v_arg1[1]
                      elif (v_arg1[0] == 3):
                        v_float1 = (0.0 + v_arg1[1])
                      else:
                        v_hasInterrupt = v_EX_InvalidArgument(v_ec, "arcsin requires a number as input.")
                      if not (v_hasInterrupt):
                        if ((v_float1 < -1) or (v_float1 > 1)):
                          v_hasInterrupt = v_EX_InvalidArgument(v_ec, "arcsin requires a number in the range of -1 to 1.")
                        else:
                          v_output = v_buildFloat(v_globals, math.asin(v_float1))
                  elif (sc_13 == 14):
                    # arcTan
                    v_valueStackSize -= 2
                    v_arg2 = v_valueStack[(v_valueStackSize + 1)]
                    v_arg1 = v_valueStack[v_valueStackSize]
                    v_bool1 = False
                    if (v_arg1[0] == 4):
                      v_float1 = v_arg1[1]
                    elif (v_arg1[0] == 3):
                      v_float1 = (0.0 + v_arg1[1])
                    else:
                      v_bool1 = True
                    if (v_arg2[0] == 4):
                      v_float2 = v_arg2[1]
                    elif (v_arg2[0] == 3):
                      v_float2 = (0.0 + v_arg2[1])
                    else:
                      v_bool1 = True
                    if v_bool1:
                      v_hasInterrupt = v_EX_InvalidArgument(v_ec, "arctan requires numeric arguments.")
                    else:
                      v_output = v_buildFloat(v_globals, math.atan2(v_float1, v_float2))
                  else:
                    # cos
                    v_valueStackSize -= 1
                    v_arg1 = v_valueStack[v_valueStackSize]
                    if (v_arg1[0] == 4):
                      v_float1 = v_arg1[1]
                      v_output = v_buildFloat(v_globals, math.cos(v_float1))
                    elif (v_arg1[0] == 3):
                      v_int1 = v_arg1[1]
                      v_output = v_buildFloat(v_globals, math.cos(v_int1))
                    else:
                      v_hasInterrupt = v_EX_InvalidArgument(v_ec, "cos requires a number argument.")
                elif (sc_13 < 19):
                  if (sc_13 == 16):
                    # ensureRange
                    v_valueStackSize -= 3
                    v_arg3 = v_valueStack[(v_valueStackSize + 2)]
                    v_arg2 = v_valueStack[(v_valueStackSize + 1)]
                    v_arg1 = v_valueStack[v_valueStackSize]
                    v_bool1 = False
                    if (v_arg2[0] == 4):
                      v_float2 = v_arg2[1]
                    elif (v_arg2[0] == 3):
                      v_float2 = (0.0 + v_arg2[1])
                    else:
                      v_bool1 = True
                    if (v_arg3[0] == 4):
                      v_float3 = v_arg3[1]
                    elif (v_arg3[0] == 3):
                      v_float3 = (0.0 + v_arg3[1])
                    else:
                      v_bool1 = True
                    if (not (v_bool1) and (v_float3 < v_float2)):
                      v_float1 = v_float3
                      v_float3 = v_float2
                      v_float2 = v_float1
                      v_value = v_arg2
                      v_arg2 = v_arg3
                      v_arg3 = v_value
                    if (v_arg1[0] == 4):
                      v_float1 = v_arg1[1]
                    elif (v_arg1[0] == 3):
                      v_float1 = (0.0 + v_arg1[1])
                    else:
                      v_bool1 = True
                    if v_bool1:
                      v_hasInterrupt = v_EX_InvalidArgument(v_ec, "ensureRange requires numeric arguments.")
                    elif (v_float1 < v_float2):
                      v_output = v_arg2
                    elif (v_float1 > v_float3):
                      v_output = v_arg3
                    else:
                      v_output = v_arg1
                  elif (sc_13 == 17):
                    # floor
                    v_valueStackSize -= 1
                    v_arg1 = v_valueStack[v_valueStackSize]
                    if (v_arg1[0] == 4):
                      v_float1 = v_arg1[1]
                      if ((v_float1 < 0) and ((v_float1 % 1) != 0)):
                        v_int1 = (int(v_float1) - 1)
                      else:
                        v_int1 = int(v_float1)
                      if (v_int1 < 2049):
                        if (v_int1 >= 0):
                          v_output = v_INTEGER_POSITIVE_CACHE[v_int1]
                        elif (v_int1 > -257):
                          v_output = v_INTEGER_NEGATIVE_CACHE[-v_int1]
                        else:
                          v_output = [3, v_int1]
                      else:
                        v_output = [3, v_int1]
                    elif (v_arg1[0] == 3):
                      v_output = v_arg1
                    else:
                      v_hasInterrupt = v_EX_InvalidArgument(v_ec, "floor expects a numeric argument.")
                  else:
                    # max
                    v_valueStackSize -= 2
                    v_arg2 = v_valueStack[(v_valueStackSize + 1)]
                    v_arg1 = v_valueStack[v_valueStackSize]
                    v_bool1 = False
                    if (v_arg1[0] == 4):
                      v_float1 = v_arg1[1]
                    elif (v_arg1[0] == 3):
                      v_float1 = (0.0 + v_arg1[1])
                    else:
                      v_bool1 = True
                    if (v_arg2[0] == 4):
                      v_float2 = v_arg2[1]
                    elif (v_arg2[0] == 3):
                      v_float2 = (0.0 + v_arg2[1])
                    else:
                      v_bool1 = True
                    if v_bool1:
                      v_hasInterrupt = v_EX_InvalidArgument(v_ec, "max requires numeric arguments.")
                    elif (v_float1 >= v_float2):
                      v_output = v_arg1
                    else:
                      v_output = v_arg2
                elif (sc_13 == 19):
                  # min
                  v_valueStackSize -= 2
                  v_arg2 = v_valueStack[(v_valueStackSize + 1)]
                  v_arg1 = v_valueStack[v_valueStackSize]
                  v_bool1 = False
                  if (v_arg1[0] == 4):
                    v_float1 = v_arg1[1]
                  elif (v_arg1[0] == 3):
                    v_float1 = (0.0 + v_arg1[1])
                  else:
                    v_bool1 = True
                  if (v_arg2[0] == 4):
                    v_float2 = v_arg2[1]
                  elif (v_arg2[0] == 3):
                    v_float2 = (0.0 + v_arg2[1])
                  else:
                    v_bool1 = True
                  if v_bool1:
                    v_hasInterrupt = v_EX_InvalidArgument(v_ec, "min requires numeric arguments.")
                  elif (v_float1 <= v_float2):
                    v_output = v_arg1
                  else:
                    v_output = v_arg2
                else:
                  # nativeInt
                  v_valueStackSize -= 2
                  v_arg2 = v_valueStack[(v_valueStackSize + 1)]
                  v_arg1 = v_valueStack[v_valueStackSize]
                  v_output = v_buildInteger(v_globals, (v_arg1[1])[3][v_arg2[1]])
              elif (sc_13 < 32):
                if (sc_13 < 27):
                  if (sc_13 < 24):
                    if (sc_13 == 21):
                      # nativeString
                      v_valueStackSize -= 3
                      v_arg3 = v_valueStack[(v_valueStackSize + 2)]
                      v_arg2 = v_valueStack[(v_valueStackSize + 1)]
                      v_arg1 = v_valueStack[v_valueStackSize]
                      v_string1 = (v_arg1[1])[3][v_arg2[1]]
                      if v_arg3[1]:
                        v_output = v_buildCommonString(v_globals, v_string1)
                      else:
                        v_output = v_buildString(v_globals, v_string1)
                    elif (sc_13 == 22):
                      # sign
                      v_valueStackSize -= 1
                      v_arg1 = v_valueStack[v_valueStackSize]
                      if (v_arg1[0] == 3):
                        v_float1 = (0.0 + (v_arg1[1]))
                      elif (v_arg1[0] == 4):
                        v_float1 = v_arg1[1]
                      else:
                        v_hasInterrupt = v_EX_InvalidArgument(v_ec, "sign requires a number as input.")
                      if (v_float1 == 0):
                        v_output = v_VALUE_INT_ZERO
                      elif (v_float1 > 0):
                        v_output = v_VALUE_INT_ONE
                      else:
                        v_output = v_INTEGER_NEGATIVE_CACHE[1]
                    else:
                      # sin
                      v_valueStackSize -= 1
                      v_arg1 = v_valueStack[v_valueStackSize]
                      if (v_arg1[0] == 4):
                        v_float1 = v_arg1[1]
                      elif (v_arg1[0] == 3):
                        v_float1 = (0.0 + v_arg1[1])
                      else:
                        v_hasInterrupt = v_EX_InvalidArgument(v_ec, "sin requires a number argument.")
                      v_output = v_buildFloat(v_globals, math.sin(v_float1))
                  elif (sc_13 == 24):
                    # tan
                    v_valueStackSize -= 1
                    v_arg1 = v_valueStack[v_valueStackSize]
                    if (v_arg1[0] == 4):
                      v_float1 = v_arg1[1]
                    elif (v_arg1[0] == 3):
                      v_float1 = (0.0 + v_arg1[1])
                    else:
                      v_hasInterrupt = v_EX_InvalidArgument(v_ec, "tan requires a number argument.")
                    if not (v_hasInterrupt):
                      v_float2 = math.cos(v_float1)
                      if (v_float2 < 0):
                        v_float2 = -v_float2
                      if (v_float2 < 0.00000000015):
                        v_hasInterrupt = v_EX_DivisionByZero(v_ec, "Tangent is undefined.")
                      else:
                        v_output = v_buildFloat(v_globals, math.tan(v_float1))
                  elif (sc_13 == 25):
                    # log
                    v_valueStackSize -= 2
                    v_arg2 = v_valueStack[(v_valueStackSize + 1)]
                    v_arg1 = v_valueStack[v_valueStackSize]
                    if (v_arg1[0] == 4):
                      v_float1 = v_arg1[1]
                    elif (v_arg1[0] == 3):
                      v_float1 = (0.0 + v_arg1[1])
                    else:
                      v_hasInterrupt = v_EX_InvalidArgument(v_ec, "logarithms require a number argument.")
                    if not (v_hasInterrupt):
                      if (v_float1 <= 0):
                        v_hasInterrupt = v_EX_InvalidArgument(v_ec, "logarithms require positive inputs.")
                      else:
                        v_output = v_buildFloat(v_globals, v_fixFuzzyFloatPrecision((math.log(v_float1) * v_arg2[1])))
                  else:
                    # intQueueClear
                    v_valueStackSize -= 1
                    v_arg1 = v_valueStack[v_valueStackSize]
                    v_output = v_VALUE_NULL
                    v_objInstance1 = v_arg1[1]
                    if (v_objInstance1[3] != None):
                      v_objInstance1[3][1] = 0
                elif (sc_13 < 30):
                  if (sc_13 == 27):
                    # intQueueWrite16
                    v_output = v_VALUE_NULL
                    v_int1 = v_row[2]
                    v_valueStackSize -= (v_int1 + 1)
                    v_value = v_valueStack[v_valueStackSize]
                    v_objArray1 = (v_value[1])[3]
                    v_intArray1 = v_objArray1[0]
                    v_len = v_objArray1[1]
                    if (v_len >= len(v_intArray1)):
                      v_intArray2 = (PST_NoneListOfOne * ((v_len * 2) + 16))
                      v_j = 0
                      while (v_j < v_len):
                        v_intArray2[v_j] = v_intArray1[v_j]
                        v_j += 1
                      v_intArray1 = v_intArray2
                      v_objArray1[0] = v_intArray1
                    v_objArray1[1] = (v_len + 16)
                    v_i = (v_int1 - 1)
                    while (v_i >= 0):
                      v_value = v_valueStack[((v_valueStackSize + 1) + v_i)]
                      if (v_value[0] == 3):
                        v_intArray1[(v_len + v_i)] = v_value[1]
                      elif (v_value[0] == 4):
                        v_float1 = (0.5 + v_value[1])
                        v_intArray1[(v_len + v_i)] = int(v_float1)
                      else:
                        v_hasInterrupt = v_EX_InvalidArgument(v_ec, "Input must be integers.")
                        v_i = -1
                      v_i -= 1
                  elif (sc_13 == 28):
                    # execCounter
                    v_output = v_buildInteger(v_globals, v_ec[8])
                  else:
                    # sleep
                    v_valueStackSize -= 1
                    v_arg1 = v_valueStack[v_valueStackSize]
                    v_float1 = v_getFloat(v_arg1)
                    if (v_row[1] == 1):
                      if (v_valueStackSize == v_valueStackCapacity):
                        v_valueStack = v_valueStackIncreaseCapacity(v_ec)
                        v_valueStackCapacity = len(v_valueStack)
                      v_valueStack[v_valueStackSize] = v_VALUE_NULL
                      v_valueStackSize += 1
                    v_prepareToSuspend(v_ec, v_stack, v_valueStackSize, v_pc)
                    v_ec[13] = [3, 0, "", v_float1, None]
                    v_hasInterrupt = True
                elif (sc_13 == 30):
                  # projectId
                  v_output = v_buildCommonString(v_globals, v_metadata[17])
                else:
                  # isJavaScript
                  v_output = v_VALUE_FALSE
              elif (sc_13 < 37):
                if (sc_13 < 35):
                  if (sc_13 == 32):
                    # isAndroid
                    v_output = v_VALUE_FALSE
                  elif (sc_13 == 33):
                    # allocNativeData
                    v_valueStackSize -= 2
                    v_arg2 = v_valueStack[(v_valueStackSize + 1)]
                    v_arg1 = v_valueStack[v_valueStackSize]
                    v_objInstance1 = v_arg1[1]
                    v_int1 = v_arg2[1]
                    v_objArray1 = (PST_NoneListOfOne * v_int1)
                    v_objInstance1[3] = v_objArray1
                  else:
                    # setNativeData
                    v_valueStackSize -= 3
                    v_arg3 = v_valueStack[(v_valueStackSize + 2)]
                    v_arg2 = v_valueStack[(v_valueStackSize + 1)]
                    v_arg1 = v_valueStack[v_valueStackSize]
                    (v_arg1[1])[3][v_arg2[1]] = v_arg3[1]
                elif (sc_13 == 35):
                  # getExceptionTrace
                  v_valueStackSize -= 1
                  v_arg1 = v_valueStack[v_valueStackSize]
                  v_intList1 = v_getNativeDataItem(v_arg1, 1)
                  v_list1 = v_makeEmptyList(v_globals[14], 20)
                  v_output = [6, v_list1]
                  if (v_intList1 != None):
                    v_stringList1 = v_tokenHelperConvertPcsToStackTraceStrings(v_vm, v_intList1)
                    v_i = 0
                    while (v_i < len(v_stringList1)):
                      v_addToList(v_list1, v_buildString(v_globals, v_stringList1[v_i]))
                      v_i += 1
                    v_reverseList(v_list1)
                else:
                  # reflectAllClasses
                  v_output = v_Reflect_allClasses(v_vm)
              elif (sc_13 < 40):
                if (sc_13 == 37):
                  # reflectGetMethods
                  v_valueStackSize -= 1
                  v_arg1 = v_valueStack[v_valueStackSize]
                  v_output = v_Reflect_getMethods(v_vm, v_ec, v_arg1)
                  v_hasInterrupt = (v_ec[13] != None)
                elif (sc_13 == 38):
                  # reflectGetClass
                  v_valueStackSize -= 1
                  v_arg1 = v_valueStack[v_valueStackSize]
                  if (v_arg1[0] != 8):
                    v_hasInterrupt = v_EX_InvalidArgument(v_ec, "Cannot get class from non-instance types.")
                  else:
                    v_objInstance1 = v_arg1[1]
                    v_output = [10, [False, v_objInstance1[0]]]
                else:
                  # convertFloatArgsToInts
                  v_int1 = v_stack[3]
                  v_i = v_localsStackOffset
                  while (v_i < v_int1):
                    v_value = v_localsStack[v_i]
                    if (v_localsStackSet[v_i] != v_localsStackSetToken):
                      v_i += v_int1
                    elif (v_value[0] == 4):
                      v_float1 = v_value[1]
                      if ((v_float1 < 0) and ((v_float1 % 1) != 0)):
                        v_int2 = (int(v_float1) - 1)
                      else:
                        v_int2 = int(v_float1)
                      if ((v_int2 >= 0) and (v_int2 < 2049)):
                        v_localsStack[v_i] = v_INTEGER_POSITIVE_CACHE[v_int2]
                      else:
                        v_localsStack[v_i] = v_buildInteger(v_globals, v_int2)
                    v_i += 1
              elif (sc_13 == 40):
                # addShutdownHandler
                v_valueStackSize -= 1
                v_arg1 = v_valueStack[v_valueStackSize]
                v_vm[10].append(v_arg1)
              if (v_row[1] == 1):
                if (v_valueStackSize == v_valueStackCapacity):
                  v_valueStack = v_valueStackIncreaseCapacity(v_ec)
                  v_valueStackCapacity = len(v_valueStack)
                v_valueStack[v_valueStackSize] = v_output
                v_valueStackSize += 1
          elif (sc_0 == 20):
            # DEBUG_SYMBOLS
            v_applyDebugSymbolData(v_vm, v_row, v_stringArgs[v_pc], v_metadata[22])
          else:
            # DEF_DICT
            v_intIntDict1 = {}
            v_stringIntDict1 = {}
            v_valueList2 = []
            v_valueList1 = []
            v_len = v_row[0]
            v_type = 3
            v_first = True
            v_i = v_len
            while (v_i > 0):
              v_valueStackSize -= 2
              v_value = v_valueStack[(v_valueStackSize + 1)]
              v_value2 = v_valueStack[v_valueStackSize]
              if v_first:
                v_type = v_value2[0]
                v_first = False
              elif (v_type != v_value2[0]):
                v_hasInterrupt = v_EX_InvalidKey(v_ec, "Dictionary keys must be of the same type.")
              if not (v_hasInterrupt):
                if (v_type == 3):
                  v_intKey = v_value2[1]
                elif (v_type == 5):
                  v_stringKey = v_value2[1]
                elif (v_type == 8):
                  v_objInstance1 = v_value2[1]
                  v_intKey = v_objInstance1[1]
                else:
                  v_hasInterrupt = v_EX_InvalidKey(v_ec, "Only integers, strings, and objects can be used as dictionary keys.")
              if not (v_hasInterrupt):
                if (v_type == 5):
                  v_stringIntDict1[v_stringKey] = len(v_valueList1)
                else:
                  v_intIntDict1[v_intKey] = len(v_valueList1)
                v_valueList2.append(v_value2)
                v_valueList1.append(v_value)
                v_i -= 1
            if not (v_hasInterrupt):
              if (v_type == 5):
                v_i = len(v_stringIntDict1)
              else:
                v_i = len(v_intIntDict1)
              if (v_i != v_len):
                v_hasInterrupt = v_EX_InvalidKey(v_ec, "Key collision")
            if not (v_hasInterrupt):
              v_i = v_row[1]
              v_classId = 0
              if (v_i > 0):
                v_type = v_row[2]
                if (v_type == 8):
                  v_classId = v_row[3]
                v_int1 = len(v_row)
                v_intArray1 = (PST_NoneListOfOne * (v_int1 - v_i))
                while (v_i < v_int1):
                  v_intArray1[(v_i - v_row[1])] = v_row[v_i]
                  v_i += 1
              else:
                v_intArray1 = None
              if (v_valueStackSize == v_valueStackCapacity):
                v_valueStack = v_valueStackIncreaseCapacity(v_ec)
                v_valueStackCapacity = len(v_valueStack)
              v_valueStack[v_valueStackSize] = [7, [v_len, v_type, v_classId, v_intArray1, v_intIntDict1, v_stringIntDict1, v_valueList2, v_valueList1]]
              v_valueStackSize += 1
        elif (sc_0 < 24):
          if (sc_0 == 22):
            # DEF_LIST
            v_int1 = v_row[0]
            v_list1 = v_makeEmptyList(None, v_int1)
            if (v_row[1] != 0):
              v_list1[0] = (PST_NoneListOfOne * (len(v_row) - 1))
              v_i = 1
              while (v_i < len(v_row)):
                v_list1[0][(v_i - 1)] = v_row[v_i]
                v_i += 1
            v_list1[1] = v_int1
            while (v_int1 > 0):
              v_valueStackSize -= 1
              v_list1[2].append(v_valueStack[v_valueStackSize])
              v_int1 -= 1
            v_list1[2].reverse()
            v_value = [6, v_list1]
            if (v_valueStackSize == v_valueStackCapacity):
              v_valueStack = v_valueStackIncreaseCapacity(v_ec)
              v_valueStackCapacity = len(v_valueStack)
            v_valueStack[v_valueStackSize] = v_value
            v_valueStackSize += 1
          else:
            # DEF_ORIGINAL_CODE
            v_defOriginalCodeImpl(v_vm, v_row, v_stringArgs[v_pc])
        elif (sc_0 == 24):
          # DEREF_CLOSURE
          v_bool1 = True
          v_closure = v_stack[12]
          v_i = v_row[0]
          if ((v_closure != None) and (v_i in v_closure)):
            v_value = v_closure[v_i][0]
            if (v_value != None):
              v_bool1 = False
              if (v_valueStackSize == v_valueStackCapacity):
                v_valueStack = v_valueStackIncreaseCapacity(v_ec)
                v_valueStackCapacity = len(v_valueStack)
              v_valueStack[v_valueStackSize] = v_value
              v_valueStackSize += 1
          if v_bool1:
            v_hasInterrupt = v_EX_UnassignedVariable(v_ec, "Variable used before it was set.")
        else:
          # DEREF_DOT
          v_value = v_valueStack[(v_valueStackSize - 1)]
          v_nameId = v_row[0]
          v_int2 = v_row[1]
          sc_14 = swlookup__interpretImpl__14.get(v_value[0], 4)
          if (sc_14 < 3):
            if (sc_14 == 0):
              v_objInstance1 = v_value[1]
              v_classId = v_objInstance1[0]
              v_classInfo = v_classTable[v_classId]
              if (v_classId == v_row[4]):
                v_int1 = v_row[5]
              else:
                v_intIntDict1 = v_classInfo[14]
                v_int1 = v_intIntDict1.get(v_int2, -1)
                v_int3 = v_classInfo[12][v_int1]
                if (v_int3 > 1):
                  if (v_int3 == 2):
                    if (v_classId != v_row[2]):
                      v_int1 = -2
                  else:
                    if ((v_int3 == 3) or (v_int3 == 5)):
                      if (v_classInfo[3] != v_row[3]):
                        v_int1 = -3
                    if ((v_int3 == 4) or (v_int3 == 5)):
                      v_i = v_row[2]
                      if (v_classId == v_i):
                        pass
                      else:
                        v_classInfo = v_classTable[v_classInfo[0]]
                        while ((v_classInfo[2] != -1) and (v_int1 < len(v_classTable[v_classInfo[2]][12]))):
                          v_classInfo = v_classTable[v_classInfo[2]]
                        v_j = v_classInfo[0]
                        if (v_j != v_i):
                          v_bool1 = False
                          while ((v_i != -1) and (v_classTable[v_i][2] != -1)):
                            v_i = v_classTable[v_i][2]
                            if (v_i == v_j):
                              v_bool1 = True
                              v_i = -1
                          if not (v_bool1):
                            v_int1 = -4
                      v_classInfo = v_classTable[v_classId]
                v_row[4] = v_objInstance1[0]
                v_row[5] = v_int1
              if (v_int1 > -1):
                v_functionId = v_classInfo[9][v_int1]
                if (v_functionId == -1):
                  v_output = v_objInstance1[2][v_int1]
                else:
                  v_output = [9, [2, v_value, v_objInstance1[0], v_functionId, None]]
              else:
                v_output = None
            elif (sc_14 == 1):
              if (v_metadata[14] == v_nameId):
                v_output = v_buildInteger(v_globals, len((v_value[1])))
              else:
                v_output = None
            elif (v_metadata[14] == v_nameId):
              v_output = v_buildInteger(v_globals, (v_value[1])[1])
            else:
              v_output = None
          elif (sc_14 == 3):
            if (v_metadata[14] == v_nameId):
              v_output = v_buildInteger(v_globals, (v_value[1])[0])
            else:
              v_output = None
          elif (v_value[0] == 1):
            v_hasInterrupt = v_EX_NullReference(v_ec, "Derferenced a field from null.")
            v_output = v_VALUE_NULL
          else:
            v_output = None
          if (v_output == None):
            v_output = v_generatePrimitiveMethodReference(v_globalNameIdToPrimitiveMethodName, v_nameId, v_value)
            if (v_output == None):
              if (v_value[0] == 1):
                v_hasInterrupt = v_EX_NullReference(v_ec, "Tried to dereference a field on null.")
              elif ((v_value[0] == 8) and (v_int1 < -1)):
                v_string1 = v_identifiers[v_row[0]]
                if (v_int1 == -2):
                  v_string2 = "private"
                elif (v_int1 == -3):
                  v_string2 = "internal"
                else:
                  v_string2 = "protected"
                v_hasInterrupt = v_EX_UnknownField(v_ec, ''.join(["The field '", v_string1, "' is marked as ", v_string2, " and cannot be accessed from here."]))
              else:
                if (v_value[0] == 8):
                  v_classId = (v_value[1])[0]
                  v_classInfo = v_classTable[v_classId]
                  v_string1 = v_classInfo[16] + " instance"
                else:
                  v_string1 = v_getTypeFromId(v_value[0])
                v_hasInterrupt = v_EX_UnknownField(v_ec, v_string1 + " does not have that field.")
          v_valueStack[(v_valueStackSize - 1)] = v_output
      elif (sc_0 < 30):
        if (sc_0 < 28):
          if (sc_0 == 26):
            # DEREF_INSTANCE_FIELD
            v_value = v_stack[6]
            v_objInstance1 = v_value[1]
            v_value = v_objInstance1[2][v_row[0]]
            if (v_valueStackSize == v_valueStackCapacity):
              v_valueStack = v_valueStackIncreaseCapacity(v_ec)
              v_valueStackCapacity = len(v_valueStack)
            v_valueStack[v_valueStackSize] = v_value
            v_valueStackSize += 1
          else:
            # DEREF_STATIC_FIELD
            v_classInfo = v_classTable[v_row[0]]
            v_staticConstructorNotInvoked = True
            if (v_classInfo[4] < 2):
              v_stack[0] = v_pc
              v_stackFrame2 = v_maybeInvokeStaticConstructor(v_vm, v_ec, v_stack, v_classInfo, v_valueStackSize, PST_IntBuffer16)
              if (PST_IntBuffer16[0] == 1):
                return v_generateException(v_vm, v_stack, v_pc, v_valueStackSize, v_ec, 0, "Static initialization loop detected. The class this field is a member of is not done being initialized.")
              if (v_stackFrame2 != None):
                v_staticConstructorNotInvoked = False
                v_stack = v_stackFrame2
                v_pc = v_stack[0]
                v_localsStackSetToken = v_stack[1]
                v_localsStackOffset = v_stack[2]
            if v_staticConstructorNotInvoked:
              if (v_valueStackSize == v_valueStackCapacity):
                v_valueStack = v_valueStackIncreaseCapacity(v_ec)
                v_valueStackCapacity = len(v_valueStack)
              v_valueStack[v_valueStackSize] = v_classInfo[5][v_row[1]]
              v_valueStackSize += 1
        elif (sc_0 == 28):
          # DUPLICATE_STACK_TOP
          if (v_row[0] == 1):
            v_value = v_valueStack[(v_valueStackSize - 1)]
            if (v_valueStackSize == v_valueStackCapacity):
              v_valueStack = v_valueStackIncreaseCapacity(v_ec)
              v_valueStackCapacity = len(v_valueStack)
            v_valueStack[v_valueStackSize] = v_value
            v_valueStackSize += 1
          elif (v_row[0] == 2):
            if ((v_valueStackSize + 1) > v_valueStackCapacity):
              v_valueStackIncreaseCapacity(v_ec)
              v_valueStack = v_ec[4]
              v_valueStackCapacity = len(v_valueStack)
            v_valueStack[v_valueStackSize] = v_valueStack[(v_valueStackSize - 2)]
            v_valueStack[(v_valueStackSize + 1)] = v_valueStack[(v_valueStackSize - 1)]
            v_valueStackSize += 2
          else:
            v_hasInterrupt = v_EX_Fatal(v_ec, "?")
        else:
          # EQUALS
          v_valueStackSize -= 2
          v_rightValue = v_valueStack[(v_valueStackSize + 1)]
          v_leftValue = v_valueStack[v_valueStackSize]
          if (v_leftValue[0] == v_rightValue[0]):
            sc_15 = swlookup__interpretImpl__15.get(v_leftValue[0], 4)
            if (sc_15 < 3):
              if (sc_15 == 0):
                v_bool1 = True
              elif (sc_15 == 1):
                v_bool1 = (v_leftValue[1] == v_rightValue[1])
              else:
                v_bool1 = (v_leftValue[1] == v_rightValue[1])
            elif (sc_15 == 3):
              v_bool1 = (v_leftValue[1] == v_rightValue[1])
            else:
              v_bool1 = (v_doEqualityComparisonAndReturnCode(v_leftValue, v_rightValue) == 1)
          else:
            v_int1 = v_doEqualityComparisonAndReturnCode(v_leftValue, v_rightValue)
            if (v_int1 == 0):
              v_bool1 = False
            elif (v_int1 == 1):
              v_bool1 = True
            else:
              v_hasInterrupt = v_EX_UnsupportedOperation(v_ec, "== and != not defined here.")
          if (v_valueStackSize == v_valueStackCapacity):
            v_valueStack = v_valueStackIncreaseCapacity(v_ec)
            v_valueStackCapacity = len(v_valueStack)
          if (v_bool1 != ((v_row[0] == 1))):
            v_valueStack[v_valueStackSize] = v_VALUE_TRUE
          else:
            v_valueStack[v_valueStackSize] = v_VALUE_FALSE
          v_valueStackSize += 1
      elif (sc_0 < 32):
        if (sc_0 == 30):
          # ESF_LOOKUP
          v_esfData = v_generateEsfData(len(v_args), v_row)
          v_metadata[18] = v_esfData
        else:
          # EXCEPTION_HANDLED_TOGGLE
          v_ec[9] = (v_row[0] == 1)
      elif (sc_0 == 32):
        # FIELD_TYPE_INFO
        v_initializeClassFieldTypeInfo(v_vm, v_row)
      else:
        # FINALIZE_INITIALIZATION
        v_finalizeInitializationImpl(v_vm, v_stringArgs[v_pc], v_row[0])
        v_identifiers = v_vm[4][0]
        v_literalTable = v_vm[4][3]
        v_globalNameIdToPrimitiveMethodName = v_vm[4][12]
        v_funcArgs = v_vm[8]
    elif (sc_0 < 51):
      if (sc_0 < 43):
        if (sc_0 < 39):
          if (sc_0 < 37):
            if (sc_0 == 34):
              # FINALLY_END
              v_value = v_ec[10]
              if ((v_value == None) or v_ec[9]):
                sc_16 = swlookup__interpretImpl__16.get(v_stack[10], 4)
                if (sc_16 < 3):
                  if (sc_16 == 0):
                    v_ec[10] = None
                  elif (sc_16 == 1):
                    v_ec[10] = None
                    v_int1 = v_row[0]
                    if (v_int1 == 1):
                      v_pc += v_row[1]
                    elif (v_int1 == 2):
                      v_intArray1 = v_esfData[v_pc]
                      v_pc = v_intArray1[1]
                    else:
                      v_hasInterrupt = v_EX_Fatal(v_ec, "break exists without a loop")
                  else:
                    v_ec[10] = None
                    v_int1 = v_row[2]
                    if (v_int1 == 1):
                      v_pc += v_row[3]
                    elif (v_int1 == 2):
                      v_intArray1 = v_esfData[v_pc]
                      v_pc = v_intArray1[1]
                    else:
                      v_hasInterrupt = v_EX_Fatal(v_ec, "continue exists without a loop")
                elif (sc_16 == 3):
                  if (v_stack[8] != 0):
                    v_markClassAsInitialized(v_vm, v_stack, v_stack[8])
                  if v_stack[5]:
                    v_valueStackSize = v_stack[7]
                    v_value = v_stack[11]
                    v_stack = v_stack[4]
                    if (v_valueStackSize == v_valueStackCapacity):
                      v_valueStack = v_valueStackIncreaseCapacity(v_ec)
                      v_valueStackCapacity = len(v_valueStack)
                    v_valueStack[v_valueStackSize] = v_value
                    v_valueStackSize += 1
                  else:
                    v_valueStackSize = v_stack[7]
                    v_stack = v_stack[4]
                  v_pc = v_stack[0]
                  v_localsStackOffset = v_stack[2]
                  v_localsStackSetToken = v_stack[1]
              else:
                v_ec[9] = False
                v_stack[0] = v_pc
                v_intArray1 = v_esfData[v_pc]
                v_value = v_ec[10]
                v_objInstance1 = v_value[1]
                v_objArray1 = v_objInstance1[3]
                v_bool1 = True
                if (v_objArray1[0] != None):
                  v_bool1 = v_objArray1[0]
                v_intList1 = v_objArray1[1]
                while ((v_stack != None) and ((v_intArray1 == None) or v_bool1)):
                  v_stack = v_stack[4]
                  if (v_stack != None):
                    v_pc = v_stack[0]
                    v_intList1.append(v_pc)
                    v_intArray1 = v_esfData[v_pc]
                if (v_stack == None):
                  return v_uncaughtExceptionResult(v_vm, v_value)
                v_int1 = v_intArray1[0]
                if (v_int1 < v_pc):
                  v_int1 = v_intArray1[1]
                v_pc = (v_int1 - 1)
                v_stack[0] = v_pc
                v_localsStackOffset = v_stack[2]
                v_localsStackSetToken = v_stack[1]
                v_ec[1] = v_stack
                v_stack[10] = 0
                v_ec[2] = v_valueStackSize
                if (False and (v_stack[13] != None)):
                  v_hasInterrupt = True
                  v_ec[13] = [5, 0, "", 0.0, v_stack[13]]
            elif (sc_0 == 35):
              # FUNCTION_DEFINITION
              v_initializeFunction(v_vm, v_row, v_pc, v_stringArgs[v_pc])
              v_pc += v_row[7]
              v_functionTable = v_metadata[10]
            else:
              # INDEX
              v_valueStackSize -= 1
              v_value = v_valueStack[v_valueStackSize]
              v_root = v_valueStack[(v_valueStackSize - 1)]
              if (v_root[0] == 6):
                if (v_value[0] != 3):
                  v_hasInterrupt = v_EX_InvalidArgument(v_ec, "List index must be an integer.")
                else:
                  v_i = v_value[1]
                  v_list1 = v_root[1]
                  if (v_i < 0):
                    v_i += v_list1[1]
                  if ((v_i < 0) or (v_i >= v_list1[1])):
                    v_hasInterrupt = v_EX_IndexOutOfRange(v_ec, "List index is out of bounds")
                  else:
                    v_valueStack[(v_valueStackSize - 1)] = v_list1[2][v_i]
              elif (v_root[0] == 7):
                v_dictImpl = v_root[1]
                v_keyType = v_value[0]
                if (v_keyType != v_dictImpl[1]):
                  if (v_dictImpl[0] == 0):
                    v_hasInterrupt = v_EX_KeyNotFound(v_ec, "Key not found. Dictionary is empty.")
                  else:
                    v_hasInterrupt = v_EX_InvalidKey(v_ec, ''.join(["Incorrect key type. This dictionary contains ", v_getTypeFromId(v_dictImpl[1]), " keys. Provided key is a ", v_getTypeFromId(v_keyType), "."]))
                else:
                  if (v_keyType == 3):
                    v_intKey = v_value[1]
                  elif (v_keyType == 5):
                    v_stringKey = v_value[1]
                  elif (v_keyType == 8):
                    v_intKey = (v_value[1])[1]
                  elif (v_dictImpl[0] == 0):
                    v_hasInterrupt = v_EX_KeyNotFound(v_ec, "Key not found. Dictionary is empty.")
                  else:
                    v_hasInterrupt = v_EX_KeyNotFound(v_ec, "Key not found.")
                  if not (v_hasInterrupt):
                    if (v_keyType == 5):
                      v_stringIntDict1 = v_dictImpl[5]
                      v_int1 = v_stringIntDict1.get(v_stringKey, -1)
                      if (v_int1 == -1):
                        v_hasInterrupt = v_EX_KeyNotFound(v_ec, ''.join(["Key not found: '", v_stringKey, "'"]))
                      else:
                        v_valueStack[(v_valueStackSize - 1)] = v_dictImpl[7][v_int1]
                    else:
                      v_intIntDict1 = v_dictImpl[4]
                      v_int1 = v_intIntDict1.get(v_intKey, -1)
                      if (v_int1 == -1):
                        v_hasInterrupt = v_EX_KeyNotFound(v_ec, "Key not found.")
                      else:
                        v_valueStack[(v_valueStackSize - 1)] = v_dictImpl[7][v_intIntDict1[v_intKey]]
              elif (v_root[0] == 5):
                v_string1 = v_root[1]
                if (v_value[0] != 3):
                  v_hasInterrupt = v_EX_InvalidArgument(v_ec, "String indices must be integers.")
                else:
                  v_int1 = v_value[1]
                  if (v_int1 < 0):
                    v_int1 += len(v_string1)
                  if ((v_int1 < 0) or (v_int1 >= len(v_string1))):
                    v_hasInterrupt = v_EX_IndexOutOfRange(v_ec, "String index out of range.")
                  else:
                    v_valueStack[(v_valueStackSize - 1)] = v_buildCommonString(v_globals, v_string1[v_int1])
              else:
                v_hasInterrupt = v_EX_InvalidArgument(v_ec, "Cannot index into this type: " + v_getTypeFromId(v_root[0]))
          elif (sc_0 == 37):
            # IS_COMPARISON
            v_value = v_valueStack[(v_valueStackSize - 1)]
            v_output = v_VALUE_FALSE
            if (v_value[0] == 8):
              v_objInstance1 = v_value[1]
              if v_isClassASubclassOf(v_vm, v_objInstance1[0], v_row[0]):
                v_output = v_VALUE_TRUE
            v_valueStack[(v_valueStackSize - 1)] = v_output
          else:
            # ITERATION_STEP
            v_int1 = (v_localsStackOffset + v_row[2])
            v_value3 = v_localsStack[v_int1]
            v_i = v_value3[1]
            v_value = v_localsStack[(v_localsStackOffset + v_row[3])]
            if (v_value[0] == 6):
              v_list1 = v_value[1]
              v_len = v_list1[1]
              v_bool1 = True
            else:
              v_string2 = v_value[1]
              v_len = len(v_string2)
              v_bool1 = False
            if (v_i < v_len):
              if v_bool1:
                v_value = v_list1[2][v_i]
              else:
                v_value = v_buildCommonString(v_globals, v_string2[v_i])
              v_int3 = (v_localsStackOffset + v_row[1])
              v_localsStackSet[v_int3] = v_localsStackSetToken
              v_localsStack[v_int3] = v_value
            else:
              v_pc += v_row[0]
            v_i += 1
            if (v_i < 2049):
              v_localsStack[v_int1] = v_INTEGER_POSITIVE_CACHE[v_i]
            else:
              v_localsStack[v_int1] = [3, v_i]
        elif (sc_0 < 41):
          if (sc_0 == 39):
            # JUMP
            v_pc += v_row[0]
          else:
            # JUMP_IF_EXCEPTION_OF_TYPE
            v_value = v_ec[10]
            v_objInstance1 = v_value[1]
            v_int1 = v_objInstance1[0]
            v_i = (len(v_row) - 1)
            while (v_i >= 2):
              if v_isClassASubclassOf(v_vm, v_int1, v_row[v_i]):
                v_i = 0
                v_pc += v_row[0]
                v_int2 = v_row[1]
                if (v_int2 > -1):
                  v_int1 = (v_localsStackOffset + v_int2)
                  v_localsStack[v_int1] = v_value
                  v_localsStackSet[v_int1] = v_localsStackSetToken
              v_i -= 1
        elif (sc_0 == 41):
          # JUMP_IF_FALSE
          v_valueStackSize -= 1
          v_value = v_valueStack[v_valueStackSize]
          if (v_value[0] != 2):
            v_hasInterrupt = v_EX_InvalidArgument(v_ec, "Boolean expected.")
          elif not (v_value[1]):
            v_pc += v_row[0]
        else:
          # JUMP_IF_FALSE_NON_POP
          v_value = v_valueStack[(v_valueStackSize - 1)]
          if (v_value[0] != 2):
            v_hasInterrupt = v_EX_InvalidArgument(v_ec, "Boolean expected.")
          elif v_value[1]:
            v_valueStackSize -= 1
          else:
            v_pc += v_row[0]
      elif (sc_0 < 47):
        if (sc_0 < 45):
          if (sc_0 == 43):
            # JUMP_IF_TRUE
            v_valueStackSize -= 1
            v_value = v_valueStack[v_valueStackSize]
            if (v_value[0] != 2):
              v_hasInterrupt = v_EX_InvalidArgument(v_ec, "Boolean expected.")
            elif v_value[1]:
              v_pc += v_row[0]
          else:
            # JUMP_IF_TRUE_NO_POP
            v_value = v_valueStack[(v_valueStackSize - 1)]
            if (v_value[0] != 2):
              v_hasInterrupt = v_EX_InvalidArgument(v_ec, "Boolean expected.")
            elif v_value[1]:
              v_pc += v_row[0]
            else:
              v_valueStackSize -= 1
        elif (sc_0 == 45):
          # LAMBDA
          if not ((v_pc in v_metadata[11])):
            v_int1 = (4 + v_row[4] + 1)
            v_len = v_row[v_int1]
            v_intArray1 = (PST_NoneListOfOne * v_len)
            v_i = 0
            while (v_i < v_len):
              v_intArray1[v_i] = v_row[(v_int1 + v_i + 1)]
              v_i += 1
            v_len = v_row[4]
            v_intArray2 = (PST_NoneListOfOne * v_len)
            v_i = 0
            while (v_i < v_len):
              v_intArray2[v_i] = v_row[(5 + v_i)]
              v_i += 1
            v_metadata[11][v_pc] = [v_pc, 0, v_pc, v_row[0], v_row[1], 5, 0, v_row[2], v_intArray2, "lambda", v_intArray1]
          v_closure = {}
          v_parentClosure = v_stack[12]
          if (v_parentClosure == None):
            v_parentClosure = {}
            v_stack[12] = v_parentClosure
          v_functionInfo = v_metadata[11][v_pc]
          v_intArray1 = v_functionInfo[10]
          v_len = len(v_intArray1)
          v_i = 0
          while (v_i < v_len):
            v_j = v_intArray1[v_i]
            if (v_j in v_parentClosure):
              v_closure[v_j] = v_parentClosure[v_j]
            else:
              v_closure[v_j] = [None]
              v_parentClosure[v_j] = v_closure[v_j]
            v_i += 1
          if (v_valueStackSize == v_valueStackCapacity):
            v_valueStack = v_valueStackIncreaseCapacity(v_ec)
            v_valueStackCapacity = len(v_valueStack)
          v_valueStack[v_valueStackSize] = [9, [5, None, 0, v_pc, v_closure]]
          v_valueStackSize += 1
          v_pc += v_row[3]
        else:
          # LIB_DECLARATION
          v_prepareToSuspend(v_ec, v_stack, v_valueStackSize, v_pc)
          v_ec[13] = [4, v_row[0], v_stringArgs[v_pc], 0.0, None]
          v_hasInterrupt = True
      elif (sc_0 < 49):
        if (sc_0 == 47):
          # LIST_SLICE
          if (v_row[2] == 1):
            v_valueStackSize -= 1
            v_arg3 = v_valueStack[v_valueStackSize]
          else:
            v_arg3 = None
          if (v_row[1] == 1):
            v_valueStackSize -= 1
            v_arg2 = v_valueStack[v_valueStackSize]
          else:
            v_arg2 = None
          if (v_row[0] == 1):
            v_valueStackSize -= 1
            v_arg1 = v_valueStack[v_valueStackSize]
          else:
            v_arg1 = None
          v_value = v_valueStack[(v_valueStackSize - 1)]
          v_value = v_performListSlice(v_globals, v_ec, v_value, v_arg1, v_arg2, v_arg3)
          v_hasInterrupt = (v_ec[13] != None)
          if not (v_hasInterrupt):
            v_valueStack[(v_valueStackSize - 1)] = v_value
        else:
          # LITERAL
          if (v_valueStackSize == v_valueStackCapacity):
            v_valueStack = v_valueStackIncreaseCapacity(v_ec)
            v_valueStackCapacity = len(v_valueStack)
          v_valueStack[v_valueStackSize] = v_literalTable[v_row[0]]
          v_valueStackSize += 1
      elif (sc_0 == 49):
        # LITERAL_STREAM
        v_int1 = len(v_row)
        if ((v_valueStackSize + v_int1) > v_valueStackCapacity):
          while ((v_valueStackSize + v_int1) > v_valueStackCapacity):
            v_valueStackIncreaseCapacity(v_ec)
            v_valueStack = v_ec[4]
            v_valueStackCapacity = len(v_valueStack)
        v_i = (v_int1 - 1)
        while (v_i >= 0):
          v_valueStack[v_valueStackSize] = v_literalTable[v_row[v_i]]
          v_valueStackSize += 1
          v_i -= 1
      else:
        # LOCAL
        v_int1 = (v_localsStackOffset + v_row[0])
        if (v_localsStackSet[v_int1] == v_localsStackSetToken):
          if (v_valueStackSize == v_valueStackCapacity):
            v_valueStack = v_valueStackIncreaseCapacity(v_ec)
            v_valueStackCapacity = len(v_valueStack)
          v_valueStack[v_valueStackSize] = v_localsStack[v_int1]
          v_valueStackSize += 1
        else:
          v_hasInterrupt = v_EX_UnassignedVariable(v_ec, "Variable used before it was set.")
    elif (sc_0 < 59):
      if (sc_0 < 55):
        if (sc_0 < 53):
          if (sc_0 == 51):
            # LOC_TABLE
            v_initLocTable(v_vm, v_row)
          else:
            # NEGATIVE_SIGN
            v_value = v_valueStack[(v_valueStackSize - 1)]
            v_type = v_value[0]
            if (v_type == 3):
              v_valueStack[(v_valueStackSize - 1)] = v_buildInteger(v_globals, -(v_value[1]))
            elif (v_type == 4):
              v_valueStack[(v_valueStackSize - 1)] = v_buildFloat(v_globals, -(v_value[1]))
            else:
              v_hasInterrupt = v_EX_InvalidArgument(v_ec, ''.join(["Negative sign can only be applied to numbers. Found ", v_getTypeFromId(v_type), " instead."]))
        elif (sc_0 == 53):
          # POP
          v_valueStackSize -= 1
        else:
          # POP_IF_NULL_OR_JUMP
          v_value = v_valueStack[(v_valueStackSize - 1)]
          if (v_value[0] == 1):
            v_valueStackSize -= 1
          else:
            v_pc += v_row[0]
      elif (sc_0 < 57):
        if (sc_0 == 55):
          # PUSH_FUNC_REF
          v_value = None
          sc_17 = swlookup__interpretImpl__17.get(v_row[1], 3)
          if (sc_17 < 2):
            if (sc_17 == 0):
              v_value = [9, [1, None, 0, v_row[0], None]]
            else:
              v_value = [9, [2, v_stack[6], v_row[2], v_row[0], None]]
          elif (sc_17 == 2):
            v_classId = v_row[2]
            v_classInfo = v_classTable[v_classId]
            v_staticConstructorNotInvoked = True
            if (v_classInfo[4] < 2):
              v_stack[0] = v_pc
              v_stackFrame2 = v_maybeInvokeStaticConstructor(v_vm, v_ec, v_stack, v_classInfo, v_valueStackSize, PST_IntBuffer16)
              if (PST_IntBuffer16[0] == 1):
                return v_generateException(v_vm, v_stack, v_pc, v_valueStackSize, v_ec, 0, "Static initialization loop detected. The class this field is a member of is not done being initialized.")
              if (v_stackFrame2 != None):
                v_staticConstructorNotInvoked = False
                v_stack = v_stackFrame2
                v_pc = v_stack[0]
                v_localsStackSetToken = v_stack[1]
                v_localsStackOffset = v_stack[2]
            if v_staticConstructorNotInvoked:
              v_value = [9, [3, None, v_classId, v_row[0], None]]
            else:
              v_value = None
          if (v_value != None):
            if (v_valueStackSize == v_valueStackCapacity):
              v_valueStack = v_valueStackIncreaseCapacity(v_ec)
              v_valueStackCapacity = len(v_valueStack)
            v_valueStack[v_valueStackSize] = v_value
            v_valueStackSize += 1
        else:
          # RETURN
          if (v_esfData[v_pc] != None):
            v_intArray1 = v_esfData[v_pc]
            v_pc = (v_intArray1[1] - 1)
            if (v_row[0] == 0):
              v_stack[11] = v_VALUE_NULL
            else:
              v_stack[11] = v_valueStack[(v_valueStackSize - 1)]
            v_valueStackSize = v_stack[7]
            v_stack[10] = 3
          else:
            if (v_stack[4] == None):
              return v_interpreterFinished(v_vm, v_ec)
            if (v_stack[8] != 0):
              v_markClassAsInitialized(v_vm, v_stack, v_stack[8])
            if v_stack[5]:
              if (v_row[0] == 0):
                v_valueStackSize = v_stack[7]
                v_stack = v_stack[4]
                if (v_valueStackSize == v_valueStackCapacity):
                  v_valueStack = v_valueStackIncreaseCapacity(v_ec)
                  v_valueStackCapacity = len(v_valueStack)
                v_valueStack[v_valueStackSize] = v_VALUE_NULL
              else:
                v_value = v_valueStack[(v_valueStackSize - 1)]
                v_valueStackSize = v_stack[7]
                v_stack = v_stack[4]
                v_valueStack[v_valueStackSize] = v_value
              v_valueStackSize += 1
            else:
              v_valueStackSize = v_stack[7]
              v_stack = v_stack[4]
            v_pc = v_stack[0]
            v_localsStackOffset = v_stack[2]
            v_localsStackSetToken = v_stack[1]
            if (False and (v_stack[13] != None)):
              v_hasInterrupt = True
              v_ec[13] = [5, 0, "", 0.0, v_stack[13]]
      elif (sc_0 == 57):
        # STACK_INSERTION_FOR_INCREMENT
        if (v_valueStackSize == v_valueStackCapacity):
          v_valueStack = v_valueStackIncreaseCapacity(v_ec)
          v_valueStackCapacity = len(v_valueStack)
        v_valueStack[v_valueStackSize] = v_valueStack[(v_valueStackSize - 1)]
        v_valueStack[(v_valueStackSize - 1)] = v_valueStack[(v_valueStackSize - 2)]
        v_valueStack[(v_valueStackSize - 2)] = v_valueStack[(v_valueStackSize - 3)]
        v_valueStack[(v_valueStackSize - 3)] = v_valueStack[v_valueStackSize]
        v_valueStackSize += 1
      else:
        # STACK_SWAP_POP
        v_valueStackSize -= 1
        v_valueStack[(v_valueStackSize - 1)] = v_valueStack[v_valueStackSize]
    elif (sc_0 < 63):
      if (sc_0 < 61):
        if (sc_0 == 59):
          # SWITCH_INT
          v_valueStackSize -= 1
          v_value = v_valueStack[v_valueStackSize]
          if (v_value[0] == 3):
            v_intKey = v_value[1]
            v_integerSwitch = v_integerSwitchesByPc[v_pc]
            if (v_integerSwitch == None):
              v_integerSwitch = v_initializeIntSwitchStatement(v_vm, v_pc, v_row)
            v_i = v_integerSwitch.get(v_intKey, -1)
            if (v_i == -1):
              v_pc += v_row[0]
            else:
              v_pc += v_i
          else:
            v_hasInterrupt = v_EX_InvalidArgument(v_ec, "Switch statement expects an integer.")
        else:
          # SWITCH_STRING
          v_valueStackSize -= 1
          v_value = v_valueStack[v_valueStackSize]
          if (v_value[0] == 5):
            v_stringKey = v_value[1]
            v_stringSwitch = v_stringSwitchesByPc[v_pc]
            if (v_stringSwitch == None):
              v_stringSwitch = v_initializeStringSwitchStatement(v_vm, v_pc, v_row)
            v_i = v_stringSwitch.get(v_stringKey, -1)
            if (v_i == -1):
              v_pc += v_row[0]
            else:
              v_pc += v_i
          else:
            v_hasInterrupt = v_EX_InvalidArgument(v_ec, "Switch statement expects a string.")
      elif (sc_0 == 61):
        # THIS
        if (v_valueStackSize == v_valueStackCapacity):
          v_valueStack = v_valueStackIncreaseCapacity(v_ec)
          v_valueStackCapacity = len(v_valueStack)
        v_valueStack[v_valueStackSize] = v_stack[6]
        v_valueStackSize += 1
      else:
        # THROW
        v_valueStackSize -= 1
        v_value = v_valueStack[v_valueStackSize]
        v_bool2 = (v_value[0] == 8)
        if v_bool2:
          v_objInstance1 = v_value[1]
          if not (v_isClassASubclassOf(v_vm, v_objInstance1[0], v_magicNumbers[0])):
            v_bool2 = False
        if v_bool2:
          v_objArray1 = v_objInstance1[3]
          v_intList1 = []
          v_objArray1[1] = v_intList1
          if not (v_isPcFromCore(v_vm, v_pc)):
            v_intList1.append(v_pc)
          v_ec[10] = v_value
          v_ec[9] = False
          v_stack[0] = v_pc
          v_intArray1 = v_esfData[v_pc]
          v_value = v_ec[10]
          v_objInstance1 = v_value[1]
          v_objArray1 = v_objInstance1[3]
          v_bool1 = True
          if (v_objArray1[0] != None):
            v_bool1 = v_objArray1[0]
          v_intList1 = v_objArray1[1]
          while ((v_stack != None) and ((v_intArray1 == None) or v_bool1)):
            v_stack = v_stack[4]
            if (v_stack != None):
              v_pc = v_stack[0]
              v_intList1.append(v_pc)
              v_intArray1 = v_esfData[v_pc]
          if (v_stack == None):
            return v_uncaughtExceptionResult(v_vm, v_value)
          v_int1 = v_intArray1[0]
          if (v_int1 < v_pc):
            v_int1 = v_intArray1[1]
          v_pc = (v_int1 - 1)
          v_stack[0] = v_pc
          v_localsStackOffset = v_stack[2]
          v_localsStackSetToken = v_stack[1]
          v_ec[1] = v_stack
          v_stack[10] = 0
          v_ec[2] = v_valueStackSize
          if (False and (v_stack[13] != None)):
            v_hasInterrupt = True
            v_ec[13] = [5, 0, "", 0.0, v_stack[13]]
        else:
          v_hasInterrupt = v_EX_InvalidArgument(v_ec, "Thrown value is not an exception.")
    elif (sc_0 < 65):
      if (sc_0 == 63):
        # TOKEN_DATA
        v_tokenDataImpl(v_vm, v_row)
      else:
        # USER_CODE_START
        v_metadata[16] = v_row[0]
    elif (sc_0 == 65):
      # VERIFY_TYPE_IS_ITERABLE
      v_valueStackSize -= 1
      v_value = v_valueStack[v_valueStackSize]
      v_i = (v_localsStackOffset + v_row[0])
      v_localsStack[v_i] = v_value
      v_localsStackSet[v_i] = v_localsStackSetToken
      v_int1 = v_value[0]
      if ((v_int1 != 6) and (v_int1 != 5)):
        v_hasInterrupt = v_EX_InvalidArgument(v_ec, ''.join(["Expected an iterable type, such as a list or string. Found ", v_getTypeFromId(v_int1), " instead."]))
      v_i = (v_localsStackOffset + v_row[1])
      v_localsStack[v_i] = v_VALUE_INT_ZERO
      v_localsStackSet[v_i] = v_localsStackSetToken
    else:
      # THIS SHOULD NEVER HAPPEN
      return v_generateException(v_vm, v_stack, v_pc, v_valueStackSize, v_ec, 0, "Bad op code: " + str(v_ops[v_pc]))
    if v_hasInterrupt:
      v_interrupt = v_ec[13]
      v_ec[13] = None
      if (v_interrupt[0] == 1):
        return v_generateException(v_vm, v_stack, v_pc, v_valueStackSize, v_ec, v_interrupt[1], v_interrupt[2])
      if (v_interrupt[0] == 3):
        return [5, "", v_interrupt[3], 0, False, ""]
      if (v_interrupt[0] == 4):
        return [6, "", 0.0, 0, False, v_interrupt[2]]
    v_pc += 1

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

def v_isClassASubclassOf(v_vm, v_subClassId, v_parentClassId):
  if (v_subClassId == v_parentClassId):
    return True
  v_classTable = v_vm[4][9]
  v_classIdWalker = v_subClassId
  while (v_classIdWalker != -1):
    if (v_classIdWalker == v_parentClassId):
      return True
    v_classInfo = v_classTable[v_classIdWalker]
    v_classIdWalker = v_classInfo[2]
  return False

def v_isPcFromCore(v_vm, v_pc):
  if (v_vm[3] == None):
    return False
  v_tokens = v_vm[3][0][v_pc]
  if (v_tokens == None):
    return False
  v_token = v_tokens[0]
  v_filename = v_tokenHelperGetFileLine(v_vm, v_token[2], 0)
  return "[Core]" == v_filename

def v_isStringEqual(v_a, v_b):
  if (v_a == v_b):
    return True
  return False

def v_isVmResultRootExecContext(v_result):
  return v_result[4]

def v_makeEmptyList(v_type, v_capacity):
  return [v_type, 0, []]

def v_markClassAsInitialized(v_vm, v_stack, v_classId):
  v_classInfo = v_vm[4][9][v_stack[8]]
  v_classInfo[4] = 2
  v_vm[7].pop()
  return 0

def v_maybeInvokeStaticConstructor(v_vm, v_ec, v_stack, v_classInfo, v_valueStackSize, v_intOutParam):
  PST_IntBuffer16[0] = 0
  v_classId = v_classInfo[0]
  if (v_classInfo[4] == 1):
    v_classIdsBeingInitialized = v_vm[7]
    if (v_classIdsBeingInitialized[(len(v_classIdsBeingInitialized) - 1)] != v_classId):
      PST_IntBuffer16[0] = 1
    return None
  v_classInfo[4] = 1
  v_vm[7].append(v_classId)
  v_functionInfo = v_vm[4][10][v_classInfo[6]]
  v_stack[0] -= 1
  v_newFrameLocalsSize = v_functionInfo[7]
  v_currentFrameLocalsEnd = v_stack[3]
  if (len(v_ec[5]) <= (v_currentFrameLocalsEnd + v_newFrameLocalsSize)):
    v_increaseLocalsStackCapacity(v_ec, v_newFrameLocalsSize)
  if (v_ec[7] > 2000000000):
    v_resetLocalsStackTokens(v_ec, v_stack)
  v_ec[7] += 1
  return [v_functionInfo[2], v_ec[7], v_currentFrameLocalsEnd, (v_currentFrameLocalsEnd + v_newFrameLocalsSize), v_stack, False, None, v_valueStackSize, v_classId, (v_stack[9] + 1), 0, None, None, None]

def v_multiplyString(v_globals, v_strValue, v_str, v_n):
  if (v_n <= 2):
    if (v_n == 1):
      return v_strValue
    if (v_n == 2):
      return v_buildString(v_globals, v_str + v_str)
    return v_globals[8]
  v_builder = []
  while (v_n > 0):
    v_n -= 1
    v_builder.append(v_str)
  v_str = "".join(v_builder)
  return v_buildString(v_globals, v_str)

def v_nextPowerOf2(v_value):
  if (((v_value - 1) & v_value) == 0):
    return v_value
  v_output = 1
  while (v_output < v_value):
    v_output *= 2
  return v_output

def v_noop():
  return 0

def v_performListSlice(v_globals, v_ec, v_value, v_arg1, v_arg2, v_arg3):
  v_begin = 0
  v_end = 0
  v_step = 0
  v_length = 0
  v_i = 0
  v_isForward = False
  v_isString = False
  v_originalString = ""
  v_originalList = None
  v_outputList = None
  v_outputString = None
  v_status = 0
  if (v_arg3 != None):
    if (v_arg3[0] == 3):
      v_step = v_arg3[1]
      if (v_step == 0):
        v_status = 2
    else:
      v_status = 3
      v_step = 1
  else:
    v_step = 1
  v_isForward = (v_step > 0)
  if (v_arg2 != None):
    if (v_arg2[0] == 3):
      v_end = v_arg2[1]
    else:
      v_status = 3
  if (v_arg1 != None):
    if (v_arg1[0] == 3):
      v_begin = v_arg1[1]
    else:
      v_status = 3
  if (v_value[0] == 5):
    v_isString = True
    v_originalString = v_value[1]
    v_length = len(v_originalString)
  elif (v_value[0] == 6):
    v_isString = False
    v_originalList = v_value[1]
    v_length = v_originalList[1]
  else:
    v_EX_InvalidArgument(v_ec, ''.join(["Cannot apply slicing to ", v_getTypeFromId(v_value[0]), ". Must be string or list."]))
    return v_globals[0]
  if (v_status >= 2):
    v_msg = None
    if v_isString:
      v_msg = "String"
    else:
      v_msg = "List"
    if (v_status == 3):
      v_msg += " slice indexes must be integers. Found "
      if ((v_arg1 != None) and (v_arg1[0] != 3)):
        v_EX_InvalidArgument(v_ec, ''.join([v_msg, v_getTypeFromId(v_arg1[0]), " for begin index."]))
        return v_globals[0]
      if ((v_arg2 != None) and (v_arg2[0] != 3)):
        v_EX_InvalidArgument(v_ec, ''.join([v_msg, v_getTypeFromId(v_arg2[0]), " for end index."]))
        return v_globals[0]
      if ((v_arg3 != None) and (v_arg3[0] != 3)):
        v_EX_InvalidArgument(v_ec, ''.join([v_msg, v_getTypeFromId(v_arg3[0]), " for step amount."]))
        return v_globals[0]
      v_EX_InvalidArgument(v_ec, "Invalid slice arguments.")
      return v_globals[0]
    else:
      v_EX_InvalidArgument(v_ec, v_msg + " slice step cannot be 0.")
      return v_globals[0]
  v_status = v_canonicalizeListSliceArgs(PST_IntBuffer16, v_arg1, v_arg2, v_begin, v_end, v_step, v_length, v_isForward)
  if (v_status == 1):
    v_begin = PST_IntBuffer16[0]
    v_end = PST_IntBuffer16[1]
    if v_isString:
      v_outputString = []
      if v_isForward:
        if (v_step == 1):
          return v_buildString(v_globals, v_originalString[v_begin:v_begin + (v_end - v_begin)])
        else:
          while (v_begin < v_end):
            v_outputString.append(v_originalString[v_begin])
            v_begin += v_step
      else:
        while (v_begin > v_end):
          v_outputString.append(v_originalString[v_begin])
          v_begin += v_step
      v_value = v_buildString(v_globals, "".join(v_outputString))
    else:
      v_outputList = v_makeEmptyList(v_originalList[0], 10)
      if v_isForward:
        while (v_begin < v_end):
          v_addToList(v_outputList, v_originalList[2][v_begin])
          v_begin += v_step
      else:
        while (v_begin > v_end):
          v_addToList(v_outputList, v_originalList[2][v_begin])
          v_begin += v_step
      v_value = [6, v_outputList]
  elif (v_status == 0):
    if v_isString:
      v_value = v_globals[8]
    else:
      v_value = [6, v_makeEmptyList(v_originalList[0], 0)]
  elif (v_status == 2):
    if not (v_isString):
      v_outputList = v_makeEmptyList(v_originalList[0], v_length)
      v_i = 0
      while (v_i < v_length):
        v_addToList(v_outputList, v_originalList[2][v_i])
        v_i += 1
      v_value = [6, v_outputList]
  else:
    v_msg = None
    if v_isString:
      v_msg = "String"
    else:
      v_msg = "List"
    if (v_status == 3):
      v_msg += " slice begin index is out of range."
    elif v_isForward:
      v_msg += " slice begin index must occur before the end index when step is positive."
    else:
      v_msg += " slice begin index must occur after the end index when the step is negative."
    v_EX_IndexOutOfRange(v_ec, v_msg)
    return v_globals[0]
  return v_value

def v_prepareToSuspend(v_ec, v_stack, v_valueStackSize, v_currentPc):
  v_ec[1] = v_stack
  v_ec[2] = v_valueStackSize
  v_stack[0] = (v_currentPc + 1)
  return 0

def v_primitiveMethodsInitializeLookup(v_nameLookups):
  v_length = len(v_nameLookups)
  v_lookup = (PST_NoneListOfOne * v_length)
  v_i = 0
  while (v_i < v_length):
    v_lookup[v_i] = -1
    v_i += 1
  if ("add" in v_nameLookups):
    v_lookup[v_nameLookups["add"]] = 0
  if ("argCountMax" in v_nameLookups):
    v_lookup[v_nameLookups["argCountMax"]] = 1
  if ("argCountMin" in v_nameLookups):
    v_lookup[v_nameLookups["argCountMin"]] = 2
  if ("choice" in v_nameLookups):
    v_lookup[v_nameLookups["choice"]] = 3
  if ("clear" in v_nameLookups):
    v_lookup[v_nameLookups["clear"]] = 4
  if ("clone" in v_nameLookups):
    v_lookup[v_nameLookups["clone"]] = 5
  if ("concat" in v_nameLookups):
    v_lookup[v_nameLookups["concat"]] = 6
  if ("contains" in v_nameLookups):
    v_lookup[v_nameLookups["contains"]] = 7
  if ("createInstance" in v_nameLookups):
    v_lookup[v_nameLookups["createInstance"]] = 8
  if ("endsWith" in v_nameLookups):
    v_lookup[v_nameLookups["endsWith"]] = 9
  if ("filter" in v_nameLookups):
    v_lookup[v_nameLookups["filter"]] = 10
  if ("get" in v_nameLookups):
    v_lookup[v_nameLookups["get"]] = 11
  if ("getName" in v_nameLookups):
    v_lookup[v_nameLookups["getName"]] = 12
  if ("indexOf" in v_nameLookups):
    v_lookup[v_nameLookups["indexOf"]] = 13
  if ("insert" in v_nameLookups):
    v_lookup[v_nameLookups["insert"]] = 14
  if ("invoke" in v_nameLookups):
    v_lookup[v_nameLookups["invoke"]] = 15
  if ("isA" in v_nameLookups):
    v_lookup[v_nameLookups["isA"]] = 16
  if ("join" in v_nameLookups):
    v_lookup[v_nameLookups["join"]] = 17
  if ("keys" in v_nameLookups):
    v_lookup[v_nameLookups["keys"]] = 18
  if ("lower" in v_nameLookups):
    v_lookup[v_nameLookups["lower"]] = 19
  if ("ltrim" in v_nameLookups):
    v_lookup[v_nameLookups["ltrim"]] = 20
  if ("map" in v_nameLookups):
    v_lookup[v_nameLookups["map"]] = 21
  if ("merge" in v_nameLookups):
    v_lookup[v_nameLookups["merge"]] = 22
  if ("pop" in v_nameLookups):
    v_lookup[v_nameLookups["pop"]] = 23
  if ("remove" in v_nameLookups):
    v_lookup[v_nameLookups["remove"]] = 24
  if ("replace" in v_nameLookups):
    v_lookup[v_nameLookups["replace"]] = 25
  if ("reverse" in v_nameLookups):
    v_lookup[v_nameLookups["reverse"]] = 26
  if ("rtrim" in v_nameLookups):
    v_lookup[v_nameLookups["rtrim"]] = 27
  if ("shuffle" in v_nameLookups):
    v_lookup[v_nameLookups["shuffle"]] = 28
  if ("sort" in v_nameLookups):
    v_lookup[v_nameLookups["sort"]] = 29
  if ("split" in v_nameLookups):
    v_lookup[v_nameLookups["split"]] = 30
  if ("startsWith" in v_nameLookups):
    v_lookup[v_nameLookups["startsWith"]] = 31
  if ("trim" in v_nameLookups):
    v_lookup[v_nameLookups["trim"]] = 32
  if ("upper" in v_nameLookups):
    v_lookup[v_nameLookups["upper"]] = 33
  if ("values" in v_nameLookups):
    v_lookup[v_nameLookups["values"]] = 34
  return v_lookup

def v_primitiveMethodWrongArgCountError(v_name, v_expected, v_actual):
  v_output = ""
  if (v_expected == 0):
    v_output = v_name + " does not accept any arguments."
  elif (v_expected == 1):
    v_output = v_name + " accepts exactly 1 argument."
  else:
    v_output = ''.join([v_name, " requires ", str(v_expected), " arguments."])
  return ''.join([v_output, " Found: ", str(v_actual)])

def v_printToStdOut(v_prefix, v_line):
  if (v_prefix == None):
    print(v_line)
  else:
    v_canonical = v_line.replace("\r\n", "\n").replace("\r", "\n")
    v_lines = v_canonical.split("\n")
    v_i = 0
    while (v_i < len(v_lines)):
      print(''.join([v_prefix, ": ", v_lines[v_i]]))
      v_i += 1
  return 0

def v_qsortHelper(v_keyStringList, v_keyNumList, v_indices, v_isString, v_startIndex, v_endIndex):
  if ((v_endIndex - v_startIndex) <= 0):
    return 0
  if ((v_endIndex - v_startIndex) == 1):
    if v_sortHelperIsRevOrder(v_keyStringList, v_keyNumList, v_isString, v_startIndex, v_endIndex):
      v_sortHelperSwap(v_keyStringList, v_keyNumList, v_indices, v_isString, v_startIndex, v_endIndex)
    return 0
  v_mid = ((v_endIndex + v_startIndex) >> 1)
  v_sortHelperSwap(v_keyStringList, v_keyNumList, v_indices, v_isString, v_mid, v_startIndex)
  v_upperPointer = (v_endIndex + 1)
  v_lowerPointer = (v_startIndex + 1)
  while (v_upperPointer > v_lowerPointer):
    if v_sortHelperIsRevOrder(v_keyStringList, v_keyNumList, v_isString, v_startIndex, v_lowerPointer):
      v_lowerPointer += 1
    else:
      v_upperPointer -= 1
      v_sortHelperSwap(v_keyStringList, v_keyNumList, v_indices, v_isString, v_lowerPointer, v_upperPointer)
  v_midIndex = (v_lowerPointer - 1)
  v_sortHelperSwap(v_keyStringList, v_keyNumList, v_indices, v_isString, v_midIndex, v_startIndex)
  v_qsortHelper(v_keyStringList, v_keyNumList, v_indices, v_isString, v_startIndex, (v_midIndex - 1))
  v_qsortHelper(v_keyStringList, v_keyNumList, v_indices, v_isString, (v_midIndex + 1), v_endIndex)
  return 0

def v_queryValue(v_vm, v_execId, v_stackFrameOffset, v_steps):
  if (v_execId == -1):
    v_execId = v_vm[1]
  v_ec = v_vm[0][v_execId]
  v_stackFrame = v_ec[1]
  while (v_stackFrameOffset > 0):
    v_stackFrameOffset -= 1
    v_stackFrame = v_stackFrame[4]
  v_current = None
  v_i = 0
  v_j = 0
  v_len = len(v_steps)
  v_i = 0
  while (v_i < len(v_steps)):
    if ((v_current == None) and (v_i > 0)):
      return None
    v_step = v_steps[v_i]
    if v_isStringEqual(".", v_step):
      return None
    elif v_isStringEqual("this", v_step):
      v_current = v_stackFrame[6]
    elif v_isStringEqual("class", v_step):
      return None
    elif v_isStringEqual("local", v_step):
      v_i += 1
      v_step = v_steps[v_i]
      v_localNamesByFuncPc = v_vm[3][5]
      v_localNames = None
      if ((v_localNamesByFuncPc == None) or (len(v_localNamesByFuncPc) == 0)):
        return None
      v_j = v_stackFrame[0]
      while (v_j >= 0):
        if (v_j in v_localNamesByFuncPc):
          v_localNames = v_localNamesByFuncPc[v_j]
          v_j = -1
        v_j -= 1
      if (v_localNames == None):
        return None
      v_localId = -1
      if (v_localNames != None):
        v_j = 0
        while (v_j < len(v_localNames)):
          if v_isStringEqual(v_localNames[v_j], v_step):
            v_localId = v_j
            v_j = len(v_localNames)
          v_j += 1
      if (v_localId == -1):
        return None
      v_localOffset = (v_localId + v_stackFrame[2])
      if (v_ec[6][v_localOffset] != v_stackFrame[1]):
        return None
      v_current = v_ec[5][v_localOffset]
    elif v_isStringEqual("index", v_step):
      return None
    elif v_isStringEqual("key-int", v_step):
      return None
    elif v_isStringEqual("key-str", v_step):
      return None
    elif v_isStringEqual("key-obj", v_step):
      return None
    else:
      return None
    v_i += 1
  return v_current

def v_read_integer(v_pindex, v_raw, v_length, v_alphaNums):
  v_num = 0
  v_c = v_raw[v_pindex[0]]
  v_pindex[0] = (v_pindex[0] + 1)
  if (v_c == "%"):
    v_value = v_read_till(v_pindex, v_raw, v_length, "%")
    v_num = int(v_value)
  elif (v_c == "@"):
    v_num = v_read_integer(v_pindex, v_raw, v_length, v_alphaNums)
    v_num *= 62
    v_num += v_read_integer(v_pindex, v_raw, v_length, v_alphaNums)
  elif (v_c == "#"):
    v_num = v_read_integer(v_pindex, v_raw, v_length, v_alphaNums)
    v_num *= 62
    v_num += v_read_integer(v_pindex, v_raw, v_length, v_alphaNums)
    v_num *= 62
    v_num += v_read_integer(v_pindex, v_raw, v_length, v_alphaNums)
  elif (v_c == "^"):
    v_num = (-1 * v_read_integer(v_pindex, v_raw, v_length, v_alphaNums))
  else:
    # TODO: string.IndexOfChar(c)
    v_num = v_alphaNums.find(v_c)
    if (v_num == -1):
      pass
  return v_num

def v_read_string(v_pindex, v_raw, v_length, v_alphaNums):
  v_b64 = v_read_till(v_pindex, v_raw, v_length, "%")
  return PST_base64ToString(v_b64)

def v_read_till(v_index, v_raw, v_length, v_end):
  v_output = []
  v_ctn = True
  v_c = " "
  while v_ctn:
    v_c = v_raw[v_index[0]]
    if (v_c == v_end):
      v_ctn = False
    else:
      v_output.append(v_c)
    v_index[0] = (v_index[0] + 1)
  return ''.join(v_output)

def v_reallocIntArray(v_original, v_requiredCapacity):
  v_oldSize = len(v_original)
  v_size = v_oldSize
  while (v_size < v_requiredCapacity):
    v_size *= 2
  v_output = (PST_NoneListOfOne * v_size)
  v_i = 0
  while (v_i < v_oldSize):
    v_output[v_i] = v_original[v_i]
    v_i += 1
  return v_output

def v_Reflect_allClasses(v_vm):
  v_generics = [None]
  v_generics[0] = 10
  v_output = v_makeEmptyList(v_generics, 20)
  v_classTable = v_vm[4][9]
  v_i = 1
  while (v_i < len(v_classTable)):
    v_classInfo = v_classTable[v_i]
    if (v_classInfo == None):
      v_i = len(v_classTable)
    else:
      v_addToList(v_output, [10, [False, v_classInfo[0]]])
    v_i += 1
  return [6, v_output]

def v_Reflect_getMethods(v_vm, v_ec, v_methodSource):
  v_output = v_makeEmptyList(None, 8)
  if (v_methodSource[0] == 8):
    v_objInstance1 = v_methodSource[1]
    v_classInfo = v_vm[4][9][v_objInstance1[0]]
    v_i = 0
    while (v_i < len(v_classInfo[9])):
      v_functionId = v_classInfo[9][v_i]
      if (v_functionId != -1):
        v_addToList(v_output, [9, [2, v_methodSource, v_objInstance1[0], v_functionId, None]])
      v_i += 1
  else:
    v_classValue = v_methodSource[1]
    v_classInfo = v_vm[4][9][v_classValue[1]]
    v_EX_UnsupportedOperation(v_ec, "static method reflection not implemented yet.")
  return [6, v_output]

def v_resetLocalsStackTokens(v_ec, v_stack):
  v_localsStack = v_ec[5]
  v_localsStackSet = v_ec[6]
  v_i = v_stack[3]
  while (v_i < len(v_localsStackSet)):
    v_localsStackSet[v_i] = 0
    v_localsStack[v_i] = None
    v_i += 1
  v_stackWalker = v_stack
  while (v_stackWalker != None):
    v_token = v_stackWalker[1]
    v_stackWalker[1] = 1
    v_i = v_stackWalker[2]
    while (v_i < v_stackWalker[3]):
      if (v_localsStackSet[v_i] == v_token):
        v_localsStackSet[v_i] = 1
      else:
        v_localsStackSet[v_i] = 0
        v_localsStack[v_i] = None
      v_i += 1
    v_stackWalker = v_stackWalker[4]
  v_ec[7] = 1
  return -1

def v_resolvePrimitiveMethodName2(v_lookup, v_type, v_globalNameId):
  v_output = v_lookup[v_globalNameId]
  if (v_output != -1):
    sc_0 = swlookup__resolvePrimitiveMethodName2__0.get((v_type + (11 * v_output)), 1)
    if (sc_0 == 0):
      return v_output
    else:
      return -1
  return -1

swlookup__resolvePrimitiveMethodName2__0 = { 82: 0, 104: 0, 148: 0, 214: 0, 225: 0, 280: 0, 291: 0, 302: 0, 335: 0, 346: 0, 357: 0, 368: 0, 6: 0, 39: 0, 50: 0, 61: 0, 72: 0, 83: 0, 116: 0, 160: 0, 193: 0, 237: 0, 259: 0, 270: 0, 292: 0, 314: 0, 325: 0, 51: 0, 62: 0, 84: 0, 128: 0, 205: 0, 249: 0, 271: 0, 381: 0, 20: 0, 31: 0, 141: 0, 174: 0, 98: 0, 142: 0, 186: 0 }

def v_resource_manager_getResourceOfType(v_vm, v_userPath, v_type):
  v_db = v_vm[9]
  v_lookup = v_db[1]
  if (v_userPath in v_lookup):
    v_output = v_makeEmptyList(None, 2)
    v_file = v_lookup[v_userPath]
    if v_file[3] == v_type:
      v_addToList(v_output, v_vm[12][1])
      v_addToList(v_output, v_buildString(v_vm[12], v_file[1]))
    else:
      v_addToList(v_output, v_vm[12][2])
    return [6, v_output]
  return v_vm[12][0]

def v_resource_manager_populate_directory_lookup(v_dirs, v_path):
  v_parts = v_path.split("/")
  v_pathBuilder = ""
  v_file = ""
  v_i = 0
  while (v_i < len(v_parts)):
    v_file = v_parts[v_i]
    v_files = None
    if not ((v_pathBuilder in v_dirs)):
      v_files = []
      v_dirs[v_pathBuilder] = v_files
    else:
      v_files = v_dirs[v_pathBuilder]
    v_files.append(v_file)
    if (v_i > 0):
      v_pathBuilder = ''.join([v_pathBuilder, "/", v_file])
    else:
      v_pathBuilder = v_file
    v_i += 1
  return 0

def v_resourceManagerInitialize(v_globals, v_manifest):
  v_filesPerDirectoryBuilder = {}
  v_fileInfo = {}
  v_dataList = []
  v_items = v_manifest.split("\n")
  v_resourceInfo = None
  v_type = ""
  v_userPath = ""
  v_internalPath = ""
  v_argument = ""
  v_isText = False
  v_intType = 0
  v_i = 0
  while (v_i < len(v_items)):
    v_itemData = v_items[v_i].split(",")
    if (len(v_itemData) >= 3):
      v_type = v_itemData[0]
      v_isText = "TXT" == v_type
      if v_isText:
        v_intType = 1
      elif ("IMGSH" == v_type or "IMG" == v_type):
        v_intType = 2
      elif "SND" == v_type:
        v_intType = 3
      elif "TTF" == v_type:
        v_intType = 4
      else:
        v_intType = 5
      v_userPath = v_stringDecode(v_itemData[1])
      v_internalPath = v_itemData[2]
      v_argument = ""
      if (len(v_itemData) > 3):
        v_argument = v_stringDecode(v_itemData[3])
      v_resourceInfo = [v_userPath, v_internalPath, v_isText, v_type, v_argument]
      v_fileInfo[v_userPath] = v_resourceInfo
      v_resource_manager_populate_directory_lookup(v_filesPerDirectoryBuilder, v_userPath)
      v_dataList.append(v_buildString(v_globals, v_userPath))
      v_dataList.append(v_buildInteger(v_globals, v_intType))
      if (v_internalPath != None):
        v_dataList.append(v_buildString(v_globals, v_internalPath))
      else:
        v_dataList.append(v_globals[0])
    v_i += 1
  v_dirs = list(v_filesPerDirectoryBuilder.keys())
  v_filesPerDirectorySorted = {}
  v_i = 0
  while (v_i < len(v_dirs)):
    v_dir = v_dirs[v_i]
    v_unsortedDirs = v_filesPerDirectoryBuilder[v_dir]
    v_dirsSorted = v_unsortedDirs[:]
    v_dirsSorted = PST_sortedCopyOfList(v_dirsSorted)
    v_filesPerDirectorySorted[v_dir] = v_dirsSorted
    v_i += 1
  return [v_filesPerDirectorySorted, v_fileInfo, v_dataList]

def v_reverseList(v_list):
  v_list[2].reverse()

def v_runInterpreter(v_vm, v_executionContextId):
  v_result = v_interpret(v_vm, v_executionContextId)
  v_result[3] = v_executionContextId
  v_status = v_result[0]
  if (v_status == 1):
    if (v_executionContextId in v_vm[0]):
      v_vm[0].pop(v_executionContextId)
    v_runShutdownHandlers(v_vm)
  elif (v_status == 3):
    v_printToStdOut(v_vm[11][3], v_result[1])
    v_runShutdownHandlers(v_vm)
  if (v_executionContextId == 0):
    v_result[4] = True
  return v_result

def v_runInterpreterWithFunctionPointer(v_vm, v_fpValue, v_args):
  v_newId = (v_vm[1] + 1)
  v_vm[1] = v_newId
  v_argList = []
  v_i = 0
  while (v_i < len(v_args)):
    v_argList.append(v_args[v_i])
    v_i += 1
  v_locals = []
  v_localsSet = []
  v_valueStack = (PST_NoneListOfOne * 100)
  v_valueStack[0] = v_fpValue
  v_valueStack[1] = [6, v_argList]
  v_stack = [(len(v_vm[2][0]) - 2), 1, 0, 0, None, False, None, 0, 0, 1, 0, None, None, None]
  v_executionContext = [v_newId, v_stack, 2, 100, v_valueStack, v_locals, v_localsSet, 1, 0, False, None, False, 0, None]
  v_vm[0][v_newId] = v_executionContext
  return v_runInterpreter(v_vm, v_newId)

def v_runShutdownHandlers(v_vm):
  while (len(v_vm[10]) > 0):
    v_handler = v_vm[10][0]
    del v_vm[10][0]
    v_runInterpreterWithFunctionPointer(v_vm, v_handler, [])
  return 0

def v_setItemInList(v_list, v_i, v_v):
  v_list[2][v_i] = v_v

def v_sortHelperIsRevOrder(v_keyStringList, v_keyNumList, v_isString, v_indexLeft, v_indexRight):
  if v_isString:
    return (v_keyStringList[v_indexLeft] > v_keyStringList[v_indexRight])
  return (v_keyNumList[v_indexLeft] > v_keyNumList[v_indexRight])

def v_sortHelperSwap(v_keyStringList, v_keyNumList, v_indices, v_isString, v_index1, v_index2):
  if (v_index1 == v_index2):
    return 0
  v_t = v_indices[v_index1]
  v_indices[v_index1] = v_indices[v_index2]
  v_indices[v_index2] = v_t
  if v_isString:
    v_s = v_keyStringList[v_index1]
    v_keyStringList[v_index1] = v_keyStringList[v_index2]
    v_keyStringList[v_index2] = v_s
  else:
    v_n = v_keyNumList[v_index1]
    v_keyNumList[v_index1] = v_keyNumList[v_index2]
    v_keyNumList[v_index2] = v_n
  return 0

def v_sortLists(v_keyList, v_parallelList, v_intOutParam):
  PST_IntBuffer16[0] = 0
  v_length = v_keyList[1]
  if (v_length < 2):
    return 0
  v_i = 0
  v_item = None
  v_item = v_keyList[2][0]
  v_isString = (v_item[0] == 5)
  v_stringKeys = None
  v_numKeys = None
  if v_isString:
    v_stringKeys = (PST_NoneListOfOne * v_length)
  else:
    v_numKeys = (PST_NoneListOfOne * v_length)
  v_indices = (PST_NoneListOfOne * v_length)
  v_originalOrder = (PST_NoneListOfOne * v_length)
  v_i = 0
  while (v_i < v_length):
    v_indices[v_i] = v_i
    v_originalOrder[v_i] = v_parallelList[2][v_i]
    v_item = v_keyList[2][v_i]
    sc_0 = swlookup__sortLists__0.get(v_item[0], 3)
    if (sc_0 < 2):
      if (sc_0 == 0):
        if v_isString:
          PST_IntBuffer16[0] = 1
          return 0
        v_numKeys[v_i] = v_item[1]
      else:
        if v_isString:
          PST_IntBuffer16[0] = 1
          return 0
        v_numKeys[v_i] = v_item[1]
    elif (sc_0 == 2):
      if not (v_isString):
        PST_IntBuffer16[0] = 1
        return 0
      v_stringKeys[v_i] = v_item[1]
    else:
      PST_IntBuffer16[0] = 1
      return 0
    v_i += 1
  v_qsortHelper(v_stringKeys, v_numKeys, v_indices, v_isString, 0, (v_length - 1))
  v_i = 0
  while (v_i < v_length):
    v_parallelList[2][v_i] = v_originalOrder[v_indices[v_i]]
    v_i += 1
  return 0

swlookup__sortLists__0 = { 3: 0, 4: 1, 5: 2 }

def v_stackItemIsLibrary(v_stackInfo):
  if (v_stackInfo[0] != "["):
    return False
  v_cIndex = v_stackInfo.find(":")
  return ((v_cIndex > 0) and (v_cIndex < v_stackInfo.find("]")))

def v_startVm(v_vm):
  return v_runInterpreter(v_vm, v_vm[1])

def v_stringDecode(v_encoded):
  if not (("%" in v_encoded)):
    v_length = len(v_encoded)
    v_per = "%"
    v_builder = []
    v_i = 0
    while (v_i < v_length):
      v_c = v_encoded[v_i]
      if ((v_c == v_per) and ((v_i + 2) < v_length)):
        v_builder.append(v_stringFromHex(''.join(["", v_encoded[(v_i + 1)], v_encoded[(v_i + 2)]])))
      else:
        v_builder.append("" + v_c)
      v_i += 1
    return "".join(v_builder)
  return v_encoded

def v_stringFromHex(v_encoded):
  v_encoded = v_encoded.upper()
  v_hex = "0123456789ABCDEF"
  v_output = []
  v_length = len(v_encoded)
  v_a = 0
  v_b = 0
  v_c = None
  v_i = 0
  while ((v_i + 1) < v_length):
    v_c = "" + v_encoded[v_i]
    v_a = v_hex.find(v_c)
    if (v_a == -1):
      return None
    v_c = "" + v_encoded[(v_i + 1)]
    v_b = v_hex.find(v_c)
    if (v_b == -1):
      return None
    v_a = ((v_a * 16) + v_b)
    v_output.append(chr(v_a))
    v_i += 2
  return "".join(v_output)

def v_suspendInterpreter():
  return [2, None, 0.0, 0, False, ""]

def v_tokenDataImpl(v_vm, v_row):
  v_tokensByPc = v_vm[3][0]
  v_pc = (v_row[0] + v_vm[4][16])
  v_line = v_row[1]
  v_col = v_row[2]
  v_file = v_row[3]
  v_tokens = v_tokensByPc[v_pc]
  if (v_tokens == None):
    v_tokens = []
    v_tokensByPc[v_pc] = v_tokens
  v_tokens.append([v_line, v_col, v_file])
  return 0

def v_tokenHelperConvertPcsToStackTraceStrings(v_vm, v_pcs):
  v_tokens = v_generateTokenListFromPcs(v_vm, v_pcs)
  v_files = v_vm[3][1]
  v_output = []
  v_i = 0
  while (v_i < len(v_tokens)):
    v_token = v_tokens[v_i]
    if (v_token == None):
      v_output.append("[No stack information]")
    else:
      v_line = v_token[0]
      v_col = v_token[1]
      v_fileData = v_files[v_token[2]]
      v_lines = v_fileData.split("\n")
      v_filename = v_lines[0]
      v_linevalue = v_lines[(v_line + 1)]
      v_output.append(''.join([v_filename, ", Line: ", str((v_line + 1)), ", Col: ", str((v_col + 1))]))
    v_i += 1
  return v_output

def v_tokenHelperGetFileLine(v_vm, v_fileId, v_lineNum):
  v_sourceCode = v_vm[3][1][v_fileId]
  if (v_sourceCode == None):
    return None
  return v_sourceCode.split("\n")[v_lineNum]

def v_tokenHelperGetFormattedPointerToToken(v_vm, v_token):
  v_line = v_tokenHelperGetFileLine(v_vm, v_token[2], (v_token[0] + 1))
  if (v_line == None):
    return None
  v_columnIndex = v_token[1]
  v_lineLength = len(v_line)
  v_line = v_line.lstrip()
  v_line = v_line.replace("\t", " ")
  v_offset = (v_lineLength - len(v_line))
  v_columnIndex -= v_offset
  v_line2 = ""
  while (v_columnIndex > 0):
    v_columnIndex -= 1
    v_line2 = v_line2 + " "
  v_line2 = v_line2 + "^"
  return ''.join([v_line, "\n", v_line2])

def v_tokenHelplerIsFilePathLibrary(v_vm, v_fileId, v_allFiles):
  v_filename = v_tokenHelperGetFileLine(v_vm, v_fileId, 0)
  return not (v_filename.lower().endswith(".cry"))

def v_typeInfoToString(v_vm, v_typeInfo, v_i):
  v_output = []
  v_typeToStringBuilder(v_vm, v_output, v_typeInfo, v_i)
  return "".join(v_output)

def v_typeToString(v_vm, v_typeInfo, v_i):
  v_sb = []
  v_typeToStringBuilder(v_vm, v_sb, v_typeInfo, v_i)
  return "".join(v_sb)

def v_typeToStringBuilder(v_vm, v_sb, v_typeInfo, v_i):
  sc_0 = swlookup__typeToStringBuilder__0.get(v_typeInfo[v_i], 11)
  if (sc_0 < 6):
    if (sc_0 < 3):
      if (sc_0 == 0):
        v_sb.append("void")
        return (v_i + 1)
      elif (sc_0 == 1):
        v_sb.append("object")
        return (v_i + 1)
      else:
        v_sb.append("int")
        return (v_i + 1)
    elif (sc_0 == 3):
      v_sb.append("float")
      return (v_i + 1)
    elif (sc_0 == 4):
      v_sb.append("bool")
      return (v_i + 1)
    else:
      v_sb.append("string")
      return (v_i + 1)
  elif (sc_0 < 9):
    if (sc_0 == 6):
      v_sb.append("List<")
      v_i = v_typeToStringBuilder(v_vm, v_sb, v_typeInfo, (v_i + 1))
      v_sb.append(">")
      return v_i
    elif (sc_0 == 7):
      v_sb.append("Dictionary<")
      v_i = v_typeToStringBuilder(v_vm, v_sb, v_typeInfo, (v_i + 1))
      v_sb.append(", ")
      v_i = v_typeToStringBuilder(v_vm, v_sb, v_typeInfo, v_i)
      v_sb.append(">")
      return v_i
    else:
      v_classId = v_typeInfo[(v_i + 1)]
      if (v_classId == 0):
        v_sb.append("object")
      else:
        v_classInfo = v_vm[4][9][v_classId]
        v_sb.append(v_classInfo[16])
      return (v_i + 2)
  elif (sc_0 == 9):
    v_sb.append("Class")
    return (v_i + 1)
  elif (sc_0 == 10):
    v_n = v_typeInfo[(v_i + 1)]
    v_optCount = v_typeInfo[(v_i + 2)]
    v_i += 2
    v_sb.append("function(")
    v_ret = []
    v_i = v_typeToStringBuilder(v_vm, v_ret, v_typeInfo, v_i)
    v_j = 1
    while (v_j < v_n):
      if (v_j > 1):
        v_sb.append(", ")
      v_i = v_typeToStringBuilder(v_vm, v_sb, v_typeInfo, v_i)
      v_j += 1
    if (v_n == 1):
      v_sb.append("void")
    v_sb.append(" => ")
    v_optStart = (v_n - v_optCount - 1)
    v_j = 0
    while (v_j < len(v_ret)):
      if (v_j >= v_optStart):
        v_sb.append("(opt) ")
      v_sb.append(v_ret[v_j])
      v_j += 1
    v_sb.append(")")
    return v_i
  else:
    v_sb.append("UNKNOWN")
    return (v_i + 1)

swlookup__typeToStringBuilder__0 = { -1: 0, 0: 1, 1: 1, 3: 2, 4: 3, 2: 4, 5: 5, 6: 6, 7: 7, 8: 8, 10: 9, 9: 10 }

def v_typeToStringFromValue(v_vm, v_value):
  v_sb = None
  sc_0 = swlookup__typeToStringFromValue__0.get(v_value[0], 10)
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
      v_classId = (v_value[1])[0]
      v_classInfo = v_vm[4][9][v_classId]
      return v_classInfo[16]
    elif (sc_0 == 7):
      v_sb = []
      v_sb.append("List<")
      v_list = v_value[1]
      if (v_list[0] == None):
        v_sb.append("object")
      else:
        v_typeToStringBuilder(v_vm, v_sb, v_list[0], 0)
      v_sb.append(">")
      return "".join(v_sb)
    else:
      v_dict = v_value[1]
      v_sb = []
      v_sb.append("Dictionary<")
      sc_1 = swlookup__typeToStringFromValue__1.get(v_dict[1], 3)
      if (sc_1 < 2):
        if (sc_1 == 0):
          v_sb.append("int")
        else:
          v_sb.append("string")
      elif (sc_1 == 2):
        v_sb.append("object")
      else:
        v_sb.append("???")
      v_sb.append(", ")
      if (v_dict[3] == None):
        v_sb.append("object")
      else:
        v_typeToStringBuilder(v_vm, v_sb, v_dict[3], 0)
      v_sb.append(">")
      return "".join(v_sb)
  elif (sc_0 == 9):
    return "Function"
  else:
    return "Unknown"

swlookup__typeToStringFromValue__1 = { 3: 0, 5: 1, 8: 2 }
swlookup__typeToStringFromValue__0 = { 1: 0, 2: 1, 3: 2, 4: 3, 5: 4, 10: 5, 8: 6, 6: 7, 7: 8, 9: 9 }

def v_uncaughtExceptionResult(v_vm, v_exception):
  return [3, v_unrollExceptionOutput(v_vm, v_exception), 0.0, 0, False, ""]

def v_unrollExceptionOutput(v_vm, v_exceptionInstance):
  v_objInstance = v_exceptionInstance[1]
  v_classInfo = v_vm[4][9][v_objInstance[0]]
  v_pcs = v_objInstance[3][1]
  v_codeFormattedPointer = ""
  v_exceptionName = v_classInfo[16]
  v_message = v_valueToString(v_vm, v_objInstance[2][1])
  v_trace = v_tokenHelperConvertPcsToStackTraceStrings(v_vm, v_pcs)
  v_trace.pop()
  v_trace.append("Stack Trace:")
  v_trace.reverse()
  v_pcs.reverse()
  v_showLibStack = v_vm[11][1]
  if (not (v_showLibStack) and not (v_stackItemIsLibrary(v_trace[0]))):
    while v_stackItemIsLibrary(v_trace[(len(v_trace) - 1)]):
      v_trace.pop()
      v_pcs.pop()
  v_tokensAtPc = v_vm[3][0][v_pcs[(len(v_pcs) - 1)]]
  if (v_tokensAtPc != None):
    v_codeFormattedPointer = "\n\n" + v_tokenHelperGetFormattedPointerToToken(v_vm, v_tokensAtPc[0])
  v_stackTrace = "\n".join(v_trace)
  return ''.join([v_stackTrace, v_codeFormattedPointer, "\n", v_exceptionName, ": ", v_message])

def v_valueConcatLists(v_a, v_b):
  return [None, (v_a[1] + v_b[1]), v_a[2] + v_b[2]]

def v_valueMultiplyList(v_a, v_n):
  v_len = (v_a[1] * v_n)
  v_output = v_makeEmptyList(v_a[0], v_len)
  if (v_len == 0):
    return v_output
  v_aLen = v_a[1]
  v_i = 0
  v_value = None
  if (v_aLen == 1):
    v_value = v_a[2][0]
    v_i = 0
    while (v_i < v_n):
      v_output[2].append(v_value)
      v_i += 1
  else:
    v_j = 0
    v_i = 0
    while (v_i < v_n):
      v_j = 0
      while (v_j < v_aLen):
        v_output[2].append(v_a[2][v_j])
        v_j += 1
      v_i += 1
  v_output[1] = v_len
  return v_output

def v_valueStackIncreaseCapacity(v_ec):
  v_stack = v_ec[4]
  v_oldCapacity = len(v_stack)
  v_newCapacity = (v_oldCapacity * 2)
  v_newStack = (PST_NoneListOfOne * v_newCapacity)
  v_i = (v_oldCapacity - 1)
  while (v_i >= 0):
    v_newStack[v_i] = v_stack[v_i]
    v_i -= 1
  v_ec[4] = v_newStack
  return v_newStack

def v_valueToString(v_vm, v_wrappedValue):
  v_type = v_wrappedValue[0]
  if (v_type == 1):
    return "null"
  if (v_type == 2):
    if v_wrappedValue[1]:
      return "true"
    return "false"
  if (v_type == 4):
    v_floatStr = str(v_wrappedValue[1])
    if not (("." in v_floatStr)):
      v_floatStr += ".0"
    return v_floatStr
  if (v_type == 3):
    return str(v_wrappedValue[1])
  if (v_type == 5):
    return v_wrappedValue[1]
  if (v_type == 6):
    v_internalList = v_wrappedValue[1]
    v_output = "["
    v_i = 0
    while (v_i < v_internalList[1]):
      if (v_i > 0):
        v_output += ", "
      v_output += v_valueToString(v_vm, v_internalList[2][v_i])
      v_i += 1
    v_output += "]"
    return v_output
  if (v_type == 8):
    v_objInstance = v_wrappedValue[1]
    v_classId = v_objInstance[0]
    v_ptr = v_objInstance[1]
    v_classInfo = v_vm[4][9][v_classId]
    v_nameId = v_classInfo[1]
    v_className = v_vm[4][0][v_nameId]
    return ''.join(["Instance<", v_className, "#", str(v_ptr), ">"])
  if (v_type == 7):
    v_dict = v_wrappedValue[1]
    if (v_dict[0] == 0):
      return "{}"
    v_output = "{"
    v_keyList = v_dict[6]
    v_valueList = v_dict[7]
    v_i = 0
    while (v_i < v_dict[0]):
      if (v_i > 0):
        v_output += ", "
      v_output += ''.join([v_valueToString(v_vm, v_dict[6][v_i]), ": ", v_valueToString(v_vm, v_dict[7][v_i])])
      v_i += 1
    v_output += " }"
    return v_output
  return "<unknown>"

def v_vm_getCurrentExecutionContextId(v_vm):
  return v_vm[1]

def v_vm_suspend(v_vm, v_status):
  return v_vm_suspend_for_context(v_getExecutionContext(v_vm, -1), 1)

def v_vm_suspend_for_context(v_ec, v_status):
  v_ec[11] = True
  v_ec[12] = v_status
  return 0

def v_vm_suspend_with_status(v_vm, v_status):
  return v_vm_suspend_for_context(v_getExecutionContext(v_vm, -1), v_status)

def v_vmEnvSetCommandLineArgs(v_vm, v_args):
  v_vm[11][0] = v_args