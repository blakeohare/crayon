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
        public override string Name { get { return "python-app"; } }
        public override string InheritsFrom { get { return "lang-python"; } }
        public override string NL { get { return "\n"; } }

        public PlatformImpl()
        {
            this.Translator = new PythonAppTranslator(this);
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

            foreach (string simpleCodeConcat in new string[] {
                "header.txt",
                // TODO: these need to go in their respective libraries.
                "image_helper.txt",
                "game.txt",
                "gfx_renderer.txt",
                "gamepad_helper.txt",
            })
            {
                runPy.Add(this.LoadTextResource("Resources/" + simpleCodeConcat, replacements));
                runPy.Add("");
            }

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

                    foreach (ExportEntity codeToAppendToLibrary in library.ExportEntities["EMBED_CODE"])
                    {
                        libraryLines.Add(codeToAppendToLibrary.StringValue);
                    }

                    output["code/lib_" + libraryName.ToLower() + ".py"] = new FileOutput()
                    {
                        Type = FileOutputType.Text,
                        TextContent = string.Join(this.NL, libraryLines),
                    };
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
            output["res/image_sheet_manifest.txt"] = resourceDatabase.ImageSheetManifestFile;

            foreach (string imageSheetFile in resourceDatabase.ImageSheetFiles.Keys)
            {
                output["res/images/" + imageSheetFile] = resourceDatabase.ImageSheetFiles[imageSheetFile];
            }

            foreach (FileOutput textResource in resourceDatabase.TextResources)
            {
                output["res/text/" + textResource.CanonicalFileName] = textResource;
            }

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
