using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common
{
    // maybe this class should be in Pastel?
    public interface IInlineImportCodeLoader
    {
        string LoadCode(string path);
    }
}
