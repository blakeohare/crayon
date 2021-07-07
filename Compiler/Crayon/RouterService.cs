using System;
using System.Collections.Generic;
using Common;

namespace Crayon
{
    public class RouterService : CommonUtil.Wax.WaxService
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
            Command command = FlagParser.Parse(commandLineArgs, IS_RELEASE);
            if (command.HasErrors)
            {
                ErrorPrinter.ShowErrors(command.Errors);
            }
            else
            {
                using (new PerformanceSection("Crayon"))
                {
                    Pipeline.MainPipeline.Run(command, IS_RELEASE, this.Hub);
                }
            }

#if DEBUG
            if (command.ShowPerformanceMarkers)
            {
                ConsoleWriter.Print(Common.ConsoleMessageType.PERFORMANCE_METRIC, Common.PerformanceTimer.GetSummary());
            }
#endif
        }
    }
}
