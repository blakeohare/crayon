namespace Wax
{
    public class CbxBundleView
    {
        public string ByteCode { get; set; }
        public ResourceDatabase ResourceDB { get; set; }

        public CbxBundleView(string byteCode, ResourceDatabase resDb)
        {
            this.ByteCode = ByteCode;
            this.ResourceDB = resDb;
        }
    }
}
