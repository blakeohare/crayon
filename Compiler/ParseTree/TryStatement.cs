using System;
using System.Collections.Generic;
using System.Linq;

namespace Crayon.ParseTree
{
    internal class TryStatement : Executable
    {
        public Token TryToken { get; set; }
        public Token FinallyToken { get; set; }
        public Executable[] TryBlock { get; set; }
        public CatchBlock[] CatchBlocks { get; set; }
        public Executable[] FinallyBlock { get; set; }
        public int ValueStackDepth { get; set; }

        public class CatchBlock
        {
            public Token CatchToken { get; set; }
            public Token[] TypeTokens { get; set; }
            public string[] Types { get; set; }
            public Token ExceptionVariableToken { get; set; }
            public int VariableLocalScopeId { get; set; }
            public Executable[] Code { get; set; }
            public ClassDefinition[] TypeClasses { get; set; }
        }

        // pardon the double-plurals for the jaggy 2D arrays
        public TryStatement(
            Token tryToken,
            IList<Executable> tryBlock,
            List<Token> catchTokens,
            List<Token> exceptionVariableTokens,
            List<Token[]> catchBlockTypeTokenses, // tricksy tokenses
            List<string[]> catchBlockTypeses,
            List<Executable[]> catchBlockExecutableses,
            Token finallyToken,
            IList<Executable> finallyBlock,
            Executable owner) : base(tryToken, owner)
        {
            this.TryToken = tryToken;
            this.TryBlock = tryBlock.ToArray();

            int catchBlockCount = catchTokens.Count; // individual catch-related inputs are trusted to all have same length
            this.CatchBlocks = new CatchBlock[catchBlockCount];
            for (int i = 0; i < catchBlockCount; ++i)
            {
                Token catchToken = catchTokens[i];
                Token variableName = exceptionVariableTokens[i];
                Token[] catchBlockTypeTokens = catchBlockTypeTokenses[i];
                string[] catchBlockTypes = catchBlockTypeses[i];
                Executable[] catchBlockExecutables = catchBlockExecutableses[i];

                this.CatchBlocks[i] = new CatchBlock()
                {
                    CatchToken = catchToken,
                    Code = catchBlockExecutables,
                    ExceptionVariableToken = variableName,
                    Types = catchBlockTypes,
                    TypeTokens = catchBlockTypeTokens,
                    VariableLocalScopeId = -1,
                };
            }

            this.FinallyToken = finallyToken;
            this.FinallyBlock = finallyBlock == null ? null : finallyBlock.ToArray();

            if (this.CatchBlocks.Length == 0 && this.FinallyBlock == null)
            {
                throw new ParserException(this.TryToken, "Cannot have a try block without a catch or finally block.");
            }
        }

        private IEnumerable<Executable> GetAllExecutables()
        {
            IEnumerable<Executable> output = this.TryBlock;
            foreach (CatchBlock cb in this.CatchBlocks)
            {
                output = output.Concat<Executable>(cb.Code);
            }
            if (this.FinallyBlock != null)
            {
                output = output.Concat<Executable>(this.FinallyBlock);
            }
            return output;
        }

        internal override IList<Executable> Resolve(Parser parser)
        {
            this.ValueStackDepth = parser.ValueStackDepth;

            foreach (Executable ex in this.TryBlock)
            {
                ex.Resolve(parser);
            }

            foreach (CatchBlock cb in this.CatchBlocks)
            {
                foreach (Executable ex in cb.Code)
                {
                    ex.Resolve(parser);
                }
            }

            foreach (Executable ex in this.FinallyBlock)
            {
                ex.Resolve(parser);
            }

            return Listify(this);
        }
        
        internal override Executable ResolveNames(Parser parser, Dictionary<string, Executable> lookup, string[] imports)
        {
            this.BatchExecutableNameResolver(parser, lookup, imports, this.TryBlock);

            ClassDefinition simpleException = Node.DoClassLookup(null, lookup, imports, "Core.Exception");

            foreach (CatchBlock cb in this.CatchBlocks)
            {
                string[] types = cb.Types;
                Token[] typeTokens = cb.TypeTokens;
                int typeCount = types.Length;
                cb.TypeClasses = new ClassDefinition[typeCount];
                for (int i = 0; i < typeCount; ++i)
                {
                    string typeName = types[i] ?? "Core.Exception";
                    Token token = typeTokens[i] ?? cb.CatchToken;
                    ClassDefinition resolvedType = Node.DoClassLookup(token, lookup, imports, typeName, true);
                    if (!resolvedType.ExtendsFrom(simpleException))
                    {
                        if (resolvedType.BaseClass == null && resolvedType.LibraryName == null)
                        {
                            throw new ParserException(token, "This class does not extend from Core.Exception.");
                        }
                        else
                        {
                            throw new ParserException(token, "Only classes that extend from Core.Exception may be caught.");
                        }
                    }
                    cb.TypeClasses[i] = resolvedType;

                    // There's only one type, it doesn't resolve into a class, there's no variable, and there's no '.' in the type name.
                    if (resolvedType == null &&
                        typeCount == 1 &&
                        cb.ExceptionVariableToken == null &&
                        !typeName.Contains("."))
                    {
                        // ...that means this is not a type but is actually a variable.

                        // Change the type to "Core.Exception", move this token to a variable
                        types[0] = "Core.Exception";
                        cb.ExceptionVariableToken = token;
                        typeTokens[0] = null;
                        --i; // and then try resolving again.
                    }
                }

                this.BatchExecutableNameResolver(parser, lookup, imports, cb.Code);
            }

            if (this.FinallyBlock != null) this.BatchExecutableNameResolver(parser, lookup, imports, this.FinallyBlock);

            return this;
        }

        internal override void PerformLocalIdAllocation(VariableIdAllocator varIds, VariableIdAllocPhase phase)
        {
            foreach (Executable ex in this.TryBlock)
            {
                ex.PerformLocalIdAllocation(varIds, phase);
            }

            foreach (CatchBlock cb in this.CatchBlocks)
            {
                if (cb.ExceptionVariableToken != null)
                {
                    if ((phase & VariableIdAllocPhase.REGISTER) != 0)
                    {
                        varIds.RegisterVariable(cb.ExceptionVariableToken.Value);
                    }

                    if ((phase & VariableIdAllocPhase.ALLOC) != 0)
                    {
                        cb.VariableLocalScopeId = varIds.GetVarId(cb.ExceptionVariableToken);
                    }
                }

                foreach (Executable ex in cb.Code)
                {
                    ex.PerformLocalIdAllocation(varIds, phase);
                }
            }

            if (this.FinallyBlock != null)
            {
                foreach (Executable ex in this.FinallyBlock)
                {
                    ex.PerformLocalIdAllocation(varIds, phase);
                }
            }
        }

        internal override void GetAllVariablesReferenced(HashSet<Variable> vars)
        {
            foreach (Executable ex in this.TryBlock.Concat(this.FinallyBlock))
            {
                ex.GetAllVariablesReferenced(vars);
            }

            foreach (CatchBlock cb in this.CatchBlocks)
            {
                foreach (Executable ex in cb.Code)
                {
                    ex.GetAllVariablesReferenced(vars);
                }
            }
        }
    }
}
