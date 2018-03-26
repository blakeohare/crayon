using System.Collections.Generic;
using System.Linq;

namespace Common
{
    public class CrayonPipelineInterpreter
    {
        private Dictionary<string, string> registeredPipelines = new Dictionary<string, string>();
        private Dictionary<string, AbstractCrayonWorker> workers = new Dictionary<string, AbstractCrayonWorker>();

        public CrayonPipelineInterpreter RegisterWorker(AbstractCrayonWorker worker)
        {
            workers[worker.Name] = worker;
            return this;
        }

        public CrayonPipelineInterpreter RegisterPipeline(string name, System.Reflection.Assembly assembly, string path)
        {
            return this.RegisterPipeline(name, Util.ReadAssemblyFileText(assembly, path, false));
        }

        public CrayonPipelineInterpreter RegisterPipeline(string name, string contents)
        {
            this.registeredPipelines[name] = contents;
            return this;
        }

        public void Interpret(string name)
        {
            InterpretImpl(name, new CrayonWorkerResult[0]);
        }

        private CrayonWorkerResult InterpretImpl(string name, CrayonWorkerResult[] pipelineArgs)
        {
            Dictionary<string, CrayonWorkerResult> variables = new Dictionary<string, CrayonWorkerResult>();
            string[] lines = this.registeredPipelines[name]
                .Split('\n')
                .Select(line => line.Trim())
                .Where(line => line.Length > 0 && line[0] != '#')
                .ToArray();

            int executionDepth = 0;
            int currentDepth = 0;
            foreach (string line in lines)
            {
                string current = line.Trim();
                if (current == "endif")
                {
                    currentDepth--;
                    if (currentDepth < executionDepth)
                    {
                        executionDepth = currentDepth;
                    }
                }
                else if (current.StartsWith("if "))
                {
                    if (currentDepth <= executionDepth)
                    {
                        string ifCheck = current.Substring(2).Trim();
                        string[] parts = ifCheck.Split('.');
                        string varName = parts[0].Trim();
                        string fieldName = parts[1].Trim();
                        if (variables.ContainsKey(varName) && variables[varName].GetField(fieldName))
                        {
                            executionDepth++;
                        }
                    }
                    currentDepth++;
                }
                else if (currentDepth <= executionDepth)
                {
                    if (current.StartsWith("return "))
                    {
                        string varName = current.Substring("return ".Length);
                        return variables[varName];
                    }
                    else if (current == "return")
                    {
                        return new CrayonWorkerResult();
                    }
                    else
                    {
                        string assignOutputTo = null;
                        string[] parts;
                        if (current.Contains('='))
                        {
                            parts = line.Split('=');
                            assignOutputTo = parts[0].Trim();
                            current = parts[1];
                        }

                        parts = current.Split('(');
                        string workerName = parts[0].Trim();

                        CrayonWorkerResult[] args = parts[1]
                            .TrimEnd(')')
                            .Split(',')
                            .Select(a => a.Trim())
                            .Where(s => s.Length > 0)
                            .Select(vName => variables[vName])
                            .ToArray();
                        CrayonWorkerResult result;
                        if (registeredPipelines.ContainsKey(workerName))
                        {
                            result = InterpretImpl(registeredPipelines[workerName], args);
                        }
                        else
                        {
                            result = workers[workerName].DoWork(args);
                        }

                        if (assignOutputTo != null)
                        {
                            variables[assignOutputTo] = result;
                        }
                    }
                }
            }
            return null;
        }
    }
}
