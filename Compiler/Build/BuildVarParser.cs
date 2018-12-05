using Build.BuildParseNodes;
using Common;
using System;
using System.Collections.Generic;

namespace Build
{
    class BuildVarParser
    {
        internal static Dictionary<string, BuildVarCanonicalized> GenerateBuildVars(
            BuildItem root,
            BuildItem target,
            Dictionary<string, string> replacements)
        {
            Dictionary<string, BuildVar> firstPass = new Dictionary<string, BuildVar>();

            if (root.Var != null)
            {
                foreach (BuildVar rootVar in root.Var)
                {
                    if (rootVar.Id == null)
                    {
                        throw new InvalidOperationException("Build file contains a <var> without an id attribute.");
                    }
                    firstPass.Add(rootVar.Id, rootVar);
                }
            }

            if (target.Var != null)
            {
                foreach (BuildVar targetVar in target.Var)
                {
                    if (targetVar.Id == null)
                    {
                        throw new InvalidOperationException("Build file target contains a <var> without an id attribute.");
                    }
                    firstPass[targetVar.Id] = targetVar;
                }
            }

            Dictionary<string, BuildVarCanonicalized> output = new Dictionary<string, BuildVarCanonicalized>();

            foreach (BuildVar rawElement in firstPass.Values)
            {
                string id = rawElement.Id;
                object value = rawElement.Value;
                VarType type = rawElement.Type;
                int intValue = type == VarType.INT ? (int)value : 0;
                double floatValue = type == VarType.FLOAT ? (double)value : 0.0;
                bool boolValue = type == VarType.BOOLEAN ? (bool)value : false;
                string strValue = type == VarType.STRING ? (string)value : "";
                if (type == VarType.STRING && strValue.Contains("%"))
                {
                    foreach (string key in replacements.Keys)
                    {
                        strValue = strValue.Replace("%" + key + "%", replacements[key].Trim());
                    }
                }

                output[id] = new BuildVarCanonicalized()
                {
                    ID = id,
                    Type = type,
                    StringValue = strValue,
                    IntValue = intValue,
                    FloatValue = floatValue,
                    BoolValue = boolValue
                };
            }

            return output;
        }
    }
}
