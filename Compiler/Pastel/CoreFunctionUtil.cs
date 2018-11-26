using System;
using System.Collections.Generic;
using System.Linq;
using Pastel.Nodes;

namespace Pastel
{
    internal static class CoreFunctionUtil
    {
        private static Dictionary<CoreFunction, PType> returnTypes;
        private static Dictionary<CoreFunction, PType[]> argTypes;
        private static Dictionary<CoreFunction, bool[]> argTypesRepeated;

        public static PType[] GetCoreFunctionArgTypes(CoreFunction functionId)
        {
            if (returnTypes == null)
            {
                CoreFunctionUtil.Init();
            }
            return argTypes[functionId].ToArray();
        }

        public static PType GetCoreFunctionReturnType(CoreFunction functionId)
        {
            if (returnTypes == null)
            {
                CoreFunctionUtil.Init();
            }

            return returnTypes[functionId];
        }

        public static bool[] GetCoreFunctionIsArgTypeRepeated(CoreFunction functionId)
        {
            if (returnTypes == null)
            {
                CoreFunctionUtil.Init();
            }

            return argTypesRepeated[functionId];
        }

        private static void Init()
        {
            Dictionary<string, CoreFunction> lookup = new Dictionary<string, CoreFunction>();
            foreach (CoreFunction func in typeof(CoreFunction).GetEnumValues().Cast<CoreFunction>())
            {
                lookup[func.ToString()] = func;
            }

            returnTypes = new Dictionary<CoreFunction, PType>();
            argTypes = new Dictionary<CoreFunction, PType[]>();
            argTypesRepeated = new Dictionary<CoreFunction, bool[]>();

            string[] rows = GetCoreFunctionSignatureManifest().Split('\n');
            foreach (string row in rows)
            {
                string definition = row.Trim();
                if (definition.Length > 0)
                {
                    TokenStream tokens = new TokenStream(Tokenizer.Tokenize("core function manifest", row));
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

                    CoreFunction func = lookup[name];
                    returnTypes[func] = returnType;
                    argTypes[func] = argList.ToArray();
                    argTypesRepeated[func] = argRepeated.ToArray();
                }
            }
        }

        private static string GetCoreFunctionSignatureManifest()
        {
            return PastelUtil.ReadAssemblyFileText(typeof(CoreFunctionUtil).Assembly, "CoreFunctionSignatures.txt");
        }
    }
}
