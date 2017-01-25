using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crayon.ParseTree;

namespace Crayon.Translator.C
{
    internal class COpenGlPlatform : AbstractPlatform
    {
        public COpenGlPlatform()
            : base(PlatformId.C_OPENGL, LanguageId.C, new CTranslator(), new CSystemFunctionTranslator())
        { }

        public override bool IntIsFloor { get { return true; } }
        public override bool IsArraySameAsList { get { return false; } }
        public override bool IsAsync { get { return false; } }
        public override bool IsCharANumber { get { return true; } }
        public override bool IsStronglyTyped { get { return true; } }
        public override bool IsThreadBlockingAllowed { get { return true; } }
        public override string PlatformShortId { get { return "game-c-opengl"; } }
        public override bool SupportsListClear { get { return true; } }

        public override Dictionary<string, FileOutput> Package(
            BuildContext buildContext,
            string projectId,
            Dictionary<string, Executable[]> finalCode,
            ICollection<StructDefinition> structDefinitions,
            ResourceDatabase resourceDatabase,
            SystemLibraryManager libraryManager)
        {
            Dictionary<string, FileOutput> output = new Dictionary<string, FileOutput>();
            Dictionary<string, string> replacements = new Dictionary<string, string>();
            List<string> mainDotC = new List<string>();
            mainDotC.Add(GetCCode("Headers.txt", replacements));
            mainDotC.Add(GetCCode("String.txt", replacements));
            mainDotC.Add(GetCCode("List.txt", replacements));
            mainDotC.Add(GetCCode("DictInt.txt", replacements));
            mainDotC.Add(GetCCode("DictString.txt", replacements));
            mainDotC.Add(GetCCode("Main.txt", replacements));

            output["main.c"] = new FileOutput()
            {
                Type = FileOutputType.Text,
                TextContent = string.Join("\n", mainDotC),
            };

            return output;
        }

        private string GetCCode(string file, Dictionary<string, string> replacements)
        {
            string cCode = Util.ReadResourceFileInternally("game-c-opengl/" + file);
            cCode = Constants.DoReplacements(cCode, replacements);
            return cCode;
        }
    }
}
