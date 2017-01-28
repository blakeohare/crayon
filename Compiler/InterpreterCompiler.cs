using System.Collections.Generic;
using System.Linq;
using Crayon.ParseTree;

namespace Crayon
{
    internal class InterpreterCompiler
    {
        // Order in which the files are compiled.
        private static readonly string[] FILES = new string[] {

            // These 3 must go first
            "Structs.cry",
            "Constants.cry",
            "Globals.cry",

            // These are just piles of functions so they can be compiled in any order.
            "BinaryOpsUtil.cry",
            "ByteCodeLoader.cry",
            "Interpreter.cry",
            "MetadataInitializer.cry",
            "PrimitiveMethods.cry",
            "ResourceManager.cry",
            "Runner.cry",
            "TypesUtil.cry",
            "ValueUtil.cry",
        };

        private AbstractPlatform platform;
        private Parser interpreterParser;

        public InterpreterCompiler(AbstractPlatform platform, SystemLibraryManager sysLibMan)
        {
            this.platform = platform;
            this.interpreterParser = new Parser(platform, null, sysLibMan);
        }

        public Dictionary<string, Executable[]> Compile()
        {
            bool isPastel = this.platform != null && this.platform.PlatformId == PlatformId.PASTEL_VM;

            Dictionary<string, Executable[]> output = new Dictionary<string, Executable[]>();

            Dictionary<string, string> replacements = this.BuildReplacementsDictionary();

            Dictionary<string, string> filesById = new Dictionary<string, string>();

            Dictionary<string, string> libraryProducedFiles = new Dictionary<string, string>();
            if (!isPastel)
            {
                libraryProducedFiles = this.interpreterParser.SystemLibraryManager.GetSupplementalTranslationFiles();
            }

            List<string> orderedFileIds = new List<string>();

            foreach (string file in FILES)
            {
                string fileId = file.Split('.')[0];
                string code = Util.ReadInterpreterFileInternally(file);
                filesById[fileId] = code;
                orderedFileIds.Add(fileId);
            }

            if (isPastel)
            {
                string[] interpreterFiles = typeof(Interpreter.InterpreterAssembly).Assembly.GetManifestResourceNames();
                foreach (string interpreterFile in interpreterFiles.Where<string>(s => s.EndsWith(".cry")))
                {
                    string path = interpreterFile.Substring("Interpreter.".Length);
                    path = path.Substring(0, path.Length - 4); // trim off .cry
                    path = path.Replace('.', '/');
                    path = path + ".cry"; // put it back

                    if (path.Contains("call_lib_function")) // This is the one with the dynamically generated switch statement using %%% that's syntactically incorrect.
                    {
                        continue;
                    }
                    string content = Util.ReadInterpreterFileInternally(path);
                    string key = path.Substring(0, path.Length - 4); // trim .cry
                    if (!filesById.ContainsKey(key))
                    {
                        filesById[key] = content;
                        orderedFileIds.Add(key);
                    }
                }
            }

            foreach (string fileId in libraryProducedFiles.Keys)
            {
                filesById[fileId] = libraryProducedFiles[fileId];
                orderedFileIds.Add(fileId);
            }

            foreach (string fileId in orderedFileIds)
            {
                string code = Constants.DoReplacements(true, filesById[fileId], replacements);

                Executable[] lines = this.interpreterParser.ParseInterpreterCode(fileId, code);
                if (lines.Length > 0)
                {
                    output[fileId] = lines.ToArray();
                }
            }

            string switchLookupCode = this.interpreterParser.GetSwitchLookupCode().Trim();
            if (switchLookupCode.Length > 0)
            {
                output["SwitchLookups"] = this.interpreterParser.ParseInterpreterCode("SwitchLookups.cry", switchLookupCode);
            }

            return output;
        }

        private Dictionary<string, string> replacementsDictionary = null;

        public Dictionary<string, string> BuildReplacementsDictionary()
        {
            if (replacementsDictionary == null)
            {
                AbstractPlatform platform = this.interpreterParser.NullablePlatform;
                if (platform != null && platform.PlatformId == PlatformId.PASTEL_VM)
                {
                    replacementsDictionary = new Dictionary<string, string>();
                    return replacementsDictionary;
                }

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
                this.replacementsDictionary = replacements;
            }
            return this.replacementsDictionary;
        }

        public StructDefinition[] GetStructDefinitions()
        {
            return this.interpreterParser.GetStructDefinitions();
        }
    }
}
