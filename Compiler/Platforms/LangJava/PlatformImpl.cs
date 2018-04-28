using Common;
using Pastel.Transpilers;
using Platform;
using System;
using System.Collections.Generic;

namespace LangJava
{
    public class PlatformImpl : AbstractPlatform
    {
        public override string Name { get { return "lang-java"; } }
        public override string InheritsFrom { get { return null; } }
        public override string NL { get { return "\n"; } }

        public PlatformImpl()
            : base(Pastel.Language.JAVA)
        { }

        public override void ExportStandaloneVm(
            Dictionary<string, FileOutput> output,
            Pastel.PastelContext pastelContext,
            IList<LibraryForExport> everyLibrary,
            ILibraryNativeInvocationTranslatorProvider libraryNativeInvocationTranslatorProviderForPlatform)
        {
            throw new NotImplementedException();
        }

        public override void ExportProject(
            Dictionary<string, FileOutput> output,
            Pastel.PastelContext pastelContext,
            IList<LibraryForExport> libraries,
            ResourceDatabase resourceDatabase,
            Options options,
            ILibraryNativeInvocationTranslatorProvider libraryNativeInvocationTranslatorProviderForPlatform)
        {
            throw new NotImplementedException();
        }

        public static void ExportJavaLibraries(
            AbstractPlatform platform,
            string srcPath,
            IList<LibraryForExport> libraries,
            Dictionary<string, FileOutput> output,
            ILibraryNativeInvocationTranslatorProvider libraryNativeInvocationTranslatorProviderForPlatform,
            string[] extraImports)
        {
            TranspilerContext ctx = new TranspilerContext(platform.Language);
            List<string> defaultImports = new List<string>()
            {
                "import java.util.ArrayList;",
                "import java.util.HashMap;",
                "import org.crayonlang.interpreter.FastList;",
                "import org.crayonlang.interpreter.Interpreter;",
                "import org.crayonlang.interpreter.LibraryFunctionPointer;",
                "import org.crayonlang.interpreter.TranslationHelper;",
                "import org.crayonlang.interpreter.VmGlobal;",
                "import org.crayonlang.interpreter.structs.*;",
            };

            defaultImports.AddRange(extraImports);
            defaultImports.Sort();

            foreach (LibraryForExport library in libraries)
            {
                if (library.ManifestFunctionDEPRECATED != null)
                {
                    ctx.CurrentLibraryFunctionTranslator = libraryNativeInvocationTranslatorProviderForPlatform.GetTranslator(library.Name);

                    List<string> libraryCode = new List<string>()
                    {
                        "package org.crayonlang.libraries." + library.Name.ToLower() + ";",
                        "",
                    };
                    libraryCode.AddRange(defaultImports);
                    libraryCode.AddRange(new string[]
                    {
                        "",
                        "public final class LibraryWrapper {",
                        "  private LibraryWrapper() {}",
                        "",
                    });

                    ctx.TabDepth = 1;

                    Pastel.PastelCompiler libCompiler = library.PastelContext.CompilerDEPRECATED;

                    string reflectionCalledPrefix = "lib_" + library.Name.ToLower() + "_function_";
                    string manifestFunctionCode = libCompiler.GetFunctionCodeForSpecificFunctionAndPopItFromFutureSerializationTEMP(
                        "lib_manifest_RegisterFunctions",
                        null,
                        ctx,
                        "");
                    libraryCode.Add(manifestFunctionCode);
                    Dictionary<string, string> lookup = libCompiler.GetFunctionCodeAsLookupTEMP(ctx, "  ");

                    foreach (string functionName in lookup.Keys)
                    {
                        string functionCode = lookup[functionName];
                        bool isFunctionPointerObject = functionName.StartsWith(reflectionCalledPrefix);

                        if (isFunctionPointerObject)
                        {
                            // This is kind of hacky, BUT...

                            // If the generated function needs to be used as a function pointer, (i.e. it's one
                            // of the library's VM-native bridge methods) change the name to "invoke" and then
                            // wrap it in a dummy class that extends LibraryFunctionPointer. The manifest
                            // function will simply instantiate this in lieu of a performant way to do
                            // function pointers in Java.
                            functionCode = functionCode.Replace(
                                "public static Value v_" + functionName + "(Value[] ",
                                "public Value invoke(Value[] ");
                            functionCode =
                                "  public static class FP_" + functionName + " extends LibraryFunctionPointer {\n" +
                                "  " + functionCode.Replace("\n", "\n  ").TrimEnd() + "\n" +
                                "  }\n";
                        }
                        libraryCode.Add(functionCode);
                    }

                    ctx.TabDepth = 0;
                    libraryCode.Add("}");
                    libraryCode.Add("");

                    string libraryPath = srcPath + "/org/crayonlang/libraries/" + library.Name.ToLower();

                    output[libraryPath + "/LibraryWrapper.java"] = new FileOutput()
                    {
                        Type = FileOutputType.Text,
                        TextContent = string.Join(platform.NL, libraryCode),
                    };

                    Dictionary<string, string> libStructs = libCompiler.GetStructCodeByClassTEMP(ctx, "  ");

                    foreach (string structName in libStructs.Keys)
                    {
                        string structCode = libStructs[structName];

                        structCode = WrapStructCodeWithImports(platform.NL, structCode);

                        // This is kind of a hack.
                        // TODO: better.
                        structCode = structCode.Replace(
                            "package org.crayonlang.interpreter.structs;",
                            "package org.crayonlang.libraries." + library.Name.ToLower() + ";");

                        output[libraryPath + "/" + structName + ".java"] = new FileOutput()
                        {
                            Type = FileOutputType.Text,
                            TextContent = structCode,
                        };
                    }

                    foreach (ExportEntity supFile in library.ExportEntities["COPY_CODE"])
                    {
                        string path = supFile.Values["target"].Replace("%LIBRARY_PATH%", libraryPath);
                        output[path] = supFile.FileOutput;
                    }
                }
            }
        }

        public static string WrapStructCodeWithImports(string nl, string original)
        {
            List<string> lines = new List<string>();
            lines.Add("package org.crayonlang.interpreter.structs;");
            lines.Add("");
            bool hasLists = original.Contains("public ArrayList<");
            bool hasFastLists = original.Contains("FastList");
            bool hasDictionaries = original.Contains("public HashMap<");
            if (hasLists) lines.Add("import java.util.ArrayList;");
            if (hasFastLists) lines.Add("import org.crayonlang.interpreter.FastList;");
            if (hasDictionaries) lines.Add("import java.util.HashMap;");
            if (hasLists || hasDictionaries) lines.Add("");

            lines.Add(original);
            lines.Add("");

            return string.Join(nl, lines);
        }

        public override Dictionary<string, string> GenerateReplacementDictionary(Options options, ResourceDatabase resDb)
        {
            // This is repeated in the JavaScriptAppAndroid platform.
            Dictionary<string, string> replacements = new Dictionary<string, string>();
            replacements["PROJECT_ID"] = options.GetString(ExportOptionKey.PROJECT_ID);
            replacements["JAVA_PACKAGE"] = (options.GetStringOrNull(ExportOptionKey.JAVA_PACKAGE) ?? options.GetString(ExportOptionKey.PROJECT_ID)).ToLower();
            replacements["DEFAULT_TITLE"] = options.GetStringOrNull(ExportOptionKey.DEFAULT_TITLE) ?? options.GetString(ExportOptionKey.PROJECT_ID);

            if (replacements["JAVA_PACKAGE"].StartsWith("org.crayonlang.interpreter"))
            {
                throw new InvalidOperationException("Cannot use org.crayonlang.interpreter as the project's package.");
            }

            return replacements;
        }

        public override IDictionary<string, object> GetConstantFlags()
        {
            return new Dictionary<string, object>()
                {
                    { "IS_ASYNC", true },
                    { "PLATFORM_SUPPORTS_LIST_CLEAR", true },
                    { "STRONGLY_TYPED", true },
                    { "IS_ARRAY_SAME_AS_LIST", false },
                    { "IS_PYTHON", false },
                    { "IS_CHAR_A_NUMBER", true },
                    { "INT_IS_FLOOR", false },
                    { "IS_THREAD_BLOCKING_ALLOWED", true },
                    { "HAS_INCREMENT", true },
                };
        }
    }
}
