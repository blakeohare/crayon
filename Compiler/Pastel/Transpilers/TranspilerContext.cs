using Pastel.Nodes;
using System.Collections.Generic;

namespace Pastel.Transpilers
{
    public class TranspilerContext
    {
        private System.Text.StringBuilder buffer = new System.Text.StringBuilder();

        public StringTableBuilder StringTableBuilder { get; set; }

        internal List<PythonFakeSwitchStatement> SwitchStatements { get; private set; }

        public string UniquePrefixForNonCollisions { get; set; }

        // This is a hack for conveying extra information to the top-level function serializer for switch statement stuff.
        // This reference is updated in TranslateFunctionDefinition.
        internal FunctionDefinition PY_HACK_CurrentFunctionDef { get; set; }
        public int SwitchCounter { get; set; }
        private int currentTab = 0;
        public string CurrentTab { get; private set; }
        internal AbstractTranslator Transpiler { get; private set; }
        public Dictionary<string, string> ExtensibleFunctionLookup { get; private set; }

        internal TranspilerContext(Language language, Dictionary<string, string> extensibleFunctions)
        {
            this.ExtensibleFunctionLookup = extensibleFunctions;
            this.Transpiler = LanguageUtil.GetTranspiler(language);
            if (language == Language.PYTHON)
            {
                this.SwitchCounter = 0;
                this.SwitchStatements = new List<PythonFakeSwitchStatement>();
            }
            this.TabDepth = 0;
        }

        public int TabDepth
        {
            get
            {
                return this.currentTab;
            }
            set
            {
                this.currentTab = value;

                while (this.currentTab >= this.Transpiler.Tabs.Length)
                {
                    // Conciseness, not efficiency. Deeply nested stuff is rare.
                    List<string> tabsBuilder = new List<string>(this.Transpiler.Tabs);
                    for (int i = 0; i < 20; ++i)
                    {
                        tabsBuilder.Add(tabsBuilder[tabsBuilder.Count - 1] + this.Transpiler.TabChar);
                    }
                    this.Transpiler.Tabs = tabsBuilder.ToArray();
                }
                this.CurrentTab = this.Transpiler.Tabs[this.currentTab];
            }

        }

        public TranspilerContext Append(char c)
        {
            this.buffer.Append(c);
            return this;
        }

        public TranspilerContext Append(string s)
        {
            this.buffer.Append(s);
            return this;
        }

        public TranspilerContext Append(int v)
        {
            this.buffer.Append(v);
            return this;
        }

        public string FlushAndClearBuffer()
        {
            string value = this.buffer.ToString();
            this.buffer.Clear();
            return value;
        }

        internal FunctionDefinition CurrentFunctionDefinition
        {
            get { return this.PY_HACK_CurrentFunctionDef; }
            set
            {
                this.PY_HACK_CurrentFunctionDef = value;
                this.SwitchCounter = 0;
            }
        }
    }
}
