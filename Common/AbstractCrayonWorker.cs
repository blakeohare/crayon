namespace Common
{
    public abstract class AbstractCrayonWorker
    {
        public abstract string Name { get; }

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
