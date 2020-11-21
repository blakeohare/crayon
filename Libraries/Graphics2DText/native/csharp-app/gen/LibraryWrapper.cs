using Interpreter.Structs;
using System.Collections.Generic;
using System.Linq;

namespace Interpreter.Libraries.Graphics2DText
{
    public static class LibraryWrapper
    {
        private static readonly int[] PST_IntBuffer16 = new int[16];

        private static Dictionary<string, System.Func<object[], object>> PST_ExtCallbacks = new Dictionary<string, System.Func<object[], object>>();

        public static void PST_RegisterExtensibleCallback(string name, System.Func<object[], object> func)
        {
            PST_ExtCallbacks[name] = func;
        }

        public static Value lib_graphics2dtext_createNativeFont(VmContext vm, Value[] args)
        {
            Value[] ints = vm.globals.positiveIntegers;
            ObjectInstance nf = (ObjectInstance)args[0].internalValue;
            object[] nfOut = nf.nativeData;
            int fontType = (int)args[1].internalValue;
            string fontPath = "";
            if ((fontType == 0))
            {
                fontType = (int)args[2].internalValue;
            }
            else
            {
                fontPath = (string)args[2].internalValue;
                if ((fontType == 1))
                {
                    Value res = Interpreter.Vm.CrayonWrapper.resource_manager_getResourceOfType(vm, fontPath, "TTF");
                    if ((res.type == 1))
                    {
                        return ints[2];
                    }
                    ListImpl resList = (ListImpl)res.internalValue;
                    if (!(bool)Interpreter.Vm.CrayonWrapper.getItemFromList(resList, 0).internalValue)
                    {
                        return ints[2];
                    }
                    fontPath = (string)Interpreter.Vm.CrayonWrapper.getItemFromList(resList, 1).internalValue;
                }
            }
            int fontClass = 0;
            int fontSize = (int)args[3].internalValue;
            int red = (int)args[4].internalValue;
            int green = (int)args[5].internalValue;
            int blue = (int)args[6].internalValue;
            int styleBitmask = (int)args[7].internalValue;
            int isBold = (styleBitmask & 1);
            int isItalic = (styleBitmask & 2);
            nfOut[0] = Graphics2DTextHelper.CreateNativeFont(fontType, fontClass, fontPath, fontSize, (isBold > 0), (isItalic > 0));
            if ((nfOut[0] == null))
            {
                if ((fontType == 3))
                {
                    return ints[1];
                }
                return ints[2];
            }
            return ints[0];
        }

        public static Value lib_graphics2dtext_getNativeFontUniqueKey(VmContext vm, Value[] args)
        {
            ListImpl list1 = (ListImpl)args[7].internalValue;
            List<Value> output = new List<Value>();
            output.AddRange(new Value[] {args[0], args[1], args[2], args[6] });
            ListImpl list2 = (ListImpl)Interpreter.Vm.CrayonWrapper.buildList(output).internalValue;
            list1.array = list2.array;
            list1.capacity = list2.capacity;
            list1.size = list2.size;
            return vm.globalNull;
        }

        public static Value lib_graphics2dtext_glGenerateAndLoadTexture(VmContext vm, Value[] args)
        {
            ListImpl xs = (ListImpl)args[0].internalValue;
            ListImpl ys = (ListImpl)args[1].internalValue;
            ListImpl tiles = (ListImpl)args[2].internalValue;
            int tileCount = xs.size;
            object[][] tileNativeDatas = new object[tileCount][];
            int[] coordinates = new int[(tileCount * 4)];
            object[] nativeData = null;
            int i = 0;
            while ((i < tileCount))
            {
                nativeData = ((ObjectInstance)Interpreter.Vm.CrayonWrapper.getItemFromList(tiles, i).internalValue).nativeData;
                tileNativeDatas[i] = nativeData;
                coordinates[(i * 4)] = (int)Interpreter.Vm.CrayonWrapper.getItemFromList(xs, i).internalValue;
                coordinates[((i * 4) + 1)] = (int)Interpreter.Vm.CrayonWrapper.getItemFromList(ys, i).internalValue;
                coordinates[((i * 4) + 2)] = (int)nativeData[1];
                coordinates[((i * 4) + 3)] = (int)nativeData[2];
                i += 1;
            }
            int height = ((int)args[3].internalValue - 1);
            int bitWalker = height;
            while ((bitWalker > 0))
            {
                bitWalker = (bitWalker >> 1);
                height = (height | bitWalker);
            }
            height += 1;
            object textureSheetBitmap = Graphics2DTextHelper.GenerateTextureAndAllocateFloatInfo(tileNativeDatas, coordinates, 1024, height);
            i = 0;
            while ((i < tileCount))
            {
                tileNativeDatas[i][6] = textureSheetBitmap;
                i += 1;
            }
            return vm.globalNull;
        }

        public static Value lib_graphics2dtext_glRenderCharTile(VmContext vm, Value[] args)
        {
            object nativeFont = ((ObjectInstance)args[0].internalValue).nativeData[0];
            object[] tileNativeData = ((ObjectInstance)args[1].internalValue).nativeData;
            int charId = (int)args[2].internalValue;
            ListImpl sizeOut = (ListImpl)args[3].internalValue;
            object bmp = Graphics2DTextHelper.RenderCharTile(nativeFont, charId, PST_IntBuffer16);
            if ((bmp == null))
            {
                return vm.globalFalse;
            }
            int width = PST_IntBuffer16[0];
            int height = PST_IntBuffer16[1];
            int effectiveLeft = PST_IntBuffer16[2];
            int effectiveWidth = PST_IntBuffer16[3];
            tileNativeData[0] = bmp;
            tileNativeData[1] = width;
            tileNativeData[2] = height;
            tileNativeData[3] = effectiveLeft;
            tileNativeData[4] = effectiveWidth;
            Interpreter.Vm.CrayonWrapper.clearList(sizeOut);
            Interpreter.Vm.CrayonWrapper.addToList(sizeOut, Interpreter.Vm.CrayonWrapper.buildInteger(vm.globals, width));
            Interpreter.Vm.CrayonWrapper.addToList(sizeOut, Interpreter.Vm.CrayonWrapper.buildInteger(vm.globals, height));
            Interpreter.Vm.CrayonWrapper.addToList(sizeOut, Interpreter.Vm.CrayonWrapper.buildInteger(vm.globals, effectiveLeft));
            Interpreter.Vm.CrayonWrapper.addToList(sizeOut, Interpreter.Vm.CrayonWrapper.buildInteger(vm.globals, effectiveWidth));
            return vm.globalTrue;
        }

        public static Value lib_graphics2dtext_glRenderTextSurface(VmContext vm, Value[] args)
        {
            object[] textureNativeData = ((ObjectInstance)args[0].internalValue).nativeData;
            int[] xs = (int[])textureNativeData[1];
            int[] ys = (int[])textureNativeData[2];
            int charCount = (int)textureNativeData[4];
            int red = (int)textureNativeData[5];
            int green = (int)textureNativeData[6];
            int blue = (int)textureNativeData[7];
            int leftOffset = (int)args[3].internalValue;
            int topOffset = (int)args[4].internalValue;
            int alpha = (int)args[5].internalValue;
            ListImpl tileValues = (ListImpl)args[1].internalValue;
            object[] tileNativeData = null;
            object[] queueData = ((ObjectInstance)args[2].internalValue).nativeData;
            int[] queue = (int[])queueData[0];
            int queueLength = (int)queueData[1];
            int queueCapacity = queue.Length;
            int requiredCapacity = (queueLength + (charCount * 16));
            if ((requiredCapacity > queueCapacity))
            {
                queue = Interpreter.Vm.CrayonWrapper.reallocIntArray(queue, requiredCapacity);
                queueData[0] = queue;
            }
            queueData[1] = requiredCapacity;
            int index = queueLength;
            int x = 0;
            int y = 0;
            int textureId = 0;
            int i = 0;
            while ((i < charCount))
            {
                tileNativeData = ((ObjectInstance)Interpreter.Vm.CrayonWrapper.getItemFromList(tileValues, i).internalValue).nativeData;
                queue[index] = 8;
                textureId = (int)tileNativeData[5];
                if ((textureId == -1))
                {
                    textureId = Graphics2DTextHelper.LoadOpenGlTexture(tileNativeData[6]);
                    tileNativeData[5] = textureId;
                    tileNativeData[6] = null;
                }
                queue[(index | 1)] = textureId;
                x = (xs[i] + leftOffset);
                y = (ys[i] + topOffset);
                queue[(index | 2)] = x;
                queue[(index | 3)] = y;
                queue[(index | 4)] = (x + (int)tileNativeData[1]);
                queue[(index | 5)] = (y + (int)tileNativeData[2]);
                queue[(index | 6)] = (int)tileNativeData[7];
                queue[(index | 7)] = (int)tileNativeData[8];
                queue[(index | 8)] = (int)tileNativeData[9];
                queue[(index | 9)] = (int)tileNativeData[10];
                queue[(index | 10)] = red;
                queue[(index | 11)] = green;
                queue[(index | 12)] = blue;
                queue[(index | 13)] = alpha;
                queue[(index | 14)] = (int)tileNativeData[11];
                queue[(index | 15)] = (int)tileNativeData[12];
                index += 16;
                i += 1;
            }
            return vm.globalNull;
        }

        public static Value lib_graphics2dtext_glSetNativeDataIntArray(VmContext vm, Value[] args)
        {
            ObjectInstance obj = (ObjectInstance)args[0].internalValue;
            object[] nativeData = obj.nativeData;
            ListImpl values = (ListImpl)args[2].internalValue;
            int length = values.size;
            int[] intArray = new int[length];
            int i = 0;
            while ((i < length))
            {
                intArray[i] = (int)Interpreter.Vm.CrayonWrapper.getItemFromList(values, i).internalValue;
                i += 1;
            }
            nativeData[(int)args[1].internalValue] = intArray;
            return vm.globalNull;
        }

        public static Value lib_graphics2dtext_isDynamicFontLoaded(VmContext vm, Value[] args)
        {
            return vm.globalTrue;
        }

        public static Value lib_graphics2dtext_isGlRenderer(VmContext vm, Value[] args)
        {
            return vm.globalTrue;
        }

        public static Value lib_graphics2dtext_isResourceAvailable(VmContext vm, Value[] args)
        {
            string path = (string)args[0].internalValue;
            Value res = Interpreter.Vm.CrayonWrapper.resource_manager_getResourceOfType(vm, path, "TTF");
            if ((res.type == 1))
            {
                return vm.globalFalse;
            }
            ListImpl resList = (ListImpl)res.internalValue;
            if (!(bool)Interpreter.Vm.CrayonWrapper.getItemFromList(resList, 0).internalValue)
            {
                return vm.globalFalse;
            }
            return vm.globalTrue;
        }

        public static Value lib_graphics2dtext_isSystemFontPresent(VmContext vm, Value[] args)
        {
            return Interpreter.Vm.CrayonWrapper.buildBoolean(vm.globals, Graphics2DTextHelper.IsSystemFontAvailable((string)args[0].internalValue));
        }

        public static Value lib_graphics2dtext_renderText(VmContext vm, Value[] args)
        {
            return vm.globalNull;
        }

        public static Value lib_graphics2dtext_simpleBlit(VmContext vm, Value[] args)
        {
            object nativeBlittableBitmap = ((ObjectInstance)args[0].internalValue).nativeData[0];
            object[] drawQueueNativeData = ((ObjectInstance)args[1].internalValue).nativeData;
            int alpha = (int)args[4].internalValue;
            int[] eventQueue = (int[])drawQueueNativeData[0];
            int index = ((int)drawQueueNativeData[1] - 16);
            object[] imageQueue = (object[])drawQueueNativeData[2];
            int imageQueueLength = (int)drawQueueNativeData[3];
            eventQueue[index] = 6;
            eventQueue[(index | 1)] = 0;
            eventQueue[(index | 8)] = (int)args[2].internalValue;
            eventQueue[(index | 9)] = (int)args[3].internalValue;
            if ((imageQueue.Length == imageQueueLength))
            {
                int oldSize = imageQueue.Length;
                int newSize = (oldSize * 2);
                object[] newImageQueue = new object[newSize];
                int i = 0;
                while ((i < oldSize))
                {
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
}
