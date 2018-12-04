using Common;
using Platform;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PythonApp
{
    public class PlatformImpl : AbstractPlatform
    {
        public override string Name { get { return "python-app"; } }
        public override string InheritsFrom { get { return "lang-python"; } }
        public override string NL { get { return "\n"; } }

        public PlatformImpl()
            : base("PYTHON")
        { }

        public override IDictionary<string, object> GetConstantFlags()
        {
            return new Dictionary<string, object>();
        }

        public override void ExportStandaloneVm(
            Dictionary<string, FileOutput> output,
            IList<LibraryForExport> everyLibrary)
        {
            throw new NotImplementedException();
        }

        public override void ExportProject(
            Dictionary<string, FileOutput> output,
            IList<LibraryForExport> libraries,
            ResourceDatabase resourceDatabase,
            Options options)
        {
            Dictionary<string, string> replacements = this.GenerateReplacementDictionary(options, resourceDatabase);

            TemplateReader templates = new TemplateReader(new PkgAwareFileUtil(), this);
            TemplateSet vmTemplates = templates.GetVmTemplates();
            output["code/vm.py"] = new FileOutput()
            {
                Type = FileOutputType.Text,
                TextContent = string.Join(this.NL, new string[] {
                    this.LoadTextResource("Resources/header.txt" , replacements),
                    this.LoadTextResource("Resources/TranslationHelper.txt", replacements),
                    this.LoadTextResource("Resources/LibraryRegistry.txt", replacements),
                    this.LoadTextResource("Resources/ResourceReader.txt", replacements),
                    vmTemplates.GetText("vm.py"),
                }),
            };

            foreach (LibraryForExport library in libraries)
            {
                if (library.HasNativeCode)
                {
                    TemplateSet libTemplates = new TemplateReader(new PkgAwareFileUtil(), this).GetLibraryTemplates(library.Name);
                    string libraryName = library.Name;
                    List<string> libraryLines = new List<string>();

                    libraryLines.Add("import math");
                    libraryLines.Add("import os");
                    libraryLines.Add("import random");
                    libraryLines.Add("import sys");
                    libraryLines.Add("import time");
                    libraryLines.Add("import inspect");
                    libraryLines.Add("from code.vm import *");
                    libraryLines.Add("");
                    foreach (string genCodePath in libTemplates.GetPaths("gen/"))
                    {
                        libraryLines.Add(libTemplates.GetText(genCodePath));
                        libraryLines.Add("");
                    }
                    libraryLines.Add("");
                    libraryLines.Add("_moduleInfo = ('" + libraryName + "', dict(inspect.getmembers(sys.modules[__name__])))");
                    libraryLines.Add("");
                    foreach (string genCodePath in libTemplates.GetPaths("source/"))
                    {
                        libraryLines.Add(libTemplates.GetText(genCodePath));
                        libraryLines.Add("");
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

        public override Dictionary<string, string> GenerateReplacementDictionary(Options options, ResourceDatabase resDb)
        {
            return this.ParentPlatform.GenerateReplacementDictionary(options, resDb);
        }
    }
}
