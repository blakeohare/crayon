using Common;
using System.Xml;
using System.Xml.Serialization;

namespace Build.BuildParseNodes
{
    public abstract class BuildItem
    {
        [XmlElement("projectname")]
        public string ProjectName { get; set; }

        [XmlElement("version")]
        public string Version { get; set; }

        [XmlElement("description")]
        public string Description { get; set; }

        [XmlElement("source")]
        public SourceItem[] Sources { get; set; }

        public SourceItem[] SourcesNonNull { get { return this.Sources ?? new SourceItem[0]; } }

        [XmlElement("output")]
        public string Output { get; set; }

        [XmlArray("imagesheets")]
        [XmlArrayItem("sheet")]
        public ImageSheet[] ImageSheets { get; set; }

        [XmlElement("jsfileprefix")]
        public string JsFilePrefix { get; set; }

        [XmlElement("jsfullpage")]
        public string JsFullPage { get; set; }

        [XmlElement("minified")]
        public string MinifiedRaw { get; set; }

        [XmlElement("readablebytecode")]
        public string ExportDebugByteCodeRaw { get; set; }

        [XmlElement("var")]
        public BuildVar[] Var { get; set; }

        [XmlElement("guidseed")]
        public string GuidSeed { get; set; }

        [XmlElement("icon")]
        public string IconFilePath { get; set; }

        [XmlElement("launchscreen")]
        public string LaunchScreen { get; set; }

        [XmlElement("title")]
        public string DefaultTitle { get; set; }

        // comma-delimited list of values
        // { portrait | upsidedown | landscape | landscapeleft | landscaperight | all }
        // landscape is a shortcut of landscapeleft,landscaperight
        // see Common/OrientationParser.cs
        [XmlElement("orientation")]
        public string Orientation { get; set; }

        [XmlElement("crayonpath")]
        public string[] CrayonPath { get; set; }

        [XmlElement("iosbundleprefix")]
        public string IosBundlePrefix { get; set; }

        [XmlElement("javapackage")]
        public string JavaPackage { get; set; }

        [XmlElement("windowsize")]
        public Size WindowSize { get; set; }

        [XmlElement("compilerlocale")]
        public string CompilerLocale { get; set; }

        public bool Minified
        {
            get { return Util.StringToBool(this.MinifiedRaw); }
        }

        public bool ExportDebugByteCode
        {
            get { return Util.StringToBool(this.ExportDebugByteCodeRaw); }
        }
    }
}
