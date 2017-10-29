using Build;
using Crayon.ParseTree;
using Localization;
using System.Collections.Generic;

namespace Crayon
{
    public class CompilationScope
    {
        public LibraryMetadata Library { get; private set; }

        public Locale Locale
        {
            get { return this.Library == null ? this.buildContext.CompilerLocale : this.Library.InternalLocale; }
        }

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

        private BuildContext buildContext;

        public CompilationScope(BuildContext buildContext, LibraryMetadata library)
        {
            this.buildContext = buildContext;
            this.Library = library;
        }
    }
}
