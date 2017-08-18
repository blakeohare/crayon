﻿using Common;
using System.Collections.Generic;

namespace Crayon
{
    internal class CompilationBundle
    {
        public ByteBuffer ByteCode { get; set; }
        public string ProjectID { get; set; }
        public string GuidSeed { get; set; }
        public ICollection<Library> LibrariesUsed { get; set; }
        public string IconPath { get; set; }
        public string DefaultTitle { get; set; }
        public string JsFilePrefix { get; set; }
        public string IosBundlePrefix { get; set; }
        public int? WindowWidth { get; set; }
        public int? WindowHeight { get; set; }
        public string Version { get; set; }
        public string Description { get; set; }

        public static CompilationBundle Compile(BuildContext buildContext)
        {
            using (new PerformanceSection("CompilationBundle.Compile"))
            {
                Parser parser = new Parser(buildContext, null);
                Crayon.ParseTree.Executable[] resolvedParseTree = parser.ParseAllTheThings();

                ByteCodeCompiler bcc = new ByteCodeCompiler();
                ByteBuffer buffer = bcc.GenerateByteCode(parser, resolvedParseTree);

                string jsFilePrefix = buildContext.JsFilePrefix == null
                    ? ""
                    : ("/" + buildContext.JsFilePrefix.Trim('/') + "/");

                return new CompilationBundle()
                {
                    ByteCode = buffer,
                    LibrariesUsed = parser.LibraryManager.LibrariesUsed,
                    ProjectID = buildContext.ProjectID,
                    Version = buildContext.Version,
                    Description = buildContext.Description,
                    GuidSeed = buildContext.GuidSeed,
                    DefaultTitle = buildContext.DefaultTitle,
                    JsFilePrefix = jsFilePrefix,
                    IosBundlePrefix = buildContext.IosBundlePrefix,
                    IconPath = buildContext.IconFilePath,
                    WindowWidth = buildContext.WindowWidth,
                    WindowHeight = buildContext.WindowHeight,
                };
            }
        }
    }
}
