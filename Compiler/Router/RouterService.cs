using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Router
{
    public class RouterService : Wax.WaxService
    {
        public RouterService() : base("router") { }

        public override async Task<Dictionary<string, object>> HandleRequest(Dictionary<string, object> request)
        {
            Wax.ToolchainCommand command = new Wax.ToolchainCommand(request);
            if (this.Hub.ErrorsAsExceptions)
            {
                return await this.HandleRequestImpl(command);
            }
            else
            {
                try
                {
                    return await this.HandleRequestImpl(command);
                }
                catch (InvalidOperationException ioe)
                {
                    return new Dictionary<string, object>() {
                        { "errors", new Wax.Error[] { new Wax.Error { Message = ioe.Message } } }
                    };
                }
            }
        }

        private async Task<Dictionary<string, object>> HandleRequestImpl(Wax.ToolchainCommand command)
        {
            Wax.Error[] errors = await MainPipeline.Run(command, this.Hub) ?? new Wax.Error[0];
            return new Dictionary<string, object>() { { "errors", errors } };
        }
    }
}
