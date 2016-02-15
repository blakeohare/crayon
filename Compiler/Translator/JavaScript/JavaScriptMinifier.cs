using System.Collections.Generic;

namespace Crayon.Translator.JavaScript
{
	internal class JavaScriptMinifier
	{
		public static string Minify(string originalCode)
		{
			TokenStream tokens = new TokenStream(Tokenizer.Tokenize("JS", originalCode, 1, true));
			List<string> output = new List<string>();
			string prev = null;
			while (tokens.HasMore)
			{
				string value = tokens.PopValue();

				// Mandatory spaces.
				switch (value)
				{
					case "case": output.Add("case "); break; // switch(x) { case 42: ...
					case "return": output.Add("return "); break; // return foo
					case "new": output.Add("new "); break; // new Audio(...
					case "in": output.Add(" in "); break; // foreach loops
					case "var": output.Add("var "); break; // var varname

					case ";": // implicit semicolon
						if (tokens.PeekValue() != "}")
						{
							output.Add(";");
						}
						break;
					case "if": // else if (...
						if (prev == "else")
						{
							output.Add(" ");
						}
						output.Add("if");
						break;
					case "function": // function foo()
						if (tokens.PeekValue() != "(")
						{
							output.Add("function ");
						}
						else
						{
							output.Add("function");
						}
						break;
					default: output.Add(value); break;
				}
				prev = value;
			}
			return string.Join("", output);
		}
	}
}
