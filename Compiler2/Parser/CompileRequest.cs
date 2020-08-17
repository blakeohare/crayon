using System.Collections.Generic;

namespace Parser
{
    public class CompileRequest
    {
        public Build.BuildContext BuildContext { get; set; }
        public string ProjectId { get { return this.BuildContext.ProjectID; } }
        public Build.AssemblyContext TopLevelAssembly { get { return this.BuildContext.TopLevelAssembly; } }
        public string DelegateMainTo { get { return this.BuildContext.DelegateMainTo; } }
        public Localization.Locale CompilerLocale { get { return this.BuildContext.CompilerLocale; } }
        public string[] LocalDeps { get { return this.BuildContext.LocalDeps; } }
        public string[] RemoteDeps { get { return this.BuildContext.RemoteDeps; } }
        public string ProjectDirectory { get { return this.BuildContext.ProjectDirectory; } }

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
