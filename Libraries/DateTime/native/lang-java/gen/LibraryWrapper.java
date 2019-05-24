package org.crayonlang.libraries.datetime;

import java.util.ArrayList;
import java.util.HashMap;
import org.crayonlang.interpreter.PlatformTranslationHelper;
import org.crayonlang.interpreter.structs.*;
import org.crayonlang.interpreter.TranslationHelper;

public final class LibraryWrapper {

  private static final String[] PST_stringBuffer16 = new String[16];

  private static final int[] PST_intBuffer16 = new int[16];

  public static Object lib_datetime_getNativeTimezone(Value value) {
    ObjectInstance tzObj = ((ObjectInstance) value.internalValue);
    if ((tzObj.nativeData == null)) {
      return null;
    }
    return tzObj.nativeData[0];
  }

  public static Value lib_datetime_getUtcOffsetAt(VmContext vm, Value[] args) {
    Object nativeTz = lib_datetime_getNativeTimezone(args[0]);
    int unixTime = ((int) args[1].internalValue);
    int offsetSeconds = DateTimeHelper.getUtcOffsetAt(nativeTz, unixTime);
    return org.crayonlang.interpreter.vm.CrayonWrapper.buildInteger(vm.globals, offsetSeconds);
  }

  public static Value lib_datetime_initTimeZone(VmContext vm, Value[] args) {
    ObjectInstance timezone = ((ObjectInstance) args[0].internalValue);
    timezone.nativeData = new Object[1];
    Object nativeTzRef = null;
    String readableName = null;
    int offsetFromUtc = 0;
    int isDstObserved = 0;
    String fingerprint = null;
    if ((args[1].type == 1)) {
      String[] strOut = PST_stringBuffer16;
      int[] intOut = PST_intBuffer16;
      nativeTzRef = DateTimeHelper.getDataForLocalTimeZone(strOut, intOut);
      readableName = strOut[0];
      fingerprint = strOut[1];
      offsetFromUtc = intOut[0];
      isDstObserved = intOut[1];
    } else {
      return vm.globalNull;
    }
    timezone.nativeData = new Object[5];
    timezone.nativeData[0] = nativeTzRef;
    timezone.nativeData[1] = readableName;
    timezone.nativeData[2] = offsetFromUtc;
    timezone.nativeData[3] = (isDstObserved == 1);
    timezone.nativeData[4] = fingerprint;
    ArrayList<Value> values = new ArrayList<Value>();
    values.add(org.crayonlang.interpreter.vm.CrayonWrapper.buildString(vm.globals, readableName));
    values.add(org.crayonlang.interpreter.vm.CrayonWrapper.buildInteger(vm.globals, offsetFromUtc));
    values.add(org.crayonlang.interpreter.vm.CrayonWrapper.buildBoolean(vm.globals, (isDstObserved == 1)));
    values.add(org.crayonlang.interpreter.vm.CrayonWrapper.buildString(vm.globals, fingerprint));
    return org.crayonlang.interpreter.vm.CrayonWrapper.buildList(values);
  }

  public static Value lib_datetime_initTimeZoneList(VmContext vm, Value[] args) {
    ObjectInstance obj = ((ObjectInstance) args[0].internalValue);
    obj.nativeData = new Object[1];
    Object[] timezones = DateTimeHelper.initializeTimeZoneList();
    obj.nativeData[0] = timezones;
    int length = timezones.length;
    return org.crayonlang.interpreter.vm.CrayonWrapper.buildInteger(vm.globals, length);
  }

  public static Value lib_datetime_isDstOccurringAt(VmContext vm, Value[] args) {
    Object nativeTz = lib_datetime_getNativeTimezone(args[0]);
    int unixtime = ((int) args[1].internalValue);
    return org.crayonlang.interpreter.vm.CrayonWrapper.buildBoolean(vm.globals, DateTimeHelper.isDstOccurringAt(nativeTz, unixtime));
  }

  public static Value lib_datetime_parseDate(VmContext vm, Value[] args) {
    int year = ((int) args[0].internalValue);
    int month = ((int) args[1].internalValue);
    int day = ((int) args[2].internalValue);
    int hour = ((int) args[3].internalValue);
    int minute = ((int) args[4].internalValue);
    int microseconds = ((int) args[5].internalValue);
    Object nullableTimeZone = lib_datetime_getNativeTimezone(args[6]);
    if (((year >= 1970) && (year < 2100) && (month >= 1) && (month <= 12) && (day >= 1) && (day <= 31) && (hour >= 0) && (hour < 24) && (minute >= 0) && (minute < 60) && (microseconds >= 0) && (microseconds < 60000000))) {
      int[] intOut = PST_intBuffer16;
      DateTimeHelper.parseDate(intOut, nullableTimeZone, year, month, day, hour, minute, microseconds);
      if ((intOut[0] == 1)) {
        double unixFloat = (intOut[1] + intOut[2] / 1000000.0);
        return org.crayonlang.interpreter.vm.CrayonWrapper.buildFloat(vm.globals, unixFloat);
      }
    }
    return vm.globalNull;
  }

  public static Value lib_datetime_unixToStructured(VmContext vm, Value[] args) {
    double unixTime = ((double) args[0].internalValue);
    Object nullableTimeZone = lib_datetime_getNativeTimezone(args[1]);
    ArrayList<Value> output = new ArrayList<Value>();
    int[] intOut = PST_intBuffer16;
    boolean success = DateTimeHelper.unixToStructured(intOut, nullableTimeZone, unixTime);
    if (!success) {
      return vm.globalNull;
    }
    int i = 0;
    while ((i < 9)) {
      output.add(org.crayonlang.interpreter.vm.CrayonWrapper.buildInteger(vm.globals, intOut[i]));
      i += 1;
    }
    return org.crayonlang.interpreter.vm.CrayonWrapper.buildList(output);
  }
}
