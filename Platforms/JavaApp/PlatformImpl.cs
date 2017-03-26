using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common;
using Pastel.Nodes;
using Platform;

namespace GameJavaAwt
{
    public class PlatformImpl : AbstractPlatform
    {
        public override string Name { get { return "java-app"; } }
        public override string InheritsFrom { get { return "lang-java"; } }
        public override string NL { get { return "\n"; } }

        public PlatformImpl()
        {
            this.Translator = new JavaAppTranslator(this);
        }

        public override Dictionary<string, FileOutput> ExportProject(
            IList<VariableDeclaration> globals,
            IList<StructDefinition> structDefinitions,
            IList<FunctionDefinition> functionDefinitions,
            IList<LibraryForExport> libraries,
            ResourceDatabase resourceDatabase,
            Options options,
            ILibraryNativeInvocationTranslatorProvider libraryNativeInvocationTranslatorProviderForPlatform)
        {
            Dictionary<string, string> replacements = this.GenerateReplacementDictionary(options, resourceDatabase);
            Dictionary<string, FileOutput> files = new Dictionary<string, FileOutput>();
            CompatibilityHack.CriticalTODO("override the package from the build file to create a proper DNS-style package name."); // okay, not critical for CBX, but embarassing that you can't currently.
            string package = options.GetString(ExportOptionKey.PROJECT_ID).ToLower();
            string sourcePath = "src/" + package + "/";

            this.CopyResourceAsText(files, sourcePath + "TranslationHelper.java", "Resources/TranslationHelper.txt", replacements);
            
            return files;
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
            Dictionary<string, string> replacements = this.ParentPlatform.GenerateReplacementDictionary(options, resDb);
            replacements["PACKAGE"] = options.GetString(ExportOptionKey.PROJECT_ID).ToLower();
            return replacements;
        }

        public override IDictionary<string, object> GetConstantFlags()
        {
            return new Dictionary<string, object>();
        }
    }
}
