package org.crayonlang.libraries.matrices;

import java.util.ArrayList;
import java.util.HashMap;
import org.crayonlang.interpreter.PlatformTranslationHelper;
import org.crayonlang.interpreter.structs.*;
import org.crayonlang.interpreter.TranslationHelper;

public final class LibraryWrapper {

  public static Value lib_matrices_addMatrix(VmContext vm, Value[] args) {
    ObjectInstance obj = ((ObjectInstance) args[0].internalValue);
    Object[] nd1 = obj.nativeData;
    if (!((boolean) args[2].internalValue)) {
      nd1[5] = "Input must be a matrix";
      return vm.globalNull;
    }
    double[] left = ((double[]) nd1[0]);
    int width = ((int) nd1[1]);
    int height = ((int) nd1[2]);
    obj = ((ObjectInstance) args[1].internalValue);
    Object[] nd2 = obj.nativeData;
    double[] right = ((double[]) nd2[0]);
    if (((((int) nd2[1]) != width) || (((int) nd2[2]) != height))) {
      nd1[5] = "Matrices must be the same size.";
      return vm.globalNull;
    }
    double[] output = left;
    boolean isInline = (args[3].type == 1);
    if (isInline) {
      nd1[4] = true;
    } else {
      if (!((boolean) args[4].internalValue)) {
        nd1[5] = "Output value must be a matrix";
        return vm.globalNull;
      } else {
        obj = ((ObjectInstance) args[3].internalValue);
        Object[] nd3 = obj.nativeData;
        output = ((double[]) nd3[0]);
        if (((((int) nd3[1]) != width) || (((int) nd3[2]) != height))) {
          nd1[5] = "Output matrix must have the same size as the inputs.";
          return vm.globalNull;
        }
        nd3[4] = true;
      }
    }
    int length = (width * height);
    int i = 0;
    while ((i < length)) {
      output[i] = (left[i] + right[i]);
      i += 1;
    }
    return args[0];
  }

  public static Value lib_matrices_copyFrom(VmContext vm, Value[] args) {
    ObjectInstance obj = ((ObjectInstance) args[0].internalValue);
    Object[] nd1 = obj.nativeData;
    obj = ((ObjectInstance) args[1].internalValue);
    Object[] nd2 = obj.nativeData;
    if (!((boolean) args[2].internalValue)) {
      nd1[5] = "value was not a matrix";
      return vm.globalNull;
    }
    if (((((int) nd1[1]) != ((int) nd2[1])) || (((int) nd1[2]) != ((int) nd2[2])))) {
      nd1[5] = "Matrices were not the same size.";
      return vm.globalNull;
    }
    double[] target = ((double[]) nd1[0]);
    double[] source = ((double[]) nd2[0]);
    int _len = target.length;
    int i = 0;
    while ((i < _len)) {
      target[i] = source[i];
      i += 1;
    }
    return args[0];
  }

  public static Value lib_matrices_getError(VmContext vm, Value[] args) {
    ObjectInstance obj = ((ObjectInstance) args[0].internalValue);
    return org.crayonlang.interpreter.vm.CrayonWrapper.buildString(vm.globals, ((String) obj.nativeData[5]));
  }

  public static Value lib_matrices_getValue(VmContext vm, Value[] args) {
    ObjectInstance obj = ((ObjectInstance) args[0].internalValue);
    Object[] nd = obj.nativeData;
    if (((args[1].type != 3) || (args[2].type != 3))) {
      nd[5] = "Invalid coordinates";
      return vm.globalNull;
    }
    int x = ((int) args[1].internalValue);
    int y = ((int) args[2].internalValue);
    int width = ((int) nd[1]);
    int height = ((int) nd[2]);
    if (((x < 0) || (x >= width) || (y < 0) || (y >= height))) {
      nd[5] = "Coordinates out of range.";
      return vm.globalNull;
    }
    Value[] valueArray = ((Value[]) nd[3]);
    if (!((boolean) nd[4])) {
      double[] data = ((double[]) nd[0]);
      int length = (width * height);
      int i = 0;
      while ((i < length)) {
        valueArray[i] = org.crayonlang.interpreter.vm.CrayonWrapper.buildFloat(vm.globals, data[i]);
        i += 1;
      }
    }
    return valueArray[((width * y) + x)];
  }

  public static Value lib_matrices_initMatrix(VmContext vm, Value[] args) {
    ObjectInstance obj = ((ObjectInstance) args[0].internalValue);
    Object[] nd = obj.nativeData;
    if (((args[1].type != 3) || (args[2].type != 3))) {
      nd[5] = "Width and height must be integers.";
      return vm.globalTrue;
    }
    int width = ((int) args[1].internalValue);
    int height = ((int) args[2].internalValue);
    int size = (width * height);
    double[] data = new double[size];
    nd[0] = data;
    nd[1] = width;
    nd[2] = height;
    nd[3] = new Value[size];
    nd[4] = false;
    nd[5] = "";
    nd[6] = new double[size];
    int i = 0;
    while ((i < size)) {
      data[i] = 0.0;
      i += 1;
    }
    return vm.globalFalse;
  }

  public static Value lib_matrices_multiplyMatrix(VmContext vm, Value[] args) {
    ObjectInstance obj = ((ObjectInstance) args[0].internalValue);
    Object[] nd1 = obj.nativeData;
    if (!((boolean) args[2].internalValue)) {
      nd1[5] = "argument must be a matrix";
      return vm.globalNull;
    }
    obj = ((ObjectInstance) args[1].internalValue);
    Object[] nd2 = obj.nativeData;
    boolean isInline = false;
    if ((args[3].type == 1)) {
      isInline = true;
    } else {
      if (!((boolean) args[4].internalValue)) {
        nd1[5] = "output matrix was unrecognized type.";
        return vm.globalNull;
      }
    }
    int m1width = ((int) nd1[1]);
    int m1height = ((int) nd1[2]);
    int m2width = ((int) nd2[1]);
    int m2height = ((int) nd2[2]);
    int m3width = m2width;
    int m3height = m1height;
    if ((m1width != m2height)) {
      nd1[5] = "Matrix size mismatch";
      return vm.globalNull;
    }
    double[] m1data = ((double[]) nd1[0]);
    double[] m2data = ((double[]) nd2[0]);
    Object[] nd3 = null;
    if (isInline) {
      nd3 = nd1;
      if ((m2width != m2height)) {
        nd1[5] = "You can only multiply a matrix inline with a square matrix.";
        return vm.globalNull;
      }
    } else {
      obj = ((ObjectInstance) args[3].internalValue);
      nd3 = obj.nativeData;
      if (((((int) nd3[1]) != m3width) || (((int) nd3[2]) != m3height))) {
        nd1[5] = "Output matrix is incorrect size.";
        return vm.globalNull;
      }
    }
    nd3[4] = true;
    double[] m3data = ((double[]) nd3[6]);
    int x = 0;
    int y = 0;
    int i = 0;
    int m1index = 0;
    int m2index = 0;
    int m3index = 0;
    double value = 0.0;
    y = 0;
    while ((y < m3height)) {
      x = 0;
      while ((x < m3width)) {
        value = 0.0;
        m1index = (y * m1height);
        m2index = x;
        i = 0;
        while ((i < m1width)) {
          value += (m1data[m1index] * m2data[m2index]);
          m1index += 1;
          m2index += m2width;
          i += 1;
        }
        m3data[m3index] = value;
        m3index += 1;
        x += 1;
      }
      y += 1;
    }
    Object t = nd3[0];
    nd3[0] = nd3[6];
    nd3[6] = t;
    return args[0];
  }

  public static Value lib_matrices_multiplyScalar(VmContext vm, Value[] args) {
    ObjectInstance obj = ((ObjectInstance) args[0].internalValue);
    Object[] nd = obj.nativeData;
    boolean isInline = (args[2].type == 1);
    double[] m1data = ((double[]) nd[0]);
    double[] m2data = m1data;
    if (isInline) {
      nd[4] = true;
    } else {
      if (!((boolean) args[3].internalValue)) {
        nd[5] = "output must be a matrix instance";
        return vm.globalNull;
      } else {
        obj = ((ObjectInstance) args[2].internalValue);
        Object[] nd2 = obj.nativeData;
        if (((((int) nd[1]) != ((int) nd2[1])) || (((int) nd[2]) != ((int) nd2[2])))) {
          nd[5] = "output matrix must be the same size.";
          return vm.globalNull;
        }
        m2data = ((double[]) nd2[0]);
        nd2[4] = true;
      }
    }
    double scalar = 0.0;
    if ((args[1].type == 4)) {
      scalar = ((double) args[1].internalValue);
    } else {
      if ((args[1].type == 3)) {
        scalar = (0.0 + ((int) args[1].internalValue));
      } else {
        nd[5] = "scalar must be a number";
        return vm.globalNull;
      }
    }
    int i = 0;
    int length = m1data.length;
    i = 0;
    while ((i < length)) {
      m2data[i] = (m1data[i] * scalar);
      i += 1;
    }
    return args[0];
  }

  public static Value lib_matrices_setValue(VmContext vm, Value[] args) {
    ObjectInstance obj = ((ObjectInstance) args[0].internalValue);
    Object[] nd = obj.nativeData;
    if (((args[1].type != 3) || (args[2].type != 3))) {
      nd[5] = "Invalid coordinates";
      return vm.globalNull;
    }
    int x = ((int) args[1].internalValue);
    int y = ((int) args[2].internalValue);
    int width = ((int) nd[1]);
    int height = ((int) nd[2]);
    if (((x < 0) || (x >= width) || (y < 0) || (y >= height))) {
      nd[5] = "Coordinates out of range.";
      return vm.globalNull;
    }
    double value = 0.0;
    if ((args[3].type == 4)) {
      value = ((double) args[3].internalValue);
    } else {
      if ((args[3].type == 3)) {
        value = (0.0 + ((int) args[3].internalValue));
      } else {
        nd[5] = "Value must be a number.";
        return vm.globalNull;
      }
    }
    int index = ((y * width) + x);
    double[] data = ((double[]) nd[0]);
    Value[] valueArray = ((Value[]) nd[3]);
    data[index] = value;
    valueArray[index] = org.crayonlang.interpreter.vm.CrayonWrapper.buildFloat(vm.globals, value);
    return args[0];
  }

  public static Value lib_matrices_toVector(VmContext vm, Value[] args) {
    ObjectInstance obj = ((ObjectInstance) args[0].internalValue);
    Object[] nd = obj.nativeData;
    double[] data = ((double[]) nd[0]);
    int width = ((int) nd[1]);
    int height = ((int) nd[2]);
    int length = (width * height);
    if ((args[1].type != 6)) {
      nd[5] = "Output argument must be a list";
      return vm.globalNull;
    }
    ListImpl output = ((ListImpl) args[1].internalValue);
    while ((output.size < length)) {
      org.crayonlang.interpreter.vm.CrayonWrapper.addToList(output, vm.globalNull);
    }
    double value = 0.0;
    Value toList = null;
    int i = 0;
    while ((i < length)) {
      value = data[i];
      if ((value == 0)) {
        toList = vm.globals.floatZero;
      } else {
        if ((value == 1)) {
          toList = vm.globals.floatOne;
        } else {
          toList = new Value(4, data[i]);
        }
      }
      output.array[i] = toList;
      i += 1;
    }
    return args[1];
  }
}
