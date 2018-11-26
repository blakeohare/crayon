using Pastel.Nodes;
using System.Collections.Generic;

namespace Pastel
{
    public static class ExtensibleFunctionMetadataParser
    {
        public static List<ExtensibleFunction> Parse(string filename, string metadataFileContents)
        {
            List<ExtensibleFunction> output = new List<ExtensibleFunction>();

            Dictionary<string, PType> returnTypeInfoForExtensibleMethods = new Dictionary<string, PType>();
            Dictionary<string, PType[]> argumentTypeInfoForExtensibleMethods = new Dictionary<string, PType[]>();

            TokenStream tokens = new TokenStream(Tokenizer.Tokenize(filename, metadataFileContents));

            while (tokens.HasMore)
            {
                PType returnType = PType.Parse(tokens);
                string functionName = VerifyNameValid(tokens.Pop());
                tokens.PopExpected("(");
                List<PType> argTypes = new List<PType>();
                while (!tokens.PopIfPresent(")"))
                {
                    if (argTypes.Count > 0) tokens.PopExpected(",");
                    argTypes.Add(PType.Parse(tokens));

                    // This is unused but could be later used as part of an auto-generated documentation for third-party platform implements of existing libraries.
                    string argumentName = VerifyNameValid(tokens.Pop());
                }
                tokens.PopExpected(";");

                returnTypeInfoForExtensibleMethods[functionName] = returnType;
                argumentTypeInfoForExtensibleMethods[functionName] = argTypes.ToArray();
            }

            foreach (string functionName in returnTypeInfoForExtensibleMethods.Keys)
            {
                output.Add(new ExtensibleFunction()
                {
                    Name = functionName,
                    ReturnType = returnTypeInfoForExtensibleMethods[functionName],
                    ArgTypes = argumentTypeInfoForExtensibleMethods[functionName],
                });
            }

            return output;
        }

        private static string VerifyNameValid(Token token)
        {
            string name = token.Value;
            char c;
            bool okay = true;
            for (int i = name.Length - 1; i >= 0; --i)
            {
                c = name[i];
                if ((c >= 'a' && c <= 'z') ||
                    (c >= 'A' && c <= 'Z') ||
                    c == '_')
                {
                    // this is fine
                }
                else if (c >= '0' && c <= '9')
                {
                    if (i == 0)
                    {
                        okay = false;
                        break;
                    }
                }
                else
                {
                    okay = false;
                    break;
                }
            }
            if (!okay)
            {
                throw new ParserException(token, "Invalid name for a extensible function or argument.");
            }
            return name;
        }

    }
}
