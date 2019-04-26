namespace Common
{
    public static class EnvironmentVariableUtil
    {
        public static string GetVariable(string name)
        {
            name = name.Trim();
            if (name.StartsWith("%") && name.EndsWith("%"))
            {
                name = name.Substring(1, name.Length - 2);
            }

            return System.Environment.GetEnvironmentVariable(name);
        }

        public static string DoReplacementsInString(string value)
        {
            if (!value.Contains("%"))
            {
                return value;
            }

            string[] parts = value.Split('%');
            if (parts.Length % 2 == 0) return value;
            for (int i = 1; i < parts.Length; i += 2)
            {
                string envVarName = parts[i];
                string envVarValue = System.Environment.GetEnvironmentVariable(envVarName);
                if (envVarValue == null)
                {
                    parts[i] = "%" + parts[i] + "%";
                }
                else
                {
                    parts[i] = envVarValue;
                }
            }
            return string.Join("", parts);
        }
    }
}
