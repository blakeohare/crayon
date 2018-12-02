using Pastel;
using System.Collections.Generic;
using System.Text;

namespace Platform
{
    public static class TemplateGenerator
    {
        public static void GenerateTemplatesForVmExport(
            TemplateStorage templates,
            PastelContext vmContext)
        {
            if (vmContext.UsesFunctionDeclarations)
            {
                string functionDeclarationCode = vmContext.GetCodeForFunctionDeclarations();
                templates.AddPastelTemplate("vm:functionsdecl", functionDeclarationCode);
            }

            string functionCode = vmContext.GetCodeForFunctions();
            templates.AddPastelTemplate("vm:functions", functionCode);

            if (vmContext.UsesStructDefinitions)
            {
                Dictionary<string, string> structLookup = vmContext.GetCodeForStructs();
                foreach (string structName in structLookup.Keys)
                {
                    templates.AddPastelTemplate("vm:struct:" + structName, structName, structLookup[structName]);
                }

                if (vmContext.UsesStructDeclarations)
                {

                    StringBuilder sb = new StringBuilder();
                    foreach (string structKey in templates.GetTemplateKeysWithPrefix("vm:struct:"))
                    {
                        string structName = templates.GetName(structKey);
                        sb.Append(vmContext.GetCodeForStructDeclaration(structName));
                    }
                    templates.AddPastelTemplate("vm:structsdecl", sb.ToString().Trim());
                }
            }
        }

        public static void GenerateTemplatesForLibraryExport(
            TemplateStorage templates,
            LibraryForExport library)
        {
            string libraryName = library.Name;
            PastelContext libContext = library.PastelContext;
            libContext.GetTranspilerContext().UniquePrefixForNonCollisions = libraryName.ToLower();

            string allFunctionCode = libContext.GetCodeForFunctions();
            templates.AddPastelTemplate("library:" + library.Name + ":functions", allFunctionCode);

            if (libContext.UsesStructDefinitions)
            {
                Dictionary<string, string> libStructLookup = libContext.GetCodeForStructs();
                foreach (string structName in libStructLookup.Keys)
                {
                    templates.AddPastelTemplate(
                        "library:" + library.Name + ":struct:" + structName,
                        structName,
                        libStructLookup[structName]);
                }
            }
        }
    }
}
