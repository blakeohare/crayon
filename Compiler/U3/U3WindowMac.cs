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

        internal U3WindowMac() {
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
                //System.Console.WriteLine("upstream socket ready to accept connections to " + this.filePath + "_us");
                System.Net.Sockets.Socket s = socket.Accept();
                SocketReader sr = new SocketReader(s);
                while (true) {
                    //System.Console.WriteLine("Waiting to accept next upstream message");
                    int length = sr.ReadLength();
                    //System.Console.WriteLine("Received US message length: " + length);
                    string data = sr.ReadString(length);
                    //System.Console.WriteLine("Received US message value: " + data);
                    int colon = data.IndexOf(':');
                    string type = colon == -1 ? data : data.Substring(0, colon);
                    string payload = colon == -1 ? "" : data.Substring(colon + 1);

                    if (type == "READY") {
                        StartSocketClient();
                        await this.SendString("SRC", JsResourceUtil.GetU3Source());
                    } else if (type == "VMJSON") {
                        IDictionary<string, object> jsonPayload = new Wax.Util.JsonParser(payload.Substring(1, payload.Length - 2)).ParseAsDictionary();
                        handleVmBoundMessage((string) jsonPayload["type"], (string) jsonPayload["message"]);
                    } else {
                        //System.Console.WriteLine("Incoming message of type '" + type + "': | " + payload + " | ");
                    }
                }
            };
            
            bgworker.RunWorkerAsync();

            //System.Console.WriteLine("Delaying 2 seconds");
            await Task.Delay(TimeSpan.FromMilliseconds(10));
            //System.Console.WriteLine("Done delaying");

            return await RunProcess(title, width, height); // Process ends when window is closed.
        }

        private class SocketReader {
            private System.Net.Sockets.Socket socket;
            private Queue<byte> bytes = new Queue<byte>();
            private byte[] buffer = new byte[1000];

            public SocketReader(System.Net.Sockets.Socket socket) {
                this.socket = socket;
            }

            private static bool IsUsableByte(byte c) {
                if (c == '@') return true;
                if (c >= '0' && c <= '9') return true;
                if (c >= 'A' && c <= 'Z') return true;
                if (c >= 'a' && c <= 'z') return true;
                if (c == '+' || c == '/' || c == '=') return true;
                return false;
            }

            private void SpinEnsureMore() {
                if (this.bytes.Count > 0) {
                    return;
                }

                while (this.bytes.Count == 0) {
                    System.Threading.Thread.Sleep(1);
                    this.ReadMore();
                }
            }

            private bool ReadMore() {
                int bytesRead = this.socket.Receive(this.buffer, 0, this.buffer.Length, System.Net.Sockets.SocketFlags.None);
                
                for (int i = 0; i < bytesRead; i++) {
                    byte b = this.buffer[i];
                    if (IsUsableByte(b)) {
                        // System.Console.WriteLine("Received byte: '" + (char)b);
                        bytes.Enqueue(b);
                    }
                }
                return this.bytes.Count > 0;
            }

            private char GetNextChar() {
                if (this.bytes.Count == 0) {
                    this.SpinEnsureMore();
                }
                byte b = this.bytes.Dequeue();
                return (char)b;
            }

            public int ReadLength() {
                //System.Console.WriteLine("Going to try to read the length");
                
                int length = 0;
                while (true) {
                    char c = this.GetNextChar();
                    if (c == '@') break;
                    else if (c >= '0' && c <= '9') {
                        length = length * 10 + (c - '0');
                    }
                }
                //System.Console.WriteLine("The length is " + length);
                return length;
            }

            public string ReadString(int length) {
                //System.Console.WriteLine("Going to try to read a string");
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                while (sb.Length < length) {
                    if (this.bytes.Count == 0) this.ReadMore();
                    byte b = this.bytes.Dequeue();
                    char c = (char) b;
                    if ((c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || (c >= '0' && c <= '9') || c == '-' || c == '/' || c == '=') {
                        sb.Append(c);
                    }
                }
                byte[] textBytes = System.Convert.FromBase64String(sb.ToString());
                string value = System.Text.Encoding.UTF8.GetString(textBytes);
                //System.Console.WriteLine("The string value is " + value);
                return value;
            }
        }

        private void StartSocketClient() {
            
            this.downstreamSocket = new System.Net.Sockets.Socket(
                System.Net.Sockets.AddressFamily.Unix,
                System.Net.Sockets.SocketType.Stream,
                System.Net.Sockets.ProtocolType.Unspecified);
            try {
                this.downstreamSocket.Connect(new System.Net.Sockets.UnixDomainSocketEndPoint(this.filePath + "_ds"));
            } catch (Exception e) {
                System.Console.WriteLine("EX: " + e.Message);
                throw new Exception("WAHHHHH");
            }
        }

        private async Task SendString(string header, string value) {
            //System.Console.WriteLine("Sending a " + header + " type of message downstream");
            //System.Console.WriteLine("It looks like: " + value.Substring(0, 100) + "...");
            byte[] msg = System.Text.Encoding.UTF8.GetBytes(header + ":" + value);
            string msgBase64 = Convert.ToBase64String(msg);
            string payloadString = msgBase64.Length + "@" + msgBase64;
            byte[] payloadBytes = System.Text.Encoding.UTF8.GetBytes(payloadString);
            lock (this.downstreamSocket) {
                this.downstreamSocket.Send(payloadBytes);
            }
            await Task.FromResult(true); // TODO: This is silly, is there a direct method to return an instant Task without generics?
        }

        internal override Task SendJsonData(string jsonString)
        {
            return this.SendString("JSON", jsonString);
        }

        private static string GetGibberishToken()
        {
            System.Random r = new System.Random();
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            string chars = "abcdefghijklmnopqrstuvwyxz0123456789";
            for (int i = 0; i < 15; i++) {
                sb.Append(chars[r.Next(chars.Length)]);
            }
            return sb.ToString();
        }

        private Task<string> RunProcess(string title, int width, int height) {
            
            System.Diagnostics.Process p = new System.Diagnostics.Process();

            string execName = "/Users/blakeohare/Repos/Crayon/Compiler/U3/webview/u3mac";
            
            string argsFlatString = string.Join(' ', new string[] { 
                "width", width + "",
                "height", height + "",
                "title", System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(title)),
                "us", this.filePath + "_us",
                "ds", this.filePath + "_ds",
                "pid", System.Diagnostics.Process.GetCurrentProcess().Id + "",

            });
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo(execName, argsFlatString);
            
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            p.EnableRaisingEvents = true;
            p.StartInfo = startInfo;

            TaskCompletionSource<string> procTask = new TaskCompletionSource<string>();

            p.Exited += (sender, e) => {
                System.Console.WriteLine("Process ended");
                procTask.TrySetResult("close-button");
            };

/*
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            
            p.OutputDataReceived += (sender, e) => {
                System.Console.WriteLine("U3 STDOUT: " + e.Data);
            };
            p.ErrorDataReceived += (sender, e) => {
                System.Console.WriteLine("U3 STDERR: " + e.Data);
            };//*/


            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal;

            bool started = p.Start();
            //System.Console.WriteLine("Process has " + (started ? "" : "NOT ") + "started.");
            return procTask.Task;
        }
    }
#endif
}
