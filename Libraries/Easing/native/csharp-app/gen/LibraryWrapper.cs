using Interpreter.Structs;
using System.Collections.Generic;
using System.Linq;

namespace Interpreter.Libraries.Easing
{
    public static class LibraryWrapper
    {
        private static readonly int[] PST_IntBuffer16 = new int[16];
        private static readonly double[] PST_FloatBuffer16 = new double[16];
        private static readonly string[] PST_StringBuffer16 = new string[16];
        private static readonly System.Random PST_Random = new System.Random();

        public static bool AlwaysTrue() { return true; }
        public static bool AlwaysFalse() { return false; }

        public static string PST_StringReverse(string value)
        {
            if (value.Length < 2) return value;
            char[] chars = value.ToCharArray();
            return new string(chars.Reverse().ToArray());
        }

        private static readonly string[] PST_SplitSep = new string[1];
        private static string[] PST_StringSplit(string value, string sep)
        {
            if (sep.Length == 1) return value.Split(sep[0]);
            if (sep.Length == 0) return value.ToCharArray().Select<char, string>(c => "" + c).ToArray();
            PST_SplitSep[0] = sep;
            return value.Split(PST_SplitSep, System.StringSplitOptions.None);
        }

        private static string PST_FloatToString(double value)
        {
            string output = value.ToString();
            if (output[0] == '.') output = "0" + output;
            if (!output.Contains('.')) output += ".0";
            return output;
        }

        private static readonly System.DateTime PST_UnixEpoch = new System.DateTime(1970, 1, 1);
        private static double PST_CurrentTime
        {
            get { return System.DateTime.UtcNow.Subtract(PST_UnixEpoch).TotalSeconds; }
        }

        private static string PST_Base64ToString(string b64Value)
        {
            byte[] utf8Bytes = System.Convert.FromBase64String(b64Value);
            string value = System.Text.Encoding.UTF8.GetString(utf8Bytes);
            return value;
        }

        // TODO: use a model like parse float to avoid double parsing.
        public static bool PST_IsValidInteger(string value)
        {
            if (value.Length == 0) return false;
            char c = value[0];
            if (value.Length == 1) return c >= '0' && c <= '9';
            int length = value.Length;
            for (int i = c == '-' ? 1 : 0; i < length; ++i)
            {
                c = value[i];
                if (c < '0' || c > '9') return false;
            }
            return true;
        }

        public static void PST_ParseFloat(string strValue, double[] output)
        {
            double num = 0.0;
            output[0] = double.TryParse(strValue, out num) ? 1 : -1;
            output[1] = num;
        }

        private static List<T> PST_ListConcat<T>(List<T> a, List<T> b)
        {
            List<T> output = new List<T>(a.Count + b.Count);
            output.AddRange(a);
            output.AddRange(b);
            return output;
        }

        private static List<Value> PST_MultiplyList(List<Value> items, int times)
        {
            List<Value> output = new List<Value>(items.Count * times);
            while (times-- > 0) output.AddRange(items);
            return output;
        }

        private static bool PST_SubstringIsEqualTo(string haystack, int index, string needle)
        {
            int needleLength = needle.Length;
            if (index + needleLength > haystack.Length) return false;
            if (needleLength == 0) return true;
            if (haystack[index] != needle[0]) return false;
            if (needleLength == 1) return true;
            for (int i = 1; i < needleLength; ++i)
            {
                if (needle[i] != haystack[index + i]) return false;
            }
            return true;
        }

        private static void PST_ShuffleInPlace<T>(List<T> list)
        {
            if (list.Count < 2) return;
            int length = list.Count;
            int tIndex;
            T tValue;
            for (int i = length - 1; i >= 0; --i)
            {
                tIndex = PST_Random.Next(length);
                tValue = list[tIndex];
                list[tIndex] = list[i];
                list[i] = tValue;
            }
        }

        public static Value lib_easing_apply_pts(VmContext vm, Value[] args)
        {
            ListImpl sampleValues = (ListImpl)args[1].internalValue;
            int _len = sampleValues.size;
            double[] samples = new double[_len];
            int i = 0;
            while ((i < _len))
            {
                samples[i] = (double)sampleValues.array[i].internalValue;
                i += 1;
            }
            samples[0] = 0.0;
            samples[(_len - 1)] = 1.0;
            ObjectInstance o = (ObjectInstance)args[0].internalValue;
            o.nativeObject = new EasingSampling(_len, samples);
            return vm.globals.valueNull;
        }

        public static Value lib_easing_interpolate(VmContext vm, Value[] args)
        {
            Value arg2 = args[1];
            Value arg3 = args[2];
            Value arg4 = args[3];
            Value arg5 = args[4];
            Value arg6 = args[5];
            ObjectInstance o = (ObjectInstance)args[0].internalValue;
            EasingSampling es = (EasingSampling)o.nativeObject;
            double[] samples = es.samples;
            int _len = es.sampleCount;
            int int1 = (int)args[6].internalValue;
            double float1 = 0.0;
            double float2 = 0.0;
            double float3 = 0.0;
            if ((arg4.type == 3))
            {
                float1 = (0.0 + (int)arg4.internalValue);
            }
            else
            {
                if ((arg4.type == 4))
                {
                    float1 = (double)arg4.internalValue;
                }
                else
                {
                    return vm.globals.valueNull;
                }
            }
            if ((arg5.type == 3))
            {
                float2 = (0.0 + (int)arg5.internalValue);
            }
            else
            {
                if ((arg5.type == 4))
                {
                    float2 = (double)arg5.internalValue;
                }
                else
                {
                    return vm.globals.valueNull;
                }
            }
            bool bool1 = false;
            bool bool2 = false;
            bool first = false;
            if ((int1 == 2))
            {
                first = true;
                if (((float1 * 2.0) > float2))
                {
                    float1 = ((float2 - float1) * 2);
                    bool1 = true;
                    bool2 = true;
                }
                else
                {
                    float1 *= 2.0;
                }
            }
            else
            {
                if ((int1 == 1))
                {
                    float1 = (float2 - float1);
                    bool1 = true;
                }
            }
            if ((float2 == 0))
            {
                float1 = samples[0];
            }
            else
            {
                if ((float2 < 0))
                {
                    float2 = -float2;
                    float1 = -float1;
                }
                if ((float1 >= float2))
                {
                    float1 = samples[(_len - 1)];
                }
                else
                {
                    if ((float1 < 0))
                    {
                        float1 = samples[0];
                    }
                    else
                    {
                        float1 = (float1) / (float2);
                        if ((_len > 2))
                        {
                            float2 = (float1 * _len);
                            int index = (int)float2;
                            float2 -= index;
                            float1 = samples[index];
                            if (((index < (_len - 1)) && (float2 > 0)))
                            {
                                float3 = samples[(index + 1)];
                                float1 = ((float1 * (1 - float2)) + (float3 * float2));
                            }
                        }
                    }
                }
            }
            if ((arg2.type == 3))
            {
                float2 = (0.0 + (int)arg2.internalValue);
            }
            else
            {
                if ((arg2.type == 4))
                {
                    float2 = (double)arg2.internalValue;
                }
                else
                {
                    return vm.globals.valueNull;
                }
            }
            if ((arg3.type == 3))
            {
                float3 = (0.0 + (int)arg3.internalValue);
            }
            else
            {
                if ((arg3.type == 4))
                {
                    float3 = (double)arg3.internalValue;
                }
                else
                {
                    return vm.globals.valueNull;
                }
            }
            if (bool1)
            {
                float1 = (1.0 - float1);
            }
            if (first)
            {
                float1 *= 0.5;
            }
            if (bool2)
            {
                float1 += 0.5;
            }
            float1 = ((float1 * float3) + ((1 - float1) * float2));
            if (((arg6.type == 2) && (bool)arg6.internalValue))
            {
                return Interpreter.Vm.CrayonWrapper.buildInteger(vm.globals, (int)(float1 + 0.5));
            }
            return Interpreter.Vm.CrayonWrapper.buildFloat(vm.globals, float1);
        }
    }
}
