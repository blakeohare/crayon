namespace Wax
{
    public static class CbxFileEncoder
    {
        public static byte[] Encode(CbxBundle cbxBundle)
        {
            ResourceDatabase resDb = cbxBundle.ResourceDB;
            FileOutput manifest = resDb.ResourceManifestFile;
            FileOutput images = resDb.ImageResourceManifestFile;
            string[] fileNames = resDb.FlatFileNames;
            FileOutput[] files = resDb.FlatFiles;

            CryPkgEncoder encoder = new CryPkgEncoder();
            encoder.AddTextFile("bytecode.txt", cbxBundle.ByteCode);
            encoder.AddTextFile("manifest.txt", manifest == null ? "" : manifest.TextContent);
            encoder.AddTextFile("images.txt", images == null ? "" : images.TextContent);

            for (int i = 0; i < files.Length; i++)
            {
                encoder.AddFile(fileNames[i], files[i].BinaryContent);
            }

            return encoder.CreateCryPkg();
        }

        public static string EncodeBase64(CbxBundle cbxBundle)
        {
            return System.Convert.ToBase64String(Encode(cbxBundle));
        }
    }
}
