﻿using System;
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
            Wax.Command command = LegacyFlagParser.Parse(commandLineArgs);
            command.ErrorsAsExceptions = errorsAsExceptions;
            MainPipeline.Run(command, this.Hub);
        }
    }
}
