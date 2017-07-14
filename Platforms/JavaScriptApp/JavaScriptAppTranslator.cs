using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pastel.Nodes;
using Platform;

namespace JavaScriptApp
{
    public class JavaScriptAppTranslator : LangJavaScript.JavaScriptTranslator
    {
        public JavaScriptAppTranslator(AbstractPlatform platform) : base(platform)
        { }
    }
}
