using Parser.Resolver;

namespace Parser.ParseTree
{
    public class DotField : Expression
    {
        public override bool CanAssignTo { get { return true; } }

        public Expression Root { get; set; }
        public Token DotToken { get; private set; }
        public Token StepToken { get; private set; }

        public DotField(Expression root, Token dotToken, Token stepToken, Node owner)
            : base(root.FirstToken, owner)
        {
            this.Root = root;
            this.DotToken = dotToken;
            this.StepToken = stepToken;
        }

        internal override Expression Resolve(ParserContext parser)
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

                if (step == parser.Keywords.FIELD_ENUM_LENGTH)
                    return new IntegerConstant(this.FirstToken, enumDef.IntValue.Count, this.Owner);
                if (step == parser.Keywords.FIELD_ENUM_MAX)
                    return new SpecialEntity.EnumMaxFunction(this.FirstToken, enumDef, this.Owner);
                if (step == parser.Keywords.FIELD_ENUM_VALUES)
                    return new SpecialEntity.EnumValuesFunction(this.FirstToken, enumDef, this.Owner);

                if (enumDef.IntValue.ContainsKey(step))
                {
                    return new IntegerConstant(this.FirstToken, enumDef.IntValue[step], this.Owner);
                }
                else
                {
                    throw new ParserException(this.StepToken, "The enum '" + enumDef.Name + "' does not contain a definition for '" + step + "'");
                }
            }

            if (this.Root is BaseKeyword)
            {
                return new BaseMethodReference(this.Root.FirstToken, this.DotToken, this.StepToken, this.Owner).Resolve(parser);
            }

            if (this.Root is StringConstant)
            {
                if (step == "length")
                {
                    int length = ((StringConstant)this.Root).Value.Length;
                    return new IntegerConstant(this.FirstToken, length, this.Owner);
                }

                // TODO: the field name against the primitive methods. Also against other primitive types
                // and do so in a localization friendly way.
                if (parser.CurrentLocale.ID == "en")
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
                }
            }

            return this;
        }

        internal override Expression ResolveEntityNames(
            ParserContext parser)
        {
            FunctionDefinition funcDef; // used in multiple places.
            FieldDefinition fieldDec;
            this.Root = this.Root.ResolveEntityNames(parser);
            Expression root = this.Root;
            string field = this.StepToken.Value;

            if (root is NamespaceReference)
            {
                // already a fully qualified namespace, therefore imports don't matter.
                string fullyQualifiedName = ((NamespaceReference)root).Template.Name + "." + field;
                TopLevelEntity entity = this.Owner.FileScope.FileScopeEntityLookup.DoEntityLookup(fullyQualifiedName, parser.CurrentCodeContainer);
                if (entity != null)
                {
                    return ResolverPipeline.ConvertStaticReferenceToExpression(entity, this.FirstToken, this.Owner);
                }

                NamespaceReferenceTemplate nrt = this.Owner.FileScope.FileScopeEntityLookup.DoNamespaceLookup(fullyQualifiedName, parser.CurrentCodeContainer);
                if (nrt != null)
                {
                    return new NamespaceReference(this.FirstToken, this.Owner, nrt);
                }

                throw new ParserException(this, "Could not find class or function by the name of: '" + fullyQualifiedName + "'");
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

                    return new FunctionReference(this.FirstToken, funcDef, this.Owner);
                }

                fieldDec = cd.GetField(field, false);
                if (fieldDec != null)
                {
                    if (!fieldDec.IsStaticField)
                    {
                        throw new ParserException(this.DotToken, "Cannot make a static reference to a non-static field.");
                    }

                    return new FieldReference(this.FirstToken, fieldDec, this.Owner);
                }

                // TODO: typeof(class name) is less error prone with localization conflicts.
                // However there's a Core.typeOf() method that sort of conflicts.
                // TODO: if this notation is kept, then this needs to be split into two class keywords
                // since they do different things.
                if (field == parser.Keywords.CLASS)
                {
                    return new ClassReferenceLiteral(this.FirstToken, cd, this.Owner);
                }

                // TODO: nested classes, enums, constants

                // TODO: show spelling suggestions.
                throw new ParserException(this.StepToken, "No static fields or methods named '" + field + "' on the class " + cd.NameToken.Value + ".");
            }

            if (root is BaseKeyword)
            {
                ClassDefinition thisClass = null;
                if (this.Owner != null)
                {
                    if (this.Owner is FunctionDefinition)
                    {
                        thisClass = this.Owner.Owner as ClassDefinition;
                    }
                    else
                    {
                        thisClass = this.Owner as ClassDefinition;
                    }
                }

                if (thisClass == null)
                {
                    throw new ParserException(root, "'base' keyword can only be used inside classes.");
                }

                ClassDefinition cd = thisClass.BaseClass;
                if (cd == null)
                {
                    throw new ParserException(root, "'base' keyword can only be used inside classes that extend from another class.");
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

                return new BaseMethodReference(this.FirstToken, this.DotToken, this.StepToken, this.Owner);
            }

            if (root is ThisKeyword)
            {
                ClassDefinition cd = null;
                TopLevelEntity owner = this.TopLevelEntity;
                if (owner is FunctionDefinition)
                {
                    if (((FunctionDefinition)owner).IsStaticMethod)
                        throw new ParserException(this.Root, "'this' keyword cannot be used in static methods.");
                    cd = (ClassDefinition)owner.Owner;
                }
                else if (owner is FieldDefinition)
                {
                    if (((FieldDefinition)owner).IsStaticField)
                        throw new ParserException(this.Root, "'this' keyword cannot be used in static fields.");
                    cd = (ClassDefinition)owner.Owner;
                }
                else if (owner is ConstructorDefinition)
                {
                    cd = (ClassDefinition)owner.Owner;
                    if (cd.StaticConstructor == owner)
                        throw new ParserException(this.Root, "'this', keyword cannot be used in static constructors.");
                }
                else
                {
                    throw new ParserException(this.Root, "'this' keyword must be used inside a class.");
                }

                funcDef = cd.GetMethod(field, true);
                if (funcDef != null)
                {
                    if (funcDef.IsStaticMethod)
                    {
                        throw new ParserException(this.DotToken, "This method is static and must be referenced by the class name, not 'this'.");
                    }
                    return new FunctionReference(this.FirstToken, funcDef, this.Owner);
                }

                FieldDefinition fieldDef = cd.GetField(field, true);
                if (fieldDef != null)
                {
                    if (fieldDef.IsStaticField)
                    {
                        throw new ParserException(this.DotToken, "This field is static and must be referenced by the class name, not 'this'.");
                    }

                    return new FieldReference(this.FirstToken, fieldDef, this.Owner);
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
                    throw new ParserException(this.Root, "'" + ((Variable)this.Root).Name + "' is not a class.");
                }
                throw new ParserException(this.DotToken, ".class can only be applied to class names.");
            }
            parser.VerifyIdentifier(this.StepToken);

            return this;
        }

        internal override void ResolveTypes(ParserContext parser, TypeResolver typeResolver)
        {
            throw new System.NotImplementedException();
        }

        internal override void PerformLocalIdAllocation(ParserContext parser, VariableScope varIds, VariableIdAllocPhase phase)
        {
            if ((phase & VariableIdAllocPhase.ALLOC) != 0)
            {
                this.Root.PerformLocalIdAllocation(parser, varIds, phase);
            }
        }
    }
}
