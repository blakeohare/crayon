using System;
using System.Collections.Generic;

namespace Router
{
    public class RouterService : Wax.WaxService
    {
        public RouterService() : base("router") { }

        public override void HandleRequest(
            Dictionary<string, object> request,
            Func<Dictionary<string, object>, bool> cb)
        {
            bool errorsAsExceptions = request.ContainsKey("exceptionsAsErrors") && (request["errorsAsExceptions"] as bool?) == true;
            string crayonSourceRoot = request.ContainsKey("crayonSourceRoot") ? request["crayonSourceRoot"].ToString() : null;

            if (errorsAsExceptions)
            {
                this.HandleRequestImpl(request, errorsAsExceptions, crayonSourceRoot);
            }
            else
            {
                try
                {
                    this.HandleRequestImpl(request, errorsAsExceptions, crayonSourceRoot);
                }
                catch (InvalidOperationException ioe)
                {
                    ErrorPrinter.ShowErrors(new Wax.Error[] { new Wax.Error { Message = ioe.Message } }, false);
                }
            }

            cb(new Dictionary<string, object>());
        }

        private void HandleRequestImpl(Dictionary<string, object> request, bool errorsAsExceptions, string crayonSourceRoot)
        {
            string[] commandLineArgs = (string[])request["args"];
            Wax.Command command = FlagParser.Parse(commandLineArgs);
            command.ErrorsAsExceptions = errorsAsExceptions;
            command.ActiveCrayonSourceRoot = crayonSourceRoot;
            MainPipeline.Run(command, this.Hub);
        }
    }
}
