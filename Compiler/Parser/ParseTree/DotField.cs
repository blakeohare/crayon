using Parser.Resolver;
using System.Collections.Generic;

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

        internal override IEnumerable<Expression> Descendants { get { return new Expression[] { this.Root }; } }

        internal override Expression Resolve(ParserContext parser)
        {
            this.Root = this.Root.Resolve(parser);

            string step = this.StepToken.Value;

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

            if (this.Root is EnumReference)
            {
                EnumDefinition enumDef = ((EnumReference)this.Root).EnumDefinition;

                if (field == parser.Keywords.FIELD_ENUM_LENGTH)
                    return new IntegerConstant(this.FirstToken, enumDef.IntValue.Count, this.Owner);
                if (field == parser.Keywords.FIELD_ENUM_MAX)
                    return new SpecialEntity.EnumMaxFunction(this.FirstToken, enumDef, this.Owner);
                if (field == parser.Keywords.FIELD_ENUM_VALUES)
                    return new SpecialEntity.EnumValuesFunction(this.FirstToken, enumDef, this.Owner);

                return new EnumFieldReference(this.FirstToken, enumDef, this.StepToken, this.Owner);
            }

            // This is done here in the resolver instead of the parser because some unallowed
            // field names (such as .class) are valid.
            if (field == parser.Keywords.CLASS)
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

        private static readonly ResolvedType[] EMPTY_TYPE_LIST = new ResolvedType[0];

        internal override Expression ResolveTypes(ParserContext parser, TypeResolver typeResolver)
        {
            this.Root.ResolveTypes(parser, typeResolver);

            string field = this.StepToken.Value;

            if (this.Root is EnumReference)
            {
                throw new System.NotImplementedException();
            }

            ResolvedType rootType = this.Root.ResolvedType;

            // TODO: all of this needs to be localized.
            switch (rootType.Category)
            {
                case ResolvedTypeCategory.NULL:
                    throw new ParserException(this.DotToken, "Cannot dereference a field from null.");

                case ResolvedTypeCategory.ANY:
                    // ¯\_(ツ)_/¯
                    this.ResolvedType = ResolvedType.ANY;
                    return this;

                case ResolvedTypeCategory.INTEGER:
                    throw new ParserException(this.DotToken, "Integers do not have any fields.");

                case ResolvedTypeCategory.FLOAT:
                    throw new ParserException(this.DotToken, "Floating point decimals do not have any fields.");

                case ResolvedTypeCategory.BOOLEAN:
                    throw new ParserException(this.DotToken, "Booleans do not have any fields.");

                case ResolvedTypeCategory.STRING:
                    switch (field)
                    {
                        case "length":
                            this.ResolvedType = ResolvedType.INTEGER;
                            return this;

                        case "contains": return BuildPrimitiveMethod(ResolvedType.BOOLEAN, ResolvedType.STRING);
                        case "endsWith": return BuildPrimitiveMethod(ResolvedType.BOOLEAN, ResolvedType.STRING);
                        case "indexOf": return BuildPrimitiveMethod(ResolvedType.INTEGER, ResolvedType.STRING);
                        case "lower": return BuildPrimitiveMethod(ResolvedType.STRING);
                        case "ltrim": return BuildPrimitiveMethod(ResolvedType.STRING);
                        case "replace": return BuildPrimitiveMethod(ResolvedType.STRING, ResolvedType.STRING, ResolvedType.STRING);
                        case "reverse": return BuildPrimitiveMethod(ResolvedType.STRING);
                        case "rtrim": return BuildPrimitiveMethod(ResolvedType.STRING);
                        case "split": return BuildPrimitiveMethod(ResolvedType.ListOrArrayOf(ResolvedType.STRING), ResolvedType.STRING);
                        case "startsWith": return BuildPrimitiveMethod(ResolvedType.BOOLEAN, ResolvedType.STRING);
                        case "trim": return BuildPrimitiveMethod(ResolvedType.STRING);
                        case "upper": return BuildPrimitiveMethod(ResolvedType.STRING);

                        default:
                            throw new ParserException(this.DotToken, "Strings do not have that method.");
                    }

                case ResolvedTypeCategory.LIST:
                    ResolvedType itemType = rootType.ListItemType;
                    switch (field)
                    {
                        case "length":
                            this.ResolvedType = ResolvedType.INTEGER;
                            return this;

                        case "add": return BuildPrimitiveMethod(ResolvedType.VOID, itemType);
                        case "choice": return BuildPrimitiveMethod(ResolvedType.VOID);
                        case "clear": return BuildPrimitiveMethod(ResolvedType.VOID);
                        case "clone": return BuildPrimitiveMethod(rootType);
                        case "concat": return BuildPrimitiveMethod(rootType, rootType);
                        case "contains": return BuildPrimitiveMethod(ResolvedType.BOOLEAN, itemType);
                        case "filter": throw new System.NotImplementedException(); // defer resolution to match arg
                        case "insert": return BuildPrimitiveMethod(ResolvedType.VOID, ResolvedType.INTEGER, itemType);
                        case "join": return BuildPrimitiveMethodWithOptionalArgs(ResolvedType.STRING, 1, ResolvedType.STRING);
                        case "map": throw new System.NotImplementedException(); // defer resolution to match arg
                        case "pop": return BuildPrimitiveMethod(itemType);
                        case "remove": return BuildPrimitiveMethod(ResolvedType.VOID, ResolvedType.INTEGER);
                        case "reverse": return BuildPrimitiveMethod(ResolvedType.VOID);
                        case "shuffle": return BuildPrimitiveMethod(ResolvedType.VOID);
                        case "sort": return BuildPrimitiveMethod(ResolvedType.VOID);

                        default:
                            throw new ParserException(this.DotToken, "Lists do not have that method.");
                    }

                case ResolvedTypeCategory.DICTIONARY:
                    ResolvedType keyType = rootType.DictionaryKeyType;
                    ResolvedType valueType = rootType.DictionaryValueType;
                    switch (field)
                    {
                        case "length":
                            this.ResolvedType = ResolvedType.INTEGER;
                            return this;

                        case "clear": return BuildPrimitiveMethod(ResolvedType.VOID);
                        case "clone": return BuildPrimitiveMethod(rootType);
                        case "contains": return BuildPrimitiveMethod(ResolvedType.BOOLEAN, valueType);
                        case "get": return BuildPrimitiveMethodWithOptionalArgs(valueType, 1, keyType, valueType);
                        case "keys": return BuildPrimitiveMethod(ResolvedType.ListOrArrayOf(keyType));
                        case "merge": return BuildPrimitiveMethod(ResolvedType.VOID, rootType);
                        case "remove": return BuildPrimitiveMethod(ResolvedType.VOID, keyType);
                        case "values": return BuildPrimitiveMethod(ResolvedType.ListOrArrayOf(valueType));

                        default:
                            throw new ParserException(this.DotToken, "Dictionaries do not have that field.");
                    }

                case ResolvedTypeCategory.CLASS_DEFINITION:
                    throw new System.NotImplementedException();

                case ResolvedTypeCategory.FUNCTION_POINTER:
                    throw new System.NotImplementedException();

                case ResolvedTypeCategory.INSTANCE:
                    FieldDefinition fieldDef = rootType.ClassTypeOrReference.GetField(field, true);
                    if (fieldDef != null)
                    {
                        this.ResolvedType = fieldDef.ResolvedFieldType;
                        return this;
                    }
                    FunctionDefinition funcDef = rootType.ClassTypeOrReference.GetMethod(field, true);
                    if (funcDef != null)
                    {
                        this.ResolvedType = ResolvedType.GetFunctionType(funcDef);
                        return this;
                    }
                    throw new ParserException(this.DotToken, "The class '" + rootType.ClassTypeOrReference.NameToken.Value + "' does not have a field called '" + field + "'.");

                default:
                    throw new System.NotImplementedException();
            }
        }

        private Expression BuildPrimitiveMethod(ResolvedType returnType, params ResolvedType[] argTypes)
        {
            return BuildPrimitiveMethodWithOptionalArgs(returnType, 0, argTypes);
        }

        private Expression BuildPrimitiveMethodWithOptionalArgs(ResolvedType returnType, int optionalCount, params ResolvedType[] argTypes)
        {
            return new PrimitiveMethodReference(this.Root, this.StepToken, ResolvedType.GetFunctionType(returnType, argTypes, optionalCount), this.Owner);
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
