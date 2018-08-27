using Common;
using Localization;
using Parser;
using Parser.ParseTree;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Exporter
{
    internal class ByteCodeCompiler
    {
        public ByteBuffer GenerateByteCode(ParserContext parser, IList<TopLevelEntity> lines)
        {
            ByteBuffer userCode = new ByteBuffer();

            this.CompileTopLevelEntities(parser, userCode, lines);

            userCode.OptimizeJumps();

            ByteBuffer literalsTable = ByteBuffer.FromLiteralLookup(parser.LiteralLookup);

            ByteBuffer tokenData = this.BuildTokenData(userCode);

            ByteBuffer fileContent = this.BuildFileContent(parser.GetFilesById());

            ByteBuffer switchStatements = this.BuildSwitchStatementTables(parser);

            ByteBuffer buildLibraryDeclarations = this.BuildLibraryDeclarations(parser);

            ByteBuffer header = new ByteBuffer();
            header.Concat(literalsTable);
            header.Concat(tokenData);
            header.Concat(fileContent);
            header.Concat(switchStatements);
            header.Concat(buildLibraryDeclarations);

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

        private ByteBuffer BuildLibraryDeclarations(ParserContext parser)
        {
            ByteBuffer output = new ByteBuffer();

            int id = 1;
            foreach (LibraryCompilationScope libraryScope in parser.LibraryManager.ImportedLibraries)
            {
                List<string> descriptorComponents = new List<string>()
                {
                    libraryScope.Library.ID,
                    libraryScope.Library.Version,
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

        private ByteBuffer BuildSwitchStatementTables(ParserContext parser)
        {
            ByteBuffer output = new ByteBuffer();
            List<Dictionary<int, int>> intSwitches = parser.GetIntegerSwitchStatements();
            for (int i = 0; i < intSwitches.Count; ++i)
            {
                List<int> args = new List<int>();
                Dictionary<int, int> lookup = intSwitches[i];
                foreach (int key in lookup.Keys)
                {
                    int offset = lookup[key];
                    args.Add(key);
                    args.Add(offset);
                }

                output.Add(null, OpCode.BUILD_SWITCH_INT, args.ToArray());
            }

            List<Dictionary<string, int>> stringSwitches = parser.GetStringSwitchStatements();
            for (int i = 0; i < stringSwitches.Count; ++i)
            {
                Dictionary<string, int> lookup = stringSwitches[i];
                foreach (string key in lookup.Keys)
                {
                    int offset = lookup[key];
                    output.Add(null, OpCode.BUILD_SWITCH_STRING, key, i, offset);
                }
            }
            return output;
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
            else if (line is Assignment) this.CompileAssignment(parser, buffer, (Assignment)line);
            else if (line is WhileLoop) this.CompileWhileLoop(parser, buffer, (WhileLoop)line);
            else if (line is BreakStatement) this.CompileBreakStatement(parser, buffer, (BreakStatement)line);
            else if (line is ContinueStatement) this.CompileContinueStatement(parser, buffer, (ContinueStatement)line);
            else if (line is ForLoop) this.CompileForLoop(parser, buffer, (ForLoop)line);
            else if (line is IfStatement) this.CompileIfStatement(parser, buffer, (IfStatement)line);
            else if (line is ReturnStatement) this.CompileReturnStatement(parser, buffer, (ReturnStatement)line);
            else if (line is SwitchStatement) this.CompileSwitchStatement(parser, buffer, (SwitchStatement)line);
            else if (line is ForEachLoop) this.CompileForEachLoop(parser, buffer, (ForEachLoop)line);
            else if (line is DoWhileLoop) this.CompileDoWhileLoop(parser, buffer, (DoWhileLoop)line);
            else if (line is TryStatement) this.CompileTryStatement(parser, buffer, (TryStatement)line);
            else if (line is ThrowStatement) this.CompileThrowStatement(parser, buffer, (ThrowStatement)line);
            else throw new NotImplementedException("Invalid target for byte code compilation");
        }

        private void CompileThrowStatement(ParserContext parser, ByteBuffer buffer, ThrowStatement throwStatement)
        {
            this.CompileExpression(parser, buffer, throwStatement.Expression, true);
            buffer.Add(throwStatement.FirstToken, OpCode.THROW);
        }

        private void CompileTryStatement(ParserContext parser, ByteBuffer buffer, TryStatement tryStatement)
        {
            ByteBuffer tryCode = new ByteBuffer();
            this.Compile(parser, tryCode, tryStatement.TryBlock);

            if (tryStatement.TryBlock.Length > 0 && tryStatement.TryBlock[0] is TryStatement)
            {
                // If the try block begins with another try block, that'll mess with the EsfToken metadata
                // which is declared at the beginning of the try block's PC and is singular.
                // Get around this limitation by tacking on a noop (JUMP +0) in this reasonably rare edge case.
                tryCode.AddFrontSlow(null, OpCode.JUMP, 0);
            }

            List<ByteBuffer> catchBlocks = new List<ByteBuffer>();

            for (int i = 0; i < tryStatement.CatchBlocks.Length; ++i)
            {
                TryStatement.CatchBlock catchBlock = tryStatement.CatchBlocks[i];
                ByteBuffer catchBlockBuffer = new ByteBuffer();
                this.Compile(parser, catchBlockBuffer, catchBlock.Code);
                catchBlocks.Add(catchBlockBuffer);
            }

            ByteBuffer finallyCode = new ByteBuffer();
            this.Compile(parser, finallyCode, tryStatement.FinallyBlock);
            finallyCode.ResolveBreaksAndContinuesForFinally(false);
            finallyCode.Add(null, OpCode.FINALLY_END,
                new int[] {
                    // First 2 args are the same as a BREAK op code
                    // Last 2 args are the same as a CONTINUE op code
                    // These are all 0 and are resolved into their final values in the same pass as BREAK and CONTINUE
                    0, // break flag 0|1|2
                    0, // break offset
                    0, // continue flag 0|1|2
                    0 // continue offset
                });


            // All user code is now compiled and offsets are sort of known.
            // Now build a lookup jump router thingy for all the catch blocks, if any.

            ByteBuffer allCatchBlocks = new ByteBuffer();
            if (catchBlocks.Count > 0)
            {
                /*
                    It'll look something like this...
                    0   EXCEPTION_HANDLED_TOGGLE true
                    1   JUMP_IF_EXCEPTION_IS_TYPE offset varId type1, type2, ...
                    2   JUMP_IF_EXCEPTION_IS_TYPE offset varId type3
                    3   EXCEPTION_HANDLED_TOGGLE false
                    4   JUMP [to finally]

                    5   catch block 1
                        ...
                    22  last line in catch block 1
                    23  JUMP [to finally]

                    24  catch block 2...
                        ...
                    72  last line in catch block 2

                    73  finally code begins...
                */

                // Add jumps to the end of each catch block to jump to the end.
                // Going in reverse order is easier for this.
                int totalSize = 0;
                for (int i = catchBlocks.Count - 1; i >= 0; --i)
                {
                    ByteBuffer catchBlockBuffer = catchBlocks[i];
                    if (totalSize > 0) // omit the last block since a JUMP 0 is pointless.
                    {
                        catchBlockBuffer.Add(null, OpCode.JUMP, totalSize);
                    }
                    totalSize += catchBlockBuffer.Size;
                }

                // Now generate the header. This is also done backwards since it's easier.
                ByteBuffer exceptionSortHeader = new ByteBuffer();

                int offset = 2 // EXCEPTION_HANDLED_TOGGLE + final JUMP
                    + catchBlocks.Count - 1; // remaining jump instructions to jump over

                // Add all the JUMP_IF_EXCEPTION_OF_TYPE instructions.
                for (int i = 0; i < catchBlocks.Count; ++i)
                {
                    TryStatement.CatchBlock cb = tryStatement.CatchBlocks[i];
                    ByteBuffer cbByteBuffer = catchBlocks[i];
                    int variableId = cb.VariableLocalScopeId.ID;

                    // for each catch block insert a type-check-jump
                    List<int> typeCheckArgs = new List<int>() { offset, variableId }; // first arg is offset, second is variable ID (or -1), successive args are all class ID's
                    typeCheckArgs.AddRange(cb.TypeClasses.Select<ClassDefinition, int>(cd => cd.ClassID));
                    exceptionSortHeader.Add(null, OpCode.JUMP_IF_EXCEPTION_OF_TYPE, typeCheckArgs.ToArray());

                    // add the block to the running total
                    offset += cbByteBuffer.Size;

                    // ...but subtract 1 for the JUMP_IF_EXCEPTION_OF_TYPE you just added.
                    offset -= 1;
                }
                exceptionSortHeader.Add(null, OpCode.EXCEPTION_HANDLED_TOGGLE, 0);
                exceptionSortHeader.Add(null, OpCode.JUMP, totalSize);

                allCatchBlocks.Add(null, OpCode.EXCEPTION_HANDLED_TOGGLE, 1);
                allCatchBlocks.Concat(exceptionSortHeader);
                foreach (ByteBuffer catchBlock in catchBlocks)
                {
                    allCatchBlocks.Concat(catchBlock);
                }
            }

            int tryBegin = buffer.Size;
            buffer.Concat(tryCode);
            buffer.Add(null, OpCode.JUMP, allCatchBlocks.Size);
            buffer.Concat(allCatchBlocks);
            buffer.ResolveBreaksAndContinuesForFinally(true);

            buffer.Concat(finallyCode);

            int offsetToCatch = tryCode.Size + 1;
            int offsetToFinally = offsetToCatch + allCatchBlocks.Size;
            buffer.SetEsfToken(tryBegin, offsetToCatch, offsetToFinally);
        }

        private void CompileForEachLoop(ParserContext parser, ByteBuffer buffer, ForEachLoop forEachLoop)
        {
            buffer.Add(null, OpCode.LITERAL, parser.GetIntConstant(0));
            buffer.Add(null, OpCode.LITERAL, parser.GetIntConstant(forEachLoop.IterationVariableId.ID));
            this.CompileExpression(parser, buffer, forEachLoop.IterationExpression, true);
            buffer.Add(forEachLoop.IterationExpression.FirstToken, OpCode.VERIFY_TYPE_IS_ITERABLE);

            buffer.SetLastValueStackDepthOffset(3);

            ByteBuffer body = new ByteBuffer();
            ByteBuffer body2 = new ByteBuffer();

            this.Compile(parser, body2, forEachLoop.Code);

            body.Add(forEachLoop.FirstToken, OpCode.ITERATION_STEP, body2.Size + 1);

            body2.Add(null, OpCode.JUMP, -body2.Size - 2);
            body.Concat(body2);

            body.ResolveBreaks();
            body.ResolveContinues();

            buffer.Concat(body);
            buffer.Add(null, OpCode.POP); // list
            buffer.Add(null, OpCode.POP); // var ID
            buffer.Add(null, OpCode.POP); // index
            buffer.SetLastValueStackDepthOffset(-3);
        }

        private void CompileSwitchStatement(ParserContext parser, ByteBuffer buffer, SwitchStatement switchStatement)
        {
            this.CompileExpression(parser, buffer, switchStatement.Condition, true);

            ByteBuffer chunkBuffer = new ByteBuffer();

            Dictionary<int, int> chunkIdsToOffsets = new Dictionary<int, int>();
            Dictionary<int, int> integersToChunkIds = new Dictionary<int, int>();
            Dictionary<string, int> stringsToChunkIds = new Dictionary<string, int>();

            int defaultChunkId = -1;
            foreach (SwitchStatement.Chunk chunk in switchStatement.Chunks)
            {
                int chunkId = chunk.ID;

                if (chunk.Cases.Length == 1 && chunk.Cases[0] == null)
                {
                    defaultChunkId = chunkId;
                }
                else
                {
                    foreach (Expression expression in chunk.Cases)
                    {
                        if (switchStatement.UsesIntegers)
                        {
                            integersToChunkIds[((IntegerConstant)expression).Value] = chunkId;
                        }
                        else
                        {
                            stringsToChunkIds[((StringConstant)expression).Value] = chunkId;
                        }
                    }
                }

                chunkIdsToOffsets[chunkId] = chunkBuffer.Size;

                this.Compile(parser, chunkBuffer, chunk.Code);
            }

            chunkBuffer.ResolveBreaks();

            int switchId = parser.RegisterByteCodeSwitch(chunkIdsToOffsets, integersToChunkIds, stringsToChunkIds, switchStatement.UsesIntegers);

            int defaultOffsetLength = defaultChunkId == -1
                ? chunkBuffer.Size
                : chunkIdsToOffsets[defaultChunkId];

            buffer.Add(switchStatement.FirstToken, switchStatement.UsesIntegers ? OpCode.SWITCH_INT : OpCode.SWITCH_STRING, switchId, defaultOffsetLength);
            buffer.Concat(chunkBuffer);
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
                            "=",
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

            string fullyQualifiedName = classDefinition.GetFullyQualifiedLocalizedName(parser.UserCodeCompilationScope.Locale);

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

        private void CompileReturnStatement(ParserContext parser, ByteBuffer buffer, ReturnStatement returnStatement)
        {
            if (returnStatement.TopLevelEntity is ConstructorDefinition)
            {
                if (returnStatement.Expression != null)
                {
                    throw new ParserException(returnStatement, "Cannot return a value from a constructor.");
                }
            }

            if (returnStatement.Expression == null || returnStatement.Expression is NullConstant)
            {
                buffer.Add(returnStatement.FirstToken, OpCode.RETURN, 0);
            }
            else
            {
                this.CompileExpression(parser, buffer, returnStatement.Expression, true);
                buffer.Add(returnStatement.FirstToken, OpCode.RETURN, 1);
            }
        }

        private void CompileIfStatement(ParserContext parser, ByteBuffer buffer, IfStatement ifStatement)
        {
            this.CompileExpression(parser, buffer, ifStatement.Condition, true);
            ByteBuffer trueCode = new ByteBuffer();
            this.Compile(parser, trueCode, ifStatement.TrueCode);
            ByteBuffer falseCode = new ByteBuffer();
            this.Compile(parser, falseCode, ifStatement.FalseCode);

            if (falseCode.Size == 0)
            {
                if (trueCode.Size == 0) buffer.Add(ifStatement.Condition.FirstToken, OpCode.POP);
                else
                {
                    buffer.Add(ifStatement.Condition.FirstToken, OpCode.JUMP_IF_FALSE, trueCode.Size);
                    buffer.Concat(trueCode);
                }
            }
            else
            {
                trueCode.Add(null, OpCode.JUMP, falseCode.Size);
                buffer.Add(ifStatement.Condition.FirstToken, OpCode.JUMP_IF_FALSE, trueCode.Size);
                buffer.Concat(trueCode);
                buffer.Concat(falseCode);
            }
        }

        private void CompileBreakStatement(ParserContext parser, ByteBuffer buffer, BreakStatement breakStatement)
        {
            buffer.Add(breakStatement.FirstToken, OpCode.BREAK, 0, 0);
        }

        private void CompileContinueStatement(ParserContext parser, ByteBuffer buffer, ContinueStatement continueStatement)
        {
            buffer.Add(continueStatement.FirstToken, OpCode.CONTINUE, 0, 0);
        }

        private void CompileForLoop(ParserContext parser, ByteBuffer buffer, ForLoop forLoop)
        {
            this.Compile(parser, buffer, forLoop.Init);

            ByteBuffer codeBuffer = new ByteBuffer();
            this.Compile(parser, codeBuffer, forLoop.Code);
            codeBuffer.ResolveContinues(true); // resolve continues as jump-to-end before you add the step instructions.
            this.Compile(parser, codeBuffer, forLoop.Step);

            ByteBuffer forBuffer = new ByteBuffer();
            this.CompileExpression(parser, forBuffer, forLoop.Condition, true);
            forBuffer.Add(forLoop.Condition.FirstToken, OpCode.JUMP_IF_FALSE, codeBuffer.Size + 1); // +1 to go past the jump I'm about to add.

            forBuffer.Concat(codeBuffer);
            forBuffer.Add(null, OpCode.JUMP, -forBuffer.Size - 1);

            forBuffer.ResolveBreaks();

            buffer.Concat(forBuffer);
        }

        private void CompileWhileLoop(ParserContext parser, ByteBuffer buffer, WhileLoop whileLoop)
        {
            ByteBuffer loopBody = new ByteBuffer();
            this.Compile(parser, loopBody, whileLoop.Code);
            ByteBuffer condition = new ByteBuffer();
            this.CompileExpression(parser, condition, whileLoop.Condition, true);

            condition.Add(whileLoop.Condition.FirstToken, OpCode.JUMP_IF_FALSE, loopBody.Size + 1);
            condition.Concat(loopBody);
            condition.Add(null, OpCode.JUMP, -condition.Size - 1);

            condition.ResolveBreaks();
            condition.ResolveContinues();

            buffer.Concat(condition);
        }

        private void CompileDoWhileLoop(ParserContext parser, ByteBuffer buffer, DoWhileLoop doWhileLoop)
        {
            ByteBuffer loopBody = new ByteBuffer();
            this.Compile(parser, loopBody, doWhileLoop.Code);
            loopBody.ResolveContinues(true); // continues should jump to the condition, hence the true.

            ByteBuffer condition = new ByteBuffer();
            this.CompileExpression(parser, condition, doWhileLoop.Condition, true);
            loopBody.Concat(condition);
            loopBody.Add(doWhileLoop.Condition.FirstToken, OpCode.JUMP_IF_TRUE, -loopBody.Size - 1);
            loopBody.ResolveBreaks();

            buffer.Concat(loopBody);
        }

        private BinaryOps ConvertOpString(Token token)
        {
            switch (token.Value)
            {
                case "++": return BinaryOps.ADDITION;
                case "+=": return BinaryOps.ADDITION;
                case "--": return BinaryOps.SUBTRACTION;
                case "-=": return BinaryOps.SUBTRACTION;
                case "*=": return BinaryOps.MULTIPLICATION;
                case "/=": return BinaryOps.DIVISION;
                case "%=": return BinaryOps.MODULO;
                case "&=": return BinaryOps.BITWISE_AND;
                case "|=": return BinaryOps.BITWISE_OR;
                case "^=": return BinaryOps.BITWISE_XOR;
                case "<<=": return BinaryOps.BIT_SHIFT_LEFT;
                case ">>=": return BinaryOps.BIT_SHIFT_RIGHT;
                default: throw new ParserException(token, "Unrecognized op.");
            }
        }

        private void CompileAssignment(ParserContext parser, ByteBuffer buffer, Assignment assignment)
        {
            if (assignment.AssignmentOp == "=")
            {
                if (assignment.Target is Variable)
                {
                    Variable varTarget = (Variable)assignment.Target;
                    this.CompileExpression(parser, buffer, assignment.Value, true);
                    VariableId varId = varTarget.LocalScopeId;
                    if (varId.UsedByClosure)
                    {
                        buffer.Add(assignment.AssignmentOpToken, OpCode.ASSIGN_CLOSURE, varId.ClosureID);
                    }
                    else
                    {
                        buffer.Add(assignment.AssignmentOpToken, OpCode.ASSIGN_LOCAL, varId.ID);
                    }
                }
                else if (assignment.Target is BracketIndex)
                {
                    BracketIndex bi = (BracketIndex)assignment.Target;
                    this.CompileExpression(parser, buffer, bi.Root, true);
                    this.CompileExpression(parser, buffer, bi.Index, true);
                    this.CompileExpression(parser, buffer, assignment.Value, true);
                    buffer.Add(assignment.AssignmentOpToken, OpCode.ASSIGN_INDEX, 0);
                }
                else if (assignment.Target is DotField)
                {
                    DotField dotStep = (DotField)assignment.Target;
                    if (dotStep.Root is ThisKeyword)
                    {
                        this.CompileExpression(parser, buffer, assignment.Value, true);
                        buffer.Add(assignment.AssignmentOpToken, OpCode.ASSIGN_THIS_STEP, parser.GetId(dotStep.StepToken.Value));
                    }
                    else
                    {
                        this.CompileExpression(parser, buffer, dotStep.Root, true);
                        this.CompileExpression(parser, buffer, assignment.Value, true);
                        int nameId = parser.GetId(dotStep.StepToken.Value);
                        int localeScopedNameId = nameId * parser.GetLocaleCount() + parser.GetLocaleId(dotStep.Owner.FileScope.CompilationScope.Locale);
                        buffer.Add(assignment.AssignmentOpToken, OpCode.ASSIGN_STEP, nameId, 0, localeScopedNameId);
                    }
                }
                else if (assignment.Target is FieldReference)
                {
                    this.CompileExpression(parser, buffer, assignment.Value, true);
                    FieldReference fieldReference = (FieldReference)assignment.Target;
                    if (fieldReference.Field.IsStaticField)
                    {
                        buffer.Add(
                            assignment.AssignmentOpToken,
                            OpCode.ASSIGN_STATIC_FIELD,
                            ((ClassDefinition)fieldReference.Field.Owner).ClassID,
                            fieldReference.Field.StaticMemberID);
                    }
                    else
                    {
                        buffer.Add(
                            assignment.AssignmentOpToken,
                            OpCode.ASSIGN_THIS_STEP,
                            fieldReference.Field.MemberID);
                    }
                }
                else
                {
                    throw new Exception("This shouldn't happen.");
                }
            }
            else
            {
                BinaryOps op = this.ConvertOpString(assignment.AssignmentOpToken);
                if (assignment.Target is Variable)
                {
                    Variable varTarget = (Variable)assignment.Target;
                    VariableId varId = varTarget.LocalScopeId;
                    bool isClosure = varId.UsedByClosure;
                    int scopeId = isClosure ? varId.ClosureID : varId.ID;

                    buffer.Add(varTarget.FirstToken, isClosure ? OpCode.DEREF_CLOSURE : OpCode.LOCAL, scopeId);
                    this.CompileExpression(parser, buffer, assignment.Value, true);
                    buffer.Add(assignment.AssignmentOpToken, OpCode.BINARY_OP, (int)op);
                    buffer.Add(assignment.Target.FirstToken, isClosure ? OpCode.ASSIGN_CLOSURE : OpCode.ASSIGN_LOCAL, scopeId);
                }
                else if (assignment.Target is DotField)
                {
                    DotField dotExpr = (DotField)assignment.Target;
                    int stepId = parser.GetId(dotExpr.StepToken.Value);
                    int localeScopedStepId = parser.GetLocaleCount() * stepId + parser.GetLocaleId(dotExpr.Owner.FileScope.CompilationScope.Locale);
                    this.CompileExpression(parser, buffer, dotExpr.Root, true);
                    if (!(dotExpr.Root is ThisKeyword))
                    {
                        buffer.Add(null, OpCode.DUPLICATE_STACK_TOP, 1);
                    }
                    buffer.Add(dotExpr.DotToken, OpCode.DEREF_DOT, stepId, localeScopedStepId);
                    this.CompileExpression(parser, buffer, assignment.Value, true);
                    buffer.Add(assignment.AssignmentOpToken, OpCode.BINARY_OP, (int)op);
                    if (dotExpr.Root is ThisKeyword)
                    {
                        buffer.Add(assignment.AssignmentOpToken, OpCode.ASSIGN_THIS_STEP, stepId);
                    }
                    else
                    {
                        int localeScopedNameId = stepId * parser.GetLocaleCount() + parser.GetLocaleId(dotExpr.Owner.FileScope.CompilationScope.Locale);
                        buffer.Add(assignment.AssignmentOpToken, OpCode.ASSIGN_STEP, stepId, 0, localeScopedNameId);
                    }
                }
                else if (assignment.Target is BracketIndex)
                {
                    BracketIndex indexExpr = (BracketIndex)assignment.Target;
                    this.CompileExpression(parser, buffer, indexExpr.Root, true);
                    this.CompileExpression(parser, buffer, indexExpr.Index, true);
                    buffer.Add(null, OpCode.DUPLICATE_STACK_TOP, 2);
                    buffer.Add(indexExpr.BracketToken, OpCode.INDEX);
                    this.CompileExpression(parser, buffer, assignment.Value, true);
                    buffer.Add(assignment.AssignmentOpToken, OpCode.BINARY_OP, (int)op);
                    buffer.Add(assignment.AssignmentOpToken, OpCode.ASSIGN_INDEX, 0);
                }
                else if (assignment.Target is FieldReference)
                {
                    FieldReference fieldRef = (FieldReference)assignment.Target;
                    this.CompileFieldReference(parser, buffer, fieldRef, true);
                    this.CompileExpression(parser, buffer, assignment.Value, true);
                    buffer.Add(assignment.AssignmentOpToken, OpCode.BINARY_OP, (int)op);

                    if (fieldRef.Field.IsStaticField)
                    {
                        buffer.Add(
                            assignment.AssignmentOpToken,
                            OpCode.ASSIGN_STATIC_FIELD,
                            ((ClassDefinition)fieldRef.Field.Owner).ClassID,
                            fieldRef.Field.StaticMemberID);
                    }
                    else
                    {
                        buffer.Add(
                            assignment.AssignmentOpToken,
                            OpCode.ASSIGN_THIS_STEP,
                            fieldRef.Field.MemberID);
                    }
                }
                else
                {
                    throw new ParserException(assignment.AssignmentOpToken, "Assignment is not allowed on this sort of expression.");
                }
            }
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

        private void CompileExpression(ParserContext parser, ByteBuffer buffer, Expression expr, bool outputUsed)
        {
            if (expr is FunctionCall) this.CompileFunctionCall(parser, buffer, (FunctionCall)expr, outputUsed);
            else if (expr is IntegerConstant) this.CompileIntegerConstant(parser, buffer, (IntegerConstant)expr, outputUsed);
            else if (expr is Variable) this.CompileVariable(parser, buffer, (Variable)expr, outputUsed);
            else if (expr is BooleanConstant) this.CompileBooleanConstant(parser, buffer, (BooleanConstant)expr, outputUsed);
            else if (expr is DotField) this.CompileDotStep(parser, buffer, (DotField)expr, outputUsed);
            else if (expr is BracketIndex) this.CompileBracketIndex(parser, buffer, (BracketIndex)expr, outputUsed);
            else if (expr is OpChain) this.CompileBinaryOpChain(parser, buffer, (OpChain)expr, outputUsed);
            else if (expr is StringConstant) this.CompileStringConstant(parser, buffer, (StringConstant)expr, outputUsed);
            else if (expr is NegativeSign) this.CompileNegativeSign(parser, buffer, (NegativeSign)expr, outputUsed);
            else if (expr is ListDefinition) this.CompileListDefinition(parser, buffer, (ListDefinition)expr, outputUsed);
            else if (expr is Increment) this.CompileIncrement(parser, buffer, (Increment)expr, outputUsed);
            else if (expr is FloatConstant) this.CompileFloatConstant(parser, buffer, (FloatConstant)expr, outputUsed);
            else if (expr is NullConstant) this.CompileNullConstant(parser, buffer, (NullConstant)expr, outputUsed);
            else if (expr is ThisKeyword) this.CompileThisKeyword(parser, buffer, (ThisKeyword)expr, outputUsed);
            else if (expr is Instantiate) this.CompileInstantiate(parser, buffer, (Instantiate)expr, outputUsed);
            else if (expr is DictionaryDefinition) this.CompileDictionaryDefinition(parser, buffer, (DictionaryDefinition)expr, outputUsed);
            else if (expr is BooleanCombination) this.CompileBooleanCombination(parser, buffer, (BooleanCombination)expr, outputUsed);
            else if (expr is BooleanNot) this.CompileBooleanNot(parser, buffer, (BooleanNot)expr, outputUsed);
            else if (expr is Ternary) this.CompileTernary(parser, buffer, (Ternary)expr, outputUsed);
            else if (expr is CompileTimeDictionary) this.CompileCompileTimeDictionary((CompileTimeDictionary)expr);
            else if (expr is ListSlice) this.CompileListSlice(parser, buffer, (ListSlice)expr, outputUsed);
            else if (expr is NullCoalescer) this.CompileNullCoalescer(parser, buffer, (NullCoalescer)expr, outputUsed);
            else if (expr is BaseKeyword) this.CompileBaseKeyword(parser, buffer, (BaseKeyword)expr, outputUsed);
            else if (expr is BaseMethodReference) this.CompileBaseMethodReference(parser, buffer, (BaseMethodReference)expr, outputUsed);
            else if (expr is LibraryFunctionCall) this.CompileLibraryFunctionCall(parser, buffer, (LibraryFunctionCall)expr, null, null, outputUsed);
            else if (expr is FunctionReference) this.CompileFunctionReference(parser, buffer, (FunctionReference)expr, outputUsed);
            else if (expr is FieldReference) this.CompileFieldReference(parser, buffer, (FieldReference)expr, outputUsed);
            else if (expr is CoreFunctionInvocation) this.CompileCoreFunctionInvocation(parser, buffer, (CoreFunctionInvocation)expr, null, null, outputUsed);
            else if (expr is IsComparison) this.CompileIsComparison(parser, buffer, (IsComparison)expr, outputUsed);
            else if (expr is ClassReferenceLiteral) this.CompileClassReferenceLiteral(parser, buffer, (ClassReferenceLiteral)expr, outputUsed);
            else if (expr is Lambda) this.CompileLambda(parser, buffer, (Lambda)expr, outputUsed);
            else throw new NotImplementedException();
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

        private void CompileCoreFunctionInvocation(
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

        private void CompileFieldReference(ParserContext parser, ByteBuffer buffer, FieldReference fieldRef, bool outputUsed)
        {
            EnsureUsed(fieldRef.FirstToken, outputUsed);

            if (fieldRef.Field.IsStaticField)
            {
                buffer.Add(
                    fieldRef.FirstToken,
                    OpCode.DEREF_STATIC_FIELD,
                    ((ClassDefinition)fieldRef.Field.Owner).ClassID,
                    fieldRef.Field.StaticMemberID);
            }
            else
            {
                buffer.Add(
                    fieldRef.FirstToken,
                    OpCode.DEREF_INSTANCE_FIELD,
                    fieldRef.Field.MemberID);
            }
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

        private void CompileTernary(ParserContext parser, ByteBuffer buffer, Ternary ternary, bool outputUsed)
        {
            EnsureUsed(ternary.FirstToken, outputUsed);

            this.CompileExpression(parser, buffer, ternary.Condition, true);
            ByteBuffer trueBuffer = new ByteBuffer();
            this.CompileExpression(parser, trueBuffer, ternary.TrueValue, true);
            ByteBuffer falseBuffer = new ByteBuffer();
            this.CompileExpression(parser, falseBuffer, ternary.FalseValue, true);
            trueBuffer.Add(null, OpCode.JUMP, falseBuffer.Size);
            buffer.Add(ternary.Condition.FirstToken, OpCode.JUMP_IF_FALSE, trueBuffer.Size);
            buffer.Concat(trueBuffer);
            buffer.Concat(falseBuffer);
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

        private void CompileBooleanCombination(ParserContext parser, ByteBuffer buffer, BooleanCombination boolComb, bool outputUsed)
        {
            if (!outputUsed) throw new ParserException(boolComb, "Cannot have this expression here.");

            ByteBuffer rightBuffer = new ByteBuffer();
            Expression[] expressions = boolComb.Expressions;
            this.CompileExpression(parser, rightBuffer, expressions[expressions.Length - 1], true);
            for (int i = expressions.Length - 2; i >= 0; --i)
            {
                ByteBuffer leftBuffer = new ByteBuffer();
                this.CompileExpression(parser, leftBuffer, expressions[i], true);
                Token op = boolComb.Ops[i];
                if (op.Value == "&&")
                {
                    leftBuffer.Add(op, OpCode.JUMP_IF_FALSE_NO_POP, rightBuffer.Size);
                }
                else
                {
                    leftBuffer.Add(op, OpCode.JUMP_IF_TRUE_NO_POP, rightBuffer.Size);
                }
                leftBuffer.Concat(rightBuffer);
                rightBuffer = leftBuffer;
            }

            buffer.Concat(rightBuffer);
        }

        private void CompileDictionaryDefinition(ParserContext parser, ByteBuffer buffer, DictionaryDefinition dictDef, bool outputUsed)
        {
            if (!outputUsed) throw new ParserException(dictDef, "Cannot have a dictionary all by itself.");

            int itemCount = dictDef.Keys.Length;
            List<Expression> expressionList = new List<Expression>();
            for (int i = 0; i < itemCount; ++i)
            {
                expressionList.Add(dictDef.Keys[i]);
                expressionList.Add(dictDef.Values[i]);
            }

            this.CompileExpressionList(parser, buffer, expressionList, true);

            buffer.Add(dictDef.FirstToken, OpCode.DEF_DICTIONARY, itemCount);
        }

        private void CompileInstantiate(ParserContext parser, ByteBuffer buffer, Instantiate instantiate, bool outputUsed)
        {
            ClassDefinition cd = instantiate.Class;
            ConstructorDefinition constructor = cd.Constructor;

            this.CompileExpressionList(parser, buffer, instantiate.Args, true);
            buffer.Add(instantiate.NameToken,
                OpCode.CALL_FUNCTION,
                (int)FunctionInvocationType.CONSTRUCTOR,
                instantiate.Args.Length,
                constructor.FunctionID,
                outputUsed ? 1 : 0,
                cd.ClassID);
        }

        private void CompileThisKeyword(ParserContext parser, ByteBuffer buffer, ThisKeyword thisKeyword, bool outputUsed)
        {
            if (!outputUsed) throw new ParserException(thisKeyword, "This expression doesn't do anything.");

            buffer.Add(thisKeyword.FirstToken, OpCode.THIS);
        }

        private void CompileNullConstant(ParserContext parser, ByteBuffer buffer, NullConstant nullConstant, bool outputUsed)
        {
            if (!outputUsed) throw new ParserException(nullConstant, "This expression doesn't do anything.");

            buffer.Add(nullConstant.FirstToken, OpCode.LITERAL, parser.GetNullConstant());
        }

        private void CompileFloatConstant(ParserContext parser, ByteBuffer buffer, FloatConstant floatConstant, bool outputUsed)
        {
            if (!outputUsed) throw new ParserException(floatConstant, "This expression doesn't do anything.");
            buffer.Add(floatConstant.FirstToken, OpCode.LITERAL, parser.GetFloatConstant(floatConstant.Value));
        }

        private void CompileIncrement(ParserContext parser, ByteBuffer buffer, Increment increment, bool outputUsed)
        {
            if (!outputUsed)
            {
                throw new Exception("This should have been optimized into a += or -=");
            }

            if (increment.Root is Variable)
            {
                // OpCode re-use be damned. This should be not one, but two top-level op codes.
                // INCREMENT_INLINE and INCREMENT_POP (depending on whether outputUsed is true)
                // In fact, the code here in its current form is actually WRONG because someString++ will have
                // a '1' appended to it when it really should be an error if the variable is not an integer.
                // Same for the others below. Ideally the DUPLICATE_STACK_TOP op should be removed.
                Variable variable = (Variable)increment.Root;
                VariableId varId = variable.LocalScopeId;
                bool isClosureVar = varId.UsedByClosure;
                int scopeId = isClosureVar ? varId.ClosureID : varId.ID;
                this.CompileExpression(parser, buffer, increment.Root, true);
                if (increment.IsPrefix)
                {
                    buffer.Add(increment.IncrementToken, OpCode.LITERAL, parser.GetIntConstant(1));
                    buffer.Add(increment.IncrementToken, OpCode.BINARY_OP, increment.IsIncrement ? (int)BinaryOps.ADDITION : (int)BinaryOps.SUBTRACTION);
                    buffer.Add(increment.IncrementToken, OpCode.DUPLICATE_STACK_TOP, 1);
                    buffer.Add(variable.FirstToken, isClosureVar ? OpCode.ASSIGN_CLOSURE : OpCode.ASSIGN_LOCAL, scopeId);
                }
                else
                {
                    buffer.Add(increment.IncrementToken, OpCode.DUPLICATE_STACK_TOP, 1);
                    buffer.Add(increment.IncrementToken, OpCode.LITERAL, parser.GetIntConstant(1));
                    buffer.Add(increment.IncrementToken, OpCode.BINARY_OP, increment.IsIncrement ? (int)BinaryOps.ADDITION : (int)BinaryOps.SUBTRACTION);
                    buffer.Add(variable.FirstToken, isClosureVar ? OpCode.ASSIGN_CLOSURE : OpCode.ASSIGN_LOCAL, scopeId);
                }
            }
            else if (increment.Root is BracketIndex)
            {
                BracketIndex bracketIndex = (BracketIndex)increment.Root;
                this.CompileExpression(parser, buffer, bracketIndex.Root, true);
                this.CompileExpression(parser, buffer, bracketIndex.Index, true);
                buffer.Add(increment.IncrementToken, OpCode.DUPLICATE_STACK_TOP, 2);
                buffer.Add(bracketIndex.BracketToken, OpCode.INDEX);
                if (increment.IsPrefix)
                {
                    buffer.Add(increment.IncrementToken, OpCode.LITERAL, parser.GetIntConstant(1));
                    buffer.Add(increment.IncrementToken, OpCode.BINARY_OP, increment.IsIncrement ? (int)BinaryOps.ADDITION : (int)BinaryOps.SUBTRACTION);
                    buffer.Add(increment.IncrementToken, OpCode.ASSIGN_INDEX, 1);
                }
                else
                {
                    buffer.Add(increment.IncrementToken, OpCode.STACK_INSERTION_FOR_INCREMENT);
                    buffer.Add(increment.IncrementToken, OpCode.LITERAL, parser.GetIntConstant(1));
                    buffer.Add(increment.IncrementToken, OpCode.BINARY_OP, increment.IsIncrement ? (int)BinaryOps.ADDITION : (int)BinaryOps.SUBTRACTION);
                    buffer.Add(increment.IncrementToken, OpCode.ASSIGN_INDEX, 0);
                }
            }
            else if (increment.Root is DotField)
            {
                DotField dotStep = (DotField)increment.Root;
                this.CompileExpression(parser, buffer, dotStep.Root, true);
                buffer.Add(increment.IncrementToken, OpCode.DUPLICATE_STACK_TOP, 1);
                int nameId = parser.GetId(dotStep.StepToken.Value);
                int localeScopedNameId = nameId * parser.GetLocaleCount() + parser.GetLocaleId(dotStep.Owner.FileScope.CompilationScope.Locale);
                buffer.Add(dotStep.DotToken, OpCode.DEREF_DOT, nameId, localeScopedNameId);
                if (increment.IsPrefix)
                {
                    buffer.Add(increment.IncrementToken, OpCode.LITERAL, parser.GetIntConstant(1));
                    buffer.Add(increment.IncrementToken, OpCode.BINARY_OP, increment.IsIncrement ? (int)BinaryOps.ADDITION : (int)BinaryOps.SUBTRACTION);
                    buffer.Add(increment.IncrementToken, OpCode.ASSIGN_STEP, nameId, 1, localeScopedNameId);
                }
                else
                {
                    buffer.Add(increment.IncrementToken, OpCode.DUPLICATE_STACK_TOP, 2);
                    buffer.Add(increment.IncrementToken, OpCode.LITERAL, parser.GetIntConstant(1));
                    buffer.Add(increment.IncrementToken, OpCode.BINARY_OP, increment.IsIncrement ? (int)BinaryOps.ADDITION : (int)BinaryOps.SUBTRACTION);
                    buffer.Add(increment.IncrementToken, OpCode.ASSIGN_STEP, nameId, 0, localeScopedNameId);
                    buffer.Add(increment.IncrementToken, OpCode.STACK_SWAP_POP);
                }
            }
            else if (increment.Root is FieldReference)
            {
                FieldReference fr = (FieldReference)increment.Root;
                bool isStatic = fr.Field.IsStaticField;
                ClassDefinition cd = (ClassDefinition)fr.Field.Owner;
                int memberId = isStatic ? fr.Field.StaticMemberID : fr.Field.MemberID;

                this.CompileExpression(parser, buffer, fr, true);
                if (increment.IsPrefix)
                {
                    buffer.Add(increment.IncrementToken, OpCode.LITERAL, parser.GetIntConstant(1));
                    buffer.Add(increment.IncrementToken, OpCode.BINARY_OP, increment.IsIncrement ? (int)BinaryOps.ADDITION : (int)BinaryOps.SUBTRACTION);
                    buffer.Add(increment.IncrementToken, OpCode.DUPLICATE_STACK_TOP, 1);
                }
                else
                {
                    buffer.Add(increment.IncrementToken, OpCode.DUPLICATE_STACK_TOP, 1);
                    buffer.Add(increment.IncrementToken, OpCode.LITERAL, parser.GetIntConstant(1));
                    buffer.Add(increment.IncrementToken, OpCode.BINARY_OP, increment.IsIncrement ? (int)BinaryOps.ADDITION : (int)BinaryOps.SUBTRACTION);
                }
                Token token = increment.IsPrefix ? increment.FirstToken : fr.FirstToken;
                if (isStatic)
                {
                    buffer.Add(token, OpCode.ASSIGN_STATIC_FIELD, ((ClassDefinition)fr.Field.Owner).ClassID, memberId);
                }
                else
                {
                    buffer.Add(token, OpCode.ASSIGN_THIS_STEP, memberId);
                }
            }
            else
            {
                throw new ParserException(increment.IncrementToken, "Cannot apply " + (increment.IsIncrement ? "++" : "--") + " to this sort of expression.");
            }
        }

        private void CompileListDefinition(ParserContext parser, ByteBuffer buffer, ListDefinition listDef, bool outputUsed)
        {
            if (!outputUsed) throw new ParserException(listDef, "List allocation made without storing it. This is likely a mistake.");
            foreach (Expression item in listDef.Items)
            {
                this.CompileExpression(parser, buffer, item, true);
            }
            buffer.Add(listDef.FirstToken, OpCode.DEF_LIST, listDef.Items.Length);
        }

        private void CompileLibraryFunctionCall(ParserContext parser, ByteBuffer buffer, LibraryFunctionCall libFunc, List<Expression> argsOverrideOrNull, Token parenTokenOverride, bool outputUsed)
        {
            List<Expression> args = argsOverrideOrNull ?? new List<Expression>(libFunc.Args);
            this.CompileExpressionList(parser, buffer, args, true);
            int argCount = libFunc.Args.Length;
            Token token = parenTokenOverride ?? libFunc.FirstToken;
            string libraryName = libFunc.LibraryName;
            LibraryMetadata library = libFunc.TopLevelEntity.Library;
            int libraryRefId = parser.LibraryManager.GetLibraryReferenceIdFromKey(library.CanonicalKey);
            int functionNameReferenceId = parser.LiteralLookup.GetLibFuncRefId(libFunc.Name);

            buffer.Add(token,
                OpCode.CALL_LIB_FUNCTION_DYNAMIC,
                functionNameReferenceId,
                libraryRefId,
                argCount,
                outputUsed ? 1 : 0);
        }

        private void CompileNegativeSign(ParserContext parser, ByteBuffer buffer, NegativeSign negativeSign, bool outputUsed)
        {
            if (!outputUsed) throw new ParserException(negativeSign, "This expression does nothing.");
            this.CompileExpression(parser, buffer, negativeSign.Root, true);
            buffer.Add(negativeSign.FirstToken, OpCode.NEGATIVE_SIGN);
        }

        private void CompileStringConstant(ParserContext parser, ByteBuffer buffer, StringConstant stringConstant, bool outputUsed)
        {
            if (!outputUsed) throw new ParserException(stringConstant, "This expression does nothing.");
            buffer.Add(stringConstant.FirstToken, OpCode.LITERAL, parser.GetStringConstant(stringConstant.Value));
        }

        private void CompileBinaryOpChain(ParserContext parser, ByteBuffer buffer, OpChain opChain, bool outputUsed)
        {
            if (!outputUsed)
            {
                if (opChain.Op.Value == "==")
                {
                    throw new ParserException(opChain.Op, "'==' cannot be used like this. Did you mean to use just a single '=' instead?");
                }
                throw new ParserException(opChain, "This expression isn't valid here.");
            }

            this.CompileExpressionList(parser, buffer, new Expression[] { opChain.Left, opChain.Right }, true);

            Token opToken = opChain.Op;
            switch (opToken.Value)
            {
                case "+": buffer.Add(opToken, OpCode.BINARY_OP, (int)BinaryOps.ADDITION); break;
                case "<": buffer.Add(opToken, OpCode.BINARY_OP, (int)BinaryOps.LESS_THAN); break;
                case "<=": buffer.Add(opToken, OpCode.BINARY_OP, (int)BinaryOps.LESS_THAN_OR_EQUAL); break;
                case ">": buffer.Add(opToken, OpCode.BINARY_OP, (int)BinaryOps.GREATER_THAN); break;
                case ">=": buffer.Add(opToken, OpCode.BINARY_OP, (int)BinaryOps.GREATER_THAN_OR_EQUAL); break;
                case "-": buffer.Add(opToken, OpCode.BINARY_OP, (int)BinaryOps.SUBTRACTION); break;
                case "*": buffer.Add(opToken, OpCode.BINARY_OP, (int)BinaryOps.MULTIPLICATION); break;
                case "/": buffer.Add(opToken, OpCode.BINARY_OP, (int)BinaryOps.DIVISION); break;
                case "%": buffer.Add(opToken, OpCode.BINARY_OP, (int)BinaryOps.MODULO); break;
                case "**": buffer.Add(opToken, OpCode.BINARY_OP, (int)BinaryOps.EXPONENT); break;
                case "|": buffer.Add(opToken, OpCode.BINARY_OP, (int)BinaryOps.BITWISE_OR); break;
                case "&": buffer.Add(opToken, OpCode.BINARY_OP, (int)BinaryOps.BITWISE_AND); break;
                case "^": buffer.Add(opToken, OpCode.BINARY_OP, (int)BinaryOps.BITWISE_XOR); break;
                case "<<": buffer.Add(opToken, OpCode.BINARY_OP, (int)BinaryOps.BIT_SHIFT_LEFT); break;
                case ">>": buffer.Add(opToken, OpCode.BINARY_OP, (int)BinaryOps.BIT_SHIFT_RIGHT); break;
                case "==": buffer.Add(opToken, OpCode.EQUALS, 0); break;
                case "!=": buffer.Add(opToken, OpCode.EQUALS, 1); break;
                default: throw new NotImplementedException("Binary op: " + opChain.Op.Value);
            }

            if (!outputUsed)
            {
                buffer.Add(null, OpCode.POP);
            }
        }

        private void CompileBracketIndex(ParserContext parser, ByteBuffer buffer, BracketIndex bracketIndex, bool outputUsed)
        {
            if (!outputUsed) throw new ParserException(bracketIndex, "This expression does nothing.");
            this.CompileExpression(parser, buffer, bracketIndex.Root, true);
            this.CompileExpression(parser, buffer, bracketIndex.Index, true);
            buffer.Add(bracketIndex.BracketToken, OpCode.INDEX);
        }

        private void CompileDotStep(ParserContext parser, ByteBuffer buffer, DotField dotStep, bool outputUsed)
        {
            if (!outputUsed) throw new ParserException(dotStep, "This expression does nothing.");
            this.CompileExpression(parser, buffer, dotStep.Root, true);
            int rawNameId = parser.GetId(dotStep.StepToken.Value);
            int localeId = parser.GetLocaleId(dotStep.Owner.FileScope.CompilationScope.Locale);
            int localeScopedNameId = rawNameId * parser.GetLocaleCount() + localeId;
            buffer.Add(dotStep.DotToken, OpCode.DEREF_DOT, rawNameId, localeScopedNameId);
        }

        private void CompileBooleanConstant(ParserContext parser, ByteBuffer buffer, BooleanConstant boolConstant, bool outputUsed)
        {
            if (!outputUsed) throw new ParserException(boolConstant, "This expression does nothing.");
            buffer.Add(boolConstant.FirstToken, OpCode.LITERAL, parser.GetBoolConstant(boolConstant.Value));
        }

        private void CompileVariable(ParserContext parser, ByteBuffer buffer, Variable variable, bool outputUsed)
        {
            if (!outputUsed) throw new ParserException(variable, "This expression does nothing.");
            int nameId = parser.GetId(variable.Name);
            Token token = variable.FirstToken;
            VariableId varId = variable.LocalScopeId;
            if (varId == null)
            {
                throw new ParserException(token, "Variable used but not declared.");
            }
            bool isClosureVar = varId.UsedByClosure;
            buffer.Add(token, isClosureVar ? OpCode.DEREF_CLOSURE : OpCode.LOCAL, isClosureVar ? varId.ClosureID : varId.ID, nameId);
        }

        private void CompileIntegerConstant(ParserContext parser, ByteBuffer buffer, IntegerConstant intConst, bool outputUsed)
        {
            if (!outputUsed) throw new ParserException(intConst, "This expression does nothing.");
            buffer.Add(intConst.FirstToken, OpCode.LITERAL, parser.GetIntConstant(intConst.Value));
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

        private void CompileInlinedLibraryFunctionCall(
            ParserContext parser,
            ByteBuffer buffer,
            FunctionCall userWrittenOuterFunctionCall,
            FunctionDefinition embedFunctionBeingInlined,
            bool outputUsed)
        {
            Expression coreOrLibFunctionBeingInlined = ((ReturnStatement)embedFunctionBeingInlined.Code[0]).Expression;

            int embedFuncLength = embedFunctionBeingInlined.ArgNames.Length;
            int userProvidedArgLength = userWrittenOuterFunctionCall.Args.Length;
            if (userProvidedArgLength > embedFuncLength)
            {
                // TODO: can this be removed? Isn't this caught elsewhere?
                throw new ParserException(userWrittenOuterFunctionCall.ParenToken, "More arguments were passed to this function than allowed.");
            }

            // First go through the args provided and map them to the arg names of the embed function that's being inlined.
            Dictionary<string, Expression> userProvidedAndImplicitArgumentsByArgName = new Dictionary<string, Expression>();
            for (int i = 0; i < embedFuncLength; ++i)
            {
                Expression argValue;
                if (i < userProvidedArgLength)
                {
                    argValue = userWrittenOuterFunctionCall.Args[i];
                }
                else
                {
                    argValue = embedFunctionBeingInlined.DefaultValues[i];
                    if (argValue == null)
                    {
                        throw new ParserException(userWrittenOuterFunctionCall.ParenToken, "Not enough arguments were supplied to this function.");
                    }
                }
                userProvidedAndImplicitArgumentsByArgName[embedFunctionBeingInlined.ArgNames[i].Value] = argValue;
            }

            LibraryFunctionCall nativeLibraryFunctionCall = coreOrLibFunctionBeingInlined as LibraryFunctionCall;
            CoreFunctionInvocation coreFunctionCall = coreOrLibFunctionBeingInlined as CoreFunctionInvocation;
            if (nativeLibraryFunctionCall == null && coreFunctionCall == null)
            {
                throw new InvalidOperationException(); // This shouldn't happen. The body of the library function should have been verified by the resolver before getting to this state.
            }

            Expression[] innermostArgList = nativeLibraryFunctionCall != null
                ? nativeLibraryFunctionCall.Args
                : coreFunctionCall.Args;

            // This is the new list of arguments that will be passed to the inner underlying lib/core function.
            List<Expression> finalArguments = new List<Expression>();

            Expression arg;
            for (int i = 0; i < innermostArgList.Length; ++i)
            {
                arg = innermostArgList[i];
                if (arg is Variable)
                {
                    string argName = ((Variable)innermostArgList[i]).Name;
                    finalArguments.Add(userProvidedAndImplicitArgumentsByArgName[argName]);
                }
                else if (arg.IsLiteral || arg is FieldReference)
                {
                    finalArguments.Add(arg);
                }
                else
                {
                    throw new NotImplementedException();
                }
            }

            if (nativeLibraryFunctionCall != null)
            {
                this.CompileLibraryFunctionCall(
                    parser,
                    buffer,
                    nativeLibraryFunctionCall,
                    finalArguments,
                    userWrittenOuterFunctionCall.ParenToken,
                    outputUsed);
            }
            else
            {
                this.CompileCoreFunctionInvocation(
                    parser,
                    buffer,
                    coreFunctionCall,
                    finalArguments.ToArray(),
                    userWrittenOuterFunctionCall.ParenToken,
                    outputUsed);
            }
        }

        private void CompileFunctionCall(ParserContext parser, ByteBuffer buffer, FunctionCall funCall, bool outputUsed)
        {
            bool argCountIsNegativeOne = false;
            FunctionDefinition ownerFunction = funCall.Owner as FunctionDefinition;
            if (ownerFunction != null &&
                ownerFunction.NameToken.Value == "_LIB_CORE_invoke" &&
                ownerFunction.FileScope.CompilationScope.Dependencies.Length == 0)
            {
                argCountIsNegativeOne = true;
            }

            Expression root = funCall.Root;
            if (root is FunctionReference)
            {
                FunctionReference verifiedFunction = (FunctionReference)root;
                FunctionDefinition fd = verifiedFunction.FunctionDefinition;

                if (parser.InlinableLibraryFunctions.Contains(fd))
                {
                    this.CompileInlinedLibraryFunctionCall(parser, buffer, funCall, fd, outputUsed);
                }
                else
                {
                    this.CompileExpressionList(parser, buffer, funCall.Args, true);
                    if (fd.Owner is ClassDefinition)
                    {
                        ClassDefinition cd = (ClassDefinition)fd.Owner;
                        if (fd.IsStaticMethod)
                        {
                            buffer.Add(
                                funCall.ParenToken,
                                OpCode.CALL_FUNCTION,
                                (int)FunctionInvocationType.STATIC_METHOD,
                                funCall.Args.Length,
                                fd.FunctionID,
                                outputUsed ? 1 : 0,
                                cd.ClassID);
                        }
                        else
                        {
                            buffer.Add(
                                funCall.ParenToken,
                                OpCode.CALL_FUNCTION,
                                (int)FunctionInvocationType.LOCAL_METHOD,
                                funCall.Args.Length,
                                fd.FunctionID,
                                outputUsed ? 1 : 0,
                                cd.ClassID,
                                verifiedFunction.FunctionDefinition.MemberID);
                        }
                    }
                    else
                    {
                        // vanilla function
                        buffer.Add(
                            funCall.ParenToken,
                            OpCode.CALL_FUNCTION,
                            (int)FunctionInvocationType.NORMAL_FUNCTION,
                            funCall.Args.Length,
                            fd.FunctionID,
                            outputUsed ? 1 : 0,
                            0);
                    }
                }
            }
            else if (root is DotField)
            {
                DotField ds = (DotField)root;
                Expression dotRoot = ds.Root;
                int globalNameId = parser.GetId(ds.StepToken.Value);
                this.CompileExpression(parser, buffer, dotRoot, true);
                this.CompileExpressionList(parser, buffer, funCall.Args, true);
                int localeId = parser.GetLocaleId(ds.Owner.FileScope.CompilationScope.Locale);
                buffer.Add(
                    funCall.ParenToken,
                    OpCode.CALL_FUNCTION,
                    (int)FunctionInvocationType.FIELD_INVOCATION,
                    funCall.Args.Length,
                    0,
                    outputUsed ? 1 : 0,
                    globalNameId,
                    localeId);
            }
            else if (root is BaseMethodReference)
            {
                BaseMethodReference bmr = (BaseMethodReference)root;
                FunctionDefinition fd = bmr.ClassToWhichThisMethodRefers.GetMethod(bmr.StepToken.Value, true);
                if (fd == null)
                {
                    throw new ParserException(bmr.DotToken, "This method does not exist on any base class.");
                }

                this.CompileExpressionList(parser, buffer, funCall.Args, true);
                buffer.Add(
                    funCall.ParenToken,
                    OpCode.CALL_FUNCTION,
                    (int)FunctionInvocationType.LOCAL_METHOD,
                    funCall.Args.Length,
                    fd.FunctionID,
                    outputUsed ? 1 : 0,
                    bmr.ClassToWhichThisMethodRefers.ClassID,
                    -1);
            }
            else
            {
                this.CompileExpression(parser, buffer, root, true);
                this.CompileExpressionList(parser, buffer, funCall.Args, true);
                buffer.Add(
                    funCall.ParenToken,
                    OpCode.CALL_FUNCTION,
                    (int)FunctionInvocationType.POINTER_PROVIDED,
                    argCountIsNegativeOne ? -1 : funCall.Args.Length,
                    0,
                    outputUsed ? 1 : 0,
                    0);
            }
        }
    }
}
