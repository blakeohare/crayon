using System.Xml;
using System.Xml.Serialization;

namespace Build.BuildParseNodes
{
    public class SourceItem
    {
        [XmlAttribute("alias")]
        public string Alias { get; set; }

        [XmlText]
        public string Value { get; set; }
    }

}
