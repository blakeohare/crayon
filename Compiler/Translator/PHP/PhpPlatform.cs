using System;
using System.Collections.Generic;
using Crayon.ParseTree;

namespace Crayon.Translator.Php
{
	internal class PhpPlatform : AbstractPlatform
	{
		public PhpPlatform() : base(
			Crayon.PlatformId.PHP_SERVER,
			Crayon.LanguageId.PHP,
			false, new PhpTranslator(),
			new PhpSystemFunctionTranslator(),
			false)
		{ }

		public override bool ImagesLoadInstantly { get { return true; } }

		public override bool IsArraySameAsList { get { return true; } }

		public override bool IsAsync { get { return false; } }

		public override bool IsStronglyTyped { get { return false; } }

		public override string PlatformShortId { get { return "php-server"; } }

		public override bool SupportsListClear { get { return false; } }

		public override Dictionary<string, FileOutput> Package(
			BuildContext buildContext,
			string projectId,
			Dictionary<string, Executable[]> finalCode,
			ICollection<StructDefinition> structDefinitions,
			string fileCopySourceRoot,
			ResourceDatabase resourceDatabase)
		{
            Dictionary<string, FileOutput> output = new Dictionary<string, FileOutput>();
            FileOutput byteCodeFile = this.GenerateByteCodeFile(resourceDatabase.ByteCodeRawData);
            output["bytecode.php"] = byteCodeFile;


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

            return output;
		}

        private FileOutput GenerateByteCodeFile(ByteBuffer byteCodeBuffer)
        {
            List<string> output = new List<string>();
            output.Add("<?php");
            output.Add("$byteCode = array(");
            List<int[]> integers = byteCodeBuffer.ToIntList();
            List<string> line = new List<string>();
            for (int rowIndex = 0; rowIndex < integers.Count; ++rowIndex)
            {
                int[] row = integers[rowIndex];
                line.Add("array(");
                for (int i = 0; i < row.Length; ++i)
                {
                    if (i > 0) line.Add(",");
                    line.Add(row[i].ToString());
                }
                line.Add(")");
                if (rowIndex + 1 < integers.Count)
                {
                    line.Add("),");
                }
                else
                {
                    line.Add("));");
                }
                output.Add(string.Join("", line));
                line.Clear();
            }
            output.Add("$byteCodeStrings = array();");
            output.Add("for ($i = count($byteCode); $i > 0; --$i) array_push($byteCodeStrings, null);");
            output.Add("$d = '$';");
            List<string> strings = byteCodeBuffer.ToStringList();
            for (int i = 0; i < strings.Count; ++i)
            {
                if (strings[i] != null)
                {
                    output.Add("$byteCodeStrings[" + i + "] = " + Util.ConvertStringValueToCode(strings[i]).Replace("$", "$d") + ";");
                }
            }
            output.Add("?>");

            return new FileOutput()
            {
                Type = FileOutputType.Text,
                TextContent = string.Join("\n", output),
            };
        }
	}
}

