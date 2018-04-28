using Pastel.Nodes;

namespace Pastel
{
    public class ExtensibleFunction
    {
        public string Name { get; set; }
        public PType ReturnType { get; set; }
        public PType[] ArgTypes { get; set; }

        // A string to swap out in the transpilation.
        // Transpiled arguments are represented as [ARG:1], [ARG:2], etc...
        // Null is okay when initializing the PastelContext, but at translation time,
        // all null Translations must be optimized out.
        public string Translation { get; set; }
    }
}
