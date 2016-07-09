using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Crayon.ParseTree
{
    internal class TryStatement : Executable
    {
        public Token TryToken { get; set; }
        public Token CatchToken { get; set; }
        public Token FinallyToken { get; set; }
        public Token ExceptionToken { get; set; }
        public Executable[] TryBlock { get; set; }
        public Executable[] CatchBlock { get; set; }
        public Executable[] FinallyBlock { get; set; }
        public int ExceptionVariableLocalScopeId { get; set; }

        public TryStatement(
            Token tryToken, 
            IList<Executable> tryBlock, 
            Token catchToken,
            Token exceptionVariableToken,
            IList<Executable> catchBlock,
            Token finallyToken,
            IList<Executable> finallyBlock,
            Executable owner) : base(tryToken, owner)
        {
            this.TryToken = tryToken;
            this.CatchToken = catchToken;
            this.FinallyToken = finallyToken;
            this.ExceptionToken = exceptionVariableToken;
            this.TryBlock = tryBlock.ToArray();
            this.CatchBlock = catchBlock == null ? null : catchBlock.ToArray();
            this.FinallyBlock = finallyBlock == null ? null : finallyBlock.ToArray();

            if (this.CatchBlock == null && this.FinallyBlock == null)
            {
                throw new ParserException(this.TryToken, "Cannot have a try block without a catch or finally block.");
            }
        }

        private IEnumerable<Executable> GetAllExecutables()
        {
            IEnumerable<Executable> output = this.TryBlock;
            if (this.CatchBlock != null)
            {
                output = output.Concat<Executable>(this.CatchBlock);
            }
            if (this.FinallyBlock != null)
            {
                output = output.Concat<Executable>(this.FinallyBlock);
            }
            return output;
        }

        internal override void CalculateLocalIdPass(VariableIdAllocator varIds)
        {
            if (this.ExceptionToken != null)
            {
                varIds.RegisterVariable(this.ExceptionToken.Value);
            }

            foreach (Executable ex in this.GetAllExecutables())
            {
                ex.CalculateLocalIdPass(varIds);
            }
        }

        internal override void GenerateGlobalNameIdManifest(VariableIdAllocator varIds)
        {
            if (this.ExceptionToken != null)
            {
                varIds.RegisterVariable(this.ExceptionToken.Value);
            }

            foreach (Executable ex in this.GetAllExecutables())
            {
                ex.GenerateGlobalNameIdManifest(varIds);
            }
        }

        internal override IList<Executable> Resolve(Parser parser)
        {
            List<Executable> builder = new List<Executable>();
            foreach (Executable ex in this.TryBlock)
            {
                builder.AddRange(ex.Resolve(parser));
            }
            this.TryBlock = builder.ToArray();
            
            if (this.CatchBlock != null)
            {
                builder.Clear();
                foreach (Executable ex in this.CatchBlock)
                {
                    builder.AddRange(ex.Resolve(parser));
                }
                this.CatchBlock = builder.ToArray();
            }

            if (this.FinallyBlock != null)
            {
                builder.Clear();
                foreach (Executable ex in this.FinallyBlock)
                {
                    builder.AddRange(ex.Resolve(parser));
                }
                this.FinallyBlock = builder.ToArray();
            }

            return Listify(this);
        }

        internal override Executable ResolveNames(Parser parser, Dictionary<string, Executable> lookup, string[] imports)
        {
            this.BatchExecutableNameResolver(parser, lookup, imports, this.TryBlock);
            if (this.CatchBlock != null) this.BatchExecutableNameResolver(parser, lookup, imports, this.CatchBlock);
            if (this.FinallyBlock != null) this.BatchExecutableNameResolver(parser, lookup, imports, this.FinallyBlock);

            return this;
        }

        internal override void SetLocalIdPass(VariableIdAllocator varIds)
        {
            if (this.ExceptionToken != null)
            {
                this.ExceptionVariableLocalScopeId = varIds.GetVarId(this.ExceptionToken, false);
            }

            foreach (Executable ex in this.GetAllExecutables())
            {
                ex.SetLocalIdPass(varIds);
            }
        }

        internal override void GetAllVariablesReferenced(HashSet<Variable> vars)
        {
            foreach (Executable ex in this.TryBlock.Concat(this.CatchBlock).Concat(this.FinallyBlock))
            {
                ex.GetAllVariablesReferenced(vars);
            }
        }
    }
}
