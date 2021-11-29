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
            bool errorsAsExceptions = !IS_RELEASE;
            if (errorsAsExceptions)
            {
                this.HandleRequestImpl(request, errorsAsExceptions);
            }
            else
            {
                try
                {
                    this.HandleRequestImpl(request, errorsAsExceptions);
                }
                catch (InvalidOperationException ioe)
                {
                    ErrorPrinter.ShowErrors(new Wax.Error[] { new Wax.Error { Message = ioe.Message } }, false);
                }
            }

            cb(new Dictionary<string, object>());
        }

        private void HandleRequestImpl(Dictionary<string, object> request, bool errorsAsExceptions)
        {
            string[] commandLineArgs = (string[])request["args"];
            Wax.Command command = FlagParser.Parse(commandLineArgs);
            command.ErrorsAsExceptions = errorsAsExceptions;
            Pipeline.MainPipeline.Run(command, this.Hub);
        }
    }
}
