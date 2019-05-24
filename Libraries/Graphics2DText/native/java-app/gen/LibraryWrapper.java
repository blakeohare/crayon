package org.crayonlang.libraries.graphics2dtext;

import java.util.ArrayList;
import java.util.HashMap;
import org.crayonlang.interpreter.PlatformTranslationHelper;
import org.crayonlang.interpreter.structs.*;
import org.crayonlang.interpreter.TranslationHelper;

public final class LibraryWrapper {

  private static final int[] PST_intBuffer16 = new int[16];

  public static Value lib_graphics2dtext_createNativeFont(VmContext vm, Value[] args) {
    Value[] ints = vm.globals.positiveIntegers;
    ObjectInstance nf = ((ObjectInstance) args[0].internalValue);
    Object[] nfOut = nf.nativeData;
    int fontType = ((int) args[1].internalValue);
    String fontPath = "";
    if ((fontType == 0)) {
      fontType = ((int) args[2].internalValue);
    } else {
      fontPath = ((String) args[2].internalValue);
      if ((fontType == 1)) {
        Value res = org.crayonlang.interpreter.vm.CrayonWrapper.resource_manager_getResourceOfType(vm, fontPath, "TTF");
        if ((res.type == 1)) {
          return ints[2];
        }
        ListImpl resList = ((ListImpl) res.internalValue);
        if (!(org.crayonlang.interpreter.vm.CrayonWrapper.getItemFromList(resList, 0).intValue == 1)) {
          return ints[2];
        }
        fontPath = ((String) org.crayonlang.interpreter.vm.CrayonWrapper.getItemFromList(resList, 1).internalValue);
      }
    }
    int fontClass = 0;
    int fontSize = ((int) args[3].internalValue);
    int red = ((int) args[4].internalValue);
    int green = ((int) args[5].internalValue);
    int blue = ((int) args[6].internalValue);
    int styleBitmask = ((int) args[7].internalValue);
    int isBold = (styleBitmask & 1);
    int isItalic = (styleBitmask & 2);
    nfOut[0] = Graphics2DTextHelper.createNativeFont(fontType, fontClass, fontPath, fontSize, (isBold > 0), (isItalic > 0));
    if ((nfOut[0] == null)) {
      if ((fontType == 3)) {
        return ints[1];
      }
      return ints[2];
    }
    return ints[0];
  }

  public static Value lib_graphics2dtext_getNativeFontUniqueKey(VmContext vm, Value[] args) {
    ListImpl list1 = ((ListImpl) args[7].internalValue);
    ArrayList<Value> output = new ArrayList<Value>();
    Graphics2DTextHelper.addAll(output, args[0], args[1], args[2], args[6]);
    ListImpl list2 = ((ListImpl) org.crayonlang.interpreter.vm.CrayonWrapper.buildList(output).internalValue);
    list1.array = list2.array;
    list1.capacity = list2.capacity;
    list1.size = list2.size;
    return vm.globalNull;
  }

  public static Value lib_graphics2dtext_glGenerateAndLoadTexture(VmContext vm, Value[] args) {
    return vm.globalNull;
  }

  public static Value lib_graphics2dtext_glRenderCharTile(VmContext vm, Value[] args) {
    return vm.globalTrue;
  }

  public static Value lib_graphics2dtext_glRenderTextSurface(VmContext vm, Value[] args) {
    return vm.globalNull;
  }

  public static Value lib_graphics2dtext_glSetNativeDataIntArray(VmContext vm, Value[] args) {
    return vm.globalNull;
  }

  public static Value lib_graphics2dtext_isDynamicFontLoaded(VmContext vm, Value[] args) {
    return vm.globalTrue;
  }

  public static Value lib_graphics2dtext_isGlRenderer(VmContext vm, Value[] args) {
    return vm.globalFalse;
  }

  public static Value lib_graphics2dtext_isResourceAvailable(VmContext vm, Value[] args) {
    String path = ((String) args[0].internalValue);
    Value res = org.crayonlang.interpreter.vm.CrayonWrapper.resource_manager_getResourceOfType(vm, path, "TTF");
    if ((res.type == 1)) {
      return vm.globalFalse;
    }
    ListImpl resList = ((ListImpl) res.internalValue);
    if (!(org.crayonlang.interpreter.vm.CrayonWrapper.getItemFromList(resList, 0).intValue == 1)) {
      return vm.globalFalse;
    }
    return vm.globalTrue;
  }

  public static Value lib_graphics2dtext_isSystemFontPresent(VmContext vm, Value[] args) {
    return org.crayonlang.interpreter.vm.CrayonWrapper.buildBoolean(vm.globals, Graphics2DTextHelper.isSystemFontAvailable(((String) args[0].internalValue)));
  }

  public static Value lib_graphics2dtext_renderText(VmContext vm, Value[] args) {
    ListImpl sizeOut = ((ListImpl) args[0].internalValue);
    ObjectInstance textSurface = ((ObjectInstance) args[1].internalValue);
    Object[] imageOut = textSurface.nativeData;
    Object nativeFont = (((ObjectInstance) args[2].internalValue)).nativeData[0];
    int sourceType = ((int) args[3].internalValue);
    int fontClass = 0;
    String fontPath = "";
    if ((sourceType == 0)) {
      fontClass = ((int) args[4].internalValue);
    } else {
      fontPath = ((String) args[4].internalValue);
    }
    int fontSize = ((int) args[5].internalValue);
    int fontStyle = ((int) args[6].internalValue);
    int isBold = (fontStyle & 1);
    int isItalic = (fontStyle & 2);
    int red = ((int) args[7].internalValue);
    int green = ((int) args[8].internalValue);
    int blue = ((int) args[9].internalValue);
    String text = ((String) args[10].internalValue);
    Object bmp = Graphics2DTextHelper.renderText(PST_intBuffer16, nativeFont, red, green, blue, text);
    Object[] spoofedNativeData = new Object[4];
    spoofedNativeData[3] = bmp;
    Object[] spoofedNativeData2 = new Object[1];
    spoofedNativeData2[0] = spoofedNativeData;
    imageOut[0] = spoofedNativeData2;
    org.crayonlang.interpreter.vm.CrayonWrapper.setItemInList(sizeOut, 0, org.crayonlang.interpreter.vm.CrayonWrapper.buildInteger(vm.globals, PST_intBuffer16[0]));
    org.crayonlang.interpreter.vm.CrayonWrapper.setItemInList(sizeOut, 1, org.crayonlang.interpreter.vm.CrayonWrapper.buildInteger(vm.globals, PST_intBuffer16[1]));
    return vm.globalNull;
  }

  public static Value lib_graphics2dtext_simpleBlit(VmContext vm, Value[] args) {
    Object nativeBlittableBitmap = (((ObjectInstance) args[0].internalValue)).nativeData[0];
    Object[] drawQueueNativeData = (((ObjectInstance) args[1].internalValue)).nativeData;
    int alpha = ((int) args[4].internalValue);
    int[] eventQueue = ((int[]) drawQueueNativeData[0]);
    int index = (((int) drawQueueNativeData[1]) - 16);
    Object[] imageQueue = ((Object[]) drawQueueNativeData[2]);
    int imageQueueLength = ((int) drawQueueNativeData[3]);
    eventQueue[index] = 6;
    eventQueue[(index | 1)] = 0;
    eventQueue[(index | 8)] = ((int) args[2].internalValue);
    eventQueue[(index | 9)] = ((int) args[3].internalValue);
    if ((imageQueue.length == imageQueueLength)) {
      int oldSize = imageQueue.length;
      int newSize = (oldSize * 2);
      Object[] newImageQueue = new Object[newSize];
      int i = 0;
      while ((i < oldSize)) {
        newImageQueue[i] = imageQueue[i];
        i += 1;
      }
      imageQueue = newImageQueue;
      drawQueueNativeData[2] = imageQueue;
    }
    imageQueue[imageQueueLength] = nativeBlittableBitmap;
    drawQueueNativeData[3] = (imageQueueLength + 1);
    return vm.globalNull;
  }
}
