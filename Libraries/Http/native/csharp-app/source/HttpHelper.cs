using System;
using System.Collections.Generic;
using System.Net;
using Interpreter.Structs;

namespace Interpreter.Libraries.Http
{
    internal static class HttpHelper
    {
        public static void ReadResponseData(
            object httpRequest,
            int[] intOut,
            string[] stringOut,
            object[] responseNativeData,
            List<string> headersOut)
        {
            HttpResponseValue response = (HttpResponseValue)httpRequest;
            intOut[0] = response.StatusCode;
            intOut[1] = response.IsContentBinary ? 1 : 0;
            stringOut[0] = response.StatusMessage;
            stringOut[1] = response.ContentString;
            responseNativeData[0] = response.ContentBytes;
            headersOut.AddRange(response.Headers);
        }

        public static void GetResponseBytes(object byteArrayObj, Value[] integersCache, List<Value> output)
        {
            byte[] bytes = (byte[])byteArrayObj;
            int length = bytes.Length;
            for (int i = 0; i < length; ++i)
            {
                output.Add(integersCache[bytes[i]]);
            }
        }

        public static void SendRequestAsync(
            object[] requestNativeData,
            string method,
            string url,
            List<string> headers,
            int contentMode, // 0 - null, 1 - text, 2 - binary
            object content,
            bool outputIsBinary,
            VmContext vmContext,
            Value callback,
            object[] mutexDataIgnored) // 
        {
            requestNativeData[1] = new object();
            System.ComponentModel.BackgroundWorker bgWorker = new System.ComponentModel.BackgroundWorker();
            bgWorker.DoWork += (sender, e) =>
            {
                HttpResponseValue response = SendRequestSyncImpl(requestNativeData, method, url, headers, contentMode, content, outputIsBinary);
                lock (requestNativeData[1])
                {
                    requestNativeData[0] = response;
                    requestNativeData[2] = true;
                }
            };
            bgWorker.RunWorkerCompleted += (sender, e) =>
            {
                Interpreter.Vm.CrayonWrapper.runInterpreterWithFunctionPointer(vmContext, callback, new Value[0]);
            };
            bgWorker.RunWorkerAsync();
        }

        public static bool SendRequestSync(
            object[] requestNativeData,
            string method,
            string url,
            List<string> headers,
            int contentMode, // 0 - null, 1 - text, 2 - binary
            object content,
            bool outputIsBinary)
        {
            HttpResponseValue response = SendRequestSyncImpl(requestNativeData, method, url, headers, contentMode, content, outputIsBinary);

            requestNativeData[0] = response;
            requestNativeData[2] = true;

            return false;
        }

        private static HttpResponseValue SendRequestSyncImpl(
            object[] requestNativeData,
            string method,
            string url,
            List<string> headers,
            int contentMode, // 0 - null, 1 - text, 2 - binary
            object content,
            bool outputIsBinary)
        {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
            request.Method = method;
            string contentType = null;
            for (int i = 0; i < headers.Count; i += 2)
            {
                string headerName = headers[i];
                string headerValue = headers[i + 1];
                switch (headerName.ToLower())
                {
                    case "user-agent":
                        request.UserAgent = headerValue;
                        break;
                    case "content-type":
                        contentType = headerValue;
                        break;
                    default:
                        request.Headers.Add(headerName, headerValue);
                        break;
                }
            }

            if (contentMode != 0)
            {
                byte[] contentBytes;
                if (contentMode == 2)
                {
                    int[] intContent = (int[])content;
                    int length = intContent.Length;
                    contentBytes = new byte[length];
                    for (int i = 0; i < length; ++i)
                    {
                        contentBytes[i] = (byte)intContent[i];
                    }
                }
                else
                {
                    string body = (string)content;
                    contentBytes = System.Text.UTF8Encoding.UTF8.GetBytes(body);
                }
                request.ContentLength = Convert.ToInt64(contentBytes.Length);
                if (contentType != null)
                {
                    request.ContentType = contentType;
                }
                System.IO.BinaryWriter streamWriter = new System.IO.BinaryWriter(request.GetRequestStream());
                streamWriter.Write(contentBytes);
                streamWriter.Flush();
                streamWriter.Close();
            }

            HttpWebResponse response;
            try
            {
                response = (HttpWebResponse)request.GetResponse();
            }
            catch (WebException we)
            {
                response = (HttpWebResponse)we.Response;
                if (response == null && we.Status == WebExceptionStatus.Timeout)
                {
                    // TODO: timeouts
                    throw new Exception();
                }
            }

            int statusCode = (int)response.StatusCode;
            string statusMessage = response.StatusDescription;
            byte[] responseBytes = null;
            string responseString = null;
            if (outputIsBinary)
            {
                System.IO.BinaryReader binaryReader = new System.IO.BinaryReader(response.GetResponseStream());
                byte[] buffer = new byte[1024];
                List<byte> contentList = new List<byte>();
                int bytesRead = 1;
                while (bytesRead > 0)
                {
                    bytesRead = binaryReader.Read(buffer, 0, buffer.Length);
                    if (bytesRead == 1024)
                    {
                        contentList.AddRange(buffer);
                    }
                    else
                    {
                        for (int i = 0; i < bytesRead; ++i)
                        {
                            contentList.Add(buffer[i]);
                        }
                    }
                }
                binaryReader.Close();
                responseBytes = contentList.ToArray();
            }
            else
            {
                System.IO.StreamReader streamReader = new System.IO.StreamReader(response.GetResponseStream());
                responseString = streamReader.ReadToEnd();
                streamReader.Close();
            }

            List<string> responseHeaders = new List<string>();
            foreach (string headerName in response.Headers.AllKeys)
            {
                foreach (string headerValue in response.Headers.GetValues(headerName))
                {
                    responseHeaders.Add(headerName);
                    responseHeaders.Add(headerValue);
                }
            }

            if (outputIsBinary && statusCode >= 400)
            {
                try
                {
                    responseString = System.Text.Encoding.UTF8.GetString(responseBytes);
                }
                catch (Exception)
                { }
            }

            return new HttpResponseValue()
            {
                StatusCode = statusCode,
                StatusMessage = statusMessage,
                IsContentBinary = outputIsBinary,
                ContentString = responseString,
                ContentBytes = responseBytes,
                Headers = responseHeaders.ToArray(),
            };
        }

        private class HttpResponseValue
        {
            public int StatusCode { get; set; }
            public string StatusMessage { get; set; }
            public bool IsContentBinary { get; set; }
            public string ContentString { get; set; }
            public byte[] ContentBytes { get; set; }
            public string[] Headers { get; set; }
        }

        public static bool PollRequest(object[] requestNativeData)
        {
            bool output = false;

            lock (requestNativeData[1])
            {
                output = (bool)requestNativeData[2];
            }

            return output;
        }
    }
}
