using System.Xml;
using System.Xml.Serialization;

namespace Build.BuildParseNodes
{
    public class BuildVar
    {
        [XmlElement("id")]
        public string Id { get; set; }

        [XmlAttribute("type")]
        public string Type { get; set; }

        [XmlElement("value")]
        public string Value { get; set; }

        [XmlElement("env")]
        public string EnvironmentVarValue { get; set; }
    }
}
