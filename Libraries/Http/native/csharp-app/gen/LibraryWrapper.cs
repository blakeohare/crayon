using Interpreter.Structs;
using System.Collections.Generic;
using System.Linq;

namespace Interpreter.Libraries.Http
{
    public static class LibraryWrapper
    {
        private static readonly int[] PST_IntBuffer16 = new int[16];

        private static readonly string[] PST_StringBuffer16 = new string[16];

        public static Value lib_http_fastEnsureAllBytes(VmContext vm, Value[] args)
        {
            if ((args[0].type == 6))
            {
                ListImpl list1 = (ListImpl)args[0].internalValue;
                int i = list1.size;
                int int1 = 0;
                int[] intArray1 = new int[i];
                Value value = null;
                while ((i > 0))
                {
                    i -= 1;
                    value = list1.array[i];
                    if ((value.type != 3))
                    {
                        return vm.globalFalse;
                    }
                    int1 = (int)value.internalValue;
                    if ((int1 < 0))
                    {
                        if ((int1 < -128))
                        {
                            return vm.globalFalse;
                        }
                        int1 += 256;
                    }
                    else
                    {
                        if ((int1 >= 256))
                        {
                            return vm.globalFalse;
                        }
                    }
                    intArray1[i] = int1;
                }
                object[] objArray1 = new object[1];
                objArray1[0] = intArray1;
                ObjectInstance objInstance1 = (ObjectInstance)args[1].internalValue;
                objInstance1.nativeData = objArray1;
                return vm.globalTrue;
            }
            return vm.globalFalse;
        }

        public static Value lib_http_getResponseBytes(VmContext vm, Value[] args)
        {
            Value outputListValue = args[1];
            ObjectInstance objInstance1 = (ObjectInstance)args[0].internalValue;
            object[] objArray1 = objInstance1.nativeData;
            List<Value> tList = new List<Value>();
            HttpHelper.GetResponseBytes(objArray1[0], vm.globals.positiveIntegers, tList);
            ListImpl outputList = (ListImpl)outputListValue.internalValue;
            Value t = Interpreter.Vm.CrayonWrapper.buildList(tList);
            ListImpl otherList = (ListImpl)t.internalValue;
            outputList.capacity = otherList.capacity;
            outputList.array = otherList.array;
            outputList.size = tList.Count;
            return outputListValue;
        }

        public static Value lib_http_pollRequest(VmContext vm, Value[] args)
        {
            ObjectInstance objInstance1 = (ObjectInstance)args[0].internalValue;
            object[] objArray1 = objInstance1.nativeData;
            if (HttpHelper.PollRequest(objArray1))
            {
                return vm.globalTrue;
            }
            return vm.globalFalse;
        }

        public static Value lib_http_populateResponse(VmContext vm, Value[] args)
        {
            Value arg2 = args[1];
            Value arg3 = args[2];
            Value arg4 = args[3];
            ObjectInstance objInstance1 = (ObjectInstance)args[0].internalValue;
            object object1 = objInstance1.nativeData[0];
            object[] objArray1 = new object[1];
            List<string> stringList1 = new List<string>();
            HttpHelper.ReadResponseData(object1, PST_IntBuffer16, PST_StringBuffer16, objArray1, stringList1);
            objInstance1 = (ObjectInstance)arg2.internalValue;
            objInstance1.nativeData = objArray1;
            ListImpl outputList = (ListImpl)arg3.internalValue;
            Interpreter.Vm.CrayonWrapper.addToList(outputList, Interpreter.Vm.CrayonWrapper.buildInteger(vm.globals, PST_IntBuffer16[0]));
            Interpreter.Vm.CrayonWrapper.addToList(outputList, Interpreter.Vm.CrayonWrapper.buildString(vm.globals, PST_StringBuffer16[0]));
            Value value = vm.globalNull;
            Value value2 = vm.globalTrue;
            if ((PST_IntBuffer16[1] == 0))
            {
                value = Interpreter.Vm.CrayonWrapper.buildString(vm.globals, PST_StringBuffer16[1]);
                value2 = vm.globalFalse;
            }
            Interpreter.Vm.CrayonWrapper.addToList(outputList, value);
            Interpreter.Vm.CrayonWrapper.addToList(outputList, value2);
            ListImpl list1 = (ListImpl)arg4.internalValue;
            int i = 0;
            while ((i < stringList1.Count))
            {
                Interpreter.Vm.CrayonWrapper.addToList(list1, Interpreter.Vm.CrayonWrapper.buildString(vm.globals, stringList1[i]));
                i += 1;
            }
            return vm.globalNull;
        }

        public static Value lib_http_sendRequest(VmContext vm, Value[] args)
        {
            Value body = args[5];
            ObjectInstance objInstance1 = (ObjectInstance)args[0].internalValue;
            object[] objArray1 = new object[3];
            objInstance1.nativeData = objArray1;
            objArray1[2] = false;
            string method = (string)args[2].internalValue;
            string url = (string)args[3].internalValue;
            List<string> headers = new List<string>();
            ListImpl list1 = (ListImpl)args[4].internalValue;
            int i = 0;
            while ((i < list1.size))
            {
                headers.Add((string)list1.array[i].internalValue);
                i += 1;
            }
            object bodyRawObject = body.internalValue;
            int bodyState = 0;
            if ((body.type == 5))
            {
                bodyState = 1;
            }
            else
            {
                if ((body.type == 8))
                {
                    objInstance1 = (ObjectInstance)bodyRawObject;
                    bodyRawObject = objInstance1.nativeData[0];
                    bodyState = 2;
                }
                else
                {
                    bodyRawObject = null;
                }
            }
            bool getResponseAsText = ((int)args[6].internalValue == 1);
            if ((bool)args[1].internalValue)
            {
                HttpHelper.SendRequestAsync(objArray1, method, url, headers, bodyState, bodyRawObject, getResponseAsText, vm, args[8], ((ObjectInstance)args[9].internalValue).nativeData);
            }
            else
            {
                int execId = (int)args[7].internalValue;
                if (HttpHelper.SendRequestSync(objArray1, method, url, headers, bodyState, bodyRawObject, getResponseAsText))
                {
                    Interpreter.Vm.CrayonWrapper.vm_suspend_context_by_id(vm, execId, 1);
                }
            }
            return vm.globalNull;
        }
    }
}
