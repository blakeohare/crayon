namespace Common
{
    public abstract class AbstractCrayonWorker
    {
        public string Name { get; private set; }

        public AbstractCrayonWorker()
        {
            string fullName = this.GetType().FullName;
            string[] parts = fullName.Split('.');
            string workerName = parts[0] + "::" + parts[parts.Length - 1];
            if (workerName.EndsWith("Worker"))
            {
                workerName = workerName.Substring(0, workerName.Length - "Worker".Length);
            }
            this.Name = workerName;
        }

        public CrayonWorkerResult DoWork(CrayonWorkerResult[] args)
        {
            // TODO: implement auto-parallelization in args
            // If any args has a ParallelValue set, split it up by the size of the thread pool (TODO: implement that)
            // and create that many parallel workers

            using (new PerformanceSection(this.Name))
            {
                return DoWorkImpl(args);
            }
        }

        public abstract CrayonWorkerResult DoWorkImpl(CrayonWorkerResult[] args);
    }
}
