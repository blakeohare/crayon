using Common;
using Pastel.Nodes;
using Pastel.Transpilers;
using Platform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LangJava
{
    public class PlatformImpl : AbstractPlatform
    {
        public override string Name { get { return "lang-java"; } }
        public override string InheritsFrom { get { return null; } }
        public override string NL { get { return "\n"; } }

        public PlatformImpl() : base()
        {
            this.ContextFreePlatformImpl = new ContextFreeLangJavaPlatform();
        }

        public override void ExportStandaloneVm(
            Dictionary<string, FileOutput> output,
            IList<VariableDeclaration> globals,
            IList<StructDefinition> structDefinitions,
            IList<FunctionDefinition> functionDefinitions,
            IList<LibraryForExport> everyLibrary,
            ILibraryNativeInvocationTranslatorProvider libraryNativeInvocationTranslatorProviderForPlatform)
        {
            throw new NotImplementedException();
        }

        public override void ExportProject(Dictionary<string, FileOutput> output, IList<VariableDeclaration> globals, IList<StructDefinition> structDefinitions, IList<FunctionDefinition> functionDefinitions, IList<LibraryForExport> libraries, ResourceDatabase resourceDatabase, Options options, ILibraryNativeInvocationTranslatorProvider libraryNativeInvocationTranslatorProviderForPlatform)
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
            TranspilerContext ctx = new TranspilerContext();
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
                if (library.ManifestFunction != null)
                {
                    platform.Translator.CurrentLibraryFunctionTranslator = libraryNativeInvocationTranslatorProviderForPlatform.GetTranslator(library.Name);

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

                    platform.Translator.TabDepth = 1;
                    platform.GenerateCodeForFunction(ctx, platform.Translator, library.ManifestFunction);
                    libraryCode.Add(ctx.FlushAndClearBuffer());
                    string reflectionCalledPrefix = "lib_" + library.Name.ToLower() + "_function_";
                    foreach (FunctionDefinition fnDef in library.Functions)
                    {
                        string name = fnDef.NameToken.Value;
                        bool isFunctionPointerObject = name.StartsWith(reflectionCalledPrefix);

                        platform.GenerateCodeForFunction(ctx, platform.Translator, fnDef);
                        string functionCode = ctx.FlushAndClearBuffer();
                        if (isFunctionPointerObject)
                        {
                            functionCode = functionCode.Replace("public static Value v_" + name + "(Value[] ", "public Value invoke(Value[] ");
                            functionCode = "  " + functionCode.Replace("\n", "\n  ").TrimEnd();
                            functionCode = "  public static class FP_" + name + " extends LibraryFunctionPointer {\n" + functionCode + "\n  }\n";

                            libraryCode.Add(functionCode);
                        }
                        else
                        {
                            libraryCode.Add(functionCode);
                        }
                    }
                    platform.Translator.TabDepth = 0;
                    libraryCode.Add("}");
                    libraryCode.Add("");

                    string libraryPath = srcPath + "/org/crayonlang/libraries/" + library.Name.ToLower();

                    output[libraryPath + "/LibraryWrapper.java"] = new FileOutput()
                    {
                        Type = FileOutputType.Text,
                        TextContent = string.Join(platform.NL, libraryCode),
                    };

                    foreach (StructDefinition structDef in library.Structs)
                    {
                        platform.GenerateCodeForStruct(ctx, platform.Translator, structDef);
                        string structCode = ctx.FlushAndClearBuffer();

                        structCode = WrapStructCodeWithImports(platform.NL, structCode);

                        // This is kind of a hack.
                        // TODO: better.
                        structCode = structCode.Replace(
                            "package org.crayonlang.interpreter.structs;",
                            "package org.crayonlang.libraries." + library.Name.ToLower() + ";");

                        output[libraryPath + "/" + structDef.NameToken.Value + ".java"] = new FileOutput()
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

        public override void GenerateCodeForFunction(TranspilerContext sb, AbstractTranslator translator, FunctionDefinition funcDef)
        {
            sb.Append(translator.CurrentTab);
            sb.Append("public static ");
            sb.Append(translator.TranslateType(funcDef.ReturnType));
            sb.Append(" v_");
            sb.Append(funcDef.NameToken.Value);
            sb.Append('(');
            Pastel.Token[] argNames = funcDef.ArgNames;
            PType[] argTypes = funcDef.ArgTypes;
            for (int i = 0; i < argTypes.Length; ++i)
            {
                if (i > 0) sb.Append(", ");
                sb.Append(translator.TranslateType(argTypes[i]));
                sb.Append(" v_");
                sb.Append(argNames[i].Value);
            }
            sb.Append(") {");
            sb.Append(this.NL);
            translator.TabDepth++;
            translator.TranslateExecutables(sb, funcDef.Code);
            translator.TabDepth--;
            sb.Append(translator.CurrentTab);
            sb.Append('}');
            sb.Append(this.NL);
        }

        public override void GenerateCodeForGlobalsDefinitions(TranspilerContext sb, AbstractTranslator translator, IList<VariableDeclaration> globals)
        {
            foreach (string line in new string[] {
                "package org.crayonlang.interpreter;",
                "",
                "import java.util.HashMap;",
                "import org.crayonlang.interpreter.structs.Value;",
                "",
                "public final class VmGlobal {",
                "",
                "  private VmGlobal() {}",
                "",
            }) {
                sb.Append(line);
                sb.Append(this.NL);
            }

            foreach (VariableDeclaration varDecl in globals)
            {
                sb.Append("  public static final ");
                sb.Append(translator.TranslateType(varDecl.Type));
                sb.Append(' ');
                sb.Append(varDecl.VariableNameToken.Value);
                sb.Append(" = ");
                translator.TranslateExpression(sb, varDecl.Value);
                sb.Append(';');
                sb.Append(this.NL);
            }
            sb.Append("}");
            sb.Append(this.NL);
        }

        public override void GenerateCodeForStruct(TranspilerContext sb, AbstractTranslator translator, StructDefinition structDef)
        {
            bool isValue = structDef.NameToken.Value == "Value";
            sb.Append("public final class ");
            sb.Append(structDef.NameToken.Value);
            sb.Append(" {");
            sb.Append(this.NL);
            string[] types = structDef.ArgTypes.Select(type => translator.TranslateType(type)).ToArray();
            string[] names = structDef.ArgNames.Select(token => token.Value).ToArray();
            int fieldCount = names.Length;
            for (int i = 0; i < fieldCount; ++i)
            {
                sb.Append("  public ");
                sb.Append(types[i]);
                sb.Append(' ');
                sb.Append(names[i]);
                sb.Append(';');
                sb.Append(this.NL);
            }

            if (isValue)
            {
                // The overhead of having extra fields on each Value is much less than the overhead
                // of Java's casting. Particularly on Android.
                sb.Append("  public int intValue;");
                sb.Append("  public FastList listValue;");
                sb.Append(this.NL);
            }

            sb.Append(this.NL);
            sb.Append("  public ");
            sb.Append(structDef.NameToken.Value);
            sb.Append('(');
            for (int i = 0; i < fieldCount; ++i)
            {
                if (i > 0) sb.Append(", ");
                sb.Append(types[i]);
                sb.Append(' ');
                sb.Append(names[i]);
            }
            sb.Append(") {");
            sb.Append(this.NL);
            for (int i = 0; i < fieldCount; ++i)
            {
                sb.Append("    this.");
                sb.Append(names[i]);
                sb.Append(" = ");
                sb.Append(names[i]);
                sb.Append(';');
                sb.Append(this.NL);
            }
            sb.Append("  }");

            if (isValue)
            {
                sb.Append(this.NL);
                sb.Append(this.NL);
                sb.Append("  public Value(int intValue) {");
                sb.Append(this.NL);
                sb.Append("    this.type = 3;");
                sb.Append(this.NL);
                sb.Append("    this.intValue = intValue;");
                sb.Append(this.NL);
                sb.Append("  }");
                sb.Append(this.NL);
                sb.Append(this.NL);
                sb.Append("  public Value(boolean boolValue) {");
                sb.Append(this.NL);
                sb.Append("    this.type = 2;");
                sb.Append(this.NL);
                sb.Append("    this.intValue = boolValue ? 1 : 0;");
                sb.Append(this.NL);
                sb.Append("  }");
                sb.Append(this.NL);
                sb.Append(this.NL);
                sb.Append("  public Value(FastList listValue) {");
                sb.Append(this.NL);
                sb.Append("    this.type = 6;");
                sb.Append(this.NL);
                sb.Append("    this.listValue = listValue;");
                sb.Append(this.NL);
                sb.Append("  }");
            }

            sb.Append(this.NL);
            sb.Append("}");
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
