using CommonUtil.Text;
using System.Collections.Generic;
using System.Linq;

namespace CommonUtil.Resources
{
    public class ResourceStore
    {
        internal System.Reflection.Assembly Assembly { get; private set; }

        public ResourceStore(object platformSpecificSourceDescriptor)
        {
            if (platformSpecificSourceDescriptor is System.Type)
            {
                this.Assembly = ((System.Type)platformSpecificSourceDescriptor).Assembly;
            }
            else
            {
                this.Assembly = (System.Reflection.Assembly)platformSpecificSourceDescriptor;
            }
        }

        public string ReadAssemblyFileText(string path)
        {
            return ReadAssemblyFileText(path, false);
        }

        public string ReadAssemblyFileText(string path, bool failSilently)
        {
            byte[] bytes = ReadAssemblyFileBytes(path, failSilently);
            if (bytes == null)
            {
                return null;
            }
            return UniversalTextDecoder.Decode(bytes);
        }

        public byte[] ReadAssemblyFileBytes(string path)
        {
            return ReadAssemblyFileBytes(path, false);
        }

        // ResourceStores are created willy-nilly for duplicate physical stores (for C# this is the System.Reflection.Assembly).
        // It's better to just cache this statically even though it seems like it would make sense by looking at the
        // code that this could be per-instance.
        private static Dictionary<System.Reflection.Assembly, Dictionary<string, string>> caseInsensitiveLookup =
            new Dictionary<System.Reflection.Assembly, Dictionary<string, string>>();

        private static readonly byte[] BUFFER = new byte[1000];

        public byte[] ReadAssemblyFileBytes(string path, bool failSilently)
        {
            System.Reflection.Assembly assembly = this.Assembly;
            string canonicalizedPath = path.Replace('/', '.');
            // a rather odd difference...
            canonicalizedPath = canonicalizedPath.Replace('-', '_');
            string assemblyName = assembly.GetName().Name.ToLowerInvariant();

            Dictionary<string, string> nameLookup;
            if (!caseInsensitiveLookup.TryGetValue(assembly, out nameLookup))
            {
                nameLookup = new Dictionary<string, string>();
                caseInsensitiveLookup[assembly] = nameLookup;
                foreach (string resource in assembly.GetManifestResourceNames())
                {
                    string lookupName = resource.ToLowerInvariant();
                    if (resource.Contains('_'))
                    {
                        // this is silly, but VS gets confused easily, even when marked as embedded resources.
                        lookupName = lookupName
                            .Replace("_cs.txt", ".cs")
                            .Replace("_csproj.txt", ".csproj")
                            .Replace("_sln.txt", ".sln")
                            .Replace("_java.txt", ".java")
                            .Replace("_js.txt", ".js")
                            .Replace("_php.txt", ".php")
                            .Replace("_py.txt", ".py")
                            .Replace("_xml.txt", ".xml")
                            .Replace("_swift.txt", ".swift");
                    }

                    nameLookup[lookupName] = resource;
                }
            }

            string fullPath = assembly.GetName().Name + "." + canonicalizedPath;
            if (!nameLookup.ContainsKey(fullPath.ToLowerInvariant()))
            {
                if (failSilently)
                {
                    return null;
                }

                throw new System.Exception(path + " not marked as an embedded resource.");
            }

            System.IO.Stream stream = assembly.GetManifestResourceStream(nameLookup[fullPath.ToLowerInvariant()]);
            List<byte> output = new List<byte>();
            int bytesRead = 1;
            while (bytesRead > 0)
            {
                bytesRead = stream.Read(BUFFER, 0, BUFFER.Length);
                if (bytesRead == BUFFER.Length)
                {
                    output.AddRange(BUFFER);
                }
                else
                {
                    for (int i = 0; i < bytesRead; ++i)
                    {
                        output.Add(BUFFER[i]);
                    }
                    bytesRead = 0;
                }
            }

            return output.ToArray();
        }
    }
}
