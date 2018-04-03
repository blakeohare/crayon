using System.Xml;
using System.Xml.Serialization;

namespace Build.BuildParseNodes
{
    public class ImageSheet
    {
        [XmlAttribute("id")]
        public string Id { get; set; }

        [XmlElement("prefix")]
        public string[] Prefixes { get; set; }
    }
}
