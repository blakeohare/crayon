using Common;
using Pastel;
using Pastel.Transpilers;
using Platform;
using System;
using System.Collections.Generic;
using System.Text;

namespace CApp
{
    public class PlatformImpl : AbstractPlatform
    {
        public override string Name { get { return "c-app"; } }
        public override string InheritsFrom { get { return "lang-c"; } }
        public override string NL { get { return "\n"; } }

        public PlatformImpl()
            : base(Language.C)
        { }

        public override void GenerateTemplates(
            TemplateStorage templates,
            PastelContext vmContext,
            IList<LibraryForExport> libraries)
        {
            vmContext.GetTranspilerContext().StringTableBuilder = new StringTableBuilder("VM");

            string functionDeclarationCode = vmContext.GetCodeForFunctionDeclarations();
            templates.AddPastelTemplate("vm:functionsdecl", functionDeclarationCode);

            string functionDefinitionCode = vmContext.GetCodeForFunctions();
            templates.AddPastelTemplate("vm:functions", functionDeclarationCode);

            Dictionary<string, string> structsLookup = vmContext.GetCodeForStructs();
            foreach (string structName in structsLookup.Keys)
            {
                templates.AddPastelTemplate("vm:struct:" + structName, structName, structsLookup[structName]);
            }
            // Override the auto generated Value struct since I need to do weird things with unions.
            templates.AddPastelTemplate(
                "vm:struct:Value",
                "Value",
                this.LoadTextResource("Resources/ValueStruct.txt",
                new Dictionary<string, string>()));


            StringBuilder sb = new StringBuilder();
            foreach (string structKey in templates.GetTemplateKeysWithPrefix("vm:struct:"))
            {
                string structName = templates.GetName(structKey);
                sb.Append("typedef struct " + structName + " " + structName + ";");
                sb.Append(this.NL);
            }
            templates.AddPastelTemplate("vm:structsdecl", sb.ToString().Trim());

            templates.AddPastelTemplate(
                "vm:stringtable",
                vmContext.GetStringConstantTable());

            // TODO: libraries for C
        }

        public override void ExportProject(
            Dictionary<string, FileOutput> output,
            TemplateStorage templates,
            PastelContext pastelContext,
            IList<LibraryForExport> libraries,
            ResourceDatabase resourceDatabase,
            Options options)
        {
            TranspilerContext ctx = pastelContext.GetTranspilerContext();
            Dictionary<string, string> replacements = this.GenerateReplacementDictionary(options, resourceDatabase);
            StringBuilder cCode = new StringBuilder();

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

            cCode.Append(templates.GetCode("vm:structsdecl"));
            cCode.Append(this.NL);

            foreach (string structKey in templates.GetTemplateKeysWithPrefix("vm:struct:"))
            {
                string structName = templates.GetName(structKey);
                cCode.Append(templates.GetCode(structKey));
            }
            cCode.Append(this.NL);

            cCode.Append(templates.GetCode("vm:stringtable"));
            cCode.Append(this.NL);
            cCode.Append(templates.GetCode("vm:functionsdecl"));
            cCode.Append(this.NL);
            cCode.Append(templates.GetCode("vm:functions"));
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
            TemplateStorage templates,
            PastelContext pastelContext,
            IList<LibraryForExport> everyLibrary)
        {
            throw new NotImplementedException();
        }

        public override Dictionary<string, string> GenerateReplacementDictionary(Options options, ResourceDatabase resDb)
        {
            return new Dictionary<string, string>();
        }

        public override IDictionary<string, object> GetConstantFlags()
        {
            return new Dictionary<string, object>();
        }
    }
}
