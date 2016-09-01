using System;
using System.Collections.Generic;

namespace Crayon.ParseTree
{
    internal class SystemFunctionCall : Expression
    {
        public override bool CanAssignTo { get { return false; } }
        public string Name { get; private set; }
        public Expression[] Args { get; private set; }
        public Library AssociatedLibrary { get; private set; }

        public SystemFunctionCall(Token token, Expression[] args, Executable owner)
            : base(token, owner)
        {
            this.Name = token.Value;
            this.Args = args;
        }

        internal override Expression Resolve(Parser parser)
        {
            for (int i = 0; i < this.Args.Length; ++i)
            {
                this.Args[i] = this.Args[i].Resolve(parser);
            }

            if (this.Name.StartsWith("$_lib_"))
            {
                string libraryName = this.Name.Split('_')[2];
                Library library = parser.SystemLibraryManager.GetLibraryFromKey(libraryName);
                if (library != null)
                {
                    this.AssociatedLibrary = library;
                }
            }

            if (this.Name == "$_has_increment")
            {
                bool hasIncrement = !parser.NullablePlatform.GetType().IsAssignableFrom(typeof(Crayon.Translator.Python.PythonPlatform));
                return new BooleanConstant(this.FirstToken, hasIncrement, this.FunctionOrClassOwner);
            }

            if (this.Name == "$_is_javascript")
            {
                bool isJavaScript = parser.NullablePlatform.GetType().IsAssignableFrom(typeof(Crayon.Translator.JavaScript.JavaScriptPlatform));
                return new BooleanConstant(this.FirstToken, isJavaScript, this.FunctionOrClassOwner);
            }

            if (this.Name == "$_ord")
            {
                if (this.Args[0] is StringConstant)
                {
                    string strValue = ((StringConstant)this.Args[0]).Value;
                    if (strValue.Length != 1) throw new ParserException(this.FirstToken, "$_ord must take in a string of length 1");
                    return new IntegerConstant(this.FirstToken, strValue[0], this.FunctionOrClassOwner);
                }
            }
            // args have already been resolved.
            return this;
        }

        internal override Expression ResolveNames(Parser parser, System.Collections.Generic.Dictionary<string, Executable> lookup, string[] imports)
        {
            this.BatchExpressionNameResolver(parser, lookup, imports, this.Args);
            return this;
        }

        internal override void PerformLocalIdAllocation(VariableIdAllocator varIds, VariableIdAllocPhase phase)
        {
            if ((phase & VariableIdAllocPhase.ALLOC) != 0)
            {
                foreach (Expression arg in this.Args)
                {
                    arg.PerformLocalIdAllocation(varIds, phase);
                }
            }
        }

        internal override void GetAllVariablesReferenced(HashSet<Variable> vars)
        {
            foreach (Expression ex in this.Args)
            {
                ex.GetAllVariablesReferenced(vars);
            }
        }
    }
}
