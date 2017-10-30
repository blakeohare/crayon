using Build;
using Crayon.ParseTree;
using Localization;
using System.Collections.Generic;

namespace Crayon
{
    public abstract class CompilationScope
    {
        public virtual Locale Locale { get; }
        public virtual string ScopeKey { get; }

        protected BuildContext buildContext;

        private List<TopLevelConstruct> executables = new List<TopLevelConstruct>();

        public List<TopLevelConstruct> GetExecutables_HACK()
        {
            return this.executables;
        }

        public void AddExecutable(TopLevelConstruct executable)
        {
            if (executable is Namespace)
            {
                ((Namespace)executable).GetFlattenedCode(this.executables);
            }
            else
            {
                this.executables.Add(executable);
            }
        }

        public CompilationScope(BuildContext buildContext)
        {
            this.buildContext = buildContext;
        }
    }

    public class UserCodeCompilationScope : CompilationScope
    {
        public UserCodeCompilationScope(BuildContext buildContext) : base(buildContext)
        { }

        public override Locale Locale
        {
            get { return this.buildContext.CompilerLocale; }
        }

        public override string ScopeKey
        {
            get { return "."; }
        }

    }

    public class LibraryCompilationScope : CompilationScope
    {
        public LibraryMetadata Library { get; private set; }
        private string scopeKey;

        public LibraryCompilationScope(BuildContext buildContext, LibraryMetadata library) : base(buildContext)
        {
            this.Library = library;
            this.scopeKey = library.CanonicalKey;
        }

        public override Locale Locale
        {
            get { return this.Library.InternalLocale; }
        }

        public override string ScopeKey { get { return this.scopeKey; } }
    }
}
