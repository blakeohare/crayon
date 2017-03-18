using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crayon.ParseTree;
using Common;

namespace Crayon.Translator.Pastel
{
    class PastelPlatform : AbstractPlatform
    {
        public class PastelShouldNeverEverAccessThisException : Exception
        {
            public PastelShouldNeverEverAccessThisException(string message) : base(message) { }
        }

        public PastelPlatform() : base(PlatformId.PASTEL_VM, LanguageId.PASTEL, new PastelTranslator(), new PastelSystemFunctionTranslator())
        {

        }

        public override bool IntIsFloor { get { throw new PastelShouldNeverEverAccessThisException("IntIsFloor"); } }
        public override bool IsArraySameAsList { get { throw new PastelShouldNeverEverAccessThisException("IsArraySameAsList"); } }
        public override bool IsAsync { get { throw new PastelShouldNeverEverAccessThisException("IsAsync"); } }
        public override bool IsCharANumber { get { throw new PastelShouldNeverEverAccessThisException("IsCharANumber"); } }
        public override bool IsStronglyTyped { get { throw new PastelShouldNeverEverAccessThisException("IsStronglyTyped"); } }
        public override bool IsThreadBlockingAllowed { get { throw new PastelShouldNeverEverAccessThisException("IsThreadBlockingAllowed"); } }
        public override string PlatformShortId { get { throw new PastelShouldNeverEverAccessThisException("pastel"); } }
        public override bool SupportsListClear { get { throw new PastelShouldNeverEverAccessThisException("SupportsListClear"); } }

        private Token MakeAToken(string value)
        {
            return new Token(value, 0, "", 0, 0, false);
        }

        private Annotation MakeATypeAnnotation(string type)
        {
            Token typeToken = MakeAToken("type");
            return new Annotation(typeToken, typeToken, new Expression[] { new StringConstant(typeToken, type, null) });
        }

        public override Dictionary<string, FileOutput> Package(
            BuildContext buildContext,
            string projectId,
            Dictionary<string, Executable[]> finalCode,
            ICollection<StructDefinition> structDefinitions,
            ResourceDatabase resourceDatabase,
            SystemLibraryManager libraryManager)
        {
            Library coreLibrary = libraryManager.GetLibraryFromKey("core");
            Dictionary<string, FileOutput> output = new Dictionary<string, FileOutput>();

            Dictionary<string, Dictionary<string, int>> argCountByFunctionNameByLibraryName = new Dictionary<string, Dictionary<string, int>>();

            foreach (string vmFileImmutable in finalCode.Keys)
            {
                string vmFile = vmFileImmutable; // need to change this variable, potentially

                if (vmFile == "Globals")
                {
                    foreach (Assignment aGlobal in finalCode[vmFile].OfType<Assignment>())
                    {
                        aGlobal.HACK_IsVmGlobal = true;
                    }
                }

                Executable[] parseTree = finalCode[vmFile];
                if (vmFile.StartsWith("Libraries/") && vmFile.Contains("/translate/"))
                {
                    string[] parts = vmFile.Split('/');
                    string libraryName = parts[1];
                    string functionName = parts[3];

                    if (functionName.StartsWith("addImageRenderEvent"))
                    {
                        if (functionName.EndsWith("ForPastel"))
                        {
                            functionName = "addImageRenderEvent";
                            vmFile = vmFile.Replace("ForPastel", "");
                        }
                        else
                        {
                            continue;
                        }
                    }

                    List<Executable> functionBody = new List<Executable>(parseTree);
                    int argCount = 0;
                    if (functionBody.Count > 0 && functionBody[0] is ImportInlineStatement)
                    {
                        ImportInlineStatement argsImport = (ImportInlineStatement)functionBody[0];
                        string path = argsImport.Path;
                        argCount = int.Parse(path.Split('/')[1].Split('_')[0]);
                        functionBody.RemoveAt(0);
                        List<Executable> argAssignmentPrefixes = new List<Executable>();
                        for (int i = 1; i <= argCount; ++i)
                        {
                            string name = "arg" + i;
                            Variable targetVariable = new Variable(MakeAToken(name), name, null)
                            {
                                Annotations = new Dictionary<string, Annotation>() {
                                    { "type", MakeATypeAnnotation("Value") }
                                },
                            };
                            SystemFunctionCall valueExpression = new SystemFunctionCall(
                                MakeAToken("$_array_get"),
                                new Expression[] {
                                    new Variable(MakeAToken("args"), "args", null),
                                    new IntegerConstant(MakeAToken("" + (i - 1)), i - 1, null)
                                },
                                null);
                            valueExpression.HACK_CoreLibraryReference = coreLibrary;
                            Assignment argAssign = new Assignment(
                                targetVariable,
                                MakeAToken("="), "=",
                                valueExpression,
                                null);
                            argAssignmentPrefixes.Add(argAssign);
                        }
                        argAssignmentPrefixes.AddRange(functionBody);
                        functionBody = argAssignmentPrefixes;
                    }

                    Assignment outputAssignment = new Assignment(
                        new Variable(MakeAToken("output"), "output", null)
                        {
                            Annotations = new Dictionary<string, Annotation>() { { "type", MakeATypeAnnotation("Value") } },
                        },
                        MakeAToken("="), "=",
                        new Variable(MakeAToken("VALUE_NULL"), "VALUE_NULL", null),
                        null);
                    functionBody.Insert(0, outputAssignment);

                    Dictionary<string, int> argCountLookup;
                    if (!argCountByFunctionNameByLibraryName.TryGetValue(libraryName, out argCountLookup))
                    {
                        argCountLookup = new Dictionary<string, int>();
                        argCountByFunctionNameByLibraryName[libraryName] = argCountLookup;
                    }

                    argCountLookup[functionName] = argCount;

                    for (int i = 0; i < functionBody.Count; ++i)
                    {
                        if (functionBody[i] is ImportInlineStatement)
                        {
                            // get rid of or whitelist these!
                            throw new Exception();
                        }
                        else if (functionBody[i] is IfStatement)
                        {
                            IfStatement ifStatement = (IfStatement)functionBody[i];
                            // it's not perfect, but hopefully this will catch most of the sneaky ones
                            foreach (Executable[] nestedBlock in new Executable[][] { ifStatement.TrueCode, ifStatement.FalseCode })
                            {
                                for (int j = 0; j < nestedBlock.Length; ++j)
                                {
                                    if (nestedBlock[j] is ImportInlineStatement)
                                    {
                                        // no no no
                                        throw new Exception();
                                    }
                                }
                            }
                        }
                    }

                    this.InjectVariableDeclarations(functionBody, libraryName, functionName);

                    ReturnStatement last = functionBody[functionBody.Count - 1] as ReturnStatement;
                    if (last != null && last.Expression is FunctionCall)
                    {
                        Variable fp = ((FunctionCall)last.Expression).Root as Variable;
                        if (fp != null && fp.Name == "suspendInterpreter")
                        {
                            Expression expr = new FunctionCall(
                                   new Variable(MakeAToken("Core.VmSuspend"), "Core.VmSuspend", null), // yeah I'm lazy
                                   MakeAToken("("),
                                   new Expression[0],
                                   null);
                            functionBody[functionBody.Count - 1] = new ExpressionAsExecutable(expr, null);
                        }
                    }

                    functionBody.Add(new ReturnStatement(
                        MakeAToken("return"),
                        new Variable(MakeAToken("output"), "output", null),
                        null));

                    FunctionDefinition newFunction = new FunctionDefinition(
                        MakeAToken("yep"),
                        null,
                        true,
                        MakeAToken("lib_" + libraryName.ToLower() + "_function_" + functionName),
                        new List<Annotation>() { MakeATypeAnnotation("Value") },
                        null)
                    {
                        ArgAnnotations = new Annotation[] { MakeATypeAnnotation("Array<Value>") },
                        ArgNames = new Token[] { MakeAToken("args") },
                        Code = functionBody.ToArray(),
                        DefaultValues = new Expression[] { null },
                    };
                    parseTree = new Executable[] { newFunction };
                }

                List<string> text = new List<string>();
                output[vmFile + ".pst"] = new FileOutput()
                {
                    Type = FileOutputType.Text,
                    TextContent = this.Translator.Translate(parseTree)
                };
            }

            foreach (string libraryName in argCountByFunctionNameByLibraryName.Keys)
            {
                // just generate the output directly instead of using the translator. It's simpler.
                List<string> manifestCode = new List<string>()
                {
                    "void lib_manifest_RegisterFunctions(object libRegData) {\n"
                };
                Dictionary<string, int> argCountByFunctionName = argCountByFunctionNameByLibraryName[libraryName];
                string[] functionNames = argCountByFunctionName.Keys.OrderBy<string, string>(s => s).ToArray();
                foreach (string functionName in functionNames)
                {
                    int argCount = argCountByFunctionName[functionName];
                    manifestCode.Add("\tCore.RegisterLibraryFunction(libRegData, \"" + functionName + "\", " + argCount + ");\n");
                }
                manifestCode.Add("}\n");

                output["Libraries/" + libraryName + "/function_registry.pst"] = new FileOutput()
                {
                    Type = FileOutputType.Text,
                    TextContent = string.Join("", manifestCode),
                };
            }

            return output;
        }

        private string GetDeclarationsToInsert(string key)
        {
            switch (key)
            {
                case "Audio.music_load_from_resource": return "string1 objInstance1";
                case "Audio.music_play": return "objInstance1 bool1 string1 float1 bool2";
                case "Audio.music_set_volume": return "float1";
                case "Audio.music_stop": return "int1";
                case "Audio.sfx_get_state": return "objInstance1 object1 object2 int1";
                case "Audio.sfx_play": return "objInstance1 objInstance2 float1 float2 object1 int1";
                case "Audio.sfx_resume": return "objInstance1 object1 object2 float1 float2";
                case "Audio.sfx_set_pan": return "objInstance1 object1 object2 float1";
                case "Audio.sfx_set_volume": return "objInstance1 object1 object2 float1";
                case "Audio.sfx_stop": return "objInstance1 object1 object2 int1 int2 bool1 bool2";

                case "FileIOCommon.directoryCreate": return "string1 int1 bool1 stringList1 i";
                case "FileIOCommon.directoryDelete": return "int1";
                case "FileIOCommon.directoryList": return "stringList1 int1 list1 i";
                case "FileIOCommon.directoryMove": return "int1";
                case "FileIOCommon.fileDelete": return "int1";
                case "FileIOCommon.fileInfo": return "int1 list1";
                case "FileIOCommon.fileMove": return "int1";
                case "FileIOCommon.fileRead": return "int1 bool1 list1";
                case "FileIOCommon.fileWrite": return "string1 object1 int1 int2";
                case "FileIOCommon.initializeDisk": return "objInstance1 objArray1 object1";

                case "Game.event_queue_get": return "objInstance1 i";
                case "Game.initialize": return "float1";
                case "Game.initialize_screen": return "int1 int2 int3 int4";

                case "Gamepad.current_device_count": return "int1";
                case "Gamepad.get_axis_1d_state": return "objInstance1 int1";
                case "Gamepad.get_axis_2d_state": return "objInstance1 int1 list1";
                case "Gamepad.get_button_state": return "objInstance1 int1";
                case "Gamepad.get_save_file_path": return "string1";
                case "Gamepad.initialize_device": return "int1 objInstance1 list1 object1";

                case "Graphics2D.addImageRenderEvent": return "objInstance1 objArray1 len intArray1 int1 intArray2 i objArrayArray1 objArrayArray2 value objInstance2 bool1 bool2 float1 bool1 int2 int3";
                case "Graphics2D.flip": return "objInstance1 objInstance2 objArray1 objArray2 bool1 bool2 i object1";
                case "Graphics2D.initializeTexture": return "objInstance1 objArray1 list1 value float1 float2";
                case "Graphics2D.initializeTextureResource": return "objInstance1 objArray1 objArray2";
                case "Graphics2D.lineToQuad": return "value objInstance1 objArray1 intArray1 len int1 int2 int3 int4 int5 float1 float2 float3 i j";
                case "Graphics2D.renderQueueAction": return "len objInstance1 objArray1 intArray1 intList1 i list1 value";
                case "Graphics2D.scale": return "int1 int2 objInstance1 object1 objArray1 objInstance2 objArray2 i";

                case "ImageResources.blit": return "objInstance1 objInstance2 objArray1 object1";
                case "ImageResources.checkLoaderIsDone": return "objInstance1 objInstance2 list1";
                case "ImageResources.flushImageChanges": return "objInstance1 objArray1 object1";
                case "ImageResources.loadAsynchronous": return "objInstance1 objInstance2 string1 objArray1 objArray2";
                case "ImageResources.loadSynchronous": return "objInstance1 string1 list1 objArray1";
                case "ImageResources.nativeImageDataInit": return "objInstance1 objArray1 int1 int2";

                case "Random.random_int": return "int1 int2";

                case "Resources.readText": return "string1";

                case "UserData.getProjectSandboxDirectory": return "string1 string2";

                default: return null;
            }
        }

        // Things I keep telling myself:
        // - "This is temporary"
        // - "This is all going into a branch"
        private void InjectVariableDeclarations(List<Executable> someCodeToAppendTo, string libraryName, string functionName)
        {
            string key = libraryName + "." + functionName;
            string varsToDeclare = GetDeclarationsToInsert(key);

            if (varsToDeclare == null) return;
            string[] varsArray = new HashSet<string>(varsToDeclare.Trim().Split(' ')).OrderBy<string, string>(s => s).ToArray();
            List<Executable> addTheseToTheTop = new List<Executable>();

            foreach (string varToDeclare in varsArray)
            {
                switch (varToDeclare)
                {
                    case "bool1":
                    case "bool2":
                    case "bool3":
                        addTheseToTheTop.Add(new Assignment(
                            MakeAVariableWithType(varToDeclare, "bool"),
                            MakeAToken("="), "=",
                            new BooleanConstant(MakeAToken("false"), false, null),
                            null));
                        break;

                    case "i":
                    case "j":
                    case "len":
                    case "int1":
                    case "int2":
                    case "int3":
                    case "int4":
                    case "int5":
                        addTheseToTheTop.Add(new Assignment(
                            MakeAVariableWithType(varToDeclare, "int"),
                            MakeAToken("="), "=",
                            new IntegerConstant(MakeAToken("0"), 0, null),
                            null));
                        break;

                    case "float1":
                    case "float2":
                    case "float3":
                        addTheseToTheTop.Add(new Assignment(
                            MakeAVariableWithType(varToDeclare, "double"),
                            MakeAToken("="), "=",
                            new FloatConstant(MakeAToken("0.0"), 0.0, null),
                            null));
                        break;

                    case "string1":
                    case "string2":
                    case "string3":
                        addTheseToTheTop.Add(new Assignment(
                            MakeAVariableWithType(varToDeclare, "string"),
                            MakeAToken("="), "=",
                            new StringConstant(MakeAToken("\"\""), "", null),
                            null));
                        break;

                    case "object1":
                    case "object2":
                    case "object3":
                        addTheseToTheTop.Add(new Assignment(
                            MakeAVariableWithType(varToDeclare, "object"),
                            MakeAToken("="), "=",
                            new NullConstant(MakeAToken(null), null),
                            null));
                        break;

                    case "stringList1":
                    case "stringList2":
                        addTheseToTheTop.Add(new Assignment(
                            MakeAVariableWithType(varToDeclare, "List<string>"),
                            MakeAToken("="), "=",
                            new NullConstant(MakeAToken(null), null),
                            null));
                        break;

                    case "value":
                    case "value2":
                    case "value3":
                        addTheseToTheTop.Add(new Assignment(
                            MakeAVariableWithType(varToDeclare, "Value"),
                            MakeAToken("="), "=",
                            new NullConstant(MakeAToken(null), null),
                            null));
                        break;

                    case "list1":
                        addTheseToTheTop.Add(new Assignment(
                            MakeAVariableWithType(varToDeclare, "List<Value>"),
                            MakeAToken("="), "=",
                            new NullConstant(MakeAToken(null), null),
                            null));
                        break;

                    case "intList1":
                        addTheseToTheTop.Add(new Assignment(
                            MakeAVariableWithType(varToDeclare, "List<int>"),
                            MakeAToken("="), "=",
                            new NullConstant(MakeAToken(null), null),
                            null));
                        break;

                    case "objInstance1":
                    case "objInstance2":
                    case "objInstance3":
                        addTheseToTheTop.Add(new Assignment(
                            MakeAVariableWithType(varToDeclare, "ObjectInstance"),
                            MakeAToken("="), "=",
                            new NullConstant(MakeAToken("null"), null),
                            null));
                        break;

                    case "intArray1":
                    case "intArray2":
                        addTheseToTheTop.Add(new Assignment(
                            MakeAVariableWithType(varToDeclare, "Array<int>"),
                            MakeAToken("="), "=",
                            new NullConstant(MakeAToken("null"), null),
                            null));
                        break;

                    case "objArray1":
                    case "objArray2":
                        addTheseToTheTop.Add(new Assignment(
                            MakeAVariableWithType(varToDeclare, "Array<object>"),
                            MakeAToken("="), "=",
                            new NullConstant(MakeAToken("null"), null),
                            null));
                        break;

                    case "objArrayArray1":
                    case "objArrayArray2":
                        addTheseToTheTop.Add(new Assignment(
                            MakeAVariableWithType(varToDeclare, "Array<Array<object>>"),
                            MakeAToken("="), "=",
                            new NullConstant(MakeAToken("null"), null),
                            null));
                        break;

                    default:
                        throw new NotImplementedException("Not defined: " + varToDeclare);
                }
            }

            someCodeToAppendTo.InsertRange(0, addTheseToTheTop);
        }

        private Variable MakeAVariableWithType(string name, string type)
        {
            return new Variable(MakeAToken(name), name, null)
            {
                Annotations = new Dictionary<string, Annotation>()
                {
                    { "type", MakeATypeAnnotation(type) },
                },
            };
        }
    }
}
