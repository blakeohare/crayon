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
            throw new NotImplementedException();
        }
    }
}
