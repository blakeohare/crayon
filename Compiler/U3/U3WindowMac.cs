namespace U3
{
#if MAC
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    internal class U3WindowMac : U3Window
    {
        private readonly string token;

        private readonly string filePath;

        private System.Net.Sockets.Socket downstreamSocket = null;

        internal U3WindowMac()
        {
            this.token = GetGibberishToken();
            this.filePath = System.IO.Path.Combine(System.Environment.GetEnvironmentVariable("TMPDIR"), "u3_" + this.token);
        }

        internal override async Task<string> CreateAndShowWindowImpl(
            string title,
            byte[] nullableIcon,
            int width,
            int height,
            Func<string, string, bool> handleVmBoundMessage)
        {
            System.Net.Sockets.Socket socket = new System.Net.Sockets.Socket(
                System.Net.Sockets.AddressFamily.Unix,
                System.Net.Sockets.SocketType.Stream,
                System.Net.Sockets.ProtocolType.Unspecified);
            socket.Bind(new System.Net.Sockets.UnixDomainSocketEndPoint(this.filePath + "_us"));
            socket.Listen(1);
            System.ComponentModel.BackgroundWorker bgworker = new System.ComponentModel.BackgroundWorker();
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            bgworker.DoWork += async (e, sender) =>
            {
                System.Net.Sockets.Socket s = socket.Accept();
                SocketReader sr = new SocketReader(s);
                while (true)
                {
                    int length = sr.ReadLength();
                    string data = sr.ReadString(length);
                    int colon = data.IndexOf(':');
                    string type = colon == -1 ? data : data.Substring(0, colon);
                    string payload = colon == -1 ? "" : data.Substring(colon + 1);

                    if (type == "READY")
                    {
                        StartSocketClient();
                        await this.SendString("SRC", JsResourceUtil.GetU3Source());
                    }
                    else if (type == "VMJSON")
                    {
                        IDictionary<string, object> jsonPayload = new Wax.Util.JsonParser(payload.Substring(1, payload.Length - 2)).ParseAsDictionary();
                        handleVmBoundMessage((string)jsonPayload["type"], (string)jsonPayload["message"]);
                    }
                    else
                    {
                        throw new Exception("Unknown message type: " + type);
                    }
                }
            };

            bgworker.RunWorkerAsync();

            await Task.Delay(TimeSpan.FromMilliseconds(10));

            return await RunProcess(title, width, height); // Process ends when window is closed.
        }

        private class SocketReader
        {
            private System.Net.Sockets.Socket socket;
            private Queue<byte> bytes = new Queue<byte>();
            private byte[] buffer = new byte[1000];

            public SocketReader(System.Net.Sockets.Socket socket)
            {
                this.socket = socket;
            }

            private static bool IsUsableByte(byte c)
            {
                if (c == '@') return true;
                if (c >= '0' && c <= '9') return true;
                if (c >= 'A' && c <= 'Z') return true;
                if (c >= 'a' && c <= 'z') return true;
                if (c == '+' || c == '/' || c == '=') return true;
                return false;
            }

            private void SpinEnsureMore()
            {
                if (this.bytes.Count > 0)
                {
                    return;
                }

                while (this.bytes.Count == 0)
                {
                    System.Threading.Thread.Sleep(1);
                    this.ReadMore();
                }
            }

            private bool ReadMore()
            {
                int bytesRead = this.socket.Receive(this.buffer, 0, this.buffer.Length, System.Net.Sockets.SocketFlags.None);

                for (int i = 0; i < bytesRead; i++)
                {
                    byte b = this.buffer[i];
                    if (IsUsableByte(b))
                    {
                        bytes.Enqueue(b);
                    }
                }
                return this.bytes.Count > 0;
            }

            private char GetNextChar()
            {
                if (this.bytes.Count == 0)
                {
                    this.SpinEnsureMore();
                }
                byte b = this.bytes.Dequeue();
                return (char)b;
            }

            public int ReadLength()
            {
                int length = 0;
                while (true)
                {
                    char c = this.GetNextChar();
                    if (c == '@') break;
                    else if (c >= '0' && c <= '9')
                    {
                        length = length * 10 + (c - '0');
                    }
                }
                return length;
            }

            public string ReadString(int length)
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                while (sb.Length < length)
                {
                    if (this.bytes.Count == 0) this.ReadMore();
                    byte b = this.bytes.Dequeue();
                    char c = (char)b;
                    if ((c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || (c >= '0' && c <= '9') || c == '-' || c == '/' || c == '=')
                    {
                        sb.Append(c);
                    }
                }
                byte[] textBytes = Convert.FromBase64String(sb.ToString());
                return System.Text.Encoding.UTF8.GetString(textBytes);
            }
        }

        private void StartSocketClient()
        {

            this.downstreamSocket = new System.Net.Sockets.Socket(
                System.Net.Sockets.AddressFamily.Unix,
                System.Net.Sockets.SocketType.Stream,
                System.Net.Sockets.ProtocolType.Unspecified);

            this.downstreamSocket.Connect(new System.Net.Sockets.UnixDomainSocketEndPoint(this.filePath + "_ds"));
        }

        private Task SendString(string header, string value)
        {
            byte[] msg = System.Text.Encoding.UTF8.GetBytes(header + ":" + value);
            string msgBase64 = Convert.ToBase64String(msg);
            string payloadString = msgBase64.Length + "@" + msgBase64;
            byte[] payloadBytes = System.Text.Encoding.UTF8.GetBytes(payloadString);
            lock (this.downstreamSocket)
            {
                this.downstreamSocket.Send(payloadBytes);
            }
            return Task.CompletedTask;
        }

        internal override Task SendJsonData(string jsonString)
        {
            return this.SendString("JSON", jsonString);
        }

        private static string GetGibberishToken()
        {
            Random r = new Random();
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            string chars = "abcdefghijklmnopqrstuvwyxz0123456789";
            for (int i = 0; i < 15; i++)
            {
                sb.Append(chars[r.Next(chars.Length)]);
            }
            return sb.ToString();
        }

        private Task<string> RunProcess(string title, int width, int height)
        {

            System.Diagnostics.Process p = new System.Diagnostics.Process();

            string execName = "/Users/blakeohare/Repos/Crayon/Compiler/U3/webview/u3mac";

            string argsFlatString = string.Join(' ', new string[] {
                "width", width + "",
                "height", height + "",
                "title", Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(title)),
                "us", this.filePath + "_us",
                "ds", this.filePath + "_ds",
                "pid", System.Diagnostics.Process.GetCurrentProcess().Id + "",

            });
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo(execName, argsFlatString);

            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            p.EnableRaisingEvents = true;
            p.StartInfo = startInfo;

            TaskCompletionSource<string> procTask = new TaskCompletionSource<string>();

            p.Exited += (sender, e) =>
            {
                Console.WriteLine("Process ended");
                procTask.TrySetResult("close-button");
            };

            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal;

            bool started = p.Start();

            return procTask.Task;
        }
    }
#endif
}
