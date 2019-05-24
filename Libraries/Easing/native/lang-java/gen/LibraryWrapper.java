package org.crayonlang.libraries.easing;

import java.util.ArrayList;
import java.util.HashMap;
import org.crayonlang.interpreter.PlatformTranslationHelper;
import org.crayonlang.interpreter.structs.*;
import org.crayonlang.interpreter.TranslationHelper;

public final class LibraryWrapper {

  public static Value lib_easing_apply_pts(VmContext vm, Value[] args) {
    ListImpl sampleValues = ((ListImpl) args[1].internalValue);
    int _len = sampleValues.size;
    double[] samples = new double[_len];
    int i = 0;
    while ((i < _len)) {
      samples[i] = ((double) sampleValues.array[i].internalValue);
      i += 1;
    }
    samples[0] = 0.0;
    samples[(_len - 1)] = 1.0;
    ObjectInstance o = ((ObjectInstance) args[0].internalValue);
    o.nativeObject = new EasingSampling(_len, samples);
    return vm.globals.valueNull;
  }

  public static Value lib_easing_interpolate(VmContext vm, Value[] args) {
    Value arg2 = args[1];
    Value arg3 = args[2];
    Value arg4 = args[3];
    Value arg5 = args[4];
    Value arg6 = args[5];
    ObjectInstance o = ((ObjectInstance) args[0].internalValue);
    EasingSampling es = ((EasingSampling) o.nativeObject);
    double[] samples = es.samples;
    int _len = es.sampleCount;
    int int1 = ((int) args[6].internalValue);
    double float1 = 0.0;
    double float2 = 0.0;
    double float3 = 0.0;
    if ((arg4.type == 3)) {
      float1 = (0.0 + ((int) arg4.internalValue));
    } else {
      if ((arg4.type == 4)) {
        float1 = ((double) arg4.internalValue);
      } else {
        return vm.globals.valueNull;
      }
    }
    if ((arg5.type == 3)) {
      float2 = (0.0 + ((int) arg5.internalValue));
    } else {
      if ((arg5.type == 4)) {
        float2 = ((double) arg5.internalValue);
      } else {
        return vm.globals.valueNull;
      }
    }
    boolean bool1 = false;
    boolean bool2 = false;
    boolean first = false;
    if ((int1 == 2)) {
      first = true;
      if (((float1 * 2.0) > float2)) {
        float1 = ((float2 - float1) * 2);
        bool1 = true;
        bool2 = true;
      } else {
        float1 *= 2.0;
      }
    } else {
      if ((int1 == 1)) {
        float1 = (float2 - float1);
        bool1 = true;
      }
    }
    if ((float2 == 0)) {
      float1 = samples[0];
    } else {
      if ((float2 < 0)) {
        float2 = -float2;
        float1 = -float1;
      }
      if ((float1 >= float2)) {
        float1 = samples[(_len - 1)];
      } else {
        if ((float1 < 0)) {
          float1 = samples[0];
        } else {
          float1 = float1 / float2;
          if ((_len > 2)) {
            float2 = (float1 * _len);
            int index = ((int) float2);
            float2 -= index;
            float1 = samples[index];
            if (((index < (_len - 1)) && (float2 > 0))) {
              float3 = samples[(index + 1)];
              float1 = ((float1 * (1 - float2)) + (float3 * float2));
            }
          }
        }
      }
    }
    if ((arg2.type == 3)) {
      float2 = (0.0 + ((int) arg2.internalValue));
    } else {
      if ((arg2.type == 4)) {
        float2 = ((double) arg2.internalValue);
      } else {
        return vm.globals.valueNull;
      }
    }
    if ((arg3.type == 3)) {
      float3 = (0.0 + ((int) arg3.internalValue));
    } else {
      if ((arg3.type == 4)) {
        float3 = ((double) arg3.internalValue);
      } else {
        return vm.globals.valueNull;
      }
    }
    if (bool1) {
      float1 = (1.0 - float1);
    }
    if (first) {
      float1 *= 0.5;
    }
    if (bool2) {
      float1 += 0.5;
    }
    float1 = ((float1 * float3) + ((1 - float1) * float2));
    if (((arg6.type == 2) && ((boolean) arg6.internalValue))) {
      return org.crayonlang.interpreter.vm.CrayonWrapper.buildInteger(vm.globals, ((int) (float1 + 0.5)));
    }
    return org.crayonlang.interpreter.vm.CrayonWrapper.buildFloat(vm.globals, float1);
  }
}
