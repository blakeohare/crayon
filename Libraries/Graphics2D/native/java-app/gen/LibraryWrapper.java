package org.crayonlang.libraries.graphics2d;

import java.util.ArrayList;
import java.util.HashMap;
import org.crayonlang.interpreter.PlatformTranslationHelper;
import org.crayonlang.interpreter.structs.*;
import org.crayonlang.interpreter.TranslationHelper;

public final class LibraryWrapper {

  public static Value lib_graphics2d_addImageRenderEvent(VmContext vm, Value[] args) {
    int i = 0;
    // get the drawing queue data;
    Object[] drawQueueData = (((ObjectInstance) args[0].internalValue)).nativeData;
    // expand the draw event queue;
    int[] eventQueue = ((int[]) drawQueueData[0]);
    int queueLength = ((int) drawQueueData[1]);
    if ((queueLength >= eventQueue.length)) {
      eventQueue = lib_graphics2d_expandEventQueueCapacity(eventQueue);
      drawQueueData[0] = eventQueue;
    }
    drawQueueData[1] = (queueLength + 16);
    // expand (or create) the image native data queue;
    Object[][] imageNativeDataQueue = ((Object[][]) drawQueueData[2]);
    int imageNativeDataQueueSize = 0;
    if ((imageNativeDataQueue == null)) {
      imageNativeDataQueue = new Object[16][];
    } else {
      imageNativeDataQueueSize = ((int) drawQueueData[3]);
    }
    if ((imageNativeDataQueueSize >= imageNativeDataQueue.length)) {
      Object[][] objArrayArray2 = new Object[((imageNativeDataQueueSize * 2) + 16)][];
      i = 0;
      while ((i < imageNativeDataQueueSize)) {
        objArrayArray2[i] = imageNativeDataQueue[i];
        i += 1;
      }
      imageNativeDataQueue = objArrayArray2;
      drawQueueData[2] = imageNativeDataQueue;
    }
    drawQueueData[3] = (imageNativeDataQueueSize + 1);
    // Add the image to the image native data queue;
    Object[] imageNativeData = (((ObjectInstance) args[1].internalValue)).nativeData;
    imageNativeDataQueue[imageNativeDataQueueSize] = imageNativeData;
    boolean isValid = true;
    boolean isNoop = false;
    // mark event as an Image event (6);
    eventQueue[queueLength] = 6;
    // get/set the draw options mask;
    int flag = ((int) args[2].internalValue);
    eventQueue[(queueLength | 1)] = flag;
    // rotation;
    if (((flag & 4) != 0)) {
      Value rotationValue = args[11];
      double theta = 0.0;
      if ((rotationValue.type == 4)) {
        theta = ((double) rotationValue.internalValue);
      } else if ((rotationValue.type == 3)) {
        theta += ((int) rotationValue.internalValue);
      } else {
        isValid = false;
      }
      eventQueue[(queueLength | 10)] = ((int) (org.crayonlang.interpreter.vm.CrayonWrapper.canonicalizeAngle(theta) * 1048576));
    }
    // alpha;
    if (((flag & 8) != 0)) {
      Value alphaValue = args[12];
      int alpha = 0;
      if ((alphaValue.type == 3)) {
        alpha = ((int) alphaValue.internalValue);
      } else if ((alphaValue.type == 4)) {
        alpha = ((int) (0.5 + ((double) alphaValue.internalValue)));
      } else {
        isValid = false;
      }
      if ((i > 254)) {
        eventQueue[(queueLength | 1)] = (flag - 8);
      } else if ((i < 0)) {
        isNoop = true;
      } else {
        eventQueue[(queueLength | 11)] = alpha;
      }
    }
    // Copy values to event queue;
    Value value = null;
    i = 3;
    while ((i < 11)) {
      value = args[i];
      if ((value.type == 3)) {
        eventQueue[(queueLength + i - 1)] = ((int) value.internalValue);
      } else if ((value.type == 4)) {
        eventQueue[(queueLength + i - 1)] = ((int) (0.5 + ((double) value.internalValue)));
      } else {
        isValid = false;
      }
      i += 1;
    }
    // slicing;
    if (((flag & 1) != 0)) {
      int actualWidth = ((int) imageNativeData[5]);
      int sourceX = eventQueue[(queueLength | 2)];
      int sourceWidth = eventQueue[(queueLength | 4)];
      if (((sourceX < 0) || ((sourceX + sourceWidth) > actualWidth) || (sourceWidth < 0))) {
        isValid = false;
      } else if ((sourceWidth == 0)) {
        isNoop = true;
      }
      int actualHeight = ((int) imageNativeData[6]);
      int sourceY = eventQueue[(queueLength | 3)];
      int sourceHeight = eventQueue[(queueLength | 5)];
      if (((sourceY < 0) || ((sourceY + sourceHeight) > actualHeight) || (sourceHeight < 0))) {
        isValid = false;
      } else if ((sourceHeight == 0)) {
        isNoop = true;
      }
    }
    // stretching;
    if (((flag & 2) != 0)) {
      if ((eventQueue[(queueLength | 6)] <= 0)) {
        if ((eventQueue[(queueLength | 6)] < 0)) {
          isValid = false;
        } else {
          isNoop = true;
        }
      }
      if ((eventQueue[(queueLength | 7)] <= 0)) {
        if ((eventQueue[(queueLength | 7)] < 0)) {
          isValid = false;
        } else {
          isNoop = true;
        }
      }
    }
    // Revert the operation if it is null or a no-op;
    if ((isNoop || !isValid)) {
      drawQueueData[1] = queueLength;
      drawQueueData[3] = imageNativeDataQueueSize;
    }
    if ((isValid || isNoop)) {
      return vm.globalTrue;
    }
    return vm.globalFalse;
  }

  public static int[] lib_graphics2d_expandEventQueueCapacity(int[] a) {
    int _len = a.length;
    int[] output = new int[((_len * 2) + 16)];
    int i = 0;
    while ((i < _len)) {
      output[i] = a[i];
      i += 1;
    }
    return output;
  }

  public static Value lib_graphics2d_flip(VmContext vm, Value[] args) {
    boolean bool1 = false;
    boolean bool2 = false;
    int i = 0;
    Object[] objArray1 = null;
    Object[] objArray2 = null;
    Object object1 = null;
    ObjectInstance objInstance1 = null;
    ObjectInstance objInstance2 = null;
    Value arg1 = args[0];
    Value arg2 = args[1];
    Value arg3 = args[2];
    Value arg4 = args[3];
    Value arg5 = args[4];
    Value arg6 = args[5];
    objInstance1 = ((ObjectInstance) arg1.internalValue);
    objInstance2 = ((ObjectInstance) arg2.internalValue);
    objArray1 = objInstance1.nativeData;
    objArray2 = new Object[7];
    objInstance2.nativeData = objArray2;
    bool1 = ((boolean) arg3.internalValue);
    bool2 = ((boolean) arg4.internalValue);
    i = 6;
    while ((i >= 0)) {
      objArray2[i] = objArray1[i];
      i -= 1;
    }
    objInstance1 = ((ObjectInstance) arg6.internalValue);
    objArray1 = objInstance1.nativeData;
    objInstance2 = ((ObjectInstance) arg2.internalValue);
    objInstance2.nativeData[0] = objArray1;
    object1 = objArray1[3];
    object1 = org.crayonlang.libraries.game.Graphics2DHelper.flipImage(object1, bool1, bool2);
    objArray1[3] = object1;
    return arg2;
  }

  public static Value lib_graphics2d_initializeTexture(VmContext vm, Value[] args) {
    Value arg1 = args[0];
    Value arg2 = args[1];
    Value arg3 = args[2];
    Value arg4 = args[3];
    Value arg5 = args[4];
    ObjectInstance objInstance1 = ((ObjectInstance) arg1.internalValue);
    Object[] objArray1 = new Object[7];
    objInstance1.nativeData = objArray1;
    objInstance1 = ((ObjectInstance) arg2.internalValue);
    objArray1[0] = objInstance1.nativeData;
    ListImpl list1 = ((ListImpl) arg3.internalValue);
    Value value = org.crayonlang.interpreter.vm.CrayonWrapper.getItemFromList(list1, 0);
    double float1 = ((double) value.internalValue);
    value = org.crayonlang.interpreter.vm.CrayonWrapper.getItemFromList(list1, 2);
    double float2 = ((double) value.internalValue);
    objArray1[1] = float1;
    objArray1[3] = float2;
    value = org.crayonlang.interpreter.vm.CrayonWrapper.getItemFromList(list1, 1);
    float1 = ((double) value.internalValue);
    value = org.crayonlang.interpreter.vm.CrayonWrapper.getItemFromList(list1, 3);
    float2 = ((double) value.internalValue);
    objArray1[2] = float1;
    objArray1[4] = float2;
    objArray1[5] = ((int) arg4.internalValue);
    objArray1[6] = ((int) arg5.internalValue);
    return vm.globalNull;
  }

  public static Value lib_graphics2d_initializeTextureResource(VmContext vm, Value[] args) {
    ObjectInstance textureResourceInstance = ((ObjectInstance) args[0].internalValue);
    Object[] textureResourceNativeData = new Object[6];
    textureResourceInstance.nativeData = textureResourceNativeData;
    ObjectInstance nativeImageDataInstance = ((ObjectInstance) args[2].internalValue);
    Object[] nativeImageDataNativeData = nativeImageDataInstance.nativeData;
    if (((boolean) args[1].internalValue)) {
      textureResourceNativeData[0] = false;
      textureResourceNativeData[1] = false;
      textureResourceNativeData[2] = -1;
      textureResourceNativeData[3] = nativeImageDataNativeData[0];
      textureResourceNativeData[4] = nativeImageDataNativeData[1];
      textureResourceNativeData[5] = nativeImageDataNativeData[2];
    } else {
      textureResourceNativeData[0] = false;
      textureResourceNativeData[1] = true;
      textureResourceNativeData[2] = -1;
      textureResourceNativeData[3] = nativeImageDataNativeData[3];
      textureResourceNativeData[4] = nativeImageDataNativeData[4];
      textureResourceNativeData[5] = nativeImageDataNativeData[5];
    }
    return vm.globalNull;
  }

  public static Value lib_graphics2d_isOpenGlBased(VmContext vm, Value[] args) {
    return vm.globalFalse;
  }

  public static Value lib_graphics2d_isPlatformUsingTextureAtlas(VmContext vm, Value[] args) {
    return vm.globalFalse;
  }

  public static Value lib_graphics2d_lineToQuad(VmContext vm, Value[] args) {
    double float1 = 0.0;
    double float2 = 0.0;
    double float3 = 0.0;
    int i = 0;
    int j = 0;
    int int1 = 0;
    int int2 = 0;
    int int3 = 0;
    int int4 = 0;
    int int5 = 0;
    ObjectInstance objInstance1 = ((ObjectInstance) args[0].internalValue);
    Object[] objArray1 = objInstance1.nativeData;
    int[] intArray1 = ((int[]) objArray1[0]);
    int _len = (((int) objArray1[1]) - 16);
    int1 = intArray1[(_len + 1)];
    int2 = intArray1[(_len + 2)];
    int3 = intArray1[(_len + 3)];
    int4 = intArray1[(_len + 4)];
    int5 = intArray1[(_len + 5)];
    float1 = ((0.0 + int4) - int2);
    float2 = ((0.0 + int3) - int1);
    float3 = float1 / float2;
    float1 = int5 / 2.0;
    if ((float1 < 0.5)) {
      float1 = 1.0;
    }
    float2 = float1 / (Math.pow(((float3 * float3) + 1), 0.5));
    float1 = (-float2 * float3);
    i = ((int) ((int1 + float1) + 0.5));
    j = ((int) ((int1 - float1) + 0.5));
    if ((i == j)) {
      j += 1;
    }
    intArray1[(_len + 1)] = i;
    intArray1[(_len + 3)] = j;
    i = ((int) ((int2 + float2) + 0.5));
    j = ((int) ((int2 - float2) + 0.5));
    if ((i == j)) {
      j += 1;
    }
    intArray1[(_len + 2)] = i;
    intArray1[(_len + 4)] = j;
    i = ((int) ((int3 - float1) + 0.5));
    j = ((int) ((int3 + float1) + 0.5));
    if ((i == j)) {
      i += 1;
    }
    intArray1[(_len + 5)] = i;
    intArray1[(_len + 7)] = j;
    i = ((int) ((int4 - float2) + 0.5));
    j = ((int) ((int4 + float2) + 0.5));
    if ((i == j)) {
      i += 1;
    }
    intArray1[(_len + 6)] = i;
    intArray1[(_len + 8)] = j;
    return vm.globalNull;
  }

  public static Value lib_graphics2d_renderQueueAction(VmContext vm, Value[] args) {
    int command = ((int) args[2].internalValue);
    ObjectInstance objInstance1 = ((ObjectInstance) args[0].internalValue);
    Object[] objArray1 = objInstance1.nativeData;
    if ((objArray1 == null)) {
      objArray1 = new Object[5];
      objInstance1.nativeData = objArray1;
    }
    int[] intArray1 = ((int[]) objArray1[0]);
    if ((intArray1 == null)) {
      intArray1 = new int[0];
      objArray1[0] = intArray1;
      objArray1[1] = 0;
      objArray1[2] = new Object[64][];
      objArray1[3] = 0;
      objArray1[4] = new ArrayList<Integer>();
    }
    ArrayList<Integer> intList1 = ((ArrayList<Integer>) objArray1[4]);
    if ((command == 1)) {
      Value charList = args[1];
      if ((charList.type == 6)) {
        Value value = null;
        ArrayList<Value> list1 = ((ArrayList<Value>) charList.internalValue);
        int _len = list1.size();
        int i = 0;
        while ((i < _len)) {
          value = list1.get(i);
          intList1.add(((int) value.internalValue));
          i += 1;
        }
      }
      Object[] renderArgs = new Object[4];
      renderArgs[0] = intArray1;
      renderArgs[1] = objArray1[1];
      renderArgs[2] = objArray1[2];
      renderArgs[3] = intList1;
      int callbackId = org.crayonlang.interpreter.vm.CrayonWrapper.getNamedCallbackId(vm, "Game", "set-render-data");
      org.crayonlang.interpreter.vm.CrayonWrapper.invokeNamedCallback(vm, callbackId, renderArgs);
    } else if ((command == 2)) {
      objArray1[1] = 0;
      objArray1[3] = 0;
      (intList1).clear();
    }
    return vm.globalNull;
  }

  public static Value lib_graphics2d_renderQueueValidateArgs(VmContext vm, Value[] args) {
    ObjectInstance o = ((ObjectInstance) args[0].internalValue);
    Object[] drawQueueRawData = o.nativeData;
    int[] drawEvents = ((int[]) drawQueueRawData[0]);
    int length = ((int) drawQueueRawData[1]);
    int r = 0;
    int g = 0;
    int b = 0;
    int a = 0;
    int i = 0;
    while ((i < length)) {
      switch (drawEvents[i]) {
        case 1:
          r = drawEvents[(i | 5)];
          g = drawEvents[(i | 6)];
          b = drawEvents[(i | 7)];
          a = drawEvents[(i | 8)];
          if ((r > 255)) {
            drawEvents[(i | 5)] = 255;
          } else if ((r < 0)) {
            drawEvents[(i | 5)] = 0;
          }
          if ((g > 255)) {
            drawEvents[(i | 6)] = 255;
          } else if ((g < 0)) {
            drawEvents[(i | 6)] = 0;
          }
          if ((b > 255)) {
            drawEvents[(i | 7)] = 255;
          } else if ((b < 0)) {
            drawEvents[(i | 7)] = 0;
          }
          if ((a > 255)) {
            drawEvents[(i | 8)] = 255;
          } else if ((a < 0)) {
            drawEvents[(i | 8)] = 0;
          }
          break;
        case 2:
          r = drawEvents[(i | 5)];
          g = drawEvents[(i | 6)];
          b = drawEvents[(i | 7)];
          a = drawEvents[(i | 8)];
          if ((r > 255)) {
            drawEvents[(i | 5)] = 255;
          } else if ((r < 0)) {
            drawEvents[(i | 5)] = 0;
          }
          if ((g > 255)) {
            drawEvents[(i | 6)] = 255;
          } else if ((g < 0)) {
            drawEvents[(i | 6)] = 0;
          }
          if ((b > 255)) {
            drawEvents[(i | 7)] = 255;
          } else if ((b < 0)) {
            drawEvents[(i | 7)] = 0;
          }
          if ((a > 255)) {
            drawEvents[(i | 8)] = 255;
          } else if ((a < 0)) {
            drawEvents[(i | 8)] = 0;
          }
          break;
        case 3:
          r = drawEvents[(i | 6)];
          g = drawEvents[(i | 7)];
          b = drawEvents[(i | 8)];
          a = drawEvents[(i | 9)];
          if ((r > 255)) {
            drawEvents[(i | 6)] = 255;
          } else if ((r < 0)) {
            drawEvents[(i | 6)] = 0;
          }
          if ((g > 255)) {
            drawEvents[(i | 7)] = 255;
          } else if ((g < 0)) {
            drawEvents[(i | 7)] = 0;
          }
          if ((b > 255)) {
            drawEvents[(i | 8)] = 255;
          } else if ((b < 0)) {
            drawEvents[(i | 8)] = 0;
          }
          if ((a > 255)) {
            drawEvents[(i | 9)] = 255;
          } else if ((a < 0)) {
            drawEvents[(i | 9)] = 0;
          }
          break;
        case 4:
          r = drawEvents[(i | 7)];
          g = drawEvents[(i | 8)];
          b = drawEvents[(i | 9)];
          a = drawEvents[(i | 10)];
          if ((r > 255)) {
            drawEvents[(i | 7)] = 255;
          } else if ((r < 0)) {
            drawEvents[(i | 7)] = 0;
          }
          if ((g > 255)) {
            drawEvents[(i | 8)] = 255;
          } else if ((g < 0)) {
            drawEvents[(i | 8)] = 0;
          }
          if ((b > 255)) {
            drawEvents[(i | 9)] = 255;
          } else if ((b < 0)) {
            drawEvents[(i | 9)] = 0;
          }
          if ((a > 255)) {
            drawEvents[(i | 10)] = 255;
          } else if ((a < 0)) {
            drawEvents[(i | 10)] = 0;
          }
          break;
        case 5:
          r = drawEvents[(i | 9)];
          g = drawEvents[(i | 10)];
          b = drawEvents[(i | 11)];
          a = drawEvents[(i | 12)];
          if ((r > 255)) {
            drawEvents[(i | 9)] = 255;
          } else if ((r < 0)) {
            drawEvents[(i | 9)] = 0;
          }
          if ((g > 255)) {
            drawEvents[(i | 10)] = 255;
          } else if ((g < 0)) {
            drawEvents[(i | 10)] = 0;
          }
          if ((b > 255)) {
            drawEvents[(i | 11)] = 255;
          } else if ((b < 0)) {
            drawEvents[(i | 11)] = 0;
          }
          if ((a > 255)) {
            drawEvents[(i | 12)] = 255;
          } else if ((a < 0)) {
            drawEvents[(i | 12)] = 0;
          }
          break;
        case 8:
          r = drawEvents[(i | 10)];
          g = drawEvents[(i | 11)];
          b = drawEvents[(i | 12)];
          a = drawEvents[(i | 13)];
          if ((r > 255)) {
            drawEvents[(i | 10)] = 255;
          } else if ((r < 0)) {
            drawEvents[(i | 10)] = 0;
          }
          if ((g > 255)) {
            drawEvents[(i | 11)] = 255;
          } else if ((g < 0)) {
            drawEvents[(i | 11)] = 0;
          }
          if ((b > 255)) {
            drawEvents[(i | 12)] = 255;
          } else if ((b < 0)) {
            drawEvents[(i | 12)] = 0;
          }
          if ((a > 255)) {
            drawEvents[(i | 13)] = 255;
          } else if ((a < 0)) {
            drawEvents[(i | 13)] = 0;
          }
          break;
      }
      i += 16;
    }
    return vm.globalNull;
  }

  public static Value lib_graphics2d_scale(VmContext vm, Value[] args) {
    Object[] objArray1 = null;
    Object[] objArray2 = null;
    ObjectInstance objInstance1 = null;
    ObjectInstance objInstance2 = null;
    Value arg2 = args[1];
    Value arg3 = args[2];
    Value arg4 = args[3];
    Value arg5 = args[4];
    Value arg6 = args[5];
    int int1 = ((int) arg3.internalValue);
    int int2 = ((int) arg4.internalValue);
    objInstance1 = ((ObjectInstance) arg5.internalValue);
    Object object1 = objInstance1.nativeData[3];
    objInstance1 = ((ObjectInstance) arg6.internalValue);
    objArray1 = new Object[6];
    objInstance1.nativeData = objArray1;
    objArray1[0] = false;
    objArray1[1] = true;
    objArray1[2] = 0;
    objArray1[3] = org.crayonlang.libraries.game.Graphics2DHelper.scaleImage(object1, int1, int2);
    objArray1[4] = int1;
    objArray1[5] = int2;
    objInstance2 = ((ObjectInstance) arg2.internalValue);
    objArray1 = new Object[7];
    objInstance2.nativeData = objArray1;
    objInstance2 = ((ObjectInstance) args[0].internalValue);
    objArray2 = objInstance2.nativeData;
    int i = 4;
    while ((i >= 1)) {
      objArray1[i] = objArray2[i];
      i -= 1;
    }
    objArray1[5] = int1;
    objArray1[6] = int2;
    objInstance1 = ((ObjectInstance) arg6.internalValue);
    objArray1[0] = objInstance1.nativeData;
    return args[0];
  }
}
