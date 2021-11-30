using System;
using System.Collections.Generic;
using Wax;

namespace Exporter
{
    public class ExportService : WaxService
    {
        private Platform.AbstractPlatform platform;
        private string platformId;

        private static Dictionary<string, Platform.AbstractPlatform> platforms = null;

        private static Platform.AbstractPlatform GetPlatform(string id)
        {
            if (platforms == null)
            {
                platforms = new Dictionary<string, Platform.AbstractPlatform>();
                foreach (Platform.AbstractPlatform platform in new Platform.AbstractPlatform[] {
                    new CSharpApp.PlatformImpl(),
                    new JavaScriptApp.PlatformImpl(),
                    new JavaScriptAppAndroid.PlatformImpl(),
                    new JavaScriptAppIos.PlatformImpl(),
                    new LangCSharp.PlatformImpl(),
                    new LangJavaScript.PlatformImpl(),
                })
                {
                    platforms[platform.Name.ToLowerInvariant()] = platform;
                }

                // HACK! This will be sorted out after these are turned into extensions, but for now, define the hierarchy of platforms here.
                platforms["csharp-app"].ParentPlatform = platforms["lang-csharp"];
                platforms["javascript-app"].ParentPlatform = platforms["lang-javascript"];
                platforms["javascript-app-android"].ParentPlatform = platforms["javascript-app"];
                platforms["javascript-app-ios"].ParentPlatform = platforms["javascript-app"];
            }

            if (platforms.ContainsKey(id)) return platforms[id];
            return null;
        }


        public ExportService(string platformId) : base("export-" + platformId)
        {
            this.platformId = platformId;
            this.platform = GetPlatform(platformId.ToLowerInvariant());
        }

        public override void HandleRequest(Dictionary<string, object> request, Func<Dictionary<string, object>, bool> cb)
        {
            BuildData buildData = new BuildData(request);
            Error[] errors = this.HandleRequestImpl(buildData) ?? new Error[0];
            cb(new Dictionary<string, object>() { { "errors", errors } });
        }

        private Error[] HandleRequestImpl(BuildData buildData)
        {
            if (this.platform == null)
            {
                return new Error[] {
                    new Error() { Message = "No platform by the name of '" + this.platformId + "'" }
                };
            }

            ExportResponse response = CbxVmBundleExporter.Run(this.platform, buildData.ExportProperties.ProjectDirectory, buildData.ExportProperties.OutputDirectory, buildData);

            return response.Errors;
        }
    }
}
