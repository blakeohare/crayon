using System.Collections.Generic;

namespace Crayon
{
    internal class CompilationBundle
    {
        public ByteBuffer ByteCode { get; set; }
        public string ProjectID { get; set; }
        public string GuidSeed { get; set; }
        public ICollection<Library> LibrariesUsed { get; set; }
        public string IconPath { get; set; }
        public string DefaultTitle { get; set; }

        public static CompilationBundle Compile(BuildContext buildContext)
        {
            Parser parser = new Parser(null, buildContext, null);
            Crayon.ParseTree.Executable[] resolvedParseTree = parser.ParseAllTheThings();

            ByteCodeCompiler bcc = new ByteCodeCompiler();
            ByteBuffer buffer = bcc.GenerateByteCode(parser, resolvedParseTree);

            return new CompilationBundle()
            {
                ByteCode = buffer,
                LibrariesUsed = parser.SystemLibraryManager.LibrariesUsed,
                ProjectID = buildContext.ProjectID,
                GuidSeed = buildContext.GuidSeed,
                DefaultTitle = buildContext.DefaultTitle,
            };
        }
    }
}
