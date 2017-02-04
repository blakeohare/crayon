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
            Dictionary<string, FileOutput> output = new Dictionary<string, FileOutput>();

            Dictionary<string, Dictionary<string, int>> argCountByFunctionNameByLibraryName = new Dictionary<string, Dictionary<string, int>>();

            foreach (string vmFile in finalCode.Keys)
            {
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

                    List<Executable> functionBody = new List<Executable>(parseTree);
                    int argCount = 0;
                    if (functionBody.Count > 0 && functionBody[0] is ImportInlineStatement)
                    {
                        ImportInlineStatement argsImport = (ImportInlineStatement)functionBody[0];
                        string path = argsImport.Path;
                        argCount = int.Parse(path.Split('/')[1].Split('_')[0]);
                        functionBody.RemoveAt(0);
                    }

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
                    "void lib_manifest_RegisterFunctions(List<object> functionPointers, List<string> functionNames, List<int> argCounts) {\n"
                };
                Dictionary<string, int> argCountByFunctionName = argCountByFunctionNameByLibraryName[libraryName];
                string[] functionNames = argCountByFunctionName.Keys.OrderBy<string, string>(s => s).ToArray();
                foreach (string functionName in functionNames)
                {
                    int argCount = argCountByFunctionName[functionName];
                    manifestCode.Add("\tCore.RegisterLibraryFunction(functionPointers, argCounts, \"" + functionName + "\", " + argCount + ");\n");
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
    }
}
