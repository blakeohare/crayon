using Parser.Resolver;
using System.Collections.Generic;
using System.Linq;

namespace Parser.ParseTree
{
    public class TryStatement : Executable
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
            public VariableId VariableLocalScopeId { get; set; }
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
            Node owner) : base(tryToken, owner)
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

                if (variableName == null)
                {
                    variableName = new Token(owner.GetNextAutoGenVarName(), TokenType.WORD, catchToken.File, catchToken.Line, catchToken.Col);
                }

                this.CatchBlocks[i] = new CatchBlock()
                {
                    CatchToken = catchToken,
                    Code = catchBlockExecutables,
                    ExceptionVariableToken = variableName,
                    Types = catchBlockTypes,
                    TypeTokens = catchBlockTypeTokens,
                    VariableLocalScopeId = null,
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
                output = output.Concat(cb.Code);
            }
            if (this.FinallyBlock != null)
            {
                output = output.Concat(this.FinallyBlock);
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

        internal override Executable ResolveEntityNames(ParserContext parser)
        {
            this.BatchExecutableEntityNameResolver(parser, this.TryBlock);

            Common.TODO.GetCoreNameFromMetadataWithLocale();
            string coreExceptionName = "Core.Exception";

            ClassDefinition simpleException = this.FileScope.DoClassLookup(this.Owner, null, coreExceptionName);

            foreach (CatchBlock cb in this.CatchBlocks)
            {
                string[] types = cb.Types;
                Token[] typeTokens = cb.TypeTokens;
                int typeCount = types.Length;
                cb.TypeClasses = new ClassDefinition[typeCount];
                for (int i = 0; i < typeCount; ++i)
                {
                    string typeName = types[i] ?? coreExceptionName;
                    Token token = typeTokens[i] ?? cb.CatchToken;
                    ClassDefinition resolvedType = this.FileScope.DoClassLookup(this.Owner, token, typeName, true);
                    if (resolvedType == null)
                    {
                        throw new ParserException(token, "Could not resolve class name for catch.");
                    }
                    if (!resolvedType.ExtendsFrom(simpleException))
                    {
                        throw new ParserException(token, "Only classes that extend from Core.Exception may be caught.");
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
                        types[0] = coreExceptionName;
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

                this.BatchExecutableEntityNameResolver(parser, cb.Code);
            }

            if (this.FinallyBlock != null) this.BatchExecutableEntityNameResolver(parser, this.FinallyBlock);

            return this;
        }

        internal override void ResolveTypes(ParserContext parser, TypeResolver typeResolver)
        {
            foreach (Executable ex in this.TryBlock)
            {
                ex.ResolveTypes(parser, typeResolver);
            }

            foreach (CatchBlock cb in this.CatchBlocks)
            {
                // Exception catch blocks have a type declaration, even in Crayon.
                cb.VariableLocalScopeId.ResolvedType = typeResolver.ResolveType(cb.VariableLocalScopeId.Type);

                foreach (Executable ex in cb.Code)
                {
                    ex.ResolveTypes(parser, typeResolver);
                }
            }

            foreach (Executable ex in this.FinallyBlock)
            {
                ex.ResolveTypes(parser, typeResolver);
            }
        }

        internal override void ResolveVariableOrigins(ParserContext parser, VariableScope varIds, VariableIdAllocPhase phase)
        {
            foreach (Executable ex in this.TryBlock)
            {
                ex.ResolveVariableOrigins(parser, varIds, phase);
            }

            foreach (CatchBlock cb in this.CatchBlocks)
            {
                if (cb.ExceptionVariableToken != null)
                {
                    if ((phase & VariableIdAllocPhase.REGISTER) != 0)
                    {
                        // TODO: this is a little flawed. Should find the common base class.
                        AType exceptionType = cb.Types.Length == 1
                            ? AType.ProvideRoot(cb.TypeTokens[0], cb.Types[0])
                            : AType.ProvideRoot(cb.CatchToken, "Core.Exception");
                        varIds.RegisterVariable(exceptionType, cb.ExceptionVariableToken.Value);
                    }

                    if ((phase & VariableIdAllocPhase.ALLOC) != 0)
                    {
                        cb.VariableLocalScopeId = varIds.GetVarId(cb.ExceptionVariableToken);
                    }
                }

                foreach (Executable ex in cb.Code)
                {
                    ex.ResolveVariableOrigins(parser, varIds, phase);
                }
            }

            if (this.FinallyBlock != null)
            {
                foreach (Executable ex in this.FinallyBlock)
                {
                    ex.ResolveVariableOrigins(parser, varIds, phase);
                }
            }
        }
    }
}
