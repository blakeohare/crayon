using System.Text;

namespace AssemblyResolver
{
    internal class RemoteAssemblyUrl
    {
        public bool IsValid { get; private set; }
        public bool IsSecure { get; private set; }
        public bool IsLatestVersion { get; private set; }
        public string Url { get; private set; }
        public string VersionInfo { get; private set; }

        public static RemoteAssemblyUrl FromUrl(string urlAndVersion)
        {
            bool isSecure;
            string[] parts = urlAndVersion.Split(':');

            if (parts.Length == 1) return new RemoteAssemblyUrl() { IsValid = false };

            switch (parts[0])
            {
                case "http": isSecure = false; break;
                case "https": isSecure = true; break;
                default: return new RemoteAssemblyUrl() { IsValid = false };
            }

            // TODO: allow a port number?

            string versionInfo = null;
            string url = parts[0] + parts[1];
            if (parts.Length > 2)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(parts[2]);
                for (int i = 3; i < parts.Length; ++i)
                {
                    sb.Append(':');
                    sb.Append(parts[i]);
                }
                versionInfo = sb.ToString();
            }

            return new RemoteAssemblyUrl()
            {
                IsValid = true,
                Url = url,
                IsSecure = isSecure,
                VersionInfo = versionInfo,
                IsLatestVersion = versionInfo == null,
            };
        }
    }
}
