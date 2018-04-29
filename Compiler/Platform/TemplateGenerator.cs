using Pastel;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Platform
{
    public static class TemplateGenerator
    {
        public static void GenerateTemplatesForVmExport(
            TemplateStorage templates,
            PastelContext vmContext)
        {
            if (vmContext.UsesStringTable)
            {
                vmContext.GetTranspilerContext().StringTableBuilder = new Pastel.Transpilers.StringTableBuilder("VM");
            }

            string vmGlobalsCode = vmContext.GetCodeForGlobals();
            templates.AddPastelTemplate("vm:globals", vmGlobalsCode);

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

            if (vmContext.Language == Language.C)
            {
                templates.AddPastelTemplate("vm:struct:Value", "Value", string.Join("\n", new string[] {
                    "struct Value {",
                    "\tint type;",
                    "\tunion {",
                    "\t\tint null_internalValue; // not used",
                    "\t\tint bool_internalValue;",
                    "\t\tint int_internalValue;",
                    "\t\tdouble double_internalValue;",
                    "\t\tint* str_internalValue;",
                    "\t\tList* list_internalValue;",
                    "\t\tDictImpl* dict_internalValue;",
                    "\t\tObjectInstance* obj_internalValue;",
                    "\t\tClassValue* class_internalValue;",
                    "\t\tFunctionPointer* func_internalValue;",
                    "\t};",
                    "};",
                }));
            }

            if (vmContext.UsesStringTable)
            {
                templates.AddPastelTemplate(
                    "vm:stringtable",
                    vmContext.GetStringConstantTable());
            }
        }

        public static void GenerateTemplatesForLibraryExport(
            TemplateStorage templates,
            LibraryForExport library)
        {
            string libraryName = library.Name;
            PastelContext libContext = library.PastelContext;
            libContext.GetTranspilerContext().UniquePrefixForNonCollisions = libraryName.ToLower();

            if (libContext.Language == Language.PYTHON ||
                libContext.Language == Language.JAVASCRIPT ||
                libContext.Language == Language.JAVA)
            {
                bool changeManifestFuncName = libContext.Language == Language.JAVASCRIPT;
                string newManifestFunctionNameIfAny = changeManifestFuncName
                    ? "lib_" + libraryName.ToLower() + "_manifest"
                    : null;

                string manifestFunction = library.PastelContext.GetFunctionCodeForSpecificFunctionAndPopItFromFutureSerialization(
                        "lib_manifest_RegisterFunctions",
                        newManifestFunctionNameIfAny);
                templates.AddPastelTemplate("library:" + libraryName + ":manifestfunc", manifestFunction);
            }

            if (libContext.Language == Language.JAVA)
            {
                Dictionary<string, string> lookup = libContext.GetCodeForFunctionsLookup();
                StringBuilder sb = new StringBuilder();
                string reflectionCalledPrefix = "lib_" + library.Name.ToLower() + "_function_";
                libContext.GetTranspilerContext().TabDepth = 1;
                foreach (string functionName in lookup.Keys.OrderBy(k => k))
                {
                    string functionCode = lookup[functionName];
                    bool isFunctionPointerObject = functionName.StartsWith(reflectionCalledPrefix);

                    if (isFunctionPointerObject)
                    {
                        // This is kind of hacky, BUT...

                        // If the generated function needs to be used as a function pointer, (i.e. it's one
                        // of the library's VM-native bridge methods) change the name to "invoke" and then
                        // wrap it in a dummy class that extends LibraryFunctionPointer. The manifest
                        // function will simply instantiate this in lieu of a performant way to do
                        // function pointers in Java.
                        functionCode = functionCode.Replace(
                            "public static Value v_" + functionName + "(Value[] ",
                            "public Value invoke(Value[] ");
                        functionCode =
                            "  public static class FP_" + functionName + " extends LibraryFunctionPointer {\n" +
                            "  " + functionCode.Replace("\n", "\n  ").TrimEnd() + "\n" +
                            "  }";
                    }
                    sb.Append(functionCode);
                    sb.Append("\n");
                }
                libContext.GetTranspilerContext().TabDepth = 0;
                templates.AddPastelTemplate("library:" + library.Name + ":functions", sb.ToString().Trim());
            }
            else
            {
                string allFunctionCode = libContext.GetCodeForFunctions();
                templates.AddPastelTemplate("library:" + library.Name + ":functions", allFunctionCode);
            }

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
