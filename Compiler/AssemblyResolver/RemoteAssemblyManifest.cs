using Common;
using CommonUtil.Disk;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssemblyResolver
{
    internal class RemoteAssemblyManifest
    {
        private const int DELETE_ASSEMBLIES_THAT_HAVENT_BEEN_USED_IN_THIS_MANY_DAYS = 180;
        private string directory = null;

        public string Directory
        {
            get
            {
                if (this.directory == null)
                {
                    string appData = CommonUtil.Environment.EnvironmentVariables.Get("APPDATA");
                    if (appData == null)
                    {
                        throw new System.InvalidOperationException("No %APPDATA% environment variable currently set.");
                    }
                    this.directory = Path.Join(appData, "Crayon", "libstore");
                    FileUtil.EnsureFolderExists(this.directory);
                }
                return this.directory;
            }
        }

        private string ManifestFilePath { get { return Path.Join(this.Directory, "libmanifest.txt"); } }

        private List<RemoteAssemblyState> remoteAssemblies = null;
        public RemoteAssemblyState[] RemoteAssemblies
        {
            get
            {
                if (this.remoteAssemblies == null)
                {
                    this.remoteAssemblies = new List<RemoteAssemblyState>(Parse());
                }
                return this.remoteAssemblies.ToArray();
            }
        }

        private IList<RemoteAssemblyState> Parse()
        {
            string manifestFile = this.ManifestFilePath;
            if (!System.IO.File.Exists(manifestFile))
            {
                return new RemoteAssemblyState[0];
            }

            int now = CommonUtil.DateTime.Time.UnixTimeNow;

            List<RemoteAssemblyState> remoteAssemblies = new List<RemoteAssemblyState>();
            List<string> invalidLines = new List<string>();
            foreach (string line in System.IO.File.ReadAllLines(manifestFile))
            {
                string trimmedLine = line.Trim();
                if (trimmedLine.Length > 0)
                {
                    RemoteAssemblyState state = RemoteAssemblyState.ParseFromManifestRow(this, trimmedLine);
                    if (state == null)
                    {
                        invalidLines.Add(line);
                    }
                    else
                    {
                        string outermostPath = Path.Join(this.Directory, state.LocalDirectory);
                        if (FileUtil.DirectoryExists(outermostPath))
                        {
                            if (FileUtil.FileExists(Path.Join(state.AbsolutePathToLibrary, "manifest.json")) &&
                                now - state.LastUsed < DELETE_ASSEMBLIES_THAT_HAVENT_BEEN_USED_IN_THIS_MANY_DAYS * 24 * 3600)
                            {
                                remoteAssemblies.Add(state);
                            }
                            else
                            {
                                FileUtil.DirectoryDelete(outermostPath);
                            }
                        }
                    }
                }
            }

            foreach (string line in invalidLines)
            {
                Common.ConsoleWriter.Print(ConsoleMessageType.REMOTE_ASSEMBLY_ERROR, "Invalid assembly in manifest! Ignoring: " + line);
            }

            return remoteAssemblies;
        }

        public RemoteAssemblyState GetAssemblyStateLatest(string url)
        {
            return GetAssemblyState(url, null);
        }

        public RemoteAssemblyState GetAssemblyState(string url, string version)
        {
            bool isRequestForLatestStable = version == null;
            List<RemoteAssemblyState> candidates = new List<RemoteAssemblyState>();
            foreach (RemoteAssemblyState state in this.RemoteAssemblies)
            {
                if (state.Url == url)
                {
                    if (isRequestForLatestStable)
                    {
                        if (state.IsStable)
                        {
                            // TODO: don't add candidates that aren't marked as stable by the server.
                            // A user could potentially specifically request version-2-alpha, but latest-stable should return version-1-release
                            candidates.Add(state);
                        }
                    }
                    else if (state.Version == version)
                    {
                        candidates.Add(state);
                    }
                }
            }

            if (candidates.Count == 0) return null;
            if (candidates.Count == 1) return candidates[0];

            VersionComparator vc = new VersionComparator();
            candidates.Sort(new AssemblyComparerByVersion());

            candidates[0].IsLatestStable = true;

            return candidates[0];
        }

        private class AssemblyComparerByVersion : IComparer<RemoteAssemblyState>
        {
            private VersionComparator versionCompare = new VersionComparator();
            public int Compare(RemoteAssemblyState x, RemoteAssemblyState y)
            {
                // flip the sign to get most recent version first.
                return -this.versionCompare.Compare(x.Version, y.Version);
            }
        }

        internal void AddOrReplaceAssemblyState(RemoteAssemblyState assembly)
        {
            bool alreadyExists = false;
            foreach (RemoteAssemblyState existingAssembly in this.remoteAssemblies.Where(a => a.Url == assembly.Url))
            {
                if (existingAssembly.Version == assembly.Version)
                {
                    alreadyExists = true;
                    existingAssembly.IsLatestStable = assembly.IsLatestStable;
                    existingAssembly.IsStable = assembly.IsStable;
                }
                else if (assembly.IsLatestStable)
                {
                    existingAssembly.IsLatestStable = false;
                }
            }

            if (!alreadyExists)
            {
                this.remoteAssemblies.Add(assembly);
            }
        }

        public void ReserializeFile()
        {
            int now = CommonUtil.DateTime.Time.UnixTimeNow;
            StringBuilder sb = new StringBuilder();
            foreach (RemoteAssemblyState ras in this.RemoteAssemblies)
            {
                ras.SerializeToRow(sb);
                sb.Append('\n');
            }

            FileUtil.EnsureFolderExists(this.Directory);
            FileUtil.WriteFileText(this.ManifestFilePath, sb.ToString());
        }
    }
}
