using System.Collections.Generic;

namespace Common
{
    public abstract class AbstractPlatform
    {
        public IPlatformProvider PlatformProvider { get; set; }
        
        public abstract string Name { get; }
        public abstract string InheritsFrom { get; }
        public abstract IDictionary<string, bool> GetConstantFlags();
        public abstract Dictionary<string, FileOutput> Export(
            Dictionary<string, object[]> executablesPerCompilationUnit,
            object[] structDefinitions);

        private Dictionary<string, bool> flattenedCached = null;
        public Dictionary<string, bool> GetFlattenedConstantFlags()
        {
            if (this.flattenedCached == null)
            {
                this.flattenedCached = this.InheritsFrom != null
                    ? new Dictionary<string, bool>(this.PlatformProvider.GetPlatform(this.InheritsFrom).GetFlattenedConstantFlags())
                    : new Dictionary<string, bool>();

                IDictionary<string, bool> thisPlatform = this.GetConstantFlags();
                foreach (string key in thisPlatform.Keys)
                {
                    this.flattenedCached[key] = thisPlatform[key];
                }
            }

            return new Dictionary<string, bool>(this.flattenedCached);
        }
    }
}
