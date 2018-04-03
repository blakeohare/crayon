using Common;
using Pastel.Nodes;
using Platform;
using System;
using System.Collections.Generic;

namespace JavaScriptAppAndroid
{
    public class PlatformImpl : AbstractPlatform
    {
        public override string InheritsFrom { get { return "javascript-app"; } }
        public override string Name { get { return "javascript-app-android"; } }
        public override string NL { get { return "\n"; } }

        public override void ExportProject(
            Dictionary<string, FileOutput> output,
            IList<VariableDeclaration> globals,
            IList<StructDefinition> structDefinitions,
            IList<FunctionDefinition> functionDefinitions,
            IList<LibraryForExport> libraries,
            ResourceDatabase resourceDatabase,
            Options options,
            ILibraryNativeInvocationTranslatorProvider libraryNativeInvocationTranslatorProviderForPlatform)
        {
            options.SetOption(ExportOptionKey.JS_FILE_PREFIX, null);
            options.SetOption(ExportOptionKey.JS_FULL_PAGE, false); // TODO: figure out if logic should be implemented in the web view for this.
            options.SetOption(ExportOptionKey.JS_HEAD_EXTRAS, string.Join(
                "\n",
                "<script type=\"text/javascript\" src=\"file:///android_asset/android.js\"></script>",
                "<style type=\"text/css\">",
                "  body { margin:0px; background-color:#000; }",
                "  #crayon_host {",
                "    background-color:#000;",
                "    text-align:left;",
                "    width:100%;",
                "    height:100%;",
                "  }",
                "</style>"
            ));
            Dictionary<string, string> replacements = this.GenerateReplacementDictionary(options, resourceDatabase);

            Dictionary<string, FileOutput> files = new Dictionary<string, FileOutput>();
            Dictionary<string, FileOutput> basicProject = new Dictionary<string, FileOutput>();
            this.ParentPlatform.ExportProject(
                basicProject,
                globals,
                structDefinitions,
                functionDefinitions,
                libraries,
                resourceDatabase,
                options,
                libraryNativeInvocationTranslatorProviderForPlatform);

            // TODO: not good. The library inclusions should automatically be populated in LangJavaScript platforms.
            // This is also done identically in the ChromeApp PlatformImpl.
            replacements["JS_LIB_INCLUSIONS"] = JavaScriptApp.PlatformImpl.GenerateJsLibInclusionHtml(basicProject.Keys);

            foreach (string filePath in basicProject.Keys)
            {
                files["%%%PROJECT_ID%%%/%%%PROJECT_ID%%%/jsres/" + filePath] = basicProject[filePath];
            }

            OrientationParser orientations = new OrientationParser(options);

            throw new NotImplementedException();
        }

        public override Dictionary<string, string> GenerateReplacementDictionary(Options options, ResourceDatabase resDb)
        {
            return this.ParentPlatform.GenerateReplacementDictionary(options, resDb);
        }

        public override IDictionary<string, object> GetConstantFlags()
        {
            return this.ParentPlatform.GetConstantFlags();
        }

        public override void ExportStandaloneVm(Dictionary<string, FileOutput> output, IList<VariableDeclaration> globals, IList<StructDefinition> structDefinitions, IList<FunctionDefinition> functionDefinitions, IList<LibraryForExport> everyLibrary, ILibraryNativeInvocationTranslatorProvider libraryNativeInvocationTranslatorProviderForPlatform) { throw new NotImplementedException(); }
        public override string GenerateCodeForFunction(AbstractTranslator translator, FunctionDefinition funcDef) { throw new NotImplementedException(); }
        public override string GenerateCodeForGlobalsDefinitions(AbstractTranslator translator, IList<VariableDeclaration> globals) { throw new NotImplementedException(); }
        public override string GenerateCodeForStruct(StructDefinition structDef) { throw new NotImplementedException(); }
    }
}
