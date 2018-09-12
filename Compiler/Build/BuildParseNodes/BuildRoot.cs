using System.Xml;
using System.Xml.Serialization;

namespace Build.BuildParseNodes
{
    [XmlRoot("build")]
    public class BuildRoot : BuildItem
    {
        [XmlElement("target")]
        public Target[] Targets { get; set; }

        [XmlElement("programminglanguage")]
        public string ProgrammingLanguage { get; set; }
    }

}
