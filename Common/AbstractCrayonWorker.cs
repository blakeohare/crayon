namespace Common
{
    public abstract class AbstractCrayonWorker
    {
        public abstract string Name { get; }
        public abstract object DoWork(object arg);
    }
}
