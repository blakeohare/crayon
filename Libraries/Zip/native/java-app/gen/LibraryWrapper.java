package org.crayonlang.libraries.zip;

import java.util.ArrayList;
import java.util.HashMap;
import org.crayonlang.interpreter.PlatformTranslationHelper;
import org.crayonlang.interpreter.structs.*;
import org.crayonlang.interpreter.TranslationHelper;

public final class LibraryWrapper {

  public static Value lib_zip_ensureValidArchiveInfo(VmContext vm, Value[] args) {
    int sc = 0;
    if ((args[0].type != 5)) {
      sc = 1;
    }
    if (((sc == 0) && (lib_zip_validateByteList(args[1], false) != null))) {
      sc = 2;
    }
    return org.crayonlang.interpreter.vm.CrayonWrapper.buildInteger(vm.globals, sc);
  }

  public static void lib_zip_initAsyncCallback(ListImpl scOut, Object[] nativeData, Object nativeZipArchive, VmContext vm, int execContext) {
    int sc = 0;
    if ((nativeZipArchive == null)) {
      sc = 2;
    }
    org.crayonlang.interpreter.vm.CrayonWrapper.setItemInList(scOut, 0, org.crayonlang.interpreter.vm.CrayonWrapper.buildInteger(vm.globals, sc));
    nativeData[0] = nativeZipArchive;
    org.crayonlang.interpreter.vm.CrayonWrapper.runInterpreter(vm, execContext);
  }

  public static Value lib_zip_initializeZipReader(VmContext vm, Value[] args) {
    int sc = 0;
    ListImpl scOut = ((ListImpl) args[2].internalValue);
    int execId = ((int) args[3].internalValue);
    int[] byteArray = lib_zip_validateByteList(args[1], true);
    if ((byteArray == null)) {
      sc = 1;
    } else {
      ObjectInstance obj = ((ObjectInstance) args[0].internalValue);
      obj.nativeData = new Object[2];
      obj.nativeData[0] = ZipHelper.createZipReader(byteArray);
      obj.nativeData[1] = 0;
      if ((obj.nativeData[0] == null)) {
        sc = 2;
      } else {
        sc = 0;
      }
      if (TranslationHelper.alwaysFalse()) {
        sc = 3;
        org.crayonlang.interpreter.vm.CrayonWrapper.vm_suspend_context_by_id(vm, execId, 1);
      }
    }
    org.crayonlang.interpreter.vm.CrayonWrapper.setItemInList(scOut, 0, org.crayonlang.interpreter.vm.CrayonWrapper.buildInteger(vm.globals, sc));
    return vm.globalNull;
  }

  public static Value lib_zip_readerPeekNextEntry(VmContext vm, Value[] args) {
    ObjectInstance obj = ((ObjectInstance) args[0].internalValue);
    Object[] nd = obj.nativeData;
    ListImpl output = ((ListImpl) args[1].internalValue);
    int execId = ((int) args[2].internalValue);
    boolean[] boolOut = new boolean[3];
    String[] nameOut = new String[1];
    ArrayList<Integer> integers = new ArrayList<Integer>();
    ZipHelper.readNextZipEntry(nd[0], ((int) nd[1]), boolOut, nameOut, integers);
    if (TranslationHelper.alwaysFalse()) {
      org.crayonlang.interpreter.vm.CrayonWrapper.vm_suspend_context_by_id(vm, execId, 1);
      return vm.globalTrue;
    }
    return lib_zip_readerPeekNextEntryCallback(!boolOut[0], boolOut[1], boolOut[2], nameOut[0], integers, nd, output, vm);
  }

  public static Value lib_zip_readerPeekNextEntryCallback(boolean problemsEncountered, boolean foundAnything, boolean isDirectory, String name, ArrayList<Integer> bytesAsIntList, Object[] nativeData, ListImpl output, VmContext vm) {
    if (problemsEncountered) {
      return vm.globalFalse;
    }
    nativeData[1] = (1 + ((int) nativeData[1]));
    org.crayonlang.interpreter.vm.CrayonWrapper.setItemInList(output, 0, org.crayonlang.interpreter.vm.CrayonWrapper.buildBoolean(vm.globals, foundAnything));
    if (!foundAnything) {
      return vm.globalTrue;
    }
    org.crayonlang.interpreter.vm.CrayonWrapper.setItemInList(output, 1, org.crayonlang.interpreter.vm.CrayonWrapper.buildString(vm.globals, name));
    if (isDirectory) {
      org.crayonlang.interpreter.vm.CrayonWrapper.setItemInList(output, 2, org.crayonlang.interpreter.vm.CrayonWrapper.buildBoolean(vm.globals, isDirectory));
      return vm.globalTrue;
    }
    ListImpl byteValues = ((ListImpl) org.crayonlang.interpreter.vm.CrayonWrapper.getItemFromList(output, 3).internalValue);
    int length = bytesAsIntList.size();
    int i = 0;
    Value[] positiveNumbers = vm.globals.positiveIntegers;
    Value[] valuesOut = new Value[length];
    i = 0;
    while ((i < length)) {
      valuesOut[i] = positiveNumbers[bytesAsIntList.get(i)];
      i += 1;
    }
    byteValues.array = valuesOut;
    byteValues.capacity = length;
    byteValues.size = length;
    return vm.globalTrue;
  }

  public static int[] lib_zip_validateByteList(Value byteListValue, boolean convert) {
    if ((byteListValue.type != 6)) {
      return null;
    }
    int[] output = null;
    ListImpl bytes = ((ListImpl) byteListValue.internalValue);
    int length = bytes.size;
    if (convert) {
      output = new int[length];
    } else {
      output = new int[1];
      output[0] = 1;
    }
    Value value = null;
    int b = 0;
    int i = 0;
    while ((i < length)) {
      value = bytes.array[i];
      if ((value.type != 3)) {
        return null;
      }
      b = ((int) value.internalValue);
      if ((b > 255)) {
        return null;
      }
      if ((b < 0)) {
        if ((b >= -128)) {
          b += 255;
        } else {
          return null;
        }
      }
      if (convert) {
        output[i] = b;
      }
      i += 1;
    }
    return output;
  }
}
