using System.Collections.Generic;
using System.Linq;

namespace AssemblyResolver
{
    internal class RemoteAssemblyManifest
    {
        private string directory = null;

        public string Directory
        {
            get
            {
                if (this.directory == null)
                {
                    string appData = System.Environment.GetEnvironmentVariable("%APPDATA%");
                    if (appData == null)
                    {
                        throw new System.InvalidOperationException("No %APPDATA% environment variable currently set.");
                    }
                    this.directory = System.IO.Path.Combine(appData, "Roaming", "Crayon", "libstore");
                    Common.FileUtil.EnsureFolderExists(this.directory);
                }
                return this.directory;
            }
        }

        private RemoteAssemblyState[] remoteAssemblies = null;
        public RemoteAssemblyState[] RemoteAssemblies
        {
            get
            {
                if (this.remoteAssemblies == null)
                {
                    this.remoteAssemblies = Parse().ToArray();
                }
                return this.remoteAssemblies;
            }
        }

        private ICollection<RemoteAssemblyState> Parse()
        {
            string manifestFile = System.IO.Path.Combine(this.Directory, "libs.manifest");
            if (!System.IO.File.Exists(manifestFile))
            {
                return new RemoteAssemblyState[0];
            }

            List<RemoteAssemblyState> remoteAssemblies = new List<RemoteAssemblyState>();
            List<string> invalidLines = new List<string>();
            foreach (string line in System.IO.File.ReadAllLines(manifestFile))
            {
                string trimmedLine = line.Trim();
                if (trimmedLine.Length == 0) continue;
                string[] columns = trimmedLine.Split(',');
                if (columns.Length < 6)
                {
                    invalidLines.Add(line);
                    continue;
                }
                string id = columns[0];
                string url = columns[1];
                string directoryName = columns[2];
                string downloadTimestampRaw = columns[3];
                string lastUsedTimestampRaw = columns[4];
                string version = columns[5];
                for (int i = 6; i < columns.Length; ++i)
                {
                    version += "," + columns[i];
                }

                int downloadTimestamp;
                int lastUsedTimestamp;

                if (!int.TryParse(downloadTimestampRaw, out downloadTimestamp) || !
                    int.TryParse(lastUsedTimestampRaw, out lastUsedTimestamp))
                {
                    invalidLines.Add(line);
                    continue;
                }

                string directory = System.IO.Path.Combine(this.Directory, directoryName);
                string manifestPath = System.IO.Path.Combine(directory, "manifest.json");

                remoteAssemblies.Add(new RemoteAssemblyState()
                {
                    Id = id,
                    LastFetched = downloadTimestamp,
                    LastUsed = lastUsedTimestamp,
                    LocalDirectory = directory,
                    Url = url,
                    Version = version
                });
            }

            foreach (string line in invalidLines)
            {
                System.Console.Error.WriteLine("Invalid assembly in manifest! Ignoring: " + line);
            }

            return remoteAssemblies;
        }

        public RemoteAssemblyState GetAssemblyState(string url, string version)
        {
            List<RemoteAssemblyState> candidates = new List<RemoteAssemblyState>();
            foreach (RemoteAssemblyState state in this.RemoteAssemblies)
            {
                if (state.Url == url)
                {
                    if (version == "LATEST")
                    {
                        candidates.Add(state);
                    }
                    else if (state.Version == version)
                    {
                        candidates.Add(state);
                    }
                }
            }

            if (candidates.Count == 0) return null;
            if (candidates.Count == 1) return candidates[0];

            // TODO: sort candidates by version
            // TODO: determine if a version was downloaded because it's the latest or because that's the version that was asked for.
            // If the latter, then you need to sync a later version and if it's the latest, mark it as the former situation.

            throw new System.NotImplementedException();
        }
    }
}
