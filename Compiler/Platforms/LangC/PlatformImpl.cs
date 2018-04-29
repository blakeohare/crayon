using Common;
using Pastel;
using Pastel.Transpilers;
using Platform;
using System;
using System.Collections.Generic;
using System.Text;

namespace LangC
{
    public class PlatformImpl : AbstractPlatform
    {
        public override string Name { get { return "lang-c"; } }
        public override string InheritsFrom { get { return null; } }
        public override string NL { get { return "\n"; } }

        public PlatformImpl()
            : base(Language.C)
        { }

        public override IDictionary<string, object> GetConstantFlags()
        {
            return new Dictionary<string, object>()
                {
                    { "IS_ASYNC", false },
                    { "PLATFORM_SUPPORTS_LIST_CLEAR", true },
                    { "STRONGLY_TYPED", true },
                    { "IS_ARRAY_SAME_AS_LIST", false },
                    { "IS_PYTHON", false },
                    { "IS_CHAR_A_NUMBER", true },
                    { "INT_IS_FLOOR", true },
                    { "IS_THREAD_BLOCKING_ALLOWED", true },
                    { "HAS_INCREMENT", true },
                    { "IS_C", true },
                };
        }

        public override void GenerateTemplates(
            TemplateStorage templates,
            PastelContext vmContext,
            IList<LibraryForExport> libraries)
        { }

        public override void ExportStandaloneVm(
            Dictionary<string, FileOutput> output,
            TemplateStorage templates,
            PastelContext pastelContext,
            IList<LibraryForExport> everyLibrary)
        {
            throw new NotImplementedException();
        }

        public override void ExportProject(
            Dictionary<string, FileOutput> output,
            TemplateStorage templates,
            PastelContext pastelContext,
            IList<LibraryForExport> libraries,
            ResourceDatabase resourceDatabase,
            Options options)
        {
            throw new NotImplementedException();
        }

        public override Dictionary<string, string> GenerateReplacementDictionary(Options options, ResourceDatabase resDb)
        {
            throw new NotImplementedException();
        }

        // TODO: this should be in Pastel
        public static string BuildStringTable(StringTableBuilder stringTable, string newline)
        {
            StringBuilder sb = new StringBuilder();
            List<string> names = stringTable.Names;
            List<string> values = stringTable.Values;
            int total = names.Count;
            for (int i = 0; i < total; ++i)
            {
                sb.Append("int* ");
                sb.Append(names[i]);
                sb.Append(';');
                sb.Append(newline);
            }
            sb.Append("void populate_string_table_for_");
            sb.Append(stringTable.Prefix);
            sb.Append("()");
            sb.Append(newline);
            sb.Append('{');
            sb.Append(newline);
            for (int i = 0; i < total; ++i)
            {
                sb.Append('\t');
                sb.Append(names[i]);
                sb.Append(" = String_from_utf8(");
                sb.Append(Util.ConvertStringValueToCode(values[i]).Replace("%", "%%"));
                sb.Append(");");
                sb.Append(newline);
            }
            sb.Append('}');
            sb.Append(newline);
            sb.Append(newline);

            return sb.ToString();
        }
    }
}
