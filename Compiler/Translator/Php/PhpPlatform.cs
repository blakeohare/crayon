﻿using System.Collections.Generic;
using Crayon.ParseTree;

namespace Crayon.Translator.Php
{
    internal class PhpPlatform : AbstractPlatform
    {
        public PhpPlatform() : base(
            Crayon.PlatformId.PHP_SERVER,
            Crayon.LanguageId.PHP,
            new PhpTranslator(),
            new PhpSystemFunctionTranslator())
        { }

        public override string PlatformShortId { get { return "server-php"; } }
        
        public override bool IsArraySameAsList { get { return true; } }
        public override bool IsAsync { get { return false; } }
        public override bool IsStronglyTyped { get { return false; } }
        public override bool SupportsListClear { get { return false; } }
        public override bool IsByteCodeLoadedDirectly { get { return true; } }
        public override bool IsCharANumber { get { return false; } }
        public override bool IntIsFloor { get { return false; } }
        public override bool IsThreadBlockingAllowed { get { return true; } }

        public override Dictionary<string, FileOutput> Package(
            BuildContext buildContext,
            string projectId,
            Dictionary<string, Executable[]> finalCode,
            ICollection<StructDefinition> structDefinitions,
            string fileCopySourceRoot,
            ResourceDatabase resourceDatabase,
            SystemLibraryManager libraryManager)
        {
            Dictionary<string, FileOutput> output = new Dictionary<string, FileOutput>();
            FileOutput byteCodeFile = this.GenerateByteCodeFile(resourceDatabase.ByteCodeRawData);
            output["bytecode.php"] = byteCodeFile;

            Dictionary<string, string> replacements = new Dictionary<string, string>();

            List<string> codePhp = new List<string>();

            codePhp.Add("<?php\n\n");
            foreach (string component in finalCode.Keys)
            {
                this.Translator.Translate(codePhp, finalCode[component]);
                codePhp.Add(this.Translator.NL);
            }
            codePhp.Add("\n\n?>");

            output["crayon.php"] = new FileOutput()
            {
                Type = FileOutputType.Text,
                TextContent = string.Join("", codePhp),
            };

            output["cryutil.php"] = new FileOutput()
            {
                Type = FileOutputType.Text,
                TextContent = Constants.DoReplacements(Util.ReadResourceFileInternally("server-php/cryutil.php"), replacements),
            };

            output["index.php"] = new FileOutput()
            {
                Type = FileOutputType.Text,
                TextContent = Constants.DoReplacements(Util.ReadResourceFileInternally("server-php/index.php"), replacements),
            };

            return output;
        }

        private FileOutput GenerateByteCodeFile(ByteBuffer byteCodeBuffer)
        {
            List<string> output = new List<string>();
            output.Add("<?php");

            List<int[]> integers = byteCodeBuffer.ToIntList();
            List<string> strings = byteCodeBuffer.ToStringList();
            List<string> line = new List<string>();

            output.Add("$bc_ops = array(");

            for (int i = 0; i < integers.Count; i += 30)
            {
                for (int j = 0; j < 30; ++j)
                {
                    int index = i + j;
                    if (index < integers.Count)
                    {
                        line.Add(integers[index][0] + ", ");
                    }
                }
                output.Add(string.Join("", line));
                line.Clear();
            }
            output.Add(");");

            output.Add("$e = new Rf(array());");
            output.Add("$z = new Rf(array(0));");
            output.Add("$o = new Rf(array(1));");
            output.Add("$bc_iargs = array(");
            for (int rowIndex = 0; rowIndex < integers.Count; ++rowIndex)
            {
                int[] row = integers[rowIndex];
                if (row.Length == 1)
                {
                    line.Add("$e,");
                }
                else if (row.Length == 2 && (row[1] == 0 || row[1] == 1))
                {
                    line.Add(row[1] == 0 ? "$z," : "$o,");
                }
                else
                {
                    line.Add("new Rf(array(");
                    for (int i = 1; i < row.Length; ++i)
                    {
                        if (i > 1) line.Add(",");
                        line.Add(row[i].ToString());
                    }
                    line.Add(")),");
                }
                output.Add(string.Join("", line));
                line.Clear();
            }
            output.Add(");");
            output.Add("$bc_sargs = array();");
            output.Add("for ($i = count($byteCode); $i > 0; --$i) array_push($bc_ops, null);");
            output.Add("$d = '$';");
            for (int i = 0; i < strings.Count; ++i)
            {
                if (strings[i] != null)
                {
                    output.Add("$bc_sargs[" + i + "] = " + Util.ConvertStringValueToCode(strings[i]).Replace("$", "$d") + ";");
                }
            }

            output.AddRange(new string[] {
                "function bytecode_get_iargs() {",
                " global $bc_iargs;",
                " return new Rf($bc_iargs);",
                "}",

                "function bytecode_get_sargs() {",
                " global $bc_sargs;",
                " return new Rf($bc_sargs);",
                "}",

                "function bytecode_get_ops() {",
                " global $bc_ops;",
                " return new Rf($bc_ops);",
                "}"
            });

            output.Add("\n?>");

            return new FileOutput()
            {
                Type = FileOutputType.Text,
                TextContent = string.Join("\n", output),
            };
        }
    }
}
