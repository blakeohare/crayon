package org.crayonlang.libraries.fileiocommon;

import java.util.ArrayList;
import java.util.HashMap;
import org.crayonlang.interpreter.PlatformTranslationHelper;
import org.crayonlang.interpreter.structs.*;
import org.crayonlang.interpreter.TranslationHelper;

public final class LibraryWrapper {

  private static final String[] PST_stringBuffer16 = new String[16];

  private static final int[] PST_intBuffer16 = new int[16];

  private static final double[] PST_floatBuffer16 = new double[16];

  public static Value lib_fileiocommon_directoryCreate(VmContext vm, Value[] args) {
    boolean bool1 = false;
    int i = 0;
    int int1 = 0;
    ArrayList<String> stringList1 = null;
    Value hostObject = args[0];
    String path = ((String) args[1].internalValue);
    if (((boolean) args[2].internalValue)) {
      int1 = 0;
      if (!FileIOHelper.directoryExists(FileIOHelper.getDirRoot(path))) {
        int1 = 4;
      } else {
        stringList1 = new ArrayList<String>();
        bool1 = true;
        while ((bool1 && !FileIOHelper.directoryExists(path))) {
          stringList1.add(path);
          int1 = FileIOHelper.getDirParent(path, PST_stringBuffer16);
          path = PST_stringBuffer16[0];
          if ((int1 != 0)) {
            bool1 = false;
          }
        }
        if (bool1) {
          i = (stringList1.size() - 1);
          while ((i >= 0)) {
            path = stringList1.get(i);
            int1 = FileIOHelper.createDirectory(path);
            if ((int1 != 0)) {
              i = -1;
            }
            i -= 1;
          }
        }
      }
    } else {
      int1 = FileIOHelper.createDirectory(path);
    }
    return org.crayonlang.interpreter.vm.CrayonWrapper.buildInteger(vm.globals, int1);
  }

  public static Value lib_fileiocommon_directoryDelete(VmContext vm, Value[] args) {
    int sc = FileIOHelper.deleteDirectory(((String) args[1].internalValue));
    return org.crayonlang.interpreter.vm.CrayonWrapper.buildInteger(vm.globals, sc);
  }

  public static Value lib_fileiocommon_directoryList(VmContext vm, Value[] args) {
    Value diskhost = args[0];
    String path = ((String) args[1].internalValue);
    boolean useFullPath = ((boolean) args[2].internalValue);
    ListImpl outputList = ((ListImpl) args[3].internalValue);
    ArrayList<String> stringList1 = new ArrayList<String>();
    int sc = FileIOHelper.getDirectoryList(path, useFullPath, stringList1);
    if ((sc == 0)) {
      int i = 0;
      while ((i < stringList1.size())) {
        org.crayonlang.interpreter.vm.CrayonWrapper.addToList(outputList, org.crayonlang.interpreter.vm.CrayonWrapper.buildString(vm.globals, stringList1.get(i)));
        i += 1;
      }
    }
    return org.crayonlang.interpreter.vm.CrayonWrapper.buildInteger(vm.globals, sc);
  }

  public static Value lib_fileiocommon_directoryMove(VmContext vm, Value[] args) {
    int statusCode = FileIOHelper.moveDirectory(((String) args[1].internalValue), ((String) args[2].internalValue));
    return org.crayonlang.interpreter.vm.CrayonWrapper.buildInteger(vm.globals, statusCode);
  }

  public static Value lib_fileiocommon_fileDelete(VmContext vm, Value[] args) {
    int statusCode = FileIOHelper.fileDelete(((String) args[1].internalValue));
    return org.crayonlang.interpreter.vm.CrayonWrapper.buildInteger(vm.globals, statusCode);
  }

  public static Value lib_fileiocommon_fileInfo(VmContext vm, Value[] args) {
    int mask = ((int) args[2].internalValue);
    FileIOHelper.getFileInfo(((String) args[1].internalValue), mask, PST_intBuffer16, PST_floatBuffer16);
    ListImpl outputList = ((ListImpl) args[3].internalValue);
    org.crayonlang.interpreter.vm.CrayonWrapper.clearList(outputList);
    VmGlobals globals = vm.globals;
    org.crayonlang.interpreter.vm.CrayonWrapper.addToList(outputList, org.crayonlang.interpreter.vm.CrayonWrapper.buildBoolean(globals, (PST_intBuffer16[0] > 0)));
    org.crayonlang.interpreter.vm.CrayonWrapper.addToList(outputList, org.crayonlang.interpreter.vm.CrayonWrapper.buildBoolean(globals, (PST_intBuffer16[1] > 0)));
    if (((mask & 1) != 0)) {
      org.crayonlang.interpreter.vm.CrayonWrapper.addToList(outputList, org.crayonlang.interpreter.vm.CrayonWrapper.buildInteger(globals, PST_intBuffer16[2]));
    } else {
      org.crayonlang.interpreter.vm.CrayonWrapper.addToList(outputList, globals.valueNull);
    }
    if (((mask & 2) != 0)) {
      org.crayonlang.interpreter.vm.CrayonWrapper.addToList(outputList, org.crayonlang.interpreter.vm.CrayonWrapper.buildBoolean(globals, (PST_intBuffer16[3] > 0)));
    } else {
      org.crayonlang.interpreter.vm.CrayonWrapper.addToList(outputList, globals.valueNull);
    }
    if (((mask & 4) != 0)) {
      org.crayonlang.interpreter.vm.CrayonWrapper.addToList(outputList, org.crayonlang.interpreter.vm.CrayonWrapper.buildFloat(globals, PST_floatBuffer16[0]));
    } else {
      org.crayonlang.interpreter.vm.CrayonWrapper.addToList(outputList, globals.valueNull);
    }
    if (((mask & 8) != 0)) {
      org.crayonlang.interpreter.vm.CrayonWrapper.addToList(outputList, org.crayonlang.interpreter.vm.CrayonWrapper.buildFloat(globals, PST_floatBuffer16[1]));
    } else {
      org.crayonlang.interpreter.vm.CrayonWrapper.addToList(outputList, globals.valueNull);
    }
    return args[3];
  }

  public static Value lib_fileiocommon_fileMove(VmContext vm, Value[] args) {
    int statusCode = FileIOHelper.fileMove(((String) args[1].internalValue), ((String) args[2].internalValue), ((boolean) args[3].internalValue), ((boolean) args[4].internalValue));
    return org.crayonlang.interpreter.vm.CrayonWrapper.buildInteger(vm.globals, statusCode);
  }

  public static Value lib_fileiocommon_fileRead(VmContext vm, Value[] args) {
    Value diskHostObject = args[0];
    String sandboxedPath = ((String) args[1].internalValue);
    boolean readDataAsBytes = ((boolean) args[2].internalValue);
    ListImpl outputList = ((ListImpl) args[3].internalValue);
    ArrayList<Value> tList = new ArrayList<Value>();
    int statusCode = FileIOHelper.fileRead(sandboxedPath, readDataAsBytes, PST_stringBuffer16, vm.globals.positiveIntegers, tList);
    if (((statusCode == 0) && !readDataAsBytes)) {
      org.crayonlang.interpreter.vm.CrayonWrapper.addToList(outputList, org.crayonlang.interpreter.vm.CrayonWrapper.buildString(vm.globals, PST_stringBuffer16[0]));
    } else {
      Value t = org.crayonlang.interpreter.vm.CrayonWrapper.buildList(tList);
      ListImpl tListImpl = ((ListImpl) t.internalValue);
      outputList.array = tListImpl.array;
      outputList.capacity = tListImpl.capacity;
      outputList.size = tList.size();
    }
    return org.crayonlang.interpreter.vm.CrayonWrapper.buildInteger(vm.globals, statusCode);
  }

  public static Value lib_fileiocommon_fileWrite(VmContext vm, Value[] args) {
    Value[] ints = vm.globals.positiveIntegers;
    if ((args[3].type != 3)) {
      return ints[3];
    }
    int statusCode = 0;
    String contentString = null;
    Object byteArrayRef = null;
    int format = ((int) args[3].internalValue);
    if ((format == 0)) {
      byteArrayRef = lib_fileiocommon_listToBytes(((ListImpl) args[2].internalValue));
      if ((byteArrayRef == null)) {
        return ints[6];
      }
    } else if ((args[2].type != 5)) {
      return ints[6];
    } else {
      contentString = ((String) args[2].internalValue);
    }
    if ((statusCode == 0)) {
      statusCode = FileIOHelper.fileWrite(((String) args[1].internalValue), format, contentString, byteArrayRef);
    }
    return org.crayonlang.interpreter.vm.CrayonWrapper.buildInteger(vm.globals, statusCode);
  }

  public static Value lib_fileiocommon_getCurrentDirectory(VmContext vm, Value[] args) {
    return org.crayonlang.interpreter.vm.CrayonWrapper.buildString(vm.globals, FileIOHelper.getCurrentDirectory());
  }

  public static Object lib_fileiocommon_getDiskObject(Value diskObjectArg) {
    ObjectInstance objInst = ((ObjectInstance) diskObjectArg.internalValue);
    return objInst.nativeData[0];
  }

  public static Value lib_fileiocommon_getUserDirectory(VmContext vm, Value[] args) {
    return org.crayonlang.interpreter.vm.CrayonWrapper.buildString(vm.globals, FileIOHelper.getUserDirectory());
  }

  public static Value lib_fileiocommon_initializeDisk(VmContext vm, Value[] args) {
    ObjectInstance objInstance1 = ((ObjectInstance) args[0].internalValue);
    Object[] objArray1 = new Object[1];
    objInstance1.nativeData = objArray1;
    Object object1 = TranslationHelper.alwaysFalse();
    objArray1[0] = object1;
    return vm.globals.valueNull;
  }

  public static Value lib_fileiocommon_isWindows(VmContext vm, Value[] args) {
    if (FileIOHelper.isWindows()) {
      return vm.globals.boolTrue;
    }
    return vm.globals.boolFalse;
  }

  public static Object lib_fileiocommon_listToBytes(ListImpl listOfMaybeInts) {
    byte[] bytes = new byte[listOfMaybeInts.size];
    Value intValue = null;
    int byteValue = 0;
    int i = (listOfMaybeInts.size - 1);
    while ((i >= 0)) {
      intValue = listOfMaybeInts.array[i];
      if ((intValue.type != 3)) {
        return null;
      }
      byteValue = ((int) intValue.internalValue);
      if ((byteValue >= 256)) {
        return null;
      }
      if ((byteValue < 0)) {
        if ((byteValue < -128)) {
          return null;
        }
        byteValue += 256;
      }
      bytes[i] = ((byte) byteValue);
      i -= 1;
    }
    return bytes;
  }

  public static Value lib_fileiocommon_textToLines(VmContext vm, Value[] args) {
    lib_fileiocommon_textToLinesImpl(vm.globals, ((String) args[0].internalValue), ((ListImpl) args[1].internalValue));
    return args[1];
  }

  public static int lib_fileiocommon_textToLinesImpl(VmGlobals globals, String text, ListImpl output) {
    ArrayList<String> stringList = new ArrayList<String>();
    FileIOHelper.textToLines(text, stringList);
    int _len = stringList.size();
    int i = 0;
    while ((i < _len)) {
      org.crayonlang.interpreter.vm.CrayonWrapper.addToList(output, org.crayonlang.interpreter.vm.CrayonWrapper.buildString(globals, stringList.get(i)));
      i += 1;
    }
    return 0;
  }
}
