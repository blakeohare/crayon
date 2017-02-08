using System;
using System.Collections.Generic;
using Common;
using Pastel;

namespace Platform
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

        private bool parentPlatformSet = false;
        private AbstractPlatform parentPlatform = null;
        public AbstractPlatform ParentPlatform
        {
            get
            {
                if (!this.parentPlatformSet)
                {
                    this.parentPlatformSet = true;
                    this.parentPlatform = this.PlatformProvider.GetPlatform(this.InheritsFrom);
                }
                return this.parentPlatform;
            }
        }

        public virtual string TranslateType(Pastel.Nodes.PType type)
        {
            if (this.parentPlatformSet)
            {
                return this.parentPlatform.TranslateType(type);
            }
            throw new InvalidOperationException("This platform does not support types.");
        }

        public abstract Dictionary<string, FileOutput> ExportProject(
            IList<Pastel.Nodes.VariableDeclaration> globals,
            IList<Pastel.Nodes.StructDefinition> structDefinitions,
            IList<Pastel.Nodes.FunctionDefinition> functionDefinitions,
            Dictionary<ExportOptionKey, object> options);
    }
}
