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

        public void GenerateVmSourceCodeForPlatform(
            Common.AbstractPlatform platform,
            CompilationBundle nullableCompilationBundle,
            SystemLibraryManager libraryManager)
        {
            this.GenerateParseTree(platform, nullableCompilationBundle);
        }

        private void GenerateParseTree(Common.AbstractPlatform platform, CompilationBundle nullableCompilationBundle)
        {
            Pastel.PastelCompiler compiler = new Pastel.PastelCompiler(
                platform.GetFlattenedConstantFlags(),
                new InlineImportCodeLoader());

            foreach (string file in INTERPRETER_BASE_FILES)
            {
                string code = LegacyUtil.ReadInterpreterFileInternally(file);
                compiler.CompileBlobOfCode(file, code);
            }

            compiler.Resolve();
        }
    }
}
