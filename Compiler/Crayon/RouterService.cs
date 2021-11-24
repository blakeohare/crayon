using System;
using System.Collections.Generic;

namespace Crayon
{
    public class RouterService : Wax.WaxService
    {
#if DEBUG
        private const bool IS_RELEASE = false;
#else
        private const bool IS_RELEASE = true;
#endif

        public RouterService() : base("router") { }

        public override void HandleRequest(
            Dictionary<string, object> request,
            Func<Dictionary<string, object>, bool> cb)
        {
            string[] commandLineArgs = (string[])request["args"];
            Wax.Command command = FlagParser.Parse(commandLineArgs, IS_RELEASE);
            if (command.HasErrors)
            {
                ErrorPrinter.ShowErrors(command.Errors, !IS_RELEASE);
            }
            else
            {
                Pipeline.MainPipeline.Run(command, IS_RELEASE, this.Hub);
            }

            cb(new Dictionary<string, object>());
        }
    }
}
