using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public interface IPlatform
    {
        string Name { get; }
        string InheritsFrom { get; }
        Dictionary<string, FileOutput> Export();
    }
}
