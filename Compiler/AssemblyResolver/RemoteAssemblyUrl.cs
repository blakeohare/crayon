using System.Collections.Generic;
using System.Text;

namespace AssemblyResolver
{
    internal class RemoteAssemblyUrl
    {
        public bool IsValid { get; private set; }
        public bool IsSecure { get; private set; }
        public bool IsLatestStableVersionRequested { get; private set; }
        public string Url { get; private set; }
        public string LibraryName { get; private set; }
        public string VersionInfo { get; private set; }

        public static RemoteAssemblyUrl FromUrl(string urlAndVersion)
        {
            bool isSecure;
            List<string> parts = new List<string>(urlAndVersion.Trim().Split(':'));

            if (parts.Count == 1) return new RemoteAssemblyUrl() { IsValid = false };

            switch (parts[0])
            {
                case "http": isSecure = false; break;
                case "https": isSecure = true; break;
                default: return new RemoteAssemblyUrl() { IsValid = false };
            }

            // This is a bit hacky...
            if (parts.Count >= 3 && parts[2].EndsWith(".crylib"))
            {
                // There's a port number here. Rejoin those segments
                parts[1] += ":" + parts[2];
                parts.RemoveAt(2);
            }

            string versionInfo = null;
            string url = parts[0] + ":" + parts[1];
            if (parts.Count > 2)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(parts[2]);
                for (int i = 3; i < parts.Count; ++i)
                {
                    sb.Append(':');
                    sb.Append(parts[i]);
                }
                versionInfo = sb.ToString();
            }

            int index = url.LastIndexOf(".crylib");
            if (index == -1) return new RemoteAssemblyUrl() { IsValid = false };
            string urlNoFileExtension = url.Substring(0, index);
            int slashIndex = urlNoFileExtension.LastIndexOf('/');
            if (slashIndex == -1) return new RemoteAssemblyUrl() { IsValid = false };
            string name = urlNoFileExtension.Substring(slashIndex + 1);

            return new RemoteAssemblyUrl()
            {
                IsValid = true,
                Url = url,
                IsSecure = isSecure,
                LibraryName = name,
                VersionInfo = versionInfo,
                IsLatestStableVersionRequested = versionInfo == null,
            };
        }
    }
}
