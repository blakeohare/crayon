using System.Collections.Generic;

namespace Builder
{
    internal class CompileRequest
    {
        private Dictionary<string, string> codeFiles = null;
        private BuildContext buildContext;

        public CompileRequest(BuildContext buildContext, string sourceRoot)
        {
            this.buildContext = buildContext;
            this.ProjectId = buildContext.ProjectID;
            this.DelegateMainTo = buildContext.DelegateMainTo;
            this.CompilerLocale = Builder.Localization.Locale.Get(buildContext.CompilerLocale.ID);
            this.LocalDeps = buildContext.LocalDeps;
            this.ProjectDirectory = buildContext.ProjectDirectory;
            this.RootProgrammingLanguage = buildContext.RootProgrammingLanguage;
            this.RemoveSymbols = buildContext.RemoveSymbols;
            this.ActiveCrayonSourceRoot = sourceRoot;

            foreach (string key in buildContext.BuildVariableLookup.Keys)
            {
                Builder.BuildVar buildVar = buildContext.BuildVariableLookup[key];
                switch (buildVar.Type)
                {
                    case Builder.VarType.BOOLEAN: this.AddCompileTimeBoolean(key, buildVar.BoolValue); break;
                    case Builder.VarType.FLOAT: this.AddCompileTimeFloat(key, buildVar.FloatValue); break;
                    case Builder.VarType.INT: this.AddCompileTimeInteger(key, buildVar.IntValue); break;
                    case Builder.VarType.STRING: this.AddCompileTimeString(key, buildVar.StringValue); break;
                    case Builder.VarType.NULL: throw new System.InvalidOperationException("The build variable '" + key + "' does not have a value assigned to it.");
                    default: throw new System.Exception(); // this should not happen.
                }
            }
        }

        public bool RemoveSymbols { get; private set; }
        public string ProjectId { get; private set; }
        public string DelegateMainTo { get; private set; }
        public Builder.Localization.Locale CompilerLocale { get; private set; }
        public string[] LocalDeps { get; private set; }
        public string ProjectDirectory { get; private set; }
        public Builder.ProgrammingLanguage RootProgrammingLanguage { get; private set; }
        public string ActiveCrayonSourceRoot { get; private set; }

        public async System.Threading.Tasks.Task<Dictionary<string, string>> GetCodeFiles()
        {
            if (this.codeFiles == null)
            {
                this.codeFiles = await buildContext.GetCodeFiles();
            }
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
