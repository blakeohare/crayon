using System.Xml;
using System.Xml.Serialization;

namespace Build.BuildParseNodes
{
    public class Target : BuildItem
    {
        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlElement("platform")]
        public string Platform { get; set; }
    }
}
