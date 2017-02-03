using System;
using System.Collections.Generic;
using System.Linq;
using Pastel.Nodes;

namespace Pastel
{
    internal static class NativeFunctionUtil
    {
        private static Dictionary<NativeFunction, PType> returnTypes;
        private static Dictionary<NativeFunction, PType[]> argTypes;
        private static Dictionary<NativeFunction, bool[]> argTypesRepeated;
        private static Dictionary<NativeFunction, bool> usesTemplates;

        public static PType[] GetNativeFunctionArgTypes(NativeFunction functionId)
        {
            if (returnTypes == null)
            {
                NativeFunctionUtil.Init();
            }
            return argTypes[functionId].ToArray();
        }

        public static PType GetNativeFunctionReturnType(NativeFunction functionId)
        {
            if (returnTypes == null)
            {
                NativeFunctionUtil.Init();
            }

            return returnTypes[functionId];
        }

        public static bool[] GetNativeFunctionIsArgTypeRepeated(NativeFunction functionId)
        {
            if (returnTypes == null)
            {
                NativeFunctionUtil.Init();
            }

            return argTypesRepeated[functionId];
        }

        private static void Init()
        {
            Dictionary<string, NativeFunction> lookup = new Dictionary<string, NativeFunction>();
            foreach (NativeFunction func in typeof(NativeFunction).GetEnumValues().Cast<NativeFunction>())
            {
                lookup[func.ToString()] = func;
            }

            returnTypes = new Dictionary<NativeFunction, PType>();
            argTypes = new Dictionary<NativeFunction, PType[]>();
            argTypesRepeated = new Dictionary<NativeFunction, bool[]>();

            string[] rows = GetNativeFunctionSignatureManifest().Split('\n');
            foreach (string row in rows)
            {
                string definition = row.Trim();
                if (definition.Length > 0)
                {
                    TokenStream tokens = new TokenStream(Tokenizer.Tokenize("native function manifest", row));
                    PType returnType = PType.Parse(tokens);
                    string name = tokens.Pop().Value;
                    tokens.PopExpected("(");
                    List<PType> argList = new List<PType>();
                    List<bool> argRepeated = new List<bool>();
                    while (!tokens.PopIfPresent(")"))
                    {
                        if (argList.Count > 0) tokens.PopExpected(",");
                        argList.Add(PType.Parse(tokens));
                        if (tokens.PopIfPresent("."))
                        {
                            argRepeated.Add(true);
                            tokens.PopExpected(".");
                            tokens.PopExpected(".");
                        }
                        else
                        {
                            argRepeated.Add(false);
                        }
                    }

                    if (tokens.HasMore)
                    {
                        throw new Exception("Invalid entry in the manifest. Stuff at the end: " + row);
                    }

                    NativeFunction func = lookup[name];
                    returnTypes[func] = returnType;
                    argTypes[func] = argList.ToArray();
                    argTypesRepeated[func] = argRepeated.ToArray();
                }
            }
        }

        private static string GetNativeFunctionSignatureManifest()
        {
            return Common.Util.ReadAssemblyFileText(typeof(NativeFunctionUtil).Assembly, "NativeFunctionSignatures.txt");
        }
    }
}
