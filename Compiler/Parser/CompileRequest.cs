using System.Collections.Generic;

namespace Parser
{
    public class CompileRequest
    {
        private Dictionary<string, string> codeFiles;

        public CompileRequest(Build.BuildContext buildContext)
        {
            this.ProjectId = buildContext.ProjectID;
            this.DelegateMainTo = buildContext.DelegateMainTo;
            this.CompilerLocale = buildContext.CompilerLocale;
            this.LocalDeps = buildContext.LocalDeps;
            this.ProjectDirectory = buildContext.ProjectDirectory;
            this.codeFiles = buildContext.GetCodeFiles();
            this.RootProgrammingLanguage = buildContext.RootProgrammingLanguage;
            this.RemoveSymbols = buildContext.RemoveSymbols;
        }

        public bool RemoveSymbols { get; private set; }
        public string ProjectId { get; private set; }
        public string DelegateMainTo { get; private set; }
        public Common.Localization.Locale CompilerLocale { get; private set; }
        public string[] LocalDeps { get; private set; }
        public string ProjectDirectory { get; private set; }
        public Common.ProgrammingLanguage RootProgrammingLanguage { get; private set; }

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
