namespace AssemblyResolver
{
    internal class RemoteAssemblyState
    {
        public string Url { get; set; }
        public string Version { get; set; }
        public int LastUsed { get; set; }
        public int LastFetched { get; set; }
        public string LocalDirectory { get; set; }
        public string Id { get; set; }

        public AssemblyMetadata CreateNewAssemblyMetadataInstance()
        {
            return new AssemblyMetadata(this.LocalDirectory, this.Id);
        }
    }
}
