using Common;

namespace Exporter.Workers
{
    public class EncodeByteCodeWorker : AbstractCrayonWorker
    {
        public override CrayonWorkerResult DoWorkImpl(CrayonWorkerResult[] args)
        {
            // byteCode = EncodeByteCode(compilationResult)
            CompilationBundle compilationResult = (CompilationBundle)args[0].Value;
            string byteCode = ByteCodeEncoder.Encode(compilationResult.ByteCode);
            return new CrayonWorkerResult() { Value = byteCode };
        }
    }
}
