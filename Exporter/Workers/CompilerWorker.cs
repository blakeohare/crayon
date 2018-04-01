using Build;
using Common;

namespace Exporter.Workers
{
    // TODO: this class is temporary and should eventually be a pipeline in the parser assembly
    public class CompilerWorker : AbstractCrayonWorker
    {
        public override CrayonWorkerResult DoWorkImpl(CrayonWorkerResult[] args)
        {
            // compilationResult = Compiler(buildContext)
            BuildContext buildContext = (BuildContext)args[0].Value;
            CompilationBundle compilationResult = CompilationBundle.Compile(buildContext);
            return new CrayonWorkerResult() { Value = compilationResult };
        }
    }

}
