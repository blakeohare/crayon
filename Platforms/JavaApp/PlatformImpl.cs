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
            Dictionary<string, FileOutput> output = new Dictionary<string, FileOutput>();
            CompatibilityHack.CriticalTODO("override the package from the build file to create a proper DNS-style package name."); // okay, not critical for CBX, but embarassing that you can't currently.
            string package = options.GetString(ExportOptionKey.PROJECT_ID).ToLower();
            string sourcePath = "src/" + package + "/";

            foreach (StructDefinition structDefinition in structDefinitions)
            {
                string structCode = this.GenerateCodeForStruct(structDefinition).Trim();
                List<string> structFileLines = new List<string>();
                structFileLines.Add("package org.crayon.interpreter.structs;");
                structFileLines.Add("");
                bool hasLists = structCode.Contains("public ArrayList<");
                bool hasDictionaries = structCode.Contains("public HashMap<");
                if (hasLists) structFileLines.Add("import java.util.ArrayList;");
                if (hasDictionaries) structFileLines.Add("import java.util.HashMap;");
                if (hasLists || hasDictionaries) structFileLines.Add("");
                structFileLines.Add(structCode);
                structFileLines.Add("");

                output["src/org/crayon/interpreter/structs/" + structDefinition.NameToken.Value + ".java"] = new FileOutput()
                {
                    Type = FileOutputType.Text,
                    TextContent = string.Join(this.NL, structFileLines),
                };
            }

            this.CopyResourceAsText(output, sourcePath + "TranslationHelper.java", "Resources/TranslationHelper.txt", replacements);

            return output;
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
