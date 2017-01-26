using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crayon.ParseTree;

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
            mainDotC.Add(GetCCode("Headers.txt", replacements));
            mainDotC.Add(GetCCode("List.txt", replacements));
            mainDotC.Add(GetCCode("String.txt", replacements));
            mainDotC.Add(GetCCode("DictInt.txt", replacements));
            mainDotC.Add(GetCCode("DictString.txt", replacements));

            List<string> sb = new List<string>();

            foreach (StructDefinition sd in structDefinitions)
            {
                sb.Add("struct " + sd.Name.Value + "_t;");
            }

            foreach (StructDefinition sd in structDefinitions)
            {
                sb.Add("typedef struct " + sd.Name.Value + "_t {");
                for (int i = 0; i < sd.FieldsByIndex.Length; ++i)
                {
                    string name = sd.FieldsByIndex[i];
                    StringConstant sc = (StringConstant)sd.Types[i].Args[0];
                    string type = this.GetTypeStringFromAnnotation(sc.FirstToken, sc.Value);
                    if (type[0] >= 'A' && type[0] <= 'Z') // This check is a little hacky.
                    {
                        int pointerIndex = type.IndexOf('*');
                        string rootType = type.Substring(0, pointerIndex);
                        string suffixes = type.Substring(pointerIndex);
                        type = "struct " + rootType + "_t" + suffixes;
                    }
                    sb.Add("\t" + type + " " + name + ";");
                }
                sb.Add("} " + sd.Name.Value + ";");
                sb.Add("");
                mainDotC.Add(string.Join("\n", sb));
                sb.Clear();
            }

            mainDotC.Add(GetCCode("Main.txt", replacements));

            output["main.c"] = new FileOutput()
            {
                Type = FileOutputType.Text,
                TextContent = string.Join("\n", mainDotC),
            };

            return output;
        }

        public string GetTypeStringFromAnnotation(Token stringToken, string value)
        {
            AnnotatedType type = new AnnotatedType(stringToken, new TokenStream(Tokenizer.Tokenize("type proxy", value, -1, false)));
            return GetTypeStringFromAnnotation(type);
        }
        public string GetTypeStringFromAnnotation(AnnotatedType type)
        {
            switch (type.Name)
            {
                case "Dictionary":
                    if (type.Generics[0].Name == "int")
                    {
                        return "DictInteger*";
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
            string cCode = Util.ReadResourceFileInternally("game-c-opengl/" + file);
            cCode = Constants.DoReplacements(cCode, replacements);
            return cCode;
        }
    }
}
