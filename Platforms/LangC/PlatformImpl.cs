﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common;
using Pastel.Nodes;
using Platform;

namespace LangC
{
    public class PlatformImpl : AbstractPlatform
    {
        public override string Name { get { return "lang-c"; } }
        public override string InheritsFrom { get { return null; } }
        public override string NL { get { return "\n"; } }

        public override IDictionary<string, object> GetConstantFlags()
        {
            return new Dictionary<string, object>()
                {
                    { "IS_ASYNC", false },
                    { "PLATFORM_SUPPORTS_LIST_CLEAR", true },
                    { "STRONGLY_TYPED", true },
                    { "IS_ARRAY_SAME_AS_LIST", false },
                    { "IS_PYTHON", false },
                    { "IS_CHAR_A_NUMBER", true },
                    { "INT_IS_FLOOR", true },
                    { "IS_THREAD_BLOCKING_ALLOWED", true },
                };
        }

        public override Dictionary<string, FileOutput> ExportStandaloneVm(IList<VariableDeclaration> globals, IList<StructDefinition> structDefinitions, IList<FunctionDefinition> functionDefinitions, IList<LibraryForExport> everyLibrary, ILibraryNativeInvocationTranslatorProvider libraryNativeInvocationTranslatorProviderForPlatform)
        {
            throw new NotImplementedException();
        }

        public override Dictionary<string, FileOutput> ExportProject(IList<VariableDeclaration> globals, IList<StructDefinition> structDefinitions, IList<FunctionDefinition> functionDefinitions, IList<LibraryForExport> libraries, ResourceDatabase resourceDatabase, Options options, ILibraryNativeInvocationTranslatorProvider libraryNativeInvocationTranslatorProviderForPlatform)
        {
            throw new NotImplementedException();
        }

        public override string GenerateCodeForStruct(StructDefinition structDef)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("struct ");
            sb.Append(structDef.NameToken.Value);
            sb.Append(" {\n");
            for (int i = 0; i < structDef.ArgNames.Length; ++i)
            {
                string fieldName = structDef.ArgNames[i].Value;
                PType fieldType = structDef.ArgTypes[i];
                sb.Append('\t');
                sb.Append(this.TranslateType(fieldType));
                sb.Append(' ');
                sb.Append(fieldName);
                sb.Append(";\n");
            }

            sb.Append("};\n\n");

            return sb.ToString();
        }

        public override string GenerateCodeForFunction(AbstractTranslator translator, FunctionDefinition funcDef)
        {
            throw new NotImplementedException();
        }

        public override string GenerateCodeForGlobalsDefinitions(AbstractTranslator translator, IList<VariableDeclaration> globals)
        {
            throw new NotImplementedException();
        }

        public override Dictionary<string, string> GenerateReplacementDictionary(Options options, ResourceDatabase resDb)
        {
            throw new NotImplementedException();
        }

        public override string TranslateType(Pastel.Nodes.PType type)
        {
            switch (type.RootValue)
            {
                case "int": return "int";
                case "string": return "int*";
                case "bool": return "int";
                case "double": return "double";
                case "object": return "void*";
                case "char": return "int";
                case "List": return "List*";
                case "Array": return this.TranslateType(type.Generics[0]) + "*";
                case "Dictionary":
                    string keyType = type.Generics[0].RootValue;
                    switch (keyType)
                    {
                        case "int":
                        case "string":
                            return "Dictionary*";
                        default:
                            throw new NotImplementedException();
                    }
                default: break;
            }

            char firstChar = type.RootValue[0];
            if (firstChar >= 'A' && firstChar <= 'Z') return type.RootValue + "*";

            throw new NotImplementedException();
        }
    }
}
