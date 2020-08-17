using CommonUtil.Json;
using System.Collections.Generic;
using System.Linq;

namespace CommonUtil.Http
{
    public class HttpResponse
    {
        internal enum ConnectionStatus
        {
            OK,
            TIMED_OUT,
            OFFLINE,
            MALFORMED_RESPONSE,
        }

        public int StatusCode { get; private set; }
        public string ContentType { get; private set; }
        public byte[] Content { get; private set; }
        public JsonLookup ContentJson { get; private set; }
        public bool HasNoConnection { get; private set; }
        public bool IsServerUnresponseive { get; private set; }
        public bool IsJson { get; private set; }

        private Dictionary<string, string> headers;

        internal HttpResponse(int statusCode, string contentType, byte[] content, List<string> rawHeadersInformation)
        {
            this.StatusCode = statusCode;
            this.ContentType = contentType;
            this.Content = content;
            this.IsJson = false;
            switch (this.ContentType.ToLowerInvariant().Trim())
            {
                case "application/json":
                    this.IsJson = true;
                    this.ContentJson = new JsonLookup(new JsonParser(this.ContentUtf8).ParseAsDictionary());
                    break;

                default:
                    break;
            }

            this.headers = new Dictionary<string, string>();
            for (int i = 0; i < rawHeadersInformation.Count; i += 2)
            {
                string name = CanonicalizeHeader(rawHeadersInformation[i]);
                string value = rawHeadersInformation[i + 1].Trim();
                this.headers[name] = value;
            }
        }

        private static string CanonicalizeHeader(string value)
        {
            return value.Trim().ToUpperInvariant().Replace('-', '_');
        }

        public string GetHeader(string name)
        {
            string k = CanonicalizeHeader(name);
            if (this.headers.ContainsKey(k))
            {
                return this.headers[k];
            }
            return null;
        }

        public bool GetHeaderAsBoolean(string name)
        {
            string value = this.GetHeader(name);
            return BoolUtil.Parse(value);
        }

        public string[] GetHeaderAsList(string name)
        {
            string value = this.GetHeader(name);
            if (value == null || value.Length == 0) return new string[0];
            return value.Split(',').Select(s => s.Trim()).ToArray();
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
