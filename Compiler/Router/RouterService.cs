using System;
using System.Collections.Generic;
using System.Linq;

namespace Router
{
    public class RouterService : Wax.WaxService
    {
        public RouterService() : base("router") { }

        public override void HandleRequest(
            Dictionary<string, object> request,
            Func<Dictionary<string, object>, bool> cb)
        {
            Wax.ToolchainCommand command = new Wax.ToolchainCommand(request);
            Dictionary<string, object> result;
            if (this.Hub.ErrorsAsExceptions)
            {
                result = this.HandleRequestImpl(command);
            }
            else
            {
                try
                {
                    result = this.HandleRequestImpl(command);
                }
                catch (InvalidOperationException ioe)
                {
                    result = new Dictionary<string, object>() {
                        { "errors", new Wax.Error[] { new Wax.Error { Message = ioe.Message } } }
                    };
                }
            }

            cb(result);
        }

        private Dictionary<string, object> HandleRequestImpl(Wax.ToolchainCommand command)
        {
            Wax.Error[] errors = MainPipeline.Run(command, this.Hub) ?? new Wax.Error[0];
            return new Dictionary<string, object>() { { "errors", errors } };
        }
    }
}
