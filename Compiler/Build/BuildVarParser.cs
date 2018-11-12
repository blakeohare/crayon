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
                string value = rawElement.Value;
                int intValue = 0;
                double floatValue = 0;
                bool boolValue = false;
                VarType type = VarType.BOOLEAN;
                switch ((rawElement.Type ?? "string").ToLowerInvariant())
                {
                    case "int":
                    case "integer":
                        type = VarType.INT;
                        break;
                    case "float":
                    case "double":
                        type = VarType.FLOAT;
                        break;
                    case "bool":
                    case "boolean":
                        type = VarType.BOOLEAN;
                        break;
                    case "string":
                        type = VarType.STRING;
                        break;
                    default:
                        throw new InvalidOperationException("Build file variable '" + id + "' contains an unrecognized type: '" + rawElement.Type + "'. Types must be 'string', 'integer', 'boolean', or 'float'.");
                }

                int score = (rawElement.EnvironmentVarValue != null ? 1 : 0)
                    + (rawElement.Value != null ? 1 : 0);

                if (score != 1)
                {
                    throw new InvalidOperationException("Build file variable '" + id + "' must contain either a <value> or a <env> content element but not both.");
                }

                if (value == null)
                {
                    value = System.Environment.GetEnvironmentVariable(rawElement.EnvironmentVarValue);
                    if (value == null)
                    {
                        throw new InvalidOperationException("Build file varaible '" + id + "' references an environment variable that is not set: '" + rawElement.EnvironmentVarValue + "'");
                    }

                    if (value.Contains("%"))
                    {
                        foreach (string key in replacements.Keys)
                        {
                            value = value.Replace("%" + key + "%", replacements[key].Trim());
                        }
                    }
                }

                switch (type)
                {
                    case VarType.INT:
                        if (!int.TryParse(value, out intValue))
                        {
                            throw new InvalidOperationException("Build file variable: '" + id + "' contains an invalid integer value.");
                        }
                        break;
                    case VarType.FLOAT:
                        if (!Util.ParseDouble(value, out floatValue))
                        {
                            throw new InvalidOperationException("Build file variable: '" + id + "' contains an invalid float value.");
                        }
                        break;
                    case VarType.BOOLEAN:
                        boolValue = Util.StringToBool(value);
                        break;
                    case VarType.STRING:
                        break;

                    default:
                        break;
                }
                output[id] = new BuildVarCanonicalized()
                {
                    ID = id,
                    Type = type,
                    StringValue = value,
                    IntValue = intValue,
                    FloatValue = floatValue,
                    BoolValue = boolValue
                };
            }

            return output;
        }
    }
}
