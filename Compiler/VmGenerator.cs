using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common;
using Pastel.Nodes;

namespace Crayon
{
    internal class VmGenerator
    {
        private static readonly string[] INTERPRETER_BASE_FILES = new string[] {
            "BinaryOpsUtil.pst",
            "ByteCodeLoader.pst",
            "Constants.pst",
            "Globals.pst",
            "Interpreter.pst",
            "MetadataInitializer.pst",
            "PrimitiveMethods.pst",
            "ResourceManager.pst",
            "Runner.pst",
            "Structs.pst",
            "TypesUtil.pst",
            "ValueUtil.pst",
        };

        public void GenerateVmSourceCodeForPlatform(Common.AbstractPlatform platform, SystemLibraryManager libraryManager)
        {
            IInlineImportCodeLoader codeLoader = new InlineImportCodeLoader();
            Pastel.PastelCompiler compiler = new Pastel.PastelCompiler(platform.GetFlattenedConstantFlags(), codeLoader);

            foreach (string file in INTERPRETER_BASE_FILES)
            {
                string code = LegacyUtil.ReadInterpreterFileInternally(file);
                compiler.CompileBlobOfCode(file, code);
            }
        }
    }
}
