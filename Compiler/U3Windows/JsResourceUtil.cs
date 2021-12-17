using System.Collections.Generic;
using System.Linq;

namespace U3
{
    internal static class JsResourceUtil
    {
        private static readonly string PLATFORM = "win";

        private static Wax.EmbeddedResourceReader resourceReader = new Wax.EmbeddedResourceReader(typeof(JsResourceUtil).Assembly);

        private static string u3Source = null;
        internal static string GetU3Source()
        {
            if (u3Source == null)
            {
                string[] lines = resourceReader.GetText("u3/index.html", "\n").Split('\n');
                List<string> newLines = new List<string>();
                foreach (string line in lines)
                {
                    if (line.Contains("SCRIPTS_GO_HERE"))
                    {
                        string[] files =
                            resourceReader.ListFiles("u3/", false)
                                .Concat(resourceReader.ListFiles("u3/" + PLATFORM + "/", true))
                            .Where(name => name.EndsWith(".js"))
                            .ToArray();

                        foreach (string file in files)
                        {
                            newLines.Add("<script>");
                            newLines.Add("// " + file);
                            newLines.Add(resourceReader.GetText(file, "\n").Trim());
                            newLines.Add("</script>");
                        }
                    }
                    else
                    {
                        newLines.Add(line);
                    }
                }

                u3Source = string.Join("\n", newLines);
            }
            return u3Source;
        }
    }
}
