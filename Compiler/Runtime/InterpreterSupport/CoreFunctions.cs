using Interpreter.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Interpreter.Vm
{
    internal static class CoreFunctions
    {
        public static object[] NamedPipeCreate(string name)
        {
            System.IO.Pipes.NamedPipeClientStream pipe = new System.IO.Pipes.NamedPipeClientStream(name);
            pipe.Connect();
            return new object[] {
                pipe,
                new System.IO.StreamReader(pipe),
                new System.IO.StreamWriter(pipe),
            };
        }

        private static System.IO.StreamWriter GetStreamWriter(object pipeWrapper)
        {
            return (System.IO.StreamWriter)((object[])pipeWrapper)[2];
        }

        private static string NamedPipeConvertErrorMessage(string dotNetError)
        {
            if (dotNetError.StartsWith("Pipe is broken")) return "Pipe is broken.";

            return "An unknown error has occurred.";
        }

        public static string NamedPipeWriteLine(object pipeWrapper, string value)
        {
            try
            {
                GetStreamWriter(pipeWrapper).WriteLine(value);
            }
            catch (System.IO.IOException ioe)
            {
                return NamedPipeConvertErrorMessage(ioe.Message);
            }
            return null;
        }

        public static string NamedPipeFlush(object pipeWrapper)
        {
            try
            {
                GetStreamWriter(pipeWrapper).Flush();
            }
            catch (System.IO.IOException ioe)
            {
                return NamedPipeConvertErrorMessage(ioe.Message);
            }
            return null;
        }

        public static object NamedPipeServerCreate(VmContext vm, string name, Value startFn, Value onDataFn, Value closeFn)
        {
            System.IO.Pipes.NamedPipeServerStream namedPipe = new System.IO.Pipes.NamedPipeServerStream(name, System.IO.Pipes.PipeDirection.In);
            System.IO.StreamReader reader = new System.IO.StreamReader(namedPipe);
            object[] output = new object[] { namedPipe, reader, startFn, onDataFn, closeFn };

            System.ComponentModel.BackgroundWorker bgWorker = new System.ComponentModel.BackgroundWorker();
            bgWorker.DoWork += (sender, e) =>
            {
                try
                {
                    namedPipe.WaitForConnection();
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine(ex.Message);
                    return;
                }

                // TODO: this is based on the MessageHub format and needs to be rewritten in a more generic way.
                // Sending individual characters is probably a bad idea performance-wise, and so some sort of
                // batching heuristic needs to be made. This "length@payload" format needs to go into the actual
                // MessageHub code directly.
                int stringLength = 0;
                System.Text.StringBuilder buffer = new System.Text.StringBuilder();
                bool isLengthParsing = true;
                while (true)
                {
                    int b = namedPipe.ReadByte();
                    if (isLengthParsing)
                    {
                        if (b == '@')
                        {
                            stringLength = int.Parse(buffer.ToString());
                            buffer.Clear();
                            if (stringLength == 0)
                            {
                                TranslationHelper.GetEventLoop(vm).ExecuteFunctionPointerNativeArgs(onDataFn, new object[] { "" });
                            }
                            else
                            {
                                isLengthParsing = false;
                            }
                        }
                        else
                        {
                            buffer.Append((char)b);
                        }
                    }
                    else
                    {
                        buffer.Append((char)b);
                        stringLength--;
                        if (stringLength == 0)
                        {
                            TranslationHelper.GetEventLoop(vm).ExecuteFunctionPointerNativeArgs(onDataFn, new object[] { buffer.ToString() });
                            buffer.Clear();
                            isLengthParsing = true;
                        }
                    }
                }
            };
            bgWorker.RunWorkerAsync();

            return output;
        }

        public static string NamedPipeServerClose(object pipeRaw)
        {
            object[] pipe = (object[])pipeRaw;
            System.IO.Pipes.NamedPipeServerStream namedPipe = (System.IO.Pipes.NamedPipeServerStream)pipe[0];
            namedPipe.Close();
            return null;
        }

        internal static string UnixSocketClientCreate(VmContext vm, object[] clientOut, string path, Value onReadyCb, Value onDisconnectCb)
        {
            System.Net.Sockets.Socket socket = new System.Net.Sockets.Socket(
                System.Net.Sockets.AddressFamily.Unix,
                System.Net.Sockets.SocketType.Stream,
                System.Net.Sockets.ProtocolType.Unspecified);
            clientOut[0] = socket;

            System.ComponentModel.BackgroundWorker bgworker = new System.ComponentModel.BackgroundWorker();
            bgworker.DoWork += (sender, e) =>
            {
                try
                {
                    socket.Connect(new System.Net.Sockets.UnixDomainSocketEndPoint(path));
                    TranslationHelper.GetEventLoop(vm).ExecuteFunctionPointer(onReadyCb, new Value[0]);
                }
                catch (Exception)
                {
                    TranslationHelper.GetEventLoop(vm).ExecuteFunctionPointer(onDisconnectCb, new Value[0]);
                }
            };
            bgworker.RunWorkerAsync();
            return null;
        }

        internal static string UnixSocketClientSend(object client, string msg)
        {
            System.Net.Sockets.Socket socket = (System.Net.Sockets.Socket)client;
            if (socket == null) return "Socket closed";
            byte[] msgBytes = System.Text.Encoding.UTF8.GetBytes(msg);
            socket.Send(msgBytes);
            return null;
        }

        internal static string UnixSocketServerCreate(VmContext vm, object[] serverOut, string path, Value onRecvCb)
        {
            if (System.IO.File.Exists(path))
            {
                System.IO.File.Delete(path);
            }

            System.Net.Sockets.Socket socket = new System.Net.Sockets.Socket(
                System.Net.Sockets.AddressFamily.Unix,
                System.Net.Sockets.SocketType.Stream,
                System.Net.Sockets.ProtocolType.Unspecified);
            serverOut[0] = socket;
            socket.Bind(new System.Net.Sockets.UnixDomainSocketEndPoint(path));
            socket.Listen(1); // TODO: customizable value
            System.ComponentModel.BackgroundWorker bgworker = new System.ComponentModel.BackgroundWorker();
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            bool intPhase = true;
            int targetLength = 0;
            bgworker.DoWork += (e, sender) =>
            {
                System.Net.Sockets.Socket s = socket.Accept();
                byte[] buffer = new byte[2048];
                int bytesRead = 0;
                bool stillRunning = true;
                while (stillRunning)
                {
                    if (intPhase)
                    {
                        bytesRead = s.Receive(buffer, 0, 1, System.Net.Sockets.SocketFlags.None);
                        if (bytesRead == 0)
                        {
                            stillRunning = false;
                        }
                        else if (buffer[0] == (byte)'@')
                        {
                            targetLength = int.Parse(sb.ToString());
                            intPhase = false;
                            sb.Clear();
                        }
                        else
                        {
                            sb.Append((char)buffer[0]);
                        }
                    }
                    else
                    {
                        bytesRead = s.Receive(buffer, 0, System.Math.Min(buffer.Length, targetLength), System.Net.Sockets.SocketFlags.None);
                        for (int i = 0; i < bytesRead; ++i)
                        {
                            // TODO: This is problematic with multi-byte encodings.
                            sb.Append((char)buffer[i]);
                        }
                        if (sb.Length == targetLength)
                        {
                            intPhase = true;
                            string msg = sb.ToString();
                            sb.Clear();
                            TranslationHelper.GetEventLoop(vm).ExecuteFunctionPointerNativeArgs(onRecvCb, new object[] { msg });
                        }
                    }
                }
            };
            bgworker.RunWorkerAsync();
            return null;
        }

        internal static string UnixSocketServerCreate_CORRECT_NO_HACK(VmContext vm, object[] serverOut, string path, Value onRecvCb)
        {
            if (System.IO.File.Exists(path))
            {
                System.IO.File.Delete(path);
            }

            System.Net.Sockets.Socket socket = new System.Net.Sockets.Socket(
                System.Net.Sockets.AddressFamily.Unix,
                System.Net.Sockets.SocketType.Stream,
                System.Net.Sockets.ProtocolType.Unspecified);
            serverOut[0] = socket;
            socket.Bind(new System.Net.Sockets.UnixDomainSocketEndPoint(path));
            socket.Listen(1); // TODO: customizable value
            System.ComponentModel.BackgroundWorker bgworker = new System.ComponentModel.BackgroundWorker();

            bgworker.DoWork += (e, sender) =>
            {
                System.Net.Sockets.Socket s = socket.Accept();
                byte[] buffer = new byte[2048];
                int bytesRead = 0;
                do
                {
                    bytesRead = s.Receive(buffer, 0, buffer.Length, System.Net.Sockets.SocketFlags.None);
                    // TODO: This is problematic with multi-byte encodings. This should honestly be a byte-based API
                    // but that would be unperformant in the VM until there's a native byte-array type. The only
                    // use case for this at the moment will only be sending base64 so it's okay (for now).
                    string msg = System.Text.Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    TranslationHelper.GetEventLoop(vm).ExecuteFunctionPointerNativeArgs(onRecvCb, new object[] { msg });
                } while (bytesRead > 0);
            };
            bgworker.RunWorkerAsync();
            return null;
        }

        internal static void UnixSocketServerDisconnect(object server)
        {
            System.Net.Sockets.Socket socket = (System.Net.Sockets.Socket)server;
            socket.Close();
        }

        public static void HttpSend(VmContext vm, Value cb, Value failCb, string url, string method, string contentType, int[] binaryContent, string textContent, string[] headersKvp, Value bytesObj, object[] bytesObjNativeData)
        {
            System.ComponentModel.BackgroundWorker bgWorker = new System.ComponentModel.BackgroundWorker();
            bgWorker.DoWork += (sender, e) =>
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = method;
                for (int i = 0; i < headersKvp.Length; i += 2)
                {
                    string headerName = headersKvp[i];
                    string headerValue = headersKvp[i + 1];
                    switch (headerName.ToLower())
                    {
                        case "user-agent":
                            request.UserAgent = headerValue;
                            break;
                        default:
                            request.Headers.Add(headerName, headerValue);
                            break;
                    }
                }

                byte[] contentBytes = null;
                if (textContent != null)
                {
                    contentBytes = System.Text.Encoding.UTF8.GetBytes(textContent);
                }
                else if (binaryContent != null)
                {
                    int length = binaryContent.Length;
                    contentBytes = new byte[length];
                    for (int i = 0; i < length; ++i)
                    {
                        contentBytes[i] = (byte)binaryContent[i];
                    }
                }
                if (contentBytes != null)
                {
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
                        TranslationHelper.GetEventLoop(vm).ExecuteFunctionPointer(failCb, new Value[0]);
                        return;
                    }
                    else if (we.Status == WebExceptionStatus.ProtocolError)
                    {
                        // This is fine, it's just a non-2XX status code. Let it go through the other getters as-is.
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                }

                int statusCode = (int)response.StatusCode;
                string statusMessage = response.StatusDescription;
                string responseString = null;

                System.IO.BinaryReader binaryReader = new System.IO.BinaryReader(response.GetResponseStream());
                byte[] buffer = new byte[1024];
                List<byte> contentList = new List<byte>();
                int bytesRead = 1;
                while (bytesRead > 0)
                {
                    bytesRead = binaryReader.Read(buffer, 0, buffer.Length);
                    for (int i = 0; i < bytesRead; ++i)
                    {
                        contentList.Add(buffer[i]);
                    }
                }
                binaryReader.Close();
                bytesObjNativeData[0] = contentList.Select(b => (int)b).ToArray();

                try
                {
                    responseString = System.Text.Encoding.UTF8.GetString(contentList.ToArray());
                }
                catch (Exception)
                {
                    // well, we tried.
                }

                List<object> responseHeaders = new List<object>();
                string responseContentType = null;
                foreach (string headerName in response.Headers.AllKeys)
                {
                    bool isContentType = headerName.ToLowerInvariant() == "content-type";

                    foreach (string headerValue in response.Headers.GetValues(headerName))
                    {
                        if (isContentType && responseContentType == null) responseContentType = headerValue;
                        responseHeaders.Add(headerName);
                        responseHeaders.Add(headerValue);
                    }
                }

                TranslationHelper.GetEventLoop(vm).ExecuteFunctionPointerNativeArgs(cb, new object[] {
                    statusCode,
                    statusMessage,
                    responseContentType,
                    responseHeaders.ToArray(),
                    bytesObj,
                    responseString,
                });

            };
            bgWorker.RunWorkerAsync();
        }

        public static void WaxSend(object eventLoopObj, object waxHubObj, string serviceId, string payloadJson, Value callback)
        {
            Wax.WaxHub waxHub = (Wax.WaxHub)waxHubObj;
            EventLoop eventLoop = (EventLoop)eventLoopObj;
            Dictionary<string, object> payload = new Dictionary<string, object>(new Wax.Util.JsonParser(payloadJson).ParseAsDictionary());

            waxHub.SendRequest(serviceId, payload).ContinueWith(responseTask =>
            {
                object[] callbackArgs = new object[] { null, null };
                if (responseTask.IsFaulted)
                {
                    callbackArgs[0] = responseTask.Exception.Message;
                }
                else
                {
                    callbackArgs[1] = new Wax.JsonBasedObject(responseTask.Result).ToJson();
                }
                eventLoop.ExecuteFunctionPointerNativeArgs(callback, callbackArgs);
            });
        }
    }
}
