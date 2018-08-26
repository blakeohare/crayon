﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Parser.ParseTree
{
    public class LibraryFunctionCall : Expression
    {
        public override bool CanAssignTo { get { return false; } }

        public string Name { get; private set; }
        public Expression[] Args { get; private set; }
        public string LibraryName { get; set; }

        public LibraryFunctionCall(Token token, string name, IList<Expression> args, Node owner)
            : base(token, owner)
        {
            string callingLibrary = null;
            while (callingLibrary == null && owner != null)
            {
                if (owner is TopLevelConstruct && !(owner is FieldDeclaration))
                {
                    callingLibrary = owner.Library.ID;
                }
                owner = owner.Owner;
            }

            if (callingLibrary == null)
            {
                throw new ParserException(this, "Cannot call native library functions from outside a library.");
            }

            this.LibraryName = callingLibrary;

            string expectedPrefix = "lib_" + callingLibrary.ToLower() + "_";
            if (!name.StartsWith(expectedPrefix))
            {
                throw new ParserException(this, "Invalid library function name. Must begin with a '$$" + expectedPrefix + "' prefix.");
            }
            this.Name = name;
            this.Args = args.ToArray();
        }

        internal override Expression Resolve(ParserContext parser)
        {
            for (int i = 0; i < this.Args.Length; ++i)
            {
                this.Args[i] = this.Args[i].Resolve(parser);
            }
            return this;
        }

        internal override Expression ResolveEntityNames(ParserContext parser)
        {
            throw new InvalidOperationException(); // this class is generated on the general resolve pass.
        }

        internal override void PerformLocalIdAllocation(ParserContext parser, VariableScope varIds, VariableIdAllocPhase phase)
        {
            if ((phase & VariableIdAllocPhase.ALLOC) != 0)
            {
                foreach (Expression ex in this.Args)
                {
                    ex.PerformLocalIdAllocation(parser, varIds, phase);
                }
            }
        }
    }
}
