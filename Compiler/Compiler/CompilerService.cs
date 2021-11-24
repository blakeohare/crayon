using System;
using System.Collections.Generic;
using System.Linq;
using Wax;

namespace Parser
{
    public class CompilerService : WaxService
    {
        public CompilerService() : base("compiler") { }

        public override void HandleRequest(Dictionary<string, object> request, Func<Dictionary<string, object>, bool> cb)
        {
            CompileRequest cr = new CompileRequest(request);
            InternalCompilationBundle icb = Compiler.Compile(cr, (bool)request["isRelease"], this.Hub);

            Dictionary<string, object> output = new Dictionary<string, object>();
            List<string> errors = new List<string>();
            if (icb.HasErrors)
            {
                foreach (Error err in icb.Errors)
                {
                    errors.AddRange(new string[] { err.FileName, err.Line + "", err.Column + "", err.Message });
                }
            }
            else
            {
                output["byteCode"] = icb.ByteCode;
                output["depTree"] = Common.AssemblyDependencyUtil.GetDependencyTreeJson(icb.RootScopeDependencyMetadata).Trim();
                output["usesU3"] = icb.AllScopesMetadata.Any(a => a.ID == "U3Direct");
                if (icb.HasErrors)
                {
                    foreach (Error err in icb.Errors)
                    {
                        errors.Add(err.FileName);
                        errors.Add(err.Line + "");
                        errors.Add(err.Column + "");
                        errors.Add(err.Message + "");
                    }
                }
            }
            output["errors"] = errors.ToArray();

            cb(output);
        }
    }
}
