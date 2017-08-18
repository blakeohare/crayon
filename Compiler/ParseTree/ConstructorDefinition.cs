﻿using System;
using System.Collections.Generic;
using System.Linq;
using Common;

namespace Crayon.ParseTree
{
    internal class ConstructorDefinition : Executable
    {
        public int FunctionID { get; private set; }
        public Executable[] Code { get; set; }
        public Token[] ArgNames { get; private set; }
        public Expression[] DefaultValues { get; private set; }
        public Expression[] BaseArgs { get; private set; }
        public Token BaseToken { get; private set; }
        public int LocalScopeSize { get; set; }
        public int MinArgCount { get; set; }
        public int MaxArgCount { get; set; }
        public bool IsDefault { get; private set; }
        public Annotation PrivateAnnotation { get; set; }

        public ConstructorDefinition(Executable owner) : base(null, owner)
        {
            this.IsDefault = true;

            this.Code = new Executable[0];
            this.ArgNames = new Token[0];
            this.DefaultValues = new Expression[0];
            this.BaseArgs = new Expression[0];
            this.MaxArgCount = 0;
            this.MinArgCount = 0;
        }

        public ConstructorDefinition(Token constructorToken, IList<Token> args, IList<Expression> defaultValues, IList<Expression> baseArgs, IList<Executable> code, Token baseToken, Executable owner)
            : base(constructorToken, owner)
        {
            this.IsDefault = false;
            this.ArgNames = args.ToArray();
            this.DefaultValues = defaultValues.ToArray();
            this.BaseArgs = baseArgs.ToArray();
            this.Code = code.ToArray();
            this.BaseToken = baseToken;

            TODO.VerifyDefaultArgumentsAreAtTheEnd();

            this.MaxArgCount = this.ArgNames.Length;
            int minArgCount = 0;
            for (int i = 0; i < this.ArgNames.Length; ++i)
            {
                if (this.DefaultValues[i] == null)
                {
                    minArgCount++;
                }
                else
                {
                    break;
                }
            }
            this.MinArgCount = minArgCount;
        }

        internal override IList<Executable> Resolve(Parser parser)
        {
            parser.ValueStackDepth = 0;

            this.FunctionID = parser.GetNextFunctionId();

            for (int i = 0; i < this.ArgNames.Length; ++i)
            {
                if (this.DefaultValues[i] != null)
                {
                    this.DefaultValues[i] = this.DefaultValues[i].Resolve(parser);
                }
            }

            for (int i = 0; i < this.BaseArgs.Length; ++i)
            {
                this.BaseArgs[i] = this.BaseArgs[i].Resolve(parser);
            }

            List<Executable> code = new List<Executable>();
            foreach (Executable line in this.Code)
            {
                code.AddRange(line.Resolve(parser));
            }
            this.Code = code.ToArray();

            return Listify(this);
        }

        internal void AllocateLocalScopeIds()
        {
            VariableIdAllocator variableIds = new VariableIdAllocator();
            for (int i = 0; i < this.ArgNames.Length; ++i)
            {
                variableIds.RegisterVariable(this.ArgNames[i].Value);
            }

            this.PerformLocalIdAllocation(variableIds, VariableIdAllocPhase.REGISTER_AND_ALLOC);

            this.LocalScopeSize = variableIds.Size;
        }

        internal override void PerformLocalIdAllocation(VariableIdAllocator varIds, VariableIdAllocPhase phase)
        {
            foreach (Expression arg in this.BaseArgs)
            {
                arg.PerformLocalIdAllocation(varIds, VariableIdAllocPhase.ALLOC);
            }

            foreach (Executable ex in this.Code)
            {
                ex.PerformLocalIdAllocation(varIds, VariableIdAllocPhase.REGISTER_AND_ALLOC);
            }
        }

        internal override Executable ResolveNames(Parser parser, Dictionary<string, Executable> lookup, string[] imports)
        {
            parser.CurrentCodeContainer = this;
            if (this.DefaultValues.Length > 0)
            {
                this.BatchExpressionNameResolver(parser, lookup, imports, this.DefaultValues);
            }

            if (this.BaseArgs.Length > 0)
            {
                this.BatchExpressionNameResolver(parser, lookup, imports, this.BaseArgs);
            }
            this.BatchExecutableNameResolver(parser, lookup, imports, this.Code);
            parser.CurrentCodeContainer = null;

            return this;
        }

        internal override void GetAllVariablesReferenced(HashSet<Variable> vars) { }

        internal override Executable PastelResolve(Parser parser)
        {
            throw new System.NotImplementedException();
        }
    }
}
