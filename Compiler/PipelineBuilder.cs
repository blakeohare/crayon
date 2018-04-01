using Common;
using System.Reflection;

namespace Crayon
{
    internal class PipelineBuilder
    {
        private CrayonPipelineInterpreter pipelineInterpreter;

        public PipelineBuilder()
        {
            this.pipelineInterpreter = new CrayonPipelineInterpreter();
        }

        public PipelineBuilder AddAssembly(Assembly assembly)
        {
            foreach (string manifestItem in assembly.GetManifestResourceNames())
            {
                if (manifestItem.Contains(".Pipeline.") && manifestItem.EndsWith(".txt"))
                {
                    string pipelineName = Util.StringSplit(manifestItem, ".Pipeline.")[1];
                    pipelineName = pipelineName.Substring(0, pipelineName.Length - ".txt".Length);

                    string assemblyName = manifestItem.Split('.')[0];
                    this.pipelineInterpreter.RegisterPipeline(assemblyName + "::" + pipelineName, assembly, "Pipeline/" + pipelineName + ".txt");
                }
            }

            foreach (System.Type type in assembly.GetTypes())
            {
                if (typeof(AbstractCrayonWorker).IsAssignableFrom(type) && !type.IsAbstract)
                {
                    AbstractCrayonWorker worker = (AbstractCrayonWorker)assembly.CreateInstance(type.FullName);
                    this.pipelineInterpreter.RegisterWorker(worker);
                }
            }

            return this;
        }

        public CrayonPipelineInterpreter GetPipeline()
        {
            return this.pipelineInterpreter;
        }
    }
}
