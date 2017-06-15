using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common;
using Pastel.Nodes;
using Platform;

namespace JavaScriptAppAndroid
{
    public class PlatformImpl : Platform.AbstractPlatform
    {
        public override string InheritsFrom {  get { return "javascript-app-gl"; } }
        public override string Name { get { return "javascript-app-android"; } }
        public override string NL { get { return "\n"; } }

        public override Dictionary<string, FileOutput> ExportProject(
            IList<VariableDeclaration> globals,
            IList<StructDefinition> structDefinitions,
            IList<FunctionDefinition> functionDefinitions,
            IList<LibraryForExport> libraries,
            ResourceDatabase resourceDatabase,
            Options options,
            ILibraryNativeInvocationTranslatorProvider libraryNativeInvocationTranslatorProviderForPlatform)
        {
            Dictionary<string, FileOutput> files = new Dictionary<string, FileOutput>();
            Dictionary<string, string> replacements = this.GenerateReplacementDictionary(options, resourceDatabase);

            JavaAppAndroid.PlatformImpl javaAndroidPlatform = (JavaAppAndroid.PlatformImpl)this.PlatformProvider.GetPlatform("java-app-android");
            javaAndroidPlatform.OutputAndroidBoilerplate(files, replacements);
            files["app/src/main/java/org/crayonlang/crayonsampleapp/app/MainActivity.java"] = new FileOutput()
            {
                TrimBomIfPresent = true,
                Type = FileOutputType.Text,
                TextContent = this.LoadTextResource("Resources/MainActivityJava.txt", replacements),
            };
            
            Dictionary<string, FileOutput> jsFiles = this.ParentPlatform.ExportProject(globals, structDefinitions, functionDefinitions, libraries, resourceDatabase, options, libraryNativeInvocationTranslatorProviderForPlatform);
            foreach (string file in jsFiles.Keys)
            {
                files["app/src/main/assets/js/" + file] = jsFiles[file];
            }
            return files;
        }

        public override Dictionary<string, FileOutput> ExportStandaloneVm(IList<VariableDeclaration> globals, IList<StructDefinition> structDefinitions, IList<FunctionDefinition> functionDefinitions, IList<LibraryForExport> everyLibrary, ILibraryNativeInvocationTranslatorProvider libraryNativeInvocationTranslatorProviderForPlatform)
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
