using System;
using System.Collections.Generic;

namespace Crayon.ParseTree
{
    internal class DotStep : Expression
    {
        internal override Expression PastelResolve(Parser parser)
        {
            Variable root = this.Root as Variable;
            if (root != null)
            {
                string[] name = root.Name.Split('$');
                if (name.Length == 2)
                {
                    root = new Variable(root.FirstToken, name[1], root.FunctionOrClassOwner);
                    this.Root = root;
                }
                else if (name.Length == 1)
                {
                    // this is an enum, probably
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
            else
            {
                throw new NotImplementedException();
            }

            this.Root = this.Root.PastelResolve(parser);
            return this;
        }

        public override bool CanAssignTo { get { return true; } }

        public Expression Root { get; set; }
        public Token DotToken { get; private set; }
        public Token StepToken { get; private set; }

        public DotStep(Expression root, Token dotToken, Token stepToken, Executable owner)
            : base(root.FirstToken, owner)
        {
            this.Root = root;
            this.DotToken = dotToken;
            this.StepToken = stepToken;
        }

        internal override Expression Resolve(Parser parser)
        {
            this.Root = this.Root.Resolve(parser);

            string step = this.StepToken.Value;

            if (this.Root is EnumReference)
            {
                EnumDefinition enumDef = ((EnumReference)this.Root).EnumDefinition;

                ConstantResolutionState resolutionState = parser.ConstantAndEnumResolutionState[enumDef];
                if (resolutionState != ConstantResolutionState.RESOLVED)
                {
                    enumDef.Resolve(parser);
                }

                switch (step)
                {
                    case "length":
                        return new IntegerConstant(this.FirstToken, enumDef.IntValue.Count, this.FunctionOrClassOwner);
                    case "max":
                        return new SpecialEntity.EnumMaxFunction(this.FirstToken, enumDef, this.FunctionOrClassOwner);
                    case "values":
                        return new SpecialEntity.EnumValuesFunction(this.FirstToken, enumDef, this.FunctionOrClassOwner);
                }

                if (enumDef.IntValue.ContainsKey(step))
                {
                    return new IntegerConstant(this.FirstToken, enumDef.IntValue[step], this.FunctionOrClassOwner);
                }
                else
                {
                    throw new ParserException(this.StepToken, "The enum '" + enumDef.Name + "' does not contain a definition for '" + step + "'");
                }
            }

            if (this.Root is BaseKeyword)
            {
                return new BaseMethodReference(this.Root.FirstToken, this.DotToken, this.StepToken, this.FunctionOrClassOwner).Resolve(parser);
            }

            if (this.Root is StringConstant)
            {
                if (step == "join")
                {
                    throw new ParserException(this.StepToken,
                        "There is no join method on strings. Did you mean to do list.join(string) instead?");
                }
                else if (step == "size")
                {
                    throw new ParserException(this.StepToken, "String size is indicated by string.length.");
                }
                else if (step == "length")
                {
                    int length = ((StringConstant)this.Root).Value.Length;
                    return new IntegerConstant(this.FirstToken, length, this.FunctionOrClassOwner);
                }
            }

            return this;
        }

        internal override void PerformLocalIdAllocation(Parser parser, VariableIdAllocator varIds, VariableIdAllocPhase phase)
        {
            if ((phase & VariableIdAllocPhase.ALLOC) != 0)
            {
                this.Root.PerformLocalIdAllocation(parser, varIds, phase);
            }
        }

        internal override Expression ResolveNames(
            Parser parser,
            Dictionary<string, Executable> lookup,
            string[] imports)
        {
            FunctionDefinition funcDef; // used in multiple places.
            FieldDeclaration fieldDec;
            this.Root = this.Root.ResolveNames(parser, lookup, imports);
            Expression root = this.Root;
            string field = this.StepToken.Value;

            if (root is PartialNamespaceReference)
            {
                // already a fully qualified namespace, therefore imports don't matter.
                string fullyQualifiedName = ((PartialNamespaceReference)root).Name + "." + field;
                if (lookup.ContainsKey(fullyQualifiedName))
                {
                    return Resolver.ConvertStaticReferenceToExpression(lookup[fullyQualifiedName], this.FirstToken, this.FunctionOrClassOwner);
                }

                throw new ParserException(this.FirstToken, "Could not find class or function by the name of: '" + fullyQualifiedName + "'");
            }

            if (root is ClassReference)
            {
                ClassDefinition cd = ((ClassReference)root).ClassDefinition;

                funcDef = cd.GetMethod(field, false);
                if (funcDef != null)
                {
                    if (!funcDef.IsStaticMethod)
                    {
                        string className = cd.NameToken.Value;
                        string functionName = funcDef.NameToken.Value;

                        throw new ParserException(this.DotToken, "'" + className + "." + functionName + "' is not a static method, but it is being used as though it is static.");
                    }

                    return new FunctionReference(this.FirstToken, funcDef, this.FunctionOrClassOwner);
                }

                fieldDec = cd.GetField(field, false);
                if (fieldDec != null)
                {
                    if (!fieldDec.IsStaticField)
                    {
                        throw new ParserException(this.DotToken, "Cannot make a static reference to a non-static field.");
                    }

                    return new FieldReference(this.FirstToken, fieldDec, this.FunctionOrClassOwner);
                }

                if (field == "class")
                {
                    return new ClassReferenceLiteral(this.FirstToken, cd, this.FunctionOrClassOwner);
                }

                // TODO: nested classes, enums, constants

                // TODO: show spelling suggestions.
                throw new ParserException(this.StepToken, "No static fields or methods named '" + field + "' on the class " + cd.NameToken.Value + ".");
            }

            if (root is BaseKeyword)
            {
                ClassDefinition thisClass = null;
                if (this.FunctionOrClassOwner != null)
                {
                    if (this.FunctionOrClassOwner is FunctionDefinition)
                    {
                        thisClass = this.FunctionOrClassOwner.FunctionOrClassOwner as ClassDefinition;
                    }
                    else
                    {
                        thisClass = this.FunctionOrClassOwner as ClassDefinition;
                    }
                }

                if (thisClass == null)
                {
                    throw new ParserException(root.FirstToken, "'base' keyword can only be used inside classes.");
                }

                ClassDefinition cd = thisClass.BaseClass;
                if (cd == null)
                {
                    throw new ParserException(root.FirstToken, "'base' keyword can only be used inside classes that extend from another class.");
                }

                FunctionDefinition fd = cd.GetMethod(field, true);
                if (fd == null)
                {
                    throw new ParserException(this.DotToken, "Cannot find a method by that name in the base class chain.");
                }

                if (fd.IsStaticMethod)
                {
                    throw new ParserException(this.DotToken, "Cannot reference static methods using 'base' keyword.");
                }

                return new BaseMethodReference(this.FirstToken, this.DotToken, this.StepToken, this.FunctionOrClassOwner);
            }

            if (root is ThisKeyword)
            {
                ClassDefinition cd = null;
                if (this.FunctionOrClassOwner != null)
                {
                    if (this.FunctionOrClassOwner is FunctionDefinition)
                    {
                        funcDef = this.FunctionOrClassOwner as FunctionDefinition;
                        if (funcDef.IsStaticMethod)
                        {
                            throw new ParserException(this.Root.FirstToken, "'this' keyword cannot be used in static methods.");
                        }
                        cd = funcDef.FunctionOrClassOwner as ClassDefinition;
                        if (cd == null)
                        {
                            throw new ParserException(this.Root.FirstToken, "'this' keyword must be used inside a class.");
                        }
                    }
                    else if (this.FunctionOrClassOwner is ClassDefinition)
                    {
                        cd = (ClassDefinition)this.FunctionOrClassOwner;
                    }
                }

                if (cd == null)
                {
                    throw new ParserException(this.Root.FirstToken, "'this' keyword is not allowed here.");
                }

                funcDef = cd.GetMethod(field, true);
                if (funcDef != null)
                {
                    if (funcDef.IsStaticMethod)
                    {
                        throw new ParserException(this.DotToken, "This method is static and must be referenced by the class name, not 'this'.");
                    }
                    return new FunctionReference(this.FirstToken, funcDef, this.FunctionOrClassOwner);
                }

                FieldDeclaration fieldDef = cd.GetField(field, true);
                if (fieldDef != null)
                {
                    if (fieldDef.IsStaticField)
                    {
                        throw new ParserException(this.DotToken, "This field is static and must be referenced by the class name, not 'this'.");
                    }

                    return new FieldReference(this.FirstToken, fieldDef, this.FunctionOrClassOwner);
                }

                // TODO: show suggestions in the error message for anything close to what was typed.
                throw new ParserException(this.StepToken, "The class '" + cd.NameToken.Value + "' does not have a field named '" + field + "'.");
            }

            // This is done here in the resolver instead of the parser because some unallowed
            // field names (such as .class) are valid.
            if (this.StepToken.Value == parser.Keywords.CLASS)
            {
                if (this.Root is Variable)
                {
                    throw new ParserException(this.Root.FirstToken, "'" + ((Variable)this.Root).Name + "' is not a class.");
                }
                throw new ParserException(this.DotToken, ".class can only be applied to class names.");
            }
            parser.VerifyIdentifier(this.StepToken);

            return this;
        }

        internal override void GetAllVariablesReferenced(HashSet<Variable> vars)
        {
            // Does this get optimized out in translate mode?
            // If you run into this outside of translate mode go ahead and do a this.Root.GetAllVariablesReferenced(vars)
            throw new System.Exception();
        }
    }
}
