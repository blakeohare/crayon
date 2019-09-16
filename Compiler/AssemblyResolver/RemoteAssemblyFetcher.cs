using Common;
using CommonUtil.Disk;
using System.Text;

namespace AssemblyResolver
{
    public enum FetchAssemblyStatus
    {
        INVALID_URL,
        NO_CONNECTION,
        SERVER_NOT_RESPONDING,
        UNKNOWN_RESPONSE,
        LIBRARY_NOT_FOUND,
        VERSION_NOT_FOUND,
        SUCCESS,
    }

    internal class RemoteAssemblyFetcher
    {
        public Pair<FetchAssemblyStatus, RemoteAssemblyState> FetchNewAssembly(RemoteAssemblyManifest manifest, string urlAndVersion)
        {
            RemoteAssemblyUrl structuredUrl = RemoteAssemblyUrl.FromUrl(urlAndVersion);
            if (!structuredUrl.IsValid) return new Pair<FetchAssemblyStatus, RemoteAssemblyState>(FetchAssemblyStatus.INVALID_URL, null);

            HttpRequestSender requestSender = new HttpRequestSender("GET", structuredUrl.Url)
                .SetHeader("CrayonLib-Component-Type", "source");

            if (!structuredUrl.IsLatestStableVersionRequested)
            {
                requestSender.SetHeader("CrayonLib-Version-Info", structuredUrl.VersionInfo);
            }

            HttpResponse response = requestSender.Send();
            Pair<FetchAssemblyStatus, RemoteAssemblyState> error = CheckForBasicErrors(response, structuredUrl.LibraryName);
            if (error != null) return error;

            string versionInfo = response.GetHeader("CrayonLib-Version-Info");
            // Not used. Not sure why this is returned.
            // string componentType = response.GetHeader("CrayonLib-Component-Type");
            bool hasNativeInterface = response.GetHeaderAsBoolean("CrayonLib-Has-Native-Interface");
            string[] platformsSupported = response.GetHeaderAsList("CrayonLib-Platforms-Supported");
            string libraryName = response.GetHeader("CrayonLib-Library-Name");
            bool isStable = response.GetHeaderAsBoolean(("CrayonLib-Is-Stable"));
            string directoryName = this.CreateNewDirectoryForLibrary(manifest.Directory, libraryName, versionInfo);

            byte[] crypkg = response.Content;
            CryPkgDecoder crypkgDecoder = new CryPkgDecoder(response.Content);
            // TODO: crypkgDecoder.IsValid

            this.ExpandCryPkgToDirectory(crypkgDecoder, System.IO.Path.Combine(manifest.Directory, directoryName, libraryName));
            int now = Util.UnixTime;
            RemoteAssemblyState ras = new RemoteAssemblyState(manifest)
            {
                HasCni = hasNativeInterface,
                Id = libraryName,
                IsStable = isStable,
                IsLatestStable = structuredUrl.IsLatestStableVersionRequested,
                LastUsed = now,
                LastLatestCheck = now,
                LocalDirectory = directoryName,
                RuntimeIsDownloaded = false,
                Url = structuredUrl.Url,
                Version = versionInfo,
            };

            foreach (string platformSupported in platformsSupported)
            {
                ras.IndicatePlatformSupport(platformSupported);
            }

            return new Pair<FetchAssemblyStatus, RemoteAssemblyState>(FetchAssemblyStatus.SUCCESS, ras);
        }

        private string CreateNewDirectoryForLibrary(string manifestDir, string libraryName, string version)
        {
            int numSuffix = 0;
            StringBuilder sb = new StringBuilder();
            sb.Append(libraryName);
            sb.Append('_');
            foreach (char c in version.ToCharArray())
            {
                if ((c >= 'a' && c <= 'z') ||
                    (c >= 'A' && c <= 'Z') ||
                    (c >= '0' && c <= '9'))
                {
                    sb.Append(c);
                }
                else
                {
                    sb.Append('_');
                }
            }
            string baseName = sb.ToString();
            while (true)
            {
                string directoryName = baseName + (numSuffix == 0 ? "" : ("_" + numSuffix));
                string directoryFullPath = System.IO.Path.Combine(manifestDir, directoryName);
                if (!System.IO.Directory.Exists(directoryFullPath))
                {
                    FileUtil.EnsureFolderExists(System.IO.Path.Combine(directoryFullPath, libraryName));
                    return directoryName;
                }
                numSuffix++;
            }
        }

        private Pair<FetchAssemblyStatus, RemoteAssemblyState> CheckForBasicErrors(HttpResponse response, string libraryName)
        {
            if (response.HasNoConnection) return new Pair<FetchAssemblyStatus, RemoteAssemblyState>(FetchAssemblyStatus.NO_CONNECTION, null);
            if (response.IsServerUnresponseive) return new Pair<FetchAssemblyStatus, RemoteAssemblyState>(FetchAssemblyStatus.SERVER_NOT_RESPONDING, null);

            if (response.StatusCode != 200)
            {
                if (response.IsJson)
                {
                    string errorReason = response.ContentJson.GetAsString("reason");
                    switch (errorReason.Trim().ToUpper().Split(' ')[0])
                    {
                        case "VERSION_NOT_FOUND": return new Pair<FetchAssemblyStatus, RemoteAssemblyState>(FetchAssemblyStatus.VERSION_NOT_FOUND, null);
                        case "LIBRARY_NOT_FOUND": return new Pair<FetchAssemblyStatus, RemoteAssemblyState>(FetchAssemblyStatus.LIBRARY_NOT_FOUND, null);
                        default:
                            return new Pair<FetchAssemblyStatus, RemoteAssemblyState>(FetchAssemblyStatus.UNKNOWN_RESPONSE, null);
                    }
                }
                return new Pair<FetchAssemblyStatus, RemoteAssemblyState>(FetchAssemblyStatus.UNKNOWN_RESPONSE, null);
            }

            string libraryNameAccordingToHeader = response.GetHeader("CrayonLib-Library-Name");

            // sanity check
            if (libraryName != libraryNameAccordingToHeader)
            {
                return new Pair<FetchAssemblyStatus, RemoteAssemblyState>(FetchAssemblyStatus.UNKNOWN_RESPONSE, null);
            }

            return null;
        }

        private void ExpandCryPkgToDirectory(CryPkgDecoder crypkg, string dir)
        {
            ExpandCryPkgToDirectory(crypkg, dir, ".");
        }

        private void ExpandCryPkgToDirectory(CryPkgDecoder crypkg, string diskCurrent, string current)
        {
            foreach (string file in crypkg.ListDirectory(current, true, false))
            {
                string pkgPath = current == "." ? file : (current + "/" + file);
                string diskPath = System.IO.Path.Combine(diskCurrent, file);
                byte[] data = crypkg.ReadFileBytes(pkgPath);
                FileUtil.WriteFileBytes(diskPath, data);
            }

            foreach (string dir in crypkg.ListDirectory(current, false, true))
            {
                string pkgPath = current == "." ? dir : (current + "/" + dir);
                string diskPath = System.IO.Path.Combine(diskCurrent, dir);
                FileUtil.EnsureFolderExists(diskPath);
                ExpandCryPkgToDirectory(crypkg, diskPath, pkgPath);
            }
        }
    }
}
