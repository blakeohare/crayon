using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GamePythonPygame
{
    public class PythonPygameTranslator : LangPython.PythonTranslator
    {
        public PythonPygameTranslator(Platform.AbstractPlatform platform)
            : base(platform)
        { }
    }
}
