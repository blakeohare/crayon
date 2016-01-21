using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crayon.ParseTree;

namespace Crayon
{
	internal class Resolver
	{
		private Parser parser;
		private IList<Executable> currentCode;
		
		public Resolver(Parser parser, IList<Executable> originalCode)
		{
			this.parser = parser;
			this.currentCode = originalCode;
		}

		public Executable[] Resolve(bool isTranslateMode)
		{
			List<Executable> output = this.SimpleFirstPassResolution(this.parser, this.currentCode);

			if (isTranslateMode)
			{
				return output.ToArray();
			}

			// These track all possible places where variables can be declared outside of the global scope.
			List<FunctionDefinition> functions = new List<FunctionDefinition>();
			List<ClassDefinition> classes = new List<ClassDefinition>();
			List<ConstructorDefinition> constructors = new List<ConstructorDefinition>();

			List<Executable> codeContainers = new List<Executable>();

			// Assign all ID's to variables.
			foreach (Executable executable in output)
			{
				if (executable is FunctionDefinition)
				{
					FunctionDefinition funcDef = (FunctionDefinition)executable;
					parser.VariableRegister(funcDef.NameToken.Value, true, funcDef.NameToken);
					codeContainers.Add(executable);
				}
				else if (executable is ClassDefinition)
				{
					codeContainers.Add(executable);
				}
				else
				{
					executable.VariableUsagePass(parser);
				}
			}

			foreach (Executable executable in output)
			{
				if (executable is FunctionDefinition)
				{
					// Code containers' usage/id pass methods are meant for doing ID allocation for the code in them.
					FunctionDefinition funcDef = (FunctionDefinition)executable;
					funcDef.NameGlobalID = parser.GetGlobalScopeId(funcDef.NameToken.Value)[1];
				}
				else if (executable is ClassDefinition)
				{
					// Do nothing.
				}
				else
				{
					executable.VariableIdAssignmentPass(parser);
				}
			}

			foreach (Executable functionOrClass in codeContainers)
			{
				parser.ResetLocalScope();
				functionOrClass.VariableUsagePass(parser);
				functionOrClass.VariableIdAssignmentPass(parser);
			}

			return output.ToArray();
		}

		// This will run for both compiled and translated code.
		private List<Executable> SimpleFirstPassResolution(Parser parser, IList<Executable> original)
		{
			List<Executable> output = new List<Executable>();
			List<Executable> namespaceMembers = new List<Executable>();
			foreach (Executable line in original)
			{
				if (line is Namespace)
				{
					((Namespace)line).AppendFlattenedCode(namespaceMembers);
					foreach (Executable namespaceLine in namespaceMembers)
					{
						output.AddRange(namespaceLine.Resolve(parser));
					}
					namespaceMembers.Clear();
				}
				else
				{
					output.AddRange(line.Resolve(parser));
				}
			}

			return output;
		}

		// Convert anything that looks like a function call into a verified pointer to the function if possible using the
		// available namespaces.
		public static List<Executable> CreateVerifiedFunctionCalls(Parser parser, IList<Executable> original)
		{
			// All code that doesn't have a function or class surrounding it.
			List<Executable> looseCode = new List<Executable>();

			// First create a fully-qualified lookup of all functions and classes.
			Dictionary<string, Executable> functionsAndClasses = new Dictionary<string, Executable>();
			foreach (Executable exec in original)
			{
				if (exec is FunctionDefinition)
				{
					FunctionDefinition fd = (FunctionDefinition)exec;
					string key = fd.Namespace + ":" + fd.NameToken.Value;
					functionsAndClasses[key] = fd;
				}
				else if (exec is ClassDefinition)
				{
					ClassDefinition cd = (ClassDefinition)exec;
					string key = cd.Namespace + ":" + cd.NameToken.Value;
					functionsAndClasses[key] = cd;
				}
			}

			List<Executable> output = new List<Executable>();

			return output;
		}
	}
}
