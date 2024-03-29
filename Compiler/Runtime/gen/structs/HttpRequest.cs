﻿using System.Collections.Generic;

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
