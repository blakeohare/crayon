using System.Collections.Generic;

namespace Parser
{
    internal class CompileRequest
    {
        private Dictionary<string, string> codeFiles;

        public CompileRequest(Dictionary<string, object> rawRequest)
        {
            this.ProjectId = (string)rawRequest["projectId"];
            this.DelegateMainTo = (string)rawRequest["delegateMainTo"];
            this.CompilerLocale = Parser.Localization.Locale.Get((string)rawRequest["locale"]);
            this.LocalDeps = (string[])rawRequest["localDeps"];
            this.ProjectDirectory = (string)rawRequest["projectDirectory"];
            this.codeFiles = CommonUtil.Collections.DictionaryUtil.FlattenedDictionaryToDictionary((string[])rawRequest["codeFiles"]);
            this.RootProgrammingLanguage = ((string)rawRequest["lang"]).ToUpper() == "CRAYON" ? Common.ProgrammingLanguage.CRAYON : Common.ProgrammingLanguage.ACRYLIC;
            this.RemoveSymbols = (bool)rawRequest["removeSymbols"];
            this.ErrorsAsExceptions = rawRequest.ContainsKey("errorsAsExceptions") ? (bool)rawRequest["errorsAsExceptions"] : false;
            string[] buildVarData = (string[])rawRequest["buildVars"];
            for (int i = 0; i < buildVarData.Length; i += 3)
            {
                string name = buildVarData[i];
                string value = buildVarData[i + 2];
                switch (buildVarData[i + 1])
                {
                    case "B": this.AddCompileTimeBoolean(name, value == "1"); break;
                    case "F": this.AddCompileTimeFloat(name, double.Parse(value)); break;
                    case "I": this.AddCompileTimeInteger(name, int.Parse(value)); break;
                    case "S": this.AddCompileTimeString(name, value + ""); break;
                    default: throw new System.NotImplementedException();
                }
            }
        }

        public bool RemoveSymbols { get; private set; }
        public string ProjectId { get; private set; }
        public string DelegateMainTo { get; private set; }
        public Parser.Localization.Locale CompilerLocale { get; private set; }
        public string[] LocalDeps { get; private set; }
        public string ProjectDirectory { get; private set; }
        public Common.ProgrammingLanguage RootProgrammingLanguage { get; private set; }
        public bool ErrorsAsExceptions { get; private set; }

        public Dictionary<string, string> GetCodeFiles()
        {
            return this.codeFiles;
        }

        private Dictionary<string, object> compileTimeVariableValues = new Dictionary<string, object>();
        private Dictionary<string, int> compileTimeVariableTypes = new Dictionary<string, int>();
        private void AddCompileTimeValue(string name, int type, object value)
        {
            this.compileTimeVariableValues[name] = value;
            this.compileTimeVariableTypes[name] = type;
        }

        public void AddCompileTimeBoolean(string name, bool value) { this.AddCompileTimeValue(name, (int)Types.BOOLEAN, value); }
        public void AddCompileTimeInteger(string name, int value) { this.AddCompileTimeValue(name, (int)Types.INTEGER, value); }
        public void AddCompileTimeFloat(string name, double value) { this.AddCompileTimeValue(name, (int)Types.FLOAT, value); }
        public void AddCompileTimeString(string name, string value) { this.AddCompileTimeValue(name, (int)Types.STRING, value); }

        public bool HasCompileTimeValue(string name) { return this.compileTimeVariableValues.ContainsKey(name); }
        public int GetCompileTimeValueType(string name) { return this.compileTimeVariableTypes[name]; }
        public int GetCompileTimeInt(string name) { return (int)this.compileTimeVariableValues[name]; }
        public bool GetCompileTimeBool(string name) { return (bool)this.compileTimeVariableValues[name]; }
        public string GetCompileTimeString(string name) { return (string)this.compileTimeVariableValues[name]; }
        public double GetCompileTimeFloat(string name) { return (double)this.compileTimeVariableValues[name]; }
    }
}
