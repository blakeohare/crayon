using Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Parser.ParseTree
{
    public class SwitchStatement : Executable
    {
        private Chunk[] chunks;

        public Chunk[] Chunks { get { return this.chunks; } }

        public bool UsesStrings { get; private set; }
        public bool UsesIntegers { get { return !this.UsesStrings; } }
        public bool ContainsDefault { get; private set; }
        public bool IsContinuous { get; private set; }
        public bool IsSafe { get; private set; }

        public class Chunk
        {
            public Token CaseOrDefaultToken { get; private set; }
            public Expression[] Cases { get; set; }
            public Executable[] Code { get; set; }
            public bool ContainsFallthrough { get; private set; }
            public int ID { get; private set; }

            public Chunk(int id, Token firstToken, IList<Expression> cases, IList<Executable> code)
            {
                this.CaseOrDefaultToken = firstToken;
                this.Cases = cases.ToArray();
                this.Code = code.ToArray();
                this.ID = id;
                this.ContainsFallthrough = this.Code.Length == 0 || !this.Code[this.Code.Length - 1].IsTerminator;
            }
        }

        public Expression Condition { get; private set; }
        // for platforms that don't have switch statements and require creative use of if/else statements to mapped ID's, you can explicitly declare the maximum integer value that will
        // ever occur as input in the condition. This allows you to create a list large enough to accomodate all values.
        // This is specifically used for the binary operator switch statement, which is a sparse lookup table defined as a list where most entries point to the default case.
        // The explicit max fills this list large enough to accomodate the maximum value beyond the largest valid lookup ID and maps them to default.
        private Expression explicitMax;
        private Token explicitMaxToken;

        public SwitchStatement(Token switchToken, Expression condition, List<Token> firstTokens, List<List<Expression>> cases, List<List<Executable>> code, Expression explicitMax, Token explicitMaxToken, TopLevelConstruct owner)
            : base(switchToken, owner)
        {
            if (cases.Count == 0) throw new ParserException(switchToken, "Switch statement needs cases.");
            if (code.Count == 0) throw new ParserException(switchToken, "Switch statement needs code.");
            if (cases[0] == null) throw new ParserException(switchToken, "Switch statement must start with a case.");
            if (cases[cases.Count - 1] != null) throw new ParserException(switchToken, "Last case in switch statement is empty.");

            this.Condition = condition;
            this.explicitMax = explicitMax;
            this.explicitMaxToken = explicitMaxToken;

            List<Chunk> chunks = new List<Chunk>();
            int counter = 0;
            for (int i = 0; i < cases.Count; i += 2)
            {
                if (cases[i] == null) throw new Exception("This should not happen.");
                if (code[i + 1] == null) throw new Exception("This should not happen.");
                Chunk chunk = new Chunk(counter++, firstTokens[i], cases[i], code[i + 1]);
                if (chunk.Code.Length > 0 && chunk.ContainsFallthrough)
                {
                    throw new ParserException(firstTokens[i], "This switch statement case contains code, but falls through to the next case. Cases that contain code must end with a return or break statement. Alternatively, you may just have mismatched curly braces somewhere.");
                }
                chunks.Add(chunk);
            }
            this.chunks = chunks.ToArray();

            if (this.chunks.Length == 1 && this.chunks[0].Cases.Length == 1 && this.chunks[0].Cases[0] == null)
            {
                throw new ParserException(switchToken, "Switches need at least 1 case to indicate type.");
            }
        }

        internal override IList<Executable> Resolve(ParserContext parser)
        {
            TODO.MakeSwitchStatementFallthroughsErrors();
            if (this.explicitMax != null)
            {
                throw new ParserException(this.explicitMaxToken, "Unexpected token: '{'");
            }

            bool useExplicitMax = this.explicitMax != null;
            int explicitMax = 0;
            if (useExplicitMax)
            {
                this.explicitMax = this.explicitMax.Resolve(parser);
                if (!(this.explicitMax is IntegerConstant)) throw new ParserException(this.explicitMax.FirstToken, "Explicit max must be an integer.");
                explicitMax = ((IntegerConstant)this.explicitMax).Value;
            }

            this.Condition = this.Condition.Resolve(parser);
            int integers = 0;
            int strings = 0;
            int largestSpan = 0;

            HashSet<int> intCases = new HashSet<int>();
            HashSet<string> stringCases = new HashSet<string>();

            foreach (Chunk chunk in this.chunks)
            {
                if (chunk.Cases.Length > largestSpan) largestSpan = chunk.Cases.Length;

                for (int i = 0; i < chunk.Cases.Length; ++i)
                {
                    if (chunk.Cases[i] != null)
                    {
                        Expression caseExpr = chunk.Cases[i].Resolve(parser);
                        chunk.Cases[i] = caseExpr;

                        if (caseExpr is IntegerConstant)
                        {
                            int intValue = ((IntegerConstant)caseExpr).Value;
                            if (intCases.Contains(intValue))
                            {
                                throw new ParserException(caseExpr.FirstToken, "Duplicate case value in same switch: " + intValue);
                            }
                            intCases.Add(intValue);
                        }
                        else if (caseExpr is StringConstant)
                        {
                            string strValue = ((StringConstant)caseExpr).Value;
                            if (stringCases.Contains(strValue))
                            {
                                throw new ParserException(caseExpr.FirstToken, "Duplicate case value in same switch: " + Util.ConvertStringValueToCode(strValue));
                            }
                            stringCases.Add(strValue);
                        }

                        if (chunk.Cases[i] is IntegerConstant) integers++;
                        else if (chunk.Cases[i] is StringConstant) strings++;
                        else
                        {
                            if (chunk.Cases[i] is DotStep)
                            {
                                // Since most enums are in all caps, offer a more helpful error message when there's a dot followed by all caps.
                                string field = ((DotStep)chunk.Cases[i]).StepToken.Value;
                                if (field.ToUpperInvariant() == field)
                                {
                                    throw new ParserException(chunk.Cases[i].FirstToken,
                                        "Only strings, integers, and enums can be used in a switch statement. It looks like this is probably supposed to be an enum. Make sure that it is spelled correctly.");
                                }
                            }
                            throw new ParserException(chunk.Cases[i].FirstToken, "Only strings, integers, and enums can be used in a switch statement.");
                        }
                    }
                }

                chunk.Code = Resolve(parser, chunk.Code).ToArray();
            }

            if (integers != 0 && strings != 0)
            {
                throw new ParserException(this.FirstToken, "Cannot mix string and integer cases in a single switch statement.");
            }

            if (integers == 0 && strings == 0)
            {
                if (this.chunks.Length == 0) throw new ParserException(this.FirstToken, "Cannot have a blank switch statement.");
                if (this.chunks.Length > 1) throw new Exception("only had default but had multiple chunks. This should have been prevented at parse-time.");
                return this.chunks[0].Code;
            }

            Chunk lastChunk = this.chunks[this.chunks.Length - 1];
            Expression lastCase = lastChunk.Cases[lastChunk.Cases.Length - 1];

            this.ContainsDefault = lastCase == null;
            this.UsesStrings = strings != 0;
            this.IsSafe = !this.ContainsDefault;
            this.IsContinuous = integers != 0 && largestSpan == 1 && this.IsSafe && this.DetermineContinuousness();

            return this.CompilationResolution(parser);
        }

        internal override Executable ResolveEntityNames(ParserContext parser)
        {
            this.Condition = this.Condition.ResolveEntityNames(parser);
            foreach (Chunk chunk in this.Chunks)
            {
                this.BatchExpressionEntityNameResolver(parser, chunk.Cases);
                this.BatchExecutableEntityNameResolver(parser, chunk.Code);
            }
            return this;
        }

        internal IList<Executable> CompilationResolution(ParserContext parser)
        {
            Chunk lastChunk = this.chunks[this.chunks.Length - 1];
            bool lastChunkContainsNull = false;
            foreach (Expression c in lastChunk.Cases)
            {
                if (c == null)
                {
                    lastChunkContainsNull = true;
                    break;
                }
            }

            if (lastChunkContainsNull)
            {
                lastChunk.Cases = new Expression[] { null };
            }

            return Listify(this);
        }

        private bool DetermineContinuousness()
        {
            HashSet<int> integersUsed = new HashSet<int>();
            bool first = true;
            int min = 0;
            int max = 0;
            foreach (Chunk chunk in this.chunks)
            {
                if (chunk.Cases.Length != 1) throw new Exception("This should have been filtered out by the largestSpane check earlier.");
                IntegerConstant c = chunk.Cases[0] as IntegerConstant;
                if (c == null) throw new Exception("How did this happen?");
                int num = c.Value;
                integersUsed.Add(num);
                if (first)
                {
                    first = false;
                    min = num;
                    max = num;
                }
                else
                {
                    if (num < min) min = num;
                    if (num > max) max = num;
                }
            }

            int expectedTotal = max - min + 1;
            return expectedTotal == integersUsed.Count;
        }

        public override bool IsTerminator
        {
            get
            {
                foreach (Chunk chunk in this.chunks)
                {
                    Executable exec = chunk.Code[chunk.Code.Length - 1];
                    if (!exec.IsTerminator || exec is BreakStatement)
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        internal override void PerformLocalIdAllocation(ParserContext parser, VariableIdAllocator varIds, VariableIdAllocPhase phase)
        {
            this.Condition.PerformLocalIdAllocation(parser, varIds, phase);

            if (phase != VariableIdAllocPhase.REGISTER_AND_ALLOC || this.chunks.Length <= 1)
            {
                foreach (Chunk chunk in this.chunks)
                {
                    foreach (Executable ex in chunk.Code)
                    {
                        ex.PerformLocalIdAllocation(parser, varIds, phase);
                    }
                }
            }
            else
            {
                VariableIdAllocator[] varIdBranches = new VariableIdAllocator[this.chunks.Length];
                for (int i = 0; i < this.chunks.Length; ++i)
                {
                    Chunk chunk = this.chunks[i];
                    varIdBranches[i] = varIds.Clone();
                    foreach (Executable ex in chunk.Code)
                    {
                        ex.PerformLocalIdAllocation(parser, varIdBranches[i], phase);
                    }
                }

                varIds.MergeClonesBack(varIdBranches);

                for (int i = 0; i < this.chunks.Length; ++i)
                {
                    Chunk chunk = this.chunks[i];
                    varIdBranches[i] = varIds.Clone();
                    foreach (Executable ex in chunk.Code)
                    {
                        ex.PerformLocalIdAllocation(parser, varIdBranches[i], VariableIdAllocPhase.ALLOC);
                    }
                }
            }
        }
    }
}
