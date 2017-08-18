using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common;

namespace Crayon
{
    internal class InlineImportCodeLoader : IInlineImportCodeLoader
    {
        public string LoadCode(string path)
        {
            return LegacyUtil.ReadInterpreterFileInternally(path);
        }
    }
}
