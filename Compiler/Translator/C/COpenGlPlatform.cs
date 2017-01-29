using System;
using System.Collections.Generic;
using System.Linq;
using Crayon.ParseTree;
using Common;

namespace Crayon.Translator.C
{
    internal class COpenGlPlatform : AbstractPlatform
    {
        public COpenGlPlatform()
            : base(PlatformId.C_OPENGL, LanguageId.C, new CTranslator(), new CSystemFunctionTranslator())
        { }

        public override bool IntIsFloor { get { return true; } }
        public override bool IsArraySameAsList { get { return false; } }
        public override bool IsAsync { get { return false; } }
        public override bool IsCharANumber { get { return true; } }
        public override bool IsStronglyTyped { get { return true; } }
        public override bool IsThreadBlockingAllowed { get { return true; } }
        public override string PlatformShortId { get { return "game-c-opengl"; } }
        public override bool SupportsListClear { get { return true; } }

        public override Dictionary<string, FileOutput> Package(
            BuildContext buildContext,
            string projectId,
            Dictionary<string, Executable[]> finalCode,
            ICollection<StructDefinition> structDefinitions,
            ResourceDatabase resourceDatabase,
            SystemLibraryManager libraryManager)
        {
            Dictionary<string, FileOutput> output = new Dictionary<string, FileOutput>();
            Dictionary<string, string> replacements = new Dictionary<string, string>();
            List<string> mainDotC = new List<string>();
            mainDotC.Add(this.GetCCode("Headers.txt", replacements));
            mainDotC.Add(this.GetCCode("List.txt", replacements));
            mainDotC.Add(this.GetCCode("String.txt", replacements));
            mainDotC.Add(this.GetCCode("DictInt.txt", replacements));
            mainDotC.Add(this.GetCCode("DictString.txt", replacements));

            mainDotC.Add(this.SerializeAllStructDefinitions(structDefinitions));

            mainDotC.Add(this.GetCCode("PostStructHeader.txt", replacements));

            mainDotC.Add(this.GetCCode("ValueConstructors.txt", replacements));

            Dictionary<string, Executable[]> mutableFinalCode = new Dictionary<string, Executable[]>(finalCode);

            List<string> globalsCode = new List<string>();
            mainDotC.Add(this.SerializeAllGlobals(mutableFinalCode["Globals"]));
            mutableFinalCode.Remove("Globals");
            mainDotC.Add(string.Join("", globalsCode));

            string[] units = new string[]
            {
                "ByteCodeLoader",
                "TypesUtil",
                //"ValueUtil",
            };

            mainDotC.Add(this.GetCCode("CTranslationHelper.txt", replacements));

            List<string> sb = new List<string>();
            foreach (string key in units)
            {
                foreach (FunctionDefinition function in mutableFinalCode[key].Cast<FunctionDefinition>())
                {
                    mainDotC.Add(this.GetFunctionSignature(function) + ";");
                }
            }
            mainDotC.Add("");

            foreach (string key in units)
            {
                mainDotC.Add(this.Translator.Translate(mutableFinalCode[key]));
            }
            
            mainDotC.Add(this.GetCCode("Main.txt", replacements));

            output["main.c"] = new FileOutput()
            {
                Type = FileOutputType.Text,
                TextContent = string.Join("\n", mainDotC),
            };

            return output;
        }

        public string GetFunctionSignature(FunctionDefinition functionDef)
        {
            List<string> output = new List<string>();
            Annotation returnType = functionDef.GetAnnotation("type");
            string returnTypeString = this.GetTypeStringFromAnnotation(new AnnotatedType(returnType));
            output.Add(returnTypeString);
            output.Add(" v_");
            output.Add(functionDef.NameToken.Value);
            output.Add("(");
            Token[] args = functionDef.ArgNames;
            Annotation[] argTypes = functionDef.ArgAnnotations;
            for (int i = 0; i < args.Length; ++i)
            {
                if (i > 0) output.Add(", ");
                string type = this.GetTypeStringFromAnnotation(new AnnotatedType(argTypes[i]));
                output.Add(type);
                output.Add(" v_");
                output.Add(args[i].Value);
            }
            output.Add(")");
            return string.Join("", output);
        }

        private string SerializeSwitchLookups(Executable[] switchLookups)
        {
            return "";
        }

        private string SerializeAllGlobals(Executable[] globalAssignments)
        {
            List<string> lines = new List<string>();
            // First need to declare the globals outside of a function scope.
            foreach (Assignment assignment in globalAssignments.Cast<Assignment>()) {
                Variable target = assignment.TargetAsVariable;
                StringConstant sc = (StringConstant)target.Annotations["type"].Args[0];
                string type = this.GetTypeStringFromAnnotation(sc.FirstToken, sc.Value);
                lines.Add(type + " v_" + target.Name + ";");
            }
            lines.Add("");

            // And then declare a wrapper function that sets them all.
            // This function is invoked from main() before v_main()

            lines.Add("void set_globals()");
            lines.Add("{");
            List<string> sb = new List<string>();
            foreach (Assignment assignment in globalAssignments.Cast<Assignment>())
            {
                string name = assignment.TargetAsVariable.Name;
                this.Translator.TranslateExpression(sb, assignment.Value);
                lines.Add("\tv_" + name + " = " + string.Join("", sb) + ";");
                sb.Clear();
            }
            lines.Add("}");
            lines.Add("");

            return string.Join("\n", lines);
        }

        private string SerializeAllStructDefinitions(ICollection<StructDefinition> structDefinitions)
        {
            List<string> lines = new List<string>();

            // This occurs in 3 passes:
            // - pre-define structs
            // - type def structs
            // - constructor code for each

            // predefine structs with _t suffix
            foreach (StructDefinition sd in structDefinitions)
            {
                lines.Add("struct " + sd.Name.Value + "_t;");
            }
            lines.Add("");

            // typedef structs
            foreach (StructDefinition sd in structDefinitions)
            {
                lines.Add("typedef struct " + sd.Name.Value + "_t {");
                for (int i = 0; i < sd.FieldsByIndex.Length; ++i)
                {
                    string name = sd.FieldsByIndex[i];
                    string type = this.GetTypeStringFromAnnotation(sd.TypeTokens[i], sd.TypeStrings[i]);
                    if (type[0] >= 'A' && type[0] <= 'Z') // This check is a little hacky.
                    {
                        int pointerIndex = type.IndexOf('*');
                        string rootType = type.Substring(0, pointerIndex);
                        string suffixes = type.Substring(pointerIndex);
                        type = "struct " + rootType + "_t" + suffixes;
                    }
                    lines.Add("\t" + type + " " + name + ";");
                }
                lines.Add("} " + sd.Name.Value + ";");
                lines.Add("");
            }

            // create a constructor for each
            List<string> sb = new List<string>();
            foreach (StructDefinition sd in structDefinitions)
            {
                string name = sd.Name.Value;
                sb.Add(name);
                sb.Add("* ");
                sb.Add(name);
                sb.Add("_new(");
                for (int i = 0; i < sd.Fields.Length; ++i)
                {
                    if (i > 0) sb.Add(", ");

                    string type = this.GetTypeStringFromAnnotation(sd.TypeTokens[i], sd.TypeStrings[i]);
                    sb.Add(type);
                    sb.Add(" v_");
                    sb.Add(sd.Fields[i].Value);
                }
                sb.Add(")");
                lines.Add(string.Join("", sb));
                sb.Clear();
                lines.Add("{");
                lines.Add("\t" + name + "* s = (" + name + "*) malloc(sizeof(" + name + "));");
                for (int i = 0; i < sd.Fields.Length; ++i)
                {
                    lines.Add("\ts->" + sd.Fields[i].Value + " = v_" + sd.Fields[i].Value + ";");
                }
                lines.Add("\treturn s;");
                lines.Add("}");
                lines.Add("");
            }

            return string.Join("\n", lines);
        }

        private Dictionary<string, string> typeConversionCache = new Dictionary<string, string>();

        private string GetTypeCached(string value)
        {
            string output;
            if (typeConversionCache.TryGetValue(value, out output))
            {
                return output;
            }
            return null;
        }

        public string GetTypeStringFromAnnotation(Token stringToken, string value)
        {
            string output = this.GetTypeCached(value);
            if (output != null) return output;
            AnnotatedType type = new AnnotatedType(stringToken, new TokenStream(Tokenizer.Tokenize("type proxy", value, -1, false)));
            output = GetTypeStringFromAnnotation(type);
            typeConversionCache[value] = output;
            return output;
        }
        
        public string GetTypeStringFromAnnotation(AnnotatedType type)
        {
            // TODO: add cache here too.
            switch (type.Name)
            {
                case "Dictionary":
                    if (type.Generics[0].Name == "int")
                    {
                        return "DictInt*";
                    }

                    if (type.Generics[0].Name == "string")
                    {
                        return "DictString*";
                    }

                    throw new Exception();

                case "List":
                    if (type.Generics[0].Name == "double")
                    {
                        throw new Exception();
                    }
                    return "List*";

                case "Array":
                    string value = this.GetTypeStringFromAnnotation(type.Generics[0]);
                    value += "*";
                    return value;

                case "bool":
                    return "int";

                case "string":
                    return "String*";

                case "double":
                    return "double";

                case "int":
                    return "int";

                case "char":
                    return "int";

                case "object":
                    return "void*";

                default:
                    char firstChar = type.Name[0];
                    if (firstChar >= 'A' && firstChar <= 'Z')
                    {
                        return type.Name + "*";
                    }
                    throw new NotImplementedException();
            }
        }

        private string GetCCode(string file, Dictionary<string, string> replacements)
        {
            string cCode = LegacyUtil.ReadResourceFileInternally("game-c-opengl/" + file);
            cCode = Constants.DoReplacements(false, cCode, replacements);
            return cCode;
        }
    }
}
