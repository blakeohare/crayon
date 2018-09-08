using Common;
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

            ByteBuffer switchStatements = SwitchStatementEncoder.BuildTables(parser);

            ByteBuffer buildLibraryDeclarations = this.BuildLibraryDeclarations(parser);

            ByteBuffer header = new ByteBuffer();
            header.Concat(literalsTable);
            header.Concat(tokenData);
            header.Concat(fileContent);
            header.Concat(switchStatements);
            header.Concat(buildLibraryDeclarations);
            header.Concat(buildCniTable);

            // These contain data about absolute PC values. Once those are finalized, come back and fill these in.
            header.Add(null, OpCode.ESF_LOOKUP); // offsets to catch and finally blocks
            header.Add(null, OpCode.VALUE_STACK_DEPTH); // changes in the depth of the value stack at given PC's

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

            // Now that ops (and PCs) have been finalized, fill in ESF and Value Stack Depth data with absolute PC's
            int[] esfOps = output.GetFinalizedEsfData();
            int[] valueStackDepthOps = output.GetFinalizedValueStackDepthData();
            int esfPc = output.GetEsfPc();
            output.SetArgs(esfPc, esfOps);
            output.SetArgs(esfPc + 1, valueStackDepthOps);

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
                if (field.IsStaticField) continue;
                topLevelConstructs.Add(field);
                tlcAnnotations.Add(field.Annotations);
                tlcDefaultNames.Add(field.NameToken.Value);
                memberIds.Add(field.MemberID);
            }
            foreach (FunctionDefinition method in cd.Methods)
            {
                if (method.IsStaticMethod) continue;
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
            if (entity is FunctionDefinition) this.CompileFunctionDefinition(parser, buffer, (FunctionDefinition)entity, false);
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
                .Where<FieldDefinition>(fd =>
                    fd.IsStaticField &&
                    fd.DefaultValue != null &&
                    !(fd.DefaultValue is NullConstant))
                .Count() > 0;

            if (hasStaticFieldsWithStartingValues)
            {
                if (classDefinition.StaticConstructor == null)
                {
                    classDefinition.StaticConstructor = new ConstructorDefinition(null, new AnnotationCollection(parser), classDefinition);
                    classDefinition.StaticConstructor.ResolvePublic(parser);
                }

                List<Executable> staticFieldInitializers = new List<Executable>();
                foreach (FieldDefinition fd in classDefinition.Fields)
                {
                    if (fd.IsStaticField && fd.DefaultValue != null && !(fd.DefaultValue is NullConstant))
                    {
                        Executable assignment = new Assignment(
                            new FieldReference(fd.FirstToken, fd, fd),
                            fd.NameToken,
                            Ops.EQUALS,
                            fd.DefaultValue, fd);
                        staticFieldInitializers.Add(assignment);
                    }
                }

                staticFieldInitializers.AddRange(classDefinition.StaticConstructor.Code);
                classDefinition.StaticConstructor.Code = staticFieldInitializers.ToArray();
            }

            if (classDefinition.StaticConstructor != null)
            {
                // All static field initializers are added here.
                this.CompileConstructor(parser, buffer, classDefinition.StaticConstructor, null);
            }

            foreach (FunctionDefinition fd in classDefinition.Methods)
            {
                int pc = buffer.Size;
                fd.FinalizedPC = pc;
                this.CompileFunctionDefinition(parser, buffer, fd, true);
            }

            int classId = classDefinition.ClassID;
            int baseClassId = classDefinition.BaseClass != null ? classDefinition.BaseClass.ClassID : -1;
            int nameId = parser.GetId(classDefinition.NameToken.Value);
            int constructorId = classDefinition.Constructor.FunctionID;
            int staticConstructorId = classDefinition.StaticConstructor != null ? classDefinition.StaticConstructor.FunctionID : -1;

            int staticFieldCount = classDefinition.Fields.Where<FieldDefinition>(fd => fd.IsStaticField).Count();
            FieldDefinition[] regularFields = classDefinition.Fields.Where<FieldDefinition>(fd => !fd.IsStaticField).ToArray();
            FunctionDefinition[] regularMethods = classDefinition.Methods.Where<FunctionDefinition>(fd => !fd.IsStaticMethod).ToArray();
            List<int> members = new List<int>();
            List<FieldDefinition> fieldsWithComplexValues = new List<FieldDefinition>();
            foreach (FieldDefinition fd in regularFields)
            {
                int memberId = fd.MemberID;
                int fieldNameId = parser.GetId(fd.NameToken.Value);
                int initInstruction;
                int literalId = 0;
                if (fd.DefaultValue is ListDefinition && ((ListDefinition)fd.DefaultValue).Items.Length == 0)
                {
                    initInstruction = 1;
                }
                else if (fd.DefaultValue is DictionaryDefinition && ((DictionaryDefinition)fd.DefaultValue).Keys.Length == 0)
                {
                    initInstruction = 2;
                }
                else
                {
                    initInstruction = 0;
                    literalId = parser.GetLiteralId(fd.DefaultValue);
                    if (literalId == -1)
                    {
                        literalId = parser.GetNullConstant();
                        fieldsWithComplexValues.Add(fd);
                    }
                }

                members.AddRange(new int[] {
                    0, // flag for field
                    memberId,
                    fieldNameId,
                    initInstruction,
                    literalId});
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
                    0, // ignored value. It's just here to keep spacing consistent.
                });
            }

            ByteBuffer initializer = new ByteBuffer();

            if (fieldsWithComplexValues.Count > 0)
            {
                foreach (FieldDefinition complexField in fieldsWithComplexValues)
                {
                    this.CompileExpression(parser, initializer, complexField.DefaultValue, true);
                    initializer.Add(complexField.FirstToken, OpCode.ASSIGN_THIS_STEP, complexField.MemberID);
                }
            }

            this.CompileConstructor(parser, buffer, classDefinition.Constructor, initializer);

            List<int> args = new List<int>()
            {
                classId,
                baseClassId,
                nameId,
                constructorId,
                staticConstructorId,
                staticFieldCount,
            };

            args.AddRange(members);

            string fullyQualifiedName = classDefinition.GetFullyQualifiedLocalizedName(parser.RootScope.Locale);

            buffer.Add(classDefinition.FirstToken, OpCode.CLASS_DEFINITION, fullyQualifiedName, args.ToArray());
        }

        private void CompileConstructor(ParserContext parser, ByteBuffer buffer, ConstructorDefinition constructor, ByteBuffer complexFieldInitializers)
        {
            TODO.ThrowErrorIfKeywordThisIsUsedInBaseArgsOrDefaultArgsAnywhereInConstructor();

            ByteBuffer tBuffer = new ByteBuffer();

            ClassDefinition cd = (ClassDefinition)constructor.Owner;

            List<int> offsetsForOptionalArgs = new List<int>();
            this.CompileFunctionArgs(parser, tBuffer, constructor.ArgNames, constructor.DefaultValues, offsetsForOptionalArgs);

            int minArgs = 0;
            int maxArgs = constructor.ArgNames.Length;
            for (int i = 0; i < constructor.ArgNames.Length; ++i)
            {
                if (constructor.DefaultValues[i] == null)
                {
                    minArgs++;
                }
                else
                {
                    break;
                }
            }

            if (constructor.BaseToken != null)
            {
                this.CompileExpressionList(parser, tBuffer, constructor.BaseArgs, true);
                tBuffer.Add(
                    constructor.BaseToken,
                    OpCode.CALL_FUNCTION,
                    (int)FunctionInvocationType.BASE_CONSTRUCTOR,
                    constructor.BaseArgs.Length,
                    cd.BaseClass.Constructor.FunctionID,
                    0,
                    cd.BaseClass.ClassID);
            }

            if (complexFieldInitializers != null)
            {
                tBuffer.Concat(complexFieldInitializers);
            }

            this.Compile(parser, tBuffer, constructor.Code);
            tBuffer.Add(null, OpCode.RETURN, 0);

            bool isStatic = constructor == cd.StaticConstructor;

            List<int> args = new List<int>()
            {
                constructor.FunctionID,
                -1,
                minArgs,
                maxArgs,
                isStatic ? 4 : 3,
                cd.ClassID,
                constructor.LocalScopeSize,
                tBuffer.Size,
                offsetsForOptionalArgs.Count,
            };

            args.AddRange(offsetsForOptionalArgs);

            buffer.Add(constructor.FirstToken, OpCode.FUNCTION_DEFINITION, "<constructor>", args.ToArray());
            buffer.Concat(tBuffer);
        }

        private void CompileFunctionArgs(ParserContext parser, ByteBuffer buffer, IList<Token> argNames, IList<Expression> argValues, List<int> offsetsForOptionalArgs)
        {
            int bufferStartSize = buffer.Size;
            for (int i = 0; i < argNames.Count; ++i)
            {
                if (argValues[i] != null)
                {
                    this.CompileExpression(parser, buffer, argValues[i], true);
                    buffer.Add(argNames[i], OpCode.ASSIGN_LOCAL, i);
                    offsetsForOptionalArgs.Add(buffer.Size - bufferStartSize);
                }
            }
        }

        private int GetMinArgCountFromDefaultValuesList(Expression[] argDefaultValues)
        {
            int minArgCount = 0;
            for (int i = 0; i < argDefaultValues.Length; ++i)
            {
                if (argDefaultValues[i] != null)
                {
                    break;
                }
                minArgCount++;
            }
            return minArgCount;
        }

        private void CompileFunctionDefinition(ParserContext parser, ByteBuffer buffer, FunctionDefinition funDef, bool isMethod)
        {
            ByteBuffer tBuffer = new ByteBuffer();

            List<int> offsetsForOptionalArgs = new List<int>();
            this.CompileFunctionArgs(parser, tBuffer, funDef.ArgNames, funDef.DefaultValues, offsetsForOptionalArgs);

            Compile(parser, tBuffer, funDef.Code);

            List<int> args = new List<int>()
            {
                funDef.FunctionID,
                parser.GetId(funDef.NameToken.Value), // local var to save in
                this.GetMinArgCountFromDefaultValuesList(funDef.DefaultValues),
                funDef.ArgNames.Length, // max number of args supplied
                isMethod ? (funDef.IsStaticMethod ? 2 : 1) : 0, // type (0 - function, 1 - method, 2 - static method)
                isMethod ? ((ClassDefinition)funDef.Owner).ClassID : 0,
                funDef.LocalScopeSize,
                tBuffer.Size,
                offsetsForOptionalArgs.Count
            };
            args.AddRange(offsetsForOptionalArgs);

            buffer.Add(
                funDef.FirstToken,
                OpCode.FUNCTION_DEFINITION,
                funDef.NameToken.Value,
                args.ToArray());

            buffer.Concat(tBuffer);
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
            else if (expr is BooleanNot) this.CompileBooleanNot(parser, buffer, (BooleanNot)expr, outputUsed);
            else if (expr is Ternary) TernaryEncoder.Compile(this, parser, buffer, (Ternary)expr, outputUsed);
            else if (expr is CompileTimeDictionary) this.CompileCompileTimeDictionary((CompileTimeDictionary)expr);
            else if (expr is ListSlice) this.CompileListSlice(parser, buffer, (ListSlice)expr, outputUsed);
            else if (expr is NullCoalescer) this.CompileNullCoalescer(parser, buffer, (NullCoalescer)expr, outputUsed);
            else if (expr is BaseKeyword) this.CompileBaseKeyword(parser, buffer, (BaseKeyword)expr, outputUsed);
            else if (expr is BaseMethodReference) this.CompileBaseMethodReference(parser, buffer, (BaseMethodReference)expr, outputUsed);
            else if (expr is FunctionReference) this.CompileFunctionReference(parser, buffer, (FunctionReference)expr, outputUsed);
            else if (expr is FieldReference) FieldReferenceEncoder.Compile(parser, buffer, (FieldReference)expr, outputUsed);
            else if (expr is CoreFunctionInvocation) this.CompileCoreFunctionInvocation(parser, buffer, (CoreFunctionInvocation)expr, null, null, outputUsed);
            else if (expr is IsComparison) this.CompileIsComparison(parser, buffer, (IsComparison)expr, outputUsed);
            else if (expr is ClassReferenceLiteral) this.CompileClassReferenceLiteral(parser, buffer, (ClassReferenceLiteral)expr, outputUsed);
            else if (expr is Lambda) this.CompileLambda(parser, buffer, (Lambda)expr, outputUsed);
            else if (expr is CniFunctionInvocation) this.CompileCniFunctionInvocation(parser, buffer, (CniFunctionInvocation)expr, null, null, outputUsed);
            else throw new NotImplementedException();
        }

        internal void CompileCniFunctionInvocation(
            ParserContext parser,
            ByteBuffer buffer,
            CniFunctionInvocation cniFuncInvocation,
            Expression[] argsOverrideOrNull,
            Token throwTokenOverrideOrNull,
            bool outputUsed)
        {
            CniFunction cniFunc = cniFuncInvocation.CniFunction;
            Expression[] args = argsOverrideOrNull ?? cniFuncInvocation.Args;
            foreach (Expression arg in args)
            {
                this.CompileExpression(parser, buffer, arg, true);
            }
            Token throwToken = throwTokenOverrideOrNull ?? cniFuncInvocation.FirstToken;
            buffer.Add(throwToken, OpCode.CNI_INVOKE, cniFunc.ID, cniFunc.ArgCount, outputUsed ? 1 : 0);
        }

        private void CompileLambda(ParserContext parser, ByteBuffer buffer, Lambda lambda, bool outputUsed)
        {
            EnsureUsed(lambda.FirstToken, outputUsed);

            ByteBuffer tBuffer = new ByteBuffer();

            List<int> offsetsForOptionalArgs = new List<int>();
            Expression[] argDefaultValues_allRequired = new Expression[lambda.Args.Length];
            this.CompileFunctionArgs(parser, tBuffer, lambda.Args, argDefaultValues_allRequired, offsetsForOptionalArgs);

            Compile(parser, tBuffer, lambda.Code);

            List<int> args = new List<int>()
            {
                lambda.Args.Length, // min number of args required
                lambda.Args.Length, // max number of args supplied
                lambda.LocalScopeSize,
                tBuffer.Size,
                offsetsForOptionalArgs.Count
            };
            args.AddRange(offsetsForOptionalArgs);

            VariableId[] closureIds = lambda.ClosureIds;
            args.Add(closureIds.Length);
            foreach (VariableId closureVarId in closureIds)
            {
                args.Add(closureVarId.ClosureID);
            }

            buffer.Add(
                lambda.FirstToken,
                OpCode.LAMBDA,
                args.ToArray());

            buffer.Concat(tBuffer);
        }

        private void CompileClassReferenceLiteral(ParserContext parser, ByteBuffer buffer, ClassReferenceLiteral classRef, bool outputUsed)
        {
            if (!outputUsed) throw new ParserException(classRef, "This class reference expression does nothing.");
            buffer.Add(classRef.FirstToken, OpCode.LITERAL, parser.GetClassRefConstant(classRef.ClassDefinition));
        }

        internal static void EnsureUsed(Node item, bool outputUsed)
        {
            EnsureUsed(item.FirstToken, outputUsed);
        }

        // TODO: delete this function and merge it into the above
        private static void EnsureUsed(Token token, bool outputUsed)
        {
            // TODO: maybe reword the expression to something along the lines of "use an if statement" if it's a ternary
            // expression in which case it actually does do something.

            if (!outputUsed)
            {
                throw new ParserException(token, "Cannot have this expression here. It does nothing. Did you mean to store this output into a variable or return it?");
            }
        }

        private void CompileIsComparison(ParserContext parser, ByteBuffer buffer, IsComparison isComp, bool outputUsed)
        {
            EnsureUsed(isComp.IsToken, outputUsed);
            this.CompileExpression(parser, buffer, isComp.Expression, true);
            buffer.Add(isComp.IsToken, OpCode.IS_COMPARISON, isComp.ClassDefinition.ClassID);
        }

        internal void CompileCoreFunctionInvocation(
            ParserContext parser,
            ByteBuffer buffer,
            CoreFunctionInvocation coreFuncInvocation,
            Expression[] argsOverrideOrNull,
            Token tokenOverrideOrNull,
            bool outputUsed)
        {
            Token token = tokenOverrideOrNull ?? coreFuncInvocation.FirstToken;
            Expression[] args = argsOverrideOrNull ?? coreFuncInvocation.Args;

            if (coreFuncInvocation.FunctionId == (int)CoreFunctionID.TYPE_IS)
            {
                EnsureUsed(coreFuncInvocation.FirstToken, outputUsed);

                this.CompileExpression(parser, buffer, args[0], true);
                int typeCount = args.Length - 1;
                int[] actualArgs = new int[typeCount + 3];
                actualArgs[0] = coreFuncInvocation.FunctionId;
                actualArgs[1] = 1; // output used
                actualArgs[2] = typeCount;
                for (int i = typeCount - 1; i >= 0; --i)
                {
                    IntegerConstant typeArg = args[args.Length - 1 - i] as IntegerConstant;
                    if (typeArg == null)
                    {
                        throw new ParserException(coreFuncInvocation, "typeis requires type enum values.");
                    }
                    actualArgs[3 + i] = typeArg.Value + 1;
                }
                buffer.Add(token, OpCode.CORE_FUNCTION, actualArgs);
                return;
            }

            foreach (Expression arg in args)
            {
                this.CompileExpression(parser, buffer, arg, true);
            }

            if (coreFuncInvocation.FunctionId == (int)CoreFunctionID.INT_QUEUE_WRITE_16)
            {
                buffer.Add(token, OpCode.CORE_FUNCTION, coreFuncInvocation.FunctionId, outputUsed ? 1 : 0, args.Length - 1);
                return;
            }

            buffer.Add(token, OpCode.CORE_FUNCTION, coreFuncInvocation.FunctionId, outputUsed ? 1 : 0);
        }

        // Non-invoked function references.
        private void CompileFunctionReference(ParserContext parser, ByteBuffer buffer, FunctionReference funcRef, bool outputUsed)
        {
            EnsureUsed(funcRef.FirstToken, outputUsed);

            FunctionDefinition funcDef = funcRef.FunctionDefinition;

            int classIdStaticCheck = 0;
            int type = 0;
            if (funcDef.Owner is ClassDefinition)
            {
                if (funcDef.IsStaticMethod)
                {
                    classIdStaticCheck = ((ClassDefinition)funcDef.Owner).ClassID;
                    type = 2;
                }
                else
                {
                    type = 1;
                }
            }
            buffer.Add(funcRef.FirstToken, OpCode.PUSH_FUNC_REF,
                funcDef.FunctionID,
                type,
                classIdStaticCheck);
        }

        private void CompileBaseKeyword(ParserContext parser, ByteBuffer buffer, BaseKeyword baseKeyword, bool outputUsed)
        {
            throw new ParserException(baseKeyword, "Cannot have a reference to 'base' without invoking a field.");
        }

        private void CompileBaseMethodReference(ParserContext parser, ByteBuffer buffer, BaseMethodReference baseMethodReference, bool outputUsed)
        {
            EnsureUsed(baseMethodReference.FirstToken, outputUsed);
            int baseClassId = baseMethodReference.ClassToWhichThisMethodRefers.ClassID;
            buffer.Add(
                baseMethodReference.DotToken,
                OpCode.PUSH_FUNC_REF,
                baseMethodReference.FunctionDefinition.FunctionID,
                1, // instance method
                0);
        }

        private void CompileCompileTimeDictionary(CompileTimeDictionary compileTimeDictionary)
        {
            if (compileTimeDictionary.Type == "var")
            {
                throw new ParserException(compileTimeDictionary, "$var is a compile-time dictionary and must be dereferenced with a hardcoded string constant.");
            }
            throw new Exception(); // should not happen.
        }

        private void CompileListSlice(ParserContext parser, ByteBuffer buffer, ListSlice listSlice, bool outputUsed)
        {
            EnsureUsed(listSlice.FirstToken, outputUsed);
            this.CompileExpression(parser, buffer, listSlice.Root, true);

            Expression step = listSlice.Items[2];
            bool isStep1 = step is IntegerConstant && ((IntegerConstant)step).Value == 1;

            int serializeThese = isStep1 ? 2 : 3;
            for (int i = 0; i < serializeThese; ++i)
            {
                Expression item = listSlice.Items[i];
                if (item != null)
                {
                    this.CompileExpression(parser, buffer, item, true);
                }
            }

            bool firstIsPresent = listSlice.Items[0] != null;
            bool secondIsPresent = listSlice.Items[1] != null;

            buffer.Add(listSlice.BracketToken, OpCode.LIST_SLICE, new int[] { firstIsPresent ? 1 : 0, secondIsPresent ? 1 : 0, isStep1 ? 0 : 1 });
        }

        private void CompileNullCoalescer(ParserContext parser, ByteBuffer buffer, NullCoalescer nullCoalescer, bool outputUsed)
        {
            EnsureUsed(nullCoalescer.FirstToken, outputUsed);

            this.CompileExpression(parser, buffer, nullCoalescer.PrimaryExpression, true);
            ByteBuffer secondaryExpression = new ByteBuffer();
            this.CompileExpression(parser, secondaryExpression, nullCoalescer.SecondaryExpression, true);
            buffer.Add(nullCoalescer.FirstToken, OpCode.POP_IF_NULL_OR_JUMP, secondaryExpression.Size);
            buffer.Concat(secondaryExpression);
        }

        private void CompileBooleanNot(ParserContext parser, ByteBuffer buffer, BooleanNot boolNot, bool outputUsed)
        {
            if (!outputUsed) throw new ParserException(boolNot, "Cannot have this expression here.");

            this.CompileExpression(parser, buffer, boolNot.Root, true);
            buffer.Add(boolNot.FirstToken, OpCode.BOOLEAN_NOT);
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
