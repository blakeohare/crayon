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
            bool isRelease = IS_RELEASE;
            if (isRelease)
            {
                try
                {
                    this.HandleRequestImpl(request);
                }
                catch (InvalidOperationException ioe)
                {
                    ErrorPrinter.ShowErrors(new Wax.Error[] { new Wax.Error { Message = ioe.Message } }, false);
                }
            }
            else
            {
                this.HandleRequestImpl(request);
            }

            cb(new Dictionary<string, object>());
        }

        private void HandleRequestImpl(Dictionary<string, object> request)
        {
            string[] commandLineArgs = (string[])request["args"];
            Wax.Command command = FlagParser.Parse(commandLineArgs);
            Pipeline.MainPipeline.Run(command, IS_RELEASE, this.Hub);
        }
    }
}
