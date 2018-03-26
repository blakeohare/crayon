using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crayon
{
    internal class TopLevelCheckWorker : Common.AbstractCrayonWorker
    {
        public override string Name { get { return "Crayon.TopLevelCheck"; } }

        // TODO: split this up in flag parsing and generating ExportCommand
        public override object DoWork(object arg)
        {
            string[] args = (string[])arg;

            return FlagParser.Parse(args);
        }
    }
}
