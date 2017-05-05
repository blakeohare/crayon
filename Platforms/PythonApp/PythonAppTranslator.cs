using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PythonApp
{
    public class PythonAppTranslator : LangPython.PythonTranslator
    {
        public PythonAppTranslator(Platform.AbstractPlatform platform)
            : base(platform)
        { }
    }
}
