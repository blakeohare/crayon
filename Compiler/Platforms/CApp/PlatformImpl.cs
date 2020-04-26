using Common;
using Platform;
using System;
using System.Collections.Generic;

namespace CApp
{
    public class PlatformImpl : AbstractPlatform
    {
        public override string Name { get { return "c-app"; } }
        public override string InheritsFrom { get { return "lang-c"; } }
        public override string NL { get { return "\n"; } }

        public PlatformImpl()
            : base("C")
        { }

        public override void ExportProject(
            Dictionary<string, FileOutput> output,
            IList<LibraryForExport> libraries,
            Build.ResourceDatabase resourceDatabase,
            Options options)
        {
            Dictionary<string, string> replacements = this.GenerateReplacementDictionary(options, resourceDatabase);
            System.Text.StringBuilder cCode = new System.Text.StringBuilder();

            cCode.Append("#include <stdio.h>");
            cCode.Append(this.NL);
            cCode.Append("#include <stdlib.h>");
            cCode.Append(this.NL);
            cCode.Append("#include <string.h>");
            cCode.Append(this.NL);
            cCode.Append(this.NL);

            cCode.Append(this.LoadTextResource("Resources/List.txt", replacements));
            cCode.Append(this.LoadTextResource("Resources/String.txt", replacements));
            cCode.Append(this.LoadTextResource("Resources/Dictionary.txt", replacements));
            cCode.Append(this.LoadTextResource("Resources/TranslationHelper.txt", replacements));
            cCode.Append(this.NL);

            //cCode.Append(templates.GetCode("vm:structsdecl"));
            cCode.Append(this.NL);

            /*
            foreach (string structKey in templates.GetTemplateKeysWithPrefix("vm:struct:"))
            {
                //string structName = templates.GetName(structKey);
                //cCode.Append(templates.GetCode(structKey));
            }
            */
            cCode.Append(this.NL);

            //cCode.Append(templates.GetCode("vm:functionsdecl"));
            cCode.Append(this.NL);
            //cCode.Append(templates.GetCode("vm:functions"));
            cCode.Append(this.NL);

            cCode.Append(this.LoadTextResource("Resources/main.txt", replacements));

            output["main.c"] = new FileOutput()
            {
                Type = FileOutputType.Text,
                TextContent = cCode.ToString(),
            };
        }

        public override void ExportStandaloneVm(
            Dictionary<string, FileOutput> output,
            IList<LibraryForExport> everyLibrary)
        {
            throw new NotImplementedException();
        }

        public override Dictionary<string, string> GenerateReplacementDictionary(
            Options options,
            Build.ResourceDatabase resDb)
        {
            return new Dictionary<string, string>();
        }

        public override IDictionary<string, object> GetConstantFlags()
        {
            return new Dictionary<string, object>();
        }
    }
}
