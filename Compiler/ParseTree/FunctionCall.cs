using System;
using System.Collections.Generic;
using System.Linq;

namespace Crayon.ParseTree
{
    internal class FunctionCall : Expression
    {
        internal override Expression PastelResolve(Parser parser)
        {
            for (int i = 0; i < this.Args.Length; ++i)
            {
                this.Args[i] = this.Args[i].PastelResolve(parser);
            }

            Variable fp = this.Root as Variable;
            if (fp == null)
            {
                throw new NotImplementedException();
            }
            string name = fp.Name;
            if (name.StartsWith("$"))
            {
                return new SystemFunctionCall(this.FirstToken, this.Args, this.FunctionOrClassOwner).PastelResolve(parser);
            }
            return this;
        }

        public override bool CanAssignTo { get { return false; } }

        public Expression Root { get; private set; }
        public Token ParenToken { get; private set; }
        public Expression[] Args { get; private set; }

        public FunctionCall(Expression root, Token parenToken, IList<Expression> args, Executable owner)
            : base(root.FirstToken, owner)
        {
            this.Root = root;
            this.ParenToken = parenToken;
            this.Args = args.ToArray();
        }

        internal override Expression Resolve(Parser parser)
        {
            for (int i = 0; i < this.Args.Length; ++i)
            {
                this.Args[i] = this.Args[i].Resolve(parser);
            }

            if (this.Root is Variable)
            {
                string varName = ((Variable)this.Root).Name;

                if (parser.IsTranslateMode && varName.StartsWith("$"))
                {
                    return new SystemFunctionCall(this.Root.FirstToken, this.Args, this.FunctionOrClassOwner).Resolve(parser);
                }

                if (parser.GetClass(varName) != null)
                {
                    throw new ParserException(this.ParenToken, "Cannot invoke a class like a function. To construct a new class, the \"new\" keyword must be used.");
                }
            }

            this.Root = this.Root.Resolve(parser);

            // TODO: this is hardcoded just for Math.floor(numeric constant). Eventually, it'd be nice
            // for a few common functions to have a compile-time codepath here.
            // e.g. Core.parseInt, Math.sin, etc.
            if (this.Root is FunctionReference && this.Args.Length == 1)
            {
                FunctionDefinition funcDef = ((FunctionReference)this.Root).FunctionDefinition;
                if (funcDef.LibraryName == "Math" && funcDef.NameToken.Value == "floor")
                {
                    Expression arg0 = this.Args[0];
                    if (arg0 is IntegerConstant)
                    {
                        int integerValue = ((IntegerConstant)arg0).Value;
                        return new IntegerConstant(this.FirstToken, integerValue, this.FunctionOrClassOwner);
                    }

                    if (arg0 is FloatConstant)
                    {
                        double floatValue = ((FloatConstant)arg0).Value;
                        int integerValue = (int)floatValue;
                        return new IntegerConstant(this.FirstToken, integerValue, this.FunctionOrClassOwner);
                    }
                }
            }

            if (this.Root is SpecialEntity)
            {
                if (this.Root is SpecialEntity.EnumMaxFunction)
                {
                    int max = ((SpecialEntity.EnumMaxFunction)this.Root).GetMax();
                    return new IntegerConstant(this.Root.FirstToken, max, this.FunctionOrClassOwner);
                }

                if (this.Root is SpecialEntity.EnumValuesFunction)
                {
                    int[] rawValues = ((SpecialEntity.EnumValuesFunction)this.Root).GetValues();
                    List<Expression> values = new List<Expression>();
                    foreach (int rawValue in rawValues)
                    {
                        values.Add(new IntegerConstant(this.Root.FirstToken, rawValue, this.FunctionOrClassOwner));
                    }
                    return new ListDefinition(this.FirstToken, values, this.FunctionOrClassOwner);
                }
            }

            return this;
        }

        internal override void PerformLocalIdAllocation(VariableIdAllocator varIds, VariableIdAllocPhase phase)
        {
            if ((phase & VariableIdAllocPhase.ALLOC) != 0)
            {
                this.Root.PerformLocalIdAllocation(varIds, phase);
                foreach (Expression arg in this.Args)
                {
                    arg.PerformLocalIdAllocation(varIds, phase);
                }
            }
        }

        internal override Expression ResolveNames(Parser parser, Dictionary<string, Executable> lookup, string[] imports)
        {
            if (this.Root is Variable && ((Variable)this.Root).Name.Contains("$$$"))
            {
                this.BatchExpressionNameResolver(parser, lookup, imports, this.Args);

                return new CoreFunctionInvocation(this.FirstToken, this.Args, this.FunctionOrClassOwner);
            }

            this.Root = this.Root.ResolveNames(parser, lookup, imports);
            this.BatchExpressionNameResolver(parser, lookup, imports, this.Args);

            if (this.Root is LibraryFunctionReference)
            {
                return new LibraryFunctionCall(
                    this.FirstToken,
                    ((LibraryFunctionReference)this.Root).Name,
                    this.Args,
                    this.FunctionOrClassOwner);
            }

            if (this.Root is SystemFunctionReference)
            {
                return new SystemFunctionCall(this.FirstToken, this.Args, this.FunctionOrClassOwner);
            }

            if (this.Root is DotStep ||
                this.Root is Variable ||
                this.Root is FieldReference ||
                this.Root is FunctionReference ||
                this.Root is BracketIndex ||
                this.Root is BaseMethodReference)
            {
                return this;
            }

            if (this.Root is IConstantValue)
            {
                if (this.Args.Length == 1 && this.Args[0] is BinaryOpChain)
                {
                    throw new ParserException(this.ParenToken, "Constants cannot be invoked like functions. Although it sort of looks like you're missing an op here.");
                }
                throw new ParserException(this.ParenToken, "Constants cannot be invoked like functions.");
            }

            if (this.Root is ClassReference)
            {
                throw new ParserException(this.Root.FirstToken, "Classes cannot be invoked like a function. If you meant to instantiate a new instance, use the 'new' keyword.");
            }

            throw new ParserException(this.ParenToken, "This cannot be invoked like a function.");
        }

        internal override void GetAllVariablesReferenced(HashSet<Variable> vars)
        {
            // Don't check the root. This is a function definition, but looks like a variable.
            // this.Root.GetAllVariablesReferenced(vars);

            foreach (Expression arg in this.Args)
            {
                arg.GetAllVariablesReferenced(vars);
            }
        }
    }
}
