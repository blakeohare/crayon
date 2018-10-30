﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Pastel.Nodes
{
    internal class ConstructorInvocation : Expression
    {
        public PType Type { get; set; }
        public Expression[] Args { get; set; }
        public StructDefinition StructType { get; set; }

        public ConstructorInvocation(Token firstToken, PType type, IList<Expression> args) : base(firstToken)
        {
            this.Type = type;
            this.Args = args.ToArray();
            this.ResolvedType = type;
        }

        public override Expression ResolveNamesAndCullUnusedCode(PastelCompiler compiler)
        {
            throw new NotImplementedException();
        }

        internal override Expression ResolveType(VariableScope varScope, PastelCompiler compiler)
        {
            throw new NotImplementedException();
        }

        internal override Expression ResolveWithTypeContext(PastelCompiler compiler)
        {
            for (int i = 0; i < this.Args.Length; ++i)
            {
                this.Args[i] = this.Args[i].ResolveWithTypeContext(compiler);
            }

            string type = this.Type.RootValue;
            switch (type)
            {
                case "Array":
                case "List":
                case "Dictionary":
                    break;

                default:
                    StructDefinition sd = compiler.GetStructDefinition(this.Type.RootValue);
                    if (sd != null)
                    {
                        this.StructType = sd;
                        int fieldCount = this.StructType.ArgTypes.Length;
                        if (fieldCount != this.Args.Length)
                        {
                            throw new ParserException(this.FirstToken, "Incorrect number of args in constructor. Expected " + fieldCount + ", found " + this.Args.Length);
                        }

                        for (int i = 0; i < fieldCount; ++i)
                        {
                            PType actualType = this.Args[i].ResolvedType;
                            PType expectedType = this.StructType.ArgTypes[i];
                            if (!PType.CheckAssignment(expectedType, actualType))
                            {
                                throw new ParserException(this.Args[i].FirstToken, "Cannot use an arg of this type for this struct field. Expected " + expectedType.ToString() + " but found " + actualType.ToString());
                            }
                        }
                    }
                    break;
            }

            return this;
        }
    }
}
