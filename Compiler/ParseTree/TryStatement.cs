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
            TopLevelConstruct owner) : base(tryToken, owner)
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
            this.FinallyBlock = finallyBlock == null ? new Executable[0] : finallyBlock.ToArray();

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

        internal override IList<Executable> Resolve(ParserContext parser)
        {
            this.ValueStackDepth = parser.ValueStackDepth;

            this.TryBlock = Executable.Resolve(parser, this.TryBlock).ToArray();
            foreach (CatchBlock cb in this.CatchBlocks)
            {
                cb.Code = Executable.Resolve(parser, cb.Code).ToArray();
            }
            this.FinallyBlock = Executable.Resolve(parser, this.FinallyBlock).ToArray();

            return this.TryBlock.Length == 0
                ? this.FinallyBlock // No try? The finally then just functions as normal unwrapped code.
                : Listify(this);
        }

        internal override Executable ResolveNames(ParserContext parser)
        {
            this.BatchExecutableNameResolver(parser, this.TryBlock);

            Common.TODO.HardCodedEnglishValue();
            ClassDefinition simpleException = Node.DoClassLookup(this.Owner, null, "Core.Exception");

            foreach (CatchBlock cb in this.CatchBlocks)
            {
                string[] types = cb.Types;
                Token[] typeTokens = cb.TypeTokens;
                int typeCount = types.Length;
                cb.TypeClasses = new ClassDefinition[typeCount];
                for (int i = 0; i < typeCount; ++i)
                {
                    Common.TODO.HardCodedEnglishValue();
                    string typeName = types[i] ?? "Core.Exception";
                    Token token = typeTokens[i] ?? cb.CatchToken;
                    ClassDefinition resolvedType = Node.DoClassLookup(this.Owner, token, typeName, true);
                    if (resolvedType == null)
                    {
                        throw new ParserException(token, "Could not resolve class name for catch.");
                    }
                    if (!resolvedType.ExtendsFrom(simpleException))
                    {
                        if (resolvedType.BaseClass == null && resolvedType.Library == null)
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
                        Common.TODO.HardCodedEnglishValue();
                        types[0] = "Core.Exception";
                        cb.ExceptionVariableToken = token;
                        typeTokens[0] = null;
                        --i; // and then try resolving again.
                    }
                }

                /*
                    TODO: throw an error or warning if an exception catch is followed by a more specific catch.
                    e.g.
                    try { ... }
                    catch (Exception) { ... }
                    catch (InvalidArgumentException) { ... }
                */

                this.BatchExecutableNameResolver(parser, cb.Code);
            }

            if (this.FinallyBlock != null) this.BatchExecutableNameResolver(parser, this.FinallyBlock);

            return this;
        }

        internal override void PerformLocalIdAllocation(ParserContext parser, VariableIdAllocator varIds, VariableIdAllocPhase phase)
        {
            foreach (Executable ex in this.TryBlock)
            {
                ex.PerformLocalIdAllocation(parser, varIds, phase);
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
                    ex.PerformLocalIdAllocation(parser, varIds, phase);
                }
            }

            if (this.FinallyBlock != null)
            {
                foreach (Executable ex in this.FinallyBlock)
                {
                    ex.PerformLocalIdAllocation(parser, varIds, phase);
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

        internal override Executable PastelResolve(ParserContext parser)
        {
            throw new System.NotImplementedException();
        }
    }
}
