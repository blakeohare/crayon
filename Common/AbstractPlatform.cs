using System.Collections.Generic;

namespace Common
{
    public abstract class AbstractPlatform
    {
        public IPlatformProvider PlatformProvider { get; set; }
        
        public abstract string Name { get; }
        public abstract string InheritsFrom { get; }
        public abstract IDictionary<string, object> GetConstantFlags();
        public abstract Dictionary<string, FileOutput> Export(
            Dictionary<string, object[]> executablesPerCompilationUnit,
            object[] structDefinitions);

        private Dictionary<string, object> flattenedCached = null;
        public Dictionary<string, object> GetFlattenedConstantFlags()
        {
            if (this.flattenedCached == null)
            {
                this.flattenedCached = this.InheritsFrom != null
                    ? new Dictionary<string, object>(this.PlatformProvider.GetPlatform(this.InheritsFrom).GetFlattenedConstantFlags())
                    : new Dictionary<string, object>();

                IDictionary<string, object> thisPlatform = this.GetConstantFlags();
                foreach (string key in thisPlatform.Keys)
                {
                    this.flattenedCached[key] = thisPlatform[key];
                }
            }

            return new Dictionary<string, object>(this.flattenedCached);
        }
    }
}
