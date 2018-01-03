using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Build.BuildParseNodes
{
    class XmlParserForBuild
    {
        public static BuildRoot Parse(string file)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(BuildRoot));
            try
            {
                return (BuildRoot)xmlSerializer.Deserialize(new System.IO.StringReader(file));
            }
            catch (InvalidOperationException e)
            {
                // Yeah, yeah, I know...
                string[] parts = e.Message.Split('(');
                if (parts.Length == 2)
                {
                    parts = parts[1].Split(')');
                    if (parts.Length == 2)
                    {
                        parts = parts[0].Split(',');
                        if (parts.Length == 2)
                        {
                            int line, col;

                            if (int.TryParse(parts[0], out line) && int.TryParse(parts[1], out col))
                            {
                                throw new InvalidOperationException("There is an XML syntax error in the build file on line " + line + ", column " + col);
                            }
                        }
                    }
                }
                throw new InvalidOperationException("An error occurred while parsing the build file.");
            }
        }
    }
}
