namespace AssemblyResolver
{
    internal class HttpResponse
    {
        public int StatusCode { get; private set; }
        public string ContentType { get; private set;  }
        public byte[] Content { get; private set; }

        public HttpResponse(int statusCode, string contentType, byte[] content)
        {
            this.StatusCode = statusCode;
            this.ContentType = contentType;
            this.Content = content;
        }

        public string ContentUtf8
        {
            get
            {
                return System.Text.Encoding.UTF8.GetString(this.Content);
            }
        }
    }
}
