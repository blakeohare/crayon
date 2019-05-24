package org.crayonlang.libraries.http;

import java.util.ArrayList;
import java.util.HashMap;
import org.crayonlang.interpreter.PlatformTranslationHelper;
import org.crayonlang.interpreter.structs.*;
import org.crayonlang.interpreter.TranslationHelper;

public final class LibraryWrapper {

  private static final int[] PST_intBuffer16 = new int[16];

  private static final String[] PST_stringBuffer16 = new String[16];

  public static Value lib_http_fastEnsureAllBytes(VmContext vm, Value[] args) {
    if ((args[0].type == 6)) {
      ListImpl list1 = ((ListImpl) args[0].internalValue);
      int i = list1.size;
      int int1 = 0;
      int[] intArray1 = new int[i];
      Value value = null;
      while ((i > 0)) {
        i -= 1;
        value = list1.array[i];
        if ((value.type != 3)) {
          return vm.globalFalse;
        }
        int1 = ((int) value.internalValue);
        if ((int1 < 0)) {
          if ((int1 < -128)) {
            return vm.globalFalse;
          }
          int1 += 256;
        } else {
          if ((int1 >= 256)) {
            return vm.globalFalse;
          }
        }
        intArray1[i] = int1;
      }
      Object[] objArray1 = new Object[1];
      objArray1[0] = intArray1;
      ObjectInstance objInstance1 = ((ObjectInstance) args[1].internalValue);
      objInstance1.nativeData = objArray1;
      return vm.globalTrue;
    }
    return vm.globalFalse;
  }

  public static Value lib_http_getResponseBytes(VmContext vm, Value[] args) {
    Value outputListValue = args[1];
    ObjectInstance objInstance1 = ((ObjectInstance) args[0].internalValue);
    Object[] objArray1 = objInstance1.nativeData;
    ArrayList<Value> tList = new ArrayList<Value>();
    crayonlib.http.HttpHelper.getResponseBytes(objArray1[0], vm.globals.positiveIntegers, tList);
    ListImpl outputList = ((ListImpl) outputListValue.internalValue);
    Value t = org.crayonlang.interpreter.vm.CrayonWrapper.buildList(tList);
    ListImpl otherList = ((ListImpl) t.internalValue);
    outputList.capacity = otherList.capacity;
    outputList.array = otherList.array;
    outputList.size = tList.size();
    return outputListValue;
  }

  public static Value lib_http_pollRequest(VmContext vm, Value[] args) {
    ObjectInstance objInstance1 = ((ObjectInstance) args[0].internalValue);
    Object[] objArray1 = objInstance1.nativeData;
    if (crayonlib.http.HttpHelper.pollRequest(objArray1)) {
      return vm.globalTrue;
    }
    return vm.globalFalse;
  }

  public static Value lib_http_populateResponse(VmContext vm, Value[] args) {
    Value arg2 = args[1];
    Value arg3 = args[2];
    Value arg4 = args[3];
    ObjectInstance objInstance1 = ((ObjectInstance) args[0].internalValue);
    Object object1 = objInstance1.nativeData[0];
    Object[] objArray1 = new Object[1];
    ArrayList<String> stringList1 = new ArrayList<String>();
    crayonlib.http.HttpHelper.readResponseData(object1, PST_intBuffer16, PST_stringBuffer16, objArray1, stringList1);
    objInstance1 = ((ObjectInstance) arg2.internalValue);
    objInstance1.nativeData = objArray1;
    ListImpl outputList = ((ListImpl) arg3.internalValue);
    org.crayonlang.interpreter.vm.CrayonWrapper.addToList(outputList, org.crayonlang.interpreter.vm.CrayonWrapper.buildInteger(vm.globals, PST_intBuffer16[0]));
    org.crayonlang.interpreter.vm.CrayonWrapper.addToList(outputList, org.crayonlang.interpreter.vm.CrayonWrapper.buildString(vm.globals, PST_stringBuffer16[0]));
    Value value = vm.globalNull;
    Value value2 = vm.globalTrue;
    if ((PST_intBuffer16[1] == 0)) {
      value = org.crayonlang.interpreter.vm.CrayonWrapper.buildString(vm.globals, PST_stringBuffer16[1]);
      value2 = vm.globalFalse;
    }
    org.crayonlang.interpreter.vm.CrayonWrapper.addToList(outputList, value);
    org.crayonlang.interpreter.vm.CrayonWrapper.addToList(outputList, value2);
    ListImpl list1 = ((ListImpl) arg4.internalValue);
    int i = 0;
    while ((i < stringList1.size())) {
      org.crayonlang.interpreter.vm.CrayonWrapper.addToList(list1, org.crayonlang.interpreter.vm.CrayonWrapper.buildString(vm.globals, stringList1.get(i)));
      i += 1;
    }
    return vm.globalNull;
  }

  public static Value lib_http_sendRequest(VmContext vm, Value[] args) {
    Value body = args[5];
    ObjectInstance objInstance1 = ((ObjectInstance) args[0].internalValue);
    Object[] objArray1 = new Object[3];
    objInstance1.nativeData = objArray1;
    objArray1[2] = false;
    String method = ((String) args[2].internalValue);
    String url = ((String) args[3].internalValue);
    ArrayList<String> headers = new ArrayList<String>();
    ListImpl list1 = ((ListImpl) args[4].internalValue);
    int i = 0;
    while ((i < list1.size)) {
      headers.add(((String) list1.array[i].internalValue));
      i += 1;
    }
    Object bodyRawObject = body.internalValue;
    int bodyState = 0;
    if ((body.type == 5)) {
      bodyState = 1;
    } else {
      if ((body.type == 8)) {
        objInstance1 = ((ObjectInstance) bodyRawObject);
        bodyRawObject = objInstance1.nativeData[0];
        bodyState = 2;
      } else {
        bodyRawObject = null;
      }
    }
    boolean getResponseAsText = (((int) args[6].internalValue) == 1);
    if (((boolean) args[1].internalValue)) {
      crayonlib.http.HttpHelper.sendRequestAsync(objArray1, method, url, headers, bodyState, bodyRawObject, getResponseAsText, vm, args[8], (((ObjectInstance) args[9].internalValue)).nativeData);
    } else {
      int execId = ((int) args[7].internalValue);
      if (crayonlib.http.HttpHelper.sendRequestSync(objArray1, method, url, headers, bodyState, bodyRawObject, getResponseAsText)) {
        org.crayonlang.interpreter.vm.CrayonWrapper.vm_suspend_context_by_id(vm, execId, 1);
      }
    }
    return vm.globalNull;
  }
}
