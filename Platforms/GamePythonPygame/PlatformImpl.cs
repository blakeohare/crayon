using System;
using System.Collections.Generic;
using Common;
using Pastel.Nodes;
using Platform;
using System.Text;

namespace GamePythonPygame
{
    public class PlatformImpl : AbstractPlatform
    {
        public override string Name { get { return "game-python-pygame-cbx"; } }
        public override string InheritsFrom { get { return "lang-python"; } }
        public override string NL { get { return "\n"; } }

        public PlatformImpl()
        {
            this.Translator = new PythonPygameTranslator(this);
        }

        public override Dictionary<string, FileOutput> Export(
            Dictionary<string, object[]> executablesPerCompilationUnit,
            object[] structDefinitions)
        {
            throw new NotImplementedException();
        }

        public override IDictionary<string, object> GetConstantFlags()
        {
            return new Dictionary<string, object>();
        }

        public override Dictionary<string, FileOutput> ExportProject(
            IList<VariableDeclaration> globals,
            IList<StructDefinition> structDefinitions,
            IList<FunctionDefinition> functionDefinitions,
            IList<LibraryForExport> libraries,
            ResourceDatabase resourceDatabase,
            Options options,
            ILibraryNativeInvocationTranslatorProvider libraryNativeInvocationTranslatorProviderForPlatform)
        {
            Dictionary<string, FileOutput> output = new Dictionary<string, FileOutput>();

            Dictionary<string, string> replacements = this.GenerateReplacementDictionary(options, resourceDatabase);

            List<string> runPy = new List<string>();

            runPy.Add(this.LoadTextResource("Resources/header.txt", replacements));
            runPy.Add("");
            runPy.Add(this.GenerateCodeForGlobalsDefinitions(this.Translator, globals));
            runPy.Add("");
            runPy.Add(this.LoadTextResource("Resources/LibraryRegistry.txt", replacements));
            runPy.Add("");
            runPy.Add(this.LoadTextResource("Resources/TranslationHelper.txt", replacements));
            runPy.Add("");
            runPy.Add(this.LoadTextResource("Resources/ResourceReader.txt", replacements));
            runPy.Add("");

            foreach (FunctionDefinition funcDef in functionDefinitions)
            {
                runPy.Add(this.GenerateCodeForFunction(this.Translator, funcDef));
            }

            output["code/vm.py"] = new FileOutput()
            {
                Type = FileOutputType.Text,
                TextContent = string.Join("\n", runPy),
            };

            foreach (LibraryForExport library in libraries)
            {
                this.Translator.CurrentLibraryFunctionTranslator = libraryNativeInvocationTranslatorProviderForPlatform.GetTranslator(library.Name);
                string libraryName = library.Name;
                List<string> libraryLines = new List<string>();
                if (library.ManifestFunction != null)
                {
                    libraryLines.Add("import math");
                    libraryLines.Add("import os");
                    libraryLines.Add("import random");
                    libraryLines.Add("import sys");
                    libraryLines.Add("import time");
                    libraryLines.Add("import inspect");
                    libraryLines.Add("");
                    libraryLines.Add("from code.vm import *");
                    libraryLines.Add("");

                    libraryLines.Add(this.GenerateCodeForFunction(this.Translator, library.ManifestFunction));
                    foreach (FunctionDefinition funcDef in library.Functions)
                    {
                        libraryLines.Add(this.GenerateCodeForFunction(this.Translator, funcDef));
                    }

                    libraryLines.Add("");
                    libraryLines.Add("_moduleInfo = ('" + libraryName + "', dict(inspect.getmembers(sys.modules[__name__])))");
                    libraryLines.Add("");

                    output["code/lib_" + libraryName.ToLower() + ".py"] = new FileOutput()
                    {
                        Type = FileOutputType.Text,
                        TextContent = string.Join(this.NL, libraryLines),
                    };

                    foreach (string filename in library.SupplementalFiles.Keys)
                    {
                        // TODO: append all this to the main file.
                        output["code/" + libraryName + "/" + filename] = library.SupplementalFiles[filename];
                    }
                }
            }

            output["main.py"] = new FileOutput()
            {
                Type = FileOutputType.Text,
                TextContent = this.LoadTextResource("Resources/main.txt", replacements),
            };

            output["code/__init__.py"] = new FileOutput()
            {
                Type = FileOutputType.Text,
                TextContent = "",
            };

            output["res/bytecode.txt"] = resourceDatabase.ByteCodeFile;
            output["res/resource_manifest.txt"] = resourceDatabase.ResourceManifestFile;

            return output;
        }

        public override string GenerateCodeForFunction(AbstractTranslator translator, FunctionDefinition funcDef)
        {
            return this.ParentPlatform.GenerateCodeForFunction(this.Translator, funcDef);
        }

        public override string GenerateCodeForStruct(StructDefinition structDef)
        {
            return this.ParentPlatform.GenerateCodeForStruct(structDef);
        }

        public override Dictionary<string, string> GenerateReplacementDictionary(Options options, ResourceDatabase resDb)
        {
            return this.ParentPlatform.GenerateReplacementDictionary(options, resDb);
        }

        public override string GenerateCodeForGlobalsDefinitions(AbstractTranslator translator, IList<VariableDeclaration> globals)
        {
            return this.ParentPlatform.GenerateCodeForGlobalsDefinitions(translator, globals);
        }
    }
}
