using Build;
using Common;
using AssemblyResolver;
using System.Collections.Generic;
using System.Linq;

namespace Exporter
{
    public class ExportBundle
    {
        public string ByteCode { get; set; }
        public string ProjectID { get; set; }
        public string GuidSeed { get; set; }
        public IList<AssemblyResolver.AssemblyMetadata> LibraryAssemblies { get; set; }
        public string[] IconPaths { get; set; }
        public string LaunchScreenPath { get; set; }
        public string ProjectTitle { get; set; }
        public string JsFilePrefix { get; set; }
        public bool JsFullPage { get; set; }
        public string IosBundlePrefix { get; set; }
        public string JavaPackage { get; set; }
        public NullableInteger WindowWidth { get; set; }
        public NullableInteger WindowHeight { get; set; }
        public string Orientations { get; set; }
        public string Version { get; set; }
        public string Description { get; set; }

        public static ExportBundle Compile(string byteCode, IList<AssemblyMetadata> libraryAssemblies, BuildContext buildContext)
        {
            string jsFilePrefix = buildContext.JsFilePrefix;
            jsFilePrefix = (jsFilePrefix == null || jsFilePrefix == "" || jsFilePrefix == "/")
                ? ""
                : ("/" + buildContext.JsFilePrefix.Trim('/') + "/");


            return new ExportBundle()
            {
                ByteCode = byteCode,
                LibraryAssemblies = libraryAssemblies.ToArray(),
                ProjectID = buildContext.ProjectID,
                Version = buildContext.TopLevelAssembly.Version,
                Description = buildContext.TopLevelAssembly.Description,
                GuidSeed = buildContext.GuidSeed,
                ProjectTitle = buildContext.ProjectTitle,
                JsFilePrefix = jsFilePrefix,
                JsFullPage = buildContext.JsFullPage,
                IosBundlePrefix = buildContext.IosBundlePrefix,
                JavaPackage = buildContext.JavaPackage,
                IconPaths = buildContext.IconFilePaths,
                LaunchScreenPath = buildContext.LaunchScreenPath,
                WindowWidth = buildContext.WindowWidth,
                WindowHeight = buildContext.WindowHeight,
                Orientations = buildContext.Orientation,
            };
        }
    }
}
