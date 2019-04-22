using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssemblyResolver
{
    internal class HttpRequestSender
    {
        private string method;
        private string url;
        private byte[] content;
        private string contentType;
        private Dictionary<string, string> requestHeaders = new Dictionary<string, string>();

        public HttpRequestSender(string method, string url)
        {
            this.method = method;
            this.url = url;
        }

        public HttpRequestSender SetHeader(string name, string value)
        {
            this.requestHeaders[name.ToLower()] = (value ?? "").Trim();
            return this;
        }

        private static readonly byte[] readBuffer = new byte[1000];

        public HttpResponse Send()
        {
            System.Net.HttpWebRequest req = (System.Net.HttpWebRequest) System.Net.WebRequest.Create(this.url);
            req.UserAgent = Common.VersionInfo.UserAgent;

            if (this.content != null)
            {
                req.ContentLength = this.content.Length;
                req.ContentType = this.contentType;
                using (System.IO.Stream stream = req.GetRequestStream())
                {
                    stream.Write(this.content, 0, this.content.Length);
                }
            }
            else
            {
                req.ContentLength = 0;
            }
            req.Method = this.method;

            System.Net.HttpWebResponse response = null;
            try
            {
                response = (System.Net.HttpWebResponse)req.GetResponse();
            }
            catch (System.Net.WebException we)
            {
                response = (System.Net.HttpWebResponse)we.Response;
            }
            catch (Exception e)
            {
                throw new NotImplementedException("Cannot handle exception: " + e.GetType().ToString());
            }

            System.IO.Stream responseStream = response.GetResponseStream();
            System.IO.BinaryReader br = new System.IO.BinaryReader(responseStream);
            int bytesRead = 0;
            List<byte> responseContent = new List<byte>();
            do
            {
                bytesRead = br.Read(readBuffer, 0, readBuffer.Length);
                if (bytesRead == readBuffer.Length)
                {
                    responseContent.AddRange(readBuffer);
                }
                else
                {
                    for (int i = 0; i < bytesRead; ++i)
                    {
                        responseContent.Add(readBuffer[i]);
                    }
                }
            } while (bytesRead > 0);

            byte[] responseBytes = responseContent.ToArray();
            string responseContentType = response.ContentType;
            int statusCode = (int)response.StatusCode;

            return new HttpResponse(statusCode, responseContentType, responseBytes);
        }
    }
}
