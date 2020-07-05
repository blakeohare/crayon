using Parser.Resolver;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Parser.ParseTree
{
    internal abstract class Executable : Node
    {
        public static readonly Executable[] EMPTY_ARRAY = new Executable[0];

        public Executable(Token firstToken, Node owner)
            : base(firstToken, owner)
        {
            if (owner == null)
            {
                throw new Exception(); // This should never happen.
            }
        }

        public virtual bool IsTerminator { get { return false; } }

        internal abstract Executable ResolveEntityNames(ParserContext parser);
        internal abstract void ResolveTypes(ParserContext parser, TypeResolver typeResolver);
        internal abstract IList<Executable> Resolve(ParserContext parser);
        internal abstract void ResolveVariableOrigins(ParserContext parser, VariableScope varIds, VariableIdAllocPhase phase);

        internal static IList<Executable> Resolve(ParserContext parser, IList<Executable> executables)
        {
            List<Executable> output = new List<Executable>();
            foreach (Executable executable in executables)
            {
                output.AddRange(executable.Resolve(parser));
            }
            return output;
        }

        internal static Executable[] EnsureBlockReturns(Executable[] codeBlock, Node owner)
        {
            if (codeBlock.Length > 0)
            {
                Executable last = codeBlock[codeBlock.Length - 1];
                if (last is ReturnStatement || last is ThrowStatement)
                {
                    return codeBlock;
                }
            }
            List<Executable> mutable = new List<Executable>(codeBlock);
            mutable.Add(new ReturnStatement(null, null, owner));
            return mutable.ToArray();
        }

        // The reason why I still run this function with actuallyDoThis = false is so that other platforms can still be exported to
        // and potentially crash if the implementation was somehow broken on Python (or some other future platform that doesn't have traditional switch statements).
        internal static Executable[] RemoveBreaksForElifedSwitch(bool actuallyDoThis, IList<Executable> executables)
        {
            List<Executable> newCode = new List<Executable>(executables);
            if (newCode.Count == 0) throw new Exception("A switch statement contained a case that had no code and a fallthrough.");
            if (newCode[newCode.Count - 1] is BreakStatement)
            {
                newCode.RemoveAt(newCode.Count - 1);
            }
            else if (newCode[newCode.Count - 1] is ReturnStatement)
            {
                // that's okay.
            }
            else
            {
                throw new Exception("A switch statement contained a case with a fall through.");
            }

            foreach (Executable executable in newCode)
            {
                if (executable is BreakStatement)
                {
                    throw new Exception("Can't break out of case other than at the end.");
                }
            }

            return actuallyDoThis ? newCode.ToArray() : executables.ToArray();
        }

        protected static IList<Executable> Listify(Executable ex)
        {
            return new Executable[] { ex };
        }
    }
}
