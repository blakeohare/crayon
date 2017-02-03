using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Crayon
{
    internal class CompilationBundle
    {
        public ByteBuffer ByteCode { get; set; }

        public static CompilationBundle Compile(BuildContext buildContext)
        {
            Parser parser = new Parser(null, buildContext, null);
            Crayon.ParseTree.Executable[] resolvedParseTree = parser.ParseAllTheThings();

            ByteCodeCompiler bcc = new ByteCodeCompiler();
            ByteBuffer buffer = bcc.GenerateByteCode(parser, resolvedParseTree);

            return new CompilationBundle()
            {
                ByteCode = buffer,
            };
        }
    }
}
