using Interpreter.Structs;
using System.Collections.Generic;
using System.Linq;

namespace Interpreter.Structs
{
    public class HttpRequest
    {
        public int statusCode;
        public string status;
        public Dictionary<string, string[]> headers;
        public string body;

        public HttpRequest(int statusCode, string status, Dictionary<string, string[]> headers, string body)
        {
            this.statusCode = statusCode;
            this.status = status;
            this.headers = headers;
            this.body = body;
        }
    }

}
