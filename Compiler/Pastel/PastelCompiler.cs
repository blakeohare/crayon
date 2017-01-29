using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crayon.Pastel.Nodes;

namespace Crayon.Pastel
{
    class PastelCompiler
    {
        // Order in which the files are compiled.
        private static readonly string[] FILES = new string[] {

            // These 3 must go first
            "Structs.pst",
            "Constants.pst",
            "Globals.pst",

            // These are just piles of functions so they can be compiled in any order.
            "BinaryOpsUtil.pst",
            "ByteCodeLoader.pst",
            "Interpreter.pst",
            "MetadataInitializer.pst",
            "PrimitiveMethods.pst",
            "ResourceManager.pst",
            "Runner.pst",
            "TypesUtil.pst",
            "ValueUtil.pst",
        };

        private AbstractPlatform platform;
        private Crayon.Pastel.Parser interpreterParser;
        private SwitchStatementTracker switchStatementTracker;
        
        public PastelCompiler(AbstractPlatform platform, SystemLibraryManager sysLibMan)
        {
            this.platform = platform;
            this.interpreterParser = new Parser(platform, sysLibMan);
            this.switchStatementTracker = new SwitchStatementTracker();
        }

        public Dictionary<string, Crayon.Pastel.Nodes.Executable[]> Compile()
        {
            Dictionary<string, Executable[]> output = new Dictionary<string, Executable[]>();

            Dictionary<string, string> platformConstants = this.BuildPlatformConstantsLookup();

            Dictionary<string, string> filesById = new Dictionary<string, string>();

            Dictionary<string, string> libraryProducedFiles = this.interpreterParser.LibraryManager.GetSupplementalTranslationFiles();
            
            List<string> orderedFileIds = new List<string>();

            foreach (string file in FILES)
            {
                string fileId = file.Split('.')[0];
                string code = Util.ReadInterpreterFileInternally(file);
                filesById[fileId] = code;
                orderedFileIds.Add(fileId);
            }

            foreach (string fileId in libraryProducedFiles.Keys)
            {
                filesById[fileId] = libraryProducedFiles[fileId];
                orderedFileIds.Add(fileId);
            }

            foreach (string fileId in orderedFileIds)
            {
                string code = filesById[fileId];

                Executable[] lines = this.interpreterParser.ParseText(fileId, code);
                if (lines.Length > 0)
                {
                    output[fileId] = lines;
                }
            }

            string switchLookupCode = this.switchStatementTracker.GetSwitchLookupCode().Trim();
            if (switchLookupCode.Length > 0)
            {
                output["SwitchLookups"] = this.interpreterParser.ParseText("SwitchLookups.pst", switchLookupCode);
            }

            return output;
        }

        private Dictionary<string, string> BuildPlatformConstantsLookup()
        {
            Dictionary<string, string> replacements = new Dictionary<string, string>();
            replacements.Add("PLATFORM_SUPPORTS_LIST_CLEAR", this.platform.SupportsListClear ? "true" : "false");
            replacements.Add("STRONGLY_TYPED", this.platform.IsStronglyTyped ? "true" : "false");
            replacements.Add("IS_ARRAY_SAME_AS_LIST", this.platform.IsArraySameAsList ? "true" : "false");
            replacements.Add("IS_BYTECODE_LOADED_DIRECTLY", this.platform.IsByteCodeLoadedDirectly ? "true" : "false");
            replacements.Add("PLATFORM_SHORT_ID", this.platform.PlatformShortId);
            replacements.Add("LIBRARY_FUNCTION_BIG_SWITCH_STATEMENT", this.platform.LibraryBigSwitchStatement);
            replacements.Add("IS_PHP", (this.platform is Crayon.Translator.Php.PhpPlatform) ? "true" : "false");
            replacements.Add("IS_PYTHON", (this.platform is Crayon.Translator.Python.PythonPlatform) ? "true" : "false");
            replacements.Add("IS_CHAR_A_NUMBER", this.platform.IsCharANumber ? "true" : "false");
            replacements.Add("INT_IS_FLOOR", this.platform.IntIsFloor ? "true" : "false");
            replacements.Add("IS_THREAD_BLOCKING_ALLOWED", this.platform.IsThreadBlockingAllowed ? "true" : "false");
            replacements.Add("IS_JAVASCRIPT", this.platform.LanguageId == LanguageId.JAVASCRIPT ? "true" : "false");
            replacements.Add("IS_ANDROID", this.platform.PlatformShortId == "game-csharp-android" ? "true" : "false");
            replacements.Add("IS_JAVA", this.platform.LanguageId == LanguageId.JAVA ? "true" : "false");
            replacements.Add("IS_RUBY", this.platform.LanguageId == LanguageId.RUBY ? "true" : "false");
            return replacements;
        }
    }
}
