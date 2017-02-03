using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common
{
    public interface IPlatformProvider
    {
        AbstractPlatform GetPlatform(string name);
    }
}
