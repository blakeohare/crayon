using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crayon.ParseTree;

namespace Crayon.Translator.Pastel
{
    class PastelPlatform : AbstractPlatform
    {
        public class PastelShouldNeverEverAccessThisException : Exception
        {
            public PastelShouldNeverEverAccessThisException(string message) : base(message) { }
        }

        public PastelPlatform() : base(PlatformId.PASTEL_VM, LanguageId.PASTEL, new PastelTranslator(), new PastelSystemFunctionTranslator())
        {

        }

        public override bool IntIsFloor { get { throw new PastelShouldNeverEverAccessThisException("IntIsFloor"); } }
        public override bool IsArraySameAsList { get { throw new PastelShouldNeverEverAccessThisException("IsArraySameAsList"); } }
        public override bool IsAsync { get { throw new PastelShouldNeverEverAccessThisException("IsAsync"); } }
        public override bool IsCharANumber { get { throw new PastelShouldNeverEverAccessThisException("IsCharANumber"); } }
        public override bool IsStronglyTyped { get { throw new PastelShouldNeverEverAccessThisException("IsStronglyTyped"); } }
        public override bool IsThreadBlockingAllowed { get { throw new PastelShouldNeverEverAccessThisException("IsThreadBlockingAllowed"); } }
        public override string PlatformShortId { get { throw new PastelShouldNeverEverAccessThisException("pastel"); } }
        public override bool SupportsListClear { get { throw new PastelShouldNeverEverAccessThisException("SupportsListClear"); } }

        public override Dictionary<string, FileOutput> Package(
            BuildContext buildContext,
            string projectId,
            Dictionary<string, Executable[]> finalCode,
            ICollection<StructDefinition> structDefinitions,
            ResourceDatabase resourceDatabase,
            SystemLibraryManager libraryManager)
        {
            Dictionary<string, FileOutput> output = new Dictionary<string, FileOutput>();

            foreach (string vmFile in finalCode.Keys)
            {
                if (vmFile == "Globals")
                {
                    foreach (Assignment aGlobal in finalCode[vmFile].OfType<Assignment>())
                    {
                        aGlobal.HACK_IsVmGlobal = true;
                    }
                }

                List<string> text = new List<string>();
                output[vmFile + ".pst"] = new FileOutput()
                {
                    Type = FileOutputType.Text,
                    TextContent = this.Translator.Translate(finalCode[vmFile])
                };
            }

            return output;
        }
    }
}
