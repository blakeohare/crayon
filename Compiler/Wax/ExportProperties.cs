﻿using System.Collections.Generic;

namespace Wax
{
    public class ExportProperties : JsonBasedObject
    {
        public ExportProperties() : base() { }
        public ExportProperties(IDictionary<string, object> data) : base(data) { }

        public string ProjectID { get { return this.GetString("projectId"); } set { this.SetString("projectId", value); } }
        public string GuidSeed { get { return this.GetString("guidSeed"); } set { this.SetString("guidSeed", value); } }
        public string[] IconPaths { get { return this.GetStrings("iconPaths"); } set { this.SetStrings("iconPaths", value); } }
        public bool HasIcon { get { return this.IconPaths != null && this.IconPaths.Length > 0; } }
        public string LaunchScreenPath { get { return this.GetString("launchScreenPath"); } set { this.SetString("launchScreenPath", value); } }
        public bool HasLaunchScreen { get { return this.LaunchScreenPath != null && this.LaunchScreenPath.Length > 0; } }
        public string ProjectTitle { get { return this.GetString("title"); } set { this.SetString("title", value); } }
        public string JsFilePrefix { get { return this.GetString("jsFilePrefix"); } set { this.SetString("jsFilePrefix", value); } }
        public bool JsFullPage { get { return this.GetBoolean("jsFullPage"); } set { this.SetBoolean("jsFullPage", value); } }
        public string JsHeadExtras { get { return this.GetString("jsHeadExtras"); } set { this.SetString("jsHeadExtras", value); } }
        public string IosBundlePrefix { get { return this.GetString("iosBundlePrefix"); } set { this.SetString("iosBundlePrefix", value); } }
        public string IosDevTeamId { get { return this.GetString("iosDevTeamId"); } set { this.SetString("iosDevTeamId", value); } }
        public string JavaPackage { get { return this.GetString("javaPackage"); } set { this.SetString("javaPackage", value); } }
        public string Orientations { get { return this.GetString("orientations"); } set { this.SetString("orientations", value); } }
        public string Version { get { return this.GetString("version"); } set { this.SetString("version", value); } }
        public string Description { get { return this.GetString("description"); } set { this.SetString("description", value); } }
        public string AndroidSdkLocation { get { return this.GetString("androidSdk"); } set { this.SetString("androidSdk", value); } }
        public bool SkipAndroidWorkspaceXml { get { return this.GetBoolean("androidSkipWorkspaceXml"); } set { this.SetBoolean("androidSkipWorkspaceXml", value); } }

        public string ExportPlatform { get { return this.GetString("exportPlatform"); } set { this.SetString("exportPlatform", value); } }
        public string ProjectDirectory { get { return this.GetString("projectDirectory"); } set { this.SetString("projectDirectory", value); } }
        public string OutputDirectory { get { return this.GetString("outputDirectory"); } set { this.SetString("outputDirectory", value); } }

        public bool IsAndroid { get { return this.ExportPlatform.ToLowerInvariant() == "javascript-app-android"; } }
    }
}
