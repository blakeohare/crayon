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
            Pastel.PastelCompiler compiler = new Pastel.PastelCompiler(platform.GetFlattenedConstantFlags());
            Dictionary<string, FunctionDefinition[]> compilationBlocks = new Dictionary<string, Pastel.Nodes.FunctionDefinition[]>();
            // List<Pastel.Nodes.ConstantDefinition> constants = new List<Pastel.Nodes.ConstantDefinition>();
            // List<Pastel.Nodes.EnumDefinition> enums = new List<Pastel.Nodes.EnumDefinition>();
            Dictionary<string, StructDefinition> structDefinitions = new Dictionary<string, Pastel.Nodes.StructDefinition>();

            foreach (string file in INTERPRETER_BASE_FILES)
            {
                string code = LegacyUtil.ReadInterpreterFileInternally(file);
                List<FunctionDefinition> fns = new List<FunctionDefinition>();
                object[] things = compiler.CompileBlock(file, code, codeLoader);
                foreach (object ex in things)
                {
                    if (ex is FunctionDefinition)
                    {
                        fns.Add((FunctionDefinition)ex);
                    }
                    else if (ex is StructDefinition)
                    {
                        StructDefinition sd = (StructDefinition)ex;
                        structDefinitions.Add(sd.NameToken.Value, sd);
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                }
            }
        }
    }
}
