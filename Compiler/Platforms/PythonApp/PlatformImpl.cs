﻿using Common;
using Pastel.Nodes;
using Pastel.Transpilers;
using Platform;
using System;
using System.Collections.Generic;

namespace PythonApp
{
    public class PlatformImpl : AbstractPlatform
    {
        public override string Name { get { return "python-app"; } }
        public override string InheritsFrom { get { return "lang-python"; } }
        public override string NL { get { return "\n"; } }

        public PlatformImpl()
        {
            this.ContextFreePlatformImpl = new ContextFreePythonAppPlatform();
            this.Translator = new PythonTranslator();
        }

        public override IDictionary<string, object> GetConstantFlags()
        {
            return new Dictionary<string, object>();
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

        public override void ExportProject(
            Dictionary<string, FileOutput> output,
            IList<VariableDeclaration> globals,
            IList<StructDefinition> structDefinitions,
            IList<FunctionDefinition> functionDefinitions,
            IList<LibraryForExport> libraries,
            ResourceDatabase resourceDatabase,
            Options options,
            ILibraryNativeInvocationTranslatorProvider libraryNativeInvocationTranslatorProviderForPlatform)
        {
            Dictionary<string, string> replacements = this.GenerateReplacementDictionary(options, resourceDatabase);

            TranspilerContext ctx = new TranspilerContext();

            List<string> runPy = new List<string>();

            TODO.PythonAppDeGamification();

            foreach (string simpleCodeConcat in new string[] {
                "header.txt",
                "image_helper.txt",
                "game.txt",
                "gfx_renderer.txt",
                "gamepad_helper.txt",
            })
            {
                runPy.Add(this.LoadTextResource("Resources/" + simpleCodeConcat, replacements));
                runPy.Add("");
            }
            this.GenerateCodeForGlobalsDefinitions(ctx, this.Translator, globals);
            runPy.Add(ctx.FlushAndClearBuffer());
            runPy.Add("");
            runPy.Add(this.LoadTextResource("Resources/LibraryRegistry.txt", replacements));
            runPy.Add("");
            runPy.Add(this.LoadTextResource("Resources/TranslationHelper.txt", replacements));
            runPy.Add("");
            runPy.Add(this.LoadTextResource("Resources/ResourceReader.txt", replacements));
            runPy.Add("");

            foreach (FunctionDefinition funcDef in functionDefinitions)
            {
                this.GenerateCodeForFunction(ctx, this.Translator, funcDef);
                runPy.Add(ctx.FlushAndClearBuffer());
            }

            output["code/vm.py"] = new FileOutput()
            {
                Type = FileOutputType.Text,
                TextContent = string.Join("\n", runPy),
            };

            foreach (LibraryForExport library in libraries)
            {
                ctx.CurrentLibraryFunctionTranslator = libraryNativeInvocationTranslatorProviderForPlatform.GetTranslator(library.Name);
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

                    this.GenerateCodeForFunction(ctx, this.Translator, library.ManifestFunction);
                    libraryLines.Add(ctx.FlushAndClearBuffer());
                    foreach (FunctionDefinition funcDef in library.Functions)
                    {
                        this.GenerateCodeForFunction(ctx, this.Translator, funcDef);
                        libraryLines.Add(ctx.FlushAndClearBuffer());
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
            if (resourceDatabase.ImageSheetManifestFile != null)
            {
                output["res/image_sheet_manifest.txt"] = resourceDatabase.ImageSheetManifestFile;
            }

            foreach (FileOutput image in resourceDatabase.ImageResources)
            {
                output["res/images/" + image.CanonicalFileName] = image;
            }

            foreach (string imageSheetFile in resourceDatabase.ImageSheetFiles.Keys)
            {
                output["res/images/" + imageSheetFile] = resourceDatabase.ImageSheetFiles[imageSheetFile];
            }

            foreach (FileOutput sound in resourceDatabase.AudioResources)
            {
                output["res/audio/" + sound.CanonicalFileName] = sound;
            }

            foreach (FileOutput textResource in resourceDatabase.TextResources)
            {
                output["res/text/" + textResource.CanonicalFileName] = textResource;
            }

            foreach (FileOutput fontResource in resourceDatabase.FontResources)
            {
                output["res/ttf/" + fontResource.CanonicalFileName] = fontResource;
            }
        }

        public override void GenerateCodeForFunction(TranspilerContext sb, AbstractTranslator translator, FunctionDefinition funcDef)
        {
            this.ParentPlatform.GenerateCodeForFunction(sb, this.Translator, funcDef);
        }

        public override void GenerateCodeForStruct(TranspilerContext sb, AbstractTranslator translator, StructDefinition structDef)
        {
            this.ParentPlatform.GenerateCodeForStruct(sb, translator, structDef);
        }

        public override Dictionary<string, string> GenerateReplacementDictionary(Options options, ResourceDatabase resDb)
        {
            return this.ParentPlatform.GenerateReplacementDictionary(options, resDb);
        }

        public override void GenerateCodeForGlobalsDefinitions(TranspilerContext sb, AbstractTranslator translator, IList<VariableDeclaration> globals)
        {
            this.ParentPlatform.GenerateCodeForGlobalsDefinitions(sb, translator, globals);
        }
    }
}
