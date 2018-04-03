using System.Xml;
using System.Xml.Serialization;

namespace Build.BuildParseNodes
{
    public class Size
    {
        [XmlAttribute("width")]
        public string Width { get; set; }

        [XmlAttribute("height")]
        public string Height { get; set; }

        public static Size Merge(Size primary, Size secondary)
        {
            if (primary == null) return secondary;
            if (secondary == null) return primary;
            return new Size()
            {
                Width = primary.Width ?? secondary.Width,
                Height = primary.Height ?? secondary.Height,
            };
        }
    }
}
