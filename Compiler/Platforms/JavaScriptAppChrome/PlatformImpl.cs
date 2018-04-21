using Common;
using Pastel.Nodes;
using Platform;
using System;
using System.Collections.Generic;

namespace JavaScriptAppChrome
{
    public class PlatformImpl : Platform.AbstractPlatform
    {
        public override string InheritsFrom { get { return "javascript-app"; } }
        public override string Name { get { return "javascript-app-chrome"; } }
        public override string NL { get { return "\n"; } }

        public PlatformImpl() : base(new ContextFreeJavaScriptAppChromePlatform())
        {
            this.Translator = new JavaScriptAppChromeTranslator(this);
        }

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
            if (!options.GetBool(ExportOptionKey.HAS_ICON))
            {
                throw new InvalidOperationException("Cannot generate a Chrome Web App without an icon resource.");
            }

            string iconFilePath = options.GetString(ExportOptionKey.ICON_PATH);
            if (!FileUtil.FileExists(iconFilePath)) throw new InvalidOperationException("Icon resource path points to non-existent file.");
            SystemBitmap iconFile = new SystemBitmap(iconFilePath);
            SystemBitmap smallIcon = iconFile.CloneToNewSize(16, 16);
            SystemBitmap largeIcon = iconFile.CloneToNewSize(128, 128);

            JavaScriptApp.PlatformImpl jsBasicPlatform = (JavaScriptApp.PlatformImpl)this.PlatformProvider.GetPlatform("javascript-app");
            Dictionary<string, FileOutput> proxyOutput = new Dictionary<string, FileOutput>();
            jsBasicPlatform.ExportProjectImpl(proxyOutput, globals, structDefinitions, functionDefinitions, libraries, resourceDatabase, options, libraryNativeInvocationTranslatorProviderForPlatform, this.Translator);
            Dictionary<string, string> replacements = this.GenerateReplacementDictionary(options, resourceDatabase);
            replacements["JS_LIB_INCLUSIONS"] = JavaScriptApp.PlatformImpl.GenerateJsLibInclusionHtml(proxyOutput.Keys);
            this.CopyResourceAsText(proxyOutput, "background.js", "Resources/BackgroundJs.txt", replacements);
            this.CopyResourceAsText(proxyOutput, "index.html", "Resources/IndexHtml.txt", replacements); // overwrites GameHostHtml.txt from javascript-app
            this.CopyResourceAsText(proxyOutput, "chrome_web_app.js", "Resources/ChromeWebAppJs.txt", replacements);
            this.CopyResourceAsText(proxyOutput, "manifest.json", "Resources/ManifestJson.txt", Util.MakeReplacementStringsJsonSafe(replacements));
            proxyOutput["icon-16.png"] = new FileOutput()
            {
                Type = FileOutputType.Image,
                Bitmap = smallIcon,
            };
            proxyOutput["icon-128.png"] = new FileOutput()
            {
                Type = FileOutputType.Image,
                Bitmap = largeIcon,
            };

            output[replacements["PROJECT_ID"] + ".zip"] = ZipCreator.Create(proxyOutput, false);
        }

        public override void ExportStandaloneVm(Dictionary<string, FileOutput> output, IList<VariableDeclaration> globals, IList<StructDefinition> structDefinitions, IList<FunctionDefinition> functionDefinitions, IList<LibraryForExport> everyLibrary, ILibraryNativeInvocationTranslatorProvider libraryNativeInvocationTranslatorProviderForPlatform)
        {
            throw new NotImplementedException();
        }

        public override string GenerateCodeForFunction(AbstractTranslator translator, FunctionDefinition funcDef)
        {
            return this.ParentPlatform.GenerateCodeForFunction(translator, funcDef);
        }

        public override string GenerateCodeForGlobalsDefinitions(AbstractTranslator translator, IList<VariableDeclaration> globals)
        {
            return this.ParentPlatform.GenerateCodeForGlobalsDefinitions(translator, globals);
        }

        public override string GenerateCodeForStruct(StructDefinition structDef)
        {
            return this.ParentPlatform.GenerateCodeForStruct(structDef);
        }

        public override Dictionary<string, string> GenerateReplacementDictionary(Options options, ResourceDatabase resDb)
        {
            return this.ParentPlatform.GenerateReplacementDictionary(options, resDb);
        }

        public override IDictionary<string, object> GetConstantFlags()
        {
            return this.ParentPlatform.GetConstantFlags();
        }
    }
}
