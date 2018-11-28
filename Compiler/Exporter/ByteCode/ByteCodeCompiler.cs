using Exporter.ByteCode.Nodes;
using Localization;
using Parser;
using Parser.ParseTree;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Exporter.ByteCode
{
    internal class ByteCodeCompiler
    {
        public ByteBuffer GenerateByteCode(ParserContext parser, IList<TopLevelEntity> lines)
        {
            // This has to go first since it allocates the CNI function ID's
            ByteBuffer buildCniTable = this.BuildCniTable(parser);

            ByteBuffer userCode = new ByteBuffer();

            this.CompileTopLevelEntities(parser, userCode, lines);

            userCode.OptimizeJumps();

            ByteBuffer literalsTable = ByteBuffer.FromLiteralLookup(parser.LiteralLookup);

            ByteBuffer tokenData = this.BuildTokenData(userCode);

            ByteBuffer fileContent = this.BuildFileContent(parser.GetFilesById());

            ByteBuffer buildLibraryDeclarations = this.BuildLibraryDeclarations(parser);

            ByteBuffer header = new ByteBuffer();
            header.Concat(literalsTable);
            header.Concat(tokenData);
            header.Concat(fileContent);
            header.Concat(buildLibraryDeclarations);
            header.Concat(buildCniTable);

            // These contain data about absolute PC values. Once those are finalized, come back and fill these in.
            header.Add(null, OpCode.ESF_LOOKUP); // offsets to catch and finally blocks

            header.Add(null, OpCode.FINALIZE_INITIALIZATION, parser.BuildContext.ProjectID, parser.GetLocaleCount());

            // FINALIZE_INITIALIZATION sets the total number of locales and so this needs that information which is
            // why it's listed afterwards. TODO: please fix.
            ByteBuffer localeNameIdTable = this.BuildLocaleNameIdTable(parser);
            header.Concat(localeNameIdTable);

            ByteBuffer output = new ByteBuffer();
            output.Add(null, OpCode.USER_CODE_START, header.Size + 1);
            output.Concat(header);
            output.Concat(userCode);

            // artificially inject a function call to main() at the very end after all declarations are done.
            if (parser.MainFunctionHasArg)
            {
                output.Add(null, OpCode.COMMAND_LINE_ARGS);
                output.Add(null, OpCode.CALL_FUNCTION, (int)FunctionInvocationType.NORMAL_FUNCTION, 1, parser.MainFunction.FunctionID, 0, 0);
            }
            else
            {
                output.Add(null, OpCode.CALL_FUNCTION, (int)FunctionInvocationType.NORMAL_FUNCTION, 0, parser.MainFunction.FunctionID, 0, 0);
            }
            output.Add(null, OpCode.RETURN, 0);

            // artificially inject a function call to _LIB_CORE_invoke after the final return.
            // When the interpreter is invoked with a function pointer, simply pop the function pointer and a Value list of the args
            // onto the value stack and point the PC to opLength-2
            output.Add(null, OpCode.CALL_FUNCTION, (int)FunctionInvocationType.NORMAL_FUNCTION, 2, parser.CoreLibInvokeFunction.FunctionID, 0, 0);
            output.Add(null, OpCode.RETURN, 0);

            // Now that ops (and PCs) have been finalized, fill in ESF data with absolute PC's
            int[] esfOps = output.GetFinalizedEsfData();
            int esfPc = output.GetEsfPc();
            output.SetArgs(esfPc, esfOps);

            return output;
        }

        private ByteBuffer BuildCniTable(ParserContext parser)
        {
            int idAlloc = 1;
            ByteBuffer buffer = new ByteBuffer();
            foreach (CompilationScope scope in parser.GetAllCompilationScopes())
            {
                foreach (CniFunction cniFunction in scope.CniFunctionsByName.Keys.OrderBy(k => k).Select(k => scope.CniFunctionsByName[k]))
                {
                    cniFunction.ID = idAlloc++;
                    buffer.Add(null, OpCode.CNI_REGISTER, cniFunction.ByteCodeLookupKey, cniFunction.ID, cniFunction.ArgCount);
                }
            }
            return buffer;
        }

        private ByteBuffer BuildLibraryDeclarations(ParserContext parser)
        {
            ByteBuffer output = new ByteBuffer();

            int id = 1;
            foreach (CompilationScope scope in parser.AssemblyManager.ImportedAssemblyScopes)
            {
                List<string> descriptorComponents = new List<string>()
                {
                    scope.Metadata.ID,
                    scope.Metadata.Version,
                    scope.Metadata.CniStartupFunction ?? ""
                };
                string libraryDescriptor = string.Join(",", descriptorComponents);
                output.Add(null, OpCode.LIB_DECLARATION, libraryDescriptor, id++);
            }

            return output;
        }

        private ByteBuffer BuildLocaleNameIdTable(ParserContext parser)
        {
            ByteBuffer output = new ByteBuffer();
            Locale[] localesUsed = parser.GetAllUsedLocales();

            foreach (CompilationScope scope in parser.GetAllCompilationScopes())
            {
                Locale scopeLocale = scope.Locale;
                foreach (ClassDefinition cd in scope.GetAllClassDefinitions())
                {
                    this.BuildLocaleNameIdTableEntry(parser, output, cd, localesUsed, scopeLocale);
                }
            }

            return output;
        }

        private void BuildLocaleNameIdTableEntry(
            ParserContext parser,
            ByteBuffer output,
            ClassDefinition cd,
            Locale[] localesUsed,
            Locale classOriginalLocale)
        {
            // TODO: This would be so much easier if there was an interface. This is super goofy.
            List<TopLevelEntity> topLevelConstructs = new List<TopLevelEntity>();
            List<AnnotationCollection> tlcAnnotations = new List<AnnotationCollection>();
            List<string> tlcDefaultNames = new List<string>();
            List<int> memberIds = new List<int>();
            foreach (FieldDefinition field in cd.Fields)
            {
                if (field.Modifiers.HasStatic) continue;
                topLevelConstructs.Add(field);
                tlcAnnotations.Add(field.Annotations);
                tlcDefaultNames.Add(field.NameToken.Value);
                memberIds.Add(field.MemberID);
            }
            foreach (FunctionDefinition method in cd.Methods)
            {
                if (method.Modifiers.HasStatic) continue;
                topLevelConstructs.Add(method);
                tlcAnnotations.Add(method.Annotations);
                tlcDefaultNames.Add(method.NameToken.Value);
                memberIds.Add(method.MemberID);
            }

            // Apparently static members are getting allocated member ID's. They shouldn't be.
            // Adding this hack to get the total member count including static members.
            int effectiveMemberCount = topLevelConstructs.Count;
            int perceivedMemberCount = 0; // updated as members are encountered

            // Build a lookup of all the localized names by member ID by locale ID
            Dictionary<int, List<int>> nameIdByMemberIdByLocaleId = new Dictionary<int, List<int>>();

            for (int i = 0; i < effectiveMemberCount; ++i)
            {
                TopLevelEntity tlc = topLevelConstructs[i];
                int memberId = memberIds[i];
                perceivedMemberCount = Math.Max(memberId + 1, perceivedMemberCount);
                AnnotationCollection annotations = tlcAnnotations[i];
                string defaultName = tlcDefaultNames[i];
                Dictionary<Locale, string> localizedNames = annotations.GetNamesByLocale(1);
                localizedNames[classOriginalLocale] = defaultName;
                int defaultNameId = parser.LiteralLookup.GetNameId(defaultName);
                foreach (Locale locale in localesUsed)
                {
                    // If this locale isn't even used anywhere, don't bother adding it to the lookup.
                    int localeId = parser.GetLocaleId(locale);
                    if (localeId == -1) throw new Exception();
                    List<int> nameIdByMemberId = null;
                    if (!nameIdByMemberIdByLocaleId.ContainsKey(localeId))
                    {
                        nameIdByMemberId = new List<int>();
                        nameIdByMemberIdByLocaleId[localeId] = nameIdByMemberId;
                    }
                    else
                    {
                        nameIdByMemberId = nameIdByMemberIdByLocaleId[localeId];
                    }
                    string name = localizedNames.ContainsKey(locale) ? localizedNames[locale] : defaultName;
                    int nameId = parser.LiteralLookup.GetNameId(name);
                    while (nameIdByMemberId.Count <= memberId)
                    {
                        nameIdByMemberId.Add(-1); // there are some gaps due to static members.
                    }
                    nameIdByMemberId[memberId] = nameId;
                }
            }

            List<int> op = new List<int>();
            op.Add(cd.ClassID);
            op.Add(perceivedMemberCount);
            foreach (int localeId in nameIdByMemberIdByLocaleId.Keys.OrderBy(k => k))
            {
                op.Add(localeId);
                List<int> nameIdsByMemberId = nameIdByMemberIdByLocaleId[localeId];
                for (int i = 0; i < perceivedMemberCount; ++i)
                {
                    int nameId = -1;
                    if (i < nameIdsByMemberId.Count)
                    {
                        int localizedNameId = nameIdsByMemberId[i];
                        if (localizedNameId != -1)
                        {
                            nameId = localizedNameId;
                        }
                    }
                    op.Add(nameId);
                }
            }

            output.Add(null, OpCode.LOC_TABLE, op.ToArray());
        }

        private ByteBuffer BuildFileContent(string[] filesById)
        {
            ByteBuffer output = new ByteBuffer();

            for (int i = 0; i < filesById.Length; ++i)
            {
                output.Add(null, OpCode.DEF_ORIGINAL_CODE, filesById[i], i);
            }

            return output;
        }

        // Build the lookup table that maps PC's to tokens. There can be multiple tokens per PC, but it's up to the Op
        // to have a consistent convention to figure out the context of those tokens.
        private ByteBuffer BuildTokenData(ByteBuffer userCode)
        {
            Token[] tokens = userCode.ToTokenList().ToArray();

            int size = tokens.Length;
            ByteBuffer output = new ByteBuffer();
            // TODO: add command line flag for excluding token data. In that case, just return here.

            Token token;
            for (int i = 0; i < size; ++i)
            {
                token = tokens[i];

                if (token != null)
                {
                    output.Add(null, OpCode.TOKEN_DATA, i, token.Line, token.Col, token.FileID);
                }
            }

            return output;
        }

        public void CompileTopLevelEntities(ParserContext parser, ByteBuffer buffer, IList<TopLevelEntity> entities)
        {
            foreach (TopLevelEntity entity in entities)
            {
                this.CompileTopLevelEntity(parser, buffer, entity);
            }
        }

        public void CompileTopLevelEntity(ParserContext parser, ByteBuffer buffer, TopLevelEntity entity)
        {
            if (entity is FunctionDefinition) FunctionDefinitionEncoder.Compile(this, parser, buffer, (FunctionDefinition)entity, false);
            else if (entity is ClassDefinition) this.CompileClass(parser, buffer, (ClassDefinition)entity);
            else throw new NotImplementedException("Invalid target for byte code compilation");
        }

        public void Compile(ParserContext parser, ByteBuffer buffer, IList<Executable> executables)
        {
            foreach (Executable ex in executables)
            {
                this.Compile(parser, buffer, ex);
            }
        }

        public void Compile(ParserContext parser, ByteBuffer buffer, Executable line)
        {
            if (line is ExpressionAsExecutable) this.CompileExpressionAsExecutable(parser, buffer, (ExpressionAsExecutable)line);
            else if (line is Assignment) AssignmentEncoder.Compile(this, parser, buffer, (Assignment)line);
            else if (line is WhileLoop) WhileLoopEncoder.Compile(this, parser, buffer, (WhileLoop)line);
            else if (line is BreakStatement) BreakEncoder.Compile(parser, buffer, (BreakStatement)line);
            else if (line is ContinueStatement) ContinueEncoder.Compile(parser, buffer, (ContinueStatement)line);
            else if (line is ForLoop) ForLoopEncoder.Compile(this, parser, buffer, (ForLoop)line);
            else if (line is IfStatement) IfStatementEncoder.Compile(this, parser, buffer, (IfStatement)line);
            else if (line is ReturnStatement) ReturnEncoder.Compile(this, parser, buffer, (ReturnStatement)line);
            else if (line is SwitchStatement) SwitchStatementEncoder.Compile(this, parser, buffer, (SwitchStatement)line);
            else if (line is ForEachLoop) ForEachEncoder.Compile(this, parser, buffer, (ForEachLoop)line);
            else if (line is DoWhileLoop) DoWhileEncoder.Compile(this, parser, buffer, (DoWhileLoop)line);
            else if (line is TryStatement) TryStatementEncoder.Compile(this, parser, buffer, (TryStatement)line);
            else if (line is ThrowStatement) ThrowEncoder.Compile(this, parser, buffer, (ThrowStatement)line);
            else throw new NotImplementedException("Invalid target for byte code compilation");
        }

        private void CompileClass(ParserContext parser, ByteBuffer buffer, ClassDefinition classDefinition)
        {
            bool hasStaticFieldsWithStartingValues = classDefinition.Fields
                .Where(fd =>
                    fd.Modifiers.HasStatic &&
                    fd.DefaultValue != null &&
                    !(fd.DefaultValue is NullConstant))
                .Count() > 0;

            if (hasStaticFieldsWithStartingValues)
            {
                if (classDefinition.StaticConstructor == null)
                {
                    classDefinition.StaticConstructor = new ConstructorDefinition(null, ModifierCollection.CreateStaticModifier(classDefinition.FirstToken), new AnnotationCollection(parser), classDefinition);
                    classDefinition.StaticConstructor.ResolvePublic(parser);
                }

                List<Executable> staticFieldInitializers = new List<Executable>();
                foreach (FieldDefinition fd in classDefinition.Fields)
                {
                    if (fd.Modifiers.HasStatic && fd.DefaultValue != null && !(fd.DefaultValue is NullConstant))
                    {
                        FieldReference fieldRef = new FieldReference(fd.FirstToken, fd, fd);
                        fieldRef.ResolvedType = fd.ResolvedFieldType;
                        Executable assignment = new Assignment(
                            fieldRef,
                            null,
                            fd.NameToken,
                            Ops.EQUALS,
                            fd.DefaultValue,
                            fd);
                        staticFieldInitializers.Add(assignment);
                    }
                }

                staticFieldInitializers.AddRange(classDefinition.StaticConstructor.Code);
                classDefinition.StaticConstructor.Code = staticFieldInitializers.ToArray();
            }

            if (classDefinition.StaticConstructor != null)
            {
                // All static field initializers are added here.
                ConstructorDefinitionEncoder.Compile(this, parser, buffer, classDefinition.StaticConstructor, null);
            }

            foreach (FunctionDefinition fd in classDefinition.Methods)
            {
                int pc = buffer.Size;
                fd.FinalizedPC = pc;
                FunctionDefinitionEncoder.Compile(this, parser, buffer, fd, true);
            }

            int classId = classDefinition.ClassID;
            int baseClassId = classDefinition.BaseClass != null ? classDefinition.BaseClass.ClassID : -1;
            int nameId = parser.GetId(classDefinition.NameToken.Value);
            int constructorId = classDefinition.Constructor.FunctionID;
            int staticConstructorId = classDefinition.StaticConstructor != null ? classDefinition.StaticConstructor.FunctionID : -1;

            int staticFieldCount = classDefinition.Fields.Where<FieldDefinition>(fd => fd.Modifiers.HasStatic).Count();
            FieldDefinition[] regularFields = classDefinition.Fields.Where<FieldDefinition>(fd => !fd.Modifiers.HasStatic).ToArray();
            FunctionDefinition[] regularMethods = classDefinition.Methods.Where<FunctionDefinition>(fd => !fd.Modifiers.HasStatic).ToArray();
            List<int> members = new List<int>();
            List<FieldDefinition> fieldsWithComplexValues = new List<FieldDefinition>();
            foreach (FieldDefinition fd in regularFields)
            {
                int memberId = fd.MemberID;
                int fieldNameId = parser.GetId(fd.NameToken.Value);
                int initInstruction;
                int literalId = 0;

                // TODO: get rid of init instruction since everything is just 0 now.
                initInstruction = 0;
                literalId = parser.GetLiteralId(fd.DefaultValue);
                if (literalId == -1)
                {
                    literalId = parser.GetNullConstant();
                    fieldsWithComplexValues.Add(fd);
                }

                members.AddRange(new int[] {
                    0, // flag for field
                    memberId,
                    fieldNameId,
                    initInstruction,
                    literalId,
                    EncodeAccessModifier(fd.Modifiers)
                });
            }

            foreach (FunctionDefinition fd in regularMethods)
            {
                int memberId = fd.MemberID;
                int methodNameId = parser.GetId(fd.NameToken.Value);
                int functionId = fd.FunctionID;

                members.AddRange(new int[] {
                    1, // flag for method
                    memberId,
                    methodNameId,
                    functionId,
                    0, // ignored value.
                    EncodeAccessModifier(fd.Modifiers),
                });
            }

            ByteBuffer initializer = new ByteBuffer();

            if (fieldsWithComplexValues.Count > 0)
            {
                foreach (FieldDefinition complexField in fieldsWithComplexValues)
                {
                    this.CompileExpression(parser, initializer, complexField.DefaultValue, true);
                    initializer.Add(complexField.FirstToken, OpCode.ASSIGN_THIS_FIELD, complexField.MemberID);
                }
            }

            ConstructorDefinitionEncoder.Compile(this, parser, buffer, classDefinition.Constructor, initializer);

            List<int> args = new List<int>()
            {
                classId,
                baseClassId,
                nameId,
                constructorId,
                staticConstructorId,
                staticFieldCount,
                classDefinition.CompilationScope.ScopeNumId,
            };

            args.AddRange(members);

            string fullyQualifiedName = classDefinition.GetFullyQualifiedLocalizedName(parser.RootScope.Locale);

            buffer.Add(classDefinition.FirstToken, OpCode.CLASS_DEFINITION, fullyQualifiedName, args.ToArray());
            foreach (FieldDefinition fd in classDefinition.Fields)
            {
                if (fd.ResolvedFieldType != ResolvedType.ANY && !fd.Modifiers.HasStatic)
                {
                    List<int> typeArgs = new List<int>();
                    typeArgs.Add(classDefinition.ClassID);
                    typeArgs.Add(fd.MemberID);
                    // TODO: change the last boolean to an enum to include behavior for not including FLOAT/INT conversion info
                    CastEncoder.EncodeTypeInfoToIntBuffer(typeArgs, fd.ResolvedFieldType, false);
                    buffer.Add(null, OpCode.FIELD_TYPE_INFO, typeArgs.ToArray());
                }
            }
        }

        private static int EncodeAccessModifier(ModifierCollection modifiers)
        {
            if (modifiers.AccessModifierType == AccessModifierType.PUBLIC) return 1;
            if (modifiers.HasPrivate) return 2;
            if (modifiers.HasInternal || modifiers.HasProtected)
            {
                if (!modifiers.HasInternal) return 4;
                if (!modifiers.HasProtected) return 3;
                return 5;
            }
            throw new Exception(); // All cases should be covered by the above.
        }

        private void CompileExpressionAsExecutable(ParserContext parser, ByteBuffer buffer, ExpressionAsExecutable expr)
        {
            this.CompileExpression(parser, buffer, expr.Expression, false);
        }

        internal void CompileExpression(ParserContext parser, ByteBuffer buffer, Expression expr, bool outputUsed)
        {
            if (expr is FunctionCall) FunctionCallEncoder.Compile(this, parser, buffer, (FunctionCall)expr, outputUsed);
            else if (expr is IntegerConstant) ConstantEncoder.CompileInteger(parser, buffer, (IntegerConstant)expr, outputUsed);
            else if (expr is Variable) VariableEncoder.Compile(parser, buffer, (Variable)expr, outputUsed);
            else if (expr is BooleanConstant) ConstantEncoder.CompileBoolean(parser, buffer, (BooleanConstant)expr, outputUsed);
            else if (expr is DotField) DotFieldEncoder.Compile(this, parser, buffer, (DotField)expr, outputUsed);
            else if (expr is BracketIndex) BracketIndexEncoder.Compile(this, parser, buffer, (BracketIndex)expr, outputUsed);
            else if (expr is OpChain) OpChainEncoder.Compile(this, parser, buffer, (OpChain)expr, outputUsed);
            else if (expr is StringConstant) ConstantEncoder.CompileString(parser, buffer, (StringConstant)expr, outputUsed);
            else if (expr is NegativeSign) NegativeSignEncoder.Compile(this, parser, buffer, (NegativeSign)expr, outputUsed);
            else if (expr is ListDefinition) ListDefinitionEncoder.Compile(this, parser, buffer, (ListDefinition)expr, outputUsed);
            else if (expr is Increment) IncrementEncoder.Compile(this, parser, buffer, (Increment)expr, outputUsed);
            else if (expr is FloatConstant) ConstantEncoder.CompileFloat(parser, buffer, (FloatConstant)expr, outputUsed);
            else if (expr is NullConstant) ConstantEncoder.CompileNull(parser, buffer, (NullConstant)expr, outputUsed);
            else if (expr is ThisKeyword) ThisEncoder.Compile(parser, buffer, (ThisKeyword)expr, outputUsed);
            else if (expr is Instantiate) InstantiateEncoder.Compile(this, parser, buffer, (Instantiate)expr, outputUsed);
            else if (expr is DictionaryDefinition) DictionaryDefinitionEncoder.Compile(this, parser, buffer, (DictionaryDefinition)expr, outputUsed);
            else if (expr is BooleanCombination) BooleanCombinationEncoder.Compile(this, parser, buffer, (BooleanCombination)expr, outputUsed);
            else if (expr is BooleanNot) BooleanNotEncoder.Compile(this, parser, buffer, (BooleanNot)expr, outputUsed);
            else if (expr is Cast) CastEncoder.Compile(this, parser, buffer, (Cast)expr, outputUsed);
            else if (expr is Ternary) TernaryEncoder.Compile(this, parser, buffer, (Ternary)expr, outputUsed);
            else if (expr is ListSlice) ListSliceEncoder.Compile(this, parser, buffer, (ListSlice)expr, outputUsed);
            else if (expr is NullCoalescer) NullCoalescerEncoder.Compile(this, parser, buffer, (NullCoalescer)expr, outputUsed);
            else if (expr is BaseMethodReference) BaseMethodReferenceEncoder.Compile(parser, buffer, (BaseMethodReference)expr, outputUsed);
            else if (expr is FunctionReference) FunctionReferenceEncoder.Compile(parser, buffer, (FunctionReference)expr, outputUsed);
            else if (expr is FieldReference) FieldReferenceEncoder.Compile(parser, buffer, (FieldReference)expr, outputUsed);
            else if (expr is CoreFunctionInvocation) CoreFunctionInvocationEncoder.Compile(this, parser, buffer, (CoreFunctionInvocation)expr, null, null, outputUsed);
            else if (expr is IsComparison) IsComparisonEncoder.Compile(this, parser, buffer, (IsComparison)expr, outputUsed);
            else if (expr is ClassReferenceLiteral) ClassReferenceEncoder.Compile(parser, buffer, (ClassReferenceLiteral)expr, outputUsed);
            else if (expr is Lambda) LambdaEncoder.Compile(this, parser, buffer, (Lambda)expr, outputUsed);
            else if (expr is CniFunctionInvocation) CniFunctionInvocationEncoder.Compile(this, parser, buffer, (CniFunctionInvocation)expr, null, null, outputUsed);
            else if (expr is PrimitiveMethodReference) DotFieldEncoder.Compile(this, parser, buffer, (PrimitiveMethodReference)expr, outputUsed);

            // The following parse tree items must be removed before reaching the byte code encoder.
            else if (expr is BaseKeyword) this.CompileBaseKeyword(parser, buffer, (BaseKeyword)expr, outputUsed);
            else if (expr is CompileTimeDictionary) this.CompileCompileTimeDictionary((CompileTimeDictionary)expr);


            else throw new NotImplementedException();
        }

        internal static void EnsureUsed(Node item, bool outputUsed)
        {
            EnsureUsed(item.FirstToken, outputUsed);
        }

        internal static void EnsureUsed(Token token, bool outputUsed)
        {
            // TODO: maybe reword the expression to something along the lines of "use an if statement" if it's a ternary
            // expression in which case it actually does do something.

            if (!outputUsed)
            {
                throw new ParserException(token, "Cannot have this expression here. It does nothing. Did you mean to store this output into a variable or return it?");
            }
        }

        // TODO: this needs to go away in the resolver.
        private void CompileBaseKeyword(ParserContext parser, ByteBuffer buffer, BaseKeyword baseKeyword, bool outputUsed)
        {
            throw new ParserException(baseKeyword, "Cannot have a reference to 'base' without invoking a field.");
        }

        // TODO: this needs to go away during the resolver phase.
        private void CompileCompileTimeDictionary(CompileTimeDictionary compileTimeDictionary)
        {
            if (compileTimeDictionary.Type == "var")
            {
                throw new ParserException(compileTimeDictionary, "$var is a compile-time dictionary and must be dereferenced with a hardcoded string constant.");
            }
            throw new Exception(); // should not happen.
        }

        private void CompileLiteralStream(ParserContext parser, ByteBuffer buffer, IList<Expression> expressions, bool outputUsed)
        {
            if (expressions.Count == 1)
            {
                this.CompileExpression(parser, buffer, expressions[0], outputUsed);
            }
            else
            {
                ByteBuffer exprBuffer = new ByteBuffer();
                foreach (Expression expression in expressions)
                {
                    this.CompileExpression(parser, exprBuffer, expression, outputUsed);
                }

                // Add the literal ID arg from the above literals to the arg list of a LITERAL_STREAM
                buffer.Add(
                    expressions[0].FirstToken,
                    OpCode.LITERAL_STREAM,
                    exprBuffer.ToIntList().Reverse<int[]>().Select<int[], int>(args => args[1]).ToArray());
            }
        }

        private const int EXPR_STREAM_OTHER = 1;
        private const int EXPR_STREAM_LITERAL = 2;

        public void CompileExpressionList(ParserContext parser, ByteBuffer buffer, IList<Expression> expressions, bool outputUsed)
        {
            if (expressions.Count == 0) return;
            if (expressions.Count == 1)
            {
                this.CompileExpression(parser, buffer, expressions[0], outputUsed);
                return;
            }

            List<Expression> literals = new List<Expression>();
            int mode = EXPR_STREAM_OTHER;

            for (int i = 0; i < expressions.Count; ++i)
            {
                Expression expr = expressions[i];
                bool modeChange = false;
                if (expr.IsLiteral)
                {
                    if (mode == EXPR_STREAM_LITERAL)
                    {
                        literals.Add(expr);
                    }
                    else
                    {
                        mode = EXPR_STREAM_LITERAL;
                        modeChange = true;
                        --i;
                    }
                }
                else
                {
                    if (mode == EXPR_STREAM_OTHER)
                    {
                        this.CompileExpression(parser, buffer, expr, true);
                    }
                    else
                    {
                        mode = EXPR_STREAM_OTHER;
                        modeChange = true;
                        --i;
                    }
                }

                if (modeChange)
                {
                    if (literals.Count > 0)
                    {
                        this.CompileLiteralStream(parser, buffer, literals, true);
                        literals.Clear();
                    }
                }
            }

            if (literals.Count > 0)
            {
                this.CompileLiteralStream(parser, buffer, literals, true);
                literals.Clear();
            }
        }
    }
}
