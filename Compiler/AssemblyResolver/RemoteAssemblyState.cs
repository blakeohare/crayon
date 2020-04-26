using CommonUtil.Disk;
using System.Collections.Generic;
using System.Linq;

namespace AssemblyResolver
{
    internal class RemoteAssemblyState
    {
        private RemoteAssemblyManifest remoteAssemblyManifest;

        internal RemoteAssemblyState(RemoteAssemblyManifest remoteAssemblyManifest)
        {
            this.remoteAssemblyManifest = remoteAssemblyManifest;
        }

        internal void SerializeToRow(System.Text.StringBuilder sb)
        {
            sb.Append(this.Id);
            sb.Append(',');
            sb.Append(this.Url);
            sb.Append(',');
            sb.Append(this.LocalDirectory);
            sb.Append(',');
            sb.Append(this.IsStable ? '1' : '0');
            sb.Append(',');
            sb.Append(this.IsLatestStable ? '1' : '0');
            sb.Append(',');
            sb.Append(this.LastUsed);
            sb.Append(',');
            sb.Append(this.LastLatestCheck);
            sb.Append(',');
            sb.Append(this.HasCni ? '1' : '0');
            sb.Append(',');
            sb.Append(this.RuntimeIsDownloaded ? '1' : '0');
            sb.Append(',');
            if (this.platformsSupportedAndCurrentSyncState.Count > 0)
            {
                sb.Append(string.Join("|", this.platformsSupportedAndCurrentSyncState.Keys
                    .OrderBy(s => s.ToLowerInvariant())
                    .Select(s => (this.platformsSupportedAndCurrentSyncState[s] ? "*" : "") + s)));
            }
            sb.Append(',');
            sb.Append(this.Version);
        }

        internal static RemoteAssemblyState ParseFromManifestRow(RemoteAssemblyManifest manifest, string line)
        {
            string[] columns = line.Split(',');
            if (columns.Length < 11)
            {
                return null;
            }
            string id = columns[0].Trim();
            string url = columns[1].Trim();
            string directoryName = columns[2].Trim();
            string isStable = columns[3].Trim();
            string wasExplicitVersionRequest = columns[4].Trim();
            string lastUsedTimestampRaw = columns[5].Trim();
            string lastLatestCheckRaw = columns[6].Trim();
            string hasCni = columns[7].Trim();
            string isDllDownloaded = columns[8].Trim();

            string[] platformsRaw = columns[9].Split('|').Select(s => s.Trim()).Where(s => s.Length > 0).ToArray();
            string[] platforms = platformsRaw.Select(s => s[0] == '*' ? s.Substring(1) : s).ToArray();
            bool[] platformIsDownloaded = platformsRaw.Select(s => s[0] == '*').ToArray();

            string version = columns[10];
            for (int i = 11; i < columns.Length; ++i)
            {
                version += "," + columns[i];
            }
            version = version.Trim();

            int lastUsedTimestamp;
            int lastLatestCheck;

            if (!int.TryParse(lastUsedTimestampRaw, out lastUsedTimestamp) ||
                !int.TryParse(lastLatestCheckRaw, out lastLatestCheck))
            {
                return null;
            }

            RemoteAssemblyState ras = new RemoteAssemblyState(manifest)
            {
                Id = id,
                LastUsed = lastUsedTimestamp,
                LastLatestCheck = lastLatestCheck,
                LocalDirectory = directoryName,
                Url = url,
                Version = version,
                IsLatestStable = wasExplicitVersionRequest == "1",
                IsStable = isStable == "1",
                HasCni = hasCni == "1",
                RuntimeIsDownloaded = isDllDownloaded == "1",
            };

            for (int i = 0; i < platforms.Length; ++i)
            {
                ras.platformsSupportedAndCurrentSyncState[platforms[i]] = platformIsDownloaded[i];
            }

            return ras;
        }

        public void IndicatePlatformSupport(string platformName)
        {
            if (!this.platformsSupportedAndCurrentSyncState.ContainsKey(platformName))
            {
                this.platformsSupportedAndCurrentSyncState[platformName] = false;
            }
        }

        private Dictionary<string, bool> platformsSupportedAndCurrentSyncState = new Dictionary<string, bool>();

        // The root URL for the assembly without the version number included. Should end in ".crylib".
        public string Url { get; set; }

        // The version of this entry. This is always the explicit version, even if it was fetched as the latest version of the library.
        public string Version { get; set; }

        // true if this is the latest stable version for the given library.
        public bool IsLatestStable { get; set; }

        // True if the distributor has marked this as stable.
        public bool IsStable { get; set; }

        // The last time this library and version combination was used by this machine
        public int LastUsed { get; set; }

        // If this library was fetched as the latest library, when was the last time you checked that this was the latest? If not fetched as the latest, then 0.
        public int LastLatestCheck { get; set; }

        // The name of the directory containing the data.
        public string LocalDirectory { get; set; }

        // Absolute path of the library directory itself.
        public string AbsolutePathToLibrary { get { return Path.Join(this.remoteAssemblyManifest.Directory, this.LocalDirectory, this.Id); } }

        // Basic name of the library
        public string Id { get; set; }

        // True if this requires native files and a runtime.
        public bool HasCni { get; set; }

        public bool RuntimeIsDownloaded { get; set; }

        public bool IsPlatformSupported(string platform) { return this.platformsSupportedAndCurrentSyncState.ContainsKey(platform); }
        public bool IsPlatformDownloaded(string platform) { return this.platformsSupportedAndCurrentSyncState.ContainsKey(platform) && this.platformsSupportedAndCurrentSyncState[platform]; }

        public AssemblyMetadata CreateNewAssemblyMetadataInstance()
        {
            return AssemblyMetadataFactory.CreateLibrary(this.LocalDirectory, this.Id);
        }
    }
}
